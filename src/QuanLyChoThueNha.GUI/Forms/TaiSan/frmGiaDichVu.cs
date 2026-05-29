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

        public frmGiaDichVu() : base("Quan ly Gia dich vu", Fields())
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
                new FieldDefinition("MaGiaDichVu", "Ma gia", typeof(string), true),
                FieldDefinition.Lookup("MaToa", "Toa nha", toaOptions),
                new FieldDefinition("GiaDien", "Gia dien", typeof(decimal)),
                new FieldDefinition("GiaNuoc", "Gia nuoc", typeof(decimal)),
                new FieldDefinition("GiaDichVuChung", "Gia dich vu chung", typeof(decimal)),
                new FieldDefinition("NgayApDung", "Ngay ap dung", typeof(DateTime)),
                new FieldDefinition("NgayKetThuc", "Ngay ket thuc", typeof(DateTime?)),
                new FieldDefinition("DangApDung", "Dang ap dung", typeof(bool))
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
