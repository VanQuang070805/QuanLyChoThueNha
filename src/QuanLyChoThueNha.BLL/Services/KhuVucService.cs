// ═══════════════════════════════════════════════════════════════════════════
// KhuVucService.cs — TV2 phụ trách
// ═══════════════════════════════════════════════════════════════════════════
using System;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class KhuVucService : BaseService<KhuVuc>
    {
        protected override IRepository<KhuVuc> Repo => _uow.KhuVucs;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.KhuVucs.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaKhuVuc, 2));
            return MaGenerator.Sinh("KV", max);
        }

        public bool Them(string tenKhuVuc, string quan, string thanhPho, string maAdmin, out string loi,
            double? viDo = null, double? kinhDo = null)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(tenKhuVuc, "Tên khu vực", out loi)) return false;

            if (viDo.HasValue && (viDo.Value < -90 || viDo.Value > 90))
            {
                loi = "Vi do phai nam trong khoang -90 den 90.";
                return false;
            }
            if (kinhDo.HasValue && (kinhDo.Value < -180 || kinhDo.Value > 180))
            {
                loi = "Kinh do phai nam trong khoang -180 den 180.";
                return false;
            }

            var kv = new KhuVuc
            {
                MaKhuVuc  = SinhMa(),
                TenKhuVuc = tenKhuVuc.Trim(),
                Quan      = quan?.Trim(),
                ThanhPho  = thanhPho?.Trim(),
                MaAdmin   = maAdmin,
                ViDo      = viDo,
                KinhDo    = kinhDo
            };
            base.Them(kv);
            return true;
        }

        public bool XoaKhuVuc(string maKhuVuc, out string loi)
        {
            loi = string.Empty;
            var khuVuc = LayTheoMa(maKhuVuc);
            if (khuVuc == null) { loi = "Khong tim thay khu vuc."; return false; }
            if (_uow.Toas.Any(t => t.MaKhuVuc == maKhuVuc))
            {
                loi = "Khong the xoa khu vuc vi dang co toa nha thuoc khu vuc nay.";
                return false;
            }
            Xoa(khuVuc);
            return true;
        }
    }
}
