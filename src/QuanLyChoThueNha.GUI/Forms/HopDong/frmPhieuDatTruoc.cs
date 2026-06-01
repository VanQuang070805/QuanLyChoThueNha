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
            HideDeleteButton();
            AddCommandButton("Xac nhan da nhan coc", BtnXacNhanCoc_Click);
            AddCommandButton("Huy phieu", BtnHuyPhieu_Click);
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var canHoService = new CanHoService();
            var khachService = new KhachThueService();
            var canHoOptions = canHoService.LayTatCa()
                .Where(c => c.TinhTrang == "Trong")
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
                new FieldDefinition("TrangThai", "Trang thai", typeof(string), true),
                new FieldDefinition("PhuongThucThanhToan", "Phuong thuc", typeof(string), false,
                    new[] { "ChuyenKhoan", "TienMat" }),
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
            if (item.TrangThai == PhieuDatTruocService.Huy ||
                item.TrangThai == PhieuDatTruocService.HetHan ||
                item.TrangThai == PhieuDatTruocService.DaKyHD)
            {
                error = "Trang thai he thong khong duoc sua thu cong.";
                return false;
            }
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(PhieuDatTruoc item, out string error)
        {
            error = string.Empty;
            return _service.XoaPhieu(item.MaPhieuDatTruoc, out error);
        }

        private void BtnXacNhanCoc_Click(object sender, EventArgs e)
        {
            var item = CurrentItem;
            if (item == null)
            {
                ShowError("Chon phieu can xac nhan coc.");
                return;
            }

            string error;
            if (!_service.XacNhanDaNhanCoc(item.MaPhieuDatTruoc, out error))
            {
                ShowError(error);
                return;
            }

            ShowInfo("Da xac nhan coc. Phieu da san sang cho buoc ky hop dong.");
            ReloadData();
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
