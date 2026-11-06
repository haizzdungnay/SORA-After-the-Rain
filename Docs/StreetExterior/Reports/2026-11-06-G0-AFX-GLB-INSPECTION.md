# SORA — G0 EXT-003: AFX GLB forensic geometry inventory (uploaded source)

**Ngày:** 06/11/2026 (Việt Nam). **Nhánh:** `feature/street-exterior-g0-audit`. **Nguồn:** ZIP gốc do chủ dự án cung cấp trong cuộc trò chuyện; **chưa và sẽ không đưa mesh/texture bên thứ ba lên repo công khai.**

## 1. Kết quả đọc file 3D thật

- Archive: `japanese_street_at_night.zip`; một file bên trong: `japanese_street_at_night.glb` (112,172,164 bytes).
- SHA-256 (GLB): `975e7a7ab192d7315bf4c46e6aa7e0899d1ecfadd20d2550ef71e034c763c5af`.
- Metadata glTF: **author** AFX/CGMotion 3DModel Maker; **title** Japanese street at night; **source** https://sketchfab.com/3d-models/japanese-street-at-night-fb1bdcd71a5544d699379d2d13dd1171; **license label** CC-BY-4.0 (bổ sung bởi điều khoản tác giả đã ghi trong báo cáo EXT-002).
- Phân tích thực tế bằng parser GLB, `trimesh` và trình render hình học `VTK`: **149 mesh objects, 51.687 triangles, 301 glTF nodes, 34 materials, 107 PNG images embedded, 107 texture slots**. Cả 149 mesh có UV; không có animation, skin hoặc camera. Không có đường texture ngoại bộ cần tải bổ sung.
- Bounding box trong tọa độ scene đã apply node transforms: `X = -4.322 .. 5.198`, `Y = -0.053 .. 8.489`, `Z = -13.106 .. 2.937`; span **9.520 × 8.542 × 16.043** (X/Y/Z). glTF quy ước đơn vị mét, nhưng **tỷ lệ thiết kế của tác giả chưa được kiểm chứng với character controller trong Unity**.
- Tất cả 149 mesh không kín theo kiểm tra watertight tự động. Đây không phải bằng chứng tất cả đều lỗi: mặt kiến trúc, decals và props thường được dựng bằng shell/open planes; yêu cầu đối chiếu face/culling khi nhập Unity.
- Đây là một **diorama hẻm duy nhất**, chứ không phải tuyến phố chính hoặc khu nhiều ngã rẽ. Ảnh nhìn từ trên và ở tầm mắt cho thấy hai hàng công trình dày đặc, bảng hiệu, biển treo, trụ điện, dây điện, máy bán hàng; mặt ngoài/backdrop có nhiều mảng tường lớn không trang trí. Do đó yêu cầu hoàn thiện **mọi mặt người chơi có thể thấy** không thể đạt chỉ bằng import GLB nguyên trạng.
- File không có sẵn định danh `SORA MART`, POS/collider gameplay hoặc anchor cửa hàng của SORA. Không giả định một mesh group tương đương một cửa hàng có thể chơi. Không có hệ thống mưa hoặc setup đèn URP tích hợp cần thiết cho gameplay.

## 2. Kiểm kê từng cụm mặt tiền thực tế (ứng viên)

Hướng đi để đánh số: đi từ đầu `+Z` về cuối `-Z`; `L` là dãy `-X`, `R` là dãy `+X`. **Tập tin CSV bên cạnh là candidate group inventory, chưa phải shop-level sign-off.**

| Candidate frontage | GLB group | Mesh | Triangles | Vị trí tương đối theo Z |
|---|---|---:|---:|---|
| `F-L-001` | A | 16 | 6,575 | 1.909 .. -0.751 |
| `F-L-002` | B | 21 | 4,588 | -0.749 .. -4.030 |
| `F-L-003` | C | 13 | 4,613 | -3.926 .. -6.396 |
| `F-L-004` | D | 13 | 4,255 | -6.401 .. -9.606 |
| `F-L-005` | I | 9 | 3,002 | -9.605 .. -12.357 |
| `F-R-001` | E | 5 | 1,000 | 2.382 .. -0.151 |
| `F-R-002` | F | 19 | 4,023 | -0.091 .. -2.778 |
| `F-R-003` | G | 14 | 7,553 | -2.771 .. -5.102 |
| `F-R-004` | H | 15 | 4,359 | -4.977 .. -7.292 |
| `F-R-005` | J | 12 | 6,419 | -10.029 .. -12.356 |

10 cụm kiến trúc **A–J** (137 mesh objects; 46.387 triangles); không được đánh đồng mặc nhiên với 10 cửa hàng độc lập. Ngoài ra còn có 4 máy/chi tiết bán nước (762 triangles), 5 pole/utility meshes (3.500 triangles), 1 light mesh (852 triangles, không có glTF light setup), 1 ground (178 triangles), 1 backdrop (8 triangles). Kết quả kiểm kê không bao gồm hệ thống Unity runtime, và phản ánh trực tiếp file đã tải.

## 3. Bằng chứng góc nhìn và điểm yếu chất lượng

Đã render thực từ mesh và texture trong file bằng `VTK`: top, hướng từ hai đầu, hai góc chéo và một góc camera tầm mắt (tổng cộng **6 ảnh**). Thêm 1 sơ đồ bbox các group A–J lấy từ geometry. Các ảnh/bảng máy đọc được đóng thành **gói bằng chứng cục bộ**, được bàn giao trực tiếp cho chủ dự án; **không commit texture/render bên thứ ba lên GitHub**. Đây là ảnh render từ GLB, **KHÔNG PHẢI** Unity Play Mode hoặc ảnh game SORA.

**Quan sát:**

1. **Tốt:** phù hợp hẻm Nhật đêm, độ dày đạo cụ, facade, signage, máy bán hàng, dây điện; có UV và base-color atlas.
2. **Chưa đạt nếu làm nguyên phố:** đường rất hẹp và chỉ có một tuyến, đầu/cuối bị kết thúc bằng nền/tường, mặt ngoài nhiều tường phẳng. Cần tạo phần mở rộng/đầu ngõ + các mặt ngoài có thể thấy, hoặc chặn di chuyển/camera hợp lý.
3. **Tích hợp SORA MART:** storefront cũ có sàn khoảng 14 m mặt tiền và 11 m chiều sâu (theo code SoraDemoWorld.cs), lớn hơn chiều ngang toàn cảnh AFX chỉ khoảng 9,52 theo trục X; không thể đặt cửa hàng cũ nguyên tỷ lệ vào diorama này. Quyết định đề xuất: dùng AFX như **đoạn hẻm/quality reference**, dựng SORA MART riêng trên tuyến thương mại chính khi G1 được duyệt. Không đưa ra vị trí giả định khi chưa có sơ đồ mới.
4. **UV/text/lighting:** hình render tạm có vài biển hiệu trông ngược/khó đọc; chưa kết luận đó là lỗi GLB do render bằng công cụ ngoài Unity. Cần recheck UV, back-face, font và emission trong Unity/Blender trước khi nghiệm thu EX-04/06/08.
5. **Collision/camera:** scene glTF chưa cung cấp Unity colliders, route anchors và third-person follow setup. Với camera 3.6m hiện tại, cần test clipping khi quay trong hẻm. Không thể coi đi bộ gameplay đã pass.

## 4. Kết quả kiểm thử / trạng thái tiếp theo

- **DONE:** kiểm tra ZIP hợp lệ, trích GLB, parse glTF, scene bounds/nodes/mesh/material/texture stats, lập group map và render 6 góc thực.
- **REVIEW:** candidate frontage groups, việc phân biệt cửa hàng/cửa vào thực sự và độ hoàn thiện từng mặt cần người xem mẫu duyệt trước khi khóa `F-*`.
- **NOT RUN:** Blender Import, Unity import/URP shader correction, Unity C# compile, EditMode/Play Mode, Windows build, visual in-game, đo VRAM/FPS (không có Editor/Blender trong môi trường audit); không tuyên bố pass những việc này.
- **G0 EXT-003:** chuyển từ `BLOCKED` sang **`REVIEW`**. Chỉ DONE sau khi xác nhận candidate groups/frontages và các mặt thiếu. G1 vẫn **chưa bắt đầu**.
- **Phương án thiết kế cần chủ dự án duyệt ở G1:** AFX là mẫu mỹ thuật cho đoạn hẻm, không phải mẫu toàn bộ quy mô phố chơi được; bố cục khu phố hoàn chỉnh phải có con đường chính và SORA MART ở tỷ lệ phù hợp, rồi mới nối với đoạn hẻm.

**Lưu ý quyền:** nguyên gói GLB/texture giữ ở máy hoặc attachments của chủ dự án, không commit vào public repo. Bản chính thức phải xem xét lại license/asset source trước release.
