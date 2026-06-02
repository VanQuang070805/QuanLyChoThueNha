using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("Admin")]
    public class Admin
    {
        [Key, Column("MaAdmin"), MaxLength(50)]
        public string MaAdmin { get; set; }

        [Required, MaxLength(50)]
        public string MaTaiKhoan { get; set; }

        [Required, MaxLength(150)]
        public string HoTen { get; set; }
    }
}
