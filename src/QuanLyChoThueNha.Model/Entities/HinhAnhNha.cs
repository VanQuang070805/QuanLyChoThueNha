using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("HinhAnhNha")]
    public class HinhAnhNha
    {
        [Key, Column("MaHinhAnh"), MaxLength(50)]
        public string MaHinhAnh { get; set; }

        [Required, MaxLength(50)]
        public string MaCanHo { get; set; }

        [Required, MaxLength(500)]
        public string DuongDanAnh { get; set; }

        [MaxLength(255)]
        public string MoTa { get; set; }

        public DateTime NgayTaiLen { get; set; } = DateTime.Now;
    }
}
