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

namespace QuanLyChoThueNha.GUI.Forms.HopDong
{
    public class frmHopDong : CrudFormBase<HopDongEntity>
    {
        private readonly HopDongService _service = new HopDongService();

        public frmHopDong() : base("Quan ly Hop dong", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var phieuService = new PhieuDatTruocService();
            var canHoService = new CanHoService();
            var khachService = new KhachThueService();
            var phieuOptions = new List<ComboOption> { new ComboOption(string.Empty, "(Khong co phieu dat truoc)") };
            phieuOptions.AddRange(phieuService.LayTatCa()
                .Where(p => p.TrangThai == "ChoKy")
                .OrderBy(p => p.MaPhieuDatTruoc)
                .Select(p => new ComboOption(p.MaPhieuDatTruoc,
                    string.Format("{0} - {1}", p.MaPhieuDatTruoc, p.TrangThai))));
            var canHoOptions = canHoService.LayTatCa()
                .OrderBy(c => c.MaCanHo)
                .Select(c => new ComboOption(c.MaCanHo,
                    string.Format("{0} - Tang {1}", c.MaCanHo, c.TangSo)))
                .ToList();
            var khachOptions = khachService.LayTatCa()
                .OrderBy(k => k.HoTen)
                .Select(k => new ComboOption(k.MaKhach,
                    string.Format("{0} - {1}", k.HoTen, k.SoCMND)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaHopDong", "Ma hop dong", typeof(string), true),
                FieldDefinition.Lookup("MaPhieuDatTruoc", "Phieu dat truoc", phieuOptions),
                FieldDefinition.Lookup("MaCanHo", "Can ho", canHoOptions),
                FieldDefinition.Lookup("MaKhach", "Khach thue", khachOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("NgayBatDau", "Ngay bat dau", typeof(DateTime)),
                new FieldDefinition("NgayKetThuc", "Ngay ket thuc", typeof(DateTime)),
                new FieldDefinition("GiaThueChot", "Gia thue chot", typeof(decimal)),
                new FieldDefinition("TienCocChot", "Tien coc chot", typeof(decimal)),
                new FieldDefinition("TrangThai", "Trang thai", typeof(string), false,
                    new[] { "HieuLuc", "HetHan", "DaHuy" }),
                new FieldDefinition("GhiChu", "Ghi chu", typeof(string), false, null, true),
                new FieldDefinition("NgayTao", "Ngay tao", typeof(DateTime), true)
            };
        }

        protected override IEnumerable<HopDongEntity> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(HopDongEntity item, out string error)
        {
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : item.MaNhanVien;
            return _service.KyHopDong(item, item.MaPhieuDatTruoc, out error);
        }

        protected override bool UpdateItem(HopDongEntity item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(HopDongEntity item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }

        protected override void AfterGridBound()
        {
            foreach (DataGridViewRow row in Grid.Rows)
            {
                var hd = row.DataBoundItem as HopDongEntity;
                if (hd == null) continue;

                var status = _service.TrangThaiHienThi(hd);
                if (Grid.Columns.Contains("TrangThai"))
                    row.Cells["TrangThai"].Value = status;

                if (status == "HetHan")
                {
                    row.DefaultCellStyle.BackColor = Color.Gainsboro;
                }
                else if (status == "SapHetHan")
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                }
            }
        }
    }
}
