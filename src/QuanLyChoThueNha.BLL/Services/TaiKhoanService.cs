using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class TaiKhoanService : BaseService<TaiKhoan>
    {
        protected override IRepository<TaiKhoan> Repo => _uow.TaiKhoans;

        private string SinhMaTaiKhoan()
        {
            int max = 0;
            foreach (var tk in _uow.TaiKhoans.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(tk.MaTaiKhoan, 2));
            return MaGenerator.Sinh("TK", max);
        }

        public string LayMaTaiKhoanTiepTheo()
        {
            return SinhMaTaiKhoan();
        }

        public bool TenDangNhapDaTon(string tenDangNhap, string maTKHienTai = null)
        {
            return _uow.TaiKhoans.Any(tk =>
                tk.TenDangNhap == tenDangNhap &&
                (maTKHienTai == null || tk.MaTaiKhoan != maTKHienTai));
        }

        public bool EmailDaTon(string email, string maTKHienTai = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _uow.TaiKhoans.Any(tk =>
                tk.Email == email &&
                (maTKHienTai == null || tk.MaTaiKhoan != maTKHienTai));
        }

        public bool TaoTaiKhoan(string tenDangNhap, string matKhauRo, string email,
            string sdt, string vaiTro, out string loi)
        {
            loi = string.Empty;

            if (!ValidationHelper.KhongRong(tenDangNhap, "Ten dang nhap", out loi)) return false;
            if (!ValidationHelper.MatKhauDuManh(matKhauRo, out loi)) return false;
            if (!string.IsNullOrWhiteSpace(email) && !ValidationHelper.EmailHopLe(email, out loi)) return false;
            if (!string.IsNullOrWhiteSpace(sdt) && !ValidationHelper.SdtHopLe(sdt, out loi)) return false;
            if (vaiTro != "Admin" && vaiTro != "NhanVien" && vaiTro != "KhachThue")
            {
                loi = "Vai tro khong hop le.";
                return false;
            }

            if (TenDangNhapDaTon(tenDangNhap))
            {
                loi = "Ten dang nhap da ton tai trong he thong.";
                return false;
            }
            if (EmailDaTon(email))
            {
                loi = "Email da ton tai trong he thong.";
                return false;
            }

            var taiKhoan = new TaiKhoan
            {
                MaTaiKhoan = SinhMaTaiKhoan(),
                TenDangNhap = tenDangNhap.Trim(),
                MatKhauHash = PasswordHelper.Hash(matKhauRo),
                Email = email == null ? null : email.Trim(),
                SoDienThoai = sdt == null ? null : sdt.Trim(),
                VaiTro = vaiTro,
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            Them(taiKhoan);
            return true;
        }

        public bool CapNhatTaiKhoan(TaiKhoan item, string vaiTroNguoiThucHien, out string loi)
        {
            loi = string.Empty;
            if (item == null)
            {
                loi = "Khong co tai khoan can cap nhat.";
                return false;
            }
            if (!LaAdmin(vaiTroNguoiThucHien))
            {
                loi = "Chi Admin moi duoc cap nhat tai khoan.";
                return false;
            }
            if (!ValidationHelper.KhongRong(item.TenDangNhap, "Ten dang nhap", out loi)) return false;
            if (!string.IsNullOrWhiteSpace(item.Email) && !ValidationHelper.EmailHopLe(item.Email, out loi)) return false;
            if (!string.IsNullOrWhiteSpace(item.SoDienThoai) && !ValidationHelper.SdtHopLe(item.SoDienThoai, out loi)) return false;
            if (item.VaiTro != "Admin" && item.VaiTro != "NhanVien")
            {
                loi = "Man hinh nay chi quan ly tai khoan Admin/NhanVien. Tai khoan khach thue duoc quan ly rieng.";
                return false;
            }
            if (item.VaiTro == "Admin" && !item.TrangThai)
            {
                loi = "Khong duoc khoa tai khoan Admin.";
                return false;
            }
            if (TenDangNhapDaTon(item.TenDangNhap, item.MaTaiKhoan))
            {
                loi = "Ten dang nhap da ton tai trong he thong.";
                return false;
            }
            if (EmailDaTon(item.Email, item.MaTaiKhoan))
            {
                loi = "Email da ton tai trong he thong.";
                return false;
            }

            Sua(item);
            return true;
        }

        public bool DoiMatKhau(string maTaiKhoan, string matKhauCu, string matKhauMoi, out string loi)
        {
            loi = string.Empty;
            var tk = LayTheoMa(maTaiKhoan);
            if (tk == null) { loi = "Khong tim thay tai khoan."; return false; }

            if (!PasswordHelper.Verify(matKhauCu, tk.MatKhauHash))
            {
                loi = "Mat khau hien tai khong dung.";
                return false;
            }
            if (!ValidationHelper.MatKhauDuManh(matKhauMoi, out loi)) return false;

            tk.MatKhauHash = PasswordHelper.Hash(matKhauMoi);
            Sua(tk);
            return true;
        }

        public bool DoiTrangThai(string maTaiKhoan, bool trangThaiMoi, string vaiTroNguoiThucHien, out string loi)
        {
            loi = string.Empty;
            if (!LaAdmin(vaiTroNguoiThucHien))
            {
                loi = "Chi Admin moi duoc khoa/mo khoa tai khoan.";
                return false;
            }

            var tk = LayTheoMa(maTaiKhoan);
            if (tk == null)
            {
                loi = "Khong tim thay tai khoan.";
                return false;
            }
            if (tk.VaiTro == "Admin" && !trangThaiMoi)
            {
                loi = "Khong duoc khoa tai khoan Admin.";
                return false;
            }

            tk.TrangThai = trangThaiMoi;
            Sua(tk);
            return true;
        }

        private bool LaAdmin(string vaiTroNguoiThucHien)
        {
            if (string.Equals((vaiTroNguoiThucHien ?? string.Empty).Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.IsNullOrWhiteSpace(SessionContext.MaTaiKhoan))
            {
                var tkDangNhap = _uow.TaiKhoans.GetById(SessionContext.MaTaiKhoan);
                return tkDangNhap != null &&
                       string.Equals((tkDangNhap.VaiTro ?? string.Empty).Trim(), "Admin", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public void DoiTrangThai(string maTaiKhoan, bool trangThaiMoi)
        {
            string loi;
            DoiTrangThai(maTaiKhoan, trangThaiMoi, "Admin", out loi);
        }

        public IEnumerable<TaiKhoan> LayTheoVaiTro(string vaiTro)
        {
            return Tim(tk => tk.VaiTro == vaiTro);
        }

        // Tra ve danh sach tai khoan Admin/NhanVien voi MaNguoiDung duoc dien san tu bang Admin/NhanVienQuanLy.
        public IEnumerable<TaiKhoan> LayTatCaVoiMaNguoiDung()
        {
            var adminMap    = _uow.Admins.GetAll().ToDictionary(a => a.MaTaiKhoan, a => a.MaAdmin);
            var nvMap       = _uow.NhanVienQuanLys.GetAll().ToDictionary(n => n.MaTaiKhoan, n => n.MaNhanVien);

            var list = new List<TaiKhoan>();
            foreach (var tk in _uow.TaiKhoans.GetAll().Where(t => t.VaiTro != "KhachThue"))
            {
                string ma;
                if (tk.VaiTro == "Admin" && adminMap.TryGetValue(tk.MaTaiKhoan, out ma))
                    tk.MaNguoiDung = ma;
                else if (tk.VaiTro == "NhanVien" && nvMap.TryGetValue(tk.MaTaiKhoan, out ma))
                    tk.MaNguoiDung = ma;
                list.Add(tk);
            }
            return list;
        }
    }
}
