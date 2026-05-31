# SmartApart - Quản lý cho thuê nhà

SmartApart là ứng dụng WinForms quản lý nhà trọ/căn hộ cho thuê, phục vụ 3 nhóm người dùng chính:

- Khách hàng: tìm phòng công khai, đặt trước, nhận QR/email, xem phiếu đặt, hợp đồng và hóa đơn.
- Nhân viên quản lý: xử lý đặt cọc, ký hợp đồng, lập hóa đơn, theo dõi email, trả nhà và vi phạm.
- Admin/bên cho thuê: quản trị tài khoản, phân quyền nhân viên, quản lý danh mục tài sản và xem báo cáo.

Dự án dùng .NET Framework 4.8, WinForms, MaterialSkin, Entity Framework 6, SQL Server, WebView2, Leaflet/OpenStreetMap và ClosedXML.

## Trạng thái kiểm tra hiện tại

- Build solution thành công bằng `dotnet build QuanLyChoThueNha.sln`.
- Database scripts hiện có đủ cho tạo mới và cập nhật DB cũ.
- Đã rà các migration `04`, `08`, `09`, `10`, `11`, `13`; chưa phát hiện lỗi logic/idempotency nghiêm trọng. `01_create_database.sql` đã được chỉnh lại phần mô tả số bảng để khớp schema hiện tại.
- Đã loại khỏi repo thư mục `private/` vì đây là nơi chứa file local/riêng tư như QR cá nhân, video hoặc tài sản không nên commit.
- Không phát hiện file code lỗi biên dịch tại thời điểm rà soát.

## Cấu trúc thư mục

```text
QuanLyChoThueNha/
├── docs/
│   ├── DATABASE/                 # Script tạo DB, seed, migration, sửa unicode, audit, email log
│   ├── EMAIL_SMARTAPART_SETUP.md # Hướng dẫn cấu hình SMTP/email
│   ├── HUONG_DAN_CAI_DAT.md      # Hướng dẫn cài đặt môi trường
│   ├── KE_HOACH_LUONG_FORM.md    # Kế hoạch luồng form nghiệp vụ
│   ├── KIEN_TRUC.md              # Tài liệu kiến trúc
│   ├── SMOKE_TEST_LUONG_FORM.ps1 # Smoke test luồng chính
│   └── THU_VIEN_VA_PACKAGE.md    # Danh sách thư viện/package
├── packages/                     # NuGet packages cục bộ theo packages.config
├── src/
│   ├── QuanLyChoThueNha.Model/   # Entity ánh xạ bảng SQL Server
│   ├── QuanLyChoThueNha.DAL/     # DbContext, Repository, UnitOfWork
│   ├── QuanLyChoThueNha.BLL/     # Service nghiệp vụ, helper, SessionContext
│   └── QuanLyChoThueNha.GUI/     # WinForms UI
├── QuanLyChoThueNha.sln
├── README.md
└── README_SUA_LOGIC.md
```

## Module chính

### Model

Chứa các entity: `TaiKhoan`, `Admin`, `NhanVienQuanLy`, `NhanVienQuyen`, `KhachThue`, `KhuVuc`, `Toa`, `CanHo`, `PhieuDatTruoc`, `HopDong`, `HoaDonThanhToan`, `EmailLog`, `PhieuTraNha`, `PhieuXuLyViPham` và các bảng danh mục.

### DAL

Chứa `AppDbContext`, `IRepository`, `Repository`, `IUnitOfWork`, `UnitOfWork`.

### BLL

Chứa các service nghiệp vụ:

- Đăng nhập/tài khoản: `AuthService`, `TaiKhoanService`.
- Tài sản: `KhuVucService`, `ToaService`, `LoaiCanHoService`, `CanHoService`, `TienNghiService`.
- Đặt trước/hợp đồng/thanh toán: `PhieuDatTruocService`, `HopDongService`, `HoaDonThanhToanService`, `GiaHanHopDongService`.
- Báo cáo/email/phân quyền: `BaoCaoService`, `EmailLogService`, `NhanVienPhanQuyenService`.

### GUI

Các nhóm form chính:

- `Forms/Auth`: đăng nhập, quản lý tài khoản, phân quyền nhân viên.
- `Forms/KhachHang`: tìm trọ công khai, trang khách hàng, đặt trước, QR thanh toán.
- `Forms/NhanVien`: trang công việc nhân viên, lịch sử email.
- `Forms/TaiSan`: khu vực, tòa nhà, loại căn hộ, căn hộ, tiện nghi, giá dịch vụ.
- `Forms/HopDong`: khách thuê, phiếu đặt trước, hợp đồng, gia hạn.
- `Forms/TraNha`: hóa đơn, loại hóa đơn, trả nhà, xử lý vi phạm.
- `Forms/BaoCao`: dashboard và báo cáo thống kê.
- `Controls`: control UI dùng chung như `RoundedPanel`, `RoundedButton`, `PlaceholderTextBox`.

## Yêu cầu môi trường

- Windows.
- Visual Studio 2019/2022 hoặc mới hơn.
- .NET Framework 4.8 Developer Pack.
- SQL Server hoặc SQL Server Express.
- WebView2 Runtime nếu dùng màn hình bản đồ.
- NuGet packages theo `packages.config`.

## Cấu hình kết nối database

Connection string nằm trong:

- `src/QuanLyChoThueNha.GUI/App.config`
- `src/QuanLyChoThueNha.DAL/App.config`

Mặc định đang dùng SQL Server Express. Nếu máy dùng instance khác, đổi `Server=.\SQLEXPRESS` theo môi trường thực tế.

## Cài đặt database

### Tạo database mới

Chạy trong SQL Server Management Studio theo thứ tự:

```text
docs/DATABASE/01_create_database.sql
docs/DATABASE/02_seed_data.sql
docs/DATABASE/03_stored_procedures.sql
docs/DATABASE/05_loai_hoadon_seed.sql
docs/DATABASE/07_test_accounts.sql
```

`01_create_database.sql` đã bao gồm schema hiện tại: audit người thao tác, tọa độ khu vực, EmailLog và bảng phân quyền nhân viên. Các migration `08` đến `13` chủ yếu dùng khi cập nhật database cũ.

### Cập nhật database cũ

Nếu đã có DB từ bản trước, chạy lần lượt:

```text
docs/DATABASE/04_migration_sua_logic.sql
docs/DATABASE/08_audit_nguoi_thao_tac.sql
docs/DATABASE/09_khuvuc_toado.sql
docs/DATABASE/10_unicode_text_columns.sql
docs/DATABASE/11_phieu_dat_truoc_coc_email_log.sql
docs/DATABASE/12_repair_unicode_seed_data.sql
docs/DATABASE/13_nhanvien_phanquyen.sql
```

## Tài khoản test

Sau khi chạy `docs/DATABASE/07_test_accounts.sql`, có thể dùng:

```text
Admin:     admin_test / Admin@123
Nhân viên: nv_test    / Admin@123
Khách:     khach_test / Admin@123
Khách demo: khach_demo / Admin@123
```

## Cấu hình email và thanh toán

Các key nằm trong `src/QuanLyChoThueNha.GUI/App.config`:

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

Không nên commit mật khẩu SMTP, token, QR ngân hàng cá nhân hoặc video/tài sản riêng. Nếu cần dùng app password Gmail, ưu tiên biến môi trường:

```powershell
$env:SMARTAPART_SMTP_PASSWORD = "app-password"
```

## Chạy ứng dụng

1. Mở `QuanLyChoThueNha.sln` bằng Visual Studio.
2. Restore NuGet packages nếu được hỏi.
3. Đặt `QuanLyChoThueNha.GUI` làm Startup Project.
4. Kiểm tra connection string trong `App.config`.
5. Build và Run.

Có thể build bằng terminal:

```powershell
dotnet build QuanLyChoThueNha.sln
```

Nếu build lỗi vì file `.exe` đang bị khóa, đóng ứng dụng đang chạy Debug rồi build lại.

## Hướng dẫn chạy từ đầu cho người mới

Thực hiện theo đúng thứ tự dưới đây trên một máy mới:

1. Cài SQL Server hoặc SQL Server Express.
2. Cài Visual Studio với workload `.NET desktop development`.
3. Cài .NET Framework 4.8 Developer Pack.
4. Cài WebView2 Runtime nếu máy chưa có.
5. Mở SQL Server Management Studio và chạy script tạo database:

```text
docs/DATABASE/01_create_database.sql
docs/DATABASE/02_seed_data.sql
docs/DATABASE/03_stored_procedures.sql
docs/DATABASE/05_loai_hoadon_seed.sql
docs/DATABASE/07_test_accounts.sql
```

6. Mở `src/QuanLyChoThueNha.GUI/App.config` và `src/QuanLyChoThueNha.DAL/App.config`.
7. Sửa connection string cho đúng SQL Server trên máy.
8. Mở `QuanLyChoThueNha.sln` trong Visual Studio.
9. Restore NuGet packages nếu Visual Studio yêu cầu.
10. Chọn project `QuanLyChoThueNha.GUI` làm Startup Project.
11. Build solution.
12. Run ứng dụng.

Sau khi chạy được, đăng nhập thử bằng tài khoản test:

```text
admin_test / Admin@123
nv_test    / Admin@123
khach_test / Admin@123
```

Nếu muốn kiểm tra cổng khách trước, mở app và dùng màn hình `Tìm trọ`; khách có thể xem phòng mà chưa cần tài khoản.

## Hướng dẫn sử dụng trong ứng dụng

Ứng dụng có nút `Hướng dẫn tôi` ở góc trên bên trái:

- Tại cổng tìm trọ: hiển thị hướng dẫn cho khách mới, gồm tìm phòng, đặt trước và đăng nhập khách hàng.
- Sau khi Admin đăng nhập: hiển thị hướng dẫn khởi tạo dữ liệu, phân quyền, báo cáo và kiểm tra audit.
- Sau khi Nhân viên đăng nhập: hiển thị hướng dẫn xử lý cọc, ký hợp đồng, lập hóa đơn, trả nhà và vi phạm.
- Sau khi Khách đăng nhập: hiển thị hướng dẫn xem phiếu đặt, hợp đồng, hóa đơn và QR thanh toán.

Mỗi hướng dẫn được hiển thị thành thẻ message riêng. Người dùng chỉ cần đọc thẻ đúng nghiệp vụ đang làm và thao tác theo từng bước từ trên xuống.

## Luồng nghiệp vụ chính

### Khách hàng

1. Mở cổng tìm trọ công khai, không cần đăng nhập.
2. Lọc theo khu vực, bán kính, tòa nhà, loại phòng và khoảng giá.
3. Xem card phòng, trạng thái, giá thuê, tiền cọc và bản đồ vị trí.
4. Khi đặt trước, khách nhập thông tin để hệ thống tạo tài khoản và phiếu đặt.
5. Hệ thống gửi email thông tin tài khoản, phiếu đặt và QR đặt cọc.
6. Sau khi đăng nhập, khách xem được phiếu đặt, hợp đồng và hóa đơn của chính mình.

### Nhân viên quản lý

1. Đăng nhập bằng cổng nội bộ.
2. Xem danh sách việc cần xử lý: phiếu chờ cọc, hợp đồng, hóa đơn, email.
3. Xác nhận đã nhận cọc để phòng chuyển sang trạng thái giữ chỗ.
4. Ký hợp đồng, lập hóa đơn, ghi nhận thanh toán, xử lý trả nhà/vi phạm.
5. Có thể gửi lại email tài khoản hoặc QR cọc khi cần.

### Admin/bên cho thuê

1. Đăng nhập bằng tài khoản Admin.
2. Quản lý danh mục tài sản: khu vực, tòa, loại căn hộ, căn hộ, tiện nghi, giá dịch vụ.
3. Quản lý tài khoản khách hàng và tài khoản nội bộ.
4. Phân quyền nhân viên theo nhóm chức năng.
5. Xem dashboard, báo cáo doanh thu, công nợ, tình trạng căn hộ.
6. Mọi thao tác nghiệp vụ được audit bằng `MaNguoiThaoTac` và `VaiTroNguoiThaoTac`.

## Trạng thái nghiệp vụ

Phiếu đặt trước:

```text
ChoThanhToanCoc -> ChoKy -> DaKyHD
ChoThanhToanCoc -> HetHan
ChoThanhToanCoc/ChoKy -> Huy
```

Căn hộ:

```text
Trong
DaDatCoc
DangThue
BaoTri
```

Hóa đơn:

```text
ChuaTra
TraThieu
DaTra
QuaHan
```

## Kiểm thử nhanh

Build:

```powershell
dotnet build QuanLyChoThueNha.sln
```

Smoke test:

```powershell
powershell -ExecutionPolicy Bypass -File docs/SMOKE_TEST_LUONG_FORM.ps1
```

Kiểm thử thủ công nên đi qua:

- Admin đăng nhập, xem dashboard, báo cáo và quản lý tài khoản.
- Nhân viên đăng nhập, thấy trang công việc và không thấy chức năng Admin nếu không được cấp quyền.
- Khách đăng nhập, chỉ thấy dữ liệu của chính khách đó.
- Thêm khu vực, tòa, căn hộ rồi làm mới cổng tìm trọ.
- Khách đặt phòng, nhận email/QR.
- Nhân viên xác nhận cọc, ký hợp đồng.
- Lập hóa đơn và kiểm tra khách xem được hóa đơn/QR thanh toán.

## Lưu ý bảo mật

- Không commit app password, token SMTP, thông tin ngân hàng cá nhân, QR cá nhân hoặc video riêng.
- Thư mục `private/` đã được đưa vào `.gitignore` và chỉ dùng cho dữ liệu local.
- Nếu thông tin nhạy cảm đã từng được commit, cần đổi app password/token hoặc cập nhật thông tin thanh toán tương ứng.
