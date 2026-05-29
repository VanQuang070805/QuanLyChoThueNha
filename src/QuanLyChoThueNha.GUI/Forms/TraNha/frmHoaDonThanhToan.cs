using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TraNha
{
    public class frmHoaDonThanhToan : CrudFormBase<HoaDonThanhToan>
    {
        private readonly HoaDonThanhToanService _service = new HoaDonThanhToanService();

        public frmHoaDonThanhToan() : base("Quan ly Hoa don thanh toan", Fields())
        {
            AddCommandButton("Lay chi so ky truoc", BtnLayChiSoCu_Click);
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
            var hopDongOptions = hopDongService.LayTatCa()
                .OrderBy(h => h.MaHopDong)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} - {1} - {2}", h.MaHopDong, h.MaCanHo, h.TrangThai)))
                .ToList();
            var loaiHoaDonService = new LoaiHoaDonService();
            var loaiOptions = loaiHoaDonService.LayTatCa()
                .OrderBy(l => l.TenLoai)
                .Select(l => new ComboOption(l.MaLoaiHoaDon, l.TenLoai))
                .ToList();
            var kyOptions = Enumerable.Range(1, 12)
                .Select(m => string.Format("{0:00}/{1}", m, DateTime.Today.Year))
                .ToArray();

            return new[]
            {
                new FieldDefinition("MaHoaDon", "Ma hoa don", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Hop dong", hopDongOptions),
                FieldDefinition.Lookup("MaLoaiHoaDon", "Loai hoa don", loaiOptions),
                new FieldDefinition("MaNhanVienThu", "Ma nhan vien thu", typeof(string), true),
                new FieldDefinition("MaViPham", "Ma vi pham"),
                new FieldDefinition("KyThanhToan", "Ky thanh toan", typeof(string), false, kyOptions),
                // BỔ SUNG (Bước 3): các trường chỉ số điện/nước. ChiSoCu tự điền, chỉ nhập ChiSoMoi.
                new FieldDefinition("ChiSoDienCu", "Chi so dien cu", typeof(double), true),
                new FieldDefinition("ChiSoDienMoi", "Chi so dien moi", typeof(double)),
                new FieldDefinition("ChiSoNuocCu", "Chi so nuoc cu", typeof(double), true),
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

        protected override IEnumerable<HoaDonThanhToan> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(HoaDonThanhToan item, out string error)
        {
            item.MaNhanVienThu = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            return _service.TaoHoaDon(item, out error);
        }

        protected override bool UpdateItem(HoaDonThanhToan item, out string error)
        {
            error = string.Empty;
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
        private void BtnLayChiSoCu_Click(object sender, EventArgs e)
        {
            var maHopDong = LayMaHopDongDangChon();
            if (string.IsNullOrWhiteSpace(maHopDong))
            {
                ShowError("Chon hop dong truoc khi lay chi so ky truoc.");
                return;
            }
            double dienCu, nuocCu;
            _service.LayChiSoKyTruoc(maHopDong, out dienCu, out nuocCu);
            SetEditorValue("ChiSoDienCu", dienCu);
            SetEditorValue("ChiSoNuocCu", nuocCu);
            ShowInfo(string.Format("Da lay chi so ky truoc: Dien = {0}, Nuoc = {1}.", dienCu, nuocCu));
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
            double dienCu = LayChiSoTuEditor("ChiSoDienCu");
            double dienMoi = LayChiSoTuEditor("ChiSoDienMoi");
            double nuocCu = LayChiSoTuEditor("ChiSoNuocCu");
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
