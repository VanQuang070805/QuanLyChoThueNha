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
            HideDeleteButton();
            AddCommandButton("Tinh dien/nuoc/dich vu", BtnTinhDichVu_Click);
            AddCommandButton("Ghi nhan thanh toan", BtnThanhToan_Click);
            var hopDongEditor = GetEditor("MaHopDong") as ComboBox;
            if (hopDongEditor != null)
                hopDongEditor.SelectedIndexChanged += delegate { CapNhatTheoHopDongDangChon(); };
            var kyEditor = GetEditor("KyThanhToan") as ComboBox;
            if (kyEditor != null)
                kyEditor.SelectedIndexChanged += delegate { CapNhatNgayDaoHanTheoKy(); };
            PrePopulateNhanVien();
            CapNhatTheoHopDongDangChon();
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
            var canHoById = new CanHoService().LayTatCa()
                .GroupBy(c => c.MaCanHo)
                .ToDictionary(g => g.Key, g => g.First());
            var hopDongOptions = hopDongService.LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .OrderBy(h => LayTenKhach(h, khachById))
                .ThenBy(h => h.MaCanHo)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} | {1} | {2} | {3:dd/MM/yyyy}-{4:dd/MM/yyyy}",
                        h.MaHopDong,
                        LayTenKhach(h, khachById),
                        LayTenCanHo(h.MaCanHo, canHoById),
                        h.NgayBatDau,
                        h.NgayKetThuc)))
                .ToList();
            var kyOptions = hopDongService.LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .SelectMany(TaoKyTrongHopDong)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            return new[]
            {
                new FieldDefinition("MaHoaDon", "Ma hoa don", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Hop dong", hopDongOptions),
                new FieldDefinition("Phong", "Can ho", typeof(string), true),
                new FieldDefinition("MaNhanVienThu", "Ma nhan vien thu", typeof(string), true),
                new FieldDefinition("KyThanhToan", "Ky thanh toan", typeof(string), false, kyOptions),
                // BỔ SUNG (Bước 3): các trường chỉ số điện/nước. ChiSoCu tự điền, chỉ nhập ChiSoMoi.
                new FieldDefinition("ChiSoDienMoi", "Chi so dien moi", typeof(double)),
                new FieldDefinition("ChiSoNuocMoi", "Chi so nuoc moi", typeof(double)),
                new FieldDefinition("SoTienPhaiTra", "So tien phai tra", typeof(decimal), true),
                new FieldDefinition("SoTienDaTra", "So tien da tra", typeof(decimal)),
                new FieldDefinition("NgayDaoHan", "Ngay dao han", typeof(DateTime)),
                new FieldDefinition("NgayThanhToan", "Ngay thanh toan", typeof(DateTime?)),
                new FieldDefinition("TrangThai", "Trang thai", typeof(string), true,
                    new[] { "ChuaTra", "DaTra", "TraThieu", "QuaHan" }),
                new FieldDefinition("PhuongThucThanhToan", "Phuong thuc", typeof(string), false,
                    new[] { "ChuyenKhoan", "TienMat" })
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

        private static string LayTenCanHo(string maCanHo, IDictionary<string, CanHo> canHoById)
        {
            CanHo canHo;
            return canHoById.TryGetValue(maCanHo, out canHo)
                ? string.Format("Can {0} ({1})", canHo.SoCanHo, maCanHo)
                : maCanHo;
        }

        private static IEnumerable<string> TaoKyTrongHopDong(HopDongEntity hopDong)
        {
            if (hopDong == null) yield break;
            var current = new DateTime(hopDong.NgayBatDau.Year, hopDong.NgayBatDau.Month, 1);
            var last = new DateTime(hopDong.NgayKetThuc.Year, hopDong.NgayKetThuc.Month, 1);
            while (current <= last)
            {
                yield return current.ToString("MM/yyyy");
                current = current.AddMonths(1);
            }
        }

        protected override IEnumerable<HoaDonThanhToan> GetItems()
        {
            var items = _service.LayTatCa().ToList();
            foreach (var item in items)
            {
                var hopDong = _hopDongService.LayTheoMa(item.MaHopDong);
                item.Phong = hopDong == null ? string.Empty : TenCanHo(hopDong.MaCanHo);
            }
            return items;
        }

        protected override bool AddItem(HoaDonThanhToan item, out string error)
        {
            item.MaNhanVienThu = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            item.MaViPham = null;
            item.SoTienPhaiTra = LayTienTuEditor("SoTienPhaiTra");
            GanChiSoCu(item);
            return _service.TaoHoaDon(item, out error);
        }

        protected override bool UpdateItem(HoaDonThanhToan item, out string error)
        {
            error = string.Empty;
            item.MaViPham = null;
            item.SoTienPhaiTra = LayTienTuEditor("SoTienPhaiTra");
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
                    HeaderText = "Tên căn hộ",
                    ReadOnly = true
                });
            }
            if (!Grid.Columns.Contains("Toa"))
            {
                Grid.Columns.Insert(3, new DataGridViewTextBoxColumn
                {
                    Name = "Toa",
                    HeaderText = "Tên tòa",
                    ReadOnly = true
                });
            }

            var canHoService = new CanHoService();
            var toaService = new ToaService();
            var canHos = canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = toaService.LayTatCa().ToDictionary(t => t.MaToa);

            foreach (DataGridViewRow row in Grid.Rows)
            {
                var hoaDon = row.DataBoundItem as HoaDonThanhToan;
                if (hoaDon == null) continue;
                var hopDong = _hopDongService.LayTheoMa(hoaDon.MaHopDong);
                if (hopDong != null)
                {
                    row.Cells["Phong"].Value = TenCanHo(hopDong.MaCanHo);
                    CanHo canHo;
                    if (canHos.TryGetValue(hopDong.MaCanHo, out canHo))
                    {
                        Toa toa;
                        row.Cells["Toa"].Value = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
                    }
                    else
                    {
                        row.Cells["Toa"].Value = string.Empty;
                    }
                }
                else
                {
                    row.Cells["Phong"].Value = string.Empty;
                    row.Cells["Toa"].Value = string.Empty;
                }
            }
        }

        private string TenCanHo(string maCanHo)
        {
            var canHo = new CanHoService().LayTheoMa(maCanHo);
            return canHo == null ? maCanHo : string.Format("Can {0}", canHo.SoCanHo);
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

        private void CapNhatTheoHopDongDangChon()
        {
            var maHopDong = LayMaHopDongDangChon();
            var hopDong = string.IsNullOrWhiteSpace(maHopDong) ? null : _hopDongService.LayTheoMa(maHopDong);
            if (hopDong == null) return;

            SetEditorValue("Phong", TenCanHo(hopDong.MaCanHo));
            var kyEditor = GetEditor("KyThanhToan") as ComboBox;
            if (kyEditor != null)
            {
                var current = kyEditor.SelectedItem == null ? null : kyEditor.SelectedItem.ToString();
                kyEditor.Items.Clear();
                var kys = TaoKyTrongHopDong(hopDong).ToArray();
                kyEditor.Items.AddRange(kys);
                if (kys.Length > 0)
                    kyEditor.SelectedItem = kys.Contains(current) ? current : kys[0];
            }
            CapNhatNgayDaoHanTheoKy();
        }

        private void CapNhatNgayDaoHanTheoKy()
        {
            var maHopDong = LayMaHopDongDangChon();
            var hopDong = string.IsNullOrWhiteSpace(maHopDong) ? null : _hopDongService.LayTheoMa(maHopDong);
            var kyEditor = GetEditor("KyThanhToan") as ComboBox;
            if (hopDong == null || kyEditor == null || kyEditor.SelectedItem == null) return;

            DateTime thang;
            if (!DateTime.TryParseExact("01/" + kyEditor.SelectedItem, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out thang)) return;
            var cuoiThang = thang.AddMonths(1).AddDays(-1);
            var ngayDaoHan = cuoiThang > hopDong.NgayKetThuc.Date ? hopDong.NgayKetThuc.Date : cuoiThang;
            if (ngayDaoHan < hopDong.NgayBatDau.Date) ngayDaoHan = hopDong.NgayBatDau.Date;
            SetEditorValue("NgayDaoHan", ngayDaoHan);
        }

        // Đọc giá trị double từ một editor NumericUpDown trên form.
        private double LayChiSoTuEditor(string propertyName)
        {
            var editor = GetEditor(propertyName) as NumericUpDown;
            return editor == null ? 0 : (double)editor.Value;
        }

        private decimal LayTienTuEditor(string propertyName)
        {
            var editor = GetEditor(propertyName) as NumericUpDown;
            return editor == null ? 0 : editor.Value;
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
                Math.Abs(dienMoi - dienCu), Math.Abs(nuocMoi - nuocCu), amount));
        }
    }
}
