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

    public class BaoCaoHangMucDto
    {
        public string Ten { get; set; }
        public decimal GiaTri { get; set; }
        public int SoLuong { get; set; }
    }

    public class CongNoDto
    {
        public string MaHoaDon { get; set; }
        public string MaHopDong { get; set; }
        public string MaCanHo { get; set; }
        public string MaKhach { get; set; }
        public string TenKhach { get; set; }
        public string KyThanhToan { get; set; }
        public decimal SoTienConNo { get; set; }
        public DateTime NgayDaoHan { get; set; }
        public string TrangThai { get; set; }
    }

    public class HopDongCanhBaoDto
    {
        public string MaHopDong { get; set; }
        public string MaCanHo { get; set; }
        public string MaKhach { get; set; }
        public string TenKhach { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoNgayConLai { get; set; }
        public string TrangThai { get; set; }
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
            var hoaDons = HoaDonDaTraTrongNam(nam);

            var data = hoaDons
                .GroupBy(h => h.NgayThanhToan.Value.Month)
                .Select(g => new DoanhThuTheoThangDto
                {
                    Thang    = $"T{g.Key:D2}/{nam}",
                    TongThu  = g.Sum(h => h.SoTienDaTra),
                    SoHoaDon = g.Count()
                })
                .OrderBy(d => d.Thang)
                .ToList();

            for (var month = 1; month <= 12; month++)
            {
                var label = $"T{month:D2}/{nam}";
                if (data.All(x => x.Thang != label))
                    data.Add(new DoanhThuTheoThangDto { Thang = label, TongThu = 0, SoHoaDon = 0 });
            }

            return data.OrderBy(x => x.Thang).ToList();
        }

        public IEnumerable<DoanhThuTheoThangDto> CongNoTheoThang(int nam)
        {
            var hoaDons = _uow.HoaDonThanhToans.GetAll()
                .Where(h => h.NgayDaoHan.Year == nam &&
                            (h.TrangThai == "ChuaTra" || h.TrangThai == "TraThieu" || h.TrangThai == "QuaHan"))
                .ToList();

            var data = hoaDons
                .GroupBy(h => h.NgayDaoHan.Month)
                .Select(g => new DoanhThuTheoThangDto
                {
                    Thang = $"T{g.Key:D2}/{nam}",
                    TongThu = g.Sum(h => Math.Max(0, h.SoTienPhaiTra - h.SoTienDaTra)),
                    SoHoaDon = g.Count()
                })
                .ToList();

            for (var month = 1; month <= 12; month++)
            {
                var label = $"T{month:D2}/{nam}";
                if (data.All(x => x.Thang != label))
                    data.Add(new DoanhThuTheoThangDto { Thang = label, TongThu = 0, SoHoaDon = 0 });
            }

            return data.OrderBy(x => x.Thang).ToList();
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
        public int HopDongMoiTrongThang(int thang, int nam) => _uow.HopDongs.Count(h => h.NgayTao.Month == thang && h.NgayTao.Year == nam);
        public decimal TongCongNo() => _uow.HoaDonThanhToans.GetAll()
            .Where(h => h.TrangThai == "ChuaTra" || h.TrangThai == "TraThieu" || h.TrangThai == "QuaHan")
            .Sum(h => Math.Max(0, h.SoTienPhaiTra - h.SoTienDaTra));

        public double TyLeLapDay()
        {
            var tong = TongCanHo();
            if (tong == 0) return 0;
            return Math.Round(CanHoDangThue() * 100.0 / tong, 1);
        }

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

        public IEnumerable<HopDongCanhBaoDto> HopDongCanhBao(int soNgay = 45)
        {
            var khachMap = _uow.KhachThues.GetAll().ToDictionary(k => k.MaKhach, k => k.HoTen);
            return HopDongSapHetHan(soNgay)
                .OrderBy(h => h.NgayKetThuc)
                .Select(h => new HopDongCanhBaoDto
                {
                    MaHopDong = h.MaHopDong,
                    MaCanHo = h.MaCanHo,
                    MaKhach = h.MaKhach,
                    TenKhach = khachMap.ContainsKey(h.MaKhach) ? khachMap[h.MaKhach] : h.MaKhach,
                    NgayKetThuc = h.NgayKetThuc,
                    SoNgayConLai = Math.Max(0, (h.NgayKetThuc.Date - DateTime.Today).Days),
                    TrangThai = h.NgayKetThuc <= DateTime.Today.AddDays(15) ? "Can xu ly" : "Sap het han"
                })
                .ToList();
        }

        public IEnumerable<CongNoDto> CongNoQuaHan()
        {
            var hopDongMap = _uow.HopDongs.GetAll().ToDictionary(h => h.MaHopDong);
            var khachMap = _uow.KhachThues.GetAll().ToDictionary(k => k.MaKhach, k => k.HoTen);
            return _uow.HoaDonThanhToans.GetAll()
                .Where(h => h.TrangThai == "ChuaTra" || h.TrangThai == "TraThieu" || h.TrangThai == "QuaHan")
                .OrderBy(h => h.NgayDaoHan)
                .Select(h =>
                {
                    HopDong hopDong;
                    hopDongMap.TryGetValue(h.MaHopDong, out hopDong);
                    var maKhach = hopDong == null ? string.Empty : hopDong.MaKhach;
                    return new CongNoDto
                    {
                        MaHoaDon = h.MaHoaDon,
                        MaHopDong = h.MaHopDong,
                        MaCanHo = hopDong == null ? string.Empty : hopDong.MaCanHo,
                        MaKhach = maKhach,
                        TenKhach = khachMap.ContainsKey(maKhach) ? khachMap[maKhach] : maKhach,
                        KyThanhToan = h.KyThanhToan,
                        SoTienConNo = Math.Max(0, h.SoTienPhaiTra - h.SoTienDaTra),
                        NgayDaoHan = h.NgayDaoHan,
                        TrangThai = h.TrangThai
                    };
                })
                .ToList();
        }

        public IEnumerable<BaoCaoHangMucDto> LoaiCanHoDuocThueNhieuNhat(int top = 10)
        {
            var canHoMap = _uow.CanHos.GetAll().ToDictionary(c => c.MaCanHo);
            var loaiMap = _uow.LoaiCanHos.GetAll().ToDictionary(l => l.MaLoai, l => l.TenLoai);
            return _uow.HopDongs.GetAll()
                .Where(h => h.TrangThai == "HieuLuc" || h.TrangThai == "HetHan")
                .Select(h =>
                {
                    CanHo canHo;
                    if (!canHoMap.TryGetValue(h.MaCanHo, out canHo)) return null;
                    var tenLoai = loaiMap.ContainsKey(canHo.MaLoai) ? loaiMap[canHo.MaLoai] : canHo.MaLoai;
                    return tenLoai;
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x)
                .Select(g => new BaoCaoHangMucDto { Ten = g.Key, SoLuong = g.Count(), GiaTri = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .Take(top)
                .ToList();
        }

        public IEnumerable<BaoCaoHangMucDto> TinhTrangTheoKhuVuc()
        {
            var toas = _uow.Toas.GetAll().ToDictionary(t => t.MaToa);
            var khuVucs = _uow.KhuVucs.GetAll().ToDictionary(k => k.MaKhuVuc, k => k.TenKhuVuc);
            return _uow.CanHos.GetAll()
                .Select(c =>
                {
                    Toa toa;
                    if (!toas.TryGetValue(c.MaToa, out toa)) return null;
                    var tenKhuVuc = khuVucs.ContainsKey(toa.MaKhuVuc) ? khuVucs[toa.MaKhuVuc] : toa.MaKhuVuc;
                    return new { Ten = tenKhuVuc + " - " + c.TinhTrang };
                })
                .Where(x => x != null)
                .GroupBy(x => x.Ten)
                .Select(g => new BaoCaoHangMucDto { Ten = g.Key, SoLuong = g.Count(), GiaTri = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ToList();
        }

        public IEnumerable<BaoCaoHangMucDto> TopCanHoDoanhThu(int nam, int top = 5)
        {
            var hopDongMap = _uow.HopDongs.GetAll().ToDictionary(h => h.MaHopDong);
            return HoaDonDaTraTrongNam(nam)
                .Select(h =>
                {
                    HopDong hopDong;
                    hopDongMap.TryGetValue(h.MaHopDong, out hopDong);
                    return new { MaCanHo = hopDong == null ? h.MaHopDong : hopDong.MaCanHo, h.SoTienDaTra };
                })
                .GroupBy(x => x.MaCanHo)
                .Select(g => new BaoCaoHangMucDto
                {
                    Ten = g.Key,
                    GiaTri = g.Sum(x => x.SoTienDaTra),
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.GiaTri)
                .Take(top)
                .ToList();
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

        private List<HoaDonThanhToan> HoaDonDaTraTrongNam(int nam)
        {
            return _uow.HoaDonThanhToans
                .Find(h => h.TrangThai == "DaTra" && h.NgayThanhToan.HasValue &&
                           h.NgayThanhToan.Value.Year == nam)
                .ToList();
        }
    }
}
