# SORA prototype design notes v0.1

## Architecture

- `SoraDemoCore` la singleton runtime, so huu shared state, inventory, POS, characters list, interaction state.
- `SoraDemoCharacter` la mot controller cho ca nguoi choi va inactive NPC; chuyen mode khong respawn.
- `SoraThirdPersonCamera` bam current character, right-mouse orbit.
- `SoraInteractable` la marker simple identifier; runtime nearest detection.
- `SoraDemoWorld` sinh khung canh game tu primitives de khong can model ngoai.
- `SoraSaveData` la schema 1, JSON thong qua `JsonUtility` va `Application.persistentDataPath`.
- `SoraDemoSceneGenerator` tao empty authored scene va them bootstrap MonoBehaviour.

## POS transaction integrity

- `HasPendingSale` KHONG thay doi inventory hoac cash.
- Phim E lan tiep theo da co draft thi `HasPendingSale = false`, tru 1 shelf, +380 cash, +1 sales.
- Tab va Esc huy draft; khong co duong nao commit 2 lan voi 1 draft.
- Luu game huy draft truoc serialize; load dat draft = false.
- Luu y: Prototype KHONG co transactional persistence atomic / sale IDs / event journal; phai bo sung idempotency ledger truoc khi mo rong.

## Social stat isolation

- `familiarity[characterId * 3 + customerId]` va `trust[...]`.
- Chua co thoi gian hoat dong khach, pheromone/affection hay romance, chi la spike quan he.

## Runtime scene note

Ban dau `SORA_Prototype.unity` khong co mesh. `Start()` se sinh world. Nut Play la bat buoc de thay. Khi xong spike, phai chuyen generated object thanh prefab va scene authored de artist lam viec trong Editor va bake lighting.

## Visual pipeline future

1. 2 avatar placeholder -> licensed anime 3D FBX/VRM + Animator, blend tree locomotion, face/eye expression.
2. Store placeholder -> modular environment meshes, lighting bake, reflection probes, signage, wet surfaces.
3. Camera -> Cinemachine follow + obstacles; add interior/exterior transition, interaction anchor.
4. NPC AI waypoint -> NavMesh or movement graph + work behavior planner.
5. GUI OnGUI prototype -> Unity UI Toolkit or uGUI + TextMeshPro localized VN/JP.
6. Social markers -> quest state machine, dialogue graph, branching content.
7. Save schema migration + atomic write + checkpoint tests.

## Security & IP

No external assets; no proprietary NTR Master content reproduced, and no connected services or network IO. Use only appropriately licensed anime models and environment assets in production. Keep build target PC Windows; mobile optimization later.
