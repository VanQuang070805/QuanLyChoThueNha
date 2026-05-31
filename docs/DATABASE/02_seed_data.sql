-- ============================================================
-- 02_seed_data.sql
-- Du lieu mau dung Unicode. Mat khau mau:
--   admin / Admin@123
--   nhanvien / Admin@123
--   khach01 / Admin@123
-- ============================================================

USE QuanLyChoThueNha;
GO

INSERT INTO TaiKhoan
    (MaTaiKhoan, TenDangNhap, MatKhauHash, Email, SoDienThoai, VaiTro, TrangThai, NgayTao)
VALUES
('TK001',N'admin',    N'$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2',N'admin@gmail.com',   '0901000001',N'Admin',     1, GETDATE()),
('TK002',N'nhanvien', N'$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2',N'nv@gmail.com',      '0901000002',N'NhanVien',  1, GETDATE()),
('TK003',N'khach01',  N'$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2',N'khach01@gmail.com', '0901000003',N'KhachThue', 1, GETDATE());

INSERT INTO Admin (MaAdmin, MaTaiKhoan, HoTen)
VALUES ('AD001','TK001',N'Nguyễn Văn Admin');

INSERT INTO NhanVienQuanLy (MaNhanVien, MaTaiKhoan, HoTen, NgayVaoLam)
VALUES ('NV001','TK002',N'Trần Thị Nhân Viên', GETDATE());

INSERT INTO KhachThue (MaKhach, MaTaiKhoan, HoTen, SoCMND, DiaChi, NgaySinh)
VALUES ('KH001','TK003',N'Lê Văn Khách','012345678',N'Hà Nội','1995-06-15');

INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
VALUES
('KV001','AD001',N'Khu Cầu Giấy',N'Cầu Giấy',N'Hà Nội',21.036237,105.790583),
('KV002','AD001',N'Khu Đống Đa',N'Đống Đa',N'Hà Nội',21.018072,105.829949);

INSERT INTO LoaiCanHo (MaLoai, MaAdmin, TenLoai, MoTa)
VALUES
('LC001','AD001',N'Studio',N'Căn hộ 1 phòng ngủ nhỏ gọn'),
('LC002','AD001',N'2 Phòng ngủ',N'Căn hộ 2 phòng ngủ tiêu chuẩn'),
('LC003','AD001',N'3 Phòng ngủ',N'Căn hộ 3 phòng ngủ cao cấp');

INSERT INTO Toa (MaToa, MaKhuVuc, TenToa, DiaChi, SoTang, MoTa)
VALUES
('TO001','KV001',N'Tòa A',N'Số 1 Đường Nguyễn Trãi, Cầu Giấy, Hà Nội',15,N'Tòa nhà mới xây 2020'),
('TO002','KV001',N'Tòa B',N'Số 2 Đường Nguyễn Trãi, Cầu Giấy, Hà Nội',12,NULL),
('TO003','KV002',N'Tòa C',N'Số 5 Phố Huế, Đống Đa, Hà Nội',10,NULL);

INSERT INTO CanHo
    (MaCanHo, MaToa, MaLoai, MaNhanVien, MaNguoiThaoTac, VaiTroNguoiThaoTac,
     DienTich, GiaThueNiemYet, TienCocNiemYet, SoCanHo, TangSo, TinhTrang, MoTa, NgayTao)
VALUES
('CH001','TO001','LC001','NV001',NULL,NULL,35.0,4500000, 9000000,101,1,N'Trong',N'Studio tầng 1, ban công nhỏ',GETDATE()),
('CH002','TO001','LC002','NV001',NULL,NULL,65.5,8000000,16000000,205,2,N'Trong',N'2PN, view đẹp',GETDATE()),
('CH003','TO001','LC003','NV001',NULL,NULL,90.0,12000000,24000000,310,3,N'DangThue',N'3PN, full nội thất',GETDATE()),
('CH004','TO002','LC001','NV001',NULL,NULL,38.0,4800000, 9600000,102,1,N'Trong',N'Studio tầng 1',GETDATE()),
('CH005','TO003','LC002','NV001',NULL,NULL,70.0,9000000,18000000,301,3,N'Trong',N'2PN view phố Huế',GETDATE());

INSERT INTO TienNghi (MaTienNghi, MaAdmin, TenTienNghi, MoTa)
VALUES
('TN001','AD001',N'Điều hòa',N'Điều hòa 2 chiều'),
('TN002','AD001',N'Máy giặt',N'Máy giặt cửa trước'),
('TN003','AD001',N'Internet cáp quang',N'Tốc độ 100Mbps'),
('TN004','AD001',N'Bãi đỗ xe',N'Bãi xe trong tòa');

INSERT INTO TienNghiCuaCanHo (MaCanHo, MaTienNghi, GhiChu)
VALUES
('CH001','TN001',NULL),
('CH001','TN003',N'Wifi miễn phí'),
('CH002','TN001',NULL),
('CH002','TN002',NULL),
('CH002','TN003',NULL),
('CH003','TN001',NULL),
('CH003','TN002',NULL),
('CH003','TN003',NULL),
('CH003','TN004',NULL);

INSERT INTO GiaDichVu
    (MaGiaDichVu, MaToa, GiaDien, GiaNuoc, GiaDichVuChung, NgayApDung, NgayKetThuc, DangApDung)
VALUES
('GDV001','TO001',3500,15000,50000,'2025-01-01',NULL,1),
('GDV002','TO002',3500,15000,45000,'2025-01-01',NULL,1),
('GDV003','TO003',4000,18000,60000,'2025-01-01',NULL,1);

INSERT INTO LoaiHoaDon (MaLoaiHoaDon, TenLoai, MoTa)
VALUES
('LHD001',N'Tiền thuê',N'Hóa đơn tiền thuê hàng tháng'),
('LHD002',N'Điện nước',N'Hóa đơn tiền điện nước'),
('LHD003',N'Dịch vụ chung',N'Phí dịch vụ chung cư'),
('LHD004',N'Vi phạm',N'Phí bồi thường vi phạm');

INSERT INTO HopDong
    (MaHopDong, MaPhieuDatTruoc, MaCanHo, MaKhach, MaNhanVien, MaNguoiThaoTac, VaiTroNguoiThaoTac,
     NgayBatDau, NgayKetThuc, GiaThueChot, TienCocChot, TienCocTruocDaTru,
     TrangThai, GhiChu, NgayTao)
VALUES ('HD001',NULL,'CH003','KH001','NV001',NULL,NULL,
    '2025-01-01','2025-12-31',12000000,24000000,0,N'HieuLuc',NULL,GETDATE());

INSERT INTO HoaDonThanhToan
    (MaHoaDon, MaHopDong, MaLoaiHoaDon, MaNhanVienThu, MaNguoiThaoTac, VaiTroNguoiThaoTac,
     MaViPham, KyThanhToan, ChiSoDienCu, ChiSoDienMoi, ChiSoNuocCu, ChiSoNuocMoi,
     SoTienPhaiTra, SoTienDaTra, NgayDaoHan, NgayThanhToan, TrangThai, PhuongThucThanhToan)
VALUES
('HOADON001','HD001','LHD001','NV001',NULL,NULL,NULL,N'01/2025',
    0,120,0,15,12000000,12000000,'2025-01-05','2025-01-03',N'DaTra',N'ChuyenKhoan'),
('HOADON002','HD001','LHD001','NV001',NULL,NULL,NULL,N'02/2025',
    120,255,15,28,12000000,0,'2025-02-05',NULL,N'ChuaTra',NULL);

PRINT N'Du lieu mau da duoc chen thanh cong.';
PRINT N'Tai khoan: admin / Admin@123 | nhanvien / Admin@123 | khach01 / Admin@123';
GO
