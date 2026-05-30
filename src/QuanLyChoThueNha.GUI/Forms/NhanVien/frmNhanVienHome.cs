using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.KhachHang;
using QuanLyChoThueNha.GUI.Helpers;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.NhanVien
{
    public class frmNhanVienHome : MaterialForm
    {
        private readonly PhieuDatTruocService _phieuDatTruocService = new PhieuDatTruocService();
        private readonly HopDongService _hopDongService = new HopDongService();
        private readonly HoaDonThanhToanService _hoaDonService = new HoaDonThanhToanService();
        private readonly CanHoService _canHoService = new CanHoService();
        private readonly KhachThueService _khachThueService = new KhachThueService();
        private readonly TaiKhoanService _taiKhoanService = new TaiKhoanService();

        private readonly FlowLayoutPanel _kpiPanel = new FlowLayoutPanel();
        private readonly DataGridView _gridDatPhong = CreateGrid();
        private readonly DataGridView _gridHopDong = CreateGrid();
        private readonly DataGridView _gridHoaDon = CreateGrid();
        private readonly MaterialButton _btnLamMoi = new MaterialButton { Text = "Lam moi", AutoSize = true };
        private readonly MaterialButton _btnXacNhanCoc = new MaterialButton { Text = "Xac nhan da nhan coc", AutoSize = true };
        private readonly MaterialButton _btnGuiLaiEmail = new MaterialButton { Text = "Gui lai email/QR", AutoSize = true };
        private readonly MaterialButton _btnEmailLog = new MaterialButton { Text = "Lich su email", AutoSize = true };
        private MaterialLabel _lblHeader;

        public frmNhanVienHome()
        {
            Text = "Trang nhan vien";
            Size = new Size(1180, 760);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            Load += delegate { LoadData(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                Padding = new Padding(16, 76, 16, 16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            _lblHeader = new MaterialLabel
            {
                Text = "Cong viec nhan vien",
                AutoSize = true,
                Font = new Font("Roboto", 14F, FontStyle.Bold),
                Padding = new Padding(0, 8, 20, 0)
            };
            toolbar.Controls.Add(_lblHeader);
            _btnLamMoi.Click += delegate { LoadData(); };
            toolbar.Controls.Add(_btnLamMoi);
            _btnXacNhanCoc.Click += BtnXacNhanCoc_Click;
            toolbar.Controls.Add(_btnXacNhanCoc);
            _btnGuiLaiEmail.Click += BtnGuiLaiEmail_Click;
            toolbar.Controls.Add(_btnGuiLaiEmail);
            _btnEmailLog.Click += delegate { new frmEmailLog().ShowDialog(this); };
            toolbar.Controls.Add(_btnEmailLog);
            root.Controls.Add(toolbar, 0, 0);

            _kpiPanel.Dock = DockStyle.Fill;
            _kpiPanel.WrapContents = false;
            _kpiPanel.AutoScroll = true;
            root.Controls.Add(_kpiPanel, 0, 1);

            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            top.Controls.Add(CreateGroup("Phieu dat phong can xu ly", _gridDatPhong), 0, 0);
            top.Controls.Add(CreateGroup("Hop dong sap het han", _gridHopDong), 1, 0);
            root.Controls.Add(top, 0, 2);

            root.Controls.Add(CreateGroup("Hoa don can theo doi", _gridHoaDon), 0, 3);
            Controls.Add(root);
        }

        private void LoadData()
        {
            _lblHeader.Text = string.Format("Cong viec cua {0} [{1}]", SessionContext.HoTen, SessionContext.MaNguoiDung);

            var tatCaPhieu = _phieuDatTruocService.LayTatCa().ToList();
            var phieuChoCoc = tatCaPhieu
                .Where(p => p.TrangThai == PhieuDatTruocService.ChoThanhToanCoc)
                .OrderBy(p => p.NgayHetHan)
                .ToList();
            var phieuChoKy = tatCaPhieu
                .Where(p => p.TrangThai == PhieuDatTruocService.ChoKy)
                .OrderBy(p => p.NgayHetHan)
                .ToList();
            var phieuCanXuLy = phieuChoCoc.Concat(phieuChoKy).ToList();
            var hopDongSapHetHan = _hopDongService.LayGanHetHan(30)
                .OrderBy(h => h.NgayKetThuc)
                .ToList();
            var hoaDonCanTheoDoi = _hoaDonService.LayTatCa()
                .Where(h => h.TrangThai == "ChuaTra" || h.TrangThai == "TraThieu" || h.TrangThai == "QuaHan")
                .OrderBy(h => h.NgayDaoHan)
                .ToList();

            _kpiPanel.Controls.Clear();
            AddKpi("Phong trong", _canHoService.LayTheoTinhTrang("Trong").Count().ToString("N0"), Color.FromArgb(0, 137, 123));
            AddKpi("Cho coc", phieuChoCoc.Count.ToString("N0"), Color.FromArgb(245, 124, 0));
            AddKpi("Cho ky HD", phieuChoKy.Count.ToString("N0"), Color.FromArgb(25, 118, 210));
            AddKpi("HD sap het han", hopDongSapHetHan.Count.ToString("N0"), Color.FromArgb(123, 31, 162));
            AddKpi("Hoa don can xu ly", hoaDonCanTheoDoi.Count.ToString("N0"), Color.FromArgb(198, 40, 40));

            _gridDatPhong.DataSource = new BindingList<object>(phieuCanXuLy.Select(p => new
            {
                p.MaPhieuDatTruoc,
                p.MaKhach,
                p.MaCanHo,
                p.SoTienDatCoc,
                p.NgayDatCoc,
                p.NgayHetHan,
                p.TrangThai
            }).Cast<object>().ToList());

            _gridHopDong.DataSource = new BindingList<object>(hopDongSapHetHan.Select(h => new
            {
                h.MaHopDong,
                h.MaKhach,
                h.MaCanHo,
                h.NgayKetThuc,
                SoNgayConLai = Math.Max(0, (h.NgayKetThuc.Date - DateTime.Today).Days),
                h.TrangThai
            }).Cast<object>().ToList());

            _gridHoaDon.DataSource = new BindingList<object>(hoaDonCanTheoDoi.Select(h => new
            {
                h.MaHoaDon,
                h.MaHopDong,
                h.KyThanhToan,
                h.SoTienPhaiTra,
                h.SoTienDaTra,
                h.NgayDaoHan,
                h.TrangThai
            }).Cast<object>().ToList());
        }

        private void BtnXacNhanCoc_Click(object sender, EventArgs e)
        {
            var maPhieu = LayMaPhieuDangChon();
            if (string.IsNullOrWhiteSpace(maPhieu))
            {
                MessageBox.Show("Chon phieu dat truoc can xac nhan coc.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string loi;
            if (!_phieuDatTruocService.XacNhanDaNhanCoc(maPhieu, out loi))
            {
                MessageBox.Show(loi, "Khong the xac nhan coc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Da xac nhan nhan coc. Phieu da chuyen sang cho ky hop dong.",
                "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
        }

        private void BtnGuiLaiEmail_Click(object sender, EventArgs e)
        {
            var maPhieu = LayMaPhieuDangChon();
            if (string.IsNullOrWhiteSpace(maPhieu))
            {
                MessageBox.Show("Chon phieu dat truoc can gui lai email/QR.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var phieu = _phieuDatTruocService.LayTheoMa(maPhieu);
            if (phieu == null)
            {
                MessageBox.Show("Khong tim thay phieu.", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var khach = _khachThueService.LayTheoMa(phieu.MaKhach);
            TaiKhoan taiKhoan = khach == null ? null : _taiKhoanService.LayTheoMa(khach.MaTaiKhoan);
            var email = taiKhoan == null ? string.Empty : taiKhoan.Email;
            var tenDangNhap = taiKhoan == null ? string.Empty : taiKhoan.TenDangNhap;
            var noiDung = NoiDungDatCoc(phieu);
            string thongBao;
            EmailNotificationHelper.GuiThongTinDatTruoc(email, khach == null ? string.Empty : khach.HoTen,
                tenDangNhap, "(mat khau da cap truoc)", phieu.MaPhieuDatTruoc, phieu.MaCanHo,
                phieu.SoTienDatCoc, phieu.NgayHetHan, noiDung, khach == null ? null : khach.MaTaiKhoan,
                out thongBao);

            MessageBox.Show(thongBao, "Gui lai email", MessageBoxButtons.OK, MessageBoxIcon.Information);
            using (var qr = new frmQrThanhToan("QR dat coc phong", phieu.MaPhieuDatTruoc,
                "Phong " + phieu.MaCanHo, phieu.SoTienDatCoc, noiDung))
            {
                qr.ShowDialog(this);
            }
        }

        private string LayMaPhieuDangChon()
        {
            if (_gridDatPhong.CurrentRow == null || !_gridDatPhong.Columns.Contains("MaPhieuDatTruoc"))
                return string.Empty;

            var value = _gridDatPhong.CurrentRow.Cells["MaPhieuDatTruoc"].Value;
            return value == null ? string.Empty : value.ToString();
        }

        private string NoiDungDatCoc(PhieuDatTruoc phieu)
        {
            return string.Format("DAT COC {0} PHONG {1}", phieu.MaPhieuDatTruoc, phieu.MaCanHo);
        }

        private static DataGridView CreateGrid()
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

        private static GroupBox CreateGroup(string title, Control content)
        {
            var group = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = title,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(8)
            };
            content.Dock = DockStyle.Fill;
            group.Controls.Add(content);
            return group;
        }

        private void AddKpi(string title, string value, Color color)
        {
            var panel = new Panel
            {
                Width = 210,
                Height = 92,
                Margin = new Padding(0, 0, 10, 0),
                BackColor = color
            };
            panel.Controls.Add(new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(12, 12),
                AutoSize = true
            });
            panel.Controls.Add(new Label
            {
                Text = value,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(12, 42),
                AutoSize = true
            });
            _kpiPanel.Controls.Add(panel);
        }
    }
}
