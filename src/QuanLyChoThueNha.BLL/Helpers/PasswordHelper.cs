using BCrypt.Net;

namespace QuanLyChoThueNha.BLL.Helpers
{
    /// <summary>
    /// Bảo mật mật khẩu với BCrypt (tự động salt).
    /// </summary>
    public static class PasswordHelper
    {
        // Work factor 12 — cân bằng bảo mật và tốc độ
        private const int WorkFactor = 12;

        /// <summary>Băm mật khẩu. Lưu kết quả vào DB.</summary>
        public static string Hash(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
        }

        /// <summary>Xác thực mật khẩu khi đăng nhập.</summary>
        public static bool Verify(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(hashedPassword))
                return false;
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
    }
}
