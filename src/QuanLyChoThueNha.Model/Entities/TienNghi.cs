using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("TienNghi")]
    public class TienNghi
    {
        [Key, Column("MaTienNghi"), MaxLength(50)]
        public string MaTienNghi { get; set; }

        [MaxLength(50)]
        public string MaAdmin { get; set; }

        [Required, MaxLength(100)]
        public string TenTienNghi { get; set; }

        [MaxLength(255)]
        public string MoTa { get; set; }
    }
}
