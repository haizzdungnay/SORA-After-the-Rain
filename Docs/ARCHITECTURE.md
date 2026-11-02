# Architecture and engineering decisions

## Scope

This document describes the source prototype at `v0.4.0-pre.1`. It is an inspection of code, not evidence of a successful Unity run. The canonical story and design remain in [GDD v0.4](GAME_DESIGN_DOCUMENT_v0.4.md).

## Existing responsibilities

| Component | Responsibility | Current tradeoff |
|---|---|---|
| `SoraDemoCore` | Bootstrap, active-character selection, input dispatch, POS, stock, dialogue, IMGUI and quicksave | Fast to explore; too many responsibilities for production |
| `SoraDemoWorld` | Generate primitive environment, twin controllers, customer markers, camera and rain | No external model dependency; environment exists after Play starts |
| `SoraDemoCharacter` | Player movement or inactive waypoint wandering | No navigation planner or actual background store work |
| `SoraThirdPersonCamera` | Follow selected twin, orbit/zoom and obstacle sphere cast | Needs in-store clipping and framing review |
| `SoraInteractable` | Register/shelf/delivery/customer/shift markers | Nearest-object detection; no line-of-sight or rich interaction arbitration |
| `SoraSaveData` | Shared cash/stock and per-character position/relation arrays | Schema 1; no migration, journal or atomic persistence |
| Editor tools | URP configuration, scene generation, smoke import and Windows build | Requires a licensed pinned Editor; not executed here |

Runtime, Editor automation and EditMode tests have separate assembly definitions. `SoraThirdPersonCamera` is currently declared inside `SoraDemoCharacter.cs`; `SoraInteractable` and `SoraSaveData` are declared inside `SoraDemoCore.cs`. They are not separate services or files yet.

## Character switching

Both twin objects remain in the scene. `SwitchCharacter()` cancels a draft sale, flips the active index, updates control ownership and retargets the camera. `SoraSaveData` remains shared. Customer relationship arrays use `characterIndex * 3 + customerIndex` to isolate progress for the two twins.

The source does not reset store cash, inventory or relation arrays when switching. Inactive behavior is waypoint wandering; production task persistence remains a future requirement.

## Store transaction boundaries

Scanning creates a draft flag without changing stock/cash. A second register interaction clears the draft and performs one synchronous mutation: shelf stock -1, cash +380 yen, sales +1. Switching, Esc and saving cancel a pending draft. Receiving the single demo delivery is guarded by a boolean; restocking moves up to three items from back stock to shelf stock.

These are in-memory prototype guarantees. There are no invoice IDs, durable deduplication records, concurrent transaction handling or crash-safe recovery. The five EditMode tests describe expected behavior but have not been executed in Unity.

## Save and load

Quicksave serializes schema-1 JSON under `Application.persistentDataPath`. Load checks schema and array shapes, restores positions/store/social data and clears transient UI/draft state. It does not fully validate numeric values, persist NPC task timing, migrate old schemas or write atomically. Treat save hardening as a separate engineering milestone.

## Rendering and content

The manifest pins URP 17.3.0; Editor setup creates renderer/pipeline assets if absent, assigns URP and retains runtime shaders. `SoraDemoWorld` creates materials and geometry at runtime. Initial compilation, actual pipeline setup, shader retention in a standalone build and visual quality still require Editor acceptance.

Project identity graphics in `Docs/Media/` are documentation assets, not Unity renders. No licensed anime production models or external game assets are included.

## Planned boundaries

After the first real Unity acceptance, extract store inventory/POS and a persistent invoice ledger; character selection and per-character state; interaction routing; quest/dialogue progression; and validated, atomic, versioned save storage. Preserve asset GUIDs and observable behavior through focused feature PRs. Do not treat these planned boundaries as implemented architecture.

## Verification layers

1. Python structure/JSON/GUID/hygiene checks — runnable without Unity.
2. Five EditMode tests — POS draft cancellation, one sale across 100 swaps, empty-stock rejection, delivery/restock conservation, separate relations; execution pending.
3. Play Mode/manual QA — actual movement, camera, prompts, rendering and save/load; pending.
4. Windows x64 acceptance — build, launch and target-PC profiling; pending.

See [CI setup](CI_SETUP.md), [manual checklist](TEST_CHECKLIST.md) and [technical notes](TECHNICAL_NOTES.md).
