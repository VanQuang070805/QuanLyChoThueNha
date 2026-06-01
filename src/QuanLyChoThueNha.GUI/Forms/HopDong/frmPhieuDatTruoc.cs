using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;
using System.Windows.Forms;

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

        protected override IEnumerable<PhieuDatTruoc> GetItems()
        {
            var items = _service.LayTatCa().ToList();
            GanThongTinHienThi(items);
            return items;
        }

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

        protected override void AfterGridBound()
        {
            GridDisplayHelper.AddTextColumn(Grid, "TenCanHo", "Tên căn hộ", 2, 125);
            GridDisplayHelper.AddTextColumn(Grid, "TenToa", "Tên tòa", 3, 115);
            GridDisplayHelper.AddTextColumn(Grid, "TenNhanVien", "Tên nhân viên", 5, 130);
            GridDisplayHelper.HideColumn(Grid, "MaCanHo");
            GridDisplayHelper.HideColumn(Grid, "MaNhanVien");
            GridDisplayHelper.HideColumn(Grid, "Toa");
            GridDisplayHelper.SetFillWeight(Grid, "MaPhieuDatTruoc", 105);
            GridDisplayHelper.SetFillWeight(Grid, "TenCanHo", 125);
            GridDisplayHelper.SetFillWeight(Grid, "TenToa", 110);
            GridDisplayHelper.SetFillWeight(Grid, "MaKhach", 100);
            GridDisplayHelper.SetFillWeight(Grid, "TenNhanVien", 130);
            GridDisplayHelper.SetFillWeight(Grid, "TrangThai", 105);
            GridDisplayHelper.SetFillWeight(Grid, "PhuongThucThanhToan", 110);
            GridDisplayHelper.SetMoneyColumn(Grid, "SoTienDatCoc");
            GridDisplayHelper.BalanceGrid(Grid);
        }

        private static void GanThongTinHienThi(IEnumerable<PhieuDatTruoc> items)
        {
            var canHoService = new CanHoService();
            var toaService = new ToaService();
            var canHos = canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = toaService.LayTatCa().ToDictionary(t => t.MaToa);
            var nhanViens = new NhanVienPhanQuyenService().LayNhanVien()
                .GroupBy(n => n.MaNhanVien)
                .ToDictionary(g => g.Key, g => g.First().HoTen);

            foreach (var phieu in items)
            {
                phieu.TenNhanVien = TenNhanVien(phieu.MaNhanVien, nhanViens);
                phieu.TenCanHo = string.Empty;
                phieu.TenToa = string.Empty;

                CanHo canHo;
                if (!canHos.TryGetValue(phieu.MaCanHo, out canHo)) continue;

                phieu.TenCanHo = "Căn " + canHo.SoCanHo;
                Toa toa;
                phieu.TenToa = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
            }
        }

        private static string TenNhanVien(string maNhanVien, IDictionary<string, string> nhanViens)
        {
            if (string.IsNullOrWhiteSpace(maNhanVien)) return "Admin";
            string hoTen;
            return nhanViens.TryGetValue(maNhanVien, out hoTen) ? hoTen : maNhanVien;
        }
    }
}
