using System.Data.Entity;
using System.Data.Entity.SqlServer;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.DAL
{
    /// <summary>
    /// DbContext EF6 — ánh xạ 19 bảng theo biểu đồ lớp.
    /// Chuỗi kết nối lấy từ App.config (key: QuanLyChoThueNhaContext).
    /// </summary>
    public class AppDbContext : DbContext
    {
        private static readonly System.Type SqlProviderServicesType = typeof(SqlProviderServices);

        public AppDbContext() : base("name=QuanLyChoThueNhaContext")
        {
            // Tắt lazy loading để tránh lỗi N+1
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        // ── Tài khoản & người dùng ──────────────────────────────
        public DbSet<TaiKhoan>         TaiKhoans         { get; set; }
        public DbSet<Admin>            Admins             { get; set; }
        public DbSet<NhanVienQuanLy>   NhanVienQuanLys    { get; set; }
        public DbSet<NhanVienQuyen>    NhanVienQuyens     { get; set; }
        public DbSet<KhachThue>        KhachThues         { get; set; }

        // ── Tài sản & danh mục ──────────────────────────────────
        public DbSet<KhuVuc>           KhuVucs            { get; set; }
        public DbSet<LoaiCanHo>        LoaiCanHos         { get; set; }
        public DbSet<Toa>              Toas               { get; set; }
        public DbSet<CanHo>            CanHos             { get; set; }
        public DbSet<HinhAnhNha>       HinhAnhNhas        { get; set; }
        public DbSet<TienNghi>         TienNghis          { get; set; }
        public DbSet<TienNghiCuaCanHo> TienNghiCuaCanHos  { get; set; }
        public DbSet<GiaDichVu>        GiaDichVus         { get; set; }

        // ── Hợp đồng & đặt cọc ─────────────────────────────────
        public DbSet<PhieuDatTruoc>    PhieuDatTruocs     { get; set; }
        public DbSet<HopDong>          HopDongs           { get; set; }
        public DbSet<GiaHanHopDong>    GiaHanHopDongs     { get; set; }

        // ── Thanh toán & vi phạm ────────────────────────────────
        public DbSet<LoaiHoaDon>       LoaiHoaDons        { get; set; }
        public DbSet<HoaDonThanhToan>  HoaDonThanhToans   { get; set; }
        public DbSet<PhieuTraNha>      PhieuTraNhas       { get; set; }
        public DbSet<PhieuXuLyViPham>  PhieuXuLyViPhams   { get; set; }
        public DbSet<EmailLog>         EmailLogs          { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Khóa kết hợp cho TienNghiCuaCanHo
            modelBuilder.Entity<TienNghiCuaCanHo>()
                .HasKey(t => new { t.MaCanHo, t.MaTienNghi });

            modelBuilder.Properties<decimal>()
                .Configure(c => c.HasPrecision(18, 2));

            // Tắt cascade delete mặc định (tránh lỗi FK nhiều bảng)
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();
        }
    }
}
