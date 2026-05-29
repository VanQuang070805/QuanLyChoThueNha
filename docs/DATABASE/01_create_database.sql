-- ============================================================
-- 01_create_database.sql
-- Tạo CSDL QuanLyChoThueNha với 19 bảng theo biểu đồ lớp
-- Chạy file này đầu tiên trong SSMS
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QuanLyChoThueNha')
    DROP DATABASE QuanLyChoThueNha;
GO

CREATE DATABASE QuanLyChoThueNha
    COLLATE Vietnamese_CI_AS;
GO

USE QuanLyChoThueNha;
GO

-- ── 1. TaiKhoan ─────────────────────────────────────────────
CREATE TABLE TaiKhoan (
    MaTaiKhoan   VARCHAR(50)   NOT NULL PRIMARY KEY,
    TenDangNhap  NVARCHAR(100) NOT NULL UNIQUE,
    MatKhauHash  NVARCHAR(255) NOT NULL,
    Email        NVARCHAR(150) NULL,
    SoDienThoai  VARCHAR(20)   NULL,
    VaiTro       NVARCHAR(50)  NOT NULL CHECK (VaiTro IN ('Admin','NhanVien','KhachThue')),
    TrangThai    BIT           NOT NULL DEFAULT 1,
    NgayTao      DATETIME      NOT NULL DEFAULT GETDATE()
);

-- ── 2. Admin ────────────────────────────────────────────────
CREATE TABLE Admin (
    MaAdmin      VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaTaiKhoan   VARCHAR(50)   NOT NULL REFERENCES TaiKhoan(MaTaiKhoan),
    HoTen        NVARCHAR(150) NOT NULL
);

-- ── 3. NhanVienQuanLy ───────────────────────────────────────
CREATE TABLE NhanVienQuanLy (
    MaNhanVien   VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaTaiKhoan   VARCHAR(50)   NOT NULL REFERENCES TaiKhoan(MaTaiKhoan),
    HoTen        NVARCHAR(150) NOT NULL,
    NgayVaoLam   DATETIME      NOT NULL DEFAULT GETDATE()
);

-- ── 4. KhachThue ────────────────────────────────────────────
CREATE TABLE KhachThue (
    MaKhach      VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaTaiKhoan   VARCHAR(50)   NOT NULL REFERENCES TaiKhoan(MaTaiKhoan),
    HoTen        NVARCHAR(150) NOT NULL,
    SoCMND       VARCHAR(20)   NULL UNIQUE,
    DiaChi       NVARCHAR(255) NULL,
    NgaySinh     DATE          NULL
);

-- ── 5. KhuVuc ───────────────────────────────────────────────
CREATE TABLE KhuVuc (
    MaKhuVuc     VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaAdmin      VARCHAR(50)   NULL REFERENCES Admin(MaAdmin),
    TenKhuVuc    NVARCHAR(150) NOT NULL,
    Quan         NVARCHAR(100) NULL,
    ThanhPho     NVARCHAR(100) NULL
);

-- ── 6. LoaiCanHo ────────────────────────────────────────────
CREATE TABLE LoaiCanHo (
    MaLoai       VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaAdmin      VARCHAR(50)   NULL REFERENCES Admin(MaAdmin),
    TenLoai      NVARCHAR(100) NOT NULL,
    MoTa         NVARCHAR(500) NULL
);

-- ── 7. Toa ──────────────────────────────────────────────────
CREATE TABLE Toa (
    MaToa        VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaKhuVuc     VARCHAR(50)   NOT NULL REFERENCES KhuVuc(MaKhuVuc),
    TenToa       NVARCHAR(100) NOT NULL,
    DiaChi       NVARCHAR(255) NULL,
    SoTang       INT           NOT NULL DEFAULT 1,
    MoTa         NVARCHAR(500) NULL
);

-- ── 8. CanHo ────────────────────────────────────────────────
CREATE TABLE CanHo (
    MaCanHo          VARCHAR(50)    NOT NULL PRIMARY KEY,
    MaToa            VARCHAR(50)    NOT NULL REFERENCES Toa(MaToa),
    MaLoai           VARCHAR(50)    NOT NULL REFERENCES LoaiCanHo(MaLoai),
    MaNhanVien       VARCHAR(50)    NULL REFERENCES NhanVienQuanLy(MaNhanVien),
    DienTich         FLOAT          NOT NULL DEFAULT 0,
    GiaThueNiemYet   DECIMAL(18,2)  NOT NULL DEFAULT 0,
    TienCocNiemYet   DECIMAL(18,2)  NOT NULL DEFAULT 0,
    SoCanHo          INT            NOT NULL DEFAULT 0,
    TangSo           INT            NOT NULL DEFAULT 1,
    TinhTrang        NVARCHAR(50)   NOT NULL DEFAULT 'Trong'
                     CHECK (TinhTrang IN ('Trong','DaDatCoc','DangThue','BaoTri')),
    MoTa             NVARCHAR(1000) NULL,
    NgayTao          DATETIME       NOT NULL DEFAULT GETDATE()
);

-- ── 9. HinhAnhNha ───────────────────────────────────────────
CREATE TABLE HinhAnhNha (
    MaHinhAnh    VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaCanHo      VARCHAR(50)   NOT NULL REFERENCES CanHo(MaCanHo),
    DuongDanAnh  NVARCHAR(500) NOT NULL,
    MoTa         NVARCHAR(255) NULL,
    NgayTaiLen   DATETIME      NOT NULL DEFAULT GETDATE()
);

-- ── 10. TienNghi ────────────────────────────────────────────
CREATE TABLE TienNghi (
    MaTienNghi   VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaAdmin      VARCHAR(50)   NULL REFERENCES Admin(MaAdmin),
    TenTienNghi  NVARCHAR(100) NOT NULL,
    MoTa         NVARCHAR(255) NULL
);

-- ── 11. TienNghiCuaCanHo (khóa kết hợp) ────────────────────
CREATE TABLE TienNghiCuaCanHo (
    MaCanHo      VARCHAR(50)   NOT NULL REFERENCES CanHo(MaCanHo),
    MaTienNghi   VARCHAR(50)   NOT NULL REFERENCES TienNghi(MaTienNghi),
    GhiChu       NVARCHAR(255) NULL,
    CONSTRAINT PK_TienNghiCuaCanHo PRIMARY KEY (MaCanHo, MaTienNghi)
);

-- ── 12. GiaDichVu ───────────────────────────────────────────
CREATE TABLE GiaDichVu (
    MaGiaDichVu      VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaToa            VARCHAR(50)   NOT NULL REFERENCES Toa(MaToa),
    GiaDien          DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiaNuoc          DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiaDichVuChung   DECIMAL(18,2) NOT NULL DEFAULT 0,
    NgayApDung       DATE          NOT NULL,
    NgayKetThuc      DATE          NULL,
    DangApDung       BIT           NOT NULL DEFAULT 1
);

-- ── 13. PhieuDatTruoc ───────────────────────────────────────
CREATE TABLE PhieuDatTruoc (
    MaPhieuDatTruoc      VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaCanHo              VARCHAR(50)   NOT NULL REFERENCES CanHo(MaCanHo),
    MaKhach              VARCHAR(50)   NOT NULL REFERENCES KhachThue(MaKhach),
    MaNhanVien           VARCHAR(50)   NULL REFERENCES NhanVienQuanLy(MaNhanVien),
    SoTienDatCoc         DECIMAL(18,2) NOT NULL,
    NgayDatCoc           DATETIME      NOT NULL DEFAULT GETDATE(),
    NgayHetHan           DATETIME      NOT NULL,
    TrangThai            NVARCHAR(50)  NOT NULL DEFAULT 'ChoKy'
                         CHECK (TrangThai IN ('ChoKy','DaKyHD','Huy','HetHan')),
    PhuongThucThanhToan  NVARCHAR(50)  NULL,
    GhiChu               NVARCHAR(500) NULL
);

-- ── 14. HopDong ─────────────────────────────────────────────
CREATE TABLE HopDong (
    MaHopDong        VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaPhieuDatTruoc  VARCHAR(50)   NULL REFERENCES PhieuDatTruoc(MaPhieuDatTruoc),
    MaCanHo          VARCHAR(50)   NOT NULL REFERENCES CanHo(MaCanHo),
    MaKhach          VARCHAR(50)   NOT NULL REFERENCES KhachThue(MaKhach),
    MaNhanVien       VARCHAR(50)   NULL REFERENCES NhanVienQuanLy(MaNhanVien),
    NgayBatDau       DATE          NOT NULL,
    NgayKetThuc      DATE          NOT NULL,
    GiaThueChot      DECIMAL(18,2) NOT NULL,
    TienCocChot      DECIMAL(18,2) NOT NULL DEFAULT 0,
    -- BO SUNG: tien coc truoc (tu PhieuDatTruoc) da duoc tru vao tien coc hop dong
    TienCocTruocDaTru DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai        NVARCHAR(50)  NOT NULL DEFAULT 'HieuLuc'
                     CHECK (TrangThai IN ('HieuLuc','HetHan','DaHuy')),
    GhiChu           NVARCHAR(1000) NULL,
    NgayTao          DATETIME      NOT NULL DEFAULT GETDATE()
);

-- ── 15. LoaiHoaDon ──────────────────────────────────────────
CREATE TABLE LoaiHoaDon (
    MaLoaiHoaDon VARCHAR(50)   NOT NULL PRIMARY KEY,
    TenLoai      NVARCHAR(100) NOT NULL,
    MoTa         NVARCHAR(255) NULL
);

-- ── 16. HoaDonThanhToan ─────────────────────────────────────
CREATE TABLE HoaDonThanhToan (
    MaHoaDon             VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaHopDong            VARCHAR(50)   NOT NULL REFERENCES HopDong(MaHopDong),
    MaLoaiHoaDon         VARCHAR(50)   NOT NULL REFERENCES LoaiHoaDon(MaLoaiHoaDon),
    MaNhanVienThu        VARCHAR(50)   NULL REFERENCES NhanVienQuanLy(MaNhanVien),
    MaViPham             VARCHAR(50)   NULL,  -- FK sau khi tạo bảng PhieuXuLyViPham
    KyThanhToan          NVARCHAR(20)  NULL,
    -- BO SUNG (Buoc 3): luu chi so dien/nuoc de truy vet. Tieu thu = Moi - Cu.
    ChiSoDienCu          FLOAT         NOT NULL DEFAULT 0,
    ChiSoDienMoi         FLOAT         NOT NULL DEFAULT 0,
    ChiSoNuocCu          FLOAT         NOT NULL DEFAULT 0,
    ChiSoNuocMoi         FLOAT         NOT NULL DEFAULT 0,
    SoTienPhaiTra        DECIMAL(18,2) NOT NULL,
    SoTienDaTra          DECIMAL(18,2) NOT NULL DEFAULT 0,
    NgayDaoHan           DATE          NOT NULL,
    NgayThanhToan        DATETIME      NULL,
    TrangThai            NVARCHAR(50)  NOT NULL DEFAULT 'ChuaTra'
                         CHECK (TrangThai IN ('ChuaTra','DaTra','TraThieu','QuaHan')),
    PhuongThucThanhToan  NVARCHAR(50)  NULL
);

-- ── 17. GiaHanHopDong ───────────────────────────────────────
CREATE TABLE GiaHanHopDong (
    MaGiaHan         VARCHAR(50)  NOT NULL PRIMARY KEY,
    MaHopDong        VARCHAR(50)  NOT NULL REFERENCES HopDong(MaHopDong),
    MaNhanVien       VARCHAR(50)  NULL REFERENCES NhanVienQuanLy(MaNhanVien),
    NgayKetThucCu    DATE         NOT NULL,
    NgayKetThucMoi   DATE         NOT NULL,
    TrangThai        NVARCHAR(50) NOT NULL DEFAULT 'ChoXetDuyet'
                     CHECK (TrangThai IN ('ChoXetDuyet','ChapThuan','TuChoi')),
    NgayYeuCau       DATETIME     NOT NULL DEFAULT GETDATE(),
    NgayDuyet        DATETIME     NULL
);

-- ── 18. PhieuTraNha ─────────────────────────────────────────
CREATE TABLE PhieuTraNha (
    MaPhieu          VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaHopDong        VARCHAR(50)   NOT NULL UNIQUE REFERENCES HopDong(MaHopDong),
    MaNhanVien       VARCHAR(50)   NULL REFERENCES NhanVienQuanLy(MaNhanVien),
    NgayTra          DATE          NOT NULL,
    TinhTrangNha     NVARCHAR(500) NULL,
    TienHoanCoc      DECIMAL(18,2) NOT NULL DEFAULT 0,
    TienKhauTru      DECIMAL(18,2) NOT NULL DEFAULT 0,
    GhiChu           NVARCHAR(500) NULL
);

-- ── 19. PhieuXuLyViPham ─────────────────────────────────────
CREATE TABLE PhieuXuLyViPham (
    MaViPham         VARCHAR(50)   NOT NULL PRIMARY KEY,
    MaHopDong        VARCHAR(50)   NOT NULL REFERENCES HopDong(MaHopDong),
    MaPhieuTraNha    VARCHAR(50)   NULL REFERENCES PhieuTraNha(MaPhieu),
    MaNhanVien       VARCHAR(50)   NULL REFERENCES NhanVienQuanLy(MaNhanVien),
    LoaiViPham       NVARCHAR(100) NULL,
    MoTa             NVARCHAR(500) NULL,
    PhiBoiThuong     DECIMAL(18,2) NOT NULL DEFAULT 0,
    TruVaoCoc        BIT           NOT NULL DEFAULT 0,
    TinhTrang        NVARCHAR(50)  NOT NULL DEFAULT 'ChoXuLy'
                     CHECK (TinhTrang IN ('ChoXuLy','DaKhauTru','DaThanhToan')),
    NgayGhiNhan      DATETIME      NOT NULL DEFAULT GETDATE()
);

-- Thêm FK ngược: HoaDonThanhToan.MaViPham → PhieuXuLyViPham
ALTER TABLE HoaDonThanhToan
    ADD CONSTRAINT FK_HoaDon_ViPham
    FOREIGN KEY (MaViPham) REFERENCES PhieuXuLyViPham(MaViPham);

-- ── Index tối ưu truy vấn ────────────────────────────────────
CREATE INDEX IX_CanHo_TinhTrang  ON CanHo(TinhTrang);
CREATE INDEX IX_HopDong_TrangThai ON HopDong(TrangThai);
CREATE INDEX IX_HopDong_MaKhach  ON HopDong(MaKhach);
CREATE INDEX IX_HoaDon_TrangThai ON HoaDonThanhToan(TrangThai);
CREATE INDEX IX_HoaDon_NgayDaoHan ON HoaDonThanhToan(NgayDaoHan);
CREATE INDEX IX_TaiKhoan_VaiTro  ON TaiKhoan(VaiTro);

PRINT 'Tạo CSDL thành công — 19 bảng.';
GO
