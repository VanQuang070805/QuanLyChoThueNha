USE QuanLyChoThueNha;
GO

IF OBJECT_ID('dbo.NhanVienQuyen', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhanVienQuyen (
        MaNhanVien   VARCHAR(50)  NOT NULL,
        MaChucNang   VARCHAR(50)  NOT NULL,
        DuocTruyCap  BIT          NOT NULL CONSTRAINT DF_NhanVienQuyen_DuocTruyCap DEFAULT 0,
        NgayCapNhat  DATETIME     NOT NULL CONSTRAINT DF_NhanVienQuyen_NgayCapNhat DEFAULT GETDATE(),
        CONSTRAINT PK_NhanVienQuyen PRIMARY KEY (MaNhanVien, MaChucNang),
        CONSTRAINT FK_NhanVienQuyen_NhanVien FOREIGN KEY (MaNhanVien)
            REFERENCES dbo.NhanVienQuanLy(MaNhanVien),
        CONSTRAINT CK_NhanVienQuyen_ChucNang CHECK
            (MaChucNang IN ('Dashboard','TaiSan','HopDong','ThanhToan','BaoCao'))
    );
END
GO

INSERT INTO dbo.NhanVienQuyen (MaNhanVien, MaChucNang, DuocTruyCap, NgayCapNhat)
SELECT nv.MaNhanVien, v.MaChucNang, v.DuocTruyCap, GETDATE()
FROM dbo.NhanVienQuanLy nv
CROSS APPLY (VALUES
    ('Dashboard', 1),
    ('TaiSan', 1),
    ('HopDong', 1),
    ('ThanhToan', 1),
    ('BaoCao', 0)
) v(MaChucNang, DuocTruyCap)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.NhanVienQuyen q
    WHERE q.MaNhanVien = nv.MaNhanVien
      AND q.MaChucNang = v.MaChucNang
);
GO
