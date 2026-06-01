using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("GiaHanHopDong")]
    public class GiaHanHopDong
    {
        [Key, Column("MaGiaHan"), MaxLength(50)]
        public string MaGiaHan { get; set; }

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

        public DateTime NgayKetThucCu { get; set; }

        public DateTime NgayKetThucMoi { get; set; }

        [Required, MaxLength(50)]
        public string TrangThai { get; set; } = "ChoXetDuyet"; // ChoXetDuyet / ChapThuan / TuChoi

        public DateTime NgayYeuCau { get; set; } = DateTime.Now;

        public DateTime? NgayDuyet { get; set; }
    }
}
