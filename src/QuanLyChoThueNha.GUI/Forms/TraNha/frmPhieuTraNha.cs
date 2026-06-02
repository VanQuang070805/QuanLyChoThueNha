using System;
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
    public class frmPhieuTraNha : CrudFormBase<PhieuTraNha>
    {
        private readonly PhieuTraNhaService _service = new PhieuTraNhaService();

        public frmPhieuTraNha() : base("Quan ly Phieu tra nha", Fields())
        {
            HideDeleteButton();
            CauHinhNgayTra();
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var hopDongService = new HopDongService();
            var hopDongOptions = hopDongService.LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .OrderBy(h => h.MaHopDong)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} - {1} - {2}", h.MaHopDong, h.MaCanHo, h.TrangThai)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaPhieu", "Ma phieu", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Hop dong", hopDongOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("NgayTra", "Ngay tra", typeof(DateTime)),
                new FieldDefinition("TinhTrangNha", "Tinh trang nha", typeof(string), false, null, true),
                new FieldDefinition("TienHoanCoc", "Tien hoan coc (tu tinh)", typeof(decimal), true),
                new FieldDefinition("TienKhauTru", "Tien khau tru", typeof(decimal)),
                new FieldDefinition("GhiChu", "Ghi chu", typeof(string), false, null, true)
            };
        }

        protected override IEnumerable<PhieuTraNha> GetItems()
        {
            var items = _service.LayTatCa().ToList();
            GanThongTinHienThi(items);
            return items;
        }

        protected override bool AddItem(PhieuTraNha item, out string error)
        {
            // Gán mã người lập phiếu (NhanVien hoặc Admin) từ phiên đăng nhập.
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            if (item.NgayTra == DateTime.MinValue) item.NgayTra = DateTime.Today;
            if (!NgayTraHopLe(item.NgayTra, out error)) return false;
            // KHÔNG tự tính TienHoanCoc ở GUI nữa — toàn bộ logic hoàn cọc/khấu trừ vi phạm
            // đã được dồn về PhieuTraNhaService.LapPhieu để đảm bảo nhất quán.
            return _service.LapPhieu(item, out error);
        }

        protected override void OnAfterAdd()
        {
            CauHinhNgayTra();
        }

        protected override bool UpdateItem(PhieuTraNha item, out string error)
        {
            if (!NgayTraHopLe(item.NgayTra, out error)) return false;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(PhieuTraNha item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
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
                var phieu = row.DataBoundItem as PhieuTraNha;
                if (phieu == null) continue;
                row.Cells["TenNhanVien"].Value = TenNhanVien(phieu.MaNhanVien, nhanViens);

                HopDongEntity hopDong;
                if (hopDongs.TryGetValue(phieu.MaHopDong, out hopDong))
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
            GridDisplayHelper.SetFillWeight(Grid, "MaPhieu", 90);
            GridDisplayHelper.SetFillWeight(Grid, "MaHopDong", 95);
            GridDisplayHelper.SetFillWeight(Grid, "TenCanHo", 125);
            GridDisplayHelper.SetFillWeight(Grid, "TenToa", 110);
            GridDisplayHelper.SetFillWeight(Grid, "TenNhanVien", 130);
            GridDisplayHelper.SetFillWeight(Grid, "TinhTrangNha", 140);
            GridDisplayHelper.SetFillWeight(Grid, "GhiChu", 150);
            GridDisplayHelper.SetMoneyColumn(Grid, "TienHoanCoc");
            GridDisplayHelper.SetMoneyColumn(Grid, "TienKhauTru");
            GridDisplayHelper.BalanceGrid(Grid);
        }

        private static string TenNhanVien(string maNhanVien, IDictionary<string, string> nhanViens)
        {
            if (string.IsNullOrWhiteSpace(maNhanVien)) return "Admin";
            string hoTen;
            return nhanViens.TryGetValue(maNhanVien, out hoTen) ? hoTen : maNhanVien;
        }

        private void CauHinhNgayTra()
        {
            var picker = GetEditor("NgayTra") as DateTimePicker;
            if (picker == null) return;
            picker.MinDate = DateTime.Today;
            if (picker.Value.Date < DateTime.Today)
                picker.Value = DateTime.Today;
        }

        private static bool NgayTraHopLe(DateTime ngayTra, out string error)
        {
            error = string.Empty;
            if (ngayTra.Date >= DateTime.Today) return true;

            error = "Ngay tra khong duoc nho hon ngay hien tai.";
            return false;
        }

        private static void GanThongTinHienThi(IEnumerable<PhieuTraNha> items)
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

            foreach (var phieu in items)
            {
                phieu.TenNhanVien = TenNhanVien(phieu.MaNhanVien, nhanViens);
                phieu.TenCanHo = string.Empty;
                phieu.TenToa = string.Empty;

                HopDongEntity hopDong;
                if (!hopDongs.TryGetValue(phieu.MaHopDong, out hopDong)) continue;
                CanHo canHo;
                if (!canHos.TryGetValue(hopDong.MaCanHo, out canHo)) continue;

                phieu.TenCanHo = "Căn " + canHo.SoCanHo;
                Toa toa;
                phieu.TenToa = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
            }
        }
    }
}
