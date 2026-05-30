using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("EmailLog")]
    public class EmailLog
    {
        [Key, Column("MaEmailLog"), MaxLength(50)]
        public string MaEmailLog { get; set; }

        [MaxLength(50)]
        public string MaPhieuDatTruoc { get; set; }

        [MaxLength(50)]
        public string MaTaiKhoan { get; set; }

        [Required, MaxLength(150)]
        public string EmailNguoiNhan { get; set; }

        [Required, MaxLength(100)]
        public string LoaiEmail { get; set; }

        [Required, MaxLength(255)]
        public string TieuDe { get; set; }

        [Required, MaxLength(50)]
        public string TrangThai { get; set; }

        [MaxLength(1000)]
        public string ThongBao { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string MaNguoiThaoTac { get; set; }

        [MaxLength(50)]
        public string VaiTroNguoiThaoTac { get; set; }
    }
}
