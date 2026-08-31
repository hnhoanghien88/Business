# Feature Specification: Thanh toán gộp lượt bàn

**Feature Branch**: `[008-restaurant-payment]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Payments`

**Menu**: Code `payments` · Name `Payments` · Route `/restaurant/payments`

**Database Tables**: `restaurant_payments`, `restaurant_payment_allocations`, `restaurant_table_sessions`, `restaurant_orders`

**Database Views**: `restaurant_order_payment_balances`, `restaurant_table_session_payment_balances`

**Input**: User description: "Tạo menu hàng đợi thu ngân và workspace thanh toán theo TableSession, hỗ trợ nhiều order, nhiều payment và phân bổ payment vào order."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Xem hàng đợi thu ngân (Priority: P1)

Thu ngân xem các lượt bàn đang chờ/thiếu thanh toán, tìm theo bàn và nhận biết tổng phải trả, đã thu, còn lại và payment pending.

**Why this priority**: Thu ngân cần chọn đúng lượt bàn trước khi thu tiền.

**Independent Test**: Chuẩn bị session có nhiều trạng thái thanh toán, lọc hàng đợi và xác nhận số liệu không đếm trùng.

**Acceptance Scenarios**:

1. **Given** user có quyền, **When** mở menu Thanh toán, **Then** hàng đợi hiển thị bàn/session, giờ mở, tổng, paid, remaining và trạng thái.
2. **Given** session có nhiều order/payment, **When** xem hàng đợi, **Then** số tổng hợp toàn session chính xác.
3. **Given** tìm/lọc, **When** chọn một dòng, **Then** workspace đúng session được mở.

---

### User Story 2 - Thu một hoặc nhiều phương thức (Priority: P1)

Thu ngân ghi nhận Cash/Card/BankTransfer/Momo/VNPay/Other cho toàn lượt bàn và có thể chia thành nhiều payment.

**Why this priority**: Đây là nghiệp vụ thu tiền cốt lõi.

**Independent Test**: Thanh toán session 500.000 bằng hai payment 200.000 và 300.000; xác nhận session/order balances và lịch sử.

**Acceptance Scenarios**:

1. **Given** session còn tiền phải thu, **When** tạo payment hợp lệ, **Then** payment thuộc session và được phân bổ vào order chưa đủ theo thứ tự ổn định.
2. **Given** khách trả nhiều phương thức, **When** tạo nhiều payment, **Then** tổng Paid và remaining cập nhật sau từng lần.
3. **Given** Cash khách đưa lớn hơn remaining, **When** xác nhận, **Then** payment amount chỉ bằng phần cần thu và tiền thừa được hiển thị.
4. **Given** cùng giao dịch được retry, **When** xử lý lại, **Then** không ghi nhận tiền hai lần.

---

### User Story 3 - Theo dõi payment điện tử Pending/Failed (Priority: P1)

Thu ngân theo dõi giao dịch điện tử chưa có kết quả, xác nhận thành công hoặc thất bại an toàn mà không coi Pending là tiền đã thu.

**Why this priority**: Kết quả không đồng bộ dễ gây thu thiếu hoặc ghi nhận trùng.

**Independent Test**: Tạo payment Pending, xác nhận chưa tăng Paid; chuyển Paid một lần và xác nhận allocation/balance; gửi callback trùng không tăng lần hai.

**Acceptance Scenarios**:

1. **Given** payment Pending, **When** xem session, **Then** khoản đó tách biệt và không tính vào Paid.
2. **Given** xác nhận thành công hợp lệ, **When** chuyển Paid, **Then** allocations được tính một lần.
3. **Given** Failed/Cancelled, **When** xem balance, **Then** amount không được tính và user có thể tạo payment mới.

---

### User Story 4 - Hoàn tất order và đóng lượt (Priority: P2)

Sau khi từng order được phân bổ đủ và không còn điều kiện bếp/phục vụ chặn, hệ thống hoàn tất order; khi toàn session đủ điều kiện, thu ngân/nhân viên đóng lượt.

**Why this priority**: Vòng đời bàn chỉ kết thúc khi nghĩa vụ tài chính chính xác.

**Independent Test**: Thanh toán đủ nhiều order, xác nhận từng order Completed, session có thể đóng và bàn Cleaning.

**Acceptance Scenarios**:

1. **Given** allocation Paid của order bằng total và điều kiện khác đủ, **When** payment hoàn tất, **Then** order Completed và history được ghi.
2. **Given** session còn một order thiếu tiền, **When** cố đóng, **Then** hệ thống từ chối với remaining rõ ràng.
3. **Given** toàn bộ order đủ, **When** đóng session, **Then** session Closed và bàn Cleaning.

### Edge Cases

- Payment amount bằng 0, âm hoặc vượt remaining.
- Hai thu ngân đồng thời thanh toán phần remaining cuối.
- Payment Paid nhưng allocations thiếu/không bằng amount.
- Allocation trỏ tới order của session khác.
- Transaction number trùng, callback đến trễ/sai thứ tự.
- Order total thay đổi sau khi đã có payment (phải bị hạn chế/reconcile).
- Refund yêu cầu sau Paid.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Menu Thanh toán MUST hiển thị hàng đợi session theo quyền và hỗ trợ tìm/filter trạng thái.
- **FR-002**: Balance session MUST bằng tổng order không Cancelled trừ tổng payment Paid, không đếm Pending/Failed/Refunded/Cancelled.
- **FR-003**: Payment MUST thuộc một TableSession và MAY có nhiều allocations vào order của chính session đó.
- **FR-004**: Tổng allocations của payment khi Paid MUST bằng payment amount.
- **FR-005**: Tổng allocations Paid vào mỗi order MUST không vượt Order.TotalAmount.
- **FR-006**: Hệ thống MUST hỗ trợ nhiều payment và phương thức Cash, Card, BankTransfer, Momo, VNPay, Other.
- **FR-007**: Payment amount MUST dương và không vượt session remaining, trừ policy riêng được phê duyệt ngoài MVP.
- **FR-008**: Tiền khách đưa và tiền thừa MAY hiển thị cho Cash nhưng amount ghi nhận MUST là phần thực thu áp dụng.
- **FR-009**: Chỉ Paid MUST tính vào thực thu và order/session paid balance.
- **FR-010**: Transaction number/idempotency identifier MUST ngăn cùng giao dịch được xác nhận nhiều lần.
- **FR-011**: Tạo/confirm payment, allocations và cập nhật trạng thái liên quan MUST không để lại kết quả một phần.
- **FR-012**: Cạnh tranh thanh toán MUST được phát hiện; chỉ một request được dùng remaining cuối cùng.
- **FR-013**: Order chỉ Completed khi allocation Paid đủ và các điều kiện hoàn tất nghiệp vụ khác được đáp ứng.
- **FR-014**: Session chỉ đóng khi mọi order không Cancelled đã đủ điều kiện; đóng chuyển bàn Cleaning.
- **FR-015**: Không cho sửa/xóa Payment Paid trực tiếp; refund là feature riêng ngoài phạm vi.
- **FR-016**: Payment history MUST hiển thị method, amount, status, transaction, actor và timestamps phù hợp.
- **FR-017**: Permission MUST phân biệt xem, tạo, confirm, cancel và override.
- **FR-018**: Workspace MUST giữ summary/CTA rõ ràng, ngăn submit trùng và hoạt động trên desktop/mobile/keyboard.

### Key Entities

- **TableSession**: Đối tượng nhận thanh toán gộp cho nhiều order.
- **Payment**: Một lần thu tiền với phương thức và trạng thái.
- **PaymentAllocation**: Phần payment được phân bổ cho một order.
- **Order Payment Balance**: Total, Paid và Remaining của từng order.
- **Session Payment Balance**: Tổng hợp nghĩa vụ toàn lượt bàn.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% thu ngân hoàn thành thanh toán tiền mặt một session trong dưới 45 giây.
- **SC-002**: 100% payment Paid có tổng allocation bằng amount và chỉ gồm order cùng session.
- **SC-003**: 100% cạnh tranh/retry không tạo thu trùng hoặc allocation vượt total.
- **SC-004**: 100% Pending/Failed không được tính vào thực thu.
- **SC-005**: 100% session còn nghĩa vụ bị chặn đóng với lý do và remaining chính xác.
- **SC-006**: 100% số liệu hàng đợi đối soát được với lịch sử payment/order.

## Assumptions

- MVP thanh toán gộp toàn session nhưng vẫn phân bổ theo order.
- Allocation mặc định ưu tiên order cũ nhất chưa đủ; người dùng không phân bổ thủ công trong MVP.
- Tip, split bill theo khách, invoice, refund chi tiết và gateway settlement ngoài phạm vi.
- Tax/service charge bằng giá trị đã chốt trong order; payment không tính lại total.
