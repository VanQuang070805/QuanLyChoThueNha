# SmartApart - Quan Ly Cho Thue Nha

SmartApart la ung dung WinForms quan ly nha tro/can ho cho thue. Du an phuc vu 3 nhom nguoi dung:

- Khach hang: tim tro cong khai, dat truoc, nhan thong tin tai khoan/QR, xem hop dong va hoa don.
- Nhan vien: xu ly dat coc, hop dong, hoa don, tra nha, vi pham va email.
- Admin: quan ly tai san, tai khoan, phan quyen, bao cao va thong ke.

Du an su dung .NET Framework 4.8, WinForms, MaterialSkin, Entity Framework 6, SQL Server, WebView2 va ClosedXML.

## Cau Truc Thu Muc

```text
QuanLyChoThueNha/
|-- docs/
|   |-- DATABASE/                 # Script tao DB, seed data, migration
|   |-- EMAIL_SMARTAPART_SETUP.md # Huong dan cau hinh email
|   |-- HUONG_DAN_CAI_DAT.md      # Huong dan cai dat
|   |-- KIEN_TRUC.md              # Tai lieu kien truc
|   |-- SMOKE_TEST_LUONG_FORM.ps1 # Script smoke test
|   `-- THU_VIEN_VA_PACKAGE.md    # Thu vien va package
|-- packages/                     # NuGet packages theo packages.config
|-- src/
|   |-- QuanLyChoThueNha.Model/   # Entity map voi bang SQL Server
|   |-- QuanLyChoThueNha.DAL/     # DbContext, Repository, UnitOfWork
|   |-- QuanLyChoThueNha.BLL/     # Service nghiep vu, helper, SessionContext
|   `-- QuanLyChoThueNha.GUI/     # WinForms UI
|-- QuanLyChoThueNha.sln
|-- README.md
`-- README_SUA_LOGIC.md
```

## Cac Project Chinh

### `QuanLyChoThueNha.Model`

Chua cac entity chinh:

- Tai khoan va nguoi dung: `TaiKhoan`, `Admin`, `NhanVienQuanLy`, `NhanVienQuyen`, `KhachThue`.
- Tai san: `KhuVuc`, `Toa`, `CanHo`, `LoaiCanHo`, `TienNghi`, `TienNghiCuaCanHo`, `HinhAnhNha`, `GiaDichVu`.
- Hop dong va thanh toan: `PhieuDatTruoc`, `HopDong`, `GiaHanHopDong`, `HoaDonThanhToan`, `LoaiHoaDon`.
- Tra nha va vi pham: `PhieuTraNha`, `PhieuXuLyViPham`.
- Email: `EmailLog`.

### `QuanLyChoThueNha.DAL`

Chua tang truy cap du lieu:

- `AppDbContext.cs`
- `IRepository.cs`, `Repository.cs`
- `IUnitOfWork.cs`, `UnitOfWork.cs`

Connection string co trong:

- `src/QuanLyChoThueNha.DAL/App.config`
- `src/QuanLyChoThueNha.GUI/App.config`

### `QuanLyChoThueNha.BLL`

Chua logic nghiep vu:

- `AuthService`, `TaiKhoanService`, `NhanVienPhanQuyenService`
- `CanHoService`, `ToaService`, `KhuVucService`, `LoaiCanHoService`, `TienNghiService`
- `PhieuDatTruocService`, `HopDongService`, `GiaHanHopDongService`
- `HoaDonThanhToanService`, `PhieuTraNhaService`, `PhieuXuLyViPhamService`
- `BaoCaoService`, `EmailLogService`
- Helper: `PasswordHelper`, `ValidationHelper`, `TextFormatHelper`, `MaGenerator`, `AuditHelper`

### `QuanLyChoThueNha.GUI`

Chua giao dien WinForms:

- `Forms/Auth`: dang nhap, quan ly tai khoan, phan quyen.
- `Forms/KhachHang`: tim tro cong khai, dang ky dat truoc, QR thanh toan, trang khach hang.
- `Forms/NhanVien`: trang cong viec nhan vien va email log.
- `Forms/TaiSan`: khu vuc, toa, loai can ho, can ho, tien nghi, gia dich vu.
- `Forms/HopDong`: khach thue, phieu dat truoc, hop dong, gia han.
- `Forms/TraNha`: hoa don, loai hoa don, phieu tra nha, xu ly vi pham.
- `Forms/BaoCao`: dashboard va bao cao thong ke.
- `Forms/Shared`: form dung chung.
- `Controls`: `RoundedPanel`, `RoundedButton`, `PlaceholderTextBox`.

Entry point cua ung dung:

```text
src/QuanLyChoThueNha.GUI/Program.cs
```

Man hinh dau tien hien tai la cong tim tro cong khai:

```csharp
Application.Run(new Forms.KhachHang.frmTimTroPublic());
```

## Yeu Cau Moi Truong

- Windows
- Visual Studio 2019/2022 hoac moi hon
- .NET Framework 4.8 Developer Pack
- SQL Server hoac SQL Server Express
- WebView2 Runtime
- NuGet packages theo `packages.config`

## Cai Dat Database

Tao database moi bang SQL Server Management Studio theo thu tu:

```text
docs/DATABASE/01_create_database.sql
docs/DATABASE/02_seed_data.sql
docs/DATABASE/03_stored_procedures.sql
docs/DATABASE/05_loai_hoadon_seed.sql
docs/DATABASE/07_test_accounts.sql
```

Neu cap nhat database cu, chay them cac script migration:

```text
docs/DATABASE/04_migration_sua_logic.sql
docs/DATABASE/08_audit_nguoi_thao_tac.sql
docs/DATABASE/09_khuvuc_toado.sql
docs/DATABASE/10_unicode_text_columns.sql
docs/DATABASE/11_phieu_dat_truoc_coc_email_log.sql
docs/DATABASE/12_repair_unicode_seed_data.sql
docs/DATABASE/13_nhanvien_phanquyen.sql
```

## Tai Khoan Test

Sau khi chay `docs/DATABASE/07_test_accounts.sql`:

```text
Admin:      admin_test  / Admin@123
Nhan vien:  nv_test     / Admin@123
Khach:      khach_test  / Admin@123
Khach demo: khach_demo  / Admin@123
```

## Build Va Chay

Mo solution:

```text
QuanLyChoThueNha.sln
```

Trong Visual Studio:

1. Restore NuGet packages neu duoc hoi.
2. Dat `QuanLyChoThueNha.GUI` lam Startup Project.
3. Kiem tra connection string trong `App.config`.
4. Build va Run.

Build bang terminal:

```powershell
dotnet build .\QuanLyChoThueNha.sln
```

Neu build loi do file DLL/EXE dang bi khoa, hay dong ung dung dang debug trong Visual Studio roi build lai.

## Luong Nghiep Vu Chinh

### Khach Hang

1. Mo man hinh `Tim tro` khong can dang nhap.
2. Loc theo khu vuc, toa, loai phong, gia.
3. Dat truoc can ho.
4. He thong tao tai khoan khach, phieu dat truoc va QR dat coc.
5. Khach dang nhap de xem phieu dat, hop dong, hoa don va QR thanh toan.

### Nhan Vien

1. Dang nhap cong noi bo.
2. Xac nhan coc.
3. Ky hop dong.
4. Lap hoa don va ghi nhan thanh toan.
5. Lap phieu tra nha, xu ly vi pham.

### Admin

1. Quan ly khu vuc, toa, can ho, loai can ho, tien nghi, gia dich vu.
2. Quan ly tai khoan va tai khoan khach.
3. Phan quyen nhan vien.
4. Xem dashboard va bao cao.

## Quy Uoc Trang Thai

Phieu dat truoc:

```text
ChoThanhToanCoc
DaThanhToanCoc
ChoKy
DaKyHD
Huy
HetHan
```

Can ho:

```text
Trong
DaDatCoc
DangThue
BaoTri
```

Luu y: `DaDatCoc` va `DangThue` la trang thai he thong tu dong dong bo tu phieu dat truoc va hop dong, khong nen sua tay.

Hoa don:

```text
ChuaTra
TraThieu
DaTra
QuaHan
```

## Kiem Tra Nhanh

Build:

```powershell
dotnet build .\QuanLyChoThueNha.sln
```

Smoke test:

```powershell
powershell -ExecutionPolicy Bypass -File docs/SMOKE_TEST_LUONG_FORM.ps1
```

Nen kiem tra thu cong cac luong:

- Khach tim tro, xoa bo loc gia, lam moi danh sach.
- Khach dat truoc va xem thong tin tai khoan/QR.
- Admin huy coc, can ho quay ve `Trong`.
- Quan ly can ho khong sua tay `DaDatCoc`/`DangThue`.
- Lap hoa don trong thoi han hop dong.
- Lap phieu tra nha.
- Dang xuat quay lai dung cong dang nhap.
- Bao cao load du lieu khong bi loi.

## Luu Y Bao Mat

- Khong commit mat khau SMTP, token, QR ngan hang ca nhan hoac file rieng tu.
- Thong tin email/thanh toan nam trong `src/QuanLyChoThueNha.GUI/App.config`.
- Neu can cau hinh SMTP an toan, uu tien bien moi truong:

```powershell
$env:SMARTAPART_SMTP_PASSWORD = "app-password"
```
