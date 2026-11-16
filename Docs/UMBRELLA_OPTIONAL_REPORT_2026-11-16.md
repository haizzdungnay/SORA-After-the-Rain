# Optional storefront umbrellas — 2026-11-16

Both twins now start empty-handed. Approach the rack outside SORA MART and press E to take an umbrella; E at the rack returns it. Rain alone never equips one. A borrowed umbrella retains existing automatic rain opening and roof folding.

## Changes

- `SoraDemoWorld`: move existing rack meshes and their attached collision from the interior window to `(-1.90, 0, -6.15)`, beside the entrance. Reuse two original folded umbrella props; keep a primitive rack fallback when authored rack art is unavailable. Original interior decorative umbrellas are merged into generic goods geometry and remain unchanged.
- `SoraDemoCore`: reuse nearest-target E interaction and dynamic take/return prompt. Rack use safely cancels a pending sale without changing cash/stock. Append the interaction enum member, preserving existing values.
- `SoraDemoCharacter` and `SoraCharacterUmbrella`: character-owned choice survives model replacement and motion resets. Unequipped prop is hidden, both arm/finger overrides have zero weight, and hand IK/grip adjustment are inactive. Empty-handed idle variations are restored.
- Schema-1 JSON gains two optional Boolean fields, one per twin. Save copies both live choices; load restores them even when model IDs are unchanged. Missing fields in old saves mean no umbrella.
- Extend existing EditMode/native Play Mode checks, controls and manual QA checklist. No new Unity dependency, regenerated third-party binary, changed rig, changed controller or asset GUID.

## Verification

Unity `6000.3.7f1`, URP `17.3.0`, Windows x64, Direct3D 11.

| Check | Result |
|---|---|
| Regression before fix | New empty-hand check fails because the prop starts active |
| Public EditMode with graphics | 69/69 passed, zero skipped |
| Owner EditMode with graphics | 69/69 passed, zero skipped |
| Final public native Play Mode | 326 checks passed, zero runtime errors |
| Final owner native Play Mode | 326 checks passed, zero runtime errors |
| Public Windows x64 build | Passed; actual Editor build log confirms success |
| Owner Windows x64 build | Passed; actual Editor build log confirms success |
| Public Direct3D 11 standalone startup | Passed; store/city/weather initialized |
| Owner Direct3D 11 standalone startup | Passed; owner city/store/weather initialized, zero exception lines |

First headless EditMode run passed 66 checks and skipped the existing wet-road rendering test because no graphics device was present. The subsequent Direct3D 11 run passed all 69 tests, including both authored/fallback rack placement and wet-road rendering.

Native checks cover the real external rack/ground support, nearest E target and take/return prompts, each twin's choice, model menu/replacement, same-model save/load, return while open on all six models, roof/lifecycle behavior, actual puddle splash/audio and 100 POS-safe switches. Read-only code review found no P1/P2 issues.

![Actual Unity capture: empty hands beside the outside rack](Media/Changelog/sora-umbrella-optional-rack.png)

Local evidence under `Artifacts/OptionalUmbrella/`: regression XML/log, final EditMode XML/log, native Play Mode log/report, build/startup logs and owner preservation hashes/backups. Native Play Mode uses real Animator, CharacterController, weather, roof, puddle contact and store state. Rack interaction is invoked through the same core method by the harness; it does not certify physical keyboard input.

Owner preservation audit: 995 unrelated baseline files remain byte-identical; 12 intentionally changed files match the scoped integration. No baseline file was deleted. Unity's generated analytics-symbol edit was restored; original project settings, existing GUIDs and owner-only assets remain intact. No task-owned Editor or player process remains running.

Offline validation: public 410 unique GUIDs / 1,228 tracked files; owner 414 GUIDs / 880 tracked files. The owner checkout still contains pre-existing uncommitted integration work; no unrelated owner files are committed by this follow-up.

## Limits

- Manual E/Tab/F5/F9 walkthrough, final grip/art review and target-PC profiling remain pending.
- Rack props are decorative; there is no finite umbrella inventory or rental charge. Each twin can equip at most one. No bespoke rack pickup/put-down clip was added.
- Existing bounded secondary motion and visual umbrella limitations remain: no cloth simulation, prop collider or per-character rain-particle occlusion.
- Editor startup Search-index, entitlement and TLS messages remain separate from runtime acceptance errors.

Plan: [UMBRELLA_OPTIONAL_PLAN_2026-11-16.md](UMBRELLA_OPTIONAL_PLAN_2026-11-16.md). Earlier animation evidence: [CHARACTER_WEATHER_MOTION_REPORT_2026-11-13.md](CHARACTER_WEATHER_MOTION_REPORT_2026-11-13.md).
