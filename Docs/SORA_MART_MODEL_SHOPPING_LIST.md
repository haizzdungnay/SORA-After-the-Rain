# SORA MART - Danh sách model hoàn chỉnh cần tìm và tải

Ngày kiểm kê: **10/11/2026**. Mốc repo: `7c4191f`, sau phần SORA MART và bộ chỉ mục asset.

Danh mục có **151 hạng mục**: **43 P0 cốt lõi**, **84 P1 hoàn thiện**, **24 P2 tùy chọn**. Đây không phải yêu cầu tải 151 pack; một pack đồng bộ có thể đáp ứng nhiều hạng mục.

## Cập nhật tải model - 11/11/2026

**Dấu `[x] Tải` chỉ xác nhận có file ứng viên thật, không xác nhận đã đủ mẫu/bao bì hoặc đạt mỹ thuật.**

- Đã có **403 GLB khác nhau theo SHA-256**, từ **38 nguồn/gói**: 211 mẫu đợt đầu + 192 mẫu bổ sung. Không tính FBX/OBJ của cùng model hay các bản sao đặt trong shop.
- **116/151 mục có ít nhất một ứng viên tải về**; **5 mục có nguồn CC0 sẵn** và **30 mục chưa có ứng viên phù hợp**; từng dòng dưới đây ghi số file và liên kết bảng kiểm. Số thực tế được lệnh kiểm tra xác nhận, không dùng số pack để đánh dấu đủ danh mục.
- **19 nguồn đã đặt trong Unity ở đợt trước**; đợt tải mới chưa được nhập Unity. **Chưa mục nào được xác nhận hoàn tất toàn bộ tiêu chí mỹ thuật/số biến thể của danh sách.**
- Có ảnh xem thử Blender cho **49 mẫu khác nhau**; ảnh Blender không phải ảnh Unity. Một số ứng viên không đạt: onigiri thiếu màng bọc, pin thiếu vỉ, fries mở không phải snack đóng túi, tủ/máy quá đơn giản.
- [Từng file đã tải, nguồn, giấy phép, SHA-256 và phần còn thiếu](StreetExterior/SORA_MART_DOWNLOADS.csv) · [Dữ liệu kiểm chứng](StreetExterior/SORA_MART_DOWNLOADS.json) · [Ảnh konbini thật và bố cục tham chiếu](StreetExterior/SORA_MART_KONBINI_REFERENCE.md).

Kho local: `LocalReferenceAssets/SoraMartIncoming/`. Tất cả file nguồn tải nguyên bản, trang nguồn và giấy phép giữ tại kho; chưa đưa 192 ứng viên mới vào thư mục `Assets/`.

Kiểm tra lại trên checkout đang có kho local:

```powershell
python Tools/StreetExterior/verify_sora_mart_downloads.py
```

Các mục 1-10 giữ yêu cầu gốc; mục 2 là **kiểm kê lịch sử ngày 10/11**, trước đợt tích hợp 19 nguồn. Trạng thái tải ở từng dòng và bảng CSV mới là mốc 11/11.

## 1. Mục tiêu và cách đọc

**Yêu cầu của chủ dự án: dùng model đã được tác giả dựng hoàn chỉnh từ đầu. Quầy, kệ, thiết bị và hàng hóa phải có hình dáng, chi tiết, vật liệu và mức hoàn thiện phù hợp khi nhìn gần trong game. Không coi các hộp/chai tự dựng đổi màu hiện tại là hàng hóa đã hoàn thành.**

Phạm vi: ngoại thất, nội thất, quầy phục vụ, hàng hóa, góc kho/nhân viên và khu ngay trước cửa của SORA MART hiện có. Bối cảnh là cửa hàng tiện lợi/bách hóa nhỏ ở Tokyo, không phải đại siêu thị. Diện tích blockout khoảng **14 x 11 m**; danh sách dưới đây là yêu cầu tìm asset, chưa phải sơ đồ bố trí cuối cùng.

| Mức | Ý nghĩa | Khi tải |
|---|---|---|
| **P0** | Bộ khung, thiết bị và hàng hóa cốt lõi cần có để thay bản dựng đơn giản hiện tại | Tìm và tải trước |
| **P1** | Hoàn thiện các khu của cửa hàng, đa dạng hàng và bổ sung chi tiết nhìn gần | Tải tiếp để lấp đầy theo cùng phong cách |
| **P2** | Dịch vụ, nhóm hàng hoặc chi tiết tùy chọn | Chỉ lấy khi chọn bố trí/chức năng tương ứng; không bắt buộc đặt tất cả vào shop |

- **Mẫu** là model/biến thể hình ảnh khác nhau cần có; **số lượng đặt** là số bản sao dùng trong cửa hàng. Một chai nước hoàn chỉnh có thể được đặt nhiều lần, không cần tải 20 file chai giống nhau.
- Một pack có thể đáp ứng nhiều ID. Nếu pack quầy đã có POS, máy in và ngăn tiền thì đánh dấu đủ các dòng tương ứng, không mua/tải trùng.
- Với hàng hóa, cùng kiểu bao bì có thể dùng chung mesh và có nhiều nhãn hợp lệ. Cần cả khác biệt về hình dáng lẫn nhãn; không chỉ thay màu một hộp trơn cho mọi sản phẩm.
- Số lượng là **gợi ý để chọn pack**, không phải phải đặt mọi dòng ở mức tối đa. Khu hàng P2 cần chọn lọc để giữ đủ lối đi.
- Từ khóa tiếng Anh trong bảng là từ khóa tìm kiếm, **không phải tên pack đã được xác minh, link tải hay cam kết còn bán**.

## 2. Kiểm kê thực tế: đã có gì, thiếu gì

Đã đối chiếu `Docs/ASSET_INDEX.md`, `Docs/asset_index.json`, thư mục nguồn môi trường, manifest SORA MART và script dựng cửa hàng; không sửa hoặc tạo lại hai file chỉ mục.

| Phần | Tình trạng hiện tại | Kết luận cho việc tìm model |
|---|---|---|
| Vỏ SORA MART, kính, cửa, biển, mái, đèn | Chủ yếu được tạo bằng script Blender từ hình khối và chữ mesh | Chưa coi là model mỹ thuật hoàn chỉnh theo yêu cầu mới; ưu tiên bộ cửa hàng có sẵn đồng bộ |
| Quầy thu ngân, POS, máy quét, máy cà phê | Hình khối tự dựng, chi tiết còn đơn giản | Cần model chuyên biệt hoàn chỉnh thuộc nhóm B và F |
| Bốn cụm kệ, dãy tủ lạnh | Hình học tự dựng; chưa có bộ thiết bị bán lẻ hoàn chỉnh để thay | Cần nhóm C; bộ kệ/tủ phải có cấu tạo thật và sản phẩm đặt rời |
| Hàng hóa trên kệ | Hộp/chai lặp lại, nhãn đơn giản, chưa phân biệt đủ loại hàng | Cần thư viện bao bì/thực phẩm/đồ dùng thuộc D và E |
| Sáu nhóm props đã tái dùng | Máy bán nước, cây chậu, điều hòa ngoài trời, bộ chai, lốc/thùng bia và thùng rác từ nguồn CC0 IzakayaAlley | Có nguồn để xem lại; **chưa tự động đạt yêu cầu nhìn gần mới** |
| Kenney Commercial/Industrial/Roads/Suburban/Modular | 321 mẫu nhà, module kiến trúc và hạ tầng; có thêm Car Kit, cây, đèn, biển đường | Dùng cho bối cảnh/ngoài cửa khi phù hợp; không thay cho bộ kệ, quầy, tủ và thực phẩm của cửa hàng |
| AFX và các bản V03/V05/V06 | Chủ yếu là phố/kiến trúc đã lắp; có giới hạn sử dụng riêng | Không coi một model khu phố là đã có đủ nội thất và hàng hóa rời của SORA MART |
| Pack nội thất bán lẻ chuyên biệt | Chưa tìm thấy tên model kệ, POS, tủ lạnh, thực phẩm hay hàng tạp hóa chuyên biệt trong chỉ mục ZIP hiện tại | Cần bổ sung; kết luận này giới hạn ở thư viện đã kiểm kê, không phải khảo sát mọi kho asset trên máy |

**Nguồn có sẵn cần lọc trước khi tải trùng:**

- `ArtSource/ThirdParty/CC0/IzakayaAlley/izakaya_starter_scene.glb`: các nhóm `095-drinks-vending-machine`, `105-potted-plant-in-can`, `117-air-conditioning-outdoor-unit`, `084-sake-bottle-set`, `087-beer-crate-stack`, `100-alley-waste-bins` đã được dùng trong bản SORA MART hiện tại.
- Các ZIP Kenney trong `3d model enviroment/` và props đường phố trong `Assets/Resources/SoraCity/Props/` là nguồn kiểm tra cho nhóm I/H, không cần tải lại chỉ vì có nhiều bản FBX/GLB/OBJ.
- Đối với các nguồn này: xem cận cảnh và đối chiếu phong cách trước. Nếu vẫn quá đơn giản hoặc thiếu chi tiết thì tìm bản thay chất lượng cao hơn; không đánh dấu xong chỉ vì file đã tồn tại.

**Những model rời còn có sẵn trong GLB IzakayaAlley cần xem trước khi tải trùng:**

Các tên dưới đây được đọc trực tiếp từ node của GLB nguồn, không suy ra từ tên ZIP. Đây là ứng viên để lọc, chưa phải kết luận đã đạt chất lượng/công năng của món cần tìm.

| Nhóm model nguồn có sẵn | ID có thể đáp ứng một phần hoặc toàn bộ | Phần phải kiểm tra trước khi dùng |
|---|---|---|
| `050-ramen-counter-module`, `051-ramen-counter-module`, `052-ramen-counter-module`, `066-prep-bench-with-chopping-board` | SM-F07 | Có thể dùng làm bàn phục vụ; quầy ramen không tự động thay được quầy POS bán lẻ SM-B01 |
| `067-chest-fridge` | SM-C06 | Kiểm tra lòng, nắp và kiểu tủ; tên node chưa chứng minh đây là tủ đông kem có nắp kính |
| `065-noodle-basket-rack`, `068-crockery-rack` | SM-C12 | Lọc khay/giá phù hợp; không coi là đã có kệ gondola hoặc máy bán lẻ |
| `069-ticket-vending-machine` | SM-J06 | Là ứng viên kiosk tùy chọn; kiểm tra mặt trước/sau, nút và chất lượng nhìn gần |
| `088-bottle-crate`, `126-bottle-crate` | SM-G03 | Kiểm tra kích thước, tay cầm, đáy và kiểu xếp chồng so với thùng giao hàng cần dùng |
| `095-drinks-vending-machine`, `096-hot-can-vending-machine` | SM-I01 | Có nhiều ứng viên máy bán nước; chọn mẫu đạt mức hoàn thiện mới rồi mới tìm thêm |
| `110-a-frame-pavement-sign`, `124-a-frame-pavement-sign` | SM-I05 | Kiểm tra chân, bản lề, hai mặt bảng và phần thay poster |
| `098-bicycle-rack` | SM-I08 | Kiểm tra kiểu dáng/kích thước theo khu ngay trước cửa |
| `105-potted-plant-in-can`, `106-potted-plant-in-can` | SM-I06 | Chọn cây/chậu phù hợp chất lượng và phong cách bộ chính |
| `117-air-conditioning-outdoor-unit`, `118-air-conditioning-outdoor-unit` | SM-H07 | Kiểm tra lưới quạt, mặt sau, gá và vật liệu |
| `119-meter-box-and-cable-bundle` | SM-I10 | Kiểm tra hộp, ống/dây, nắp và mức chi tiết nhìn gần |

Những ứng viên này có thể giảm số món cần tải thêm. Chỉ gạch một ID khỏi danh sách cần bổ sung sau khi model có sẵn đáp ứng tiêu chí của ID đó; vẫn ưu tiên một bộ thiết bị bán lẻ đồng bộ cho các phần cốt lõi còn thiếu.

## 3. Ưu tiên tìm theo bộ để đồng bộ và bớt tải lẻ

| Thứ tự | Bộ nên tìm | Đáp ứng chính | Từ khóa gợi ý |
|---|---|---|---|
| 1 | Bộ cửa hàng tiện lợi Nhật hoàn chỉnh, có cả props rời | A, B, C và H | `Japanese convenience store interior modular game ready`, `konbini interior complete props` |
| 2 | Bộ thiết bị bán lẻ: quầy, POS, kệ, tủ lạnh | Các P0 còn thiếu của B/C/F/G | `retail store equipment pack`, `supermarket checkout shelving refrigerator` |
| 3 | Bộ thực phẩm đóng gói kiểu Nhật | D01-D10, D18-D31 | `Japanese convenience store food pack`, `onigiri bento sandwich packaged food 3D` |
| 4 | Bộ chai/lon/hộp đồ uống có nhãn hoàn chỉnh | D11-D17 | `convenience store drinks pack`, `Japanese beverage bottles cans cartons` |
| 5 | Bộ hàng tạp hóa và chăm sóc cá nhân | E | `grocery household products pack`, `toiletries retail packaging 3D` |
| 6 | Bộ góc cà phê/đồ ăn nóng và kho | F/G | `convenience store coffee hot food equipment`, `stockroom retail delivery props` |
| 7 | Phụ kiện mặt tiền và trang trí hoàn thiện | A/H/I còn thiếu | `Japanese shopfront accessories`, `convenience store exterior props` |

Một bộ lớn có thể bao phủ nhiều hàng trên. Ưu tiên cùng tác giả/bộ sản phẩm để màu sắc, độ chi tiết và tỷ lệ khớp nhau. Nên thử trước **một quầy + một kệ + một tủ lạnh + 10-15 sản phẩm** để kiểm tra độ đồng bộ, rồi mới tải toàn bộ danh mục cùng hướng đó.

## 4. Danh sách chi tiết

### A. Vỏ cửa hàng và mặt tiền

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-A01 | [x] Tải 2 ứng viên | P1 | Bộ vỏ shop modular có nội thất thật | 1 bộ | 1 cửa hàng | Có thể ghép kích thước gần 14 x 11 m, tường có chiều dày, góc và mặt sau hoàn thiện; `modular convenience store building interior` |
| SM-A02 | [x] Tải 2 ứng viên | P0 | Bộ mặt tiền kính và khung nhôm | 1 bộ | 1 mặt tiền | Chân kính, nẹp, mullion, góc kết thúc, vật liệu kính riêng; `retail storefront glazing frame` |
| SM-A03 | [x] Tải 1 ứng viên | P0 | Cửa kính trượt tự động hai cánh | 1 bộ | 1 bộ cửa | Đủ cánh, ray, khung, cảm biến; hai cánh là object riêng, có thể đặt mở/đóng; `automatic sliding glass door` |
| SM-A04 | [x] Tải 3 ứng viên | P1 | Tường trong/ngoài, góc, chân tường | 1 bộ module | Theo chu vi shop | Mặt trước/bên/sau, nẹp và chân tiếp nền đồng bộ; có thể nằm sẵn trong A01; `retail interior wall skirting kit` |
| SM-A05 | [x] Tải 2 ứng viên | P1 | Trần cửa hàng và hệ khung trần | 1 bộ | Theo diện tích trần | Có tấm trần, khung, nẹp biên và ô bảo trì; `commercial suspended ceiling kit` |
| SM-A06 | [x] Tải 2 ứng viên | P1 | Sàn gạch thương mại và nẹp ngưỡng | 1-2 bộ bề mặt | 1 sàn + cửa vào | Vật liệu có ron, độ nhám hợp lý và nẹp ngưỡng; sàn phẳng dùng texture tốt là hợp lệ; `retail tile floor threshold` |
| SM-A07 | [x] Tải 1 ứng viên | P0 | Hộp đèn/biển chính mặt tiền | 1 bộ | 1 biển chính | Thân, cạnh, khung gắn, vật liệu phát sáng riêng; đổi được chữ/texture sang SORA MART; `convenience store illuminated fascia sign` |
| SM-A08 | [x] Tải 1 ứng viên | P1 | Biển treo/vẫy ở góc cửa hàng | 1-2 mẫu | 1-2 biển | Có gá, cạnh và mặt sau; không bắt buộc trùng thương hiệu có sẵn; `Japanese shop projecting lightbox sign` |
| SM-A09 | [x] Tải 2 ứng viên | P1 | Mái hiên và thoát nước mái | 1 bộ | 1 dải mái hiên | Có kết cấu đỡ, mép mái, máng và ống xuống; `shop canopy awning gutter` |
| SM-A10 | [x] Tải 2 ứng viên | P1 | Cửa hậu/cửa nhân viên | 1 mẫu | 1 cửa | Đủ khuôn, tay nắm, bản lề; cánh tách riêng nếu cần mở; `commercial back service door` |
| SM-A11 | [x] Tải 1 ứng viên | P1 | Khung poster, bảng quảng cáo gắn kính | 2-3 mẫu | 3-6 khung | Có holder/kẹp và vùng thay hình; texture poster đi kèm hoặc bổ sung ở mục 6; `retail window poster holder` |
| SM-A12 | [ ] Chưa tải | P1 | Nắp/mép mái, khe thông gió, chi tiết mặt bên | 1 bộ | Theo các mặt nhìn thấy | Che hết cạnh hở, có lưới và nắp kỹ thuật; tránh tải thêm cả nhà chỉ để lấy một khe gió; `shop building roof edge vent kit` |

### B. Quầy thu ngân và thanh toán

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-B01 | [x] Tải 5 ứng viên | P0 | Quầy thu ngân bán lẻ hoàn chỉnh | 1 bộ | 1 quầy | Mặt quầy, thân, hộc, ngăn và mặt phía nhân viên; gần 2.5-3 m ngang hoặc ghép module; `convenience store checkout counter` |
| SM-B02 | [x] Tải 2 ứng viên | P0 | Máy POS và chân đế | 1 mẫu | 1 máy | Đủ màn hình, viền, chân, cổng/dây nhìn thấy; texture màn hình riêng; `retail POS terminal` |
| SM-B03 | [x] Tải 4 ứng viên | P0 | Ngăn kéo tiền | 1 mẫu | 1 ngăn | Thân và khay có ô tiền; ngăn kéo tách nếu mở; có thể đi kèm B01/B02; `cash register drawer` |
| SM-B04 | [x] Tải 1 ứng viên | P0 | Máy quét mã vạch | 1-2 kiểu | 1-2 máy | Thân, cửa quét, đế hoặc tay cầm và dây; `barcode scanner retail` |
| SM-B05 | [x] Tải 2 ứng viên | P1 | Máy thanh toán thẻ/IC/contactless | 1 mẫu | 1 máy | Màn hình, phím, khe thẻ, vị trí chạm; `payment card reader terminal` |
| SM-B06 | [ ] Chưa tải | P1 | Màn hình giá hướng về khách | 1 mẫu | 1 màn hình | Chân/gá và vùng hiển thị riêng; có thể đã nằm trong bộ POS; `customer display POS` |
| SM-B07 | [x] Tải 2 ứng viên | P1 | Máy in hóa đơn và cuộn giấy | 1 bộ | 1 máy + giấy dự trữ | Khe giấy, nút, thân và giấy có chiều dày; `thermal receipt printer` |
| SM-B08 | [x] Tải 1 ứng viên | P1 | Khu/khay đặt hàng và đóng túi | 1 bộ | 1 vị trí cạnh POS | Có mặt đỡ, mép, giá túi hoặc khoang theo bộ quầy; `checkout bagging station` |
| SM-B09 | [ ] Chưa tải | P1 | Khay tiền lẻ/khay trả đồ nhỏ | 1-2 mẫu | 1-2 khay | Mép và đáy có chiều dày, vật liệu riêng; `Japanese shop coin tray` |
| SM-B10 | [x] Tải 6 ứng viên | P1 | Túi mua hàng và bộ cấp túi | 3 kiểu túi + giá | 1 giá + 5-10 túi | Có túi gấp, túi mở và quai; nhãn thay được; `shopping paper plastic bag dispenser pack` |
| SM-B11 | [x] Tải 2 ứng viên | P0 | Kệ hàng nhỏ trên quầy | 1-2 mẫu | 1-2 kệ | Kệ kẹo/gum/pin có tầng và mép giá, hàng đặt rời; `checkout impulse display rack` |
| SM-B12 | [x] Tải 1 ứng viên | P2 | Cọc/dây hoặc bảng chỉ dẫn xếp hàng | 1 bộ | 0-1 khu | Chọn bộ gọn, không làm nghẽn cửa; `retail queue barrier sign stand` |

### C. Kệ, tủ lạnh và thiết bị trưng bày

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-C01 | [x] Tải 2 ứng viên | P0 | Kệ gondola hai mặt giữa cửa hàng | 1-2 mẫu/module | 4 module ban đầu | Chân, lưng, tầng, tay đỡ, nẹp giá; có bản trống và hàng rời; `retail gondola shelving double sided` |
| SM-C02 | [x] Tải 2 ứng viên | P1 | Kệ đơn áp tường | 1-2 module | 2-4 module | Cùng phong cách C01, có đầu kết thúc; `supermarket wall shelving` |
| SM-C03 | [x] Tải 2 ứng viên | P0 | Kệ đầu dãy/endcap | 1-2 mẫu | 2-4 đầu dãy | Đủ mặt bên, tầng, chân và khu nhãn; có thể thuộc bộ C01; `gondola endcap display` |
| SM-C04 | [x] Tải 4 ứng viên | P0 | Tủ mát đứng cửa kính | 1 bộ ghép | Khoảng 4 module hoặc 6-8 cánh theo kích thước | Đủ lòng tủ, tầng, cửa, gioăng, tay nắm, chân và đèn; cửa kính không che đặc hàng; `commercial glass door refrigerator drinks` |
| SM-C05 | [x] Tải 4 ứng viên | P0 | Tủ mát trưng bày cơm hộp/sandwich | 1-2 mẫu | 1-2 tủ | Lòng tủ, tầng, nhãn giá và khe gió hoàn chỉnh; `convenience store chilled food multideck display` |
| SM-C06 | [x] Tải 1 ứng viên | P1 | Tủ đông kem dạng nằm | 1 mẫu | 1 tủ | Thành/lòng tủ, nắp kính, ray, giỏ và tay nắm; `ice cream chest freezer glass top` |
| SM-C07 | [x] Tải 1 ứng viên | P0 | Kệ bánh mì/bánh đóng gói | 1 mẫu | 1 kệ | Khay, tầng, viền và biển; hàng hóa là object rời; `retail bakery bread rack` |
| SM-C08 | [x] Tải 2 ứng viên | P1 | Kệ móc/pegboard treo hàng | 1 bộ | 1-2 module | Tấm lỗ, móc và chân/khung đầy đủ; `retail pegboard display hooks` |
| SM-C09 | [x] Tải 1 ứng viên | P1 | Giá báo/tạp chí | 1 mẫu | 1 giá | Tầng nghiêng, thanh giữ, mặt bên và chân; `convenience store magazine rack` |
| SM-C10 | [x] Tải 2 ứng viên | P0 | Giỏ mua hàng và giá xếp giỏ | 1-2 giỏ + giá | 8-12 giỏ + 1 giá | Thành có lỗ/lưới, đáy, quai và trạng thái xếp chồng; `shopping basket stack holder` |
| SM-C11 | [x] Tải 1 ứng viên | P1 | Thùng/kệ khuyến mãi nhỏ | 1 mẫu | 1 vị trí | Có lòng, mép và mặt gắn giá; không thay bằng hộp không chi tiết; `retail promotional dump bin` |
| SM-C12 | [x] Tải 4 ứng viên | P0 | Khay, giỏ nhỏ và vách chia hàng | 4-6 kiểu | 12-24 bộ | Dùng đặt onigiri/bánh/hàng nhỏ, đáy/mép thật; `retail food display trays shelf dividers` |
| SM-C13 | [x] Tải 1 ứng viên | P1 | Ray giá, kẹp nhãn và holder bảng giá | 1 bộ | Theo các tầng kệ/tủ | Vật thể giữ nhãn hoàn chỉnh; có thể đi sẵn cùng C01/C04; `shelf price label holder rail` |

### D. Thực phẩm và đồ uống - cần model có bao bì thật

Mỗi mẫu bên dưới cần bao bì/nhãn hoàn chỉnh, tỷ lệ đúng và hình dáng nhận ra được. Đồ ăn trong hộp trong phải có phần thức ăn nhìn thấy; một tấm ảnh món ăn trên hộp đặc không đủ khi camera có thể nhìn xuyên nắp.

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-D01 | [x] Tải 2 ứng viên | P0 | Onigiri đóng gói | 3-6 loại | 12-24 gói | Cơm/rong biển, lớp bọc, mép gói và nhãn; `Japanese packaged onigiri 3D` |
| SM-D02 | [x] Tải 1 ứng viên | P0 | Bento/cơm hộp | 4-6 loại | 12-18 hộp | Khay, nắp, cơm và món ăn khác nhau, tem; `Japanese convenience store bento food pack` |
| SM-D03 | [x] Tải 2 ứng viên | P0 | Sandwich đóng gói | 3-4 loại | 8-12 gói | Bánh/nhân có thể thấy, màng và nhãn; `packaged triangle sandwich` |
| SM-D04 | [x] Tải 5 ứng viên | P1 | Sushi/cuộn cơm đóng khay | 2-3 loại | 6-8 khay | Món ăn, khay và nắp riêng; `packaged sushi tray` |
| SM-D05 | [x] Tải 2 ứng viên | P1 | Salad/hộp rau trộn | 2-3 loại | 6-8 hộp | Có thành phần trong hộp và nắp, không chỉ ảnh món; `packaged salad bowl` |
| SM-D06 | [x] Tải 2 ứng viên | P1 | Mì lạnh/udon/soba đóng hộp | 2-3 loại | 6-8 hộp | Sợi mì, topping, khay/nắp và tem; `Japanese chilled noodles food tray` |
| SM-D07 | [x] Tải 10 ứng viên | P0 | Bánh mì/bánh ngọt trong túi | 4-6 loại | 18-30 gói | Hình bánh khác nhau, túi có nếp/khóa và nhãn; `Japanese packaged bakery bread` |
| SM-D08 | [x] Tải 4 ứng viên | P1 | Bánh kem/bánh tráng miệng đóng hộp | 2-3 loại | 8-12 hộp | Lớp bánh, đế, hộp và nhãn; `packaged dessert cake` |
| SM-D09 | [x] Tải 3 ứng viên | P0 | Sữa chua/pudding/thạch cốc | 3-4 loại | 10-16 cốc | Cốc, nắp niêm phong, vành và nhãn; `yogurt pudding cup packaging` |
| SM-D10 | [x] Tải 7 ứng viên | P1 | Kem que/kem ốc quế/cốc kem | 3-5 loại | 12-20 gói | Có bản bán lẻ đã bọc/hộp, không chỉ kem ăn dở; `wrapped ice cream retail pack` |
| SM-D11 | [x] Tải 3 ứng viên | P0 | Nước suối chai PET | 2-3 mẫu | 18-24 chai | Chai, nắp, rãnh/thành và nhãn hoàn chỉnh; `water bottle retail label` |
| SM-D12 | [ ] Chưa tải | P0 | Trà xanh/trà đóng chai | 3-4 loại | 18-24 chai | Khác nhãn/dáng chai hợp lý, vật liệu PET; `Japanese tea bottle beverage pack` |
| SM-D13 | [x] Tải 5 ứng viên | P0 | Nước ngọt chai/lon | 4-6 loại | 20-30 chai/lon | Đủ miệng/nắp lon hoặc chai; texture nhãn có sẵn; `soda bottle can drinks pack` |
| SM-D14 | [ ] Chưa tải | P0 | Cà phê lon/chai | 3-4 loại | 16-24 đơn vị | Lon nhỏ, chai và nhãn đúng tỷ lệ; `Japanese canned coffee bottle` |
| SM-D15 | [ ] Chưa tải | P1 | Nước tăng lực/thể thao | 2-3 loại | 12-18 đơn vị | Lon/chai và nắp có chi tiết; `energy sports drink 3D pack` |
| SM-D16 | [x] Tải 4 ứng viên | P0 | Nước ép trái cây | 3-4 loại | 12-18 đơn vị | Chai hoặc hộp giấy có vòi/nắp phù hợp; `juice carton bottle grocery` |
| SM-D17 | [x] Tải 3 ứng viên | P0 | Sữa/sữa có vị trong hộp giấy | 3-4 loại | 10-16 hộp | Nếp gấp đỉnh, đáy, ống hút/nắp và nhãn; `milk carton grocery packaging` |
| SM-D18 | [x] Tải 1 ứng viên | P0 | Mì ly/mì cốc | 5-8 loại | 24-36 ly | Cốc có côn, vành/nắp và nhãn đủ mặt; `instant cup noodles Japanese pack` |
| SM-D19 | [ ] Chưa tải | P1 | Mì gói | 3-4 loại | 12-18 gói | Túi phồng, mép hàn và hình in; `instant noodle packet` |
| SM-D20 | [x] Tải 1 ứng viên | P0 | Snack/khoai tây chiên dạng túi | 5-8 loại | 24-40 túi | Túi có thể tích, nếp gấp, mép hàn và nhãn; `chips snack bag food pack` |
| SM-D21 | [x] Tải 1 ứng viên | P0 | Bánh quy/cracker | 4-6 loại | 18-30 gói/hộp | Có cả hộp và gói hoàn chỉnh, không cùng một hộp trơn; `cookie cracker retail packaging` |
| SM-D22 | [x] Tải 3 ứng viên | P0 | Thanh chocolate/bánh phủ chocolate | 3-5 loại | 15-25 thanh | Bọc có chiều dày và mép gói; `chocolate bar wrapped pack` |
| SM-D23 | [x] Tải 1 ứng viên | P1 | Kẹo/gum/thạch dẻo | 3-5 loại | 15-25 gói | Hộp/gói nhỏ phù hợp kệ đầu quầy; `candy chewing gum retail pack` |
| SM-D24 | [ ] Chưa tải | P1 | Hạt/khô/snack mặn | 3-4 loại | 12-20 gói | Túi zip hoặc gói hàn có nhãn; `nuts jerky snack packet` |
| SM-D25 | [x] Tải 2 ứng viên | P1 | Ngũ cốc/hộp đồ ăn sáng | 3-4 loại | 12-20 hộp | Hộp có cạnh/nắp gấp và đủ mặt nhãn; `cereal breakfast box grocery` |
| SM-D26 | [x] Tải 7 ứng viên | P1 | Thực phẩm đóng lon | 3-5 loại | 12-18 lon | Lon, nắp/khóa bật và nhãn; `canned food grocery pack` |
| SM-D27 | [x] Tải 14 ứng viên | P1 | Nước tương/dầu/gia vị | 3-5 loại | 10-15 chai/lọ | Đa dạng chai/lọ, nắp và nhãn; `Japanese condiments soy sauce grocery` |
| SM-D28 | [x] Tải 3 ứng viên | P1 | Hộp cà phê/trà bán lẻ | 3-4 loại | 8-12 hộp/gói | Có hộp, túi hoặc lọ hoàn chỉnh; `coffee tea retail package` |
| SM-D29 | [x] Tải 5 ứng viên | P1 | Gạo/bột/đường gói nhỏ | 2-3 loại | 6-9 gói | Bao/túi có khối và đường hàn, nhãn; `rice flour sugar grocery bag` |
| SM-D30 | [x] Tải 5 ứng viên | P1 | Trứng/đậu phụ/bơ/phô mai đóng gói | 4-6 mẫu trong nhóm | 8-12 đơn vị | Hộp trứng, khay/nắp hoặc hộp sữa phù hợp từng món; `eggs tofu butter cheese grocery pack` |
| SM-D31 | [ ] Chưa tải | P1 | Đồ ăn đông lạnh đóng gói | 3-4 loại | 6-12 gói | Túi/hộp bán lẻ đầy đủ; `frozen food retail packaging` |
| SM-D32 | [x] Tải 9 ứng viên | P2 | Trái cây bán lẻ nhỏ | 2-4 loại | 6-10 phần | Chuối/táo/quýt hoặc túi trái cây; chỉ chọn nếu có kệ phù hợp; `convenience store fruit pack` |
| SM-D33 | [x] Tải 2 ứng viên | P2 | Bia/rượu đóng lon/chai | 3-6 loại | 12-24 đơn vị nếu chọn | Có bộ chai/lốc từ CC0 để đánh giá trước; chọn bổ sung theo nhóm hàng được duyệt; `Japanese beer sake retail pack` |

### E. Hàng tạp hóa, chăm sóc cá nhân và đồ dùng nhỏ

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-E01 | [x] Tải 2 ứng viên | P0 | Khăn giấy hộp và gói bỏ túi | 2-3 mẫu | 10-18 đơn vị | Hộp/gói, khe mở và nhãn; `tissue box pocket tissue retail` |
| SM-E02 | [ ] Chưa tải | P1 | Khăn ướt | 2-3 mẫu | 8-12 gói | Túi phồng, nắp/tem đóng và mép gói; `wet wipes packaging` |
| SM-E03 | [x] Tải 1 ứng viên | P1 | Giấy vệ sinh đóng lốc | 1-2 mẫu | 4-6 lốc | Thấy cuộn và lớp bọc, không phải hộp đặc thay màu; `toilet paper retail pack` |
| SM-E04 | [x] Tải 2 ứng viên | P0 | Xà phòng rửa tay | 2 mẫu | 4-8 chai/gói | Chai, vòi bơm/nắp và nhãn; `hand soap dispenser refill pack` |
| SM-E05 | [x] Tải 1 ứng viên | P1 | Dầu gội/dầu xả | 2-3 loại | 6-10 chai | Thân, vai, nắp/vòi và nhãn; `shampoo conditioner retail bottle` |
| SM-E06 | [ ] Chưa tải | P1 | Sữa tắm | 2 loại | 4-8 chai | Có hình dáng riêng, vòi/nắp và nhãn; `body wash bottle packaging` |
| SM-E07 | [x] Tải 1 ứng viên | P1 | Bàn chải/kem đánh răng | 3 mẫu | 8-12 gói | Tuýp và vỉ/hộp bán lẻ hoàn chỉnh; `toothbrush toothpaste retail pack` |
| SM-E08 | [ ] Chưa tải | P1 | Mỹ phẩm/chăm sóc da cỡ nhỏ | 3-4 mẫu | 6-10 đơn vị | Chai/lọ/hộp có nắp và nhãn, không chỉ render mỹ phẩm; `travel skincare cosmetic packaging` |
| SM-E09 | [ ] Chưa tải | P1 | Sản phẩm vệ sinh cá nhân đóng gói | 2-3 mẫu | 6-10 gói | Gói/hộp có thể tích và nhãn đủ mặt; `feminine hygiene retail pack` |
| SM-E10 | [ ] Chưa tải | P1 | Dao cạo/đồ cạo râu | 2 mẫu | 4-6 vỉ/hộp | Có vỉ/hộp và sản phẩm bên trong nếu nhìn thấy; `razor shaving retail blister pack` |
| SM-E11 | [x] Tải 1 ứng viên | P1 | Nước giặt/gói giặt nhỏ | 2-3 mẫu | 6-10 đơn vị | Chai hoặc túi refill có vòi/nắp/đường hàn; `laundry detergent bottle pouch` |
| SM-E12 | [x] Tải 1 ứng viên | P1 | Nước rửa chén | 2 mẫu | 4-6 chai | Đủ chai, cổ/nắp, nhãn; `dishwashing liquid retail bottle` |
| SM-E13 | [ ] Chưa tải | P1 | Mút rửa chén/găng tay vệ sinh | 2-3 mẫu | 6-10 gói | Có hàng trong túi/vỉ và phần treo; `cleaning sponge gloves retail pack` |
| SM-E14 | [x] Tải 1 ứng viên | P1 | Pin AA/AAA đóng vỉ | 2-3 mẫu | 6-10 vỉ | Pin, vỉ và bìa có nhãn; `battery blister retail packaging` |
| SM-E15 | [ ] Chưa tải | P1 | Cáp điện thoại/củ sạc | 2-3 mẫu | 6-10 hộp/vỉ | Model bán lẻ đóng gói, không chỉ một sợi dây rời; `phone charger cable retail box` |
| SM-E16 | [ ] Chưa tải | P1 | Bút/sổ/đồ văn phòng nhỏ | 4-6 mẫu | 8-16 đơn vị | Có hộp/vỉ khi bán theo bộ, kích thước đúng; `stationery retail pen notebook pack` |
| SM-E17 | [ ] Chưa tải | P1 | Khẩu trang/băng cá nhân/sơ cứu bán lẻ | 3 mẫu | 8-12 hộp/gói | Nhãn, nếp gấp và hình dạng bao bì đúng; `mask bandage first aid retail pack` |
| SM-E18 | [x] Tải 1 ứng viên | P1 | Ô bán lẻ gấp và ô mẫu mở | 2 mẫu | 4-6 ô gấp + 1 mẫu nếu cần | Tay cầm, nan/vải và dây giữ; `transparent Japanese umbrella folded open` |
| SM-E19 | [ ] Chưa tải | P2 | Tất/quần tất đóng gói | 2 mẫu | 4-6 gói | Có bao bì và phần hàng nhìn thấy; `socks stockings retail packaging` |
| SM-E20 | [ ] Chưa tải | P2 | Khăn nhỏ/khăn bỏ túi | 2 mẫu | 4-6 gói | Hình vải/nếp gấp cùng đai/bao bì; `towel handkerchief retail pack` |
| SM-E21 | [x] Tải 1 ứng viên | P2 | Thức ăn thú nuôi gói/lon | 2-3 loại | 6-10 đơn vị | Bao/lon có nhãn hoàn chỉnh; `pet food pouch can retail` |
| SM-E22 | [ ] Chưa tải | P1 | Màng bọc/giấy bạc/túi rác bán lẻ | 3 mẫu | 6-9 hộp/gói | Cấu tạo hộp dài hoặc gói phù hợp từng món; `cling film foil garbage bag retail pack` |
| SM-E23 | [x] Tải 3 ứng viên | P1 | Đồ dùng một lần bán theo gói | 3 mẫu | 6-12 gói | Cốc/đũa/đĩa trong bao, có mép và nhãn; `disposable cups chopsticks plates retail pack` |
| SM-E24 | [x] Tải 1 ứng viên | P1 | Báo/tạp chí/truyện bán lẻ | 4-8 bìa | 8-16 cuốn/tờ | Có gáy, cạnh giấy, độ dày và nội dung được phép dùng; `Japanese magazine newspaper prop pack` |

### F. Góc cà phê, hâm nóng và đồ ăn nóng

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-F01 | [x] Tải 4 ứng viên | P0 | Máy cà phê thương mại/tự phục vụ | 1 mẫu | 1 máy | Thân, vòi, khay hứng, màn hình/nút và khoang cốc; `convenience store self service coffee machine` |
| SM-F02 | [x] Tải 1 ứng viên | P0 | Lò vi sóng thương mại | 1 mẫu | 1 máy | Có lòng lò, cửa/kính, tay nắm và nút; cửa tách nếu mở; `commercial microwave oven` |
| SM-F03 | [x] Tải 3 ứng viên | P1 | Tủ kính giữ nóng gà/đồ chiên | 1 mẫu | 1 tủ | Khay, kính, khung, cửa và đèn, nhìn được món bên trong; `convenience store hot food display case` |
| SM-F04 | [ ] Chưa tải | P2 | Tủ hấp bánh bao | 1 mẫu | 0-1 tủ | Tầng/khay, kính, cửa và máy hoàn chỉnh; `Japanese steamed bun warmer display` |
| SM-F05 | [ ] Chưa tải | P2 | Nồi/quầy oden có chia ngăn | 1 bộ | 0-1 bộ | Thân nồi, lòng, vách ngăn và nắp; `Japanese convenience store oden warmer` |
| SM-F06 | [x] Tải 2 ứng viên | P2 | Bình giữ nóng/bộ cấp đồ uống nóng | 1 mẫu | 0-1 máy | Vòi, nắp, tay cầm, chân/khay; `commercial hot beverage dispenser` |
| SM-F07 | [x] Tải 3 ứng viên | P0 | Bàn/tủ góc cà phê | 1 bộ | 1 cụm | Mặt bàn, thân, hộc và phía nhìn thấy hoàn thiện; có thể đi cùng F01; `coffee service counter cabinet` |
| SM-F08 | [x] Tải 4 ứng viên | P0 | Cốc cà phê, nắp và sleeve | 2-3 kiểu | 12-20 bộ/cốc xếp chồng | Miệng cốc, thành/lòng, nắp và đai giấy đầy đủ; `takeaway coffee cup lid sleeve pack` |
| SM-F09 | [x] Tải 4 ứng viên | P1 | Giá đường/sữa/que khuấy/ống hút | 1 bộ + 4 loại đồ | 1 cụm cấp đồ | Gói nhỏ, que và giá có mô hình rời; `coffee condiment stirrer dispenser set` |
| SM-F10 | [x] Tải 1 ứng viên | P1 | Kẹp gắp và giá/khay để kẹp | 1-2 kiểu | 1-2 bộ | Tay cầm, đầu gắp, khớp và khay; `food serving tongs tray` |
| SM-F11 | [x] Tải 5 ứng viên | P1 | Khay/bao giấy đựng đồ ăn nóng | 2-3 mẫu | 6-12 đơn vị | Có lòng, thành, mép gấp và lớp lót; `fried food paper tray bag` |
| SM-F12 | [x] Tải 4 ứng viên | P1 | Đồ chiên/corndog/xúc xích trưng bày | 3-5 món | 8-16 phần | Món ăn có hình khối, lớp vỏ và que khi phù hợp; `Japanese convenience store fried food pack` |
| SM-F13 | [x] Tải 1 ứng viên | P2 | Bánh bao hấp trưng bày | 2-3 loại | 6-10 bánh | Nếp bột, đáy và biến thể nhân; chỉ cần nếu có F04; `nikuman steamed bun 3D` |
| SM-F14 | [x] Tải 2 ứng viên | P2 | Nguyên liệu oden | 5-7 loại | 8-16 miếng | Củ cải, trứng, chả/đậu phụ theo bộ; chỉ cần nếu có F05; `oden ingredients food pack` |
| SM-F15 | [x] Tải 3 ứng viên | P1 | Bồn rửa nhỏ và vòi khu phục vụ | 1 bộ | 1 vị trí nếu bố trí phù hợp | Chậu có lòng, vòi, thoát nước và mặt tủ; `commercial small sink faucet cabinet` |
| SM-F16 | [x] Tải 3 ứng viên | P1 | Hộp cấp khăn giấy/khăn ăn tại quầy | 1-2 kiểu | 1-2 hộp | Cửa cấp giấy, giấy nhìn thấy và vật liệu; `countertop napkin dispenser` |

### G. Nhận hàng, góc kho và nhân viên

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-G01 | [x] Tải 2 ứng viên | P0 | Kệ thép kho nhỏ | 1 bộ | 1-2 module | Chân, giằng, tầng có chiều dày; không cần mô hình cả nhà kho; `small retail stockroom shelving` |
| SM-G02 | [x] Tải 2 ứng viên | P0 | Thùng carton giao hàng | 3-4 kiểu/trạng thái | 8-15 thùng | Bản đóng/mở, nắp, mép, băng keo và mặt trong khi mở; `cardboard delivery box pack` |
| SM-G03 | [x] Tải 1 ứng viên | P0 | Sọt/thùng nhựa giao hàng | 2-3 kiểu | 4-8 thùng | Lòng, đáy, thành, tay cầm và trạng thái xếp chồng; đánh giá nguồn thùng CC0 trước; `retail stackable delivery crate` |
| SM-G04 | [x] Tải 2 ứng viên | P1 | Xe lồng giao hàng | 1 mẫu | 1 xe nếu có chỗ | Bánh, khung/lưới, đáy và cửa; `retail roll cage trolley` |
| SM-G05 | [x] Tải 1 ứng viên | P1 | Xe đẩy hàng nhỏ/xe sàn | 1 mẫu | 1 xe | Bánh, trục, sàn và tay đẩy hoàn chỉnh; `platform hand trolley retail` |
| SM-G06 | [ ] Chưa tải | P2 | Pallet nhỏ gỗ/nhựa | 1-2 kiểu | 0-2 pallet | Có cấu tạo mặt dưới và khe càng; chỉ dùng nếu còn đủ diện tích; `small warehouse pallet` |
| SM-G07 | [x] Tải 1 ứng viên | P0 | Máy quét/máy kiểm kho cầm tay + dock | 1 bộ | 1 máy + đế | Màn hình, nút, cửa quét, chân sạc; thay vật thể kiểm kho đơn giản hiện tại; `inventory handheld barcode terminal` |
| SM-G08 | [x] Tải 1 ứng viên | P1 | Bảng kẹp giấy, sổ giao hàng và tem thùng | 1 bộ | 1-3 bộ | Giấy có độ dày, kẹp và texture thay được; `delivery clipboard paperwork props` |
| SM-G09 | [x] Tải 2 ứng viên | P2 | Tủ đồ nhân viên gọn | 1 module | 0-1 tủ | Cửa, tay nắm, khe gió và lòng tủ nếu mở; `small staff locker cabinet` |
| SM-G10 | [x] Tải 2 ứng viên | P2 | Ghế đẩu/ghế nhân viên nhỏ | 1 mẫu | 0-1 ghế | Có chân, mặt dưới và kết cấu thật; `retail staff stool` |
| SM-G11 | [x] Tải 1 ứng viên | P1 | Bình chữa cháy và gá treo | 1 bộ | 1-2 bộ | Van, ống, chốt, nhãn và gá; `fire extinguisher wall bracket` |
| SM-G12 | [x] Tải 1 ứng viên | P0 | Đồng hồ/thiết bị chấm ca | 1 mẫu | 1 thiết bị | Mặt đồng hồ hoặc màn hình, phím/khe theo mẫu; có thể dùng một đồng hồ hoàn chỉnh cho điểm chốt ca hiện tại; `staff time clock attendance terminal` |
| SM-G13 | [x] Tải 3 ứng viên | P1 | Bộ dụng cụ vệ sinh và biển sàn ướt | 1 bộ, 4-6 món | 1 góc nhỏ | Chổi, cây lau, xô, hót rác và biển gấp đủ cấu tạo; `retail janitor mop bucket wet floor sign pack` |
| SM-G14 | [x] Tải 2 ứng viên | P1 | Thùng rác khu nhân viên | 1 mẫu | 1 thùng | Nắp, tay cầm, lòng và túi lót khi nhìn thấy; `indoor commercial trash bin` |

### H. Đèn, điện và thiết bị gắn tường/trần

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-H01 | [x] Tải 1 ứng viên | P0 | Bộ đèn tuyến tính/đèn tuýp thương mại | 1-2 module | Khoảng 6-8 bộ hoặc module ghép | Vỏ, tấm tán sáng/bóng, đầu bịt và gá; vật liệu phát sáng riêng; `retail linear LED fluorescent light fixture` |
| SM-H02 | [x] Tải 1 ứng viên | P1 | Đèn downlight/spot gắn trần | 1-2 mẫu | 4-6 đèn | Vỏ, viền, nguồn sáng và gá đầy đủ; `commercial ceiling downlight fixture` |
| SM-H03 | [ ] Chưa tải | P1 | Đèn tường/đèn cửa hậu | 1-2 mẫu | 1-2 đèn | Vỏ, chụp, gá và mặt sau nhìn thấy; `commercial wall bulkhead light` |
| SM-H04 | [x] Tải 2 ứng viên | P1 | Biển đèn lối thoát | 1 bộ | 1-2 biển | Vỏ/hộp, gá và mặt biển đúng hướng; `Japanese emergency exit sign light` |
| SM-H05 | [x] Tải 1 ứng viên | P1 | Camera an ninh dome/bullet | 1-2 mẫu | 2-4 camera | Ống kính, vỏ/gá và dây nhìn thấy; `store CCTV security camera` |
| SM-H06 | [x] Tải 2 ứng viên | P1 | Điều hòa trong nhà/cassette | 1 kiểu phù hợp trần | 1-2 bộ | Miệng gió, cánh/lưới và mặt điều khiển; `commercial ceiling cassette air conditioner` |
| SM-H07 | [x] Có nguồn sẵn | P1 | Cục nóng điều hòa | 1 mẫu | 1-2 cục | Quạt, lưới, ống và gá; đã có nguồn CC0 cần xem chất lượng trước; `air conditioner outdoor condenser unit` |
| SM-H08 | [ ] Chưa tải | P1 | Công tắc, ổ cắm và bảng điều khiển | 1 bộ, 3-4 kiểu | 4-8 vị trí nhìn thấy | Mặt, khung, nút và vít khi nhìn gần; `Japanese wall outlet switch thermostat` |
| SM-H09 | [x] Tải 2 ứng viên | P2 | Máng dây/ống và hộp kỹ thuật lộ | 1 bộ module | Một số đoạn có thể thấy | Có góc nối, nắp và điểm kết thúc; `electrical conduit cable tray kit` |
| SM-H10 | [ ] Chưa tải | P1 | Báo khói/đầu báo gắn trần | 1-2 kiểu | 2-4 điểm | Đủ vỏ, rãnh và gá; `commercial smoke detector ceiling` |
| SM-H11 | [ ] Chưa tải | P1 | Gương lồi quan sát góc cửa hàng | 1 mẫu | 1 gương | Vỏ, viền, gá và mặt cong riêng; hiệu ứng phản chiếu sẽ kiểm tra trong URP; `retail convex security mirror` |

### I. Khu ngay trước cửa và phụ kiện ngoài cửa

Các nguồn ghi “đã có” vẫn cần qua kiểm tra nhìn gần theo mục 5. Nếu đạt thì dùng lại, không cần tải bản khác.

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-I01 | [x] Có nguồn sẵn | P1 | Máy bán nước tự động | 1-2 mẫu | 1-2 máy | Nút, khe tiền/thẻ, khay nhận hàng, hàng mẫu, cạnh/mặt sau; đã có nguồn CC0 để lọc; `Japanese drinks vending machine game ready` |
| SM-I02 | [x] Tải 1 ứng viên | P1 | Bộ thùng phân loại rác | 1 bộ | 3-4 ngăn/thùng | Miệng thùng, nắp, chân và ký hiệu; đã có nguồn CC0 để lọc; `Japanese convenience store recycling bins` |
| SM-I03 | [x] Tải 1 ứng viên | P1 | Giá để ô và khay nước | 1 mẫu | 1 giá | Có lỗ/ngăn, chân và khay; dùng cùng E18; `umbrella stand drip tray Japanese shop` |
| SM-I04 | [x] Tải 2 ứng viên | P0 | Thảm cửa thương mại | 1-2 mẫu | 1-2 thảm | Bề mặt/vân thảm và mép cao su hoàn chỉnh, nằm sát sàn; `commercial entrance floor mat` |
| SM-I05 | [x] Tải 1 ứng viên | P1 | Bảng quảng cáo chữ A đứng vỉa hè | 1 mẫu | 1 bảng | Chân, bản lề, mặt sau và vùng thay poster; `shop A frame pavement sign` |
| SM-I06 | [x] Có nguồn sẵn | P1 | Chậu cây sát cửa | 1-2 bộ | 1-3 chậu | Cây, đất, thành/đáy chậu; đã có nguồn CC0/Kenney để lọc; `shop entrance potted plant` |
| SM-I07 | [x] Tải 1 ứng viên | P2 | Ghế băng/ghế nghỉ sát mặt tiền | 1 mẫu | 0-1 ghế | Có mặt dưới, chân/gá và kết cấu hoàn chỉnh; `convenience store outdoor bench` |
| SM-I08 | [x] Có nguồn sẵn | P2 | Trụ chặn/giá xe đạp gọn | 1-2 kiểu | 0-1 cụm | Chân tiếp nền, gá, mép và vật liệu phù hợp; không cản lối cửa; `shop bicycle rack bollard` |
| SM-I09 | [ ] Chưa tải | P1 | Rãnh thoát nước và nắp grate gần cửa | 1-2 module | 2-4 đoạn | Nắp có độ dày/khe, khung và đầu nối; `sidewalk drainage channel grate` |
| SM-I10 | [x] Tải 1 ứng viên | P2 | Hộp điện/đồng hồ điện phía ngoài | 1 bộ | 0-1 cụm | Vỏ, cửa, khóa, đồng hồ và ống nối nhìn thấy; `Japanese shop utility meter box` |

### J. Dịch vụ bổ sung - đề xuất chưa chốt, không bắt buộc tải

Chỉ chọn khi quyết định dành chỗ cho dịch vụ đó. Một model có màn hình/khay không đồng nghĩa game đã có chức năng tương ứng.

| ID | Tải ứng viên | Mức | Model/bộ cần tìm | Mẫu nên có | Số lượng đặt dự kiến | Chi tiết cần đủ và từ khóa tìm |
|---|---|---|---|---|---|---|
| SM-J01 | [x] Tải 1 ứng viên | P2 | ATM đặt trong cửa hàng | 1 mẫu | 0-1 máy | Màn hình, phím, khe thẻ/tiền, vỏ và mặt sau; `Japanese convenience store ATM` |
| SM-J02 | [x] Tải 1 ứng viên | P2 | Máy photocopy/in đa chức năng | 1 mẫu | 0-1 máy | Khay giấy, nắp scan, màn hình và các mặt nhìn thấy; `convenience store multifunction copier printer` |
| SM-J03 | [x] Tải 1 ứng viên | P2 | Tủ nhận/gửi bưu kiện | 1 module | 0-1 cụm | Cánh/ngăn, màn hình/khóa và mặt bên đầy đủ; `parcel locker retail cabinet` |
| SM-J04 | [x] Tải 2 ứng viên | P2 | Bàn/ledge và ghế ăn nhanh nhỏ | 1 bộ | 0-1 cụm | Chân/gá, mặt dưới và tỷ lệ ngồi; chỉ bố trí khi đủ lối đi; `convenience store eat in counter stools` |
| SM-J05 | [ ] Chưa tải | P2 | Tủ hàng giới hạn sau quầy | 1 bộ | 0-1 tủ | Nếu chọn trưng bày thuốc lá hoặc hàng tương tự: tủ/cửa và bao bì hoàn chỉnh, thương hiệu có quyền dùng; `behind counter tobacco display cabinet` |
| SM-J06 | [x] Có nguồn sẵn | P2 | Kiosk bán vé/thiết bị dịch vụ đa phương tiện | 1 mẫu | 0-1 máy | Vỏ, màn hình, phím, khe và chân; `Japanese convenience store ticket kiosk` |

## 5. Điều kiện để coi một model là hoàn chỉnh

### Chất lượng hình ảnh và cấu tạo

| Kiểm tra | Nhận khi | Cần loại hoặc hỏi lại tác giả khi |
|---|---|---|
| File thật | Có mesh tải được, mở xem được nhiều góc | Chỉ là ảnh/render/thumbnail, không có model hoặc chỉ cấp một ảnh làm mặt phẳng |
| Cấu tạo | Quầy có mặt/thân/hộc; kệ có chân/tầng/đỡ; tủ có lòng/tầng/cửa; máy có bộ phận nhận diện | Thiếu bộ phận chính, tủ không có ruột, cửa chỉ là màu vẽ trên khối đặc |
| Mặt có thể thấy | Front/side/rear/top phù hợp việc đặt trong shop đều hoàn thiện; không lộ mặt hở ngoài ý muốn | Chỉ đẹp ở một góc; xoay ra sau mất mặt, lộ ruột hoặc thiếu texture |
| Bao bì/hàng hóa | Chai có nắp/nhãn; gói có thể tích và mép; hộp có cạnh/nắp; món qua nắp trong có đồ ăn | Mọi hàng là một hộp/chai đơn sắc; tên gọi khác nhưng hình và nhãn vẫn sơ sài |
| Nhìn gần | Đọc được hình dáng và chi tiết ở camera khoảng 0.5-3 m; không thấy UV kéo méo rõ | Chỉ dùng được làm vật thể rất xa; chi tiết đẹp chỉ nằm trong ảnh render của người bán |
| Đồng bộ | Phù hợp phong cách anime 3D Nhật, tỷ lệ đời thường; màu, độ nhám và mức chi tiết nhất quán | Trộn mô hình khối đồ chơi rất đơn giản với thiết bị chi tiết cao mà không có hướng mỹ thuật chung |
| Mesh rời | Có bản quầy/kệ/tủ trống; sản phẩm là object/mesh riêng; cửa/ngăn kéo tách nếu muốn mở | Cả shop và hàng bị hàn vào một mesh, không thể lấy riêng sản phẩm hoặc bố trí lại mà phải dựng lại |

**“Model hoàn chỉnh” không đồng nghĩa mọi món phải nhiều polygon hay photorealistic.** Một thảm phẳng có texture tốt, một hộp giấy có nếp/cạnh/nhãn đầy đủ hoặc model stylized được tác giả hoàn thiện vẫn hợp lệ. Tiêu chí là chất lượng nhìn gần và cấu tạo phù hợp, không phải đuôi `.glb` hay số tam giác lớn.

### Tương thích để nhập vào project

- Đích hiện tại: **Unity 6000.3.7f1, URP 17.3.0, Windows PC**. Ưu tiên `.fbx` hoặc `.glb` kèm đầy đủ vật liệu/texture; `.blend` hữu ích làm nguồn chỉnh sửa. OBJ dùng được cho props tĩnh nếu có MTL và texture; vật liệu có thể cần chuyển.
- Nếu chỉ bán `.unitypackage`, cần biết render pipeline và có mesh/texture gốc. Pack Unreal/C4D/Cycles chỉ có shader riêng, chưa có texture baked, không phải bộ có thể đưa thẳng vào URP.
- Texture dùng **PNG/JPG** là lựa chọn dễ nhập hiện tại. File GLB chỉ có `EXT_texture_webp` từng làm importer hiện tại thất bại; chọn bản PNG/JPG hoặc nguồn cho phép chuyển texture.
- Với model textured: có UV và Base Color, Normal, Roughness/Metallic/AO theo cách đóng gói được ghi rõ; kính/emission là vật liệu riêng khi cần. Không yêu cầu mọi model stylized đều phải có đủ mọi map nếu vật liệu cuối đã hoàn chỉnh.
- Tỷ lệ theo mét, pivot hợp lý; có thể lấy riêng từng vật thể. Tham chiếu khi tìm: cửa người đi cao khoảng 2.1-2.4 m; quầy cao khoảng 0.9-1.1 m; kệ giữa khoảng 1.4-1.8 m; tủ mát đứng khoảng 1.8-2.2 m. Đây là hướng chọn model, không phải yêu cầu ép méo model để khớp blockout cũ.
- Tìm bản dùng cho game, biết số polygon/texture. Model CAD/sculpt nhiều triệu polygon cần có bản giảm phù hợp; ưu tiên LOD cho thiết bị lớn. Không lấy dung lượng ZIP hoặc ảnh 4K làm bằng chứng tối ưu.
- Khoảng đi lại khi bố trí sẽ được kiểm lại với cả hai nhân vật; tham chiếu lối chính khoảng 1.2 m trở lên và giữ cửa vào rõ ràng. Model tốt vẫn cần làm prefab/collider, chỉnh vật liệu và gắn điểm tương tác trong Unity.
- Cần thông tin tác giả, link nguồn và license dùng trong game. Quyền dùng trong bản build khác quyền đưa file nguồn lên GitHub public; asset mua hoặc có hạn chế nguồn sẽ được giữ riêng. Ưu tiên nhãn hư cấu/được phép sử dụng và giữ giấy phép của texture/font đi kèm.

## 6. Các tài nguyên phụ cần đi cùng model

Đây không phải yêu cầu mua thêm một model riêng cho từng hình in. Pack đã có tài nguyên hợp lệ thì dùng chung; nội dung nhận diện SORA sẽ được thay cho phù hợp.

| Tài nguyên | Cần để hoàn thiện |
|---|---|
| Nhãn bao bì đầy đủ | Mặt trước/bên/sau của hàng hóa, nhiều loại đọc được; không chỉ một ô màu trắng |
| Biển SORA MART và biển 24H | Hình in/texture có thể thay trên model hộp đèn; vật liệu emission điều chỉnh được |
| Poster món ăn/khuyến mãi | Nội dung gắn khung A11/I05, đúng ngữ cảnh cửa hàng Nhật |
| Nhãn giá và nhãn ngành hàng | Texture/atlas hoặc vùng đặt chữ phù hợp ray kệ, có giá yen và biển phân khu |
| Font hỗ trợ tiếng Nhật | Có giấy phép sử dụng phù hợp; ví dụ họ Noto Sans JP kèm license của font |
| Vật liệu nội thất | Gạch/ron, laminate/kim loại quầy, kính, nhựa tủ, thảm và vải nếu pack chưa cung cấp |
| Ký hiệu cửa hàng | Phân loại rác, nhân viên, vệ sinh, lối thoát, chỉ dẫn kính và cửa tự động |

Ánh sáng, phản chiếu kính, nền ướt và mưa là phần thiết lập Unity/vật liệu, không thể được bảo đảm chỉ bằng việc tải một model có ảnh preview đẹp.

## 7. Mức độ lấp đầy tham chiếu

Mục tiêu là đủ chủng loại và đủ chiều sâu trên kệ, không phải hàng nghìn model khác nhau.

| Khu | Cách ước lượng sau khi chọn model |
|---|---|
| Bốn module kệ giữa | Ví dụ 4 module x 2 mặt x 4 tầng x 6 vị trí ngang = 192 vị trí hàng phía trước; xếp sâu 2-3 đơn vị tạo khoảng 384-576 bản sao |
| Dãy tủ đồ uống | Ví dụ 4 module x 4 tầng x 7 vị trí x 2 hàng sâu = 224 bản sao; điều chỉnh theo tủ thật tải được |
| Đồ ăn tươi/bánh/tủ đông | Khoảng 40-80 phần/gói, phân loại theo khay và kích thước tủ |
| Đầu quầy và kệ phụ | Khoảng 20-60 sản phẩm nhỏ, giá/nhãn không bị che |
| Kho | Thùng, sọt và hàng dự trữ có lý do bố trí; giữ tuyến giao hàng và đi lại |

Có thể bắt đầu bằng khoảng **100-160 biến thể hàng hóa hoàn chỉnh** trong các pack, rồi tái sử dụng để đạt khoảng **700-1,000 đơn vị trưng bày** theo bố trí đã chọn. Đây là ước lượng nội dung, **không phải số model bắt buộc tải, số SKU gameplay đã triển khai hoặc cam kết FPS**. Các nhóm P2 không phải phần bắt buộc của mốc đầu.

Khi có đủ model mới, phần quầy/kệ/tủ/hàng đơn giản hiện tại sẽ là vật đối chiếu để thay, không được dùng thêm hộp/chai đổi màu để đánh dấu các mục còn thiếu là đã hoàn thành.

## 8. Cách lưu các bộ tải về để tiếp tục tích hợp

Thư mục nhận đề xuất trong **checkout Unity chính anh đang dùng**: **`LocalReferenceAssets/SoraMartIncoming/`**, hiện đã nằm trong vùng `.gitignore` của repo. Giữ nguyên ZIP gốc, license và link nguồn; mỗi pack một thư mục. Không cần tạo một thư mục cho mỗi bản sao sản phẩm trong shop.

```text
LocalReferenceAssets/SoraMartIncoming/
  SM-B01_B02_B03_RetailCheckout_PackName/
    original-download.zip
    LICENSE.txt
    SOURCE.txt
  SM-D01_D02_D03_JapaneseFood_PackName/
    original-download.zip
    LICENSE.txt
    SOURCE.txt
```

Trong `SOURCE.txt` ghi: tên pack, tác giả, link trang gốc, ngày tải, license/quyền dùng và các ID trong danh sách mà pack đáp ứng. Nếu license nằm sẵn trong ZIP thì giữ nguyên, không cần tự tạo lại nội dung giấy phép.

Mẫu ghi một pack đã tìm được:

| Pack/tên file | ID đáp ứng | Link nguồn/tác giả | License | Định dạng + texture | Thư mục local/ghi chú |
|---|---|---|---|---|---|
| Điền sau khi chọn | Ví dụ SM-B01, SM-B02, SM-B03 | Điền link thật | Điền tên/đường dẫn license | Ví dụ FBX + PNG, có props rời | Điền đường dẫn và phần còn thiếu |

Khi nhận bộ mới, kiểm tra model/texture/license, chọn mẫu đạt chất lượng, tạo prefab/collider phù hợp, rồi mới thay vào SORA MART. Các trạng thái nên ghi rõ: **đã tìm**, **đã tải**, **đã mở kiểm tra**, **đạt chất lượng**, **đã tích hợp Unity**; không coi các trạng thái này là một.

## 9. Checklist chọn tải nhanh

- [ ] Có model và texture thực tế, xem được front/side/back; hình preview khớp hàng nhận.
- [ ] Quầy/kệ/tủ có cấu tạo đầy đủ; cửa/ngăn và sản phẩm tách được theo nhu cầu.
- [ ] Hàng hóa có bao bì/nhãn hoàn chỉnh; đồ ăn nhìn qua nắp có phần bên trong.
- [ ] Phong cách và tỷ lệ khớp bộ chính đã chọn; không trộn ngẫu nhiên nhiều mức chi tiết.
- [ ] Có FBX/GLB hoặc nguồn chuyển được, PNG/JPG/vật liệu đầy đủ và thông số polygon/texture.
- [ ] Có license, tác giả, link nguồn; lưu được dữ liệu gốc để kiểm tra và ghi asset register.
- [ ] Đánh dấu đủ những ID pack thực sự chứa, chưa đánh dấu những món chỉ xuất hiện trong ảnh minh họa.

## 10. Nguồn kiểm kê trong repo

- [Chỉ mục các ZIP hiện có](ASSET_INDEX.md) và [dữ liệu chỉ mục](asset_index.json).
- [Các nguồn môi trường đã nhận](../3d%20model%20enviroment/README.md).
- [Manifest SORA MART hiện tại](StreetExterior/SORA_MART_ASSETS.json).
- [Script cho biết các phần quầy/kệ/hàng tự dựng hiện tại](../Tools/Blender/build_sora_mart.py).
- [Báo cáo kỹ thuật và ảnh Unity của bản trước](StreetExterior/Reports/2026-11-10-SORA-MART.md).
- [Sổ nguồn và quyền dùng asset](ASSET_REGISTER.csv).

Kết quả compile/test trong báo cáo trước chứng minh bản trước chạy được; **không chứng minh các model đã đạt chuẩn mỹ thuật mới**. Danh sách này là yêu cầu tìm model theo chỉ đạo mới của chủ dự án, không phải xác nhận đã có hoặc đã kiểm tra chất lượng của các pack sẽ tải.
