using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TaiSan
{
    public class frmCanHo : MaterialForm
    {
        private enum FormMode { View, Adding, Editing }

        private readonly CanHoService _canHoSvc = new CanHoService();
        private readonly ToaService _toaSvc = new ToaService();
        private readonly LoaiCanHoService _loaiSvc = new LoaiCanHoService();
        private readonly TienNghiService _tienNghiSvc = new TienNghiService();
        private readonly ErrorProvider _errors = new ErrorProvider();

        private FormMode _mode = FormMode.View;

        private DataGridView dgv;
        private MaterialTextBox txtMa;
        private MaterialTextBox txtSoCanHo;
        private PlaceholderTextBox txtTimKiem;
        private MaterialTextBox txtMoTa;
        private ComboBox cboToa;
        private ComboBox cboLoai;
        private ComboBox cboTinhTrang;
        private CheckedListBox lstTienNghi;
        private Label lblTinhTrangHienTai;
        private NumericUpDown numDienTich;
        private NumericUpDown numGiaThue;
        private NumericUpDown numTienCoc;
        private NumericUpDown numTang;
        private Label lblTienNghi;
        private PictureBox picAnh;

        // Button references de doi nhan / trang thai theo mode
        private RoundedButton btnThem;
        private RoundedButton btnSua;
        private RoundedButton btnXoa;
        private RoundedButton btnLamMoi;
        private RoundedButton btnThemAnh;
        private Label _lblMsg;
        private string _tinhTrangHienTai = "Trong";

        public frmCanHo()
        {
            Text = "Quản lý Căn hộ";
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
                Padding = new Padding(12, 12, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var searchPanel = CreateSearchPanel("Tìm kiếm", "Tìm theo mã căn hộ, số căn hộ, tình trạng");
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
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
            dgv.RowTemplate.Height = 30;
            TaoCotBang();
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
            cboToa = CreateComboBox();
            cboLoai = CreateComboBox();
            numDienTich = CreateNumber(0, 10000, 2);
            numGiaThue = CreateNumber(0, 1000000000000, 0);
            numTienCoc = CreateNumber(0, 1000000000000, 0);
            numTang = CreateNumber(1, 200, 0);
            txtSoCanHo = CreateTextBox("VD: A101", false);
            cboTinhTrang = CreateComboBox();
            cboTinhTrang.Items.AddRange(new object[] { "Trong", "BaoTri" });
            lblTinhTrangHienTai = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 24,
                ForeColor = Color.FromArgb(75, 85, 99),
                Font = new Font("Segoe UI", 9F)
            };
            txtMoTa = CreateTextBox("Ghi chú", false);
            txtMoTa.Multiline = true;
            txtMoTa.Height = 72;
            lstTienNghi = new CheckedListBox
            {
                Dock = DockStyle.Top,
                Height = 110,
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F)
            };
            lblTienNghi = new Label { Dock = DockStyle.Top, AutoSize = false, Height = 48, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5F) };
            picAnh = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 110,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(236, 240, 244),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Bat dau tat ca cac truong co the sua - se duoc bat khi vao Add/Edit mode
            SetEditorsEnabled(false);

            AddField(right, "Mã căn hộ", txtMa);
            AddField(right, "Tòa nhà *", cboToa);
            AddField(right, "Loại căn hộ *", cboLoai);
            AddField(right, "Diện tích (m²) *", numDienTich);
            AddField(right, "Giá thuê niêm yết *", numGiaThue);
            AddField(right, "Tiền cọc niêm yết", numTienCoc);
            AddField(right, "Tầng số *", numTang);
            AddField(right, "Số căn hộ", txtSoCanHo);
            AddField(right, "Tình trạng", cboTinhTrang);
            right.Controls.Add(lblTinhTrangHienTai);
            AddField(right, "Mô tả", txtMoTa);
            AddField(right, "Tiện nghi", lstTienNghi);
            AddField(right, "Tiện nghi đã gán", lblTienNghi);
            AddField(right, "Ảnh phòng", picAnh);

            var commands = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            btnThem   = CreateButton("Thêm",    btnThem_Click);
            btnSua    = CreateButton("Sửa",     btnSua_Click);
            btnXoa    = CreateButton("Xóa",     btnXoa_Click);
            btnLamMoi = CreateButton("Làm mới", btnLamMoi_Click);
            btnThemAnh = CreateButton("Thêm ảnh", btnThemAnh_Click);
            commands.Controls.Add(btnThem);
            commands.Controls.Add(btnSua);
            commands.Controls.Add(btnXoa);
            commands.Controls.Add(btnLamMoi);
            commands.Controls.Add(btnThemAnh);
            right.Controls.Add(commands);
            UpdateButtonStyles();

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
            btnThem.Text  = "Lưu";  btnThem.Enabled  = true;
            btnSua.Text   = "Sửa";  btnSua.Enabled   = false;
            btnXoa.Enabled = false; btnLamMoi.Text = "Làm mới";
            UpdateButtonStyles();
        }

        private void EnterViewMode()
        {
            _mode = FormMode.View;
            HideMsg(); SetEditorsEnabled(false);
            btnThem.Text  = "Thêm"; btnThem.Enabled  = true;
            btnSua.Text   = "Sửa";  btnSua.Enabled   = true;
            btnXoa.Enabled = true;  btnLamMoi.Text = "Làm mới";
            UpdateButtonStyles();
        }

        private void EnterEditMode()
        {
            _mode = FormMode.Editing;
            HideMsg(); SetEditorsEnabled(true);
            btnThem.Text  = "Thêm"; btnThem.Enabled  = false;
            btnSua.Text   = "Lưu";  btnSua.Enabled   = true;
            btnXoa.Enabled = false; btnLamMoi.Text = "Hủy";
            UpdateButtonStyles();
        }

        private void SetEditorsEnabled(bool enabled)
        {
            cboToa.Enabled        = enabled;
            cboLoai.Enabled       = enabled;
            cboTinhTrang.Enabled  = enabled && _tinhTrangHienTai != "DaDatCoc" && _tinhTrangHienTai != "DangThue";
            numDienTich.Enabled   = enabled;
            numGiaThue.Enabled    = enabled;
            numTienCoc.Enabled    = enabled;
            numTang.Enabled       = enabled;
            txtSoCanHo.ReadOnly   = !enabled;
            txtMoTa.ReadOnly      = !enabled;
            lstTienNghi.Enabled   = enabled;
        }

        // ── Helper constructors ──────────────────────────────────────────────────

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

        private MaterialTextBox CreateTextBox(string hint, bool readOnly)
        {
            var txt = new MaterialTextBox { Dock = DockStyle.Top, Hint = hint, ReadOnly = readOnly };
            txt.Font = new Font("Segoe UI", 10F);
            return txt;
        }

        private ComboBox CreateComboBox()
        {
            var combo = new ComboBox { Dock = DockStyle.Top, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            combo.Font = new Font("Segoe UI", 10F);
            return combo;
        }

        private NumericUpDown CreateNumber(decimal min, decimal max, int decimalPlaces)
        {
            var num = new KeyboardOnlyNumericUpDown
            {
                Dock = DockStyle.Top, Height = 32, Minimum = min, Maximum = max,
                DecimalPlaces = decimalPlaces, ThousandsSeparator = true
            };
            num.Font = new Font("Segoe UI", 10F);
            return num;
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

        private RoundedButton CreateButton(string text, EventHandler handler)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Width = text.Length > 8 ? 130 : 90,
                Height = 36,
                Radius = 10,
                Margin = new Padding(0, 4, 6, 4)
            };
            btn.Click += handler;
            return btn;
        }

        // ── Data ─────────────────────────────────────────────────────────────────

        private void TaoCotBang()
        {
            dgv.Columns.Clear();
            AddGridColumn("MaCanHo", "Mã căn hộ", 90);
            AddGridColumn("TenCanHo", "Tên căn hộ", 95);
            AddGridColumn("TenToa", "Tên tòa", 100);
            AddGridColumn("TenLoai", "Loại", 80);
            AddGridColumn("DienTich", "Diện tích", 90, "N1");
            AddGridColumn("GiaThueNiemYet", "Giá thuê", 110, "N0");
            AddGridColumn("TienCocNiemYet", "Tiền cọc", 110, "N0");
            AddGridColumn("SoCanHo", "Số căn", 80);
            AddGridColumn("TangSo", "Tầng", 70);
            AddGridColumn("TinhTrang", "Tình trạng", 100);
        }

        private void AddGridColumn(string propertyName, string header, int minWidth, string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                Name = propertyName,
                HeaderText = header,
                MinimumWidth = minWidth,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            if (!string.IsNullOrEmpty(format))
            {
                column.DefaultCellStyle.Format = format;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgv.Columns.Add(column);
        }

        private void NapComboBox()
        {
            cboToa.DisplayMember = "TenToa";
            cboToa.ValueMember   = "MaToa";
            cboToa.DataSource    = _toaSvc.LayTatCa().OrderBy(t => t.TenToa).ToList();

            cboLoai.DisplayMember = "TenLoai";
            cboLoai.ValueMember   = "MaLoai";
            cboLoai.DataSource    = _loaiSvc.LayTatCa().OrderBy(l => l.TenLoai).ToList();

            lstTienNghi.DisplayMember = "TenTienNghi";
            lstTienNghi.ValueMember = "MaTienNghi";
            lstTienNghi.Items.Clear();
            foreach (var tienNghi in _tienNghiSvc.LayTatCa().OrderBy(t => t.TenTienNghi))
                lstTienNghi.Items.Add(tienNghi, false);

            if (cboTinhTrang.Items.Count > 0) cboTinhTrang.SelectedIndex = 0;
        }

        private void TaiDuLieu()
        {
            var data = _canHoSvc.LayTatCa().OrderBy(c => c.MaCanHo).ToList();
            CapNhatGrid(data);
            dgv.ClearSelection();
        }

        private void TimKiem()
        {
            string kw = TextFormatHelper.NormalizeSearch(txtTimKiem.Text);
            var data = _canHoSvc.LayTatCa();
            if (!string.IsNullOrEmpty(kw))
            {
                data = data.Where(c =>
                    TextFormatHelper.ContainsNormalized(c.MaCanHo, kw) ||
                    TextFormatHelper.ContainsNormalized(c.SoCanHo.ToString(), kw) ||
                    TextFormatHelper.ContainsNormalized(c.TinhTrang, kw));
            }
            CapNhatGrid(data.OrderBy(c => c.MaCanHo).ToList());
        }

        private void CapNhatGrid(List<CanHo> canHos)
        {
            var toas = _toaSvc.LayTatCa().ToDictionary(t => t.MaToa, t => t.TenToa);
            var loais = _loaiSvc.LayTatCa().ToDictionary(l => l.MaLoai, l => l.TenLoai);

            var list = canHos.Select(c => new
            {
                c.MaCanHo,
                TenCanHo = "Căn " + c.SoCanHo,
                TenToa = toas.ContainsKey(c.MaToa) ? toas[c.MaToa] : c.MaToa,
                TenLoai = loais.ContainsKey(c.MaLoai) ? loais[c.MaLoai] : c.MaLoai,
                c.DienTich,
                c.GiaThueNiemYet,
                c.TienCocNiemYet,
                c.SoCanHo,
                c.TangSo,
                c.TinhTrang
            }).ToList();

            dgv.DataSource = new BindingList<object>(list.Cast<object>().ToList());
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var maCanHo = dgv.CurrentRow.Cells["MaCanHo"].Value?.ToString();
            if (string.IsNullOrEmpty(maCanHo)) return;
            var c = _canHoSvc.LayTheoMa(maCanHo);
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
            _tinhTrangHienTai = c.TinhTrang;
            if (c.TinhTrang == "Trong" || c.TinhTrang == "BaoTri")
                cboTinhTrang.SelectedItem = c.TinhTrang;
            else
                cboTinhTrang.SelectedIndex = -1;
            lblTinhTrangHienTai.Text = "Tình trạng hiện tại: " + HienThiTinhTrang(c.TinhTrang);
            txtMoTa.Text = c.MoTa;
            NapTienNghiVaAnh(c.MaCanHo);
        }

        private string HienThiTinhTrang(string tinhTrang)
        {
            if (tinhTrang == "DaDatCoc") return "Đã đặt cọc (từ phiếu đặt trước)";
            if (tinhTrang == "DangThue") return "Đang thuê (từ hợp đồng)";
            if (tinhTrang == "Trong") return "Còn trống";
            if (tinhTrang == "BaoTri") return "Bảo trì";
            return tinhTrang;
        }

        private void NapTienNghiVaAnh(string maCanHo)
        {
            var tienNghiById = _tienNghiSvc.LayTatCa()
                .GroupBy(t => t.MaTienNghi)
                .ToDictionary(g => g.Key, g => g.First().TenTienNghi);
            var names = _canHoSvc.LayTienNghiCuaCanHo(maCanHo)
                .Select(t =>
                {
                    string ten;
                    return tienNghiById.TryGetValue(t.MaTienNghi, out ten) ? ten : t.MaTienNghi;
                })
                .ToList();
            lblTienNghi.Text = names.Count == 0 ? "Chưa gán tiện nghi." : string.Join(", ", names);
            CapNhatDanhSachTienNghiDaChon(maCanHo);

            if (picAnh.Image != null)
            {
                picAnh.Image.Dispose();
                picAnh.Image = null;
            }
            var anh = _canHoSvc.LayAnhDaiDien(maCanHo);
            if (anh != null && System.IO.File.Exists(anh.DuongDanAnh))
            {
                Image preview;
                string loi;
                if (TryCreatePreviewImage(anh.DuongDanAnh, out preview, out loi))
                {
                    picAnh.Image = preview;
                }
                else
                {
                    ShowError(loi);
                }
            }
        }

        private bool TryCreatePreviewImage(string path, out Image preview, out string error)
        {
            preview = null;
            error = string.Empty;

            try
            {
                using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                using (var original = Image.FromStream(stream, false, true))
                {
                    var maxWidth = picAnh.Width > 0 ? picAnh.Width : 640;
                    var maxHeight = picAnh.Height > 0 ? picAnh.Height : 360;
                    var ratio = Math.Min((float)maxWidth / original.Width, (float)maxHeight / original.Height);
                    if (ratio <= 0) ratio = 1f;
                    if (ratio > 1f) ratio = 1f;

                    var width = Math.Max(1, (int)(original.Width * ratio));
                    var height = Math.Max(1, (int)(original.Height * ratio));
                    var bitmap = new Bitmap(width, height);

                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.DrawImage(original, 0, 0, width, height);
                    }

                    preview = bitmap;
                    return true;
                }
            }
            catch (OutOfMemoryException)
            {
                error = "File anh khong hop le, bi loi hoac kich thuoc qua lon.";
                return false;
            }
            catch (ArgumentException)
            {
                error = "File duoc chon khong phai anh hop le.";
                return false;
            }
            catch (Exception ex)
            {
                error = "Khong the tai anh: " + LayLoiSauCung(ex);
                return false;
            }
        }

        private void CapNhatDanhSachTienNghiDaChon(string maCanHo)
        {
            var selected = new HashSet<string>(_canHoSvc.LayTienNghiCuaCanHo(maCanHo).Select(t => t.MaTienNghi));
            for (var i = 0; i < lstTienNghi.Items.Count; i++)
            {
                var tienNghi = lstTienNghi.Items[i] as TienNghi;
                lstTienNghi.SetItemChecked(i, tienNghi != null && selected.Contains(tienNghi.MaTienNghi));
            }
        }

        private IEnumerable<string> LayTienNghiDangTick()
        {
            return lstTienNghi.CheckedItems
                .Cast<TienNghi>()
                .Select(t => t.MaTienNghi)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
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
                loi = "Vui lòng chọn tòa nhà.";
                _errors.SetError(cboToa, loi);
                return false;
            }
            if (cboLoai.SelectedValue == null)
            {
                loi = "Vui lòng chọn loại căn hộ.";
                _errors.SetError(cboLoai, loi);
                return false;
            }
            if (numDienTich.Value <= 0)
            {
                loi = "Diện tích phải lớn hơn 0.";
                _errors.SetError(numDienTich, loi);
                return false;
            }
            if (numGiaThue.Value <= 0)
            {
                loi = "Giá thuê phải lớn hơn 0.";
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
                var canHo = DocForm();
                if (!_canHoSvc.Them(canHo, out loi)) { ShowError(loi); return; }
                _canHoSvc.DongBoTienNghi(canHo.MaCanHo, LayTienNghiDangTick());
            }
            catch (Exception ex) { ShowError(LayLoiSauCung(ex)); return; }
            TaiDuLieu();
            EnterAddMode();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View)
            {
                if (string.IsNullOrWhiteSpace(txtMa.Text)) { ShowError("Chọn căn hộ cần sửa."); return; }
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
                try 
                { 
                     _canHoSvc.Sua(canHo); 
                     _canHoSvc.DongBoTienNghi(canHo.MaCanHo, LayTienNghiDangTick());
                }
                catch (Exception ex) { ShowError(LayLoiSauCung(ex)); return; }
                TaiDuLieu();
                EnterAddMode();
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { ShowError("Chọn căn hộ cần xóa."); return; }
            if (MessageBox.Show("Xóa căn hộ đang chọn?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                string loi;
                if (!_canHoSvc.XoaCanHo(txtMa.Text, out loi))
                {
                    ShowError(loi);
                    return;
                }
                TaiDuLieu();
                EnterAddMode();
            }
            catch (Exception ex) { ShowError("Không thể xóa vì có dữ liệu liên quan: " + LayLoiSauCung(ex)); }
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



        private void btnThemAnh_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { ShowError("Chọn căn hộ trước khi thêm ảnh."); return; }
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Chọn ảnh phòng";
                dialog.Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                Image preview;
                string loi;
                if (!TryCreatePreviewImage(dialog.FileName, out preview, out loi))
                {
                    ShowError(loi);
                    return;
                }

                _canHoSvc.ThemAnh(txtMa.Text, dialog.FileName, "Ảnh phòng");
                if (picAnh.Image != null)
                    picAnh.Image.Dispose();
                picAnh.Image = preview;
            }
            HideMsg();
        }

        private static string LayLoiSauCung(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }

        private void XoaTrong()
        {
            txtMa.Clear();
            _tinhTrangHienTai = "Trong";
            txtSoCanHo.Clear();
            txtMoTa.Clear();
            numDienTich.Value = 0;
            numGiaThue.Value  = 0;
            numTienCoc.Value  = 0;
            numTang.Value     = 1;
            if (cboToa.Items.Count > 0) cboToa.SelectedIndex = 0;
            if (cboLoai.Items.Count > 0) cboLoai.SelectedIndex = 0;
            if (cboTinhTrang.Items.Count > 0) cboTinhTrang.SelectedIndex = 0;
            if (lblTinhTrangHienTai != null) lblTinhTrangHienTai.Text = "Chỉ được chọn Trống hoặc Bảo trì. Đã đặt cọc/Đang thuê do hệ thống cập nhật.";
            lblTienNghi.Text = "Chọn căn hộ để xem tiện nghi.";
            if (picAnh.Image != null)
            {
                picAnh.Image.Dispose();
                picAnh.Image = null;
            }
            dgv.ClearSelection();
            _errors.Clear();
        }

        private void UpdateButtonStyles()
        {
            ApplyButtonStyle(btnThem);
            ApplyButtonStyle(btnSua);
            ApplyButtonStyle(btnXoa);
            ApplyButtonStyle(btnLamMoi);
            ApplyButtonStyle(btnThemAnh);
        }

        private void ApplyButtonStyle(RoundedButton btn)
        {
            if (btn == null) return;

            string txt = btn.Text;
            if (txt == "Thêm")
            {
                btn.BackColor = Color.FromArgb(37, 99, 235); // blue-600
                btn.BorderColor = Color.FromArgb(29, 78, 216); // blue-700
                btn.ForeColor = Color.White;
            }
            else if (txt == "Sửa")
            {
                btn.BackColor = Color.FromArgb(245, 158, 11); // amber-500
                btn.BorderColor = Color.FromArgb(217, 119, 6); // amber-600
                btn.ForeColor = Color.White;
            }
            else if (txt == "Xóa")
            {
                btn.BackColor = Color.FromArgb(239, 68, 68); // red-500
                btn.BorderColor = Color.FromArgb(220, 38, 38); // red-600
                btn.ForeColor = Color.White;
            }
            else if (txt == "Lưu")
            {
                btn.BackColor = Color.FromArgb(16, 185, 129); // emerald-500
                btn.BorderColor = Color.FromArgb(5, 150, 105); // emerald-600
                btn.ForeColor = Color.White;
            }
            else if (txt == "Hủy")
            {
                btn.BackColor = Color.FromArgb(239, 68, 68); // red-500
                btn.BorderColor = Color.FromArgb(220, 38, 38); // red-600
                btn.ForeColor = Color.White;
            }
            else if (txt == "Thêm ảnh")
            {
                btn.BackColor = Color.FromArgb(6, 182, 212); // cyan-500
                btn.BorderColor = Color.FromArgb(8, 145, 178); // cyan-600
                btn.ForeColor = Color.White;
            }
            else // "Làm mới" or other
            {
                btn.BackColor = Color.FromArgb(107, 114, 128); // gray-500
                btn.BorderColor = Color.FromArgb(75, 85, 99); // gray-600
                btn.ForeColor = Color.White;
            }
        }
    }
}
