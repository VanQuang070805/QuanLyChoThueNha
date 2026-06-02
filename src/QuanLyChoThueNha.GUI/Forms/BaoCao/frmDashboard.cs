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
        private readonly TableLayoutPanel _kpiPanel = new TableLayoutPanel();
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
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            _kpiPanel.Dock = DockStyle.Fill;
            _kpiPanel.RowCount = 1;
            _kpiPanel.ColumnCount = 6;
            _kpiPanel.Margin = new Padding(0, 4, 0, 16);
            _kpiPanel.Padding = new Padding(0);
            _kpiPanel.BackColor = Color.FromArgb(248, 250, 252);
            for (int i = 0; i < 6; i++)
                _kpiPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6f));
            root.Controls.Add(_kpiPanel, 0, 0);

            root.Controls.Add(CreateGroup("Hợp đồng sắp hết hạn", _gridHopDong), 0, 1);
            root.Controls.Add(CreateGroup("Doanh thu 12 tháng", _gridDoanhThu), 0, 2);
            Controls.Add(root);
        }

        private Control CreateGroup(string title, DataGridView grid)
        {
            ConfigureGrid(grid);

            var panel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 16,
                BorderColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.White,
                Margin = new Padding(0, 8, 0, 10),
                Padding = new Padding(16)
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.White };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private static void ConfigureGrid(DataGridView grid)
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
            grid.GridColor = Color.FromArgb(226, 232, 240);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private void LoadData()
        {
            try
            {
                var now = DateTime.Today;
                _kpiPanel.Controls.Clear();
                AddKpi("Tổng căn hộ", _service.TongCanHo().ToString("N0"), Color.FromArgb(37, 99, 235));
                AddKpi("Đang thuê", _service.CanHoDangThue().ToString("N0"), Color.FromArgb(22, 163, 74));
                AddKpi("Tổng doanh thu", _service.TongDoanhThu().ToString("N0"), Color.FromArgb(14, 116, 144));
                AddKpi("Số hóa đơn", _service.TongHoaDon().ToString("N0"), Color.FromArgb(124, 58, 237));
                AddKpi("Doanh thu tháng", _service.DoanhThuThang(now.Month, now.Year).ToString("N0"), Color.FromArgb(245, 124, 0));
                AddKpi("Hóa đơn chưa trả", _service.HoaDonChuaTra().ToString("N0"), Color.FromArgb(220, 38, 38));

                _gridHopDong.DataSource = _service.HopDongSapHetHan(30).ToList();
                _gridDoanhThu.DataSource = _service.DoanhThuTheoThang(now.Year).ToList();
                FormatGrids();
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
            int index = _kpiPanel.Controls.Count;
            int left = index == 0 ? 0 : 6;
            int right = index == 5 ? 0 : 6;

            var panel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(left, 0, right, 0),
                BackColor = color,
                Radius = 12,
                BorderColor = color,
                Padding = new Padding(12, 10, 12, 10)
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                BackColor = color
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            }, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = value,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            }, 0, 1);
            panel.Controls.Add(layout);
            _kpiPanel.Controls.Add(panel, index, 0);
        }
    }
}
