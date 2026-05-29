using System;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.DAL.Interfaces
{
    /// <summary>
    /// Unit of Work — quản lý transaction và các repository.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TaiKhoan>         TaiKhoans         { get; }
        IRepository<Admin>            Admins             { get; }
        IRepository<NhanVienQuanLy>   NhanVienQuanLys    { get; }
        IRepository<KhachThue>        KhachThues         { get; }
        IRepository<KhuVuc>           KhuVucs            { get; }
        IRepository<LoaiCanHo>        LoaiCanHos         { get; }
        IRepository<Toa>              Toas               { get; }
        IRepository<CanHo>            CanHos             { get; }
        IRepository<HinhAnhNha>       HinhAnhNhas        { get; }
        IRepository<TienNghi>         TienNghis          { get; }
        IRepository<TienNghiCuaCanHo> TienNghiCuaCanHos  { get; }
        IRepository<GiaDichVu>        GiaDichVus         { get; }
        IRepository<PhieuDatTruoc>    PhieuDatTruocs     { get; }
        IRepository<HopDong>          HopDongs           { get; }
        IRepository<GiaHanHopDong>    GiaHanHopDongs     { get; }
        IRepository<LoaiHoaDon>       LoaiHoaDons        { get; }
        IRepository<HoaDonThanhToan>  HoaDonThanhToans   { get; }
        IRepository<PhieuTraNha>      PhieuTraNhas       { get; }
        IRepository<PhieuXuLyViPham>  PhieuXuLyViPhams   { get; }

        int Complete();                     // SaveChanges
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
