using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("NhanVienQuyen")]
    public class NhanVienQuyen
    {
        [Key, Column(Order = 0), MaxLength(50)]
        public string MaNhanVien { get; set; }

        [Key, Column(Order = 1), MaxLength(50)]
        public string MaChucNang { get; set; }

        public bool DuocTruyCap { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
    }
}
