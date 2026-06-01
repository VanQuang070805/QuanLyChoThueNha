using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("PhieuXuLyViPham")]
    public class PhieuXuLyViPham
    {
        [Key, Column("MaViPham"), MaxLength(50)]
        public string MaViPham { get; set; }

        [Required, MaxLength(50)]
        public string MaHopDong { get; set; }

        [MaxLength(50)]
        public string MaPhieuTraNha { get; set; }

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

        [MaxLength(100)]
        public string LoaiViPham { get; set; }

        [MaxLength(500)]
        public string MoTa { get; set; }

        [Column(TypeName = "decimal")]
        public decimal PhiBoiThuong { get; set; }

        public bool TruVaoCoc { get; set; } = false;

        [Required, MaxLength(50)]
        public string TinhTrang { get; set; } = "ChoXuLy"; // ChoXuLy / DaKhauTru / DaThanhToan

        public DateTime NgayGhiNhan { get; set; } = DateTime.Now;
    }
}
