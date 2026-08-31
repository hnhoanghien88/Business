# Restaurant Specs — Review Index

## Database naming convention

- Database nghiệp vụ: `restaurant_db`.
- Mọi bảng nghiệp vụ MUST dùng tiền tố `restaurant_`; tên entity/domain không mang tiền tố này.
- EF Core dùng `__EFMigrationsHistory` làm bảng metadata kỹ thuật duy nhất không theo tiền tố nghiệp vụ.
- Các view đọc cũng dùng tiền tố `restaurant_`.

Tập spec này phân rã hệ thống nhà hàng thành 10 feature độc lập. Mỗi feature có `spec.md` và `checklists/requirements.md` đã qua quality validation.

| # | Feature | Spec | Phụ thuộc chính | Review trọng tâm |
|---:|---|---|---|---|
| 001 | Nhóm món | [spec](001-restaurant-categories/spec.md) | Identity | Mã bất biến, deactivate, ảnh hưởng món con |
| 002 | Món & biến thể | [spec](002-restaurant-products/spec.md) | 001 | Default variant, lịch sử giá, availability, tương thích Product |
| 003 | Khu vực & bàn | [spec](003-restaurant-layout/spec.md) | Identity | Ranh giới cấu hình/vận hành, disable an toàn |
| 004 | Sơ đồ bàn | [spec](004-restaurant-table-operations/spec.md) | 003 | Một open session/bàn, override sức chứa/đóng bàn |
| 005 | Gọi món | [spec](005-restaurant-ordering/spec.md) | 001, 002, 004 | Order mới mỗi lần gọi, giá server-side, idempotency, gửi bếp từng phần |
| 006 | Bếp | [spec](006-restaurant-kitchen/spec.md) | 005 | State machine item/ticket, quantity sent/remaining, reconnect |
| 007 | Khuyến mãi | [spec](007-restaurant-promotions/spec.md) | 001, 002; tích hợp 005 | Scope explicit, quota concurrency, hoàn usage |
| 008 | Thanh toán | [spec](008-restaurant-payment/spec.md) | 004, 005 | Payment theo session, allocation theo order, Paid/Pending, concurrency |
| 009 | Đơn hàng | [spec](009-restaurant-order-history/spec.md) | 005–008 | Snapshot/audit, cancel/override, reconciliation |
| 010 | Dashboard | [spec](010-restaurant-dashboard/spec.md) | 004–009 | Định nghĩa KPI, net sales vs thực thu, không đếm trùng guest |

## Thứ tự review đề xuất

1. Review quyết định xuyên feature: Identity resource/actions, state machine, cancel/override, timezone/tax.
2. Review Wave 1: 001–003.
3. Review luồng E2E: 004 → 005 → 006 → 008 → đóng bàn.
4. Review promotion (007) và tác động lên ordering/cancellation.
5. Review dữ liệu lịch sử/báo cáo: 009–010.

## Quyết định đã được áp dụng nhất quán

- Mỗi lần gọi thêm tạo một Order mới trong cùng TableSession.
- “Xác nhận & gửi bếp” là một workflow duy nhất đối với người dùng MVP.
- Một OrderItem có thể được gửi bếp qua nhiều phiếu; tổng quantity hợp lệ không vượt quantity đặt.
- Payment thuộc TableSession và được phân bổ vào từng Order.
- Chỉ Payment `Paid` tính vào thực thu/paid balance.
- Category, food và variant có lịch sử dùng deactivate thay hard delete.
- Menu/quyền lấy từ Identity Application `Business`; API nghiệp vụ vẫn là nơi thực thi quyền cuối cùng.

## Điểm cần chủ dự án xác nhận khi review

- Tax/service charge MVP giữ bằng 0 hay có cấu hình ngay từ đầu.
- Chính sách Served ↔ Kitchen Completed và actor thực hiện.
- Mức trạng thái nào cho phép cancel thường; mức nào bắt buộc manager override.
- Ngưỡng SLA bếp và giới hạn khoảng thời gian dashboard/order search.
- Danh sách custom Identity actions sẽ tạo ngay hay tạm dùng CRUD actions rộng hơn.
