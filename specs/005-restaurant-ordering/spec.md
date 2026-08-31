# Feature Specification: Gọi món tại bàn

**Feature Branch**: `[005-restaurant-ordering]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Ordering`

**Database Tables**: `restaurant_table_sessions`, `restaurant_foods`, `restaurant_food_variants`, `restaurant_orders`, `restaurant_order_items`, `restaurant_kitchen_orders`, `restaurant_kitchen_order_items`

**Database Views**: `restaurant_order_item_kitchen_quantities`

**Input**: User description: "Tạo workflow gọi món theo TableSession gồm duyệt menu, giỏ hàng, xác nhận order và gửi bếp an toàn."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Chọn món cho bàn đang phục vụ (Priority: P1)

Nhân viên từ bàn Occupied duyệt menu theo nhóm, tìm món, chọn biến thể/số lượng và ghi chú vào giỏ của đúng lượt bàn.

**Why this priority**: Đây là thao tác tạo doanh thu cốt lõi của nhà hàng.

**Independent Test**: Mở session hợp lệ, thêm nhiều món/biến thể vào giỏ, chỉnh số lượng/ghi chú và xác nhận tạm tính hiển thị đúng theo dữ liệu hiện hành.

**Acceptance Scenarios**:

1. **Given** session Open, **When** mở gọi món, **Then** menu chỉ gồm nhóm/món/biến thể hoạt động và thể hiện rõ availability.
2. **Given** món có nhiều biến thể, **When** chọn món, **Then** nhân viên chọn biến thể trước khi thêm; biến thể Default không tạo bước dư thừa.
3. **Given** item trong giỏ, **When** đổi số lượng/ghi chú hoặc xóa, **Then** giỏ của đúng session được cập nhật.
4. **Given** món hết, **When** nhân viên cố thêm, **Then** hệ thống chặn và hiển thị lý do.

---

### User Story 2 - Xác nhận và gửi bếp (Priority: P1)

Nhân viên xác nhận giỏ để hệ thống kiểm tra lại giá, availability và tạo một lần gọi món được gửi bếp đúng một lần.

**Why this priority**: Giá và trạng thái phải được kiểm soát tại thời điểm cam kết, không dựa vào dữ liệu giỏ cũ.

**Independent Test**: Xác nhận giỏ hợp lệ và kiểm tra order snapshot cùng phiếu bếp được tạo, giỏ được xóa và lần retry không tạo bản sao.

**Acceptance Scenarios**:

1. **Given** giỏ hợp lệ, **When** xác nhận, **Then** hệ thống dùng giá hiện tại, tạo order/items snapshot và gửi toàn bộ quantity yêu cầu đến bếp như một workflow nhất quán.
2. **Given** giá thay đổi từ lúc thêm giỏ, **When** xác nhận, **Then** nhân viên thấy tổng mới và phải xác nhận lại trước khi cam kết.
3. **Given** một item vừa hết hoặc session vừa đóng, **When** xác nhận, **Then** toàn bộ lần gọi bị từ chối, chỉ rõ item/điều kiện và không tạo dữ liệu một phần.
4. **Given** phản hồi bị gián đoạn sau thành công, **When** cùng yêu cầu được thử lại, **Then** hệ thống trả kết quả cũ thay vì tạo order/phiếu bếp mới.

---

### User Story 3 - Gọi thêm món (Priority: P1)

Nhân viên quay lại session đang mở và tạo một lần gọi món mới mà không thay đổi order đã gửi trước đó.

**Why this priority**: Khách thường gọi nhiều lần trong cùng lượt ngồi.

**Independent Test**: Tạo hai lần gọi cho cùng session và xác nhận có hai order riêng, cả hai được tổng hợp vào lượt bàn.

**Acceptance Scenarios**:

1. **Given** session đã có order, **When** gọi thêm, **Then** hệ thống tạo order mới cùng session.
2. **Given** order cũ đang được bếp xử lý, **When** gọi thêm, **Then** trạng thái/snapshot order cũ không bị sửa.
3. **Given** xem chi tiết session, **When** lần gọi mới thành công, **Then** tổng lượt bàn gồm cả order mới.

---

### User Story 4 - Áp mã khuyến mãi khi gọi món (Priority: P2)

Nhân viên nhập một mã khuyến mãi để xem kết quả hợp lệ và số giảm trước khi xác nhận order.

**Why this priority**: Khuyến mãi ảnh hưởng giá phải trả nhưng có thể triển khai sau luồng gọi món cơ bản.

**Independent Test**: Áp mã hợp lệ và không hợp lệ trên cùng giỏ, xác nhận lý do và snapshot giảm giá đúng sau khi tạo order.

**Acceptance Scenarios**:

1. **Given** mã hợp lệ cho các item trong giỏ, **When** áp mã, **Then** số giảm và tổng dự kiến được hiển thị.
2. **Given** mã hết hạn/không đúng scope/không đủ tối thiểu, **When** áp, **Then** lý do cụ thể được hiển thị và tổng không đổi.
3. **Given** điều kiện mã thay đổi trước xác nhận, **When** tạo order, **Then** hệ thống kiểm tra lại và không dùng kết quả cũ.

### Edge Cases

- Cart còn dữ liệu khi session đã đóng hoặc user chuyển sang bàn khác.
- Quantity thập phân không phù hợp với món đếm theo phần.
- Hai nhân viên gọi cùng món cho một bàn cùng lúc.
- Giá, category hoặc availability đổi trong lúc màn hình mở.
- Promotion đạt usage limit giữa validate và commit.
- Gửi một phần quantity sau khi order tồn tại nhưng chưa gửi hết.
- Mất mạng trước/sau khi người dùng nhấn xác nhận.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Chỉ session Open trên bàn Occupied và người có quyền mới được tạo order DineIn.
- **FR-002**: Menu bán MUST chỉ gồm category/food/variant hoạt động, đồng thời vẫn hiển thị rõ variant tạm hết nhưng không cho chọn.
- **FR-003**: Người dùng MUST có thể tìm, lọc theo category, chọn variant, quantity và note.
- **FR-004**: Giỏ MUST được cô lập theo TableSession và không chuyển item ngầm sang session khác.
- **FR-005**: Dữ liệu do người dùng gửi từ giao diện MUST không là nguồn quyết định giá, discount hoặc total khi tạo order.
- **FR-006**: Khi xác nhận, hệ thống MUST kiểm tra lại session, active, availability, giá và promotion từ nguồn hiện hành.
- **FR-007**: Nếu giá/tổng thay đổi so với thông tin người dùng vừa xem, hệ thống MUST yêu cầu xác nhận lại.
- **FR-008**: Tạo order MUST lưu snapshot mã/tên/variant/giá/quantity/note và totals để lịch sử không đổi theo catalog.
- **FR-009**: MVP MUST thực hiện Xác nhận & gửi bếp như một workflow duy nhất từ góc nhìn người dùng; lỗi bất kỳ phần nào không được để lại order/phiếu bếp không nhất quán.
- **FR-010**: Mỗi lần gọi thêm MUST tạo order mới cùng TableSession, không append vào order đã gửi.
- **FR-011**: Hệ thống MUST hỗ trợ gửi từng phần OrderItem; tổng quantity ở các phiếu còn hiệu lực không vượt quantity đã đặt.
- **FR-012**: Cancelled/Rejected kitchen quantity MUST được phép gửi lại; các trạng thái khác tính là đã gửi.
- **FR-013**: Command tạo/gửi MUST chống tạo trùng khi retry bằng một định danh yêu cầu ổn định.
- **FR-014**: MVP MUST hỗ trợ tối đa một promotion code trên một order.
- **FR-015**: Promotion MUST được kiểm tra lại trong cùng thao tác cam kết order; kết quả lưu phải là snapshot.
- **FR-016**: Thành công MUST xóa giỏ session, hiển thị order/kitchen number và cập nhật tổng session.
- **FR-017**: Lỗi MUST giữ giỏ, chỉ rõ item cần sửa và không làm mất note/quantity hợp lệ.
- **FR-018**: Menu và CTA MUST tuân theo permissions của Resource `Ordering` và authorization của Identity Application code `restaurant`.
- **FR-019**: Layout MUST dùng menu grid + cart responsive, ngăn submit trùng và thao tác được bằng bàn phím.

### Key Entities

- **TableSession**: Ngữ cảnh lượt bàn nhận nhiều order.
- **Order**: Một lần gọi món độc lập trong session.
- **OrderItem**: Snapshot món/variant/giá và quantity đã cam kết.
- **Cart**: Dữ liệu tạm theo session trước khi cam kết.
- **KitchenOrder**: Phiếu chứa phần quantity được gửi bếp.
- **OrderPromotion**: Snapshot mã và số giảm áp dụng cho order.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% nhân viên tạo và gửi một order 5 món trong dưới 90 giây.
- **SC-002**: 100% order dùng giá hiện hành tại thời điểm xác nhận và giữ snapshot không đổi.
- **SC-003**: 100% retry cùng yêu cầu không tạo order hoặc kitchen ticket trùng.
- **SC-004**: 100% quantity gửi bếp không vượt quantity đã đặt, kể cả yêu cầu đồng thời.
- **SC-005**: 100% lỗi validation quan trọng không để lại dữ liệu giao dịch một phần.
- **SC-006**: 99% thay đổi availability được phản ánh cho màn hình đang mở trong dưới 3 giây ở điều kiện bình thường.

## Assumptions

- MVP chỉ hỗ trợ DineIn; TakeAway/Delivery ngoài phạm vi.
- Mỗi xác nhận cart tạo một order mới và gửi bếp ngay.
- Quantity mặc định theo phần nguyên; hỗ trợ quantity thập phân chỉ khi catalog có chính sách riêng trong tương lai.
- Split bill, modifier/topping, combo và inventory ngoài phạm vi.
- Feature Promotion có thể được tích hợp sau; gọi món không promotion vẫn hoạt động độc lập.
