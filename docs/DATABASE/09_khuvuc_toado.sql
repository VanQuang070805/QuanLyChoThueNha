-- ============================================================
-- 09_khuvuc_toado.sql
-- Them toa do khu vuc de loc nha tro theo ban kinh.
-- Script idempotent: co the chay lai nhieu lan.
-- ============================================================

USE QuanLyChoThueNha;
GO

IF COL_LENGTH('KhuVuc', 'ViDo') IS NULL
    ALTER TABLE KhuVuc ADD ViDo FLOAT NULL;
IF COL_LENGTH('KhuVuc', 'KinhDo') IS NULL
    ALTER TABLE KhuVuc ADD KinhDo FLOAT NULL;
GO

UPDATE KhuVuc
SET ViDo = 21.036237, KinhDo = 105.790583
WHERE MaKhuVuc = 'KV001' AND (ViDo IS NULL OR KinhDo IS NULL);

UPDATE KhuVuc
SET ViDo = 21.018072, KinhDo = 105.829949
WHERE MaKhuVuc = 'KV002' AND (ViDo IS NULL OR KinhDo IS NULL);

PRINT 'Da them/cap nhat toa do KhuVuc.';
GO
