using System;

namespace QuanLyChoThueNha.BLL.Helpers
{
    /// <summary>
    /// Sinh mã khóa chính dạng chuỗi, ví dụ: TK001, TK002...
    /// Cách dùng: MaGenerator.Sinh("TK", maxHienTai)
    /// </summary>
    public static class MaGenerator
    {
        /// <summary>
        /// Sinh mã tiếp theo.
        /// <param name="prefix">Tiền tố, VD: "TK", "NV", "KH"</param>
        /// <param name="currentMax">Số thứ tự lớn nhất hiện tại (lấy từ DB)</param>
        /// <param name="padding">Số chữ số, mặc định 3 (001, 002...)</param>
        /// </summary>
        public static string Sinh(string prefix, int currentMax, int padding = 3)
        {
            int next = currentMax + 1;
            return prefix + next.ToString("D" + padding);
        }

        /// <summary>Lấy số thứ tự từ mã, VD: "TK007" → 7</summary>
        public static int LaySoThuTu(string ma, int prefixLength)
        {
            if (string.IsNullOrEmpty(ma) || ma.Length <= prefixLength)
                return 0;
            if (int.TryParse(ma.Substring(prefixLength), out int so))
                return so;
            return 0;
        }
    }
}
