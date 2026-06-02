using System;
using System.Text.RegularExpressions;

namespace QuanLyChoThueNha.BLL.Helpers
{
    /// <summary>
    /// Kiểm tra tính hợp lệ dữ liệu đầu vào — theo quy trình 3 bước của cô:
    /// Bước 1: Kiểm tra sự tương đồng và tính chính xác.
    /// Bước 2 (SAI): Thông báo lỗi, yêu cầu nhập lại.
    /// Bước 3 (ĐÚNG): Chuyển dữ liệu vào DB.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>Kiểm tra chuỗi không rỗng.</summary>
        public static bool KhongRong(string value, string tenTruong, out string loi)
        {
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                loi = $"{tenTruong} không được để trống.";
                return false;
            }
            return true;
        }

        /// <summary>Kiểm tra email hợp lệ.</summary>
        public static bool EmailHopLe(string email, out string loi)
        {
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(email)) { loi = "Email không được để trống."; return false; }
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(email)) { loi = "Email không đúng định dạng."; return false; }
            return true;
        }

        /// <summary>Kiểm tra số điện thoại (10-11 số).</summary>
        public static bool SdtHopLe(string sdt, out string loi)
        {
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(sdt)) { loi = "Số điện thoại không được để trống."; return false; }
            if (!Regex.IsMatch(sdt, @"^[0-9]{10,11}$")) { loi = "Số điện thoại phải gồm 10-11 chữ số."; return false; }
            return true;
        }

        /// <summary>Kiểm tra mật khẩu đủ mạnh (ít nhất 6 ký tự, có chữ hoa, số).</summary>
        public static bool MatKhauDuManh(string matKhau, out string loi)
        {
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(matKhau)) { loi = "Mật khẩu không được để trống."; return false; }
            if (matKhau.Length < 6) { loi = "Mật khẩu phải có ít nhất 6 ký tự."; return false; }
            if (!Regex.IsMatch(matKhau, @"[A-Z]")) { loi = "Mật khẩu phải có ít nhất 1 chữ hoa."; return false; }
            if (!Regex.IsMatch(matKhau, @"[0-9]")) { loi = "Mật khẩu phải có ít nhất 1 chữ số."; return false; }
            return true;
        }

        /// <summary>Kiểm tra số thập phân dương.</summary>
        public static bool SoDuong(decimal value, string tenTruong, out string loi)
        {
            loi = string.Empty;
            if (value <= 0) { loi = $"{tenTruong} phải là số dương."; return false; }
            return true;
        }

        /// <summary>Kiểm tra ngày kết thúc phải sau ngày bắt đầu.</summary>
        public static bool NgayHopLe(DateTime batDau, DateTime ketThuc, out string loi)
        {
            loi = string.Empty;
            if (ketThuc <= batDau) { loi = "Ngày kết thúc phải sau ngày bắt đầu."; return false; }
            return true;
        }

        /// <summary>
        /// Kiểm tra khách hàng phải đủ 18 tuổi trở lên (tính theo ngày sinh).
        /// Yêu cầu nghiệp vụ: người ký hợp đồng/đặt cọc phải đủ năng lực hành vi dân sự.
        /// </summary>
        public static bool DuTuoiTroLen(DateTime? ngaySinh, int tuoiToiThieu, out string loi)
        {
            loi = string.Empty;
            if (ngaySinh == null)
            {
                loi = "Ngày sinh không được để trống.";
                return false;
            }
            if (ngaySinh.Value.Date > DateTime.Today)
            {
                loi = "Ngày sinh không hợp lệ (lớn hơn ngày hiện tại).";
                return false;
            }

            // Tính tuổi chính xác: lấy hiệu số năm, trừ 1 nếu chưa tới sinh nhật trong năm nay
            int tuoi = DateTime.Today.Year - ngaySinh.Value.Year;
            if (ngaySinh.Value.Date > DateTime.Today.AddYears(-tuoi)) tuoi--;

            if (tuoi < tuoiToiThieu)
            {
                loi = $"Khách hàng phải đủ {tuoiToiThieu} tuổi trở lên (hiện {tuoi} tuổi).";
                return false;
            }
            return true;
        }

        /// <summary>Kiểm tra CMND/CCCD (9 hoặc 12 số).</summary>
        public static bool CmndHopLe(string cmnd, out string loi)
        {
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(cmnd)) { loi = "CMND/CCCD không được để trống."; return false; }
            if (!Regex.IsMatch(cmnd, @"^[0-9]{9}$|^[0-9]{12}$")) { loi = "CMND/CCCD phải gồm 9 hoặc 12 chữ số."; return false; }
            return true;
        }
    }
}
