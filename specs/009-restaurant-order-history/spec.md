# Feature Specification: Tra cứu đơn hàng

**Feature Branch**: `[009-restaurant-order-history]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Orders`

**Menu**: Code `orders` · Name `Orders` · Route `/restaurant/orders`

**Database Tables**: `restaurant_orders`, `restaurant_order_items`, `restaurant_order_promotions`, `restaurant_order_status_histories`, `restaurant_kitchen_orders`, `restaurant_kitchen_order_items`, `restaurant_payment_allocations`

**Input**: User description: "Tạo menu Đơn hàng gồm danh sách, chi tiết snapshot, bếp, thanh toán và timeline trạng thái; hỗ trợ hủy có kiểm soát."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Tìm và lọc đơn hàng (Priority: P1)

Người có quyền tra cứu order theo thời gian, số đơn, bàn, trạng thái, loại và người tạo để hỗ trợ vận hành/đối soát.

**Why this priority**: Tra cứu là giá trị cơ bản và không phụ thuộc action thay đổi.

**Independent Test**: Chuẩn bị dữ liệu nhiều ngày/trạng thái, áp filter và xác nhận kết quả, sort, pagination.

**Acceptance Scenarios**:

1. **Given** user có quyền đọc, **When** mở menu, **Then** danh sách hiển thị order no, bàn/session, thời gian, số món, total, paid và status.
2. **Given** filter/search, **When** áp dụng, **Then** kết quả đúng và điều kiện được giữ khi mở chi tiết rồi quay lại.
3. **Given** dữ liệu lớn, **When** chuyển trang/sort, **Then** kết quả không trùng hoặc bỏ sót trong snapshot truy vấn.

---

### User Story 2 - Xem hồ sơ đầy đủ của order (Priority: P1)

Người dùng mở chi tiết để xem snapshot item, promotion, kitchen tickets, payment allocations và timeline status.

**Why this priority**: Một hồ sơ tổng hợp giúp giải quyết thắc mắc mà không ghép dữ liệu thủ công từ nhiều màn hình.

**Independent Test**: Mở order đã qua đầy đủ workflow và xác nhận các tab/số liệu/timeline khớp giao dịch gốc.

**Acceptance Scenarios**:

1. **Given** order tồn tại, **When** mở chi tiết, **Then** header/totals/items snapshot được hiển thị.
2. **Given** order có nhiều ticket/payment, **When** xem tabs, **Then** mọi liên kết và trạng thái đúng theo order.
3. **Given** catalog/promotion đã đổi tên/giá, **When** xem order cũ, **Then** snapshot lịch sử vẫn được hiển thị.

---

### User Story 3 - Theo dõi timeline trạng thái (Priority: P2)

Quản lý xem order đã chuyển trạng thái khi nào, bởi ai và lý do nào để audit sự cố.

**Why this priority**: Timeline cần cho kiểm soát nhưng danh sách/chi tiết có thể hoạt động trước.

**Independent Test**: Chuyển order qua nhiều trạng thái và xác nhận timeline đầy đủ, đúng thứ tự.

**Acceptance Scenarios**:

1. **Given** order có lịch sử, **When** xem timeline, **Then** from/to status, actor, time và reason/note được hiển thị theo thời gian.
2. **Given** transition tự động, **When** xem, **Then** nguồn hệ thống được phân biệt với actor người dùng.

---

### User Story 4 - Hủy order/item có kiểm soát (Priority: P2)

Người có quyền hủy order hoặc item còn hợp lệ với lý do; item đã vào chế biến cần quyền quản lý/override.

**Why this priority**: Sai món và khách đổi ý cần xử lý nhưng không được phá bếp/thanh toán.

**Independent Test**: Hủy item Pending và item Preparing bằng các quyền khác nhau, xác nhận totals/status/kitchen quantity/promotion được reconcile và audit.

**Acceptance Scenarios**:

1. **Given** item chưa chế biến và chưa thanh toán, **When** hủy với lý do, **Then** item Cancelled và totals được tính lại theo policy.
2. **Given** item Preparing/Ready, **When** user thường cố hủy, **Then** bị từ chối; manager override được khi cung cấp lý do.
3. **Given** order có Payment Paid, **When** hủy làm giảm total dưới Paid, **Then** hệ thống từ chối và hướng dẫn quy trình refund/manager resolution.

### Edge Cases

- Order không có TableSession (loại đơn tương lai).
- Snapshot field khác catalog hiện tại hoặc đối tượng catalog đã deactivate.
- Timeline có nhiều transition cùng timestamp.
- Payment allocation bị Pending/Failed/Refunded.
- Hủy item đã chia qua nhiều kitchen tickets.
- User truy cập URL order không tồn tại hoặc không có quyền.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Menu/order data MUST giới hạn theo permission đọc.
- **FR-002**: Danh sách MUST hỗ trợ filter khoảng thời gian, order no, bàn, status, type, creator; sort và pagination.
- **FR-003**: Khoảng thời gian MUST hiển thị timezone/kỳ lọc rõ ràng.
- **FR-004**: Danh sách MUST hiển thị total và Paid từ nguồn khác nhau, không dùng total như tiền đã thu.
- **FR-005**: Chi tiết MUST hiển thị OrderItem snapshot, promotion snapshot, kitchen tickets/items, payment allocations và status timeline.
- **FR-006**: Dữ liệu lịch sử MUST dùng snapshot đã lưu thay vì thay thế bằng catalog/promotion hiện tại.
- **FR-007**: Timeline MUST ghi mọi transition order với from/to, actor/source, time và note/reason phù hợp.
- **FR-008**: Hủy order/item MUST yêu cầu permission và lý do không rỗng.
- **FR-009**: Hủy sau Preparing/Ready MUST yêu cầu override riêng.
- **FR-010**: Hủy MUST reconcile totals, promotion eligibility/quota và kitchen quantities như một thay đổi nhất quán.
- **FR-011**: Không cho hủy nếu làm nghĩa vụ tài chính thấp hơn Paid mà không có quy trình resolution được phép.
- **FR-012**: Cancelled items MUST không tính vào số món/doanh thu bán nhưng vẫn hiển thị trong lịch sử.
- **FR-013**: Người dùng quay lại danh sách MUST giữ filter/sort/page trước đó.
- **FR-014**: Trạng thái loading/empty/error/forbidden/not-found MUST phân biệt rõ.
- **FR-015**: Detail tabs/cards MUST responsive và thao tác bằng bàn phím.
- **FR-016**: Export nằm ngoài MVP trừ khi được cấp permission và định dạng/phạm vi riêng trong plan sau.

### Key Entities

- **Order**: Header lần gọi món và totals/status.
- **OrderItem Snapshot**: Chi tiết lịch sử món/giá/quantity.
- **OrderStatusHistory**: Timeline transition.
- **KitchenOrder/Item**: Tiến trình chế biến liên quan.
- **PaymentAllocation**: Tiền Paid/Pending phân bổ vào order.
- **OrderPromotion**: Snapshot khuyến mãi.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% người dùng tìm một order theo số trong dưới 15 giây với 1 triệu order.
- **SC-002**: 95% chi tiết order đầy đủ sẵn sàng để xem trong dưới 2 giây ở điều kiện bình thường.
- **SC-003**: 100% order cũ hiển thị đúng snapshot dù catalog/promotion thay đổi.
- **SC-004**: 100% transition order xuất hiện trong timeline audit.
- **SC-005**: 100% hủy/override có actor, time, reason và không tạo totals/payment không nhất quán.
- **SC-006**: 100% user thiếu quyền nhận forbidden và không thấy action nhạy cảm.

## Assumptions

- MVP mặc định tìm trong khoảng ngày gần đây; user có thể đổi phạm vi.
- Export, invoice và refund workflow là feature sau.
- Hủy order/item chỉ dành cho vận hành ngoại lệ, không thay thế refund.
- Dữ liệu DineIn là trọng tâm nhưng danh sách có thể hiển thị type khác nếu đã tồn tại.
