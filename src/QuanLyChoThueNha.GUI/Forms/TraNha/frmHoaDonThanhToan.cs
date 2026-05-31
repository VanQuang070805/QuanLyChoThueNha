using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;
using HopDongEntity = QuanLyChoThueNha.Model.Entities.HopDong;

namespace QuanLyChoThueNha.GUI.Forms.TraNha
{
    public class frmHoaDonThanhToan : CrudFormBase<HoaDonThanhToan>
    {
        private readonly HoaDonThanhToanService _service = new HoaDonThanhToanService();
        private readonly HopDongService _hopDongService = new HopDongService();

        public frmHoaDonThanhToan() : base("Quan ly Hoa don thanh toan", Fields())
        {
            AddCommandButton("Tinh dien/nuoc/dich vu", BtnTinhDichVu_Click);
            AddCommandButton("Ghi nhan thanh toan", BtnThanhToan_Click);
            PrePopulateNhanVien();
        }

        protected override void OnAfterAdd() { PrePopulateNhanVien(); }

        private void PrePopulateNhanVien()
        {
            SetEditorValue("MaNhanVienThu",
                SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : "(Admin)");
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var hopDongService = new HopDongService();
            var khachService = new KhachThueService();
            var khachById = khachService.LayTatCa()
                .GroupBy(k => k.MaKhach)
                .ToDictionary(g => g.Key, g => g.First());
            var hopDongOptions = hopDongService.LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .OrderBy(h => LayTenKhach(h, khachById))
                .ThenBy(h => h.MaCanHo)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} | {1} | Phong {2} | {3:dd/MM/yyyy}-{4:dd/MM/yyyy}",
                        h.MaHopDong,
                        LayTenKhach(h, khachById),
                        h.MaCanHo,
                        h.NgayBatDau,
                        h.NgayKetThuc)))
                .ToList();
            var viPhamOptions = new List<ComboOption> { new ComboOption(string.Empty, "(Khong gan vi pham)") };
            viPhamOptions.AddRange(new PhieuXuLyViPhamService().LayTatCa()
                .Where(v => v.TinhTrang == "ChoXuLy")
                .OrderBy(v => v.MaViPham)
                .Select(v => new ComboOption(v.MaViPham,
                    string.Format("{0} | Hop dong {1} | {2:N0}", v.MaViPham, v.MaHopDong, v.PhiBoiThuong))));
            var kyOptions = Enumerable.Range(1, 12)
                .Select(m => string.Format("{0:00}/{1}", m, DateTime.Today.Year))
                .ToArray();

            return new[]
            {
                new FieldDefinition("MaHoaDon", "Ma hoa don", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Hop dong", hopDongOptions),
                new FieldDefinition("Phong", "Phong", typeof(string), true),
                new FieldDefinition("MaNhanVienThu", "Ma nhan vien thu", typeof(string), true),
                FieldDefinition.Lookup("MaViPham", "Ma vi pham", viPhamOptions),
                new FieldDefinition("KyThanhToan", "Ky thanh toan", typeof(string), false, kyOptions),
                // BỔ SUNG (Bước 3): các trường chỉ số điện/nước. ChiSoCu tự điền, chỉ nhập ChiSoMoi.
                new FieldDefinition("ChiSoDienMoi", "Chi so dien moi", typeof(double)),
                new FieldDefinition("ChiSoNuocMoi", "Chi so nuoc moi", typeof(double)),
                new FieldDefinition("SoTienPhaiTra", "So tien phai tra", typeof(decimal)),
                new FieldDefinition("SoTienDaTra", "So tien da tra", typeof(decimal)),
                new FieldDefinition("NgayDaoHan", "Ngay dao han", typeof(DateTime)),
                new FieldDefinition("NgayThanhToan", "Ngay thanh toan", typeof(DateTime?)),
                new FieldDefinition("TrangThai", "Trang thai", typeof(string), false,
                    new[] { "ChuaTra", "DaTra", "TraThieu", "QuaHan" }),
                new FieldDefinition("PhuongThucThanhToan", "Phuong thuc")
            };
        }

        private static string LayTenKhach(HopDongEntity hopDong, IDictionary<string, KhachThue> khachById)
        {
            if (hopDong == null || string.IsNullOrWhiteSpace(hopDong.MaKhach))
                return "Khach khong ro";

            KhachThue khach;
            return khachById.TryGetValue(hopDong.MaKhach, out khach)
                ? string.Format("{0} ({1})", khach.HoTen, khach.MaKhach)
                : hopDong.MaKhach;
        }

        protected override IEnumerable<HoaDonThanhToan> GetItems()
        {
            var items = _service.LayTatCa().ToList();
            foreach (var item in items)
            {
                var hopDong = _hopDongService.LayTheoMa(item.MaHopDong);
                item.Phong = hopDong == null ? string.Empty : hopDong.MaCanHo;
            }
            return items;
        }

        protected override bool AddItem(HoaDonThanhToan item, out string error)
        {
            item.MaNhanVienThu = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            item.MaViPham = string.IsNullOrWhiteSpace(item.MaViPham) ? null : item.MaViPham;
            GanChiSoCu(item);
            return _service.TaoHoaDon(item, out error);
        }

        protected override bool UpdateItem(HoaDonThanhToan item, out string error)
        {
            error = string.Empty;
            item.MaViPham = string.IsNullOrWhiteSpace(item.MaViPham) ? null : item.MaViPham;
            GanChiSoCu(item);
            _service.Sua(item);
            return true;  // exceptions propagate to CrudFormBase catch block
        }

        protected override bool DeleteItem(HoaDonThanhToan item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            var item = CurrentItem;
            if (item == null) { ShowError("Chon hoa don can thanh toan."); return; }
            string error;
            var maNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            if (!_service.ThanhToan(item.MaHoaDon, item.SoTienDaTra, item.PhuongThucThanhToan, maNhanVien, out error))
            {
                ShowError(error);
                return;
            }
            ReloadData();
        }

        protected override void AfterGridBound()
        {
            ThemCotPhong();
            AnCotKyThuat();

            foreach (DataGridViewRow row in Grid.Rows)
            {
                var hd = row.DataBoundItem as HoaDonThanhToan;
                if (hd != null && (hd.TrangThai == "QuaHan" ||
                    (hd.TrangThai == "ChuaTra" && hd.NgayDaoHan < DateTime.Today)))
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                }
            }
        }

        private void ThemCotPhong()
        {
            if (!Grid.Columns.Contains("Phong"))
            {
                Grid.Columns.Insert(2, new DataGridViewTextBoxColumn
                {
                    Name = "Phong",
                    HeaderText = "Phong",
                    ReadOnly = true
                });
            }

            foreach (DataGridViewRow row in Grid.Rows)
            {
                var hoaDon = row.DataBoundItem as HoaDonThanhToan;
                if (hoaDon == null) continue;
                var hopDong = _hopDongService.LayTheoMa(hoaDon.MaHopDong);
                row.Cells["Phong"].Value = hopDong == null ? string.Empty : hopDong.MaCanHo;
            }
        }

        private void AnCotKyThuat()
        {
            var hiddenColumns = new[]
            {
                "ChiSoDienCu",
                "ChiSoDienMoi",
                "ChiSoNuocCu",
                "ChiSoNuocMoi",
                "MaNguoiThaoTac",
                "VaiTroNguoiThaoTac",
                "MaLoaiHoaDon"
            };

            foreach (var columnName in hiddenColumns)
            {
                if (Grid.Columns.Contains(columnName))
                {
                    Grid.Columns[columnName].Visible = false;
                }
            }
        }

        // Lấy mã hợp đồng đang chọn trên combo (dùng chung cho nhiều nút).
        private string LayMaHopDongDangChon()
        {
            var hopDongEditor = GetEditor("MaHopDong") as ComboBox;
            return hopDongEditor == null || hopDongEditor.SelectedValue == null
                ? string.Empty
                : hopDongEditor.SelectedValue.ToString();
        }

        // Đọc giá trị double từ một editor NumericUpDown trên form.
        private double LayChiSoTuEditor(string propertyName)
        {
            var editor = GetEditor(propertyName) as NumericUpDown;
            return editor == null ? 0 : (double)editor.Value;
        }

        // ===== BỔ SUNG (Bước 3): tự động điền chỉ số CŨ từ hóa đơn kỳ liền trước =====
        private void GanChiSoCu(HoaDonThanhToan hoaDon)
        {
            if (hoaDon == null || string.IsNullOrWhiteSpace(hoaDon.MaHopDong)) return;

            double dienCu, nuocCu;
            _service.LayChiSoKyTruoc(hoaDon.MaHopDong, out dienCu, out nuocCu);
            hoaDon.ChiSoDienCu = dienCu;
            hoaDon.ChiSoNuocCu = nuocCu;
        }

        private void BtnTinhDichVu_Click(object sender, EventArgs e)
        {
            var maHopDong = LayMaHopDongDangChon();
            if (string.IsNullOrWhiteSpace(maHopDong))
            {
                ShowError("Chon hop dong truoc khi tinh tien dien/nuoc/dich vu.");
                return;
            }

            // Chỉ số cũ lấy từ form (đã điền sẵn qua nút "Lay chi so ky truoc"),
            // chỉ số mới do nhân viên nhập trực tiếp vào ô ChiSoDienMoi / ChiSoNuocMoi.
            double dienCu, nuocCu;
            _service.LayChiSoKyTruoc(maHopDong, out dienCu, out nuocCu);
            double dienMoi = LayChiSoTuEditor("ChiSoDienMoi");
            double nuocMoi = LayChiSoTuEditor("ChiSoNuocMoi");

            string error;
            var amount = _service.TinhTienTheoChiSo(maHopDong, dienCu, dienMoi, nuocCu, nuocMoi, true, out error);
            if (!string.IsNullOrWhiteSpace(error))
            {
                ShowError(error);
                return;
            }

            SetEditorValue("SoTienPhaiTra", amount);
            SetEditorValue("SoTienDaTra", 0);
            ShowInfo(string.Format("Tieu thu: Dien {0} kWh, Nuoc {1} m3. Thanh tien: {2:N0} d.",
                dienMoi - dienCu, nuocMoi - nuocCu, amount));
        }
    }
}
