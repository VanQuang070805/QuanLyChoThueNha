using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TraNha
{
    public class frmPhieuXuLyViPham : CrudFormBase<PhieuXuLyViPham>
    {
        private readonly PhieuXuLyViPhamService _service = new PhieuXuLyViPhamService();

        public frmPhieuXuLyViPham() : base("Quan ly Xu ly vi pham", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var hopDongOptions = new HopDongService().LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .OrderBy(h => h.MaHopDong)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} | Khach {1} | Phong {2}", h.MaHopDong, h.MaKhach, h.MaCanHo)))
                .ToList();
            var phieuTraOptions = new List<ComboOption> { new ComboOption(string.Empty, "(Khong gan phieu tra nha)") };
            phieuTraOptions.AddRange(new PhieuTraNhaService().LayTatCa()
                .OrderBy(p => p.MaPhieu)
                .Select(p => new ComboOption(p.MaPhieu,
                    string.Format("{0} | Hop dong {1} | {2:dd/MM/yyyy}", p.MaPhieu, p.MaHopDong, p.NgayTra))));

            return new[]
            {
                new FieldDefinition("MaViPham", "Ma vi pham", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Ma hop dong", hopDongOptions),
                FieldDefinition.Lookup("MaPhieuTraNha", "Ma phieu tra nha", phieuTraOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("LoaiViPham", "Loai vi pham"),
                new FieldDefinition("MoTa", "Mo ta", typeof(string), false, null, true),
                new FieldDefinition("PhiBoiThuong", "Phi boi thuong", typeof(decimal)),
                new FieldDefinition("TruVaoCoc", "Tru vao coc", typeof(bool)),
                new FieldDefinition("TinhTrang", "Tinh trang", typeof(string), false,
                    new[] { "ChoXuLy", "DaThanhToan" }),
                new FieldDefinition("NgayGhiNhan", "Ngay ghi nhan", typeof(System.DateTime), true)
            };
        }

        protected override IEnumerable<PhieuXuLyViPham> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(PhieuXuLyViPham item, out string error)
        {
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            item.MaPhieuTraNha = string.IsNullOrWhiteSpace(item.MaPhieuTraNha) ? null : item.MaPhieuTraNha;
            if (item.TruVaoCoc) item.TinhTrang = "ChoXuLy";
            return _service.GhiNhan(item, out error);
        }

        protected override bool UpdateItem(PhieuXuLyViPham item, out string error)
        {
            error = string.Empty;
            item.MaPhieuTraNha = string.IsNullOrWhiteSpace(item.MaPhieuTraNha) ? null : item.MaPhieuTraNha;
            if (item.TruVaoCoc) item.TinhTrang = "ChoXuLy";
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(PhieuXuLyViPham item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }
    }
}
