# Thư viện model nhân vật local

Ngày nhập: **09/11/2026**. Unity **6000.3.7f1**, URP **17.3.0**.

Sáu gói do chủ dự án cung cấp trong `3d model/` được chuẩn bị tại
`Assets/SORA/Characters/LocalImported/`. Đây là thư viện để kéo thả và thử nghiệm
trên máy đang giữ model. ZIP gốc, nguồn đã giải nén và model nhập **không nằm trong
Git công khai** vì quyền phân phối chưa được xác minh đầy đủ. Clone repo trên máy
khác không tự có sáu model này.

## Dùng ngay trong Unity

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

Prefab có root riêng, chiều cao chuẩn **1,7 m**, chân tại Y = 0. Tỉ lệ và offset
đặt trên model con; root giữ Scale = 1. Model không tự có collider, điều khiển,
NPC AI hoặc logic cửa hàng. Thư viện không tự thay hai nhân vật Tei Toshiharu và
Tei Akiko, không đổi canonical GDD hoặc scene prototype.

## Rig, chuyển động và biểu cảm

- **Không gói nào trong sáu gói có animation clip.** Chisa, Citlali, Denia, Lynae
  và Zhongli có rig; **Male Rover là 28 mesh tĩnh, không có skin/bone** trong FBX
  được cung cấp. Muốn Rover cử động tay/chân phải rig và skin thêm từ nguồn hợp lệ.
- Năm model có rig giữ chế độ **Generic**, hierarchy bone và blendshape. Có rig
  không có nghĩa là gói đã có chuyển động Idle/Walk/Run.
- Mở rộng file `.fbx` trong Project để xem các sub-asset mesh, Avatar và clip thực
  sự được Unity nhập. Danh sách chính xác được ghi trong báo cáo bên dưới.
- Với Denia: chọn model con có **Skinned Mesh Renderer**, mở **BlendShapes** rồi
  thử từng weight từ 0 đến 100. Blendshape là biến dạng biểu cảm, không phải clip
  chuyển động toàn thân.
- Khi cần dùng animation Humanoid: trong **bản sao FBX thử nghiệm**, đổi Rig thành
  **Humanoid**, chọn **Create From This Model**, Apply rồi **Configure** và kiểm
  tra từng bone/T-pose. Chỉ dùng clip Humanoid sau khi Avatar hợp lệ; controller
  Generic không tự retarget clip của Akiko sang skeleton khác.
- Chạy lại menu Build sẽ khôi phục cấu hình Generic và các material do importer
  quản lý. Giữ chỉnh sửa tùy biến trong prefab variant/material riêng.

Số liệu Unity đã nhập:

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

CC BY do người tải model ghi không tự chứng minh người đó nắm quyền cấp phép IP
của game nguồn. Chưa dùng thư viện này làm tài sản phát hành thương mại hoặc đẩy
binary/screenshot model lên repo công khai trước khi làm rõ quyền. Git LFS đã
cấu hình cho FBX/texture nhân vật nhưng **không thay thế giấy phép**.

Credit nguyên văn do gói Citlali cung cấp:

> This work is based on "Genshin impact: Citlail" (https://sketchfab.com/3d-models/genshin-impact-citlail-d099ba4ebeec4366a05bed6e0a583b51) by kimvinh040276 (https://sketchfab.com/kimvinh040276) licensed under CC-BY-4.0 (http://creativecommons.org/licenses/by/4.0/)

Credit nguyên văn do gói Zhongli cung cấp:

> This work is based on "zhongli from genshin impact" (https://sketchfab.com/3d-models/zhongli-from-genshin-impact-aac0ffcadd6948b1800aa5b26360dcc2) by SLDDL (https://sketchfab.com/simon.masse200) licensed under CC-BY-4.0 (http://creativecommons.org/licenses/by/4.0/)

License gốc cũng được giữ tại `source-license.txt` trong thư mục local tương ứng.
Nguồn và SHA-256: [báo cáo nhập](CHARACTER_MODEL_IMPORT_REPORT_2026-11-09.md).
Sổ nguồn chung: [ASSET_REGISTER.csv](ASSET_REGISTER.csv).

## Tạo lại thư viện trên máy có ZIP gốc

1. Giải nén mỗi ZIP vào `LocalReferenceAssets/Characters/<ID>/`, với ID đúng như
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

4. Mở Unity, chọn **SORA > Characters > 1. Build Local Model Library**. Unity tự
   tạo metadata, material và prefab. Không xóa `.meta` khi nhập lại hoặc chuyển
   máy; sao lưu/chuyển asset cùng `.meta` để giữ GUID.
5. Chạy menu Validate; kiểm tra báo cáo tại
   `Artifacts/CharacterIntake/unity-import-report.json`. Hình ảnh, log và báo cáo
   local được Git bỏ qua; mã importer và hướng dẫn nằm trong repo.

Không cài thêm Unity package hoặc Python package. Script chuyển định dạng dùng
Python và các importer/exporter có sẵn trong Blender. Nếu sau này quyền phân
phối đã được duyệt, cập nhật asset register và chính sách ignore trước khi đưa
model vào Git LFS.
