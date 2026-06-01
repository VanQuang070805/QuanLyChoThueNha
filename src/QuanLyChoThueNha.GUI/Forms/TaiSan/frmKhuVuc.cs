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
        private MaterialButton btnThem;
        private MaterialButton btnSua;
        private MaterialButton btnXoa;
        private MaterialButton btnLamMoi;
        private MaterialButton btnLayToaDo;

        public frmKhuVuc()
        {
            Text = "Quan ly Khu vuc";
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
                Padding = new Padding(12, 76, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
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

            txtMa = CreateTextBox("(tu sinh)", true);
            txtTen = CreateTextBox("Nhap ten khu vuc", false);
            cboThanhPho = CreateComboBox();
            cboQuan = CreateComboBox();
            numViDo = CreateCoordinateNumber(-90, 90);
            numKinhDo = CreateCoordinateNumber(-180, 180);
            cboThanhPho.SelectedIndexChanged += delegate { NapQuanTheoThanhPho(); };

            AddField(right, "Ma khu vuc", txtMa);
            AddField(right, "Ten khu vuc *", txtTen);
            AddField(right, "Thanh pho *", cboThanhPho);
            AddField(right, "Quan/Huyen *", cboQuan);
            AddField(right, "Vi do (lat)", numViDo);
            AddField(right, "Kinh do (lng)", numKinhDo);

            var commands = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Margin = new Padding(0, 12, 0, 0) };
            btnThem = CreateButton("Them", btnThem_Click);
            btnSua = CreateButton("Sua", btnSua_Click);
            btnXoa = CreateButton("Xoa", btnXoa_Click);
            btnLamMoi = CreateButton("Lam moi", delegate { TaiDuLieu(); XoaTrong(); });
            btnLayToaDo = CreateButton("Lay toa do tu ban do", btnLayToaDo_Click);
            commands.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLayToaDo, btnLamMoi });
            right.Controls.Add(commands);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);
            Controls.Add(root);
        }

        private MaterialTextBox CreateTextBox(string hint, bool readOnly)
        {
            return new MaterialTextBox { Dock = DockStyle.Top, Hint = hint, ReadOnly = readOnly };
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
            return new ComboBox { Dock = DockStyle.Top, Height = 30, DropDownStyle = ComboBoxStyle.DropDownList };
        }

        private NumericUpDown CreateCoordinateNumber(decimal min, decimal max)
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Top,
                Height = 30,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = 6,
                Increment = 0.000100M
            };
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

        private MaterialButton CreateButton(string text, EventHandler handler)
        {
            var btn = new MaterialButton { Text = text, AutoSize = true, Margin = new Padding(0, 4, 6, 4) };
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
                loi = "Ten khu vuc khong duoc de trong.";
                _errors.SetError(txtTen, loi);
                return false;
            }
            if (cboThanhPho.SelectedItem == null)
            {
                loi = "Vui long chon thanh pho.";
                _errors.SetError(cboThanhPho, loi);
                return false;
            }
            if (cboQuan.SelectedItem == null)
            {
                loi = "Vui long chon quan/huyen.";
                _errors.SetError(cboQuan, loi);
                return false;
            }
            return true;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            string loi;
            if (!ValidateForm(out loi)) { MessageBox.Show(loi, "Loi nhap lieu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!_svc.Them(txtTen.Text, cboQuan.SelectedItem.ToString(), cboThanhPho.SelectedItem.ToString(),
                    SessionContext.LaAdmin ? SessionContext.MaNguoiDung : null, out loi,
                    (double)numViDo.Value, (double)numKinhDo.Value))
            {
                MessageBox.Show(loi, "Loi nhap lieu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TaiDuLieu();
            XoaTrong();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { MessageBox.Show("Chon khu vuc can sua."); return; }
            string loi;
            if (!ValidateForm(out loi)) { MessageBox.Show(loi, "Loi nhap lieu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var kv = _svc.LayTheoMa(txtMa.Text);
            if (kv == null) return;
            kv.TenKhuVuc = txtTen.Text.Trim();
            kv.Quan = cboQuan.SelectedItem.ToString();
            kv.ThanhPho = cboThanhPho.SelectedItem.ToString();
            kv.ViDo = (double)numViDo.Value;
            kv.KinhDo = (double)numKinhDo.Value;
            _svc.Sua(kv);
            TaiDuLieu();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { MessageBox.Show("Chon khu vuc can xoa."); return; }
            if (MessageBox.Show("Xoa khu vuc dang chon?", "Xac nhan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                string loi;
                if (!_svc.XoaKhuVuc(txtMa.Text, out loi))
                {
                    MessageBox.Show(loi, "Khong the xoa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                TaiDuLieu();
                XoaTrong();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong the xoa vi co du lieu lien quan: " + ex.Message, "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                        MessageBox.Show("Khong tim thay toa do tu dia chi nay.", "Ban do",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    numViDo.Value = ClampCoordinate(double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture),
                        numViDo.Minimum, numViDo.Maximum);
                    numKinhDo.Value = ClampCoordinate(double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture),
                        numKinhDo.Minimum, numKinhDo.Maximum);
                    MessageBox.Show("Da lay toa do tu OpenStreetMap.", "Ban do",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong lay duoc toa do: " + ex.Message, "Ban do",
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
