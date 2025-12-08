# H??ng D?n S? D?ng Docker Compose

## Môi Tr??ng Development (Không dùng Portainer)

S? d?ng giao di?n Docker Desktop ?? qu?n lý containers.

```sh
# Kh?i ??ng t?t c? services
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# Xem logs
docker-compose -f docker-compose.yml -f docker-compose.dev.yml logs -f

# D?ng t?t c? services
docker-compose -f docker-compose.yml -f docker-compose.dev.yml down

# Build l?i và kh?i ??ng
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d --build
```

**Truy c?p các services:**
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- Kibana: http://localhost:5601
- Elasticsearch: http://localhost:9200

---

## Môi Tr??ng Production (Có Portainer)

### Thi?t l?p l?n ??u:

```sh
# 1. Copy file template bi?n môi tr??ng
cp .env.prod.example .env.prod

# 2. Ch?nh s?a .env.prod v?i m?t kh?u th?c t?
nano .env.prod

# 3. Kh?i ??ng services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d

# 4. Truy c?p Portainer (l?n ??u tiên)
# M?: https://ip-server-cua-ban:9443
# T?o tài kho?n admin
# K?t n?i v?i Docker environment local
```

### V?n hành hàng ngày:

```sh
# Xem t?t c? containers
docker ps

# Xem logs
docker logs -f orderdb

# Kh?i ??ng l?i m?t service
docker restart orderdb

# C?p nh?t services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod pull
docker-compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d
```

**Truy c?p các services:**
- Portainer: https://ip-server-cua-ban:9443
- RabbitMQ Management: http://ip-server-cua-ban:15672 (dùng thông tin t? .env.prod)
- Kibana: http://ip-server-cua-ban:5601
- Elasticsearch: http://ip-server-cua-ban:9200

---

## Tính N?ng C?a Portainer Trong Production

### 1. Qu?n Lý Containers
- Kh?i ??ng/D?ng/Kh?i ??ng l?i containers không c?n SSH
- Xem th?ng kê th?i gian th?c (CPU, Memory, Network)
- Truy c?p console c?a container
- Xem và t?i logs

### 2. ?a Ng??i Dùng
- T?o users v?i các vai trò khác nhau:
  - **Admin**: Toàn quy?n truy c?p
  - **Ch? ??c**: Ch? xem containers, logs
  - **H? tr?**: Ch? kh?i ??ng l?i containers

### 3. Truy C?p T? Xa
- Truy c?p t? b?t k? ?âu qua HTTPS
- Giao di?n web thân thi?n v?i mobile
- Không c?n VPN (n?u ???c b?o m?t ?úng cách)

### 4. Giám Sát & C?nh Báo
- Thi?t l?p webhooks khi container l?i
- Giám sát m?c s? d?ng tài nguyên
- Nh?n thông báo khi containers d?ng

### 5. Qu?n Lý Stack
- Deploy stacks m?i t? Git repositories
- C?p nh?t stacks ch? v?i m?t click
- Quay l?i phiên b?n tr??c ?ó

---

## Các Th?c Hành B?o M?t T?t Nh?t

### Cho Portainer Trong Production:

1. **Ch? dùng HTTPS**
   ```yaml
   ports:
     - "9443:9443"  # HTTPS
     # Không bao gi? m? port 9000 (HTTP) trong production
   ```

2. **M?t kh?u admin m?nh**
   - T?i thi?u 12 ký t?
   - K?t h?p ch? hoa, ch? th??ng, s? và ký t? ??c bi?t

3. **H?n ch? truy c?p**
   ```sh
   # Dùng firewall ?? gi?i h?n truy c?p
   sudo ufw allow from IP_CUA_BAN to any port 9443
   ```

4. **Docker socket ch? ??c** (n?u có th?)
   ```yaml
   volumes:
     - /var/run/docker.sock:/var/run/docker.sock:ro
   ```

5. **C?p nh?t th??ng xuyên**
   ```sh
   docker pull portainer/portainer-ce:latest
   docker-compose -f docker-compose.prod.yml up -d portainer
   ```

6. **B?t audit logs**
   - Theo dõi ai làm gì và khi nào
   - Có s?n trong Portainer Business Edition

---

## So Sánh: Development vs Production

| Tính N?ng | Development | Production |
|-----------|-------------|------------|
| **Công C? Qu?n Lý** | Docker Desktop | Portainer |
| **Chính Sách Kh?i ??ng L?i** | unless-stopped | always |
| **M?t Kh?u** | Hardcode (??n gi?n) | Bi?n môi tr??ng (ph?c t?p) |
| **Logging** | M?c ??nh | Gi?i h?n kích th??c + xoay vòng |
| **HTTPS** | Không c?n | B?t bu?c |
| **Giám Sát** | Tùy ch?n | Thi?t y?u |
| **Backups** | Không quan tr?ng | T? ??ng hóa |

---

## X? Lý S? C?

### Không truy c?p ???c Portainer:

```sh
# Ki?m tra container có ?ang ch?y không
docker ps | grep portainer

# Ki?m tra logs
docker logs portainer

# Ki?m tra firewall
sudo ufw status

# Kh?i ??ng l?i Portainer
docker restart portainer
```

### Quên m?t kh?u admin Portainer:

```sh
# Reset m?t kh?u admin
docker stop portainer
docker run --rm -v portainer_data:/data portainer/helper-reset-password

# Kh?i ??ng l?i Portainer
docker start portainer
```

---

## Chi?n L??c Backup

### Databases:

```sh
# SQL Server
docker exec orderdb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Passw0rd!" -Q "BACKUP DATABASE OrderDb TO DISK = '/var/opt/mssql/backup/OrderDb.bak'"

# MySQL
docker exec productdb mysqldump -u root -pPassw0rd! --all-databases > backup.sql

# PostgreSQL
docker exec customerdb pg_dump -U admin CustomerDb > customerdb_backup.sql

# MongoDB
docker exec inventorydb mongodump --out /backup

# Redis (n?u b?t persistence)
docker exec basketdb redis-cli --rdb /data/dump.rdb
```

### D? li?u Portainer:

```sh
# Backup c?u hình Portainer
docker run --rm -v portainer_data:/data -v $(pwd):/backup alpine tar czf /backup/portainer-backup.tar.gz /data
```

---

## Chuy?n T? Development Sang Production

1. **Test local tr??c:**
   ```sh
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

2. **Chuy?n lên production server:**
   ```sh
   scp -r src/ user@production-server:/opt/microservices/
   ```

3. **Thi?t l?p bi?n môi tr??ng trên server**

4. **Kh?i ??ng services:**
   ```sh
   cd /opt/microservices/src
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d
   ```

5. **Truy c?p Portainer và ki?m tra t?t c? containers ?ang ch?y**

---

## Tham Kh?o Nhanh

### Development:
```sh
# Alias ?? ti?n l?i
alias dc-dev='docker-compose -f docker-compose.yml -f docker-compose.dev.yml'

# Cách dùng
dc-dev up -d
dc-dev logs -f orderdb
dc-dev down
```

### Production:
```sh
# Alias
alias dc-prod='docker-compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod'

# Cách dùng
dc-prod up -d
dc-prod logs -f orderdb
dc-prod down
```

### Truy C?p Portainer:
- Development: ? Không c?n (dùng Docker Desktop)
- Production: ? https://ip-server-cua-ban:9443

---

## Gi?i Thích Chi Ti?t Các L?nh

### 1. Kh?i ??ng services:
```sh
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
```
- `-f docker-compose.yml`: S? d?ng file c?u hình base
- `-f docker-compose.dev.yml`: K?t h?p v?i file c?u hình development
- `up`: Kh?i ??ng containers
- `-d`: Ch?y ? ch? ?? background (detached mode)

### 2. Xem logs:
```sh
docker-compose logs -f orderdb
```
- `logs`: Xem logs c?a container
- `-f`: Follow mode (xem real-time)
- `orderdb`: Tên container c? th?

### 3. D?ng services:
```sh
docker-compose down
```
- `down`: D?ng và xóa containers (nh?ng gi? volumes)

### 4. Xem tr?ng thái:
```sh
docker ps
```
- Hi?n th? danh sách các containers ?ang ch?y

---

## Các L?i Th??ng G?p

### L?i 1: Port ?ã ???c s? d?ng
```
Error: Bind for 0.0.0.0:1435 failed: port is already allocated
```

**Gi?i pháp:**
```sh
# Ki?m tra process ?ang dùng port
netstat -ano | findstr :1435

# Ho?c ??i port trong docker-compose file
ports:
  - "1436:1433"  # ??i t? 1435 sang 1436
```

### L?i 2: Container không kh?i ??ng ???c
```sh
# Xem logs ?? bi?t l?i
docker logs orderdb

# Xem chi ti?t container
docker inspect orderdb
```

### L?i 3: Không k?t n?i ???c database
```sh
# Ki?m tra container có ch?y không
docker ps | grep orderdb

# Ki?m tra network
docker network inspect microservices

# Test k?t n?i
docker exec -it orderdb bash
```

---

## M?o H?u Ích

### 1. Xem resource usage:
```sh
docker stats
```

### 2. D?n d?p system:
```sh
# Xóa containers ?ã d?ng
docker container prune

# Xóa images không dùng
docker image prune

# Xóa volumes không dùng
docker volume prune

# Xóa t?t c? (C?NH BÁO: S? m?t d? li?u!)
docker system prune -a --volumes
```

### 3. Backup nhanh m?t database:
```sh
# SQL Server
docker exec orderdb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Passw0rd!" -Q "BACKUP DATABASE OrderDb TO DISK = '/var/opt/mssql/backup/OrderDb_$(date +%Y%m%d).bak'"
```

### 4. Restore database:
```sh
# SQL Server
docker exec orderdb /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Passw0rd!" -Q "RESTORE DATABASE OrderDb FROM DISK = '/var/opt/mssql/backup/OrderDb.bak'"
```

---

## Checklist Tr??c Khi Deploy Production

- [ ] ?ã test t?t c? services ? local
- [ ] ?ã c?u hình t?t c? m?t kh?u m?nh trong .env.prod
- [ ] ?ã setup HTTPS cho Portainer
- [ ] ?ã c?u hình firewall
- [ ] ?ã thi?t l?p backup t? ??ng
- [ ] ?ã test k?t n?i t? API services ??n databases
- [ ] ?ã c?u hình monitoring và alerts
- [ ] ?ã document t?t c? credentials và l?u tr? an toàn
- [ ] ?ã setup log rotation
- [ ] ?ã test disaster recovery plan
