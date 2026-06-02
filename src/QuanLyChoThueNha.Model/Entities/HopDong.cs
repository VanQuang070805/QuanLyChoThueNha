using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("HopDong")]
    public class HopDong
    {
        [Key, Column("MaHopDong"), MaxLength(50)]
        public string MaHopDong { get; set; }

        [MaxLength(50)]
        public string MaPhieuDatTruoc { get; set; }

        [Required, MaxLength(50)]
        public string MaCanHo { get; set; }

        [Required, MaxLength(50)]
        public string MaKhach { get; set; }

        [MaxLength(50)]
        public string MaNhanVien { get; set; }

        [MaxLength(50)]
        public string MaNguoiThaoTac { get; set; }

        [MaxLength(50)]
        public string VaiTroNguoiThaoTac { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        [Column(TypeName = "decimal")]
        public decimal GiaThueChot { get; set; }

        [Column(TypeName = "decimal")]
        public decimal TienCocChot { get; set; }

        // ===== BỔ SUNG (Bước 1): minh bạch việc trừ cọc trước =====
        /// <summary>
        /// Số tiền khách đã đặt cọc trước (từ PhieuDatTruoc) được trừ vào TienCocChot.
        /// Tự động lấy từ phiếu đặt trước khi ký hợp đồng. Bằng 0 nếu không có phiếu đặt trước.
        /// </summary>
        [Column(TypeName = "decimal")]
        public decimal TienCocTruocDaTru { get; set; }

        /// <summary>
        /// Số tiền cọc khách CÒN phải nộp khi ký hợp đồng = TienCocChot - TienCocTruocDaTru.
        /// Trường tự sinh (computed) nên không map xuống DB.
        /// </summary>
        [NotMapped]
        public decimal TienCocConPhaiNop => TienCocChot - TienCocTruocDaTru;

        [Required, MaxLength(50)]
        public string TrangThai { get; set; } = "HieuLuc"; // HieuLuc / HetHan / DaHuy

        [MaxLength(1000)]
        public string GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
