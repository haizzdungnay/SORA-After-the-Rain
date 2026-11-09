# Chọn model hai nhân vật và locomotion - 09/11/2026

## Hành vi mới

Menu chọn ngoại hình mở khi bắt đầu Play; **M** mở lại. Toshiharu dùng hai model
nam, Akiko dùng bốn model nữ. Nhân vật, tên canonical, quan hệ và controller giữ
nguyên; thay visual không respawn gameplay object. Mặc định local là Zhongli và
Chisa. Chọn bằng chuột hoặc phím 1-6, Enter/Esc tiếp tục.

| Model | Slot | Chiều cao geometry ở tư thế chuẩn |
|---|---|---:|
| Chisa | Akiko | 1,65 m |
| Citlali | Akiko | 1,52 m |
| Denia | Akiko | 1,58 m |
| Lynae | Akiko | 1,65 m |
| Male Rover | Toshiharu | 1,80 m |
| Zhongli | Toshiharu | 1,80 m |

Đo cả tóc/phụ kiện; scale đồng đều, chân tại root Y = 0, root Scale = 1. Capsule
và điểm nhìn camera cập nhật theo chiều cao model. Cả hai tạm ngừng di chuyển
khi chọn; mở menu hủy POS draft trước khi thay ngoại hình.

## Mã nguồn và tài sản

- `SoraCharacterModels`: danh sách ID, slot, chiều cao và đường dẫn Resources.
- `SoraDemoCharacter`: giữ visual prototype làm fallback, đổi model con và tham
  chiếu Animator; giữ nguyên CharacterController. Speed tiếp tục nhận vận tốc
  ngang thực tế từ CharacterController cho cả player và AI.
- `SoraDemoCore`: chọn ngoại hình, UI hiện có bằng IMGUI và hai trường
  `toshiharuModel`/`akikoModel` trong JSON schema 1. **F5/F9** lưu/khôi phục hai
  model riêng, không tự ghi đè save khi bấm chọn.
- `SoraDemoWorld`: đánh dấu visual ban đầu của mỗi twin; phần dựng world giữ nguyên.
- `SoraLocalCharacterImporter`: chuẩn hóa chiều cao nguồn theo danh sách mới.
- `SoraPlayableCharacterImporter`: menu **SORA > Characters > 4. Build Playable
  Model Library**, tạo native Avatar/mesh/controller/prefab bằng Unity.

Asset playable ở `Assets/SORA/Characters/LocalImported/Resources/SoraCharacters/`.
Asset nguồn, Avatar, mesh rig mới, controller và preview đều nằm trong vùng local
bị Git bỏ qua. Không sửa ZIP/FBX gốc, không thêm package và không tải animation.

Save cũ không có trường model dùng visual prototype và giữ tiền/kho/quan hệ.
Save chỉ nhận ID đúng slot; model sai hoặc vị trí không hữu hạn bị từ chối trước
khi thay state. Nếu ID hợp lệ nhưng binary local thiếu, dùng fallback và thông
báo, vẫn giữ ID trong save để máy có thư viện có thể khôi phục.

## Rig và chuyển động

| Model | Xử lý playable |
|---|---|
| Chisa | Map bone `n15`, `n16`, `n52`, `n53`, `n212`, `n213` và các chuỗi tay/chân theo vị trí khớp đã đo trong Unity |
| Citlali | Map theo suffix ID vì tên bị mất mã hóa; đưa spine vào dưới hips, dùng chuỗi D/EX thực sự có skin thay cho chuỗi điều khiển chân không có weight |
| Denia | Dùng `Upper Chest` trong chuỗi torso chính; giữ nhánh `Chest` phụ và đủ 187 blendshape |
| Lynae | Map skeleton `Bip001`; giữ các bone phụ và skin gốc |
| Male Rover | Tạo skeleton Humanoid 21 bone và skin hai weight theo vùng cho 28 mesh tĩnh; nguồn không có rig/skin |
| Zhongli | Map các tên `Hips_02`, `Spine_03`, `Chest_04` và chuỗi chi theo prefix |

Tay về T-pose, chân thẳng ở tư thế chuẩn và giữ hướng bàn chân; đo/chuẩn hóa lại
geometry trước khi tạo Avatar. Không đổi riêng tỉ lệ thân thể.

Controller local dùng BlendTree Speed của bộ **Idle/Walk/Run** do dự án tạo tại
`Tools/Blender/sweater_girl_rig.py`, bật **AnimatorState.iKOnFeet**. Không sửa
controller Akiko cũ. Root motion tắt vì CharacterController điều khiển vị trí.
Animator Humanoid, mesh có skin và controller được lưu trong prefab ready-to-use.

## Bằng chứng local

Các log/report/PNG/XML dưới `Artifacts/CharacterSelection`, `Logs` và
`TestResults` là kết quả local; không đưa artwork chưa rõ quyền lên Git công khai.
Hướng dẫn chạy/tạo lại: [CHARACTER_MODEL_LIBRARY.md](CHARACTER_MODEL_LIBRARY.md).

- Unity **6000.3.7f1 / URP 17.3.0**: import/build Avatar, mesh và prefab đã chạy
  thực tế, exit 0. Unity biên dịch code runtime/Editor/tests thành công.
- **15/15 EditMode Passed**, 0 failed/skipped: sáu model đúng chiều cao, không
  collider, Avatar hợp lệ, clip loop, mesh thật biến dạng, Denia đủ blendshape,
  Foot IK bật; lựa chọn đúng slot, save JSON cũ/mới và dữ liệu sai; các test POS,
  inventory, quan hệ và 100 lần switch của prototype vẫn đạt.
  XML: `TestResults/character-selection-editmode.xml`;
  log: `Logs/character-selection-editmode-final.log`.
- **Play Mode tự động đạt**, exit 0: menu mở lúc bắt đầu, mặc định Zhongli/Chisa,
  Akiko AI thực sự di chuyển và đưa vận tốc vào Animator, chọn được cả sáu model,
  giữ ID hai controller và tiền/kho. Mẫu 16 phase/state với controller thật;
  report: `Artifacts/CharacterSelection/playmode-report.json`;
  log: `Logs/character-selection-playmode-gui-v2.log`.
- Đã xem ảnh Game View có UI `playmode-model-selection-upright.png` và các ảnh
  `playmode-<nam>-<nữ>-Idle/Walk/Run.png` trong `Artifacts/CharacterSelection`.
  Đây là capture từ Play Mode; ảnh pose dùng camera đặt để kiểm tra, controller
  di chuyển bị tắt sau bước đo AI. Game View readback gốc được đảo trục Y để hiển thị.
- **291/291 GUID có trước thay đổi được giữ nguyên**, gồm toàn bộ thư viện gốc.
  Không thay package, Unity pin, scene prototype hoặc source animation controller.
- `python Tools/validate_project.py`: **PASS**, 336 GUID duy nhất tại checkout
  có model local, 231 file tracked. `git diff --check`: đạt. Validator offline
  không thay thế các kết quả Unity ghi riêng ở trên.

### Tiếp đất: số đo và phạm vi

Đáy geometry so với **pivot Y = 0**, đơn vị mm; mỗi gait lấy mức thấp nhất của
16 phase trong Animator Play Mode có Foot IK. Số âm không tự chứng minh xuyên
collider sàn: vị trí CharacterController và mặt sàn thực tế còn có offset riêng.

| Model | Idle | Walk | Run |
|---|---:|---:|---:|
| Chisa | -0,1 | -7,3 | -21,1 |
| Citlali | -0,1 | -6,5 | -16,9 |
| Denia | -0,1 | -7,3 | -19,7 |
| Lynae | 0,0 | -7,3 | -20,5 |
| Male Rover | -0,1 | -5,4 | -17,2 |
| Zhongli | -0,1 | -9,0 | -27,0 |

Không phát hiện đổi bên chân theo trục hông trong các mẫu. Không dùng riêng thứ
tự X trong world để kết luận chân chéo vì hips xoay khi bước. Test clip EditMode
bỏ qua state Foot IK và dùng giới hạn bounds 4 cm để bắt biến dạng lớn; không
coi đó là nghiệm thu tiếp đất chính xác. Chưa nghiệm thu foot locking/trượt chân.

## Giới hạn

- Chưa kiểm tra thao tác bàn phím/chuột thủ công đầy đủ, Windows build hoặc FPS
  trên máy đích. Không coi kiểm tra offline là bằng chứng biên dịch Unity.
- Retarget là locomotion prototype. Foot IK native không thay thế dò nền và
  foot locking; vẫn có trượt chân và chìm nhẹ ở vài nhịp chạy. Chưa nghiệm thu
  nền dốc/bậc, cloth/tóc, rig ngón tay Rover, hoạt ảnh POS hoặc nhảy.
- Hai nhân vật dùng lại cùng phong cách gait; chưa chỉnh diễn hoạt riêng cho từng
  ngoại hình. Rover cần artist tinh chỉnh weight trước khi quay cận cảnh.
- Quyền phân phối model game nguồn vẫn chưa xác minh; fresh clone chỉ có code,
  tài liệu và fallback prototype. Không phát hành binary local như asset được cấp phép.
- Nhánh feature nối tiếp `feature/character-model-library` (PR #19), vì dependency
  importer đang chờ review và `develop` còn là baseline cũ. Không merge vào main.
