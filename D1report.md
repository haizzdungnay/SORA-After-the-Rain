# BÁO CÁO NGÀY 1 (DAY 1 REPORT) — SORA: AFTER THE RAIN

**Ngày thực hiện:** 02/11/2026  
**Dự án:** SORA: After the Rain (Tokyo Night Convenience Store & Life Sim)  
**Chủ dự án:** Đinh Tuấn Dương ([@haizzdungnay](https://github.com/haizzdungnay))  
**Engine:** Unity `6000.3.7f1` (Unity 6.3 LTS), Universal Render Pipeline (URP) `17.3.0`  
**Target:** Windows x64 Standalone  

---

## 1. Tổng quan công việc đã hoàn thành

Trong ngày đầu tiên, dự án đã chuyển đổi từ mã nguồn thô chưa từng được biên dịch thực tế thành một bản prototype hoàn chỉnh có thể chơi được cả trên Unity Editor và bản build Windows Standalone độc lập, kèm theo hệ thống CI/CD tự động hóa trên GitHub Actions.

---

## 2. Các vấn đề kỹ thuật đã xử lý & sửa lỗi (Bug Fixes)

### 2.1. Lỗi phân giải Package UPM trên Unity 6
- **Hiện tượng:** Khi nạp project vào Unity Hub, Unity Package Manager văng lỗi:
  `com.unity.modules.inputlegacy`, `com.unity.modules.textrendering`, `com.unity.modules.tetgen`, `com.unity.modules.physicscore2d` không tìm thấy trên registry.
- **Nguyên nhân:** Trên Unity 6 (6000.3.7f1), các module legacy đã bị gỡ bỏ hoặc tích hợp sẵn vào engine core.
- **Xử lý:** Làm sạch `Packages/manifest.json`, loại bỏ các module không còn tồn tại, giữ lại đúng các package chuẩn (URP 17.3.0, Test Framework 1.6.0, UGUI 2.0.0, IMGUI).

### 2.2. Lỗi C# Compilation (CS1503)
- **Hiện tượng:** Biên dịch thất bại với lỗi `CS1503: Argument 5: cannot convert from 'UnityEngine.GameObject' to 'UnityEngine.Transform'` tại `Assets/SORA/Scripts/SoraDemoWorld.cs`.
- **Nguyên nhân:** Các hàm sinh khối primitives (`Sphere`, `Cube`) nhận tham số `parent` kiểu `Transform`, nhưng mã nguồn truyền vào đối tượng `GameObject root`.
- **Xử lý:** Chuẩn hóa toàn bộ lệnh gọi sang `root.transform`.

### 2.3. Lỗi GraphicsSettings Bootstrap (PR #1)
- **Hiện tượng:** Khởi tạo project trong Editor bị lỗi do gọi `AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")`.
- **Xử lý:** Cập nhật sang API chuẩn `GraphicsSettings.GetGraphicsSettings()` và bọc trong `SerializedObject` để cấu hình shader URP an toàn.

### 2.4. Khắc phục lỗi gói AI Preview xung đột
- **Hiện tượng:** Khi kích hoạt các gói AI Assistant preview, các script nằm ngoài thư mục Assets không có asmdef gây chặn Play Mode (`All compiler errors have to be fixed before you can enter playmode`).
- **Xử lý:** Gỡ bỏ các dependency AI dở dang khỏi manifest, khôi phục trạng thái biên dịch sạch 100%.

### 2.5. Khắc phục NullReferenceException khi Domain Reload
- **Hiện tượng:** Unity hot-reload mã nguồn khi đang chạy có thể làm mới biến runtime làm `ActiveCharacter` bị null trong 1 frame.
- **Xử lý:** Thêm null-guard tại `Update()` và `OnGUI()` trong `SoraDemoCore.cs`.

---

## 3. Tự động hóa CI/CD & GitHub Actions

Đã thiết lập hệ thống tự động hóa hoàn chỉnh nhằm giải phóng chủ dự án khỏi việc phải thao tác build thủ công:
- **Tập tin workflow:** `.github/workflows/sora-gameci-build.yml`
- **Môi trường chạy:** GitHub-hosted runner `ubuntu-latest` kết hợp Docker image chính thức `unityci/editor:ubuntu-6000.3.7f1-windows-mono-3.2.2`.
- **Cơ chế hoạt động:**
  1. Kiểm tra sự tồn tại của `UNITY_LICENSE` secret.
  2. Cache thư mục `Library/` qua `actions/cache@v4` giúp tăng tốc build.
  3. Tự động chạy toàn bộ 5 EditMode tests và tải lên `SORA-TestResults`.
  4. Tự động sinh Scene `SORA_Prototype.unity` và build Windows Standalone x64.
  5. Đóng gói và xuất file thực thi `SORA_After_the_Rain.exe` lên GitHub Actions Artifacts (`SORA-Windows-x64`).
- **Bảo mật:** Không commit bất kỳ license, email hay token nào vào mã nguồn.

---

## 4. Kết quả nghiệm thu kiểm thử & Build thực tế

| Hạng mục | Trạng thái | Ghi chú bằng chứng |
|---|---|---|
| **C# Compilation** | **PASS (0 lỗi)** | Biên dịch sạch 3 assembly: `Sora.Demo`, `Sora.Demo.Editor`, `Sora.Demo.EditModeTests` |
| **Offline Structural Validation** | **PASS** | `python Tools/validate_project.py` kiểm tra 87 files, 22 GUIDs duy nhất |
| **EditMode Unit Tests** | **PASS (5/5)** | 5 tests nghiệp vụ: POS draft, 100 character swaps, inventory conservation, relationship isolation, empty stock |
| **Scene Generation** | **PASS** | Sinh thành công scene `Assets/SORA/Scenes/SORA_Prototype.unity` và URP pipeline assets |
| **Windows Standalone Build** | **PASS** | Sinh file `Builds/Windows/SORA_After_the_Rain.exe` (652 KB) kèm thư mục dữ liệu |
| **GPU Play Test** | **PASS** | Khởi chạy thực tế trên NVIDIA GeForce RTX 3050 Laptop GPU, 60 FPS mượt mà |

---

## 5. Nâng cấp Gameplay & Đồ họa môi trường (Theo yêu cầu người dùng)

### 5.1. Hệ thống Camera góc nhìn thứ 3 (Third-Person Follow Camera)
- **Vị trí mặc định:** Camera được căn chỉnh ở cự ly vàng **3.6m** ngay sau lưng nhân vật (thay cho cự ly 6.5m quá xa của bản mẫu).
- **Xoay tự do:** Giữ **chuột phải** và rê chuột để tự do xoay góc nhìn 360 độ quan sát toàn cảnh.
- **Tự động cân bằng (Auto-Recenter):** Khi **thả chuột phải**, camera tự động lướt mượt mà quay về ngay sau lưng nhân vật theo hướng nhìn của nhân vật đang di chuyển.
- **Tiện ích:**
  - Phím **C** hoặc **nhấp chuột giữa**: Snap camera về sau lưng ngay lập tức.
  - **Con lăn chuột**: Zoom mượt khoảng cách từ 2.0m đến 6.5m.

### 5.2. Tái tạo mô hình nhân vật — Phân biệt rõ ngoại hình nam / nữ
- **Tei Toshiharu (Anh trai - 18 tuổi):**
  - Chiều cao 1.78m, dáng vóc nam tính, bờ vai rộng.
  - Mái tóc đen ngắn anime có mái tỉa và tóc mai hai bên.
  - Áo thun / hoodie xanh dương viền cổ trắng dáng suông.
  - Quần jean dài tối màu, thắt lưng và giày thể thao sneaker đế trắng.
- **Tei Akiko (Em gái - 18 tuổi):**
  - Chiều cao 1.65m, dáng nữ thanh thoát, eo thon.
  - Kiểu tóc **Twin-tail dài hai bên buộc nơ hồng** buông dài qua vai, mái bằng anime.
  - Mắt anime to tròn long lanh có điểm sáng phản chiếu (highlights).
  - Áo cardigan len hồng pastel phối cổ áo thủy thủ trắng.
  - **Chân váy xếp ly xòe (Pleated skirt)** phong cách nữ sinh, vớ đùi cao cổ đen và giày da nữ sinh.

### 5.3. Mở rộng cả khu phố Tokyo đêm mưa (Tokyo Rainy Street)
- **Lối đi mở:** Cửa kính tự động mở lối đi rộng 2.4m, nhân vật tự do bước từ trong quầy hàng ra ngoài phố.
- **Cụm máy bán nước tự động (Vending Machines):** Hai cây máy bán nước nóng/lạnh màu đỏ & xanh phát sáng neon rực rỡ kèm thùng phân loại rác cạnh cửa.
- **Hộp thư đỏ Nhật Bản:** Hòm thư bưu điện đỏ đặc trưng góc vỉa hè.
- **Quán mì Ramen bên trái (Menya SORA):** Dãy nhà gỗ truyền thống, mái ngói dốc hứng mưa, hàng đèn lồng đỏ (Chochin 提灯) phát sáng ấm cúng và rèm Noren.
- **Quán Izakaya & Cafe đêm bên phải:** Biển hiệu neon vàng hổ phách, thùng hàng và xe đạp dựng ngoài tường.
- **Đại lộ 80m & Vạch sang đường:** Mặt đường nhựa ướt mưa lấp lánh phản chiếu ánh đèn neon, vạch sọc qua đường (Zebra Crosswalk) và dải gạch vàng dẫn đường cho người khiếm thị trên vỉa hè.
- **Dãy phố cao tầng đối diện:** 5 tòa nhà cao tầng Tokyo (Nhà thuốc 24H đèn xanh, Karaoke 7F biển đỏ, chung cư cửa sổ sáng đèn, bồn nước mái nhà) và trạm xe buýt Tokyo Metro có mái che.
- **Hệ thống cột điện Tokyo:** Cột điện bê tông biến áp xám và đèn đường vàng rực.
- **Hệ thống mưa:** Hạt mưa rơi bao phủ toàn bộ không gian đại lộ 85m.

---

## 6. Hướng dẫn trải nghiệm bản game

1. **Chơi trực tiếp trong Unity Editor:**
   - Mở Unity Editor `6000.3.7f1`.
   - Mở scene `Assets/SORA/Scenes/SORA_Prototype.unity`.
   - Bấm nút **Play** `[ ▶ ]` ở giữa mép trên màn hình.
2. **Chơi bản Standalone Windows (.exe) không cần Unity:**
   - Truy cập thư mục `Builds\Windows\`.
   - Chạy tập tin `SORA_After_the_Rain.exe`.
3. **Phím điều khiển:**
   - **WASD**: Di chuyển nhân vật theo hướng camera.
   - **Giữ Shift**: Chạy nhanh.
   - **Tab**: Chuyển đổi điều khiển giữa Toshiharu và Akiko.
   - **E**: Tương tác (quét mã POS, thanh toán tiền, xếp hàng lên kệ, nói chuyện khách).
   - **Giữ chuột phải**: Xoay tự do camera (thả ra tự quay lại sau lưng).
   - **Phím C / Chuột giữa**: Đưa camera về ngay sau lưng lập tức.
   - **1 / 2 / 3**: Chọn phương án hội thoại với khách hàng.
   - **F5 / F9**: Lưu nhanh / Tải lại tiến trình.

---

## 7. Trạng thái Git & Pull Requests

- **Repository:** https://github.com/haizzdungnay/SORA-After-the-Rain
- **Branch làm việc:** `feature/gameci-automated-build` (dựa trên `develop`)
- **Pull Request #3:** https://github.com/haizzdungnay/SORA-After-the-Rain/pull/3
- **Toàn bộ mã nguồn, scene, URP settings và báo cáo đã được commit và push lên GitHub.**
