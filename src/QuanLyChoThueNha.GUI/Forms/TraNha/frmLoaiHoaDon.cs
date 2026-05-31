using System.Collections.Generic;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TraNha
{
    public class frmLoaiHoaDon : CrudFormBase<LoaiHoaDon>
    {
        private readonly LoaiHoaDonService _service = new LoaiHoaDonService();

        public frmLoaiHoaDon() : base("Quan ly Loai hoa don", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            return new[]
            {
                new FieldDefinition("MaLoaiHoaDon", "Ma loai hoa don", typeof(string), true),
                new FieldDefinition("TenLoai", "Ten loai"),
                new FieldDefinition("MoTa", "Mo ta", typeof(string), false, null, true)
            };
        }

        protected override IEnumerable<LoaiHoaDon> GetItems()
        {
            return _service.LayTatCa();
        }

        protected override bool AddItem(LoaiHoaDon item, out string error)
        {
            return _service.Them(item, out error);
        }

        protected override bool UpdateItem(LoaiHoaDon item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(LoaiHoaDon item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }
    }
}
