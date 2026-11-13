#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Editor
{
    /// <summary>
    /// Rebuilds the shared Humanoid locomotion and interruptible idle/jump/turn states in place.
    /// </summary>
    public static class SoraAnimationControllerBuilder
    {
        public const string ClipSource = "Assets/SORA/Characters/SweaterGirl/sweater_girl.fbx";
        const string ControllerPath = SoraLocalCharacterImporter.Root + "/PlayableLocomotion.controller";

        // Yaw rate (deg/s) that counts as a deliberate pivot rather than steering while walking.
        const float TurnThreshold = 100f;

        /// <summary>A condition-driven transition fires as soon as its conditions hold, with no exit time.</summary>
        const float NoExit = -1f;

        /// <summary>Animator parameters the controller exposes, in the order SoraDemoCharacter writes them.</summary>
        public static readonly (string Name, AnimatorControllerParameterType Type)[] Parameters =
        {
            ("Speed", AnimatorControllerParameterType.Float),
            ("IsGrounded", AnimatorControllerParameterType.Bool),
            ("VerticalSpeed", AnimatorControllerParameterType.Float),
            ("Jump", AnimatorControllerParameterType.Trigger),
            ("Turn", AnimatorControllerParameterType.Float), // signed yaw rate; positive = to the character's right
            ("GaitRate", AnimatorControllerParameterType.Float),
            ("IdleAction", AnimatorControllerParameterType.Trigger),
            ("IdleVariant", AnimatorControllerParameterType.Int),
            ("MoveStart", AnimatorControllerParameterType.Trigger),
            ("MoveStop", AnimatorControllerParameterType.Trigger),
            ("StopFoot", AnimatorControllerParameterType.Int),
            ("Moving", AnimatorControllerParameterType.Bool),
            ("MotionBlocked", AnimatorControllerParameterType.Bool),
            ("UmbrellaOpen", AnimatorControllerParameterType.Bool),
            ("UmbrellaBusy", AnimatorControllerParameterType.Bool),
            ("UmbrellaAdjust", AnimatorControllerParameterType.Trigger),
        };

        [MenuItem("SORA/Characters/5. Upgrade Motion Library")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before upgrading motion.");
            SoraUmbrellaBuilder.Build();
            SoraSweaterGirlImporter.Import();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            var paths = SoraCharacterModels.All.Select(m => SoraLocalCharacterImporter.Root + "/Resources/" + SoraCharacterModels.ResourceFolder + m.Id + ".prefab")
                .Append("Assets/SORA/Characters/Resources/" + SoraDemoWorld.AkikoModelResource + ".prefab");
            foreach (string path in paths)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                var prefab = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var animator = prefab.GetComponent<Animator>();
                    if (animator == null || !animator.isHuman) throw new InvalidOperationException("Humanoid missing: " + path);
                    var description = animator.avatar.humanDescription;
                    if (AddRightFingers(prefab, ref description, Path.GetFileNameWithoutExtension(path)))
                    {
                        var avatar = AvatarBuilder.BuildHumanAvatar(prefab, description);
                        if (!avatar.isValid || !avatar.isHuman) throw new InvalidOperationException("Finger avatar invalid: " + path);
                        avatar.name = animator.avatar.name;
                        EditorUtility.CopySerialized(avatar, animator.avatar);
                        EditorUtility.SetDirty(animator.avatar);
                        UnityEngine.Object.DestroyImmediate(avatar);
                    }
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    if (prefab.GetComponent<SoraCharacterMotion>() == null) prefab.AddComponent<SoraCharacterMotion>();
                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(prefab); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("SORA MOTION: upgraded shared controller and playable/fallback prefabs without replacing GUIDs.");
        }

        // Existing chains verified in Artifacts/UmbrellaMotion/rigs.json; source meshes/rest transforms stay intact.
        public static bool AddRightFingers(GameObject model, ref HumanDescription description, string id)
        {
            if (id == "MaleRover" || id == "TeiAkiko") return false; // These rigs have no separate fingers.
            var transforms = model.GetComponentsInChildren<Transform>();
            var mapping = description.human.ToList();
            int before = mapping.Count;
            string[] fingers = { "Thumb", "Index", "Middle", "Ring", "Little" };
            string[] joints = { "Proximal", "Intermediate", "Distal" };
            for (int finger = 0; finger < fingers.Length; finger++)
                for (int joint = 0; joint < 3; joint++)
                {
                    string name = HumanTrait.BoneName[(int)Enum.Parse<HumanBodyBones>("Right" + fingers[finger] + joints[joint])];
                    if (mapping.Any(b => b.humanName == name)) continue;
                    string source = id == "Chisa" ? "n" + (104 + finger * 3 + joint)
                        : id == "Citlali" ? "_" + (284 + finger * 4 + joint).ToString("D4")
                        : id == "Lynae" ? "Bip001RFinger" + finger + (joint == 0 ? "" : joint.ToString())
                        : fingers[finger] + (finger == 0 ? "" : "Finger") + (joint + (finger == 0 ? 0 : 1)) + "_R";
                    var bone = transforms.FirstOrDefault(t => id == "Citlali" ? t.name.EndsWith(source, StringComparison.Ordinal)
                        : t.name == source || t.name.StartsWith(source + "_", StringComparison.Ordinal));
                    if (bone == null) throw new InvalidOperationException("Missing verified finger: " + id + "/" + source);
                    mapping.Add(new HumanBone { humanName = name, boneName = bone.name, limit = new HumanLimit { useDefaultValues = true } });
                }
            description.human = mapping.ToArray();
            return mapping.Count != before;
        }

        public static void InspectRigs()
        {
            Directory.CreateDirectory("Artifacts/UmbrellaMotion");
            var facts = new List<RigFact>();
            foreach (var model in SoraCharacterModels.All)
            {
                var instance = UnityEngine.Object.Instantiate(model.Prefab);
                try
                {
                    var animator = instance.GetComponent<Animator>();
                    var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    facts.Add(new RigFact { model = model.Id, bones = hand.GetComponentsInChildren<Transform>().Select(b =>
                        new BoneFact { name = b.name, parent = b.parent != null ? b.parent.name : "", at = hand.InverseTransformPoint(b.position) }).ToArray(),
                        otherBones = instance.GetComponentsInChildren<SkinnedMeshRenderer>().SelectMany(s => s.bones).Where(b => b != null).Select(b => b.name).Distinct().ToArray() });
                }
                finally { UnityEngine.Object.DestroyImmediate(instance); }
            }
            File.WriteAllText("Artifacts/UmbrellaMotion/rigs.json", JsonUtility.ToJson(new RigFacts { models = facts.ToArray() }, true));
        }
        [Serializable] private class BoneFact { public string name, parent; public Vector3 at; }
        [Serializable] private class RigFact { public string model; public BoneFact[] bones; public string[] otherBones; }
        [Serializable] private class RigFacts { public RigFact[] models; }

        public static void ConfigureClips()
        {
            var importer = AssetImporter.GetAtPath(ClipSource) as ModelImporter
                ?? throw new InvalidOperationException("Missing motion FBX: " + ClipSource);
            importer.clipAnimations = importer.defaultClipAnimations.Select(c =>
            {
                c.name = c.takeName.Split('|').Last();
                c.loopTime = c.loopPose = new[] { "Idle", "Walk", "Jog", "Run", "Fall", "SitIdle", "UmbrellaCarry", "UmbrellaHold" }.Contains(c.name);
                c.lockRootRotation = c.lockRootHeightY = c.lockRootPositionXZ = true;
                c.keepOriginalOrientation = c.keepOriginalPositionY = c.keepOriginalPositionXZ = true;
                c.events = new[] { "Walk", "Jog", "Run" }.Contains(c.name)
                    ? new[] { new AnimationEvent { functionName = "Footstep", time = .25f, intParameter = 0, messageOptions = SendMessageOptions.DontRequireReceiver },
                              new AnimationEvent { functionName = "Footstep", time = .75f, intParameter = 1, messageOptions = SendMessageOptions.DontRequireReceiver } }
                    : new[] { "MoveStart", "StopLeft", "StopRight" }.Contains(c.name)
                        ? new[] { new AnimationEvent { functionName = "Footstep", time = .8f, intParameter = c.name == "StopLeft" ? 1 : 0, messageOptions = SendMessageOptions.DontRequireReceiver } }
                        : Array.Empty<AnimationEvent>();
                return c;
            }).ToArray();
            importer.SaveAndReimport();
        }

        /// <summary>Idempotent: reuses the existing controller asset so its GUID and every prefab reference survive.</summary>
        public static AnimatorController Build()
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(ClipSource).OfType<AnimationClip>()
                .GroupBy(c => c.name).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
            AnimationClip Clip(string name) => clips.TryGetValue(name, out var clip) ? clip
                : throw new InvalidOperationException($"{ClipSource} has no clip '{name}'. Re-run Tools/Blender/build_sweater_girl.py, then add the take to clipAnimations in sweater_girl.fbx.meta.");

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath)
                ?? AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            foreach (string name in new[] { "Idle", "Walk", "Jog", "Run", "Jump", "Fall", "Land", "TurnLeft", "TurnRight", "IdleLookAround", "IdleStretch",
                "MoveStart", "StopLeft", "StopRight", "UmbrellaCarry", "UmbrellaRaise", "UmbrellaHold", "UmbrellaLower", "UmbrellaAdjust" }) Clip(name);
            controller.parameters = Parameters.Select(p => new AnimatorControllerParameter
                { name = p.Name, type = p.Type, defaultFloat = p.Name == "GaitRate" ? 1 : 0 }).ToArray();

            var machine = controller.layers[0].stateMachine;
            machine.anyStateTransitions = Array.Empty<AnimatorStateTransition>();
            machine.entryTransitions = Array.Empty<AnimatorTransition>();
            foreach (var sub in machine.stateMachines) machine.RemoveStateMachine(sub.stateMachine);
            foreach (var child in machine.states)
            {
                if (child.state.motion != null && AssetDatabase.GetAssetPath(child.state.motion) != ControllerPath)
                    child.state.motion = null;
                machine.RemoveState(child.state);
            }
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<BlendTree>()) UnityEngine.Object.DestroyImmediate(old, true);
            var speedBlend = new BlendTree { name = "Locomotion speed", blendParameter = "Speed", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(speedBlend, controller);
            speedBlend.children = new[]
            {
                new ChildMotion { motion = Clip("Idle"), threshold = 0, timeScale = 1 },
                new ChildMotion { motion = Clip("Walk"), threshold = 1.25f, timeScale = 1.25f / 1.16f },
                new ChildMotion { motion = Clip("Jog"), threshold = 3.05f, timeScale = 3.05f / 1.9f },
                new ChildMotion { motion = Clip("Run"), threshold = 5, timeScale = 5f / 2.42f }
            };

            // Foot IK on every state whose feet are meant to meet the floor; off while airborne.
            var locomotion = State(machine, "Locomotion", speedBlend, new Vector3(300, 0), ik: true);
            var jump = State(machine, "Jump", Clip("Jump"), new Vector3(600, -120), ik: false);
            var fall = State(machine, "Fall", Clip("Fall"), new Vector3(840, -120), ik: false);
            var land = State(machine, "Land", Clip("Land"), new Vector3(600, 120), ik: true);
            var turnLeft = State(machine, "TurnLeft", Clip("TurnLeft"), new Vector3(60, -140), ik: true);
            var turnRight = State(machine, "TurnRight", Clip("TurnRight"), new Vector3(60, 140), ik: true);
            var look = State(machine, "IdleLookAround", Clip("IdleLookAround"), new Vector3(300, 240), ik: true);
            var stretch = State(machine, "IdleStretch", Clip("IdleStretch"), new Vector3(300, 360), ik: true);
            var start = State(machine, "MoveStart", Clip("MoveStart"), new Vector3(300, -280), ik: true);
            var stopLeft = State(machine, "StopLeft", Clip("StopLeft"), new Vector3(600, 280), ik: true);
            var stopRight = State(machine, "StopRight", Clip("StopRight"), new Vector3(840, 280), ik: true);
            machine.defaultState = locomotion;
            locomotion.speedParameter = "GaitRate";
            locomotion.speedParameterActive = true;
            var layers = controller.layers;
            layers[0].iKPass = true;
            controller.layers = layers;

            // Physics validates jump/coyote/buffer. It may already report airborne when the trigger is consumed.
            var launch = machine.AddAnyStateTransition(jump);
            launch.hasExitTime = false;
            launch.duration = .045f;
            launch.hasFixedDuration = true;
            launch.canTransitionToSelf = false;
            launch.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
            launch.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            var airborne = machine.AddAnyStateTransition(fall);
            airborne.hasExitTime = false;
            airborne.duration = .08f;
            airborne.hasFixedDuration = true;
            airborne.canTransitionToSelf = false;
            airborne.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
            airborne.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            airborne.AddCondition(AnimatorConditionMode.Less, -.8f, "VerticalSpeed");
            Link(jump, land, .08f, NoExit, ("IsGrounded", AnimatorConditionMode.If, 0), ("VerticalSpeed", AnimatorConditionMode.Less, -.2f));
            Link(fall, land, .08f, NoExit, ("IsGrounded", AnimatorConditionMode.If, 0));
            Link(land, locomotion, .06f, NoExit, ("Speed", AnimatorConditionMode.Greater, .2f));
            Link(land, locomotion, .12f, .65f);
            Link(locomotion, turnLeft, .1f, NoExit, ("Speed", AnimatorConditionMode.Less, .8f), ("Turn", AnimatorConditionMode.Less, -TurnThreshold), ("IsGrounded", AnimatorConditionMode.If, 0));
            Link(locomotion, turnRight, .1f, NoExit, ("Speed", AnimatorConditionMode.Less, .8f), ("Turn", AnimatorConditionMode.Greater, TurnThreshold), ("IsGrounded", AnimatorConditionMode.If, 0));
            Link(turnLeft, locomotion, .12f, .85f);
            Link(turnRight, locomotion, .12f, .85f);
            foreach (var turn in new[] { turnLeft, turnRight })
                Link(turn, locomotion, .06f, NoExit, ("Speed", AnimatorConditionMode.Greater, .15f));
            foreach (var idle in new[] { (look, 1), (stretch, 2) })
            {
                Link(locomotion, idle.Item1, .2f, NoExit, ("IdleAction", AnimatorConditionMode.If, 0),
                    ("IdleVariant", AnimatorConditionMode.Equals, idle.Item2), ("IsGrounded", AnimatorConditionMode.If, 0), ("Speed", AnimatorConditionMode.Less, .08f));
                Link(idle.Item1, locomotion, .06f, NoExit, ("IdleVariant", AnimatorConditionMode.Equals, 0));
                Link(idle.Item1, locomotion, .08f, NoExit, ("Speed", AnimatorConditionMode.Greater, .08f));
                Link(idle.Item1, locomotion, .08f, NoExit, ("Turn", AnimatorConditionMode.Greater, 20));
                Link(idle.Item1, locomotion, .08f, NoExit, ("Turn", AnimatorConditionMode.Less, -20));
                Link(idle.Item1, locomotion, .18f, .95f);
            }

            Link(locomotion, start, .04f, NoExit, ("MoveStart", AnimatorConditionMode.If, 0), ("IsGrounded", AnimatorConditionMode.If, 0), ("Moving", AnimatorConditionMode.If, 0));
            Link(start, locomotion, .04f, NoExit, ("Moving", AnimatorConditionMode.IfNot, 0));
            var beginGait = Link(start, locomotion, .06f, .85f);
            beginGait.offset = .25f;
            foreach (var source in new[] { locomotion, start })
            {
                Link(source, stopLeft, .05f, NoExit, ("MoveStop", AnimatorConditionMode.If, 0), ("StopFoot", AnimatorConditionMode.Equals, 0), ("Moving", AnimatorConditionMode.IfNot, 0));
                Link(source, stopRight, .05f, NoExit, ("MoveStop", AnimatorConditionMode.If, 0), ("StopFoot", AnimatorConditionMode.Equals, 1), ("Moving", AnimatorConditionMode.IfNot, 0));
            }
            foreach (var stop in new[] { stopLeft, stopRight })
            {
                Link(stop, locomotion, .12f, .95f);
                Link(stop, locomotion, .04f, NoExit, ("Moving", AnimatorConditionMode.If, 0));
            }
            foreach (var action in new[] { start, stopLeft, stopRight, turnLeft, turnRight, look, stretch, land })
                Link(action, locomotion, .04f, NoExit, ("MotionBlocked", AnimatorConditionMode.If, 0));

            if (controller.layers.Length == 1) controller.AddLayer("Umbrella");
            var upper = controller.layers[1].stateMachine;
            upper.anyStateTransitions = Array.Empty<AnimatorStateTransition>();
            foreach (var child in upper.states) { child.state.motion = null; upper.RemoveState(child.state); }
            var carry = State(upper, "UmbrellaCarry", Clip("UmbrellaCarry"), new Vector3(0, 0), false);
            var raise = State(upper, "UmbrellaRaise", Clip("UmbrellaRaise"), new Vector3(250, -100), false);
            var hold = State(upper, "UmbrellaHold", Clip("UmbrellaHold"), new Vector3(500, 0), false);
            var lower = State(upper, "UmbrellaLower", Clip("UmbrellaLower"), new Vector3(250, 100), false);
            var adjust = State(upper, "UmbrellaAdjust", Clip("UmbrellaAdjust"), new Vector3(750, 0), false);
            upper.defaultState = carry;
            Link(carry, raise, .08f, NoExit, ("UmbrellaOpen", AnimatorConditionMode.If, 0));
            Link(raise, hold, .06f, .95f);
            Link(raise, lower, .08f, NoExit, ("UmbrellaOpen", AnimatorConditionMode.IfNot, 0));
            Link(hold, lower, .08f, NoExit, ("UmbrellaOpen", AnimatorConditionMode.IfNot, 0));
            Link(lower, carry, .06f, .95f);
            Link(lower, raise, .08f, NoExit, ("UmbrellaOpen", AnimatorConditionMode.If, 0));
            Link(hold, adjust, .12f, NoExit, ("UmbrellaAdjust", AnimatorConditionMode.If, 0), ("UmbrellaBusy", AnimatorConditionMode.IfNot, 0));
            Link(adjust, hold, .12f, .95f);
            Link(adjust, hold, .08f, NoExit, ("UmbrellaBusy", AnimatorConditionMode.If, 0));
            Link(adjust, lower, .08f, NoExit, ("UmbrellaOpen", AnimatorConditionMode.IfNot, 0));
            var mask = AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<AvatarMask>().FirstOrDefault(m => m.name == "Umbrella right arm only");
            if (mask == null) { mask = new AvatarMask { name = "Umbrella right arm only" }; AssetDatabase.AddObjectToAsset(mask, controller); }
            for (int part = 0; part < (int)AvatarMaskBodyPart.LastBodyPart; part++) mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)part, false);
            foreach (var part in new[] { AvatarMaskBodyPart.RightArm, AvatarMaskBodyPart.RightFingers, AvatarMaskBodyPart.RightHandIK })
                mask.SetHumanoidBodyPartActive(part, true);
            layers = controller.layers;
            layers[1].avatarMask = mask;
            layers[1].defaultWeight = 0;
            layers[1].iKPass = true;
            controller.layers = layers;

            // Source rig has no finger bones. Native Humanoid muscle curves retarget the grip where fingers exist.
            if (controller.layers.Length == 2) controller.AddLayer("UmbrellaGrip");
            var gripClip = AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<AnimationClip>().FirstOrDefault(c => c.name == "UmbrellaGrip");
            if (gripClip == null) { gripClip = new AnimationClip { name = "UmbrellaGrip" }; AssetDatabase.AddObjectToAsset(gripClip, controller); }
            foreach (string finger in new[] { "Thumb", "Index", "Middle", "Ring", "Little" })
                for (int joint = 1; joint <= 3; joint++)
                    gripClip.SetCurve("", typeof(Animator), $"RightHand.{finger}.{joint} Stretched", AnimationCurve.Constant(0, 1, finger == "Thumb" ? -.3f : -.65f));
            var gripSettings = AnimationUtility.GetAnimationClipSettings(gripClip);
            gripSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(gripClip, gripSettings);
            var gripMask = AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<AvatarMask>().FirstOrDefault(m => m.name == "Umbrella fingers only");
            if (gripMask == null) { gripMask = new AvatarMask { name = "Umbrella fingers only" }; AssetDatabase.AddObjectToAsset(gripMask, controller); }
            for (int part = 0; part < (int)AvatarMaskBodyPart.LastBodyPart; part++) gripMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)part, false);
            gripMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            var gripMachine = controller.layers[2].stateMachine;
            foreach (var child in gripMachine.states) { child.state.motion = null; gripMachine.RemoveState(child.state); }
            gripMachine.defaultState = State(gripMachine, "UmbrellaGrip", gripClip, Vector3.zero, false);
            layers = controller.layers;
            layers[2].avatarMask = gripMask; layers[2].defaultWeight = 0;
            controller.layers = layers;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        static AnimatorState State(AnimatorStateMachine machine, string name, Motion motion, Vector3 position, bool ik)
        {
            var state = machine.AddState(name, position);
            state.motion = motion;
            state.iKOnFeet = ik;
            return state;
        }

        static AnimatorStateTransition Link(AnimatorState from, AnimatorState to, float duration, float exitTime = NoExit,
            params (string Parameter, AnimatorConditionMode Mode, float Threshold)[] conditions)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = exitTime >= 0f;
            transition.exitTime = Mathf.Max(exitTime, 0f);
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.offset = 0f;
            transition.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
            transition.conditions = conditions
                .Select(c => new AnimatorCondition { parameter = c.Parameter, mode = c.Mode, threshold = c.Threshold }).ToArray();
            return transition;
        }
    }
}
#endif
