using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyChoThueNha.Model.Entities
{
    [Table("HoaDonThanhToan")]
    public class HoaDonThanhToan
    {
        [Key, Column("MaHoaDon"), MaxLength(50)]
        public string MaHoaDon { get; set; }

        [Required, MaxLength(50)]
        public string MaHopDong { get; set; }

        [Required, MaxLength(50)]
        public string MaLoaiHoaDon { get; set; }

        [MaxLength(50)]
        public string MaNhanVienThu { get; set; }

        [MaxLength(50)]
        public string MaViPham { get; set; } // nullable — phiếu vi phạm nếu có

        [MaxLength(20)]
        public string KyThanhToan { get; set; } // VD: 06/2025

        // ===== BỔ SUNG (Bước 3): lưu chỉ số điện/nước để truy vết =====
        // Trước đây form chỉ tính ra số tiền rồi vứt chỉ số đi -> không kiểm tra lại được.
        // Nay lưu cả chỉ số cũ/mới của điện và nước. Tiêu thụ = Mới - Cũ.

        /// <summary>Chỉ số công-tơ điện kỳ trước (kWh). Tự lấy từ hóa đơn kỳ liền trước.</summary>
        public double ChiSoDienCu { get; set; }

        /// <summary>Chỉ số công-tơ điện kỳ này (kWh). Nhân viên nhập.</summary>
        public double ChiSoDienMoi { get; set; }

        /// <summary>Chỉ số đồng hồ nước kỳ trước (m3). Tự lấy từ hóa đơn kỳ liền trước.</summary>
        public double ChiSoNuocCu { get; set; }

        /// <summary>Chỉ số đồng hồ nước kỳ này (m3). Nhân viên nhập.</summary>
        public double ChiSoNuocMoi { get; set; }

        [Column(TypeName = "decimal")]
        public decimal SoTienPhaiTra { get; set; }

        [Column(TypeName = "decimal")]
        public decimal SoTienDaTra { get; set; }

        public DateTime NgayDaoHan { get; set; }

        public DateTime? NgayThanhToan { get; set; }

        [Required, MaxLength(50)]
        public string TrangThai { get; set; } = "ChuaTra"; // ChuaTra / DaTra / TraThieu / QuaHan

        [MaxLength(50)]
        public string PhuongThucThanhToan { get; set; }
    }
}
