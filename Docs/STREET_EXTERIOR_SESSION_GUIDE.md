# SORA — Hướng dẫn tiếp tục một phiên xây ngoại thất

Đọc [kế hoạch chính](STREET_EXTERIOR_REBUILD_PLAN.md), [quyết định kitbash thành phố SORA](StreetExterior/AFX_KITBASH_CITY_DESIGN_DECISION_2026-11-06.md) và [backlog](STREET_EXTERIOR_WORK_PACKAGES.csv) trước khi chọn việc. **Tách module từ AFX để xây thành phố SORA bố cục mới; tuyệt đối không copy nguyên scene AFX thành map.** Tài liệu này là quy trình cho các phiên sau, không khẳng định có thêm scene hoặc asset đã hoàn thành.

## 1. Bắt đầu phiên

1. Đọc `AGENTS.md`, GDD, kế hoạch ngoại thất và bàn giao gần nhất.
2. Fetch `main`, `develop`, nhánh liên quan và PR mở. Kiểm tra thay đổi của người khác trước khi sửa.
3. Chọn gói nhỏ nhất có dependency đã đạt. Với gói lặp, chọn **một** ID căn hoặc đoạn cụ thể.
4. Ghi mục tiêu và những điều kiện cần xem được cuối phiên. Nếu chưa có Unity/file mẫu thì chỉ làm phần không phụ thuộc; ghi phần còn lại `BLOCKED`.
5. Tạo `feature/street-exterior-<scope>` từ baseline phù hợp. Giữ các thay đổi của người khác và prototype cũ.

## 2. Kích thước một phiên

Một phiên chỉ cần hoàn tất một phần có thể review được: audit một nguồn, chốt một đoạn mặt bằng, hình học một mặt tiền, vật liệu/biển một mặt tiền, ghép hai căn, hoặc kiểm tra mưa một đoạn. Không gộp thay nhân vật, rewrite POS, thêm quest và dựng phố trong cùng PR.

Một cửa hàng khó có thể có các gói con: `geometry` → `materials-signage` → `props-glazing` → `lighting-weather` → `review`. Chỉ hàng căn tổng được `DONE` khi toàn bộ phần cần thiết đã pass. Mọi căn dùng cùng chuẩn ngoại thất; phân kỳ theo thời gian, không hạ chuẩn những căn làm sau.

## 3. Bằng chứng tối thiểu cuối phiên

| Loại phiên | Bằng chứng |
|---|---|
| Audit G0 | Link nguồn/quyền dùng, mesh/node inventory, category + KIT-ID candidates, pivot/UV/tính tách được, ảnh module từ mô hình thật, phần thiếu phải dựng mới |
| Bố cục G1 | Mặt bằng thành phố SORA mới theo gameplay (không copy hẻm AFX); SORA MART, hướng L/R, F-ID/SEG-ID, mapping KIT-ID, camera và tỷ lệ có nhãn |
| Một mặt tiền | Front + nhìn chéo + mặt khác nhìn thấy; ảnh debug sáng và đêm; bảng EX-01…12 có pending rõ |
| Một đoạn | Ảnh nhìn hai chiều, video đi bộ, kiểm tra seam/collider/camera và đồ vật tiếp xúc nền |
| Tích hợp | Log compile, test/regression đã chạy và bước kiểm tra state; video/ảnh scene thực |
| Tối ưu/Windows | Cấu hình máy, commit, preset/resolution, tuyến đo, log, frame time và vấn đề còn lại |

`python Tools/validate_project.py` chạy trong các PR theo quy tắc repo; kết quả này chỉ xác minh cấu trúc. Không gọi đó là kiểm tra render, compile C# hoặc bằng chứng ngoại thất đẹp. Gói tài liệu không cần chụp Unity; gói thay hình ảnh cần ảnh thật.

## 4. Biểu mẫu bàn giao

Lưu báo cáo thực tế dưới `Docs/StreetExterior/Reports/<date>-<package-id>.md` khi có phiên thực hiện. Thư mục/đường dẫn này là đề xuất; hiện chưa có báo cáo xây dựng ở đó.

```text
# Bàn giao <EXT-ID / F-ID / SEG-ID>
Ngày (Việt Nam):
Base branch + SHA:
Nhánh / commit / PR:
Giai đoạn / gói / ID căn-đoạn:
Trạng thái: PLANNED | READY | IN_PROGRESS | REVIEW | DONE | BLOCKED

Mục tiêu phiên:
Nguồn và quyền dùng đã kiểm tra:
File/asset đã thay đổi, phần thích nghi khác mẫu:
Kết quả thực tế:
Ảnh/video/log (đường dẫn hoặc URL có quyền dùng phù hợp):
QA ngoại thất: EX-01..12, ghi PASS/FAIL/PENDING cho từng mục áp dụng
Kiểm tra đã chạy (lệnh + kết quả thực):
Kiểm tra chưa chạy và lý do:
Số liệu hiệu năng (nếu phiên có đo):
Vấn đề / blocker / rủi ro:
Nhận xét mỹ thuật của chủ dự án (nếu có):
Việc còn dở và cách khôi phục:
Gói tiếp theo + dependency + điểm bắt đầu chính xác:
```

## 5. Prompt cho phiên đầu tiên sau kế hoạch

```text
Tiếp tục SORA: After the Rain trong repo haizzdungnay/SORA-After-the-Rain.
Đọc AGENTS.md và Docs/STREET_EXTERIOR_REBUILD_PLAN.md cùng backlog/session guide.
Fetch refs/PRs hiện tại; tìm nhánh/PR kế hoạch, không mặc định main đã có tài liệu.

Chỉ thực hiện G0: EXT-001, EXT-002 và EXT-003 khi đủ điều kiện.
Mẫu chủ dự án chọn: Japanese street at night của AFX/CGMotion, model UID
fb1bdcd71a5544d699379d2d13dd1171. Ngoại thất phố ngoài trời mưa đêm là phạm vi
hiện tại; chưa làm nội thất mới hoặc mở rộng khu phố.

Xác minh cách mở mô hình hợp lệ, điều khoản tác giả + phạm vi dùng trong repo
công khai. Không tự mua asset, chấp nhận điều khoản thay người dùng hoặc đăng
nguồn chưa rõ quyền phân phối. Nếu viewer không hoạt động, ghi blocker cụ thể;
không tuyên bố đã xem đủ mọi góc và không tự chế một phố thay thế.

Khi mở được: kiểm kê từng mặt tiền, mặt bên/mặt sau thấy được, đầu/cuối phố,
các file/texture, tỷ lệ, mặt thiếu; chụp overview và góc đối chiếu thực tế.
Phân biệt dữ liệu đo với ước lượng; xác định có sẵn cửa hàng tiện lợi hay cần
thích nghi riêng. Thay hàng mẫu bằng inventory thật sau khi đủ dữ liệu.

Đầu ra: báo cáo G0, inventory có nguồn, cập nhật trạng thái backlog và gói kế.
Giữ nguyên gameplay/nhân vật; không bắt đầu G1/G2 hay dựng hàng loạt trong phiên.
Chạy kiểm tra phù hợp, mở PR và ghi rõ bằng chứng nào đã có/chưa có.
```
