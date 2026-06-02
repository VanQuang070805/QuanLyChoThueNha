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

## 3. Thống Kê & Chi Tiết Hệ Thống 28 Giao Diện (Forms)

Hệ thống bao gồm **28 giao diện (Form)** chính được xây dựng bằng Windows Forms kết hợp thư viện giao diện hiện đại **MaterialSkin 2** và các linh hồn nghiệp vụ (BLL Services) tương tác với cơ sở dữ liệu qua **Entity Framework 6**.

Dưới đây là thống kê chi tiết của từng Form phân theo các nhóm chức năng nghiệp vụ, bao gồm: đường dẫn tập tin, vai trò/mô tả, chức năng chi tiết và luồng dữ liệu đầu vào/đầu ra.

---

### 3.1. Nhóm Giao Diện Chung & Điều Hướng (Shared)

#### 1. Form Giao Diện Chính - [frmMain.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/frmMain.cs)
*   **Mô tả**: Đây là cổng điều hướng trung tâm của hệ thống dành cho Admin và Nhân viên. Thiết kế dạng Tab Control hiện đại, phân quyền truy cập năng động.
*   **Các chức năng chi tiết**:
    *   **Điều hướng đa nhiệm**: Chuyển đổi nhanh giữa các phân hệ: Quản lý tòa nhà, hợp đồng, tài sản, báo cáo.
    *   **Kiểm soát trạng thái đăng nhập**: Hiển thị tên người dùng đang làm việc, vai trò tài khoản trên góc màn hình.
    *   **Đăng xuất an toàn**: Thực hiện xóa phiên làm việc (`SessionContext.Clear()`), giải phóng bộ nhớ và đóng kết nối tạm thời.
    *   **Tải động giao diện**: Tự động hiển thị/ẩn các nút chức năng và các tab tùy theo bảng phân quyền của nhân viên đang đăng nhập.
*   **Đầu vào (Input)**:
    *   Tài khoản đăng nhập hiện tại từ `SessionContext`.
    *   Tương tác chuột/bàn phím của người quản lý chuyển đổi các Tab nghiệp vụ.
*   **Đầu ra (Output)**:
    *   Hiển thị các Form con/User Control chức năng tương ứng trên vùng làm việc chính.
    *   Khi bấm Đăng xuất, đóng `frmMain` và hiển thị lại form đăng nhập [frmLogin.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/Auth/frmLogin.cs).

#### 2. Form Hướng Dẫn Sử Dụng - [frmHuongDanSuDung.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/Shared/frmHuongDanSuDung.cs)
*   **Mô tả**: Cung cấp tài liệu hướng dẫn vận hành hệ thống trực quan cho từng nhóm đối tượng người dùng.
*   **Các chức năng chi tiết**:
    *   Hiển thị cẩm nang hướng dẫn sử dụng dưới dạng văn bản và hình ảnh minh họa.
    *   **Lọc nội dung theo phân quyền**: Tự động chuyển đến chương hướng dẫn dành riêng cho Admin, Nhân viên hoặc Khách thuê căn hộ tương ứng.
*   **Đầu vào (Input)**: Người dùng nhấn nút "Hướng dẫn" trên thanh menu chính hoặc nhấn phím trợ giúp `F1`.
*   **Đầu ra (Output)**: Cửa sổ thông tin tài liệu hướng dẫn cụ thể.

---

### 3.2. Nhóm Xác Thực & Quản Lý Phân Quyền (Auth)

#### 3. Form Đăng Nhập Hệ Thống - [frmLogin.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/Auth/frmLogin.cs)
*   **Mô tả**: Chốt chặn bảo mật đầu tiên, xác minh thông tin tài khoản của nhân viên, admin và khách thuê để cấp quyền truy cập.
*   **Các chức năng chi tiết**:
    *   **Xác thực thông tin**: Nhập tên tài khoản, mật khẩu. Tích hợp tính năng hiển thị/ẩn mật khẩu giúp bảo mật nơi đông người.
    *   **Mã hóa bảo mật**: Mã hóa mật khẩu đầu vào bằng thuật toán SHA-256 để so khớp với chuỗi mật khẩu băm lưu trong database.
    *   **Kiểm tra trạng thái tài khoản**: Ngăn chặn đăng nhập nếu tài khoản đang bị khóa (`TrangThai = Locked`).
    *   **Khởi tạo Session**: Thiết lập thông tin phiên làm việc trong `SessionContext` khi đăng nhập thành công.
*   **Đầu vào (Input)**: Tên đăng nhập và Mật khẩu do người dùng nhập từ bàn phím.
*   **Đầu ra (Output)**:
    *   Nếu là Admin/Nhân viên: Mở giao diện chính [frmMain.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/frmMain.cs).
    *   Nếu là Khách thuê: Mở giao diện dành riêng cho khách hàng [frmKhachHangHome.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/KhachHang/frmKhachHangHome.cs).

#### 4. Form Quản Lý Tài Khoản Nội Bộ - [frmQuanLyTaiKhoan.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/Auth/frmQuanLyTaiKhoan.cs)
*   **Mô tả**: Dành riêng cho Admin để thêm, sửa, xóa, khóa/mở khóa các tài khoản vận hành hệ thống (Admin, Nhân viên).
*   **Các chức năng chi tiết**:
    *   Hiển thị danh sách tài khoản nội bộ dạng lưới `DataGridView`.
    *   **Thêm mới tài khoản**: Đặt tên đăng nhập, tự động băm mật khẩu, chọn vai trò (`Admin`/`NhanVien`) và gán Email liên hệ.
    *   **Đổi mật khẩu/Khóa tài khoản**: Cập nhật mật khẩu mới hoặc thay đổi trạng thái hoạt động của tài khoản.
*   **Đầu vào (Input)**: Thông tin tài khoản nhập từ form (tên đăng nhập, vai trò, email, mật khẩu).
*   **Đầu ra (Output)**: Thêm mới hoặc cập nhật bản ghi trong bảng `TaiKhoan` trong database.

#### 5. Form Quản Lý Tài Khoản Khách Thuê - [frmQuanLyTaiKhoanKhach.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/Auth/frmQuanLyTaiKhoanKhach.cs)
*   **Mô tả**: Quản lý tài khoản đăng nhập của các khách thuê căn hộ để họ truy cập cổng thông tin tự phục vụ.
*   **Các chức năng chi tiết**:
    *   Liệt kê danh sách tài khoản khách thuê, liên kết với thực thể `KhachThue`.
    *   **Tạo nhanh tài khoản**: Cho phép tạo nhanh tài khoản cho khách thuê vừa ký hợp đồng mà chưa có tài khoản.
    *   **Reset mật khẩu**: Thiết lập lại mật khẩu mặc định và khóa tài khoản khi khách thuê trả phòng thanh lý hợp đồng.
*   **Đầu vào (Input)**: Lựa chọn khách thuê trên lưới, điền thông tin tài khoản đăng nhập.
*   **Đầu ra (Output)**: Cập nhật thông tin bảng `TaiKhoan` và cập nhật trường liên kết khóa ngoại `MaTaiKhoan` trong bảng `KhachThue`.

#### 6. Form Phân Quyền Chi Tiết Nhân Viên - [frmPhanQuyenNhanVien.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/Auth/frmPhanQuyenNhanVien.cs)
*   **Mô tả**: Giúp Admin phân chia các nhóm công việc cụ thể cho từng nhân viên quản lý (không phải nhân viên nào cũng được quyền xem báo cáo hoặc sửa xóa tòa nhà).
*   **Các chức năng chi tiết**:
    *   Hiển thị danh sách nhân viên quản lý.
    *   **Cấu hình phân quyền dạng Checkbox**:
        *   Quyền quản lý tòa nhà, khu vực (`QuyenToaNha`).
        *   Quyền lập và ký hợp đồng, phiếu đặt phòng (`QuyenHopDong`).
        *   Quyền quản lý tài sản, tiện nghi, loại phòng (`QuyenTaiSan`).
        *   Quyền xem biểu đồ doanh số và báo cáo tài chính (`QuyenBaoCao`).
*   **Đầu vào (Input)**: Chọn nhân viên từ danh sách và tích/bỏ tích các quyền hạn tương ứng.
*   **Đầu ra (Output)**: Tạo mới hoặc cập nhật bản ghi phân quyền trong bảng `NhanVienQuyen`.

---

### 3.3. Nhóm Quản Lý Tài Sản & Cấu Hình Giá (TaiSan)

#### 7. Form Quản Lý Khu Vực - [frmKhuVuc.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmKhuVuc.cs)
*   **Mô tả**: Quản lý các phân khu địa giới hành chính nơi doanh nghiệp sở hữu các tòa nhà cho thuê.
*   **Các chức năng chi tiết**:
    *   Tìm kiếm khu vực theo Tên, Quận, Thành phố.
    *   **Định vị GPS tự động**: Nhập tên địa bàn, hệ thống tự động gọi OpenStreetMap Nominatim API để lấy tọa độ Kinh độ (`KinhDo`) và Vĩ độ (`ViDo`) lưu trữ hỗ trợ vẽ bản đồ.
    *   Phân công nhân viên quản lý phụ trách toàn bộ khu vực này.
*   **Đầu vào (Input)**: Tên khu vực, quận, thành phố, nhân viên phụ trách.
*   **Đầu ra (Output)**: Thêm mới hoặc cập nhật bản ghi trong bảng `KhuVuc`.

#### 8. Form Quản Lý Tòa Nhà - [frmToa.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmToa.cs)
*   **Mô tả**: Quản lý các tòa nhà (chung cư mini, nhà trọ lớn) xây dựng trên các khu vực.
*   **Các chức năng chi tiết**:
    *   Hiển thị danh sách tòa nhà, bộ lọc theo khu vực địa lý.
    *   Thêm mới tòa nhà: Tên tòa, Địa chỉ chi tiết, Số tầng, chọn khu vực quản lý trực thuộc.
    *   Tự động xác định tọa độ GPS của địa chỉ tòa nhà thông qua API bản đồ địa lý.
*   **Đầu vào (Input)**: Thông tin tên tòa, địa chỉ, số tầng, kinh độ, vĩ độ, mã khu vực.
*   **Đầu ra (Output)**: Bản ghi trong bảng `Toa` trong database.

#### 9. Form Quản Lý Loại Căn Hộ - [frmLoaiCanHo.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmLoaiCanHo.cs)
*   **Mô tả**: Định nghĩa danh mục các phân loại phòng trọ phục vụ kinh doanh (Ví dụ: Phòng đơn, phòng đôi, penthouse, chung cư mini 2 ngủ).
*   **Các chức năng chi tiết**:
    *   Xem danh sách các loại phòng cùng mô tả chi tiết diện tích tiêu chuẩn và công năng.
    *   Thêm mới/Sửa/Xóa cấu hình loại căn hộ.
    *   **Ràng buộc an toàn**: Ngăn chặn xóa loại phòng nếu có căn hộ thực tế đang liên kết với loại phòng này.
*   **Đầu vào (Input)**: Tên loại căn hộ và mô tả diện tích.
*   **Đầu ra (Output)**: Cập nhật hoặc thêm mới trong bảng `LoaiCanHo`.

#### 10. Form Hồ Sơ Chi Tiết Căn Hộ - [frmCanHo.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmCanHo.cs)
*   **Mô tả**: Quản lý thông tin chi tiết nhất của từng phòng trọ/căn hộ. Đây là nơi chứa hầu hết các thông số kỹ thuật, hình ảnh và trạng thái của căn phòng.
*   **Các chức năng chi tiết**:
    *   Nhập thông tin căn hộ: Số phòng, tầng số, chọn tòa nhà, chọn loại phòng.
    *   Cấu hình giá thuê niêm yết và số tiền đặt cọc niêm yết tiêu chuẩn.
    *   **Quản lý tiện nghi**: Chọn các tiện nghi có sẵn trong phòng (Máy giặt, Tủ lạnh, Điều hòa...) bằng checkbox list.
    *   **Quản lý hình ảnh**: Tải lên các hình ảnh thực tế của căn hộ (lưu đường dẫn file ảnh cục bộ hoặc CDN).
    *   Thay đổi trạng thái căn hộ (`Trong`, `DaDatCoc`, `DangThue`, `BaoTri`).
*   **Đầu vào (Input)**: Số phòng, tòa nhà, loại phòng, diện tích, giá niêm yết, tiền cọc, danh sách tiện nghi và tệp tin hình ảnh đính kèm.
*   **Đầu ra (Output)**: Thêm/Sửa thông tin căn hộ trong bảng `CanHo`, cập nhật các liên kết tiện nghi trong bảng quan hệ `TienNghiCuaCanHo` và danh sách ảnh trong `HinhAnhNha`.

#### 11. Form Quản Lý Tiện Nghi - [frmTienNghi.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmTienNghi.cs)
*   **Mô tả**: Quản lý danh mục các trang thiết bị nội thất cung cấp kèm theo căn hộ để khách thuê sử dụng.
*   **Các chức năng chi tiết**:
    *   Hiển thị danh sách tiện nghi hiện có của hệ thống.
    *   Thêm mới các tiện nghi (Máy giặt, Điều hòa, Bếp điện, Nóng lạnh, Tủ quần áo...).
    *   Chỉnh sửa tên và mô tả chi tiết của tiện nghi.
*   **Đầu vào (Input)**: Tên tiện nghi mới và mô tả công năng.
*   **Đầu ra (Output)**: Bản ghi danh mục trong bảng `TienNghi`.

#### 12. Form Cấu Hình Giá Dịch Vụ - [frmGiaDichVu.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmGiaDichVu.cs)
*   **Mô tả**: Định cấu hình đơn giá các dịch vụ tính theo lượng tiêu thụ thực tế hoặc phí dịch vụ cố định áp dụng cho các căn hộ.
*   **Các chức năng chi tiết**:
    *   Thiết lập Đơn giá điện (đồng/kWh).
    *   Thiết lập Đơn giá nước (đồng/m3).
    *   Thiết lập Phí dịch vụ cố định hàng tháng (phí quản lý, thang máy, vệ sinh tòa nhà).
    *   Lưu lịch sử áp dụng đơn giá theo ngày để làm căn cứ tính hóa đơn chuẩn xác theo từng thời điểm.
*   **Đầu vào (Input)**: Đơn giá điện, đơn giá nước, phí dịch vụ cố định, ngày bắt đầu có hiệu lực áp dụng đơn giá.
*   **Đầu ra (Output)**: Ghi nhận đơn giá dịch vụ vào bảng `GiaDichVu` trong database.

---

### 3.4. Nhóm Hợp Đồng & Đặt Chỗ (HopDong)

#### 13. Form Hồ Sơ Khách Thuê - [frmKhachThue.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/HopDong/frmKhachThue.cs)
*   **Mô tả**: Quản lý thông tin hồ sơ lý lịch cá nhân của tất cả các khách hàng thuê căn hộ trong hệ thống.
*   **Các chức năng chi tiết**:
    *   Thêm mới/Sửa thông tin khách thuê: Họ tên, số CMND/CCCD, Số điện thoại, Email, Ngày sinh, Quê quán, Tình trạng công việc.
    *   **Ràng buộc kiểm tra tính hợp lệ**: Kiểm tra định dạng SĐT, độ dài CMND/CCCD, và bắt buộc khách hàng phải đủ từ 18 tuổi trở lên dựa trên Ngày sinh.
*   **Đầu vào (Input)**: Thông tin cá nhân do nhân viên nhập từ hồ sơ của khách thuê.
*   **Đầu ra (Output)**: Bản ghi thông tin khách hàng trong bảng `KhachThue` trong database.

#### 14. Form Quản Lý Phiếu Đặt Trước - [frmPhieuDatTruoc.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/HopDong/frmPhieuDatTruoc.cs)
*   **Mô tả**: Quản lý việc đặt chỗ giữ phòng của khách hàng trước khi ký hợp đồng chính thức (thực hiện trực tiếp tại văn phòng).
*   **Các chức năng chi tiết**:
    *   **Tạo phiếu giữ chỗ**: Chọn căn hộ đang ở trạng thái `Trong`, chọn khách thuê, nhập số tiền đặt cọc giữ phòng.
    *   Tự động thiết lập thời hạn hiệu lực của phiếu đặt giữ chỗ (mặc định là 24 giờ).
    *   **Xác nhận nộp cọc**: Chuyển trạng thái phiếu từ `ChoThanhToanCoc` sang `DaThanhToanCoc` khi khách đã chuyển khoản/nộp tiền mặt.
    *   **Hủy phiếu đặt**: Cho phép hủy giữ chỗ, giải phóng trạng thái phòng về lại `"Trong"`.
*   **Đầu vào (Input)**: Khách thuê được chọn, Căn hộ được chọn, Số tiền cọc giữ chỗ.
*   **Đầu ra (Output)**: Thêm bản ghi vào bảng `PhieuDatTruoc` và cập nhật trạng thái căn hộ tương ứng trong bảng `CanHo` thành `DaDatCoc`.

#### 15. Form Quản Lý Hợp Đồng Thuê Nhà - [frmHopDong.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/HopDong/frmHopDong.cs)
*   **Mô tả**: Form khởi tạo và quản lý toàn bộ vòng đời của Hợp đồng thuê nhà chính thức giữa khách thuê và nhà quản lý.
*   **Các chức năng chi tiết**:
    *   **Tạo hợp đồng thuê**: Chọn căn hộ (nếu căn hộ đã có phiếu đặt trước, hệ thống sẽ tự động liên kết và truy xuất thông tin).
    *   Nhập Ngày bắt đầu thuê, Ngày kết thúc hợp đồng thuê.
    *   Nhập giá thuê chốt hàng tháng và tiền cọc chốt (mặc định bằng 1 tháng tiền nhà).
    *   **Tự động cấn trừ tiền cọc**: Nếu căn hộ có phiếu đặt trước hợp lệ, hệ thống tự động cấn trừ số tiền cọc giữ phòng vào số tiền cọc chốt, đồng thời tính toán số tiền khách hàng còn thiếu cần đóng thêm.
    *   Cập nhật trạng thái hợp đồng (`HieuLuc`, `HetHan`, `DaHuy`).
*   **Đầu vào (Input)**: Căn hộ chọn, khách thuê chọn, ngày bắt đầu, ngày kết thúc, giá thuê, tiền cọc thực tế.
*   **Đầu ra (Output)**: Ghi nhận hợp đồng mới trong bảng `HopDong`, cập nhật phiếu đặt trước liên quan thành `DaKyHD`, chuyển trạng thái căn hộ thành `DangThue`.

#### 16. Form Gia Hạn Hợp Đồng - [frmGiaHanHopDong.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/HopDong/frmGiaHanHopDong.cs)
*   **Mô tả**: Thực hiện gia hạn thêm thời gian thuê khi hợp đồng cũ của khách chuẩn bị hết hạn.
*   **Các chức năng chi tiết**:
    *   Chọn hợp đồng đang có hiệu lực gần hết hạn từ danh sách.
    *   Nhập ngày kết thúc mới của thời hạn thuê kéo dài.
    *   Điều chỉnh giá thuê và tiền cọc mới cho kỳ gia hạn mới (nếu có thay đổi).
*   **Đầu vào (Input)**: Chọn hợp đồng gia hạn, ngày kết thúc mới, giá thuê mới.
*   **Đầu ra (Output)**: Cập nhật lại ngày kết thúc trong bảng `HopDong` và tạo bản ghi lịch sử gia hạn trong bảng `GiaHanHopDong`.

---

### 3.5. Phân Hệ Phục Vụ Khách Hàng Tự Phục Vụ (KhachHang)

#### 17. Giao Diện Tìm Trọ Công Khai - [frmTimTroPublic.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/KhachHang/frmTimTroPublic.cs)
*   **Mô tả**: Màn hình tìm trọ công khai không cần đăng nhập. Phù hợp đặt tại kiosk tìm phòng ở sảnh tòa nhà hoặc chạy trên màn hình giới thiệu công cộng.
*   **Các chức năng chi tiết**:
    *   **Bộ lọc đa tiêu chí**: Tìm kiếm căn hộ trống theo từ khóa, khu vực, tòa nhà, loại phòng, khoảng giá thuê và diện tích.
    *   **Hiển thị trực quan**: Danh sách căn hộ trống hiển thị dạng thẻ phòng (Card) sinh động kèm hình ảnh đại diện, giá thuê, diện tích.
    *   **Bản đồ tương tác**: Tích hợp điều khiển WebView2 hiển thị bản đồ OpenStreetMap định vị trực tiếp vị trí tòa nhà khi người dùng click vào thẻ phòng.
    *   Nút **"Đặt phòng ngay"** để chuyển tiếp khách đến form điền thông tin đăng ký giữ phòng trực tuyến.
*   **Đầu vào (Input)**: Từ khóa tìm kiếm và các tiêu chí lọc phòng từ người dùng.
*   **Đầu ra (Output)**: Lưới/Danh sách các căn hộ trống thỏa mãn điều kiện lọc.

#### 18. Form Đăng Ký Đặt Phòng Trực Tuyến - [frmDangKyDatTruoc.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/KhachHang/frmDangKyDatTruoc.cs)
*   **Mô tả**: Biểu mẫu giúp khách hàng tự đăng ký thông tin cá nhân và tạo phiếu đặt phòng giữ chỗ trực tuyến trên hệ thống.
*   **Các chức năng chi tiết**:
    *   Cho phép khách điền thông tin cá nhân: Họ tên, Số điện thoại, CCCD, Ngày sinh, Email.
    *   **Tạo tài khoản tự động**: Hệ thống tự động tạo 1 tài khoản đăng nhập cho khách hàng trong bảng `TaiKhoan` với vai trò là `KhachThue` và mật khẩu ngẫu nhiên được băm an toàn.
    *   **Tự động gửi email**: Gửi email thông báo đặt phòng thành công kèm thông tin tài khoản đăng nhập tạm thời, hạn đóng tiền cọc.
    *   **Tích hợp VietQR**: Hiển thị ảnh mã QR thanh toán động chứa đúng số tiền cọc giữ phòng và cú pháp chuyển khoản ngân hàng định sẵn để khách quét mã đóng tiền cọc ngay lập tức.
*   **Đầu vào (Input)**: Thông tin cá nhân do khách hàng tự nhập trên form.
*   **Đầu ra (Output)**: Tạo bản ghi `KhachThue`, `TaiKhoan`, `PhieuDatTruoc` (trạng thái `ChoThanhToanCoc`), đổi trạng thái căn hộ sang `DaDatCoc` và ghi log vào `EmailLog`.

#### 19. Trang Thông Tin Khách Thuê - [frmKhachHangHome.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/KhachHang/frmKhachHangHome.cs)
*   **Mô tả**: Cổng thông tin cá nhân tự phục vụ dành riêng cho Khách thuê sau khi đăng nhập tài khoản của họ để theo dõi hợp đồng, thanh toán hóa đơn.
*   **Các chức năng chi tiết**:
    *   **Xem Hợp đồng**: Hiển thị chi tiết hợp đồng hiện tại, thời hạn, giá thuê và số tiền cọc đã đóng.
    *   **Xem hóa đơn hàng tháng**: Danh sách hóa đơn tiền điện, nước, phòng hàng tháng kèm trạng thái thanh toán (Đã trả / Chưa trả).
    *   **Xem lịch sử vi phạm**: Danh sách lỗi vi phạm nội quy tòa nhà hoặc làm hư hỏng tài sản kèm phí phạt.
    *   **Thanh toán hóa đơn nhanh**: Tích chọn hóa đơn chưa đóng và nhấn nút thanh toán để hiển thị mã QR chuyển khoản ngân hàng động (VietQR).
*   **Đầu vào (Input)**: Mã khách thuê lấy từ `SessionContext` sau khi đăng nhập thành công.
*   **Đầu ra (Output)**: Hiển thị trực quan dữ liệu cá nhân của khách thuê trên các tab giao diện.

#### 20. Form Hiển Thị QR Thanh Toán - [frmQrThanhToan.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/KhachHang/frmQrThanhToan.cs)
*   **Mô tả**: Cửa sổ chuyên dụng hiển thị mã QR thanh toán ngân hàng (VietQR) tự động tạo để khách hàng quét thanh toán tiện lợi.
*   **Các chức năng chi tiết**:
    *   Nhận thông tin thanh toán (Mã hóa đơn/Mã phiếu đặt trước, số tiền phải trả, nội dung chuyển khoản).
    *   Tự động sinh ảnh mã QR chứa các thông tin tài khoản ngân hàng thụ hưởng của nhà quản lý, số tiền chính xác và cú pháp chuyển khoản.
*   **Đầu vào (Input)**: Mã hóa đơn/Mã phiếu đặt, số tiền cần trả và nội dung ghi chú chuyển khoản.
*   **Đầu ra (Output)**: Hiển thị mã QR động trên giao diện cho khách hàng quét bằng ứng dụng ngân hàng di động.

---

### 3.6. Phân Hệ Điều Hành Của Nhân Viên (NhanVien)

#### 21. Bảng Làm Việc Của Nhân Viên - [frmNhanVienHome.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/NhanVien/frmNhanVienHome.cs)
*   **Mô tả**: Trung tâm theo dõi các nhiệm vụ khẩn cấp hàng ngày của nhân viên quản lý để kịp thời xử lý nghiệp vụ.
*   **Các chức năng chi tiết**:
    *   **Danh sách chờ xác nhận cọc**: Liệt kê các phiếu đặt trước trực tuyến đang ở trạng thái `ChoThanhToanCoc` của khách để nhân viên đối soát ngân hàng và xác nhận cọc.
    *   **Danh sách hợp đồng sắp hết hạn**: Liệt kê các hợp đồng thuê nhà sắp hết hạn (trong vòng 30 ngày) để nhân viên liên hệ gia hạn hoặc chuẩn bị trả nhà.
    *   **Danh sách hóa đơn quá hạn**: Liệt kê các hóa đơn trễ hạn đóng tiền để nhân viên đôn đốc, gửi email nhắc nợ.
    *   **Xử lý nhanh**: Cho phép chọn dòng dữ liệu trực tiếp và click các nút hành động (Xác nhận cọc, Ký hợp đồng, Lập hóa đơn) để chuyển nhanh đến form xử lý tương ứng.
*   **Đầu vào (Input)**: Số liệu truy vấn tổng hợp từ các bảng phiếu đặt, hợp đồng, hóa đơn trễ hạn.
*   **Đầu ra (Output)**: Điều hướng nhân viên đến các form chức năng nghiệp vụ nhanh chóng.

#### 22. Form Nhật Ký Gửi Thư Điện Tử - [frmEmailLog.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/NhanVien/frmEmailLog.cs)
*   **Mô tả**: Nơi tra cứu và giám sát hệ thống gửi email tự động (Hóa đơn, nhắc nợ, tài khoản, xác nhận cọc) để nhân viên hỗ trợ khách hàng khi cần thiết.
*   **Các chức năng chi tiết**:
    *   Hiển thị danh sách email hệ thống đã gửi: Địa chỉ email nhận, tiêu đề thư, thời gian gửi, trạng thái gửi thư (`Success` / `Failed`).
    *   Tìm kiếm email theo địa chỉ email nhận, theo mã khách thuê hoặc mã hóa đơn liên quan.
    *   Xem chi tiết nội dung email (Text hoặc mã HTML) của từng dòng nhật ký.
*   **Đầu vào (Input)**: Từ khóa tìm kiếm, bộ lọc trạng thái gửi email.
*   **Đầu ra (Output)**: Nhật ký chi tiết của dịch vụ gửi mail tự động hiển thị trên lưới.

---

### 3.7. Nhóm Nghiệp Vụ Thu Phí & Trả Nhà (TraNha)

#### 23. Form Lập Hóa Đơn Thanh Toán - [frmHoaDonThanhToan.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TraNha/frmHoaDonThanhToan.cs)
*   **Mô tả**: Tính toán và lập hóa đơn thu phí định kỳ hàng tháng (tiền nhà + tiền điện + tiền nước + phụ phí phát sinh) của từng phòng.
*   **Các chức năng chi tiết**:
    *   Chọn hợp đồng thuê đang có hiệu lực.
    *   **Tự động điền chỉ số cũ**: Hệ thống tự động truy vấn chỉ số điện, nước mới của hóa đơn tháng trước điền làm chỉ số cũ tháng này để tránh nhân viên nhập sai.
    *   Nhập Chỉ số điện mới và Chỉ số nước mới (Ràng buộc: Chỉ số mới phải lớn hơn hoặc bằng chỉ số cũ).
    *   **Tính tiền tự động**: Hệ thống tính lượng điện nước tiêu thụ và nhân với đơn giá dịch vụ hiện hành trong bảng `GiaDichVu`, cộng thêm tiền nhà cố định để ra tổng số tiền hóa đơn.
    *   **Quản lý phụ phí**: Thêm các dịch vụ phụ phát sinh (như dọn dẹp vệ sinh thêm, giặt là...) vào hóa đơn.
    *   Gửi email thông báo hóa đơn tự động kèm link QR thanh toán tới khách hàng sau khi lập.
*   **Đầu vào (Input)**: Hợp đồng chọn, chỉ số điện mới, chỉ số nước mới, các phụ phí đi kèm.
*   **Đầu ra (Output)**: Bản ghi hóa đơn mới trong bảng `HoaDonThanhToan` ở trạng thái `ChuaTra`, gửi email thông báo hóa đơn đến hòm thư khách hàng.

#### 24. Form Quản Lý Loại Hóa Đơn Dịch Vụ - [frmLoaiHoaDon.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TraNha/frmLoaiHoaDon.cs)
*   **Mô tả**: Danh mục quản lý các dịch vụ phụ phí phát sinh đi kèm ngoài tiền điện và nước (Ví dụ: Tiền gửi xe máy, Phí dọn dẹp, Phí internet tòa nhà).
*   **Các chức năng chi tiết**:
    *   Xem danh sách các loại dịch vụ phụ phí hiện có.
    *   Thêm mới loại dịch vụ phụ phí: Tên phí, Đơn giá mặc định, Đơn vị tính (đồng/tháng, đồng/lần...).
    *   Chỉnh sửa tên và cập nhật biểu giá mặc định của các loại phụ phí này.
*   **Đầu vào (Input)**: Tên dịch vụ phụ phí, đơn giá, đơn vị tính.
*   **Đầu ra (Output)**: Bản ghi danh mục trong bảng `LoaiHoaDon` trong database.

#### 25. Form Lập Phiếu Trả Nhà & Quyết Toán - [frmPhieuTraNha.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TraNha/frmPhieuTraNha.cs)
*   **Mô tả**: Thực hiện thủ tục thanh lý hợp đồng thuê, bàn giao lại phòng và quyết toán hoàn lại tiền cọc cho khách thuê.
*   **Các chức năng chi tiết**:
    *   Chọn hợp đồng cần thanh lý, nhập Ngày trả phòng thực tế và ghi nhận tình trạng bàn giao phòng (hao mòn hao phí tài sản).
    *   **Tự động quét lỗi vi phạm khấu trừ cọc**: Hệ thống tự động quét toàn bộ các phiếu vi phạm đang ở trạng thái `ChoXuLy` có tích chọn cờ khấu trừ cọc (`TruVaoCoc = true`) liên kết với hợp đồng này -> Tính tổng số tiền phạt cộng vào `TienKhauTru`.
    *   **Tính toán hoàn cọc chính xác**: Thực hiện phép tính quyết toán: `TienHoanCoc = max(0, Tiền cọc chốt - Tiền khấu trừ)`.
    *   **Quy trình giao dịch an toàn (DB Transaction)**: Thực thi đồng loạt các bước cập nhật database trong một Transaction để đảm bảo tính toàn vẹn (nếu một bước lỗi sẽ rollback toàn bộ dữ liệu).
*   **Đầu vào (Input)**: Hợp đồng chọn thanh lý, ngày trả nhà thực tế, ghi chú tình trạng nhà bàn giao, số tiền khấu trừ phát sinh ngoài.
*   **Đầu ra (Output)**: Tạo phiếu trả nhà trong bảng `PhieuTraNha`, chuyển trạng thái hợp đồng thành `HetHan` hoặc `DaHuy`, chuyển trạng thái căn hộ về lại `"Trong"`, cập nhật các phiếu vi phạm liên quan thành `DaKhauTru` và liên kết với phiếu trả nhà.

#### 26. Form Ghi Nhận Phạt Vi Phạm - [frmPhieuXuLyViPham.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TraNha/frmPhieuXuLyViPham.cs)
*   **Mô tả**: Ghi nhận các biên bản vi phạm nội quy tòa nhà hoặc làm hỏng hóc trang thiết bị nội thất của phòng.
*   **Các chức năng chi tiết**:
    *   Chọn hợp đồng của khách thuê vi phạm.
    *   Nhập Loại vi phạm, Mô tả chi tiết mức độ hư hại tài sản.
    *   Nhập phí bồi thường (yêu cầu > 0).
    *   **Cơ chế cờ Khấu trừ cọc**: Tích chọn cờ **Trừ vào cọc** (`TruVaoCoc = true`) để hệ thống tự động gom tiền phạt này trừ vào tiền cọc khi trả phòng ở form [frmPhieuTraNha.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/TraNha/frmPhieuTraNha.cs). Nếu không tích chọn, khách hàng sẽ phải đóng tiền mặt/chuyển khoản riêng như hóa đơn thông thường.
*   **Đầu vào (Input)**: Hợp đồng liên quan, loại vi phạm, mô tả, mức phí phạt, cờ trừ vào cọc.
*   **Đầu ra (Output)**: Ghi nhận biên bản vi phạm mới trong bảng `PhieuXuLyViPham` ở trạng thái `ChoXuLy`.

---

### 3.8. Nhóm Báo Cáo Doanh Thu & Thống Kê (BaoCao)

#### 27. Bảng Dashboard Tổng Quan - [frmDashboard.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/BaoCao/frmDashboard.cs)
*   **Mô tả**: Bảng tổng quan thông minh hiển thị các chỉ số kinh doanh chính (KPI) thời gian thực dành cho Admin quản trị hệ thống.
*   **Các chức năng chi tiết**:
    *   **KPI trạng thái căn hộ**: Thống kê số lượng căn hộ: Tổng số, Đang cho thuê, Đang trống, Đang bảo trì.
    *   **Biểu đồ lấp đầy**: Vẽ biểu đồ tròn thể hiện tỷ lệ lấp đầy phòng của toàn hệ thống một cách trực quan.
    *   **Thống kê doanh số**: Hiển thị tổng doanh thu thu về trong tháng hiện tại và doanh số trễ hạn (công nợ quá hạn).
    *   Hiển thị danh sách nhanh các hóa đơn quá hạn cần thu tiền gấp.
*   **Đầu vào (Input)**: Số liệu tổng hợp tự động từ database.
*   **Đầu ra (Output)**: Biểu đồ trực quan và danh sách tóm tắt hoạt động kinh doanh hiển thị trên giao diện chính.

#### 28. Form Thống Kê & Kết Xuất Báo Cáo - [frmBaoCao.cs](file:///e:/QuanLyChoThueNha/src/QuanLyChoThueNha.GUI/Forms/BaoCao/frmBaoCao.cs)
*   **Mô tả**: Hỗ trợ Admin trích xuất dữ liệu chi tiết của hoạt động kinh doanh và xuất dữ liệu ra file Excel phục vụ kiểm toán.
*   **Các chức năng chi tiết**:
    *   **Hỗ trợ 4 loại báo cáo chuyên sâu**:
        1.  *Báo cáo doanh thu theo kỳ*: Tổng doanh số thu về từ tiền phòng và dịch vụ điện/nước/phí phát sinh theo khoảng thời gian.
        2.  *Báo cáo công nợ*: Danh sách các hóa đơn quá hạn chưa thanh toán, số ngày trễ hẹn của từng khách thuê.
        3.  *Báo cáo hợp đồng ký mới*: Tổng số hợp đồng thuê nhà được thiết lập mới trong khoảng thời gian chọn.
        4.  *Báo cáo top căn hộ doanh thu*: Xếp hạng các căn hộ mang lại nguồn thu lớn nhất cho hệ thống.
    *   Chọn khoảng thời gian lọc dữ liệu (Từ ngày - Đến ngày).
    *   **Xuất dữ liệu ra Excel**: Sử dụng thư viện **ClosedXML** để kết xuất lưới dữ liệu thành file Excel (.xlsx) được căn chỉnh cột, định dạng màu sắc đẹp mắt và chuyên nghiệp để tải về máy tính cá nhân.
*   **Đầu vào (Input)**: Chọn loại báo cáo cần xem, chọn khoảng thời gian lọc (Từ ngày - Đến ngày).
*   **Đầu ra (Output)**: Hiển thị lưới dữ liệu kết quả báo cáo trên form, tạo và lưu trữ file Excel báo cáo ra máy tính người dùng khi click **Xuất Excel**.

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
