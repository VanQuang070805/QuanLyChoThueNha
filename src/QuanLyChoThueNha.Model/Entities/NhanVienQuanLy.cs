using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("NhanVienQuanLy")]
    public class NhanVienQuanLy
    {
        [Key, Column("MaNhanVien"), MaxLength(50)]
        public string MaNhanVien { get; set; }

        [Required, MaxLength(50)]
        public string MaTaiKhoan { get; set; }

        [Required, MaxLength(150)]
        public string HoTen { get; set; }

        public DateTime NgayVaoLam { get; set; } = DateTime.Now;
    }
}
