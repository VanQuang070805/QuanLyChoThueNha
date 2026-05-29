using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;

namespace QuanLyChoThueNha.GUI.Forms.BaoCao
{
    public class frmBaoCao : MaterialForm
    {
        private readonly BaoCaoService _service = new BaoCaoService();
        private readonly NumericUpDown _numNam = new NumericUpDown();
        private readonly DataGridView _grid = new DataGridView();

        public frmBaoCao()
        {
            Text = "Bao cao";
            Size = new Size(1100, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BuildLayout();
            Load += delegate { LoadReport(); };
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

            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            toolbar.Controls.Add(new Label { Text = "Nam", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 4, 0) });
            _numNam.Minimum = 2020;
            _numNam.Maximum = 2100;
            _numNam.Value = DateTime.Today.Year;
            _numNam.Width = 90;
            toolbar.Controls.Add(_numNam);

            var btnXem = new MaterialButton { Text = "Xem", AutoSize = true };
            btnXem.Click += delegate { LoadReport(); };
            toolbar.Controls.Add(btnXem);

            var btnExcel = new MaterialButton { Text = "Xuat Excel", AutoSize = true };
            btnExcel.Click += BtnExcel_Click;
            toolbar.Controls.Add(btnExcel);

            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            root.Controls.Add(toolbar, 0, 0);
            root.Controls.Add(_grid, 0, 1);
            Controls.Add(root);
        }

        private void LoadReport()
        {
            try
            {
                _grid.DataSource = _service.DoanhThuTheoThang((int)_numNam.Value).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong tai bao cao: " + ex.Message, "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            var data = _service.DoanhThuTheoThang((int)_numNam.Value).ToList();
            using (var dialog = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = "BaoCaoDoanhThu.xlsx" })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Doanh thu");
                    ws.Cell(1, 1).Value = "Thang";
                    ws.Cell(1, 2).Value = "Tong thu";
                    ws.Cell(1, 3).Value = "So hoa don";
                    ws.Row(1).Style.Font.Bold = true;

                    for (int i = 0; i < data.Count; i++)
                    {
                        ws.Cell(i + 2, 1).Value = data[i].Thang;
                        ws.Cell(i + 2, 2).Value = data[i].TongThu;
                        ws.Cell(i + 2, 3).Value = data[i].SoHoaDon;
                    }
                    ws.Columns().AdjustToContents();
                    wb.SaveAs(dialog.FileName);
                }
            }
            MessageBox.Show("Da xuat Excel.", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
