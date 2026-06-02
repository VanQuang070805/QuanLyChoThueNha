using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.GUI.Helpers;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmKhachHangHome : MaterialForm
    {
        private CanHoService _canHoService = new CanHoService();
        private ToaService _toaService = new ToaService();
        private PhieuDatTruocService _phieuDatTruocService = new PhieuDatTruocService();
        private HopDongService _hopDongService = new HopDongService();
        private HoaDonThanhToanService _hoaDonService = new HoaDonThanhToanService();
        private PhieuXuLyViPhamService _viPhamService = new PhieuXuLyViPhamService();
        private KhachThueService _khachThueService = new KhachThueService();
        private TaiKhoanService _taiKhoanService = new TaiKhoanService();

        private FlowLayoutPanel roomCards;
        private DataGridView gridPhieuDatTruoc;
        private DataGridView gridHopDong;
        private DataGridView gridHoaDon;
        private DataGridView gridViPham;
        private MaterialLabel lblHeader;
        private RoundedButton btnQrThanhToan;
        private TableLayoutPanel _shell;
        private FlowLayoutPanel _customerMenu;
        private Panel _contentHost;
        private RoundedButton _btnHamburger;
        private readonly Dictionary<string, RoundedButton> _sectionButtons = new Dictionary<string, RoundedButton>();
        private readonly Dictionary<string, Control> _sectionViews = new Dictionary<string, Control>();
        private string _activeSection = "dat-truoc";
        private bool _menuCollapsed;

        public frmKhachHangHome()
        {
            Text = "Giao diện Khách hàng";
            Size = new Size(1120, 720);
            StartPosition = FormStartPosition.CenterParent;
            BuildModernLayout();
            Load += delegate { TaiDuLieu(); };
            Resize += delegate { ResizeRoomCards(); };
        }

        private void BuildModernLayout()
        {
            BackColor = Color.FromArgb(248, 250, 252);

            _shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(18),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            _shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 238));
            _shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var sidebar = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 14,
                BorderColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var sideLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                BackColor = Color.White
            };
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            sidebar.Controls.Add(sideLayout);

            _btnHamburger = new RoundedButton
            {
                Text = "Menu",
                Dock = DockStyle.Fill,
                Radius = 10,
                BackColor = Color.FromArgb(15, 23, 42),
                BorderColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 0, 8)
            };
            _btnHamburger.Click += delegate { ToggleCustomerMenu(); };
            sideLayout.Controls.Add(_btnHamburger, 0, 0);

            _customerMenu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 0)
            };
            sideLayout.Controls.Add(_customerMenu, 0, 1);

            AddSectionButton("dat-truoc", "Can ho da dat");
            AddSectionButton("hop-dong", "Hop dong");
            AddSectionButton("hoa-don", "Hoa don");
            AddSectionButton("vi-pham", "Vi pham");
            AddSectionButton("can-ho-trong", "Can ho dang trong");

            var btnLamMoi = new RoundedButton
            {
                Text = "Lam moi",
                Dock = DockStyle.Fill,
                Radius = 10,
                BackColor = Color.FromArgb(241, 245, 249),
                BorderColor = Color.FromArgb(203, 213, 225),
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 8, 0, 0)
            };
            btnLamMoi.Click += delegate { LamMoiDuLieu(); };
            sideLayout.Controls.Add(btnLamMoi, 0, 2);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(14, 0, 0, 0)
            };
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 162));

            lblHeader = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Thong tin khach hang",
                Font = new Font("Roboto", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblHeader, 0, 0);

            btnQrThanhToan = new RoundedButton
            {
                Text = "Hien QR hoa don",
                Dock = DockStyle.Fill,
                Radius = 10,
                BackColor = Color.FromArgb(37, 99, 235),
                BorderColor = Color.FromArgb(29, 78, 216),
                ForeColor = Color.White,
                Margin = new Padding(10, 10, 0, 10)
            };
            btnQrThanhToan.Click += BtnQrThanhToan_Click;
            header.Controls.Add(btnQrThanhToan, 1, 0);
            content.Controls.Add(header, 0, 0);

            roomCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(8),
                BackColor = Color.White
            };
            gridHopDong = CreateGrid();
            gridHoaDon = CreateGrid();
            gridPhieuDatTruoc = CreateGrid();
            gridViPham = CreateGrid();

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            _sectionViews["dat-truoc"] = CreateContentSurface(gridPhieuDatTruoc);
            _sectionViews["hop-dong"] = CreateContentSurface(gridHopDong);
            _sectionViews["hoa-don"] = CreateContentSurface(gridHoaDon);
            _sectionViews["vi-pham"] = CreateContentSurface(gridViPham);
            _sectionViews["can-ho-trong"] = CreateContentSurface(roomCards);

            foreach (var view in _sectionViews.Values)
            {
                view.Dock = DockStyle.Fill;
                view.Visible = false;
                _contentHost.Controls.Add(view);
            }

            content.Controls.Add(_contentHost, 0, 1);
            _shell.Controls.Add(sidebar, 0, 0);
            _shell.Controls.Add(content, 1, 0);
            Controls.Add(_shell);
            ShowSection(_activeSection);
        }

        private Control CreateContentSurface(Control content)
        {
            var surface = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 14,
                BorderColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            content.Dock = DockStyle.Fill;
            surface.Controls.Add(content);
            return surface;
        }

        private void AddSectionButton(string key, string text)
        {
            var button = new RoundedButton
            {
                Text = text,
                Width = 210,
                Height = 40,
                Radius = 10,
                BackColor = Color.White,
                BorderColor = Color.Transparent,
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 3, 0, 3)
            };
            button.Click += delegate { ShowSection(key); };
            _sectionButtons[key] = button;
            _customerMenu.Controls.Add(button);
        }

        private void ShowSection(string key)
        {
            _activeSection = key;
            foreach (var view in _sectionViews)
                view.Value.Visible = view.Key == key;

            foreach (var item in _sectionButtons)
            {
                var active = item.Key == key;
                item.Value.BackColor = active ? Color.FromArgb(219, 234, 254) : Color.White;
                item.Value.BorderColor = active ? Color.FromArgb(147, 197, 253) : Color.Transparent;
                item.Value.ForeColor = active ? Color.FromArgb(29, 78, 216) : Color.FromArgb(71, 85, 105);
            }

            if (key == "can-ho-trong")
                ResizeRoomCards();
        }

        private void ToggleCustomerMenu()
        {
            _menuCollapsed = !_menuCollapsed;
            _shell.ColumnStyles[0].Width = _menuCollapsed ? 74 : 238;
            _btnHamburger.Text = _menuCollapsed ? "=" : "Menu";
            foreach (var item in _sectionButtons)
            {
                item.Value.Width = _menuCollapsed ? 44 : 210;
                item.Value.TextAlign = _menuCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
                item.Value.Text = _menuCollapsed ? "." : GetSectionText(item.Key);
            }
        }

        private string GetSectionText(string key)
        {
            switch (key)
            {
                case "dat-truoc": return "Can ho da dat";
                case "hop-dong": return "Hop dong";
                case "hoa-don": return "Hoa don";
                case "vi-pham": return "Vi pham";
                case "can-ho-trong": return "Can ho dang trong";
                default: return key;
            }
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(16, 16, 16, 14)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            lblHeader = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Thông tin khách hàng",
                Font = new Font("Roboto", 13F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(lblHeader, 0, 0);

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                Padding = new Padding(0, 8, 0, 0)
            };
            var btnLamMoi = new RoundedButton
            {
                Text = "Làm mới",
                Width = 90,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(107, 114, 128),
                BorderColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White
            };
            btnLamMoi.Click += delegate { LamMoiDuLieu(); };
            btnQrThanhToan = new RoundedButton
            {
                Text = "Hiện QR hóa đơn",
                Width = 140,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(37, 99, 235),
                BorderColor = Color.FromArgb(29, 78, 216),
                ForeColor = Color.White
            };
            btnQrThanhToan.Click += BtnQrThanhToan_Click;
            filters.Controls.Add(btnLamMoi);
            filters.Controls.Add(btnQrThanhToan);
            root.Controls.Add(filters, 0, 1);

            roomCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(4),
                BackColor = Color.White
            };
            gridHopDong = CreateGrid();
            gridHoaDon = CreateGrid();
            gridPhieuDatTruoc = CreateGrid();
            gridViPham = CreateGrid();

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            tabs.TabPages.Add(CreateTab("Can ho da dat", gridPhieuDatTruoc));
            tabs.TabPages.Add(CreateTab("Hop dong", gridHopDong));
            tabs.TabPages.Add(CreateTab("Hoa don", gridHoaDon));
            tabs.TabPages.Add(CreateTab("Vi pham", gridViPham));
            tabs.TabPages.Add(CreateTab("Can ho dang trong", roomCards));
            root.Controls.Add(tabs, 0, 2);
            Controls.Add(root);
        }

        private TabPage CreateTab(string text, Control content)
        {
            var tab = new TabPage(text) { Padding = new Padding(8), BackColor = Color.White };
            content.Dock = DockStyle.Fill;
            tab.Controls.Add(content);
            return tab;
        }

        private void LamMoiDuLieu()
        {
            TaiDuLieu();
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
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(226, 232, 240),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowTemplate = { Height = 34 }
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            return grid;
        }

        private void TaiDuLieu()
        {
            if (roomCards == null || gridPhieuDatTruoc == null || gridHopDong == null || gridHoaDon == null || gridViPham == null) return;
            
            // BỔ SUNG: Khởi tạo lại các dịch vụ để tránh EF caching dữ liệu cũ
            _canHoService = new CanHoService();
            _toaService = new ToaService();
            _phieuDatTruocService = new PhieuDatTruocService();
            _hopDongService = new HopDongService();
            _hoaDonService = new HoaDonThanhToanService();
            _viPhamService = new PhieuXuLyViPhamService();
            _khachThueService = new KhachThueService();
            _taiKhoanService = new TaiKhoanService();

            lblHeader.Text = string.Format("Xin chao {0} [{1}]", SessionContext.HoTen, SessionContext.MaNguoiDung);
            GuiThongBaoPhieuHetHanMoi();
            TaiPhieuDatTruoc();
            TaiPhongTrong();
            TaiHopDongVaHoaDon();
        }

        private void TaiPhieuDatTruoc()
        {
            var phieus = _phieuDatTruocService.LayTatCa()
                .Where(p => p.MaKhach == SessionContext.MaNguoiDung)
                .OrderByDescending(p => p.NgayDatCoc)
                .ToList();
            var canHos = _canHoService.LayTatCa().GroupBy(c => c.MaCanHo).ToDictionary(g => g.Key, g => g.First());
            var toas = _toaService.LayTatCa().GroupBy(t => t.MaToa).ToDictionary(g => g.Key, g => g.First());
 
            gridPhieuDatTruoc.DataSource = new BindingList<object>(phieus.Select(p => new
            {
                p.MaPhieuDatTruoc,
                TenCanHo = LayTenCanHo(p.MaCanHo, canHos),
                TenToa = LayTenToa(p.MaCanHo, canHos, toas),
                p.SoTienDatCoc,
                p.NgayDatCoc,
                p.NgayHetHan,
                p.TrangThai,
                p.PhuongThucThanhToan,
                p.GhiChu
            }).Cast<object>().ToList());
            DinhDangGrid(gridPhieuDatTruoc);
        }

        private void GuiThongBaoPhieuHetHanMoi()
        {
            var phieusHetHan = _phieuDatTruocService.XuLyPhieuChoCocQuaHan24h();
            foreach (var phieu in phieusHetHan.Where(p => p.MaKhach == SessionContext.MaNguoiDung))
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

        private void TaiPhongTrong()
        {
            roomCards.Controls.Clear();

            var phongDangChoCoc = _phieuDatTruocService.LayTatCa()
                .Where(p => p.TrangThai == PhieuDatTruocService.ChoThanhToanCoc ||
                            p.TrangThai == PhieuDatTruocService.DaThanhToanCoc ||
                            p.TrangThai == PhieuDatTruocService.ChoKy)
                .Select(p => p.MaCanHo)
                .ToList();
            var data = _canHoService.LayTheoTinhTrang("Trong");
            data = data.Where(c => !phongDangChoCoc.Contains(c.MaCanHo));
            var rooms = data.OrderBy(c => c.MaToa).ThenBy(c => c.SoCanHo).ToList();
            if (rooms.Count == 0)
            {
                roomCards.Controls.Add(new Label
                {
                    Text = "Khong co can ho phu hop.",
                    AutoSize = true,
                    Padding = new Padding(12)
                });
                return;
            }

            foreach (var room in rooms)
                roomCards.Controls.Add(CreateRoomCard(room));
            ResizeRoomCards();
        }

        private Control CreateRoomCard(CanHo room)
        {
            return CreateModernRoomCard(room);
        }

        private Control CreateModernRoomCard(CanHo room)
        {
            var card = new RoundedPanel
            {
                Width = 300,
                Height = 180,
                Margin = new Padding(4, 4, 14, 14),
                BackColor = Color.White,
                Tag = "room-card",
                Radius = 14,
                BorderColor = Color.FromArgb(226, 232, 240),
                BorderThickness = 1,
                Padding = new Padding(14)
            };

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                BackColor = Color.White
            };
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            body.Controls.Add(new Label
            {
                Text = string.Format("{0} - Can {1}", room.MaToa, room.SoCanHo),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoEllipsis = true
            }, 0, 0);
            body.Controls.Add(new Label
            {
                Text = string.Format("Tang {0} | Ma phong {1}", room.TangSo, room.MaCanHo),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoEllipsis = true
            }, 0, 1);
            body.Controls.Add(new Label
            {
                Text = string.Format("{0:N0} VND/thang", room.GiaThueNiemYet),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(29, 78, 216),
                AutoEllipsis = true
            }, 0, 2);
            body.Controls.Add(new Label
            {
                Text = string.Format("Tien coc: {0:N0} VND", room.TienCocNiemYet),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoEllipsis = true
            }, 0, 3);

            var btnDatPhong = new RoundedButton
            {
                Text = "Dat phong",
                Dock = DockStyle.Fill,
                Radius = 9,
                BackColor = Color.FromArgb(22, 163, 74),
                BorderColor = Color.FromArgb(21, 128, 61),
                ForeColor = Color.White,
                Margin = new Padding(0)
            };
            btnDatPhong.Click += delegate { DatPhong(room); };
            body.Controls.Add(btnDatPhong, 0, 4);
            card.Controls.Add(body);
            return card;
        }

        private Control CreateLegacyRoomCard(CanHo room)
        {
            var card = new Panel
            {
                Width = 280,
                Height = 154,
                Margin = new Padding(4, 4, 12, 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Tag = "room-card"
            };

            card.Controls.Add(new Label
            {
                Text = string.Format("{0} - Can {1}", room.MaToa, room.SoCanHo),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            });
            card.Controls.Add(new Label
            {
                Text = string.Format("Tang: {0} | Ma phong: {1}", room.TangSo, room.MaCanHo),
                Location = new Point(10, 38),
                AutoSize = true
            });
            card.Controls.Add(new Label
            {
                Text = string.Format("Gia: {0:N0} VND | Coc: {1:N0} VND", room.GiaThueNiemYet, room.TienCocNiemYet),
                Location = new Point(10, 62),
                AutoSize = true
            });

            var btnDatPhong = new RoundedButton
            {
                Text = "Đặt phòng",
                Width = 100,
                Height = 32,
                Radius = 8,
                BackColor = Color.FromArgb(16, 185, 129),
                BorderColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White,
                Location = new Point(8, 92)
            };
            btnDatPhong.Click += delegate { DatPhong(room); };
            card.Controls.Add(btnDatPhong);
            return card;
        }

        private void ResizeRoomCards()
        {
            if (roomCards == null || roomCards.Width <= 0) return;
            var available = Math.Max(260, roomCards.ClientSize.Width - 12);
            var columns = Math.Max(1, available / 300);
            if (available >= 1180) columns = Math.Max(columns, 4);
            else if (available >= 860) columns = Math.Max(columns, 3);
            else if (available >= 560) columns = Math.Max(columns, 2);
            var width = Math.Max(250, (available - (columns * 16)) / columns);
            foreach (Control control in roomCards.Controls)
            {
                if ((control.Tag as string) == "room-card")
                    control.Width = width;
            }
        }

        private void DatPhong(CanHo room)
        {
            if (!SessionContext.LaKhachThue || string.IsNullOrWhiteSpace(SessionContext.MaNguoiDung))
            {
                MessageBox.Show("Chi khach hang moi duoc dat phong.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tienCoc = room.TienCocNiemYet > 0 ? room.TienCocNiemYet : room.GiaThueNiemYet;
            var confirm = MessageBox.Show(
                string.Format("Dat phong {0} voi tien coc {1:N0}?", room.MaCanHo, tienCoc),
                "Xac nhan dat phong",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var phieu = new PhieuDatTruoc
            {
                MaCanHo = room.MaCanHo,
                MaKhach = SessionContext.MaNguoiDung,
                SoTienDatCoc = tienCoc,
                NgayHetHan = DateTime.Now.AddHours(24),
                PhuongThucThanhToan = "VietQRDatCoc",
                GhiChu = "Khach hang dat phong tu trang ca nhan; cho xac nhan chuyen khoan dat coc trong 24h"
            };

            string loi;
            if (!_phieuDatTruocService.TaoPhieu(phieu, out loi))
            {
                MessageBox.Show(loi, "Khong the dat phong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var noiDungChuyenKhoan = string.Format("DAT COC {0} PHONG {1}", phieu.MaPhieuDatTruoc, room.MaCanHo);
            GuiEmailDatCoc(phieu, noiDungChuyenKhoan);
            MessageBox.Show("Da tao phieu dat phong. Vui long quet QR de thanh toan tien dat coc.",
                "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
            using (var qr = new frmQrThanhToan("QR dat coc phong", phieu.MaPhieuDatTruoc,
                "Phong " + room.MaCanHo, tienCoc, noiDungChuyenKhoan))
            {
                qr.ShowDialog(this);
            }
            TaiDuLieu();
        }

        private void GuiEmailDatCoc(PhieuDatTruoc phieu, string noiDungChuyenKhoan)
        {
            var khach = _khachThueService.LayTheoMa(phieu.MaKhach);
            var taiKhoan = khach == null ? null : _taiKhoanService.LayTheoMa(khach.MaTaiKhoan);
            string thongBao;
            EmailNotificationHelper.GuiThongTinDatTruoc(
                taiKhoan == null ? string.Empty : taiKhoan.Email,
                khach == null ? string.Empty : khach.HoTen,
                taiKhoan == null ? string.Empty : taiKhoan.TenDangNhap,
                "(mat khau da cap truoc)",
                phieu.MaPhieuDatTruoc,
                phieu.MaCanHo,
                phieu.SoTienDatCoc,
                phieu.NgayHetHan,
                noiDungChuyenKhoan,
                khach == null ? null : khach.MaTaiKhoan,
                out thongBao);
        }

        private void TaiHopDongVaHoaDon()
        {
            var hopDongs = _hopDongService.LayTheoKhach(SessionContext.MaNguoiDung)
                .OrderByDescending(h => h.NgayTao)
                .ToList();
            var canHos = _canHoService.LayTatCa().GroupBy(c => c.MaCanHo).ToDictionary(g => g.Key, g => g.First());
            var toas = _toaService.LayTatCa().GroupBy(t => t.MaToa).ToDictionary(g => g.Key, g => g.First());
            var hopDongMap = hopDongs.ToDictionary(h => h.MaHopDong, h => h);

            gridHopDong.DataSource = new BindingList<object>(hopDongs.Select(h => new
            {
                h.MaHopDong,
                TenCanHo = LayTenCanHo(h.MaCanHo, canHos),
                TenToa = LayTenToa(h.MaCanHo, canHos, toas),
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
                .ToList();
            
            gridHoaDon.DataSource = new BindingList<object>(hoaDons.Select(h => {
                var hd = hopDongMap.ContainsKey(h.MaHopDong) ? hopDongMap[h.MaHopDong] : null;
                var maCanHo = hd?.MaCanHo;
                return new {
                    h.MaHoaDon,
                    h.MaHopDong,
                    TenCanHo = maCanHo != null ? LayTenCanHo(maCanHo, canHos) : "",
                    TenToa = maCanHo != null ? LayTenToa(maCanHo, canHos, toas) : "",
                    h.MaLoaiHoaDon,
                    h.KyThanhToan,
                    h.ChiSoDienCu,
                    h.ChiSoDienMoi,
                    h.ChiSoNuocCu,
                    h.ChiSoNuocMoi,
                    h.SoTienPhaiTra,
                    h.SoTienDaTra,
                    h.NgayDaoHan,
                    h.NgayThanhToan,
                    h.TrangThai,
                    h.PhuongThucThanhToan
                };
            }).Cast<object>().ToList());
 
            var viPhams = maHopDongs
                .SelectMany(ma => _viPhamService.LayTheoHopDong(ma))
                .OrderByDescending(v => v.NgayGhiNhan)
                .ToList();
            
            gridViPham.DataSource = new BindingList<object>(viPhams.Select(v => {
                var hd = hopDongMap.ContainsKey(v.MaHopDong) ? hopDongMap[v.MaHopDong] : null;
                var maCanHo = hd?.MaCanHo;
                return new
                {
                    v.MaViPham,
                    v.MaHopDong,
                    TenCanHo = maCanHo != null ? LayTenCanHo(maCanHo, canHos) : "",
                    TenToa = maCanHo != null ? LayTenToa(maCanHo, canHos, toas) : "",
                    v.LoaiViPham,
                    v.MoTa,
                    v.PhiBoiThuong,
                    v.TruVaoCoc,
                    v.TinhTrang,
                    v.NgayGhiNhan
                };
            }).Cast<object>().ToList());

            DinhDangGrid(gridHopDong);
            DinhDangGrid(gridHoaDon);
            DinhDangGrid(gridViPham);
        }
 
        private void DinhDangGrid(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                switch (col.Name)
                {
                    case "MaPhieuDatTruoc": col.HeaderText = "Mã phiếu"; break;
                    case "TenCanHo": col.HeaderText = "Tên căn hộ"; break;
                    case "TenToa": col.HeaderText = "Tên tòa"; break;
                    case "SoTienDatCoc": col.HeaderText = "Tiền đặt cọc"; break;
                    case "NgayDatCoc": col.HeaderText = "Ngày đặt cọc"; break;
                    case "NgayHetHan": col.HeaderText = "Ngày hết hạn"; break;
                    case "TrangThai": col.HeaderText = "Trạng thái"; break;
                    case "PhuongThucThanhToan": col.HeaderText = "Phương thức"; break;
                    case "GhiChu": col.HeaderText = "Ghi chú"; break;

                    case "MaHopDong": col.HeaderText = "Mã hợp đồng"; break;
                    case "NgayBatDau": col.HeaderText = "Ngày bắt đầu"; break;
                    case "NgayKetThuc": col.HeaderText = "Ngày kết thúc"; break;
                    case "GiaThueChot": col.HeaderText = "Giá thuê chốt"; break;
                    case "TienCocChot": col.HeaderText = "Tiền cọc chốt"; break;
                    case "CocTruocDaTru": col.HeaderText = "Cọc trước đã trừ"; break;
                    case "CocConPhaiNop": col.HeaderText = "Cọc còn phải nộp"; break;

                    case "MaHoaDon": col.HeaderText = "Mã hóa đơn"; break;
                    case "MaLoaiHoaDon": col.HeaderText = "Loại hóa đơn"; break;
                    case "KyThanhToan": col.HeaderText = "Kỳ thanh toán"; break;
                    case "ChiSoDienCu": col.HeaderText = "Chỉ số điện cũ"; break;
                    case "ChiSoDienMoi": col.HeaderText = "Chỉ số điện mới"; break;
                    case "ChiSoNuocCu": col.HeaderText = "Chỉ số nước cũ"; break;
                    case "ChiSoNuocMoi": col.HeaderText = "Chỉ số nước mới"; break;
                    case "SoTienPhaiTra": col.HeaderText = "Số tiền phải trả"; break;
                    case "SoTienDaTra": col.HeaderText = "Số tiền đã trả"; break;
                    case "NgayDaoHan": col.HeaderText = "Ngày đáo hạn"; break;
                    case "NgayThanhToan": col.HeaderText = "Ngày thanh toán"; break;

                    case "MaViPham": col.HeaderText = "Mã vi phạm"; break;
                    case "LoaiViPham": col.HeaderText = "Loại vi phạm"; break;
                    case "MoTa": col.HeaderText = "Mô tả"; break;
                    case "PhiBoiThuong": col.HeaderText = "Phí bồi thường"; break;
                    case "TruVaoCoc": col.HeaderText = "Trừ vào cọc"; break;
                    case "TinhTrang": col.HeaderText = "Tình trạng"; break;
                    case "NgayGhiNhan": col.HeaderText = "Ngày ghi nhận"; break;
                }

                if (col.Name.Contains("Tien") || col.Name.Contains("Gia") || col.Name.Contains("Phi") || col.Name.Contains("Coc"))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private string LayTenCanHo(string maCanHo, IDictionary<string, CanHo> canHos)
        {
            CanHo canHo;
            return canHos.TryGetValue(maCanHo, out canHo)
                ? string.Format("Can {0}", canHo.SoCanHo)
                : maCanHo;
        }

        private string LayTenToa(string maCanHo, IDictionary<string, CanHo> canHos, IDictionary<string, Toa> toas)
        {
            CanHo canHo;
            if (!canHos.TryGetValue(maCanHo, out canHo)) return string.Empty;
            Toa toa;
            return toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
        }

        private void BtnQrThanhToan_Click(object sender, EventArgs e)
        {
            if (gridHoaDon.CurrentRow == null)
            {
                MessageBox.Show("Chon hoa don can thanh toan.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
 
            var maHoaDon = gridHoaDon.CurrentRow.Cells["MaHoaDon"].Value?.ToString();
            if (string.IsNullOrEmpty(maHoaDon)) return;
            var hoaDon = _hoaDonService.LayTheoMa(maHoaDon);
            if (hoaDon == null) return;
            if (hoaDon.SoTienPhaiTra <= hoaDon.SoTienDaTra)
            {
                MessageBox.Show("Hoa don nay da thanh toan du.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            new frmQrThanhToan(hoaDon).ShowDialog(this);
        }
    }
}
