# Sơ đồ Kiến trúc MicroserviceApp

Tài liệu này trình bày các sơ đồ kiến trúc tổng quan của hệ thống MicroserviceApp, bao gồm các BuildingBlocks dùng chung, luồng dữ liệu giữa các service, cơ sở dữ liệu và triển khai Docker.

---

## 1. Sơ đồ tổng quan kiến trúc microservice

```mermaid
flowchart TB
    Client[Client / Browser]
    Gateway[Ocelot API Gateway<br/>:5293]

    subgraph Services ["Microservices Layer"]
        Product[Product.Api<br/>:5037]
        Customer[Customer.Api<br/>:5001]
        Basket[Basket.Api<br/>:5068]
        Ordering[Ordering.Api<br/>:5143]
        Inventory[Inventory.Api<br/>:5281]
    end

    subgraph MessageBus ["Message Bus"]
        RMQ[(RabbitMQ)]
    end

    subgraph Observability ["Observability"]
        Jaeger[Jaeger<br/>OTLP :4317]
        ES[(ElasticSearch)]
        Kibana[Kibana]
    end

    Client -->|HTTP| Gateway
    Gateway --> Product
    Gateway --> Customer
    Gateway --> Basket
    Gateway --> Ordering
    Gateway --> Inventory

    Product -->|publish/subscribe| RMQ
    Customer -->|publish/subscribe| RMQ
    Ordering -->|publish/subscribe| RMQ
    Inventory -->|publish/subscribe| RMQ

    Product -->|OTLP| Jaeger
    Customer -->|OTLP| Jaeger
    Basket -->|OTLP| Jaeger
    Ordering -->|OTLP| Jaeger
    Inventory -->|OTLP| Jaeger
    Gateway -->|OTLP| Jaeger

    Product -.->|Serilog| ES
    Customer -.->|Serilog| ES
    Kibana -.->|query| ES
```

**Mô tả:** Client gửi yêu cầu qua API Gateway (Ocelot). Gateway định tuyến đến các service tương ứng. Giao tiếp bất đồng bộ giữa các service thông qua RabbitMQ. Tất cả service đều xuất tracing OpenTelemetry đến Jaeger và log qua Serilog đến ElasticSearch/Kibana.

---

## 2. Sơ đồ BuildingBlocks và dependencies

```mermaid
flowchart LR
    subgraph BB ["BuildingBlocks Layer"]
        Shared[Shared<br/>ApiResponse, Exceptions,<br/>Extensions]
        Contracts[Contracts<br/>DTO, Interfaces]
        Infrastructure[Infrastructure<br/>Repository, UnitOfWork]
        EventBus[EventBus.Messages<br/>Integration Events]
        Logging[Common.Logging<br/>Serilog Config]
        AspNetCore[AspNetCore.Extensions<br/>JWT, Telemetry,<br/>Exception Middleware,<br/>Swagger, Resilience]
    end

    subgraph ServicesLayer ["Services"]
        ProductApi[Product.Api]
        CustomerApi[Customer.Api]
        OrderingApi[Ordering.Api]
        InventoryApi[Inventory.Api]
        BasketApi[Basket.Api]
        OcelotGw[OcelotApiGw]
        ScheduleJob[ScheduleJob]
    end

    AspNetCore -->|depends on| Shared
    Shared -->|independent| Shared
    Contracts -->|independent| Contracts
    Infrastructure -->|uses EF Core| Infrastructure
    EventBus -->|independent| EventBus
    Logging -->|uses Serilog| Logging

    ProductApi --> Shared
    ProductApi --> Contracts
    ProductApi --> Infrastructure
    ProductApi --> EventBus
    ProductApi --> Logging
    ProductApi --> AspNetCore

    CustomerApi --> Shared
    CustomerApi --> Contracts
    CustomerApi --> Infrastructure
    CustomerApi --> EventBus
    CustomerApi --> Logging
    CustomerApi --> AspNetCore

    OrderingApi --> Shared
    OrderingApi --> Contracts
    OrderingApi --> Infrastructure
    OrderingApi --> EventBus
    OrderingApi --> Logging
    OrderingApi --> AspNetCore

    InventoryApi --> Shared
    InventoryApi --> EventBus
    InventoryApi --> Logging
    InventoryApi --> AspNetCore

    BasketApi --> Shared
    BasketApi --> Logging
    BasketApi --> AspNetCore

    OcelotGw --> Shared
    OcelotGw --> Logging
    OcelotGw --> AspNetCore

    ScheduleJob --> Logging
```

**Ma trận phụ thuộc:**

| Service | Infrastructure | Shared | Contracts | EventBus | Logging | AspNetCore.Ext |
|---|---|---|---|---|---|---|
| Product.Api | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Customer.Api | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Ordering.Api | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Inventory.Api | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ |
| Basket.Api | ❌ | ✅ | ❌ | ✅ | ✅ | ✅ |
| OcelotApiGw | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| ScheduleJob | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ |

---

## 3. Sơ đồ luồng dữ liệu giữa các service

### Luồng đồng bộ (HTTP qua API Gateway)

```mermaid
sequenceDiagram
    participant Client
    participant GW as Ocelot Gateway
    participant Product as Product.Api
    participant Customer as Customer.Api
    participant Ordering as Ordering.Api
    participant Inventory as Inventory.Api

    Client->>GW: GET /api/products
    GW->>Product: /api/products
    Product-->>GW: JSON response
    GW-->>Client: JSON response

    Client->>GW: POST /api/orders
    GW->>Ordering: /api/orders
    Ordering->>Customer: validate customerId (HTTP)
    Customer-->>Ordering: OK
    Ordering->>Inventory: check stock (HTTP)
    Inventory-->>Ordering: available
    Ordering-->>GW: order created
    GW-->>Client: 201 Created
```

### Luồng bất đồng bộ (Event Bus qua RabbitMQ)

```mermaid
sequenceDiagram
    participant Product as Product.Api
    participant RMQ as RabbitMQ
    participant Inventory as Inventory.Api
    participant Ordering as Ordering.Api
    participant Customer as Customer.Api

    Note over Product: Tạo sản phẩm mới
    Product->>RMQ: Publish ProductCreatedEvent
    par Inventory consumes
        RMQ->>Inventory: Consume ProductCreatedEvent
        Inventory->>Inventory: Tạo bản ghi inventory
    and Customer consumes
        RMQ->>Customer: Consume ProductCreatedEvent (nếu cần)
    and Ordering consumes
        RMQ->>Ordering: Consume ProductCreatedEvent (nếu cần)
    end

    Note over Ordering: Đặt hàng thành công
    Ordering->>RMQ: Publish OrderCreatedEvent
    RMQ->>Inventory: Consume OrderCreatedEvent
    Inventory->>Inventory: Giảm số lượng tồn kho
```

### Các sự kiện được định nghĩa trong EventBus.Messages

| Event | Publisher | Consumer(s) |
|---|---|---|
| ProductCreatedEvent | Product.Api | Inventory.Api, Customer.Api |
| ProductUpdatedEvent | Product.Api | Inventory.Api |
| ProductDeletedEvent | Product.Api | Inventory.Api |
| ProductStockUpdatedEvent | Inventory.Api | Product.Api |
| CustomerCreatedEvent | Customer.Api | Ordering.Api |
| OrderCreatedEvent | Ordering.Api | Inventory.Api |

---

## 4. Sơ đồ database của từng service

```mermaid
flowchart TB
    subgraph ProductDB ["Product.Api — MySQL"]
        P_Products[Products<br/>Id BIGINT PK<br/>Name NVARCHAR<br/>Price DECIMAL<br/>StockQuantity INT<br/>CategoryId BIGINT<br/>CreatedAt DATETIME]
        P_Categories[Categories<br/>Id BIGINT PK<br/>Name NVARCHAR<br/>Description TEXT]
        P_Suppliers[Suppliers<br/>Id BIGINT PK<br/>Name NVARCHAR<br/>ContactEmail NVARCHAR]
    end

    subgraph CustomerDB ["Customer.Api — PostgreSQL"]
        C_Customers[Customers<br/>Id BIGINT PK<br/>FirstName NVARCHAR<br/>LastName NVARCHAR<br/>Email NVARCHAR UNIQUE<br/>Phone NVARCHAR<br/>DateOfBirth DATE<br/>CreatedAt TIMESTAMP]
        C_Addresses[Addresses<br/>Id BIGINT PK<br/>CustomerId BIGINT FK<br/>Street NVARCHAR<br/>City NVARCHAR<br/>Country NVARCHAR<br/>IsDefault BOOLEAN]
    end

    subgraph OrderingDB ["Ordering.Api — SQL Server"]
        O_Orders[Orders<br/>Id BIGINT PK<br/>CustomerId BIGINT<br/>OrderDate DATETIME2<br/>Status NVARCHAR<br/>TotalAmount DECIMAL<br/>ShippingAddressId BIGINT]
        O_OrderItems[OrderItems<br/>Id BIGINT PK<br/>OrderId BIGINT FK<br/>ProductId BIGINT<br/>Quantity INT<br/>UnitPrice DECIMAL]
        O_Payments[Payments<br/>Id BIGINT PK<br/>OrderId BIGINT FK<br/>Amount DECIMAL<br/>PaymentMethod NVARCHAR<br/>Status NVARCHAR<br/>PaidAt DATETIME2]
    end

    subgraph InventoryDB ["Inventory.Api — MongoDB"]
        I_Inventory[InventoryItems<br/>{<br/>  _id: ObjectId,<br/>  ProductId: Int64,<br/>  ProductName: string,<br/>  WarehouseCode: string,<br/>  QuantityOnHand: int,<br/>  ReservedQuantity: int,<br/>  LastRestocked: Date<br/>}]
        I_Warehouses[Warehouses<br/>{<br/>  _id: ObjectId,<br/>  Code: string,<br/>  Name: string,<br/>  Location: string<br/>}]
    end

    subgraph BasketDB ["Basket.Api — Redis"]
        B_Basket[Cart Key: string<br/>{<br/>  UserName: string,<br/>  Items: CartItem[],<br/>  TotalPrice: decimal<br/>}]
    end

    ProductDB -->|EF Core| Product
    CustomerDB -->|EF Core| Customer
    OrderingDB -->|EF Core| Ordering
    InventoryDB -->|MongoDB Driver| Inventory
    BasketDB -->|StackExchange.Redis| Basket
```

| Service | Database Engine | ORM/Driver |
|---|---|---|
| Product.Api | MySQL 8.0 | EF Core |
| Customer.Api | PostgreSQL (Alpine) | EF Core |
| Ordering.Api | SQL Server 2022 | EF Core |
| Inventory.Api | MongoDB | MongoDB.Driver |
| Basket.Api | Redis (Alpine) | StackExchange.Redis |

---

## 5. Sơ đồ deployment (Docker)

```mermaid
flowchart TB
    subgraph DockerNetwork ["Docker Network: microservices"]
        subgraph Infra ["Infrastructure Containers"]
            direction TB
            MySQL[productdb<br/>mysql:8.0.29<br/>:3306]
            PG[customerdb<br/>postgres:alpine3.16<br/>:5432]
            MSSQL[orderdb<br/>mcr.microsoft.com/mssql/server:2022-latest<br/>:1433]
            Mongo[inventorydb<br/>mongo<br/>:27017]
            Redis[basketdb<br/>redis:alpine<br/>:6379]
            RMQ[rabbitmq<br/>rabbitmq:3-management-alpine<br/>:5672 :15672]
            ES[elasticsearch<br/>elasticsearch:7.17.2<br/>:9200]
            Kibana[kibana<br/>kibana:7.17.2<br/>:5601]
        end

        subgraph AppContainers ["Application Containers"]
            direction TB
            ProductSvc[product-api<br/>Product.Api]
            CustomerSvc[customer-api<br/>Customer.Api]
            BasketSvc[basket-api<br/>Basket.Api]
            OrderingSvc[ordering-api<br/>Ordering.Api]
            InventorySvc[inventory-api<br/>Inventory.Api]
            GatewaySvc[ocelot-apigw<br/>OcelotApiGw<br/>:5293 → 8080]
            JaegerSvc[jaeger<br/>jaegertracing/all-in-one<br/>:16686 :4317]
        end
    end

    Host[Host Machine<br/>docker-compose]

    ProductSvc --> MySQL
    CustomerSvc --> PG
    OrderingSvc --> MSSQL
    InventorySvc --> Mongo
    BasketSvc --> Redis
    ProductSvc --> RMQ
    CustomerSvc --> RMQ
    OrderingSvc --> RMQ
    InventorySvc --> RMQ
    GatewaySvc --> ProductSvc
    GatewaySvc --> CustomerSvc
    GatewaySvc --> BasketSvc
    GatewaySvc --> OrderingSvc
    GatewaySvc --> InventorySvc
    ProductSvc --> JaegerSvc
    CustomerSvc --> JaegerSvc
    OrderingSvc --> JaegerSvc
    InventorySvc --> JaegerSvc
    BasketSvc --> JaegerSvc
    GatewaySvc --> JaegerSvc
```

### Cách chạy

```bash
# Chỉ infrastructure (DB + message queue)
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Infrastructure + ứng dụng + Jaeger
docker-compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.apps.yml up -d --build

# Môi trường production
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

| Container | Image | Port (Host) |
|---|---|---|
| `productdb` | mysql:8.0.29 | 3306 |
| `customerdb` | postgres:alpine3.16 | 5432 |
| `orderdb` | mcr.microsoft.com/mssql/server:2022-latest | 1433 |
| `inventorydb` | mongo | 27017 |
| `basketdb` | redis:alpine | 6379 |
| `rabbitmq` | rabbitmq:3-management-alpine | 5672, 15672 |
| `elasticsearch` | elasticsearch:7.17.2 | 9200 |
| `kibana` | kibana:7.17.2 | 5601 |
| `jaeger` | jaegertracing/all-in-one:latest | 16686, 4317 |
| `ocelot-apigw` | (build từ Dockerfile) | 5293 → 8080 |
| `product-api` | (build từ Dockerfile) | — |
| `customer-api` | (build từ Dockerfile) | — |
| `basket-api` | (build từ Dockerfile) | — |
| `ordering-api` | (build từ Dockerfile) | — |
| `inventory-api` | (build từ Dockerfile) | — |

---

> **Ghi chú:** Tất cả container chạy trong cùng mạng Docker `microservices` (bridge driver). API Gateway là cổng vào duy nhất từ bên ngoài (port 5293). Các service nội bộ giao tiếp qua hostname container. OpenTelemetry tracing được cấu hình qua biến môi trường `OTEL_EXPORTER_OTLP_ENDPOINT` trỏ đến Jaeger.
