#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Editor
{
    /// <summary>
    /// Rebuilds the shared Humanoid locomotion and interruptible idle/jump/turn states in place.
    /// </summary>
    // ponytail: starts/stops use acceleration and 0.05 s Animator damping rather than dedicated clips;
    // dedicated clips; author planted-foot stop clips in Tools/Blender/sweater_girl_rig.py when the slide shows.
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
        };

        [MenuItem("SORA/Characters/5. Upgrade Motion Library")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before upgrading motion.");
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

        public static void ConfigureClips()
        {
            var importer = AssetImporter.GetAtPath(ClipSource) as ModelImporter
                ?? throw new InvalidOperationException("Missing motion FBX: " + ClipSource);
            importer.clipAnimations = importer.defaultClipAnimations.Select(c =>
            {
                c.name = c.takeName.Split('|').Last();
                c.loopTime = c.loopPose = new[] { "Idle", "Walk", "Jog", "Run", "Fall", "SitIdle" }.Contains(c.name);
                c.lockRootRotation = c.lockRootHeightY = c.lockRootPositionXZ = true;
                c.keepOriginalOrientation = c.keepOriginalPositionY = c.keepOriginalPositionXZ = true;
                c.events = new[] { "Walk", "Jog", "Run" }.Contains(c.name)
                    ? new[] { new AnimationEvent { functionName = "Footstep", time = .25f, intParameter = 0, messageOptions = SendMessageOptions.DontRequireReceiver },
                              new AnimationEvent { functionName = "Footstep", time = .75f, intParameter = 1, messageOptions = SendMessageOptions.DontRequireReceiver } }
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
            foreach (string name in new[] { "Idle", "Walk", "Jog", "Run", "Jump", "Fall", "Land", "TurnLeft", "TurnRight", "IdleLookAround", "IdleStretch" }) Clip(name);
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

        static void Link(AnimatorState from, AnimatorState to, float duration, float exitTime = NoExit,
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
        }
    }
}
#endif
