# Microservices — trạng thái triển khai

Tài liệu **đồng bộ với code**. Kiến trúc tổng thể và Docker/Gateway/JWT/telemetry: xem **`MICROSERVICE_ARCHITECTURE.md`** ở root repo.

## Dịch vụ backend

| API | CSDL | Cổng dev (HTTP) | Ghi chú |
|-----|------|-----------------|--------|
| Product.Api | MySQL | 5037 | CRUD + publish product events |
| Customer.Api | PostgreSQL | 5001 | CRUD + publish customer events |
| Basket.Api | Redis | 5068 | Giỏ hàng qua Redis |
| Ordering.Api | SQL Server | 5143 | Orders + consume customer events |
| Inventory.Api | MongoDB | 5281 | Consume product/order events |

## API Gateway

- **Ocelot** (`src/ApiGateways/OcelotApiGw`): cổng **5293**, route trong `ocelot.json` (localhost) và `ocelot.Docker.json` (`ASPNETCORE_ENVIRONMENT=Docker`).
- Không dùng Swagger trên gateway; gọi Swagger trực tiếp từng service (`/swagger`) khi cần.

## Cross-cutting đã gắn vào mọi API

- `AspNetCore.Extensions`: JWT (khi `Jwt:Key` có giá trị), OpenTelemetry OTLP, Swagger security Bearer (khi JWT bật).
- Health: `GET /health`.

## Docker (API + Jaeger)

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.apps.yml up -d --build
```

- Gateway: `http://localhost:5293`
- Jaeger UI: `http://localhost:16686`

## Kiểm tra nhanh

1. Chạy infra: `docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d`
2. Chạy từng API hoặc full stack qua compose apps như trên.
3. Gọi qua gateway: `GET http://localhost:5293/api/products` (khi Product.Api đã chạy).
