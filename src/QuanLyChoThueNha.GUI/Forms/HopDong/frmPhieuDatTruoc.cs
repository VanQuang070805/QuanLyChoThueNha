using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.HopDong
{
    public class frmPhieuDatTruoc : CrudFormBase<PhieuDatTruoc>
    {
        private readonly PhieuDatTruocService _service = new PhieuDatTruocService();

        public frmPhieuDatTruoc() : base("Quan ly Phieu dat truoc", Fields())
        {
            AddCommandButton("Huy phieu", BtnHuyPhieu_Click);
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var canHoService = new CanHoService();
            var khachService = new KhachThueService();
            var canHoOptions = canHoService.LayTatCa()
                .OrderBy(c => c.MaCanHo)
                .Select(c => new ComboOption(c.MaCanHo,
                    string.Format("{0} - Tang {1} - {2}", c.MaCanHo, c.TangSo, c.TinhTrang)))
                .ToList();
            var khachOptions = khachService.LayTatCa()
                .OrderBy(k => k.HoTen)
                .Select(k => new ComboOption(k.MaKhach,
                    string.Format("{0} - {1}", k.HoTen, k.SoCMND)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaPhieuDatTruoc", "Ma phieu", typeof(string), true),
                FieldDefinition.Lookup("MaCanHo", "Can ho", canHoOptions),
                FieldDefinition.Lookup("MaKhach", "Khach thue", khachOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("SoTienDatCoc", "So tien dat coc", typeof(decimal)),
                new FieldDefinition("NgayDatCoc", "Ngay dat coc", typeof(DateTime), true),
                new FieldDefinition("NgayHetHan", "Ngay het han", typeof(DateTime)),
                new FieldDefinition("TrangThai", "Trang thai", typeof(string), false,
                    new[] { "ChoKy", "DaKyHD", "Huy", "HetHan" }),
                new FieldDefinition("PhuongThucThanhToan", "Phuong thuc"),
                new FieldDefinition("GhiChu", "Ghi chu", typeof(string), false, null, true)
            };
        }

        protected override IEnumerable<PhieuDatTruoc> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(PhieuDatTruoc item, out string error)
        {
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            return _service.TaoPhieu(item, out error);
        }

        protected override bool UpdateItem(PhieuDatTruoc item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(PhieuDatTruoc item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }

        private void BtnHuyPhieu_Click(object sender, EventArgs e)
        {
            var item = CurrentItem;
            if (item == null)
            {
                ShowError("Chon phieu can huy.");
                return;
            }
            string error;
            if (!_service.HuyPhieu(item.MaPhieuDatTruoc, out error))
            {
                ShowError(error);
                return;
            }
            ReloadData();
        }
    }
}
