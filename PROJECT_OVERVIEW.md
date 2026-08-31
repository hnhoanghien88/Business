# Business Platform

Business Platform là dự án full-stack được tổ chức theo module nghiệp vụ để có thể nhân bản nhanh cho nhiều lĩnh vực như `Restaurant`, `Fashion` hoặc `Retail`. Phiên bản hiện tại triển khai module Restaurant với CRUD Product theo CQRS, một `BusinessDbContext`, MySQL cho dữ liệu bền vững và Redis hoặc bộ nhớ tiến trình cho distributed rate limiting.

## Điểm nổi bật

- Backend phân lớp theo hướng `Domain ← Application ← Infrastructure/API`.
- Feature được nhóm theo module nghiệp vụ thay vì đặt chung ở root.
- CRUD Product dùng MediatR CQRS và FluentValidation pipeline.
- Command side dùng Entity Framework Core; query side dùng Dapper.
- Một `BusinessDbContext` quản lý toàn bộ module, trong khi entity và bảng có namespace/prefix riêng.
- Correlation ID, structured request logging và Problem Details được áp dụng thống nhất.
- Performance behavior cảnh báo các MediatR request chạy chậm.
- Dynamic rate limiting hỗ trợ Fixed Window, Sliding Window, Token Bucket và Concurrency.
- Rate-limit state có thể lưu trong memory hoặc Redis với Lua script nguyên tử.
- Backend xác minh JWT và kiểm tra permission động qua Identity cho từng request được bảo vệ.
- EF Core migrations hỗ trợ chạy kèm embedded SQL để quản lý view và stored procedure.
- Frontend React được tổ chức theo feature, hiện có trang Restaurant Products và tích hợp Identity client.

## Kiến trúc

```text
React + Material UI
        │ REST
        ▼
ASP.NET Core API
  ├── Correlation ID
  ├── Structured logging
  ├── Exception / Problem Details
  ├── Dynamic rate limiting
  ├── JWT authentication + Identity permission authorization
  └── Controllers theo module
        │
        ▼
Application
  ├── Commands / Queries (CQRS)
  ├── FluentValidation
  ├── Performance behavior
  └── Persistence abstractions theo module
        │
        ▼
Infrastructure
  ├── EF Core repositories cho write side
  ├── Dapper repositories cho read side
  ├── BusinessDbContext
  └── EF migrations + embedded SQL
        │
        ▼
MySQL + Redis/InMemory rate-limit store
```

## Công nghệ

### Backend

- .NET 10 và ASP.NET Core Web API
- Entity Framework Core 10 và MySql.EntityFrameworkCore
- Dapper và MySqlConnector
- MediatR 14
- FluentValidation 12
- Serilog với console, application log và performance log
- StackExchange.Redis và Lua scripts
- Swagger/OpenAPI

### Frontend

- React 19
- Vite 8
- Material UI 9 và Emotion
- oxlint

### Data và vận hành

- MySQL cho dữ liệu nghiệp vụ và rate-limit policies
- Redis hoặc InMemory cho rate-limit counters/concurrency leases
- EF Core migrations kết hợp embedded SQL migrations
- Cấu hình phân tách theo environment

### Quy trình đặc tả

- GitHub Spec Kit 0.16.1 cho Spec-Driven Development.
- Tích hợp Codex dạng project skills trong `.agents/skills/`.
- PowerShell automation scripts và templates dùng chung trong `.specify/`.

## Cấu trúc module Restaurant

```text
Business.Api
└── Controllers
    └── Restaurant
        └── ProductsController.cs

Business.Application
├── Abstractions
│   └── Persistence
│       └── Restaurant
│           ├── IProductRepository.cs
│           └── IProductReadRepository.cs
└── Restaurant
    └── Products
        ├── CreateProduct
        ├── UpdateProduct
        ├── DeleteProduct
        ├── GetProductByCode
        ├── GetProducts
        ├── Dtos
        └── ProductRules.cs

Business.Domain
└── Entities
    └── Restaurant
        └── Product.cs

Business.Infrastructure
├── Persistence
│   ├── Configurations
│   │   └── Restaurant
│   │       └── ProductConfiguration.cs
│   ├── MySqlProductsRepository.cs
│   └── DapperProductsReadRepository.cs
└── Migrations
    └── Restaurant
        └── *_RestaurantInitial.cs
```

Khi thêm module mới, có thể giữ cùng tên entity như `Product` nhờ namespace riêng, ví dụ `Business.Domain.Entities.Fashion.Product`. Mỗi entity phải được ánh xạ sang bảng có prefix riêng như `restaurant_products` và `fashion_products`.

## CQRS Product

| Thao tác | HTTP endpoint | MediatR request | Persistence |
|---|---|---|---|
| Danh sách/tìm kiếm | `GET /api/restaurant/products` | `GetProductsQuery` | Dapper |
| Chi tiết | `GET /api/restaurant/products/{code}` | `GetProductByCodeQuery` | Dapper |
| Tạo | `POST /api/restaurant/products` | `CreateProductCommand` | EF Core |
| Cập nhật | `PUT /api/restaurant/products/{code}` | `UpdateProductCommand` | EF Core |
| Xóa | `DELETE /api/restaurant/products/{code}` | `DeleteProductCommand` | EF Core |

Product code được trim và chuẩn hóa thành chữ hoa. Create/Update được kiểm tra bằng FluentValidation; lỗi not found, validation và conflict được trả về theo Problem Details, kèm `correlationId`.

## BusinessDbContext và quy ước database

Dự án dùng một `BusinessDbContext` cho các module. Việc tách module được thực hiện ở namespace, folder, EF configuration và tên bảng:

```text
Module Restaurant → restaurant_products
Module Fashion    → fashion_products
Module Retail     → retail_products
```

Không sao chép hoặc chỉnh migration cũ khi thêm module. Sau khi thêm entity, configuration và DbSet, tạo migration mới để EF so sánh model hiện tại với `BusinessDbContextModelSnapshot`.

Migration `RestaurantInitial` nâng cấp bảng `product` cũ bằng `ALTER TABLE ... RENAME` thành `restaurant_products`, vì vậy không xóa dữ liệu. Migration cũng thay:

```text
product_view     → restaurant_products_view
sp_get_products  → sp_get_restaurant_products
```

SQL bổ sung của migration nằm trong:

```text
Business-api/src/Business.Infrastructure/Persistence/Sql/Migrations/RestaurantInitial
```

## Middleware pipeline

```text
CorrelationIdMiddleware
→ StructuredRequestLoggingMiddleware
→ ExceptionMiddleware
→ HTTPS / Routing
→ Authentication
→ DynamicRateLimitMiddleware
→ Authorization (permission được xác thực qua Identity)
→ Controller
→ PerformanceBehavior
→ ValidationBehavior
→ Handler
```

API nhận correlation ID hợp lệ từ header `X-Correlation-ID` hoặc tự sinh mới. ID cuối cùng được trả trong response header, đưa vào Serilog context và đính kèm các Problem Details response.

## Xác thực và phân quyền

Restaurant API xác minh JWT bearer trước khi request vào controller. Token phải:

- Là access token (`token_type=access`).
- Được phát hành cho Identity Application `Restaurant` (`application_code=restaurant`, audience `restaurant`).
- Có các claim `sub` và `permissionversion` để thực hiện kiểm tra permission.

Các endpoint Foods dùng permission policy tương ứng cho thao tác đọc, tạo, cập nhật và xóa. Khi policy được đánh giá, Restaurant API chuyển bearer token hiện tại tới endpoint `authorization` của Identity cùng `ApplicationCode=restaurant`. Identity kiểm tra token, phiên bản permission hiện hành và trả về danh sách quyền.

Kết quả authorization **không được cache tại Business**. Vì vậy, sau khi Identity tăng `permissionversion` do thay đổi hoặc thu hồi quyền, token mang phiên bản cũ sẽ bị từ chối ngay ở lần gọi Business tiếp theo. Memory cache trong API chỉ tiếp tục được dùng cho rate-limit policies và không lưu kết quả xác thực quyền.

Cấu hình kết nối Identity:

```json
{
  "IdentityAuthorization": {
    "BaseUrl": "https://localhost:7203/",
    "ApplicationCode": "restaurant"
  }
}
```

## Logging và hiệu năng

- Application logs: `Business-api/src/Business.Api/logs/application-*.log`.
- Performance logs: `Business-api/src/Business.Api/logs/performance-*.log`.
- Application log rolling theo ngày/kích thước 25 MB và giữ tối đa 30 file.
- Performance log rolling theo ngày/kích thước 10 MB và giữ tối đa 14 file.
- MediatR request được cảnh báo khi thời gian pipeline đạt `Observability:SlowRequestThresholdMilliseconds`, mặc định 500 ms.
- HTTP request trên 3 giây được ghi ở mức Warning; response từ 500 được ghi ở mức Error.
- Thư mục `logs/` không được commit vào Git.

## Rate limiting

Rate-limit policies được lưu trong bảng `rate_limit_policies` và cache ngắn hạn trong memory. Counter có thể chạy bằng:

- `InMemory`: phù hợp local development, test hoặc một API instance.
- `Redis`: phù hợp nhiều API instances và dùng Lua script để cập nhật state nguyên tử.

Cấu hình chính:

```json
{
  "RateLimiting": {
    "Enabled": true,
    "Store": "InMemory",
    "FailureMode": "Open",
    "KeyPrefix": "restaurant:rl",
    "ApplicationCode": "restaurant",
    "PolicyCacheSeconds": 30
  }
}
```

## Migrations

Liệt kê migration:

```powershell
dotnet ef migrations list `
  --project Business-api/src/Business.Infrastructure `
  --startup-project Business-api/src/Business.Api
```

Tạo migration cho module mới:

```powershell
dotnet ef migrations add FashionInitial `
  --project Business-api/src/Business.Infrastructure `
  --startup-project Business-api/src/Business.Api `
  --output-dir Migrations/Fashion
```

Áp dụng migrations:

```powershell
dotnet ef database update `
  --project Business-api/src/Business.Infrastructure `
  --startup-project Business-api/src/Business.Api
```

## Chạy local

Yêu cầu: .NET 10 SDK, Node.js và MySQL. Redis chỉ bắt buộc khi cấu hình `RateLimiting:Store` là `Redis`.

Backend:

```powershell
dotnet run --project Business-api/src/Business.Api
```

Frontend:

```powershell
cd Business-client
npm install
npm run dev
```

Swagger UI được bật trong Development. Health check có tại `GET /health`.

## Spec Kit

Repo đã được khởi tạo Spec Kit cho Codex với PowerShell scripts. Quy trình chuẩn:

```text
$speckit-constitution
→ $speckit-specify
→ $speckit-plan
→ $speckit-tasks
→ $speckit-implement
```

Các skill bổ trợ gồm `$speckit-clarify`, `$speckit-checklist`, `$speckit-analyze`, `$speckit-converge` và `$speckit-taskstoissues`. Constitution của dự án nằm tại `.specify/memory/constitution.md`; các feature spec được tạo theo workflow sẽ nằm trong `specs/`.

## Phạm vi hiện tại

- CRUD Restaurant Product đã hoàn chỉnh ở backend.
- Frontend có feature Restaurant Products và Identity session client.
- Backend xác minh JWT bearer và đã áp dụng permission policies lên các endpoint Restaurant Product.
- Business không tự phát hành token; việc xác nhận phiên bản và danh sách quyền hiện hành được ủy quyền cho Identity trên mỗi lần kiểm tra policy.
- Chưa có automated test projects trong solution hiện tại.
