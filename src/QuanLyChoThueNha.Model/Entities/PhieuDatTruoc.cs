using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("PhieuDatTruoc")]
    public class PhieuDatTruoc
    {
        [Key, Column("MaPhieuDatTruoc"), MaxLength(50)]
        public string MaPhieuDatTruoc { get; set; }

        [Required, MaxLength(50)]
        public string MaCanHo { get; set; }

        [Required, MaxLength(50)]
        public string MaKhach { get; set; }

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

        [Column(TypeName = "decimal")]
        public decimal SoTienDatCoc { get; set; }

        public DateTime NgayDatCoc { get; set; } = DateTime.Now;

        public DateTime NgayHetHan { get; set; }

        [Required, MaxLength(50)]
        public string TrangThai { get; set; } = "ChoThanhToanCoc";

        [MaxLength(50)]
        public string PhuongThucThanhToan { get; set; }

        [MaxLength(500)]
        public string GhiChu { get; set; }
    }
}
