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
            AddCommandButton("Chap thuan", BtnChapThuan_Click);
            AddCommandButton("Tu choi", BtnTuChoi_Click);
            var hopDongEditor = GetEditor("MaHopDong") as ComboBox;
            if (hopDongEditor != null)
                hopDongEditor.SelectedIndexChanged += delegate { DienNgayKetThucCu(); };
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var hopDongOptions = new HopDongService().LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .OrderBy(h => h.NgayKetThuc)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} | Khach {1} | Phong {2} | Het han {3:dd/MM/yyyy}",
                        h.MaHopDong, h.MaKhach, h.MaCanHo, h.NgayKetThuc)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaGiaHan", "Ma gia han", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Ma hop dong", hopDongOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("NgayKetThucCu", "Ngay ket thuc cu", typeof(DateTime), true),
                new FieldDefinition("NgayKetThucMoi", "Ngay ket thuc moi", typeof(DateTime)),
                new FieldDefinition("TrangThai", "Trang thai", typeof(string), false,
                    new[] { "ChoXetDuyet", "ChapThuan", "TuChoi" }),
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

        protected override IEnumerable<GiaHanHopDong> GetItems() { return _service.LayTatCa(); }

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
    }
}
