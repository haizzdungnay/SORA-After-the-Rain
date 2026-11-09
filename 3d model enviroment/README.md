# Thư viện model môi trường gốc

Kiểm tra ngày 09/11/2026. Thư mục chứa nguồn art; các bản đã lắp V03/V05 được nạp vào gameplay Unity theo [báo cáo tích hợp](../Docs/StreetExterior/Reports/2026-11-09-LOCAL-ENVIRONMENT-INTEGRATION.md). ZIP nằm ngoài `Assets/` vẫn không tự xuất hiện trong game.

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
- Unity dùng glTFast **6.20.0** để import GLB trực tiếp; không cần chuyển sang FBX. Bộ Kenney/asphalt gốc giữ làm thư viện; những phần đã lắp trong V03/V05 mới được nạp vào map.
- GLB AFX gốc **không phải** `SORA_Neighborhood_V03_Integrated.glb`. V05 thay V04. V06 hiện chỉ dành cho **V05 public** đúng SHA trong manifest; không ghép lên V05 trong ZIP riêng vì chiều cao và chi tiết mặt tiền khác nhau.

## Hai ZIP riêng mới và cài map local

| ZIP local | Nội dung dùng trong game | Lưu trữ |
|---|---|---|
| `SORA_Neighborhood_V05_Kenney_Private_2026-11-06.zip` | V03 đã lắp; V05 gồm 60 nhà và 42 xe Kenney; collider đúng phiên bản | Giữ local vì gói chứa AFX-derived V03 |
| `SORA_Unity_Private_AFX_Install_2026-11-09.zip` | V03 đã bỏ 129 mesh SORA MART trùng với cửa hàng tương tác | Giữ local; không đưa lên GitHub public |

Cả hai ZIP đã qua CRC và kiểm tra đường dẫn. Installer kiểm SHA-256 của ZIP/model, chuẩn hóa collider và từ chối ghi đè asset local khác phiên bản; không chạy script trong ZIP.

```powershell
python Tools/StreetExterior/install_private_city.py
python Tools/StreetExterior/install_private_city.py --verify
```

Mở Unity **6000.3.7f1**, chờ import, nhấn **F6**, chọn model rồi **Enter**. Runtime ưu tiên cặp **V03 private + V05 private** và collider tương ứng. Nếu chưa cài gói riêng, runtime dùng **V03 public + V05 public + V06**. Phố thủ tục chỉ là fallback khi asset không dùng được.

Thư mục `Assets/Resources/SoraCity/Private/` và hai ZIP riêng được `.gitignore` bảo vệ. Xem báo cáo để phân biệt kiểm thử tự động, ảnh camera Unity và các phần QA còn lại.
