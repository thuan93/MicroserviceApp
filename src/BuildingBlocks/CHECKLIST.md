# Danh Sách Công Việc Triển Khai Microservice

> Cập nhật lần cuối: 15/05/2026

---

## 1. Tầng Cơ Sở Dữ Liệu (Database Layer)

- [x] Cấu hình DbContext cho từng service (Product, Customer, Ordering, Inventory, Basket)
- [x] Định nghĩa Entity và Fluent API Configuration cho từng service
- [x] Migration cho Product.Api (InitialCreate + UpdatedEntities)
- [x] Migration cho Customer.Api (InitialCreate)
- [x] Migration cho Ordering.Api (InitialCreate)
- [x] Migration cho Inventory.Api
- [x] Migration cho Basket.Api
- [ ] Thiết lập đánh chỉ mục (Index) cho các cột truy vấn thường xuyên
- [ ] Tối ưu hóa kiểu dữ liệu cột (decimal precision, varchar length)
- [ ] Thiết lập Soft Delete cho các entity chính
- [ ] Cơ chế tự động áp dụng migration khi khởi động (Auto Migrate)
- [ ] Seed data cho môi trường phát triển
- [ ] Phân tách database riêng cho mỗi service (Database per Service)

## 2. Tầng Repository (Repository Layer)

- [x] Generic Repository (`RepositoryBase<TEntity, TContext>`) — CRUD cơ bản
- [x] Interface `IRepository<TEntity>` với các phương thức: GetAll, GetById, Add, Update, Delete, Find, Count
- [x] `IUnitOfWork` / `UnitOfWork<TContext>` hỗ trợ transaction
- [x] `ProductRepository` / `IProductRepository` — thêm truy vấn theo Category, Supplier
- [x] `CustomerRepository` / `ICustomerRepository` — thêm truy vấn theo Email
- [x] `OrderRepository` / `IOrderRepository` — thêm truy vấn theo CustomerId, trạng thái
- [x] `InventoryRepository` / `IInventoryRepository` — ReserveStock, ReleaseStock, UpdateStock
- [ ] Specification Pattern cho truy vấn phức tạp (BaseSpecification đã có, cần tích hợp vào Repository)
- [ ] Repository hỗ trợ phân trang (PagedResult)
- [ ] Repository hỗ trợ Include/ThenInclude linh hoạt
- [ ] Caching layer (Redis) cho Repository

## 3. Tầng Service / Business Logic (Service Layer)

- [ ] Tách Service layer riêng giữa Controller và Repository
- [ ] `ProductService` — xử lý nghiệp vụ sản phẩm, phát sự kiện khi tạo/cập nhật/xoá
- [ ] `CustomerService` — xử lý nghiệp vụ khách hàng, phát sự kiện khi tạo/cập nhật/xoá
- [ ] `OrderService` — xử lý nghiệp vụ đơn hàng, kiểm tra tồn kho trước khi tạo
- [ ] `BasketService` — xử lý giỏ hàng, tính toán tổng tiền
- [ ] `InventoryService` — xử lý tồn kho, cảnh báo hàng thấp
- [ ] Validation với FluentValidation (đã có cho Customer và Product DTO)
- [ ] AutoMapper hoặc Mapster để ánh xạ Entity ↔ DTO
- [ ] Xử lý ngoại lệ tập trung (ExceptionMiddleware đã có)
- [ ] Outbox Pattern cho phát sự kiện đảm bảo consistent
- [ ] Circuit Breaker / Retry cho các gọi service ngoài

## 4. Tầng API / Controller (API Layer)

- [x] `ProductsController` — CRUD sản phẩm, phân loại theo Category
- [x] `CustomersController` — CRUD khách hàng
- [x] `OrdersController` — CRUD đơn hàng
- [x] `BasketController` — thêm/sửa/xoá sản phẩm trong giỏ
- [ ] `SuppliersController` — quản lý nhà cung cấp
- [ ] `CategoriesController` — quản lý danh mục
- [ ] Định nghĩa DTO đầy đủ cho Request/Response
- [ ] API Versioning
- [ ] Rate Limiting
- [ ] Response chuẩn hoá (ApiResponse<T> đã có)
- [ ] Attribute-based validation
- [ ] API Documentation với Swagger/OpenAPI (SwaggerGenJwt đã có)

## 5. Event Bus / Messaging

- [x] Cấu hình MassTransit với RabbitMQ trong tất cả service
- [x] `IntegrationBaseEvent` — lớp cơ sở cho tất cả sự kiện
- [x] `ProductCreatedEvent`, `ProductUpdatedEvent`, `ProductDeletedEvent`, `ProductStockUpdatedEvent`
- [x] `CustomerCreatedEvent`, `CustomerUpdatedEvent`, `CustomerDeletedEvent`
- [x] `OrderCreatedEvent`, `OrderUpdatedEvent`, `OrderCancelledEvent`
- [x] `InventoryReservedEvent`, `InventoryReleasedEvent`, `InventoryLowStockEvent`
- [x] `CustomerCreatedConsumer` + `CustomerUpdatedConsumer` (Ordering.Api)
- [x] `ProductCreatedConsumer`, `ProductUpdatedConsumer`, `ProductDeletedConsumer`, `ProductStockUpdatedConsumer` (Inventory.Api)
- [x] `OrderCreatedConsumer`, `OrderCancelledConsumer` (Inventory.Api)
- [ ] Sự kiện xoá khách hàng — xoá dữ liệu liên quan ở Ordering
- [ ] Sự kiện đơn hàng bị huỷ — ReleaseStock hoàn chỉnh
- [ ] Sự kiện thanh toán (PaymentCompleted, PaymentFailed)
- [ ] Saga Pattern / Orchestration cho quy trình đặt hàng
- [ ] Dead Letter Queue và xử lý lỗi messaging
- [ ] Kiểm tra tính đúng đắn của message (message contract validation)
- [ ] Monitoring queue (RabbitMQ Management UI)

## 6. Bảo Mật (Security)

- [x] JWT Authentication — `AddMicroserviceJwtAuthentication`
- [x] Cấu hình JWT Bearer token validation
- [x] Swagger tích hợp JWT (Authorize button)
- [x] CORS — `AddMicroserviceCors`
- [ ] Xác thực người dùng (Identity/Login)
- [ ] Authorization Policies cho từng endpoint
- [ ] Role-based access control (Admin, User, Manager)
- [ ] Bảo vệ API Key cho Ocelot Api Gateway
- [ ] Mã hoá dữ liệu nhạy cảm trong database
- [ ] HTTPS enforcement
- [ ] Input sanitization
- [ ] Audit Log (ghi lại các thao tác quan trọng)

## 7. Giám Sát & Quan Sát (Monitoring & Observability)

- [x] Serilog — ghi log cấu trúc ra Console và Debug
- [x] OpenTelemetry — tracing với OTLP export hỗ trợ Jaeger/ Aspire
- [x] Health Checks endpoint `/health` trong tất cả service
- [x] `WebHealthStatus` — dashboard theo dõi health check
- [ ] Serilog ghi log ra file / Elasticsearch
- [ ] Serilog ghi log ra Seq
- [ ] Metrics với Prometheus (số request, thời gian phản hồi, lỗi)
- [ ] Grafana dashboard cho metrics
- [ ] Distributed tracing hoàn chỉnh qua tất cả service
- [ ] Cảnh báo (Alert) khi service ngừng hoạt động
- [ ] Centralized logging (ELK stack hoặc Graylog)
- [ ] Custom Health Checks (kiểm tra kết nối DB, RabbitMQ, Redis)

## 8. Docker & Triển Khai (Docker & Deployment)

- [x] Dockerfile cho Product.Api
- [x] Dockerfile cho Customer.Api
- [x] Dockerfile cho Ordering.Api
- [x] Dockerfile cho Basket.Api
- [x] Dockerfile cho Inventory.Api
- [x] Dockerfile cho OcelotApiGw
- [x] `docker-compose.yml` — cấu hình cơ bản
- [x] `docker-compose.dev.yml` — môi trường phát triển
- [x] `docker-compose.prod.yml` — môi trường sản xuất
- [x] `docker-compose.override.yml`
- [ ] Dockerfile cho ScheduleJob
- [ ] Docker Compose với PostgreSQL, RabbitMQ, Redis
- [ ] Docker Compose với Seq, Jaeger
- [ ] Biến môi trường (.env) cho tất cả cấu hình
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Kubernetes manifests (Deployment, Service, ConfigMap, Secret)
- [ ] Liveness / Readiness probes cho Kubernetes
- [ ] Horizontal Pod Autoscaler (HPA)

## 9. Kiểm Thử (Testing)

- [ ] Tạo project `Product.Api.Tests`
- [ ] Tạo project `Customer.Api.Tests`
- [ ] Tạo project `Ordering.Api.Tests`
- [ ] Tạo project `Basket.Api.Tests`
- [ ] Tạo project `Inventory.Api.Tests`
- [ ] Unit Test cho Repository (dùng InMemoryProvider / SQLite)
- [ ] Unit Test cho Service / Business Logic
- [ ] Unit Test cho Validators (FluentValidation)
- [ ] Integration Test cho API endpoints
- [ ] Integration Test cho MassTransit Consumers
- [ ] Integration Test cho Health Checks
- [ ] Test Containers cho database thật
- [ ] Load Test / Performance Test (k6 hoặc NBomber)
- [ ] Code coverage report
- [ ] Test tự động trong CI/CD pipeline

---

## Chú Thích

| Ký hiệu | Ý nghĩa |
|---------|---------|
| [x] | Đã hoàn thành |
| [ ] | Chưa thực hiện / Cần bổ sung |
