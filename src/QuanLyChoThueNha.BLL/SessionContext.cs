namespace QuanLyChoThueNha.BLL
{
    /// <summary>
    /// Singleton lưu thông tin phiên đăng nhập hiện tại.
    /// Dùng chung toàn app để phân quyền hiển thị menu/form.
    /// </summary>
    public static class SessionContext
    {
        public static string MaTaiKhoan  { get; private set; }
        public static string TenDangNhap { get; private set; }
        public static string HoTen       { get; private set; }
        public static string VaiTro      { get; private set; } // Admin / NhanVien / KhachThue
        public static string MaNguoiDung { get; private set; } // MaAdmin / MaNhanVien / MaKhach

        /// <summary>Gọi sau khi đăng nhập thành công.</summary>
        public static void DangNhap(string maTaiKhoan, string tenDangNhap, string hoTen,
                                     string vaiTro, string maNguoiDung)
        {
            MaTaiKhoan  = maTaiKhoan;
            TenDangNhap = tenDangNhap;
            HoTen       = hoTen;
            VaiTro      = vaiTro;
            MaNguoiDung = maNguoiDung;
        }

        /// <summary>Gọi khi đăng xuất.</summary>
        public static void DangXuat()
        {
            MaTaiKhoan  = null;
            TenDangNhap = null;
            HoTen       = null;
            VaiTro      = null;
            MaNguoiDung = null;
        }

        public static bool DaXacThuc => !string.IsNullOrEmpty(MaTaiKhoan);
        public static bool LaAdmin   => VaiTro == "Admin";
        public static bool LaNhanVien => VaiTro == "NhanVien";
        public static bool LaKhachThue => VaiTro == "KhachThue";
    }
}
