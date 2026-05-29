using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmKhachHangHome : MaterialForm
    {
        private readonly CanHoService _canHoService = new CanHoService();
        private readonly HopDongService _hopDongService = new HopDongService();
        private readonly HoaDonThanhToanService _hoaDonService = new HoaDonThanhToanService();

        private DataGridView gridCanHo;
        private DataGridView gridHopDong;
        private DataGridView gridHoaDon;
        private MaterialLabel lblHeader;

        public frmKhachHangHome()
        {
            Text = "Giao dien Khach hang";
            Size = new Size(1120, 680);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            Load += delegate { TaiDuLieu(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                Padding = new Padding(12, 76, 12, 12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 33));

            lblHeader = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Thong tin khach hang",
                Font = new Font("Roboto", 13F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(lblHeader, 0, 0);

            gridCanHo = CreateGrid();
            gridHopDong = CreateGrid();
            gridHoaDon = CreateGrid();

            root.Controls.Add(CreateGroup("Can ho dang trong", gridCanHo), 0, 1);
            root.Controls.Add(CreateGroup("Hop dong cua toi", gridHopDong), 0, 2);
            root.Controls.Add(CreateGroup("Hoa don cua toi", gridHoaDon), 0, 3);
            Controls.Add(root);
        }

        private GroupBox CreateGroup(string text, Control content)
        {
            var group = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(8)
            };
            content.Dock = DockStyle.Fill;
            group.Controls.Add(content);
            return group;
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
        }

        private void TaiDuLieu()
        {
            lblHeader.Text = string.Format("Xin chao {0} [{1}]", SessionContext.HoTen, SessionContext.MaNguoiDung);

            var canHoTrong = _canHoService.LayTheoTinhTrang("Trong")
                .Select(c => new
                {
                    c.MaCanHo,
                    c.MaToa,
                    c.MaLoai,
                    c.DienTich,
                    c.GiaThueNiemYet,
                    c.TienCocNiemYet,
                    c.TangSo,
                    c.SoCanHo
                })
                .ToList();
            gridCanHo.DataSource = new BindingList<object>(canHoTrong.Cast<object>().ToList());

            var hopDongs = _hopDongService.LayTheoKhach(SessionContext.MaNguoiDung)
                .OrderByDescending(h => h.NgayTao)
                .ToList();
            gridHopDong.DataSource = new BindingList<object>(hopDongs.Select(h => new
            {
                h.MaHopDong,
                h.MaCanHo,
                h.NgayBatDau,
                h.NgayKetThuc,
                h.GiaThueChot,
                h.TienCocChot,
                CocTruocDaTru = h.TienCocTruocDaTru,
                CocConPhaiNop = h.TienCocConPhaiNop,
                h.TrangThai
            }).Cast<object>().ToList());

            var maHopDongs = hopDongs.Select(h => h.MaHopDong).ToList();
            var hoaDons = maHopDongs
                .SelectMany(ma => _hoaDonService.LayTheoHopDong(ma))
                .OrderByDescending(h => h.NgayDaoHan)
                .Select(h => new
                {
                    h.MaHoaDon,
                    h.MaHopDong,
                    h.KyThanhToan,
                    h.SoTienPhaiTra,
                    h.SoTienDaTra,
                    h.NgayDaoHan,
                    h.NgayThanhToan,
                    h.TrangThai
                })
                .ToList();
            gridHoaDon.DataSource = new BindingList<object>(hoaDons.Cast<object>().ToList());
        }
    }
}
