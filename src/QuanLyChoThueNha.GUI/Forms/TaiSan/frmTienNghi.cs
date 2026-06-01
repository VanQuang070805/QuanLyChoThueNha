using System.Collections.Generic;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TaiSan
{
    public class frmTienNghi : CrudFormBase<TienNghi>
    {
        private readonly TienNghiService _service = new TienNghiService();

        public frmTienNghi() : base("Quan ly Tien nghi", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            return new[]
            {
                new FieldDefinition("MaTienNghi", "Ma tien nghi", typeof(string), true),
                new FieldDefinition("MaAdmin", "Ma admin", typeof(string), true),
                new FieldDefinition("TenTienNghi", "Ten tien nghi"),
                new FieldDefinition("MoTa", "Mo ta", typeof(string), false, null, true)
            };
        }

        protected override IEnumerable<TienNghi> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(TienNghi item, out string error)
        {
            var maAdmin = SessionContext.LaAdmin ? SessionContext.MaNguoiDung : null;
            return _service.Them(item.TenTienNghi, item.MoTa, maAdmin, out error);
        }

        protected override bool UpdateItem(TienNghi item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(TienNghi item, out string error)
        {
            return _service.XoaTienNghi(item.MaTienNghi, out error);
        }
    }
}
