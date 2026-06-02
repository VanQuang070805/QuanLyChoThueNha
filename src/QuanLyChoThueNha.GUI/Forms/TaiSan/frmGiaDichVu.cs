using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TaiSan
{
    public class frmGiaDichVu : CrudFormBase<GiaDichVu>
    {
        private readonly GiaDichVuService _service = new GiaDichVuService();

        public frmGiaDichVu() : base("Quản lý Giá dịch vụ", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var toaService = new ToaService();
            var toaOptions = toaService.LayTatCa()
                .OrderBy(t => t.TenToa)
                .Select(t => new ComboOption(t.MaToa, t.TenToa))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaGiaDichVu", "Mã giá", typeof(string), true),
                FieldDefinition.Lookup("MaToa", "Tòa nhà", toaOptions),
                new FieldDefinition("GiaDien", "Giá điện", typeof(decimal)),
                new FieldDefinition("GiaNuoc", "Giá nước", typeof(decimal)),
                new FieldDefinition("GiaDichVuChung", "Giá dịch vụ chung", typeof(decimal)),
                new FieldDefinition("NgayApDung", "Ngày áp dụng", typeof(DateTime)),
                new FieldDefinition("NgayKetThuc", "Ngày kết thúc", typeof(DateTime?)),
                new FieldDefinition("DangApDung", "Đang áp dụng", typeof(bool))
            };
        }

        protected override IEnumerable<GiaDichVu> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(GiaDichVu item, out string error)
        {
            if (item.NgayApDung == DateTime.MinValue) item.NgayApDung = DateTime.Today;
            return _service.Them(item, out error);
        }

        protected override bool UpdateItem(GiaDichVu item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(GiaDichVu item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }

        protected override void AfterGridBound()
        {
            foreach (DataGridViewRow row in Grid.Rows)
            {
                var gdv = row.DataBoundItem as GiaDichVu;
                if (gdv != null && gdv.DangApDung)
                {
                    row.DefaultCellStyle.BackColor = Color.Honeydew;
                }
            }
        }
    }
}
