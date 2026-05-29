using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("LoaiCanHo")]
    public class LoaiCanHo
    {
        [Key, Column("MaLoai"), MaxLength(50)]
        public string MaLoai { get; set; }

        [MaxLength(50)]
        public string MaAdmin { get; set; }

        [Required, MaxLength(100)]
        public string TenLoai { get; set; }

        [MaxLength(500)]
        public string MoTa { get; set; }
    }
}
