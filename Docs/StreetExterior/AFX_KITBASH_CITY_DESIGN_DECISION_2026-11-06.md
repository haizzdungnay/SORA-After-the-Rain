# SORA — Quyết định sản xuất: AFX là thư viện cấu kiện (Kitbash), KHÔNG phải bố cục thành phố

**Ngày chốt:** 2026-11-06. **Chủ dự án xác nhận trực tiếp:** sử dụng các thành phần 3D có trong `japanese_street_at_night.glb` để xây dựng **một thành phố/khu phố SORA mới**, không copy nguyên con hẻm, không giữ tỷ lệ hoặc trật tự A–J của scene gốc.

## 1. Quy tắc bắt buộc

**Source scene ≠ target world.** Bố cục 9,52 × 16,04 đơn vị và 10 cụm nhà A–J chỉ là metadata **của nguồn asset**, không phải yêu cầu kích thước hoặc topology của bản đồ SORA.

Mỗi thành phần phù hợp được **trích xuất, làm sạch, chuẩn hóa, điều chỉnh hoặc xây thêm** để tạo library reusable. Phố mới do gameplay + thiết kế đô thị quyết định: tuyến phố chính có SORA MART, nhánh rẽ, hẻm phụ, cửa hàng và hạ tầng; không kế thừa thứ tự/đường nét tuyến hẻm AFX.

Hai căn có thể dùng lại module giống nhau nhưng **không được trở thành bản sao lặp**: thay đổi tổ hợp module, hình khối, loại cửa hàng, vật liệu, biển hiệu, cách bố trí và hình khối trên tầng. Mọi mặt nhìn được (trước/bên/sau/mái) phải hoàn chỉnh theo EX-01…12.

## 2. Từ scene AFX thành thư viện asset

| Lớp | Ứng viên cần audit từ mesh thật | Đầu ra thư viện đề xuất |
|---|---|---|
| Kiến trúc khối lớn | 10 building groups A–J | `KIT-BLD-*`: tham chiếu mặt tiền/tầng/mái, đánh dấu module tách được và module cần dựng mới |
| Vỏ nhà | vách, mái, khung, cửa, cửa sổ, cầu thang/ban công **nếu có geometry riêng** | `KIT-FAC-*`, `KIT-ROOF-*`, `KIT-DOOR-*`, `KIT-WIN-*`; không tự nhận đã có mesh rời |
| Cửa hàng | biển, hộp đèn, mái che, kính, khung trưng bày **khi có trong nguồn** | `KIT-SHOP-*`, `KIT-SIGN-*`; nhãn/text gốc có thể cần thiết kế lại |
| Props | máy bán nước, trụ, dây điện, đèn, biển nhỏ, đồ ven đường | `KIT-PROP-*`, `KIT-UTIL-*`; kiểm tra tách/mesh pivot trước khi tái sử dụng |
| Nền và đô thị | asphalt, vỉa hè, curb, rãnh/đường ống **nếu có trong nguồn** | `KIT-ROAD-*`, `KIT-SIDEWALK-*`; ưu tiên tạo module mới tương thích giao lộ |
| Vật liệu | 34 materials, 107 PNG nhúng trong GLB | Mapping nguồn → URP material shared, atlas/UV review, bảng provenance từng module |

Các nhãn trên là **nhóm ứng viên**, không phải kiểm kê chứng thực rằng file AFX đã có đủ một bộ modular kit sạch. Một số mesh có thể là một khối ghép nhiều chi tiết/texture atlas. Cần dùng Blender để kiểm tra object origin, local transforms, UV islands, material slots, tách mặt/hình học, và bù mặt sau trước khi export. Không chỉ cắt các node tên A–J ra và gọi đó là modular assets.

### Catalog tối thiểu trước khi dựng bản đồ

Mỗi `KIT-ID` phải có: `source_node_path`, `category`, `separable` (YES/NO/NEEDS_REMODEL), `geometry_extent`, `pivot`, `materials`, `uv_issues`, `visible_sides`, `target_variants`, `provenance`, `preview`, `review_status`. Ghi đủ mesh nào lấy từ AFX, mesh nào do nhóm tự dựng, mesh nào buộc phải thay thế. Scene A–J là metadata để truy ngược asset chứ **không phải danh sách cửa hàng SORA**.

## 3. Pipeline kỹ thuật được ưu tiên

```text
Original AFX GLB (local/private only)
          |
          v
Blender asset audit: nodes, local scale, UV, material, side coverage
          |
          v
Classify > isolate mesh / remodel where needed > correct pivots & scale
          |
          v
Reusable kit + original SORA-made variants (controlled library)
          |
          v
Unity URP materials, modular prefabs, LOD/collider conventions
          |
          v
SORA city layout: main street + SORA MART + side streets/alleys
          |
          v
Two-storefront quality benchmark > expand each block > full street QA
```

- Một **thành phố** không có nghĩa phải dựng cả thành phố trong một phiên. G1 thiết kế bố cục **độc lập** từ dữ liệu tham chiếu và nhu cầu gameplay; chốt phần playable trước. Bắt đầu bằng một dãy liên tục có chất lượng đạt chuẩn, sau đó nhân rộng.
- Unity chỉ import **asset được phép** vào môi trường phù hợp; giữ bản raw AFX, texture và asset sửa từ AFX ở **local private workspace** khi chưa xác minh quyền phân phối mã nguồn. Repo public có thể chứa code tool, catalog metadata, hướng dẫn và module do nhóm tự sáng tạo đủ quyền phân phối.
- Tích hợp sớm: prefab SORA MART, collider/interaction anchors, two-character camera 3,6m, lưu tọa độ nhân vật, không spawn trùng world; chưa đợi đến G7 mới đánh giá tính tương thích.

## 4. Tiêu chí nghiệm thu mới

G0 chỉ đạt khi **đã có asset-level candidate catalog** (không chỉ ten building-group counts), có ít nhất mỗi loại cấu kiện thực sự được tìm thấy/không tìm thấy, mức tách được và thiếu asset gì phải tự dựng.

G1 chỉ đạt khi có **bản đồ SORA mới** (sơ đồ/top-down và camera đi bộ) do gameplay quyết định, mapping giữa từng cửa hàng SORA với các KIT-ID, và ảnh đối chiếu chất lượng từ AFX. **Không copy scene gốc; không suy kích thước thành phố từ bounding box AFX.**

G2/G3 phải có **một tuyến thử** ghép từ nhiều module được tái cấu trúc, không đặt nguyên scene AFX vào Unity rồi tuyên bố kết quả.

Quyết định đã chốt này **thay thế** tất cả nhận định trong các báo cáo G0 trước đó hàm ý AFX chỉ là một hẻm để giữ nguyên hoặc kéo dài. Những báo cáo đó vẫn có giá trị làm kiểm kê dữ liệu nguồn, không phải thiết kế đích.
