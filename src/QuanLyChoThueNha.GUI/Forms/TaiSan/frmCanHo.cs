using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TaiSan
{
    public class frmCanHo : MaterialForm
    {
        private enum FormMode { View, Adding, Editing }

        private readonly CanHoService _canHoSvc = new CanHoService();
        private readonly ToaService _toaSvc = new ToaService();
        private readonly LoaiCanHoService _loaiSvc = new LoaiCanHoService();
        private readonly ErrorProvider _errors = new ErrorProvider();

        private FormMode _mode = FormMode.View;

        private DataGridView dgv;
        private MaterialTextBox txtMa;
        private MaterialTextBox txtSoCanHo;
        private MaterialTextBox txtTimKiem;
        private MaterialTextBox txtMoTa;
        private ComboBox cboToa;
        private ComboBox cboLoai;
        private ComboBox cboTinhTrang;
        private NumericUpDown numDienTich;
        private NumericUpDown numGiaThue;
        private NumericUpDown numTienCoc;
        private NumericUpDown numTang;

        // Button references de doi nhan / trang thai theo mode
        private MaterialButton btnThem;
        private MaterialButton btnSua;
        private MaterialButton btnXoa;
        private MaterialButton btnLamMoi;
        private Label _lblMsg;

        public frmCanHo()
        {
            Text = "Quan ly Can ho";
            Size = new Size(1220, 700);
            StartPosition = FormStartPosition.CenterParent;
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            BuildLayout();
            NapComboBox();
            TaiDuLieu();
            EnterAddMode();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(12, 76, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            txtTimKiem = new MaterialTextBox { Dock = DockStyle.Fill, Hint = "Tim theo ma can ho, so can ho, tinh trang" };
            txtTimKiem.TextChanged += delegate { TimKiem(); };
            left.Controls.Add(txtTimKiem, 0, 0);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgv.SelectionChanged += Dgv_SelectionChanged;
            left.Controls.Add(dgv, 0, 1);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoScroll = true,
                Padding = new Padding(12, 0, 0, 0)
            };

            txtMa = CreateTextBox("(tu sinh)", true);
            cboToa = CreateComboBox();
            cboLoai = CreateComboBox();
            numDienTich = CreateNumber(0, 10000, 2);
            numGiaThue = CreateNumber(0, 1000000000000, 0);
            numTienCoc = CreateNumber(0, 1000000000000, 0);
            numTang = CreateNumber(1, 200, 0);
            txtSoCanHo = CreateTextBox("VD: A101", false);
            cboTinhTrang = CreateComboBox();
            cboTinhTrang.Items.AddRange(new object[] { "Trong", "DaDatCoc", "DangThue", "BaoTri" });
            txtMoTa = CreateTextBox("Ghi chu", false);
            txtMoTa.Multiline = true;
            txtMoTa.Height = 72;

            // Bat dau tat ca cac truong co the sua - se duoc bat khi vao Add/Edit mode
            SetEditorsEnabled(false);

            AddField(right, "Ma can ho", txtMa);
            AddField(right, "Toa nha *", cboToa);
            AddField(right, "Loai can ho *", cboLoai);
            AddField(right, "Dien tich (m2) *", numDienTich);
            AddField(right, "Gia thue niem yet *", numGiaThue);
            AddField(right, "Tien coc niem yet", numTienCoc);
            AddField(right, "Tang so *", numTang);
            AddField(right, "So can ho", txtSoCanHo);
            AddField(right, "Tinh trang", cboTinhTrang);
            AddField(right, "Mo ta", txtMoTa);

            var commands = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            btnThem   = CreateButton("Them",    btnThem_Click);
            btnSua    = CreateButton("Sua",     btnSua_Click);
            btnXoa    = CreateButton("Xoa",     btnXoa_Click);
            btnLamMoi = CreateButton("Lam moi", btnLamMoi_Click);
            commands.Controls.Add(btnThem);
            commands.Controls.Add(btnSua);
            commands.Controls.Add(btnXoa);
            commands.Controls.Add(btnLamMoi);
            right.Controls.Add(commands);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);

            _lblMsg = new Label
            {
                Dock = DockStyle.Bottom, Height = 34, AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                Font = new Font("Segoe UI", 9f), Visible = false
            };
            Controls.Add(root);
            Controls.Add(_lblMsg);
        }

        private void ShowError(string msg)
        {
            _lblMsg.Text = msg; _lblMsg.BackColor = Color.MistyRose;
            _lblMsg.ForeColor = Color.DarkRed; _lblMsg.Visible = true;
        }

        private void HideMsg() { _lblMsg.Visible = false; }

        // ── Mode management ──────────────────────────────────────────────────────

        private void EnterAddMode()
        {
            _mode = FormMode.Adding;
            HideMsg(); XoaTrong(); SetEditorsEnabled(true);
            btnThem.Text  = "Luu";  btnThem.Enabled  = true;
            btnSua.Text   = "Sua";  btnSua.Enabled   = false;
            btnXoa.Enabled = false; btnLamMoi.Text = "Lam moi";
        }

        private void EnterViewMode()
        {
            _mode = FormMode.View;
            HideMsg(); SetEditorsEnabled(false);
            btnThem.Text  = "Them"; btnThem.Enabled  = true;
            btnSua.Text   = "Sua";  btnSua.Enabled   = true;
            btnXoa.Enabled = true;  btnLamMoi.Text = "Lam moi";
        }

        private void EnterEditMode()
        {
            _mode = FormMode.Editing;
            HideMsg(); SetEditorsEnabled(true);
            btnThem.Text  = "Them"; btnThem.Enabled  = false;
            btnSua.Text   = "Luu";  btnSua.Enabled   = true;
            btnXoa.Enabled = false; btnLamMoi.Text = "Huy";
        }

        private void SetEditorsEnabled(bool enabled)
        {
            cboToa.Enabled        = enabled;
            cboLoai.Enabled       = enabled;
            cboTinhTrang.Enabled  = enabled;
            numDienTich.Enabled   = enabled;
            numGiaThue.Enabled    = enabled;
            numTienCoc.Enabled    = enabled;
            numTang.Enabled       = enabled;
            txtSoCanHo.ReadOnly   = !enabled;
            txtMoTa.ReadOnly      = !enabled;
        }

        // ── Helper constructors ──────────────────────────────────────────────────

        private MaterialTextBox CreateTextBox(string hint, bool readOnly)
        {
            return new MaterialTextBox { Dock = DockStyle.Top, Hint = hint, ReadOnly = readOnly };
        }

        private ComboBox CreateComboBox()
        {
            return new ComboBox { Dock = DockStyle.Top, Height = 30, DropDownStyle = ComboBoxStyle.DropDownList };
        }

        private NumericUpDown CreateNumber(decimal min, decimal max, int decimalPlaces)
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Top, Height = 30, Minimum = min, Maximum = max,
                DecimalPlaces = decimalPlaces, ThousandsSeparator = true
            };
        }

        private void AddField(TableLayoutPanel parent, string label, Control editor)
        {
            parent.Controls.Add(new Label
            {
                Text = label, AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 8, 0, 2)
            });
            parent.Controls.Add(editor);
        }

        private MaterialButton CreateButton(string text, EventHandler handler)
        {
            var btn = new MaterialButton { Text = text, AutoSize = true, Margin = new Padding(0, 4, 6, 4) };
            btn.Click += handler;
            return btn;
        }

        // ── Data ─────────────────────────────────────────────────────────────────

        private void NapComboBox()
        {
            cboToa.DisplayMember = "TenToa";
            cboToa.ValueMember   = "MaToa";
            cboToa.DataSource    = _toaSvc.LayTatCa().OrderBy(t => t.TenToa).ToList();

            cboLoai.DisplayMember = "TenLoai";
            cboLoai.ValueMember   = "MaLoai";
            cboLoai.DataSource    = _loaiSvc.LayTatCa().OrderBy(l => l.TenLoai).ToList();

            if (cboTinhTrang.Items.Count > 0) cboTinhTrang.SelectedIndex = 0;
        }

        private void TaiDuLieu()
        {
            dgv.DataSource = new BindingList<CanHo>(_canHoSvc.LayTatCa().OrderBy(c => c.MaCanHo).ToList());
            dgv.ClearSelection();
        }

        private void TimKiem()
        {
            string kw = txtTimKiem.Text.Trim().ToLowerInvariant();
            var data = _canHoSvc.LayTatCa();
            if (!string.IsNullOrEmpty(kw))
            {
                data = data.Where(c =>
                    (c.MaCanHo ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                    (c.SoCanHo.ToString()).ToLowerInvariant().Contains(kw) ||
                    (c.TinhTrang ?? string.Empty).ToLowerInvariant().Contains(kw));
            }
            dgv.DataSource = new BindingList<CanHo>(data.OrderBy(c => c.MaCanHo).ToList());
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var c = dgv.CurrentRow.DataBoundItem as CanHo;
            if (c == null) return;

            // Hien thi du lieu cua dong duoc chon
            BindRowToForm(c);
            // Bat buoc vao View mode khi chon dong
            EnterViewMode();
        }

        private void BindRowToForm(CanHo c)
        {
            txtMa.Text = c.MaCanHo;
            if (cboToa.Items.Count > 0) cboToa.SelectedValue = c.MaToa;
            if (cboLoai.Items.Count > 0) cboLoai.SelectedValue = c.MaLoai;
            numDienTich.Value = Clamp((decimal)c.DienTich, numDienTich.Minimum, numDienTich.Maximum);
            numGiaThue.Value  = Clamp(c.GiaThueNiemYet, numGiaThue.Minimum, numGiaThue.Maximum);
            numTienCoc.Value  = Clamp(c.TienCocNiemYet, numTienCoc.Minimum, numTienCoc.Maximum);
            numTang.Value     = Clamp(c.TangSo, numTang.Minimum, numTang.Maximum);
            txtSoCanHo.Text   = c.SoCanHo.ToString();
            cboTinhTrang.SelectedItem = c.TinhTrang;
            txtMoTa.Text = c.MoTa;
        }

        private decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // ── Validation ───────────────────────────────────────────────────────────

        private bool ValidateForm(out string loi)
        {
            _errors.Clear();
            loi = string.Empty;
            if (cboToa.SelectedValue == null)
            {
                loi = "Vui long chon toa nha.";
                _errors.SetError(cboToa, loi);
                return false;
            }
            if (cboLoai.SelectedValue == null)
            {
                loi = "Vui long chon loai can ho.";
                _errors.SetError(cboLoai, loi);
                return false;
            }
            if (numDienTich.Value <= 0)
            {
                loi = "Dien tich phai lon hon 0.";
                _errors.SetError(numDienTich, loi);
                return false;
            }
            if (numGiaThue.Value <= 0)
            {
                loi = "Gia thue phai lon hon 0.";
                _errors.SetError(numGiaThue, loi);
                return false;
            }
            return true;
        }

        private CanHo DocForm()
        {
            int soCanHo;
            int.TryParse(txtSoCanHo.Text.Trim(), out soCanHo);

            return new CanHo
            {
                MaToa          = cboToa.SelectedValue == null ? null : cboToa.SelectedValue.ToString(),
                MaLoai         = cboLoai.SelectedValue == null ? null : cboLoai.SelectedValue.ToString(),
                MaNhanVien     = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null,
                DienTich       = Convert.ToDouble(numDienTich.Value),
                GiaThueNiemYet = numGiaThue.Value,
                TienCocNiemYet = numTienCoc.Value,
                TangSo         = Convert.ToInt32(numTang.Value),
                SoCanHo        = soCanHo,
                TinhTrang      = cboTinhTrang.SelectedItem == null ? "Trong" : cboTinhTrang.SelectedItem.ToString(),
                MoTa           = txtMoTa.Text.Trim()
            };
        }

        // ── Button handlers ──────────────────────────────────────────────────────

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View || _mode == FormMode.Editing)
            {
                EnterAddMode();
                return;
            }
            // Adding mode -> luu
            string loi;
            if (!ValidateForm(out loi)) { ShowError(loi); return; }
            try
            {
                if (!_canHoSvc.Them(DocForm(), out loi)) { ShowError(loi); return; }
            }
            catch (Exception ex) { ShowError(LayLoiSauCung(ex)); return; }
            TaiDuLieu();
            EnterAddMode();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View)
            {
                if (string.IsNullOrWhiteSpace(txtMa.Text)) { ShowError("Chon can ho can sua."); return; }
                EnterEditMode();
                return;
            }
            if (_mode == FormMode.Editing)
            {
                string loi;
                if (!ValidateForm(out loi)) { ShowError(loi); return; }
                var canHo = _canHoSvc.LayTheoMa(txtMa.Text);
                if (canHo == null) return;
                var upd = DocForm();
                canHo.MaToa         = upd.MaToa;
                canHo.MaLoai        = upd.MaLoai;
                canHo.DienTich      = upd.DienTich;
                canHo.GiaThueNiemYet = upd.GiaThueNiemYet;
                canHo.TienCocNiemYet = upd.TienCocNiemYet;
                canHo.TangSo        = upd.TangSo;
                canHo.SoCanHo       = upd.SoCanHo;
                canHo.TinhTrang     = upd.TinhTrang;
                canHo.MoTa          = upd.MoTa;
                try { _canHoSvc.Sua(canHo); }
                catch (Exception ex) { ShowError(LayLoiSauCung(ex)); return; }
                TaiDuLieu();
                EnterAddMode();
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { ShowError("Chon can ho can xoa."); return; }
            if (MessageBox.Show("Xoa can ho dang chon?", "Xac nhan",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                _canHoSvc.Xoa(_canHoSvc.LayTheoMa(txtMa.Text));
                TaiDuLieu();
                EnterAddMode();
            }
            catch (Exception ex) { ShowError("Khong the xoa vi co du lieu lien quan: " + LayLoiSauCung(ex)); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.Editing)
            {
                // Huy sua: khoi phuc du lieu goc
                if (dgv.CurrentRow != null)
                {
                    var c = dgv.CurrentRow.DataBoundItem as CanHo;
                    if (c != null) { BindRowToForm(c); EnterViewMode(); return; }
                }
            }
            TaiDuLieu();
            EnterAddMode();
        }

        private static string LayLoiSauCung(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }

        private void XoaTrong()
        {
            txtMa.Clear();
            txtSoCanHo.Clear();
            txtMoTa.Clear();
            numDienTich.Value = 0;
            numGiaThue.Value  = 0;
            numTienCoc.Value  = 0;
            numTang.Value     = 1;
            if (cboToa.Items.Count > 0) cboToa.SelectedIndex = 0;
            if (cboLoai.Items.Count > 0) cboLoai.SelectedIndex = 0;
            if (cboTinhTrang.Items.Count > 0) cboTinhTrang.SelectedIndex = 0;
            dgv.ClearSelection();
            _errors.Clear();
        }
    }
}
