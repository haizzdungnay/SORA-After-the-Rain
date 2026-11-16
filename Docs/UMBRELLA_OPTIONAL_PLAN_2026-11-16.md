# Optional umbrellas at the storefront

Owner request: umbrellas are optional. Approach the rack outside SORA MART and choose to take one; characters must not always carry one.

## Approved behavior

- New games start with both twins empty-handed, including during rain.
- Reuse the existing umbrella rack, moving it outside beside the entrance without blocking the doorway. Put two folded original umbrella props in it; retain the interior decorative stock, which is merged with other goods. Keep a primitive fallback for the original shop.
- Existing nearest-target interaction shows `[E] Lay o` when empty-handed and `[E] Cat o` when carrying. Only E at the rack changes the choice. Menus retain their current input blocking.
- After pickup, keep the existing rain/roof opening and folding behavior. Returning hides the prop and releases both arm and finger animation overrides immediately.
- Each twin owns its own choice. Character switching, model selection and motion resets preserve it. F5/F9 save and restore it; old schema-1 saves with missing Boolean fields default to no umbrella.
- Keep POS, inventory, relationships, weather and locomotion contracts. Rack lending is free and limited to one equipped umbrella per twin; rack contents remain decorative rather than a separate stock system.

## Implementation plan

- [x] Add a character-owned Boolean and setter; persist two Boolean fields in `SoraSaveData`. Reapply through existing model binding and load reset.
- [x] Gate prop visibility, arm/finger layers, hand IK, grip adjustment and idle restrictions on that Boolean in `SoraCharacterUmbrella` and `SoraCharacterMotion`.
- [x] Append `UmbrellaStand` to `InteractKind`; add take/return and dynamic prompt in `SoraDemoCore`. Reuse E and existing nearest-target range.
- [x] Relocate the existing stand meshes and colliders in `SoraDemoWorld`, register its external interaction anchor and retain a simple fallback. No regenerated third-party binaries.
- [x] Extend existing EditMode and native Play Mode checks: empty hands in rain, pickup, rain/roof transitions, return while open, model/switch/lifecycle retention, old/new saves and POS cancellation. Adapt the six-model umbrella review to explicit pickup.
- [x] Run offline validator, actual Unity 6000.3.7f1 EditMode, native Play Mode and Windows build/startup. Capture actual storefront interaction and preserve all owner-only edits during integration.
- [x] Update controls/report and existing PR #36; leave PR open.

## Limits and review points

No new dependency, inventory currency, finite rack stock, bespoke pickup clip or new save schema. Existing umbrella opening, grip and locomotion clips are retained. Real keyboard/art QA remains distinct from scripted Play Mode evidence. Verify the external anchor remains reachable and clear of the central doorway on both authored and fallback shops.
