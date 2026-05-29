using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("Toa")]
    public class Toa
    {
        [Key, Column("MaToa"), MaxLength(50)]
        public string MaToa { get; set; }

        [Required, MaxLength(50)]
        public string MaKhuVuc { get; set; }

        [Required, MaxLength(100)]
        public string TenToa { get; set; }

        [MaxLength(255)]
        public string DiaChi { get; set; }

        public int SoTang { get; set; }

        [MaxLength(500)]
        public string MoTa { get; set; }
    }
}
