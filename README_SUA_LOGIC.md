# Báo cáo sửa lại logic — Quản lý Cho thuê Nhà

Tài liệu này giải thích **những gì đã được sửa**, **vì sao sửa**, và **cách chạy lại** sau khi cập nhật. Mọi thay đổi đều bám sát đúng luồng nghiệp vụ thực tế đã mô tả.

---

## 1. Luồng nghiệp vụ chuẩn (để đối chiếu)

1. Khách đi xem phòng → có thể **đặt cọc trước** (giữ phòng) **hoặc** làm hợp đồng luôn.
   - Đặt cọc trước: đặt một phần tiền để giữ phòng. Hết hạn giữ phòng mà không vào ở → **mất tiền cọc trước**.
   - Đặt cọc khi làm hợp đồng (thường = 1 tháng). Nếu đã cọc trước thì **trừ luôn** phần cọc trước vào tiền cọc hợp đồng.
2. Nhận đủ cọc → nhân viên **tạo tài khoản** cho khách → **tạo hợp đồng**.
3. Mỗi tháng nhân viên **nhập số điện, số nước** vào hóa đơn để khách thanh toán.
4. Mỗi tháng khách **đăng nhập** xem hợp đồng và **thanh toán hóa đơn**.
5. Hết hạn hợp đồng → khách chọn **gia hạn** hoặc **trả nhà**.
   - 5.1 Gia hạn: nhân viên lập phiếu gia hạn, hợp đồng được kéo dài.
   - 5.2 Trả nhà: lập phiếu trả nhà, **hoàn cọc** nếu nhà nguyên trạng; nếu hỏng hóc → lập **phiếu vi phạm** và **trừ vào tiền cọc**.

---

## 2. Kiến trúc dự án (giữ nguyên)

Mô hình 3 lớp, Entity Framework 6 (Code-First mapping), SQL Server:

```
QuanLyChoThueNha.Model   → Entity (bảng dữ liệu)
QuanLyChoThueNha.DAL     → AppDbContext, Repository, UnitOfWork (truy cập DB)
QuanLyChoThueNha.BLL     → Service (nghiệp vụ), Helper, SessionContext
QuanLyChoThueNha.GUI     → WinForms (MaterialSkin)
```

Nguyên tắc đã được củng cố: **mọi logic nghiệp vụ nằm ở tầng BLL Service**, GUI chỉ thu thập dữ liệu và hiển thị. Trước đây có chỗ GUI "lén" tính toán nghiệp vụ (xem mục 3.4) — đã được dồn về đúng tầng.

---

## 3. Các lỗi đã sửa (theo từng bước nghiệp vụ)

### 3.1. Bước 1 — Tự động trừ cọc trước, tự động hết hạn phiếu giữ phòng

**Lỗi cũ:** Khi ký hợp đồng từ phiếu đặt trước, code chỉ *kiểm tra* `TienCocChot >= SoTienDatCoc` nhưng **không ghi lại** việc cọc trước đã được trừ, và khách/nhân viên không thấy được "còn phải nộp bao nhiêu". Phiếu đặt trước quá hạn cũng **không tự hết hạn**, căn hộ bị "treo" ở trạng thái `DaDatCoc` mãi mãi.

**Đã sửa:**

- **`HopDong`** thêm 2 trường:
  - `TienCocTruocDaTru` *(lưu DB)* — số tiền cọc trước được trừ. **Tự sinh** khi ký hợp đồng: lấy đúng bằng `SoTienDatCoc` của phiếu đặt trước (= 0 nếu không có phiếu).
  - `TienCocConPhaiNop` *(NotMapped, tự tính)* — `= TienCocChot − TienCocTruocDaTru`. Là trường **computed**, không lưu DB, luôn đúng.
- **`HopDongService.KyHopDong`**: tự gán `hopDong.TienCocTruocDaTru = phieu?.SoTienDatCoc`.
- **`PhieuDatTruocService.CapNhatPhieuHetHan()`** *(mới)*: mỗi lần tải danh sách, quét các phiếu `ChoKy` đã quá `NgayHetHan` → chuyển `HetHan` (khách mất cọc) và **giải phóng căn hộ** về `Trong`. Gọi tự động trong `LayTatCa()`.

### 3.2. Yêu cầu riêng — Khách phải đủ 18 tuổi

**Lỗi cũ:** Không có bất kỳ kiểm tra tuổi nào. Form còn cho phép **bỏ trống** ngày sinh.

**Đã sửa:**

- **`ValidationHelper.DuTuoiTroLen(ngaySinh, tuoiToiThieu, out loi)`** *(mới)* — tính tuổi chính xác (có xét đã qua sinh nhật trong năm chưa), từ chối ngày sinh tương lai/để trống.
- Áp dụng tại **cả 2 hàm** tạo khách trong `KhachThueService` (`Them` và `TaoKhachKemTaiKhoan`).
- **`frmKhachThue`**: ngày sinh **bắt buộc** (bỏ checkbox cho phép null), `MaxDate = hôm nay`, mặc định trỏ về mốc 18 tuổi, và chặn lỗi ngay tại form trước khi gọi BLL.

### 3.3. Bước 3 — Lưu chỉ số điện/nước (trước đây bị vứt đi)

**Lỗi cũ:** Form hóa đơn hỏi số điện/nước qua hộp thoại tạm, tính ra tiền **rồi vứt bỏ chỉ số** — không lưu, không truy vết được, kỳ sau phải nhập lại chỉ số cũ thủ công.

**Đã sửa:**

- **`HoaDonThanhToan`** thêm 4 trường: `ChiSoDienCu`, `ChiSoDienMoi`, `ChiSoNuocCu`, `ChiSoNuocMoi`. Lượng tiêu thụ = *Mới − Cũ*.
- **`HoaDonThanhToanService.LayChiSoKyTruoc()`** *(mới)*: tự lấy chỉ số mới của hóa đơn kỳ liền trước làm **chỉ số cũ** kỳ này → nhân viên chỉ cần nhập chỉ số mới.
- **`HoaDonThanhToanService.TinhTienTheoChiSo()`** *(mới)*: tính tiền từ tiêu thụ, có kiểm tra "mới ≥ cũ".
- **`frmHoaDonThanhToan`**: thêm 4 ô nhập chỉ số (ô "Cũ" để chỉ đọc), nút **"Lấy chỉ số kỳ trước"** và nút **"Tính điện/nước/dịch vụ"** dùng đúng các chỉ số trên form.
- Thêm ràng buộc **không lập 2 hóa đơn cùng kỳ + cùng loại** trên một hợp đồng.

### 3.4. Bước 5.2 — Trả nhà, hoàn cọc, trừ vi phạm (lỗi nặng nhất)

**Lỗi cũ:** Tiền hoàn cọc được **tính ở tầng GUI** (`frmPhieuTraNha`), và phiếu vi phạm dù bật cờ `TruVaoCoc = true` **cũng không hề** tác động đến tiền hoàn cọc. Hai thứ rời rạc, dễ tính sai.

**Đã sửa — toàn bộ dồn về `PhieuTraNhaService.LapPhieu`:**

1. Tự tổng hợp **mọi phiếu vi phạm** của hợp đồng có `TruVaoCoc = true` và còn `ChoXuLy`.
2. `TienKhauTru = (nhân viên nhập thêm) + tổng phí vi phạm trừ cọc`.
3. `TienHoanCoc = max(0, TienCocChot − TienKhauTru)` — **tự tính**, GUI không tính nữa (ô này để chỉ đọc).
4. Đánh dấu các phiếu vi phạm thành `DaKhauTru` và **liên kết ngược** về `MaPhieuTraNha`.
5. Tất cả gói trong **một transaction** (cùng việc đổi hợp đồng → `HetHan`, căn hộ → `Trong`).
- Bổ sung: vi phạm có `TruVaoCoc = true` thì **bắt buộc phí bồi thường > 0**.

### 3.5. Các củng cố chung

- **Trạng thái "QuaHan" của hóa đơn**: trước chỉ tô màu ở GUI; nay `HoaDonThanhToanService` tự cập nhật trạng thái thật trong DB khi tải danh sách.
- **Mã người lập phiếu**: trước chỉ gán khi vai trò là Nhân viên (Admin thao tác thì để trống → khó truy vết). Nay gán `SessionContext.MaNguoiDung` cho **cả Admin lẫn Nhân viên** trên các form đặt trước, hóa đơn, vi phạm, trả nhà, gia hạn.
- **Trang khách hàng** (`frmKhachHangHome`): hiển thị thêm cột *Cọc trước đã trừ* và *Cọc còn phải nộp* để khách thấy rõ.
- **Nút Đăng xuất**: đã có sẵn trong `frmMain` (xác nhận trước khi thoát, mở lại màn hình đăng nhập).

---

## 4. Trường tự sinh / tự lấy (tổng hợp)

| Trường | Bảng | Cách điền |
|---|---|---|
| `MaKhach`, `MaTaiKhoan`, `MaHopDong`, `MaPhieuDatTruoc`, `MaHoaDon`, `MaGiaHan`, `MaPhieu` (trả nhà), `MaViPham` | tương ứng | **Tự sinh** theo tiền tố + số thứ tự (`MaGenerator`) |
| `NgayTao`, `NgayDatCoc`, `NgayYeuCau`, `NgayGhiNhan` | nhiều bảng | **Tự sinh** = thời điểm hiện tại |
| `TienCocTruocDaTru` | HopDong | **Tự lấy** từ `PhieuDatTruoc.SoTienDatCoc` |
| `TienCocConPhaiNop` | HopDong | **Tự tính** = `TienCocChot − TienCocTruocDaTru` |
| `ChiSoDienCu`, `ChiSoNuocCu` | HoaDonThanhToan | **Tự lấy** từ hóa đơn kỳ trước |
| `SoTienPhaiTra` (điện/nước) | HoaDonThanhToan | **Tự tính** từ tiêu thụ × bảng giá dịch vụ của tòa |
| `TienKhauTru`, `TienHoanCoc` | PhieuTraNha | **Tự tính** từ tiền cọc + phí vi phạm trừ cọc |
| `MaNhanVien` / `MaNhanVienThu` | nhiều phiếu | **Tự lấy** từ phiên đăng nhập |
| `TrangThai` các phiếu | nhiều bảng | **Tự chuyển** theo nghiệp vụ (ChoKy→HetHan, HieuLuc→HetHan, ChuaTra→QuaHan, ChoXuLy→DaKhauTru…) |

---

## 5. Cách chạy lại

### Trường hợp A — Tạo database mới
Chạy lần lượt trong SQL Server Management Studio:
1. `docs/DATABASE/01_create_database.sql`
2. `docs/DATABASE/02_seed_data.sql`
3. `docs/DATABASE/03_stored_procedures.sql`

### Trường hợp B — Đã có database cũ (giữ dữ liệu)
Chỉ chạy thêm:
- `docs/DATABASE/04_migration_sua_logic.sql` *(mới)* — thêm các cột mới một cách an toàn (chạy lại nhiều lần không lỗi).
- `docs/DATABASE/08_audit_nguoi_thao_tac.sql` — thêm cột audit `MaNguoiThaoTac` và `VaiTroNguoiThaoTac` cho các bảng nghiệp vụ có `MaNhanVien`, giúp Admin thao tác không phá FK nhân viên.

### Mở dự án
1. Mở `QuanLyChoThueNha.sln` bằng Visual Studio (2019/2022).
2. Sửa chuỗi kết nối trong `src/QuanLyChoThueNha.GUI/App.config` (và `DAL/App.config`) cho khớp tên SQL Server của bạn.
3. Visual Studio tự khôi phục NuGet (đã kèm sẵn thư mục `packages/`).
4. Đặt **QuanLyChoThueNha.GUI** làm Startup Project → chạy.

**Tài khoản mẫu:** `admin` / `Admin@123`, `nhanvien` / `Admin@123`, `khach01` / `Admin@123`.

---

## 6. Danh sách file đã thay đổi

**Model:** `HopDong.cs`, `HoaDonThanhToan.cs`
**BLL:** `Helpers/ValidationHelper.cs`, `Services/TienNghiService.cs` (chứa `KhachThueService`), `Services/HopDongService.cs`, `Services/PhieuDatTruocService.cs`, `Services/GiaHanHopDongService.cs` (chứa `HoaDonThanhToanService`, `PhieuTraNhaService`, `PhieuXuLyViPhamService`)
**GUI:** `Forms/HopDong/frmKhachThue.cs`, `Forms/HopDong/frmPhieuDatTruoc.cs`, `Forms/HopDong/frmGiaHanHopDong.cs`, `Forms/TraNha/frmHoaDonThanhToan.cs`, `Forms/TraNha/frmPhieuTraNha.cs`, `Forms/TraNha/frmPhieuXuLyViPham.cs`, `Forms/KhachHang/frmKhachHangHome.cs`
**SQL:** `docs/DATABASE/01_create_database.sql`, `02_seed_data.sql`, `04_migration_sua_logic.sql`, `08_audit_nguoi_thao_tac.sql` (mới)

Mọi thay đổi trong code đều có **comment inline tiếng Việt** đánh dấu `BỔ SUNG` / `YÊU CẦU NGHIỆP VỤ` để bạn dễ tra cứu.
