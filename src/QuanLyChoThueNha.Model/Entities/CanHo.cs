using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("CanHo")]
    public class CanHo
    {
        [Key, Column("MaCanHo"), MaxLength(50)]
        public string MaCanHo { get; set; }

        [Required, MaxLength(50)]
        public string MaToa { get; set; }

        [Required, MaxLength(50)]
        public string MaLoai { get; set; }

        [MaxLength(50)]
        public string MaNhanVien { get; set; }

        [MaxLength(50)]
        public string MaNguoiThaoTac { get; set; }

        [MaxLength(50)]
        public string VaiTroNguoiThaoTac { get; set; }

        public double DienTich { get; set; }

        [Column(TypeName = "decimal")]
        public decimal GiaThueNiemYet { get; set; }

        [Column(TypeName = "decimal")]
        public decimal TienCocNiemYet { get; set; }

        public int SoCanHo { get; set; }

        public int TangSo { get; set; }

        [Required, MaxLength(50)]
        public string TinhTrang { get; set; } = "Trong"; // Trong / DaDatCoc / DangThue / BaoTri

        [MaxLength(1000)]
        public string MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
