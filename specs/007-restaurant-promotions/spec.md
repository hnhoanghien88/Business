# Feature Specification: Quản lý mã khuyến mãi

**Feature Branch**: `[007-restaurant-promotions]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Promotions`

**Menu**: Code `promotions` · Name `Promotions` · Route `/restaurant/promotions`

**Database Tables**: `restaurant_promotion_codes`, `restaurant_promotion_foods`, `restaurant_promotion_categories`, `restaurant_foods`, `restaurant_categories`

**Input**: User description: "Tạo menu quản lý promotion code và áp một mã vào order theo phạm vi món/nhóm."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Tra cứu chương trình khuyến mãi (Priority: P1)

Người quản lý xem và lọc mã theo code, loại giảm, trạng thái và thời gian hiệu lực để biết chương trình nào đang/chưa/hết hiệu lực.

**Why this priority**: Cần nhìn được trạng thái trước khi tạo hoặc sửa chương trình.

**Independent Test**: Chuẩn bị promotion ở nhiều trạng thái, tìm/lọc và xác nhận kết quả, quota và trạng thái hiệu lực.

**Acceptance Scenarios**:

1. **Given** user có quyền xem, **When** mở danh sách, **Then** code, tên, loại/giá trị, thời gian, quota và trạng thái được hiển thị.
2. **Given** filter hiệu lực/loại giảm, **When** áp dụng, **Then** danh sách và tổng kết quả chính xác.
3. **Given** tải lỗi/rỗng, **When** hoàn tất, **Then** trạng thái phù hợp và retry/xóa lọc được cung cấp.

---

### User Story 2 - Tạo và cập nhật mã (Priority: P1)

Người quản lý cấu hình mã giảm phần trăm hoặc số tiền cố định, điều kiện đơn tối thiểu, giảm tối đa, thời gian, quota và trạng thái.

**Why this priority**: Đây là khả năng cốt lõi để vận hành promotion.

**Independent Test**: Tạo mã hợp lệ, preview trên subtotal mẫu, cập nhật trước khi sử dụng và xác nhận validation.

**Acceptance Scenarios**:

1. **Given** dữ liệu hợp lệ, **When** tạo, **Then** promotion được lưu với code duy nhất và trạng thái đã chọn.
2. **Given** percentage/fixed không hợp lệ hoặc end không sau start, **When** lưu, **Then** hệ thống từ chối và chỉ rõ lỗi.
3. **Given** promotion đã được áp cho order, **When** sửa cấu hình, **Then** order cũ giữ snapshot và kết quả cũ.

---

### User Story 3 - Chọn phạm vi áp dụng (Priority: P1)

Người quản lý cho promotion áp toàn menu hoặc giới hạn theo nhóm/món, và xem trước item nào đủ điều kiện.

**Why this priority**: Phạm vi quyết định tính đúng của discount và tránh giảm nhầm món.

**Independent Test**: Tạo promotion toàn menu, theo nhóm và theo món; kiểm tra eligibility trên giỏ mẫu.

**Acceptance Scenarios**:

1. **Given** chọn toàn menu, **When** lưu, **Then** mọi item hợp lệ theo điều kiện chung được tính.
2. **Given** chọn categories/foods, **When** áp, **Then** chỉ subtotal item trong scope được dùng tính giảm.
3. **Given** đối tượng scope ngừng hoạt động sau đó, **When** áp promotion mới, **Then** item không còn bán không tạo lợi ích mới.

---

### User Story 4 - Áp mã an toàn vào order (Priority: P2)

Nhân viên bán hàng áp một mã hợp lệ, hệ thống kiểm tra thời gian/quota/minimum/scope và lưu snapshot discount.

**Why this priority**: Promotion quản trị có thể tồn tại độc lập trước khi tích hợp order.

**Independent Test**: Hai yêu cầu tranh quota cuối cùng và xác nhận chỉ số lượng cho phép thành công; hủy order đủ điều kiện hoàn quota theo policy.

**Acceptance Scenarios**:

1. **Given** mã hợp lệ, **When** áp vào order, **Then** discount đúng scope/cap và usage tăng một lần.
2. **Given** quota cuối cùng bị tranh đồng thời, **When** hai order áp, **Then** không vượt usage limit.
3. **Given** order bị hủy trước payment Paid, **When** hoàn quota, **Then** usage có thể dùng lại đúng một lần.

### Edge Cases

- Percentage lớn hơn 100, fixed lớn hơn subtotal, max discount nhỏ hơn 0.
- Thời gian biên start/end và timezone nhà hàng.
- Hai mã giống nhau khác hoa thường.
- Scope rỗng: phải được hiểu rõ là toàn menu, không phải không áp đâu cả.
- Item hủy sau khi promotion đã phân bổ.
- Promotion deactivate trong khi user đang preview.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hệ thống MUST giới hạn menu và actions theo permission.
- **FR-002**: Danh sách MUST hỗ trợ tìm code/tên, lọc hiệu lực/loại/trạng thái và phân trang.
- **FR-003**: Code MUST duy nhất không phân biệt hoa thường và bất biến sau tạo.
- **FR-004**: Promotion MUST hỗ trợ Percentage và FixedAmount với giá trị dương.
- **FR-005**: Percentage MUST không vượt 100; discount thực tế MUST không âm hoặc vượt subtotal eligible.
- **FR-006**: End time MUST sau start time; hiệu lực được đánh giá theo timezone nhà hàng.
- **FR-007**: Minimum order, maximum discount và usage limit nếu có MUST không âm; usage count không vượt limit.
- **FR-008**: Scope MUST được biểu diễn tường minh là All, Categories hoặc Foods; danh sách rỗng chỉ hợp lệ với All.
- **FR-009**: Discount MUST chỉ tính trên subtotal item thuộc scope, sau đó áp cap nếu có; scope category bao gồm category đã chọn và toàn bộ hậu duệ.
- **FR-010**: MVP MUST cho tối đa một promotion trên một order.
- **FR-011**: Validation MUST được thực hiện lại khi cam kết order, không dựa vào preview.
- **FR-012**: Áp mã và tăng usage MUST không thể hoàn tất một phần và MUST an toàn dưới cạnh tranh.
- **FR-013**: Hủy order trước bất kỳ payment Paid nào MUST hoàn usage đúng một lần; sau Paid không tự hoàn quota.
- **FR-014**: OrderPromotion MUST giữ snapshot code, tên và discount để sửa promotion không đổi lịch sử.
- **FR-015**: Discount SHOULD được phân bổ vào eligible items để hỗ trợ báo cáo/hủy item chính xác.
- **FR-016**: Deactivate thay hard delete đối với promotion đã được sử dụng.
- **FR-017**: Form MUST có preview dễ hiểu, validation field/tổng quát, ngăn submit trùng và hỗ trợ bàn phím/mobile.

### Key Entities

- **Promotion Code**: Chương trình giảm giá, điều kiện, thời gian, quota và scope.
- **Promotion Scope**: Toàn menu hoặc tập category/food eligible.
- **Order Promotion**: Snapshot promotion và discount đã áp vào order.
- **Usage**: Số lần promotion đã được cam kết và chưa được hoàn hợp lệ.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% người quản lý tạo promotion hợp lệ trong dưới 3 phút.
- **SC-002**: 100% mã hết hạn, sai scope, thiếu minimum hoặc hết quota bị từ chối.
- **SC-003**: 100% cạnh tranh quota không làm usage vượt limit.
- **SC-004**: 100% order lịch sử giữ discount snapshot sau khi promotion thay đổi.
- **SC-005**: 100% discount không âm, không vượt subtotal eligible và tuân cap.
- **SC-006**: 95% nhân viên hiểu được lý do mã không hợp lệ trong lần thử đầu tiên.

## Assumptions

- MVP không stack nhiều promotion và không có per-customer limit.
- BOGO, combo, coupon cá nhân và loyalty ngoài phạm vi.
- Scope All là lựa chọn explicit.
- Policy hoàn quota chỉ áp khi order hủy trước payment Paid.
