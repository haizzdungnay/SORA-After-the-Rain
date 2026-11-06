#!/usr/bin/env python3
"""Build a redistributable CC0-only V03 overlay for SORA's 126m V02 street grid.

Inputs: verified CC0 source meshes in ArtSource/ThirdParty/CC0 and the SORA
layout. The overlay contains no AFX or third-party non-CC0 meshes. It can be
aligned with the privately held V02 base mesh at the SAME world origin in Unity.
Import with Blender/glTF-compatible tools. No Unity Editor test is claimed.

Run: python Tools/StreetExterior/build_v03_cc0_overlay.py --root . \
  --output ArtSource/Generated/V03_CC0
Requires: trimesh>=4.5, numpy>=1.26, Pillow>=10.
"""
import argparse
import hashlib
import json
import math
import struct
from collections import defaultdict
from pathlib import Path

import numpy as np
import trimesh
from PIL import Image
from trimesh.visual.material import PBRMaterial
from trimesh.visual.texture import TextureVisuals

EXTRAS = [
    ("NW-05","NW","Ame Studio",-18,38.25,180,7,6.4,7.4),
    ("NW-06","NW","Tomo Vinyl",-38.25,24,90,7.8,7.8,6.5),
    ("NE-05","NE","Akari Pharmacy",17,38.25,180,9,6,7.4),
    ("NE-06","NE","Sumi Tea",38.25,24,-90,7.8,7.8,6.5),
    ("SW-05","SW","Momo Cafe",-16.9,-38.25,0,8,6.5,7),
    ("SW-06","SW","Koi Laundry",-38.25,-24,90,7.8,7.8,6.6),
    ("SE-05","SE","Yuki Stationery",18,-38.25,0,8,6.2,7.5),
    ("SE-06","SE","Nami Gallery",38.25,-24,-90,7.8,7.8,6.8),
]
FACADES = {
 "NW-01":["021-izakaya-front-bay","001-shopfront-laundrette"],
 "NW-02":["003-shopfront-shutter","003-shopfront-shutter"],
 "NW-03":["002-shopfront-bookshop","001-shopfront-laundrette"],
 "NW-04":["002-shopfront-bookshop","002-shopfront-bookshop"],
 "NE-02":["022-ramen-shop-front-bay","000-shopfront-ramen-shop"],
 "NE-03":["000-shopfront-ramen-shop","001-shopfront-laundrette"],
 "NE-04":["003-shopfront-shutter","002-shopfront-bookshop"],
 "SW-01":["003-shopfront-shutter","001-shopfront-laundrette"],
 "SW-02":["001-shopfront-laundrette","001-shopfront-laundrette"],
 "SW-03":["002-shopfront-bookshop","002-shopfront-bookshop"],
 "SW-04":["021-izakaya-front-bay","002-shopfront-bookshop"],
 "SE-01":["002-shopfront-bookshop","000-shopfront-ramen-shop"],
 "SE-02":["024-izakaya-front-bay","023-ramen-shop-front-bay"],
 "SE-03":["003-shopfront-shutter","001-shopfront-laundrette"],
 "SE-04":["002-shopfront-bookshop","003-shopfront-shutter"],
}
EXTRA_IDS={v[0] for v in EXTRAS}


def digest(data):
    return hashlib.sha256(data).hexdigest()


def glb_scene(path):
    payload=path.read_bytes()
    if payload[:4]!=b'glTF' or len(payload)<20:
        raise ValueError('Not GLB: '+str(path))
    length,kind=struct.unpack_from('<I4s',payload,12)
    if kind!=b'JSON':
        raise ValueError('Missing GLB JSON chunk')
    gltf=json.loads(payload[20:20+length])
    positions=[gltf['accessors'][p['attributes']['POSITION']]
        for m in gltf['meshes'] for p in m['primitives']]
    if not all(p.get('normalized') and p.get('componentType')==5122 for p in positions):
        raise ValueError('Unexpected quantization: review source before use')
    scene=trimesh.load(path,force='scene',process=False)
    for mesh in scene.geometry.values():
        mesh.vertices=np.clip(mesh.vertices/32767.0,-1.,1.)
    if np.ptp(scene.bounds,axis=0).max()>100:
        raise ValueError('Mesh positions not normalized')
    return scene


def index(scene):
    parents={child:parent for parent,child,_ in scene.graph.to_edgelist()}
    groups=defaultdict(list)
    for node in scene.graph.nodes_geometry:
        groups[parents.get(node,node)].append(node)
    return groups


def tr(v):
    M=np.eye(4);M[:3,3]=v;return M


def ry(deg):
    t=math.radians(deg);M=np.eye(4)
    M[0,0]=M[2,2]=math.cos(t)
    M[0,2]=math.sin(t);M[2,0]=-math.sin(t)
    return M


def fit_group(dst,source,groups,group,name,at,yaw=0,width=None,height=None,turn=0):
    if group not in groups:
        raise KeyError('Unknown CC0 module '+group)
    R=ry(turn); corners=[]
    for node in groups[group]:
        tf,g=source.graph[node]
        corners.append(trimesh.transform_points(
            trimesh.bounds.corners(source.geometry[g].bounds),R@tf))
    pts=np.concatenate(corners);lo=pts.min(axis=0);hi=pts.max(axis=0)
    size=hi-lo;center=(lo+hi)/2
    scales=np.diag([
        width/size[0] if width else 1.,
        height/size[1] if height else 1.,
        1.,1.])
    T=tr(at)@ry(yaw)@scales@R@tr([-center[0],-lo[1],-center[2]])
    prefix='I' if source is SOURCE_IZ else 'C'
    for node in groups[group]:
        tf,g=source.graph[node]
        shared=prefix+'__'+g
        if shared not in dst.geometry:
            dst.geometry[shared]=source.geometry[g]
        dst.graph.update(frame_to=name+'/'+node,frame_from=dst.graph.base_frame,
                         matrix=T@tf,geometry=shared)


def colored_box(dst,frame,id,pos,size,mat):
    mesh=trimesh.creation.box(extents=size)
    mesh.visual=TextureVisuals(material=mat)
    dst.add_geometry(mesh,node_name=id,geom_name=id,transform=frame@tr(pos))


def building_shell(dst,shop):
    id,block,title,x,z,yaw,w,d,h=shop
    frame=tr([x,0,z])@ry(yaw)
    swatches=[[171,112,102,255],[107,141,142,255],
              [190,173,143,255],[115,119,145,255]]
    color=swatches[sum(map(ord,id))%4]
    walls=PBRMaterial(name='CC0_Overlay_Authored_Wall',baseColorFactor=color,roughnessFactor=.8)
    roof=PBRMaterial(name='CC0_Overlay_Authored_Roof',baseColorFactor=[61,70,80,255],roughnessFactor=.84)
    glass=PBRMaterial(name='CC0_Overlay_Window',baseColorFactor=[65,106,123,235],roughnessFactor=.22)
    trim=PBRMaterial(name='CC0_Overlay_Trim',baseColorFactor=[216,211,194,255],roughnessFactor=.66)
    base='V03/INFILL/'+id+'/'
    def box(n,p,s,mat):
        colored_box(dst,frame,base+n,p,s,mat)
    box('slab',(0,.12,d/2),(w,.24,d),walls)
    box('back',(0,h/2,d-.13),(w-.2,h,.26),walls)
    box('front_upper',(0,(h+3.65)/2,.13),(w-.2,h-3.65,.26),walls)
    for side in (-1,1):
        box('side_'+str(side),(side*(w/2-.13),h/2,d/2),(.26,h,d),walls)
        for k,zz in enumerate((d*.3,d*.7)):
            box('side_window_'+str(side)+'_'+str(k),
                (side*(w/2+.018),h*.68,zz),(.05,1.2,1.14),glass)
            box('side_sill_'+str(side)+'_'+str(k),
                (side*(w/2+.05),h*.68-.64,zz),(.12,.09,1.4),trim)
    for k in range(3):
        box('rear_window_'+str(k),(-w*.28+k*w*.28,h*.68,d+.025),(1.1,1.2,.065),glass)
    box('roof',(0,h+.15,d/2),(w+.34,.30,d+.35),roof)
    box('awning',(0,3.4,-.46),(w+.4,.14,1.4),roof)
    box('awning_lip',(0,3.32,-1.1),(w+.4,.09,.12),trim)
    box('sign',(0,3.85,-.12),(w-.45,.60,.17),trim)
    for j in range(2):
        box('roof_HVAC_'+str(j),(-w*.24+j*w*.48,h+.54,d*.62),(.8,.6,.7),trim)


def asphalt_pbr(srcdir):
    color=Image.open(srcdir/'asphalt_diffuse_2k.jpg').convert('RGB')
    normal=Image.open(srcdir/'asphalt_normal_gl_2k.png').convert('RGB')
    rough=np.asarray(Image.open(srcdir/'asphalt_roughness_2k.png').convert('RGB'))
    packed=np.zeros((2048,2048,3),dtype=np.uint8)
    packed[:,:,0]=255
    packed[:,:,1]=rough[:,:,1]
    return PBRMaterial(name='CC0_Asphalt01_PBR',baseColorTexture=color,
        normalTexture=normal,metallicRoughnessTexture=Image.fromarray(packed),
        baseColorFactor=[192,207,223,255],metallicFactor=0.,roughnessFactor=.76)


def asphalt_overlays(dst,asphalt_dir):
    mat=asphalt_pbr(asphalt_dir)
    zones=[('EW_'+str(z),(-63,63,z-4.5,z+4.5)) for z in (-45,0,45)]
    segments=[(-63,-49.5),(-40.5,-4.5),(4.5,40.5),(49.5,63)]
    for x in (-45,0,45):
        for a,b in segments:
            zones.append((f'NS_{x}_{a}',(x-4.5,x+4.5,a,b)))
    for name,(x0,x1,z0,z1) in zones:
        mesh=trimesh.Trimesh(vertices=[[x0,-.055,z0],[x1,-.055,z0],
            [x1,-.055,z1],[x0,-.055,z1]],faces=[[0,2,1],[0,3,2]],process=False)
        uv=[[0,0],[(x1-x0)/4,0],[(x1-x0)/4,(z1-z0)/4],[0,(z1-z0)/4]]
        mesh.visual=TextureVisuals(uv=uv,material=mat)
        dst.add_geometry(mesh,node_name='V03/ROAD_PBR/'+name,
                         geom_name='ROAD_PBR_'+name)
    return len(zones)


def build(root,out):
    layout=json.loads((root/'Docs/StreetExterior/V02_GRID_LAYOUT.json').read_text(encoding='utf-8'))
    base=[(s['id'],s['block'],s['name'],s['front_xyz'][0],
        s['front_xyz'][2],s['yaw_degrees'],*s['nominal_footprint_m'],
        s['nominal_height_m']) for s in layout['shops']]
    shops=base+EXTRAS
    assert len(shops)==24 and len({s[0] for s in shops})==24
    source=root/'ArtSource/ThirdParty/CC0'
    izpath=source/'IzakayaAlley/izakaya_starter_scene.glb'
    citypath=source/'JapaneseSchoolCity/school_city_starter_scene.glb'
    iz=glb_scene(izpath)
    city=glb_scene(citypath)
    global SOURCE_IZ
    SOURCE_IZ=iz
    izindex=index(iz);cityindex=index(city)
    scene=trimesh.Scene()
    for s in shops:
        id,block,title,x,z,yaw,w,d,h=s
        if id in EXTRA_IDS:
            building_shell(scene,s)
        if id!='NE-01':
            modules=FACADES.get(id,['001-shopfront-laundrette','002-shopfront-bookshop'])
            frame=tr([x,0,z])@ry(yaw)
            for bay,grp in enumerate(modules):
                localx=(-.5 if bay==0 else .5)*w*.49
                point=(frame@np.array([localx,.02,.22,1]))[:3]
                izmodule='front-bay' in grp
                fit_group(scene,iz if izmodule else city,izindex if izmodule else cityindex,
                    grp,'V03/CC0_FACADE/'+id+'/'+str(bay),point,
                    yaw=yaw,width=w*.49,height=3.56,turn=90 if izmodule else 0)
                upper=['004-upper-floor-wall-balcony','005-upper-floor-wall-balcony'][bay]
                point=(frame@np.array([localx,3.58,.27,1]))[:3]
                fit_group(scene,city,cityindex,upper,
                    'V03/CC0_UPPER/'+id+'/'+str(bay),point,
                    yaw=yaw,width=w*.49,height=max(2.6,h-3.65))
        frame=tr([x,0,z])@ry(yaw)
        modules=[
            (city,cityindex,'068-bicycle-commuter',(-w*.33,0,-2)),
            (iz,izindex,'110-a-frame-pavement-sign',(w*.33,0,-1.55)),
            (iz,izindex,'105-potted-plant-in-can',(-w*.44,0,-.95)),
            (iz,izindex,'111-paper-lantern-round',(w*.34,2.18,-1.15)),
        ]
        if id.endswith(('01','04')):
            modules.append((city,cityindex,'065-vending-machine',(w*.4,0,-2.25)))
        for k,(source_scene,source_groups,group,loc) in enumerate(modules):
            point=(frame@np.array([*loc,1]))[:3]
            fit_group(scene,source_scene,source_groups,group,
                f'V03/CC0_PROP/{id}/{k}',point,yaw=yaw)
    for block,x,z in [('NW',-22.5,22.5),('NE',22.5,22.5),
                      ('SW',-22.5,-22.5),('SE',22.5,-22.5)]:
        for k,dx in enumerate((-5.2,5.2)):
            fit_group(scene,city,cityindex,'073-cherry-tree-blossom',
                f'V03/CC0_COURT/{block}/tree{k}',[x+dx,.05,z])
        fit_group(scene,city,cityindex,'067-bicycle-rack',
            f'V03/CC0_COURT/{block}/bike',[x,0,z+3.4])
        fit_group(scene,city,cityindex,'071-flower-bed',
            f'V03/CC0_COURT/{block}/flower',[x,0,z-4])
    n_asphalt=asphalt_overlays(scene,source/'Asphalt01')
    nodes=set(scene.graph.nodes_geometry)
    checks={
        '23_facades_plus_SORA_MART_original':sum(any(n.startswith(
            'V03/CC0_FACADE/'+s[0]+'/') for n in nodes) for s in shops)==23,
        '24_prop_zones':sum(any(n.startswith('V03/CC0_PROP/'+s[0]+'/')
                            for n in nodes) for s in shops)==24,
        '8_complete_infill_shells':sum(any(n.startswith(
            'V03/INFILL/'+s[0]+'/') for n in nodes) for s in EXTRAS)==8,
        '4_courtyard_groups':all(any(n.startswith('V03/CC0_COURT/'+b+'/')
                         for n in nodes) for b in ('NW','NE','SW','SE')),
        '15_asphalt_PBR_sections':n_asphalt==15,
        'no_AFX_meshes':not any('AFX_KIT' in n or 'AFX_PROP' in n for n in nodes),
    }
    if not all(checks.values()):
        raise AssertionError(checks)
    out.mkdir(parents=True,exist_ok=True)
    binary=scene.export(file_type='glb')
    file=out/'SORA_V03_CC0_Overlay.glb'
    file.write_bytes(binary)
    report={
        'version':'V03','type':'CC0+authored_overlay_only',
        'overlay_world_alignment':'same origin as private V02',
        'license':'CC0 external meshes + original SORA geometry',
        'files':[{'path':file.name,'sha256':digest(binary),'bytes':len(binary)}],
        'input_sha256':{p.name:digest(p.read_bytes()) for p in (izpath,citypath)},
        'storefronts':23,'SORA_MART_visual_in_base':True,
        'additional_authored_shells':len(EXTRAS),
        'mesh_instances':len(nodes),'checks':checks,
        'not_tested':['Unity import','Unity URP shaders','Play Mode','collisions',
                      'rain particles','FPS and Windows build'],
    }
    (out/'manifest.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    print('SUCCESS:',file,len(binary),'bytes;',len(nodes),'mesh instances',checks,flush=True)


if __name__=='__main__':
    p=argparse.ArgumentParser()
    p.add_argument('--root',type=Path,default=Path('.'))
    p.add_argument('--output',type=Path,required=True)
    a=p.parse_args()
    build(a.root.resolve(),a.output.resolve())
