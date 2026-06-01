using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TaiSan
{
    public class frmToa : CrudFormBase<Toa>
    {
        private readonly ToaService _service = new ToaService();

        public frmToa() : base("Quan ly Toa nha", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var khuVucService = new KhuVucService();
            var khuVucOptions = khuVucService.LayTatCa()
                .OrderBy(k => k.ThanhPho)
                .ThenBy(k => k.Quan)
                .ThenBy(k => k.TenKhuVuc)
                .Select(k => new ComboOption(k.MaKhuVuc,
                    string.Format("{0} - {1}, {2}", k.TenKhuVuc, k.Quan, k.ThanhPho)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaToa", "Ma toa", typeof(string), true),
                FieldDefinition.Lookup("MaKhuVuc", "Khu vuc", khuVucOptions),
                new FieldDefinition("TenToa", "Ten toa"),
                new FieldDefinition("DiaChi", "Dia chi"),
                new FieldDefinition("SoTang", "So tang", typeof(int)),
                new FieldDefinition("MoTa", "Mo ta", typeof(string), false, null, true)
            };
        }

        protected override IEnumerable<Toa> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(Toa item, out string error)
        {
            return _service.Them(item.TenToa, item.MaKhuVuc, item.DiaChi, item.SoTang, item.MoTa, out error);
        }

        protected override bool UpdateItem(Toa item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(Toa item, out string error)
        {
            return _service.XoaToa(item.MaToa, out error);
        }
    }
}
