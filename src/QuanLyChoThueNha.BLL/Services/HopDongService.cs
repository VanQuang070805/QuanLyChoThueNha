using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class HopDongService : BaseService<HopDong>
    {
        protected override IRepository<HopDong> Repo => _uow.HopDongs;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.HopDongs.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaHopDong, 2));
            return MaGenerator.Sinh("HD", max);
        }

        public bool KyHopDong(HopDong hopDong, string maPhieuDatTruoc, out string loi)
        {
            loi = string.Empty;
            maPhieuDatTruoc = string.IsNullOrWhiteSpace(maPhieuDatTruoc) ? null : maPhieuDatTruoc.Trim();

            if (!ValidationHelper.KhongRong(hopDong.MaCanHo, "Can ho", out loi)) return false;
            if (!ValidationHelper.KhongRong(hopDong.MaKhach, "Khach thue", out loi)) return false;
            if (!ValidationHelper.NgayHopLe(hopDong.NgayBatDau, hopDong.NgayKetThuc, out loi)) return false;
            if (!ValidationHelper.SoDuong(hopDong.GiaThueChot, "Gia thue chot", out loi)) return false;

            var canHo = _uow.CanHos.GetById(hopDong.MaCanHo);
            if (canHo == null) { loi = "Can ho khong ton tai. Vui long chon lai can ho."; return false; }

            var khach = _uow.KhachThues.GetById(hopDong.MaKhach);
            if (khach == null) { loi = "Khach thue khong ton tai. Vui long chon lai khach thue."; return false; }

            PhieuDatTruoc phieu = null;
            if (!string.IsNullOrEmpty(maPhieuDatTruoc))
            {
                phieu = _uow.PhieuDatTruocs.GetById(maPhieuDatTruoc);
                if (phieu == null) { loi = "Phieu dat truoc khong ton tai."; return false; }
                if (phieu.TrangThai != PhieuDatTruocService.ChoKy) { loi = "Phieu dat truoc nay chua du dieu kien ky hop dong."; return false; }
                if (_uow.HopDongs.Any(h => h.MaPhieuDatTruoc == maPhieuDatTruoc))
                {
                    loi = "Phieu dat truoc nay da duoc dung de tao hop dong.";
                    return false;
                }
                if (phieu.MaCanHo != hopDong.MaCanHo || phieu.MaKhach != hopDong.MaKhach)
                {
                    loi = "Can ho/khach thue tren hop dong phai trung voi phieu dat truoc.";
                    return false;
                }
                if (hopDong.TienCocChot < phieu.SoTienDatCoc)
                {
                    loi = "Tien coc chot phai lon hon hoac bang tien coc truoc.";
                    return false;
                }
            }
            else if (canHo.TinhTrang != "Trong")
            {
                loi = "Hop dong khong co phieu dat truoc chi tao duoc cho can ho dang Trong.";
                return false;
            }

            _uow.BeginTransaction();
            try
            {
                hopDong.MaHopDong = SinhMa();
                hopDong.MaPhieuDatTruoc = maPhieuDatTruoc;
                hopDong.TrangThai = "HieuLuc";
                hopDong.NgayTao = DateTime.Now;
                AuditHelper.GanNguoiThaoTac(hopDong);

                // ===== BỔ SUNG (Bước 1): tự động trừ tiền cọc trước vào tiền cọc hợp đồng =====
                // Nếu có phiếu đặt trước: số tiền đã cọc trước được ghi nhận là đã trừ,
                // khách chỉ còn phải nộp phần chênh lệch (TienCocConPhaiNop = TienCocChot - TienCocTruocDaTru).
                hopDong.TienCocTruocDaTru = phieu != null ? phieu.SoTienDatCoc : 0m;

                _uow.HopDongs.Add(hopDong);

                if (phieu != null)
                {
                    phieu.TrangThai = PhieuDatTruocService.DaKyHD;
                    _uow.PhieuDatTruocs.Update(phieu);
                }

                canHo.TinhTrang = "DangThue";
                _uow.CanHos.Update(canHo);

                _uow.Complete();
                _uow.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _uow.RollbackTransaction();
                var _inner = ex; while (_inner.InnerException != null) _inner = _inner.InnerException;
                loi = _inner.Message;
                return false;
            }
        }

        public override IEnumerable<HopDong> LayTatCa()
        {
            CapNhatHopDongHetHan();
            return base.LayTatCa();
        }

        public IEnumerable<HopDong> LayTheoTrangThai(string trangThai)
        {
            CapNhatHopDongHetHan();
            return Tim(h => h.TrangThai == trangThai);
        }

        public IEnumerable<HopDong> LayTheoKhach(string maKhach)
        {
            CapNhatHopDongHetHan();
            return Tim(h => h.MaKhach == maKhach);
        }

        public IEnumerable<HopDong> LayGanHetHan(int soNgay)
        {
            CapNhatHopDongHetHan();
            var ngayCanhBao = DateTime.Today.AddDays(soNgay);
            return Tim(h => h.TrangThai == "HieuLuc" && h.NgayKetThuc <= ngayCanhBao);
        }

        public IEnumerable<HopDong> LayCoTheLapPhieuTraNha()
        {
            CapNhatHopDongHetHan();
            var homNay = DateTime.Today;
            var hopDongDaTraNha = new HashSet<string>(
                _uow.PhieuTraNhas.GetAll()
                    .Where(p => !string.IsNullOrWhiteSpace(p.MaHopDong))
                    .Select(p => p.MaHopDong));

            return _uow.HopDongs
                .Find(h => h.TrangThai == "HieuLuc" &&
                           h.NgayBatDau <= homNay &&
                           h.NgayKetThuc >= homNay)
                .ToList()
                .Where(h =>
                {
                    if (hopDongDaTraNha.Contains(h.MaHopDong)) return false;
                    var canHo = _uow.CanHos.GetById(h.MaCanHo);
                    return canHo != null && canHo.TinhTrang == "DangThue";
                });
        }

        public string TrangThaiHienThi(HopDong hopDong)
        {
            if (hopDong == null) return string.Empty;
            if (hopDong.TrangThai == "HieuLuc" && hopDong.NgayKetThuc < DateTime.Today) return "HetHan";
            if (hopDong.TrangThai == "HieuLuc" && hopDong.NgayKetThuc <= DateTime.Today.AddDays(30)) return "SapHetHan";
            return hopDong.TrangThai;
        }

        public bool CapNhatTrangThai(string maHopDong, string trangThai, out string loi)
        {
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(maHopDong))
            {
                loi = "Vui long chon hop dong can cap nhat.";
                return false;
            }

            trangThai = (trangThai ?? string.Empty).Trim();
            if (trangThai != "HieuLuc" && trangThai != "HetHan" && trangThai != "DaHuy")
            {
                loi = "Trang thai hop dong khong hop le.";
                return false;
            }

            var hopDong = _uow.HopDongs.GetById(maHopDong);
            if (hopDong == null)
            {
                loi = "Hop dong khong ton tai.";
                return false;
            }

            hopDong.TrangThai = trangThai;
            AuditHelper.GanNguoiThaoTac(hopDong);
            _uow.HopDongs.Update(hopDong);
            _uow.Complete();
            return true;
        }

        private void CapNhatHopDongHetHan()
        {
            var expired = _uow.HopDongs
                .Find(h => h.TrangThai == "HieuLuc" && h.NgayKetThuc < DateTime.Today)
                .ToList();
            if (expired.Count == 0) return;

            foreach (var hd in expired)
            {
                hd.TrangThai = "HetHan";
                _uow.HopDongs.Update(hd);
            }
            _uow.Complete();
        }
    }
}
