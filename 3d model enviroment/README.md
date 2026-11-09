# Thư viện model môi trường gốc

Kiểm tra ngày 09/11/2026. Chủ dự án yêu cầu lưu toàn bộ nguồn môi trường/ZIP lên GitHub bằng Git LFS. Thư mục này chứa **nguồn art**, chưa phải scene Unity đã import hay bản game đã build.

## Nội dung đã đọc

| ZIP | Dung lượng | Nội dung thực tế | License trong nguồn |
|---|---:|---|---|
| `japanese_street_at_night.zip` | 108,193,583 B | 1 GLB phố Nhật hoàn chỉnh từ AFX/CGMotion; 149 mesh, 301 node, 34 material, 107 ảnh PNG | GLB ghi CC-BY-4.0, kèm tên tác giả và link Sketchfab; listing dẫn tới điều khoản bổ sung |
| `asphalt_01_4k.blend.zip` | 84,494,529 B | 1 Blender material và 4 texture 4K: diffuse JPG, normal OpenGL EXR, roughness EXR, displacement PNG | Poly Haven CC0 theo asset register; ZIP không kèm license text |
| `kenney_city-kit-commercial_2.1.zip` | 4,096,974 B | 41 mẫu nhà thương mại; FBX, GLB, OBJ/MTL, texture và preview | `License.txt`: CC0 |
| `kenney_city-kit-industrial_2.0.zip` | 5,045,077 B | 37 mẫu nhà/công trình công nghiệp; FBX, GLB, OBJ/MTL | `License.txt`: CC0 |
| `kenney_city-kit-roads.zip` | 2,852,289 B | City Kit Roads **2.2**: 95 mẫu đường, cầu, rào, cột điện và phụ kiện | `License.txt`: CC0 |
| `kenney_city-kit-suburban_20.zip` | 3,038,740 B | City Kit Suburban **2.0**: 40 mẫu nhà ngoại ô và phụ kiện | `License.txt`: CC0 |
| `kenney_modular-buildings.zip` | 1,825,490 B | Modular Buildings **2.1**: 108 module tường, góc, cửa, mái và chi tiết nhà | `License.txt`: CC0 |

Các định dạng Kenney là nhiều bản xuất của cùng bộ mẫu: tổng **321 mẫu**, không cộng FBX/GLB/OBJ thành các mẫu riêng.

`japanese_street_at_night.glb` rời (**112,172,164 B**) trùng SHA-256 với GLB bên trong ZIP. Model có 51,687 tam giác trong các mesh riêng, chưa nhân theo instance; không có animation; toàn bộ buffer/ảnh được nhúng, không cần tải texture ngoài. Có `KHR_materials_emissive_strength`; khi chuyển sang URP cần kiểm tra lại emission, màu, normal và độ bóng.

## Nguồn và quyền sử dụng

- AFX/CGMotion 3DModel Maker: [Japanese street at night](https://sketchfab.com/3d-models/japanese-street-at-night-fb1bdcd71a5544d699379d2d13dd1171). GLB và API Sketchfab ngày kiểm tra đều ghi [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/). Listing còn yêu cầu đọc [điều khoản của tác giả](https://affexion-license.web.app/license_en.pdf); chưa kết luận điều khoản bổ sung tương thích với việc phát hành game/nguồn. Giữ attribution và thông tin nguồn khi sử dụng. Bản gốc được lưu theo yêu cầu trực tiếp của chủ dự án; yêu cầu lưu không thay thế giấy phép.
- Kenney: [kenney.nl](https://kenney.nl/), giữ nguyên `License.txt` trong cả năm ZIP; cho phép dùng cá nhân, giáo dục và thương mại theo CC0.
- Poly Haven: [Asphalt 01](https://polyhaven.com/a/asphalt_01), CC0. ZIP khớp checksum đã ghi trong `Docs/StreetExterior/Reports/2026-11-06-V02-CC0-ASSET-INTAKE.md` và `Tools/StreetExterior/import_cc0_v02.py`.

## Kết quả kiểm tra và cách dùng

- Cả **7/7 ZIP** đọc được, CRC không lỗi, không có đường dẫn thoát thư mục trong danh sách entry. Không thực thi script trong archive.
- `SOURCE_MANIFEST.json` ghi dung lượng, SHA-256, số file và định dạng cho cả 8 file nguồn. ZIP/GLB lưu qua Git LFS; sau clone chạy `git lfs pull`.
- Các file ngoài `Assets/` không được Unity tự import. Kenney đã có FBX; AFX cần chuyển GLB sang FBX và kiểm tra URP trước khi tạo prefab/scene. Asphalt là vật liệu, không phải một khu phố.
- GLB AFX gốc **không phải** `SORA_Neighborhood_V03_Integrated.glb`. Lớp V03 đã lắp ráp vẫn chưa có trong project. V05 thay V04; V06 chỉ bổ sung chi tiết cho **V03 + V05**. Không chồng cả V04 và V05 hoặc coi V06 là bản đồ hoàn chỉnh.
- Demo hiện chạy phố blockout thủ tục v0.5. Dừng Play Mode rồi nhấn **F6**, chọn model và **Enter** để test game. Việc lưu nguồn không chứng minh import, collision, FPS hay Windows build của map mới.
