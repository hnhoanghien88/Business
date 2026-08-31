# Feature Specification: Vận hành bếp

**Feature Branch**: `[006-restaurant-kitchen]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Kitchens`

**Menu**: Code `kitchens` · Name `Kitchens` · Route `/restaurant/kitchens`

**Database Tables**: `restaurant_kitchen_orders`, `restaurant_kitchen_order_items`, `restaurant_order_items`, `restaurant_food_variants`

**Database Views**: `restaurant_order_item_kitchen_quantities`

**Input**: User description: "Tạo Kitchen Display realtime, hiển thị theo phiếu và xử lý trạng thái theo từng món, hỗ trợ gửi quantity từng phần."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Theo dõi hàng đợi bếp (Priority: P1)

Nhân viên bếp xem phiếu mới, đang làm và sẵn sàng theo thứ tự ưu tiên, cùng bàn, thời gian chờ, món/quantity và ghi chú.

**Why this priority**: Bếp cần nhìn thấy công việc mới chính xác để bắt đầu chế biến.

**Independent Test**: Tạo nhiều phiếu ở các trạng thái, xác nhận cột/tab, sort, timer, note và đồng bộ phiếu mới.

**Acceptance Scenarios**:

1. **Given** user có quyền bếp, **When** mở KDS, **Then** phiếu được nhóm theo trạng thái và sắp theo thời gian gửi.
2. **Given** phiếu mới được gửi, **When** màn hình đang kết nối, **Then** phiếu xuất hiện và có thông báo theo cài đặt người dùng.
3. **Given** kết nối gián đoạn, **When** kết nối lại, **Then** màn hình tải snapshot và không nhân đôi/mất phiếu.

---

### User Story 2 - Nhận và chế biến theo món (Priority: P1)

Nhân viên bếp nhận phiếu và chuyển từng kitchen item từ chờ sang đang làm rồi sẵn sàng.

**Why this priority**: Các món trong cùng phiếu có thời gian làm khác nhau và phải được theo dõi riêng.

**Independent Test**: Nhận phiếu có hai món, start/ready từng món độc lập và xác nhận trạng thái phiếu tổng hợp đúng.

**Acceptance Scenarios**:

1. **Given** phiếu Pending, **When** nhân viên nhận, **Then** người/thời điểm nhận được ghi và các item có thể bắt đầu.
2. **Given** item Accepted/Pending hợp lệ, **When** bắt đầu, **Then** item Preparing và timer bắt đầu.
3. **Given** item Preparing, **When** đánh dấu sẵn sàng, **Then** item Ready và màn hình phục vụ được thông báo.
4. **Given** hai nhân viên thao tác cùng item, **When** command cạnh tranh, **Then** chỉ transition hợp lệ thành công.

---

### User Story 3 - Hoàn tất, từ chối hoặc hủy phần món (Priority: P2)

Bếp hoàn tất item đã giao đi hoặc từ chối/hủy phần không thể làm với lý do, cho phép quantity hợp lệ được gửi lại.

**Why this priority**: Ngoại lệ phải được phản ánh rõ và không làm mất quantity khách đã đặt.

**Independent Test**: Từ chối một phần quantity với lý do, xác nhận remaining được giải phóng và có thể gửi lại; hoàn tất phần còn lại.

**Acceptance Scenarios**:

1. **Given** item Ready, **When** hoàn tất, **Then** item kết thúc và thời điểm hoàn tất được ghi.
2. **Given** item có thể từ chối, **When** từ chối với lý do, **Then** quantity đó không còn tính là đã gửi và phục vụ nhận cảnh báo.
3. **Given** item đã Preparing/Ready, **When** user thiếu quyền ngoại lệ cố hủy, **Then** hệ thống từ chối.

---

### User Story 4 - Cập nhật hết món từ bếp (Priority: P2)

Nhân viên bếp có quyền đánh dấu biến thể hết/còn để ngăn nhận order mới không thể phục vụ.

**Why this priority**: Bếp thường là nơi biết availability thực tế sớm nhất.

**Independent Test**: Đánh dấu hết món và xác nhận order screens bị chặn; bán lại và xác nhận có thể chọn.

**Acceptance Scenarios**:

1. **Given** variant đang bán, **When** bếp đánh dấu hết với lý do, **Then** order screen cập nhật và chặn chọn mới.
2. **Given** variant hết, **When** bán lại, **Then** availability được phục hồi mà không thay đổi active/price.

### Edge Cases

- Event đến sai thứ tự hoặc thao tác offline rồi reconnect.
- Phiếu có tất cả item Rejected/Cancelled.
- Một OrderItem chia thành nhiều kitchen item trên nhiều phiếu.
- Hủy/từ chối sau khi một phần quantity đã Completed.
- Clock/timezone khác nhau giữa màn hình bếp và server.
- Phiếu quá SLA, rất nhiều phiếu hoặc màn hình fullscreen.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: KDS MUST chỉ hiển thị cho user có quyền và cung cấp Pending, Preparing, Ready cùng lịch sử ngắn hạn Completed.
- **FR-002**: Phiếu MUST hiển thị kitchen number, bàn/order, giờ gửi, thời gian chờ, note và các item snapshot.
- **FR-003**: Hàng đợi MUST sắp ưu tiên theo thời gian gửi và làm nổi bật phiếu vượt ngưỡng chờ cấu hình.
- **FR-004**: Item MUST tuân state transitions hợp lệ Pending/Accepted → Preparing → Ready → Completed; Rejected/Cancelled là kết thúc.
- **FR-005**: Nhận phiếu MUST ghi actor và thời gian.
- **FR-006**: Reject/cancel MUST yêu cầu lý do và permission phù hợp.
- **FR-007**: Trạng thái KitchenOrder MUST được suy ra nhất quán từ các item, không do client quyết định.
- **FR-008**: Mỗi command MUST kiểm tra trạng thái hiện hành và chống xử lý trùng.
- **FR-009**: Một OrderItem MAY xuất hiện trên nhiều phiếu; tổng quantity không Cancelled/Rejected MUST không vượt quantity đặt.
- **FR-010**: Cancelled/Rejected MUST giải phóng quantity để gửi lại; Completed không được giải phóng.
- **FR-011**: Khi item Ready, người phục vụ MUST nhận được cập nhật liên kết đúng order/session/table.
- **FR-012**: Reconnect MUST tải snapshot mới và bỏ qua event cũ/trùng.
- **FR-013**: KDS MUST cung cấp lựa chọn âm thanh và fullscreen; các lựa chọn không thay đổi nghiệp vụ.
- **FR-014**: Change availability MUST tách biệt active/price và phản ánh cho order screen.
- **FR-015**: Permission MUST phân biệt xem, accept/start/ready/complete/reject/change availability.
- **FR-016**: Layout MUST dùng tốt trên desktop/tablet ngang và cung cấp tabs trên màn hình hẹp.
- **FR-017**: Mọi transition và ngoại lệ MUST có actor/time/reason cần thiết để audit.

### Key Entities

- **KitchenOrder**: Phiếu bếp thuộc một order và tổng hợp trạng thái item.
- **KitchenOrderItem**: Quantity của OrderItem được xử lý trên một phiếu.
- **OrderItem**: Quantity khách đặt và nguồn giới hạn tổng lượng gửi.
- **FoodVariant Availability**: Khả năng bán hiện tại được bếp cập nhật theo quyền.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 99% phiếu mới xuất hiện trên KDS trong dưới 3 giây ở điều kiện bình thường.
- **SC-002**: 95% nhân viên bếp nhận và bắt đầu một item mới trong dưới 10 giây thao tác.
- **SC-003**: 100% transitions không hợp lệ hoặc cạnh tranh bị chặn mà không mất trạng thái đúng.
- **SC-004**: 100% tổng quantity phiếu hợp lệ không vượt quantity order item.
- **SC-005**: 100% Reject/Cancel có lý do và cập nhật lại remaining chính xác.
- **SC-006**: Sau reconnect, 100% phiếu trên màn hình khớp snapshot hiện hành, không trùng.

## Assumptions

- MVP có một hàng đợi bếp chung; chia station nằm ngoài phạm vi.
- Served do feature phục vụ xử lý; Kitchen Completed thể hiện bếp đã giao/hoàn tất trách nhiệm.
- Ngưỡng SLA là cấu hình vận hành được cung cấp khi lập kế hoạch.
- In ticket vật lý nằm ngoài phạm vi.
