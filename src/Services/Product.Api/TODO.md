# Danh Sách Công Việc Còn Lại Cho Product.Api

## Đã Hoàn Thành

- [x] **FluentValidation** — Kiểm tra đầu vào với các validator riêng biệt, pipeline behavior qua MediatR.
- [x] **Authentication / Authorization** — Tích hợp JWT Bearer, phân quyền dựa trên Policy và Role.
- [x] **Event Publishing** — Publish domain events (ProductCreated, ProductUpdated, ProductDeleted, ProductStockUpdated) qua MassTransit/RabbitMQ.
- [x] **Global Exception Handler** — Middleware xử lý ngoại lệ tập trung, chuẩn hóa response lỗi.
- [x] **Repository Pattern** — Generic repository + interface, tách biệt tầng Infrastructure.
- [x] **DTO Mapping** — AutoMapper mapping profiles giữa Entity và DTO.
- [x] **API Gateway Routing** — Cấu hình Ocelot routing cho Product.Api.
- [x] **OpenTelemetry** — Tracing, metrics và logging cơ bản.
- [x] **Health Checks** — Kiểm tra kết nối DB, RabbitMQ và Redis.

## Cần Thực Hiện

### Unit Tests
- [ ] Viết unit test cho **Application Services** (CreateProductHandler, UpdateProductHandler, DeleteProductHandler).
- [ ] Viết unit test cho **Domain Entities** (Product, Category).
- [ ] Viết unit test cho **Validators** (CreateProductValidator, UpdateProductValidator).
- [ ] Viết integration test cho **Repositories**.
- [ ] Viết integration test cho **Event Publishing**.
- [ ] Đạt coverage tối thiểu 80%.

### Caching
- [ ] Tích hợp **Redis Cache** cho các endpoint GET phổ biến.
- [ ] Triển khai **Cache-Aside pattern** với distributed cache.
- [ ] Cache invalidation khi có thay đổi dữ liệu.
- [ ] Tùy chỉnh thời gian cache cho từng loại dữ liệu.

### API Versioning
- [ ] Thiết lập API Versioning (URL path hoặc header).
- [ ] Hỗ trợ versioning cho tất cả endpoint hiện tại.
- [ ] Deprecation policy cho version cũ.
- [ ] Document versioning strategy trong Swagger.

### Logging & Monitoring
- [ ] Hoàn thiện cấu hình **Serilog** với sink Elasticsearch.
- [ ] Thiết lập **alerting rules** cho các metrics quan trọng.
- [ ] Tạo dashboard Grafana cho Product.Api.

### Documentation
- [ ] Hoàn thiện Swagger/OpenAPI documentation.
- [ ] Viết integration guide cho team khác sử dụng Product.Api events.
- [ ] Thêm XML comments cho tất cả public API.

### Performance
- [ ] Benchmark các endpoint quan trọng.
- [ ] Tối ưu query EF Core (includes, splits, no-tracking).
- [ ] Xem xét **pagination** cho danh sách sản phẩm.
- [ ] Rate limiting qua Ocelot hoặc middleware.

### Security
- [ ] Audit log cho các thao tác nhạy cảm.
- [ ] Input sanitization cho tất cả request.
- [ ] Kiểm tra SQL injection và XSS.
- [ ] Quét dependency vulnerabilities.

### CI/CD
- [ ] GitHub Actions workflow cho build + test + publish.
- [ ] Dockerfile tối ưu cho production.
- [ ] Kubernetes manifests (deployment, service, configmap).
- [ ] Database migration script cho CI pipeline.
