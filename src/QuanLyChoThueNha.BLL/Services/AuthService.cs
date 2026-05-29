using System;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.DAL.Repositories;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    /// <summary>
    /// Xử lý đăng nhập, xác thực, đăng xuất — TV1 phụ trách.
    /// </summary>
    public class AuthService
    {
        private readonly IUnitOfWork _uow;

        public AuthService()
        {
            _uow = new UnitOfWork();
        }

        /// <summary>
        /// Đăng nhập hệ thống.
        /// Trả về true nếu thành công và cập nhật SessionContext.
        /// </summary>
        public bool DangNhap(string tenDangNhap, string matKhau, out string thongBaoLoi)
        {
            thongBaoLoi = string.Empty;

            // Bước 1: Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                thongBaoLoi = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return false;
            }

            // Bước 2: Tìm tài khoản
            var taiKhoan = _uow.TaiKhoans
                .FirstOrDefault(tk => tk.TenDangNhap == tenDangNhap);

            if (taiKhoan == null)
            {
                thongBaoLoi = "Tên đăng nhập không tồn tại.";
                return false;
            }

            if (!taiKhoan.TrangThai)
            {
                thongBaoLoi = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.";
                return false;
            }

            // Bước 3: Xác thực mật khẩu BCrypt
            if (!PasswordHelper.Verify(matKhau, taiKhoan.MatKhauHash))
            {
                thongBaoLoi = "Mật khẩu không chính xác.";
                return false;
            }

            // Lấy họ tên và mã người dùng theo vai trò
            string hoTen = string.Empty;
            string maNguoiDung = string.Empty;

            switch (taiKhoan.VaiTro)
            {
                case "Admin":
                    var admin = _uow.Admins.FirstOrDefault(a => a.MaTaiKhoan == taiKhoan.MaTaiKhoan);
                    hoTen = admin?.HoTen ?? tenDangNhap;
                    maNguoiDung = admin?.MaAdmin ?? string.Empty;
                    break;
                case "NhanVien":
                    var nv = _uow.NhanVienQuanLys.FirstOrDefault(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);
                    hoTen = nv?.HoTen ?? tenDangNhap;
                    maNguoiDung = nv?.MaNhanVien ?? string.Empty;
                    break;
                case "KhachThue":
                    var kh = _uow.KhachThues.FirstOrDefault(k => k.MaTaiKhoan == taiKhoan.MaTaiKhoan);
                    hoTen = kh?.HoTen ?? tenDangNhap;
                    maNguoiDung = kh?.MaKhach ?? string.Empty;
                    break;
            }

            SessionContext.DangNhap(taiKhoan.MaTaiKhoan, tenDangNhap, hoTen,
                                     taiKhoan.VaiTro, maNguoiDung);
            return true;
        }

        public void DangXuat() => SessionContext.DangXuat();
    }
}
