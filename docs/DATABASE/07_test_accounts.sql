-- ============================================================
-- 07_test_accounts.sql
-- Tao tai khoan thu nghiem cho 3 vai tro: Admin, NhanVien, KhachThue.
-- Script idempotent: co the chay lai nhieu lan ma khong tao trung.
--
-- Mat khau mac dinh cho tat ca tai khoan: Admin@123
-- ============================================================

USE QuanLyChoThueNha;
GO

DECLARE @HashAdmin123 NVARCHAR(255) =
    '$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2';

-- Admin test
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'admin_test')
BEGIN
    INSERT INTO TaiKhoan
        (MaTaiKhoan, TenDangNhap, MatKhauHash, Email, SoDienThoai, VaiTro, TrangThai, NgayTao)
    VALUES
        ('TK901', 'admin_test', @HashAdmin123, 'admin.test@example.com', '0919000901', 'Admin', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Admin WHERE MaAdmin = 'AD901')
BEGIN
    INSERT INTO Admin (MaAdmin, MaTaiKhoan, HoTen)
    VALUES ('AD901', 'TK901', N'Admin Thu Nghiem');
END

-- Nhan vien test
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'nv_test')
BEGIN
    INSERT INTO TaiKhoan
        (MaTaiKhoan, TenDangNhap, MatKhauHash, Email, SoDienThoai, VaiTro, TrangThai, NgayTao)
    VALUES
        ('TK902', 'nv_test', @HashAdmin123, 'nv.test@example.com', '0919000902', 'NhanVien', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM NhanVienQuanLy WHERE MaNhanVien = 'NV902')
BEGIN
    INSERT INTO NhanVienQuanLy (MaNhanVien, MaTaiKhoan, HoTen, NgayVaoLam)
    VALUES ('NV902', 'TK902', N'Nhan Vien Thu Nghiem', GETDATE());
END

-- Khach hang test 1: khach co the dat phong
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'khach_test')
BEGIN
    INSERT INTO TaiKhoan
        (MaTaiKhoan, TenDangNhap, MatKhauHash, Email, SoDienThoai, VaiTro, TrangThai, NgayTao)
    VALUES
        ('TK903', 'khach_test', @HashAdmin123, 'khach.test@example.com', '0919000903', 'KhachThue', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM KhachThue WHERE MaKhach = 'KH903')
BEGIN
    INSERT INTO KhachThue (MaKhach, MaTaiKhoan, HoTen, SoCMND, DiaChi, NgaySinh)
    VALUES ('KH903', 'TK903', N'Khach Hang Thu Nghiem', '0900000903', N'Ha Noi', '1998-03-15');
END

-- Khach hang test 2: dung de kiem thu khoa/mo khoa va loc danh sach
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'khach_demo')
BEGIN
    INSERT INTO TaiKhoan
        (MaTaiKhoan, TenDangNhap, MatKhauHash, Email, SoDienThoai, VaiTro, TrangThai, NgayTao)
    VALUES
        ('TK904', 'khach_demo', @HashAdmin123, 'khach.demo@example.com', '0919000904', 'KhachThue', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM KhachThue WHERE MaKhach = 'KH904')
BEGIN
    INSERT INTO KhachThue (MaKhach, MaTaiKhoan, HoTen, SoCMND, DiaChi, NgaySinh)
    VALUES ('KH904', 'TK904', N'Khach Hang Demo', '0900000904', N'TP Ho Chi Minh', '1999-07-20');
END

PRINT 'Da tao/kiem tra tai khoan test: admin_test, nv_test, khach_test, khach_demo.';
PRINT 'Mat khau mac dinh: Admin@123';
GO
