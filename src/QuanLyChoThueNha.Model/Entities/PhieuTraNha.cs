using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("PhieuTraNha")]
    public class PhieuTraNha
    {
        [Key, Column("MaPhieu"), MaxLength(50)]
        public string MaPhieu { get; set; }

        [Required, MaxLength(50)]
        public string MaHopDong { get; set; }

        [MaxLength(50)]
        public string MaNhanVien { get; set; }

        [NotMapped]
        public string TenNhanVien { get; set; }

        [NotMapped]
        public string TenCanHo { get; set; }

        [NotMapped]
        public string TenToa { get; set; }

        [MaxLength(50)]
        public string MaNguoiThaoTac { get; set; }

        [MaxLength(50)]
        public string VaiTroNguoiThaoTac { get; set; }

        public DateTime NgayTra { get; set; }

        [MaxLength(500)]
        public string TinhTrangNha { get; set; }

        [Column(TypeName = "decimal")]
        public decimal TienHoanCoc { get; set; }

        [Column(TypeName = "decimal")]
        public decimal TienKhauTru { get; set; }

        [MaxLength(500)]
        public string GhiChu { get; set; }
    }
}
