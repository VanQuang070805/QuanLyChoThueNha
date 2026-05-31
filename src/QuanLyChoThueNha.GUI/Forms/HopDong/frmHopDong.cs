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
        private string _trangThaiFilter = "TatCa";

        public frmHopDong() : base("Quan ly Hop dong", Fields())
        {
            AddCommandButton("Tat ca", delegate { _trangThaiFilter = "TatCa"; ReloadData(); });
            AddCommandButton("Hieu luc", delegate { _trangThaiFilter = "HieuLuc"; ReloadData(); });
            AddCommandButton("Sap het han", delegate { _trangThaiFilter = "SapHetHan"; ReloadData(); });
            AddCommandButton("Het han", delegate { _trangThaiFilter = "HetHan"; ReloadData(); });
            AddCommandButton("Da huy", delegate { _trangThaiFilter = "DaHuy"; ReloadData(); });
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
                new FieldDefinition("TrangThai", "Trang thai", typeof(string), true,
                    new[] { "HieuLuc", "HetHan", "DaHuy" }),
                new FieldDefinition("GhiChu", "Ghi chu", typeof(string), false, null, true),
                new FieldDefinition("NgayTao", "Ngay tao", typeof(DateTime), true)
            };
        }

        protected override IEnumerable<HopDongEntity> GetItems()
        {
            var data = _service.LayTatCa();
            if (_trangThaiFilter != "TatCa")
                data = data.Where(h => _service.TrangThaiHienThi(h) == _trangThaiFilter);
            return data;
        }

        protected override IEnumerable<string> GridColumnNames()
        {
            return new[]
            {
                "MaHopDong",
                "MaCanHo",
                "MaKhach",
                "NgayBatDau",
                "NgayKetThuc",
                "GiaThueChot",
                "TienCocChot",
                "TrangThai"
            };
        }

        protected override bool AddItem(HopDongEntity item, out string error)
        {
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            item.TrangThai = "HieuLuc";
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

                // Tô màu dựa trên mã trạng thái nội bộ
                if (status == "HetHan")
                    row.DefaultCellStyle.BackColor = Color.Gainsboro;
                else if (status == "SapHetHan")
                    row.DefaultCellStyle.BackColor = Color.MistyRose;

                // Hiển thị tiếng Việt trong cột TrangThai
                if (Grid.Columns.Contains("TrangThai"))
                    row.Cells["TrangThai"].ToolTipText = HienThiTrangThai(status);
            }
        }

        private static string ChuanHoaTrangThaiLuu(string trangThai)
        {
            var value = (trangThai ?? string.Empty).Trim();
            switch (value)
            {
                case "HieuLuc":
                case "HetHan":
                case "DaHuy":
                    return value;
                case "Hieu luc":
                    return "HieuLuc";
                case "Sap het han":
                    return "HieuLuc";
                case "Da het han":
                    return "HetHan";
                case "Da huy":
                    return "DaHuy";
                default:
                    return null;
            }
        }

        private static string HienThiTrangThai(string ma)
        {
            switch (ma)
            {
                case "HieuLuc":   return "Hieu luc";
                case "SapHetHan": return "Sap het han";
                case "HetHan":    return "Da het han";
                case "DaHuy":     return "Da huy";
                default:          return ma;
            }
        }
    }
}
