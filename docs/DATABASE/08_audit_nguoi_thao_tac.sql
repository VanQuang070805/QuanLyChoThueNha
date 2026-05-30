-- ============================================================
-- 08_audit_nguoi_thao_tac.sql
-- Them audit nguoi thao tac cho cac bang nghiep vu co MaNhanVien.
-- Script idempotent: co the chay lai nhieu lan.
-- ============================================================

USE QuanLyChoThueNha;
GO

IF COL_LENGTH('CanHo', 'MaNguoiThaoTac') IS NULL
    ALTER TABLE CanHo ADD MaNguoiThaoTac VARCHAR(50) NULL;
IF COL_LENGTH('CanHo', 'VaiTroNguoiThaoTac') IS NULL
    ALTER TABLE CanHo ADD VaiTroNguoiThaoTac NVARCHAR(50) NULL;

IF COL_LENGTH('PhieuDatTruoc', 'MaNguoiThaoTac') IS NULL
    ALTER TABLE PhieuDatTruoc ADD MaNguoiThaoTac VARCHAR(50) NULL;
IF COL_LENGTH('PhieuDatTruoc', 'VaiTroNguoiThaoTac') IS NULL
    ALTER TABLE PhieuDatTruoc ADD VaiTroNguoiThaoTac NVARCHAR(50) NULL;

IF COL_LENGTH('HopDong', 'MaNguoiThaoTac') IS NULL
    ALTER TABLE HopDong ADD MaNguoiThaoTac VARCHAR(50) NULL;
IF COL_LENGTH('HopDong', 'VaiTroNguoiThaoTac') IS NULL
    ALTER TABLE HopDong ADD VaiTroNguoiThaoTac NVARCHAR(50) NULL;

IF COL_LENGTH('HoaDonThanhToan', 'MaNguoiThaoTac') IS NULL
    ALTER TABLE HoaDonThanhToan ADD MaNguoiThaoTac VARCHAR(50) NULL;
IF COL_LENGTH('HoaDonThanhToan', 'VaiTroNguoiThaoTac') IS NULL
    ALTER TABLE HoaDonThanhToan ADD VaiTroNguoiThaoTac NVARCHAR(50) NULL;

IF COL_LENGTH('GiaHanHopDong', 'MaNguoiThaoTac') IS NULL
    ALTER TABLE GiaHanHopDong ADD MaNguoiThaoTac VARCHAR(50) NULL;
IF COL_LENGTH('GiaHanHopDong', 'VaiTroNguoiThaoTac') IS NULL
    ALTER TABLE GiaHanHopDong ADD VaiTroNguoiThaoTac NVARCHAR(50) NULL;

IF COL_LENGTH('PhieuTraNha', 'MaNguoiThaoTac') IS NULL
    ALTER TABLE PhieuTraNha ADD MaNguoiThaoTac VARCHAR(50) NULL;
IF COL_LENGTH('PhieuTraNha', 'VaiTroNguoiThaoTac') IS NULL
    ALTER TABLE PhieuTraNha ADD VaiTroNguoiThaoTac NVARCHAR(50) NULL;

IF COL_LENGTH('PhieuXuLyViPham', 'MaNguoiThaoTac') IS NULL
    ALTER TABLE PhieuXuLyViPham ADD MaNguoiThaoTac VARCHAR(50) NULL;
IF COL_LENGTH('PhieuXuLyViPham', 'VaiTroNguoiThaoTac') IS NULL
    ALTER TABLE PhieuXuLyViPham ADD VaiTroNguoiThaoTac NVARCHAR(50) NULL;

PRINT 'Da them/cap nhat cac cot audit nguoi thao tac.';
GO
