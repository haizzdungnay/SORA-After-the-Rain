#!/usr/bin/env python3
"""Rebuild SORA V04 nine-block CC0-only overlay into Git LFS.

Requires the V03 asset importer/helpers and V04_DENSE_GRID_LAYOUT.json.
No owner-private AFX geometry is loaded or stored in this artifact.
The separately delivered PRIVATE V04 model has additional Japanese sign art.
"""
import argparse
import hashlib
import importlib.util
import json
import math
from pathlib import Path

import numpy as np
import trimesh
from PIL import Image
from trimesh.visual.material import PBRMaterial
from trimesh.visual.texture import TextureVisuals

def setup_helper(path):
    sp=importlib.util.spec_from_file_location("sora_v03_kit",str(path))
    mod=importlib.util.module_from_spec(sp)
    sp.loader.exec_module(mod)
    return mod

def material(name,rgba,rough=.8):
    return PBRMaterial(name=name,baseColorFactor=rgba,roughnessFactor=rough)

def tf(x=0,y=0,z=0,yaw=0,s=(1,1,1)):
    a=math.radians(yaw);c,b=math.cos(a),math.sin(a)
    return np.array([[c*s[0],0,b*s[2],x],[0,s[1],0,y],
                     [-b*s[0],0,c*s[2],z],[0,0,0,1]],float)

class MeshFactory:
    def __init__(self,layout,source_root,helper):
        self.scene=trimesh.Scene()
        self.layout=layout
        self.mat={}
        self.count=0
        self.cc0_nodes=0
        self.module=helper
        self.iz=helper.glb_scene(source_root/"IzakayaAlley/izakaya_starter_scene.glb")
        self.city=helper.glb_scene(source_root/"JapaneseSchoolCity/school_city_starter_scene.glb")
        helper.SOURCE_IZ=self.iz
        self.ig=helper.index(self.iz);self.cg=helper.index(self.city)
        self.source_root=source_root
        self.colliders=[]
    def shade(self,name,rgb,rough=.77):
        if name not in self.mat:self.mat[name]=material(name,[*rgb,255],rough)
        return self.mat[name]
    def box(self,id,center,size,shade,yaw=0):
        key="BOX/"+shade.name
        if key not in self.scene.geometry:
            mesh=trimesh.creation.box(extents=[1,1,1])
            mesh.visual=TextureVisuals(material=shade)
            self.scene.geometry[key]=mesh
        self.count+=1
        self.scene.graph.update(frame_to=f"V04/{id}/{self.count}",frame_from=self.scene.graph.base_frame,
            matrix=tf(*center,yaw=yaw,s=size),geometry=key)
    def group(self,id,group,at,yaw=0,width=None,height=None):
        iz=group in self.ig and group not in self.cg
        src=self.iz if iz else self.city
        keys=self.ig if iz else self.cg
        turn=90 if iz and "front-bay" in group else 0
        self.module.fit_group(self.scene,src,keys,group,f"V04/CC0/{id}",at,
                              yaw=yaw,width=width,height=height,turn=turn)
        self.cc0_nodes+=len(keys[group])
    def building(self,b,i):
        ident=b["id"];x,z=b["front_xz"];yaw=b["yaw"]
        w=b["width"];d=b["depth"];h=b["height"]
        fr=tf(x,0,z,yaw=yaw)
        colors=[(159,133,114),(102,133,145),(171,164,143),(146,113,112),
                (121,135,119),(164,147,119),(135,131,154)]
        wall=self.shade("V04_Wall_"+str(i%7),colors[i%7])
        trim=self.shade("V04_Trim",(208,199,170))
        glass=self.shade("V04_Glass",(65,96,110),rough=.22)
        roof=self.shade("V04_Roof",(50,60,73))
        def put(name,local,size,mat):
            p=fr@np.array([*local,1])
            self.box(f"BUILDING/{ident}/{name}",p[:3],size,mat,yaw)
        put("rear",(0,h/2,d-.12),(w,h,.24),wall)
        put("front_upper",(0,(h+3.6)/2,.09),(w,h-3.6,.21),wall)
        for sign in (-1,1):
            put("side_"+str(sign),(sign*(w/2-.1),h/2,d/2),(.2,h,d),wall)
            for zz in (d*.24,d*.6,d*.83):
                put("side_window_"+str(sign)+"_"+str(zz),(sign*w/2,h*.67,zz),(.07,1.16,1.1),glass)
        for xx in (-w*.30,0,w*.30):
            put("rear_glass_"+str(xx),(xx,h*.60,d+.04),(1.1,1.26,.06),glass)
        put("shopglass",(0,1.57,-.15),(w-.4,2.94,.09),glass)
        put("shopfront_trim",(0,3.4,-.15),(w,.18,.4),trim)
        put("roof",(0,h+.15,d/2),(w+.20,.30,d+.2),roof)
        put("awning",(0,3.47,-.47),(w+.22,.15,1.2),roof)
        put("sign",(0,3.98,-.21),(w-.25,.74,.13),
            self.shade("V04_Sign_"+str(i%7),colors[(i+3)%7]))
        for k in (-1,1):
            put("roof_unit_"+str(k),(k*w*.22,h+.6,d*.68),(.74,.6,.72),trim)
        # Source-module facade is cut from an individual shopfront mesh group,
        # never reuses the entire source sample alley.
        src=[
            "021-izakaya-front-bay","022-ramen-shop-front-bay",
            "000-shopfront-ramen-shop","001-shopfront-laundrette",
            "002-shopfront-bookshop","003-shopfront-shutter"]
        self.group("FAC/"+ident,src[i%len(src)],[x,.02,z],yaw,w*.98,3.42)
        world=fr@np.array([0,h/2,d/2,1])
        dims=[w,h,d] if yaw%180==0 else [d,h,w]
        self.colliders.append({"id":ident,"group":"BUILDING",
          "cx":float(world[0]),"cy":h/2,"cz":float(world[2]),
          "sx":dims[0],"sy":h,"sz":dims[2]})
    def parked(self,c,i):
        x,z=c["center_xz"];yaw=c["yaw"];ident=c["id"]
        kind=c["kind"]
        length=5 if kind=="van" else 3.6 if kind=="kei" else 4.45
        height=2.12 if kind=="van" else 1.65
        width=1.8 if kind=="van" else 1.5 if kind=="kei" else 1.72
        paint=self.shade("V04_CarPaint_"+str(i%6),
            [(151,163,165),(79,102,120),(171,164,142),
             (133,103,95),(88,99,112),(184,190,191)][i%6],.3)
        gl=self.shade("V04_CarWindow",(41,72,87),.17)
        tire=self.shade("V04_Tire",(31,31,34))
        t=tf(x,0,z,yaw=yaw)
        def put(label,pt,sz,mat):
            p=t@np.array([*pt,1])
            self.box(f"CAR/{ident}/{label}",p[:3],sz,mat,yaw)
        put("body",(0,.79,0),(width,.8,length),paint)
        put("cab",(0,1.3,-.1),(width*.86,height-1.05,length*.53),paint)
        put("windshield",(0,1.31,-length*.31),(width*.75,.53,.07),gl)
        for side in (-1,1):
            put("side_glass"+str(side),(side*width*.44,1.31,0),(.06,.48,length*.42),gl)
            for zz in (-length*.31,length*.31):
                put(f"tire{side}{zz}",(side*(width/2+.035),.39,zz),(.23,.69,.69),tire)
        w,d=(width,length) if yaw%180==0 else (length,width)
        self.colliders.append({"id":ident,"group":"VEHICLE","cx":x,
                              "cy":height/2,"cz":z,"sx":w+.15,"sy":height,"sz":d+.15})
    def road_and_boundary(self):
        # Extend V03's 3 roads without copying its original intersections.
        road=self.shade("V04_Asphalt_Fallback",(53,60,65))
        edge=self.shade("V04_Concrete",(101,107,108))
        white=self.shade("V04_RoadPaint",(213,213,198))
        warnings=self.shade("V04_BarrierOrange",(201,113,71))
        self.box("GROUND/FALLBACK",[22.5,-.46,22.5],[185,.42,185],edge)
        roadareas=[]
        for c in (-45,0,45):
            roadareas.extend([(63,85.5,c-4.5,c+4.5),(94.5,108,c-4.5,c+4.5),
                              (c-4.5,c+4.5,63,85.5),(c-4.5,c+4.5,94.5,108)])
        roadareas.extend([(-63,108,85.5,94.5),(85.5,94.5,-63,85.5),
                          (85.5,94.5,94.5,108)])
        asphalt=self.source_root/"Asphalt01"
        maps=[Image.open(asphalt/q).convert("RGB") for q in (
           "asphalt_diffuse_2k.jpg","asphalt_normal_gl_2k.png","asphalt_roughness_2k.png")]
        rough=np.asarray(maps[2],dtype=np.uint8)[:,:,1]
        packed=np.zeros((2048,2048,3),dtype=np.uint8)
        packed[:,:,0]=255;packed[:,:,1]=rough
        pbr=PBRMaterial(name="V04_Asphalt01_CC0",baseColorTexture=maps[0],normalTexture=maps[1],
             metallicRoughnessTexture=Image.fromarray(packed),roughnessFactor=.78,
             metallicFactor=0,baseColorFactor=[190,200,207,255])
        for i,(x0,x1,z0,z1) in enumerate(roadareas):
            mesh=trimesh.Trimesh(vertices=[[x0,-.045,z0],[x1,-.045,z0],
                 [x1,-.045,z1],[x0,-.045,z1]],faces=[[0,2,1],[0,3,2]],process=False)
            mesh.visual=TextureVisuals(material=pbr,uv=[[0,0],[(x1-x0)/4,0],
                  [(x1-x0)/4,(z1-z0)/4],[0,(z1-z0)/4]])
            self.scene.add_geometry(mesh,node_name="V04/ASPHALT/"+str(i),geom_name="V04_ROAD_"+str(i))
        # 7 new intersections; V03 already has 9.
        for x in self.layout["roads_x"]:
          for z in self.layout["roads_z"]:
            if x!=90 and z!=90:continue
            for axis in range(4):
              for t in range(6):
                q=(t-2.5)*.82
                if axis<2:pos=[x+q,.02,z+(-6.3 if axis==0 else 6.3)];sz=[.53,.02,1.86]
                else:pos=[x+(-6.3 if axis==2 else 6.3),.02,z+q];sz=[1.86,.02,.53]
                self.box("CROSSING/"+str(x)+"_"+str(z),pos,sz,white)
        # Overlapping wall segments close the FOUR corners without diagonal slits.
        b=[
          ("WEST",[-63.65,2.7,22.5],[1.3,5.4,173]),
          ("EAST",[108.65,2.7,22.5],[1.3,5.4,173]),
          ("SOUTH",[22.5,2.7,-63.65],[173,5.4,1.3]),
          ("NORTH",[22.5,2.7,108.65],[173,5.4,1.3])]
        for name,center,size in b:
            self.box("PERIMETER/"+name,center,size,edge)
            self.colliders.append({"id":"BOUNDARY_"+name,"group":"HARD_BOUNDARY",
                    "cx":center[0],"cy":center[1],"cz":center[2],
                    "sx":size[0],"sy":size[1],"sz":size[2]})
        for side in ("WEST","EAST","SOUTH","NORTH"):
          for coordinate in self.layout["roads_x"]:
            if side=="WEST":pt=[-60.3,.65,coordinate];sz=[.65,1.3,8.4]
            elif side=="EAST":pt=[105.3,.65,coordinate];sz=[.65,1.3,8.4]
            elif side=="SOUTH":pt=[coordinate,.65,-60.3];sz=[8.4,1.3,.65]
            else:pt=[coordinate,.65,105.3];sz=[8.4,1.3,.65]
            self.box("ROAD_END/"+side+"_"+str(coordinate),pt,sz,warnings)
            self.colliders.append({"id":"ROAD_END_"+side+"_"+str(coordinate),"group":"ROAD_BLOCK",
                    "cx":pt[0],"cy":pt[1],"cz":pt[2],
                    "sx":sz[0],"sy":sz[1],"sz":sz[2]})
        # Rendered skyline stands beyond collider bounds and is never accessible.
        background=self.shade("V04_Backdrop",(84,95,112))
        for i in range(13):
            t=-57+i*13
            h=12+i%6*2
            for s in ("W","E","S","N"):
              pos={"W":[-70,h/2,t],"E":[115,h/2,t],
                   "S":[t,h/2,-70],"N":[t,h/2,115]}[s]
              size=[8,h,10] if s in ("W","E") else [10,h,8]
              self.box("SKYLINE/"+s+str(i),pos,size,background)
        # Existing V03 courtyards get back-yard walls, retaining small Tokyo-style service spaces.
        for b in self.layout["blocks"]:
            if b["state"].startswith("V03_"):
                cx,cz=b["center_xz"]
                for side in (-1,1):
                    self.box("COURTYARD/"+b["id"],[cx+side*8.8,1.25,cz],[.33,2.5,14.5],edge)
                    self.box("COURTYARD/"+b["id"],[cx,1.25,cz+side*7.25],[17.6,2.5,.33],edge)

def main(root,out):
    layout=json.loads((root/"Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json").read_text())
    assert layout["validation"]["nine_blocks"] and layout["validation"]["new_buildings"]
    imp=setup_helper(root/"Tools/StreetExterior/build_v03_cc0_overlay.py")
    f=MeshFactory(layout,root/"ArtSource/ThirdParty/CC0",imp)
    for i,b in enumerate(layout["building_catalog"]):f.building(b,i)
    for i,c in enumerate(layout["vehicles"]):f.parked(c,i)
    f.road_and_boundary()
    assert len(f.colliders)==60+42+4+16
    assert len(layout["blocks"])==9 and len(layout["building_catalog"])==60
    out.mkdir(parents=True,exist_ok=True)
    path=out/"SORA_V04_CC0_Dense_Overlay.glb"
    binary=f.scene.export(file_type="glb")
    path.write_bytes(binary)
    checksum=hashlib.sha256(binary).hexdigest()
    doc={"version":"V04","geometry_nodes":len(f.scene.graph.nodes_geometry),
       "new_blocks":5,"new_buildings":60,"retained_v03_buildings":24,
       "junctions":16,"parked_cars":42,"perimeter_segments":4,"road_end_barricades":16,
       "glb_sha256":checksum,"bytes":len(binary),"no_AFX_assets":True,
       "unity_tests":"NOT RUN"}
    (out/"manifest.json").write_text(json.dumps(doc,indent=2)+"\n")
    (out/"colliders.json").write_text(json.dumps({"items":f.colliders},indent=2)+"\n")
    print("SUCCESS:",path,len(binary),"bytes",doc,flush=True)

if __name__=="__main__":
    p=argparse.ArgumentParser()
    p.add_argument("--root",type=Path,default=Path("."))
    p.add_argument("--output",type=Path,required=True)
    a=p.parse_args()
    main(a.root.resolve(),a.output.resolve())
