using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("KhachThue")]
    public class KhachThue
    {
        [Key, Column("MaKhach"), MaxLength(50)]
        public string MaKhach { get; set; }

        [Required, MaxLength(50)]
        public string MaTaiKhoan { get; set; }

        [Required, MaxLength(150)]
        public string HoTen { get; set; }

        [MaxLength(20)]
        public string SoCMND { get; set; }

        [MaxLength(255)]
        public string DiaChi { get; set; }

        public DateTime? NgaySinh { get; set; }
    }
}
