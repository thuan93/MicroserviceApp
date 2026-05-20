# Hướng dẫn sử dụng Git — Git Workflow Guide

> Phiên bản: 1.0  
> Mục đích: Chuẩn hóa quy trình làm việc với Git trong dự án MicroserviceApp.

---

## Mục lục

1. [Các file quan trọng](#1-các-file-quan-trọng)
2. [Workflow cơ bản](#2-workflow-cơ-bản)
3. [Kiểm tra file nào đang bị ignore](#3-kiểm-tra-file-nào-đang-bị-ignore)
4. [Dọn dẹp file không cần thiết](#4-dọn-dẹp-file-không-cần-thiết)
5. [Các file KHÔNG NÊN commit](#5-các-file-không-nên-commit)
6. [Bảo vệ thông tin nhạy cảm](#6-bảo-vệ-thông-tin-nhạy-cảm)
7. [Branching strategy](#7-branching-strategy)
8. [Commit message convention](#8-commit-message-convention)
9. [Sync với remote repository & xử lý conflicts](#9-sync-với-remote-repository--xử-lý-conflicts)
10. [Các lệnh hữu ích](#10-các-lệnh-hữu-ích)
11. [Kiểm tra repository health](#11-kiểm-tra-repository-health)
12. [Best practices](#12-best-practices)
13. [Troubleshooting](#13-troubleshooting)
14. [Git aliases](#14-git-aliases)

---

## 1. Các file quan trọng

### `.gitignore`

File `.gitignore` liệt kê các pattern mà Git sẽ bỏ qua (không track). Mỗi pattern trên một dòng.

```gitignore
# Dependencies
node_modules/
vendor/

# Build output
dist/
build/
out/
*.exe
*.dll
*.so
*.dylib

# Environment files
.env
.env.local
.env.*.local

# IDE & OS
.vs/
.vscode/
.idea/
*.suo
*.user
Thumbs.db
.DS_Store

# Logs
*.log
npm-debug.log*

# Runtime
*.pid
*.seed
*.pid.lock

# Coverage
coverage/
.nyc_output/
```

### `.gitattributes`

File `.gitattributes` kiểm soát cách Git xử lý file trên các hệ điều hành khác nhau.

```gitattributes
# Normalize line endings
* text=auto eol=lf

# Binary files
*.png binary
*.jpg binary
*.gif binary
*.ico binary
*.pdf binary
*.zip binary
*.tar.gz binary

# Scripts
*.sh text eol=lf
*.bat text eol=crlf
*.ps1 text eol=crlf
```

### `.dockerignore`

File `.dockerignore` giúp Docker bỏ qua file không cần thiết khi build image.

```dockerignore
.git
.gitignore
.gitattributes
node_modules/
npm-debug.log
.vs/
.vscode/
.idea/
*.md
coverage/
.env
.env.*
```

---

## 2. Workflow cơ bản

### Quy trình 4 bước hàng ngày

```bash
# Bước 1: Kiểm tra trạng thái
git status

# Bước 2: Thêm file vào staging
git add <file>          # Thêm một file
git add .               # Thêm tất cả file (cẩn thận!)
git add -p              # Thêm từng phần (interactive)

# Bước 3: Commit với message
git commit -m "type(scope): message"

# Bước 4: Đẩy lên remote
git push origin <branch>
```

### Quy trình đầy đủ

```bash
# Pull mới nhất trước khi làm việc
git checkout main
git pull origin main

# Tạo branch mới
git checkout -b feature/ten-tinh-nang

# Làm việc và commit nhiều lần
git add .
git commit -m "feat: thêm chức năng A"
git commit -m "fix: sửa lỗi B"

# Push và tạo Pull Request
git push origin feature/ten-tinh-nang
```

---

## 3. Kiểm tra file nào đang bị ignore

```bash
# Liệt kê tất cả file đang bị ignore
git status --ignored

# Kiểm tra một file cụ thể có bị ignore không
git check-ignore -v path/to/file

# Xem pattern nào đang ignore file
git check-ignore -v node_modules/package.json

# Liệt kê tất cả file ignored (chỉ tên file)
git ls-files --others --ignored --exclude-standard

# Xem tất cả file đang được track
git ls-files
```

---

## 4. Dọn dẹp file không cần thiết

### Xóa file khỏi track nhưng giữ trên ổ đĩa

```bash
git rm --cached <file>
git rm --cached -r <folder>
```

### Xóa file khỏi cả track lẫn ổ đĩa

```bash
git rm <file>
git rm -r <folder>
```

### Dọn dẹp file không cần thiết khỏi lịch sử

```bash
# Xóa file nhạy cảm khỏi toàn bộ lịch sử (dùng BFG hoặc filter-branch)
# CẢNH BÁO: Chỉ làm khi thực sự cần, sẽ rewrite history

# Cách 1: Dùng git filter-branch
git filter-branch --force --index-filter \
  "git rm --cached --ignore-unmatch path/to/file" \
  --prune-empty --tag-name-filter cat -- --all

# Cách 2: Dùng BFG (khuyến nghị)
java -jar bfg.jar --delete-files file.env
```

### Dọn dẹp local branches đã merged

```bash
# Xóa local branch đã merge vào main
git branch --merged main | grep -v "main" | xargs -n 1 git branch -d

# Xóa remote branch đã merged (cẩn thận!)
git branch -r --merged main | grep -v "main" | sed 's/origin\///' | xargs -n 1 git push origin --delete
```

### Dọn dẹp git history (squash)

```bash
# Squash N commit gần nhất
git rebase -i HEAD~N
```

---

## 5. Các file KHÔNG NÊN commit

### Tuyệt đối không commit

| Loại file | Ví dụ | Lý do |
|-----------|-------|-------|
| Biến môi trường | `.env`, `.env.local` | Chứa secret key, database password |
| Dependency lock | `node_modules/`, `vendor/` | Có thể tái tạo từ package.json |
| Build output | `dist/`, `build/`, `out/` | Có thể build lại |
| File log | `*.log`, `npm-debug.log*` | Quá lớn, không cần thiết |
| IDE config | `.vs/`, `.vscode/`, `.idea/` | Cá nhân, không liên quan dự án |
| OS files | `Thumbs.db`, `.DS_Store` | Hệ điều hành tự sinh |
| Binary files lớn | `*.exe`, `*.dll`, `*.so` | Làm chậm repository |
| File backup | `*.bak`, `*.swp`, `*~` | File tạm |
| Certificate/Key | `*.pem`, `*.key`, `*.cert` | Bảo mật |

### File nhạy cảm cần cẩn trọng

- `appsettings.*.json` — nếu chứa connection string thật
- `docker-compose.override.yml` — nếu chứa mật khẩu
- `web.config` — nếu chứa thông tin nhạy cảm

---

## 6. Bảo vệ thông tin nhạy cảm

### Phát hiện secret trong repository

```bash
# Dùng git-secrets (AWS)
git secrets --scan

# Dùng truffleHog
trufflehog --regex --entropy=True https://github.com/user/repo

# Dùng Gitleaks
gitleaks detect -v
```

### Xóa secret đã commit (khẩn cấp)

```bash
# 1. Thêm file vào .gitignore
echo ".env" >> .gitignore
git add .gitignore
git commit -m "chore: thêm .env vào gitignore"

# 2. Xóa file khỏi Git cache
git rm --cached .env

# 3. Xóa khỏi toàn bộ lịch sử
git filter-branch --force --index-filter \
  "git rm --cached --ignore-unmatch .env" \
  --prune-empty --tag-name-filter cat -- --all

# 4. Force push (cẩn thận!)
git push origin --force --all
git push origin --force --tags
```

### Ngăn chặn ngay từ đầu

```bash
# Bật git-secrets hooks
git secrets --install
git secrets --register-aws

# Hoặc dùng pre-commit hooks
# File .pre-commit-config.yaml
repos:
  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.4.0
    hooks:
      - id: detect-private-key
      - id: check-added-large-files
  - repo: https://github.com/awslabs/git-secrets
    rev: master
    hooks:
      - id: git-secrets
```

---

## 7. Branching strategy

### Cấu trúc branch

```
main
├── develop
│   ├── feature/dang-nhap
│   ├── feature/thanh-toan
│   ├── bugfix/sua-loi-login
│   ├── refactor/toi-uu-api
│   └── docs/update-readme
├── hotfix/crash-production
└── release/v1.2.0
```

### Quy tắc đặt tên

| Loại branch | Pattern | Ví dụ |
|-------------|---------|-------|
| Feature | `feature/<tên-ngắn-gọn>` | `feature/dang-nhap-google` |
| Bugfix | `bugfix/<tên-lỗi>` | `bugfix/null-reference-user` |
| Hotfix | `hotfix/<mô-tả>` | `hotfix/crash-checkout-page` |
| Refactor | `refactor/<phạm-vi>` | `refactor/api-response-format` |
| Docs | `docs/<nội-dung>` | `docs/api-endpoints` |
| Release | `release/v<version>` | `release/v1.2.0` |
| Chore | `chore/<công-việc>` | `chore/update-dependencies` |

### Quy trình làm việc

```bash
# 1. Luôn bắt đầu từ develop (hoặc main nếu không có develop)
git checkout develop
git pull origin develop

# 2. Tạo feature branch
git checkout -b feature/ten-tinh-nang

# 3. Làm việc, commit thường xuyên
git add .
git commit -m "feat: thêm chức năng X"

# 4. Sync với develop thường xuyên
git fetch origin
git rebase origin/develop
# Hoặc: git merge origin/develop

# 5. Push và tạo Pull Request
git push origin feature/ten-tinh-nang
```

---

## 8. Commit message convention

### Conventional Commits

```
<type>(<scope>): <description>

[body]

[footer(s)]
```

### Các type phổ biến

| Type | Ý nghĩa | Ví dụ |
|------|---------|-------|
| `feat` | Thêm tính năng mới | `feat(auth): thêm đăng nhập Google` |
| `fix` | Sửa lỗi | `fix(api): sửa null reference khi get user` |
| `docs` | Thay đổi tài liệu | `docs: cập nhật API docs` |
| `style` | Format code, không thay đổi logic | `style: format code theo ESLint` |
| `refactor` | Tái cấu trúc code | `refactor: tách UserService thành interface` |
| `test` | Thêm/sửa test | `test: thêm unit test cho AuthController` |
| `chore` | Công việc bảo trì | `chore: cập nhật dependencies` |
| `perf` | Cải thiện hiệu năng | `perf: tối ưu query database` |
| `ci` | CI/CD config | `ci: thêm GitHub Actions workflow` |
| `build` | Build system | `build: cập nhật Dockerfile` |
| `revert` | Revert commit trước | `revert: hoàn tác commit abc123` |

### Ví dụ commit message

```
feat(order): thêm API tạo đơn hàng mới

- Thêm POST /api/orders
- Validate dữ liệu đầu vào
- Gửi email xác nhận sau khi tạo thành công

Closes #123
```

```
fix(auth): sửa lỗi token hết hạn không refresh

- Kiểm tra expiry time trước khi gọi API
- Tự động refresh token khi còn 5 phút

Hotfix: S-456
```

```
refactor(core): tách BusinessService thành interface và implementation

ISSUES: PROJ-789
```

---

## 9. Sync với remote repository & xử lý conflicts

### Fetch, Pull, Rebase

```bash
# Cập nhật thông tin remote
git fetch origin

# Pull với rebase (giữ lịch sử sạch)
git pull --rebase origin develop

# Pull thông thường
git pull origin develop
```

### Cấu hình pull mặc định với rebase

```bash
git config --global pull.rebase true
git config --global rebase.autoStash true
```

### Xử lý conflict

```bash
# Khi có conflict, Git sẽ báo file bị conflict
# Mở file và tìm:
<<<<<<< HEAD
// Code hiện tại
=======
// Code từ branch khác
>>>>>>> feature/xxx

# Sau khi sửa xong:
git add <file-da-sua>
git rebase --continue
# hoặc
git merge --continue
```

### Các lệnh conflict hữu ích

```bash
# Xem danh sách file conflict
git diff --name-only --diff-filter=U

# Xem nội dung conflict
git diff

# Hủy rebase đang thực hiện
git rebase --abort

# Hủy merge đang thực hiện
git merge --abort

# Dùng công cụ merge (cần cấu hình)
git mergetool
```

---

## 10. Các lệnh hữu ích

### Git log

```bash
# Log cơ bản
git log

# Log một dòng
git log --oneline

# Log đồ họa
git log --oneline --graph --all --decorate

# Log chi tiết
git log --oneline --graph --all --decorate --author="ten"

# Log với date range
git log --since="2024-01-01" --until="2024-12-31"

# Tìm commit theo message
git log --grep="fix:"
git log --oneline --grep="feat:" --all

# Xem file nào thay đổi trong commit
git log --name-status

# Xem thống kê thay đổi
git log --stat

# Log với format tùy chỉnh
git log --pretty=format:"%h - %an, %ar : %s"
```

### Undo các thao tác

```bash
# Undo unstaged changes (phục hồi file về trạng thái staged)
git checkout -- <file>
git restore <file>              # Git 2.23+

# Unstage file (giữ nguyên nội dung)
git reset HEAD <file>
git restore --staged <file>     # Git 2.23+

# Undo commit gần nhất (giữ changes trong working directory)
git reset --soft HEAD~1

# Undo commit gần nhất (xóa changes luôn)
git reset --hard HEAD~1

# Undo commit và tạo commit mới để đảo ngược
git revert HEAD
git revert <commit-hash>
```

### Stash

```bash
# Lưu tạm thay đổi
git stash
git stash push -m "message"

# List stash
git stash list

# Áp dụng stash gần nhất
git stash pop          # Áp dụng và xóa khỏi stash
git stash apply        # Áp dụng nhưng giữ lại stash

# Áp dụng stash cụ thể
git stash apply stash@{2}

# Xóa stash
git stash drop stash@{0}
git stash clear        # Xóa tất cả

# Stash bao gồm untracked files
git stash -u
git stash --include-untracked
```

### Cherry-pick và các lệnh hữu ích khác

```bash
# Lấy một commit cụ thể từ branch khác
git cherry-pick <commit-hash>

# So sánh hai branch
git diff main..develop

# Xem branch nào đã merge
git branch --merged
git branch --no-merged

# Đánh tag
git tag v1.0.0
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# Bisect (tìm commit gây lỗi)
git bisect start
git bisect bad          # Commit hiện tại bị lỗi
git bisect good v1.0.0  # Commit này chạy tốt
# Git sẽ checkout commit ở giữa, kiểm tra và chạy:
git bisect good         # hoặc git bisect bad
git bisect reset
```

---

## 11. Kiểm tra repository health

### Kiểm tra dung lượng

```bash
# Dung lượng repository
git count-objects -vH

# File lớn nhất trong repository
git rev-list --objects --all | \
  git cat-file --batch-check='%(objecttype) %(objectname) %(objectsize) %(rest)' | \
  sort -t' ' -k3 -n -r | head -10

# Top 10 file lớn nhất
git rev-list --objects --all | git cat-file \
  --batch-check='%(objecttype) %(objectname) %(objectsize) %(rest)' | \
  sed -n 's/^blob //p' | sort --numeric-sort --key=2 | \
  tail -10 | cut -c 1-12,41- | $(command -v gnumfmt || echo numfmt) --field=2 --to=iec-i --suffix=B --padding=7 --round=nearest
```

### Kiểm tra tính toàn vẹn

```bash
# Kiểm tra objects có bị corrupt không
git fsck

# Kiểm tra chi tiết
git fsck --full

# Dọn dẹp
git gc --auto
git prune
```

### Phân tích và tối ưu

```bash
# Xem thống kê contributor
git shortlog -sn

# Xem số lượng commit theo ngày
git log --date=short --format="%ad" | sort | uniq -c

# Dọn dẹp repository
git gc --aggressive
git repack -a -d --depth=250 --window=250
```

---

## 12. Best practices

### Hàng ngày

1. **Pull trước khi push**: Luôn `git pull --rebase` trước khi push.
2. **Commit thường xuyên**: Commit nhỏ, mỗi commit một nhiệm vụ.
3. **Viết commit message tốt**: Tuân thủ Conventional Commits.
4. **Kiểm tra git status**: Trước khi commit, kiểm tra file nào đang thay đổi.
5. **Đừng commit file build**: Thêm `dist/`, `build/` vào `.gitignore`.

### Branch management

6. **Không commit trực tiếp lên main/develop**: Luôn dùng Pull Request.
7. **Xóa branch sau khi merge**: Giữ repository sạch sẽ.
8. **Đặt tên branch có ý nghĩa**: `feature/thanh-toan-vnpay` thay vì `branch-1`.
9. **Rebase thay vì merge**: Giữ lịch sử tuyến tính.
10. **Giữ branch ngắn**: Feature branch không nên sống quá vài ngày.

### Code & Security

11. **Không commit secret**: Dùng `.env` file, không commit thật.
12. **Không commit file lớn**: File > 50MB nên dùng Git LFS.
13. **Dùng .gitignore từ đầu**: Tránh commit file không mong muốn.
14. **Review Pull Request**: Luôn có ít nhất 1 người review.
15. **Kiểm tra diff trước commit**: `git diff` để xem chính xác thay đổi.

### Git config nên thiết lập

```bash
# Tên và email
git config --global user.name "Tên Của Bạn"
git config --global user.email "email@example.com"

# Pull với rebase
git config --global pull.rebase true
git config --global rebase.autoStash true

# Mặc định branch là main
git config --global init.defaultBranch main

# Hiển thị màu
git config --global color.ui auto

# Editor
git config --global core.editor "code --wait"
```

---

## 13. Troubleshooting

### Lỗi thường gặp và cách xử lý

#### "Please commit your changes or stash them before you merge/rebase"

```bash
# Giải pháp 1: Stash changes
git stash
git pull --rebase
git stash pop

# Giải pháp 2: Commit tạm
git add .
git commit -m "temp: tạm thời commit"
git pull --rebase
```

#### "Your branch is ahead of 'origin/main' by X commits"

```bash
# Kiểm tra
git status

# Push lên remote
git push origin main
```

#### "Cannot pull with rebase: Your index contains uncommitted changes"

```bash
git stash
git pull --rebase
git stash pop
```

#### "Merge conflict" khi rebase

```bash
# Xem file conflict
git status

# Sau khi sửa conflict
git add <file>
git rebase --continue

# Nếu muốn hủy rebase
git rebase --abort
```

#### "Detached HEAD" state

```bash
# Tạo branch từ commit hiện tại
git checkout -b temp-branch

# Hoặc quay lại branch cũ
git checkout main
```

#### Lỡ commit nhầm file, muốn sửa

```bash
# Sửa commit message
git commit --amend -m "message mới"

# Thêm file vào commit trước
git add <file-thieu>
git commit --amend --no-edit

# Xóa file khỏi commit trước
git reset --soft HEAD~1
git reset HEAD <file-khong-muon>
git commit -m "message đúng"
```

#### Lỡ push nhầm, muốn revert

```bash
# Cách an toàn: revert (không xóa lịch sử)
git revert HEAD
git push origin main

# Cách nguy hiểm: reset và force push
git reset --hard HEAD~1
git push origin main --force   # CẢNH BÁO!
```

#### "Permission denied (publickey)"

```bash
# Kiểm tra SSH key
ssh -T git@github.com

# Tạo SSH key mới
ssh-keygen -t ed25519 -C "email@example.com"
# Thêm key vào ssh-agent
eval "$(ssh-agent -s)"
ssh-add ~/.ssh/id_ed25519
# Thêm public key vào GitHub/GitLab
```

#### "Failed to push some refs"

```bash
# Nguyên nhân: remote có commit mới hơn
git fetch origin
git rebase origin/main
git push origin main
```

#### File quá lớn không push được

```bash
# Tìm file lớn
git rev-list --objects --all | git cat-file \
  --batch-check='%(objecttype) %(objectname) %(objectsize) %(rest)' | \
  awk '/^blob/ {print $3, $4}' | sort -rn | head -5

# Xóa file lớn khỏi lịch sử
git filter-branch --tree-filter 'rm -f path/to/large/file' HEAD
git push origin --force
```

---

## 14. Git aliases

### Aliases hữu ích — thêm vào `~/.gitconfig`

```ini
[alias]
  # Trạng thái
  s = status
  st = status -sb
  ss = status --ignored

  # Log
  l = log --oneline --graph --all --decorate
  ll = log --oneline --graph --all --decorate -20
  lg = log --oneline --graph --all --decorate --date=relative
  lga = log --oneline --graph --all --decorate --simplify-by-decoration
  lp = log --oneline --graph --decorate -20
  ls = log --name-status
  lf = log --all --full-history --oneline -- "*.cs"

  # Diff
  d = diff
  dc = diff --cached
  dw = diff --word-diff

  # Commit
  c = commit
  ca = commit --amend
  cane = commit --amend --no-edit
  cm = commit -m
  co = checkout
  cob = checkout -b

  # Branch
  b = branch
  ba = branch -a
  bd = branch -d
  bdd = branch -D
  bm = branch --merged
  bnm = branch --no-merged

  # Push/Pull
  p = push
  pf = push --force-with-lease
  po = push origin
  pu = pull
  pur = pull --rebase

  # Stash
  sa = stash
  sl = stash list
  sp = stash pop
  sa = stash apply
  ss = stash save
  sd = stash drop

  # Reset
  rh = reset HEAD
  rh1 = reset HEAD~1
  rhard = reset --hard HEAD
  rsoft = reset --soft HEAD~1

  # Other
  unstage = reset HEAD --
  undo = reset --soft HEAD~1
  amend = commit --amend
  who = shortlog -sn
  br = branch -a --sort=-committerdate
  cleanup = !git branch --merged | grep -v \"*\" | xargs -n 1 git branch -d
  tagl = tag -l --sort=-v:refname
  f = fetch --all --prune
  undo-file = checkout --
  ignore = "!git ls-files --others --ignored --exclude-standard"
  contributors = shortlog -sn --all
  changelog = log --oneline --no-merges --format='* %s (%h)'
```

### Cài đặt aliases qua command line

```bash
# Hoặc thêm từng alias qua command line
git config --global alias.s status
git config --global alias.l "log --oneline --graph --all --decorate"
git config --global alias.lg "log --oneline --graph --all --decorate --date=relative"
git config --global alias.d diff
git config --global alias.dc "diff --cached"
git config --global alias.co checkout
git config --global alias.cob "checkout -b"
git config --global alias.cm "commit -m"
git config --global alias.ca "commit --amend"
git config --global alias.p push
git config --global alias.pf "push --force-with-lease"
git config --global alias.pur "pull --rebase"
git config --global alias.sa stash
git config --global alias.sl "stash list"
git config --global alias.sp "stash pop"
git config --global alias.unstage "reset HEAD --"
git config --global alias.undo "reset --soft HEAD~1"
```

### Cách sử dụng aliases

```bash
git s          # git status
git l          # git log đồ họa
git cob ten    # git checkout -b ten
git pur        # git pull --rebase
git pf         # git push --force-with-lease
git unstage    # git reset HEAD --
git undo       # git reset --soft HEAD~1
```

---

## Phụ lục: Cheatsheet nhanh

```bash
# Khởi tạo
git init                                # Tạo repo mới
git clone <url>                         # Clone repo

# Thay đổi cơ bản
git status                              # Kiểm tra trạng thái
git add <file>                          # Stage file
git commit -m "message"                 # Commit
git push origin <branch>                # Push

# Branch
git branch <name>                       # Tạo branch
git checkout <name>                     # Chuyển branch
git checkout -b <name>                  # Tạo + chuyển
git merge <branch>                      # Merge
git branch -d <name>                    # Xóa branch

# Cập nhật
git pull --rebase                       # Pull với rebase
git fetch                               # Fetch remote

# Undo
git restore <file>                      # Restore file
git reset HEAD~1                        # Undo commit
git revert HEAD                         # Revert commit

# Xem
git log --oneline --graph --all         # Xem lịch sử
git diff                                # Xem thay đổi
git blame <file>                        # Xem ai sửa dòng nào
```

---

> **Lưu ý**: Hãy luôn đảm bảo `.gitignore` được cập nhật đầy đủ trước khi commit lần đầu tiên. Một khi file nhạy cảm đã lên remote, việc xóa bỏ hoàn toàn khỏi lịch sử là rất khó khăn.
