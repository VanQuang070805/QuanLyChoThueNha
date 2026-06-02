using System.Collections.Generic;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TaiSan
{
    public class frmLoaiCanHo : CrudFormBase<LoaiCanHo>
    {
        private readonly LoaiCanHoService _service = new LoaiCanHoService();

        public frmLoaiCanHo() : base("Quản lý Loại căn hộ", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            return new[]
            {
                new FieldDefinition("MaLoai", "Mã loại", typeof(string), true),
                new FieldDefinition("MaAdmin", "Mã admin", typeof(string), true),
                new FieldDefinition("TenLoai", "Tên loại"),
                new FieldDefinition("MoTa", "Mô tả", typeof(string), false, null, true)
            };
        }

        protected override IEnumerable<LoaiCanHo> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(LoaiCanHo item, out string error)
        {
            var maAdmin = SessionContext.LaAdmin ? SessionContext.MaNguoiDung : null;
            return _service.Them(item.TenLoai, item.MoTa, maAdmin, out error);
        }

        protected override bool UpdateItem(LoaiCanHo item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(LoaiCanHo item, out string error)
        {
            return _service.XoaLoaiCanHo(item.MaLoai, out error);
        }
    }
}
