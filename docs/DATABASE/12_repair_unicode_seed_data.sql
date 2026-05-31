USE QuanLyChoThueNha;
GO

/*
  Sua cac dong du lieu mau da tung bi luu thanh dau '?' do chay script/insert
  khong dung Unicode. Script nay chi sua cac ma du lieu mau biet truoc.
  Du lieu nguoi dung nhap sau khi da bi mat dau thi can sua lai thu cong.
*/

UPDATE KhuVuc
SET TenKhuVuc = N'Khu Cầu Giấy',
    Quan = N'Cầu Giấy',
    ThanhPho = N'Hà Nội'
WHERE MaKhuVuc = 'KV001';

UPDATE KhuVuc
SET TenKhuVuc = N'Khu Đống Đa',
    Quan = N'Đống Đa',
    ThanhPho = N'Hà Nội'
WHERE MaKhuVuc = 'KV002';

UPDATE Toa
SET TenToa = N'Tòa A',
    DiaChi = N'Số 1 Đường Nguyễn Trãi, Cầu Giấy, Hà Nội',
    MoTa = N'Tòa nhà mới xây 2020'
WHERE MaToa = 'TO001';

UPDATE Toa
SET TenToa = N'Tòa B',
    DiaChi = N'Số 2 Đường Nguyễn Trãi, Cầu Giấy, Hà Nội'
WHERE MaToa = 'TO002';

UPDATE Toa
SET TenToa = N'Tòa C',
    DiaChi = N'Số 5 Phố Huế, Đống Đa, Hà Nội'
WHERE MaToa = 'TO003';

UPDATE LoaiCanHo
SET TenLoai = N'Studio',
    MoTa = N'Căn hộ 1 phòng ngủ nhỏ gọn'
WHERE MaLoai = 'LC001';

UPDATE LoaiCanHo
SET TenLoai = N'2 Phòng ngủ',
    MoTa = N'Căn hộ 2 phòng ngủ tiêu chuẩn'
WHERE MaLoai = 'LC002';

UPDATE LoaiCanHo
SET TenLoai = N'3 Phòng ngủ',
    MoTa = N'Căn hộ 3 phòng ngủ cao cấp'
WHERE MaLoai = 'LC003';

UPDATE CanHo
SET TinhTrang = N'Trong',
    MoTa = N'Studio tầng 1, ban công nhỏ'
WHERE MaCanHo = 'CH001';

UPDATE CanHo
SET TinhTrang = N'Trong',
    MoTa = N'2PN, view đẹp'
WHERE MaCanHo = 'CH002';

UPDATE CanHo
SET TinhTrang = N'DangThue',
    MoTa = N'3PN, full nội thất'
WHERE MaCanHo = 'CH003';

UPDATE CanHo
SET TinhTrang = N'Trong',
    MoTa = N'Studio tầng 1'
WHERE MaCanHo = 'CH004';

UPDATE CanHo
SET TinhTrang = N'Trong',
    MoTa = N'2PN view phố Huế'
WHERE MaCanHo = 'CH005';

UPDATE TienNghi
SET TenTienNghi = N'Điều hòa',
    MoTa = N'Điều hòa 2 chiều'
WHERE MaTienNghi = 'TN001';

UPDATE TienNghi
SET TenTienNghi = N'Máy giặt',
    MoTa = N'Máy giặt cửa trước'
WHERE MaTienNghi = 'TN002';

UPDATE TienNghi
SET TenTienNghi = N'Internet cáp quang',
    MoTa = N'Tốc độ 100Mbps'
WHERE MaTienNghi = 'TN003';

UPDATE TienNghi
SET TenTienNghi = N'Bãi đỗ xe',
    MoTa = N'Bãi xe trong tòa'
WHERE MaTienNghi = 'TN004';

UPDATE TienNghiCuaCanHo
SET GhiChu = N'Wifi miễn phí'
WHERE MaCanHo = 'CH001' AND MaTienNghi = 'TN003';

UPDATE LoaiHoaDon
SET TenLoai = N'Tiền thuê',
    MoTa = N'Hóa đơn tiền thuê hàng tháng'
WHERE MaLoaiHoaDon = 'LHD001';

UPDATE LoaiHoaDon
SET TenLoai = N'Điện nước',
    MoTa = N'Hóa đơn tiền điện nước'
WHERE MaLoaiHoaDon = 'LHD002';

UPDATE LoaiHoaDon
SET TenLoai = N'Dịch vụ chung',
    MoTa = N'Phí dịch vụ chung cư'
WHERE MaLoaiHoaDon = 'LHD003';

UPDATE LoaiHoaDon
SET TenLoai = N'Vi phạm',
    MoTa = N'Phí bồi thường vi phạm'
WHERE MaLoaiHoaDon = 'LHD004';

UPDATE KhachThue
SET DiaChi = N'Hà Nội'
WHERE MaKhach = 'KH001';

UPDATE Admin
SET HoTen = N'Nguyễn Văn Admin'
WHERE MaAdmin = 'AD001';

UPDATE NhanVienQuanLy
SET HoTen = N'Trần Thị Nhân Viên'
WHERE MaNhanVien = 'NV001';

UPDATE KhachThue
SET HoTen = N'Khách mẫu SmartApart',
    DiaChi = N'Hồ sơ mẫu gửi email'
WHERE MaKhach = 'KH907';

UPDATE PhieuDatTruoc
SET GhiChu = N'Phiếu mẫu SmartApart để kiểm thử gửi email'
WHERE MaPhieuDatTruoc = 'PDT003';

PRINT N'Da sua lai du lieu mau Unicode. Cac dong nguoi dung nhap da bi mat dau can duoc cap nhat thu cong.';
GO
