# Git Workflow - H??ng D?n S? D?ng Git

## ?? Các File Quan Tr?ng ?ã Thêm

### 1. `.gitignore`
- Lo?i b? các file không c?n thi?t kh?i Git
- B?o v? thông tin nh?y c?m (passwords, keys)
- Gi? repo s?ch s?

### 2. `.gitattributes`
- ??m b?o line endings ??ng nh?t (LF vs CRLF)
- T?i ?u merge conflicts
- X? lý file binaries ?úng cách

### 3. `.dockerignore`
- T?i ?u Docker build
- Gi?m kích th??c Docker images
- Lo?i b? files không c?n trong containers

---

## ?? Workflow C? B?n

### Ki?m tra tr?ng thái hi?n t?i:
```sh
git status
```

### Xem files ?ã b? ignore:
```sh
git status --ignored
```

### Thêm t?t c? files m?i:
```sh
git add .
```

### Commit thay ??i:
```sh
git commit -m "feat: add comprehensive .gitignore and Docker configs"
```

### Push lên GitHub:
```sh
git push origin main
```

---

## ?? Ki?m Tra Files Nào ?ang B? Ignore

### Ki?m tra m?t file c? th?:
```sh
# Ki?m tra xem file có b? ignore không
git check-ignore -v bin/Debug/net8.0/MyApp.dll

# K?t qu? s? hi?n th? rule nào ?ang ignore file này
```

### Xem t?t c? files b? ignored:
```sh
git status --ignored
```

---

## ?? D?n D?p Files Không C?n Thi?t

### Xóa files ?ã tracked nh?ng gi? ?ã thêm vào .gitignore:

```sh
# 1. Commit t?t c? changes tr??c
git add .
git commit -m "chore: save work before cleanup"

# 2. Xóa t?t c? files t? Git index (không xóa kh?i disk)
git rm -r --cached .

# 3. Thêm l?i t?t c? files (l?n này s? respect .gitignore)
git add .

# 4. Commit cleanup
git commit -m "chore: apply .gitignore rules to existing files"

# 5. Push lên remote
git push origin main
```

**?? C?nh báo**: L?nh này s? xóa files kh?i Git history. N?u files ?ã ???c push lên remote, nh?ng ng??i khác s? th?y files b? xóa khi h? pull.

---

## ?? Các Files KHÔNG NÊN Commit

### ? Files luôn b? ignore:

1. **Build outputs**
   - `bin/`, `obj/`
   - Lý do: ???c t?o l?i m?i l?n build

2. **IDE configs**
   - `.vs/`, `.vscode/`, `.idea/`
   - Lý do: M?i ng??i có settings riêng

3. **Environment files**
   - `.env.prod`, `.env.local`
   - Lý do: Ch?a passwords và secrets

4. **Docker volumes**
   - `postgres-data/`, `mysql-data/`
   - Lý do: D? li?u local, không c?n share

5. **Logs**
   - `*.log`, `logs/`
   - Lý do: Files t?m th?i, không quan tr?ng

6. **Certificates**
   - `*.pem`, `*.key`, `*.pfx`
   - Lý do: Thông tin b?o m?t

### ? Files NÊN commit:

1. **Source code**
   - `*.cs`, `*.csproj`

2. **Configurations**
   - `appsettings.json` (không có secrets)
   - `docker-compose.yml`

3. **Documentation**
   - `README.md`, `DOCKER_GUIDE.md`

4. **Example files**
   - `.env.example`, `.env.prod.example`

---

## ?? B?o V? Thông Tin Nh?y C?m

### N?u ?ã commit password/secret nh?m:

```sh
# 1. Xóa file kh?i Git nh?ng gi? trên disk
git rm --cached .env.prod

# 2. Thêm vào .gitignore
echo ".env.prod" >> .gitignore

# 3. Commit
git add .gitignore
git commit -m "chore: remove sensitive file from Git"

# 4. Push
git push origin main
```

**?? Quan tr?ng**: File v?n còn trong Git history! ?? xóa hoàn toàn, c?n dùng `git filter-branch` ho?c BFG Repo-Cleaner.

### ??i t?t c? passwords:
Sau khi xóa secrets kh?i Git, **B?T BU?C** ph?i ??i t?t c? passwords vì chúng ?ã b? exposed!

---

## ?? Branching Strategy

### Feature Branch Workflow:

```sh
# 1. T?o branch m?i t? main
git checkout main
git pull origin main
git checkout -b feature/add-order-service

# 2. Làm vi?c trên branch
# ... code changes ...

# 3. Commit changes
git add .
git commit -m "feat: add Order service API"

# 4. Push lên remote
git push origin feature/add-order-service

# 5. T?o Pull Request trên GitHub

# 6. Sau khi merge, xóa branch local
git checkout main
git pull origin main
git branch -d feature/add-order-service
```

### Branch naming convention:

- `feature/ten-tinh-nang` - Tính n?ng m?i
- `bugfix/ten-bug` - S?a bug
- `hotfix/ten-issue` - S?a urgent bug trong production
- `refactor/ten-refactor` - Refactor code
- `docs/ten-doc` - C?p nh?t documentation

---

## ?? Commit Message Convention

### Format:
```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types:
- `feat`: Tính n?ng m?i
- `fix`: S?a bug
- `docs`: Thay ??i documentation
- `style`: Format code (không thay ??i logic)
- `refactor`: Refactor code
- `test`: Thêm ho?c s?a tests
- `chore`: Maintenance tasks (build, configs, etc.)
- `perf`: C?i thi?n performance

### Examples:

```sh
# Good commits
git commit -m "feat(ordering): add create order endpoint"
git commit -m "fix(basket): resolve Redis connection timeout"
git commit -m "docs: update Docker setup guide"
git commit -m "chore: add .gitignore and .dockerignore"

# Bad commits (avoid these)
git commit -m "fix bug"
git commit -m "update"
git commit -m "WIP"
```

---

## ?? Sync v?i Remote Repository

### Pull changes t? remote:
```sh
# Pull và merge
git pull origin main

# Pull và rebase (recommended)
git pull --rebase origin main
```

### X? lý conflicts:
```sh
# 1. Git s? báo conflicts
# 2. M? files có conflict và s?a
# 3. Mark resolved
git add .

# 4a. N?u ?ang merge
git commit

# 4b. N?u ?ang rebase
git rebase --continue
```

---

## ??? Các L?nh H?u Ích

### Xem history:
```sh
# Xem log ??p
git log --oneline --graph --all --decorate

# Xem changes c?a m?t commit
git show <commit-hash>

# Xem changes ch?a commit
git diff

# Xem changes ?ã staged
git diff --staged
```

### Undo changes:
```sh
# Undo changes ch?a stage (nguy hi?m!)
git checkout -- <file>

# Unstage file (gi? changes)
git reset HEAD <file>

# Undo commit g?n nh?t (gi? changes)
git reset --soft HEAD~1

# Undo commit g?n nh?t (xóa changes - NGUY HI?M!)
git reset --hard HEAD~1
```

### Stash (l?u t?m changes):
```sh
# L?u changes t?m
git stash

# Xem stash list
git stash list

# Apply stash g?n nh?t
git stash pop

# Apply stash c? th?
git stash apply stash@{0}

# Xóa stash
git stash drop
```

---

## ?? Ki?m Tra Repository Health

### Xem kích th??c repo:
```sh
git count-objects -vH
```

### Tìm files l?n trong history:
```sh
git rev-list --objects --all | git cat-file --batch-check='%(objecttype) %(objectname) %(objectsize) %(rest)' | grep '^blob' | sort -k3 -n -r | head -n 10
```

### Xóa files l?n kh?i history (nâng cao):
```sh
# S? d?ng BFG Repo-Cleaner (recommended)
java -jar bfg.jar --delete-files '*.mdf' .
git reflog expire --expire=now --all
git gc --prune=now --aggressive
```

---

## ?? Best Practices

### ? Nên làm:

1. **Commit th??ng xuyên** v?i messages rõ ràng
2. **Pull tr??c khi push** ?? tránh conflicts
3. **Review code** tr??c khi commit
4. **Test code** tr??c khi push
5. **S? d?ng branches** cho features m?i
6. **Không commit secrets** (passwords, keys)
7. **Gi? commits nh?** và focused
8. **Document changes** trong commit messages

### ? Không nên:

1. ? Commit files binary l?n (videos, databases)
2. ? Commit generated files (bin/, obj/)
3. ? Force push lên main branch
4. ? Commit secrets/passwords
5. ? Commit work-in-progress lên main
6. ? S? d?ng `git add -A` mà không review
7. ? Commit messages không rõ ràng ("fix", "update")
8. ? Rewrite history c?a shared branches

---

## ?? Troubleshooting

### Problem: Files ?ã ignore v?n xu?t hi?n trong git status
**Solution:**
```sh
git rm -r --cached .
git add .
git commit -m "chore: apply .gitignore"
```

### Problem: Conflict khi pull
**Solution:**
```sh
# Xem files conflict
git status

# S?a conflicts trong files
# Sau ?ó:
git add .
git commit -m "merge: resolve conflicts"
```

### Problem: Commit nh?m file
**Solution:**
```sh
# Ch?a push
git reset --soft HEAD~1
# S?a và commit l?i

# ?ã push (c?n c?n th?n!)
git revert <commit-hash>
git push origin main
```

### Problem: C?n quay l?i version c?
**Solution:**
```sh
# T?o branch t? commit c?
git checkout -b recovery-branch <commit-hash>

# Ho?c reset (nguy hi?m!)
git reset --hard <commit-hash>
```

---

## ?? Tài Li?u Tham Kh?o

- [Git Official Documentation](https://git-scm.com/doc)
- [GitHub Guides](https://guides.github.com/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Git Ignore Generator](https://www.toptal.com/developers/gitignore)

---

## ?? Useful Git Aliases

Thêm vào `~/.gitconfig`:

```ini
[alias]
    st = status
    co = checkout
    br = branch
    ci = commit
    unstage = reset HEAD --
    last = log -1 HEAD
    visual = log --oneline --graph --all --decorate
    amend = commit --amend --no-edit
    undo = reset --soft HEAD~1
```

Sau ?ó có th? dùng:
```sh
git st          # thay vì git status
git co main     # thay vì git checkout main
git visual      # xem git tree ??p
```
