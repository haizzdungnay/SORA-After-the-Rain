# SORA — Kế hoạch xây lại ngoại thất con phố

**Ngày:** 05/11/2026 (Việt Nam)

**Trạng thái cập nhật 10/11/2026:** Map đã có tích hợp Unity; chủ dự án ưu tiên hoàn thiện ngoại thất và nội thất SORA MART hiện có trước. Xem [báo cáo SORA MART](StreetExterior/Reports/2026-11-10-SORA-MART.md). Các bảng baseline trong tài liệu này ghi lại thời điểm lập kế hoạch, không thay thế bằng chứng kiểm tra mới.

**Mốc mã nguồn đã đọc:** `main` tại `083a29b0b19ac08fd408c0e7efa8e7eb988e8a2b`.

**Mẫu mỹ thuật chính đã được chủ dự án chọn:** *Japanese street at night* — AFX/CGMotion 3DModel Maker.

**Phạm vi được yêu cầu hiện tại:** ngoại thất; thực hiện từng phần qua nhiều phiên.

Tài liệu này điều chỉnh hướng sản xuất môi trường tiếp theo. GDD, nhân vật, nghiệp vụ cửa hàng và dữ liệu chơi hiện có tiếp tục được giữ. Bố cục khu phố v0.5 là baseline để đối chiếu/khôi phục, không phải bố cục bắt buộc cho phố mới.


> **QUYẾT ĐỊNH MỚI 06/11/2026 — THAY THẾ CÁCH HIỂU BỐ CỤC:** Chủ dự án xác nhận **dùng các thành phần 3D bên trong AFX để xây dựng khu phố/thành phố SORA có bố cục hoàn toàn mới**. **KHÔNG copy/paste nguyên hẻm AFX; KHÔNG bắt buộc giữ A–J, kích thước hay vị trí source scene.** AFX là nguồn cấu kiện và chuẩn chất lượng tham chiếu. Xem [Quyết định AFX Kitbash → SORA city mới](StreetExterior/AFX_KITBASH_CITY_DESIGN_DECISION_2026-11-06.md). G0 phải bổ sung inventory cấp component; G1 định hình bản đồ theo gameplay, rồi gắn các module mới/tái chế phù hợp.

## 1. Các quyết định đã chốt

| Nội dung | Yêu cầu |
|---|---|
| Không gian | Phố Nhật ngoài trời, về đêm, có mưa; mái hiên cục bộ phù hợp, không chọn phố mái che kín làm mẫu chính |
| Tham chiếu | AFX cung cấp các cấu kiện và phong cách 3D; bản đồ SORA thiết kế mới có tuyến chính, ngã rẽ, hẻm phụ, SORA MART và tỷ lệ theo gameplay; không sao chép layout AFX |
| Mẫu chính | AFX/CGMotion; GLB đã được kiểm kê sơ bộ (149 mesh, 34 materials, 107 ảnh nhúng); chưa tách thành prefab, chưa import Unity |
| Chất lượng | Mọi cửa hàng trong vùng chơi cuối cùng đều hoàn thiện ngoại thất, kể cả mặt bên/mặt sau nhìn thấy được |
| Cửa kính | Phần nhìn qua kính có chiều sâu và bố trí hợp lý; không phát sinh yêu cầu nội thất có thể bước vào |
| Phạm vi đầu tiên | Hoàn thiện một đoạn phố liền mạch trước, sau đó mới xét mở rộng thành khu phố |
| Cách thực hiện | Một gói việc nhỏ mỗi phiên; có ảnh thực tế, bằng chứng và bàn giao để phiên sau tiếp tục |
| Giữ nguyên | Windows PC trước; Unity `6000.3.7f1`; URP `17.3.0`; hai nhân vật, POS, kho, hội thoại và save/load |

**Để sau:** nội thất cửa hàng mới có thể đi vào, tương tác cửa hàng mới, NPC/quest mới, mở rộng thành nhiều phố, nâng cấp nhân vật, mobile. Nội thất và nghiệp vụ SORA MART hiện có phải tiếp tục hoạt động khi tích hợp ngoại thất.

### Các phương án cũ không còn là bản thiết kế sản xuất

- Sơ đồ tự chia ô v0.1, kích thước 160 × 140 m và hai vòng đi bộ không được mặc định dùng lại.
- Shimokitazawa Ichibangai chỉ còn là tham khảo không khí/đời sống Tokyo; bộ dữ liệu hiện thu thập chưa đủ để tái hiện cả phố.
- Nimikko là nguồn phụ để quan sát chi tiết mặt tiền, không phải mẫu mái che của phố SORA. Chưa mua hay nhập asset của bộ này.
- Không mặc định AFX tái hiện đúng một địa chỉ ở Tokyo hoặc có sẵn cửa hàng tiện lợi SORA MART.

## 2. Hiện trạng repo và điều chưa kiểm chứng

| Hạng mục | Bằng chứng hiện có | Hệ quả cho kế hoạch |
|---|---|---|
| Khu phố v0.5 | `SoraNeighborhoodDistrict.cs`, envelope khoảng 168 × 143 m, 20 mặt tiền tạo bằng primitive | Cần thay chất lượng mô hình/vật liệu/cách authoring, không chỉ tăng số nhà |
| Bootstrap hiện tại | `SoraDemoWorld.Build()` tạo môi trường, cửa hàng, đường, nhân vật, khách, camera, mưa rồi gọi builder khu phố | Không thể chồng scene phố mới lên nguyên world cũ mà không xử lý hình học/collider trùng |
| Scene và nhân vật | Repo đã có scene, prefab/animation Akiko và ảnh Play Mode trong `Docs/Media/` | Giữ GUID và nội dung hiện có; ảnh nhân vật không chứng minh chất lượng phố mới |
| Kết quả Unity lịch sử | `D1report.md` báo compile/test/build/RTX 3050 PASS; một số README/QA cũ vẫn ghi pending | Ghi đây là báo cáo lịch sử; chạy baseline mới ở phiên tích hợp, không suy diễn rằng phố mới đã pass |
| AFX | Trang tác giả + metadata xác nhận mô hình tải được, khoảng 51.8k triangles, có preview | Đủ xác định ứng viên là mô hình 3D; chưa xác định số căn, mặt khuất, toàn bố cục hoặc chất lượng đứng gần |
| Trình xem AFX | Trong lần kiểm tra trước khi lập kế hoạch, viewer báo không hiển thị được mô hình | G0 phải giải quyết quyền truy cập/file và kiểm tra trực tiếp trước khi dựng theo mẫu |
| Nhánh | Lúc kiểm tra: `main` đi trước `develop` 22 commit, không có PR mở | Nhánh kế hoạch dựa trên `main`; không đưa các thay đổi cũ trở lại từ `develop` |

### Nguồn chính thức

1. [Mô hình gốc AFX/CGMotion trên Sketchfab](https://sketchfab.com/3d-models/japanese-street-at-night-fb1bdcd71a5544d699379d2d13dd1171).
2. [Metadata công khai của mô hình](https://api.sketchfab.com/v3/models/fb1bdcd71a5544d699379d2d13dd1171).
3. [Điều khoản bổ sung tác giả liên kết](https://affexion-license.web.app/license_en.pdf).
4. [Nguồn phụ Nimikko](https://nimikko.co.jp/en/assets/japanese-shopping-street-pack).

Metadata AFX ghi CC Attribution và liên kết CC BY 4.0; mô tả đồng thời dẫn tới PDF điều khoản riêng chưa được đọc đầy đủ. Đây là dữ kiện cần giải quyết ở G0, **không phải xác nhận quyền đưa file gốc lên repo công khai**. Kế hoạch này chỉ thêm văn bản/link, không phân phối mô hình, texture hay ảnh nguồn của bên thứ ba.

## 3. Cách chia con phố thành phần có thể làm xong

Phân biệt **source asset IDs** (từ model AFX; ví dụ `SRC-A`…`SRC-J`, `KIT-*` cho module thực sự được kiểm kê) với **target SORA frontage IDs** (`F-*` chỉ phát sinh từ bản đồ SORA). Không sử dụng số thứ tự source để quyết định thứ tự căn trong game. Bản vẽ map mới phải được thiết kế theo gameplay và có bản mapping KIT-ID → từng mặt tiền được ghép. Sau đó đánh số từng cửa hàng SORA trên bản đồ mới:

| Đối tượng | Quy ước ID | Cách chia |
|---|---|---|
| Mỗi mặt tiền/cửa hàng | `F-L-001`, `F-R-001`… | L/R theo một hướng đi cố định được ghi trong bản đồ audit; một cửa hàng một hồ sơ |
| Căn có nhiều mặt nhìn thấy | Giữ cùng `F-*`, ghi các mặt front/side/rear/roof-visible | Tránh đánh dấu xong mặt trước nhưng bỏ mặt bên |
| SORA MART | `F-SORA-001` | Một phần thích nghi riêng của SORA; chỉ định rõ vị trí/độ lệch so với mẫu |
| Từng đoạn sản xuất | `SEG-01`, `SEG-02`… | Mỗi đoạn thường 2–3 mặt tiền liền nhau và phần đường/đồ đạc đi kèm; điều chỉnh theo độ phức tạp |
| Đường và hạ tầng dùng chung | `ENV-ROAD-*`, `ENV-UTIL-*` | Dùng chung asset nhưng kiểm tra liên tục qua ranh giới đoạn |
| Điểm chụp đối chiếu | `CAM-01`… | Lưu transform/FOV/resolution để chụp lại cùng góc |

**Chưa đặt trước số căn, chiều dài phố hay vị trí SORA MART.** Số căn, chiều dài phố và kích thước giao lộ phải quyết định từ thiết kế game, không phải bbox hay số nhóm mesh AFX. Hoàn tất component inventory và sơ đồ thiết kế mới rồi mới lập tổng khối lượng. `STREET_FRONTAGE_AUDIT_TEMPLATE.csv` chỉ có hàng mẫu, chưa phải kiểm kê mô hình AFX.

Mỗi đoạn phải có ít nhất một góc đi bộ nhìn dọc và một góc nhìn ngược lại. Đầu/cuối phố, cửa ngõ và góc rẽ thuộc phạm vi nghiệm thu; không để camera nhìn thấy vách cắt mô hình. Phần chưa sản xuất phải được ghi rõ trong build thử và giới hạn tiếp cận; không được ghi là hoàn thành toàn phố.

## 4. Các giai đoạn và điểm dừng nghiệm thu

| Giai đoạn | Công việc | Đầu ra bắt buộc | Điều kiện chuyển bước |
|---|---|---|---|
| **G0 — Kiểm kê asset** | Đọc GLB và license; phân loại từng mesh/texture thành KIT-ID ứng viên, đo pivot/UV/kích thước và tính tách được | Source audit + **component catalog** + ảnh từng loại cấu kiện + danh sách phải dựng bổ sung | Biết rõ module nào tận dụng/biến thể/thiết kế mới, không coi 10 nhóm nhà là 10 shop game |
| **G1 — Chốt bố cục SORA mới** | Thiết kế bản đồ phố chính, giao lộ, ngõ phụ và SORA MART theo gameplay; phân bố các F-ID và mapping KIT-ID | Mặt bằng thành phố **độc lập scene AFX** + danh sách shop và mapping module + góc camera | Chủ dự án xem được map hoàn toàn mới có logic đô thị, tỷ lệ camera/chơi được và danh sách asset thiếu |
| **G2 — Nền làm việc** | Tách/hiệu chỉnh các module hợp lệ trong Blender; tạo staging Editor/URP prefab/material, colliders/camera và adapter gameplay thử sớm | Scene ngoại thất riêng + một bộ module tái cấu trúc và bổ sung do SORA tạo + baseline tích hợp | Scene mở lại được, có thể ghép không theo thứ tự source, kiểm tra tỷ lệ nhân vật và tránh spawn world trùng |
| **G3 — Đoạn mẫu đạt chuẩn** | Hoàn thiện 1 cửa hàng đại diện rồi 1 căn kề và phần đường, ánh sáng/mưa tối thiểu | Một đoạn liền mạch xem gần và đi qua được; ảnh debug sáng rõ + mưa đêm | Ngoại thất đạt bảng QA bên dưới; khóa chuẩn vật liệu/chi tiết trước khi nhân rộng |
| **G4 — Ngoại thất SORA MART** | Làm cửa kính, biển, mái hiên, chân tường, trưng bày nhìn qua kính và mặt bên có thể thấy | `F-SORA-001` đạt cùng chuẩn, anchor cửa/POS được ghi rõ | Mặt tiền phù hợp phố; chưa làm thêm nội thất hay nghiệp vụ mới |
| **G5 — Hoàn thiện từng đoạn còn lại** | Lặp G3 theo `SEG-*`, mỗi phiên 1 căn hoặc 1 phần rõ ràng của căn | Hồ sơ từng căn và bảng tiến độ có bằng chứng | Mọi mặt tiền trong từng đoạn đều pass; không dùng vài căn đẹp đại diện cho tất cả |
| **G6 — Ghép đường và môi trường toàn phố** | Nối vật liệu đường, điện/dây, rãnh, props, ánh sáng, reflection và vùng mưa | Toàn tuyến đi bộ liền mạch, đầu/cuối hợp lý | Không lộ seam, khoảng rỗng, bóng cắt, mưa xuyên vùng che hoặc lặp đèn gây chói |
| **G7 — Tích hợp gameplay** | Tách/đổi visual bootstrap có fallback, gắn cửa hàng hiện tại, giữ state/nhân vật | Scene chơi sử dụng ngoại thất mới, không spawn hai world | Movement/camera/switch/POS/kho/dialogue/save-load đạt regression thực tế |
| **G8 — Tối ưu và nghiệm thu Windows** | LOD/culling, light/VFX budget, build và đo ở cấu hình ghi rõ | Windows build, ảnh/video, bảng frame time và coverage mọi căn | Tất cả mặt tiền đạt QA; baseline kỹ thuật đạt; rủi ro/giới hạn công bố rõ |

**Không chuyển G0 → dựng hàng loạt khi chưa xem được dữ liệu 3D.** Nếu truy cập bị chặn, ghi trạng thái `BLOCKED` và chốt đầu ra phiên là báo cáo thiếu dữ liệu. Không thay bằng một bản phố tự chế hoặc chọn mẫu khác âm thầm. Những việc độc lập như tổ chức hồ sơ và kiểm tra baseline có thể tiếp tục.

## 5. Lịch theo phiên, không hứa hoàn thành phố trong một lần

Đây là thứ tự và kích thước phiên đề xuất, không phải deadline. Một gói có thể cần nhiều phiên nếu asset hoặc Editor chưa sẵn sàng.

| Phiên | Gói việc | Kết quả có thể xem/kiểm tra |
|---|---|---|
| 01 | `EXT-001`…`EXT-003` — audit nguồn, quyền dùng, phạm vi mô hình | Báo cáo mẫu có thể/không thể dùng và coverage thực |
| 02 | `EXT-004`…`EXT-005` — thiết kế bản đồ SORA mới, ID mặt tiền, chia đoạn, vị trí SORA MART + mapping KIT-ID | Mặt bằng mới theo gameplay, không sao chép bbox/bố cục AFX |
| 03 | `EXT-006`…`EXT-007` — staging và bộ module tối thiểu | Scene đi bộ kiểm tra tỷ lệ, collider/camera |
| 04 | `EXT-008` — hình học/vật liệu căn đại diện | Cận mặt tiền trong ánh sáng kiểm tra rõ |
| 05 | `EXT-009` — căn kề + đường + ánh sáng/mưa thử | Đoạn mẫu hoàn chỉnh ban đêm và video đi qua |
| 06–07 | `EXT-010`…`EXT-011` — ngoại thất SORA MART | Mặt trước, kính/trưng bày, mặt bên và lối vào |
| Các phiên tiếp | `EXT-012` — lặp cho mỗi `F-*` còn lại; `EXT-013` — review mỗi `SEG-*` | Từng căn/đoạn đạt QA theo cùng chuẩn |
| Sau khi đủ căn | `EXT-014`…`EXT-016` — liền mạch toàn phố, ánh sáng, mưa | Đi trọn tuyến, quay ngược và quan sát mọi mặt tiếp cận |
| Tích hợp | `EXT-017`…`EXT-018` | Ngoại thất mới chạy cùng gameplay cũ và kiểm thử regression |
| Kết thúc mốc | `EXT-019`…`EXT-020` | Profiling Windows và hồ sơ nghiệm thu |

Gọi `N` là số mặt tiền **được thiết kế cho bản đồ SORA mới sau G1** (tính cả SORA MART), không phải số cụm A–J trong file tham khảo. Sau đoạn mẫu 2 căn và SORA MART, khối lượng còn lại là các gói `EXT-012/F-*` cho từng căn còn lại, cộng review `EXT-013/SEG-*`. Chỉ ước lượng tổng phiên sau G1 và sau khi đo tốc độ làm đoạn mẫu G3; không lấy số căn suy ra từ một thumbnail.

## 6. Hồ sơ bắt buộc cho từng cửa hàng

Mỗi hồ sơ gồm: ID, nguồn/thời điểm kiểm tra, ảnh front/oblique/side/rear-visible, đoạn phố, kích thước và mức tin cậy, kiến trúc riêng, vật liệu, biển hiệu, props, phần nhìn qua kính, mặt khuất, quyền sử dụng và ảnh Unity tương ứng.

Nếu dùng lại mesh nguồn hợp lệ: ghi phần tái sử dụng, phần sửa và asset/material mapping. Nếu dựng mới: ghi ảnh/hình học nào là tham chiếu, phần nào là thích nghi của SORA. Không tuyên bố bản dựng giống địa điểm thật khi chỉ dựa trên scene nghệ thuật của AFX.

### Chuẩn hoàn thành ngoại thất — áp dụng cho mọi căn

| ID | Kiểm tra | Pass khi |
|---|---|---|
| EX-01 | Hình khối và tỷ lệ | Cửa, khung, mái, chân tường có độ sâu hợp lý; tỷ lệ so với hai nhân vật được kiểm tra |
| EX-02 | Mặt nhìn thấy | Front, side, rear và phần mái camera có thể thấy đều được hoàn thiện; không lộ mặt hở/backface |
| EX-03 | Công năng và nét riêng | Nhận diện loại cửa hàng qua kiến trúc/đồ trưng bày; không chỉ đổi màu cùng một khối |
| EX-04 | Vật liệu gần | UV, mật độ texture, normal, roughness và cạnh không lỗi rõ ở khoảng cách camera chơi |
| EX-05 | Kính và cửa | Nhìn qua kính có chiều sâu; cửa đang đóng/mở có cách thể hiện hợp lý; không bắt buộc có nội thất đi vào |
| EX-06 | Biển hiệu | Chữ không ngược/lỗi font, sáng vừa đủ, nội dung phù hợp; nguồn font/texture có trong sổ asset |
| EX-07 | Props và sinh hoạt | Props gắn đúng vị trí, có chân tiếp xúc, không nổi/xuyên; không chặn cửa/lối đi |
| EX-08 | Ánh sáng | Giữ chi tiết ở cả chế độ debug sáng rõ và mưa đêm; không dùng bóng tối/bloom để giấu mô hình thiếu |
| EX-09 | Mưa/nền ướt | Mép mái có vùng che, bề mặt ướt có biến thiên; reflection không là các dải neon hình học giả |
| EX-10 | Chơi và camera | Đi sát, quay lưng, đổi nhân vật không làm camera xuyên tường hoặc kẹt collider |
| EX-11 | Tính liên tục | Không hở nền, lệch curb, Z-fighting hay đổi texture đột ngột ở ranh giới căn/đoạn |
| EX-12 | Hồ sơ | Có ID, nguồn, ảnh Unity cùng góc đối chiếu, trạng thái và ghi chú vấn đề còn lại |

Pass phải gắn với commit + scene + bằng chứng. Một ảnh xa hoặc ảnh tham chiếu đẹp không chứng minh đủ EX-01…12. Các mục môi trường chung chỉ được pass ở G6; trước đó ghi `PENDING`, không miễn trừ.

## 7. Hướng tích hợp kỹ thuật đề xuất

Các đường dẫn/tên dưới đây là **đề xuất**, chưa tồn tại hoặc chưa được triển khai trong phiên kế hoạch:

- Staging: `Assets/SORA/Scenes/SORA_StreetExterior_Staging.unity`.
- Nội dung author: `Assets/SORA/Environment/StreetExterior/`, chia `Prefabs/Frontages`, `Prefabs/Street`, `Materials`, `Textures`, `Lighting` theo nhu cầu thực.
- Root logic: `StreetExteriorRoot` → `SEG-*` → `F-*` và các nhóm hạ tầng. ID tồn tại độc lập với tên asset nguồn.
- Material dùng chung khi thích hợp; tránh tạo một material/đèn realtime riêng cho mọi chi tiết. Thông số cụ thể đo ở G3 rồi mới khóa.
- Đoạn mới phải xem/chỉnh được trong Editor, không tạo hàng nghìn primitive trong `Update` hay mỗi lần vào Play.
- G7 giới thiệu chế độ visual legacy/new hoặc adapter tương đương: chỉ chọn một bộ ngoại thất. Vì bootstrap hiện tại trộn hình ảnh và gameplay, cần tách có kiểm soát các vị trí/anchor và đăng ký tương tác; không gọi lại toàn bộ `Build()` khi load/switch.
- Bảo toàn root/transform nhân vật, active character, quỹ, tồn kho, draft POS, quan hệ, save schema và `.meta`/GUID. Không tạo POS thứ hai để gắn mặt tiền mới.
- Mưa follower hiện tại chưa chứng minh có che mái: G6 kiểm tra riêng vùng khô dưới mái/đi vào cửa hàng và chuyển vùng khi Tab. Không chỉ đổi smoothness để tuyên bố có phản chiếu hoàn chỉnh.
- Giữ prototype cũ làm fallback trong nhánh cho đến khi bản mới đạt regression; không xóa generator trong PR mỹ thuật đầu tiên.
- Khi refactor visual bootstrap, cập nhật các guard của `Tools/validate_project.py` cùng PR theo hành vi mới. Hiện validator cố định một số marker v0.5; không giữ world cũ spawn trùng chỉ để test pass.
- Unity sinh scene/prefab/material YAML và metadata; không viết tay các file Unity để tạo cảm giác đã có scene được kiểm tra.

## 8. Bằng chứng và hiệu năng

Mỗi căn: front, nhìn chéo, mặt bên/khác nhìn thấy, cận vật liệu; mỗi đoạn: hai chiều đi bộ. Toàn phố: overview bố cục và video đi hết tuyến, kể cả đầu/cuối. Dùng cùng preset camera và lưu transform/FOV.

Ảnh phải ghi commit, scene, Editor/build, cấu hình đồ họa và vị trí camera. Phân biệt ảnh nguồn, Blender render, Unity Editor, Unity Play Mode và Windows build. Không dùng ảnh tạo sinh hay ảnh của AFX làm screenshot game SORA.

G2/G3 ghi cấu hình máy, độ phân giải và preset để khóa ngân sách theo máy mục tiêu. **60 FPS ở 1080p là mục tiêu đề xuất để đo**, chưa phải cam kết hoặc kết quả đạt. Báo cáo frame time CPU/GPU, thời gian khung hình phân vị 99% khi đo được, draw calls/batches, triangles và VRAM; dùng cùng tuyến, thời lượng, điều kiện mưa. Không lấy số FPS từ asset listing hay báo cáo D1 làm benchmark phố mới.

Ở G8: chạy Windows build trên máy mục tiêu đã ghi, lưu log và bảng đo. Nếu cần tối ưu, giảm chi phí render bằng LOD/culling/material/light/VFX hợp lý nhưng vẫn giữ chuẩn mọi mặt tiền trong phạm vi tiếp cận.

## 9. Trạng thái, PR và bàn giao qua phiên

Backlog thực thi: [STREET_EXTERIOR_WORK_PACKAGES.csv](STREET_EXTERIOR_WORK_PACKAGES.csv).

Biểu mẫu từng căn: [STREET_FRONTAGE_AUDIT_TEMPLATE.csv](STREET_FRONTAGE_AUDIT_TEMPLATE.csv).

Quy trình/prompt phiên kế: [STREET_EXTERIOR_SESSION_GUIDE.md](STREET_EXTERIOR_SESSION_GUIDE.md).

Trạng thái dùng: `PLANNED` → `READY` → `IN_PROGRESS` → `REVIEW` → `DONE`; `BLOCKED` phải ghi blocker. `DONE` chỉ khi có bằng chứng theo gói; đánh giá mỹ thuật của chủ dự án được ghi riêng với kết quả kiểm tra kỹ thuật. Trong lần lập kế hoạch này mọi gói xây dựng đều `PLANNED`.

Mỗi phiên bắt đầu bằng fetch refs/open PRs và đọc bàn giao trước đó. Chọn một gói đã đủ dependency; tạo nhánh feature và PR nhỏ. Kết phiên ghi: căn/đoạn đã đụng tới, file/commit/PR, ảnh/video, test thực chạy, test pending, vấn đề, quyết định và gói kế tiếp. Gói lặp phải có một hàng thực riêng cho mỗi `F-*`/`SEG-*`, không đánh dấu hàng tổng là xong sau một căn.

Không mặc định merge vào `main`. Nhánh phiên kế dựa trên base đã review mới nhất; nếu `develop` vẫn chậm hơn `main`, ghi rõ lý do dùng `main` hoặc nhánh kế hoạch đã duyệt, không đồng bộ hai nhánh bằng force-push. Không chạy CI cần license ngoài cấu hình được chủ dự án cung cấp.

## 10. Điều kiện hoàn thành mốc con phố

- [ ] G0 xác minh nguồn, scene, license và cách dùng; inventory có thật thay hàng mẫu.
- [ ] Mặt bằng bám mẫu được review, mọi thích nghi SORA được đánh dấu.
- [ ] Mọi `F-*` trong vùng chơi đều đạt EX-01…12, gồm `F-SORA-001`.
- [ ] Mọi `SEG-*` nối thành tuyến hoàn chỉnh; không lộ vùng chưa sản xuất khi chơi bình thường.
- [ ] Mưa đêm, nền ướt, vùng che, ánh sáng và camera được kiểm tra toàn tuyến.
- [ ] Gameplay cũ không hồi quy, không spawn trùng world/POS và không làm mất state.
- [ ] Windows build được đo ở cấu hình ghi rõ; có ảnh/video và danh sách giới hạn còn lại.
- [ ] Chủ dự án review chất lượng toàn phố; không suy từ một căn đẹp sang toàn khu đã đạt.

**Ưu tiên hiện tại, theo yêu cầu 10/11/2026:** Hoàn thiện và review SORA MART trước (`EXT-010`/`EXT-011`), gồm ngoại thất và nội thất hiện có. Thứ tự này do chủ dự án chọn thay cho việc chờ hoàn tất đoạn mẫu toàn phố; các chuẩn QA và bảo toàn gameplay vẫn áp dụng. Tình trạng nghiệm thu thực tế ghi trong báo cáo SORA MART; các gate còn lại chưa tự động được coi là hoàn thành.
