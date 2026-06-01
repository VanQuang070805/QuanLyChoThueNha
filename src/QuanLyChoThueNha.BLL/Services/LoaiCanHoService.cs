using System;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    /// <summary>TV2 phụ trách.</summary>
    public class LoaiCanHoService : BaseService<LoaiCanHo>
    {
        protected override IRepository<LoaiCanHo> Repo => _uow.LoaiCanHos;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.LoaiCanHos.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaLoai, 2));
            return MaGenerator.Sinh("LC", max);
        }

        public bool Them(string tenLoai, string moTa, string maAdmin, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(tenLoai, "Tên loại căn hộ", out loi)) return false;
            var loai = new LoaiCanHo { MaLoai = SinhMa(), TenLoai = tenLoai.Trim(), MoTa = moTa?.Trim(), MaAdmin = maAdmin };
            base.Them(loai);
            return true;
        }

        public bool XoaLoaiCanHo(string maLoai, out string loi)
        {
            loi = string.Empty;
            var loai = LayTheoMa(maLoai);
            if (loai == null) { loi = "Khong tim thay loai can ho."; return false; }
            if (_uow.CanHos.Any(c => c.MaLoai == maLoai))
            {
                loi = "Khong the xoa loai can ho vi dang co can ho su dung loai nay.";
                return false;
            }
            Xoa(loai);
            return true;
        }
    }
}
