# Feature Specification: Vận hành sơ đồ bàn

**Feature Branch**: `[004-restaurant-table-operations]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `TableOperations`

**Menu**: Code `table-operations` · Name `Table operations` · Route `/restaurant/table-operations`

**Database Tables**: `restaurant_areas`, `restaurant_tables`, `restaurant_table_sessions`

**Input**: User description: "Tạo chức năng menu Sơ đồ bàn gồm xem trạng thái, mở bàn, xem lượt bàn, đóng lượt và xác nhận dọn xong."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Theo dõi trạng thái bàn (Priority: P1)

Nhân viên phục vụ xem bàn theo khu vực và nhận biết tức thời bàn trống, có khách, đang dọn hoặc bị khóa.

**Why this priority**: Nhân viên cần một nguồn trạng thái chung trước mọi thao tác phục vụ.

**Independent Test**: Chuẩn bị bàn ở từng trạng thái, mở sơ đồ và xác nhận card, filter, thông tin session và cập nhật trạng thái chính xác.

**Acceptance Scenarios**:

1. **Given** người dùng có quyền xem, **When** mở sơ đồ, **Then** bàn được nhóm theo khu vực với tên, sức chứa, trạng thái và thông tin session hiện hành.
2. **Given** trạng thái bàn thay đổi từ nơi khác, **When** màn hình nhận đồng bộ hoặc kết nối lại, **Then** card phản ánh trạng thái mới mà không nhân đôi session.
3. **Given** tải lỗi, **When** mở màn hình, **Then** người dùng thấy trạng thái lỗi/thử lại và trạng thái kết nối rõ ràng.

---

### User Story 2 - Mở bàn (Priority: P1)

Nhân viên chọn bàn Available, nhập số khách và ghi chú để bắt đầu một lượt phục vụ.

**Why this priority**: Mở bàn tạo ngữ cảnh bắt buộc cho gọi món tại chỗ.

**Independent Test**: Mở một bàn Available và xác nhận đúng một session Open được tạo, bàn chuyển Occupied và có thể đi đến gọi món.

**Acceptance Scenarios**:

1. **Given** bàn Available, **When** nhập số khách dương và xác nhận, **Then** bàn chuyển Occupied với một session Open duy nhất.
2. **Given** số khách vượt sức chứa, **When** mở bàn, **Then** hệ thống cảnh báo và chỉ người có quyền override được tiếp tục với lý do.
3. **Given** hai nhân viên cùng mở một bàn, **When** yêu cầu được xử lý, **Then** chỉ một yêu cầu thành công và người còn lại nhận trạng thái mới.
4. **Given** bàn Disabled/Occupied/Cleaning, **When** cố mở, **Then** hệ thống từ chối.

---

### User Story 3 - Xem và tiếp tục phục vụ lượt bàn (Priority: P1)

Nhân viên mở chi tiết bàn Occupied để xem số khách, thời gian, các order, tổng phải trả/đã thu/còn lại và đi tới gọi thêm hoặc thanh toán.

**Why this priority**: Lượt bàn là đầu mối phối hợp gọi món, bếp và thu ngân.

**Independent Test**: Mở chi tiết một session có nhiều order và payment, xác nhận tổng và các CTA đúng theo quyền/trạng thái.

**Acceptance Scenarios**:

1. **Given** bàn Occupied, **When** mở chi tiết, **Then** thông tin session và số liệu tổng hợp của toàn lượt được hiển thị.
2. **Given** user có quyền gọi món/thanh toán, **When** xem chi tiết, **Then** CTA tương ứng điều hướng đúng session.
3. **Given** session vừa đóng nơi khác, **When** user thao tác, **Then** hệ thống chặn thay đổi và tải trạng thái mới.

---

### User Story 4 - Đóng lượt và hoàn tất dọn bàn (Priority: P2)

Nhân viên đóng session khi tất cả nghĩa vụ xử lý/thanh toán hoàn tất, sau đó xác nhận dọn xong để trả bàn về Available.

**Why this priority**: Bàn phải quay lại vòng đời sẵn sàng một cách an toàn.

**Independent Test**: Đóng session đủ điều kiện, xác nhận bàn Cleaning; đánh dấu dọn xong và xác nhận Available.

**Acceptance Scenarios**:

1. **Given** mọi order đủ điều kiện và không còn tiền phải thu, **When** đóng lượt, **Then** session Closed và bàn Cleaning.
2. **Given** còn order/payment chưa hoàn tất, **When** đóng lượt, **Then** hệ thống từ chối và nêu rõ mục còn thiếu.
3. **Given** người có quyền override, **When** đóng ngoại lệ với lý do, **Then** thao tác và lý do được lưu để audit.
4. **Given** bàn Cleaning, **When** xác nhận dọn xong, **Then** bàn Available.

### Edge Cases

- Không có khu vực hoặc không có bàn hoạt động.
- Session Open tồn tại nhưng trạng thái bàn không phải Occupied.
- Người dùng mất kết nối khi mở/đóng bàn rồi thử lại.
- Số khách bằng 0, âm hoặc rất lớn.
- Bàn bị disable giữa lúc dialog mở bàn đang hiển thị.
- Event trạng thái đến sai thứ tự hoặc bị bỏ lỡ.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hệ thống MUST hiển thị sơ đồ card theo khu vực và hỗ trợ tìm/lọc trạng thái.
- **FR-002**: Card MUST truyền đạt trạng thái bằng nhãn và hình thức không chỉ dựa vào màu.
- **FR-003**: Bàn Occupied MUST hiển thị tối thiểu số khách, giờ mở và thời lượng.
- **FR-004**: Hệ thống MUST cập nhật trạng thái bàn/session đang hoạt động và tải lại snapshot sau khi kết nối lại.
- **FR-005**: Chỉ bàn Available hoạt động mới được mở.
- **FR-006**: Mở bàn MUST tạo đúng một session Open và chuyển bàn Occupied như một thao tác không thể hoàn tất một phần.
- **FR-007**: Mỗi bàn MUST có tối đa một session Open.
- **FR-008**: Guest count MUST lớn hơn 0; vượt capacity MUST cần cảnh báo và permission override kèm lý do.
- **FR-009**: Chi tiết session MUST tổng hợp mọi order, tổng phải trả, payment Paid và số còn lại mà không đếm trùng.
- **FR-010**: CTA gọi món/thanh toán/đóng bàn MUST phản ánh permission và trạng thái hiện hành.
- **FR-011**: Session chỉ được đóng thông thường khi mọi order và payment đáp ứng chính sách hoàn tất.
- **FR-012**: Đóng session MUST chuyển bàn Cleaning; không chuyển thẳng Available.
- **FR-013**: Chỉ bàn Cleaning mới được MarkClean để chuyển Available.
- **FR-014**: Override MUST yêu cầu permission riêng, lý do không rỗng và audit actor/time/reason.
- **FR-015**: Các command MUST chống xử lý trùng khi retry và phát hiện trạng thái lỗi thời.
- **FR-016**: Menu/route/action MUST lấy từ authorization của Identity Application code `restaurant`.
- **FR-017**: Layout MUST dùng toàn chiều rộng workspace, thích ứng sidebar/drawer và thao tác được bằng bàn phím.

### Key Entities

- **Bàn**: Tài nguyên vật lý với trạng thái Available, Occupied, Cleaning hoặc Disabled trong MVP.
- **Lượt bàn**: Một lần khách dùng bàn, gồm số khách, thời điểm và nhiều order/payment.
- **Khu vực**: Nhóm hiển thị bàn trên sơ đồ.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% nhân viên xác định đúng bàn Available trong dưới 10 giây với 300 bàn.
- **SC-002**: 95% thao tác mở bàn hợp lệ hoàn tất trong dưới 15 giây.
- **SC-003**: 100% cạnh tranh mở bàn chỉ tạo một session Open.
- **SC-004**: 100% lượt đóng thường bị chặn khi còn nghĩa vụ chưa hoàn tất.
- **SC-005**: 99% thay đổi trạng thái được phản ánh trên màn hình đang mở trong dưới 3 giây ở điều kiện bình thường.
- **SC-006**: 100% override có actor, thời gian và lý do tra cứu được.

## Assumptions

- Reservation/Reserved, chuyển/gộp bàn và nhiều chi nhánh nằm ngoài MVP.
- Thời lượng hiển thị tính từ thời điểm mở session đến hiện tại.
- Gọi món và thanh toán là feature riêng được mở theo session.
- Sau reconnect, snapshot hiện tại luôn thắng event cũ.
