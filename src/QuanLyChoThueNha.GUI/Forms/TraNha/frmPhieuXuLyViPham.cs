using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;
using System.Windows.Forms;
using HopDongEntity = QuanLyChoThueNha.Model.Entities.HopDong;

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
                FieldDefinition.Lookup("MaHopDong", "Hop dong", TaoHopDongOptions(false)),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("LoaiViPham", "Loai vi pham"),
                new FieldDefinition("MoTa", "Mo ta", typeof(string), false, null, true),
                new FieldDefinition("PhiBoiThuong", "Phi boi thuong", typeof(decimal)),
                new FieldDefinition("TruVaoCoc", "Tru vao coc", typeof(bool)),
                new FieldDefinition("TinhTrang", "Tinh trang", typeof(string), false,
                    new[] { "ChoXuLy", "DaThanhToan", "DaKhauTru" }),
                new FieldDefinition("NgayGhiNhan", "Ngay ghi nhan", typeof(System.DateTime), true)
            };
        }

        private static List<ComboOption> TaoHopDongOptions(bool chiHopDongHieuLuc)
        {
            var options = new List<ComboOption> { new ComboOption(string.Empty, "(Chon hop dong)") };
            var hopDongs = new HopDongService().LayTatCa();
            if (chiHopDongHieuLuc)
                hopDongs = hopDongs.Where(h => h.TrangThai == "HieuLuc");

            options.AddRange(hopDongs
                .OrderBy(h => h.MaHopDong)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} - Can ho {1} - Khach {2} - {3}",
                        h.MaHopDong, h.MaCanHo, h.MaKhach, h.TrangThai))));
            return options;
        }

        protected override IEnumerable<PhieuXuLyViPham> GetItems()
        {
            var items = _service.LayTatCa().ToList();
            GanThongTinHienThi(items);
            return items;
        }

        protected override bool AddItem(PhieuXuLyViPham item, out string error)
        {
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            if (item.TruVaoCoc) item.TinhTrang = "ChoXuLy";
            return _service.GhiNhan(item, out error);
        }

        protected override void OnAfterAdd()
        {
            NapHopDongOptions(true);
        }

        private void NapHopDongOptions(bool chiHopDongHieuLuc)
        {
            var editor = GetEditor("MaHopDong") as ComboBox;
            if (editor == null) return;
            editor.DataSource = TaoHopDongOptions(chiHopDongHieuLuc);
            editor.DisplayMember = "Display";
            editor.ValueMember = "Value";
            if (editor.Items.Count > 0) editor.SelectedIndex = 0;
        }

        protected override bool UpdateItem(PhieuXuLyViPham item, out string error)
        {
            return _service.CapNhat(item, out error);
        }

        protected override bool DeleteItem(PhieuXuLyViPham item, out string error)
        {
            return _service.Xoa(item, out error);
        }

        protected override void AfterGridBound()
        {
            GridDisplayHelper.AddTextColumn(Grid, "TenCanHo", "Tên căn hộ", 2, 125);
            GridDisplayHelper.AddTextColumn(Grid, "TenToa", "Tên tòa", 3, 115);
            GridDisplayHelper.AddTextColumn(Grid, "TenNhanVien", "Tên nhân viên", 4, 130);

            if (!Grid.Columns.Contains("TenCanHo"))
            {
                Grid.Columns.Insert(2, new DataGridViewTextBoxColumn
                {
                    Name = "TenCanHo",
                    HeaderText = "Tên căn hộ",
                    ReadOnly = true
                });
            }
            if (!Grid.Columns.Contains("TenToa"))
            {
                Grid.Columns.Insert(3, new DataGridViewTextBoxColumn
                {
                    Name = "TenToa",
                    HeaderText = "Tên tòa",
                    ReadOnly = true
                });
            }

            var hopDongService = new HopDongService();
            var canHoService = new CanHoService();
            var toaService = new ToaService();

            var canHos = canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = toaService.LayTatCa().ToDictionary(t => t.MaToa);
            var hopDongs = hopDongService.LayTatCa().ToDictionary(h => h.MaHopDong);
            var nhanViens = new NhanVienPhanQuyenService().LayNhanVien()
                .GroupBy(n => n.MaNhanVien)
                .ToDictionary(g => g.Key, g => g.First().HoTen);

            foreach (DataGridViewRow row in Grid.Rows)
            {
                var viPham = row.DataBoundItem as PhieuXuLyViPham;
                if (viPham == null) continue;
                row.Cells["TenNhanVien"].Value = TenNhanVien(viPham.MaNhanVien, nhanViens);

                HopDongEntity hopDong;
                if (hopDongs.TryGetValue(viPham.MaHopDong, out hopDong))
                {
                    CanHo canHo;
                    if (canHos.TryGetValue(hopDong.MaCanHo, out canHo))
                    {
                        row.Cells["TenCanHo"].Value = "Căn " + canHo.SoCanHo;
                        Toa toa;
                        row.Cells["TenToa"].Value = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
                        continue;
                    }
                }
                row.Cells["TenCanHo"].Value = string.Empty;
                row.Cells["TenToa"].Value = string.Empty;
            }

            GridDisplayHelper.HideColumn(Grid, "MaNhanVien");
            GridDisplayHelper.SetFillWeight(Grid, "MaViPham", 90);
            GridDisplayHelper.SetFillWeight(Grid, "MaHopDong", 95);
            GridDisplayHelper.SetFillWeight(Grid, "TenCanHo", 125);
            GridDisplayHelper.SetFillWeight(Grid, "TenToa", 110);
            GridDisplayHelper.SetFillWeight(Grid, "TenNhanVien", 130);
            GridDisplayHelper.SetFillWeight(Grid, "LoaiViPham", 120);
            GridDisplayHelper.SetFillWeight(Grid, "MoTa", 155);
            GridDisplayHelper.SetFillWeight(Grid, "TinhTrang", 95);
            GridDisplayHelper.SetMoneyColumn(Grid, "PhiBoiThuong");
            GridDisplayHelper.BalanceGrid(Grid);
        }

        private static string TenNhanVien(string maNhanVien, IDictionary<string, string> nhanViens)
        {
            if (string.IsNullOrWhiteSpace(maNhanVien)) return "Admin";
            string hoTen;
            return nhanViens.TryGetValue(maNhanVien, out hoTen) ? hoTen : maNhanVien;
        }

        private static void GanThongTinHienThi(IEnumerable<PhieuXuLyViPham> items)
        {
            var hopDongService = new HopDongService();
            var canHoService = new CanHoService();
            var toaService = new ToaService();
            var canHos = canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = toaService.LayTatCa().ToDictionary(t => t.MaToa);
            var hopDongs = hopDongService.LayTatCa().ToDictionary(h => h.MaHopDong);
            var nhanViens = new NhanVienPhanQuyenService().LayNhanVien()
                .GroupBy(n => n.MaNhanVien)
                .ToDictionary(g => g.Key, g => g.First().HoTen);

            foreach (var viPham in items)
            {
                viPham.TenNhanVien = TenNhanVien(viPham.MaNhanVien, nhanViens);
                viPham.TenCanHo = string.Empty;
                viPham.TenToa = string.Empty;

                HopDongEntity hopDong;
                if (!hopDongs.TryGetValue(viPham.MaHopDong, out hopDong)) continue;
                CanHo canHo;
                if (!canHos.TryGetValue(hopDong.MaCanHo, out canHo)) continue;

                viPham.TenCanHo = "Căn " + canHo.SoCanHo;
                Toa toa;
                viPham.TenToa = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
            }
        }
    }
}
