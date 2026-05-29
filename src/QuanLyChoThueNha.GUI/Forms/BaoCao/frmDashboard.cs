using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;

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
            Text = "Dashboard";
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
                Padding = new Padding(16, 76, 16, 16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            _kpiPanel.Dock = DockStyle.Fill;
            _kpiPanel.WrapContents = false;
            root.Controls.Add(_kpiPanel, 0, 0);

            root.Controls.Add(CreateGroup("Hop dong sap het han", _gridHopDong), 0, 1);
            root.Controls.Add(CreateGroup("Doanh thu 12 thang", _gridDoanhThu), 0, 2);
            Controls.Add(root);
        }

        private GroupBox CreateGroup(string title, DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            return new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8), Controls = { grid } };
        }

        private void LoadData()
        {
            try
            {
                var now = DateTime.Today;
                _kpiPanel.Controls.Clear();
                AddKpi("Tong can ho", _service.TongCanHo().ToString("N0"), Color.FromArgb(25, 118, 210));
                AddKpi("Dang thue", _service.CanHoDangThue().ToString("N0"), Color.FromArgb(46, 125, 50));
                AddKpi("Doanh thu thang", _service.DoanhThuThang(now.Month, now.Year).ToString("N0"), Color.FromArgb(245, 124, 0));
                AddKpi("Hoa don chua tra", _service.HoaDonChuaTra().ToString("N0"), Color.FromArgb(198, 40, 40));

                _gridHopDong.DataSource = _service.HopDongSapHetHan(30).ToList();
                _gridDoanhThu.DataSource = _service.DoanhThuTheoThang(now.Year).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong tai du lieu dashboard: " + ex.Message, "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddKpi(string title, string value, Color color)
        {
            var panel = new Panel
            {
                Width = 240,
                Height = 92,
                Margin = new Padding(0, 0, 12, 0),
                BackColor = color
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
    }
}
