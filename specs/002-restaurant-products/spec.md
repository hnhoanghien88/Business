# Feature Specification: Quản lý món ăn và biến thể

**Feature Branch**: `[002-restaurant-products]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Foods`

**Menu**: Code `foods` · Name `Foods` · Route `/restaurant/foods`

**Database Tables**: `restaurant_foods`, `restaurant_food_variants`, `restaurant_food_price_histories`

**Input**: User description: "Tạo chức năng menu Món ăn & biến thể theo bản đồ chức năng nhà hàng, kế thừa hành vi Product hiện có và mở rộng thành catalog thực tế."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Tra cứu catalog món ăn (Priority: P1)

Người quản lý thực đơn xem, tìm và lọc món theo nhóm, trạng thái hoạt động và khả năng bán để nhanh chóng xác định món cần thao tác.

**Why this priority**: Đây là điểm vào chính để kiểm soát catalog đang được dùng cho gọi món.

**Independent Test**: Chuẩn bị catalog có nhiều nhóm và trạng thái, tìm/lọc rồi xác nhận danh sách, tổng kết quả và thông tin giá/biến thể chính xác.

**Acceptance Scenarios**:

1. **Given** người dùng có quyền xem, **When** mở màn hình, **Then** danh sách hiển thị ảnh, mã, tên, nhóm, biến thể, khoảng giá và trạng thái.
2. **Given** có nhiều món, **When** tìm theo mã/tên hoặc lọc theo nhóm/trạng thái, **Then** chỉ kết quả phù hợp xuất hiện và danh sách về trang đầu.
3. **Given** không có kết quả hoặc tải lỗi, **When** tải hoàn tất, **Then** trạng thái rỗng hoặc lỗi có thể thử lại được hiển thị rõ ràng.

---

### User Story 2 - Tạo và cập nhật món (Priority: P1)

Người dùng được cấp quyền tạo món trong một nhóm và cập nhật thông tin trình bày của món mà không làm thay đổi lịch sử giao dịch.

**Why this priority**: Catalog chỉ có giá trị khi nhà hàng có thể duy trì thông tin món chính xác.

**Independent Test**: Tạo món hợp lệ, sửa tên/mô tả/ảnh/thứ tự, tải lại và xác nhận dữ liệu mới trong quản trị trong khi snapshot order cũ không đổi.

**Acceptance Scenarios**:

1. **Given** nhóm món đang hoạt động, **When** lưu món có mã và tên hợp lệ, **Then** món được tạo đúng một lần trong nhóm đã chọn.
2. **Given** mã trùng hoặc nhóm không còn hoạt động, **When** lưu, **Then** hệ thống từ chối và chỉ rõ nguyên nhân.
3. **Given** món đã tồn tại, **When** cập nhật dữ liệu hợp lệ, **Then** thay đổi xuất hiện trong catalog mới nhưng lịch sử order giữ snapshot cũ.
4. **Given** dữ liệu đã bị cập nhật nơi khác, **When** lưu bản cũ, **Then** hệ thống ngăn ghi đè và yêu cầu tải lại.

---

### User Story 3 - Quản lý biến thể và giá (Priority: P1)

Người quản lý cấu hình một hoặc nhiều biến thể, chọn biến thể mặc định, đổi giá và xem lịch sử giá của từng biến thể.

**Why this priority**: Biến thể và giá là dữ liệu bắt buộc để bán món và tính tiền chính xác.

**Independent Test**: Tạo hai biến thể, chọn một mặc định, đổi giá một biến thể và xác nhận giá hiện tại cùng lịch sử hiệu lực chính xác.

**Acceptance Scenarios**:

1. **Given** một món hoạt động, **When** tạo biến thể hợp lệ, **Then** biến thể xuất hiện đúng thứ tự và có giá hiện tại.
2. **Given** món có nhiều biến thể hoạt động, **When** đổi biến thể mặc định, **Then** chỉ một biến thể hoạt động là mặc định.
3. **Given** người dùng có quyền đổi giá, **When** nhập giá mới hợp lệ, **Then** giá hiện tại thay đổi và lịch sử giá cũ/mới vẫn tra cứu được.
4. **Given** người dùng chỉ có quyền cập nhật món, **When** xem chi tiết, **Then** không có quyền đổi giá nếu thiếu quyền chuyên biệt.

---

### User Story 4 - Quản lý còn/hết món (Priority: P2)

Người dùng được cấp quyền đánh dấu một biến thể hết món hoặc bán lại và cung cấp lý do để các màn hình gọi món phản ánh đúng.

**Why this priority**: Nhân viên phải ngừng nhận món không thể phục vụ mà không xóa cấu hình catalog.

**Independent Test**: Đánh dấu hết món, xác nhận lý do và việc không thể chọn trên order screen; sau đó bán lại và xác nhận có thể chọn lại.

**Acceptance Scenarios**:

1. **Given** biến thể đang bán, **When** đánh dấu hết món với lý do, **Then** order screen hiển thị hết món và không cho chọn mới.
2. **Given** biến thể hết món, **When** đánh dấu bán lại, **Then** lý do hết món được xóa và biến thể có thể được chọn nếu vẫn hoạt động.
3. **Given** món/biến thể đã có lịch sử, **When** ngừng hoạt động, **Then** dữ liệu lịch sử được giữ và đối tượng không còn dùng cho giao dịch mới.

### Edge Cases

- Món chưa có biến thể hoặc có nhiều biến thể nhưng không có mặc định.
- Giá bằng không, giá âm hoặc vượt giới hạn tiền tệ.
- Hai người đồng thời đặt hai biến thể khác nhau làm mặc định.
- Món chuyển sang nhóm đã ngừng hoạt động.
- URL ảnh không hợp lệ hoặc ảnh không tải được.
- Giá hoặc availability thay đổi trong lúc món đang nằm trong giỏ hàng.
- Biến thể mặc định bị ngừng hoạt động.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hệ thống MUST giới hạn xem và thao tác catalog theo quyền hiện hành.
- **FR-002**: Hệ thống MUST cung cấp tìm kiếm, lọc theo nhóm/hoạt động/khả dụng và phân trang danh sách món.
- **FR-003**: Mỗi món MUST có mã duy nhất không phân biệt chữ hoa/chữ thường, tên, nhóm, thứ tự và trạng thái.
- **FR-004**: Mã món MUST bất biến sau khi tạo; tên và thông tin trình bày có thể cập nhật bởi người có quyền.
- **FR-005**: Món mới MUST thuộc một nhóm đang hoạt động.
- **FR-006**: Ngừng hoạt động món MUST loại toàn bộ biến thể khỏi giao dịch mới nhưng giữ lịch sử.
- **FR-007**: Mỗi biến thể MUST có mã duy nhất trong phạm vi món, tên, giá hiện tại, thứ tự, trạng thái hoạt động và khả dụng.
- **FR-008**: Mỗi món có biến thể hoạt động MUST có đúng một biến thể mặc định hoạt động.
- **FR-009**: Hệ thống MUST ngăn ngừng hoạt động biến thể mặc định nếu chưa chọn mặc định thay thế, trừ khi toàn bộ món bị ngừng.
- **FR-010**: Giá MUST không âm và MUST chỉ được thay đổi qua hành động đổi giá dành riêng cho người có quyền.
- **FR-011**: Mỗi lần đổi giá MUST giữ được giá, thời điểm bắt đầu/kết thúc và người thực hiện để tra cứu lịch sử.
- **FR-012**: Giá trên giao dịch đã tạo MUST không thay đổi khi catalog đổi giá sau đó.
- **FR-013**: `active` và `available` MUST là hai trạng thái độc lập với ý nghĩa cấu hình và khả năng bán hiện tại.
- **FR-014**: Biến thể hết món MUST không được chọn cho giao dịch mới và SHOULD có lý do dễ hiểu cho nhân viên bán hàng.
- **FR-015**: Thay đổi availability MUST được phản ánh cho các màn hình gọi món đang hoạt động mà không yêu cầu đăng nhập lại.
- **FR-016**: Hệ thống MUST phát hiện cập nhật đồng thời ảnh hưởng đến mặc định, giá hoặc trạng thái và không ghi đè âm thầm.
- **FR-017**: Menu và permission MUST dùng Resource `Foods`; các permission `Product.*` cũ MUST được migrate hoặc ánh xạ tương thích có thời hạn trong giai đoạn chuyển đổi.
- **FR-018**: Các route cũ `/product` và `/restaurant/products` MUST đưa người dùng hợp lệ đến route canonical `/restaurant/foods` mà không tạo menu trùng.
- **FR-019**: Form và hành động MUST có trạng thái tải/lưu/lỗi/xác nhận, ngăn submit trùng và dùng được bằng bàn phím.
- **FR-020**: Hard delete món/biến thể đã được tham chiếu bởi giao dịch MUST nằm ngoài phạm vi; hệ thống dùng ngừng hoạt động.

### Key Entities

- **Món ăn (Food)**: Mục catalog thuộc một nhóm, chứa thông tin trình bày và nhiều biến thể.
- **Biến thể món (Food Variant)**: Lựa chọn bán cụ thể với giá hiện tại, mặc định và availability.
- **Lịch sử giá (Price History)**: Khoảng hiệu lực của từng mức giá của biến thể.
- **Nhóm món (Category)**: Phân loại đã được quản lý bởi feature phụ thuộc.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% người dùng tìm được món cần quản lý trong dưới 20 giây với 10.000 món.
- **SC-002**: 95% thao tác tạo món cùng biến thể mặc định hoàn tất trong dưới 2 phút.
- **SC-003**: 100% món có biến thể hoạt động duy trì đúng một mặc định sau mọi thao tác hợp lệ.
- **SC-004**: 100% giao dịch lịch sử giữ nguyên tên và giá snapshot sau khi catalog thay đổi.
- **SC-005**: 100% biến thể hết món bị chặn khỏi giao dịch mới chậm nhất sau lần đồng bộ trạng thái kế tiếp.
- **SC-006**: 100% thay đổi giá tạo lịch sử liên tục, không chồng lấn hoặc mất khoảng hiệu lực.
- **SC-007**: 100% hành động bị giới hạn quyền bị ẩn khỏi giao diện và bị từ chối khi gọi trực tiếp.

## Assumptions

- Feature Category đã tồn tại và cung cấp nhóm đang hoạt động.
- MVP chỉ đổi giá có hiệu lực ngay; giá lên lịch trong tương lai nằm ngoài phạm vi.
- Upload/lưu trữ ảnh riêng chưa nằm trong phạm vi; món lưu tham chiếu ảnh hợp lệ và có fallback khi ảnh lỗi.
- Modifier, topping, combo, công thức nguyên liệu và tồn kho nằm ngoài phạm vi.
- Product mẫu hiện có được phát triển tiếp hoặc migrate, không tồn tại song song như một catalog thứ hai.
