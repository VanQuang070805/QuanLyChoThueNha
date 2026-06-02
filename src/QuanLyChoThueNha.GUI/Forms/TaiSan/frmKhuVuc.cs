using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TaiSan
{
    public class frmKhuVuc : MaterialForm
    {
        private readonly KhuVucService _svc = new KhuVucService();
        private readonly ErrorProvider _errors = new ErrorProvider();
        private readonly Dictionary<string, string[]> _districtsByCity = new Dictionary<string, string[]>
        {
            { "Ha Noi", new[] { "Ba Dinh", "Hoan Kiem", "Dong Da", "Cau Giay", "Hai Ba Trung", "Thanh Xuan", "Nam Tu Liem", "Ha Dong" } },
            { "TP Ho Chi Minh", new[] { "Quan 1", "Quan 3", "Quan 7", "Binh Thanh", "Tan Binh", "Go Vap", "Thu Duc", "Binh Tan" } },
            { "Da Nang", new[] { "Hai Chau", "Thanh Khe", "Son Tra", "Ngu Hanh Son", "Lien Chieu", "Cam Le" } },
            { "Hai Phong", new[] { "Hong Bang", "Ngo Quyen", "Le Chan", "Kien An", "Hai An", "Duong Kinh" } },
            { "Can Tho", new[] { "Ninh Kieu", "Binh Thuy", "Cai Rang", "O Mon", "Thot Not" } }
        };

        private DataGridView dgv;
        private MaterialTextBox txtMa;
        private MaterialTextBox txtTen;
        private PlaceholderTextBox txtTimKiem;
        private ComboBox cboThanhPho;
        private ComboBox cboQuan;
        private NumericUpDown numViDo;
        private NumericUpDown numKinhDo;
        private RoundedButton btnThem;
        private RoundedButton btnLuu;
        private RoundedButton btnSua;
        private RoundedButton btnXoa;
        private RoundedButton btnHuy;
        private RoundedButton btnLamMoi;
        private RoundedButton btnLayToaDo;

        private enum FormMode { View, Adding, Editing }
        private FormMode _mode = FormMode.View;

        public frmKhuVuc()
        {
            Text = "Quản lý Khu vực";
            Size = new Size(1120, 660);
            StartPosition = FormStartPosition.CenterParent;
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            BuildLayout();
            NapThanhPho();
            TaiDuLieu();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(12, 12, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var searchPanel = CreateSearchPanel("Tìm kiếm", "Tìm theo tên khu vực, quận/huyện, thành phố");
            txtTimKiem.TextChanged += delegate { TimKiem(); };
            left.Controls.Add(searchPanel, 0, 0);

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
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtMa = CreateTextBox(string.Empty, true);
            txtTen = CreateTextBox("Nhập tên khu vực", false);
            cboThanhPho = CreateComboBox();
            cboQuan = CreateComboBox();
            numViDo = CreateCoordinateNumber(-90, 90);
            numKinhDo = CreateCoordinateNumber(-180, 180);
            cboThanhPho.SelectedIndexChanged += delegate { NapQuanTheoThanhPho(); };

            AddField(right, "Mã khu vực", txtMa);
            AddField(right, "Tên khu vực *", txtTen);
            AddField(right, "Thành phố *", cboThanhPho);
            AddField(right, "Quận/Huyện *", cboQuan);
            AddField(right, "Vĩ độ (lat)", numViDo);
            AddField(right, "Kinh độ (lng)", numKinhDo);

            var commands = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Margin = new Padding(0, 12, 0, 0) };
            btnThem     = CreateButton("Thêm",     btnThem_Click);
            btnLuu      = CreateButton("Lưu",      btnLuu_Click);
            btnSua      = CreateButton("Sửa",      btnSua_Click);
            btnXoa      = CreateButton("Xóa",      btnXoa_Click);
            btnHuy      = CreateButton("Hủy",      btnHuy_Click);
            btnLayToaDo = CreateButton("Lấy tọa độ", btnLayToaDo_Click);
            btnLamMoi   = CreateButton("Làm mới",  delegate { TaiDuLieu(); XoaTrong(); EnterViewMode(); });
            commands.Controls.AddRange(new Control[] { btnThem, btnLuu, btnSua, btnXoa, btnHuy, btnLayToaDo, btnLamMoi });
            right.Controls.Add(commands);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);
            Controls.Add(root);

            EnterViewMode();
        }

        private MaterialTextBox CreateTextBox(string hint, bool readOnly)
        {
            var txt = new MaterialTextBox { Dock = DockStyle.Top, Hint = hint, ReadOnly = readOnly };
            txt.Font = new Font("Segoe UI", 10F);
            return txt;
        }

        private Control CreateSearchPanel(string label, string placeholder)
        {
            var panel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                BorderColor = Color.FromArgb(226, 232, 240),
                Padding = new Padding(12, 5, 12, 6),
                BackColor = Color.White
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.White };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            txtTimKiem = new PlaceholderTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10.5F),
                Placeholder = placeholder
            };
            layout.Controls.Add(txtTimKiem, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private ComboBox CreateComboBox()
        {
            var combo = new ComboBox { Dock = DockStyle.Top, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            combo.Font = new Font("Segoe UI", 10F);
            return combo;
        }

        private NumericUpDown CreateCoordinateNumber(decimal min, decimal max)
        {
            var num = new KeyboardOnlyNumericUpDown
            {
                Dock = DockStyle.Top,
                Height = 32,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = 6,
                Increment = 0.000100M
            };
            num.Font = new Font("Segoe UI", 10F);
            return num;
        }

        private void AddField(TableLayoutPanel parent, string label, Control editor)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 8, 0, 2)
            });
            parent.Controls.Add(editor);
        }

        private RoundedButton CreateButton(string text, EventHandler handler)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Width = text.Length > 10 ? 140 : 90,
                Height = 36,
                Radius = 10,
                Margin = new Padding(0, 4, 6, 4)
            };

            if (text == "Thêm")
            {
                btn.BackColor  = Color.FromArgb(37, 99, 235);  // blue-600
                btn.BorderColor = Color.FromArgb(29, 78, 216); // blue-700
                btn.ForeColor  = Color.White;
            }
            else if (text == "Sửa")
            {
                btn.BackColor  = Color.FromArgb(245, 158, 11);  // amber-500
                btn.BorderColor = Color.FromArgb(217, 119, 6);  // amber-600
                btn.ForeColor  = Color.White;
            }
            else if (text == "Xóa")
            {
                btn.BackColor  = Color.FromArgb(239, 68, 68);   // red-500
                btn.BorderColor = Color.FromArgb(220, 38, 38);  // red-600
                btn.ForeColor  = Color.White;
            }
            else if (text == "Lưu")
            {
                btn.BackColor  = Color.FromArgb(16, 185, 129);  // emerald-500
                btn.BorderColor = Color.FromArgb(5, 150, 105);  // emerald-600
                btn.ForeColor  = Color.White;
            }
            else if (text == "Hủy")
            {
                btn.BackColor  = Color.FromArgb(239, 68, 68);   // red-500
                btn.BorderColor = Color.FromArgb(220, 38, 38);  // red-600
                btn.ForeColor  = Color.White;
            }
            else if (text == "Lấy tọa độ")
            {
                btn.BackColor  = Color.FromArgb(6, 182, 212);   // cyan-500
                btn.BorderColor = Color.FromArgb(8, 145, 178);  // cyan-600
                btn.ForeColor  = Color.White;
            }
            else // "Làm mới" và các nút khác
            {
                btn.BackColor  = Color.FromArgb(99, 102, 241);  // indigo-500
                btn.BorderColor = Color.FromArgb(79, 70, 229);  // indigo-600
                btn.ForeColor  = Color.White;
            }

            btn.Click += handler;
            return btn;
        }

        private void NapThanhPho()
        {
            cboThanhPho.Items.Clear();
            foreach (var city in _districtsByCity.Keys.OrderBy(x => x))
                cboThanhPho.Items.Add(city);

            foreach (var city in _svc.LayTatCa().Select(k => k.ThanhPho).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                if (!cboThanhPho.Items.Contains(city)) cboThanhPho.Items.Add(city);

            if (cboThanhPho.Items.Count > 0) cboThanhPho.SelectedIndex = 0;
        }

        private void NapQuanTheoThanhPho()
        {
            var city = cboThanhPho.SelectedItem == null ? string.Empty : cboThanhPho.SelectedItem.ToString();
            cboQuan.Items.Clear();

            string[] districts;
            if (_districtsByCity.TryGetValue(city, out districts))
            {
                cboQuan.Items.AddRange(districts);
            }

            foreach (var district in _svc.LayTatCa()
                .Where(k => k.ThanhPho == city)
                .Select(k => k.Quan)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct())
                if (!cboQuan.Items.Contains(district)) cboQuan.Items.Add(district);

            if (cboQuan.Items.Count > 0) cboQuan.SelectedIndex = 0;
        }

        private void TaiDuLieu()
        {
            dgv.DataSource = new BindingList<KhuVuc>(_svc.LayTatCa().OrderBy(k => k.ThanhPho).ThenBy(k => k.Quan).ToList());
            dgv.ClearSelection();
        }

        private void TimKiem()
        {
            string kw = TextFormatHelper.NormalizeSearch(txtTimKiem.Text);
            var data = _svc.LayTatCa();
            if (!string.IsNullOrEmpty(kw))
            {
                data = data.Where(k =>
                    TextFormatHelper.ContainsNormalized(k.TenKhuVuc, kw) ||
                    TextFormatHelper.ContainsNormalized(k.Quan, kw) ||
                    TextFormatHelper.ContainsNormalized(k.ThanhPho, kw));
            }
            dgv.DataSource = new BindingList<KhuVuc>(data.OrderBy(k => k.ThanhPho).ThenBy(k => k.Quan).ToList());
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (_mode != FormMode.View) return; // không đổi state khi đang nhập
            if (dgv.CurrentRow == null) return;
            var kv = dgv.CurrentRow.DataBoundItem as KhuVuc;
            if (kv == null) return;

            txtMa.Text = kv.MaKhuVuc;
            txtTen.Text = kv.TenKhuVuc;
            ChonGiaTri(cboThanhPho, kv.ThanhPho);
            NapQuanTheoThanhPho();
            ChonGiaTri(cboQuan, kv.Quan);
            numViDo.Value = ClampCoordinate(kv.ViDo, numViDo.Minimum, numViDo.Maximum);
            numKinhDo.Value = ClampCoordinate(kv.KinhDo, numKinhDo.Minimum, numKinhDo.Maximum);
            btnSua.Enabled = true;
            btnXoa.Enabled = true;
        }

        /// <summary>Vào trạng thái View: hiện Thêm/Sửa/Xóa/Làm mới, ẩn Lưu/Hủy.</summary>
        private void EnterViewMode()
        {
            _mode = FormMode.View;
            bool hasRow = !string.IsNullOrWhiteSpace(txtMa.Text);
            SetInputsReadOnly(true);

            btnThem.Visible = true;  btnThem.Enabled = true;
            btnSua.Visible  = true;  btnSua.Enabled  = hasRow;
            btnXoa.Visible  = true;  btnXoa.Enabled  = hasRow;
            btnLamMoi.Visible = true;
            btnLuu.Visible  = false;
            btnHuy.Visible  = false;
        }

        /// <summary>Vào trạng thái Adding: chỉ Lưu và Hủy khả dụng.</summary>
        private void EnterAddMode()
        {
            _mode = FormMode.Adding;
            XoaTrong();
            SetInputsReadOnly(false);

            btnThem.Visible = false;
            btnSua.Visible  = false;
            btnXoa.Visible  = false;
            btnLamMoi.Visible = false;
            btnLuu.Visible  = true;  btnLuu.Enabled  = true;
            btnHuy.Visible  = true;  btnHuy.Enabled  = true;
        }

        /// <summary>Vào trạng thái Editing: chỉ Lưu và Hủy khả dụng.</summary>
        private void EnterEditMode()
        {
            _mode = FormMode.Editing;
            SetInputsReadOnly(false);

            btnThem.Visible = false;
            btnSua.Visible  = false;
            btnXoa.Visible  = false;
            btnLamMoi.Visible = false;
            btnLuu.Visible  = true;  btnLuu.Enabled  = true;
            btnHuy.Visible  = true;  btnHuy.Enabled  = true;
        }

        private void SetInputsReadOnly(bool readOnly)
        {
            txtTen.ReadOnly = readOnly;
            cboThanhPho.Enabled = !readOnly;
            cboQuan.Enabled = !readOnly;
            numViDo.Enabled = !readOnly;
            numKinhDo.Enabled = !readOnly;
            btnLayToaDo.Enabled = !readOnly;
        }

        private void ChonGiaTri(ComboBox combo, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (!combo.Items.Contains(value)) combo.Items.Add(value);
            combo.SelectedItem = value;
        }

        private bool ValidateForm(out string loi)
        {
            _errors.Clear();
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(txtTen.Text))
            {
                loi = "Tên khu vực không được để trống.";
                _errors.SetError(txtTen, loi);
                return false;
            }
            if (cboThanhPho.SelectedItem == null)
            {
                loi = "Vui lòng chọn thành phố.";
                _errors.SetError(cboThanhPho, loi);
                return false;
            }
            if (cboQuan.SelectedItem == null)
            {
                loi = "Vui lòng chọn quận/huyện.";
                _errors.SetError(cboQuan, loi);
                return false;
            }
            return true;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            EnterAddMode();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string loi;
            if (!ValidateForm(out loi)) { MessageBox.Show(loi, "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (_mode == FormMode.Adding)
            {
                if (!_svc.Them(txtTen.Text, cboQuan.SelectedItem.ToString(), cboThanhPho.SelectedItem.ToString(),
                    SessionContext.LaAdmin ? SessionContext.MaNguoiDung : null, out loi,
                    (double)numViDo.Value, (double)numKinhDo.Value))
                {
                    MessageBox.Show(loi, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                TaiDuLieu();
                XoaTrong();
                EnterViewMode();
            }
            else if (_mode == FormMode.Editing)
            {
                var kv = _svc.LayTheoMa(txtMa.Text);
                if (kv == null) return;
                kv.TenKhuVuc = txtTen.Text.Trim();
                kv.Quan = cboQuan.SelectedItem.ToString();
                kv.ThanhPho = cboThanhPho.SelectedItem.ToString();
                kv.ViDo = (double)numViDo.Value;
                kv.KinhDo = (double)numKinhDo.Value;
                _svc.Sua(kv);
                TaiDuLieu();
                EnterViewMode();
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { MessageBox.Show("Chọn khu vực cần sửa."); return; }
            EnterEditMode();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (_mode != FormMode.View) return;
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { MessageBox.Show("Chọn khu vực cần xóa."); return; }
            if (MessageBox.Show("Xóa khu vực đang chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                string loi;
                if (!_svc.XoaKhuVuc(txtMa.Text, out loi))
                {
                    MessageBox.Show(loi, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                TaiDuLieu();
                XoaTrong();
                EnterViewMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa vì có dữ liệu liên quan: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            TaiDuLieu();
            XoaTrong();
            EnterViewMode();
        }

        private void btnLayToaDo_Click(object sender, EventArgs e)
        {
            var address = string.Format("{0}, {1}, {2}, Viet Nam",
                txtTen.Text,
                cboQuan.SelectedItem == null ? string.Empty : cboQuan.SelectedItem.ToString(),
                cboThanhPho.SelectedItem == null ? string.Empty : cboThanhPho.SelectedItem.ToString());

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.UserAgent] = "QuanLyChoThueNha/1.0";
                    var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&q=" +
                        Uri.EscapeDataString(address);
                    var json = client.DownloadString(url);
                    var lat = DocGiaTriJson(json, "lat");
                    var lon = DocGiaTriJson(json, "lon");
                    if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lon))
                    {
                        MessageBox.Show("Không tìm thấy tọa độ từ địa chỉ này.", "Bản đồ",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    numViDo.Value = ClampCoordinate(double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture),
                        numViDo.Minimum, numViDo.Maximum);
                    numKinhDo.Value = ClampCoordinate(double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture),
                        numKinhDo.Minimum, numKinhDo.Maximum);
                    MessageBox.Show("Đã lấy tọa độ từ OpenStreetMap.", "Bản đồ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lấy được tọa độ: " + ex.Message, "Bản đồ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string DocGiaTriJson(string json, string key)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + key + "\"\\s*:\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private void XoaTrong()
        {
            txtMa.Clear();
            txtTen.Clear();
            numViDo.Value = 0;
            numKinhDo.Value = 0;
            if (cboThanhPho.Items.Count > 0) cboThanhPho.SelectedIndex = 0;
            if (cboQuan.Items.Count > 0) cboQuan.SelectedIndex = 0;
            dgv.ClearSelection();
            _errors.Clear();
        }

        private decimal ClampCoordinate(double? value, decimal min, decimal max)
        {
            if (!value.HasValue) return 0;
            var decimalValue = (decimal)value.Value;
            if (decimalValue < min) return min;
            if (decimalValue > max) return max;
            return decimalValue;
        }
    }
}
