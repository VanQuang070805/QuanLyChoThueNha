-- ============================================================
-- 05_loai_hoadon_seed.sql
-- Chay script nay neu LoaiHoaDon chua co du lieu (loi "Chưa có Loại hóa đơn").
-- An toan: chi INSERT neu chua ton tai (kiem tra truoc).
-- ============================================================

USE QuanLyChoThueNha;
GO

IF NOT EXISTS (SELECT 1 FROM LoaiHoaDon WHERE MaLoaiHoaDon = 'LHD001')
    INSERT INTO LoaiHoaDon VALUES ('LHD001', N'Tien thue', N'Hoa don tien thue hang thang');
GO

IF NOT EXISTS (SELECT 1 FROM LoaiHoaDon WHERE MaLoaiHoaDon = 'LHD002')
    INSERT INTO LoaiHoaDon VALUES ('LHD002', N'Dien nuoc', N'Hoa don tien dien nuoc');
GO

IF NOT EXISTS (SELECT 1 FROM LoaiHoaDon WHERE MaLoaiHoaDon = 'LHD003')
    INSERT INTO LoaiHoaDon VALUES ('LHD003', N'Dich vu chung', N'Phi dich vu chung cu');
GO

IF NOT EXISTS (SELECT 1 FROM LoaiHoaDon WHERE MaLoaiHoaDon = 'LHD004')
    INSERT INTO LoaiHoaDon VALUES ('LHD004', N'Vi pham', N'Phi boi thuong vi pham');
GO

PRINT 'Da them LoaiHoaDon thanh cong.';
GO
