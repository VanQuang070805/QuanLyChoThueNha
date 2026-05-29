-- ============================================================
-- 03_stored_procedures.sql
-- Stored Procedures & Functions thống kê
-- Gọi từ C# qua EF6: context.Database.SqlQuery<T>()
-- ============================================================

USE QuanLyChoThueNha;
GO

-- ── sp_DoanhThuTheoThang ─────────────────────────────────────
-- Thống kê doanh thu theo từng tháng trong năm
-- Gọi: EXEC sp_DoanhThuTheoThang 2025
CREATE OR ALTER PROCEDURE sp_DoanhThuTheoThang
    @Nam INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        CAST(@Nam AS VARCHAR) + '-' + RIGHT('00' + CAST(MONTH(NgayThanhToan) AS VARCHAR), 2) AS Thang,
        SUM(SoTienDaTra)  AS TongThu,
        COUNT(*)          AS SoHoaDon
    FROM HoaDonThanhToan
    WHERE TrangThai = 'DaTra'
      AND NgayThanhToan IS NOT NULL
      AND YEAR(NgayThanhToan) = @Nam
    GROUP BY MONTH(NgayThanhToan)
    ORDER BY MONTH(NgayThanhToan);
END
GO

-- ── sp_TinhTrangCanHo ────────────────────────────────────────
-- Tổng hợp số lượng căn hộ theo từng tình trạng
CREATE OR ALTER PROCEDURE sp_TinhTrangCanHo
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TinhTrang, COUNT(*) AS SoLuong
    FROM CanHo
    GROUP BY TinhTrang;
END
GO

-- ── fn_SoDuCoc ───────────────────────────────────────────────
-- Tính số dư cọc còn lại của một hợp đồng
-- = TienCocChot - TienKhauTru (từ PhieuTraNha)
CREATE OR ALTER FUNCTION fn_SoDuCoc(@MaHopDong VARCHAR(50))
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Coc       DECIMAL(18,2) = 0;
    DECLARE @KhauTru   DECIMAL(18,2) = 0;

    SELECT @Coc = TienCocChot FROM HopDong WHERE MaHopDong = @MaHopDong;
    SELECT @KhauTru = ISNULL(TienKhauTru, 0)
        FROM PhieuTraNha WHERE MaHopDong = @MaHopDong;

    RETURN @Coc - @KhauTru;
END
GO

-- ── sp_HopDongSapHetHan ──────────────────────────────────────
-- Lấy hợp đồng sắp hết hạn trong N ngày tới
CREATE OR ALTER PROCEDURE sp_HopDongSapHetHan
    @SoNgay INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        hd.MaHopDong, hd.NgayKetThuc,
        kt.HoTen AS TenKhach, kt.SoCMND,
        ch.MaCanHo, t.TenToa,
        DATEDIFF(DAY, GETDATE(), hd.NgayKetThuc) AS SoNgayConLai
    FROM HopDong hd
    JOIN KhachThue kt ON hd.MaKhach = kt.MaKhach
    JOIN CanHo     ch ON hd.MaCanHo = ch.MaCanHo
    JOIN Toa        t ON ch.MaToa   = t.MaToa
    WHERE hd.TrangThai = 'HieuLuc'
      AND hd.NgayKetThuc <= DATEADD(DAY, @SoNgay, GETDATE())
    ORDER BY hd.NgayKetThuc;
END
GO

-- ── sp_HoaDonQuaHan ──────────────────────────────────────────
-- Lấy hóa đơn quá hạn chưa thanh toán
CREATE OR ALTER PROCEDURE sp_HoaDonQuaHan
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        hd.MaHoaDon, hd.KyThanhToan, hd.SoTienPhaiTra,
        hd.NgayDaoHan,
        kt.HoTen AS TenKhach,
        ch.MaCanHo
    FROM HoaDonThanhToan hd
    JOIN HopDong       hop ON hd.MaHopDong = hop.MaHopDong
    JOIN KhachThue     kt  ON hop.MaKhach  = kt.MaKhach
    JOIN CanHo         ch  ON hop.MaCanHo  = ch.MaCanHo
    WHERE hd.TrangThai = 'ChuaTra'
      AND hd.NgayDaoHan < GETDATE()
    ORDER BY hd.NgayDaoHan;
END
GO

PRINT 'Stored procedures & functions đã tạo thành công.';
GO
