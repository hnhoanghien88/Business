# Restaurant Management System — System Design Blueprint

## 1. Mục tiêu

Xây dựng hệ thống quản lý nhà hàng gồm: quản lý khu vực/bàn, menu món ăn và variant, lịch sử giá, trạng thái hết món, gọi món nhiều lần trong một lượt ngồi bàn, mã khuyến mãi, gửi bếp và bếp xác nhận theo từng món, thanh toán, lịch sử trạng thái đơn và dashboard thống kê.

Kiến trúc đề xuất:

- **Frontend hiện tại:** React 19 + Vite 8 + MUI 9/Emotion, JavaScript ES modules, feature-based structure.
- **Backend:** ASP.NET Core Web API, Clean Architecture, CQRS + MediatR.
- **Database:** MySQL 8.x.
- **Realtime:** SignalR cho Kitchen Display và cập nhật trạng thái món/bàn.
- **Authentication/Authorization:** dùng Identity service riêng; các cột `created_by`, `updated_by`, `accepted_by` lưu ID người dùng nhưng không tạo FK sang database Identity.

---

## 2. Domain chính

```text
RestaurantArea
    ↓
RestaurantTable
    ↓
TableSession
    ↓
Order
    ├── OrderItem ─── FoodVariant ─── Food ─── Category
    │                      ↓
    │               FoodPriceHistory
    ├── OrderPromotion ─── PromotionCode
    ├── KitchenOrder ─── KitchenOrderItem
    ├── PaymentAllocation ─── Payment ─── TableSession
    └── OrderStatusHistory
```

Ý nghĩa nghiệp vụ quan trọng:

- `RestaurantTable`: bàn vật lý trong nhà hàng.
- `TableSession`: một lượt khách sử dụng bàn.
- `Order`: một lần gọi món. Một `TableSession` có thể có nhiều `Order`.
- `OrderItem`: snapshot món và giá tại thời điểm đặt.
- `KitchenOrder`: phiếu gửi bếp.
- `KitchenOrderItem`: một phần hoặc toàn bộ số lượng của `OrderItem` được gửi trong một phiếu bếp.
- `Payment`: tiền thực tế thu cho toàn bộ lượt bàn (`TableSession`).
- `PaymentAllocation`: phân bổ một payment của lượt bàn vào một hoặc nhiều order để đối soát.

---

## 3. Danh sách bảng

| Table | Vai trò | Quan hệ chính |
|---|---|---|
| `restaurant_areas` | Khu vực/tầng | 1-N `restaurant_tables` |
| `restaurant_tables` | Bàn, số ghế, trạng thái | N-1 area, 1-N session |
| `table_sessions` | Một lượt khách ngồi bàn | N-1 table, 1-N order |
| `categories` | Cây nhóm món cha–con | self 1-N children, 1-N food |
| `foods` | Thông tin món | N-1 category, 1-N variant |
| `food_variants` | Size/loại/giá hiện tại/trạng thái còn món | N-1 food |
| `food_price_histories` | Lịch sử thay đổi giá | N-1 variant |
| `promotion_codes` | Mã giảm giá | 1-N order promotion |
| `promotion_foods` | Promotion giới hạn theo món | N-N |
| `promotion_categories` | Promotion giới hạn theo category | N-N |
| `orders` | Header đơn hàng | N-1 table session |
| `order_items` | Chi tiết món và snapshot giá | N-1 order |
| `order_promotions` | Promotion đã áp vào đơn | N-1 order/promotion |
| `order_status_histories` | Audit trạng thái order | N-1 order |
| `kitchen_orders` | Phiếu gửi bếp | N-1 order |
| `kitchen_order_items` | Số lượng món được gửi trong từng phiếu bếp | N-1 kitchen order/order item; một order item có thể ở nhiều phiếu |
| `payments` | Thanh toán thực tế của lượt bàn | N-1 table session |
| `payment_allocations` | Phân bổ payment vào các order của lượt bàn | N-1 payment/order |

---

## 4. Trạng thái chuẩn

### RestaurantTable

```text
Available → Occupied → Cleaning → Available
Available → Reserved → Occupied
Disabled
```

Giá trị:

- `Available`
- `Occupied`
- `Reserved`
- `Cleaning`
- `Disabled`

### TableSession

- `Open`
- `Closed`
- `Cancelled`

### Order

- `Draft`
- `Pending`
- `Confirmed`
- `Serving`
- `Completed`
- `Cancelled`

### KitchenOrder / KitchenOrderItem

- `Pending`
- `Accepted`
- `Preparing`
- `Ready`
- `Completed`
- `Rejected`
- `Cancelled`

### Payment

- `Pending`
- `Paid`
- `Failed`
- `Refunded`
- `Cancelled`

---

## 5. Màn hình chức năng và dữ liệu liên quan

| Màn hình | Chức năng | Đọc từ | Ghi vào | API đề xuất |
|---|---|---|---|---|
| Dashboard | KPI ngày/tháng, top món | orders, order_items, payments, table_sessions | - | `GET /api/dashboard` |
| Area Management | CRUD khu vực | restaurant_areas | restaurant_areas | `/api/restaurant-areas` |
| Table Management | CRUD bàn, số ghế | restaurant_tables, restaurant_areas | restaurant_tables | `/api/tables` |
| Table Map | Sơ đồ bàn trống/có khách | restaurant_tables, table_sessions | - | `GET /api/tables/status` |
| Open Table | Mở bàn cho khách | restaurant_tables | table_sessions, restaurant_tables | `POST /api/table-sessions/open` |
| Close Table | Đóng lượt bàn | table_sessions, orders, payments | table_sessions, restaurant_tables | `POST /api/table-sessions/{id}/close` |
| Category Management | CRUD cây nhóm món, di chuyển nhánh | categories | categories | `/api/categories` |
| Food Management | CRUD món | foods, categories | foods | `/api/foods` |
| Variant Management | Size/default variant | food_variants | food_variants | `/api/foods/{id}/variants` |
| Price Management | Đổi giá và lưu history | variants, price histories | cả hai | `PUT /api/food-variants/{id}/price` |
| Availability | Đánh dấu hết/còn món | food_variants | food_variants | `PUT /api/food-variants/{id}/availability` |
| Menu / Order Screen | Chọn món | categories, foods, variants | frontend cart | `GET /api/menu` |
| Create Order | Tạo đơn mới | variant, promotion | orders, order_items, promotions | `POST /api/orders` |
| Add Order Items | Gọi thêm món | order, variants | order_items | `POST /api/orders/{id}/items` |
| Promotion Management | CRUD voucher | promotion tables | promotion tables | `/api/promotions` |
| Apply Promotion | Validate và áp voucher | promotion tables, order | order_promotions, orders | `POST /api/orders/{id}/promotions` |
| Send To Kitchen | Gửi món xuống bếp | order, order_items | kitchen tables | `POST /api/orders/{id}/send-kitchen` |
| Kitchen Display | Danh sách món chờ bếp | kitchen tables, order items | - | `GET /api/kitchen/orders` |
| Kitchen Accept | Bếp xác nhận phiếu | kitchen_orders | kitchen_orders | `POST /api/kitchen/orders/{id}/accept` |
| Start Item | Bắt đầu làm món | kitchen_order_items | kitchen_order_items | `POST /api/kitchen/items/{id}/start` |
| Ready Item | Món đã xong | kitchen_order_items | kitchen_order_items | `POST /api/kitchen/items/{id}/ready` |
| Serve Item | Đã mang món ra bàn | order_items | order_items | `POST /api/orders/items/{id}/serve` |
| Payment | Thanh toán gộp lượt bàn | table_sessions, orders, payments, payment_allocations | payments, payment_allocations, orders | `POST /api/table-sessions/{id}/payments` |
| Order History | Tra cứu đơn | order tables | - | `GET /api/orders` |

---

## 6. Luồng chính: khách vào bàn → thanh toán

```text
1. Nhân viên chọn bàn Available
        ↓
2. Open Table
   - table_sessions: insert Open
   - restaurant_tables.status = Occupied
        ↓
3. Chọn món
   - GET /api/menu
   - MVP lưu cart bằng React state theo `tableSessionId`
   - Chỉ dùng Zustand/global state nếu Spec Kit có task bổ sung dependency và chứng minh cần chia sẻ state xuyên route
        ↓
4. Đặt món
   - Backend nhận variantId + quantity
   - Backend tự đọc current_price
   - Insert orders
   - Insert order_items snapshot
        ↓
5. Gửi bếp
   - Insert kitchen_orders
   - Khóa order_items bằng `SELECT ... FOR UPDATE`
   - Kiểm tra quantity gửi không vượt remaining quantity
   - Insert kitchen_order_items; cho phép chia một OrderItem qua nhiều phiếu
        ↓
6. Bếp xử lý realtime
   Pending → Accepted → Preparing → Ready
        ↓
7. Nhân viên phục vụ
   order_item.status = Served
        ↓
8. Khách có thể gọi thêm
   - Có thể tạo Order mới cùng table_session_id
        ↓
9. Thanh toán
   - Insert payment theo table_session_id
   - Insert payment_allocations cho các order trong lượt bàn
   - Tổng payment Paid của session >= tổng số tiền phải thu
        ↓
10. Complete Order / Close Table Session
        ↓
11. restaurant_tables.status = Cleaning
        ↓
12. Sau khi dọn bàn → Available
```

---

## 7. Quy tắc quan trọng khi tạo Order

Frontend **không được quyết định giá**.

Request chỉ nên gửi:

```json
{
  "tableSessionId": 1001,
  "items": [
    {
      "foodVariantId": 101,
      "quantity": 2,
      "note": "Không hành"
    }
  ],
  "promotionCode": "WELCOME10"
}
```

Backend thực hiện:

```text
foodVariantId
    ↓
Validate is_active + is_available
    ↓
Đọc current_price từ DB
    ↓
Tính subtotal
    ↓
Validate promotion
    ↓
Tính discount/total
    ↓
Insert Order + OrderItems
```

Không nhận `unitPrice` từ frontend để tránh sửa request bằng DevTools/Postman.

---

## 8. Snapshot OrderItem

`order_items` giữ cả khóa và dữ liệu snapshot:

```text
food_id
food_variant_id
food_code
food_name
variant_name
quantity
unit_price
discount_amount
total_amount
```

Mục đích:

- `food_id`, `food_variant_id`: dùng cho thống kê và liên kết.
- `food_name`, `variant_name`, `unit_price`: giữ hóa đơn lịch sử chính xác khi admin đổi tên/giá sau này.

---

## 9. Thiết kế giá

Ba giá trị khác nhau về nghiệp vụ:

```text
food_variants.current_price
= giá hiện tại dùng khi đặt món

food_price_histories.price
= lịch sử giá/audit

order_items.unit_price
= giá khách thực sự mua tại thời điểm đặt
```

Khi đổi giá:

1. Đóng record history hiện hành bằng `effective_to`.
2. Insert history mới.
3. Update `food_variants.current_price`.
4. Toàn bộ thao tác chạy trong một transaction.

---

## 10. Kiểm soát hết món

Không dùng `is_active` thay cho hết hàng.

```text
is_active = món/variant còn được cấu hình trong hệ thống
is_available = hiện tại có thể bán
```

Ví dụ:

```text
FoodVariant: Cơm gà / Default
is_active = true
is_available = false
sold_out_reason = "Hết gà"
```

Frontend vẫn có thể hiển thị món nhưng disable nút đặt và hiện `Hết món`.

Khi bếp cập nhật availability, SignalR có thể broadcast để các Order Screen cập nhật ngay.

---

## 11. Promotion

Validation cơ bản:

```text
is_active?
now nằm trong start_date/end_date?
usage_count < usage_limit?
subtotal >= min_order_amount?
áp dụng đúng food/category?
```

Loại giảm:

```text
Percentage
FixedAmount
```

Công thức:

```text
subtotal_amount = SUM(order_item unit_price × quantity)
discount_amount = promotion discount + item discount
total_amount    = subtotal_amount - discount_amount + tax_amount
```

`order_promotions` lưu snapshot `promotion_code`, `promotion_name`, `discount_amount` để lịch sử không thay đổi.

---

## 12. Kitchen Display

Kitchen Display nên hiển thị theo `KitchenOrder`, nhưng thao tác theo từng `KitchenOrderItem`.

### Gửi bếp một phần và chống gửi vượt số lượng

Một `OrderItem` có thể được chia qua nhiều `KitchenOrder`. Ví dụ khách đặt 5 phần nhưng lần đầu chỉ gửi 3, lần sau gửi 2:

```text
ordered_quantity   = order_items.quantity
sent_quantity      = SUM(kitchen_order_items.quantity
                         WHERE status NOT IN ('Cancelled', 'Rejected'))
remaining_quantity = ordered_quantity - sent_quantity
```

Quy tắc bắt buộc:

- Trong cùng một KitchenOrder, một OrderItem chỉ xuất hiện một lần.
- Tổng quantity ở các phiếu còn hiệu lực không được vượt `order_items.quantity`.
- Khi gửi bếp, backend khóa `order_items` bằng `SELECT ... FOR UPDATE`, tính lại remaining từ DB rồi mới insert toàn bộ phiếu trong một transaction.
- `Cancelled` hoặc `Rejected` giải phóng quantity để được gửi lại; các trạng thái còn lại đều được tính là đã gửi.
- Không nhận `sentQuantity` hoặc `remainingQuantity` từ frontend làm nguồn sự thật.
- View `order_item_kitchen_quantities` cung cấp `OrderedQuantity`, `SentQuantity`, `RemainingQuantity` để đọc; invariant ghi vẫn do transaction application bảo vệ.

Ví dụ:

```text
Bàn 05 — ORD-000123

2 × Cơm gà
Không hành
[Pending] [Bắt đầu]

1 × Pizza Hải sản - Size L
[Preparing] [Hoàn thành]
```

SignalR events gợi ý:

```text
KitchenOrderCreated
KitchenOrderAccepted
KitchenItemPreparing
KitchenItemReady
FoodAvailabilityChanged
OrderStatusChanged
TableStatusChanged
```

---

## 13. Thanh toán gộp theo lượt bàn

Không dùng `orders.total_amount` để đại diện cho tiền đã thu. Payment thuộc `TableSession`, vì một lượt bàn có thể có nhiều order và khách thường thanh toán một lần cho cả lượt.

```text
orders.total_amount        = số tiền phải trả của từng order
payments.amount            = tiền thực tế thu cho cả lượt bàn
payment_allocations.amount = phần payment phân bổ vào từng order
```

Một lượt bàn có thể có nhiều order và nhiều payment:

```text
Order A       300,000
Order B       200,000
Tổng lượt bàn 500,000

Payment 1 / Cash          200,000
  └── Order A             200,000
Payment 2 / BankTransfer  300,000
  ├── Order A             100,000
  └── Order B             200,000
```

Chỉ `payments.status = Paid` mới tính vào thực thu và số đã thanh toán của order. Khi payment chuyển `Paid`, backend phải bảo đảm:

- Payment và tất cả allocation cùng thuộc một `TableSession`.
- Tổng allocation bằng `payments.amount`.
- Tổng allocation Paid của mỗi order không vượt `orders.total_amount`.
- Khóa session, các order và payment liên quan trong transaction để chống thanh toán trùng.
- Chỉ complete từng order khi allocation Paid của order đó đủ; chỉ đóng session khi mọi order đủ điều kiện.

---

## 14. Dashboard và báo cáo

Không cần bảng summary ở giai đoạn đầu. Query trực tiếp:

| KPI | Nguồn |
|---|---|
| Số đơn | `orders` |
| Số món bán | `order_items` |
| Gross sales | `orders.subtotal_amount` |
| Discount | `orders.discount_amount` |
| Net sales | `orders.total_amount` |
| Thực thu | `payments.amount WHERE status='Paid'` |
| Khách phục vụ | `SUM(table_sessions.guest_count)` |
| Món bán chạy | group `order_items.food_id` |
| Variant bán chạy | group `order_items.food_variant_id` |
| Bàn sử dụng nhiều | group `table_sessions.table_id` |
| Giá trị đơn trung bình | `AVG(orders.total_amount)` |

Sau này khi dữ liệu rất lớn, có thể dùng Hangfire aggregate sang bảng daily summary.

---

## 15. React feature structure

### Application shell

Frontend dùng shell thống nhất gồm sidebar trái và workspace phải:

```text
┌──────────────────────┬─────────────────────────────────────────┐
│ Sidebar              │ Topbar / breadcrumb / page actions     │
│ - Expanded 240–280px ├─────────────────────────────────────────┤
│ - Collapsed 64–80px  │ Page workspace                          │
│ - Drawer trên mobile │                                         │
└──────────────────────┴─────────────────────────────────────────┘
```

- Desktop cho phép user mở rộng hoặc thu gọn sidebar; ghi nhớ lựa chọn theo user trong local storage.
- Khi thu gọn chỉ hiển thị icon, có tooltip/flyout và active state rõ ràng.
- Tablet/mobile dùng drawer overlay, mặc định đóng; không chiếm chiều rộng workspace.
- Sidebar/topbar cố định, chỉ page workspace cuộn; table map và kitchen dùng toàn chiều rộng.
- Menu được tạo từ authorization của Identity; ẩn item không có quyền nhưng route/API vẫn kiểm tra lại.
- “Gọi món” đi từ bàn/session cụ thể, không phải menu độc lập. Menu “Thanh toán” mở hàng đợi thu ngân rồi điều hướng vào session.
- Layout và wireframe chi tiết từng màn hình nằm trong `restaurant_menu_screen_specification_map.md`.

### Frontend implementation baseline

- Dùng function component và React hooks; ứng dụng chạy trong `StrictMode`.
- UI dùng MUI Material/Icons và theme chung; không thêm component framework thứ hai.
- Feature API chỉ gọi `apiFetch` trong `shared/api/apiClient.js`, không gọi `fetch` trực tiếp.
- `apiFetch` quản lý Bearer token, refresh một lần khi 401 và chuẩn hóa `ApiError`/Problem Details.
- List dùng server pagination; MUI page zero-based được đổi sang API page one-based tại page/API boundary.
- Form xử lý validation field từ `error.errors`, async state riêng và chặn submit trùng.
- Menu/permission đến từ Identity session; client ẩn action, backend vẫn kiểm tra authorization.
- Stack hiện chưa có React Router, global state library, query library, test runner hoặc SignalR client. Mọi Spec Kit cần task cài/thiết kế rõ nếu sử dụng, không giả định dependency đã tồn tại.
- Chạy `npm run lint` và `npm run build` trước khi hoàn tất frontend feature.
- Chi tiết technology baseline và coding rules lấy từ feature `restaurant/products` nằm trong `restaurant_menu_screen_specification_map.md`.

### Identity integration readiness

- Application Identity có tên `Restaurant`, dùng code/audience `restaurant`; database nghiệp vụ là `restaurant_db`.
- Permission theo contract `<ResourceCode>.<ActionCode>` và so sánh case-sensitive; menu nghiệp vụ cần `<ResourceCode>.ViewMenu`.
- Mỗi feature phải chuẩn bị Identity Resource, Permissions, Menu, RolePermissions và test-user assignment trước hoặc cùng wave triển khai.
- Business API dùng permission policy ở endpoint; client chỉ dùng authorization context để render menu/action.
- Runtime authorization của Identity hiện trả `Id`, `Name`, `Route`, `Children` cho menu. Cần bổ sung `Code` và `Icon` trước khi triển khai sidebar icon/code-driven hoàn chỉnh, hoặc có fallback theo route.
- Permission thay đổi làm tăng PermissionVersion; test phải bao phủ token cũ, refresh/login lại và authorization context mới.
- Seed Identity phải idempotent, có rollback, không dùng ID hard-code giữa môi trường; lookup bằng Application/Resource/Action/Menu code.
- Ma trận Resource/Menu/Actions/Role và Definition of Ready chi tiết nằm trong mục 2.5 của `restaurant_menu_screen_specification_map.md`.

```text
src/
├── app/
│   ├── router/
│   └── providers/
│
├── features/
│   ├── dashboard/
│   ├── restaurant-area/
│   ├── restaurant-table/
│   ├── table-session/
│   ├── category/
│   ├── food/
│   │   ├── food-list/
│   │   ├── food-form/
│   │   ├── food-variant/
│   │   ├── food-price/
│   │   └── availability/
│   ├── menu/
│   ├── cart/
│   ├── order/
│   ├── promotion/
│   ├── kitchen/
│   ├── payment/
│   └── reports/
│
├── shared/
│   ├── components/
│   ├── hooks/
│   ├── api/
│   ├── utils/
│   └── types/
```

---

## 16. ASP.NET Core Clean Architecture

```text
src/
├── Restaurant.Domain/
├── Restaurant.Application/
├── Restaurant.Infrastructure/
└── Restaurant.Api/
```

Application features:

```text
Application/
├── RestaurantAreas/
├── RestaurantTables/
├── TableSessions/
├── Categories/
├── Foods/
├── FoodVariants/
├── FoodPrices/
├── Promotions/
├── Orders/
├── Kitchen/
├── Payments/
└── Reports/
```

Ví dụ Order CQRS:

```text
Orders/
├── Commands/
│   ├── CreateOrder/
│   ├── AddOrderItems/
│   ├── ApplyPromotion/
│   ├── CancelOrder/
│   └── CompleteOrder/
└── Queries/
    ├── GetOrder/
    ├── GetOrders/
    └── GetOrderHistory/
```

Kitchen:

```text
Kitchen/
├── Commands/
│   ├── SendToKitchen/
│   ├── AcceptKitchenOrder/
│   ├── StartKitchenItem/
│   ├── MarkKitchenItemReady/
│   └── CompleteKitchenItem/
└── Queries/
    └── GetKitchenOrders/
```

---

## 17. Transaction boundaries

Những nghiệp vụ sau phải chạy DB transaction:

### Open Table

```text
Insert table_session
Update restaurant_table = Occupied
```

### Create Order

```text
Insert order
Insert order_items
Insert order_promotions (nếu có)
Insert order_status_history
```

### Change Price

```text
Close current history
Insert new history
Update current_price
```

### Send To Kitchen

```text
Insert kitchen_order
Lock order_items và tính remaining quantity
Insert kitchen_order_items
Update order/order_item status nếu cần
```

### Payment / Complete

```text
Lock table_session và các orders
Insert payment theo table_session_id
Insert payment_allocations
Validate tổng allocation = payment amount
Recalculate Paid theo từng order và toàn session
Complete từng order khi đủ điều kiện
Insert order status history
```

---

## 18. Validation nghiệp vụ cần có

- Không mở bàn nếu table đang `Occupied` hoặc `Disabled`.
- `guest_count > 0`; có thể cảnh báo nếu lớn hơn `capacity`.
- Không đặt variant `is_active = false` hoặc `is_available = false`.
- Backend luôn đọc lại `current_price`.
- Không áp promotion hết hạn / vượt usage limit.
- Không gửi cùng một OrderItem xuống bếp lặp ngoài chủ đích.
- Không đóng TableSession nếu còn order chưa xử lý/thanh toán, trừ khi role có quyền override.
- Không Complete order nếu chưa đủ điều kiện thanh toán theo policy.
- Mọi thay đổi Order status nên ghi `order_status_histories`.

---

## 19. API response cho Menu

Category được trả theo cây. Mỗi node có thể đồng thời có `foods` trực tiếp và `children`; node chỉ xuất hiện trong menu bán khi nó và toàn bộ tổ tiên active.

```json
[
  {
    "id": 1,
    "code": "FOOD",
    "name": "Món ăn",
    "children": [],
    "foods": [
      {
        "id": 10,
        "code": "COM_GA",
        "name": "Cơm gà",
        "imageUrl": null,
        "variants": [
          {
            "id": 100,
            "code": "DEFAULT",
            "name": "Default",
            "price": 50000,
            "isDefault": true,
            "isAvailable": true,
            "soldOutReason": null
          }
        ]
      }
    ]
  }
]
```

Nếu variant là `Default`, frontend không cần hiển thị chữ `Default`.

---

## 20. Thứ tự triển khai đề xuất

1. Categories / Foods / Variants / Price.
2. Restaurant Areas / Tables / Table Map.
3. TableSession Open/Close.
4. Menu + Cart.
5. Create Order + Add Order Items.
6. Kitchen workflow + SignalR.
7. Payment.
8. Promotion.
9. Dashboard / Reports.
10. Các chức năng nâng cao: reservation, chuyển bàn, gộp bàn, tách hóa đơn, inventory nguyên liệu.

---

## 21. Phạm vi chưa có trong schema hiện tại

Có thể bổ sung ở phase sau mà không phá kiến trúc chính:

- Đặt bàn trước (`reservations`).
- Nhiều chi nhánh (`branches`).
- Topping / modifier / add-on.
- Combo / buy-one-get-one / promotion engine nâng cao.
- Kho nguyên liệu (`ingredients`, `recipes`, `ingredient_inventory`).
- In hóa đơn / invoice.
- Refund chi tiết.
- Delivery address / shipper.
- Customer loyalty / points.
- Gộp bàn, chuyển bàn, tách bill.

---

## 22. File database

Import file `restaurant_database.sql` vào MySQL 8.x. Script sẽ tạo database `restaurant_db`, toàn bộ table, foreign key, index, check constraint và một ít seed data cơ bản.
