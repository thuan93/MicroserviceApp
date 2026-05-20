# Hướng Dẫn Sử Dụng Docker Compose Cho MicroserviceApp

> **Lưu ý:** Dự án này sử dụng Docker Compose v2. Tất cả các lệnh bên dưới dùng `docker compose` thay vì `docker-compose`.

---

## Mục Lục

1. [Môi Trường Development (Không Portainer)](#1-môi-trường-development-không-portainer)
2. [Môi Trường Production (Có Portainer)](#2-môi-trường-production-có-portainer)
3. [Tính Năng Của Portainer Trong Production](#3-tính-năng-của-portainer-trong-production)
4. [Các Thực Hành Bảo Mật](#4-các-thực-hành-bảo-mật)
5. [So Sánh Development vs Production](#5-so-sánh-development-vs-production)
6. [Xử Lý Sự Cố](#6-xử-lý-sự-cố)
7. [Chiến Lược Backup](#7-chiến-lược-backup)
8. [Chuyển Từ Development Sang Production](#8-chuyển-từ-development-sang-production)
9. [Giải Thích Chi Tiết Các Lệnh](#9-giải-thích-chi-tiết-các-lệnh)
10. [Các Lỗi Thường Gặp](#10-các-lỗi-thường-gặp)
11. [Mẹo Hữu Ích](#11-mẹo-hữu-ích)
12. [Checklist Trước Khi Deploy Production](#12-checklist-trước-khi-deploy-production)

---

## 1. Môi Trường Development (Không Portainer)

### 1.1. Giới Thiệu

Môi trường Development sử dụng **Docker Desktop** để quản lý containers. Không cần Portainer vì Docker Desktop đã cung cấp giao diện đồ họa trực quan cho các tác vụ hàng ngày.

Các file Docker Compose:
- `docker-compose.yml` — Cấu hình base (databases, infrastructure)
- `docker-compose.dev.yml` — Cấu hình development (override)
- `docker-compose.apps.yml` — Cấu hình application services (.NET APIs + Jaeger)
- `docker-compose.override.yml` — Cấu hình override mặc định

### 1.2. Các Lệnh Cơ Bản

```sh
# Khởi động databases + infrastructure
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# Khởi động databases + infrastructure + application services
docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml up -d --build

# Xem logs real-time của tất cả services
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs -f

# Xem logs của một service cụ thể
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs -f orderdb

# Dừng tất cả services (giữ volumes)
docker compose -f docker-compose.yml -f docker-compose.dev.yml down

# Dừng và xóa volumes (mất dữ liệu databases)
docker compose -f docker-compose.yml -f docker-compose.dev.yml down -v

# Build lại và khởi động
docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml up -d --build

# Liệt kê containers đang chạy
docker ps

# Xem trạng thái tất cả containers
docker compose -f docker-compose.yml -f docker-compose.dev.yml ps
```

### 1.3. Cổng Truy Cập Các Services (Development)

| Service               | URL                                       | Thông tin đăng nhập |
|-----------------------|-------------------------------------------|---------------------|
| RabbitMQ Management   | http://localhost:15672                     | guest / guest       |
| Kibana                | http://localhost:5601                      | —                   |
| Elasticsearch         | http://localhost:9200                      | —                   |
| SQL Server (orderdb)  | `localhost,1435`                           | sa / Passw0rd!      |
| MySQL (productdb)     | `localhost:3307`                           | root / Passw0rd!    |
| PostgreSQL (customerdb)| `localhost:5433`                          | admin / admin1234   |
| Redis (basketdb)      | `localhost:6379`                           | —                   |
| MongoDB (inventorydb) | `localhost:27017`                          | —                   |
| API Gateway           | http://localhost:5293                      | —                   |
| Jaeger UI             | http://localhost:16686                     | —                   |

### 1.4. Development Workflow

```sh
# 1. Clone repository và cd vào thư mục dự án
git clone <repo-url> MicroserviceApp
cd MicroserviceApp

# 2. Khởi động cơ sở dữ liệu và hạ tầng
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# 3. Chạy ứng dụng .NET từ IDE (Visual Studio / Rider)
#    Hoặc chạy trực tiếp:
dotnet run --project src/Services/Product.Api

# 4. Khi cần chạy mọi thứ trong container:
docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml up -d --build
```

---

## 2. Môi Trường Production (Có Portainer)

### 2.1. Thiết Lập Lần Đầu

```sh
# Bước 1: Clone dự án lên server
git clone <repo-url> /opt/microservices
cd /opt/microservices

# Bước 2: Copy file template biến môi trường
cp .env.prod.example .env.prod

# Bước 3: Chỉnh sửa .env.prod với mật khẩu thực tế
# Dùng nano, vim, hoặc bất kỳ editor nào
nano .env.prod

# Bước 4: Khởi động tất cả services
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d

# Bước 5: Kiểm tra trạng thái
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod ps

# Bước 6: Thiết lập Portainer lần đầu
# Mở trình duyệt: https://<IP-SERVER>:9443
# Tạo tài khoản admin
# Chọn "Docker Standalone" → kết nối với local Docker environment
```

### 2.2. Vận Hành Hàng Ngày

```sh
# Xem tất cả containers đang chạy
docker ps

# Xem logs của một service
docker logs -f product-api

# Xem logs với timestamp và giới hạn dòng
docker logs --tail 100 -f --timestamps product-api

# Khởi động lại một service
docker restart product-api

# Dừng một service
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod stop product-api

# Xóa và khởi tạo lại một service
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod rm -sf product-api
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d product-api

# Cập nhật images và khởi động lại
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d

# Dọn dẹp images cũ không dùng
docker image prune -f
```

### 2.3. Cổng Truy Cập Các Services (Production)

| Service               | URL                                             | Ghi chú                        |
|-----------------------|-------------------------------------------------|--------------------------------|
| Portainer             | https://<IP-SERVER>:9443                        | HTTPS bắt buộc                 |
| RabbitMQ Management   | http://<IP-SERVER>:15672                        | Dùng thông tin từ .env.prod    |
| Kibana                | http://<IP-SERVER>:5601                         | Dùng elastic user + password   |
| Elasticsearch         | http://<IP-SERVER>:9200                         | Có xpack security              |
| SQL Server (orderdb)  | `<IP-SERVER>,1435`                              | Dùng mật khẩu từ .env.prod     |
| MySQL (productdb)     | `<IP-SERVER>:3306`                              | Dùng mật khẩu từ .env.prod     |
| PostgreSQL (customerdb)| `<IP-SERVER>:5433`                             | Dùng thông tin từ .env.prod    |
| Redis (basketdb)      | `<IP-SERVER>:6379`                              | Yêu cầu password               |
| MongoDB (inventorydb) | `<IP-SERVER>:27017`                             | Yêu cầu username + password    |
| API Gateway           | http://<IP-SERVER>:5293                         | —                               |

### 2.4. Sử Dụng Portainer Để Quản Lý

1. **Dashboard:** Xem tổng quan tài nguyên (CPU, RAM, disk)
2. **Containers:** Start/Stop/Restart, xem logs, attach console
3. **Images:** Quản lý Docker images, pull/push
4. **Volumes:** Xem dung lượng, backup dữ liệu
5. **Networks:** Kiểm tra cấu hình mạng giữa các containers
6. **Stacks:** Deploy và quản lý stacks từ docker-compose files

---

## 3. Tính Năng Của Portainer Trong Production

### 3.1. Quản Lý Containers

- **Khởi động/Dừng/Khởi động lại** containers mà không cần SSH vào server
- **Xem thống kê thời gian thực** (CPU, Memory, Network I/O)
- **Truy cập console** của container để debug
- **Xem và tải logs** với filter theo thời gian
- **Tạo container mới** với giao diện web trực quan

### 3.2. Đa Người Dùng

Portainer hỗ trợ tạo nhiều users với các vai trò khác nhau:

- **Administrator:** Toàn quyền truy cập, quản lý users, endpoints
- **Operator:** Quản lý containers, images, volumes nhưng không thay đổi cấu hình hệ thống
- **Read-only user:** Chỉ xem containers, logs — không thể thực hiện thay đổi
- **Helpdesk:** Giới hạn chỉ được khởi động lại containers

### 3.3. Truy Cập Từ Xa

- Giao diện web HTTPS an toàn
- Truy cập từ bất kỳ thiết bị nào (desktop, mobile, tablet)
- Portainer Agent cho phép quản lý nhiều Docker hosts từ một giao diện
- Không cần VPN (nếu firewall được cấu hình đúng cách)

### 3.4. Giám Sát & Cảnh Báo

- **Webhooks:** Thiết lập webhooks khi container gặp lỗi
- **Resource monitoring:** Giám sát mức sử dụng CPU, RAM, disk
- **Health checks:** Kiểm tra trạng thái containers định kỳ
- **Notifications:** Nhận thông báo qua email, Slack, Teams khi containers dừng

### 3.5. Quản Lý Stacks

- **Deploy stacks** từ docker-compose files trực tiếp qua giao diện web
- **Cập nhật stacks** chỉ với một cú click
- **Rollback** về phiên bản trước đó nếu cần
- **Quản lý biến môi trường** cho từng stack

---

## 4. Các Thực Hành Bảo Mật

### 4.1. Bảo Mật Portainer

```yaml
# docker-compose.prod.yml — Cấu hình Portainer an toàn
portainer:
  image: portainer/portainer-ce:latest
  container_name: portainer
  restart: always
  security_opt:
    - no-new-privileges:true     # Ngăn leo thang đặc quyền
  ports:
    - "9443:9443"                # CHỈ mở HTTPS — KHÔNG mở port 9000
  volumes:
    - /var/run/docker.sock:/var/run/docker.sock:ro   # Chế độ read-only!
    - portainer_data:/data
  command: --sslcert /data/cert.pem --sslkey /data/key.pem
```

### 4.2. Bảo Mật Databases

| Database    | Biện pháp bảo mật trong Production                 |
|-------------|----------------------------------------------------|
| SQL Server  | Mật khẩu SA từ biến môi trường, không hardcode     |
| MySQL       | Mật khẩu root từ biến môi trường                   |
| PostgreSQL  | User + password từ biến môi trường                 |
| Redis       | Yêu cầu password qua `--requirepass`               |
| MongoDB     | Authentication bắt buộc (username + password)       |
| Elasticsearch| xpack.security.enabled=true, có password           |

### 4.3. Biến Môi Trường

```sh
# KHÔNG BAO GIỜ commit file .env.prod vào Git!
# File .env.prod.example là template an toàn để commit

# Đảm bảo file .env.prod có quyền hạn chế
chmod 600 .env.prod

# Sử dụng --env-file flag khi chạy docker compose
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d
```

### 4.4. Firewall & Network

```sh
# Chỉ mở các port cần thiết (ví dụ trên Ubuntu với ufw)
sudo ufw default deny incoming
sudo ufw default allow outgoing

# SSH
sudo ufw allow 22/tcp

# Portainer (giới hạn IP nguồn nếu có thể)
sudo ufw allow from <IP-MẠNG-NỘI-BỘ> to any port 9443

# API Gateway
sudo ufw allow 5293/tcp

# Giới hạn truy cập databases — chỉ cho phép từ internal Docker network
# KHÔNG mở port databases ra ngoài Internet trong production!

# Bảo vệ RabbitMQ
sudo ufw allow from 127.0.0.1 to any port 15672

sudo ufw enable
```

### 4.5. Log Rotation

```yaml
# Cấu hình trong docker-compose.prod.yml
logging:
  driver: "json-file"
  options:
    max-size: "10m"       # Mỗi file log tối đa 10MB
    max-file: "3"          # Chỉ giữ 3 file log gần nhất
```

### 4.6. Cập Nhật Thường Xuyên

```sh
# Cập nhật Portainer
docker compose -f docker-compose.yml -f docker-compose.prod.yml pull portainer
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d portainer

# Cập nhật tất cả images
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d

# Kiểm tra images cũ cần dọn
docker image ls
docker image prune -a -f
```

---

## 5. So Sánh Development vs Production

| Tiêu Chí              | Development                        | Production                             |
|-----------------------|------------------------------------|----------------------------------------|
| **Công cụ quản lý**  | Docker Desktop                     | Portainer (HTTPS)                     |
| **Chính sách restart**| `unless-stopped`                  | `always`                              |
| **Mật khẩu**         | Hardcode (đơn giản cho dev)        | Biến môi trường (.env.prod)           |
| **Logging**          | Mặc định (không giới hạn)          | Giới hạn kích thước + xoay vòng        |
| **Tài nguyên ES**    | `ES_JAVA_OPTS=-Xms256m -Xmx256m`   | `ES_JAVA_OPTS=-Xms1g -Xmx1g`          |
| **HTTPS**            | Không cần                          | Bắt buộc (Portainer 9443)             |
| **Elasticsearch security**| Tắt                            | Bật (xpack.security)                  |
| **Redis password**   | Không yêu cầu                      | Bắt buộc (--requirepass)              |
| **MongoDB auth**     | Không yêu cầu                      | username + password từ .env.prod       |
| **Giám sát**         | Docker Desktop dashboard           | Portainer + Kibana                     |
| **Backup**           | Không quan trọng                   | Tự động hóa, kiểm tra định kỳ          |
| **Ports**            | Ports tránh xung đột (3307, 5433)  | Ports mặc định (3306, 5432)            |

---

## 6. Xử Lý Sự Cố

### 6.1. Không Truy Cập Được Portainer

```sh
# Kiểm tra container có đang chạy không
docker ps | findstr portainer

# Kiểm tra logs
docker logs portainer

# Kiểm tra firewall (trên Linux server)
sudo ufw status

# Kiểm tra container có listen đúng port không
docker port portainer

# Khởi động lại Portainer
docker restart portainer

# Nếu vẫn không được, kiểm tra certificate
docker exec portainer ls -la /data/cert.pem /data/key.pem
```

### 6.2. Quên Mật Khẩu Admin Portainer

```sh
# Dừng Portainer
docker stop portainer

# Chạy helper reset password
docker run --rm -v portainer_data:/data portainer/helper-reset-password

# Khởi động lại Portainer
docker start portainer

# Lấy mật khẩu mới từ output của lệnh helper-reset-password
```

### 6.3. Container Không Khởi Động Được

```sh
# Xem logs để biết lỗi
docker logs product-api

# Xem chi tiết container
docker inspect product-api

# Kiểm tra resource (có thể hết RAM/disk)
docker stats

# Kiểm tra volume có đủ dung lượng không
docker system df

# Thử restart với timeout dài hơn
docker restart -t 30 product-api
```

### 6.4. Không Kết Nối Được Database

```sh
# Kiểm tra container database có chạy không
docker ps | findstr orderdb

# Kiểm tra network
docker network inspect microservices

# Kiểm tra container có trong đúng network không
docker inspect orderdb | findstr "NetworkMode"

# Test kết nối từ container khác
docker exec -it product-api bash
# Bên trong container:
curl http://orderdb:1433
# Hoặc telnet orderdb 1433

# Kiểm tra xem service application có depends_on đúng không
```

### 6.5. Port Đã Được Sử Dụng

```
Error: Bind for 0.0.0.0:1435 failed: port is already allocated
```

```sh
# Kiểm tra process đang dùng port
netstat -ano | findstr :1435

# Tìm PID và dừng process
taskkill /PID <PID> /F

# Hoặc đổi port trong docker-compose file
#   ports:
#     - "1436:1433"   # Đổi từ 1435 sang 1436
```

### 6.6. Hết Dung Lượng Đĩa

```sh
# Kiểm tra dung lượng Docker đang dùng
docker system df

# Dọn dẹp
docker container prune -f        # Xóa containers đã dừng
docker image prune -f            # Xóa dangling images
docker volume prune -f           # Xóa volumes không dùng
docker builder prune -f          # Xóa build cache

# CẢNH BÁO: Chỉ chạy khi chắc chắn
docker system prune -a --volumes -f
```

---

## 7. Chiến Lược Backup

### 7.1. Backup Databases

```sh
# SQL Server
docker exec orderdb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$ORDERDB_SA_PASSWORD" -Q "BACKUP DATABASE OrderDb TO DISK = '/var/opt/mssql/backup/OrderDb_$(date +%Y%m%d).bak'"

# MySQL
docker exec productdb mysqldump -u root -p"$PRODUCTDB_ROOT_PASSWORD" --all-databases --result-file=/backup/mysql_full_$(date +%Y%m%d).sql

# PostgreSQL
docker exec customerdb pg_dump -U "$CUSTOMERDB_USER" CustomerDb > /backup/customerdb_$(date +%Y%m%d).sql

# MongoDB
docker exec inventorydb mongodump --username "$MONGO_USERNAME" --password "$MONGO_PASSWORD" --out /backup/mongo_$(date +%Y%m%d)

# Redis (nếu bật persistence)
docker exec basketdb redis-cli --rdb /data/dump.rdb
cp /var/lib/docker/volumes/microservices_redis_data/_data/dump.rdb /backup/redis_$(date +%Y%m%d).rdb
```

### 7.2. Backup Dữ Liệu Portainer

```sh
# Backup cấu hình Portainer (users, endpoints, stacks)
docker run --rm -v portainer_data:/data -v $(pwd):/backup alpine tar czf /backup/portainer-backup-$(date +%Y%m%d).tar.gz /data

# Restore Portainer
docker stop portainer
docker run --rm -v portainer_data:/data -v $(pwd):/backup alpine tar xzf /backup/portainer-backup-YYYYMMDD.tar.gz -C /
docker start portainer
```

### 7.3. Backup Docker Volumes

```sh
# Liệt kê tất cả volumes
docker volume ls

# Backup một volume
docker run --rm -v sqlserver_data:/source -v $(pwd):/backup alpine tar czf /backup/sqlserver_data-$(date +%Y%m%d).tar.gz -C /source .

# Restore volume
docker run --rm -v sqlserver_data:/destination -v $(pwd):/backup alpine tar xzf /backup/sqlserver_data-YYYYMMDD.tar.gz -C /destination
```

### 7.4. Script Backup Tự Động

Tạo file `backup.sh` trên server production:

```sh
#!/bin/bash
# Backup script cho MicroserviceApp
set -e

BACKUP_DIR="/backup/microservices"
DATE=$(date +%Y%m%d_%H%M%S)
mkdir -p "$BACKUP_DIR/$DATE"

# Load biến môi trường
source /opt/microservices/.env.prod

# Backup SQL Server
docker exec orderdb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$ORDERDB_SA_PASSWORD" -Q "BACKUP DATABASE OrderDb TO DISK = '/var/opt/mssql/backup/OrderDb_$DATE.bak'"
docker cp "orderdb:/var/opt/mssql/backup/OrderDb_$DATE.bak" "$BACKUP_DIR/$DATE/"

# Backup MySQL
docker exec productdb mysqldump -u root -p"$PRODUCTDB_ROOT_PASSWORD" --all-databases > "$BACKUP_DIR/$DATE/mysql.sql"

# Backup PostgreSQL
PGPASSWORD="$CUSTOMERDB_PASSWORD" docker exec -e PGPASSWORD="$CUSTOMERDB_PASSWORD" customerdb pg_dump -U "$CUSTOMERDB_USER" CustomerDb > "$BACKUP_DIR/$DATE/customerdb.sql"

# Backup Portainer config
docker run --rm -v portainer_data:/data -v "$BACKUP_DIR/$DATE":/backup alpine tar czf /backup/portainer.tar.gz /data

# Nén và xóa backup cũ hơn 30 ngày
cd "$BACKUP_DIR"
tar czf "$DATE.tar.gz" "$DATE"
rm -rf "$DATE"
find . -name "*.tar.gz" -mtime +30 -delete

echo "Backup hoàn tất: $BACKUP_DIR/$DATE.tar.gz"
```

Thêm vào crontab (Linux):
```sh
# Chạy backup mỗi ngày lúc 2h sáng
0 2 * * * /opt/microservices/backup.sh
```

---

## 8. Chuyển Từ Development Sang Production

### 8.1. Chuẩn Bị

```sh
# 1. Đảm bảo tất cả services hoạt động trên môi trường development
docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml up -d --build

# 2. Kiểm tra logs không có lỗi
docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml logs --tail=50

# 3. Kiểm tra API Gateway hoạt động
curl http://localhost:5293/health
```

### 8.2. Copy Lên Production Server

```sh
# Tạo thư mục trên server
ssh user@production-server
sudo mkdir -p /opt/microservices
sudo chown $USER:$USER /opt/microservices
exit

# Copy toàn bộ source code và cấu hình
scp -r . user@production-server:/opt/microservices/

# Hoặc dùng rsync để đồng bộ (nhanh hơn khi cập nhật)
rsync -avz --exclude '.git' --exclude 'bin' --exclude 'obj' --exclude '.vs' . user@production-server:/opt/microservices/
```

### 8.3. Thiết Lập Trên Server

```sh
# SSH vào server
ssh user@production-server
cd /opt/microservices

# Copy và cấu hình biến môi trường
cp .env.prod.example .env.prod
nano .env.prod   # Nhập mật khẩu thực tế

# Đặt quyền hạn chế cho file .env.prod
chmod 600 .env.prod

# Khởi động services
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d

# Kiểm tra tất cả containers đã chạy
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod ps
```

### 8.4. Kiểm Tra Sau Deploy

```sh
# 1. Kiểm tra tất cả containers đang chạy
docker ps | findstr /R microservices

# 2. Kiểm tra logs không có lỗi critical
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod logs --tail=20

# 3. Test API Gateway
curl http://localhost:5293/api/product/health

# 4. Truy cập Portainer để kiểm tra giao diện
# https://<IP-SERVER>:9443

# 5. Kiểm tra Elasticsearch
curl http://localhost:9200 -u elastic:"$ELASTIC_PASSWORD"

# 6. Kiểm tra RabbitMQ
# http://<IP-SERVER>:15672
```

---

## 9. Giải Thích Chi Tiết Các Lệnh

### 9.1. Khởi Động Services

```sh
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d
```

| Thành phần             | Ý nghĩa                                                   |
|------------------------|-----------------------------------------------------------|
| `docker compose`       | Docker Compose v2 (plugin tích hợp trong Docker CLI)     |
| `-f docker-compose.yml`| File cấu hình base (services, networks, volumes)          |
| `-f docker-compose.prod.yml`| File override cho production (mật khẩu, logging, Portainer) |
| `--env-file .env.prod` | File chứa biến môi trường cho production                   |
| `up`                   | Tạo và khởi động containers                                |
| `-d`                   | Detached mode — chạy ngầm ở background                     |

### 9.2. Build và Khởi Động

```sh
docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml up -d --build
```

| Thành phần             | Ý nghĩa                                                   |
|------------------------|-----------------------------------------------------------|
| `--build`              | Build lại images trước khi khởi động containers            |
| `docker-compose.apps.yml`| File cấu hình application services (.NET APIs + Jaeger)  |

### 9.3. Xem Logs

```sh
docker compose logs -f --tail=100 orderdb
```

| Thành phần | Ý nghĩa                                           |
|------------|---------------------------------------------------|
| `logs`     | In logs của containers                             |
| `-f`       | Follow mode — xem real-time (giống tail -f)        |
| `--tail=100`| Chỉ hiển thị 100 dòng gần nhất                    |
| `orderdb`  | Tên service cụ thể (nếu không chỉ định, xem tất cả)|

### 9.4. Dừng Services

```sh
docker compose -f docker-compose.yml -f docker-compose.dev.yml down
```

| Thành phần | Ý nghĩa                                                              |
|------------|----------------------------------------------------------------------|
| `down`     | Dừng và xóa containers, networks (giữ volumes và images)              |
| `-v`       | Xóa luôn volumes (CẢNH BÁO: mất dữ liệu databases!)                  |
| `--rmi all`| Xóa luôn images được tạo từ Dockerfile                                |

### 9.5. Liệt Kê và Kiểm Tra

```sh
docker ps                          # Containers đang chạy
docker ps -a                       # Tất cả containers (kể cả đã dừng)
docker compose ps                  # Trạng thái services trong compose
docker stats                       # Resource usage real-time
docker system df                   # Dung lượng Docker đang dùng
docker network inspect microservices # Chi tiết network
docker volume ls                   # Liệt kê volumes
docker images                      # Liệt kê images
```

---

## 10. Các Lỗi Thường Gặp

### 10.1. Lỗi Port Đã Được Sử Dụng

```
Error: Bind for 0.0.0.0:1435 failed: port is already allocated
```

**Nguyên nhân:** Port đã bị process khác hoặc container khác chiếm dụng.

**Giải pháp:**
```sh
# Tìm process đang dùng port
netstat -ano | findstr :1435

# Dừng process đó hoặc đổi port trong docker-compose
# ports:
#   - "1438:1433"   # Đổi port host
```

### 10.2. Lỗi Container Exit Ngay Sau Khi Start

```
CONTAINER ID   IMAGE     COMMAND   CREATED    STATUS                        PORTS     NAMES
abc123...      mysql     ...       1 min ago  Exited (1) 30 seconds ago     ...       productdb
```

**Nguyên nhân:** Lỗi cấu hình, thiếu biến môi trường, hoặc resource không đủ.

**Giải pháp:**
```sh
# Xem logs để biết lỗi chính xác
docker logs productdb

# Kiểm tra biến môi trường
docker inspect productdb | findstr "MYSQL"

# Kiểm tra resource
docker stats --no-stream
```

### 10.3. Lỗi Không Kết Nối Được Từ App Đến Database

```
System.Data.SqlClient.SqlException: Cannot open database 'OrderDb'
```

**Nguyên nhân:** Connection string sai, network sai, hoặc database chưa sẵn sàng.

**Giải pháp:**
```sh
# Kiểm tra các container có cùng network không
docker network inspect microservices

# Kiểm tra tên service trong connection string (dùng tên service, không dùng localhost)
# Đúng: "Server=orderdb,1433;Database=OrderDb;..."
# Sai: "Server=localhost,1433;..."

# Thêm depends_on và health check
```

### 10.4. Lỗi Hết Disk Space

```
Error response from daemon: write /var/lib/docker/overlay2/...: no space left on device
```

**Giải pháp:**
```sh
# Kiểm tra dung lượng
df -h
docker system df

# Xóa logs cũ của containers
docker compose -f docker-compose.prod.yml logs --tail=0

# Dọn dẹp Docker
docker system prune -f

# Cấu hình log rotation (đã có trong docker-compose.prod.yml)
# logging:
#   options:
#     max-size: "10m"
#     max-file: "3"
```

### 10.5. Lỗi Elasticsearch Không Khởi Động

```
ElasticsearchException: java.io.IOException: failed to read [id:0, file:/usr/share/elasticsearch/data/nodes/0]
```

**Nguyên nhân:** Elasticsearch không có quyền ghi vào volume, hoặc vm.max_map_count chưa đủ.

**Giải pháp (Linux):**
```sh
# Tăng vm.max_map_count (cần thiết cho Elasticsearch)
sudo sysctl -w vm.max_map_count=262144

# Làm cho persistent
echo "vm.max_map_count=262144" | sudo tee -a /etc/sysctl.conf
```

### 10.6. Lỗi SQL Server Container

```
SQL Server 2022 will run as non-root by default
This container is running as user mssql
```

**Giải pháp:**
```sh
# Chạy với user root nếu cần
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -u 0 mcr.microsoft.com/mssql/server:2022-latest

# Hoặc cấp quyền cho thư mục data
# Trên host:
sudo chown -R 10001:10001 /var/lib/docker/volumes/sqlserver_data/_data
```

---

## 11. Mẹo Hữu Ích

### 11.1. Aliases Cho Tiện Lợi

Thêm vào `~/.bashrc` hoặc `~/.zshrc` (Linux/Mac) hoặc `$PROFILE` (PowerShell):

```sh
# === Development Aliases ===
alias dc-dev='docker compose -f docker-compose.yml -f docker-compose.dev.yml'
alias dc-dev-all='docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml'

# === Production Aliases ===
alias dc-prod='docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod'

# === Shortcuts ===
alias dcup='docker compose up -d'
alias dcdown='docker compose down'
alias dclogs='docker compose logs -f'
alias dcps='docker compose ps'
alias dps='docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
```

### 11.2. Xem Resource Usage

```sh
# Real-time monitoring
docker stats

# Một lần (không liên tục)
docker stats --no-stream

# Chỉ một container
docker stats product-api

# Custom format
docker stats --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}"
```

### 11.3. Dọn Dẹp Hệ Thống

```sh
# An toàn — chỉ xóa những thứ không dùng
docker container prune          # Xóa containers đã dừng
docker image prune              # Xóa dangling images
docker volume prune             # Xóa volumes không dùng
docker network prune            # Xóa networks không dùng
docker builder prune            # Xóa build cache

# Mạnh hơn — xóa cả images không dùng đến
docker image prune -a

# CẢNH BÁO — xóa gần như tất cả
docker system prune -a --volumes
```

### 11.4. Debug Container

```sh
# Mở shell bên trong container đang chạy
docker exec -it orderdb bash

# Chạy lệnh một lần trong container
docker exec productdb mysql -u root -p"$PASSWORD" -e "SHOW DATABASES;"

# Copy file từ container ra host
docker cp orderdb:/var/opt/mssql/backup/OrderDb.bak .

# Copy file từ host vào container
docker cp ./init.sql orderdb:/docker-entrypoint-initdb.d/

# Xem chi tiết cấu hình container
docker inspect orderdb

# Xem resource usage chi tiết
docker stats --no-stream --format "{{.Name}}: {{.MemUsage}} CPU: {{.CPUPerc}}"
```

### 11.5. Kiểm Tra Health

```sh
# Kiểm tra health status của containers
docker ps --filter "health=healthy"
docker ps --filter "health=unhealthy"

# Kiểm tra restart count
docker inspect --format '{{.Name}} - Restarts: {{.RestartCount}}' $(docker ps -aq)

# Kiểm tra container đã chạy được bao lâu
docker ps --format "table {{.Names}}\t{{.RunningFor}}\t{{.Status}}"
```

### 11.6. Network Debugging

```sh
# Kiểm tra DNS resolution giữa các containers
docker exec product-api ping orderdb

# Kiểm tra kết nối TCP
docker exec product-api bash -c "cat < /dev/tcp/orderdb/1433"

# Xem chi tiết network
docker network inspect microservices

# Kiểm tra container nào đang trong network
docker network inspect microservices --format '{{range .Containers}}{{.Name}} {{end}}'
```

---

## 12. Checklist Trước Khi Deploy Production

### 12.1. Chuẩn Bị

- [ ] **Đã test tất cả services trên môi trường development** — không có lỗi critical
- [ ] **Đã kiểm tra logs** — không có cảnh báo hoặc lỗi bất thường
- [ ] **Đã chạy unit tests và integration tests** — tất cả đều pass
- [ ] **Đã kiểm tra API Gateway** — tất cả routes hoạt động đúng
- [ ] **Đã kiểm tra Jaeger tracing** — distributed traces hoạt động

### 12.2. Bảo Mật

- [ ] **Đã cấu hình mật khẩu mạnh trong .env.prod** (tối thiểu 16 ký tự, hỗn hợp ký tự)
- [ ] **Đã loại bỏ mật khẩu mặc định** — không còn "Passw0rd!" hay "admin1234"
- [ ] **File .env.prod đã được thêm vào .gitignore** — không commit lên Git
- [ ] **File .env.prod có quyền 600** — chỉ owner mới đọc được
- [ ] **Portainer chỉ mở port 9443 (HTTPS)** — không mở port 9000 (HTTP)
- [ ] **Docker socket được mount với chế độ read-only** (`:ro`)
- [ ] **security_opt: no-new-privileges:true** đã được cấu hình
- [ ] **Redis** yêu cầu password (`--requirepass`)
- [ ] **MongoDB** yêu cầu authentication (username + password)
- [ ] **Elasticsearch** đã bật xpack.security

### 12.3. Hạ Tầng

- [ ] **Firewall đã được cấu hình** — chỉ mở các port cần thiết
- [ ] **vm.max_map_count** đã được set cho Elasticsearch (262144)
- [ ] **Server có đủ tài nguyên** — RAM, CPU, Disk cho tất cả containers
- [ ] **Log rotation đã được cấu hình** — `max-size: 10m`, `max-file: 3`
- [ ] **Network microservices bridge** đã được tạo
- [ ] **SSL certificates** cho Portainer đã sẵn sàng

### 12.4. Backup

- [ ] **Script backup tự động** đã được tạo và kiểm tra
- [ ] **Backup databases** đã được kiểm tra (có thể restore được không?)
- [ ] **Backup Portainer config** đã được thiết lập
- [ ] **Lịch crontab** đã được cấu hình (backup hàng ngày)
- [ ] **Backup được lưu ở vị trí an toàn** (khác server production)
- [ ] **Đã test disaster recovery plan** — restore từ backup hoạt động

### 12.5. Kiểm Tra Trước Khi Chạy

- [ ] **Copy source code và cấu hình lên server** — `scp` hoặc `rsync`
- [ ] **Chạy thử với compose** — `docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d`
- [ ] **Tất cả containers đều ở trạng thái Up** — kiểm tra với `docker ps`
- [ ] **Kiểm tra kết nối từ API services đến databases**
- [ ] **Kiểm tra API Gateway trả về response đúng**
- [ ] **Kiểm tra Portainer có thể truy cập qua HTTPS**
- [ ] **Kiểm tra Elasticsearch hoạt động với authentication**
- [ ] **Kiểm tra RabbitMQ management interface**

### 12.6. Giám Sát & Vận Hành

- [ ] **Đã cấu hình monitoring** — Portainer dashboard hoạt động
- [ ] **Đã thiết lập alerts** — webhooks hoặc email notifications
- [ ] **Đã document tất cả credentials** — lưu trữ an toàn (password manager)
- [ ] **Đã có kế hoạch update** — quy trình pull image mới và restart
- [ ] **Đã kiểm tra dung lượng đĩa** — đủ cho dữ liệu và logs
- [ ] **Đã cấu hình health checks** cho critical services

### 12.7. Sau Khi Deploy

- [ ] **Kiểm tra toàn bộ hệ thống** — end-to-end test qua API Gateway
- [ ] **Kiểm tra logs không có lỗi** — tất cả services hoạt động ổn định
- [ ] **Kiểm tra resource usage** — CPU, RAM không ở mức quá cao
- [ ] **Tạo snapshot đầu tiên** — backup ngay sau khi deploy thành công
- [ ] **Thông báo cho team** — deployment hoàn tất

---

## Phụ Lục: Cấu Trúc File Docker Compose

```
MicroserviceApp/
├── docker-compose.yml           # Base: databases + infrastructure
├── docker-compose.dev.yml       # Development override
├── docker-compose.prod.yml      # Production override (có Portainer)
├── docker-compose.apps.yml      # Application services (.NET APIs + Jaeger)
├── docker-compose.override.yml  # Default override (dùng khi không chỉ định -f)
├── .env.prod.example            # Template biến môi trường production
├── .dockerignore                # File loại trừ khi build Docker images
└── src/
    └── Services|ApiGateways/
        └── */Dockerfile         # Dockerfile cho từng service
```

**Thứ tự merge của docker-compose files:**

```sh
# Development (override từ phải sang trái)
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d
#   → docker-compose.yml:     định nghĩa services, networks, volumes
#   → docker-compose.dev.yml: override environment, ports, resources cho dev

# Production
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d
#   → docker-compose.yml:     định nghĩa services, networks, volumes
#   → docker-compose.prod.yml: override environment, logging, security, Portainer
#   → --env-file .env.prod:   biến môi trường từ file

# Full stack (cả apps)
docker compose -f docker-compose.yml -f docker-compose.dev.yml -f docker-compose.apps.yml up -d --build
#   → Thêm application services và Jaeger
```

---

> **Tài liệu này dành cho dự án MicroserviceApp.**  
> Mọi thắc mắc hoặc đề xuất cải thiện, vui lòng tạo issue trên repository.
