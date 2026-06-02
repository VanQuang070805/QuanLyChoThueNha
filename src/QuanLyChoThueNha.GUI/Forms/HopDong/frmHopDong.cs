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
        private readonly CanHoService _canHoService = new CanHoService();
        private readonly PhieuDatTruocService _phieuDatTruocService = new PhieuDatTruocService();
        private readonly ComboBox _cboToaFilter = new ComboBox();
        private readonly ComboBox _cboTrangThaiFilter = new ComboBox();
        private string _trangThaiFilter = "TatCa";
        private bool _dangNapCanHo;

        public frmHopDong() : base("Quan ly Hop dong", Fields())
        {
            TaoBoLocToa();
            TaoBoLocTrangThai();
            var canHoEditor = GetEditor("MaCanHo") as ComboBox;
            if (canHoEditor != null)
                canHoEditor.SelectedIndexChanged += delegate { DienPhieuVaKhachTheoCanHo(); };
        }

        private void TaoBoLocToa()
        {
            var options = new List<ComboOption> { new ComboOption(string.Empty, "Tất cả tòa") };
            options.AddRange(new ToaService().LayTatCa()
                .OrderBy(t => t.TenToa)
                .Select(t => new ComboOption(t.MaToa, t.TenToa)));

            _cboToaFilter.DisplayMember = "Display";
            _cboToaFilter.ValueMember = "Value";
            _cboToaFilter.DataSource = options;
            _cboToaFilter.SelectedIndexChanged += delegate { NapCanHoTheoToa(); };
            AddSearchFilter("Tòa", _cboToaFilter, 130);
            NapCanHoTheoToa();
        }

        private void TaoBoLocTrangThai()
        {
            var options = new List<ComboOption>
            {
                new ComboOption("TatCa", "Tất cả"),
                new ComboOption("HieuLuc", "Hiệu lực"),
                new ComboOption("SapHetHan", "Sắp hết hạn"),
                new ComboOption("HetHan", "Hết hạn"),
                new ComboOption("DaHuy", "Đã hủy")
            };

            _cboTrangThaiFilter.DisplayMember = "Display";
            _cboTrangThaiFilter.ValueMember = "Value";
            _cboTrangThaiFilter.DataSource = options;
            _cboTrangThaiFilter.SelectedIndexChanged += delegate 
            {
                if (_cboTrangThaiFilter.SelectedValue != null)
                {
                    _trangThaiFilter = _cboTrangThaiFilter.SelectedValue.ToString();
                    ReloadData();
                }
            };
            AddSearchFilter("Trạng thái", _cboTrangThaiFilter, 140);
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var phieuService = new PhieuDatTruocService();
            var canHoService = new CanHoService();
            var khachService = new KhachThueService();
            var phieuOptions = new List<ComboOption> { new ComboOption(string.Empty, "(Khong co phieu dat truoc)") };
            var phieusChoKy = phieuService.LayTatCa()
                .Where(p => p.TrangThai == "ChoKy")
                .ToList();
            phieuOptions.AddRange(phieusChoKy
                .OrderBy(p => p.MaPhieuDatTruoc)
                .Select(p => new ComboOption(p.MaPhieuDatTruoc,
                    string.Format("{0} - {1}", p.MaPhieuDatTruoc, p.TrangThai))));
            var canHoCoPhieuChoKy = phieusChoKy.Select(p => p.MaCanHo).ToList();
            var canHoOptions = canHoService.LayTatCa()
                .Where(c => c.TinhTrang == "Trong" || canHoCoPhieuChoKy.Contains(c.MaCanHo))
                .OrderBy(c => c.MaCanHo)
                .Select(c => new ComboOption(c.MaCanHo,
                    string.Format("{0} - Toa {1} - Tang {2} - {3}", c.MaCanHo, c.MaToa, c.TangSo, c.TinhTrang)))
                .ToList();
            var khachOptions = khachService.LayTatCa()
                .OrderBy(k => k.HoTen)
                .Select(k => new ComboOption(k.MaKhach,
                    string.Format("{0} - {1}", k.HoTen, k.SoCMND)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaHopDong", "Ma hop dong", typeof(string), true),
                FieldDefinition.Lookup("MaPhieuDatTruoc", "Phieu dat truoc", phieuOptions, true),
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
            var phieuEditor = GetEditor("MaPhieuDatTruoc") as ComboBox;
            if (phieuEditor != null && phieuEditor.SelectedValue != null)
                item.MaPhieuDatTruoc = phieuEditor.SelectedValue.ToString();
            return _service.KyHopDong(item, item.MaPhieuDatTruoc, out error);
        }

        protected override bool UpdateItem(HopDongEntity item, out string error)
        {
            error = "Hop dong da ky khong nen sua truc tiep tren form nay. Hay lap gia han/phieu tra nha de thay doi trang thai.";
            return false;
        }

        protected override bool DeleteItem(HopDongEntity item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }

        private void DienPhieuVaKhachTheoCanHo()
        {
            if (_dangNapCanHo) return;
            var canHoEditor = GetEditor("MaCanHo") as ComboBox;
            if (canHoEditor == null || canHoEditor.SelectedValue == null) return;
            var maCanHo = canHoEditor.SelectedValue.ToString();
            var phieu = _phieuDatTruocService.LayTatCa()
                .Where(p => p.MaCanHo == maCanHo && p.TrangThai == PhieuDatTruocService.ChoKy)
                .OrderByDescending(p => p.NgayDatCoc)
                .FirstOrDefault();
            if (phieu != null)
            {
                SetEditorValue("MaPhieuDatTruoc", phieu.MaPhieuDatTruoc);
                SetEditorValue("MaKhach", phieu.MaKhach);
                var khachEditor = GetEditor("MaKhach");
                if (khachEditor != null) khachEditor.Enabled = false;
                return;
            }

            SetEditorValue("MaPhieuDatTruoc", string.Empty);
            var freeKhachEditor = GetEditor("MaKhach");
            if (freeKhachEditor != null) freeKhachEditor.Enabled = true;
        }

        private void NapCanHoTheoToa()
        {
            var canHoEditor = GetEditor("MaCanHo") as ComboBox;
            if (canHoEditor == null) return;

            var selectedToa = _cboToaFilter.SelectedValue == null ? string.Empty : _cboToaFilter.SelectedValue.ToString();
            var canHoCoPhieuChoKy = _phieuDatTruocService.LayTatCa()
                .Where(p => p.TrangThai == PhieuDatTruocService.ChoKy)
                .Select(p => p.MaCanHo)
                .ToList();

            var options = _canHoService.LayTatCa()
                .Where(c => string.IsNullOrWhiteSpace(selectedToa) || c.MaToa == selectedToa)
                .Where(c => c.TinhTrang == "Trong" || canHoCoPhieuChoKy.Contains(c.MaCanHo))
                .OrderBy(c => c.MaToa)
                .ThenBy(c => c.SoCanHo)
                .Select(c => new ComboOption(c.MaCanHo,
                    string.Format("{0} - Toa {1} - Tang {2} - {3}", c.MaCanHo, c.MaToa, c.TangSo, c.TinhTrang)))
                .ToList();

            _dangNapCanHo = true;
            canHoEditor.DataSource = options;
            canHoEditor.DisplayMember = "Display";
            canHoEditor.ValueMember = "Value";
            if (options.Count > 0) canHoEditor.SelectedIndex = 0;
            _dangNapCanHo = false;
            DienPhieuVaKhachTheoCanHo();
        }

        protected override void AfterGridBound()
        {
            if (!Grid.Columns.Contains("Toa"))
            {
                Grid.Columns.Insert(3, new DataGridViewTextBoxColumn
                {
                    Name = "Toa",
                    HeaderText = "Tên tòa",
                    ReadOnly = true
                });
            }

            var canHos = _canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = new ToaService().LayTatCa().ToDictionary(t => t.MaToa);

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

                CanHo canHo;
                if (canHos.TryGetValue(hd.MaCanHo, out canHo))
                {
                    Toa toa;
                    row.Cells["Toa"].Value = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
                }
                else
                {
                    row.Cells["Toa"].Value = string.Empty;
                }
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
