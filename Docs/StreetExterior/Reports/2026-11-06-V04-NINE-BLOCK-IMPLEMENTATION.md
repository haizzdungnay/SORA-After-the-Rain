# SORA V04 — Dense Nine-Block Neighborhood, World Boundary & CC0 Vehicles

**Ngày:** 2026-11-06. **Mục tiêu của chủ dự án:** loại bỏ cảm giác các lô đất trống và khắc phục việc nhân vật đi ra khỏi bản đồ, với đường phố dạng lưới gồm **9 ô nhà**. Giữ mô hình SORA MART, các anchors của V03 và hệ thống POS/switch/save-load nguyên trạng.

## Đã thực sự dựng

- Bảo tồn toàn bộ 4 ô phố, 24 công trình và 9 giao lộ V03 trong **lớp mô hình riêng**.
- Mở rộng thêm **5 ô phố** về phía Bắc và Đông, mạng đường tăng từ 3×3 lên **4 đường ngang × 4 đường dọc = 16 giao lộ**, diện tích khu chơi trong tọa độ danh nghĩa **171 × 171 m** (trục X/Z -63..108, theo tỷ lệ hiện dùng).
- **60 công trình 3D mới** ghép CC0 storefront/mesh với vỏ kiến trúc do SORA author: mặt tiền, kính, khung, bảng hiệu, mái, hai mặt bên và mặt sau. Tổng **84 công trình có hình học**.
- **5 ô phố mới có mật độ nền công trình ~77,6%** theo footprints danh nghĩa; kiểm tra hình chữ nhật diện tích giao nhau của 60 công trình mới cho kết quả **0 va chạm footprint**. Các sân kỹ thuật trong 4 ô cũ được rào kín thay vì để khoảng rỗng có thể thấy từ bên ngoài.
- **42 xe ô tô 3D tự dựng** (kei, sedan, van/delivery) làm cảnh/đỗ. Đây là placeholder art của SORA, **chưa dùng pack xe thật của bên thứ ba**.
- Nền dự phòng dưới toàn bản đồ, **4 dải tường/boundary collider liền nhau có chồng ở góc**, **16 rào chắn vật lý tại các đầu đường**, và các dãy nhà nền phía ngoài hạn chế lộ void.
- Các đoạn đường mở rộng dùng PBR Asphalt 01 CC0, bổ sung vạch qua đường cho 7 giao lộ mới (9 cũ được giữ).

## Đầu ra có thể tải và kết hợp

**1. Private owner package:** `SORA_Neighborhood_V04_Private_2026-11-06.zip` bàn giao trong cuộc trò chuyện, gồm: 
- `maps/SORA_Neighborhood_V03_Integrated.glb`: base V03 có nguồn AFX, **chỉ nội bộ**.
- `maps/SORA_V04_Dense_9Block_Overlay.glb`: 20,896,516 bytes, 3.772 mesh instances, SHA-256 `cd921d85051104d71bbfef42e901ee3d47f3c2d629c9ab7cd84ad90b53eb3aa8`.
- `maps/SORA_V04_Combined_Clay_Preview.glb`: bản xem nhanh V03 + V04 chung trong một file 3D geometry/clay, không chứa chất lượng texture cuối.
- `V04_MANIFEST.json`, `V04_COLLIDERS.json`, hình render thật từ cả hai nguồn, mã builder và script Blender import 2 model.

**2. Public Git LFS overlay:** `ArtSource/Generated/V04_CC0/SORA_V04_CC0_Dense_Overlay.glb`, **20,254,020 bytes**, **2.350 mesh instances**, SHA-256 `8f96ca8d4a6a62d4f18556d8cf525b10d9a5af448c8a8f6f1961bfe17c2ac096`. Bản public được dựng lại từ `Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json` bằng `Tools/StreetExterior/build_v04_dense_grid.py`, chỉ dùng geometry tự author và source CC0. **Không có AFX raw hay derived mesh.** Bản public giản lược biển hiệu/đạo cụ so với gói private, nhưng cùng mạng đường, 60 building IDs, 42 vehicles, 4 boundary walls và 16 road barricades.

**Lưu ý:** Vì xuất chung mọi material/mô hình V03+V04 thành một GLB có texture mất quá nhiều thời gian trong runtime, **bản texture đầy đủ được giao làm hai GLB đồng tọa độ**. Import cả hai tại local (0,0,0), scale (1,1,1). Script `builder/import_v04_blender.py` hỗ trợ Blender mở hai model và lưu thành một .blend (chưa chạy ở môi trường có Blender). Bản clay kết hợp được export và reload thành công; không giả vờ đó là textured scene.

## Kiểm thử đã thực hiện

| Nội dung | Trạng thái |
|---|---|
| V04 GLB local export/reload | PASS — 3.772 nodes |
| V03+V04 geometry clay combined real-mesh preview | PASS, PNG top/aerial/street-eye |
| Số ô, giao lộ, tòa nhà, xe | PASS — 9 / 16 / 84 / 42 |
| 5 ô mới, 60 footprints mới | PASS — không overlap, occupancy 77.6% |
| Tường bao đóng kín 4 phía và 16 road-end barricades | PASS metadata/geometry |
| Remote CC0-only generator workflow GitHub | PASS [action run](https://github.com/haizzdungnay/SORA-After-the-Rain/actions/runs/42193823811) |
| Remote GLB Git LFS pointer + checksum | PASS |
| Unity Editor FBX import / URP materials | **NOT RUN** |
| Thực tế CharacterController không vượt barrier | **NOT RUN** |
| Camera clipping, NPC navigation, Tab switching, POS, save/load | **NOT RUN** |
| Rain/weather, quality mọi mặt tiền (EX-01..12), FPS Windows | **NOT RUN / ART PASS PENDING** |

Tường 3D và BoxCollider **chưa tương đương nghiệm thu gameplay**; để tránh nhân vật rơi thực sự còn cần import scene, bật collision layer phù hợp, thử nhiều hướng và nghiệm thu camera. `Tools/Unity/SoraNeighborhoodV04Staging.cs` tạo art-only scene, **không sửa scene gameplay hiện tại**. `Tools/Unity/SoraNeighborhoodV04BoundaryGuard.cs` là failsafe optional, phải bind cả hai nhân vật trong bước tích hợp thật.

## Art backlog

1. Thay ô tô placeholder bằng [Kenney Car Kit](https://kenney.nl/assets/car-kit), giấy phép CC0, phù hợp xe đậu ven đường; điều chỉnh phong cách khi phối asset Nhật hiện có. **Chưa nhận/tải**.
2. Tăng khác biệt kiến trúc, signage và cửa kính ở 60 công trình mới. Không được đánh đồng việc tạo shell với nghiệm thu đẹp từng cửa hàng.
3. Blender import → Unity URP material/normal/roughness, collider mesh/box, hai nhân vật; quay video di chuyển quanh **toàn bộ vành đai 171m**.
4. Mưa đêm wet-reflection, biển hiệu có phát sáng, hệ thống camera tránh vách.
5. Gameplay adapter single-world; không spawn world cũ và scene mới cùng lúc, không đổi save schema âm thầm.

**Nguồn và quyền:** AFX original & derivatives giữ ở private ZIP theo điều khoản tác giả; hai GLB 3DAssets.dev và Asphalt Poly Haven là CC0, công trình/xe thủ công do SORA author và có thể push lên public source repo. Không đưa AFX lên public GitHub.
