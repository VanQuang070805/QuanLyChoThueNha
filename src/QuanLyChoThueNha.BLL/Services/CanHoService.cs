using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class CanHoService : BaseService<CanHo>
    {
        protected override IRepository<CanHo> Repo => _uow.CanHos;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.CanHos.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaCanHo, 2));
            return MaGenerator.Sinh("CH", max);
        }

        private static bool LaTrangThaiHeThong(string tinhTrang)
        {
            return tinhTrang == "DaDatCoc" || tinhTrang == "DangThue";
        }

        private string TinhTrangTheoNghiepVu(CanHo canHo)
        {
            if (canHo == null) return null;

            var coHopDongHieuLuc = _uow.HopDongs.Any(h =>
                h.MaCanHo == canHo.MaCanHo &&
                h.TrangThai == "HieuLuc" &&
                h.NgayBatDau <= DateTime.Today &&
                h.NgayKetThuc >= DateTime.Today);
            if (coHopDongHieuLuc) return "DangThue";

            var coPhieuDatTruocMo = _uow.PhieuDatTruocs.Any(p =>
                p.MaCanHo == canHo.MaCanHo &&
                p.NgayHetHan >= DateTime.Now &&
                (p.TrangThai == PhieuDatTruocService.ChoThanhToanCoc ||
                 p.TrangThai == PhieuDatTruocService.DaThanhToanCoc ||
                 p.TrangThai == PhieuDatTruocService.ChoKy));
            if (coPhieuDatTruocMo) return "DaDatCoc";

            return LaTrangThaiHeThong(canHo.TinhTrang) ? "Trong" : canHo.TinhTrang;
        }

        private void DongBoTinhTrangTheoNghiepVu()
        {
            var changed = false;
            foreach (var canHo in _uow.CanHos.GetAll())
            {
                var tinhTrangDung = TinhTrangTheoNghiepVu(canHo);
                if (!string.IsNullOrWhiteSpace(tinhTrangDung) && canHo.TinhTrang != tinhTrangDung)
                {
                    canHo.TinhTrang = tinhTrangDung;
                    _uow.CanHos.Update(canHo);
                    changed = true;
                }
            }
            if (changed) _uow.Complete();
        }

        public override IEnumerable<CanHo> LayTatCa()
        {
            DongBoTinhTrangTheoNghiepVu();
            return base.LayTatCa();
        }

        public override CanHo LayTheoMa(object ma)
        {
            DongBoTinhTrangTheoNghiepVu();
            return base.LayTheoMa(ma);
        }

        public bool Them(CanHo canHo, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(canHo.MaToa, "Toa", out loi)) return false;
            if (!ValidationHelper.KhongRong(canHo.MaLoai, "Loai can ho", out loi)) return false;
            if (_uow.Toas.GetById(canHo.MaToa) == null) { loi = "Toa nha khong ton tai."; return false; }
            if (_uow.LoaiCanHos.GetById(canHo.MaLoai) == null) { loi = "Loai can ho khong ton tai."; return false; }
            if (canHo.DienTich <= 0) { loi = "Dien tich phai lon hon 0."; return false; }
            if (canHo.TangSo <= 0) { loi = "Tang so phai lon hon 0."; return false; }
            if (!ValidationHelper.SoDuong(canHo.GiaThueNiemYet, "Gia thue", out loi)) return false;
            if (canHo.TienCocNiemYet < 0) { loi = "Tien coc khong duoc am."; return false; }

            canHo.MaCanHo = SinhMa();
            canHo.NgayTao = DateTime.Now;
            AuditHelper.GanNguoiThaoTac(canHo);
            base.Them(canHo);
            return true;
        }

        public IEnumerable<CanHo> LayTheoTinhTrang(string tinhTrang)
        {
            DongBoTinhTrangTheoNghiepVu();
            return Tim(c => c.TinhTrang == tinhTrang);
        }

        public IEnumerable<CanHo> LayTheoToa(string maToa)
        {
            return Tim(c => c.MaToa == maToa);
        }

        public void CapNhatTinhTrang(string maCanHo, string tinhTrangMoi)
        {
            if (LaTrangThaiHeThong(tinhTrangMoi)) return;
            var canHo = LayTheoMa(maCanHo);
            if (canHo == null) return;
            if (TinhTrangTheoNghiepVu(canHo) != canHo.TinhTrang) return;
            canHo.TinhTrang = tinhTrangMoi;
            Sua(canHo);
        }

        public override void Sua(CanHo canHo)
        {
            var current = _uow.CanHos.GetById(canHo.MaCanHo);
            if (current != null)
            {
                var tinhTrangTheoHeThong = TinhTrangTheoNghiepVu(current);
                if (tinhTrangTheoHeThong == "DaDatCoc" || tinhTrangTheoHeThong == "DangThue")
                    canHo.TinhTrang = tinhTrangTheoHeThong;
                else if (LaTrangThaiHeThong(canHo.TinhTrang))
                    canHo.TinhTrang = "Trong";
            }
            base.Sua(canHo);
        }

        public void ThemTienNghi(string maCanHo, string maTienNghi, string ghiChu = null)
        {
            var canHo = _uow.CanHos.GetById(maCanHo);
            var tienNghi = _uow.TienNghis.GetById(maTienNghi);
            if (canHo == null || tienNghi == null) return;
            var exists = _uow.TienNghiCuaCanHos.Any(t => t.MaCanHo == maCanHo && t.MaTienNghi == maTienNghi);
            if (exists) return;
            _uow.TienNghiCuaCanHos.Add(new TienNghiCuaCanHo { MaCanHo = maCanHo, MaTienNghi = maTienNghi, GhiChu = ghiChu });
            _uow.Complete();
        }

        public void XoaTienNghi(string maCanHo, string maTienNghi)
        {
            var item = _uow.TienNghiCuaCanHos.FirstOrDefault(t => t.MaCanHo == maCanHo && t.MaTienNghi == maTienNghi);
            if (item == null) return;
            _uow.TienNghiCuaCanHos.Remove(item);
            _uow.Complete();
        }

        private string SinhMaHinhAnh()
        {
            int max = 0;
            foreach (var x in _uow.HinhAnhNhas.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaHinhAnh, 3));
            return MaGenerator.Sinh("HA", max, 3);
        }

        public void ThemAnh(string maCanHo, string duongDanAnh, string moTa = null)
        {
            if (string.IsNullOrWhiteSpace(maCanHo) || string.IsNullOrWhiteSpace(duongDanAnh)) return;
            if (_uow.CanHos.GetById(maCanHo) == null) return;
            _uow.HinhAnhNhas.Add(new HinhAnhNha
            {
                MaHinhAnh = SinhMaHinhAnh(),
                MaCanHo = maCanHo,
                DuongDanAnh = duongDanAnh.Trim(),
                MoTa = moTa,
                NgayTaiLen = DateTime.Now
            });
            _uow.Complete();
        }

        public IEnumerable<TienNghiCuaCanHo> LayTienNghiCuaCanHo(string maCanHo)
        {
            return _uow.TienNghiCuaCanHos.Find(t => t.MaCanHo == maCanHo);
        }

        public HinhAnhNha LayAnhDaiDien(string maCanHo)
        {
            return _uow.HinhAnhNhas
                .Find(h => h.MaCanHo == maCanHo)
                .OrderBy(h => h.NgayTaiLen)
                .FirstOrDefault();
        }
    }
}
