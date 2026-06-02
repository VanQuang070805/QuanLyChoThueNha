using System.Data.Entity;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.DAL.Repositories;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.DAL.Repositories
{
    /// <summary>
    /// Unit of Work — một instance DbContext, nhiều repository dùng chung.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private DbContextTransaction _transaction;

        public UnitOfWork()
        {
            _context = new AppDbContext();
        }

        // ── Lazy-init repositories ──────────────────────────────────────────
        private IRepository<TaiKhoan>         _taiKhoans;
        private IRepository<Admin>            _admins;
        private IRepository<NhanVienQuanLy>   _nhanVienQuanLys;
        private IRepository<NhanVienQuyen>    _nhanVienQuyens;
        private IRepository<KhachThue>        _khachThues;
        private IRepository<KhuVuc>           _khuVucs;
        private IRepository<LoaiCanHo>        _loaiCanHos;
        private IRepository<Toa>              _toas;
        private IRepository<CanHo>            _canHos;
        private IRepository<HinhAnhNha>       _hinhAnhNhas;
        private IRepository<TienNghi>         _tienNghis;
        private IRepository<TienNghiCuaCanHo> _tienNghiCuaCanHos;
        private IRepository<GiaDichVu>        _giaDichVus;
        private IRepository<PhieuDatTruoc>    _phieuDatTruocs;
        private IRepository<HopDong>          _hopDongs;
        private IRepository<GiaHanHopDong>    _giaHanHopDongs;
        private IRepository<LoaiHoaDon>       _loaiHoaDons;
        private IRepository<HoaDonThanhToan>  _hoaDonThanhToans;
        private IRepository<PhieuTraNha>      _phieuTraNhas;
        private IRepository<PhieuXuLyViPham>  _phieuXuLyViPhams;
        private IRepository<EmailLog>         _emailLogs;

        public IRepository<TaiKhoan>         TaiKhoans         => _taiKhoans         ?? (_taiKhoans         = new Repository<TaiKhoan>(_context));
        public IRepository<Admin>            Admins             => _admins             ?? (_admins             = new Repository<Admin>(_context));
        public IRepository<NhanVienQuanLy>   NhanVienQuanLys    => _nhanVienQuanLys    ?? (_nhanVienQuanLys    = new Repository<NhanVienQuanLy>(_context));
        public IRepository<NhanVienQuyen>    NhanVienQuyens     => _nhanVienQuyens     ?? (_nhanVienQuyens     = new Repository<NhanVienQuyen>(_context));
        public IRepository<KhachThue>        KhachThues         => _khachThues         ?? (_khachThues         = new Repository<KhachThue>(_context));
        public IRepository<KhuVuc>           KhuVucs            => _khuVucs            ?? (_khuVucs            = new Repository<KhuVuc>(_context));
        public IRepository<LoaiCanHo>        LoaiCanHos         => _loaiCanHos         ?? (_loaiCanHos         = new Repository<LoaiCanHo>(_context));
        public IRepository<Toa>              Toas               => _toas               ?? (_toas               = new Repository<Toa>(_context));
        public IRepository<CanHo>            CanHos             => _canHos             ?? (_canHos             = new Repository<CanHo>(_context));
        public IRepository<HinhAnhNha>       HinhAnhNhas        => _hinhAnhNhas        ?? (_hinhAnhNhas        = new Repository<HinhAnhNha>(_context));
        public IRepository<TienNghi>         TienNghis          => _tienNghis          ?? (_tienNghis          = new Repository<TienNghi>(_context));
        public IRepository<TienNghiCuaCanHo> TienNghiCuaCanHos  => _tienNghiCuaCanHos  ?? (_tienNghiCuaCanHos  = new Repository<TienNghiCuaCanHo>(_context));
        public IRepository<GiaDichVu>        GiaDichVus         => _giaDichVus         ?? (_giaDichVus         = new Repository<GiaDichVu>(_context));
        public IRepository<PhieuDatTruoc>    PhieuDatTruocs     => _phieuDatTruocs     ?? (_phieuDatTruocs     = new Repository<PhieuDatTruoc>(_context));
        public IRepository<HopDong>          HopDongs           => _hopDongs           ?? (_hopDongs           = new Repository<HopDong>(_context));
        public IRepository<GiaHanHopDong>    GiaHanHopDongs     => _giaHanHopDongs     ?? (_giaHanHopDongs     = new Repository<GiaHanHopDong>(_context));
        public IRepository<LoaiHoaDon>       LoaiHoaDons        => _loaiHoaDons        ?? (_loaiHoaDons        = new Repository<LoaiHoaDon>(_context));
        public IRepository<HoaDonThanhToan>  HoaDonThanhToans   => _hoaDonThanhToans   ?? (_hoaDonThanhToans   = new Repository<HoaDonThanhToan>(_context));
        public IRepository<PhieuTraNha>      PhieuTraNhas       => _phieuTraNhas       ?? (_phieuTraNhas       = new Repository<PhieuTraNha>(_context));
        public IRepository<PhieuXuLyViPham>  PhieuXuLyViPhams   => _phieuXuLyViPhams   ?? (_phieuXuLyViPhams   = new Repository<PhieuXuLyViPham>(_context));
        public IRepository<EmailLog>         EmailLogs          => _emailLogs          ?? (_emailLogs          = new Repository<EmailLog>(_context));

        // ── Lưu & Transaction ──────────────────────────────────────────────
        public int Complete() => _context.SaveChanges();

        public void BeginTransaction() => _transaction = _context.Database.BeginTransaction();

        public void CommitTransaction()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
            _transaction = null;
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
