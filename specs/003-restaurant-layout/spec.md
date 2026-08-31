# Feature Specification: Thiết lập khu vực và bàn

**Feature Branch**: `[003-restaurant-layout]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Layouts`

**Menu**: Code `layouts` · Name `Layouts` · Route `/restaurant/layouts`

**Database Tables**: `restaurant_areas`, `restaurant_tables`

**Input**: User description: "Tạo chức năng menu Thiết lập Khu vực & bàn cho hệ thống quản lý nhà hàng."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Quản lý khu vực (Priority: P1)

Người quản lý cấu hình các tầng/khu vực để tổ chức bàn vật lý và thứ tự hiển thị trên sơ đồ vận hành.

**Why this priority**: Bàn không thể được tổ chức hoặc tìm nhanh nếu chưa có khu vực.

**Independent Test**: Tạo, sửa, ngừng và kích hoạt lại khu vực; xác nhận danh sách và ảnh hưởng đến lựa chọn cấu hình bàn.

**Acceptance Scenarios**:

1. **Given** người dùng có quyền, **When** tạo khu vực hợp lệ, **Then** khu vực xuất hiện đúng thứ tự.
2. **Given** mã trùng hoặc dữ liệu thiếu, **When** lưu, **Then** hệ thống từ chối và chỉ rõ lỗi.
3. **Given** khu vực có bàn, **When** ngừng hoạt động, **Then** hệ thống cảnh báo ảnh hưởng và không xóa bàn/lịch sử.

---

### User Story 2 - Quản lý bàn (Priority: P1)

Người quản lý tạo và cập nhật bàn với mã, tên, khu vực, sức chứa và trạng thái cấu hình.

**Why this priority**: Bàn là tài nguyên trung tâm của luồng phục vụ tại chỗ.

**Independent Test**: Tạo bàn trong khu vực, sửa sức chứa/chuyển khu vực khi hợp lệ và xác nhận bàn xuất hiện trên sơ đồ.

**Acceptance Scenarios**:

1. **Given** khu vực hoạt động, **When** tạo bàn có mã duy nhất và sức chứa dương, **Then** bàn được tạo ở trạng thái Available.
2. **Given** bàn không có lượt mở, **When** chuyển sang khu vực khác, **Then** vị trí cấu hình được cập nhật.
3. **Given** bàn có lượt mở, **When** cố chuyển khu vực hoặc disable, **Then** hệ thống từ chối và giải thích điều kiện đang chặn.

---

### User Story 3 - Disable và kích hoạt bàn an toàn (Priority: P2)

Người quản lý tạm ngừng sử dụng bàn hỏng/bảo trì và kích hoạt lại khi sẵn sàng.

**Why this priority**: Nhà hàng phải ngăn nhân viên mở bàn không sử dụng được mà vẫn giữ lịch sử.

**Independent Test**: Disable bàn trống, xác nhận không mở được; kích hoạt lại và xác nhận bàn trở về Available.

**Acceptance Scenarios**:

1. **Given** bàn Available không có session mở, **When** disable, **Then** bàn chuyển Disabled và không thể mở.
2. **Given** bàn Occupied/Cleaning hoặc có session mở, **When** disable, **Then** thao tác bị từ chối.
3. **Given** bàn Disabled, **When** kích hoạt lại, **Then** bàn trở về Available.

### Edge Cases

- Khu vực/bàn có mã chỉ khác hoa thường.
- Sức chứa bằng 0, âm hoặc vượt giới hạn hợp lý.
- Hai người đồng thời chuyển/disable cùng bàn.
- Khu vực ngừng hoạt động nhưng còn bàn Occupied.
- Bàn có trạng thái vận hành thay đổi trong lúc form cấu hình đang mở.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hệ thống MUST cung cấp một menu Thiết lập với workspace hai tab Khu vực và Bàn cho người có quyền.
- **FR-002**: Khu vực MUST có mã duy nhất không phân biệt hoa thường, tên, mô tả, thứ tự và trạng thái.
- **FR-003**: Bàn MUST có mã duy nhất không phân biệt hoa thường, tên, khu vực, sức chứa, trạng thái vận hành và trạng thái cấu hình.
- **FR-004**: Mã khu vực và bàn MUST bất biến sau khi tạo.
- **FR-005**: Bàn mới MUST thuộc khu vực hoạt động và có sức chứa lớn hơn 0.
- **FR-006**: CRUD cấu hình MUST không cho sửa trực tiếp trạng thái Occupied hoặc Cleaning.
- **FR-007**: Bàn chỉ được chuyển khu vực khi không có session mở.
- **FR-008**: Bàn chỉ được disable khi không có session mở và không ở trạng thái Occupied/Cleaning.
- **FR-009**: Kích hoạt lại bàn Disabled MUST đưa bàn về Available.
- **FR-010**: Ngừng khu vực có bàn MUST yêu cầu xác nhận, giữ dữ liệu lịch sử và không tự ý đóng session.
- **FR-011**: Khu vực ngừng hoạt động và bàn của khu vực MUST không được dùng để mở lượt mới.
- **FR-012**: Hệ thống MUST hỗ trợ tìm kiếm, lọc, phân trang và thứ tự ổn định cho cả hai tab.
- **FR-013**: Hệ thống MUST phát hiện cập nhật đồng thời và không ghi đè trạng thái vận hành mới hơn.
- **FR-014**: Mọi action MUST được giới hạn theo permission; route/menu thuộc Identity Application code `restaurant`.
- **FR-015**: Thay đổi cấu hình hợp lệ MUST phản ánh trên sơ đồ bàn chậm nhất ở lần tải/đồng bộ kế tiếp.
- **FR-016**: Hard delete khu vực/bàn đã có tham chiếu lịch sử MUST nằm ngoài phạm vi.
- **FR-017**: Màn hình MUST dùng được với sidebar mở/thu gọn, trên mobile và bằng bàn phím.

### Key Entities

- **Khu vực nhà hàng**: Tầng hoặc vùng vật lý chứa nhiều bàn.
- **Bàn nhà hàng**: Bàn vật lý có sức chứa và vòng đời trạng thái vận hành.
- **Lượt bàn**: Lần sử dụng bàn; quyết định các thao tác cấu hình có an toàn hay không.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% người quản lý tạo một khu vực và bàn hợp lệ trong dưới 2 phút.
- **SC-002**: 100% bàn có session mở không thể bị disable hoặc chuyển khu vực.
- **SC-003**: 100% trạng thái Occupied/Cleaning chỉ thay đổi qua luồng vận hành, không qua form cấu hình.
- **SC-004**: 100% dữ liệu lịch sử vẫn tra cứu được sau khi khu vực/bàn ngừng hoạt động.
- **SC-005**: 95% tìm kiếm bàn/khu vực có kết quả có thể thao tác trong dưới 2 giây với 10.000 bàn.
- **SC-006**: 100% người thiếu quyền không thể xem hoặc thực hiện action bị cấm.

## Assumptions

- MVP phục vụ một chi nhánh; branch management nằm ngoài phạm vi.
- Bố trí tọa độ kéo thả trên sơ đồ nằm ngoài phạm vi; thứ tự/khu vực quyết định cách nhóm card.
- Reserved không được đặt thủ công trong feature cấu hình.
- Thay đổi sức chứa không sửa guest count của session đã mở.
