# SORA MART - ảnh konbini thật và bảng đối chiếu model

Ngày đối chiếu: **11/11/2026**. Yêu cầu của chủ dự án: lấy ảnh cửa hàng tiện lợi
Nhật làm mẫu, tìm model theo vật thể trong ảnh và ghi rõ món đã tải.

## Ảnh gốc

| Mốc | Nguồn, tác giả và giấy phép | Phần dùng để đối chiếu |
|---|---|---|
| R1 - ảnh chính | [7-Eleven ở trung tâm Kyoto, 2014](https://commons.wikimedia.org/wiki/File:Convenience_store_interior.jpg), Japanexperterna, [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) | Kệ giữa thấp, chiều sâu xếp hàng, các dãy hàng đóng gói, tủ thực phẩm áp tường và đèn trần |
| R2 - thực phẩm lạnh | [Cvs fresh, 2016](https://commons.wikimedia.org/wiki/File:Cvs_fresh.jpg), Ocr7041r38, [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/) | Sandwich/onigiri có màng và nhãn, cơm/mì có khay trong, chia ô và ray giá |
| R3 - đồ nóng | [Fast foods of Circle K Sunkus, 2009](https://commons.wikimedia.org/wiki/File:Fast_foods_of_Circle_K_Sunkus.jpg), Corpse Reviver, [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) | Tủ đồ nóng có kính, tầng, khay và món bên trong; đặt trong khu phục vụ |
| R4 - đồ uống | [Konbini drink corner, 2009](https://commons.wikimedia.org/wiki/File:Konbini_drink_corner_2009_(3819547946).jpg), Yuya Tamai, [CC BY 2.0](https://creativecommons.org/licenses/by/2.0/) | Dãy đồ uống theo loại, lon/chai/hộp có nhãn, nhiều hàng sâu |
| R5 - onigiri | [Onigiri](https://commons.wikimedia.org/wiki/File:Onigiri_(6959188603).jpg), Brice Argenson, [CC BY-SA 2.0](https://creativecommons.org/licenses/by-sa/2.0/) | Bao bì, tem và nếp gói; model cơm tam giác trần chưa đạt yêu cầu |
| R6 - quan hệ các khu | [Japan Guide - Convenience Stores](https://www.japan-guide.com/e/e2071.html), sơ đồ mẫu của trang | Thực phẩm lạnh cuối cửa hàng; hàng khô giữa; báo/tạp chí gần mặt tiền; quầy, đồ nóng và cà phê cùng phía |

Đây là ảnh lịch sử và sơ đồ hướng dẫn, không chứng minh cách bố trí mọi konbini
hiện nay. R1 chỉ cho thấy một phần cửa hàng; không suy ra kích thước hay các góc
khuất từ một ảnh. R6 là sơ đồ mẫu, không phải mặt bằng đo đạc của cửa hàng R1.

Năm ảnh Commons và metadata gốc giữ trong
`LocalReferenceAssets/SoraMartIncoming/References/`; file `manifest.json` ghi nguồn,
tác giả, giấy phép và SHA-256. Ảnh/sơ đồ Japan Guide chỉ giữ local để đối chiếu;
không đưa chúng vào game hoặc kho Git công khai. Các ảnh không dùng làm texture
bao bì, poster hay nhận diện thương hiệu của SORA MART.

## Model đối ứng và kết quả kiểm tra

| Mốc ảnh | ID cần tìm | File/bộ đã tải | Phần còn thiếu |
|---|---|---|---|
| R1 - kệ giữa | SM-C01, C03, C13 | Bộ Retail Store Fixtures and Mall và Supermarket Operations | Chọn bộ đồng nhất, kiểm tra ray giá và khoảng đi; chỉ bộ đầu tiên đã có nguồn được đặt trong Unity |
| R1/R2 - kệ thực phẩm lạnh | SM-C05, C12 | Open Chiller, Grab and go chiller, khay thực phẩm | Chưa kiểm tra đủ cấu tạo, chiếu sáng và phong cách; chưa nhập Unity |
| R2/R5 - onigiri/bento/sandwich | SM-D01, D02, D03 | Onigiri của Quaternius; rice-ball, styrofoam-dinner, sandwich/sub của Kenney nếu có trong gói | Các nguồn chỉ đáp ứng phần món ăn; màng bọc, khay/nắp đúng kiểu, nhãn Nhật và đủ biến thể còn thiếu |
| R1 - snack đóng túi | SM-D20, D21, D22 | Bag of Crisps, Pack of Biscuits, Chocolate Bar và gói Kenney | Có hình dáng khác nhau nhưng nhiều bản đang mở hoặc chỉ có dải màu; chưa đủ nhãn/bao bì bán lẻ |
| R4 - đồ uống | SM-D11 đến D17 | Chai nước, lon soda, hộp sữa/nước ép từ hai bộ hàng hóa | Chưa có nguồn riêng cho trà chai, cà phê lon và nước thể thao; không đổi tên lon soda để đánh dấu đủ |
| R3 - khu đồ nóng | SM-F03, F11, F12 | Hot food cabinet, holding cabinet, túi/khay và món Kenney | Kính, ruột tủ, khay và tỷ lệ cần xem tiếp; chưa đủ bộ trưng bày chốt mỹ thuật |
| R6 - quầy | SM-B04, B05, B07, B10, B11 | Handheld Scanner, card terminal, receipt printer, bag stand, counter riser | Scanner chưa có dock/màn hình kiểm kho; bố trí dây, phía khách/nhân viên và tương tác chưa kiểm tra |
| R6 - bánh, báo và giỏ | SM-C07, C09, C10, E24 | Bakery Wall Rack, Magazine Rack, Hand Basket/Basket Stack và Magazine | Giỏ xem thử thành đặc, bìa tạp chí chưa thành bộ tiếng Nhật, bao bì bánh chưa đủ |
| R6 - hàng thiết yếu | SM-E01, E04, E05, E07, E11, E12 | Tissue Box, Soap Bar, Shampoo Bottle Variant A, Toothpaste Tube, Detergent/Spray Bottle | Thiếu hộp/vỉ, nhãn hoặc biến thể; Spray Bottle chỉ là ứng viên hình dáng, chưa xác nhận nước rửa chén |

Chi tiết từng file, hash, đường dẫn local, trạng thái xem mẫu và nguồn đang dùng
trong Unity: [CSV](SORA_MART_DOWNLOADS.csv) và [JSON](SORA_MART_DOWNLOADS.json).
Các model của 3D Assets có trang khai báo nội dung do AI tạo; giữ nguyên trang
nguồn và trường khai báo khi có. Nhãn CC0 không chứng minh độ hoàn thiện của mesh.

## Bố trí dự kiến dựa trên tham chiếu

Đây là **đề xuất sửa bố cục**, chưa phải ảnh hay kết quả bố trí mới trong Unity.
Giữ diện tích và các mốc gameplay của SORA MART, dùng R6 để xác định quan hệ khu,
R1-R5 để chọn model và cách xếp hàng. Mốc POS của game ở bên trái, vì vậy bố trí
quầy/đồ uống được phản chiếu theo chiều ngang so với R6. Không mô tả đây là bản
sao 1:1 của 7-Eleven trong R1.

```text
                      TƯỜNG CUỐI SHOP
  Kho nhỏ       Thực phẩm lạnh: onigiri / bento / sandwich
  Cà phê        Kệ bánh và snack          Tủ đồ uống
  Phục vụ       Kệ mì / hàng khô          Tủ đồ uống
  Quầy POS      Kệ đồ dùng thiết yếu      Tủ đồ uống
  Đồ nóng       Báo / giỏ gần mặt tiền
                        CỬA VÀO
```

Thứ tự ghép tiếp: chọn một bộ kệ/tủ đạt chất lượng; thử quầy + kệ + tủ lạnh và
10-15 sản phẩm theo ảnh; kiểm tra hai nhân vật đi lại; chụp ảnh Unity cùng góc
đối chiếu; sau đó mới nhân rộng. Tọa độ và số module chốt sau khi đo model thật,
giữ lối chính khoảng 1.2 m và các mốc POS/restock/delivery hoạt động.

Không nhập hàng loạt cả 403 ứng viên để tăng số loại trên giấy. Nhiều mẫu xem
thử chưa đạt yêu cầu nhìn gần; đợt này hoàn tất tải và kiểm kê, chưa thay model
hay bố cục đang chạy. 19 nguồn tích hợp trước đó giữ trạng thái riêng.

## Mục còn thiếu cần tìm tiếp

**30 mục chưa có ứng viên đã tải hoặc nguồn phù hợp đã kiểm kê**:

- A12; B06, B09: chi tiết mái/mặt bên, màn hình giá khách, khay tiền.
- D12, D14, D15, D19, D24, D31: trà chai, cà phê lon, nước thể thao, mì gói,
  hạt/khô và đồ đông lạnh đóng gói. D12/D14 là P0.
- E02, E06, E08, E09, E10, E13, E15, E16, E17, E19, E20, E22: khăn ướt,
  sữa tắm, mỹ phẩm, vệ sinh cá nhân, dao cạo, mút/găng, cáp/sạc, văn phòng phẩm,
  khẩu trang/sơ cứu, tất, khăn nhỏ, màng bọc/giấy bạc/túi rác bán lẻ.
- F04, F05; G06: tủ hấp bánh bao, quầy oden, pallet nhỏ. Đều P2 tùy chọn.
- H03, H08, H10, H11; I09; J05: đèn tường, ổ cắm/công tắc, báo khói,
  gương lồi, rãnh thoát nước và tủ hàng giới hạn sau quầy.

5 mục I01/I06/I08/H07/J06 đã có nguồn CC0 IzakayaAlley từ trước, không tải lại
hay cộng lại số GLB. 116 mục còn lại có ứng viên mới/cũ trong kho intake; cả hai
nhóm vẫn phải kiểm tra chi tiết và số biến thể theo danh sách 151 mục.
