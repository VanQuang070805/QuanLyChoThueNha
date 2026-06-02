using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;

namespace QuanLyChoThueNha.GUI.Forms.BaoCao
{
    public class frmBaoCao : MaterialForm
    {
        private readonly BaoCaoService _service = new BaoCaoService();
        private readonly ComboBox _cboKyBaoCao = new ComboBox();
        private readonly DateTimePicker _dtpMocBaoCao = new DateTimePicker();
        private readonly TrackBar _zoomBaoCao = new TrackBar();
        private readonly FlowLayoutPanel _kpiPanel = new FlowLayoutPanel();
        private readonly SmartChartPanel _chartDoanhThu = new SmartChartPanel();
        private readonly SmartChartPanel _chartTinhTrang = new SmartChartPanel();
        private readonly SmartChartPanel _chartLoaiCanHo = new SmartChartPanel();
        private readonly SmartChartPanel _chartTopCanHo = new SmartChartPanel();
        private readonly DataGridView _gridCanhBao = CreateGrid();
        private readonly DataGridView _gridCongNo = CreateGrid();
        private Panel _scrollHost;
        private TableLayoutPanel _root;

        public frmBaoCao()
        {
            Text = "Báo cáo & Thống kê";
            Size = new Size(1280, 820);
            StartPosition = FormStartPosition.CenterScreen;
            BuildLayout();
            Load += delegate { LoadReport(); };
        }

        private void BuildLayout()
        {
            _scrollHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            _scrollHost.Resize += delegate { ResizeReportRoot(); };

            _root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                RowCount = 5,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 24));

            var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 9, BackColor = Color.FromArgb(248, 250, 252) };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            toolbar.Controls.Add(new Label
            {
                Text = "Báo cáo & Thống kê",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            toolbar.Controls.Add(new Label
            {
                Text = "Kỳ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 1, 0);
            _cboKyBaoCao.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboKyBaoCao.Items.AddRange(new object[] { "Ngày", "Tuần", "Tháng", "Năm" });
            _cboKyBaoCao.SelectedIndex = 2;
            _cboKyBaoCao.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _cboKyBaoCao.SelectedIndexChanged += delegate
            {
                CapNhatDinhDangMocBaoCao();
                if (IsHandleCreated) LoadReport();
            };
            toolbar.Controls.Add(_cboKyBaoCao, 2, 0);

            toolbar.Controls.Add(new Label
            {
                Text = "Mốc",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 3, 0);
            _dtpMocBaoCao.Value = DateTime.Today;
            _dtpMocBaoCao.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _dtpMocBaoCao.ValueChanged += delegate { if (IsHandleCreated) LoadReport(); };
            toolbar.Controls.Add(_dtpMocBaoCao, 4, 0);

            var btnXem = new RoundedButton
            {
                Text = "Xem",
                Height = 36,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Radius = 10,
                BackColor = Color.FromArgb(37, 99, 235),
                BorderColor = Color.FromArgb(29, 78, 216),
                ForeColor = Color.White
            };
            btnXem.Click += delegate { LoadReport(); };
            toolbar.Controls.Add(btnXem, 5, 0);

            var btnExcel = new RoundedButton
            {
                Text = "Xuất Excel",
                Height = 36,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Radius = 10,
                BackColor = Color.FromArgb(16, 185, 129),
                BorderColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White
            };
            btnExcel.Click += BtnExcel_Click;
            toolbar.Controls.Add(btnExcel, 6, 0);

            toolbar.Controls.Add(new Label
            {
                Text = "Zoom",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 7, 0);
            _zoomBaoCao.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _zoomBaoCao.Minimum = 80;
            _zoomBaoCao.Maximum = 140;
            _zoomBaoCao.TickFrequency = 20;
            _zoomBaoCao.Value = 100;
            _zoomBaoCao.ValueChanged += delegate { ApDungZoomBaoCao(); };
            toolbar.Controls.Add(_zoomBaoCao, 8, 0);
            _root.Controls.Add(toolbar, 0, 0);

            _kpiPanel.Dock = DockStyle.Fill;
            _kpiPanel.WrapContents = false;
            _kpiPanel.AutoScroll = false;
            _kpiPanel.BackColor = Color.FromArgb(248, 250, 252);
            _kpiPanel.SizeChanged += delegate { ResizeKpis(); };
            _root.Controls.Add(_kpiPanel, 0, 1);

            var chartsTop = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            chartsTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            chartsTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            chartsTop.Controls.Add(Card("Doanh thu vs Công nợ", _chartDoanhThu), 0, 0);
            chartsTop.Controls.Add(Card("Cơ cấu tình trạng căn hộ", _chartTinhTrang), 1, 0);
            _root.Controls.Add(chartsTop, 0, 2);

            var chartsBottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            chartsBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            chartsBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            chartsBottom.Controls.Add(Card("Loại nhà được thuê nhiều", _chartLoaiCanHo), 0, 0);
            chartsBottom.Controls.Add(Card("Top căn hộ doanh thu cao", _chartTopCanHo), 1, 0);
            _root.Controls.Add(chartsBottom, 0, 3);

            var grids = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grids.Controls.Add(Card("Hợp đồng sắp hết hạn", _gridCanhBao), 0, 0);
            grids.Controls.Add(Card("Khách nợ tiền thuê", _gridCongNo), 1, 0);
            _root.Controls.Add(grids, 0, 4);

            _scrollHost.Controls.Add(_root);
            Controls.Add(_scrollHost);
            ApDungZoomBaoCao();
        }

        private void ApDungZoomBaoCao()
        {
            var zoom = _zoomBaoCao.Value / 100f;
            _chartDoanhThu.Zoom = zoom;
            _chartTinhTrang.Zoom = zoom;
            _chartLoaiCanHo.Zoom = zoom;
            _chartTopCanHo.Zoom = zoom;
            if (_root != null)
            {
                _root.RowStyles[1].Height = (int)Math.Round(108 * zoom);
                _root.RowStyles[2].Height = 42 + (zoom - 1f) * 8;
                _root.RowStyles[3].Height = 34 + (zoom - 1f) * 6;
                _root.RowStyles[4].Height = 24;
                ResizeKpis();
                ResizeReportRoot();
                _root.PerformLayout();
            }
        }

        private void ResizeReportRoot()
        {
            if (_scrollHost == null || _root == null) return;
            var zoom = _zoomBaoCao.Value / 100f;
            _root.Width = Math.Max(900, _scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4);
            _root.Height = Math.Max(_scrollHost.ClientSize.Height + 260, (int)Math.Round(900 * zoom));
        }

        private void CapNhatDinhDangMocBaoCao()
        {
            var ky = LayKyBaoCao();
            _dtpMocBaoCao.Format = DateTimePickerFormat.Custom;
            if (ky == "Ngay" || ky == "Tuan")
            {
                _dtpMocBaoCao.CustomFormat = "dd/MM/yyyy";
            }
            else if (ky == "Thang")
            {
                _dtpMocBaoCao.CustomFormat = "MM/yyyy";
            }
            else
            {
                _dtpMocBaoCao.CustomFormat = "yyyy";
            }
        }

        private string LayKyBaoCao()
        {
            var text = Convert.ToString(_cboKyBaoCao.SelectedItem) ?? "Tháng";
            switch (text)
            {
                case "Ngày": return "Ngay";
                case "Tuần": return "Tuan";
                case "Năm": return "Nam";
                default: return "Thang";
            }
        }

        private void LayKhoangBaoCao(out DateTime tuNgay, out DateTime denNgay, out string nhomTheo)
        {
            var moc = _dtpMocBaoCao.Value.Date;
            var ky = LayKyBaoCao();
            if (ky == "Ngay")
            {
                tuNgay = moc;
                denNgay = moc;
                nhomTheo = "Ngay";
                return;
            }

            if (ky == "Tuan")
            {
                var offset = ((int)moc.DayOfWeek + 6) % 7;
                tuNgay = moc.AddDays(-offset);
                denNgay = tuNgay.AddDays(6);
                nhomTheo = "Ngay";
                return;
            }

            if (ky == "Nam")
            {
                tuNgay = new DateTime(moc.Year, 1, 1);
                denNgay = new DateTime(moc.Year, 12, 31);
                nhomTheo = "Thang";
                return;
            }

            tuNgay = new DateTime(moc.Year, moc.Month, 1);
            denNgay = tuNgay.AddMonths(1).AddDays(-1);
            nhomTheo = "Ngay";
        }

        private void LoadReport()
        {
            try
            {
                DateTime tuNgay;
                DateTime denNgay;
                string nhomTheo;
                LayKhoangBaoCao(out tuNgay, out denNgay, out nhomTheo);
                LoadKpis(tuNgay, denNgay);
                LoadCharts(tuNgay, denNgay, nhomTheo);
                _gridCanhBao.DataSource = _service.HopDongCanhBao(45).ToList();
                _gridCongNo.DataSource = _service.CongNoQuaHan(tuNgay, denNgay).ToList();
                DinhDangGrids();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được báo cáo: " + ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DinhDangGrids()
        {
            if (_gridCanhBao.Columns.Contains("MaHopDong")) _gridCanhBao.Columns["MaHopDong"].HeaderText = "Mã hợp đồng";
            if (_gridCanhBao.Columns.Contains("MaCanHo")) _gridCanhBao.Columns["MaCanHo"].HeaderText = "Tên căn hộ";
            if (_gridCanhBao.Columns.Contains("MaKhach")) _gridCanhBao.Columns["MaKhach"].HeaderText = "Tên khách";
            if (_gridCanhBao.Columns.Contains("NgayKetThuc")) _gridCanhBao.Columns["NgayKetThuc"].HeaderText = "Ngày kết thúc";
            if (_gridCanhBao.Columns.Contains("SoNgayConLai")) _gridCanhBao.Columns["SoNgayConLai"].HeaderText = "Số ngày còn lại";
            if (_gridCanhBao.Columns.Contains("TrangThai")) _gridCanhBao.Columns["TrangThai"].HeaderText = "Trạng thái";
            
            if (_gridCanhBao.Columns.Contains("NgayKetThuc"))
                _gridCanhBao.Columns["NgayKetThuc"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (_gridCanhBao.Columns.Contains("TenKhach"))
                _gridCanhBao.Columns["TenKhach"].Visible = false;

            if (_gridCongNo.Columns.Contains("MaHoaDon")) _gridCongNo.Columns["MaHoaDon"].HeaderText = "Mã hóa đơn";
            if (_gridCongNo.Columns.Contains("MaHopDong")) _gridCongNo.Columns["MaHopDong"].HeaderText = "Mã hợp đồng";
            if (_gridCongNo.Columns.Contains("MaCanHo")) _gridCongNo.Columns["MaCanHo"].HeaderText = "Tên căn hộ";
            if (_gridCongNo.Columns.Contains("MaKhach")) _gridCongNo.Columns["MaKhach"].HeaderText = "Tên khách";
            if (_gridCongNo.Columns.Contains("KyThanhToan")) _gridCongNo.Columns["KyThanhToan"].HeaderText = "Kỳ thanh toán";
            if (_gridCongNo.Columns.Contains("SoTienConNo")) _gridCongNo.Columns["SoTienConNo"].HeaderText = "Số tiền còn nợ";
            if (_gridCongNo.Columns.Contains("NgayDaoHan")) _gridCongNo.Columns["NgayDaoHan"].HeaderText = "Ngày đáo hạn";
            if (_gridCongNo.Columns.Contains("TrangThai")) _gridCongNo.Columns["TrangThai"].HeaderText = "Trạng thái";

            if (_gridCongNo.Columns.Contains("NgayDaoHan"))
                _gridCongNo.Columns["NgayDaoHan"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (_gridCongNo.Columns.Contains("SoTienConNo"))
            {
                _gridCongNo.Columns["SoTienConNo"].DefaultCellStyle.Format = "N0";
                _gridCongNo.Columns["SoTienConNo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (_gridCongNo.Columns.Contains("TenKhach"))
                _gridCongNo.Columns["TenKhach"].Visible = false;

            QuanLyChoThueNha.GUI.Helpers.GridFormatterHelper.SetupCellFormatting(_gridCanhBao);
            QuanLyChoThueNha.GUI.Helpers.GridFormatterHelper.SetupCellFormatting(_gridCongNo);
        }

        private void LoadKpis(DateTime tuNgay, DateTime denNgay)
        {
            _kpiPanel.Controls.Clear();
            AddKpi("Tổng doanh thu", _service.DoanhThuTheoKy(tuNgay, denNgay, "Ngay").Sum(x => x.TongThu).ToString("N0") + " VNĐ", Color.FromArgb(4, 86, 197));
            AddKpi("Tỷ lệ lấp đầy", _service.TyLeLapDay().ToString("N1") + "%", Color.FromArgb(245, 158, 11));
            AddKpi("Công nợ", _service.TongCongNo(tuNgay, denNgay).ToString("N0") + " VNĐ", Color.FromArgb(239, 68, 68));
            AddKpi("Hợp đồng mới", _service.HopDongMoiTrongKhoang(tuNgay, denNgay).ToString("N0"), Color.FromArgb(16, 185, 129));
            ResizeKpis();
        }

        private void LoadCharts(DateTime tuNgay, DateTime denNgay, string nhomTheo)
        {
            var revenue = _service.DoanhThuTheoKy(tuNgay, denNgay, nhomTheo).ToList();
            var debt = _service.CongNoTheoKy(tuNgay, denNgay, nhomTheo).ToList();
            _chartDoanhThu.SetGroupedColumns(
                revenue.Select(x => x.Thang).ToList(),
                revenue.Select(x => x.TongThu).ToList(),
                debt.Select(x => x.TongThu).ToList(),
                "Doanh thu",
                "Công nợ");

            _chartTinhTrang.SetDonut(_service.TinhTrangCanHo()
                .Select(x => new SmartChartPoint(x.TinhTrang, x.SoLuong)).ToList());

            _chartLoaiCanHo.SetBars(_service.LoaiCanHoDuocThueNhieuNhat(10)
                .Select(x => new SmartChartPoint(x.Ten, x.SoLuong)).ToList(), Color.FromArgb(16, 185, 129));

            _chartTopCanHo.SetBars(_service.TopCanHoDoanhThu(tuNgay, denNgay, 5)
                .Select(x => new SmartChartPoint(x.Ten, x.GiaTri)).ToList(), Color.FromArgb(4, 86, 197));
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(226, 232, 240),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 34 },
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(239, 246, 255),
                    ForeColor = Color.FromArgb(30, 64, 175),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    SelectionBackColor = Color.FromArgb(239, 246, 255),
                    SelectionForeColor = Color.FromArgb(30, 64, 175)
                },
                DefaultCellStyle =
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(15, 23, 42),
                    SelectionBackColor = Color.FromArgb(219, 234, 254),
                    SelectionForeColor = Color.FromArgb(15, 23, 42),
                    Font = new Font("Segoe UI", 9F)
                },
                AlternatingRowsDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(248, 250, 252)
                }
            };
        }

        private static Control Card(string title, Control content)
        {
            var panel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(16),
                Radius = 16,
                BorderColor = Color.FromArgb(226, 232, 240)
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.White };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            content.Dock = DockStyle.Fill;
            layout.Controls.Add(content, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private void AddKpi(string title, string value, Color color)
        {
            var panel = new RoundedPanel
            {
                Width = 260,
                Height = 94,
                Margin = new Padding(0, 0, 12, 0),
                BackColor = color,
                BorderColor = color,
                Radius = 12,
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
                Text = title.ToUpperInvariant(),
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
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            }, 0, 1);
            panel.Controls.Add(layout);
            _kpiPanel.Controls.Add(panel);
        }

        private void ResizeKpis()
        {
            if (_kpiPanel.ClientSize.Width <= 0 || _kpiPanel.Controls.Count == 0) return;
            var count = _kpiPanel.Controls.Count;
            var gap = 12;
            var width = Math.Max(190, (_kpiPanel.ClientSize.Width - (gap * (count - 1)) - 4) / count);
            foreach (Control control in _kpiPanel.Controls)
            {
                control.Width = width;
                control.Height = Math.Max(86, _kpiPanel.ClientSize.Height - 8);
            }
        }

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            DateTime tuNgay;
            DateTime denNgay;
            string nhomTheo;
            LayKhoangBaoCao(out tuNgay, out denNgay, out nhomTheo);
            using (var dialog = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = "BaoCaoSmartApart.xlsx" })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var wb = new XLWorkbook())
                    {
                        WriteSheet(wb, "Doanh thu", _service.DoanhThuTheoKy(tuNgay, denNgay, nhomTheo).Select(x => new { x.Thang, x.TongThu, x.SoHoaDon }).ToList());
                        WriteSheet(wb, "Cong no", _service.CongNoQuaHan(tuNgay, denNgay).ToList());
                        WriteSheet(wb, "Hop dong canh bao", _service.HopDongCanhBao(45).ToList());
                        WriteSheet(wb, "Loai nha", _service.LoaiCanHoDuocThueNhieuNhat(10).ToList());
                        WriteSheet(wb, "Top can ho", _service.TopCanHoDoanhThu(tuNgay, denNgay, 5).ToList());
                        wb.SaveAs(dialog.FileName);
                    }
                    MessageBox.Show("Đã xuất Excel.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không xuất được Excel: " + ex.Message, "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private static void WriteSheet<T>(XLWorkbook wb, string sheetName, System.Collections.Generic.IEnumerable<T> data)
        {
            var ws = wb.Worksheets.Add(sheetName);
            ws.Cell(1, 1).InsertTable(data);
            ws.Columns().AdjustToContents();
        }

        private class SmartChartPoint
        {
            public SmartChartPoint(string label, decimal value)
            {
                Label = label;
                Value = value;
            }

            public string Label { get; private set; }
            public decimal Value { get; private set; }
        }

        private class SmartChartPanel : Panel
        {
            private string _mode = "bars";
            private List<string> _labels = new List<string>();
            private List<decimal> _valuesA = new List<decimal>();
            private List<decimal> _valuesB = new List<decimal>();
            private List<SmartChartPoint> _points = new List<SmartChartPoint>();
            private string _legendA = "A";
            private string _legendB = "B";
            private readonly ToolTip _tooltip = new ToolTip();
            private readonly List<DonutSlice> _donutSlices = new List<DonutSlice>();
            private int _activeSlice = -1;
            private Color _primary = Color.FromArgb(4, 86, 197);
            private readonly Color _secondary = Color.FromArgb(239, 68, 68);
            private readonly Color[] _palette = new[]
            {
                Color.FromArgb(4, 86, 197),
                Color.FromArgb(16, 185, 129),
                Color.FromArgb(245, 158, 11),
                Color.FromArgb(239, 68, 68),
                Color.FromArgb(99, 102, 241),
                Color.FromArgb(20, 184, 166)
            };
            private float _zoom = 1f;

            public float Zoom
            {
                get { return _zoom; }
                set
                {
                    _zoom = Math.Max(0.8f, Math.Min(1.6f, value));
                    Invalidate();
                }
            }

            public SmartChartPanel()
            {
                Dock = DockStyle.Fill;
                BackColor = Color.White;
                DoubleBuffered = true;
                Resize += delegate { Invalidate(); };
                MouseMove += SmartChartPanel_MouseMove;
                MouseLeave += delegate
                {
                    _activeSlice = -1;
                    _tooltip.Hide(this);
                };
            }

            public void SetGroupedColumns(List<string> labels, List<decimal> valuesA, List<decimal> valuesB, string legendA, string legendB)
            {
                _mode = "columns";
                _labels = labels;
                _valuesA = valuesA;
                _valuesB = valuesB;
                _legendA = legendA;
                _legendB = legendB;
                Invalidate();
            }

            public void SetBars(List<SmartChartPoint> points, Color color)
            {
                _mode = "bars";
                _points = points;
                _primary = color;
                Invalidate();
            }

            public void SetDonut(List<SmartChartPoint> points)
            {
                _mode = "donut";
                _points = points;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                if (_mode == "columns") DrawColumns(e.Graphics);
                else if (_mode == "donut") DrawDonut(e.Graphics);
                else DrawBars(e.Graphics);
            }

            private void DrawColumns(Graphics g)
            {
                if (_labels.Count == 0) return;
                var area = new Rectangle(44, 18, Math.Max(10, Width - 68), Math.Max(10, Height - (int)(70 * _zoom)));
                var max = Math.Max(1, _valuesA.Concat(_valuesB).DefaultIfEmpty(0).Max());
                using (var gridPen = new Pen(Color.FromArgb(229, 231, 235)))
                using (var font = new Font("Segoe UI", 8F))
                using (var blue = new SolidBrush(_primary))
                using (var red = new SolidBrush(_secondary))
                using (var text = new SolidBrush(Color.FromArgb(107, 114, 128)))
                {
                    for (var i = 0; i <= 4; i++)
                    {
                        var y = area.Bottom - (area.Height * i / 4);
                        g.DrawLine(gridPen, area.Left, y, area.Right, y);
                    }

                    var groupWidth = area.Width / Math.Max(1, _labels.Count);
                    var visibleLabelCount = Math.Max(1d, area.Width / 58d);
                    var labelStep = Math.Max(1, (int)Math.Ceiling(_labels.Count / visibleLabelCount));
                    for (var i = 0; i < _labels.Count; i++)
                    {
                        var a = i < _valuesA.Count ? _valuesA[i] : 0;
                        var b = i < _valuesB.Count ? _valuesB[i] : 0;
                        var hA = (int)(area.Height * (double)(a / max));
                        var hB = (int)(area.Height * (double)(b / max));
                        var x = area.Left + i * groupWidth + groupWidth / 4;
                        var barW = Math.Max(4, groupWidth / 5);
                        g.FillRectangle(blue, x, area.Bottom - hA, barW, hA);
                        g.FillRectangle(red, x + barW + 3, area.Bottom - hB, barW, hB);
                        if (i % labelStep == 0 || i == _labels.Count - 1)
                            g.DrawString(_labels[i].Replace("/" + DateTime.Today.Year, ""), font, text, area.Left + i * groupWidth + 2, area.Bottom + 6);
                    }

                    DrawLegend(g, _legendA, _legendB);
                }
            }

            private void DrawBars(Graphics g)
            {
                if (_points.Count == 0)
                {
                    DrawEmpty(g);
                    return;
                }

                var area = new Rectangle(18, 18, Math.Max(10, Width - 36), Math.Max(10, Height - 36));
                var max = Math.Max(1, _points.Max(p => p.Value));
                var rowHeight = Math.Max(26, area.Height / Math.Max(1, _points.Count));
                using (var fill = new SolidBrush(_primary))
                using (var bg = new SolidBrush(Color.FromArgb(238, 242, 247)))
                using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (var small = new Font("Segoe UI", 8F))
                using (var text = new SolidBrush(Color.FromArgb(17, 24, 39)))
                using (var muted = new SolidBrush(Color.FromArgb(107, 114, 128)))
                {
                    for (var i = 0; i < _points.Count; i++)
                    {
                        var p = _points[i];
                        var y = area.Top + i * rowHeight;
                        g.DrawString(TrimLabel(p.Label, 24), font, text, area.Left, y);
                        g.DrawString(p.Value.ToString("N0"), small, muted, area.Right - 70, y);
                        var barY = y + 20;
                        var barW = area.Width - 8;
                        g.FillRectangle(bg, area.Left, barY, barW, 8);
                        g.FillRectangle(fill, area.Left, barY, (int)(barW * (double)(p.Value / max)), 8);
                    }
                }
            }

            private void DrawDonut(Graphics g)
            {
                _donutSlices.Clear();
                if (_points.Count == 0)
                {
                    DrawEmpty(g);
                    return;
                }

                var legendWidth = Width >= 420 ? 150 : 0;
                var availableWidth = Math.Max(100, Width - legendWidth - 28);
                var availableHeight = Math.Max(90, Height - 26);
                var size = (int)(Math.Min(availableWidth, availableHeight) * _zoom);
                size = Math.Max(96, Math.Min(size, Math.Min(availableWidth, availableHeight)));
                var left = legendWidth > 0 ? legendWidth + (availableWidth - size) / 2 : (Width - size) / 2;
                var rect = new Rectangle(Math.Max(10, left), Math.Max(8, (Height - size) / 2), size, size);
                var total = Math.Max(1, _points.Sum(p => p.Value));
                float start = -90;
                for (var i = 0; i < _points.Count; i++)
                {
                    var sweep = (float)(360m * _points[i].Value / total);
                    _donutSlices.Add(new DonutSlice
                    {
                        Index = i,
                        Rect = rect,
                        StartAngle = start,
                        SweepAngle = sweep,
                        InnerRadius = rect.Width / 4f,
                        Text = string.Format("{0}: {1:N0}", _points[i].Label, _points[i].Value)
                    });
                    using (var brush = new SolidBrush(_palette[i % _palette.Length]))
                        g.FillPie(brush, rect, start, sweep);
                    start += sweep;
                }
                using (var brush = new SolidBrush(Color.White))
                    g.FillEllipse(brush, Rectangle.Inflate(rect, -rect.Width / 4, -rect.Height / 4));

                using (var font = new Font("Segoe UI", Math.Max(18F, 22F * _zoom), FontStyle.Bold))
                using (var text = new SolidBrush(Color.FromArgb(17, 24, 39)))
                {
                    var percent = _points.Count == 0 ? "0%" : "100%";
                    var sizeText = g.MeasureString(percent, font);
                    g.DrawString(percent, font, text, rect.Left + (rect.Width - sizeText.Width) / 2, rect.Top + (rect.Height - sizeText.Height) / 2);
                }

                DrawDonutLegend(g, legendWidth > 0 ? 18 : Math.Min(rect.Bottom + 8, Height - 94));
            }

            private void DrawLegend(Graphics g, string a, string b)
            {
                using (var font = new Font("Segoe UI", 8F))
                using (var blue = new SolidBrush(_primary))
                using (var red = new SolidBrush(_secondary))
                using (var text = new SolidBrush(Color.FromArgb(107, 114, 128)))
                {
                    var y = Height - 24;
                    g.FillEllipse(blue, Width / 2 - 86, y + 4, 10, 10);
                    g.DrawString(a, font, text, Width / 2 - 70, y);
                    g.FillEllipse(red, Width / 2 + 20, y + 4, 10, 10);
                    g.DrawString(b, font, text, Width / 2 + 36, y);
                }
            }

            private void DrawDonutLegend(Graphics g, int top)
            {
                using (var font = new Font("Segoe UI", 8F))
                using (var text = new SolidBrush(Color.FromArgb(17, 24, 39)))
                {
                    var y = top;
                    for (var i = 0; i < _points.Count && i < 5; i++)
                    {
                        using (var brush = new SolidBrush(_palette[i % _palette.Length]))
                            g.FillEllipse(brush, 18, y + 4, 9, 9);
                        g.DrawString(TrimLabel(_points[i].Label, 24) + " - " + _points[i].Value.ToString("N0"), font, text, 34, y);
                        y += 18;
                    }
                }
            }

            private void DrawEmpty(Graphics g)
            {
                using (var font = new Font("Segoe UI", 10F))
                using (var text = new SolidBrush(Color.FromArgb(107, 114, 128)))
                    g.DrawString("Chưa có dữ liệu", font, text, 18, 20);
            }

            private void SmartChartPanel_MouseMove(object sender, MouseEventArgs e)
            {
                if (_mode != "donut" || _donutSlices.Count == 0) return;
                var index = HitTestDonut(e.Location);
                if (index == _activeSlice) return;
                _activeSlice = index;
                if (index < 0)
                {
                    _tooltip.Hide(this);
                    return;
                }

                var slice = _donutSlices.First(s => s.Index == index);
                _tooltip.Show(slice.Text, this, e.Location.X + 14, e.Location.Y + 14, 2500);
            }

            private int HitTestDonut(Point point)
            {
                foreach (var slice in _donutSlices)
                {
                    var cx = slice.Rect.Left + slice.Rect.Width / 2f;
                    var cy = slice.Rect.Top + slice.Rect.Height / 2f;
                    var dx = point.X - cx;
                    var dy = point.Y - cy;
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    var outer = slice.Rect.Width / 2f;
                    if (distance < slice.InnerRadius || distance > outer) continue;

                    var angle = Math.Atan2(dy, dx) * 180d / Math.PI;
                    angle = (angle + 360d) % 360d;
                    var start = (slice.StartAngle + 360d) % 360d;
                    var end = (start + slice.SweepAngle) % 360d;
                    var inside = slice.SweepAngle >= 360 ||
                                 (start <= end ? angle >= start && angle <= end : angle >= start || angle <= end);
                    if (inside) return slice.Index;
                }
                return -1;
            }

            private class DonutSlice
            {
                public int Index { get; set; }
                public Rectangle Rect { get; set; }
                public float StartAngle { get; set; }
                public float SweepAngle { get; set; }
                public float InnerRadius { get; set; }
                public string Text { get; set; }
            }

            private static string TrimLabel(string label, int max)
            {
                if (string.IsNullOrEmpty(label) || label.Length <= max) return label;
                return label.Substring(0, max - 3) + "...";
            }
        }
    }
}
