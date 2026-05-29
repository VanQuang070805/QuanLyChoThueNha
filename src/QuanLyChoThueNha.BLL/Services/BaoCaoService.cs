using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.DAL.Repositories;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    // DTO đơn giản cho báo cáo (không cần project riêng)
    public class DoanhThuTheoThangDto
    {
        public string Thang    { get; set; }
        public decimal TongThu { get; set; }
        public int SoHoaDon    { get; set; }
    }

    public class TinhTrangCanHoDto
    {
        public string TinhTrang { get; set; }
        public int SoLuong      { get; set; }
    }

    /// <summary>TV5 phụ trách — Báo cáo & Thống kê.</summary>
    public class BaoCaoService
    {
        private readonly UnitOfWork _uow;

        public BaoCaoService()
        {
            _uow = new UnitOfWork();
        }

        // ── Doanh thu theo tháng ──────────────────────────────────────────
        public IEnumerable<DoanhThuTheoThangDto> DoanhThuTheoThang(int nam)
        {
            var hoaDons = _uow.HoaDonThanhToans
                .Find(h => h.TrangThai == "DaTra" && h.NgayThanhToan.HasValue &&
                           h.NgayThanhToan.Value.Year == nam)
                .ToList();

            return hoaDons
                .GroupBy(h => h.NgayThanhToan.Value.Month)
                .Select(g => new DoanhThuTheoThangDto
                {
                    Thang    = $"T{g.Key:D2}/{nam}",
                    TongThu  = g.Sum(h => h.SoTienDaTra),
                    SoHoaDon = g.Count()
                })
                .OrderBy(d => d.Thang)
                .ToList();
        }

        // ── Tình trạng căn hộ ─────────────────────────────────────────────
        public IEnumerable<TinhTrangCanHoDto> TinhTrangCanHo()
        {
            return _uow.CanHos.GetAll()
                .GroupBy(c => c.TinhTrang)
                .Select(g => new TinhTrangCanHoDto
                {
                    TinhTrang = g.Key,
                    SoLuong   = g.Count()
                })
                .ToList();
        }

        // ── Tổng quan ─────────────────────────────────────────────────────
        public int TongCanHo()          => _uow.CanHos.Count();
        public int CanHoDangThue()      => _uow.CanHos.Count(c => c.TinhTrang == "DangThue");
        public int CanHoTrong()         => _uow.CanHos.Count(c => c.TinhTrang == "Trong");
        public int HopDongHieuLuc()     => _uow.HopDongs.Count(h => h.TrangThai == "HieuLuc");
        public int HoaDonChuaTra()      => _uow.HoaDonThanhToans.Count(h => h.TrangThai == "ChuaTra");

        public decimal DoanhThuThang(int thang, int nam)
        {
            var ds = _uow.HoaDonThanhToans.Find(h =>
                h.TrangThai == "DaTra" &&
                h.NgayThanhToan.HasValue &&
                h.NgayThanhToan.Value.Month == thang &&
                h.NgayThanhToan.Value.Year  == nam);
            decimal tong = 0;
            foreach (var h in ds) tong += h.SoTienDaTra;
            return tong;
        }

        // ── Hợp đồng sắp hết hạn ─────────────────────────────────────────
        public IEnumerable<HopDong> HopDongSapHetHan(int soNgay = 30)
        {
            var ngayCanhBao = DateTime.Today.AddDays(soNgay);
            return _uow.HopDongs.Find(h =>
                h.TrangThai == "HieuLuc" && h.NgayKetThuc <= ngayCanhBao);
        }

        // ── Khách thuê nhiều nhất ─────────────────────────────────────────
        public IEnumerable<KhachThue> LayKhachThueHieuLuc()
        {
            var maKhachDangThue = new HashSet<string>();
            foreach (var hd in _uow.HopDongs.Find(h => h.TrangThai == "HieuLuc"))
                maKhachDangThue.Add(hd.MaKhach);

            var result = new List<KhachThue>();
            foreach (var ma in maKhachDangThue)
            {
                var kh = _uow.KhachThues.GetById(ma);
                if (kh != null) result.Add(kh);
            }
            return result;
        }
    }
}
