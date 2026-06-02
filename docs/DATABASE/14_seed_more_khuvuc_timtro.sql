-- ============================================================
-- 14_seed_more_khuvuc_timtro.sql
-- Bo sung du lieu khu vuc cho bo loc "Tim tro".
-- Khong them bang/cot/cau truc moi. Script co the chay lai nhieu lan.
-- ============================================================

USE QuanLyChoThueNha;
GO

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV003')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV003','AD001',N'Khu Ba Đình',N'Ba Đình',N'Hà Nội',21.035916,105.814120);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV004')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV004','AD001',N'Khu Hoàn Kiếm',N'Hoàn Kiếm',N'Hà Nội',21.028511,105.854167);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV005')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV005','AD001',N'Khu Hai Bà Trưng',N'Hai Bà Trưng',N'Hà Nội',21.006944,105.857222);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV006')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV006','AD001',N'Khu Thanh Xuân',N'Thanh Xuân',N'Hà Nội',20.993134,105.812259);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV007')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV007','AD001',N'Khu Tây Hồ',N'Tây Hồ',N'Hà Nội',21.068123,105.823066);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV008')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV008','AD001',N'Khu Nam Từ Liêm',N'Nam Từ Liêm',N'Hà Nội',21.016886,105.765458);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV009')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV009','AD001',N'Khu Hà Đông',N'Hà Đông',N'Hà Nội',20.971208,105.778393);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV010')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV010','AD001',N'Khu Long Biên',N'Long Biên',N'Hà Nội',21.038377,105.888161);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV011')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV011','AD001',N'Khu Hoàng Mai',N'Hoàng Mai',N'Hà Nội',20.974388,105.868813);

IF NOT EXISTS (SELECT 1 FROM KhuVuc WHERE MaKhuVuc = 'KV012')
    INSERT INTO KhuVuc (MaKhuVuc, MaAdmin, TenKhuVuc, Quan, ThanhPho, ViDo, KinhDo)
    VALUES ('KV012','AD001',N'Khu Gia Lâm',N'Gia Lâm',N'Hà Nội',21.024625,105.941244);

PRINT N'Da bo sung khu vuc cho trang tim tro.';
GO
