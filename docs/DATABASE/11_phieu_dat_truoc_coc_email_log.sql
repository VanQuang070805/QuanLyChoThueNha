USE QuanLyChoThueNha;
GO

DECLARE @ConstraintName SYSNAME;
DECLARE @Sql NVARCHAR(MAX);

DECLARE TrangThaiConstraints CURSOR LOCAL FAST_FORWARD FOR
SELECT cc.name
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('PhieuDatTruoc')
  AND cc.definition LIKE '%TrangThai%';

OPEN TrangThaiConstraints;
FETCH NEXT FROM TrangThaiConstraints INTO @ConstraintName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'ALTER TABLE PhieuDatTruoc DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
    EXEC sp_executesql @Sql;
    FETCH NEXT FROM TrangThaiConstraints INTO @ConstraintName;
END
CLOSE TrangThaiConstraints;
DEALLOCATE TrangThaiConstraints;
GO

IF COL_LENGTH('PhieuDatTruoc', 'TrangThai') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PhieuDatTruoc_TrangThai')
BEGIN
    ALTER TABLE PhieuDatTruoc
        ADD CONSTRAINT CK_PhieuDatTruoc_TrangThai
        CHECK (TrangThai IN ('ChoThanhToanCoc','DaThanhToanCoc','ChoKy','DaKyHD','Huy','HetHan'));
END
GO

IF OBJECT_ID('EmailLog', 'U') IS NULL
BEGIN
    CREATE TABLE EmailLog (
        MaEmailLog          VARCHAR(50)    NOT NULL PRIMARY KEY,
        MaPhieuDatTruoc     VARCHAR(50)    NULL REFERENCES PhieuDatTruoc(MaPhieuDatTruoc),
        MaTaiKhoan          VARCHAR(50)    NULL REFERENCES TaiKhoan(MaTaiKhoan),
        EmailNguoiNhan      NVARCHAR(150)  NOT NULL,
        LoaiEmail           NVARCHAR(100)  NOT NULL,
        TieuDe              NVARCHAR(255)  NOT NULL,
        TrangThai           NVARCHAR(50)   NOT NULL CHECK (TrangThai IN ('ThanhCong','ThatBai')),
        ThongBao            NVARCHAR(1000) NULL,
        NgayGui             DATETIME       NOT NULL DEFAULT GETDATE(),
        MaNguoiThaoTac      VARCHAR(50)    NULL,
        VaiTroNguoiThaoTac  NVARCHAR(50)   NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmailLog_Phieu' AND object_id = OBJECT_ID('EmailLog'))
    CREATE INDEX IX_EmailLog_Phieu ON EmailLog(MaPhieuDatTruoc);
GO

PRINT N'Da chuan hoa trang thai phieu dat truoc va them EmailLog.';
GO
