using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.Auth
{
    public class frmPhanQuyenNhanVien : MaterialForm
    {
        private readonly NhanVienPhanQuyenService _service = new NhanVienPhanQuyenService();
        private readonly ListBox _lstNhanVien = new ListBox();
        private readonly FlowLayoutPanel _permissionPanel = new FlowLayoutPanel();
        private readonly Label _lblNhanVien = new Label();
        private readonly Dictionary<string, CheckBox> _checks = new Dictionary<string, CheckBox>();

        public frmPhanQuyenNhanVien()
        {
            Text = "Phân quyền nhân viên";
            Size = new Size(1180, 760);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            Load += delegate { LoadNhanVien(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(18, 18, 18, 18),
                BackColor = Color.FromArgb(248, 249, 252)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var left = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14),
                BorderStyle = BorderStyle.FixedSingle
            };
            var leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            leftLayout.Controls.Add(new Label
            {
                Text = "Danh sách nhân viên",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            _lstNhanVien.Dock = DockStyle.Fill;
            _lstNhanVien.Font = new Font("Segoe UI", 10F);
            _lstNhanVien.DisplayMember = "HoTen";
            _lstNhanVien.ValueMember = "MaNhanVien";
            _lstNhanVien.SelectedIndexChanged += delegate { LoadQuyenDangChon(); };
            leftLayout.Controls.Add(_lstNhanVien, 0, 1);
            left.Controls.Add(leftLayout);
            root.Controls.Add(left, 0, 0);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                BackColor = Color.FromArgb(248, 249, 252),
                Padding = new Padding(16, 0, 0, 0)
            };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 42, 69), Padding = new Padding(18, 10, 18, 10) };
            _lblNhanVien.Text = "Chọn nhân viên để phân quyền";
            _lblNhanVien.Dock = DockStyle.Fill;
            _lblNhanVien.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            _lblNhanVien.ForeColor = Color.White;
            _lblNhanVien.TextAlign = ContentAlignment.MiddleLeft;
            header.Controls.Add(_lblNhanVien);
            right.Controls.Add(header, 0, 0);

            _permissionPanel.Dock = DockStyle.Fill;
            _permissionPanel.AutoScroll = true;
            _permissionPanel.WrapContents = true;
            _permissionPanel.BackColor = Color.FromArgb(248, 249, 252);
            right.Controls.Add(_permissionPanel, 0, 1);

            var commands = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnLuu = new RoundedButton
            {
                Text = "Lưu phân quyền",
                Width = 150,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(16, 185, 129),
                BorderColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White
            };
            btnLuu.Click += BtnLuu_Click;
            var btnMacDinh = new RoundedButton
            {
                Text = "Khôi phục mặc định",
                Width = 170,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(107, 114, 128),
                BorderColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White
            };
            btnMacDinh.Click += delegate { GanMacDinh(); };
            commands.Controls.Add(btnLuu);
            commands.Controls.Add(btnMacDinh);
            right.Controls.Add(commands, 0, 2);

            root.Controls.Add(right, 1, 0);
            Controls.Add(root);
        }

        private void LoadNhanVien()
        {
            var data = _service.LayNhanVien().ToList();
            _lstNhanVien.DataSource = data;
            if (data.Count > 0) _lstNhanVien.SelectedIndex = 0;
        }

        private void LoadQuyenDangChon()
        {
            var nv = _lstNhanVien.SelectedItem as NhanVienQuanLy;
            _permissionPanel.Controls.Clear();
            _checks.Clear();
            if (nv == null)
            {
                _lblNhanVien.Text = "Chọn nhân viên để phân quyền";
                return;
            }

            _lblNhanVien.Text = string.Format("{0} [{1}]", nv.HoTen, nv.MaNhanVien);
            var quyen = _service.LayQuyen(nv.MaNhanVien);
            foreach (var item in NhanVienPhanQuyenService.DanhSachChucNang())
            {
                var check = new CheckBox
                {
                    Text = item.TenChucNang,
                    Checked = quyen.ContainsKey(item.MaChucNang) && quyen[item.MaChucNang],
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(14, 12)
                };
                _checks[item.MaChucNang] = check;
                _permissionPanel.Controls.Add(CreatePermissionCard(item, check));
            }
        }

        private Control CreatePermissionCard(ChucNangNhanVienDto item, CheckBox check)
        {
            var card = new Panel
            {
                Width = 360,
                Height = 138,
                Margin = new Padding(0, 0, 14, 14),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            card.Controls.Add(new Panel { BackColor = Color.FromArgb(4, 86, 197), Dock = DockStyle.Left, Width = 5 });
            card.Controls.Add(check);
            card.Controls.Add(new Label
            {
                Text = item.MoTa,
                Location = new Point(18, 46),
                Size = new Size(320, 60),
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9F),
                AutoEllipsis = true
            });
            card.Controls.Add(new Label
            {
                Text = item.MacDinh ? "Mặc định: bật" : "Mặc định: tắt",
                Location = new Point(18, 106),
                AutoSize = true,
                ForeColor = item.MacDinh ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            });
            return card;
        }

        private void GanMacDinh()
        {
            foreach (var item in NhanVienPhanQuyenService.DanhSachChucNang())
                if (_checks.ContainsKey(item.MaChucNang))
                    _checks[item.MaChucNang].Checked = item.MacDinh;
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            var nv = _lstNhanVien.SelectedItem as NhanVienQuanLy;
            if (nv == null)
            {
                MessageBox.Show("Chọn nhân viên cần phân quyền.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var data = _checks.ToDictionary(x => x.Key, x => x.Value.Checked);
            string loi;
            if (!_service.LuuQuyen(nv.MaNhanVien, data, out loi))
            {
                MessageBox.Show(loi, "Không lưu được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Đã cập nhật quyền cho nhân viên.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
