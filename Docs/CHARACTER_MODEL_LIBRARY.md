# Thư viện model nhân vật

Ngày nhập: **09/11/2026**. Unity **6000.3.7f1**, URP **17.3.0**.

Sáu gói do chủ dự án cung cấp trong `3d model charater/` và toàn bộ thư viện Unity tại
`Assets/SORA/Characters/LocalImported/` được đưa lên GitHub theo yêu cầu trực tiếp
của chủ dự án ngày 09/11/2026. ZIP, FBX và texture dùng **Git LFS**; prefab, Avatar,
material, mesh rig Rover, controller và `.meta` cũng đi kèm. Không cần chạy Blender
hoặc giải nén ZIP để sử dụng prefab đã chuẩn bị.

## Nạp thư viện từ GitHub

Trước khi asset được tích hợp vào `main`, chọn nhánh có đủ model:

```powershell
git lfs install
git clone --branch feature/protagonist-model-selection https://github.com/haizzdungnay/SORA-After-the-Rain.git
cd SORA-After-the-Rain
git lfs pull
```

Nếu đã có checkout trên nhánh này, chạy `git pull` rồi `git lfs pull`. Kiểm tra
`git lfs fsck` trước khi mở Unity. ZIP gốc khoảng 185 MB, thư viện Unity khoảng
258 MB trước khi Git LFS loại nội dung trùng. Download ZIP của GitHub có thể chỉ
chứa pointer LFS; dùng Git và `git lfs pull` để chắc chắn nhận đủ binary.

Mở project bằng Unity **6000.3.7f1**, đợi import rồi Play scene prototype.
Nguồn đã giải nén, cache, log và bản phục hồi Unity vẫn là dữ liệu local.

## Chọn model cho hai nhân vật chính

1. Mở scene `Assets/SORA/Scenes/SORA_Prototype.unity`, nhấn **Play**.
2. Menu chọn ngoại hình mở đầu phiên chơi. Toshiharu chọn **Male Rover/Zhongli**;
   Akiko chọn **Chisa/Citlali/Denia/Lynae**. Tên, tuổi và vai trò hai nhân vật giữ nguyên.
3. Bấm model hoặc dùng **1–2** cho nam, **3–6** cho nữ theo thứ tự trên menu.
   **Enter/Esc** tiếp tục chơi; **M** mở lại lựa chọn; **Tab** đổi người điều khiển.
4. **F5** lưu cả hai ngoại hình cùng tiến trình; **F9** khôi phục. Đổi ngoại hình
   không tự ghi đè save. Save schema 1 cũ không có model vẫn đọc được, dùng ngoại
   hình prototype. Máy thiếu model local dùng fallback và báo rõ khi load.

Mặc định khi có thư viện playable: **Zhongli + Chisa**. Menu tạm dừng di chuyển
của cả hai; mở menu hủy POS draft an toàn. Chọn model không respawn controller,
không reset tiền, hàng, vị trí hoặc quan hệ. Camera và CharacterController đổi
chiều cao theo model.

| Model | Nhân vật sử dụng | Chiều cao |
|---|---|---:|
| Chisa | Tei Akiko | 165 cm |
| Citlali | Tei Akiko | 152 cm |
| Denia | Tei Akiko | 158 cm |
| Lynae | Tei Akiko | 165 cm |
| Male Rover | Tei Toshiharu | 180 cm |
| Zhongli | Tei Toshiharu | 180 cm |

Chiều cao đo trên geometry ở tư thế chuẩn, gồm tóc/phụ kiện, chân tại Y = 0;
không kéo giãn riêng từng bộ phận. Root prefab giữ Scale = 1.

## Dùng prefab có chuyển động

Prefab sẵn cho gameplay nằm tại:
`Assets/SORA/Characters/LocalImported/Resources/SoraCharacters/<ID>.prefab`.
Mỗi prefab có Humanoid Avatar và controller **Idle/Walk/Run**, tham số **Speed**
tính bằng m/s. Ví dụ: **0** đứng, **1.25** đi, **3.05** di chuyển nhanh,
**5** chạy Shift. Controller dùng lại clip do dự án tạo trong
`Tools/Blender/sweater_girl_rig.py`, không tải animation từ game nguồn.

Kéo prefab này vào scene để dùng làm visual; nếu tự tạo NPC, điều khiển tham số
`Animator.SetFloat("Speed", speed)`. **Apply Root Motion = false**; controller
gameplay chịu trách nhiệm di chuyển. Prefab model không có collider hoặc AI.

Nếu các nút ghi **chưa nạp**, dừng Play, chạy `git lfs pull` rồi đợi Unity import.
Các prefab playable đã nằm trong Git. Khi cần tạo lại từ FBX đã chuẩn bị, chạy
**SORA > Characters > 4. Build Playable Model Library**; menu cập nhật thư viện gốc
đúng chiều cao, rồi tạo Avatar/prefab playable. Chỉ cần pipeline Blender cuối tài
liệu khi muốn chuẩn bị lại FBX từ ZIP gốc.

## Thư viện nguồn để nghiên cứu trong Unity

1. Mở project bằng Unity **6000.3.7f1** và đợi import hoàn tất.
2. Trong cửa sổ **Project**, mở `Assets/SORA/Characters/LocalImported`.
3. Mở thư mục nhân vật, kéo **file `.prefab`** vào Scene/Hierarchy.
4. Đặt Position/Rotation theo scene; giữ Scale của prefab root là **1, 1, 1**.
5. Dùng **SORA > Characters > 2. Validate Local Model Library** để kiểm tra mesh,
   material URP, texture, vị trí chân và chiều cao.

| Nhân vật | Prefab | Định dạng nguồn |
|---|---|---|
| Chisa | `Assets/SORA/Characters/LocalImported/Chisa/Chisa.prefab` | USDZ, chuyển FBX bằng Blender |
| Citlali | `Assets/SORA/Characters/LocalImported/Citlali/Citlali.prefab` | glTF trong ZIP lồng, chuyển FBX |
| Denia | `Assets/SORA/Characters/LocalImported/Denia/Denia.prefab` | FBX binary gốc |
| Lynae | `Assets/SORA/Characters/LocalImported/Lynae/Lynae.prefab` | FBX binary trong ZIP lồng |
| Male Rover | `Assets/SORA/Characters/LocalImported/MaleRover/MaleRover.prefab` | FBX ASCII trong RAR lồng |
| Zhongli | `Assets/SORA/Characters/LocalImported/Zhongli/Zhongli.prefab` | glTF trong ZIP lồng, chuyển FBX |

Các prefab trong bảng là bản nghiên cứu nguồn, chiều cao theo bảng lựa chọn,
chân tại Y = 0, chưa có controller. Dùng prefab trong thư mục **Resources** phía
trên để có chuyển động. Không đổi canonical GDD hoặc thay logic cửa hàng.

## Rig, chuyển động và biểu cảm

- **Không gói nguồn nào có animation clip.** Năm model có rig Generic; FBX nguồn
  **Male Rover là 28 mesh tĩnh, không có skin/bone**. Pipeline playable tạo thêm
  rig 21 bone và skin theo vùng cho Rover; FBX/ZIP gốc không bị sửa.
- Bản playable map Humanoid cho cả sáu, chỉnh tay/chân về tư thế chuẩn rồi dùng
  clip của dự án. Chisa dùng bone `n*`; Citlali dùng chuỗi chân D/EX có skin và
  nối lại nhánh spine/hips kiểu MMD. Giữ rig phụ và blendshape gốc.
- Mở rộng file `.fbx` trong Project để xem các sub-asset mesh, Avatar và clip thực
  sự được Unity nhập. Danh sách chính xác được ghi trong báo cáo bên dưới.
- Với Denia: chọn model con có **Skinned Mesh Renderer**, mở **BlendShapes** rồi
  thử từng weight từ 0 đến 100. Blendshape là biến dạng biểu cảm, không phải clip
  chuyển động toàn thân.
- `PlayableAvatar.asset` nằm cạnh FBX; Avatar Humanoid tạo bằng API Unity, không
  cần sửa Rig của FBX nguồn. Mesh có skin mới của Rover là `PlayableMesh_*.asset`.
- Chạy lại menu Build sẽ ghi lại asset do importer quản lý nhưng giữ GUID. Giữ
  chỉnh sửa art riêng trong prefab variant/material riêng.
- Controller playable bật **Foot IK** có sẵn của Unity khi retarget. Chưa dò nền
  dốc/bậc bằng raycast hoặc khóa chân hoàn chỉnh; vẫn còn trượt/chìm chân nhẹ khi
  chạy. Skin Rover phục vụ locomotion cơ bản, chưa rig ngón tay. Chưa có cloth/tóc
  vật lý, chuyển động tương tác POS, nhảy hoặc facial animation tự động.

Số liệu nguồn ở lần nhập ban đầu (cột Clip không tính clip dùng lại ở bản playable):

| Model | Mesh | Tam giác | Bone được skin tham chiếu | Blendshape | Clip |
|---|---:|---:|---:|---:|---:|
| Chisa | 7 | 95.786 | 373 | 0 | 0 |
| Citlali | 22 | 43.187 | 330 | 0 | 0 |
| Denia | 1 | 101.834 | 403 | 187 | 0 |
| Lynae | 8 | 103.474 | 405 | 0 | 0 |
| Male Rover | 28 | 34.574 | 0 | 0 | 0 |
| Zhongli | 4 | 16.884 | 185 | 0 | 0 |

Bone được skin tham chiếu không phải tổng node hierarchy. Ví dụ Blender đọc
skeleton Denia có 483 bone; Unity skin sử dụng 403 bone. Chưa đo hiệu năng runtime;
các model nhiều bone/tam giác cần đánh giá trước khi dùng nhiều NPC đồng thời.

## Material và texture

- Material **URP/Lit** nằm trong `<nhân vật>/Materials/`; base color được remap
  vào FBX. Material hai mặt để giữ các mặt mỏng như tóc/phụ kiện.
- Texture nguồn và base-color trích xuất nằm trong `<nhân vật>/Textures/`.
  Lynae có cả normal và các map dữ liệu riêng như FTM, RGID, HM, ID; các map này
  được giữ để nghiên cứu, không tự coi chúng là metallic/roughness chuẩn URP.
- Citlali có hai mesh mặt trùng vị trí: lớp phủ dùng atlas biểu cảm RGB che mặt
  da mặc định. Material của lớp phủ (`material`, map `Base_00.png`) được giữ nhưng
  đặt transparent alpha = 0 mặc định; mesh và texture không bị xóa. Chỉnh lớp này
  trong material riêng nếu muốn nghiên cứu biểu cảm.
- Pipeline hiện tại phục vụ nhập/thử model bằng base color; chưa tái tạo shader
  toon, outline, vật lý tóc/váy, cloth hoặc biểu cảm tự động của game nguồn.
- Dùng **SORA > Characters > 3. Capture Local Model Preview** để tạo ảnh render
  từ Unity Editor tại `Artifacts/CharacterIntake/unity-character-library.png`.
  Thứ tự trái sang phải: Chisa, Citlali, Denia, Lynae, Male Rover, Zhongli.
  Đây là ảnh preview Editor, không phải bằng chứng Play Mode.

## Nguồn và quyền sử dụng

| Gói | Bằng chứng đi kèm | Trạng thái |
|---|---|---|
| Chisa | ZIP chủ dự án, không có license.txt | Chưa xác minh quyền phân phối |
| Citlali | `license.txt` của kimvinh040276, ghi CC BY 4.0 | Giữ attribution; cần xác minh quyền đối với IP Genshin Impact |
| Denia | ZIP chủ dự án, không có license.txt | Chưa xác minh quyền phân phối |
| Lynae | ZIP có tên `by_IcePaper`, FBX, PSKX và texture; không có license.txt | Tên gói chưa đủ để xác nhận tác giả/quyền |
| Male Rover | RAR chứa FBX và texture; không có license.txt | Chưa xác minh quyền phân phối |
| Zhongli | `license.txt` của SLDDL, ghi CC BY 4.0 | Giữ attribution; cần xác minh quyền đối với IP Genshin Impact |

Chủ dự án yêu cầu đưa cả nguồn, asset Unity và ảnh minh họa lên GitHub sau khi
được thông báo trạng thái quyền chưa xác minh. Yêu cầu này thay đổi phạm vi bản
đẩy, không xác nhận quyền của tác giả hoặc cấp giấy phép thương mại cho model.
CC BY do người tải model ghi không tự chứng minh người đó nắm quyền cấp phép IP
của game nguồn. Bảng trên vẫn ghi đúng bằng chứng hiện có; Git LFS **không thay
thế giấy phép**.

Credit nguyên văn do gói Citlali cung cấp:

> This work is based on "Genshin impact: Citlail" (https://sketchfab.com/3d-models/genshin-impact-citlail-d099ba4ebeec4366a05bed6e0a583b51) by kimvinh040276 (https://sketchfab.com/kimvinh040276) licensed under CC-BY-4.0 (http://creativecommons.org/licenses/by/4.0/)

Credit nguyên văn do gói Zhongli cung cấp:

> This work is based on "zhongli from genshin impact" (https://sketchfab.com/3d-models/zhongli-from-genshin-impact-aac0ffcadd6948b1800aa5b26360dcc2) by SLDDL (https://sketchfab.com/simon.masse200) licensed under CC-BY-4.0 (http://creativecommons.org/licenses/by/4.0/)

License gốc cũng được giữ tại `source-license.txt` trong thư mục nhân vật tương ứng.
Nguồn và SHA-256: [báo cáo nhập](CHARACTER_MODEL_IMPORT_REPORT_2026-11-09.md).
Sổ nguồn chung: [ASSET_REGISTER.csv](ASSET_REGISTER.csv).

## Tạo lại thư viện từ ZIP gốc trong repo

1. Giải nén mỗi ZIP từ `3d model charater/` vào `LocalReferenceAssets/Characters/<ID>/`, với ID đúng như
   bảng prefab. Giải nén tiếp ZIP/RAR trong thư mục `source/`; giữ texture và
   license. Mỗi `<ID>` cần đúng một FBX, glTF hoặc USDZ nguồn. Không giải nén USDZ
   thành các bản model thứ hai trong cùng cây thư mục.
2. Male Rover cần công cụ giải RAR đầy đủ, đọc được tên Unicode. `tar` trên máy
   nhập gặp lỗi hai tên texture; bản 7-Zip đầy đủ đã giải được cả 25 file.
   Bản 7-Zip rút gọn trong Unity không hỗ trợ RAR.
3. Đóng Unity trong khi chuẩn bị file. Chạy Blender có hỗ trợ USD/glTF/FBX:

   ```powershell
   & 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python-exit-code 1 --python Tools/Blender/prepare_character_models.py -- --sources LocalReferenceAssets/Characters
   ```

4. Mở Unity, chọn **SORA > Characters > 4. Build Playable Model Library**. Unity tự
   tạo metadata, material, Avatar và prefab. Không xóa `.meta` khi nhập lại hoặc chuyển
   máy; sao lưu/chuyển asset cùng `.meta` để giữ GUID.
5. Chạy menu Validate; kiểm tra báo cáo tại
   `Artifacts/CharacterIntake/unity-import-report.json`. Log, báo cáo và ảnh mới
   trong Artifacts được Git bỏ qua; ảnh minh họa đã chọn nằm tại
   `Docs/Media/Changelog/sora-character-*.png` và được hiển thị trong changelog.

Không cài thêm Unity package hoặc Python package. Script chuyển định dạng dùng
Python và các importer/exporter có sẵn trong Blender. Giữ `.meta` và file nguồn
cùng nhau khi chuyển máy để Unity nhận đúng GUID.

Chi tiết thay đổi lựa chọn/rig và kết quả kiểm thử:
[CHARACTER_SELECTION_REPORT_2026-11-09.md](CHARACTER_SELECTION_REPORT_2026-11-09.md).
