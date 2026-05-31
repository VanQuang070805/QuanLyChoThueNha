# SmartApart - Quan Ly Cho Thue Nha

Ung dung WinForms quan ly nha tro/can ho cho thue theo 3 nhom nguoi dung:

- Khach hang: tim tro cong khai, dat truoc phong, xem phieu dat, hop dong va hoa don.
- Nhan vien quan ly: xu ly phieu dat truoc, xac nhan coc, tao khach thue, ky hop dong, lap hoa don, tra nha va vi pham.
- Admin/ben cho thue: quan tri danh muc, tai khoan, bao cao va thuc hien cac nghiep vu can audit.

Du an su dung .NET Framework 4.8, WinForms, MaterialSkin, Entity Framework 6 va SQL Server.

## Tinh nang chinh

- Cong tim tro cong khai khong can dang nhap.
- Hien thi card phong/tro responsive kem hinh anh, thong tin gia, coc, trang thai.
- Loc theo khu vuc, ban kinh, toa nha, loai can ho va khoang gia.
- WebView2 + Leaflet/OpenStreetMap de xem vi tri toa nha.
- Khach tao phieu dat truoc, nhan QR thanh toan coc va email thong tin.
- Luong coc 24h:
  - Tao phieu: `ChoThanhToanCoc`.
  - Nhan vien xac nhan da nhan coc: chuyen `ChoKy`, phong sang `DaDatCoc`.
  - Qua 24h chua xac nhan: phieu sang `HetHan`, phong duoc mo lai.
- EmailLog ghi nhan email gui thanh cong/that bai.
- Menu noi bo phan cap theo nhom chuc nang va phan quyen.
- Quan ly tai khoan khach hang rieng cho Admin.
- Audit nguoi thao tac cho Admin/Nhan vien ma khong pha khoa ngoai nhan vien.

## Cau truc thu muc

```text
QuanLyChoThueNha/
├── docs/
│   ├── DATABASE/                 # Script tao DB, seed, migration, sua unicode, email log
│   ├── EMAIL_SMARTAPART_SETUP.md # Huong dan cau hinh email SMTP
│   ├── HUONG_DAN_CAI_DAT.md      # Huong dan cai dat
│   ├── KE_HOACH_LUONG_FORM.md    # Ke hoach/luong form nghiep vu
│   ├── KIEN_TRUC.md              # Mo ta kien truc
│   ├── SMOKE_TEST_LUONG_FORM.ps1 # Smoke test luong chinh
│   └── THU_VIEN_VA_PACKAGE.md    # Thu vien va package
├── packages/                     # NuGet packages dang duoc project tham chieu
├── private/                      # Du lieu rieng tu/cau hinh local, khong dua vao README
├── src/
│   ├── QuanLyChoThueNha.Model/   # Entity map bang SQL Server
│   ├── QuanLyChoThueNha.DAL/     # DbContext, Repository, UnitOfWork
│   ├── QuanLyChoThueNha.BLL/     # Service nghiep vu, helper, session
│   └── QuanLyChoThueNha.GUI/     # WinForms UI
├── QuanLyChoThueNha.sln
└── README_SUA_LOGIC.md           # Bao cao sua logic cu/chi tiet
```

## Kien truc project

```text
QuanLyChoThueNha.Model
  Entity: Admin, TaiKhoan, KhachThue, KhuVuc, Toa, CanHo, HopDong,
  PhieuDatTruoc, HoaDonThanhToan, PhieuTraNha, PhieuXuLyViPham, EmailLog...

QuanLyChoThueNha.DAL
  AppDbContext, IRepository, Repository, UnitOfWork.

QuanLyChoThueNha.BLL
  Service nghiep vu: AuthService, TaiKhoanService, KhuVucService, ToaService,
  CanHoService, PhieuDatTruocService, HopDongService, HoaDonThanhToanService...

QuanLyChoThueNha.GUI
  Forms:
  - Auth: dang nhap, quan ly tai khoan.
  - KhachHang: tim tro cong khai, trang khach hang, QR thanh toan.
  - NhanVien: trang cong viec nhan vien, lich su email.
  - TaiSan: khu vuc, toa nha, loai can ho, can ho, tien nghi, gia dich vu.
  - HopDong: khach thue, phieu dat truoc, hop dong, gia han.
  - TraNha: hoa don, loai hoa don, phieu tra nha, xu ly vi pham.
  - BaoCao: dashboard, bao cao thong ke.
```

## Yeu cau moi truong

- Windows.
- Visual Studio 2019/2022 hoac moi hon co workload .NET desktop development.
- .NET Framework 4.8 Developer Pack.
- SQL Server hoac SQL Server Express.
- WebView2 Runtime de dung man hinh ban do.
- NuGet restore cho cac package trong `packages.config`.

## Cai dat database

Ket noi mac dinh nam trong:

- `src/QuanLyChoThueNha.GUI/App.config`
- `src/QuanLyChoThueNha.DAL/App.config`

Doi `Server=.\SQLEXPRESS` thanh SQL Server thuc te tren may.

### Tao database moi

Chay lan luot trong SQL Server Management Studio:

```text
docs/DATABASE/01_create_database.sql
docs/DATABASE/02_seed_data.sql
docs/DATABASE/03_stored_procedures.sql
docs/DATABASE/05_loai_hoadon_seed.sql
docs/DATABASE/07_test_accounts.sql
```

Neu can du lieu/cot moi theo ban hien tai, tiep tuc chay:

```text
docs/DATABASE/08_audit_nguoi_thao_tac.sql
docs/DATABASE/09_khuvuc_toado.sql
docs/DATABASE/10_unicode_text_columns.sql
docs/DATABASE/11_phieu_dat_truoc_coc_email_log.sql
docs/DATABASE/12_repair_unicode_seed_data.sql
docs/DATABASE/13_nhanvien_phanquyen.sql
```

### Cap nhat database cu

Chay cac migration idempotent tu `04` den `12` tuy tinh trang DB:

```text
docs/DATABASE/04_migration_sua_logic.sql
docs/DATABASE/08_audit_nguoi_thao_tac.sql
docs/DATABASE/09_khuvuc_toado.sql
docs/DATABASE/10_unicode_text_columns.sql
docs/DATABASE/11_phieu_dat_truoc_coc_email_log.sql
docs/DATABASE/12_repair_unicode_seed_data.sql
docs/DATABASE/13_nhanvien_phanquyen.sql
```

## Cau hinh email va thanh toan

Cac key cau hinh nam trong `src/QuanLyChoThueNha.GUI/App.config`:

```xml
<add key="EmailEnabled" value="true" />
<add key="SmtpHost" value="smtp.gmail.com" />
<add key="SmtpPort" value="587" />
<add key="SmtpEnableSsl" value="true" />
<add key="SmtpUser" value="..." />
<add key="SmtpPassword" value="" />
<add key="PaymentBankCode" value="..." />
<add key="PaymentAccountNo" value="..." />
<add key="PaymentAccountName" value="..." />
<add key="PaymentQrImagePath" value="" />
```

Khuyen nghi khong commit mat khau SMTP. Ung dung uu tien bien moi truong:

```powershell
$env:SMARTAPART_SMTP_PASSWORD = "app-password"
```

## Chay ung dung

1. Mo `QuanLyChoThueNha.sln` bang Visual Studio.
2. Restore NuGet packages neu Visual Studio yeu cau.
3. Dat `QuanLyChoThueNha.GUI` lam Startup Project.
4. Kiem tra connection string trong `App.config`.
5. Build solution.
6. Run.

Co the build bang terminal:

```powershell
dotnet build QuanLyChoThueNha.sln
```

Neu build bao file `.exe` bi khoa, hay dong app dang Debug hoac dung process `QuanLyChoThueNha.GUI.exe` roi build lai.

## Luong su dung chinh

### Khach hang

1. Mo cong tim tro.
2. Tim theo khu vuc, ban kinh, gia, toa, loai phong.
3. Xem card phong va ban do.
4. Chon phong trong de dat truoc.
5. Dang ky thong tin khi lap phieu.
6. Nhan email thong tin tai khoan/QR coc.
7. Dang nhap de xem phieu dat, hop dong va hoa don cua minh.

### Nhan vien quan ly

1. Dang nhap cong noi bo bang vai tro NhanVien.
2. Vao trang cong viec nhan vien.
3. Xem phieu `ChoThanhToanCoc`.
4. Xac nhan da nhan coc.
5. Ky hop dong, lap hoa don, xu ly tra nha/vi pham.
6. Gui lai email/QR neu khach chua nhan.

### Admin/ben cho thue

1. Dang nhap cong noi bo bang vai tro Admin.
2. Quan ly danh muc tai san: khu vuc, toa nha, can ho, tien nghi, gia dich vu.
3. Quan ly tai khoan noi bo va tai khoan khach.
4. Xem dashboard, bao cao thong ke.
5. Thao tac nghiep vu khi can, duoc ghi audit theo `MaNguoiThaoTac` va `VaiTroNguoiThaoTac`.

## Trang thai nghiep vu quan trong

Phieu dat truoc:

```text
ChoThanhToanCoc -> ChoKy -> DaKyHD
ChoThanhToanCoc -> HetHan
ChoThanhToanCoc/ChoKy -> Huy
```

Can ho:

```text
Trong
DaDatCoc
DangThue
BaoTri
```

Hoa don:

```text
ChuaTra
TraThieu
DaTra
QuaHan
```

## Kiem thu nhanh

Chay build:

```powershell
dotnet build QuanLyChoThueNha.sln
```

Chay smoke test:

```powershell
powershell -ExecutionPolicy Bypass -File docs/SMOKE_TEST_LUONG_FORM.ps1
```

Kiem thu thu cong nen di qua cac buoc:

- Admin dang nhap thay dashboard, quan ly tai khoan va bao cao.
- Nhan vien dang nhap thay trang cong viec, khong thay menu quan tri Admin.
- Khach dang nhap thay phieu dat, hop dong, hoa don cua chinh minh.
- Them khu vuc -> toa nha -> can ho, sau do bam Lam moi o cong tim tro de thay du lieu.
- Khach dat phong, nhan QR/email.
- Nhan vien xac nhan coc, phong chuyen sang trang thai giu coc/cho ky.
- Tao hop dong, lap hoa don, khach xem QR thanh toan.

## Luu y bao mat

- Khong commit app password, token SMTP, thong tin ngan hang ca nhan hoac file rieng tu.
- Thu muc `private/` chi nen dung cho cau hinh local.
- Neu da tung commit thong tin nhay cam, can rotate/revoke thong tin do tren nha cung cap.
