-- ============================================================
-- 02_seed_data.sql
-- Dữ liệu mẫu để chạy demo ngay.
-- Mật khẩu băm BCrypt work factor 12:
--   admin / Admin@123
--   nhanvien / Admin@123
--   khach / Admin@123
-- ============================================================

USE QuanLyChoThueNha;
GO

-- ── TaiKhoan ────────────────────────────────────────────────
INSERT INTO TaiKhoan VALUES
('TK001','admin',    '$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2','admin@gmail.com',    '0901000001','Admin',    1, GETDATE()),
('TK002','nhanvien', '$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2','nv@gmail.com',       '0901000002','NhanVien', 1, GETDATE()),
('TK003','khach01',  '$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2','khach01@gmail.com',  '0901000003','KhachThue',1, GETDATE());

-- ── Admin ───────────────────────────────────────────────────
INSERT INTO Admin VALUES ('AD001','TK001','Nguyễn Văn Admin');

-- ── NhanVienQuanLy ──────────────────────────────────────────
INSERT INTO NhanVienQuanLy VALUES ('NV001','TK002','Trần Thị Nhân Viên', GETDATE());

-- ── KhachThue ───────────────────────────────────────────────
INSERT INTO KhachThue VALUES ('KH001','TK003','Lê Văn Khách','012345678','Hà Nội','1995-06-15');

-- ── KhuVuc ──────────────────────────────────────────────────
INSERT INTO KhuVuc VALUES ('KV001','AD001','Khu Cầu Giấy','Cầu Giấy','Hà Nội');
INSERT INTO KhuVuc VALUES ('KV002','AD001','Khu Đống Đa','Đống Đa','Hà Nội');

-- ── LoaiCanHo ───────────────────────────────────────────────
INSERT INTO LoaiCanHo VALUES ('LC001','AD001','Studio','Căn hộ 1 phòng ngủ nhỏ gọn');
INSERT INTO LoaiCanHo VALUES ('LC002','AD001','2 Phòng ngủ','Căn hộ 2 phòng ngủ tiêu chuẩn');
INSERT INTO LoaiCanHo VALUES ('LC003','AD001','3 Phòng ngủ','Căn hộ 3 phòng ngủ cao cấp');

-- ── Toa ─────────────────────────────────────────────────────
INSERT INTO Toa VALUES ('TO001','KV001','Tòa A','Số 1 Đường Nguyễn Trãi, CG, HN',15,'Tòa nhà mới xây 2020');
INSERT INTO Toa VALUES ('TO002','KV001','Tòa B','Số 2 Đường Nguyễn Trãi, CG, HN',12,NULL);
INSERT INTO Toa VALUES ('TO003','KV002','Tòa C','Số 5 Phố Huế, ĐĐ, HN',10,NULL);

-- ── CanHo ───────────────────────────────────────────────────
INSERT INTO CanHo VALUES ('CH001','TO001','LC001','NV001',35.0,4500000,9000000,101,1,'Trong','Studio tầng 1, ban công nhỏ',GETDATE());
INSERT INTO CanHo VALUES ('CH002','TO001','LC002','NV001',65.5,8000000,16000000,205,2,'Trong','2PN, view đẹp',GETDATE());
INSERT INTO CanHo VALUES ('CH003','TO001','LC003','NV001',90.0,12000000,24000000,310,3,'DangThue','3PN, full nội thất',GETDATE());
INSERT INTO CanHo VALUES ('CH004','TO002','LC001','NV001',38.0,4800000,9600000,102,1,'Trong','Studio tầng 1',GETDATE());
INSERT INTO CanHo VALUES ('CH005','TO003','LC002','NV001',70.0,9000000,18000000,301,3,'Trong','2PN view phố Huế',GETDATE());

-- ── TienNghi ────────────────────────────────────────────────
INSERT INTO TienNghi VALUES ('TN001','AD001','Điều hòa','Điều hòa 2 chiều');
INSERT INTO TienNghi VALUES ('TN002','AD001','Máy giặt','Máy giặt cửa trước');
INSERT INTO TienNghi VALUES ('TN003','AD001','Internet cáp quang','Tốc độ 100Mbps');
INSERT INTO TienNghi VALUES ('TN004','AD001','Bãi đỗ xe','Bãi xe trong tòa');

-- ── TienNghiCuaCanHo ────────────────────────────────────────
INSERT INTO TienNghiCuaCanHo VALUES ('CH001','TN001',NULL);
INSERT INTO TienNghiCuaCanHo VALUES ('CH001','TN003','Wifi miễn phí');
INSERT INTO TienNghiCuaCanHo VALUES ('CH002','TN001',NULL);
INSERT INTO TienNghiCuaCanHo VALUES ('CH002','TN002',NULL);
INSERT INTO TienNghiCuaCanHo VALUES ('CH002','TN003',NULL);
INSERT INTO TienNghiCuaCanHo VALUES ('CH003','TN001',NULL);
INSERT INTO TienNghiCuaCanHo VALUES ('CH003','TN002',NULL);
INSERT INTO TienNghiCuaCanHo VALUES ('CH003','TN003',NULL);
INSERT INTO TienNghiCuaCanHo VALUES ('CH003','TN004',NULL);

-- ── GiaDichVu ───────────────────────────────────────────────
INSERT INTO GiaDichVu VALUES ('GDV001','TO001',3500,15000,50000,'2025-01-01',NULL,1);
INSERT INTO GiaDichVu VALUES ('GDV002','TO002',3500,15000,45000,'2025-01-01',NULL,1);
INSERT INTO GiaDichVu VALUES ('GDV003','TO003',4000,18000,60000,'2025-01-01',NULL,1);

-- ── LoaiHoaDon ──────────────────────────────────────────────
INSERT INTO LoaiHoaDon VALUES ('LHD001','Tiền thuê','Hóa đơn tiền thuê hàng tháng');
INSERT INTO LoaiHoaDon VALUES ('LHD002','Điện nước','Hóa đơn tiền điện nước');
INSERT INTO LoaiHoaDon VALUES ('LHD003','Dịch vụ chung','Phí dịch vụ chung cư');
INSERT INTO LoaiHoaDon VALUES ('LHD004','Vi phạm','Phí bồi thường vi phạm');

-- ── HopDong mẫu (CH003 đang thuê) ───────────────────────────
-- Liet ke cot tuong minh de tranh loi khi schema thay doi.
INSERT INTO HopDong
    (MaHopDong, MaPhieuDatTruoc, MaCanHo, MaKhach, MaNhanVien,
     NgayBatDau, NgayKetThuc, GiaThueChot, TienCocChot, TienCocTruocDaTru,
     TrangThai, GhiChu, NgayTao)
VALUES ('HD001',NULL,'CH003','KH001','NV001',
    '2025-01-01','2025-12-31',12000000,24000000,0,'HieuLuc',NULL,GETDATE());

-- ── HoaDon mẫu ──────────────────────────────────────────────
INSERT INTO HoaDonThanhToan
    (MaHoaDon, MaHopDong, MaLoaiHoaDon, MaNhanVienThu, MaViPham, KyThanhToan,
     ChiSoDienCu, ChiSoDienMoi, ChiSoNuocCu, ChiSoNuocMoi,
     SoTienPhaiTra, SoTienDaTra, NgayDaoHan, NgayThanhToan, TrangThai, PhuongThucThanhToan)
VALUES ('HOADON001','HD001','LHD001','NV001',NULL,'01/2025',
    0,120,0,15,12000000,12000000,'2025-01-05','2025-01-03','DaTra','ChuyenKhoan');
INSERT INTO HoaDonThanhToan
    (MaHoaDon, MaHopDong, MaLoaiHoaDon, MaNhanVienThu, MaViPham, KyThanhToan,
     ChiSoDienCu, ChiSoDienMoi, ChiSoNuocCu, ChiSoNuocMoi,
     SoTienPhaiTra, SoTienDaTra, NgayDaoHan, NgayThanhToan, TrangThai, PhuongThucThanhToan)
VALUES ('HOADON002','HD001','LHD001','NV001',NULL,'02/2025',
    120,255,15,28,12000000,0,'2025-02-05',NULL,'ChuaTra',NULL);

PRINT 'Dữ liệu mẫu đã được chèn thành công.';
PRINT 'Tài khoản: admin / Admin@123  |  nhanvien / Admin@123  |  khach01 / Admin@123';
GO
