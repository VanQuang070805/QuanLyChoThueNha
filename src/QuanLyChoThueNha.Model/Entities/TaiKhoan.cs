using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key, Column("MaTaiKhoan"), MaxLength(50)]
        public string MaTaiKhoan { get; set; }

        [Required, MaxLength(100)]
        public string TenDangNhap { get; set; }

        [Required, MaxLength(255)]
        public string MatKhauHash { get; set; }

        [MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string SoDienThoai { get; set; }

        [Required, MaxLength(50)]
        public string VaiTro { get; set; } // Admin / NhanVien / KhachThue

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Ma cua Admin/NhanVien gan voi tai khoan nay (NotMapped - chi dung de hien thi)
        [NotMapped]
        public string MaNguoiDung { get; set; }
    }
}
