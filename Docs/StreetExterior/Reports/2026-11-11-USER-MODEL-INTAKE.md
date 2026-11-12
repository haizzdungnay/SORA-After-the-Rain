# Kiểm kê model anh trai tải ngày 11/11/2026

Thư mục nguồn: `3dmodel 11-11/`. Kiểm tra trực tiếp ngày 11/11/2026.

- [x] Đã nhận và đọc **10 file**, tổng **578.052.169 byte**, khoảng **551,27 MiB**.
- [x] Đối chiếu SHA-256: **9 nguồn khác nhau**, gồm **6 GLB và 3 ZIP**; một GLB tải trùng.
- [x] Đọc metadata, node, mesh, vật liệu và texture; mở cả ZIP/RAR lồng bên trong.
- [x] Import thử **6 GLB khác nhau và 2 OBJ** bằng Blender **5.2.3 LTS**; tạo ảnh kiểm tra local.
- [x] Đọc `downtown.gltf` bên trong hai lớp ZIP và kiểm tra đủ file phụ thuộc. Chưa import khu Toronto vào Blender.
- [ ] Ghép các nguồn mới này vào Unity. Không có SHA-256 nào trùng 67 nguồn đang chọn trong manifest SORA MART hoặc các file của kho tải cũ.

## Danh sách đã tải

Dấu tải chỉ xác nhận file có thật. Số mesh/object không đồng nghĩa số sản phẩm khác nhau hoặc tiêu chí mỹ thuật đã hoàn tất. Số tam giác GLB là tổng mesh nguồn, chưa phải chi phí render trong Unity.

| Tải | File | Nội dung đã đọc | Tam giác nguồn | Giấy phép đọc được | Đánh giá cho cửa hàng |
|---|---|---|---:|---|---|
| [x] | `food_collection_-_game_ready.glb` | 40 object sản phẩm và 1 mặt nền; 3 vật liệu, 7 ảnh | 31.796 | Metadata ghi CC BY 4.0; Melon Polygons | Ưu tiên: chai/lon, sữa, nước hộp, đồ hộp, bánh kẹo, mì và hóa phẩm có bao bì. Chưa phải toàn bộ nhãn hàng Nhật. |
| [x] | `psxps1-food-props-lowpoly.zip` | `source/ITWORKS.rar`, bên trong có `props.obj`, `props.mtl`, 64 object hàng hóa, 15 vật liệu và 14 PNG | 3.796 sau triangulate OBJ | Không thấy giấy phép/tác giả trong gói | Nhiều biến thể sữa, trà, cà phê, ngũ cốc, bánh, đồ hộp, mì ly, hộp pizza. Texture 320 x 320; cần đồng bộ phong cách và sửa hai liên kết ảnh trước khi dùng. |
| [x] | `free_store_props_asset_pack.glb` | 27 mesh, 2 vật liệu, 1 ảnh | 6.529 | Metadata ghi CC BY 4.0; SimplePolygon | Kệ, module quầy, thùng và props bán lẻ; hình học đơn giản, cần xét chất lượng cận cảnh. |
| [x] | `convenience_store_2.glb` | Cảnh cửa hàng; 135 mesh, 100 vật liệu, 75 ảnh | 697.331 | Metadata ghi CC BY 4.0; Pasha | Hợp tham khảo bố cục và lọc thiết bị. Tên node phần lớn là `Material2`/`Material3`; cần tách, đặt tên và xác minh đơn vị trước tích hợp. |
| [x] | `convenience_store_2 (1).glb` | Trùng từng byte với `convenience_store_2.glb` | Không tính thêm | Như bản gốc | Giữ nguyên file; không tính thêm nguồn hoặc tải thêm model. |
| [x] | `vr-store.zip` | `source/01.zip`, bên trong `03/03.obj`, `03/03.mtl`, 232 object và 106 khai báo vật liệu; có quầy, kệ, tủ đông, khu bánh/đồ nóng và ô | 96.201 trong OBJ | Không thấy giấy phép/tác giả trong gói | Có thể tham khảo layout và chọn thiết bị. Texture có sẵn; cần kiểm tra vật liệu bake/transparency và đơn vị nguồn. |
| [x] | `asian_shop_pack_free_gameready.glb` | Các mặt tiền cửa hàng Á Đông; 41 mesh, 31 vật liệu, 28 ảnh | 38.870 | Metadata ghi CC BY 4.0; abhayexe | Dùng cân nhắc cho phố ngoài cửa hàng. Không phải bộ hàng hóa nội thất; chưa xác nhận biển hiệu phù hợp bối cảnh Nhật. |
| [x] | `free_simple_storefront_interior_pack.glb` | 3 mesh: người mẫu, `Interiors`, `Background`; 3 vật liệu, 1 ảnh | 4.777 | Metadata ghi CC BY 4.0; 99.Miles | Nội thất trang trí mặt tiền/cửa kính, hình học gộp; không phải thư viện sản phẩm rời cho kệ SORA MART. |
| [x] | `marlboro_cigarettes_-_4096px2.glb` | Hộp, nắp và thuốc; 3 mesh, 2 vật liệu, 6 ảnh | 6.432 | Metadata ghi **SKETCHFAB Standard**, Mark Peters | Cần đối chiếu quyền sử dụng đã nhận trước khi đưa vào repo/game. Ảnh nhúng thực tế **1024 x 1024**, dù tên ghi 4096. |
| [x] | `downtown-toronto-yonge-and-dundas.zip` | `source/sketchfab atlases.zip`, bên trong `downtown.gltf`, `downtown.bin` và ảnh; 7.502 node, 550 mesh, 99 vật liệu, 90 ảnh tham chiếu | 432.804 mesh riêng; chưa tính lặp instance | Không thấy giấy phép/tác giả trong gói hoặc metadata glTF | Khu phố Toronto/Canada, không phù hợp thay nguyên khu Tokyo. Chỉ cân nhắc props rời sau khi rõ quyền sử dụng. |

## Kết quả kiểm tra file

- Cả ba ZIP ngoài và hai ZIP nguồn bên trong đều qua kiểm tra CRC. Các đường dẫn texture/buffer của `downtown.gltf` tồn tại trong ZIP nguồn.
- RAR đọc và giải nén được bằng Windows `tar` khi dùng file trên đĩa. Lần đọc qua stdin báo `Header CRC error`; bản 7-Zip đi kèm Unity không có bộ đọc RAR. Các lỗi thử công cụ này không phải kết luận file tải hỏng.
- Sáu GLB nhúng đầy đủ ảnh, không cần tải file texture ngoài. Sau import Blender, cả sáu không báo thiếu ảnh.
- VR Store có đủ tên ảnh được tham chiếu trong MTL. Blender báo không hỗ trợ `map_Ka`; cần kiểm tra lại bề mặt khi chuẩn bị xuất Unity. Blender đếm 96.119 tam giác sau import, khác 96.201 mặt tam giác khai báo trong OBJ.
- PSX import được 64 mesh và 3.796 tam giác. Hai tham chiếu ảnh không khớp file có sẵn: **`chocolate.psd`** và **`noodles.png`**. Trong gói có **`chocolate.png`** và **`noodles, soaps.png`** để đối chiếu và trỏ lại khi xử lý bản sao. Bản nguồn giữ nguyên.
- Convenience Store 2 import được 135 mesh; Blender đếm 695.737 tam giác, khác 697.331 tam giác nguồn. Cần kiểm tra hình học khi chọn phần sử dụng.
- Kích thước đọc từ nguồn của hai cảnh cửa hàng lớn không được coi mặc định là mét. Chưa xác nhận tỷ lệ nhân vật, collider, đường đi hoặc hiệu năng Unity.

## Nguồn và credit nhúng trong GLB

Giấy phép dưới đây được đọc từ `asset.extras`; chưa xác minh thêm điều khoản trên trang tác giả trong lượt kiểm kê này. Khi chọn dùng cần lưu bằng chứng nguồn, giấy phép và credit vào hồ sơ asset.

| Gói | Tác giả | Trang nguồn nhúng |
|---|---|---|
| Food Collection | Melon Polygons | https://sketchfab.com/3d-models/food-collection-game-ready-969b4e6ef7444650ba8a22a46d57ff43 |
| Store Props | SimplePolygon | https://sketchfab.com/3d-models/free-store-props-asset-pack-87a7f07bf1414543a073060388294f59 |
| Convenience Store 2 | Pasha | https://sketchfab.com/3d-models/convenience-store-2-3e9ccca610614bcb84cb3b8390c8a9a0 |
| Asian Shop Pack | abhayexe | https://sketchfab.com/3d-models/asian-shop-pack-free-gameready-2f603ad8d5c640f7b9794f6916f9fbdd |
| Simple Storefront Interior | 99.Miles | https://sketchfab.com/3d-models/free-simple-storefront-interior-pack-348da01c195d46eca29e478e98aab16c |
| Marlboro Cigarettes | Mark Peters | https://sketchfab.com/3d-models/marlboro-cigarettes-4096px2-b107a710d4934ef4b0fd92a6868774fb |

## Bằng chứng local

- `Artifacts/UserModels11-11/scan.json`: dung lượng, SHA-256 của cả 10 file, metadata, tên node và đối chiếu kho cũ.
- `Artifacts/UserModels11-11/*/scene_audit.json` và `mesh_inventory.csv`: kết quả import, mesh, vật liệu, kích thước và ảnh thiếu.
- `Artifacts/UserModels11-11/selected-packs.jpg`: ảnh xem thử Blender của Food Collection, PSX Food, Store Props và Asian Shop Pack. Không phải ảnh Unity; PSX còn hiện bề mặt hồng ở hai liên kết ảnh thiếu.
- `Artifacts/UserModels11-11/blender.log` và `psx-blender.log`: log import/render thật. Bản giải nén chỉ nằm trong `Artifacts/UserModels11-11/sources/`.

Thứ tự đề xuất: chọn Food Collection cho hàng đóng gói; lọc Store Props cho thiết bị; xác minh giấy phép và sửa liên kết ảnh PSX để bổ sung biến thể. Hai cảnh cửa hàng dùng tham khảo bố cục hoặc chọn từng thiết bị sau kiểm tra. Đợt này chưa cộng vào 403 GLB và 67 nguồn Unity của đợt trước.

## Cập nhật: phân tách tài nguyên để dùng sau này

- [x] Tạo kho local `LocalReferenceAssets/UserModels_2026-11-11/`, ngoài `Assets` và được quy tắc `/LocalReferenceAssets/` hiện có loại khỏi Git. [Hướng dẫn sử dụng](../../../LocalReferenceAssets/UserModels_2026-11-11/README.md).
- [x] **825 file tài nguyên `.gltf`**, gồm 41 FoodCollection (40 hàng hóa + 1 nền), 64 PSXFood, 27 StoreProps, 135 ConvenienceStore2, 232 VRStore, 22 nhóm AsianShopPack, 3 StorefrontInterior, 4 Marlboro (3 bộ phận + 1 assembly), 297 cụm Toronto.
- [x] Tách theo object/cụm của tác giả, giữ các phần cùng cây node trong một cụm; không tách thân xe và bánh thành các xe thiếu bộ phận. Các mesh đã gộp theo vật liệu vẫn là mảnh nguồn cần chọn/lắp lại; không tự suy ra tên thiết bị. 21 object hình học đường được ghi `LineGeometry` và `Reference`.
- [x] `catalog.csv` cho Excel; `catalog.json` lưu ID, node nguồn, đường dẫn, nhóm, số mesh/tam giác, kích thước và pivot, SHA-256, nguồn và giấy phép.
- [x] 9 cảnh `Scenes/SourceLayout.gltf` giữ bố trí nguồn. File từng tài nguyên dùng chung `geometry.bin` và thư mục `Textures` trong mỗi gói; mesh/UV/ảnh gốc của GLB và glTF Toronto giữ byte nguồn, không nhân bản atlas theo từng món.
- [x] `Originals/` giữ 9 file nguồn khác nhau; catalog ghi nhận cả 10 file đầu vào và bản tải trùng. Ba ZIP được giải nén vào `WorkingSource/`; file tải về giữ nguyên.
- [x] Sửa `chocolate.psd` thành `chocolate.png`, `noodles.png` thành `noodles, soaps.png` và đường dẫn tuyệt đối trong MTL PSX trên bản sao; ảnh xem lại không còn hai bề mặt hồng vì thiếu ảnh. OBJ của PSX/VR được chuyển sang glTF bằng Blender 5.2.3 LTS.
- [x] Chia 5 gói có metadata CC BY 4.0 và 4 gói cần kiểm tra giấy phép; mỗi gói có `SOURCE.json`, SHA-256 nguồn/texture/mesh và credit nhúng. Đây là trạng thái hồ sơ, chưa phải chấp thuận phát hành.
- [ ] Chưa import hoặc ghép các file tài nguyên tách riêng vào scene Unity. Khi dùng cần sao chép cả cấu trúc gói, giữ `geometry.bin` và `Textures` đúng vị trí; glTFast 6.20.0 hiện có hỗ trợ importer `.gltf`.

Kiểm tra lại kho bằng Python, không cần Blender:

```powershell
python -X utf8 LocalReferenceAssets/UserModels_2026-11-11/build_library.py --verify
```

**Kết quả Blender: 825/825 file tài nguyên mở lại thành công**, không thiếu ảnh; kiểm tra giới hạn tọa độ theo đỉnh thật, số tam giác và hình học đường đã qua. Log dựng: `Artifacts/UserModels11-11/library-build.log`. Kiểm tra mở lại: `Artifacts/UserModels11-11/library-import-checks.json` và `.log`. Ảnh PSX sau sửa: `Artifacts/UserModels11-11/SourceLayout/overview.png`. Không dùng ảnh Blender để khẳng định kết quả Unity.

## Cập nhật: tài nguyên đã chọn cho SORA MART ngày 12/11

Đã chọn 107 tài nguyên: 40 Food Collection, 64 PSX Food và 3 Store Props. Hai liên kết ảnh PSX đã sửa trên bản sao; model tải gốc và kho 825 tài nguyên giữ nguyên. Đối chiếu trang Sketchfab xác nhận CC BY 4.0 cho ba nguồn; bằng chứng và credit trong `ArtSource/ThirdParty/SoraMart/`. Gói VR Store có mô tả tài sản của công ty do đồng nghiệp dựng, nên giữ local dù metadata trang ghi CC BY 4.0.

[CSV 107 tài nguyên đã dùng](../SORA_MART_USER_LIBRARY.csv) và [báo cáo Unity hiện tại](2026-11-12-SORA-MART-RETAIL-FINISH.md). Các dấu chưa ghép ở phần kiểm kê ban đầu là lịch sử tại thời điểm kiểm tra.
