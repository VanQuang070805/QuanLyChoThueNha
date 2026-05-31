-- ============================================================
-- 06_fix_matkhau.sql
-- Van de: seed data dung hash cua chu "password", comment ghi la "Admin@123" -> sai.
-- Script nay cap nhat dung hash BCrypt cua "Admin@123" (work factor 12).
--
-- Chay script nay neu dang nhap vao app bi bao "Mat khau khong chinh xac".
-- ============================================================

USE QuanLyChoThueNha;
GO

UPDATE TaiKhoan
SET MatKhauHash = '$2a$12$i0sWNlTL0T1.YtkCZgHx9ek8XfxBn/3UDB5YC/w/zwgj6EWQaLzM2'
WHERE TenDangNhap IN ('admin', 'nhanvien', 'khach01');

PRINT 'Da cap nhat mat khau thanh Admin@123 cho 3 tai khoan mau.';
GO
