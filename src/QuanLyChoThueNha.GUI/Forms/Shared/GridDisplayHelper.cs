using System.Drawing;
using System.Windows.Forms;

namespace QuanLyChoThueNha.GUI.Forms.Shared
{
    internal static class GridDisplayHelper
    {
        public static void AddTextColumn(DataGridView grid, string name, string headerText, int index, int fillWeight = 120)
        {
            if (grid.Columns.Contains(name)) return;

            var column = new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = headerText,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = fillWeight,
                MinimumWidth = 90
            };

            grid.Columns.Insert(System.Math.Min(index, grid.Columns.Count), column);
        }

        public static void HideColumn(DataGridView grid, string name)
        {
            if (grid.Columns.Contains(name))
                grid.Columns[name].Visible = false;
        }

        public static void SetFillWeight(DataGridView grid, string name, int fillWeight, int minimumWidth = 90)
        {
            if (!grid.Columns.Contains(name)) return;
            grid.Columns[name].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns[name].FillWeight = fillWeight;
            grid.Columns[name].MinimumWidth = minimumWidth;
        }

        public static void SetMoneyColumn(DataGridView grid, string name)
        {
            if (!grid.Columns.Contains(name)) return;
            grid.Columns[name].DefaultCellStyle.Format = "N0";
            grid.Columns[name].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        public static void BalanceGrid(DataGridView grid)
        {
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.RowTemplate.Height = 30;
        }
    }
}
