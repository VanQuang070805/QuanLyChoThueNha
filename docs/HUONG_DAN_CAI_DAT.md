# Hướng dẫn Cài đặt & Chạy Dự án

## Yêu cầu phần mềm

| Phần mềm | Phiên bản | Ghi chú |
|---|---|---|
| **Visual Studio** | 2019 hoặc 2022 | Workload: **.NET desktop development** |
| **SQL Server** | 2016+ | Express hoặc Developer (miễn phí) |
| **SSMS** | 19+ | SQL Server Management Studio |
| **.NET Framework** | **4.8** | Đã cài sẵn trong Windows 10/11 |
| **NuGet** | Tích hợp VS | Tự tải package khi mở solution |

---

## Bước 1 — Clone / Giải nén dự án

```
git clone <url-repo>
```
hoặc giải nén file `QuanLyChoThueNha.zip`.

---

## Bước 2 — Tạo CSDL

Mở SSMS, kết nối SQL Server, chạy **lần lượt** 3 file:

```
docs/DATABASE/01_create_database.sql   ← Tạo DB + 19 bảng
docs/DATABASE/02_seed_data.sql          ← Dữ liệu mẫu
docs/DATABASE/03_stored_procedures.sql  ← Stored procedures
```

---

## Bước 3 — Cấu hình chuỗi kết nối

Mở `src/QuanLyChoThueNha.GUI/App.config`, sửa dòng:

```xml
<add name="QuanLyChoThueNhaContext"
     connectionString="Server=.\SQLEXPRESS;Database=QuanLyChoThueNha;Integrated Security=True;..."
```

Thay `.\SQLEXPRESS` bằng tên SQL Server trên máy bạn:
- `(localdb)\MSSQLLocalDB` — nếu dùng LocalDB
- `.\SQLEXPRESS` — SQL Server Express mặc định
- `localhost` hoặc tên máy — SQL Server Developer

---

## Bước 4 — Mở và Restore NuGet

1. Mở `QuanLyChoThueNha.sln` bằng Visual Studio
2. Visual Studio tự tải NuGet packages (cần internet lần đầu)
3. Hoặc: **Tools → NuGet Package Manager → Package Manager Console** → `Update-Package -reinstall`

**Packages cần thiết:**

| Package | Project | Mục đích |
|---|---|---|
| `EntityFramework 6.4.4` | DAL | ORM kết nối SQL Server |
| `BCrypt.Net-Next 4.0.3` | BLL | Băm mật khẩu bảo mật |
| `MaterialSkin.2 2.3.1` | GUI | Giao diện Material Design |
| `ClosedXML 0.102.2` | GUI | Xuất báo cáo Excel |

---

## Bước 5 — Chạy

1. Đặt `QuanLyChoThueNha.GUI` là **Startup Project** (chuột phải → Set as Startup Project)
2. Nhấn **F5**
3. Đăng nhập: **admin / Admin@123**

---

## Tài khoản mặc định

| Tài khoản | Mật khẩu | Vai trò |
|---|---|---|
| `admin` | `Admin@123` | Admin — toàn quyền |
| `nhanvien` | `Admin@123` | Nhân viên |
| `khach01` | `Admin@123` | Khách thuê |

---

## Cấu trúc thư mục

```
QuanLyChoThueNha/
├── QuanLyChoThueNha.sln
├── docs/
│   ├── DATABASE/
│   │   ├── 01_create_database.sql
│   │   ├── 02_seed_data.sql
│   │   └── 03_stored_procedures.sql
│   ├── HUONG_DAN_CAI_DAT.md       ← File này
│   ├── THU_VIEN_VA_PACKAGE.md
│   └── KIEN_TRUC.md
└── src/
    ├── QuanLyChoThueNha.Model/    ← 19 Entity
    │   └── Entities/
    ├── QuanLyChoThueNha.DAL/      ← EF6, Repository, UnitOfWork
    │   ├── AppDbContext.cs
    │   ├── Interfaces/
    │   └── Repositories/
    ├── QuanLyChoThueNha.BLL/      ← Services, Helpers, SessionContext
    │   ├── Helpers/
    │   └── Services/
    └── QuanLyChoThueNha.GUI/      ← WinForms + MaterialSkin
        ├── Forms/
        │   ├── Auth/              ← TV1
        │   ├── TaiSan/            ← TV2
        │   ├── HopDong/           ← TV3
        │   ├── TraNha/            ← TV4
        │   └── BaoCao/            ← TV5
        └── Program.cs
```

---

## Lỗi thường gặp

| Lỗi | Nguyên nhân | Giải pháp |
|---|---|---|
| `Cannot open database` | Sai chuỗi kết nối | Kiểm tra lại App.config |
| `BCrypt not found` | NuGet chưa restore | Restore NuGet packages |
| `MaterialSkin missing` | NuGet chưa restore | Restore NuGet packages |
| Login sai mật khẩu | Hash không khớp | Chạy lại 02_seed_data.sql |
