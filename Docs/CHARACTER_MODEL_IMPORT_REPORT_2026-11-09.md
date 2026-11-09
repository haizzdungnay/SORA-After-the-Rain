# Báo cáo nhập model nhân vật - 09/11/2026

## Phạm vi

Nhập sáu ZIP do chủ dự án giữ vào thư viện local `Assets/SORA/Characters/LocalImported/`.
Git công khai chỉ nhận công cụ, metadata mã nguồn, asset register và tài liệu; không nhận model/texture gốc hoặc bản chuyển đổi.
Hướng dẫn dùng: [CHARACTER_MODEL_LIBRARY.md](CHARACTER_MODEL_LIBRARY.md).

## Nguồn và checksum

SHA-256 tính trực tiếp từ ZIP gốc, trước chuyển định dạng.

| ID | ZIP | SHA-256 |
|---|---|---|
| Chisa | `chisa-wuthering-waves-character-graceful-fighter.zip` | `597338e383e22c935e971b40f339fe307f31e8be2519f3a454c1c3744ddd55b8` |
| Citlali | `citlali.zip` | `6e33636924f34b7ace31d331eb574df60118cdf589d4e7e09be4600bd6f78fb0` |
| Denia | `denia-breakdown-form-wuthering-waves.zip` | `ea4b0e106db28d51d02eeca6719c3d18bb2217f6a49469980a1b3d16d6c05936` |
| Lynae | `lynae-from-wuthering-waves.zip` | `0f1e2e13bde28787e8af9b46edfa91fd587f915841475cb05e35f2bf92d4ced0` |
| MaleRover | `male-rover1-02-wuthering-waves.zip` | `cf7bff15f273d349cd731c05e8821e35a707da964f96f90f3adcc83865e2d532` |
| Zhongli | `zhongli.zip` | `252fd4fbda762a4d25b5155ca503a126210146f649914fec5d942d93462ff309` |

Citlali và Zhongli có `license.txt` ghi CC BY 4.0 và credit nguồn Sketchfab; quyền cấp phép IP game vẫn chưa được xác minh. Bốn ZIP còn lại không có giấy phép kèm theo. Toàn bộ binary giữ local, bị Git bỏ qua.

## Số liệu Unity thực tế

Các số dưới đây lấy từ `Artifacts/CharacterIntake/unity-import-report.json`, do Unity Editor tạo sau khi đọc prefab và kiểm tra mesh, material, texture và bounds.
Số bone là các transform khác nhau được `SkinnedMeshRenderer.bones` tham chiếu; không phải tổng node hierarchy.

| ID | Mesh | Vertex | Tam giác | Skinned renderer | Bone tham chiếu | Blendshape | Material có base map / tổng | Clip |
|---|---:|---:|---:|---:|---:|---:|---|---:|
| Chisa | 7 | 64551 | 95786 | 7 | 373 | 0 | 7/7 | 0 |
| Citlali | 22 | 37155 | 43187 | 22 | 330 | 0 | 20/22 | 0 |
| Denia | 1 | 58100 | 101834 | 1 | 403 | 187 | 15/15 | 0 |
| Lynae | 8 | 75831 | 103474 | 8 | 405 | 0 | 8/8 | 0 |
| MaleRover | 28 | 26365 | 34574 | 0 | 0 | 0 | 5/13 | 0 |
| Zhongli | 4 | 13556 | 16884 | 4 | 185 | 0 | 4/4 | 0 |

**Không có animation clip trong cả sáu model.** Năm model có rig Generic; Male Rover là mesh tĩnh không có skin/bone. Denia có 187 blendshape biểu cảm; đây không phải 187 chuyển động.

Hai material Citlali và tám material Male Rover không có base map trong dữ liệu nguồn được nhập; giữ màu material, không tự bịa texture.

## Pipeline

- Giải nén vào `LocalReferenceAssets/Characters/<ID>/`; giữ nguyên ZIP gốc. RAR Rover cần 7-Zip đầy đủ để đọc hai tên texture Unicode mà Windows tar bỏ qua.
- `Tools/Blender/prepare_character_models.py` chuyển Chisa USDZ và Citlali/Zhongli glTF sang FBX, xuất base-color và ghi mapping material. Loại custom bone-shape Icosphere của importer glTF khỏi mesh xuất.
- Denia/Lynae giữ FBX binary nguồn. Rover giữ FBX ASCII, chỉ sửa các đường dẫn texture cũ trong bản sao local; Unity nhập trực tiếp vì Blender không hỗ trợ FBX ASCII.
- `SoraLocalCharacterImporter.Import` tạo 69 material URP/Lit, remap FBX và lưu 6 prefab. Model con được scale/offset; root Scale = 1, chân tại Y = 0 và chiều cao 1,7 m. Không thêm collider hoặc tự gắn gameplay.
- Đo skinned mesh bằng `BakeMesh(mesh, true)` để bù scale trước `TransformPoint`; self-check đã phát hiện và ngăn lỗi áp scale hai lần ở lần nhập đầu.
- Nguồn FBX có transparency factor không tương đương base-color alpha; bản chuẩn bị dùng alpha màu trung tính và giữ alpha trong texture. Shader toon/outline và các data map riêng chưa được port.
- Citlali có mesh mặt phủ atlas biểu cảm RGB trùng vị trí mặt da. Giữ nguyên geometry/texture; material lớp phủ dùng transparent alpha = 0 mặc định để không che mặt chính.

## Kiểm chứng

- Blender 5.2.3 LTS: đã đọc nguồn và chuẩn bị đủ 6 FBX; không cài thêm package.
- Unity 6000.3.7f1 / URP 17.3.0: C# đã biên dịch; import và `SoraLocalCharacterImporter.Validate` đạt cho 6 prefab.
- Ảnh render Editor local: `Artifacts/CharacterIntake/unity-character-library.png`; không phải Play Mode và không đưa ảnh model lên Git công khai khi quyền chưa rõ.
- Unity EditMode: **6/6 Passed**, 0 failed/skipped/inconclusive; XML `TestResults/character-library-editmode.xml`, log `Logs/character-library-editmode.log`. Gồm model/animation Akiko, quan hệ tách riêng, hủy POS draft khi switch, inventory, kệ hết hàng và 100 lần switch không charge trùng.
- Reimport: **238/238 GUID trong thư viện giữ nguyên** khi chuẩn bị/nhập lại; đối chiếu các `.meta` local trước và sau.
- `python Tools/validate_project.py`: **PASS**, gồm 291 GUID duy nhất trong Assets tại máy nhập. Validator chỉ kiểm tra offline; kết quả C#/Unity nêu riêng phía trên.

Điểm vào Unity đã chạy:

```text
-batchmode -nographics -quit -executeMethod Sora.AfterTheRain.Demo.Editor.SoraLocalCharacterImporter.Import -logFile Logs/character-import.log
-batchmode -quit -force-d3d11 -executeMethod Sora.AfterTheRain.Demo.Editor.SoraLocalCharacterImporter.CapturePreview -logFile Logs/character-preview.log
-batchmode -nographics -runTests -testPlatform EditMode -testResults TestResults/character-library-editmode.xml -logFile Logs/character-library-editmode.log
```

Các lệnh dùng `-projectPath` trỏ checkout hiện tại; log local không được commit vì chứa thông tin máy/license.

## Giới hạn

- Chưa xác minh quyền phát hành/phân phối; clone repo không tự có các binary local.
- Rig giữ Generic, chưa retarget Humanoid hoặc tạo Idle/Walk/Run. Rover cần rig/skin nếu muốn chuyển động tay/chân.
- Material dùng base color URP; không coi preview này là nghiệm thu shader game nguồn, facial overlay, tóc/váy, vật lý hoặc chất lượng art cuối.
- Chưa chạy Play Mode, build Windows hoặc đo FPS/draw calls cho thư viện mới. Không thay model hai nhân vật chính, scene prototype, POS, inventory hoặc save/load.
- Nhánh feature dựa trên `origin/main` tại `d27a3e8` vì `origin/develop` vẫn là baseline cũ; đã fetch refs và kiểm tra không có PR mở trước thay đổi. Không merge tự động vào main.
