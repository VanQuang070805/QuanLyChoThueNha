using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.NhanVien
{
    public class frmEmailLog : MaterialForm
    {
        private readonly EmailLogService _service = new EmailLogService();
        private readonly DataGridView _grid = new DataGridView();
        private readonly TextBox _txtSearch = new TextBox();
        private readonly MaterialButton _btnLamMoi = new MaterialButton { Text = "Lam moi", AutoSize = true };

        public frmEmailLog()
        {
            Text = "Lich su email";
            Size = new Size(1080, 620);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            Load += delegate { TaiDuLieu(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                Padding = new Padding(16, 76, 16, 16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            _txtSearch.Dock = DockStyle.Fill;
            _txtSearch.TextChanged += delegate { TaiDuLieu(); };
            toolbar.Controls.Add(_txtSearch, 0, 0);
            _btnLamMoi.Dock = DockStyle.Fill;
            _btnLamMoi.Click += delegate { TaiDuLieu(); };
            toolbar.Controls.Add(_btnLamMoi, 1, 0);
            root.Controls.Add(toolbar, 0, 0);

            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.ReadOnly = true;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.BackgroundColor = Color.White;
            root.Controls.Add(_grid, 0, 1);
            Controls.Add(root);
        }

        private void TaiDuLieu()
        {
            var keyword = (_txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            var data = _service.LayTatCa();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(x =>
                    (x.EmailNguoiNhan ?? string.Empty).ToLowerInvariant().Contains(keyword) ||
                    (x.MaPhieuDatTruoc ?? string.Empty).ToLowerInvariant().Contains(keyword) ||
                    (x.TrangThai ?? string.Empty).ToLowerInvariant().Contains(keyword));
            }

            _grid.DataSource = new BindingList<EmailLog>(data.ToList());
        }
    }
}
