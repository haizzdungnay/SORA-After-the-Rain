# SORA: After the Rain — Unity Development Environment v0.2

## Bắt đầu Play Test trên Windows — 09/11/2026

- Trong Unity, chọn **SORA > 6. Play Demo** hoặc nhấn **F6** khi đã dừng Play Mode. Lệnh mở đúng `Assets/SORA/Scenes/SORA_Prototype.unity` rồi chạy; nếu scene hiện tại chưa lưu, Unity hỏi lưu trước.
- Nếu bấm Play trong scene `Untitled` chỉ có Main Camera/Directional Light, game không được khởi tạo. Dừng Play, dùng lệnh trên. Có thể dùng **SORA > 2. Open Demo Scene** rồi bấm Play.
- Menu chọn model: **1–6**, **Enter** để vào game, **M** để mở lại. **WASD / Shift** di chuyển/chạy, **Tab** đổi nhân vật, **E** tương tác, giữ **chuột phải** để xoay camera, **F5/F9** lưu/tải.
- Map đã import trực tiếp bằng glTFast. Nếu có hai ZIP riêng trong `3d model enviroment/`, chạy `python Tools/StreetExterior/install_private_city.py` trước khi mở Unity để dùng **AFX V03 + V05 ZIP + collider đúng cặp**. Không cần chuyển FBX. Máy không có ZIP riêng dùng **V03 public + V05 public + V06**; không nạp V04 cùng V05.
- [Báo cáo tích hợp và ảnh Unity mới](Docs/StreetExterior/Reports/2026-11-09-LOCAL-ENVIRONMENT-INTEGRATION.md): Play Mode đã nạp **5.704 renderer**, kiểm tra switch/POS/save-load và hai controller ở 16 đầu đường + 4 cạnh map bằng script. Xem báo cáo để biết kết quả test/build cuối cùng và giới hạn QA; không coi kiểm tra tự động là walkthrough thủ công.

**Ngày chuẩn bị:** 02/11/2026  
**Mục tiêu:** chuẩn hóa môi trường Unity 6.3 để import source, tạo scene, Play Test, kiểm tra compile và xuất Windows x64.  
**Trạng thái nghiệm thu:** _Các bước kiểm tra cấu trúc offline đã PASS; CHƯA cài Unity Editor, CHƯA compile trong Editor, CHƯA chạy Play Mode và CHƯA build được EXE trong container tạo gói._

## 1. Tại sao chưa thể chạy Unity trong môi trường ChatGPT hiện tại?

Kiểm tra trực tiếp môi trường vào 02/11/2026:
- Debian 13 x86_64; khoảng 5.8 GiB RAM, 30 GiB đĩa trống, có quyền root.
- Không thấy Unity Editor, Unity Hub hoặc GPU DRM `/dev/dri`.
- `curl https://hub.unity3d.com/...` và `https://download.unity3d.com` trả về `Could not resolve host`.
- Unity Editor cần tải bản phân phối và kích hoạt license của chủ tài khoản. Unity Personal chỉ kích hoạt chính thức bằng Unity Hub (tham khảo tài liệu Unity).

**Không có cách hợp lệ để biến container này thành Unity Editor có giao diện và license chỉ bằng việc cung cấp source.** Bộ này vì thế được thiết kế để mở trên Windows PC đã cài Unity, và có kịch bản build tự động do anh chạy.

## 2. Mở project trực tiếp trên Windows

1. Giải nén ZIP ra thư mục **không dấu/không nằm trong OneDrive**, ví dụ `D:\SORA\SORA_Unity_Dev_Environment_2026-11-02`.
2. Cài **Unity Hub** từ https://unity.com/download, đăng nhập tài khoản Unity của anh, chọn license phù hợp trong Hub.
3. Trong Hub chọn **Unity Editor 6000.3.7f1 (Unity 6.3 LTS)**. Khi cài, giữ thành phần Windows Build Support nếu Hub hiển thị tùy chọn này.
4. Mở PowerShell tại thư mục đã giải nén và chạy:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\Tools\Setup-SORA.ps1
   ```

   Script kiểm tra Hub/Editor. Nếu muốn thử cài Editor bằng CLI của Hub:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\Tools\Setup-SORA.ps1 -InstallEditor
   ```

   > Hub CLI hiện là experimental/deprecated; nếu lệnh cài đặt không chạy, dùng Hub GUI. Tài khoản/license cần được chính anh kích hoạt.

5. Unity Hub > **Projects > Add project from disk**, chọn **thư mục gốc** có `Assets`, `Packages`, `ProjectSettings`. Không cần copy file qua một project khác.
6. Đợi Package Manager tải URP 17.3.0. Nếu Unity yêu cầu nâng cấp/re-import project settings, xem log lỗi trước khi đồng ý. Không cập nhật Unity Editor tự động nếu muốn build tái lập cùng phiên bản.
7. Vào **SORA > 1. Generate Demo Scene**. Mở `Assets/SORA/Scenes/SORA_Prototype.unity`, nhấn **Play**.
8. Nếu Input error, trong **Edit > Project Settings > Player > Active Input Handling**, chọn **Both** hoặc **Input Manager (Old)** rồi restart.
9. Nếu material bị hồng, kiểm tra Render Pipeline asset của project. Phiên bản này có shader fallback cho Built-in, còn pipeline visual URP hoàn chỉnh cần cấu hình URP asset trước giai đoạn nâng cấp đồ họa.

## 3. Chạy smoke check và xuất Windows `.exe`

Sau khi Editor đã cài + license kích hoạt:

```powershell
# Import source + biên dịch + tạo Scene (KHÔNG chạy gameplay)
powershell -ExecutionPolicy Bypass -File .\Tools\Build-SORA.ps1 -SmokeOnly

# Build Windows x64 qua Editor -batchmode (KHÔNG chạy gameplay)
powershell -ExecutionPolicy Bypass -File .\Tools\Build-SORA.ps1
```

Nếu Editor ở đường dẫn khác, thêm `-UnityExe 'D:\Unity\...\Editor\Unity.exe'`.
- Log: `Logs\smoke-check.log` hoặc `Logs\windows-build.log`.
- Build: `Builds\Windows\SORA_After_the_Rain.exe` (+ thư mục dữ liệu đi kèm).
- Nếu smoke PASS nhưng Play vẫn lỗi thì chưa đạt nghiệm thu. Cần chạy Play Mode theo checklist ở `Docs/TEST_CHECKLIST.md`.
- Không chạy đồng thời Unity Editor GUI và batchmode trên cùng thư mục project.

## 4. Kiểm tra cấu trúc mà không cần Unity

```powershell
python Tools\validate_project.py
```

Chỉ kiểm tra source/file manifest. Không được xem kết quả này là C# compile PASS hay test gameplay.

## 5. Các file chính

- `Assets/SORA/Scripts/`: logic gameplay của prototype v0.1.
- `Assets/SORA/Scripts/Editor/SoraDevAutomation.cs`: tự tạo scene, smoke check, build Windows.
- `Packages/manifest.json`: pin URP 17.3.0 cho Unity 6.3.
- `ProjectSettings/ProjectVersion.txt`: khóa Unity Editor 6000.3.7f1.
- `Tools/Setup-SORA.ps1`: kiểm tra Hub, tùy chọn cài Editor theo CLI.
- `Tools/Build-SORA.ps1`: smoke/build trong Unity batchmode.
- `Tools/validate_project.py`: kiểm tra offline.
- `.github/workflows/sora-static-check.yml`: GitHub CI kiểm tra cấu trúc source, không cần license.
- `.github/workflows/sora-local-runner-build.yml`: **tùy chọn** build bằng Windows self-hosted runner đã có Editor + license; chưa cấu hình runner nên workflow không tự chạy.
- `Docs/TEST_CHECKLIST.md`: quy trình test 5–10 phút.

## 6. Giới hạn về chất lượng và license

- Model nhân vật/mưa/ánh sáng hiện là primitive, chưa phải demo đồ họa anime theo video; chưa có file `.exe` được nghiệm thu.
- **Không chia sẻ thông tin đăng nhập tài khoản Unity và không commit license `.ulf` vào GitHub.**
- Không sử dụng cách lách hoặc bypass kích hoạt Unity. Nếu dùng máy khác hoặc CI thì cần cấu hình license hợp lệ.
- Unity Hub CLI không bảo đảm cài được trên mọi phiên bản Hub; luôn có lựa chọn thủ công trong Hub.

## 7. Đánh giá hoàn thành

| Bước | Trạng thái khi tạo gói |
|---|---|
| Có cấu trúc project Unity | ĐÃ TẠO |
| Có source gameplay | ĐÃ CHÉP + điều chỉnh shader fallback |
| Có pin Unity Editor/URP | ĐÃ TẠO (chưa thử import) |
| Có script setup/smoke/build | ĐÃ TẠO (chưa chạy trên Windows) |
| Offline integrity check | PASS |
| Unity Editor installed / activated | CHƯA |
| Unity C# compile | CHƯA |
| Play Mode | CHƯA |
| Windows x64 build | CHƯA |

---

**Nguồn tham khảo:**
- Unity Hub Linux: https://docs.unity.com/en-us/hub/install-hub-linux
- Unity Hub CLI: https://docs.unity.com/en-us/hub/use-hub-cli
- Unity license: https://docs.unity.com/en-us/engine/6000.3/manual/get-started/install-and-upgrade/licenses-and-activation/license-activation-methods
- Unity command line: https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/command-line-arguments/run-unity

## GitHub import update — 02/11/2026

Clone the public repo `https://github.com/haizzdungnay/SORA-After-the-Rain.git` instead of maintaining ZIP copies. Before scene generation, run **SORA > 0. Configure Prototype Project**; it creates URP assets and retains runtime shaders. Scene generators/build automation call it too. This new setup path still requires Unity verification. InputManager axes are included; retain legacy Input Manager or Both. New EditMode tests and manual licensed CI instructions: `Docs/CI_SETUP.md`. Older container observations above describe the source ZIP preparation session, not the current host.
