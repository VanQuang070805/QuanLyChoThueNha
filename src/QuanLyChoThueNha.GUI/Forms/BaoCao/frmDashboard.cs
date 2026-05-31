using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;

namespace QuanLyChoThueNha.GUI.Forms.BaoCao
{
    public class frmDashboard : MaterialForm
    {
        private readonly BaoCaoService _service = new BaoCaoService();
        private readonly FlowLayoutPanel _kpiPanel = new FlowLayoutPanel();
        private readonly DataGridView _gridHopDong = new DataGridView();
        private readonly DataGridView _gridDoanhThu = new DataGridView();

        public frmDashboard()
        {
            Text = "Bảng điều khiển";
            Size = new Size(1200, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BuildLayout();
            Load += delegate { LoadData(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(18, 18, 18, 18),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            _kpiPanel.Dock = DockStyle.Fill;
            _kpiPanel.WrapContents = true;
            _kpiPanel.AutoScroll = true;
            _kpiPanel.BackColor = Color.FromArgb(248, 250, 252);
            root.Controls.Add(_kpiPanel, 0, 0);

            root.Controls.Add(CreateGroup("Hợp đồng sắp hết hạn", _gridHopDong), 0, 1);
            root.Controls.Add(CreateGroup("Doanh thu 12 tháng", _gridDoanhThu), 0, 2);
            Controls.Add(root);
            Resize += delegate { ResizeKpis(); };
        }

        private Control CreateGroup(string title, DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            var panel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 14,
                BorderColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.White,
                Margin = new Padding(0, 8, 0, 8),
                Padding = new Padding(14)
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.White };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private void LoadData()
        {
            try
            {
                var now = DateTime.Today;
                _kpiPanel.Controls.Clear();
                AddKpi("Tổng căn hộ", _service.TongCanHo().ToString("N0"), Color.FromArgb(37, 99, 235));
                AddKpi("Đang thuê", _service.CanHoDangThue().ToString("N0"), Color.FromArgb(22, 163, 74));
                AddKpi("Doanh thu tháng", _service.DoanhThuThang(now.Month, now.Year).ToString("N0"), Color.FromArgb(245, 124, 0));
                AddKpi("Hóa đơn chưa trả", _service.HoaDonChuaTra().ToString("N0"), Color.FromArgb(220, 38, 38));

                _gridHopDong.DataSource = _service.HopDongSapHetHan(30).ToList();
                _gridDoanhThu.DataSource = _service.DoanhThuTheoThang(now.Year).ToList();
                FormatGrids();
                ResizeKpis();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được dữ liệu bảng điều khiển: " + ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FormatGrids()
        {
            Rename(_gridHopDong, "MaHopDong", "Mã HĐ");
            Rename(_gridHopDong, "MaCanHo", "Căn hộ");
            Rename(_gridHopDong, "MaKhach", "Khách");
            Rename(_gridHopDong, "NgayKetThuc", "Ngày kết thúc");
            Rename(_gridHopDong, "TrangThai", "Trạng thái");
            HideExcept(_gridHopDong, "MaHopDong", "MaCanHo", "MaKhach", "NgayKetThuc", "TrangThai");

            Rename(_gridDoanhThu, "Thang", "Tháng");
            Rename(_gridDoanhThu, "TongThu", "Tổng thu");
            Rename(_gridDoanhThu, "SoHoaDon", "Số hóa đơn");
        }

        private static void Rename(DataGridView grid, string name, string header)
        {
            if (grid.Columns.Contains(name)) grid.Columns[name].HeaderText = header;
        }

        private static void HideExcept(DataGridView grid, params string[] names)
        {
            foreach (DataGridViewColumn column in grid.Columns)
                column.Visible = names.Contains(column.Name);
        }

        private void AddKpi(string title, string value, Color color)
        {
            var panel = new RoundedPanel
            {
                Width = 240,
                Height = 92,
                Margin = new Padding(0, 0, 12, 0),
                BackColor = color,
                Radius = 14,
                BorderColor = color
            };
            panel.Controls.Add(new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(14, 12),
                AutoSize = true
            });
            panel.Controls.Add(new Label
            {
                Text = value,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(14, 42),
                AutoSize = true
            });
            _kpiPanel.Controls.Add(panel);
        }

        private void ResizeKpis()
        {
            var width = Math.Max(220, (_kpiPanel.ClientSize.Width - 48) / 4);
            if (_kpiPanel.ClientSize.Width < 900) width = Math.Max(240, (_kpiPanel.ClientSize.Width - 36) / 2);
            foreach (Control control in _kpiPanel.Controls)
                control.Width = width;
        }
    }
}
