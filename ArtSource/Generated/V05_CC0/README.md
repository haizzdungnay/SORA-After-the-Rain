# SORA V05 — 42 xe Kenney đã thay thế

**Bàn giao ngày:** 06/11/2026. **Trạng thái:** Model thật đã build, kiểm tra và lưu Git LFS. Unity Editor / Play Mode chưa chạy.

## Bản đồ nào cần kết hợp?

- Lớp nền **V03**: `SORA_Neighborhood_V03_Integrated.glb` (mô hình riêng do chủ dự án giữ vì có AFX).
- Lớp nâng cấp **V05**: `SORA_V05_Kenney_9Block_Overlay.glb` (CC0, có đầy đủ công trình V04 và 42 xe Kenney mới).

**V05 THAY THẾ HOÀN TOÀN lớp V04, không phải một lớp xe bổ sung.** Tuyệt đối không import V04 và V05 đồng thời, vì sẽ nhân đôi nhà, đường, ô tô và vật lý.

Cả hai GLB được đặt cùng origin `(0,0,0)` và scale `(1,1,1)` trước khi đưa vào Blender/Unity.

## Đã push lên GitHub

- [Source Kenney Car Kit 3.1](../../ThirdParty/CC0/KenneyCarKit/kenney_car-kit.zip) — **4.814.237 bytes**, Git LFS SHA-256 `fac7dacac5c7874348cf19729af3ef205f3d366493edaf0a827d93f4fdf3d0c4`. Đây là **chính xác ZIP người dùng cung cấp**, được đối chiếu với bản do Kenney đăng trên OpenGameArt khi CDN Kenney chặn tải tự động.
- [GLB V05 CC0 thật](SORA_V05_Kenney_9Block_Overlay.glb) — **20.653.268 bytes**, 2.198 mesh instances, Git LFS SHA-256 `805fa2e82e5243361db1a7ab8c897462a235e0da7d3f171622eff042ef13f603`.
- [manifest.json](manifest.json) — toàn bộ 42 xe, loại xe đã chọn, source/license/hash và số mesh.
- [colliders.json](colliders.json) — 122 collider proxies: 60 nhà, 42 xe có kích thước mới, 4 boundary và 16 rào cuối đường.

## Các thao tác đã thực hiện

1. Đọc `License.txt` trong ZIP gốc (Kenney Car Kit **3.1**, **CC0 1.0**).
2. Tháo toàn bộ mesh xe placeholder trong V04 (mỗi xe có nhiều mesh tạo bằng primitive).
3. Dùng mesh 3D Kenney riêng biệt, áp kích thước kei / sedan / van / xe giao hàng; giữ đúng 42 vị trí đỗ của V04.
4. Lấy bảng màu `Models/GLB format/Textures/colormap.png` từ ZIP và **nhúng lại** vào GLB, vì các GLB gốc tham chiếu URI tương đối; nếu bỏ qua, xe sẽ trắng/xám.
5. Xuất lại V05 GLB, đọc lại sau xuất; xác nhận **42 IDs duy nhất, không còn các node `V04/CAR/` hoặc `V04/VEHICLE/`**.
6. Cập nhật BoxCollider proxy thực tế, giữ 4 bức tường bao liên tục, 16 rào đường và bản đồ 9 ô không thay đổi.

## Tái dựng và Unity

Từ root repository, sau `git lfs pull`:

```bash
python -m pip install 'trimesh>=4.5,<5' 'numpy>=1.26,<3' 'Pillow>=10,<13'
python Tools/StreetExterior/build_v05_kenney.py \
  --base-glb ArtSource/Generated/V04_CC0/SORA_V04_CC0_Dense_Overlay.glb \
  --cars-json Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json \
  --colliders ArtSource/Generated/V04_CC0/colliders.json \
  --kenney-zip ArtSource/ThirdParty/CC0/KenneyCarKit/kenney_car-kit.zip \
  --output ArtSource/Generated/V05_CC0
```

Blender chuyển V03 và V05 thành FBX/texture hoặc dùng script import trong gói V05 riêng của chủ dự án. Tại máy Unity, copy `Tools/Unity/SoraNeighborhoodV05Staging.cs` vào `Assets/SORA/Scripts/Editor/`, import FBX về `Assets/SORA/Environment/StreetExterior/PrivateGenerated/` và chọn **SORA > Street Exterior > V05 - Build 9-block Kenney staging scene**. Để `colliders.json` từ GitHub đúng vị trí như repo yêu cầu.

Công cụ staging này **không sửa `SORA_Prototype.unity`** và không spawn `SoraDemoCore`, POS, nhân vật hoặc save loader mới.

## QA giới hạn

**PASS:** GitHub Action build Kenney 3.1 (CRC/license/SHA), GLB export+reload, 42 xe không trùng, phiên bản public/owner-private đều đã build, source source-color atlas được nhúng, collider metadata đủ 122 entries, 4 tường và 16 rào còn nguyên.

**CHƯA CHẠY:** Unity Editor / URP shader, physics character/camera, Tab switch, save/load, POS, NPC, nighttime wet-reflection, Windows build và FPS. Xe Kenney low-poly có thể cần phối lại vật liệu với phong cách AFX trước nghiệm thu art.
