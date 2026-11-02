# SORA short roadmap and acceptance gates (2026-11-02)

## Gate 0 — Repository bootstrap
- [x] Create PUBLIC repo under the owner's account.
- [x] Push the prepared source and GDD; run static-check CI.
- [ ] Ensure Unity Editor 6000.3.7f1 & required modules are installed and licensed on a Windows PC.
- [ ] Confirm Unity LFS policy and asset rights before adding binary models.

## Gate 1 — First real Unity compile
- [ ] Open project and resolve Package Manager/import errors.
- [ ] Generate demo scene and verify scene asset saved.
- [ ] Character movement, camera rotation/zoom and `Tab` swap work.
- [ ] Inactive character continues to work without teleporting/resetting shared state.
- [ ] POS scan/pay only commits a sale once even if user swaps mid-flow.
- [ ] Run smoke script and record actual Editor logs.

## Gate 2 — Interaction baseline
- [ ] Manual interaction prompts and keyboard controls are discoverable.
- [ ] NPC dialogue interaction is stable.
- [ ] Inventory operations, checkout, end-shift and save/load are consistent.
- [ ] Save and reload after a character swap preserves state.
- [ ] Build and run Windows x64 standalone on a target PC.

## Gate 3 — Visual blockout
- [ ] Anime character asset audit/licensing; replace primitives with legitimate placeholder models.
- [ ] Camera and animation transition pass.
- [ ] Night Tokyo street / convenience-store lighting and wet-road shader prototype.
- [ ] Record in-Editor FPS at chosen PC target and scene screenshot comparisons.

No gate is declared complete by source existence alone.

## Portfolio source milestone — v0.4.0-pre.1

- English, Vietnamese and Japanese READMEs, original identity artwork, architecture and contribution/issue templates prepared.
- Release notes and LinkedIn copy describe a source prototype with explicit Unity acceptance limits.
- Next engineering milestone remains the first real Editor acceptance; repository presentation does not complete gameplay gates.
