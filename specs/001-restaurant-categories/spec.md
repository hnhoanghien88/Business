# Feature Specification: Quản lý nhóm món nhà hàng

**Feature Branch**: `[001-restaurant-categories]`

**Created**: 2026-08-27

**Status**: Draft

**Identity Resource**: `Categories`

**Menu**: Code `categories` · Name `Categories` · Route `/restaurant/categories`

**Database Tables**: `restaurant_categories`

**Input**: User description: "Tạo chức năng menu Nhóm món dạng cây cha–con dựa trên bản đồ menu, màn hình và phạm vi Spec Kit của hệ thống quản lý nhà hàng."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Xem và tìm nhóm món (Priority: P1)

Người quản lý thực đơn mở menu **Nhóm món** để xem cây nhóm cha–con, mở/thu gọn từng nhánh, tìm nhanh theo mã hoặc tên, và phân biệt nhóm đang hoạt động với nhóm đã ngừng hoạt động.

**Why this priority**: Danh sách nhóm món là điểm vào cơ bản để quản trị cấu trúc thực đơn và là điều kiện để quản lý món ăn nhất quán.

**Independent Test**: Chuẩn bị cây ít nhất ba cấp ở cả hai trạng thái, mở màn hình bằng người dùng có quyền xem, mở/thu gọn nhánh, tìm kiếm và lọc, rồi xác nhận đúng quan hệ cha–con và đường dẫn của kết quả.

**Acceptance Scenarios**:

1. **Given** người dùng có quyền xem và có dữ liệu nhóm món nhiều cấp, **When** mở menu Nhóm món, **Then** hệ thống hiển thị tree list đúng quan hệ cha–con, mã, tên, mô tả, thứ tự và trạng thái.
2. **Given** cây có nhiều nhánh, **When** người dùng mở hoặc thu gọn một node, **Then** chỉ nhánh đó thay đổi và trạng thái các nhánh khác được giữ.
3. **Given** cây có nhiều nhóm món, **When** người dùng tìm theo một phần mã hoặc tên, **Then** kết quả phù hợp cùng đường dẫn tổ tiên được hiển thị để giữ ngữ cảnh.
4. **Given** có nhóm hoạt động và ngừng hoạt động, **When** người dùng lọc theo trạng thái, **Then** kết quả và đường dẫn cây phản ánh đúng trạng thái đã chọn.
5. **Given** không có kết quả phù hợp hoặc tải thất bại, **When** tải hoàn tất, **Then** trạng thái rỗng hoặc lỗi có thể thử lại được hiển thị rõ ràng.

---

### User Story 2 - Tạo nhóm món (Priority: P1)

Người dùng được cấp quyền tạo một nhóm gốc hoặc nhóm con bằng cách chọn nhóm cha tùy chọn và khai báo thông tin nhóm.

**Why this priority**: Không có khả năng tạo nhóm thì nhà hàng không thể hình thành hoặc mở rộng cấu trúc thực đơn.

**Independent Test**: Tạo một nhóm gốc và hai cấp nhóm con, tải lại cây, rồi xác nhận từng nhóm xuất hiện dưới đúng cha và đúng thứ tự.

**Acceptance Scenarios**:

1. **Given** người dùng có quyền tạo, **When** không chọn cha và lưu dữ liệu hợp lệ, **Then** nhóm được tạo ở cấp gốc.
2. **Given** đã chọn một nhóm cha hoạt động, **When** lưu dữ liệu hợp lệ, **Then** nhóm mới xuất hiện trực tiếp dưới nhóm cha.
3. **Given** mã/tên không hợp lệ hoặc mã trùng, **When** lưu, **Then** hệ thống chỉ rõ lỗi và không tạo nhóm.
4. **Given** thao tác lưu đang xử lý, **When** người dùng nhấn lưu nhiều lần, **Then** hệ thống chỉ xử lý một yêu cầu.
5. **Given** người dùng không có quyền tạo, **When** mở màn hình hoặc gửi yêu cầu trực tiếp, **Then** hành động không được cung cấp và yêu cầu bị từ chối.

---

### User Story 3 - Cập nhật nhóm món (Priority: P2)

Người dùng được cấp quyền chỉnh sửa thông tin và di chuyển toàn bộ nhánh sang nhóm cha hợp lệ nhưng không thay đổi mã nhận diện đã phát hành.

**Why this priority**: Thực đơn cần được điều chỉnh thường xuyên trong khi mã ổn định giúp tránh nhầm lẫn và giữ liên kết lịch sử.

**Independent Test**: Chọn một nhóm có con, đổi nhóm cha, lưu và xác nhận cả nhánh chuyển vị trí mà quan hệ bên trong được giữ.

**Acceptance Scenarios**:

1. **Given** người dùng có quyền cập nhật, **When** mở một nhóm, **Then** form hiển thị dữ liệu hiện tại và mã ở trạng thái chỉ đọc.
2. **Given** chọn cha hợp lệ, **When** lưu, **Then** nhóm và toàn bộ hậu duệ chuyển sang vị trí mới.
3. **Given** chọn chính nhóm hoặc một hậu duệ làm cha, **When** lưu, **Then** hệ thống từ chối để không tạo chu trình.
4. **Given** bản ghi đã được người khác thay đổi, **When** lưu dữ liệu cũ, **Then** hệ thống không ghi đè âm thầm.
5. **Given** người dùng không có quyền cập nhật, **When** xem hoặc gửi yêu cầu trực tiếp, **Then** action không được cung cấp và yêu cầu bị từ chối.

---

### User Story 4 - Ngừng sử dụng nhóm món an toàn (Priority: P2)

Người quản lý ngừng sử dụng một nhóm để toàn bộ nhánh và các món trong nhánh không còn xuất hiện trong thực đơn bán, trong khi dữ liệu lịch sử vẫn được giữ nguyên.

**Why this priority**: Nhà hàng cần ẩn nhóm không còn kinh doanh mà không làm mất lịch sử đơn hàng hoặc phá vỡ liên kết món ăn.

**Independent Test**: Ngừng hoạt động một nhóm cha có nhiều cấp con, xác nhận toàn bộ nhánh không còn hiệu lực để bán nhưng trạng thái riêng của các node con không bị ghi đè.

**Acceptance Scenarios**:

1. **Given** nhóm đang hoạt động có hậu duệ/món, **When** chọn ngừng hoạt động, **Then** hệ thống cảnh báo toàn bộ nhánh và món trong nhánh sẽ không còn xuất hiện trong thực đơn bán.
2. **Given** người dùng xác nhận cảnh báo và có quyền cập nhật, **When** thao tác hoàn tất, **Then** nhóm chuyển sang ngừng hoạt động nhưng không bị xóa khỏi lịch sử quản trị.
3. **Given** nhóm cha được kích hoạt lại, **When** nhánh trở lại hiệu lực, **Then** node con/món chỉ xuất hiện nếu trạng thái riêng của chúng cũng đang hoạt động.
4. **Given** người dùng hủy tại bước xác nhận, **When** dialog đóng, **Then** trạng thái nhóm và các món không thay đổi.

### Edge Cases

- Người dùng nhập mã hoặc tên chỉ gồm khoảng trắng.
- Mã khác nhau về chữ hoa/chữ thường nhưng có thể được hiểu là cùng một mã nghiệp vụ.
- Mô tả đạt giới hạn tối đa hoặc chứa ký tự tiếng Việt và ký tự đặc biệt hợp lệ.
- Thứ tự hiển thị bằng nhau giữa nhiều nhóm; hệ thống vẫn phải có thứ tự kết quả ổn định.
- Nhóm được di chuyển dưới chính nó hoặc một hậu duệ sâu nhiều cấp.
- Nhóm cha bị ngừng hoạt động trong khi node con vẫn có `IsActive=true`.
- Xóa/di chuyển nhóm đang có món, promotion hoặc nhiều hậu duệ tham chiếu.
- Người dùng đang ở trang sau nhưng kết quả tìm kiếm hoặc cập nhật làm trang đó không còn bản ghi.
- Nhóm bị ngừng hoạt động trong khi một màn hình gọi món đang mở; lần tải hoặc làm mới tiếp theo không được cho phép chọn món từ nhóm đó.
- Phiên đăng nhập hết hạn hoặc quyền thay đổi trong khi người dùng đang mở form.
- Yêu cầu tạo/cập nhật thành công nhưng phản hồi đến người dùng bị gián đoạn; thử lại không được tạo dữ liệu trùng theo cùng mã.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hệ thống MUST chỉ cho người dùng đã được cấp quyền xem truy cập màn hình và dữ liệu nhóm món.
- **FR-002**: Hệ thống MUST cung cấp menu Nhóm món trong nhóm Thực đơn cho người dùng có quyền xem menu tương ứng.
- **FR-003**: Hệ thống MUST hiển thị tree list nhiều cấp gồm quan hệ cha–con, mã, tên, mô tả, thứ tự và trạng thái.
- **FR-004**: Hệ thống MUST hỗ trợ tìm kiếm không phân biệt chữ hoa/chữ thường theo toàn bộ hoặc một phần mã và tên.
- **FR-005**: Hệ thống MUST hỗ trợ mở/thu gọn nhánh, lọc trạng thái và tìm kiếm kèm đường dẫn tổ tiên của kết quả.
- **FR-006**: Hệ thống MUST giữ điều kiện tìm kiếm, lọc và trang hiện tại nhất quán trong quá trình xem danh sách; thay đổi điều kiện tìm kiếm hoặc kích thước trang MUST đưa người dùng về trang đầu.
- **FR-007**: Hệ thống MUST cho tạo nhóm gốc hoặc nhóm con bằng Parent tùy chọn cùng mã, tên, mô tả, thứ tự và trạng thái.
- **FR-008**: Mã và tên MUST được loại bỏ khoảng trắng thừa ở đầu/cuối và MUST không được rỗng sau khi chuẩn hóa.
- **FR-009**: Mã nhóm MUST là duy nhất theo cách so sánh không phân biệt chữ hoa/chữ thường trong phạm vi nhà hàng.
- **FR-010**: Mã nhóm MUST dài tối đa 50 ký tự, tên tối đa 150 ký tự và mô tả tối đa 500 ký tự.
- **FR-011**: Thứ tự hiển thị MUST là số nguyên trong từng nhóm anh em; khi bằng nhau MUST sắp ổn định theo tên rồi mã.
- **FR-012**: Hệ thống MUST không cho thay đổi mã của nhóm sau khi nhóm đã được tạo.
- **FR-013**: Hệ thống MUST cho người dùng có quyền cập nhật tên, mô tả, thứ tự, trạng thái và Parent.
- **FR-014**: Hệ thống MUST phát hiện cập nhật dựa trên dữ liệu lỗi thời và MUST ngăn ghi đè âm thầm thay đổi mới hơn.
- **FR-015**: Hệ thống MUST yêu cầu xác nhận trước khi ngừng nhóm và thông báo ảnh hưởng đến toàn bộ hậu duệ/món trong nhánh.
- **FR-016**: Category chỉ hiệu lực để bán khi chính nó và mọi tổ tiên đều active; nhóm không hiệu lực và toàn bộ nhánh MUST bị loại khỏi lựa chọn bán mới nhưng giữ lịch sử.
- **FR-017**: Khi kích hoạt lại nhóm, hệ thống MUST không tự động thay đổi trạng thái hoạt động hoặc khả dụng riêng của từng món.
- **FR-018**: Hệ thống MUST không xóa vĩnh viễn nhóm đã hoặc đang được món ăn tham chiếu trong phạm vi feature này.
- **FR-019**: Hệ thống MUST chỉ cung cấp các hành động tạo và cập nhật cho người dùng có permission tương ứng, đồng thời MUST từ chối yêu cầu thay đổi không được cấp quyền.
- **FR-020**: Hệ thống MUST hiển thị trạng thái đang tải, danh sách rỗng, lỗi có thể thử lại, đang lưu và kết quả thành công theo cách không cho phép thao tác trùng.
- **FR-021**: Hệ thống MUST hỗ trợ thao tác bằng bàn phím, focus nhìn thấy được, nhãn có ý nghĩa và bố cục dùng được khi menu trái mở rộng, thu gọn hoặc chuyển thành drawer trên màn hình nhỏ.
- **FR-022**: Menu, route và permission của feature MUST thuộc Identity Application code `restaurant` và MUST dùng cùng mã permission giữa giao diện và dịch vụ nghiệp vụ.
- **FR-023**: Thay đổi trạng thái nhóm MUST được phản ánh cho các màn hình bán đang hoạt động chậm nhất ở lần làm mới dữ liệu kế tiếp.
- **FR-024**: Mọi lỗi validation hoặc xung đột MUST chỉ rõ nguyên nhân có thể xử lý mà không làm mất dữ liệu người dùng đã nhập.
- **FR-025**: Hệ thống MUST cho di chuyển toàn bộ nhánh sang Parent khác mà không thay đổi quan hệ nội bộ của hậu duệ.
- **FR-026**: Parent MUST không được là chính node hoặc bất kỳ hậu duệ nào; mọi thao tác tạo chu trình MUST bị từ chối không ghi dữ liệu một phần.
- **FR-027**: Category MAY đồng thời chứa món và category con; món vẫn thuộc đúng một category trực tiếp.
- **FR-028**: Promotion gắn category MUST bao phủ món thuộc category đó và toàn bộ hậu duệ, theo điều kiện promotion hiện hành.
- **FR-029**: Kích hoạt lại tổ tiên MUST không tự thay đổi trạng thái hoạt động riêng của hậu duệ hoặc món.

### Key Entities

- **Nhóm món (Category)**: Một node trong cây thực đơn; có Parent tùy chọn, nhiều Children, mã bất biến, thông tin hiển thị/trạng thái và có thể chứa nhiều món trực tiếp.
- **Món ăn (Food)**: Món thuộc đúng một nhóm; nằm ngoài phạm vi quản trị của feature này nhưng quyết định ảnh hưởng khi nhóm ngừng hoạt động.
- **Quyền truy cập nhóm món**: Quyền xem menu, đọc danh sách, tạo và cập nhật nhóm được cấp cho người dùng thông qua Identity Application code `restaurant`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Ít nhất 95% người dùng được cấp quyền tìm thấy một nhóm theo mã hoặc tên trong không quá 15 giây với danh sách 10.000 nhóm.
- **SC-002**: Ít nhất 95% người dùng được cấp quyền hoàn thành tạo một nhóm hợp lệ trong không quá 60 giây ở lần thử đầu tiên.
- **SC-003**: 100% trường hợp mã trùng, dữ liệu bắt buộc rỗng hoặc dữ liệu vượt giới hạn bị từ chối mà không tạo hoặc cập nhật một phần.
- **SC-004**: 100% người dùng thiếu quyền không nhìn thấy hành động bị cấm và không thể thực hiện thay đổi bằng yêu cầu trực tiếp.
- **SC-005**: 100% lần ngừng hoạt động nhóm có món liên quan yêu cầu xác nhận và không làm mất dữ liệu lịch sử.
- **SC-006**: 100% xung đột cập nhật được phát hiện và không ghi đè âm thầm dữ liệu mới hơn.
- **SC-007**: Danh sách, tìm kiếm, lọc và chuyển trang cho phản hồi có thể thao tác trong không quá 2 giây ở điều kiện vận hành bình thường.
- **SC-008**: Toàn bộ luồng xem, tạo, sửa, ngừng và kích hoạt lại có thể hoàn thành bằng bàn phím ở các kích thước màn hình được hỗ trợ.
- **SC-009**: 100% thao tác di chuyển không tạo chu trình và giữ nguyên cấu trúc bên trong nhánh được chuyển.
- **SC-010**: 100% món trong nhánh có tổ tiên inactive bị loại khỏi bán mới dù category trực tiếp vẫn active.

## Assumptions

- Feature này quản lý category của thực đơn nhà hàng, không phải Product mẫu hiện có dùng để kiểm chứng tích hợp hệ thống.
- Một món thuộc đúng một nhóm; chuyển món giữa nhóm được xử lý trong feature Món ăn & biến thể.
- Category không giới hạn cứng số cấp; kiểm thử chất lượng dùng cây sâu tối thiểu 20 cấp.
- Category có thể vừa chứa món trực tiếp vừa có category con.
- Ngừng hoạt động là cơ chế loại nhóm khỏi bán hàng; hard delete và khôi phục dữ liệu đã xóa nằm ngoài phạm vi.
- Phiên bản đầu cho phép nhập số thứ tự; kéo thả sắp xếp nằm ngoài phạm vi.
- Mã nhóm được chuẩn hóa và so sánh không phân biệt chữ hoa/chữ thường nhưng giữ cách viết đã nhập để hiển thị.
- Giao diện sử dụng Identity Application code `restaurant` và nguồn menu/quyền hiện có từ Identity.
- Các màn hình bán có thể cập nhật trạng thái category khi tải lại; realtime riêng cho category không bắt buộc trong feature này.
- Quản lý món, giá, availability, khuyến mãi và báo cáo nằm trong các feature riêng.
