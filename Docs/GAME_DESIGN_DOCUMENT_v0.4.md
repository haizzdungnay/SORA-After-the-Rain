# SORA: After the Rain — Game Design Document v0.4

**Ngày:** 02/11/2026  
**Trạng thái:** Bản định hướng cập nhật theo quyết định mới của chủ dự án; chi tiết chưa xác định gắn nhãn [ĐỀ XUẤT].  
**Thay thế về mặt cốt truyện:** Các mục hai nhân vật Haruto/Aoi, mối quan hệ người yêu cũ, bí mật cửa hàng cũ và phương án A–E trong GDD v0.2. Những nguyên tắc kỹ thuật về switch, POS, save/load và mục tiêu art của v0.2 vẫn giữ lại nếu không mâu thuẫn.

## 1. Logline [ĐÃ CHỐT]

**Tei Toshiharu (丁俊陽)** và **Tei Akiko (明子)** là hai anh em sinh đôi, **tân sinh viên năm nhất đại học**, vừa từ quê chuyển lên Tokyo. Lần đầu tự lo sinh hoạt ở thủ đô, họ choáng váng vì tiền thuê nhà, điện nước và chi phí ăn uống. Cả hai xin làm thêm tại cùng một cửa hàng tiện lợi để trang trải cuộc sống. Nhưng bên cạnh chuyện kiếm tiền bằng ca làm, họ còn âm thầm nuôi một giấc mơ rất thực dụng: gặp được một “nửa kia” có điều kiện kinh tế tốt để khỏi phải chật vật phấn đấu hai mươi năm. Hai anh em vừa phải hoàn thành những công việc thực sự ở cửa hàng, vừa tìm cách gây ấn tượng với khách, xây dựng quan hệ và đối phó với cạnh tranh tình cảm, bao gồm những khách đã có người yêu.

**Tông truyện:** đời thường, hài hước, châm biếm nhẹ chuyện tiền bạc/tình cảm; dần chuyển sang romantic drama, ghen tuông, hiểu lầm, cạnh tranh và những cái giá của lựa chọn. Không mở đầu bằng bí ẩn lớn, quá khứ bi kịch hay tình tiết giật gân cưỡng ép.

**Độ tuổi [ĐÃ CHỐT KHUNG / QUY ƯỚC THIẾT KẾ]:** Chủ dự án muốn giai đoạn **17–18 tuổi**, **sinh viên đại học năm nhất**. Để hợp nhịp nhập học đại học phổ thông tại Nhật và thuận lợi cho các tuyến tình cảm trưởng thành, **bản chơi hiện tại đặt cả hai 18 tuổi**; tuổi 17 thuộc giai đoạn hồi tưởng trước khi rời quê. Ngoại lệ nhập học ở tuổi 17 cần thiết lập đặc biệt nếu sau này muốn dùng. Không đặt cảnh tình dục cho nhân vật chưa đủ 18 tuổi. Các tuyến romance giữa người trưởng thành được triển khai dưới hình thức lựa chọn và hệ quả, không mặc định khách giàu nào cũng trở thành mục tiêu có thể chinh phục.

**Xuất phát điểm [ĐÃ CHỐT]:** Hai anh em **mới từ quê lên Tokyo**, thiếu kinh nghiệm sống tự lập; cuộc sống đắt đỏ khiến cả hai cùng quyết định kiếm việc ở cửa hàng tiện lợi. **[ĐỀ XUẤT]** Mở màn tại căn phòng thuê nhỏ, với hóa đơn tiền nhà đầu tháng và lời tuyên bố nửa đùa nửa thật: "Phải kiếm được phú ông/phú bà trước khi ví hết tiền!"

### Premise mở đầu (đề xuất)

Sau chuyến tàu từ quê tới Tokyo, hai anh em dọn vào căn phòng thuê nhỏ. Chưa kịp vui vì đỗ đại học, họ đã phải tính toán tiền phòng, đồ dùng thiết yếu, học phí và tiền ăn. Người chơi nhìn thấy con số trong ví tụt xuống; Toshiharu hoặc Akiko buột miệng: "Lương làm thêm chắc đủ sống... nhưng nếu gặp được một người vừa tốt bụng vừa giàu thì sao nhỉ?" Cả hai biến giấc mơ đó thành trò thi đua vui, rồi bước vào ca thử việc ở cửa hàng tiện lợi.

**Chú ý phạm vi demo:** Phòng trọ có thể chỉ là đoạn hội thoại/cutscene ngắn hoặc UI tĩnh. Không bắt buộc dựng đại học, ga tàu hay quê nhà ở v0.1.

## 2. Nhân vật chính [ĐÃ CHỐT / CẦN BỔ SUNG]

| Nhân vật | Thông tin đã xác nhận | Đặc điểm/tương phản đề xuất |
|---|---|---|
| **Tei Toshiharu (丁俊陽)** | Nam, anh/em sinh đôi của Akiko; sinh viên đại học năm nhất, khoảng 18 tuổi; vừa từ quê lên Tokyo, đi làm thêm | Điềm tĩnh ngoài mặt, bên trong cực kỳ tính toán; chuyên lập chiến lược “đổi đời” nhưng thường bị đời phản đòn |
| **Tei Akiko (明子)** | Nữ, chị/em sinh đôi của Toshiharu; sinh viên đại học năm nhất, khoảng 18 tuổi; vừa từ quê lên Tokyo, đi làm thêm | Hoạt bát, nhạy chuyện xã hội; ứng biến tốt nhưng dễ đẩy tình huống vượt tầm kiểm soát |

- **[CHƯA CHỐT]** Ai là anh/chị sinh trước, tạo hình, kiểu đồng phục, tính cách chính thức, tên cửa hàng và cách đọc/viết kana chính thức của tên Nhật. Chữ Hán đã ghi chính xác theo thông tin người dùng; không tự ý thay thế.
- Cả hai tồn tại đồng thời; người chơi đổi điều khiển bất cứ khi nào hợp lệ, người kia tiếp tục NPC work AI. Hai anh em có thể trêu chọc, đối đầu, giúp đỡ nhau, giấu thông tin về crush hoặc hợp lực xử lý ca làm.
- Không xây dựng quan hệ lãng mạn giữa hai anh em. Quan hệ anh em là nguồn hài, cạnh tranh và hỗ trợ.

## 3. Game fantasy và core loop [ĐÃ CHỐT + ĐỀ XUẤT]

**Mỗi ca:** Đi học/vào ca (tối giản trong demo) → nhận nhiệm vụ cửa hàng → tiếp khách → thực hiện nghiệp vụ (POS, xếp kệ, nhập hàng, kiểm kê) → chọn cách gây thiện cảm với khách → xử lý hệ quả, lịch hẹn, đối thủ tình cảm → chốt ca, tính lương, chi phí sinh hoạt → mở ngày mới.

Hai lớp mục tiêu chạy song song:
1. **Work track:** hoàn thành ca, tránh sai POS/thiếu hàng, giữ uy tín với quản lý, nhận lương.
2. **Social track:** làm quen khách, nhớ sở thích, chọn lời thoại/hành động, gây thiện cảm hoặc mất điểm, mở những tuyến chuyện riêng.

**Tension chủ đạo:** Dành nhiều thời gian tán tỉnh thì công việc dễ trễ; chỉ chăm chăm làm việc thì có thể bỏ lỡ cơ hội gặp người mình thích. Hai anh em có thể hỗ trợ chia việc hoặc cạnh tranh cơ hội.

## 4. Quy tắc bắt đầu đơn giản, drama tăng dần [ĐỀ XUẤT]

- **Ngày 1 — 'Lên Tokyo đã khó, sống ở Tokyo còn khó hơn':** Hai anh em mới chuyển đến, nhận việc và nếm trải quy trình cửa hàng. Gặp vài khách có vẻ tiềm năng; những kế hoạch làm giàu 'trong một nốt nhạc' chủ yếu mang tính hài.
- **Ngày 2–3 — 'Khách quen':** Những khách từng gặp quay lại. Người chơi học thói quen mua hàng, giờ xuất hiện, cách nói chuyện; bắt đầu được chọn theo đuổi những mối quan hệ phù hợp độ tuổi.
- **Ngày 4–6 — 'Cạnh tranh':** Xuất hiện người được khách quan tâm, bạn học, hoặc người yêu của một khách. Có thể cạnh tranh để được chú ý, nhưng lựa chọn thiếu trung thực sẽ gây hậu quả xã hội và giảm uy tín.
- **Ngày 7 trở đi — 'Giấc mơ đổi đời có cái giá':** Xung đột công việc, tiền thuê nhà, bạn bè, gia đình và tình cảm; mỗi tuyến NPC có drama riêng thay vì một bí mật lớn bắt buộc.

Không dùng nhịp drama này như lịch cứng; ngày chỉ là cấu trúc để phân bổ nội dung trong tương lai.

## 5. Hệ thống khách hàng và quan hệ [ĐỀ XUẤT]

**NPC customer profile**: `npcId`, `ageBand`, `role`, `schedule`, `purchasePreferences`, `relationshipStatus`, `knownFacts`, `routeAvailability`, `encounterCount`, `affinityByCharacter`, `trustByCharacter`, `reputationImpact`.

**Chỉ số tách biệt**:
- `affinity`: thiện cảm theo từng nhân vật với NPC.
- `trust`: mức độ tin tưởng, có thể giảm nếu nói dối/giấu chuyện.
- `familiarity`: mức độ đã quen, tăng qua tương tác đơn giản.
- `storeReputation`: ảnh hưởng chung của hành vi tại cửa hàng.
- `shiftPerformance`: hiệu suất ca làm; không gộp chung với tình cảm.
- `money`: lương/tích lũy/chi tiêu, thể hiện áp lực kinh tế thực tế trong game.

**Lựa chọn khi tiếp khách** (không phải mọi khách đều mở nhánh tình cảm):
- Phục vụ nhanh, đúng chuẩn → tăng hiệu suất/uy tín.
- Bắt chuyện, nhớ sở thích, giới thiệu sản phẩm phù hợp → có cơ hội tăng familiarity/affinity nhưng tốn thời gian.
- Giúp khách giải quyết một rắc rối nhỏ → mở quest cá nhân.
- Cố vượt ranh giới, tung tin sai hoặc chơi xấu đối thủ → có rủi ro mất trust, bị phàn nàn, đóng tuyến quan hệ. Hệ thống không thưởng tuyệt đối cho hành vi gây hại.

**Đối tượng lãng mạn:** Ở thời điểm gameplay, hai nhân vật chính đều 18 tuổi. Các nhánh tình cảm trưởng thành chỉ dành cho nhân vật từ 18 tuổi trở lên và dựa trên sự đồng thuận. Có thể phát triển tình tay ba, cạnh tranh, nói dối, chiếm cảm tình người đã có người yêu và hậu quả xã hội; tránh biến thao túng hay cưỡng ép thành cơ chế chinh phục được bảo đảm thành công.

## 6. Hai anh em khác nhau ở lối chơi [ĐỀ XUẤT]

- **Toshiharu:** thiên về chuẩn bị, nhớ dữ kiện khách, xử lý nghiệp vụ cẩn thận, chọn cách tiếp cận có kế hoạch.
- **Akiko:** thiên về trò chuyện, ứng biến và xử lý các tình huống bất ngờ, mở các góc đối thoại khác.
- **Không khóa nghiệp vụ:** cả hai đều thu ngân, kiểm kho, giao hàng, sắp kệ và nói chuyện được.
- **Shared state:** hàng tồn, tiền cửa hàng, trạng thái ca và công việc là một; không reset khi đổi nhân vật.
- **Per-character state:** quan hệ với khách, độ quen, vị trí, trạng thái hoạt động và dữ kiện nhân vật đã biết được lưu riêng.
- **Switch rule prototype:** POS draft phải commit/cancel an toàn trước switch hoặc tạm khóa khi commit; không được nhân đôi doanh thu/stock/quest.

## 7. Ca demo v0.1: 'Hai anh em đi săn vận may' [ĐỀ XUẤT]

**Mục tiêu:** Một ca 20–30 phút trên Windows, một cửa hàng nhỏ và một đoạn phố, ba khách đáng nhớ, một quest nghiệp vụ, một quest xã hội; có thể mở bằng đoạn đối thoại ngắn trong phòng trọ, chưa cần dựng toàn bộ đại học hay miền quê.

| Beat | Việc ở cửa hàng | Cơ hội xã hội/hài | Nghiệm thu |
|---|---|---|---|
| 0–3 phút | Hai anh em vừa nhận việc, nhắc chuyện tiền phòng rồi bắt đầu ca; hướng dẫn di chuyển/switch | Cả hai bàn 'kế hoạch đổi đời' một cách hài hước | 2 character cùng tồn tại; Tab hoạt động |
| 3–8 phút | Khách đầu tiên, quét hàng và thanh toán | Một khách ăn mặc sang trọng khiến cả hai tranh làm thu ngân | POS commit chính xác một lần |
| 8–15 phút | Nhận hàng, phát hiện số lượng lệch, chuyển hàng lên kệ | Khách quay lại, buộc người chơi chọn tiếp chuyện hay hoàn thành công việc | Kho và quỹ chính xác; quest không bị nhân |
| 15–21 phút | Sắp kệ, nhận yêu cầu từ quản lý | Một khách quen tuổi phù hợp có tình huống nhỏ; chọn giúp hay bỏ qua | Social quest thay đổi familiarity/trust |
| 21–25 phút | Đối soát/chốt ca | Hai anh em so kết quả: ai được khách nhớ tên, ai gây rắc rối | End shift report + save/load |

**Mẫu quest chính:** `Q-WORK-001` kiểm hàng nhận thiếu; cả hai đều có thể hoàn thành, nhân vật kia tiếp tục công việc nền.

**Mẫu quest xã hội:** `Q-SOCIAL-001` một khách quen bỏ quên món đồ đã thanh toán. Người chơi tìm, trả lại, chọn cách nói chuyện; nếu không xử lý đúng có thể bị mất uy tín. Quest không khóa ca và không cần tán tỉnh người chưa phù hợp độ tuổi.

**Hook cuối ca:** Hai anh em về nhà so doanh thu, tiền công và 'kế hoạch đổi đời'; một khách đã gặp sẽ quay lại trong ca tiếp theo.

## 8. Hướng phát triển drama tương lai [ĐỀ XUẤT]

- Nhánh cạnh tranh tình cảm: một NPC đã có người yêu; người chơi có thể gây chú ý và lựa chọn trung thực hoặc thiếu trung thực, để câu chuyện phản ánh hậu quả, không ép kết cục.
- Khách dễ gần nhưng kinh tế bình thường so với khách giàu nhưng khó tiếp cận: lựa chọn giữa tình cảm chân thành và mục tiêu vật chất.
- Anh em có thể tranh nhau lời khen của quản lý, quyền chọn ca và lượng thời gian tiếp khách; đôi khi phải cứu nhau khỏi rắc rối.
- Người chơi có thể bỏ mọi nhánh tình cảm và vẫn hoàn tất store sim; game không phá khóa cốt truyện bắt buộc.

## 9. Hệ thống kỹ thuật giữ nguyên từ GDD v0.2

- Unity/URP/C# là stack của source prototype v0.4; repo `haizzdungnay/SORA-After-the-Rain` được tạo ngày 02/11/2026. Chưa nghiệm thu compile/Play Mode trong Unity Editor.
- `CharacterManager`, NPC work AI, `SharedWorldState`, `StoreManager`, POS invoice ledger idempotent, inventory, `NarrativeManager`, `VersionedSaveSystem`.
- Windows PC trước, mobile về sau.
- Phong cách anime 3D, bối cảnh cửa hàng và phố Tokyo mưa đêm; tham chiếu NTR Master ở mức **chất lượng hình ảnh nhân vật**, không sao chép.
- Kiểm thử cốt lõi: switch 100 lần, không trùng hóa đơn; nhập/xuất/tiền nhất quán; 2 bộ quan hệ độc lập; side quest tùy chọn; save/load khôi phục đúng.

## 10. Quyết định đã thay đổi so với bản trước

| Nội dung | Trước (v0.2) | Nay (v0.3) |
|---|---|---|
| Nam | Haruto, 24 tuổi (đề xuất) | **Tei Toshiharu (丁俊陽)** [ĐÃ CHỐT] |
| Nữ | Aoi, 23 tuổi (đề xuất) | **Tei Akiko (明子)** [ĐÃ CHỐT] |
| Quan hệ | Đồng nghiệp/người yêu cũ (đề xuất) | **Anh em sinh đôi** [ĐÃ CHỐT] |
| Bối cảnh cuộc sống | Nhân viên trưởng thành độc lập (đề xuất) | **Tân sinh viên đại học năm nhất, 18 tuổi ở đầu gameplay; vừa từ quê lên Tokyo; làm thêm lo sinh hoạt** [ĐÃ CHỐT] |
| Động cơ | Drama/bí mật/điều tra (đề xuất) | **Làm thêm + mơ tìm người giàu để bớt vất vả** [ĐÃ CHỐT] |
| Nhịp truyện | Bí ẩn bắt đầu ngay | **Đời thường/hài trước, drama phát triển dần** [ĐÃ CHỐT] |
| Vai trò NPC | Nhiệm vụ cửa hàng là trục chính | **Khách hàng vừa là người mua hàng vừa là nguồn tuyến quan hệ** [ĐÃ CHỐT] |
| Gameplay công việc | POS/nhập xuất/kiểm kê | **Giữ nguyên, phải chơi thật** [ĐÃ CHỐT] |

## 11. Quyết định mới ngày 02/11/2026 [ĐÃ CHỐT]

- **Bối cảnh học tập:** Cả Toshiharu và Akiko là **sinh viên năm nhất đại học**; không còn là học sinh cao trung.
- **Khoảng tuổi tác giả mong muốn:** **17–18 tuổi**; để tương thích thông lệ nhập học tại Nhật, **tuổi trong gameplay là 18** và 17 chỉ có thể xuất hiện ở phần quá khứ trước khi lên Tokyo.
- **Xuất thân:** Hai anh em sinh đôi **vừa rời quê lên Tokyo**, còn lạ lẫm với cuộc sống thủ đô.
- **Áp lực đời sống:** Tiền thuê nhà, sinh hoạt phí, học phí và các chi tiêu đầu đời thúc đẩy việc làm thêm.
- **Động cơ hài:** Ngoài kiếm lương, họ cùng nuôi hy vọng tìm người yêu giàu có, được "bao nuôi" để bớt cảnh phấn đấu; lựa chọn của người chơi quyết định liệu mục tiêu thực dụng ấy dẫn tới tình cảm, xung đột hay trưởng thành.
- **Không đổi:** Hai nhân vật điều khiển luân phiên, làm việc thật tại cửa hàng, khách hàng là nguồn quest, drama tăng dần và PC trước mobile.

## 12. Các chi tiết vẫn còn mở [ĐỀ XUẤT / CHƯA CHỐT]

- Quê của hai anh em (tỉnh/thành hoặc thị trấn hư cấu), ngành học và trường đại học.
- Hai anh em ở cùng một phòng trọ hay ký túc xá; ngân sách sinh hoạt mở đầu; mức lương trong game.
- Ai sinh trước, các sắc thái tính cách, tên chính thức của cửa hàng và danh sách khách hàng.
- Mức độ trưởng thành của nội dung, các tuyến tình cảm và phân loại độ tuổi khi phát hành phải chốt theo nội dung thực tế.
- **[ĐỀ XUẤT]** Mở đầu bằng cảnh nhận hóa đơn tiền thuê nhà tại phòng trọ rồi cùng lên đường đến ca làm đầu tiên, giúp thiết lập mục tiêu nhanh mà không phải xây thêm map lớn.
