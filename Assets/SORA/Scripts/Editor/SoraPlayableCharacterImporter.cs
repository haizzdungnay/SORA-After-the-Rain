#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sora.AfterTheRain.Demo.Editor
{
    public static class SoraPlayableCharacterImporter
    {
        const string Root = SoraLocalCharacterImporter.Root;
        const string Output = Root + "/Resources/" + SoraCharacterModels.ResourceFolder;

        [MenuItem("SORA/Characters/4. Build Playable Model Library")]
        public static void Import()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before rebuilding character assets.");
            SoraLocalCharacterImporter.Import();
            BuildFromLibrary();
        }

        public static void BuildFromLibrary()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before rebuilding character assets.");
            var source = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/SORA/Characters/SweaterGirl/TeiAkiko.controller");
            if (source == null) throw new InvalidOperationException("Import the authored Akiko locomotion first.");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Root + "/PlayableLocomotion.controller")
                ?? AnimatorController.CreateAnimatorControllerAtPath(Root + "/PlayableLocomotion.controller");
            controller.parameters = new[] { new AnimatorControllerParameter { name = "Speed", type = AnimatorControllerParameterType.Float } };
            var machine = controller.layers[0].stateMachine;
            var state = machine.defaultState ?? machine.AddState("Locomotion");
            machine.defaultState = state;
            state.motion = source.layers[0].stateMachine.defaultState.motion;
            state.iKOnFeet = true;
            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(controller);
            Directory.CreateDirectory(Output);
            foreach (var model in SoraCharacterModels.All)
            {
                var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + model.Id + "/" + model.Id + ".prefab"));
                try
                {
                    instance.name = model.Id + " Playable";
                    foreach (var old in instance.GetComponentsInChildren<Animator>()) Object.DestroyImmediate(old);
                    var map = model.Id == "MaleRover" ? RigRover(instance, model.Height) : MapBones(instance, model.Id);
                    foreach (string side in new[] { "Left", "Right" })
                    {
                        var foot = map[side + "Foot"];
                        Quaternion footRotation = foot.rotation;
                        Align(map[side + "UpperLeg"], map[side + "LowerLeg"], Vector3.down);
                        Align(map[side + "LowerLeg"], foot, Vector3.down);
                        foot.rotation = footRotation;
                        Vector3 direction = side == "Left" ? Vector3.left : Vector3.right;
                        Align(map[side + "UpperArm"], map[side + "LowerArm"], direction);
                        Align(map[side + "LowerArm"], map[side + "Hand"], direction);
                    }
                    var bounds = SoraLocalCharacterImporter.MeshBounds(instance);
                    float scale = model.Height / bounds.size.y;
                    foreach (Transform child in instance.transform)
                    {
                        child.localPosition = (child.localPosition - Vector3.up * bounds.min.y) * scale;
                        child.localScale *= scale;
                    }
                    var description = new HumanDescription
                    {
                        human = map.Select(p => new HumanBone { humanName = HumanTrait.BoneName[(int)Enum.Parse<HumanBodyBones>(p.Key)], boneName = p.Value.name, limit = new HumanLimit { useDefaultValues = true } }).ToArray(),
                        skeleton = instance.GetComponentsInChildren<Transform>().Select(t => new SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale }).ToArray(),
                        upperArmTwist = 0.5f, lowerArmTwist = 0.5f, upperLegTwist = 0.5f, lowerLegTwist = 0.5f,
                        armStretch = 0.05f, legStretch = 0.05f
                    };
                    var avatar = AvatarBuilder.BuildHumanAvatar(instance, description);
                    if (!avatar.isValid || !avatar.isHuman) throw new InvalidOperationException("Invalid Humanoid avatar: " + model.Id);
                    avatar.name = model.Id + " Humanoid";
                    var animator = instance.AddComponent<Animator>();
                    animator.avatar = SaveAsset(avatar, Root + "/" + model.Id + "/PlayableAvatar.asset");
                    // ponytail: reuse authored Idle/Walk/Run with Unity retarget foot IK; add ground raycasts for uneven terrain.
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    PrefabUtility.SaveAsPrefabAsset(instance, Output + model.Id + ".prefab");
                    Debug.Log("SORA: playable Humanoid ready: " + model.Id + " / " + model.Height + "m / Idle, Walk, Run.");
                }
                finally { Object.DestroyImmediate(instance); }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void Align(Transform bone, Transform child, Vector3 direction)
        {
            bone.rotation = Quaternion.FromToRotation(child.position - bone.position, direction) * bone.rotation;
        }

        static T SaveAsset<T>(T value, string path) where T : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null) { AssetDatabase.CreateAsset(value, path); return value; }
            EditorUtility.CopySerialized(value, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(value);
            return existing;
        }

        static Dictionary<string, Transform> MapBones(GameObject root, string id)
        {
            var transforms = root.GetComponentsInChildren<Transform>();
            var map = new Dictionary<string, Transform>();
            void Add(string human, string name, bool suffix = false)
            {
                map.Add(human, transforms.Single(t => suffix ? t.name.EndsWith(name, StringComparison.Ordinal) : t.name == name));
            }
            if (id == "Chisa")
            {
                Add("Hips", "n15"); Add("Spine", "n16"); Add("Chest", "n52"); Add("UpperChest", "n53"); Add("Neck", "n212"); Add("Head", "n213");
                foreach (var pair in new[] { ("Left", 54, 362), ("Right", 100, 375) })
                {
                    Add(pair.Item1 + "Shoulder", "n" + pair.Item2); Add(pair.Item1 + "UpperArm", "n" + (pair.Item2 + 1));
                    Add(pair.Item1 + "LowerArm", "n" + (pair.Item2 + 2)); Add(pair.Item1 + "Hand", "n" + (pair.Item2 + 3));
                    Add(pair.Item1 + "UpperLeg", "n" + pair.Item3); Add(pair.Item1 + "LowerLeg", "n" + (pair.Item3 + 1));
                    Add(pair.Item1 + "Foot", "n" + (pair.Item3 + 2)); Add(pair.Item1 + "Toes", "n" + (pair.Item3 + 3));
                }
            }
            else if (id == "Citlali")
            {
                Add("Hips", "_010", true); Add("Spine", "_0163", true); Add("Chest", "_0164", true);
                Add("UpperChest", "_0165", true); Add("Neck", "_0166", true); Add("Head", "_0167", true);
                // MMD torso/hips are siblings and only the D/EX leg chain carries skin weights.
                map["Spine"].SetParent(map["Hips"], true);
                foreach (var p in new[] { ("Left", "_0238", "_0240", "_0242", "_0244", "_016", "_017", "_018", "_019"),
                                          ("Right", "_0276", "_0278", "_0280", "_0282", "_026", "_027", "_028", "_029") })
                {
                    Add(p.Item1 + "Shoulder", p.Item2, true); Add(p.Item1 + "UpperArm", p.Item3, true);
                    Add(p.Item1 + "LowerArm", p.Item4, true); Add(p.Item1 + "Hand", p.Item5, true);
                    Add(p.Item1 + "UpperLeg", p.Item6, true); Add(p.Item1 + "LowerLeg", p.Item7, true);
                    Add(p.Item1 + "Foot", p.Item8, true); Add(p.Item1 + "Toes", p.Item9, true);
                }
            }
            else if (id == "Lynae")
            {
                Add("Hips", "Bip001Pelvis"); Add("Spine", "Bip001Spine"); Add("Chest", "Bip001Spine1");
                Add("UpperChest", "Bip001Spine2"); Add("Neck", "Bip001Neck"); Add("Head", "Bip001Head");
                foreach (string side in new[] { "Left", "Right" })
                    foreach (var p in new[] { ("Shoulder", "Clavicle"), ("UpperArm", "UpperArm"), ("LowerArm", "Forearm"), ("Hand", "Hand"),
                                              ("UpperLeg", "Thigh"), ("LowerLeg", "Calf"), ("Foot", "Foot"), ("Toes", "Toe0") })
                        Add(side + p.Item1, "Bip001" + side[0] + p.Item2);
            }
            else
            {
                bool suffix = id == "Zhongli";
                void Match(string human, string name)
                {
                    if (suffix) map.Add(human, transforms.Single(t => t.name.StartsWith(name + "_", StringComparison.Ordinal)));
                    else Add(human, name);
                }
                Match("Hips", "Hips"); Match("Spine", "Spine"); Match("Chest", id == "Denia" ? "Upper Chest" : "Chest");
                Match("Neck", "Neck"); Match("Head", "Head");
                foreach (string side in new[] { "Left", "Right" })
                    foreach (var p in new[] { ("Shoulder", "shoulder"), ("UpperArm", "arm"), ("LowerArm", "elbow"), ("Hand", "wrist"),
                                              ("UpperLeg", "leg"), ("LowerLeg", "knee"), ("Foot", "ankle"), ("Toes", "toe") })
                        Match(side + p.Item1, side + " " + p.Item2);
            }
            return map;
        }

        static Dictionary<string, Transform> RigRover(GameObject root, float height)
        {
            var map = new Dictionary<string, Transform>();
            void Bone(string name, string parent, float x, float y, float z)
            {
                var t = new GameObject(name).transform;
                t.SetParent(parent == null ? root.transform : map[parent], false);
                t.position = root.transform.TransformPoint(new Vector3(x, y, z) * height);
                map.Add(name, t);
            }
            Bone("Hips", null, 0, .57f, .05f); Bone("Spine", "Hips", 0, .64f, .05f);
            Bone("Chest", "Spine", 0, .74f, .05f); Bone("Neck", "Chest", 0, .85f, .05f); Bone("Head", "Neck", 0, .89f, .05f);
            foreach (string side in new[] { "Left", "Right" })
            {
                float sign = side == "Left" ? -1 : 1;
                Bone(side + "Shoulder", "Chest", sign * .035f, .825f, .05f);
                Bone(side + "UpperArm", side + "Shoulder", sign * .11f, .815f, .05f);
                Bone(side + "LowerArm", side + "UpperArm", sign * .21f, .74f, .05f);
                Bone(side + "Hand", side + "LowerArm", sign * .30f, .66f, .05f);
                Bone(side + "UpperLeg", "Hips", sign * .055f, .57f, .05f);
                Bone(side + "LowerLeg", side + "UpperLeg", sign * .055f, .32f, .045f);
                Bone(side + "Foot", side + "LowerLeg", sign * .055f, .07f, .035f);
                Bone(side + "Toes", side + "Foot", sign * .055f, .025f, .105f);
            }
            var bones = map.Values.ToArray();
            var indices = map.Keys.Select((n, i) => (n, i)).ToDictionary(p => p.n, p => p.i);
            int index = 0;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                var mesh = Object.Instantiate(filter.sharedMesh);
                var vertices = mesh.vertices;
                var weights = new BoneWeight[vertices.Length];
                // ponytail: region skinning for Rover's supplied A-pose; replace weights with artist skinning for fingers/cloth.
                bool head = renderer.bounds.center.y > height * .84f || filter.name == "bianzi1" || filter.name == "\u8fab\u5b502";
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 p = root.transform.InverseTransformPoint(filter.transform.TransformPoint(vertices[i])) / height;
                    string side = p.x < 0 ? "Left" : "Right";
                    string a, b;
                    float t;
                    if (head) { a = b = "Head"; t = 0; }
                    else if (Mathf.Abs(p.x) > .105f && p.y > .59f)
                    {
                        Vector3 elbow = root.transform.InverseTransformPoint(map[side + "LowerArm"].position) / height;
                        float reach = Mathf.Abs(p.x);
                        if (reach < Mathf.Abs(elbow.x)) { a = "Chest"; b = side + "UpperArm"; t = Mathf.InverseLerp(.10f, .14f, reach); }
                        else { a = side + "LowerArm"; b = side + "Hand"; t = Mathf.InverseLerp(.275f, .31f, reach); }
                        if (Mathf.Abs(reach - Mathf.Abs(elbow.x)) < .035f) { a = side + "UpperArm"; b = side + "LowerArm"; t = Mathf.InverseLerp(.175f, .245f, reach); }
                    }
                    else if (p.y < .61f)
                    {
                        if (p.y < .12f) { a = side + "Foot"; b = side + "LowerLeg"; t = Mathf.InverseLerp(.07f, .13f, p.y); }
                        else if (p.y < .50f) { a = side + "LowerLeg"; b = side + "UpperLeg"; t = Mathf.InverseLerp(.28f, .36f, p.y); }
                        else { a = side + "UpperLeg"; b = "Hips"; t = Mathf.InverseLerp(.50f, .60f, p.y); }
                    }
                    else if (p.y < .72f) { a = "Hips"; b = "Spine"; t = Mathf.InverseLerp(.60f, .70f, p.y); }
                    else if (p.y < .84f) { a = "Spine"; b = "Chest"; t = Mathf.InverseLerp(.72f, .80f, p.y); }
                    else { a = "Neck"; b = "Head"; t = Mathf.InverseLerp(.84f, .88f, p.y); }
                    t = Mathf.SmoothStep(0, 1, t);
                    weights[i] = new BoneWeight { boneIndex0 = indices[a], weight0 = 1 - t, boneIndex1 = indices[b], weight1 = t };
                }
                mesh.boneWeights = weights;
                mesh.bindposes = bones.Select(b => b.worldToLocalMatrix * filter.transform.localToWorldMatrix).ToArray();
                var materials = renderer.sharedMaterials;
                var skin = filter.gameObject.AddComponent<SkinnedMeshRenderer>();
                skin.sharedMesh = SaveAsset(mesh, Root + "/MaleRover/PlayableMesh_" + (index++) + ".asset");
                skin.bones = bones;
                skin.rootBone = map["Hips"];
                skin.sharedMaterials = materials;
                skin.updateWhenOffscreen = true;
                if (renderer != null) Object.DestroyImmediate(renderer);
                Object.DestroyImmediate(filter);
            }
            return map;
        }
    }
}
#endif
