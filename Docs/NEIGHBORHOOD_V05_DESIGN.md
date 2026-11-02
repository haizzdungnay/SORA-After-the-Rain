# SORA — Neighborhood v0.5: Connected Tokyo District

**Stage:** spatial blockout for gameplay, NOT production art.
**Reference:** owner-provided 39.5-second rainy convenience store street video.
**Branch:** feature/neighborhood-v05-connected-district.

## 1. What the reference actually shows

The video focuses on a dense convenience-store frontage and nearby wet streets, not a large open-world metropolis. The identifiable quality bar is:
- Traversable public space: curb, safety rail, sidewalk, zebra crossing, side streets.
- Foreground detail: vending machines, bicycles, shop interiors, merchandise, signs.
- A nighttime palette with illuminated windows, wet-road reflections and rain.
- A strong human-scale third-person view of one character entering/exiting the store.

An expanded map alone cannot recreate the reference. Authored materials, models, lighting and animation are still needed.

## 2. Connected street plan

The interactive SORA MART, POS, NPCs, twin switching and save/load remain untouched. The separate SoraNeighborhoodDistrict builder creates the surroundings at Play start.

| District zone | Approximate world coordinates | Intent |
|---|---|---|
| Existing SORA MART | x -7..7, z -5..5 | Interactive hub preserved |
| Extended main avenue | x -84..84, z -15.5 | Connects original street to both side roads |
| West connector | x -47, z -69..36 | New north-south street |
| East connector | x +47, z -69..36 | New north-south street |
| North residential market | z +26 | Residential / local shops |
| South commercial market | z -57 | Services / storefronts |
| Courtyard behind original row | x -14..14, z -42 | Small pedestrian social-space candidate |
| Arcade passages | x approx +/-31 | Pedestrian-scale connecting routes |

Approximate blockout envelope: **168 m east–west and 143 m north–south**.

### Rough map (NOT gameplay capture)

    north edge: HOTEL | RECORDS | BAKERY | CLINIC | LAUNDRY
       +================ NORTH STREET =================+
       |               residential blocks             |
       |   RAMEN       SORA MART       IZAKAYA         |
       +================ MAIN AVENUE =================+
       |       original five frontage buildings       |
       |                 COURTYARD                    |
       +================ SOUTH STREET ================+
    south edge: ARCADE | FLOWERS | BENTO | CINEMA | DRUGSTORE

Vertical side roads at x=-47 and x=+47 connect the three cross streets.
A full loop is geometrically planned but awaits Play Mode traversal verification.
Buildings are solid shells: no new enterable interiors or quests are claimed.

## 3. Implemented in source

- 20 named multi-story facades, windows, shop glazing, signs, awnings, roof units,
  and a further detail pass on the original five opposite-side buildings.
- Connected roads, sidewalks, crosswalk markings, arcades, courtyard and tactile paving.
- Streetlights, vending pairs, bicycles, planters, benches and hanging wires.
- Extra stocked fridge display in the existing convenience store, no extra POS behavior.
- Deterministic procedural asphalt texture, subdued puddle patches.
- Rain particle emitter follows the active twin when walking beyond the old street.
- No third-party FBX, textures or external services introduced.

**Technical limitation:** buildings and props are still GameObject primitives generated during Play. This is not a prefab-authored, production-ready neighborhood. In particular, smooth materials alone do not provide mirror-quality wet asphalt reflections.

## 4. Quality bar against the video

| Need | v0.5 status | Art milestone |
|---|---|---|
| Multi-route city block | Built in C# | Editor/Windows navigation QA |
| Readable human-scale storefronts | Geometric placeholders | Licensed 3D modular storefront prefabs |
| Densely stocked shop | Placeholder stock increased | Designed product models/textures |
| Wet streets | Asphalt noise + puddle approximations | Authored normals, roughness, puddles, reflection capture |
| Rainy neon atmosphere | Existing lamps + procedural emitter | URP exposure/bloom, wet VFX and calibrated lighting |
| Human characters | Existing primitive figures | Rigged anime avatars + locomotion animation |
| Performance | Unverified | GPU/CPU profiler and standalone frame-time measurements |

The goal is to exceed the reference in navigable spatial scope but **not to claim parity in final visual quality** until actual screenshots or gameplay footage are reviewed.

## 5. Mandatory Play Mode acceptance

- [ ] Unity 6000.3.7f1 loads the branch and compiles without errors.
- [ ] Original SORA_Prototype.unity loads and spawns both twins.
- [ ] Walk from SORA MART to west connector, north street, east connector, and back.
- [ ] Reach southern cross street and courtyard without falling or invisible walls.
- [ ] Examine sidewalk seams, crosswalks, balconies, sign orientation and Z-fighting.
- [ ] Move far away from store, switch twins, confirm rain repositions and game remains responsive.
- [ ] Confirm POS, restocking, dialogue, Tab and F5/F9 retain existing behavior.
- [ ] Capture screenshots from street, shop entrance, shop interior, north and south routes.
- [ ] Run the five EditMode tests; save XML and Editor logs.
- [ ] Produce Windows x64 standalone, run on a GPU PC and record FPS/1% lows, draw calls and frame time.

## 6. Next production-quality phase

1. Sign off on walkable block layout, collider boundaries and camera clipping.
2. Convert procedural blockout into prefabs that artists can edit in Unity Editor.
3. Model one hero convenience store with transparent facade and detailed interior.
4. Replace facades with modular anime-styled assets, trim sheets and signs.
5. Build actual URP wet-surface material and appropriate reflections; apply bloom, fog, exposure.
6. Add umbrella pedestrian AI, shop schedules, environmental audio and quest anchors.
7. Profile performance; add instancing, LOD, occlusion culling and VFX distance budgets.

New meshes, textures, fonts and audio must have legitimate redistribution rights and entries in Docs/ASSET_REGISTER.csv. Do not copy game assets or copyrighted video assets.

## 7. Validation policy

Offline validator catches project structure, source wiring and asset GUID hygiene.
It does not compile C# or assess graphics. This branch must remain unmerged until actual Unity acceptance evidence exists.
