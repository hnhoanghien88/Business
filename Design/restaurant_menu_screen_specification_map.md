# Restaurant Management — Bản đồ menu, màn hình và phạm vi Spec Kit

> Nguồn phân tích: `restaurant_system_design.md`, `restaurant_db.sql` và hiện trạng `Business-client` ngày 2026-08-27.  
> Mục tiêu: dùng tài liệu này làm bản đồ phạm vi để tạo **một Spec Kit cho từng menu/nhóm nghiệp vụ**, tránh bỏ sót màn hình, hành động, trạng thái và quy tắc liên màn hình.

## 1. Kết luận và nguyên tắc phân rã

Hệ thống nên có 6 nhóm menu cấp một, gồm 10 feature có thể triển khai độc lập theo thứ tự phụ thuộc:

| Thứ tự | Menu cấp một | Menu/màn hình | Mã feature đề xuất | Phase |
|---:|---|---|---|---:|
| 1 | Tổng quan | Dashboard | `restaurant-dashboard` | 4 |
| 2 | Vận hành | Sơ đồ bàn | `restaurant-table-operations` | 2 |
| 3 | Vận hành | Gọi món tại bàn | `restaurant-ordering` | 2 |
| 4 | Vận hành | Bếp | `restaurant-kitchen` | 3 |
| 5 | Vận hành | Thanh toán | `restaurant-payment` | 3 |
| 6 | Giao dịch | Đơn hàng | `restaurant-order-history` | 3 |
| 7 | Thực đơn | Nhóm món | `restaurant-categories` | 1 |
| 8 | Thực đơn | Món ăn & biến thể | `restaurant-products` | 1 |
| 9 | Khuyến mãi | Mã khuyến mãi | `restaurant-promotions` | 3 |
| 10 | Thiết lập | Khu vực & bàn | `restaurant-layout` | 1 |

Không nên biến “Mở bàn”, “Đóng bàn”, “Đổi giá”, “Hết món”, “Gửi bếp”, “Áp mã” thành menu riêng. Đây là hành động hoặc màn hình con trong một ngữ cảnh nghiệp vụ. Cart cũng là panel của màn hình gọi món, không phải một menu điều hướng.

### Phạm vi phiên bản đầu

- Chỉ phục vụ tại bàn (`DineIn`) trong luồng vận hành chính.
- Không đưa `Reserved` lên UI cho đến khi có feature đặt bàn; trạng thái vẫn có thể tồn tại trong DB.
- `TakeAway`, `Delivery`, topping, combo, kho, loyalty, chuyển/gộp bàn, tách bill, refund chi tiết và in hóa đơn thuộc phase sau.
- Hỗ trợ nhiều lần gọi món trong một `TableSession`; mỗi lần xác nhận giỏ tạo một `Order` mới. Đây là cách hiểu hiện tại của blueprint.

## 2. Vai trò và ma trận quyền tối thiểu

Tên role chỉ là gợi ý; hệ thống Identity nên cấp permission chi tiết, UI ẩn/disable hành động và API luôn kiểm tra lại.

| Nhóm chức năng | Quản trị | Quản lý | Phục vụ | Bếp | Thu ngân |
|---|:---:|:---:|:---:|:---:|:---:|
| Dashboard/báo cáo | ✓ | ✓ | — | — | Có thể xem |
| Cấu hình khu vực, bàn | ✓ | ✓ | Xem | — | Xem |
| Cấu hình nhóm/món/biến thể/giá | ✓ | ✓ | Xem menu | Xem + đổi availability nếu cấp quyền | Xem menu |
| Mở bàn, gọi món, gửi bếp, phục vụ | ✓ | ✓ | ✓ | — | Có thể xem |
| Xử lý phiếu bếp | ✓ | ✓ | Xem | ✓ | — |
| Thanh toán | ✓ | ✓ | Theo quyền | — | ✓ |
| Quản lý khuyến mãi | ✓ | ✓ | Chỉ áp dụng | — | Chỉ áp dụng |
| Tra cứu/hủy/override | ✓ | ✓ | Hạn chế | Hạn chế theo món | Theo quyền |

Permission code nên đi theo hành động, ví dụ: `restaurant.tables.view`, `restaurant.tables.open`, `restaurant.orders.create`, `restaurant.kitchen.update`, `restaurant.payments.create`, `restaurant.reports.view`; không khóa nghiệp vụ chỉ bằng tên role.

## 2.1 Application shell và menu trái dùng chung

Toàn bộ màn hình desktop dùng một layout thống nhất: menu điều hướng bên trái, vùng xử lý bên phải. Sidebar có thể giữ nguyên hoặc thu gọn theo lựa chọn người dùng.

```text
┌──────────────────────┬─────────────────────────────────────────────────────┐
│ Logo + tên hệ thống  │ Topbar: breadcrumb | tiêu đề | hành động | tài khoản│
│ [‹ Thu gọn]          ├─────────────────────────────────────────────────────┤
│                      │                                                     │
│ Tổng quan            │ VÙNG XỬ LÝ CỦA MÀN HÌNH                            │
│ Vận hành             │                                                     │
│  • Sơ đồ bàn         │ Filter / toolbar                                   │
│  • Bếp               │ Nội dung chính                                     │
│ Giao dịch            │ Drawer/dialog phụ khi cần                          │
│ Thực đơn             │                                                     │
│ Khuyến mãi           │                                                     │
│ Thiết lập            │                                                     │
│                      │                                                     │
│ Người dùng / Đăng xuất│                                                    │
└──────────────────────┴─────────────────────────────────────────────────────┘
```

### Trạng thái sidebar

| Chế độ | Desktop | Tablet | Mobile |
|---|---|---|---|
| Mở rộng | Rộng khoảng 240–280px; icon + tên + nhóm menu | Drawer overlay | Drawer overlay |
| Thu gọn | Rộng khoảng 64–80px; chỉ icon, tooltip khi hover/focus | Không chiếm chỗ cố định | Không chiếm chỗ cố định |
| Mặc định | Khôi phục lựa chọn gần nhất | Đóng | Đóng |

- Nút toggle luôn có `aria-label`, hỗ trợ keyboard và không làm mất focus khi sidebar đổi kích thước.
- Ghi lựa chọn `expanded/collapsed` vào local storage theo người dùng; nếu chưa có thì mặc định mở trên desktop.
- Khi thu gọn, menu cha dùng popover/flyout hoặc tooltip có tên; item đang chọn vẫn có dấu hiệu active rõ ràng.
- Chỉ render menu user được Identity cấp quyền; route trực tiếp và API vẫn phải kiểm tra quyền.
- Nhóm menu có thể expand/collapse độc lập và ghi nhớ; tự mở nhóm chứa route hiện tại.
- Sidebar và topbar cố định; chỉ vùng nội dung cuộn. Không để hai scrollbar dọc cạnh nhau.
- Vùng phải dùng `min-width: 0` để bảng/chart không đẩy vỡ layout; nội dung có max-width tùy màn hình, riêng table map/kitchen dùng toàn chiều rộng.

### Cấu trúc vùng xử lý bên phải

```text
PageHeader
├── Breadcrumb (ẩn bớt trên mobile)
├── PageTitle + mô tả/trạng thái realtime
└── PrimaryActions

PageBody
├── FilterToolbar (sticky nếu danh sách dài)
├── Summary/KPI (nếu có)
├── MainContent
└── Pagination / status bar
```

- Hành động chính đặt trên PageHeader; hành động theo dòng nằm trong row/card; hành động nguy hiểm luôn xác nhận.
- Filter có nút áp dụng/xóa, thể hiện số filter đang bật và đồng bộ query string để refresh/back không mất ngữ cảnh.
- Drawer bên phải dùng cho xem nhanh/chỉnh sửa nhẹ; dialog cho xác nhận; route riêng cho workflow dài như gọi món và thanh toán.
- Breakpoint gợi ý: mobile `< 768px`, tablet `768–1199px`, desktop `≥ 1200px`; con số cuối cùng theo theme chung.

## 2.2 Cây menu trái đề xuất

```text
Tổng quan
└── Dashboard

Vận hành
├── Sơ đồ bàn
├── Bếp
└── Thanh toán                 (có thể vào từ bàn; menu là hàng đợi thu ngân)

Giao dịch
└── Đơn hàng

Thực đơn
├── Nhóm món
└── Món ăn & biến thể

Khuyến mãi
└── Mã khuyến mãi

Thiết lập
└── Khu vực & bàn
```

“Gọi món tại bàn” không cần item menu riêng vì phải đi từ bàn/session cụ thể. “Thanh toán” trong sidebar mở danh sách các lượt bàn đang chờ/thiếu thanh toán; từ đó mới vào màn hình payment của session.

## 2.3 Technology baseline theo Business-client hiện tại

Feature tham chiếu: `Business-client/src/features/restaurant/products`. Các Spec Kit frontend phải dùng baseline dưới đây, không tự thay stack nếu chưa có quyết định kiến trúc riêng.

| Thành phần | Công nghệ/convention hiện có | Quy tắc áp dụng |
|---|---|---|
| Runtime UI | React `19.2.x`, React DOM `19.2.x` | Function component + hooks; chạy trong `StrictMode`. |
| Build/dev | Vite `8.2.x`, ES modules (`type: module`) | Import có phần mở rộng `.js/.jsx` như code hiện tại. |
| Component library | MUI Material `9.3.x`, MUI Icons | Ưu tiên MUI component và theme; không thêm UI framework thứ hai. |
| Styling | MUI theme + global `index.css` + className | Token màu/font/radius đặt trong theme; `sx` chỉ cho chỉnh cục bộ nhỏ. |
| Emotion | `@emotion/react`, `@emotion/styled` | Là styling engine của MUI, không dùng trực tiếp nếu MUI/class CSS đã đủ. |
| HTTP | Native `fetch` qua `shared/api/apiClient.js` | Feature không gọi `fetch` trực tiếp; luôn dùng `apiFetch`. |
| Authentication | Bearer access token + refresh một lần khi 401 | `apiClient` chịu trách nhiệm token/refresh/logout; feature không tự xử lý token. |
| Authorization | Menus và permissions từ Identity session | Sidebar lấy menu được cấp; button/action kiểm tra permission; API vẫn enforce. |
| Routing hiện tại | History API tự quản lý trong `App.jsx` | Chưa có React Router; khi số route tăng phải tạo router module hoặc có ADR trước khi thêm dependency. |
| State hiện tại | `useState`, `useEffect`, `useCallback` | Local UI/server state đơn giản giữ trong page; chưa coi Zustand/Redux/React Query là dependency sẵn có. |
| Lint | Oxlint `1.75.x` | Mọi code mới phải qua `npm run lint`. Disable rule chỉ tại dòng, kèm lý do. |
| Test | Chưa có test runner trong `package.json` | Spec phải nêu nhu cầu test; muốn dùng Vitest/Testing Library/Playwright phải thêm dependency/task rõ ràng. |
| Realtime | Chưa có SignalR client trong `package.json` | Feature bếp/bàn phải bổ sung `@microsoft/signalr` như một task hạ tầng, không giả định đã có. |

### Theme và ngôn ngữ

- Theme hiện dùng primary `#4b5ed3`, background `#eef2f7`, font `Inter/system UI`, radius cơ sở `4px`; feature mới kế thừa theme thay vì hard-code lại.
- Chuỗi giao diện nghiệp vụ dùng tiếng Việt thống nhất. Tên code, component, biến và API dùng tiếng Anh.
- Tiền tệ/decimal/ngày giờ phải qua shared formatter; không định dạng thủ công khác nhau giữa các feature. Nếu formatter chưa có, tạo ở `shared/utils` trước khi nhân rộng.
- Không hiển thị raw enum tiếng Anh cho người dùng; ánh xạ status sang label tiếng Việt và màu dùng chung.

## 2.4 Feature structure và coding rules

### Cấu trúc chuẩn

```text
src/features/restaurant/<feature>/
├── <Feature>Page.jsx                 # orchestration, query state, permission
├── api/
│   └── <feature>Api.js               # endpoint và request/response transport
├── components/
│   ├── <Entity>FormDialog.jsx        # component trình bày/use case nhỏ
│   └── ...
├── hooks/                            # chỉ tạo khi logic được tái dùng/phức tạp
└── utils/                            # logic thuần riêng feature nếu cần
```

- Giữ page mỏng dần khi feature lớn: page điều phối load/filter/dialog; bảng, form và confirmation tách thành component.
- API module không chứa state UI; component không ghép URL API.
- Shared chỉ nhận code thật sự dùng từ hai feature trở lên; không đưa mọi helper vào shared quá sớm.
- Một file export một component chính; component PascalCase, function/variable camelCase, API function theo động từ (`get`, `create`, `update`, `delete`).
- Tránh barrel export cho đến khi codebase thống nhất; dùng import path trực tiếp như feature Product.

### Server list, tìm kiếm và pagination

- Danh sách lớn dùng pagination server-side. UI MUI `TablePagination` dùng page zero-based; API dùng page one-based, nên chuyển `page + 1` đúng tại lời gọi API.
- Khi search/filter/page size đổi phải reset page về `0`.
- Tách `searchInput` và `search` đã áp dụng để không gọi API theo từng phím; form submit trim chuỗi rồi mới load.
- API list trả tối thiểu `{ items, totalCount }`; page không phụ thuộc metadata không được định nghĩa.
- Các filter quan trọng của màn hình mới phải đồng bộ query string theo quy tắc layout; Product hiện tại là baseline đơn giản và sẽ được nâng cấp khi chuẩn router hoàn thiện.
- Sau delete item cuối trang, lùi một trang nếu không phải trang đầu; nếu không thì reload trang hiện tại.

### API và error contract

- Tất cả endpoint restaurant dùng prefix `/api/restaurant/...`; `apiFetch` tự thêm proxy base `/backend`.
- Body JSON chỉ set khi có body; path parameter luôn `encodeURIComponent`.
- Success response theo envelope hiện tại có `data`; API function trả `response.data` để UI không biết envelope transport.
- Error chuẩn là Problem Details: ưu tiên `detail`, sau đó `title`; validation errors là object `errors` theo field.
- `ApiError` giữ `message`, `status`, `errors`. Page có thể ánh xạ riêng 403; form ánh xạ `errors[field]?.[0]` và vẫn hiển thị lỗi tổng quát.
- 401 refresh đúng một lần tại shared client; feature không retry authentication. Retry command tạo giao dịch chỉ được làm khi có idempotency key.
- Không nuốt lỗi. List hiện Alert trong card; form/dialog hiện Alert gần đầu form; delete error nằm ngay trong confirmation dialog.

### Component state và async actions

- Mỗi async context có state riêng: `isLoading`, `isSaving`, `isDeleting`; không dùng một cờ loading cho cả trang.
- Trước request xóa lỗi cũ; reset cờ trong `finally`.
- Trong khi save/delete: disable nút submit/cancel phù hợp, không cho đóng dialog gây request trùng.
- Sau create/update thành công reload nguồn dữ liệu server rồi mới đóng; sau delete xử lý page boundary.
- Remote data load trong `useEffect` qua function `useCallback` với dependency đầy đủ. Không disable lint rộng; comment hiện tại của Product là mẫu cho exception có lý do.
- Không optimistic update cho giá, payment, order hoặc kitchen status nếu chưa có version/conflict handling.

### Form và dialog

- Form dùng `<form onSubmit>` để hỗ trợ Enter và accessibility.
- Validate required/trim/length ở client để phản hồi nhanh, nhưng backend là nguồn validation cuối cùng.
- Lỗi field từ backend đặt ở `error/helperText`; lỗi nghiệp vụ tổng quát dùng `Alert`.
- Code/identity field bị khóa khi edit nếu API dùng code làm key như Product hiện tại; entity mới ưu tiên immutable ID cho route và code là business identifier.
- Confirmation bắt buộc cho delete/cancel/destructive action; hiển thị rõ tên + code đối tượng.
- Product đang hard delete; các entity nhà hàng có lịch sử giao dịch phải dùng deactivate theo tài liệu, không sao chép hành vi delete một cách máy móc.
- Dialog nhỏ dùng `maxWidth="sm"`; workflow dài theo layout dùng drawer/full-screen mobile hoặc route riêng.

### Permission và menu rules

- Page nhận `grantedPermissions` từ shell/session và chuyển sang `Set` trước khi kiểm tra nhiều lần.
- Không render action user không có quyền; không chỉ disable nếu việc lộ hành động không cần thiết.
- Feature Product hiện dùng `Product.Create`, `Product.Update`, `Product.Delete` và menu code/route từ Identity. Khi refactor phải migrate sang Resource `Foods` và duy trì redirect route cũ trong giai đoạn chuyển đổi.
- Permission cho feature mới phải được chốt đồng thời ở Identity/API/client. Convention mục tiêu có thể là `Restaurant.<Resource>.<Action>`; không tự đổi các permission production hiện có trong một feature UI.
- Menu fallback chỉ dùng cho bootstrap/development. Khi hệ thống menu hoàn chỉnh, route không được cấp phải trả forbidden/not-found thay vì tự tạo menu mặc định.

### Definition of Done frontend

- `npm run lint` và `npm run build` thành công.
- Có đủ loading, empty, error, forbidden và success feedback theo loại màn hình.
- Action permission được ẩn đúng; API 401/403 không làm UI treo.
- Pagination/search/filter giữ đúng state sau CRUD và back navigation.
- Dialog không submit trùng; button/icon có label hoặc tooltip; thao tác được bằng keyboard.
- Kiểm tra ít nhất desktop expanded/collapsed sidebar, tablet drawer và mobile layout.
- Không thêm package mới ngoài `package.json` mà thiếu lý do, task cài đặt và cập nhật tài liệu technology baseline.

## 2.5 Chuẩn bị tích hợp Identity trước khi phát triển feature

Nguồn đối chiếu: dự án `E:/Identity`, đặc biệt runtime authorization feature `010-runtime-authorization`, menu feature `005-manage-menus` và schema Identity hiện tại.

### Hiện trạng đã có

- Identity đã có Application tên `Restaurant`, code/audience `restaurant`.
- Restaurant client đăng nhập/refresh với `applicationCode=restaurant`, sau đó gọi `GET /authorization?applicationCode=restaurant` để nhận `roles`, `permissions`, `menus`.
- Business API xác thực JWT và dùng dynamic authorization policy; mỗi permission check gọi Identity authorization context bằng access token hiện tại.
- Identity đã có actions chung: `Read`, `Create`, `Update`, `Delete`, `Export`, `Import`, `ViewMenu`.
- Resource `Product`, menu `/product` và các permission `Product.*` đã tồn tại; đây là mẫu tích hợp đang hoạt động.
- Menu nghiệp vụ chỉ được Identity trả về khi user có `<ResourceCode>.ViewMenu`; menu cha có thể được giữ nếu có con hợp lệ.
- Khi role/permission thay đổi, `PermissionVersion` làm token cũ hết hiệu lực; client phải refresh/login lại theo behavior của Identity.

### Khoảng trống phải xử lý

1. **Runtime menu thiếu metadata:** `RuntimeMenuResponse` hiện chỉ trả `Id`, `Name`, `Route`, `Children`, chưa trả `Code` và `Icon`. Sidebar mới cần icon ổn định và code để mapping component/active route. Identity nên bổ sung ít nhất `Code`, `Icon`, và nếu cần `ResourceCode`; Business client phải vẫn chịu được null trong giai đoạn rollout.
2. **Tên `IsVisible` có semantics đặc thù:** implementation hiện tại flatten node khi `IsVisible=true`, còn node `false` có thể xuất hiện nếu có `ViewMenu`/children. Khi seed phải kiểm thử response runtime thực tế, không suy đoán chỉ từ tên cột.
3. **Actions chung chưa đủ nghiệp vụ:** `Update` có thể cấp quyền quá rộng cho bếp/phục vụ/thu ngân. Cần chốt dùng action nghiệp vụ chi tiết hay tách resource nhỏ trước khi seed.
4. **Chưa có resources/permissions/menu tree/roles cho 9 feature còn lại.** Nếu làm UI trước, user đăng nhập sẽ không nhìn thấy route và API trả 403.
5. **Business API gọi Identity qua HTTP cho permission check:** cần kiểm tra timeout, failure policy, cache và tải khi Kitchen/Order có nhiều command; không được fail-open cho authorization.

### Convention Identity bắt buộc

- Application name: `Restaurant`.
- Application code/audience: `restaurant` (lowercase, so sánh case-sensitive).
- Business database name: `restaurant_db`.
- Resource code: PascalCase, ổn định sau khi phát hành, không dùng tiền tố `Restaurant`; ví dụ `TableOperations`, `Kitchens`, `Payments`.
- Permission code: `<ResourceCode>.<ActionCode>`, so sánh ordinal/case-sensitive.
- Mỗi menu route nghiệp vụ gắn đúng một Resource và resource đó phải có `ViewMenu`.
- Menu nhóm không cần permission thao tác; chỉ chứa children và phải được cấu hình theo semantics runtime đã kiểm thử.
- Route trong Identity phải trùng tuyệt đối route canonical của Business client; redirect legacy `/product` xử lý ở client, không tạo hai menu.
- Icon lưu dưới dạng key trung lập đã thống nhất, ví dụ MUI icon name; client dùng allow-list mapping, không dynamic import từ chuỗi tùy ý.
- Permission constant phải tồn tại đồng thời ở Identity seed, Business Application/API và Business client capability map; không viết string rải rác trong component/controller.

### Resource và menu cần chuẩn bị

| Nhóm/menu | Resource | Menu code | Menu name | Route canonical | Permission menu |
|---|---|---|---|---|---|
| Dashboard | `Dashboard` | `dashboard` | `Dashboard` | `/restaurant/dashboard` | `Dashboard.ViewMenu` |
| Sơ đồ bàn | `TableOperations` | `table-operations` | `Table operations` | `/restaurant/table-operations` | `TableOperations.ViewMenu` |
| Bếp | `Kitchens` | `kitchens` | `Kitchens` | `/restaurant/kitchens` | `Kitchens.ViewMenu` |
| Thanh toán | `Payments` | `payments` | `Payments` | `/restaurant/payments` | `Payments.ViewMenu` |
| Đơn hàng | `Orders` | `orders` | `Orders` | `/restaurant/orders` | `Orders.ViewMenu` |
| Nhóm món | `Categories` | `categories` | `Categories` | `/restaurant/categories` | `Categories.ViewMenu` |
| Món ăn & biến thể | `Foods` | `foods` | `Foods` | `/restaurant/foods` | `Foods.ViewMenu` |
| Mã khuyến mãi | `Promotions` | `promotions` | `Promotions` | `/restaurant/promotions` | `Promotions.ViewMenu` |
| Khu vực & bàn | `Layouts` | `layouts` | `Layouts` | `/restaurant/layouts` | `Layouts.ViewMenu` |

`Ordering` không cần menu vì route có context bàn/session, nhưng vẫn cần Resource/permissions để bảo vệ API và các CTA từ Table Map.

### Action nghiệp vụ đề xuất

Giữ actions CRUD chung cho màn hình quản trị. Bổ sung action nghiệp vụ trong Identity khi cần tách quyền:

| Resource | Actions tối thiểu ngoài `ViewMenu/Read` |
|---|---|
| `Layouts` | `Create`, `Update`, `Delete`, `Disable` |
| `TableOperations` | `Open`, `Close`, `MarkClean`, `Override` |
| `Ordering` | `Create`, `SendKitchen`, `Serve`, `Cancel` |
| `Kitchens` | `Accept`, `Start`, `Ready`, `Complete`, `Reject`, `ChangeAvailability` |
| `Payments` | `Create`, `Confirm`, `Cancel`, `Override` |
| `Orders` | `Read`, `Cancel`, `Override`, tùy chọn `Export` |
| `Categories` | `Create`, `Update`, `Delete` (thực tế deactivate nếu có tham chiếu) |
| `Foods` | `Create`, `Update`, `Delete`, `ChangePrice`, `ChangeAvailability` |
| `Promotions` | `Create`, `Update`, `Delete`, `Apply` |
| `Dashboard` | `Read`, tùy chọn `Export` |

Nếu chưa muốn mở rộng `permission_actions`, MVP có thể dùng `Create/Update/Delete`, nhưng phải ghi rõ mapping và chấp nhận quyền rộng hơn. Không dùng role name trong controller để thay cho permission.

### Role template đề xuất

| Role Business | Quyền chính |
|---|---|
| `RestaurantAdmin` | Toàn bộ resources/actions của nhà hàng. |
| `RestaurantManager` | Vận hành + catalog + promotion + reports + override được chọn. |
| `Waiter` | Table map, open/order/send/serve; không đổi giá hoặc quản lý payment mặc định. |
| `Kitchen` | Kitchen read/update và change availability; không xem payment/report tài chính. |
| `Cashier` | Payment queue/create/confirm, đọc order/session cần thiết; không quản trị catalog. |

Role chỉ là template gán permission trong Identity. Business code không hard-code các tên role này.

### Trình tự chuẩn bị cho mỗi Spec Kit

1. Chốt route, Resource code và action list trong spec.
2. Tạo migration/seed **idempotent** bên Identity cho Resource, Permissions, Menu và role-permission mặc định; có down script hoặc rollback plan.
3. Bổ sung permission constants/capability map ở Business API/client.
4. Gắn `[Authorize(Policy = ...)]` cho mọi endpoint; page chỉ dùng permission để trình bày, không thay thế API authorization.
5. Gán role cho ít nhất một test user của từng persona.
6. Login/refresh lại để nhận token có PermissionVersion mới; kiểm tra `/authorization` trả đúng tree và permission codes.
7. Contract test bốn trường hợp: có menu+có action, có menu+thiếu action, thiếu ViewMenu, token permission version cũ.
8. Kiểm tra 401/403/Identity unavailable; authorization không được fail-open.

### Definition of Ready về Identity

Một feature chỉ sẵn sàng implement khi:

- Resource/menu/route/action codes đã được chốt và không trùng trong Application `Business`.
- Identity migration/seed và rollback đã nằm trong plan/tasks.
- Permission constants giữa Identity, Business API và client khớp case tuyệt đối.
- Có test users/roles đại diện và expected authorization payload.
- Runtime menu đủ metadata cho thiết kế sidebar hoặc có fallback được ghi rõ.
- Quyết định cache/timeout/failure behavior của permission check đã được kiểm thử cho luồng dự kiến.

## 3. Chi tiết từng menu và màn hình

## 3.1 Tổng quan / Dashboard

**Route đề xuất:** `/restaurant/dashboard`  
**Mục tiêu:** cung cấp ảnh chụp hoạt động trong khoảng thời gian, không thao tác thay đổi giao dịch.

### Layout đề xuất

```text
[Page title] [Khoảng thời gian] [Làm mới]
[KPI][KPI][KPI][KPI]        hàng 1
[KPI][KPI][KPI][KPI]        hàng 2 hoặc cuộn ngang trên mobile
[Doanh thu/thực thu 8 cột]  [Trạng thái bàn/bếp 4 cột]
[Top món 6 cột]             [Top bàn/variant 6 cột]
```

Desktop dùng grid 12 cột; tablet 2 widget/hàng; mobile 1 widget/hàng. Bộ lọc thời gian nằm PageHeader và sticky khi cần.

### Thành phần và chức năng

- Bộ lọc: hôm nay/7 ngày/tháng/khoảng tùy chọn; mặc định hôm nay theo múi giờ nhà hàng.
- KPI: số đơn hoàn tất, số món bán, doanh thu gộp, giảm giá, doanh thu thuần, thực thu, số khách, giá trị đơn trung bình.
- Biểu đồ doanh thu/thực thu theo ngày hoặc giờ.
- Bảng top món, top biến thể, bàn dùng nhiều.
- Trạng thái nhanh: số bàn trống/đang phục vụ/đang dọn; số phiếu bếp chờ/quá thời gian.
- Loading skeleton, empty state, lỗi tải từng widget và nút thử lại.

### Quy tắc dữ liệu

- KPI bán hàng chỉ tính order `Completed`, loại item `Cancelled`.
- Thực thu chỉ tính payment `Paid`; refund phải được trừ theo policy sau khi có nghiệp vụ refund.
- Phải trả về cả `from`, `to`, `timezone` để người dùng hiểu kỳ báo cáo.
- Không cộng `TableSession.GuestCount` nhiều lần khi join với nhiều order.

### Acceptance tối thiểu

- Đổi bộ lọc cập nhật đồng nhất toàn bộ widget.
- Tổng tiền dùng decimal, hiển thị tiền tệ và múi giờ thống nhất.
- User không có `restaurant.reports.view` không truy cập được cả route lẫn API.

---

## 3.2 Vận hành / Sơ đồ bàn

**Route đề xuất:** `/restaurant/table-operations`  
**Mục tiêu:** màn hình trung tâm của nhân viên phục vụ, thể hiện trạng thái realtime và mở/tiếp tục/đóng lượt bàn.

### Layout đề xuất

```text
[Sơ đồ bàn] [Realtime ●]                  [Mở bàn nhanh]
[Khu vực tabs] [Tìm bàn] [Trạng thái] [Chú thích màu]
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│ Bàn A1 │ │ Bàn A2 │ │ Bàn A3 │ │ Bàn A4 │  responsive card grid
└────────┘ └────────┘ └────────┘ └────────┘
                                      [Detail drawer →]
```

Desktop hiển thị 4–6 card/hàng và mở chi tiết bằng drawer phải 360–480px; mobile 2 card/hàng, chạm card mở bottom sheet/full-screen drawer. Không đặt sơ đồ bàn trong bảng dữ liệu.

### Màn hình danh sách/sơ đồ

- Tab hoặc filter theo khu vực; tìm theo mã/tên bàn.
- Card bàn hiển thị: tên, sức chứa, trạng thái, số khách, giờ mở, thời lượng sử dụng, tổng tạm tính và cảnh báo món Ready (nếu có).
- Màu/nhãn riêng cho `Available`, `Occupied`, `Cleaning`, `Disabled`; không chỉ dùng màu để truyền đạt trạng thái.
- SignalR cập nhật `TableStatusChanged`; khi reconnect phải fetch snapshot mới.
- Hành động theo trạng thái:
  - `Available`: Mở bàn.
  - `Occupied`: Xem/gọi thêm, thanh toán, đóng bàn khi đủ điều kiện.
  - `Cleaning`: Xác nhận dọn xong → `Available`.
  - `Disabled`: không mở bàn.

### Dialog mở bàn

- Trường: bàn (readonly), số khách bắt buộc > 0, ghi chú.
- Nếu số khách vượt sức chứa: cảnh báo và yêu cầu xác nhận; backend policy cần chốt có chặn hay không.
- Submit nguyên tử: tạo `TableSession(Open)` và đổi bàn thành `Occupied`.
- Chống double-click/concurrency: chỉ một open session cho một bàn.

### Panel chi tiết lượt bàn

- Thông tin lượt: thời gian, người mở, số khách, ghi chú.
- Danh sách các order của lượt bàn, tổng tiền phải trả, đã thu, còn thiếu.
- CTA: Gọi thêm món, xem order, thanh toán, đóng lượt.

### Đóng bàn và dọn bàn

- Chỉ đóng khi mọi order đã kết thúc và đủ thanh toán; override phải có permission, lý do và audit.
- Đóng session chuyển bàn sang `Cleaning`, không chuyển thẳng `Available`.
- Hành động “Dọn xong” chuyển `Cleaning → Available` và cần chống ghi đè trạng thái mới hơn.

---

## 3.3 Vận hành / Gọi món tại bàn

**Route đề xuất:** `/restaurant/table-operations/:tableId/order`  
**Mục tiêu:** chọn món, xác nhận giá từ backend, tạo lần gọi món và gửi bếp.

### Layout đề xuất

```text
[← Bàn A1 | 4 khách | 00:42]             [Lịch sử gọi món]
┌──────────────────────────────────────┬─────────────────────────┐
│ Search + category tabs              │ GIỎ HÀNG               │
│ [Món][Món][Món] responsive grid     │ item / qty / note       │
│ [Món][Món][Món]                     │ mã khuyến mãi           │
│                                     │ subtotal/discount/total │
│                                     │ [Xác nhận & gửi bếp]    │
└──────────────────────────────────────┴─────────────────────────┘
```

Desktop: menu khoảng 65–70%, cart 30–35% và sticky. Tablet: menu + cart drawer. Mobile: menu toàn màn hình, thanh tóm tắt giỏ cố định dưới; mở cart thành bottom sheet/full screen. Sidebar trái nên tự thu gọn ở màn hình này để ưu tiên diện tích, nhưng user vẫn có thể mở lại.

### Bố cục

- Header: bàn, số khách, thời gian mở, tổng hiện tại, nút quay lại sơ đồ.
- Khu menu: tìm kiếm, category tabs, card món, ảnh, giá từ/giá variant, nhãn hết món.
- Chọn variant trong dialog/drawer nếu có nhiều variant; variant `Default` không cần hiển thị tên.
- Cart cố định: món/variant, số lượng, ghi chú món, xóa, tạm tính; dữ liệu chỉ là tạm thời phía client.
- Khu mã khuyến mãi: nhập mã, validate và hiện lý do không hợp lệ; số tiền cuối cùng do backend tính.

### Hành động và quy tắc

- Không thêm variant inactive/unavailable; nếu availability đổi sau khi đã vào cart, backend từ chối item đó và UI chỉ rõ item cần sửa.
- Frontend chỉ gửi `foodVariantId`, `quantity`, `note`, `promotionCode`; tuyệt đối không gửi giá làm nguồn sự thật.
- Xác nhận giỏ tạo order + snapshot item + promotion + status history trong transaction.
- “Tạo đơn” và “Gửi bếp” nên là hai bước rõ ràng hoặc một CTA có semantics thống nhất. Khuyến nghị MVP: **Xác nhận & gửi bếp** chạy orchestration nguyên tử/idempotent ở mức use case.
- Sau khi thành công: xóa cart, hiển thị order/kitchen number, cập nhật panel lượt bàn.
- Gọi thêm tạo order mới cùng session theo blueprint; không append vào order đã gửi bếp.

### Trạng thái cần xử lý

- Session đã đóng trong lúc đang chọn món.
- Giá vừa thay đổi: backend trả giá mới; UI yêu cầu xác nhận lại nếu tổng thay đổi.
- Món vừa hết, promotion vừa hết hạn/vượt quota, request trùng do retry.
- Mất mạng: giữ cart cục bộ theo `tableSessionId`, nhưng không tự gửi lại nếu chưa có idempotency key.

---

## 3.4 Vận hành / Bếp

**Route đề xuất:** `/restaurant/kitchens`  
**Mục tiêu:** Kitchen Display System realtime, nhóm theo phiếu và thao tác theo từng món.

### Layout đề xuất

```text
[Bếp] [Realtime ●] [Âm thanh] [Fullscreen]
┌──────────────┬──────────────┬──────────────┐
│ CHỜ NHẬN     │ ĐANG LÀM     │ SẴN SÀNG    │
│ ticket cards │ ticket cards │ ticket cards │
│              │              │              │
└──────────────┴──────────────┴──────────────┘
```

Desktop/tablet ngang dùng Kanban 3 cột cuộn độc lập trong vùng nội dung. Mobile dùng tabs theo trạng thái. Có chế độ fullscreen ẩn sidebar/topbar cho màn hình bếp chuyên dụng; thoát fullscreen trả về trạng thái sidebar trước đó.

### Thành phần và chức năng

- Cột/tab: Chờ nhận, Đang làm, Sẵn sàng; có thể có Đã xong để tra cứu ngắn hạn.
- Card phiếu: kitchen no, bàn/order, giờ gửi, thời gian chờ, ghi chú chung.
- Item: số lượng, tên snapshot, variant, ghi chú, trạng thái và timer.
- Hành động phiếu: nhận/từ chối phiếu (từ chối bắt buộc lý do).
- Hành động item: bắt đầu, đánh dấu sẵn sàng, hoàn tất; cập nhật availability món nếu được cấp quyền.
- Sort ưu tiên thời gian gửi; cảnh báo SLA theo ngưỡng cấu hình.
- Âm thanh/notification cho phiếu mới có tùy chọn bật tắt.

### Quy tắc trạng thái

- Item: `Pending → Accepted/Preparing → Ready → Completed`; `Rejected/Cancelled` là nhánh kết thúc.
- Trạng thái kitchen order được suy ra/đồng bộ từ các item theo quy tắc phải nêu trong spec; không cho client tự tính rồi ghi.
- Mọi command phải idempotent hoặc kiểm tra expected status để tránh hai màn bếp cập nhật đè nhau.
- Cho phép gửi một phần quantity của OrderItem qua nhiều phiếu. `sent = SUM(quantity)` của kitchen item không `Cancelled/Rejected`; `remaining = ordered - sent`.
- Command gửi bếp phải khóa OrderItem, tính remaining từ DB và từ chối toàn bộ transaction nếu bất kỳ quantity yêu cầu nào vượt remaining.
- SignalR là kênh cập nhật, API/DB vẫn là nguồn sự thật; reconnect phải tải lại snapshot.
- Khi Ready, màn hình phục vụ nhận sự kiện; khi Served cần làm rõ ánh xạ `order_item` và `kitchen_order_item` (xem mục quyết định mở).

---

## 3.5 Vận hành / Thanh toán

**Route đề xuất:** `/restaurant/table-sessions/:sessionId/payment`  
**Mục tiêu:** thu tiền cho toàn bộ lượt bàn và chỉ đóng bàn khi nghĩa vụ thanh toán hoàn tất.

### Layout đề xuất

Màn hình menu `/restaurant/payments` là hàng đợi thu ngân; route session là workspace thanh toán.

```text
Hàng đợi: [Tìm bàn] [Chờ thanh toán/Thiếu/Pending]
[Bàn | giờ mở | tổng | đã thu | còn lại | CTA Thanh toán]

Workspace:
[← Hàng đợi] [Bàn A1 / Session]           [Đóng lượt khi đủ]
┌──────────────────────────────────────┬─────────────────────────┐
│ Order list + payment history         │ TÓM TẮT                │
│ allocation theo từng order           │ phải trả/đã thu/còn lại│
│                                      │ phương thức + số tiền   │
│                                      │ [Xác nhận thanh toán]  │
└──────────────────────────────────────┴─────────────────────────┘
```

Desktop chia 7/5 hoặc 8/4; panel tóm tắt sticky. Mobile xếp dọc, summary và CTA cố định dưới nhưng không che form. Kết quả payment gateway Pending hiển thị trạng thái riêng, không đóng dialog như thành công.

Payment thuộc `TableSession`; bảng `payment_allocations` phân bổ một payment vào một hoặc nhiều order trong cùng lượt bàn. Thiết kế này hỗ trợ thanh toán gộp nhưng vẫn đối soát và complete từng order chính xác.

### Thành phần và chức năng

- Tổng hợp các order: subtotal, giảm giá, thuế, phải trả, đã thu, còn lại.
- Danh sách lần thanh toán và trạng thái.
- Tạo payment: phương thức (`Cash`, `Card`, `BankTransfer`, `Momo`, `VNPay`, `Other`), số tiền, mã giao dịch, ghi chú.
- Cho phép nhiều payment/split tender; tiền mặt hiển thị tiền khách đưa và tiền thừa (có thể chỉ là UI, payment amount không vượt phần phải thu nếu policy không cho tip).
- Kết quả gateway có Pending/Failed và retry an toàn; không coi Pending là đã thu.
- Biên nhận/xác nhận thành công; nút hoàn tất order và đóng lượt khi đủ điều kiện.

### Quy tắc

- Chỉ `Paid` tính vào đã thu; không sửa/xóa payment Paid trực tiếp.
- Khi chuyển Paid, tổng allocation phải bằng payment amount; mọi order được phân bổ phải thuộc cùng session và không được nhận vượt total amount.
- Backend khóa session, orders và payment liên quan; tạo payment + allocations trong một transaction.
- Không để cùng `TransactionNo` được xác nhận hai lần; schema hiện chưa có unique constraint nên API phải bảo vệ và nên bổ sung index unique theo provider.
- Hoàn tất phải atomic: ghi payment, tính lại tổng paid, đổi trạng thái order, ghi history. Đóng session là bước riêng khi tất cả order đạt điều kiện.
- Không triển khai refund chỉ bằng đổi `Paid → Refunded`; cần feature ledger/refund riêng ở phase sau.

---

## 3.6 Giao dịch / Đơn hàng

**Routes:** `/restaurant/orders`, `/restaurant/orders/:orderId`  
**Mục tiêu:** tra cứu lịch sử và xem đầy đủ audit của một order.

### Layout đề xuất

```text
[Đơn hàng]                          [Export - phase sau]
[Khoảng ngày][Trạng thái][Bàn][Tìm order no][Xóa lọc]
┌──────────────────── data table ──────────────────────┐
│ order no | bàn | thời gian | món | tổng | paid | status│
└──────────────────────────────────────────────────────┘
[pagination]
```

Click row mở route chi tiết; chi tiết dùng header summary và tabs `Món | Bếp | Thanh toán | Lịch sử trạng thái`. Mobile thay table bằng compact cards, filter mở trong drawer.

### Danh sách

- Filter: thời gian, order no, bàn, trạng thái, loại đơn, người tạo; phân trang server-side.
- Cột: số đơn, bàn/lượt, giờ đặt, số món, tổng, đã thu, trạng thái, người tạo.
- Export là phase sau nếu chưa có yêu cầu định dạng và giới hạn dữ liệu.

### Chi tiết

- Header và snapshot order items; promotion, kitchen tickets, payments, status timeline.
- Hành động theo quyền: gửi lại các item chưa gửi bếp, hủy order/item hợp lệ, thanh toán.
- Hủy bắt buộc lý do và audit; item đã Preparing/Ready có thể cần quyền quản lý.
- Dữ liệu lịch sử hiển thị snapshot, không lấy tên/giá hiện tại để thay thế.

---

## 3.7 Thực đơn / Nhóm món

**Route đề xuất:** `/restaurant/categories`  
**Mục tiêu:** quản lý nhóm hiển thị trên menu gọi món.

### Layout đề xuất

PageHeader có nút `Thêm nhóm món`; dưới là search/status filter và tree table có thể mở/thu gọn từng nhánh. Tạo/sửa dùng drawer phải để vẫn thấy cây; mobile dùng full-screen drawer. Form có trường nhóm cha tùy chọn, hiển thị đường dẫn cây và không cho chọn chính node/hậu duệ làm cha.

- Danh sách dạng cây cha–con có tìm kiếm, trạng thái, thứ tự; có thể mở/thu gọn nhánh.
- Trường: parent tùy chọn, code (unique, chuẩn hóa), name, description, display order, active.
- Di chuyển cả nhánh khi đổi parent; chặn chọn chính node hoặc hậu duệ để không tạo chu trình.
- Không hard delete category đã được tham chiếu; dùng deactivate. Khi deactivate phải cảnh báo toàn bộ hậu duệ và món trong nhánh sẽ biến mất khỏi menu bán.
- Category hiệu lực để bán khi chính nó và toàn bộ tổ tiên active; kích hoạt lại cha không tự kích hoạt các node con đã bị tắt riêng.
- Promotion gắn category cha áp dụng cho món thuộc toàn bộ cây con, trừ khi spec promotion quy định scope cụ thể hơn.
- Sắp xếp có thể nhập số ở MVP; drag-and-drop là cải tiến sau.
- Acceptance: validate required/length/duplicate; concurrency conflict; empty/error/loading; permission create/update.

---

## 3.8 Thực đơn / Món ăn & biến thể

**Routes:** `/product` và `/restaurant/products` là route cũ; route canonical là `/restaurant/foods` và giữ redirect tương thích trong giai đoạn chuyển đổi.  
**Mục tiêu:** quản lý food, variant, giá, lịch sử giá và availability trong một ngữ cảnh.

### Layout đề xuất

```text
[Món ăn & biến thể]                              [Thêm món]
[Search][Category][Active][Availability] [Table/Card toggle]
[Ảnh | code/tên | category | variants | giá | trạng thái | actions]

Chi tiết món:
[Thông tin chung] [Biến thể] [Lịch sử giá]
```

Danh sách desktop ưu tiên table; có card mode nếu ảnh quan trọng. Form món dùng drawer; quản lý variant và lịch sử giá dùng route/detail workspace hoặc drawer rộng. Đổi giá và hết món là dialog nhỏ, không nhồi vào form sửa variant.

### Danh sách món

- Filter category, active, availability; tìm code/name; phân trang.
- Cột/card: ảnh, code, name, category, variants, khoảng giá, active/availability, updated date.
- CRUD food: category, code, name, description, image URL/upload policy, display order, active.
- Deactivate thay hard delete nếu đã có order history.

### Chi tiết/biến thể

- Danh sách variant: code, name, current price, default, available, reason, order, active.
- Tạo/sửa/deactivate variant; mỗi food phải có đúng một default variant đang active (DB hiện chưa enforce).
- Đổi giá qua action riêng: giá mới, effective from, lý do (schema chưa có cột reason); transaction đóng history hiện hành, mở history mới, cập nhật current price.
- Tab lịch sử giá readonly, newest first.
- Quick action còn/hết món; hết món yêu cầu/cho phép lý do theo policy và broadcast `FoodAvailabilityChanged`.
- Không cho sửa `CurrentPrice` qua update variant thông thường.

### Hiện trạng cần lưu ý

Client hiện mới render một menu Product tại `/product`; đây là feature đã có code bước đầu, không phải bằng chứng rằng toàn bộ catalog trong tài liệu đã hoàn tất. Khi tạo Spec Kit nên kiểm kê và giữ tương thích phần đang chạy.

---

## 3.9 Khuyến mãi / Mã khuyến mãi

**Routes:** `/restaurant/promotions`, `/restaurant/promotions/:id`  
**Mục tiêu:** cấu hình voucher và phạm vi áp dụng, tách khỏi thao tác áp mã trong order screen.

### Layout đề xuất

Danh sách dùng table với filter hiệu lực/loại giảm và CTA tạo mới. Form promotion dài nên dùng route riêng hoặc full-width drawer, chia step/section: `Thông tin → Điều kiện → Phạm vi → Xem lại`; desktop có panel preview bên phải, mobile preview nằm cuối form.

- Danh sách/filter theo code, trạng thái hiệu lực, loại giảm; hiển thị usage count/limit.
- Form: code, name, description, percentage/fixed, value, min order, max discount, start/end, usage limit, active.
- Phạm vi: toàn menu hoặc chọn categories/foods. Cần một field/suy luận rõ “all”; hai danh sách rỗng được đề xuất nghĩa là áp dụng toàn menu.
- Preview kết quả trên subtotal mẫu.
- Không sửa quy tắc promotion đã được áp làm thay đổi order cũ vì `order_promotions` giữ snapshot.
- Apply promotion phải chạy phía server, khóa/tăng `UsageCount` an toàn dưới concurrency; cần định nghĩa khi nào tăng và khi hủy order có hoàn quota hay không.
- MVP nên giới hạn tối đa một promotion/order vì request blueprint dùng một `promotionCode`; nếu cho stack nhiều mã phải có spec riêng về thứ tự tính.

---

## 3.10 Thiết lập / Khu vực & bàn

**Route đề xuất:** `/restaurant/layouts` với hai tab `Khu vực` và `Bàn`  
**Mục tiêu:** cấu hình layout vật lý; không trộn với màn hình vận hành Sơ đồ bàn.

### Layout đề xuất

```text
[Khu vực & bàn]                                  [Thêm theo tab]
[Tab Khu vực] [Tab Bàn]
[Filter/toolbar]
[Data table]
```

Desktop dùng một workspace với hai tabs để tránh thêm cấp menu. Form khu vực dùng drawer thường; form bàn dùng drawer rộng vừa, trong đó area là autocomplete/select. Mobile chuyển table thành cards và giữ tab bar sticky dưới PageHeader.

### Tab Khu vực

- Danh sách/CRUD: code, name, description, display order, active.
- Không hard delete khi có bàn; deactivate và cảnh báo ảnh hưởng.

### Tab Bàn

- Filter khu vực/trạng thái/active; CRUD code, name, area, capacity, active.
- Trạng thái vận hành không sửa tùy ý trong form cấu hình. `Disabled` là action có kiểm tra; không disable khi có open session.
- Chuyển bàn sang area khác chỉ khi không có open session.
- Trạng thái `Occupied/Cleaning` phải đến từ use case vận hành, không từ CRUD update.

## 4. Luồng xuyên menu

```text
Thiết lập khu vực/bàn + cấu hình thực đơn
        ↓
Sơ đồ bàn: Available → Mở bàn → Occupied + TableSession(Open)
        ↓
Gọi món: Menu → Cart → validate giá/availability/promotion → Order
        ↓
Gửi bếp → KitchenOrder/Items → Preparing → Ready
        ↓
Phục vụ: order item → Served/Completed
        ↓
Thanh toán: một hoặc nhiều Payment(Paid)
        ↓
Order Completed → TableSession Closed → Table Cleaning
        ↓
Dọn xong → Table Available; dữ liệu đi vào Dashboard/Order History
```

### Hợp đồng realtime tối thiểu

| Event | Producer | Consumer | Payload tối thiểu |
|---|---|---|---|
| `TableStatusChanged` | Table session | Table map | tableId, old/new status, version |
| `KitchenOrderCreated` | Ordering | Kitchen | kitchenOrderId, kitchenNo, sent time |
| `KitchenItemStatusChanged` | Kitchen | Kitchen, waiter/order | IDs, old/new status, time, version |
| `FoodAvailabilityChanged` | Catalog/Kitchen | Order screen | variantId, availability, reason, version |
| `OrderStatusChanged` | Order/payment | Table map, order detail | orderId, status, sessionId, version |

Không gửi payload SignalR như nguồn dữ liệu hoàn chỉnh lâu dài; consumer dùng ID/version để merge hoặc refetch khi thiếu sự kiện.

## 5. Yêu cầu ngang bắt buộc cho mọi Spec Kit

Mỗi spec phải nêu rõ:

1. Actor, permission cho view và từng command.
2. Route, entry point, responsive target và trạng thái loading/empty/error/forbidden.
3. Trường hiển thị, filter, sort, pagination và timezone/currency.
4. Form validation ở UI lẫn API; mã lỗi nghiệp vụ ổn định để UI ánh xạ thông báo.
5. State machine, transition hợp lệ, precondition và hành vi khi concurrency conflict.
6. Transaction boundary, idempotency cho command tạo giao dịch và audit actor/time/reason.
7. API contract request/response/problem details; backend không tin giá/tổng/quyền từ client.
8. Realtime event nếu có, hành vi reconnect và fallback polling/refetch.
9. Acceptance scenarios theo Given/When/Then gồm happy path, boundary, forbidden, stale data, retry.
10. Test: unit domain, application/integration với DB transaction, API authorization, UI component/E2E cho luồng chính.
11. Observability: correlation ID, structured log, metric lỗi/latency; không log dữ liệu nhạy cảm.
12. Migration/backward compatibility với route/API/code hiện có.

## 6. Các điểm chưa rõ phải chốt trước khi implement

| ID | Mức | Câu hỏi/quyết định cần chốt | Khuyến nghị mặc định |
|---|---|---|---|
| D01 | Đã chốt | Thanh toán theo từng Order hay toàn TableSession/bill? | Payment thuộc TableSession; PaymentAllocation phân bổ chính xác vào từng Order. |
| D02 | Blocker | Gọi thêm tạo order mới hay append order hiện tại? | Order mới cho mỗi lần gửi bếp như blueprint. |
| D03 | Blocker | “Xác nhận order” và “Gửi bếp” một hay hai thao tác? | Một CTA orchestration cho MVP, vẫn giữ hai aggregate/audit. |
| D04 | Blocker | Quy tắc suy ra trạng thái Order và KitchenOrder từ item? | Backend state machine duy nhất, ghi rõ bảng transition trong spec. |
| D05 | Cao | `Served` của OrderItem tương ứng `Completed` của KitchenItem khi nào? | Một command Serve cập nhật cả hai trong transaction. |
| D06 | Cao | Thuế/service charge được tính theo cấu hình nào? | MVP tax = 0 nếu chưa có cấu hình; không hard-code phần trăm. |
| D07 | Cao | Promotion áp toàn order hay chỉ subtotal item hợp lệ? | Tính discount trên tổng item thuộc scope; phân bổ discount để báo cáo/hủy item. |
| D08 | Cao | Tăng/hoàn usage promotion thời điểm nào? | Reserve/apply khi order tạo; hoàn khi order hủy nếu payment chưa Paid. |
| D09 | Cao | Cho phép vượt sức chứa? | Cảnh báo, cho quản lý override; lưu audit nếu vượt. |
| D10 | Cao | Quy tắc hủy order/item theo trạng thái bếp? | Sau Preparing cần quyền quản lý và reason. |
| D11 | Trung bình | Giá có hiệu lực tương lai hay chỉ đổi ngay? | MVP chỉ đổi ngay; nếu cho tương lai cần scheduler/price resolver. |
| D12 | Trung bình | Nhiều promotion/order và stacking? | MVP một code/order. |
| D13 | Trung bình | Reserved có được dùng khi chưa có reservation? | Ẩn action Reserved ở MVP. |
| D14 | Trung bình | Tiền khách đưa vượt tổng có phải tip? | Không; trả tiền thừa, amount ghi bằng phần cần thu. |
| D15 | Trung bình | Business day có thể qua nửa đêm? | MVP ngày lịch Asia/Bangkok; bổ sung business-day cutoff khi có yêu cầu. |

## 7. Khoảng trống giữa blueprint và database hiện tại

- Tên file trong blueprint ghi `restaurant_database.sql`, file thực tế là `restaurant_db.sql`.
- Không có unique constraint bảo đảm mỗi bàn chỉ có một `TableSession(Open)`; phải bảo vệ bằng transaction/locking hoặc thiết kế constraint phù hợp.
- Không có constraint “mỗi food đúng một default variant active”.
- Gửi bếp từng phần đã được mô hình hóa: cùng OrderItem được phép xuất hiện ở nhiều KitchenOrder, view `order_item_kitchen_quantities` tính sent/remaining; command phải khóa OrderItem và ngăn tổng quantity phiếu còn hiệu lực vượt quantity đã đặt.
- Thanh toán gộp đã được mô hình hóa: `payments` thuộc TableSession và `payment_allocations` phân bổ vào từng Order. Các invariant tổng allocation vẫn cần application transaction vì CHECK constraint không kiểm tra tổng qua nhiều dòng.
- `TransactionNo` chưa unique và chưa có provider/idempotency key.
- Price history chưa có reason và chưa enforce chỉ một record hiện hành cho variant.
- Promotion chưa biểu diễn tường minh scope “all”, stacking, per-customer usage hay phân bổ discount theo item.
- `CustomerId`, `TakeAway`, `Delivery` xuất hiện trong DB nhưng domain/customer/delivery UI chưa có; tránh vô tình mở scope MVP.
- Chưa có audit lịch sử cho table/session/kitchen/payment tương đương `order_status_histories`.
- Blueprint nhắc tax nhưng chưa có cấu hình/công thức làm tròn.
- API naming đang không đồng nhất: blueprint dùng `/api/foods`, client hiện dùng `/api/restaurant/products`. Mỗi Spec Kit phải chọn convention và migration path.

## 8. Kế hoạch tạo Spec Kit và phụ thuộc

```text
Wave 1 (nền tảng, có thể làm song song)
├── restaurant-categories
├── restaurant-products (phụ thuộc categories)
└── restaurant-layout

Wave 2
├── restaurant-table-operations (phụ thuộc layout)
└── restaurant-ordering (phụ thuộc products + table operations)

Wave 3
├── restaurant-kitchen (phụ thuộc ordering)
├── restaurant-promotions (phụ thuộc products; tích hợp ordering)
├── restaurant-payment (phụ thuộc ordering; D01 đã chốt)
└── restaurant-order-history (phụ thuộc ordering/kitchen/payment)

Wave 4
└── restaurant-dashboard (phụ thuộc dữ liệu giao dịch ổn định)
```

Với mỗi feature, quy trình đề xuất là: `specify → clarify → plan → tasks → analyze → implement → converge`. Không nên tạo toàn bộ spec một lần trước khi chốt D02–D05 vì các quyết định này thay đổi API và ranh giới transaction của nhiều menu; D01 đã được chốt trong tài liệu này.

## 9. Definition of Done cấp chương trình

- Tất cả menu lấy từ Identity authorization, route/API đều enforce cùng permission.
- Luồng `Available → open → order → kitchen → serve → pay → close → cleaning → available` chạy E2E và chịu được retry/concurrency cơ bản.
- Giá, promotion, tổng tiền và trạng thái chỉ do backend quyết định; mọi giao dịch quan trọng có transaction và audit.
- UI xử lý đủ loading, empty, validation, forbidden, conflict, server error và realtime reconnect.
- Báo cáo đối soát được: `orders.total` (phải thu) khác `payments Paid` (thực thu), snapshot lịch sử không đổi theo catalog.
- Migrations có up/down phù hợp, seed không phá môi trường có dữ liệu, test tự động bao phủ state transition và authorization.
- Các mục ngoài MVP được khóa phạm vi rõ ràng, không để enum/schema có sẵn kéo feature sang delivery/reservation/refund ngoài kế hoạch.
