using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.GUI.Forms.KhachHang;
using QuanLyChoThueNha.GUI.Helpers;
using QuanLyChoThueNha.Model.Entities;
using HopDongEntity = QuanLyChoThueNha.Model.Entities.HopDong;

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
        
        private readonly RoundedButton _btnLamMoi = new RoundedButton 
        { 
            Text = "Làm mới", 
            Width = 110, 
            Height = 36, 
            Radius = 8, 
            BackColor = Color.FromArgb(107, 114, 128), 
            BorderColor = Color.FromArgb(75, 85, 99),
            Margin = new Padding(0, 8, 8, 4)
        };
        
        private readonly RoundedButton _btnXacNhanCoc = new RoundedButton 
        { 
            Text = "Xác nhận đã nhận cọc", 
            Width = 190, 
            Height = 36, 
            Radius = 8, 
            BackColor = Color.FromArgb(22, 163, 74), 
            BorderColor = Color.FromArgb(21, 128, 61),
            Margin = new Padding(0, 8, 8, 4)
        };
        
        private readonly RoundedButton _btnGuiLaiEmail = new RoundedButton 
        { 
            Text = "Gửi lại email/QR", 
            Width = 160, 
            Height = 36, 
            Radius = 8, 
            BackColor = Color.FromArgb(37, 99, 235), 
            BorderColor = Color.FromArgb(29, 78, 216),
            Margin = new Padding(0, 8, 8, 4)
        };
        
        private readonly RoundedButton _btnEmailLog = new RoundedButton 
        { 
            Text = "Lịch sử email", 
            Width = 140, 
            Height = 36, 
            Radius = 8, 
            BackColor = Color.FromArgb(124, 58, 237), 
            BorderColor = Color.FromArgb(109, 40, 217),
            Margin = new Padding(0, 8, 8, 4)
        };
        
        private MaterialLabel _lblHeader;

        public frmNhanVienHome()
        {
            Text = "Trang nhân viên";
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
                Padding = new Padding(16, 16, 16, 16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            _lblHeader = new MaterialLabel
            {
                Text = "Công việc nhân viên",
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
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
            top.Controls.Add(CreateGroup("Phiếu đặt phòng cần xử lý", _gridDatPhong), 0, 0);
            top.Controls.Add(CreateGroup("Hợp đồng sắp hết hạn", _gridHopDong), 1, 0);
            root.Controls.Add(top, 0, 2);

            root.Controls.Add(CreateGroup("Hóa đơn cần theo dõi", _gridHoaDon), 0, 3);
            Controls.Add(root);
        }

        private void LoadData()
        {
            _lblHeader.Text = string.Format("Công việc của {0} [{1}]", SessionContext.HoTen, SessionContext.MaNguoiDung);
            GuiThongBaoPhieuHetHanMoi();

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
            AddKpi("Phòng trống", _canHoService.LayTheoTinhTrang("Trong").Count().ToString("N0"), Color.FromArgb(0, 137, 123));
            AddKpi("Chờ cọc", phieuChoCoc.Count.ToString("N0"), Color.FromArgb(245, 124, 0));
            AddKpi("Chờ ký HĐ", phieuChoKy.Count.ToString("N0"), Color.FromArgb(25, 118, 210));
            AddKpi("HĐ sắp hết hạn", hopDongSapHetHan.Count.ToString("N0"), Color.FromArgb(123, 31, 162));
            AddKpi("Hóa đơn cần xử lý", hoaDonCanTheoDoi.Count.ToString("N0"), Color.FromArgb(198, 40, 40));

            var canHos = _canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = new ToaService().LayTatCa().ToDictionary(t => t.MaToa);
            var hopDongs = _hopDongService.LayTatCa().ToDictionary(hd => hd.MaHopDong);
            var khachs = _khachThueService.LayTatCa().ToDictionary(k => k.MaKhach);

            _gridDatPhong.DataSource = new BindingList<object>(phieuCanXuLy.Select(p => new
            {
                p.MaPhieuDatTruoc,
                TenKhach = khachs.ContainsKey(p.MaKhach) ? khachs[p.MaKhach].HoTen : p.MaKhach,
                TenCanHo = canHos.ContainsKey(p.MaCanHo) ? "Căn " + canHos[p.MaCanHo].SoCanHo : p.MaCanHo,
                TenToa = canHos.ContainsKey(p.MaCanHo) && toas.ContainsKey(canHos[p.MaCanHo].MaToa) ? toas[canHos[p.MaCanHo].MaToa].TenToa : "",
                p.SoTienDatCoc,
                p.NgayDatCoc,
                p.NgayHetHan,
                p.TrangThai
            }).Cast<object>().ToList());

            _gridHopDong.DataSource = new BindingList<object>(hopDongSapHetHan.Select(h => new
            {
                h.MaHopDong,
                TenKhach = khachs.ContainsKey(h.MaKhach) ? khachs[h.MaKhach].HoTen : h.MaKhach,
                TenCanHo = canHos.ContainsKey(h.MaCanHo) ? "Căn " + canHos[h.MaCanHo].SoCanHo : h.MaCanHo,
                TenToa = canHos.ContainsKey(h.MaCanHo) && toas.ContainsKey(canHos[h.MaCanHo].MaToa) ? toas[canHos[h.MaCanHo].MaToa].TenToa : "",
                h.NgayKetThuc,
                SoNgayConLai = Math.Max(0, (h.NgayKetThuc.Date - DateTime.Today).Days),
                h.TrangThai
            }).Cast<object>().ToList());

            _gridHoaDon.DataSource = new BindingList<object>(hoaDonCanTheoDoi.Select(h => {
                HopDongEntity hd = null;
                hopDongs.TryGetValue(h.MaHopDong, out hd);
                var maCanHo = hd?.MaCanHo;
                var maKhach = hd?.MaKhach;
                return new
                {
                    h.MaHoaDon,
                    h.MaHopDong,
                    TenKhach = maKhach != null && khachs.ContainsKey(maKhach) ? khachs[maKhach].HoTen : maKhach,
                    TenCanHo = maCanHo != null && canHos.ContainsKey(maCanHo) ? "Căn " + canHos[maCanHo].SoCanHo : "",
                    TenToa = maCanHo != null && canHos.ContainsKey(maCanHo) && toas.ContainsKey(canHos[maCanHo].MaToa) ? toas[canHos[maCanHo].MaToa].TenToa : "",
                    h.KyThanhToan,
                    h.SoTienPhaiTra,
                    h.SoTienDaTra,
                    h.NgayDaoHan,
                    h.TrangThai
                };
            }).Cast<object>().ToList());

            DinhDangGrid(_gridDatPhong);
            DinhDangGrid(_gridHopDong);
            DinhDangGrid(_gridHoaDon);
            GridFormatterHelper.SetupCellFormatting(_gridDatPhong);
            GridFormatterHelper.SetupCellFormatting(_gridHopDong);
            GridFormatterHelper.SetupCellFormatting(_gridHoaDon);
        }

        private void GuiThongBaoPhieuHetHanMoi()
        {
            var phieusHetHan = _phieuDatTruocService.XuLyPhieuChoCocQuaHan24h();
            foreach (var phieu in phieusHetHan)
            {
                var khach = _khachThueService.LayTheoMa(phieu.MaKhach);
                TaiKhoan taiKhoan = khach == null ? null : _taiKhoanService.LayTheoMa(khach.MaTaiKhoan);
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

        private void BtnXacNhanCoc_Click(object sender, EventArgs e)
        {
            var maPhieu = LayMaPhieuDangChon();
            if (string.IsNullOrWhiteSpace(maPhieu))
            {
                MessageBox.Show("Chọn phiếu đặt trước cần xác nhận cọc.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string loi;
            if (!_phieuDatTruocService.XacNhanDaNhanCoc(maPhieu, out loi))
            {
                MessageBox.Show(loi, "Không thể xác nhận cọc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Đã xác nhận nhận cọc. Phiếu đã chuyển sang chờ ký hợp đồng.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
        }

        private void BtnGuiLaiEmail_Click(object sender, EventArgs e)
        {
            var maPhieu = LayMaPhieuDangChon();
            if (string.IsNullOrWhiteSpace(maPhieu))
            {
                MessageBox.Show("Chọn phiếu đặt trước cần gửi lại email/QR.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var phieu = _phieuDatTruocService.LayTheoMa(maPhieu);
            if (phieu == null)
            {
                MessageBox.Show("Không tìm thấy phiếu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            MessageBox.Show(thongBao, "Gửi lại email", MessageBoxButtons.OK, MessageBoxIcon.Information);
            using (var qr = new frmQrThanhToan("QR đặt cọc phòng", phieu.MaPhieuDatTruoc,
                "Phòng " + phieu.MaCanHo, phieu.SoTienDatCoc, noiDung))
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
            return string.Format("ĐẶT CỌC {0} PHÒNG {1}", phieu.MaPhieuDatTruoc, phieu.MaCanHo);
        }

        private static DataGridView CreateGrid()
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
                EnableHeadersVisualStyles = false
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
            return grid;
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

        private void DinhDangGrid(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                switch (col.Name)
                {
                    case "MaPhieuDatTruoc": col.HeaderText = "Mã phiếu"; break;
                    case "MaKhach":
                    case "TenKhach": col.HeaderText = "Tên khách"; break;
                    case "TenCanHo": col.HeaderText = "Tên căn hộ"; break;
                    case "TenToa": col.HeaderText = "Tên tòa"; break;
                    case "SoTienDatCoc": col.HeaderText = "Tiền đặt cọc"; break;
                    case "NgayDatCoc": col.HeaderText = "Ngày đặt cọc"; break;
                    case "NgayHetHan": col.HeaderText = "Ngày hết hạn"; break;
                    case "TrangThai": col.HeaderText = "Trạng thái"; break;
                    case "PhuongThucThanhToan": col.HeaderText = "Phương thức"; break;

                    case "MaHopDong": col.HeaderText = "Mã hợp đồng"; break;
                    case "NgayBatDau": col.HeaderText = "Ngày bắt đầu"; break;
                    case "NgayKetThuc": col.HeaderText = "Ngày kết thúc"; break;
                    case "GiaThueChot": col.HeaderText = "Giá thuê chốt"; break;
                    case "TienCocChot": col.HeaderText = "Tiền cọc chốt"; break;
                    case "SoNgayConLai": col.HeaderText = "Số ngày còn lại"; break;

                    case "MaHoaDon": col.HeaderText = "Mã hóa đơn"; break;
                    case "KyThanhToan": col.HeaderText = "Kỳ thanh toán"; break;
                    case "SoTienPhaiTra": col.HeaderText = "Số tiền phải trả"; break;
                    case "SoTienDaTra": col.HeaderText = "Số tiền đã trả"; break;
                    case "NgayDaoHan": col.HeaderText = "Ngày đáo hạn"; break;
                }

                if (col.Name.Contains("Tien") || col.Name.Contains("Gia") || col.Name.Contains("Phi") || col.Name.Contains("Coc"))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }
    }
}
