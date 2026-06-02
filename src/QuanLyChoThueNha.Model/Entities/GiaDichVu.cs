using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("GiaDichVu")]
    public class GiaDichVu
    {
        [Key, Column("MaGiaDichVu"), MaxLength(50)]
        public string MaGiaDichVu { get; set; }

        [Required, MaxLength(50)]
        public string MaToa { get; set; }

        [Column(TypeName = "decimal")]
        public decimal GiaDien { get; set; }

        [Column(TypeName = "decimal")]
        public decimal GiaNuoc { get; set; }

        [Column(TypeName = "decimal")]
        public decimal GiaDichVuChung { get; set; }

        public DateTime NgayApDung { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public bool DangApDung { get; set; } = true;
    }
}
