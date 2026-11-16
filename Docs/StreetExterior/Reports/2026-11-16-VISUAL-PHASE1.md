# SORA MART — VIS-00 / VIS-01, 16/11/2026

## Phạm vi và kết quả

Giai đoạn đầu chỉnh ánh sáng và vật liệu trên tuyến mặt tiền, lối kệ và quầy. Shop chuyển từ trắng xanh sang trắng ấm; ngoài trời giữ xanh đêm. Đây chưa phải nghiệm thu chất lượng cuối theo video: bóng tiếp xúc, phản chiếu người, mưa/ô và camera còn thuộc VIS-02–VIS-06.

Base: `0436cbd`, `origin/main` sau khi PR #36/#38 được gộp; đã chứa ô tùy chọn tại `80e499e`. Nhánh `feature/store-visual-style`, worktree riêng; checkout đang dở của chủ dự án được giữ nguyên trong lúc triển khai.

## Thay đổi và nơi sở hữu

| File | Thay đổi |
|---|---|
| `SoraDemoWorld.cs` | Native Trilight: sky `(0.28,0.34,0.52)`, equator `(0.40,0.38,0.37)`, ground `(0.13,0.12,0.16)`; giữ fog và Gamma color space |
| `SoraCityRuntimeWorld.cs` | Đèn directional nền xanh là nguồn ám màu trong shop; intensity `0.45 → 0.22`, màu `(0.52,0.65,1)` |
| `SoraMartVisuals.cs` | Chỉnh sáu đèn sẵn có về intensity `6`, range `7.5`, màu `(1,0.92,0.82)`; đèn trước cửa bớt xanh; spot spill `10 → 8`, range `18 → 16`; không thêm đèn |
| `SoraWeatherSetup.cs`, `NightVolume.asset` | Giữ Neutral; exposure `+0.35 → +0.2`, contrast `12 → 8`, saturation `8 → 4`, bloom `0.25 → 0.18`, threshold `1.1 → 1.4`; Unity sinh asset bằng `Prepare()` |
| `SoraWeather.cs` | Chọn SMAA native sau A/B 1080p; giữ nét tóc, thiết bị và nhãn tốt hơn FXAA trong góc kiểm tra; chưa đo chi phí GPU riêng |
| `build_sora_mart.py`, `SoraMart.glb`, `SORA_MART_ASSETS.json` | Tách nền gạch satin roughness `0.34` khỏi tường roughness `0.76`; giảm bóng khung/kim loại; kính trung tính hơn, giữ alpha `0.13`; bỏ emission nhỏ trên trần |
| `SoraStoreVisualReview.cs`, `SoraWeatherReview.cs` | Bốn camera cố định, ảnh thật 1920×1080 và JSON cấu hình; dùng lại hàm capture hiện có; không đọc/ghi save người chơi |
| `Tools/verify_store_visual_captures.py` | Kiểm tra hai bộ ảnh cùng camera, FOV, model, vị trí, city và trạng thái thời tiết |

Không thêm package, shader, geometry/model nguồn, shadow feature hoặc hệ thống cấu hình mới. `.meta`/GUID cũ được giữ. 154 mesh nhập khẩu giữ nguyên từng tam giác, normal, UV, màu vertex, winding, transform và tên material; thứ tự vertex khi export có thể đổi. 26 ảnh nhúng giữ nguyên byte. Chỉ năm material SORA thay đổi và thêm một material nền.

## Đối chứng thật

Capture trên Unity `6000.3.7f1`, URP `17.3.0`, Direct3D 11, NVIDIA GeForce RTX 3050 Laptop GPU, Gamma, 1920×1080. Hai twin dùng Zhongli/Chisa. Heavy rain cố định: rain/wetness `1`, puddles bắt đầu `0.9`; không chạy chu kỳ tự động.

| Góc | Camera / nhìn tới / FOV |
|---|---|
| C01 | `(0,1.65,-13.5)` / `(0,1.65,-5.5)` / 58° |
| C02 | `(0.2,1.7,-4.6)` / `(0.1,1.45,2.8)` / 60° |
| C03 | `(-1.8,1.7,-4.6)` / `(-4.85,1.3,-2.5)` / 50° |
| C04 | `(-0.5,1.1,-12)` / `(0.8,1.1,-7.3)` / 60° |

C03/C04 điều chỉnh một lần ở baseline để tránh NPC che quầy và giữ tán ô trong khung. C01–C03 tay không; C04 harness chủ động cho Chisa cầm ô. Đây là pose review cố định, không phải bằng chứng input hay một lần lấy ô trong gameplay.

Nguồn local tại `Artifacts/VisualStyleAcceptance/`: baseline public/owner, từng lượt thử, ảnh cuối, JSON, log, hash và bảng trước–mẫu–sau màu/thang xám. Các khung video 06/14/16/24s chỉ làm đối chứng local, không đưa vào asset hoặc PR công khai. Bố cục khác mẫu nên không so pixel 1:1.

Ảnh PR bên dưới chỉ chứa scene dùng asset công khai. Ảnh owner và khung video giữ local.

![Mặt tiền trước](../../Media/Changelog/sora-visual-phase1-before-front.png)
![Mặt tiền sau](../../Media/Changelog/sora-visual-phase1-after-front.png)
![Quầy trước](../../Media/Changelog/sora-visual-phase1-before-checkout.png)
![Quầy sau](../../Media/Changelog/sora-visual-phase1-after-checkout.png)

## Kiểm tra

| Kiểm tra | Kết quả |
|---|---|
| Baseline offline validator / Git LFS | PASS: 410 GUID, 1228 tracked files; `git lfs fsck` OK |
| Baseline Unity EditMode, Direct3D 11 | 69/69 PASS, 0 skipped |
| Baseline public và owner | 4 ảnh/cấu hình mỗi bộ; đã xem ảnh và chỉnh góc trước khi đổi runtime |
| Blender 5.2.3 export | Exit 0; GLB 65,381,700 byte; hình học/UV/texture nhập khẩu được đối chiếu như trên |
| So sánh framing / thời tiết | PASS cho C01–C04 của các lượt lighting và material/tone |
| Final Unity EditMode, Direct3D 11 | 69/69 PASS, 0 skipped |
| Final native weather Play Mode | PASS: 100 switches, 8 controller routes, 9 roof probes, weather/legacy/malformed save và POS |
| Final native motion Play Mode | 326 checks, 0 runtime errors; sáu model, ô tùy chọn và ranh giới mái |
| Final Windows x64 build / khởi động | PASS: build exit 0, player Direct3D 11 tải shop, city owner và weather; không thấy exception/runtime error trong log player |
| Phím OS, lần đầu | Đã xác nhận Enter, Tab, F5/F9, E lấy/trả ô, E draft/cancel/pay và một sale duy nhất qua snapshot save; save cũ phục hồi nguyên byte |
| Phím OS, giới hạn | Bộ điều khiển cửa sổ và timing không ổn định qua các lần chạy; lần đầu thất bại ở tuyến W qua cửa, lần sau không tái hiện ổn định các bước đã qua. Không tính toàn bộ keyboard walkthrough PASS; cần người chơi duyệt lại |
| Ảnh cuối public/owner và grayscale | 4 góc mỗi bộ; framing/weather verifier PASS. Bảng màu/thang xám giữ local với ảnh video |

Lượt capture đầu thất bại vì `UnityEditor.Search.SearchDatabase` ném exception khi tạo chỉ mục. Harness giữ riêng lỗi đúng stack Editor này trong `editor-search.txt`; lỗi runtime khác vẫn làm capture thất bại. Log gốc vẫn giữ các thông báo Search/entitlement/TLS của Editor. Không gọi log có các thông báo này là hoàn toàn sạch.

Review độc lập phát hiện guard từ chối output cũ vẫn ghi `failure.txt` vào bằng chứng cũ. Đã sửa để chỉ ghi khi harness sở hữu output mới; compile/capture public sau sửa đã qua. Test từ chối chạy lại trên thư mục có ảnh và đối chiếu toàn bộ hash được ghi riêng trong log nghiệm thu.

## Tái hiện

Đóng Editor của đúng project trước mỗi lệnh. Dùng Unity đã kích hoạt, giữ graphics device cho capture và test GPU; không dùng `-nographics`. Không truyền `-quit` cho review hoặc Test Runner. Review tự thoát. Chọn output mới để giữ bằng chứng cũ.

```powershell
python Tools/validate_project.py
# Unity arguments: -batchmode -force-d3d11 -projectPath <root>
# Baseline: -soraVisualReview -soraVisualOutput <fresh-before-folder> -logFile <before.log>
# After lighting/material edits: add -soraVisualPrepare once to regenerate NightVolume.asset.
# Final default is native SMAA. A/B AA: add -soraVisualFxaa for the FXAA comparison.
python Tools/verify_store_visual_captures.py <before-folder> <after-folder>
# Existing regression: -runTests -testPlatform EditMode -testResults <xml> -logFile <log>
# Existing runtime acceptance: -soraWeatherReview / -soraMotionReview, each in a separate run.
```

## Giới hạn và bước tiếp

Native ambient và đèn điểm chưa thay thế GI/bóng tiếp xúc. Mặt trước nhân vật ngoài cửa vẫn kém sáng và có chiều sâu kém video; không dùng tăng exposure toàn cảnh để tuyên bố đã đạt. Kệ và NPC placeholder còn khác phong cách mẫu. SSAO/shadow, phản chiếu, mưa/ô và motion/camera sâu giữ đúng các gói tiếp theo.

Ảnh review tĩnh không chứng minh camera gameplay, FPS hoặc khả năng dựng kịch bản cuối. Code VIS-01 đã có, VIS-00 còn mục gameplay/phím cần duyệt lại; không đánh dấu cả giai đoạn đã nghiệm thu. Chỉ VIS-06 mới quyết định nghiệm thu tổng thể với walkthrough liên tục, profiling Windows và duyệt của chủ dự án.
