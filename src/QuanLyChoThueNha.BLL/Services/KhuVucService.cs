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

        public bool Them(string tenKhuVuc, string quan, string thanhPho, string maAdmin, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(tenKhuVuc, "Tên khu vực", out loi)) return false;

            var kv = new KhuVuc
            {
                MaKhuVuc  = SinhMa(),
                TenKhuVuc = tenKhuVuc.Trim(),
                Quan      = quan?.Trim(),
                ThanhPho  = thanhPho?.Trim(),
                MaAdmin   = maAdmin
            };
            base.Them(kv);
            return true;
        }
    }
}
