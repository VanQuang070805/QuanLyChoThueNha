using System;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class TienNghiService : BaseService<TienNghi>
    {
        protected override IRepository<TienNghi> Repo => _uow.TienNghis;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.TienNghis.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaTienNghi, 2));
            return MaGenerator.Sinh("TN", max);
        }

        public bool Them(string tenTienNghi, string moTa, string maAdmin, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(tenTienNghi, "Ten tien nghi", out loi)) return false;
            var tn = new TienNghi { MaTienNghi = SinhMa(), TenTienNghi = tenTienNghi.Trim(), MoTa = moTa?.Trim(), MaAdmin = maAdmin };
            base.Them(tn);
            return true;
        }

        public bool XoaTienNghi(string maTienNghi, out string loi)
        {
            loi = string.Empty;
            var tienNghi = LayTheoMa(maTienNghi);
            if (tienNghi == null) { loi = "Khong tim thay tien nghi."; return false; }
            if (_uow.TienNghiCuaCanHos.Any(t => t.MaTienNghi == maTienNghi))
            {
                loi = "Khong the xoa tien nghi vi dang duoc gan cho can ho.";
                return false;
            }
            Xoa(tienNghi);
            return true;
        }
    }

    public class GiaDichVuService : BaseService<GiaDichVu>
    {
        protected override IRepository<GiaDichVu> Repo => _uow.GiaDichVus;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.GiaDichVus.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaGiaDichVu, 3));
            return MaGenerator.Sinh("GDV", max, 3);
        }

        public bool Them(GiaDichVu gdv, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(gdv.MaToa, "Toa", out loi)) return false;
            if (_uow.Toas.GetById(gdv.MaToa) == null) { loi = "Toa nha khong ton tai."; return false; }
            if (gdv.GiaDien <= 0) { loi = "Gia dien phai > 0."; return false; }
            if (gdv.GiaNuoc <= 0) { loi = "Gia nuoc phai > 0."; return false; }
            if (gdv.GiaDichVuChung < 0) { loi = "Gia dich vu chung khong duoc am."; return false; }

            foreach (var old in _uow.GiaDichVus.Find(g => g.MaToa == gdv.MaToa && g.DangApDung))
            {
                old.DangApDung = false;
                old.NgayKetThuc = DateTime.Today;
                _uow.GiaDichVus.Update(old);
            }

            gdv.MaGiaDichVu = SinhMa();
            gdv.DangApDung = true;
            _uow.GiaDichVus.Add(gdv);
            _uow.Complete();
            return true;
        }

        public GiaDichVu LayGiaHienTai(string maToa)
        {
            return TimMotBan(g => g.MaToa == maToa && g.DangApDung);
        }
    }

    public class LoaiHoaDonService : BaseService<LoaiHoaDon>
    {
        protected override IRepository<LoaiHoaDon> Repo => _uow.LoaiHoaDons;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.LoaiHoaDons.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaLoaiHoaDon, 3));
            return MaGenerator.Sinh("LHD", max, 3);
        }

        public bool Them(LoaiHoaDon loai, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(loai.TenLoai, "Ten loai hoa don", out loi)) return false;
            if (_uow.LoaiHoaDons.Any(l => l.TenLoai == loai.TenLoai))
            {
                loi = "Ten loai hoa don da ton tai.";
                return false;
            }
            loai.MaLoaiHoaDon = SinhMa();
            loai.TenLoai = loai.TenLoai.Trim();
            loai.MoTa = loai.MoTa?.Trim();
            base.Them(loai);
            return true;
        }
    }

    public class KhachThueService : BaseService<KhachThue>
    {
        protected override IRepository<KhachThue> Repo => _uow.KhachThues;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.KhachThues.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaKhach, 2));
            return MaGenerator.Sinh("KH", max);
        }

        private string SinhMaTaiKhoan()
        {
            int max = 0;
            foreach (var x in _uow.TaiKhoans.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaTaiKhoan, 2));
            return MaGenerator.Sinh("TK", max);
        }

        public bool Them(KhachThue khach, string maTaiKhoan, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(khach.HoTen, "Ho ten", out loi)) return false;
            if (!ValidationHelper.CmndHopLe(khach.SoCMND, out loi)) return false;
            // YÊU CẦU NGHIỆP VỤ: khách hàng phải đủ 18 tuổi mới được đăng ký.
            if (!ValidationHelper.DuTuoiTroLen(khach.NgaySinh, 18, out loi)) return false;
            if (_uow.TaiKhoans.GetById(maTaiKhoan) == null) { loi = "Tai khoan khach khong ton tai."; return false; }
            if (_uow.KhachThues.Any(k => k.SoCMND == khach.SoCMND)) { loi = "CMND/CCCD da duoc dang ky."; return false; }
            if (_uow.KhachThues.Any(k => k.MaTaiKhoan == maTaiKhoan)) { loi = "Tai khoan nay da gan cho khach thue khac."; return false; }

            khach.MaKhach = SinhMa();
            khach.MaTaiKhoan = maTaiKhoan;
            base.Them(khach);
            return true;
        }

        public bool TaoKhachKemTaiKhoan(KhachThue khach, string tenDangNhap, string matKhau,
            string email, string soDienThoai, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(khach.HoTen, "Ho ten", out loi)) return false;
            if (!ValidationHelper.CmndHopLe(khach.SoCMND, out loi)) return false;
            // YÊU CẦU NGHIỆP VỤ: khách hàng phải đủ 18 tuổi mới được đăng ký.
            if (!ValidationHelper.DuTuoiTroLen(khach.NgaySinh, 18, out loi)) return false;
            if (!ValidationHelper.KhongRong(tenDangNhap, "Ten dang nhap", out loi)) return false;
            if (!ValidationHelper.MatKhauDuManh(matKhau, out loi)) return false;
            if (!string.IsNullOrWhiteSpace(email) && !ValidationHelper.EmailHopLe(email, out loi)) return false;
            if (!ValidationHelper.KhongRong(soDienThoai, "So dien thoai", out loi)) return false;
            if (!ValidationHelper.SdtHopLe(soDienThoai, out loi)) return false;
            if (_uow.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap)) { loi = "Ten dang nhap da ton tai."; return false; }
            if (!string.IsNullOrWhiteSpace(email) && _uow.TaiKhoans.Any(t => t.Email == email)) { loi = "Email da duoc su dung."; return false; }
            if (_uow.KhachThues.Any(k => k.SoCMND == khach.SoCMND)) { loi = "CMND/CCCD da duoc dang ky."; return false; }

            _uow.BeginTransaction();
            try
            {
                var taiKhoan = new TaiKhoan
                {
                    MaTaiKhoan = SinhMaTaiKhoan(),
                    TenDangNhap = tenDangNhap.Trim(),
                    MatKhauHash = PasswordHelper.Hash(matKhau),
                    Email = email?.Trim(),
                    SoDienThoai = soDienThoai?.Trim(),
                    VaiTro = "KhachThue",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };
                _uow.TaiKhoans.Add(taiKhoan);
                _uow.Complete();

                khach.MaKhach = SinhMa();
                khach.MaTaiKhoan = taiKhoan.MaTaiKhoan;
                _uow.KhachThues.Add(khach);
                _uow.Complete();
                _uow.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _uow.RollbackTransaction();
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                loi = inner.Message;
                return false;
            }
        }

        public string LayMaKhachTiepTheo()   => SinhMa();
        public string LayMaTaiKhoanTiepTheo() => SinhMaTaiKhoan();
    }
}
