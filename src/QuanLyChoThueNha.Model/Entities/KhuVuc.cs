using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("KhuVuc")]
    public class KhuVuc
    {
        [Key, Column("MaKhuVuc"), MaxLength(50)]
        public string MaKhuVuc { get; set; }

        [MaxLength(50)]
        public string MaAdmin { get; set; }

        [Required, MaxLength(150)]
        public string TenKhuVuc { get; set; }

        [MaxLength(100)]
        public string Quan { get; set; }

        [MaxLength(100)]
        public string ThanhPho { get; set; }

        public double? ViDo { get; set; }

        public double? KinhDo { get; set; }
    }
}
