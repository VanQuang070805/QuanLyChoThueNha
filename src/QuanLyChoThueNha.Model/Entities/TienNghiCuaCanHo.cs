using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    // Bảng quan hệ nhiều-nhiều giữa CanHo và TienNghi
    [Table("TienNghiCuaCanHo")]
    public class TienNghiCuaCanHo
    {
        // Khóa kết hợp — cấu hình trong AppDbContext (HasKey)
        [Required, MaxLength(50)]
        public string MaCanHo { get; set; }

        [Required, MaxLength(50)]
        public string MaTienNghi { get; set; }

        [MaxLength(255)]
        public string GhiChu { get; set; }
    }
}
