# Hướng dẫn chạy MicroserviceApp (theo từng bước)

Làm việc từ **thư mục gốc repo** (`MicroserviceApp`, nơi có file `MicroserviceApp.sln` và các file `docker-compose*.yml`).

---

## Bước 0 — Kiểm tra môi trường

1. **Cài [.NET SDK 10](https://dotnet.microsoft.com/download)**  
   Repo dùng `net10.0`; file `global.json` cho phép `rollForward: latestMajor` nên SDK preview (ví dụ 10.0.300‑preview) cũng được.

2. **Cài [Docker Desktop](https://www.docker.com/products/docker-desktop/)** và bật Docker (Windows: WSL2 backend).

3. Mở terminal (PowerShell hoặc CMD) và vào thư mục repo:

   ```powershell
   cd D:\ws\MicroserviceApp
   ```

4. Kiểm tra phiên bản:

   ```powershell
   dotnet --version
   docker compose version
   ```

---

## Cách A — Toàn bộ chạy bằng Docker (infra + API + Gateway + Jaeger)

Phù hợp khi bạn muốn **một lệnh** khởi động hầu hết thành phần.

### Bước A1 — Khởi động stack

Từ thư mục gốc repo:

```powershell
docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.apps.yml up -d --build
```

- Lần đầu có thể mất vài phút (pull ảnh .NET preview, MySQL, v.v.).
- Ảnh runtime dùng tag `10.0-preview`. Nếu pull lỗi, cập nhật Dockerfile theo tag mới trong [dotnet-docker](https://github.com/dotnet/dotnet-docker).

### Bước A2 — Kiểm tra container đã lên

```powershell
docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.apps.yml ps
```

Các container quan trọng: `orderdb`, `productdb`, `customerdb`, `basketdb`, `inventorydb`, `rabbitmq`, `jaeger`, `product-api`, `customer-api`, `basket-api`, `ordering-api`, `inventory-api`, `ocelot-apigw`.

### Bước A3 — Cơ sở dữ liệu (migration) — làm một lần nếu chưa có bảng

Các API dùng Entity Framework/MySQL/PG/SQL Server: **lần đầu chạy** có thể cần áp migration cho từng service có migration.

Mở **terminal thứ hai** (Docker vẫn chạy), vẫn ở thư mục gốc repo. Ví dụ (điều chỉnh tên migration nếu project của bạn khác):

```powershell
# Product (MySQL — host port 3307)
dotnet ef database update --project src\Services\Product.Api\Product.Api.csproj

# Customer (PostgreSQL — host port 5433)
dotnet ef database update --project src\Services\Customer.Api\Customer.Api.csproj

# Ordering (SQL Server — host port 1435)
dotnet ef database update --project src\Services\Ordering.Api\Ordering.Api.csproj
```

Nếu lệnh `dotnet ef` không có, cài tool:

```powershell
dotnet tool install --global dotnet-ef
```

**(Inventory MongoDB / Basket Redis** thường không cần `database update` theo kiểu EF.)

### Bước A4 — Gọi API qua Gateway

- **Gateway (Ocelot):** http://localhost:5293  
- Ví dụ:

  ```text
  GET http://localhost:5293/api/products
  ```

- **Jaeger (trace):** http://localhost:16686  
- **RabbitMQ Management:** http://localhost:15672 (user/pass mặc định trong compose override thường là `guest` / `guest` nếu chưa đổi)

### Bước A5 — Dừng stack

```powershell
docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.apps.yml down
```

Để **xóa luôn volume** DB (reset dữ liệu):

```powershell
docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.apps.yml down -v
```

---

## Cách B — Chỉ Docker cho DB/infra; API chạy bằng `dotnet run` trên máy

Phù hợp khi debug từng service trong IDE.

### Bước B1 — Chỉ chạy hạ tầng + DB

```powershell
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

### Bước B2 — Restore & build solution (một lần hoặc sau khi pull code)

```powershell
dotnet restore MicroserviceApp.sln
dotnet build MicroserviceApp.sln
```

### Bước B3 — Migration DB (như Bước A3)

Chạy `dotnet ef database update` cho các project có EF khi DB trống.

### Bước B4 — Chạy từng API (nhiều terminal hoặc multi-start trong IDE)

Thứ tự gợi ý: **Product → Customer → Basket → Ordering → Inventory**, sau đó **Gateway**.

Mỗi lệnh dưới đây chạy **một cửa sổ terminal** riêng, thư mục gốc repo:

```powershell
dotnet run --project src\Services\Product.Api\Product.Api.csproj
```

```powershell
dotnet run --project src\Services\Customer.Api\Customer.Api.csproj
```

```powershell
dotnet run --project src\Services\Basket.Api\Basket.Api.csproj
```

```powershell
dotnet run --project src\Services\Ordering.Api\Ordering.Api.csproj
```

```powershell
dotnet run --project src\Services\Inventory.Api\Inventory.Api.csproj
```

```powershell
dotnet run --project src\ApiGateways\OcelotApiGw\OcelotApiGw.csproj
```

Gateway mặc định trong `src\ApiGateways\OcelotApiGw\ocelot.json` trỏ tới **localhost** và cổng trùng `launchSettings` của từng API:

| Service        | HTTP (dotnet run) |
|----------------|-------------------|
| OcelotApiGw    | 5293              |
| Product.Api    | 5037              |
| Customer.Api   | 5001              |
| Basket.Api     | 5068              |
| Ordering.Api   | 5143              |
| Inventory.Api  | 5281              |

### Bước B5 — Gọi qua Gateway

Giống Cách A, Bước A4 (`http://localhost:5293/...`).

Swagger trực tiếp từng API (không qua Gateway), ví dụ: `http://localhost:5037/swagger` cho Product.

### Bước B6 — Tracing cục bộ (tùy chọn)

Để telemetry OTLP không báo không kết nối được, có thể chạy Jaeger đơn giản:

```powershell
docker run -d --name jaeger-dev -p 16686:16686 -p 4317:4317 -e COLLECTOR_OTLP_ENABLED=true jaegertracing/all-in-one:1.62
```

Các API đã cấu hình gửi trace tới `http://localhost:4317` (xem `appsettings.json`: `OpenTelemetry:OtlpEndpoint`).

---

## Ghi chú nhanh

- **Đọc kiến trúc chi tiết:** `MICROSERVICE_ARCHITECTURE.md`
- **Bật JWT:** đặt `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` (cùng key/issuer/audience) trong appsettings của API và Gateway; khi JWT được bật, **tất cả endpoint yêu cầu authenticated user** (fallback policy).
- **Firewall / port trùng:** nếu `address already in use`, đổi cổng trong `launchSettings.json` hoặc dừng process đang giữ cổng.
- **Pagination:** List endpoints hỗ trợ phân trang: `GET /api/products?pageIndex=1&pageSize=20`, `GET /api/customers?pageIndex=1&pageSize=20`.
- **Global Exception Handler:** Mọi lỗi đều được bắt và trả về `ApiResponse` chuẩn (không leak stack trace).

---

## Checklist cuối (xác nhận chạy được)

- [ ] `docker compose ... ps` không có container **Exit** (Cách A) hoặc DB containers **running** (Cách B).
- [ ] Migration đã chạy xong với các API dùng EF (nếu cần).
- [ ] `GET http://localhost:5293/api/products` (hoặc service khác) trả HTTP 200 hoặc 404 hợp lệ chứ không phải gateway “connection refused”.
- [ ] RabbitMQ và (nếu bật) Jaeger không báo crash loop trong `docker logs <container>`.
