using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.NhanVien
{
    public class frmEmailLog : MaterialForm
    {
        private readonly EmailLogService _emailService = new EmailLogService();
        private readonly TaiKhoanService _taiKhoanService = new TaiKhoanService();
        private readonly KhachThueService _khachThueService = new KhachThueService();

        private readonly DataGridView _gridEmail = new DataGridView();
        private readonly DataGridView _gridTaiKhoan = new DataGridView();

        private readonly PlaceholderTextBox _txtSearchEmail = new PlaceholderTextBox();
        private readonly PlaceholderTextBox _txtSearchTK = new PlaceholderTextBox();

        private TabControl _tabs;

        public frmEmailLog()
        {
            Text = "Lịch sử Email & Tài khoản";
            Size = new Size(1160, 700);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            Load += delegate { TaiDuLieuEmail(); TaiDuLieuTaiKhoan(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                Padding = new Padding(16, 16, 16, 16),
                BackColor = Color.FromArgb(248, 249, 252)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Header row
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

            var title = new Label
            {
                Text = "📋  Lịch sử Email & Tài khoản",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(title, 0, 0);

            var btnLamMoi = new RoundedButton
            {
                Text = "🔄  Làm mới",
                Dock = DockStyle.Fill,
                Height = 38,
                Radius = 8,
                BackColor = Color.FromArgb(37, 99, 235),
                BorderColor = Color.FromArgb(29, 78, 216),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnLamMoi.Click += delegate { TaiDuLieuEmail(); TaiDuLieuTaiKhoan(); };
            header.Controls.Add(btnLamMoi, 1, 0);
            root.Controls.Add(header, 0, 0);

            // Tabs
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            // --- Tab 1: Email Log ---
            var tabEmail = new TabPage("📧  Lịch sử gửi Email") { BackColor = Color.White, Padding = new Padding(8) };
            var emailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                BackColor = Color.White
            };
            emailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            emailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            emailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var emailSearchRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.White
            };
            emailSearchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            emailSearchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

            _txtSearchEmail.Placeholder = "Tìm kiếm email, mã phiếu, loại email, trạng thái...";
            _txtSearchEmail.Dock = DockStyle.Fill;
            _txtSearchEmail.Font = new Font("Segoe UI", 10F);
            _txtSearchEmail.BorderStyle = BorderStyle.FixedSingle;
            _txtSearchEmail.TextChanged += delegate { TaiDuLieuEmail(); };

            var btnExport = new RoundedButton
            {
                Text = "📊  Xuất Excel",
                Dock = DockStyle.Fill,
                Radius = 8,
                BackColor = Color.FromArgb(16, 185, 129),
                BorderColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(8, 0, 0, 0)
            };
            btnExport.Click += delegate { ExportEmailToCSV(); };

            emailSearchRow.Controls.Add(_txtSearchEmail, 0, 0);
            emailSearchRow.Controls.Add(btnExport, 1, 0);
            emailLayout.Controls.Add(emailSearchRow, 0, 0);

            var statBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(239, 246, 255), Padding = new Padding(8, 4, 8, 4) };
            var statLabel = new Label
            {
                Name = "lblEmailStat",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(37, 99, 235),
                TextAlign = ContentAlignment.MiddleLeft
            };
            statBar.Controls.Add(statLabel);
            emailLayout.Controls.Add(statBar, 0, 1);

            ConfigureGrid(_gridEmail);
            _gridEmail.CellFormatting += GridEmail_CellFormatting;
            emailLayout.Controls.Add(_gridEmail, 0, 2);
            tabEmail.Controls.Add(emailLayout);

            // --- Tab 2: Tài khoản khách đã đăng ký ---
            var tabTK = new TabPage("👤  Lịch sử đăng ký tài khoản khách") { BackColor = Color.White, Padding = new Padding(8) };
            var tkLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                BackColor = Color.White
            };
            tkLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            tkLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tkLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var tkSearchRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.White
            };
            tkSearchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tkSearchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

            _txtSearchTK.Placeholder = "Tìm kiếm tên đăng nhập, email, vai trò, họ tên khách...";
            _txtSearchTK.Dock = DockStyle.Fill;
            _txtSearchTK.Font = new Font("Segoe UI", 10F);
            _txtSearchTK.BorderStyle = BorderStyle.FixedSingle;
            _txtSearchTK.TextChanged += delegate { TaiDuLieuTaiKhoan(); };

            var btnExportTK = new RoundedButton
            {
                Text = "📊  Xuất Excel",
                Dock = DockStyle.Fill,
                Radius = 8,
                BackColor = Color.FromArgb(16, 185, 129),
                BorderColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(8, 0, 0, 0)
            };
            btnExportTK.Click += delegate { ExportTKToCSV(); };

            tkSearchRow.Controls.Add(_txtSearchTK, 0, 0);
            tkSearchRow.Controls.Add(btnExportTK, 1, 0);
            tkLayout.Controls.Add(tkSearchRow, 0, 0);

            var tkStatBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 253, 244), Padding = new Padding(8, 4, 8, 4) };
            var tkStatLabel = new Label
            {
                Name = "lblTKStat",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(21, 128, 61),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tkStatBar.Controls.Add(tkStatLabel);
            tkLayout.Controls.Add(tkStatBar, 0, 1);

            ConfigureGrid(_gridTaiKhoan);
            tkLayout.Controls.Add(_gridTaiKhoan, 0, 2);
            tabTK.Controls.Add(tkLayout);

            _tabs.TabPages.Add(tabEmail);
            _tabs.TabPages.Add(tabTK);
            root.Controls.Add(_tabs, 0, 1);
            Controls.Add(root);
        }

        private void ConfigureGrid(DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 36;
            grid.RowTemplate.Height = 30;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);
            grid.GridColor = Color.FromArgb(226, 232, 240);
        }

        private void GridEmail_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _gridEmail.Rows.Count) return;
            var row = _gridEmail.Rows[e.RowIndex];
            var trangThai = row.Cells["TrangThai"]?.Value?.ToString() ?? "";
            if (trangThai == "ThanhCong")
                row.DefaultCellStyle.ForeColor = Color.FromArgb(21, 128, 61);
            else if (trangThai == "ThatBai")
                row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
        }

        private void TaiDuLieuEmail()
        {
            var keyword = TextFormatHelper.NormalizeSearch(_txtSearchEmail.Text);
            var data = _emailService.LayTatCa();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(x =>
                    TextFormatHelper.ContainsNormalized(x.EmailNguoiNhan, keyword) ||
                    TextFormatHelper.ContainsNormalized(x.MaPhieuDatTruoc, keyword) ||
                    TextFormatHelper.ContainsNormalized(x.TrangThai, keyword) ||
                    TextFormatHelper.ContainsNormalized(x.LoaiEmail, keyword) ||
                    TextFormatHelper.ContainsNormalized(x.TieuDe, keyword) ||
                    TextFormatHelper.ContainsNormalized(x.MaTaiKhoan, keyword));
            }

            var list = data.ToList();
            _gridEmail.DataSource = new BindingList<EmailLog>(list);
            FormatEmailGrid();

            // Update stat label
            var statLabel = FindControl(_gridEmail.Parent, "lblEmailStat") as Label;
            if (statLabel != null)
            {
                int total = list.Count;
                int success = list.Count(x => x.TrangThai == "ThanhCong");
                int fail = list.Count(x => x.TrangThai == "ThatBai");
                statLabel.Text = string.Format(
                    "Tổng: {0} bản ghi  |  ✅ Thành công: {1}  |  ❌ Thất bại: {2}",
                    total, success, fail);
            }
        }

        private void FormatEmailGrid()
        {
            foreach (DataGridViewColumn col in _gridEmail.Columns)
            {
                switch (col.Name)
                {
                    case "MaEmailLog":           col.HeaderText = "Mã Email";            col.FillWeight = 60;  break;
                    case "MaPhieuDatTruoc":      col.HeaderText = "Mã phiếu đặt";        col.FillWeight = 80;  break;
                    case "MaTaiKhoan":           col.HeaderText = "Mã tài khoản";        col.FillWeight = 80;  break;
                    case "EmailNguoiNhan":       col.HeaderText = "Email người nhận";     col.FillWeight = 140; break;
                    case "LoaiEmail":            col.HeaderText = "Loại email";           col.FillWeight = 90;  break;
                    case "TieuDe":               col.HeaderText = "Tiêu đề";             col.FillWeight = 160; break;
                    case "TrangThai":            col.HeaderText = "Trạng thái";          col.FillWeight = 70;  break;
                    case "ThongBao":             col.HeaderText = "Thông báo";           col.FillWeight = 180; break;
                    case "NgayGui":
                        col.HeaderText = "Ngày gửi";
                        col.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        col.FillWeight = 90;
                        break;
                    case "MaNguoiThaoTac":       col.HeaderText = "Người thực hiện";     col.FillWeight = 70;  break;
                    case "VaiTroNguoiThaoTac":   col.HeaderText = "Vai trò";             col.FillWeight = 60;  break;
                }
            }
        }

        private void TaiDuLieuTaiKhoan()
        {
            var keyword = TextFormatHelper.NormalizeSearch(_txtSearchTK.Text);

            // Build joined view of TaiKhoan + KhachThue
            var taiKhoans = _taiKhoanService.LayTatCa().OrderByDescending(t => t.NgayTao).ToList();
            var khachMap = _khachThueService.LayTatCa().ToDictionary(k => k.MaTaiKhoan ?? "", k => k);

            var rows = taiKhoans.Select(tk =>
            {
                KhachThue kh;
                khachMap.TryGetValue(tk.MaTaiKhoan ?? "", out kh);
                return new TaiKhoanRow
                {
                    MaTaiKhoan     = tk.MaTaiKhoan,
                    TenDangNhap    = tk.TenDangNhap,
                    Email          = tk.Email,
                    SoDienThoai    = tk.SoDienThoai,
                    VaiTro         = tk.VaiTro,
                    TrangThai      = tk.TrangThai ? "Hoạt động" : "Bị khóa",
                    NgayTao        = tk.NgayTao,
                    HoTenKhach     = kh != null ? kh.HoTen : "(Không phải khách thuê)",
                    MaKhach        = kh != null ? kh.MaKhach : ""
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    TextFormatHelper.ContainsNormalized(r.TenDangNhap, keyword) ||
                    TextFormatHelper.ContainsNormalized(r.Email, keyword) ||
                    TextFormatHelper.ContainsNormalized(r.VaiTro, keyword) ||
                    TextFormatHelper.ContainsNormalized(r.HoTenKhach, keyword) ||
                    TextFormatHelper.ContainsNormalized(r.MaTaiKhoan, keyword) ||
                    TextFormatHelper.ContainsNormalized(r.MaKhach, keyword)
                ).ToList();
            }

            _gridTaiKhoan.DataSource = new BindingList<TaiKhoanRow>(rows);
            FormatTKGrid();

            var statLabel = FindControl(_gridTaiKhoan.Parent, "lblTKStat") as Label;
            if (statLabel != null)
            {
                int total = rows.Count;
                int khach = rows.Count(r => r.VaiTro == "KhachThue");
                int active = rows.Count(r => r.TrangThai == "Hoạt động");
                statLabel.Text = string.Format(
                    "Tổng: {0} tài khoản  |  👤 Khách thuê: {1}  |  ✅ Đang hoạt động: {2}",
                    total, khach, active);
            }
        }

        private void FormatTKGrid()
        {
            foreach (DataGridViewColumn col in _gridTaiKhoan.Columns)
            {
                switch (col.Name)
                {
                    case "MaTaiKhoan":   col.HeaderText = "Mã tài khoản";   col.FillWeight = 80;  break;
                    case "TenDangNhap":  col.HeaderText = "Tên đăng nhập";   col.FillWeight = 120; break;
                    case "HoTenKhach":   col.HeaderText = "Họ tên khách";    col.FillWeight = 140; break;
                    case "MaKhach":      col.HeaderText = "Mã khách";        col.FillWeight = 70;  break;
                    case "Email":        col.HeaderText = "Email";            col.FillWeight = 160; break;
                    case "SoDienThoai":  col.HeaderText = "Số điện thoại";   col.FillWeight = 90;  break;
                    case "VaiTro":       col.HeaderText = "Vai trò";         col.FillWeight = 80;  break;
                    case "TrangThai":    col.HeaderText = "Trạng thái";      col.FillWeight = 80;  break;
                    case "NgayTao":
                        col.HeaderText = "Ngày đăng ký";
                        col.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        col.FillWeight = 100;
                        break;
                }
            }

            // Color-code status column
            foreach (DataGridViewRow row in _gridTaiKhoan.Rows)
            {
                var status = row.Cells["TrangThai"]?.Value?.ToString() ?? "";
                if (status == "Bị khóa")
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                else
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(21, 128, 61);
            }
        }

        private void ExportEmailToCSV()
        {
            using (var dlg = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "EmailLog_" + DateTime.Now.ToString("yyyyMMdd") + ".csv" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var lines = new System.Collections.Generic.List<string>
                    {
                        "Mã Email,Mã phiếu đặt,Mã tài khoản,Email người nhận,Loại email,Tiêu đề,Trạng thái,Ngày gửi"
                    };
                    foreach (var log in _emailService.LayTatCa())
                    {
                        lines.Add(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"",
                            log.MaEmailLog, log.MaPhieuDatTruoc, log.MaTaiKhoan, log.EmailNguoiNhan,
                            log.LoaiEmail, log.TieuDe, log.TrangThai, log.NgayGui.ToString("dd/MM/yyyy HH:mm")));
                    }
                    System.IO.File.WriteAllLines(dlg.FileName, lines, System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất CSV thành công:\n" + dlg.FileName, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportTKToCSV()
        {
            using (var dlg = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "TaiKhoan_" + DateTime.Now.ToString("yyyyMMdd") + ".csv" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var all = _taiKhoanService.LayTatCa().OrderByDescending(t => t.NgayTao).ToList();
                    var khachMap = _khachThueService.LayTatCa().ToDictionary(k => k.MaTaiKhoan ?? "", k => k);
                    var lines = new List<string>
                    {
                        "Mã tài khoản,Tên đăng nhập,Họ tên,Email,Số điện thoại,Vai trò,Trạng thái,Ngày đăng ký"
                    };
                    foreach (var tk in all)
                    {
                        KhachThue kh;
                        khachMap.TryGetValue(tk.MaTaiKhoan ?? "", out kh);
                        var hoTen = kh != null ? kh.HoTen : "";
                        lines.Add(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"",
                            tk.MaTaiKhoan, tk.TenDangNhap, hoTen, tk.Email, tk.SoDienThoai,
                            tk.VaiTro, tk.TrangThai ? "Hoạt động" : "Bị khóa",
                            tk.NgayTao.ToString("dd/MM/yyyy HH:mm")));
                    }
                    System.IO.File.WriteAllLines(dlg.FileName, lines, System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất CSV thành công:\n" + dlg.FileName, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Recursively find a named control inside a parent container.</summary>
        private static Control FindControl(Control parent, string name)
        {
            if (parent == null) return null;
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name) return c;
                var found = FindControl(c, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>View-model for joined TaiKhoan + KhachThue display in grid.</summary>
        private class TaiKhoanRow
        {
            public string MaTaiKhoan   { get; set; }
            public string TenDangNhap  { get; set; }
            public string HoTenKhach   { get; set; }
            public string MaKhach      { get; set; }
            public string Email         { get; set; }
            public string SoDienThoai  { get; set; }
            public string VaiTro        { get; set; }
            public string TrangThai     { get; set; }
            public DateTime NgayTao    { get; set; }
        }
    }
}
