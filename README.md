# SmartApart - Quản Lý Cho Thuê Nhà

SmartApart là ứng dụng WinForms quản lý nhà trọ/căn hộ cho thuê. Dự án phục vụ 3 nhóm người dùng:

- **Khách hàng**: Tìm trọ công khai, đặt trước, nhận thông tin tài khoản/QR, xem hợp đồng và hóa đơn.
- **Nhân viên**: Xử lý đặt cọc, hợp đồng, hóa đơn, trả nhà, vi phạm và email.
- **Admin**: Quản lý tài sản, tài khoản, phân quyền, báo cáo và thống kê.

Dự án sử dụng .NET Framework 4.8, WinForms, MaterialSkin, Entity Framework 6, SQL Server, WebView2 và ClosedXML.

---

## 1. Cấu Trúc Thư Mục & Kiến Trúc Dự Án

Mô hình 3 lớp kết hợp Entity Framework 6 (Code-First mapping):
```text
QuanLyChoThueNha/
|-- docs/
|   |-- DATABASE/                 # Script tạo DB, seed data, stored procedures
|   |-- EMAIL_SMARTAPART_SETUP.md # Hướng dẫn cấu hình email
|   |-- HUONG_DAN_CAI_DAT.md      # Hướng dẫn cài đặt dự án
|   |-- KIEN_TRUC.md              # Tài liệu kiến trúc
|   |-- SMOKE_TEST_LUONG_FORM.ps1 # Script chạy smoke test tự động
|   `-- THU_VIEN_VA_PACKAGE.md    # Các thư viện phụ thuộc
|-- packages/                     # NuGet packages
|-- src/
|   |-- QuanLyChoThueNha.Model/   # Entity classes map trực tiếp với các bảng SQL
|   |-- QuanLyChoThueNha.DAL/     # AppDbContext, Repository, UnitOfWork
|   |-- QuanLyChoThueNha.BLL/     # Service nghiệp vụ, helper, SessionContext
|   `-- QuanLyChoThueNha.GUI/     # WinForms UI sử dụng MaterialSkin
|-- QuanLyChoThueNha.sln
|-- README.md
`-- README_SUA_LOGIC.md
```

---

## 2. Chi Tiết Cơ Sở Dữ Liệu (21 Bảng)

Hệ thống lưu trữ dữ liệu trên SQL Server gồm 21 thực thể (bảng):

1. **`TaiKhoan`**: Lưu trữ tài khoản đăng nhập.
   - *Cột chính*: `MaTaiKhoan` (PK), `TenDangNhap`, `MatKhauHash`, `VaiTro` (Admin/NhanVien/KhachThue), `TrangThai` (Active/Locked), `Email`, `NgayTao`.
2. **`Admin`**: Thông tin quản trị viên.
   - *Cột chính*: `MaAdmin` (PK), `TenDangNhap`, `HoTen`.
3. **`NhanVienQuanLy`**: Thông tin chi tiết nhân viên.
   - *Cột chính*: `MaNhanVien` (PK), `HoTen`, `SoDienThoai`, `Email`, `BoPhan`, `MaTaiKhoan` (FK).
4. **`NhanVienQuyen`**: Phân quyền chi tiết của nhân viên.
   - *Cột chính*: `MaNhanVien` (PK/FK), `QuyenToaNha` (bool), `QuyenHopDong` (bool), `QuyenTaiSan` (bool), `QuyenBaoCao` (bool).
5. **`KhachThue`**: Thông tin khách thuê căn hộ.
   - *Cột chính*: `MaKhach` (PK), `HoTen`, `SoCMND`, `SoDienThoai`, `NgaySinh`, `QueQuan`, `TinhTrangLamViec`, `MaTaiKhoan` (FK).
6. **`KhuVuc`**: Khu vực địa lý quản lý tòa nhà.
   - *Cột chính*: `MaKhuVuc` (PK), `TenKhuVuc`, `Quan`, `ThanhPho`, `ViDo`, `KinhDo`, `NgayTao`, `MaNhanVien` (FK).
7. **`Toa`**: Thông tin các tòa nhà thuộc các khu vực.
   - *Cột chính*: `MaToa` (PK), `TenToa`, `DiaChi`, `SoTang`, `MaKhuVuc` (FK), `KinhDo`, `ViDo`, `NgayTao`, `MaNhanVien` (FK).
8. **`CanHo`**: Căn hộ/phòng trọ trong tòa nhà.
   - *Cột chính*: `MaCanHo` (PK), `MaToa` (FK), `MaLoai` (FK), `MaNhanVien` (FK), `DienTich`, `GiaThueNiemYet`, `TienCocNiemYet`, `SoCanHo`, `TangSo`, `TinhTrang` (Trong/DaDatCoc/DangThue/BaoTri), `MoTa`, `NgayTao`.
9. **`LoaiCanHo`**: Định nghĩa loại phòng.
   - *Cột chính*: `MaLoai` (PK), `TenLoai`, `MoTa`.
10. **`TienNghi`**: Danh mục tiện nghi (Điều hòa, Nóng lạnh, Máy giặt...).
    - *Cột chính*: `MaTienNghi` (PK), `TenTienNghi`, `MoTa`.
11. **`TienNghiCuaCanHo`**: Bảng quan hệ N-N giữa tiện nghi và căn hộ.
    - *Cột chính*: `MaCanHo` (PK/FK), `MaTienNghi` (PK/FK).
12. **`HinhAnhNha`**: Quản lý ảnh đính kèm của căn hộ.
    - *Cột chính*: `MaAnh` (PK), `MaCanHo` (FK), `DuongDanAnh`, `NgayTaiUp`.
13. **`GiaDichVu`**: Giá bán lẻ dịch vụ cố định/biến đổi.
    - *Cột chính*: `MaGiaDichVu` (PK), `DonGiaDien`, `DonGiaNuoc`, `PhiDichVuCoDinh`, `NgayApDung`, `MaNhanVien` (FK).
14. **`PhieuDatTruoc`**: Đăng ký giữ chỗ căn hộ của khách hàng.
    - *Cột chính*: `MaPhieuDatTruoc` (PK), `MaCanHo` (FK), `MaKhach` (FK), `MaNhanVien` (FK), `SoTienDatCoc`, `NgayDatCoc`, `NgayHetHan`, `TrangThai` (ChoThanhToanCoc/DaThanhToanCoc/ChoKy/DaKyHD/Huy/HetHan), `PhuongThucThanhToan`, `GhiChu`.
15. **`HopDong`**: Hợp đồng thuê nhà chính thức.
    - *Cột chính*: `MaHopDong` (PK), `MaPhieuDatTruoc` (FK/Null), `MaCanHo` (FK), `MaKhach` (FK), `MaNhanVien` (FK), `NgayBatDau`, `NgayKetThuc`, `GiaThueChot`, `TienCocChot`, `TienCocTruocDaTru` (Số tiền lấy từ cọc giữ phòng được cấn trừ), `NgayTao`, `TrangThai` (HieuLuc/HetHan/DaHuy), `GhiChu`.
16. **`GiaHanHopDong`**: Lịch sử gia hạn hợp đồng thuê.
    - *Cột chính*: `MaGiaHan` (PK), `MaHopDong` (FK), `NgayGiaHanCu`, `NgayGiaHanMoi`, `GiaThueMoi`, `TienCocMoi`, `NgayTao`, `MaNhanVien` (FK).
17. **`HoaDonThanhToan`**: Hóa đơn tiền phòng & dịch vụ hàng tháng.
    - *Cột chính*: `MaHoaDon` (PK), `MaHopDong` (FK), `Phong` (Tên căn hộ lưu vết), `MaNhanVienThu` (FK), `KyThanhToan` (MM/yyyy), `ChiSoDienCu`, `ChiSoDienMoi`, `ChiSoNuocCu`, `ChiSoNuocMoi`, `SoTienPhaiTra`, `SoTienDaTra`, `NgayDaoHan`, `NgayThanhToan`, `TrangThai` (ChuaTra/TraThieu/DaTra/QuaHan), `PhuongThucThanhToan`, `MaViPham` (FK/Null).
18. **`LoaiHoaDon`**: Định nghĩa các loại phí phát sinh ngoài phòng.
    - *Cột chính*: `MaLoaiHoaDon` (PK), `TenLoaiHoaDon`, `DonGiaMacDinh`, `DonViTinh`.
19. **`PhieuTraNha`**: Hồ sơ trả nhà khi thanh lý hợp đồng.
    - *Cột chính*: `MaPhieu` (PK), `MaHopDong` (FK), `MaNhanVien` (FK), `NgayTra`, `TinhTrangNha`, `TienHoanCoc` (Số tiền thực tế trả lại khách sau khi trừ vi phạm), `TienKhauTru` (Tiền phạt khấu trừ), `GhiChu`.
20. **`PhieuXuLyViPham`**: Ghi nhận hư hỏng, vi phạm nội quy.
    - *Cột chính*: `MaViPham` (PK), `MaHopDong` (FK), `MaPhieuTraNha` (FK/Null), `MaNhanVien` (FK), `LoaiViPham`, `MoTa`, `PhiBoiThuong`, `TruVaoCoc` (bool - trừ trực tiếp vào tiền cọc khi trả phòng), `TinhTrang` (ChoXuLy/DaThanhToan/DaKhauTru), `NgayGhiNhan`.
21. **`EmailLog`**: Nhật ký gửi email tự động của hệ thống.
    - *Cột chính*: `Id` (PK), `EmailNhan`, `TieuDe`, `NoiDung`, `NgayGui`, `MaKhach` (FK), `MaPhieuDatTruoc` (FK), `MaHopDong` (FK), `MaHoaDon` (FK), `TinhTrang` (Success/Failed).

---

## 3. Hệ Thống 28 Giao Diện (Forms)

Dự án gồm **28 giao diện** riêng biệt được thiết kế theo chuẩn MaterialSkin:

### Nhóm Giao diện Chung (Shared)
1. **`frmHuongDanSuDung`**: Hiển thị cẩm nang hướng dẫn sử dụng.
   - *Input*: Nhấp nút "Hướng dẫn" trên thanh điều hướng chính.
   - *Output*: Mở file PDF/HTML hướng dẫn tương ứng với phân quyền người dùng.
2. **`frmMain`**: Giao diện chính phân phối các chức năng quản lý.
   - *Input*: Nhấp các tab nghiệp vụ và các nút chức năng trên menu.
   - *Output*: Mở các form con tương ứng; thực hiện Đăng xuất (`SessionContext.Clear()`).

### Nhóm Xác thực (Auth)
3. **`frmLogin`**: Đăng nhập tài khoản nội bộ (Admin/Nhân viên) hoặc Khách hàng.
   - *Input*: Nhập `TenDangNhap`, `MatKhau`.
   - *Output*: Kiểm tra thông tin trong `TaiKhoan` và phân quyền → Thiết lập `SessionContext` → Chuyển tiếp tới giao diện chính.
4. **`frmQuanLyTaiKhoan`**: Quản lý tài khoản Admin/Nhân viên.
   - *Input*: Nhập tên tài khoản, vai trò, mật khẩu.
   - *Output*: Thêm/Sửa/Xóa tài khoản trong bảng `TaiKhoan`.
5. **`frmQuanLyTaiKhoanKhach`**: Quản lý tài khoản cho khách thuê.
   - *Input*: Tên đăng nhập, email liên kết của khách hàng.
   - *Output*: Tạo/Khóa tài khoản khách hàng, liên kết với thực thể `KhachThue`.
6. **`frmPhanQuyenNhanVien`**: Phân chia chi tiết nhiệm vụ nhân viên.
   - *Input*: Chọn nhân viên, tích chọn các quyền cụ thể (Tòa nhà, Hợp đồng, Tài sản, Báo cáo).
   - *Output*: Lưu trạng thái boolean vào bảng `NhanVienQuyen`.

### Nhóm Quản lý Tài sản (TaiSan)
7. **`frmKhuVuc`**: Thiết lập và quản lý các khu vực địa lý.
   - *Input*: Nhập tên khu vực, quận, thành phố. Lấy tọa độ GPS tự động bằng Nominatim API.
   - *Output*: Ghi nhận khu vực mới vào bảng `KhuVuc`.
8. **`frmToa`**: Quản lý thông tin tòa nhà.
   - *Input*: Nhập tên tòa, địa chỉ, số tầng, chọn khu vực quản lý.
   - *Output*: Lưu thông tin vào bảng `Toa`.
9. **`frmLoaiCanHo`**: Phân loại các định dạng phòng.
   - *Input*: Tên loại phòng (Vip, Thường, Studio...), mô tả diện tích tiêu chuẩn.
   - *Output*: Thêm/Sửa/Xóa cấu hình trong bảng `LoaiCanHo`.
10. **`frmCanHo`**: Hồ sơ chi tiết từng phòng trọ.
    - *Input*: Nhập số căn hộ, tầng, chọn tòa, chọn loại căn hộ, diện tích, giá thuê niêm yết, tiền cọc niêm yết, hình ảnh phòng và tiện nghi.
    - *Output*: Tạo căn hộ mới với tình trạng mặc định là `"Trong"`.
11. **`frmTienNghi`**: Danh mục trang thiết bị đi kèm căn hộ.
    - *Input*: Nhập tên tiện nghi (Máy giặt, Điều hòa, Bếp từ...).
    - *Output*: Cập nhật vào bảng `TienNghi`.
12. **`frmGiaDichVu`**: Biểu phí đơn giá điện nước.
    - *Input*: Nhập đơn giá điện/kWh, giá nước/m3, phí dịch vụ cố định hàng tháng.
    - *Output*: Lưu vết lịch sử áp dụng vào bảng `GiaDichVu`.

### Nhóm Hợp đồng & Đặt chỗ (HopDong)
13. **`frmKhachThue`**: Quản lý thông tin hồ sơ khách thuê.
    - *Input*: Họ tên, CMND/CCCD, SĐT, ngày sinh, quê quan, việc làm.
    - *Logic*: Ngày sinh bắt buộc, kiểm tra khách thuê phải đủ 18 tuổi trở lên.
    - *Output*: Lưu thông tin khách thuê mới vào bảng `KhachThue`.
14. **`frmPhieuDatTruoc`**: Quản lý thông tin đặt chỗ của khách tại văn phòng.
    - *Input*: Chọn căn hộ trống, chọn khách hàng, nhập số tiền đặt cọc giữ chỗ.
    - *Output*: Lập phiếu đặt trước ở trạng thái `ChoThanhToanCoc`. Cập nhật căn hộ sang `DaDatCoc`.
15. **`frmHopDong`**: Giao diện khởi tạo và quản lý hợp đồng thuê nhà chính thức.
    - *Input*: Chọn căn hộ (hoặc phiếu đặt trước), ngày bắt đầu, ngày kết thúc, giá thuê chốt, tiền cọc chốt.
    - *Output*: Hợp đồng mới ghi nhận trong bảng `HopDong`. Căn hộ tự động chuyển sang trạng thái `DangThue`. Cấn trừ tiền cọc giữ phòng vào tiền cọc chốt (nếu có).
16. **`frmGiaHanHopDong`**: Thực hiện gia hạn thuê căn hộ.
    - *Input*: Chọn hợp đồng chuẩn bị hết hạn, nhập thời hạn mới, điều chỉnh giá thuê/tiền cọc mới (nếu có).
    - *Output*: Kéo dài ngày kết thúc hợp đồng, lưu thông tin vào bảng `GiaHanHopDong`.

### Nhóm Khách hàng (KhachHang)
17. **`frmTimTroPublic`**: Màn hình tìm trọ công khai (Không cần đăng nhập).
    - *Input*: Tìm kiếm theo từ khóa; lọc theo khu vực, tòa nhà, loại phòng, giá thuê.
    - *Output*: Hiển thị danh sách thẻ phòng trống kèm bản đồ vị trí; cho phép đặt phòng trực tiếp.
18. **`frmDangKyDatTruoc`**: Biểu mẫu khai báo thông tin đặt phòng trực tuyến của khách.
    - *Input*: Khai báo thông tin cá nhân khách thuê (Họ tên, SĐT, CCCD, ngày sinh).
    - *Output*: Tự động khởi tạo phiếu đặt phòng mới, tạo tài khoản khách thuê, gửi email thông tin đăng nhập và hiển thị QR chuyển khoản cọc.
19. **`frmKhachHangHome`**: Cổng thông tin cá nhân dành riêng cho Khách hàng đăng nhập.
    - *Input*: Đăng nhập bằng tài khoản khách hàng. Nhấp làm mới để tải dữ liệu mới nhất.
    - *Output*: Hiển thị trực quan toàn bộ hợp đồng hiện tại, lịch sử thanh toán hóa đơn, danh sách vi phạm và thông tin phiếu đặt trước đang có.
20. **`frmQrThanhToan`**: Hiển thị QR thanh toán tự động (VietQR).
    - *Input*: Nhận thông tin hóa đơn cần trả (Mã hóa đơn, Số tiền, Nội dung).
    - *Output*: Tạo ảnh mã QR động để khách quét thanh toán chuyển khoản qua ngân hàng.

### Nhóm Nhân viên (NhanVien)
21. **`frmNhanVienHome`**: Bảng điều khiển công việc hàng ngày của Nhân viên.
    - *Input*: Xem danh sách công việc cần xử lý gấp (Đặt phòng chờ cọc, hợp đồng sắp hết hạn, hóa đơn chưa thu).
    - *Output*: Nhấp chọn dòng để chuyển nhanh sang các form xử lý (Xác nhận cọc, Ký hợp đồng, Lập hóa đơn).
22. **`frmEmailLog`**: Tra cứu lịch sử gửi email tự động của hệ thống.
    - *Input*: Tìm kiếm theo tên khách hàng, mã hóa đơn hoặc ngày gửi.
    - *Output*: Hiển thị chi tiết nội dung email đã gửi và trạng thái gửi thành công/thất bại.

### Nhóm Thu phí & Trả nhà (TraNha)
23. **`frmHoaDonThanhToan`**: Lập hóa đơn dịch vụ định kỳ hàng tháng.
    - *Input*: Chọn hợp đồng, chọn kỳ thu (tháng/năm), nhập chỉ số điện mới và chỉ số nước mới.
    - *Logic*: Chỉ số cũ được hệ thống tự động điền từ chỉ số mới của hóa đơn kỳ liền trước. Kiểm tra chỉ số mới >= chỉ số cũ.
    - *Output*: Hóa đơn mới trạng thái `ChuaTra` trong bảng `HoaDonThanhToan`.
24. **`frmLoaiHoaDon`**: Định nghĩa danh mục các phụ phí (Dọn vệ sinh, Gửi xe, Rác thải...).
    - *Input*: Nhập tên dịch vụ phụ, đơn giá mặc định.
    - *Output*: Cập nhật vào bảng `LoaiHoaDon`.
25. **`frmPhieuTraNha`**: Lập biên bản trả nhà và quyết toán cọc.
    - *Input*: Chọn hợp đồng cần thanh lý, nhập ngày trả thực tế, ghi nhận hao mòn.
    - *Logic BLL*: Hệ thống tự động quét các vi phạm chưa thu tiền phạt của hợp đồng có tích chọn cờ `TruVaoCoc = true` liên kết với hợp đồng này -> Tính tổng tiền phạt cộng vào `TienKhauTru`.
    - *Output BLL*: Số tiền hoàn trả khách được tính toán chính xác: `TienHoanCoc = max(0, TienCocChot - TienKhauTru)`. Lưu phiếu trả nhà, cập nhật trạng thái hợp đồng thành `HetHan`, căn hộ về `"Trong"`, các phiếu vi phạm liên quan chuyển thành `DaKhauTru` và liên kết với mã phiếu trả nhà. Tất cả các bước được bọc trong một Database Transaction đảm bảo tính toàn vẹn dữ liệu.
26. **`frmPhieuXuLyViPham`**: Ghi nhận phạt vi phạm nội quy / làm hỏng tài sản.
    - *Input*: Chọn hợp đồng, nhập loại hư hại, phí bồi thường (> 0), tích chọn cờ `TruVaoCoc`.
    - *Output*: Tạo phiếu phạt trong bảng `PhieuXuLyViPham` ở trạng thái `ChoXuLy`.

### Nhóm Báo cáo & Thống kê (BaoCao)
27. **`frmDashboard`**: Bảng điều khiển trực quan dành cho Admin.
    - *Input*: Số liệu tổng hợp tự động từ database.
    - *Output*: Hiển thị biểu đồ KPI căn hộ (Tổng, Đang thuê, Trống, Bảo trì), tổng doanh thu tháng hiện tại và danh sách hóa đơn quá hạn.
28. **`frmBaoCao`**: Trích xuất dữ liệu báo cáo thống kê dạng bảng và xuất Excel.
    - *Input*: Chọn loại báo cáo (Doanh thu theo kỳ, Công nợ, Hợp đồng mới, Top căn hộ doanh thu), chọn khoảng thời gian (Từ ngày - Đến ngày).
    - *Output*: Hiển thị danh sách kết quả lên lưới, cho phép xuất file Excel báo cáo bằng thư viện ClosedXML.

---

## 4. Các Luồng Logic Nghiệp Vụ Chuẩn (Chi tiết Input & Output)

### Luồng 1: Đặt Phòng & Nhận Cọc Giữ Chỗ (Booking Flow)
* **Bước 1 (Khách đặt phòng trực tuyến)**:
  - **Input**: Khách hàng chọn căn hộ trống trên giao diện tìm kiếm (`frmTimTroPublic`), điền thông tin cá nhân trên biểu mẫu (`frmDangKyDatTruoc`): Họ tên, SĐT, Số CMND/CCCD, Ngày sinh (bắt buộc, hệ thống kiểm tra phải đủ 18 tuổi trở lên).
  - **Output**: 
    - Khởi tạo 1 bản ghi `PhieuDatTruoc` ở trạng thái `ChoThanhToanCoc` với thời hạn giữ chỗ mặc định là 24 giờ.
    - Tự động tạo tài khoản đăng nhập cho khách hàng trong bảng `TaiKhoan` với vai trò là `KhachThue`.
    - Trạng thái căn hộ trong bảng `CanHo` tự động chuyển từ `"Trong"` sang `"DaDatCoc"`.
    - Gửi email thông báo tự động (bảng `EmailLog`) chứa thông tin tài khoản đăng nhập tạm thời, hạn nộp cọc và hình ảnh mã QR (VietQR) động để chuyển khoản cọc.
* **Bước 2 (Xác nhận cọc)**:
  - **Input**: Nhân viên đối soát tài khoản ngân hàng, nhấp chọn phiếu đặt tương ứng trên danh sách chờ xử lý của `frmNhanVienHome` và nhấn nút **Xác nhận đã nhận cọc**.
  - **Output**: Trạng thái phiếu đặt trước chuyển thành `ChoKy`. Căn hộ tiếp tục được giữ ở trạng thái `DaDatCoc` chờ bước tiếp theo.
* **Bước 3 (Xử lý quá hạn)**:
  - **Input**: Hệ thống tự động thực thi (hoặc nhân viên bấm làm mới dữ liệu).
  - **Output**: Quét toàn bộ phiếu đặt ở trạng thái `ChoThanhToanCoc` quá hạn 24 giờ kể từ thời điểm lập phiếu mà chưa nộp cọc -> Chuyển trạng thái phiếu đặt thành `HetHan` hoặc `Huy`. Trạng thái căn hộ tự động giải phóng, chuyển từ `DaDatCoc` quay về `"Trong"`.

### Luồng 2: Lập Hợp Đồng & Ký Hợp Đồng Thuê (Contract Flow)
* **Bước 1 (Khai báo hợp đồng)**:
  - **Input**: Nhân viên mở `frmHopDong`, bấm thêm mới, chọn căn hộ (hoặc chọn phiếu đặt trước đang ở trạng thái `ChoKy`), nhập Ngày bắt đầu thuê, Ngày kết thúc thuê, Giá thuê chốt hàng tháng, Tiền cọc chốt (mặc định bằng 1 tháng tiền thuê).
  - **Output**: 
    - Khởi tạo bản ghi hợp đồng mới trong bảng `HopDong` trạng thái `HieuLuc`.
    - *Logic tự động cấn trừ cọc*: Nếu căn hộ được chọn liên kết với một phiếu đặt trước đang hiệu lực, hệ thống tự động gán `TienCocTruocDaTru = PhieuDatTruoc.SoTienDatCoc`. Tiền cọc còn thiếu khách cần đóng thêm được tự động tính: `TienCocConPhaiNop = Tiền cọc chốt - Tiền cọc trước đã trừ`.
    - Phiếu đặt trước liên quan tự động chuyển trạng thái thành `DaKyHD`.
    - Căn hộ tự động cập nhật trạng thái từ `DaDatCoc` sang `DangThue`.

### Luồng 3: Ghi Chỉ Số Điện/Nước & Lập Hóa Đơn (Utility & Billing Flow)
* **Bước 1 (Nhập chỉ số và lập hóa đơn)**:
  - **Input**: Định kỳ hàng tháng, nhân viên mở `frmHoaDonThanhToan` chọn hợp đồng, nhập Kỳ thanh toán (tháng/năm), nhập Chỉ số điện mới, Chỉ số nước mới.
  - **Output**:
    - Hệ thống tự động truy vấn chỉ số điện/nước mới của kỳ trước điền làm chỉ số cũ kỳ này.
    - Yêu cầu kiểm tra: `Chỉ số mới >= Chỉ số cũ`. Ngăn chặn lập 2 hóa đơn cùng kỳ thanh toán trên cùng 1 hợp đồng.
    - Bản ghi hóa đơn mới được lưu vào bảng `HoaDonThanhToan` ở trạng thái `ChuaTra`.
    - Số tiền phải trả được tự động tính toán: `SoTienPhaiTra = (Chỉ số mới - Chỉ số cũ) * Đơn giá dịch vụ + Các phụ phí cố định`.
    - Gửi email thông báo hóa đơn tự động kèm QR thanh toán ngân hàng đến hòm thư khách hàng.
* **Bước 2 (Cập nhật trạng thái và thanh toán)**:
  - **Input**: Khách quét mã QR chuyển khoản, hoặc nhân viên ghi nhận thanh toán thủ công.
  - **Output**:
    - Nếu quá ngày thanh toán so với ngày đáo hạn, hệ thống tự động cập nhật trạng thái hóa đơn thành `QuaHan`.
    - Khi hoàn thành thanh toán, hóa đơn chuyển sang `DaTra`, ghi nhận ngày thanh toán thực tế và phương thức thanh toán.

### Luồng 4: Trả Nhà & Quyết Toán Hợp Đồng (Checkout Flow)
* **Bước 1 (Lập phiếu phạt vi phạm nếu có hư hỏng)**:
  - **Input**: Nhân viên phát hiện hao mòn hỏng hóc, mở `frmPhieuXuLyViPham` nhập loại vi phạm, mô tả thiệt hại, phí bồi thường (> 0), tích chọn cờ **Trừ vào cọc** (`TruVaoCoc = true`).
  - **Output**: Lưu bản ghi vi phạm trạng thái `ChoXuLy` trong bảng `PhieuXuLyViPham`.
* **Bước 2 (Thanh lý hợp đồng quyết toán cọc)**:
  - **Input**: Nhân viên mở `frmPhieuTraNha`, chọn hợp đồng cần thanh lý, nhập Ngày trả nhà thực tế, Tình trạng bàn giao căn hộ, Số tiền khấu trừ phát sinh ngoài vi phạm (nếu có).
  - **Output**:
    - Hệ thống tự động quét toàn bộ các phiếu vi phạm đang ở trạng thái `ChoXuLy` có tích chọn cờ `TruVaoCoc = true` liên kết với hợp đồng này -> Tính tổng tiền phạt cộng dồn vào `TienKhauTru`.
    - Thực hiện quyết toán số tiền trả lại khách: `TienHoanCoc = max(0, Tiền cọc chốt - Tiền khấu trừ)`.
    - Bản ghi hợp đồng chính thức chuyển trạng thái `HetHan` (kết thúc hiệu lực).
    - Căn hộ tự động giải phóng, chuyển trạng thái từ `DangThue` về `"Trong"`.
    - Trạng thái các phiếu vi phạm liên quan tự động cập nhật thành `DaKhauTru` và liên kết trực tiếp với mã phiếu trả nhà vừa tạo.

### Luồng 5: Phân Quyền Nhân Viên Quản Lý (Permissions Flow)
* **Bước 1 (Admin phân quyền)**:
  - **Input**: Admin mở `frmPhanQuyenNhanVien`, chọn tài khoản nhân viên từ danh sách, tích chọn các quyền chi tiết: Quyền tòa nhà, Quyền hợp đồng, Quyền tài sản, Quyền báo cáo.
  - **Output**: Cập nhật trạng thái boolean tương ứng của bản ghi trong bảng `NhanVienQuyen`.
* **Bước 2 (Kiểm soát đăng nhập thực tế)**:
  - **Input**: Nhân viên đăng nhập vào hệ thống.
  - **Output**: Hệ thống kiểm tra bảng `NhanVienQuyen`, tự động ẩn/khóa các Tab và các nút chức năng trên giao diện chính `frmMain` mà tài khoản nhân viên đó không có quyền truy cập.

---

## 5. Hướng Dẫn Cài Đặt & Khởi Chạy Dự Án

### Yêu Cầu Môi Trường
- Hệ điều hành: Windows.
- IDE: Visual Studio 2019/2022 trở lên.
- Target Framework: .NET Framework 4.8.
- Cơ sở dữ liệu: SQL Server hoặc SQL Server Express.
- Hỗ trợ: WebView2 Runtime cài sẵn trên Windows.

### Khởi Tạo Database
Kết nối vào SQL Server Management Studio (SSMS) và thực thi các tệp tin script SQL theo đúng trình tự sau:
1. `docs/DATABASE/01_create_database.sql` (Tạo cấu trúc database)
2. `docs/DATABASE/02_seed_data.sql` (Nạp dữ liệu danh mục mặc định)
3. `docs/DATABASE/03_stored_procedures.sql` (Tạo các stored procedure hỗ trợ báo cáo)
4. `docs/DATABASE/05_loai_hoadon_seed.sql` (Nạp danh mục loại phụ phí)
5. `docs/DATABASE/07_test_accounts.sql` (Nạp dữ liệu tài khoản chạy thử)

*Lưu ý*: Nếu đang cập nhật từ phiên bản cũ hơn, hãy thực thi tuần tự các tập tin script từ `04_migration_sua_logic.sql` đến `13_nhanvien_phanquyen.sql` trong thư mục `docs/DATABASE/`.

### Cấu Hình & Chạy Ứng Dụng
1. Mở tệp tin giải pháp `QuanLyChoThueNha.sln` bằng Visual Studio.
2. Kiểm tra và cập nhật Connection String khớp với cấu hình máy chủ SQL Server của bạn tại các tệp cấu hình:
   - `src/QuanLyChoThueNha.DAL/App.config`
   - `src/QuanLyChoThueNha.GUI/App.config`
3. Nhấp chuột phải vào dự án `QuanLyChoThueNha.GUI` -> Chọn **Set as StartUp Project**.
4. Bấm **F5** để biên dịch và bắt đầu chạy ứng dụng.

---

## 6. Tài Khoản Đăng Nhập Thử Nghiệm

Hệ thống được cung cấp sẵn các tài khoản thử nghiệm sau để kiểm tra đầy đủ các vai trò chức năng:

| Vai Trò | Tên Đăng Nhập | Mật Khẩu | Form Giao Diện Chính |
|---|---|---|---|
| **Admin** | `admin_test` | `Admin@123` | `frmMain` (Toàn quyền quản lý, cấu hình hệ thống, xem báo cáo) |
| **Nhân Viên** | `nv_test` | `Admin@123` | `frmNhanVienHome` (Nhận cọc, lập hợp đồng, lập hóa đơn, giải quyết trả phòng) |
| **Khách Thuê** | `khach_test` | `Admin@123` | `frmKhachHangHome` (Xem phòng đã thuê, hóa đơn tiền phòng hàng tháng, QR thanh toán) |
| **Khách Demo** | `khach_demo` | `Admin@123` | `frmKhachHangHome` (Trang khách hàng thử nghiệm) |
