using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Web.WebView2.WinForms;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.GUI.Forms.Auth;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.GUI.Helpers;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmTimTroPublic : MaterialForm
    {
        private CanHoService _canHoService = new CanHoService();
        private KhuVucService _khuVucService = new KhuVucService();
        private ToaService _toaService = new ToaService();
        private LoaiCanHoService _loaiService = new LoaiCanHoService();
        private TienNghiService _tienNghiService = new TienNghiService();
        private KhachThueService _khachThueService = new KhachThueService();
        private TaiKhoanService _taiKhoanService = new TaiKhoanService();
        private PhieuDatTruocService _phieuDatTruocService = new PhieuDatTruocService();

        private readonly FlowLayoutPanel _roomCards = new FlowLayoutPanel();
        private readonly ComboBox _cboKhuVuc = new ComboBox();
        private readonly ComboBox _cboToa = new ComboBox();
        private readonly ComboBox _cboLoai = new ComboBox();
        private readonly NumericUpDown _numBanKinh = new NumericUpDown();
        private readonly PlaceholderTextBox _txtGiaTu = new PlaceholderTextBox();
        private readonly PlaceholderTextBox _txtGiaDen = new PlaceholderTextBox();
        private readonly PlaceholderTextBox _txtTimKiem = new PlaceholderTextBox();
        private readonly MaterialLabel _lblCount = new MaterialLabel();
        private readonly WebView2 _mapView = new WebView2();
        private Panel _detailOverlay;
        private Panel _detailPopup;
        private bool _dangLamMoiBoLoc;

        private class FilterOption
        {
            public FilterOption(string value, string display)
            {
                Value = value;
                Display = display;
            }

            public string Value { get; private set; }
            public string Display { get; private set; }
        }

        public frmTimTroPublic()
        {
            Text = "Tìm trọ";
            Size = new Size(1220, 820);
            StartPosition = FormStartPosition.CenterScreen;

            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            skinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE);

            BuildLayout();
            NapBoLoc();
            TaiDanhSachTro();
            Resize += delegate
            {
                ResizeCards();
                ResizeDetailPopup();
            };
        }

        private void BuildLayout()
        {
            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                Padding = new Padding(0, 64, 0, 0),
                BackColor = Color.FromArgb(248, 249, 252)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 249, 252)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                AutoScroll = false,
                Padding = new Padding(0, 0, 0, 8),
                BackColor = Color.FromArgb(248, 249, 252)
            };
            var btnHuongDan = new MaterialButton { Text = "Hướng dẫn tôi", AutoSize = false, Width = 146, Height = 42, Margin = new Padding(0, 8, 8, 4) };
            btnHuongDan.Click += delegate { new frmHuongDanSuDung("Public").ShowDialog(this); };
            var title = new MaterialLabel
            {
                Text = "Tìm trọ đang trống",
                Width = 300,
                Height = 52,
                Font = new Font("Roboto", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 4, 16, 4)
            };
            var btnKhach = new MaterialButton { Text = "Tài khoản khách", AutoSize = false, Width = 168, Height = 42, Margin = new Padding(0, 8, 8, 4) };
            btnKhach.Click += delegate { new frmLogin(new[] { "KhachThue" }, "Đăng nhập khách hàng").Show(); };
            var btnNoiBo = new MaterialButton { Text = "Cổng nội bộ", AutoSize = false, Width = 146, Height = 42, Margin = new Padding(0, 8, 8, 4) };
            btnNoiBo.Click += delegate { new frmLogin(new[] { "Admin", "NhanVien" }, "Đăng nhập nội bộ").Show(); };
            var btnLamMoi = new MaterialButton { Text = "Làm mới", AutoSize = false, Width = 118, Height = 42, Margin = new Padding(0, 8, 8, 4) };
            btnLamMoi.Click += delegate { LamMoiDuLieu(); };

            top.Controls.Add(btnHuongDan);
            top.Controls.Add(title);
            top.Controls.Add(btnLamMoi);
            top.Controls.Add(btnKhach);
            top.Controls.Add(btnNoiBo);
            root.Controls.Add(top, 0, 0);

            _txtTimKiem.Placeholder = "Tìm theo địa chỉ, khu vực, tòa, mã phòng...";
            _txtTimKiem.BorderStyle = BorderStyle.None;
            _txtTimKiem.Font = new Font("Segoe UI", 11F);
            _txtTimKiem.BackColor = Color.White;
            _txtTimKiem.TextChanged += delegate { TaiDanhSachTro(); };

            SetupCombo(_cboToa, 0);
            SetupCombo(_cboKhuVuc, 0);
            SetupCombo(_cboLoai, 0);
            _cboKhuVuc.SelectedIndexChanged += delegate { TaiDanhSachTro(); };
            _cboToa.SelectedIndexChanged += delegate { TaiDanhSachTro(); };
            _cboLoai.SelectedIndexChanged += delegate { TaiDanhSachTro(); };

            _numBanKinh.Width = 0;
            _numBanKinh.Minimum = 1;
            _numBanKinh.Maximum = 100;
            _numBanKinh.Value = 3;
            _numBanKinh.ValueChanged += delegate { TaiDanhSachTro(); };

            SetupMoneyText(_txtGiaTu, "0");
            SetupMoneyText(_txtGiaDen, "0");
            _txtGiaTu.TextChanged += delegate { TaiDanhSachTro(); };
            _txtGiaDen.TextChanged += delegate { TaiDanhSachTro(); };

            root.Controls.Add(CreateFilterPanel(), 0, 1);

            _roomCards.Dock = DockStyle.Fill;
            _roomCards.AutoScroll = true;
            _roomCards.WrapContents = true;
            _roomCards.Padding = new Padding(2);
            _roomCards.BackColor = Color.White;

            _mapView.Dock = DockStyle.Fill;
            _mapView.DefaultBackgroundColor = Color.White;

            root.Controls.Add(_roomCards, 0, 2);
            shell.Controls.Add(root, 0, 0);
            Controls.Add(shell);
            CreateDetailOverlay();
        }

        private Control CreateSidebar()
        {
            var sidebar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 10,
                BackColor = Color.FromArgb(30, 42, 69),
                Padding = new Padding(18, 18, 10, 18)
            };
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            for (var i = 1; i < 9; i++)
                sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var brand = new Label
            {
                Text = "SmartApart\r\nManagement Suite",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            sidebar.Controls.Add(brand, 0, 0);

            var btnTimTro = CreateSidebarButton("Tim tro", true);
            var btnKhach = CreateSidebarButton("Tai khoan khach", false);
            btnKhach.Click += delegate { new frmLogin(new[] { "KhachThue" }, "Dang nhap khach hang").Show(); };
            var btnNoiBo = CreateSidebarButton("Cong noi bo", false);
            btnNoiBo.Click += delegate { new frmLogin(new[] { "Admin", "NhanVien" }, "Dang nhap noi bo").Show(); };
            var btnLamMoi = CreateSidebarButton("Lam moi du lieu", false);
            btnLamMoi.Click += delegate { LamMoiDuLieu(); };

            sidebar.Controls.Add(btnTimTro, 0, 1);
            sidebar.Controls.Add(btnKhach, 0, 2);
            sidebar.Controls.Add(btnNoiBo, 0, 3);
            sidebar.Controls.Add(btnLamMoi, 0, 4);
            return sidebar;
        }

        private Button CreateSidebarButton(string text, bool active)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(224, 231, 255),
                BackColor = active ? Color.FromArgb(42, 59, 95) : Color.FromArgb(30, 42, 69),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = active ? 1 : 0;
            button.FlatAppearance.BorderColor = Color.FromArgb(79, 134, 247);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 59, 95);
            return button;
        }

        private void CreateDetailOverlay()
        {
            _detailOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Visible = false
            };
            _detailOverlay.Click += delegate { CloseDetailPopup(); };
            Controls.Add(_detailOverlay);
            _detailOverlay.BringToFront();
        }

        private void ResizeDetailPopup()
        {
            if (_detailPopup == null || !_detailPopup.Visible) return;
            var width = Math.Min(1040, Math.Max(680, ClientSize.Width - 120));
            var height = Math.Min(560, Math.Max(420, ClientSize.Height - 150));
            _detailPopup.Size = new Size(width, height);
            _detailPopup.Location = new Point((ClientSize.Width - width) / 2, (ClientSize.Height - height) / 2 + 28);
        }

        private void CloseDetailPopup()
        {
            if (_detailOverlay == null) return;
            if (_mapView.Parent != null)
                _mapView.Parent.Controls.Remove(_mapView);
            _detailOverlay.Controls.Clear();
            _detailPopup = null;
            _detailOverlay.Visible = false;
        }

        private void ShowDetailPopup(CanHo room, bool dangChoCoc)
        {
            if (_detailOverlay == null) return;
            if (_mapView.Parent != null)
                _mapView.Parent.Controls.Remove(_mapView);
            _detailOverlay.Controls.Clear();
            _detailOverlay.Visible = true;
            _detailOverlay.BringToFront();

            var coTheDat = room.TinhTrang == "Trong" && !dangChoCoc;
            var trangThaiHienThi = dangChoCoc ? "Đang chờ cọc" :
                room.TinhTrang == "Trong" ? "Còn trống" :
                room.TinhTrang == "DangThue" ? "Đang cho thuê" : room.TinhTrang;

            _detailPopup = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _detailPopup.Click += delegate { };
            _detailOverlay.Controls.Add(_detailPopup);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(18),
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            _detailPopup.Controls.Add(layout);

            var info = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, BackColor = Color.White };
            info.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            info.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            info.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            info.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            info.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            info.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            info.Controls.Add(new Label
            {
                Text = string.Format("{0} - Căn {1}", LayTenToa(room.MaToa), room.SoCanHo),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            }, 0, 0);
            info.Controls.Add(InfoCard("Vị trí", LayDiaChiPhong(room)), 0, 1);
            info.Controls.Add(InfoCard("Thông tin phòng",
                "Trạng thái: " + trangThaiHienThi + Environment.NewLine +
                string.Format("Tầng {0} | {1:N1} m2 | Mã phòng {2}", room.TangSo, room.DienTich, room.MaCanHo) + Environment.NewLine +
                string.Format("Giá {0:N0} VNĐ - Cọc {1:N0} VNĐ", room.GiaThueNiemYet, room.TienCocNiemYet)), 0, 2);
            info.Controls.Add(InfoCard("Tiện nghi và mô tả",
                LayTienNghiHienThi(room.MaCanHo) + Environment.NewLine +
                (string.IsNullOrWhiteSpace(room.MoTa) ? "Phòng đang sẵn sàng cho thuê." : room.MoTa)), 0, 3);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            var btnDat = new MaterialButton { Text = coTheDat ? "Đặt trước" : "Chưa thể đặt", AutoSize = false, Width = 130, Height = 42, Enabled = coTheDat };
            btnDat.Click += delegate
            {
                CloseDetailPopup();
                DangKyVaDatTruoc(room);
            };
            var btnDong = new MaterialButton { Text = "Đóng", AutoSize = false, Width = 90, Height = 42 };
            btnDong.Click += delegate { CloseDetailPopup(); };
            actions.Controls.Add(btnDat);
            actions.Controls.Add(btnDong);
            info.Controls.Add(actions, 0, 4);
            info.Controls.Add(new Label { Text = "Bấm bên ngoài khung để đóng chi tiết.", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(107, 114, 128) }, 0, 5);

            var mapBox = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 249, 252), Padding = new Padding(0) };
            mapBox.Controls.Add(_mapView);
            layout.Controls.Add(info, 0, 0);
            layout.Controls.Add(mapBox, 1, 0);
            ResizeDetailPopup();
            HienThiBanDoToa(room);
        }

        private Control InfoCard(string title, string content)
        {
            var card = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                BorderColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 12, 10)
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = card.BackColor };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235)
            }, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = content,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoEllipsis = true
            }, 0, 1);
            card.Controls.Add(layout);
            return card;
        }

        private Control CreateFilterPanel()
        {
            var card = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14, 8, 14, 8),
                Margin = new Padding(0, 4, 0, 10),
                Radius = 14,
                BorderColor = Color.FromArgb(226, 232, 240)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 8,
                RowCount = 2,
                BackColor = Color.White
            };
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));

            AddFilterCell(grid, "Tìm kiếm", _txtTimKiem, 0);
            AddFilterCell(grid, "Khu vực", _cboKhuVuc, 1);
            AddFilterCell(grid, "Bán kính", _numBanKinh, 2);
            AddFilterCell(grid, "Tòa", _cboToa, 3);
            AddFilterCell(grid, "Loại phòng", _cboLoai, 4);
            AddFilterCell(grid, "Giá từ", _txtGiaTu, 5);
            AddFilterCell(grid, "Giá đến", _txtGiaDen, 6);

            grid.Controls.Add(new Label
            {
                Text = "Kết quả",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft
            }, 7, 0);
            _lblCount.Dock = DockStyle.Fill;
            _lblCount.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _lblCount.ForeColor = Color.FromArgb(4, 86, 197);
            _lblCount.TextAlign = ContentAlignment.MiddleLeft;
            grid.Controls.Add(_lblCount, 7, 1);

            card.Controls.Add(grid);
            return card;
        }

        private void AddFilterCell(TableLayoutPanel grid, string label, Control editor, int column)
        {
            grid.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 10, 0)
            }, column, 0);
            var inputShell = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 9,
                BorderColor = Color.FromArgb(226, 232, 240),
                BorderThickness = 1,
                BackColor = Color.White,
                Padding = new Padding(10, 3, 10, 3),
                Margin = new Padding(0, 0, 12, 0)
            };
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0);
            if (editor is ComboBox)
            {
                ((ComboBox)editor).FlatStyle = FlatStyle.Flat;
                editor.BackColor = Color.White;
            }
            if (editor is NumericUpDown)
            {
                editor.BackColor = Color.White;
            }
            inputShell.Controls.Add(editor);
            grid.Controls.Add(inputShell, column, 1);
        }

        private Label Label(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(12, 15, 4, 0)
            };
        }

        private void SetupCombo(ComboBox combo, int width)
        {
            combo.Width = width;
            combo.Height = 32;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void SetupMoney(NumericUpDown number)
        {
            number.Width = 140;
            number.Maximum = 1000000000000;
            number.ThousandsSeparator = true;
            number.DecimalPlaces = 0;
        }

        private void SetupMoneyText(PlaceholderTextBox textbox, string placeholder)
        {
            textbox.BorderStyle = BorderStyle.None;
            textbox.Font = new Font("Segoe UI", 10F);
            textbox.BackColor = Color.White;
            textbox.Placeholder = placeholder;
            textbox.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };
            textbox.Leave += delegate
            {
                decimal value;
                if (TryReadMoney(textbox, out value) && value > 0)
                    textbox.Text = value.ToString("N0");
            };
        }

        private static bool TryReadMoney(TextBox textbox, out decimal value)
        {
            value = 0;
            if (textbox == null || string.IsNullOrWhiteSpace(textbox.Text)) return false;
            var raw = textbox.Text.Replace(".", string.Empty).Replace(",", string.Empty).Trim();
            return decimal.TryParse(raw, out value);
        }

        private void NapBoLoc()
        {
            var selectedKhuVuc = _cboKhuVuc.SelectedValue == null ? string.Empty : _cboKhuVuc.SelectedValue.ToString();
            var selectedToa = _cboToa.SelectedValue == null ? string.Empty : _cboToa.SelectedValue.ToString();
            var selectedLoai = _cboLoai.SelectedValue == null ? string.Empty : _cboLoai.SelectedValue.ToString();

            _cboKhuVuc.DisplayMember = "Display";
            _cboKhuVuc.ValueMember = "Value";
            var khuVucOptions = new List<FilterOption> { new FilterOption(string.Empty, "Tất cả khu vực") };
            khuVucOptions.AddRange(_khuVucService.LayTatCa()
                .OrderBy(k => k.TenKhuVuc)
                .Select(k => new FilterOption(k.MaKhuVuc, k.TenKhuVuc)));
            _cboKhuVuc.DataSource = khuVucOptions;
            _cboKhuVuc.SelectedValue = khuVucOptions.Any(x => x.Value == selectedKhuVuc) ? selectedKhuVuc : string.Empty;

            _cboToa.DisplayMember = "Display";
            _cboToa.ValueMember = "Value";
            var toaOptions = new List<FilterOption> { new FilterOption(string.Empty, "Tất cả tòa") };
            toaOptions.AddRange(_toaService.LayTatCa()
                .OrderBy(t => t.TenToa)
                .Select(t => new FilterOption(t.MaToa, t.TenToa)));
            _cboToa.DataSource = toaOptions;
            _cboToa.SelectedValue = toaOptions.Any(x => x.Value == selectedToa) ? selectedToa : string.Empty;

            _cboLoai.DisplayMember = "Display";
            _cboLoai.ValueMember = "Value";
            var loaiOptions = new List<FilterOption> { new FilterOption(string.Empty, "Tất cả loại") };
            loaiOptions.AddRange(_loaiService.LayTatCa()
                .OrderBy(l => l.TenLoai)
                .Select(l => new FilterOption(l.MaLoai, l.TenLoai)));
            _cboLoai.DataSource = loaiOptions;
            _cboLoai.SelectedValue = loaiOptions.Any(x => x.Value == selectedLoai) ? selectedLoai : string.Empty;
        }

        private void LamMoiDuLieu()
        {
            // BỔ SUNG: Khởi tạo lại các dịch vụ để làm mới EF cache khi bấm làm mới
            _canHoService = new CanHoService();
            _khuVucService = new KhuVucService();
            _toaService = new ToaService();
            _loaiService = new LoaiCanHoService();
            _tienNghiService = new TienNghiService();
            _khachThueService = new KhachThueService();
            _taiKhoanService = new TaiKhoanService();
            _phieuDatTruocService = new PhieuDatTruocService();

            _dangLamMoiBoLoc = true;
            NapBoLoc();
            _txtTimKiem.Clear();
            _txtGiaTu.Clear();
            _txtGiaDen.Clear();
            _numBanKinh.Value = 3;
            if (_cboKhuVuc.Items.Count > 0) _cboKhuVuc.SelectedIndex = 0;
            if (_cboToa.Items.Count > 0) _cboToa.SelectedIndex = 0;
            if (_cboLoai.Items.Count > 0) _cboLoai.SelectedIndex = 0;
            _dangLamMoiBoLoc = false;
            TaiDanhSachTro();
        }

        private void TaiDanhSachTro()
        {
            if (_roomCards == null) return;
            if (_dangLamMoiBoLoc) return;
            _roomCards.Controls.Clear();
            GuiThongBaoPhieuHetHanMoi();

            var phongDangChoCoc = _phieuDatTruocService.LayTatCa()
                .Where(p => p.TrangThai == PhieuDatTruocService.ChoThanhToanCoc ||
                            p.TrangThai == PhieuDatTruocService.DaThanhToanCoc ||
                            p.TrangThai == PhieuDatTruocService.ChoKy)
                .Select(p => p.MaCanHo)
                .ToList();
            var data = _canHoService.LayTatCa();
            var khuVucs = _khuVucService.LayTatCa().ToDictionary(k => k.MaKhuVuc);
            var toas = _toaService.LayTatCa().ToDictionary(t => t.MaToa);
            var kw = TextFormatHelper.NormalizeSearch(_txtTimKiem.Text);
            if (!string.IsNullOrWhiteSpace(kw))
            {
                data = data.Where(c =>
                {
                    Toa toa;
                    KhuVuc khuVuc = null;
                    toas.TryGetValue(c.MaToa, out toa);
                    if (toa != null) khuVucs.TryGetValue(toa.MaKhuVuc, out khuVuc);

                    var haystack = TextFormatHelper.JoinSearchParts(
                        c.MaCanHo, c.MaToa, c.SoCanHo, c.MaLoai, c.TangSo,
                        toa == null ? null : toa.TenToa,
                        toa == null ? null : toa.DiaChi,
                        khuVuc == null ? null : khuVuc.TenKhuVuc,
                        khuVuc == null ? null : khuVuc.Quan,
                        khuVuc == null ? null : khuVuc.ThanhPho);
                    return TextFormatHelper.ContainsNormalized(haystack, kw);
                });
            }
            if (_cboToa.SelectedValue != null && !string.IsNullOrWhiteSpace(_cboToa.SelectedValue.ToString()))
            {
                var maToa = _cboToa.SelectedValue.ToString();
                data = data.Where(c => c.MaToa == maToa);
            }
            if (_cboLoai.SelectedValue != null && !string.IsNullOrWhiteSpace(_cboLoai.SelectedValue.ToString()))
            {
                var maLoai = _cboLoai.SelectedValue.ToString();
                data = data.Where(c => c.MaLoai == maLoai);
            }
            decimal giaTu;
            if (TryReadMoney(_txtGiaTu, out giaTu) && giaTu > 0)
            {
                data = data.Where(c => c.GiaThueNiemYet >= giaTu);
            }
            decimal giaDen;
            if (TryReadMoney(_txtGiaDen, out giaDen) && giaDen > 0)
            {
                data = data.Where(c => c.GiaThueNiemYet <= giaDen);
            }

            if (_cboKhuVuc.SelectedValue != null && !string.IsNullOrWhiteSpace(_cboKhuVuc.SelectedValue.ToString()))
            {
                var selectedMaKhuVuc = _cboKhuVuc.SelectedValue.ToString();
                KhuVuc selectedKhuVuc;
                khuVucs.TryGetValue(selectedMaKhuVuc, out selectedKhuVuc);
                data = data.Where(c => TrongBanKinhKhuVuc(c, selectedMaKhuVuc, selectedKhuVuc, toas, khuVucs));
            }

            var rooms = data.OrderBy(c => c.TinhTrang == "Trong" && !phongDangChoCoc.Contains(c.MaCanHo) ? 0 : 1)
                .ThenBy(c => c.MaToa)
                .ThenBy(c => c.SoCanHo)
                .ToList();
            _lblCount.Text = string.Format("{0:N0} phòng", rooms.Count);

            if (rooms.Count == 0)
            {
                _roomCards.Controls.Add(new Label
                {
                    Text = "Không có phòng phù hợp.",
                    AutoSize = true,
                    Padding = new Padding(16)
                });
                return;
            }

            foreach (var room in rooms)
                _roomCards.Controls.Add(CreateRoomCard(room, phongDangChoCoc.Contains(room.MaCanHo)));
            ResizeCards();
        }

        private void GuiThongBaoPhieuHetHanMoi()
        {
            var phieusHetHan = _phieuDatTruocService.XuLyPhieuChoCocQuaHan24h();
            foreach (var phieu in phieusHetHan)
            {
                var khach = _khachThueService.LayTheoMa(phieu.MaKhach);
                var taiKhoan = khach == null ? null : _taiKhoanService.LayTheoMa(khach.MaTaiKhoan);
                string thongBao;
                EmailNotificationHelper.GuiThongBaoHetHanDatCoc(
                    taiKhoan == null ? string.Empty : taiKhoan.Email,
                    khach == null ? string.Empty : khach.HoTen,
                    phieu.MaPhieuDatTruoc,
                    phieu.MaCanHo,
                    khach == null ? null : khach.MaTaiKhoan,
                    out thongBao);
            }
        }

        private Control CreateRoomCard(CanHo room, bool dangChoCoc)
        {
            var coTheDat = room.TinhTrang == "Trong" && !dangChoCoc;
            var mauNen = coTheDat ? Color.White :
                room.TinhTrang == "DangThue" ? Color.FromArgb(255, 247, 237) :
                Color.FromArgb(239, 246, 255);
            var mauVien = coTheDat ? Color.FromArgb(34, 197, 94) :
                room.TinhTrang == "DangThue" ? Color.FromArgb(251, 146, 60) :
                Color.FromArgb(96, 165, 250);
            var mauTag = coTheDat ? Color.FromArgb(22, 163, 74) :
                room.TinhTrang == "DangThue" ? Color.FromArgb(234, 88, 12) :
                Color.FromArgb(37, 99, 235);
            var trangThaiHienThi = dangChoCoc ? "Đang chờ cọc" :
                room.TinhTrang == "Trong" ? "Còn trống" :
                room.TinhTrang == "DangThue" ? "Đang cho thuê" : room.TinhTrang;

            var card = new RoundedPanel
            {
                Height = 330,
                Width = 330,
                Margin = new Padding(4, 4, 18, 18),
                BackColor = mauNen,
                Tag = "room-card",
                Radius = 14,
                BorderColor = coTheDat ? Color.FromArgb(148, 163, 184) : mauVien,
                BorderThickness = 2,
                Padding = new Padding(2)
            };

            var image = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 160,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(236, 240, 244)
            };
            GanAnh(image, room.MaCanHo);
            card.Controls.Add(image);

            var status = new Label
            {
                Text = trangThaiHienThi,
                AutoSize = false,
                Width = 104,
                Height = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = mauTag,
                Location = new Point(card.Width - 116, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(status);
            status.BringToFront();

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                Padding = new Padding(10, 8, 10, 10)
            };
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            body.Controls.Add(new Label
            {
                Text = string.Format("{0} - Can {1}", LayTenToa(room.MaToa), room.SoCanHo),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            }, 0, 0);
            body.Controls.Add(new Label
            {
                Text = string.Format("Tầng {0} | {1:N1} m2 | {2}",
                    room.TangSo, room.DienTich, room.MaCanHo),
                Dock = DockStyle.Fill,
                ForeColor = mauVien
            }, 0, 1);
            body.Controls.Add(new Label
            {
                Text = string.Format("Giá {0:N0} VNĐ - Cọc {1:N0} VNĐ", room.GiaThueNiemYet, room.TienCocNiemYet),
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(0, 105, 92)
            }, 0, 2);
            body.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(room.MoTa) ? "Phòng đang sẵn sàng cho thuê." : room.MoTa,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            }, 0, 3);

            var btnDatTruoc = new RoundedButton
            {
                Text = "Chi tiết",
                Width = 104,
                Height = 38,
                Enabled = true,
                Radius = 10,
                BackColor = Color.FromArgb(37, 99, 235),
                BorderColor = Color.FromArgb(37, 99, 235)
            };
            btnDatTruoc.Click += delegate { ShowDetailPopup(room, dangChoCoc); };
            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, coTheDat ? 232 : 112));
            footer.Controls.Add(new Label
            {
                Text = LayTienNghiHienThi(room.MaCanHo),
                AutoSize = false,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            }, 0, 0);
            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0)
            };
            actionPanel.Controls.Add(btnDatTruoc);
            if (coTheDat)
            {
                var btnDatNhanh = new RoundedButton
                {
                    Text = "Đặt nhanh",
                    Width = 108,
                    Height = 38,
                    Radius = 10,
                    BackColor = Color.FromArgb(22, 163, 74),
                    BorderColor = Color.FromArgb(22, 163, 74),
                    Margin = new Padding(0, 0, 8, 0)
                };
                btnDatNhanh.Click += delegate { DangKyVaDatTruoc(room); };
                actionPanel.Controls.Add(btnDatNhanh);
            }
            footer.Controls.Add(actionPanel, 1, 0);
            body.Controls.Add(footer, 0, 4);
            card.Controls.Add(body);
            body.BringToFront();
            status.BringToFront();
            WireDetailClick(card, room, dangChoCoc, actionPanel);
            return card;
        }

        private void WireDetailClick(Control parent, CanHo room, bool dangChoCoc, Control excluded)
        {
            foreach (Control child in parent.Controls)
            {
                if (child == excluded) continue;
                child.Cursor = Cursors.Hand;
                child.Click += delegate { ShowDetailPopup(room, dangChoCoc); };
                WireDetailClick(child, room, dangChoCoc, excluded);
            }
            parent.Cursor = Cursors.Hand;
            parent.Click += delegate { ShowDetailPopup(room, dangChoCoc); };
        }

        private string LayTenToa(string maToa)
        {
            var toa = _toaService.LayTheoMa(maToa);
            return toa == null || string.IsNullOrWhiteSpace(toa.TenToa) ? maToa : toa.TenToa;
        }

        private string LayDiaChiPhong(CanHo room)
        {
            if (room == null) return "Đang cập nhật vị trí.";
            var toa = _toaService.LayTheoMa(room.MaToa);
            KhuVuc khuVuc = null;
            if (toa != null && !string.IsNullOrWhiteSpace(toa.MaKhuVuc))
                khuVuc = _khuVucService.LayTheoMa(toa.MaKhuVuc);
            var address = DiaChiDayDu(toa, khuVuc);
            return string.IsNullOrWhiteSpace(address) ? "Đang cập nhật vị trí." : address;
        }

        private string LayTienNghiHienThi(string maCanHo)
        {
            var tienNghiById = _tienNghiService.LayTatCa()
                .GroupBy(t => t.MaTienNghi)
                .ToDictionary(g => g.Key, g => g.First().TenTienNghi);
            var names = _canHoService.LayTienNghiCuaCanHo(maCanHo)
                .Select(t =>
                {
                    string ten;
                    return tienNghiById.TryGetValue(t.MaTienNghi, out ten) ? ten : t.MaTienNghi;
                })
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Take(4)
                .ToList();
            return names.Count == 0 ? "Tiện nghi: đang cập nhật" : "Tiện nghi: " + string.Join(", ", names);
        }

        private bool TrongBanKinhKhuVuc(CanHo room, string selectedMaKhuVuc, KhuVuc selectedKhuVuc,
            System.Collections.Generic.Dictionary<string, Toa> toas,
            System.Collections.Generic.Dictionary<string, KhuVuc> khuVucs)
        {
            Toa toa;
            if (!toas.TryGetValue(room.MaToa, out toa)) return false;
            if (toa.MaKhuVuc == selectedMaKhuVuc) return true;
            if (selectedKhuVuc == null || !selectedKhuVuc.ViDo.HasValue || !selectedKhuVuc.KinhDo.HasValue)
                return false;

            KhuVuc roomKhuVuc;
            if (!khuVucs.TryGetValue(toa.MaKhuVuc, out roomKhuVuc)) return false;
            if (!roomKhuVuc.ViDo.HasValue || !roomKhuVuc.KinhDo.HasValue) return false;
            return TinhKhoangCachKm(selectedKhuVuc.ViDo.Value, selectedKhuVuc.KinhDo.Value,
                roomKhuVuc.ViDo.Value, roomKhuVuc.KinhDo.Value) <= (double)_numBanKinh.Value;
        }

        private double TinhKhoangCachKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private void GanAnh(PictureBox picture, string maCanHo)
        {
            var anh = _canHoService.LayAnhDaiDien(maCanHo);
            if (anh != null && File.Exists(anh.DuongDanAnh))
            {
                using (var temp = Image.FromFile(anh.DuongDanAnh))
                    picture.Image = new Bitmap(temp);
                return;
            }

            var bitmap = new Bitmap(420, 220);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(232, 238, 243));
                using (var brush = new SolidBrush(Color.FromArgb(80, 98, 112)))
                using (var font = new Font("Segoe UI", 16F, FontStyle.Bold))
                {
                    var text = "NHÀ TRỌ";
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (bitmap.Width - size.Width) / 2, (bitmap.Height - size.Height) / 2);
                }
            }
            picture.Image = bitmap;
        }

        private async void HienThiBanDoMacDinh()
        {
            try
            {
                if (_mapView.CoreWebView2 == null)
                    await _mapView.EnsureCoreWebView2Async(null);
                _mapView.NavigateToString(TaoHtmlBanDoMacDinh());
            }
            catch
            {
                // WebView2 runtime may be missing on the machine; the room list still works.
            }
        }

        private async void HienThiBanDoToa(CanHo room)
        {
            try
            {
                if (room == null) return;
                Toa toa;
                KhuVuc khuVuc;
                var toas = _toaService.LayTatCa().ToDictionary(t => t.MaToa);
                var khuVucs = _khuVucService.LayTatCa().ToDictionary(k => k.MaKhuVuc);
                if (!toas.TryGetValue(room.MaToa, out toa)) return;
                khuVucs.TryGetValue(toa.MaKhuVuc, out khuVuc);

                if (_mapView.CoreWebView2 == null)
                    await _mapView.EnsureCoreWebView2Async(null);
                _mapView.NavigateToString(TaoHtmlBanDoToa(room, toa, khuVuc));
            }
            catch
            {
                // WebView2 runtime may be missing on the machine; booking flow still works.
            }
        }

        private string TaoHtmlBanDoMacDinh()
        {
            return @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    html, body { height:100%; margin:0; font-family: Segoe UI, Arial, sans-serif; background:#f8f9fc; color:#1f2937; }
    .empty { height:100%; display:flex; align-items:center; justify-content:center; padding:24px; box-sizing:border-box; text-align:center; }
    .box { background:white; border:1px solid #e5e7eb; border-radius:8px; padding:24px; box-shadow:0 8px 24px rgba(30,42,69,.08); }
    .title { font-weight:700; font-size:18px; color:#1E2A45; margin-bottom:8px; }
    .text { font-size:13px; color:#6B7280; line-height:20px; }
  </style>
</head>
<body>
  <div class=""empty"">
    <div class=""box"">
      <div class=""title"">Bản đồ tòa nhà</div>
      <div class=""text"">Danh sách đang hiển thị tất cả phòng. Bản đồ chỉ hiện khi khách bấm vào một phòng để xem chi tiết hoặc đặt trước.</div>
    </div>
  </div>
</body>
</html>";
        }

        private string TaoHtmlBanDoToa(CanHo room, Toa toa, KhuVuc khuVuc)
        {
            var address = DiaChiDayDu(toa, khuVuc);
            var fallbackLat = khuVuc != null && khuVuc.ViDo.HasValue ? khuVuc.ViDo.Value : 10.762622;
            var fallbackLng = khuVuc != null && khuVuc.KinhDo.HasValue ? khuVuc.KinhDo.Value : 106.660172;
            var popup = string.Format(
                "<b>{0} - Căn {1}</b><br/>Địa chỉ: {2}<br/>Giá: {3:N0} đ<br/>Trạng thái: {4}",
                HtmlEncode(toa.TenToa),
                room.SoCanHo,
                HtmlEncode(address),
                room.GiaThueNiemYet,
                HtmlEncode(room.TinhTrang));

            return @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"" />
  <style>
    html, body, #map { height: 100%; margin: 0; font-family: Segoe UI, Arial, sans-serif; }
    .notice { position:absolute; z-index:1000; top:12px; left:12px; right:12px; background:#fff; border:1px solid #e5e7eb; padding:10px; border-radius:6px; color:#1f2937; box-shadow:0 8px 24px rgba(30,42,69,.08); }
  </style>
</head>
<body>
  <div class=""notice"">Đang hiển thị vị trí: <b>" + HtmlEncode(toa.TenToa) + @"</b><br/>" + HtmlEncode(address) + @"</div>
  <div id=""map""></div>
  <script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js""></script>
  <script>
    var fallback = [" + fallbackLat.ToString(System.Globalization.CultureInfo.InvariantCulture) + @", " + fallbackLng.ToString(System.Globalization.CultureInfo.InvariantCulture) + @"];
    var map = L.map('map').setView(fallback, 15);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap'
    }).addTo(map);
    function addMarker(lat, lon) {
      map.setView([lat, lon], 16);
      L.marker([lat, lon]).addTo(map).bindPopup('" + JsString(popup) + @"').openPopup();
    }
    fetch('https://nominatim.openstreetmap.org/search?format=json&limit=1&q=' + encodeURIComponent('" + JsString(address) + @"'))
      .then(function(r) { return r.json(); })
      .then(function(data) {
        if (data && data.length > 0) addMarker(parseFloat(data[0].lat), parseFloat(data[0].lon));
        else addMarker(fallback[0], fallback[1]);
      })
      .catch(function() { addMarker(fallback[0], fallback[1]); });
  </script>
</body>
</html>";
        }

        private string DiaChiDayDu(Toa toa, KhuVuc khuVuc)
        {
            var parts = new[]
            {
                toa == null ? null : toa.DiaChi,
                khuVuc == null ? null : khuVuc.Quan,
                khuVuc == null ? null : khuVuc.ThanhPho
            };
            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private static string HtmlEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        private static string JsString(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", string.Empty)
                .Replace("\n", "<br/>");
        }

        private void ResizeCards()
        {
            if (_roomCards.Width <= 0) return;
            var cardCount = _roomCards.Controls.Cast<Control>().Count(c => (c.Tag as string) == "room-card");
            if (cardCount == 0) return;

            var scrollbar = _roomCards.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
            var available = Math.Max(320, _roomCards.ClientSize.Width - scrollbar - 18);
            var minCardWidth = 318;
            var gap = 18;
            var columns = Math.Max(1, available / minCardWidth);
            columns = Math.Min(columns, cardCount);
            var width = Math.Max(300, (available - ((columns - 1) * gap)) / columns - 6);

            foreach (Control control in _roomCards.Controls)
            {
                if ((control.Tag as string) == "room-card")
                    control.Width = width;
            }
            _roomCards.PerformLayout();
        }

        private void DangKyVaDatTruoc(CanHo room)
        {
            using (var dialog = new frmDangKyDatTruoc(room))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string loi;
                if (!_khachThueService.TaoKhachKemTaiKhoan(dialog.Khach, dialog.TenDangNhap,
                        dialog.MatKhau, dialog.Email, dialog.SoDienThoai, out loi))
                {
                    MessageBox.Show(loi, "Không thể tạo tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var phieu = new PhieuDatTruoc
                {
                    MaCanHo = room.MaCanHo,
                    MaKhach = dialog.Khach.MaKhach,
                    SoTienDatCoc = dialog.TienCoc,
                    NgayHetHan = DateTime.Now.AddHours(24),
                    PhuongThucThanhToan = "VietQRDatCoc",
                    GhiChu = "Khach vang lai dang ky tu man hinh tim tro; cho xac nhan chuyen khoan dat coc"
                };

                if (!_phieuDatTruocService.TaoPhieu(phieu, out loi))
                {
                    MessageBox.Show(loi, "Không thể tạo phiếu đặt trước", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var noiDungChuyenKhoan = NoiDungDatCoc(phieu);
                string emailStatus;
                EmailNotificationHelper.GuiThongTinDatTruoc(dialog.Email, dialog.Khach.HoTen,
                    dialog.TenDangNhap, dialog.MatKhau, phieu.MaPhieuDatTruoc, room.MaCanHo,
                    dialog.TienCoc, phieu.NgayHetHan, noiDungChuyenKhoan, dialog.Khach.MaTaiKhoan, out emailStatus);

                var message = new StringBuilder();
                message.AppendLine("Da tao tai khoan va phieu dat truoc.");
                message.AppendLine();
                message.AppendLine("Tên đăng nhập: " + dialog.TenDangNhap);
                message.AppendLine("Mật khẩu tạm: " + dialog.MatKhau);
                message.AppendLine("Mã khách: " + dialog.Khach.MaKhach);
                message.AppendLine("Mã phiếu: " + phieu.MaPhieuDatTruoc);
                message.AppendLine("Mã phòng: " + room.MaCanHo);
                message.AppendLine("Tiền cọc: " + dialog.TienCoc.ToString("N0"));
                message.AppendLine("STK nhận cọc: " + frmQrThanhToan.SoTaiKhoanNhan);
                message.AppendLine("Nội dung CK: " + noiDungChuyenKhoan);
                message.AppendLine(emailStatus);
                Clipboard.SetText(message.ToString());
                MessageBox.Show(message + "\nThông tin đăng nhập và đặt cọc đã được copy.",
                    "Đặt trước thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                using (var qr = new frmQrThanhToan("QR đặt cọc phòng", phieu.MaPhieuDatTruoc,
                    "Phòng " + room.MaCanHo, dialog.TienCoc, noiDungChuyenKhoan))
                {
                    qr.ShowDialog(this);
                }
                TaiDanhSachTro();
            }
        }

        private string NoiDungDatCoc(PhieuDatTruoc phieu)
        {
            return string.Format("DAT COC {0} PHONG {1}", phieu.MaPhieuDatTruoc, phieu.MaCanHo);
        }
    }
}
