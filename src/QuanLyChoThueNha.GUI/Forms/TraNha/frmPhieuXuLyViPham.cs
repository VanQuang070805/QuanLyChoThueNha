using System.Collections.Generic;
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
            return new[]
            {
                new FieldDefinition("MaViPham", "Ma vi pham", typeof(string), true),
                new FieldDefinition("MaHopDong", "Ma hop dong"),
                new FieldDefinition("MaPhieuTraNha", "Ma phieu tra nha"),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("LoaiViPham", "Loai vi pham"),
                new FieldDefinition("MoTa", "Mo ta", typeof(string), false, null, true),
                new FieldDefinition("PhiBoiThuong", "Phi boi thuong", typeof(decimal)),
                new FieldDefinition("TruVaoCoc", "Tru vao coc", typeof(bool)),
                new FieldDefinition("TinhTrang", "Tinh trang", typeof(string), false,
                    new[] { "ChoXuLy", "DaKhauTru", "DaThanhToan" }),
                new FieldDefinition("NgayGhiNhan", "Ngay ghi nhan", typeof(System.DateTime), true)
            };
        }

        protected override IEnumerable<PhieuXuLyViPham> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(PhieuXuLyViPham item, out string error)
        {
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            return _service.GhiNhan(item, out error);
        }

        protected override bool UpdateItem(PhieuXuLyViPham item, out string error)
        {
            error = string.Empty;
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
