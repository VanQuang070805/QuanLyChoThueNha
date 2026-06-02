using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("LoaiHoaDon")]
    public class LoaiHoaDon
    {
        [Key, Column("MaLoaiHoaDon"), MaxLength(50)]
        public string MaLoaiHoaDon { get; set; }

        [Required, MaxLength(100)]
        public string TenLoai { get; set; }

        [MaxLength(255)]
        public string MoTa { get; set; }
    }
}
