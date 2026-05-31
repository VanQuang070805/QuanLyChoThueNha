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
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Auth;
using QuanLyChoThueNha.GUI.Helpers;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmTimTroPublic : MaterialForm
    {
        private readonly CanHoService _canHoService = new CanHoService();
        private readonly KhuVucService _khuVucService = new KhuVucService();
        private readonly ToaService _toaService = new ToaService();
        private readonly LoaiCanHoService _loaiService = new LoaiCanHoService();
        private readonly TienNghiService _tienNghiService = new TienNghiService();
        private readonly KhachThueService _khachThueService = new KhachThueService();
        private readonly TaiKhoanService _taiKhoanService = new TaiKhoanService();
        private readonly PhieuDatTruocService _phieuDatTruocService = new PhieuDatTruocService();

        private readonly FlowLayoutPanel _roomCards = new FlowLayoutPanel();
        private readonly ComboBox _cboKhuVuc = new ComboBox();
        private readonly ComboBox _cboToa = new ComboBox();
        private readonly ComboBox _cboLoai = new ComboBox();
        private readonly NumericUpDown _numBanKinh = new NumericUpDown();
        private readonly NumericUpDown _numGiaTu = new NumericUpDown();
        private readonly NumericUpDown _numGiaDen = new NumericUpDown();
        private readonly MaterialTextBox _txtTimKiem = new MaterialTextBox();
        private readonly MaterialLabel _lblCount = new MaterialLabel();
        private readonly WebView2 _mapView = new WebView2();

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
            Text = "Tim tro";
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
            HienThiBanDoMacDinh();
            TaiDanhSachTro();
            Resize += delegate { ResizeCards(); };
        }

        private void BuildLayout()
        {
            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(0, 64, 0, 0),
                BackColor = Color.FromArgb(248, 249, 252)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.Controls.Add(CreateSidebar(), 0, 0);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(24),
                BackColor = Color.FromArgb(248, 249, 252)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

            var title = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Tim tro dang trong",
                Font = new Font("Roboto", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var btnKhach = new MaterialButton { Text = "Tai khoan khach", Dock = DockStyle.Fill };
            btnKhach.Click += delegate { new frmLogin(new[] { "KhachThue" }, "Dang nhap khach hang").Show(); };
            var btnNoiBo = new MaterialButton { Text = "Cong noi bo", Dock = DockStyle.Fill };
            btnNoiBo.Click += delegate { new frmLogin(new[] { "Admin", "NhanVien" }, "Dang nhap noi bo").Show(); };
            var btnLamMoi = new MaterialButton { Text = "Lam moi", Dock = DockStyle.Fill };
            btnLamMoi.Click += delegate { LamMoiDuLieu(); };

            top.Controls.Add(title, 0, 0);
            top.Controls.Add(btnLamMoi, 1, 0);
            top.Controls.Add(btnKhach, 2, 0);
            top.Controls.Add(btnNoiBo, 3, 0);
            root.Controls.Add(top, 0, 0);

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                BackColor = Color.FromArgb(248, 249, 252),
                Padding = new Padding(0, 14, 0, 0)
            };
            _txtTimKiem.Width = 280;
            _txtTimKiem.Hint = "Tim theo ma, toa, so can";
            _txtTimKiem.TextChanged += delegate { TaiDanhSachTro(); };

            SetupCombo(_cboToa, 180);
            SetupCombo(_cboKhuVuc, 210);
            SetupCombo(_cboLoai, 180);
            _cboKhuVuc.SelectedIndexChanged += delegate { TaiDanhSachTro(); };
            _cboToa.SelectedIndexChanged += delegate { TaiDanhSachTro(); };
            _cboLoai.SelectedIndexChanged += delegate { TaiDanhSachTro(); };

            _numBanKinh.Width = 90;
            _numBanKinh.Minimum = 1;
            _numBanKinh.Maximum = 100;
            _numBanKinh.Value = 3;
            _numBanKinh.ValueChanged += delegate { TaiDanhSachTro(); };

            SetupMoney(_numGiaTu);
            SetupMoney(_numGiaDen);
            _numGiaTu.ValueChanged += delegate { TaiDanhSachTro(); };
            _numGiaDen.ValueChanged += delegate { TaiDanhSachTro(); };

            filters.Controls.Add(_txtTimKiem);
            filters.Controls.Add(Label("Khu vuc"));
            filters.Controls.Add(_cboKhuVuc);
            filters.Controls.Add(Label("Ban kinh km"));
            filters.Controls.Add(_numBanKinh);
            filters.Controls.Add(Label("Toa"));
            filters.Controls.Add(_cboToa);
            filters.Controls.Add(Label("Loai"));
            filters.Controls.Add(_cboLoai);
            filters.Controls.Add(Label("Gia tu"));
            filters.Controls.Add(_numGiaTu);
            filters.Controls.Add(Label("Gia den"));
            filters.Controls.Add(_numGiaDen);
            filters.Controls.Add(_lblCount);
            root.Controls.Add(filters, 0, 1);

            _roomCards.Dock = DockStyle.Fill;
            _roomCards.AutoScroll = true;
            _roomCards.WrapContents = true;
            _roomCards.Padding = new Padding(2);
            _roomCards.BackColor = Color.White;

            _mapView.Dock = DockStyle.Fill;
            _mapView.DefaultBackgroundColor = Color.White;

            var content = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            content.Panel1.Controls.Add(_roomCards);
            content.Panel2.Controls.Add(_mapView);
            content.Panel1.BackColor = Color.White;
            content.Panel2.BackColor = Color.White;
            content.SizeChanged += delegate { CapNhatKhoangChia(content); };
            root.Controls.Add(content, 0, 2);
            shell.Controls.Add(root, 1, 0);
            Controls.Add(shell);
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

        private void CapNhatKhoangChia(SplitContainer content)
        {
            const int panel1Min = 280;
            const int panel2Min = 260;
            if (content.Width <= panel1Min + panel2Min + content.SplitterWidth)
                return;

            var desired = Math.Max(panel1Min,
                Math.Min(content.Width - panel2Min - content.SplitterWidth,
                    (int)(content.Width * 0.62)));
            if (content.SplitterDistance != desired)
                content.SplitterDistance = desired;
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
        }

        private void NapBoLoc()
        {
            var selectedKhuVuc = _cboKhuVuc.SelectedValue == null ? string.Empty : _cboKhuVuc.SelectedValue.ToString();
            var selectedToa = _cboToa.SelectedValue == null ? string.Empty : _cboToa.SelectedValue.ToString();
            var selectedLoai = _cboLoai.SelectedValue == null ? string.Empty : _cboLoai.SelectedValue.ToString();

            _cboKhuVuc.DisplayMember = "Display";
            _cboKhuVuc.ValueMember = "Value";
            var khuVucOptions = new List<FilterOption> { new FilterOption(string.Empty, "Tat ca khu vuc") };
            khuVucOptions.AddRange(_khuVucService.LayTatCa()
                .OrderBy(k => k.TenKhuVuc)
                .Select(k => new FilterOption(k.MaKhuVuc, k.TenKhuVuc)));
            _cboKhuVuc.DataSource = khuVucOptions;
            _cboKhuVuc.SelectedValue = khuVucOptions.Any(x => x.Value == selectedKhuVuc) ? selectedKhuVuc : string.Empty;

            _cboToa.DisplayMember = "Display";
            _cboToa.ValueMember = "Value";
            var toaOptions = new List<FilterOption> { new FilterOption(string.Empty, "Tat ca toa") };
            toaOptions.AddRange(_toaService.LayTatCa()
                .OrderBy(t => t.TenToa)
                .Select(t => new FilterOption(t.MaToa, t.TenToa)));
            _cboToa.DataSource = toaOptions;
            _cboToa.SelectedValue = toaOptions.Any(x => x.Value == selectedToa) ? selectedToa : string.Empty;

            _cboLoai.DisplayMember = "Display";
            _cboLoai.ValueMember = "Value";
            var loaiOptions = new List<FilterOption> { new FilterOption(string.Empty, "Tat ca loai") };
            loaiOptions.AddRange(_loaiService.LayTatCa()
                .OrderBy(l => l.TenLoai)
                .Select(l => new FilterOption(l.MaLoai, l.TenLoai)));
            _cboLoai.DataSource = loaiOptions;
            _cboLoai.SelectedValue = loaiOptions.Any(x => x.Value == selectedLoai) ? selectedLoai : string.Empty;
        }

        private void LamMoiDuLieu()
        {
            NapBoLoc();
            TaiDanhSachTro();
        }

        private void TaiDanhSachTro()
        {
            if (_roomCards == null) return;
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
            var kw = (_txtTimKiem.Text ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(kw))
            {
                data = data.Where(c =>
                {
                    Toa toa;
                    KhuVuc khuVuc = null;
                    toas.TryGetValue(c.MaToa, out toa);
                    if (toa != null) khuVucs.TryGetValue(toa.MaKhuVuc, out khuVuc);

                    return (c.MaCanHo ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                           (c.MaToa ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                           c.SoCanHo.ToString().Contains(kw) ||
                           (toa != null && (
                               (toa.TenToa ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                               (toa.DiaChi ?? string.Empty).ToLowerInvariant().Contains(kw))) ||
                           (khuVuc != null && (
                               (khuVuc.TenKhuVuc ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                               (khuVuc.Quan ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                               (khuVuc.ThanhPho ?? string.Empty).ToLowerInvariant().Contains(kw)));
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
            if (_numGiaTu.Value > 0)
            {
                var giaTu = _numGiaTu.Value;
                data = data.Where(c => c.GiaThueNiemYet >= giaTu);
            }
            if (_numGiaDen.Value > 0)
            {
                var giaDen = _numGiaDen.Value;
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
            _lblCount.Text = string.Format("{0:N0} phong", rooms.Count);

            if (rooms.Count == 0)
            {
                _roomCards.Controls.Add(new Label
                {
                    Text = "Khong co phong phu hop.",
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
            var mauVien = coTheDat ? Color.FromArgb(229, 231, 235) :
                room.TinhTrang == "DangThue" ? Color.FromArgb(251, 146, 60) :
                Color.FromArgb(96, 165, 250);
            var trangThaiHienThi = dangChoCoc ? "Dang cho coc" :
                room.TinhTrang == "Trong" ? "Con trong" :
                room.TinhTrang == "DangThue" ? "Dang cho thue" : room.TinhTrang;

            var card = new Panel
            {
                Height = 330,
                Width = 330,
                Margin = new Padding(4, 4, 12, 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = mauNen,
                Tag = "room-card"
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
                Text = string.Format("{0} | Tang {1} | {2:N1} m2 | {3}",
                    room.MaLoai, room.TangSo, room.DienTich, trangThaiHienThi),
                Dock = DockStyle.Fill,
                ForeColor = mauVien
            }, 0, 1);
            body.Controls.Add(new Label
            {
                Text = string.Format("Gia {0:N0} - Coc {1:N0}", room.GiaThueNiemYet, room.TienCocNiemYet),
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(0, 105, 92)
            }, 0, 2);
            body.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(room.MoTa) ? "Phong dang san sang cho thue." : room.MoTa,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            }, 0, 3);

            var btnDatTruoc = new MaterialButton
            {
                Text = coTheDat ? "Lien he dat truoc" : "Xem vi tri",
                Dock = DockStyle.Left,
                AutoSize = true,
                Enabled = true
            };
            btnDatTruoc.Click += delegate
            {
                HienThiBanDoToa(room);
                if (coTheDat)
                    DangKyVaDatTruoc(room);
            };
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            footer.Controls.Add(new Label
            {
                Text = LayTienNghiHienThi(room.MaCanHo),
                AutoSize = false,
                Width = 190,
                Height = 36,
                AutoEllipsis = true
            });
            footer.Controls.Add(btnDatTruoc);
            body.Controls.Add(footer, 0, 4);
            card.Controls.Add(body);
            body.BringToFront();
            return card;
        }

        private string LayTenToa(string maToa)
        {
            var toa = _toaService.LayTheoMa(maToa);
            return toa == null || string.IsNullOrWhiteSpace(toa.TenToa) ? maToa : toa.TenToa;
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
            return names.Count == 0 ? "Tien nghi: dang cap nhat" : "Tien nghi: " + string.Join(", ", names);
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
                    var text = "NHA TRO";
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
      <div class=""title"">Ban do toa nha</div>
      <div class=""text"">Danh sach dang hien tat ca phong. Ban do chi hien khi khach bam vao mot phong de xem/len he dat truoc.</div>
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
                "<b>{0} - Can {1}</b><br/>Dia chi: {2}<br/>Gia: {3:N0} d<br/>Trang thai: {4}",
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
  <div class=""notice"">Dang hien vi tri: <b>" + HtmlEncode(toa.TenToa) + @"</b><br/>" + HtmlEncode(address) + @"</div>
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
            var columns = Math.Max(1, _roomCards.ClientSize.Width / 340);
            var width = Math.Max(280, (_roomCards.ClientSize.Width - (columns * 18)) / columns);
            foreach (Control control in _roomCards.Controls)
            {
                if ((control.Tag as string) == "room-card")
                    control.Width = width;
            }
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
                    MessageBox.Show(loi, "Khong the tao tai khoan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show(loi, "Khong the tao phieu dat truoc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                message.AppendLine("Ten dang nhap: " + dialog.TenDangNhap);
                message.AppendLine("Mat khau tam: " + dialog.MatKhau);
                message.AppendLine("Ma khach: " + dialog.Khach.MaKhach);
                message.AppendLine("Ma phieu: " + phieu.MaPhieuDatTruoc);
                message.AppendLine("Ma phong: " + room.MaCanHo);
                message.AppendLine("Tien coc: " + dialog.TienCoc.ToString("N0"));
                message.AppendLine("Noi dung CK: " + noiDungChuyenKhoan);
                message.AppendLine(emailStatus);
                Clipboard.SetText(message.ToString());
                MessageBox.Show(message + "\nThong tin dang nhap va dat coc da duoc copy.",
                    "Dat truoc thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                using (var qr = new frmQrThanhToan("QR dat coc phong", phieu.MaPhieuDatTruoc,
                    "Phong " + room.MaCanHo, dialog.TienCoc, noiDungChuyenKhoan))
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
