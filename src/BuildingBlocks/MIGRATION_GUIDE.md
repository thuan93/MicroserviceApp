# Hướng dẫn Migration - Entity Framework Core

## 1. Tổng quan về Migrations

Migration trong EF Core là cơ chế tự động đồng bộ schema cơ sở dữ liệu với model code (C#). Khi bạn thay đổi entity, migration sẽ tạo ra file C# mô tả các thay đổi cần áp dụng lên database.

Trong MicroserviceApp, ba service sử dụng EF Core migrations:

| Service       | Database     | Provider                            | Connection                     |
|---------------|-------------|-------------------------------------|--------------------------------|
| Product.Api   | MySQL       | Pomelo.EntityFrameworkCore.MySql    | Port 3307, Database ProductDb  |
| Customer.Api  | PostgreSQL  | Npgsql.EntityFrameworkCore.PostgreSQL | Port 5433, Database CustomerDb |
| Ordering.Api  | SQL Server  | Microsoft.EntityFrameworkCore.SqlServer | Port 1435, Database OrderingDb |

**Lưu ý:** Basket.Api (Redis) và Inventory.Api (MongoDB) **không** sử dụng EF Core, do đó không áp dụng migration.

---

## 2. Khi nào cần tạo migration mới

Tạo migration mới khi bạn thực hiện bất kỳ thay đổi nào sau đây trong entity hoặc cấu hình DbContext:

- Thêm, sửa, xóa property trong entity class
- Thêm, sửa, xóa DbSet trong DbContext
- Thay đổi mapping configuration (Fluent API, Data Annotation)
- Thay đổi relationship giữa các entity
- Thêm index, constraint, hoặc default value

**Ví dụ:** Khi bạn thêm property `PhoneNumber` vào entity `Customer`:

```csharp
public class Customer
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty; // Thêm mới
}
```

---

## 3. Cách tạo migration (dùng dotnet-ef)

### 3.1. Cài đặt công cụ (chỉ làm một lần)

```bash
dotnet tool install --global dotnet-ef
```

Kiểm tra phiên bản:

```bash
dotnet ef --version
```

### 3.2. Tạo migration cho từng service

**Product.Api (MySQL):**

```bash
cd src/Services/Product.Api

dotnet ef migrations add TenMigration `
    --context ProductContext `
    --output-dir Migrations
```

**Customer.Api (PostgreSQL):**

```bash
cd src/Services/Customer.Api

dotnet ef migrations add TenMigration `
    --context CustomerContext `
    --output-dir Migrations
```

**Ordering.Api (SQL Server):**

```bash
cd src/Services/Ordering.Api

dotnet ef migrations add TenMigration `
    --context OrderingContext `
    --output-dir Migrations
```

**Giải thích tham số:**
- `TenMigration`: tên migration, nên đặt theo chức năng (ví dụ: `AddPhoneNumberToCustomer`)
- `--context`: tên DbContext (mặc định sẽ tự tìm nếu chỉ có một)
- `--output-dir`: thư mục chứa file migration

### 3.3. Xem migration đã tạo thành công

```bash
dotnet ef migrations list --context ProductContext
```

---

## 4. Cách áp dụng migration

### 4.1. Áp dụng lên database

**Cách 1 - Áp dụng trực tiếp qua CLI:**

```bash
dotnet ef database update --context <DbContext>
```

Ví dụ:

```bash
# Product.Api
cd src/Services/Product.Api
dotnet ef database update --context ProductContext

# Customer.Api
cd src/Services/Customer.Api
dotnet ef database update --context CustomerContext

# Ordering.Api
cd src/Services/Ordering.Api
dotnet ef database update --context OrderingContext
```

**Cách 2 - Tự động áp dụng khi ứng dụng khởi động (không khuyến khích cho production):**

```csharp
// Program.cs
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
await context.Database.MigrateAsync();
```

### 4.2. Áp dụng lên migration cụ thể

```bash
dotnet ef database update TenMigration --context ProductContext
```

---

## 5. Cách rollback migration

### 5.1. Rollback về migration trước đó

```bash
dotnet ef database update TenTruocDo --context ProductContext
```

Ví dụ - quay về migration `InitialCreate`:

```bash
dotnet ef database update InitialCreate --context ProductContext
```

### 5.2. Xóa migration chưa áp dụng

Nếu bạn vừa tạo migration nhưng chưa `database update`, có thể xóa bằng:

```bash
dotnet ef migrations remove --context ProductContext
```

### 5.3. Rollback hoàn toàn (xóa hết database)

⚠️ **Chỉ dùng trong môi trường development:**

```bash
dotnet ef database update 0 --context ProductContext
```

Sau đó xóa thư mục Migrations và tạo lại từ đầu.

---

## 6. Xử lý lỗi migration thường gặp

### 6.1. Lỗi "No project was found"

Nguyên nhân: Chạy lệnh sai thư mục.
Giải pháp: `cd` vào đúng thư mục project (nơi có file `.csproj`).

### 6.2. Lỗi "The entity type X requires a primary key"

Nguyên nhân: Entity thiếu khóa chính.
Giải pháp: Thêm property `Id` hoặc dùng `[Key]` attribute.

### 6.3. Lỗi kết nối database (timeout, access denied)

Nguyên nhân: Database chưa chạy hoặc sai connection string.
Giải pháp: Kiểm tra Docker container hoặc connection string trong `appsettings.json`. Product.Api chạy MySQL port 3307, Customer.Api chạy PostgreSQL port 5433, Ordering.Api chạy SQL Server port 1435.

### 6.4. Lỗi "An error occurred while accessing the database"

Kiểm tra:

```bash
docker ps
```

Đảm bảo các container database đang chạy. Nếu dùng Docker Compose:

```bash
docker-compose up -d
```

### 6.5. Lỗi "Unable to create a 'DbContext' of type 'X'"

Thêm `IDesignTimeDbContextFactory<TContext>` vào project:

```csharp
public class ProductContextFactory : IDesignTimeDbContextFactory<ProductContext>
{
    public ProductContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductContext>();
        optionsBuilder.UseMySql("Server=localhost;Port=3307;Database=ProductDb;Uid=root;Pwd=Passw0rd!",
            ServerVersion.AutoDetect("Server=localhost;Port=3307;Database=ProductDb;Uid=root;Pwd=Passw0rd!"));
        return new ProductContext(optionsBuilder.Options);
    }
}
```

---

## 7. Best practices

### 7.1. Đặt tên migration có ý nghĩa

Không đặt tên chung chung như `Migration1`, `test`, `fix`. Ví dụ:

```bash
✅ dotnet ef migrations add AddPhoneNumberToCustomer
✅ dotnet ef migrations add CreateOrderIndex
❌ dotnet ef migrations add test1
```

### 7.2. Commit migration vào source control

Luôn commit các file migration (`*.cs`, `*.Designer.cs`, `ModelSnapshot.cs`) vào Git. Điều này giúp cả team đồng bộ schema.

### 7.3. Không sửa file migration đã được áp dụng

Nếu migration đã được `database update` và đã commit, **không được sửa**. Thay vào đó, tạo migration mới để khắc phục.

### 7.4. Review migration trước khi áp dụng

Kiểm tra file `Up()` và `Down()` trong migration để đảm bảo chúng làm đúng những gì bạn mong đợi:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "PhoneNumber",
        table: "Customers",
        type: "longtext",
        nullable: false);
}
```

### 7.5. Migration riêng cho từng service

Mỗi service quản lý migration riêng, không gộp chung. Product.Api dùng MySQL, Customer.Api dùng PostgreSQL, Ordering.Api dùng SQL Server.

### 7.6. Không dùng MigrateAsync trong production

Tự động migrate khi startup có thể gây lỗi nếu có nhiều instance cùng chạy. Sử dụng CI/CD pipeline để chạy `dotnet ef database update`.

### 7.7. Dùng transaction cho critical migration

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Transaction sẽ tự động rollback nếu có lỗi
    migrationBuilder.Sql("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE");
    // ... các thao tác migration ...
}
```
