# Feature Specification: Dashboard nhà hàng

**Feature Branch**: `[010-restaurant-dashboard]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Dashboard`

**Menu**: Code `dashboard` · Name `Dashboard` · Route `/restaurant/dashboard`

**Database Tables**: `restaurant_areas`, `restaurant_tables`, `restaurant_table_sessions`, `restaurant_orders`, `restaurant_order_items`, `restaurant_kitchen_orders`, `restaurant_kitchen_order_items`, `restaurant_payments`, `restaurant_payment_allocations`

**Database Views**: `restaurant_order_item_kitchen_quantities`, `restaurant_order_payment_balances`, `restaurant_table_session_payment_balances`

**Input**: User description: "Tạo menu Dashboard KPI bán hàng, thực thu, khách, top món/bàn và trạng thái vận hành theo khoảng thời gian."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Xem KPI kinh doanh (Priority: P1)

Quản lý xem số order hoàn tất, số món bán, gross sales, discount, net sales, thực thu, số khách và giá trị order trung bình trong kỳ.

**Why this priority**: Đây là giá trị chính để hiểu kết quả kinh doanh nhanh.

**Independent Test**: Chuẩn bị order/payment/session đã biết, chọn hôm nay và xác nhận từng KPI theo định nghĩa.

**Acceptance Scenarios**:

1. **Given** user có quyền báo cáo, **When** mở Dashboard, **Then** KPI mặc định hôm nay theo timezone nhà hàng được hiển thị.
2. **Given** đổi kỳ hôm nay/7 ngày/tháng/tùy chọn, **When** áp dụng, **Then** mọi widget dùng cùng from/to/timezone.
3. **Given** không có dữ liệu, **When** tải hoàn tất, **Then** KPI hiển thị 0 và widget có trạng thái rỗng phù hợp.

---

### User Story 2 - Phân tích xu hướng và top hiệu suất (Priority: P1)

Quản lý xem xu hướng doanh thu/thực thu và top món, biến thể, bàn để ra quyết định vận hành.

**Why this priority**: KPI tổng chưa giải thích nguồn tạo ra kết quả.

**Independent Test**: Chuẩn bị giao dịch nhiều ngày/món/bàn, xác nhận series và ranking theo kỳ.

**Acceptance Scenarios**:

1. **Given** kỳ nhiều ngày, **When** xem biểu đồ, **Then** gross/net/paid được nhóm theo đơn vị thời gian phù hợp và tổng khớp KPI.
2. **Given** item Cancelled, **When** tính top món, **Then** item đó không được tính bán.
3. **Given** nhiều order trong một session, **When** tính khách, **Then** guest count chỉ được tính một lần cho session.

---

### User Story 3 - Theo dõi trạng thái vận hành (Priority: P2)

Quản lý xem số bàn theo trạng thái và số phiếu bếp chờ/quá thời gian để can thiệp kịp thời.

**Why this priority**: Kết hợp tài chính với vận hành giúp dashboard hữu ích trong ca hiện tại.

**Independent Test**: Chuẩn bị bàn/phiếu bếp trạng thái khác nhau, xác nhận counters và điều hướng đến màn hình nghiệp vụ nếu có quyền.

**Acceptance Scenarios**:

1. **Given** bàn đang hoạt động, **When** xem dashboard, **Then** counts Available/Occupied/Cleaning/Disabled chính xác.
2. **Given** kitchen items quá SLA, **When** xem trạng thái bếp, **Then** số chờ/quá giờ chính xác và được làm nổi bật.
3. **Given** user thiếu quyền màn hình đích, **When** xem widget, **Then** không cung cấp điều hướng bị cấm.

---

### User Story 4 - Xử lý dữ liệu lỗi từng phần (Priority: P2)

Quản lý vẫn xem được widget thành công khi một phần dashboard lỗi và có thể thử lại phần thất bại.

**Why this priority**: Dashboard tổng hợp nhiều nguồn; một lỗi không nên làm mất toàn bộ thông tin.

**Independent Test**: Mô phỏng một widget lỗi và xác nhận phần khác hiển thị, lỗi có retry và kỳ lọc không mất.

**Acceptance Scenarios**:

1. **Given** một widget tải lỗi, **When** dashboard hoàn tất, **Then** widget khác vẫn dùng được và widget lỗi có retry.
2. **Given** retry thành công, **When** dữ liệu về, **Then** widget dùng cùng kỳ lọc hiện tại.

### Edge Cases

- Kỳ bắt đầu sau kết thúc hoặc vượt giới hạn cho phép.
- Giao dịch ở đúng biên ngày/timezone, daylight/time offset.
- Payment Paid sau ngày order hoặc payment Refunded.
- Order Completed rồi Cancelled/điều chỉnh.
- Top items bằng điểm nhau.
- Guest count bị nhân khi session có nhiều order/items.
- Widget dữ liệu lớn hoặc user đổi filter nhanh.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Dashboard MUST chỉ dành cho user có permission báo cáo và menu tương ứng.
- **FR-002**: Mặc định MUST dùng ngày hiện tại theo timezone nhà hàng và hiển thị from/to/timezone.
- **FR-003**: User MUST chọn hôm nay, 7 ngày, tháng hiện tại hoặc khoảng tùy chọn hợp lệ.
- **FR-004**: Mọi widget MUST dùng cùng kỳ lọc đã áp dụng.
- **FR-005**: Số order, gross, discount, net và average order MUST chỉ tính order Completed theo policy báo cáo.
- **FR-006**: Số món/top món/top variant MUST loại OrderItem Cancelled và dựa trên snapshot giao dịch.
- **FR-007**: Thực thu MUST chỉ tính Payment Paid trong kỳ thanh toán; không dùng Order.TotalAmount thay thế.
- **FR-008**: Số khách MUST tổng guest count theo session, không nhân do nhiều order/items.
- **FR-009**: Top bàn MUST dựa trên số session sử dụng hoặc metric được ghi nhãn rõ, không trộn nhiều định nghĩa.
- **FR-010**: Biểu đồ trend MUST có tổng đối soát được với KPI tương ứng cho cùng định nghĩa/kỳ.
- **FR-011**: Trạng thái bàn MUST phản ánh snapshot hiện hành; kitchen backlog/SLA MUST dùng trạng thái item hiện hành.
- **FR-012**: Widget MUST có loading, empty, error và retry độc lập.
- **FR-013**: Currency, decimal, date/time và status labels MUST nhất quán, dễ hiểu.
- **FR-014**: Filter MUST được giữ khi refresh/back và thao tác đổi kỳ cũ không được ghi đè kết quả kỳ mới.
- **FR-015**: Điều hướng từ widget MUST tuân permission màn hình đích.
- **FR-016**: Export và bảng aggregate summary nằm ngoài MVP; số liệu phải được định nghĩa đủ để bổ sung sau không đổi ý nghĩa.
- **FR-017**: Layout MUST responsive từ grid nhiều cột đến một cột và dùng được bằng bàn phím.

### Key Entities

- **Kỳ báo cáo**: From/to/timezone áp dụng đồng nhất.
- **KPI bán hàng**: Các số tổng hợp từ order và item hoàn tất.
- **KPI thực thu**: Tổng payment Paid theo paid time.
- **KPI khách/session**: Tổng guest count không đếm trùng.
- **Operational Snapshot**: Counts bàn và kitchen backlog hiện hành.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% lượt mở Dashboard hiển thị KPI chính trong dưới 2 giây ở điều kiện bình thường.
- **SC-002**: 100% KPI/trend cùng định nghĩa đối soát được cho cùng kỳ.
- **SC-003**: 100% thực thu chỉ gồm payment Paid và phân biệt rõ với net sales.
- **SC-004**: 100% guest count không bị nhân bởi quan hệ order/item.
- **SC-005**: 95% quản lý xác định được top 5 món và trạng thái bàn/bếp trong dưới 30 giây.
- **SC-006**: Lỗi một widget không làm mất dữ liệu đã tải thành công ở widget khác trong 100% tình huống kiểm thử.

## Assumptions

- MVP dùng ngày lịch theo timezone Asia/Bangkok; business-day cutoff riêng nằm ngoài phạm vi.
- Refund chưa có workflow hoàn chỉnh; khi triển khai phải chốt cách phản ánh trước khi mở KPI refund.
- Dữ liệu đủ lớn cho aggregate background sẽ được đánh giá sau; định nghĩa KPI không đổi.
- Dashboard không cho chỉnh sửa giao dịch.
