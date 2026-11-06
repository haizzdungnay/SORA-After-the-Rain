# V04 — Asset miễn phí còn cần để nâng chất lượng

**Đã có:** AFX local/private, 2 GLB phố Nhật CC0 (Izakaya Alley + Japanese School City), Poly Haven Asphalt 01 2K PBR. Chúng đủ để dựng **khung V04 9 ô nhà**. Xe và tường bao hiện là geometry do SORA tự tạo.

**Ưu tiên tải tiếp (01 bộ, miễn phí thật):**

- **[Kenney — Car Kit](https://kenney.nl/assets/car-kit)** — [trang tải miễn phí thay thế trên itch.io](https://kenney-assets.itch.io/car-kit). Giấy phép **CC0 1.0**. Có khoảng **45 mẫu xe và chi tiết**, gồm xe con, xe tải, van; hỗ trợ glTF, FBX và OBJ. Anh tải ZIP (miễn phí; Kenney cho phép bỏ qua khoản ủng hộ), rồi gửi lại nguyên ZIP để em thay 42 xe placeholder dọc rìa bản đồ và những xe đỗ khu dân cư. **CHƯA TẢI / CHƯA IMPORT vào V04**, không tuyên bố đã dùng.

**Có thể tải sau, chưa phải blocker:**
- [Kenney City Kit Roads](https://kenney.nl/assets/city-kit-roads) — CC0; giúp bổ sung dải phân cách, đèn giao thông, chắn đường, biển cảnh báo. Chỉ cần nếu muốn nâng hạ tầng từ mô hình đường PBR đang có, không bắt buộc để khóa map.

**Quan trọng:** Xe đẹp hơn **không thay thế collider và giới hạn chơi**. Bản V04 giữ tường bao vật lý + rào 16 đầu đường + tùy chọn runtime failsafe; chỉ đổi art nếu tải thêm Car Kit. Lớp giới hạn này phải được test trong Unity Play Mode với **cả hai nhân vật**.

Sau khi nhận Kenney ZIP: xác minh license kèm theo, checksum, tách prefab/LOD, thay xe từng cụm, cập nhật `Docs/ASSET_REGISTER.csv`, test collider/đi bộ và chỉ đẩy source CC0 hợp lệ qua Git LFS.
