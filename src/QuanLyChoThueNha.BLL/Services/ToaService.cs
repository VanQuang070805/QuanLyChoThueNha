using System;
using System.Collections.Generic;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class ToaService : BaseService<Toa>
    {
        protected override IRepository<Toa> Repo => _uow.Toas;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.Toas.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaToa, 2));
            return MaGenerator.Sinh("TO", max);
        }

        public bool Them(string tenToa, string maKhuVuc, string diaChi, int soTang, string moTa, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(tenToa, "Ten toa", out loi)) return false;
            if (!ValidationHelper.KhongRong(maKhuVuc, "Khu vuc", out loi)) return false;
            if (_uow.KhuVucs.GetById(maKhuVuc) == null) { loi = "Khu vuc khong ton tai."; return false; }
            if (soTang <= 0) { loi = "So tang phai lon hon 0."; return false; }

            var toa = new Toa
            {
                MaToa = SinhMa(),
                TenToa = tenToa.Trim(),
                MaKhuVuc = maKhuVuc,
                DiaChi = diaChi?.Trim(),
                SoTang = soTang,
                MoTa = moTa?.Trim()
            };
            base.Them(toa);
            return true;
        }

        public IEnumerable<Toa> LayTheoKhuVuc(string maKhuVuc)
        {
            return Tim(t => t.MaKhuVuc == maKhuVuc);
        }

        public bool XoaToa(string maToa, out string loi)
        {
            loi = string.Empty;
            var toa = LayTheoMa(maToa);
            if (toa == null) { loi = "Khong tim thay toa nha."; return false; }
            if (_uow.CanHos.Any(c => c.MaToa == maToa))
            {
                loi = "Khong the xoa toa nha vi dang co can ho thuoc toa nay.";
                return false;
            }
            if (_uow.GiaDichVus.Any(g => g.MaToa == maToa))
            {
                loi = "Khong the xoa toa nha vi dang co bang gia dich vu lien quan.";
                return false;
            }
            Xoa(toa);
            return true;
        }
    }
}
