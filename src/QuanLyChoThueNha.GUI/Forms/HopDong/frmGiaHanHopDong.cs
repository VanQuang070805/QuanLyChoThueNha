using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.HopDong
{
    public class frmGiaHanHopDong : CrudFormBase<GiaHanHopDong>
    {
        private readonly GiaHanHopDongService _service = new GiaHanHopDongService();

        public frmGiaHanHopDong() : base("Quan ly Gia han hop dong", Fields())
        {
            var hopDongEditor = GetEditor("MaHopDong") as ComboBox;
            if (hopDongEditor != null)
                hopDongEditor.SelectedIndexChanged += delegate { DienNgayKetThucCu(); };
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var hopDongOptions = new HopDongService().LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .Select(h => new
                {
                    HopDong = h,
                    Khach = new KhachThueService().LayTheoMa(h.MaKhach),
                    CanHo = new CanHoService().LayTheoMa(h.MaCanHo)
                })
                .OrderBy(x => x.HopDong.NgayKetThuc)
                .Select(x => new ComboOption(x.HopDong.MaHopDong,
                    string.Format("{0} | {1} | {2} | Het han {3:dd/MM/yyyy}",
                        x.HopDong.MaHopDong,
                        x.Khach == null ? x.HopDong.MaKhach : x.Khach.HoTen,
                        x.CanHo == null ? x.HopDong.MaCanHo : "Can " + x.CanHo.SoCanHo,
                        x.HopDong.NgayKetThuc)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaGiaHan", "Ma gia han", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Ma hop dong", hopDongOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("NgayKetThucMoi", "Ngay ket thuc moi", typeof(DateTime)),
                new FieldDefinition("NgayYeuCau", "Ngay yeu cau", typeof(DateTime), true),
                new FieldDefinition("NgayDuyet", "Ngay duyet", typeof(DateTime?))
            };
        }

        private void DienNgayKetThucCu()
        {
            var hopDongEditor = GetEditor("MaHopDong") as ComboBox;
            if (hopDongEditor == null || hopDongEditor.SelectedValue == null) return;

            var hopDong = new HopDongService().LayTheoMa(hopDongEditor.SelectedValue.ToString());
            if (hopDong != null)
                SetEditorValue("NgayKetThucCu", hopDong.NgayKetThuc);
        }

        protected override IEnumerable<GiaHanHopDong> GetItems()
        {
            var items = _service.LayTatCa().ToList();
            GanThongTinHienThi(items);
            return items;
        }

        protected override bool AddItem(GiaHanHopDong item, out string error)
        {
            var maNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            return _service.YeuCauGiaHan(item.MaHopDong, item.NgayKetThucMoi, maNhanVien, out error);
        }

        protected override bool UpdateItem(GiaHanHopDong item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(GiaHanHopDong item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }

        private void BtnChapThuan_Click(object sender, EventArgs e)
        {
            var item = CurrentItem;
            if (item == null) { ShowError("Chon yeu cau gia han."); return; }
            string error;
            if (!_service.ChapThuan(item.MaGiaHan, out error)) { ShowError(error); return; }
            ReloadData();
        }

        private void BtnTuChoi_Click(object sender, EventArgs e)
        {
            var item = CurrentItem;
            if (item == null) { ShowError("Chon yeu cau gia han."); return; }
            item.TrangThai = "TuChoi";
            item.NgayDuyet = DateTime.Now;
            _service.Sua(item);
            ReloadData();
        }

        protected override void AfterGridBound()
        {
            GridDisplayHelper.AddTextColumn(Grid, "TenCanHo", "Tên căn hộ", 2, 125);
            GridDisplayHelper.AddTextColumn(Grid, "TenToa", "Tên tòa", 3, 115);
            GridDisplayHelper.AddTextColumn(Grid, "TenNhanVien", "Tên nhân viên", 4, 130);
            GridDisplayHelper.HideColumn(Grid, "MaNhanVien");
            GridDisplayHelper.SetFillWeight(Grid, "MaGiaHan", 90);
            GridDisplayHelper.SetFillWeight(Grid, "MaHopDong", 95);
            GridDisplayHelper.SetFillWeight(Grid, "TenCanHo", 125);
            GridDisplayHelper.SetFillWeight(Grid, "TenToa", 110);
            GridDisplayHelper.SetFillWeight(Grid, "TenNhanVien", 130);
            GridDisplayHelper.SetFillWeight(Grid, "TrangThai", 105);
            GridDisplayHelper.BalanceGrid(Grid);
        }

        private static void GanThongTinHienThi(IEnumerable<GiaHanHopDong> items)
        {
            var hopDongService = new HopDongService();
            var canHoService = new CanHoService();
            var toaService = new ToaService();
            var hopDongs = hopDongService.LayTatCa().ToDictionary(h => h.MaHopDong);
            var canHos = canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = toaService.LayTatCa().ToDictionary(t => t.MaToa);
            var nhanViens = new NhanVienPhanQuyenService().LayNhanVien()
                .GroupBy(n => n.MaNhanVien)
                .ToDictionary(g => g.Key, g => g.First().HoTen);

            foreach (var giaHan in items)
            {
                giaHan.TenNhanVien = TenNhanVien(giaHan.MaNhanVien, nhanViens);
                giaHan.TenCanHo = string.Empty;
                giaHan.TenToa = string.Empty;

                QuanLyChoThueNha.Model.Entities.HopDong hopDong;
                if (!hopDongs.TryGetValue(giaHan.MaHopDong, out hopDong)) continue;
                CanHo canHo;
                if (!canHos.TryGetValue(hopDong.MaCanHo, out canHo)) continue;

                giaHan.TenCanHo = "Căn " + canHo.SoCanHo;
                Toa toa;
                giaHan.TenToa = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
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
