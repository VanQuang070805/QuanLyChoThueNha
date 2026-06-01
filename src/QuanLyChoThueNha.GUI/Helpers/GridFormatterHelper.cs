using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Helpers
{
    public static class GridFormatterHelper
    {
        private static readonly object _cacheLock = new object();
        private static Dictionary<string, string> _toaNames = new Dictionary<string, string>();
        private static Dictionary<string, string> _canHoNames = new Dictionary<string, string>();
        private static bool _cacheLoaded = false;

        public static void LoadNamesCache()
        {
            lock (_cacheLock)
            {
                if (_cacheLoaded) return;
                try
                {
                    var toaService = new ToaService();
                    var toas = toaService.LayTatCa().ToList();
                    _toaNames = toas.ToDictionary(t => t.MaToa, t => t.TenToa);

                    var canHoService = new CanHoService();
                    var canHos = canHoService.LayTatCa().ToList();
                    _canHoNames = canHos.ToDictionary(c => c.MaCanHo, c => string.Format("Căn {0}", c.SoCanHo));
                    _cacheLoaded = true;
                }
                catch
                {
                    // Fallback
                }
            }
        }

        public static void ClearNamesCache()
        {
            lock (_cacheLock)
            {
                _toaNames.Clear();
                _canHoNames.Clear();
                _cacheLoaded = false;
            }
        }

        public static string GetCanHoName(string maCanHo)
        {
            LoadNamesCache();
            string name;
            return _canHoNames.TryGetValue(maCanHo ?? string.Empty, out name) ? name : maCanHo;
        }

        public static string GetToaName(string maToa)
        {
            LoadNamesCache();
            string name;
            return _toaNames.TryGetValue(maToa ?? string.Empty, out name) ? name : maToa;
        }

        public static void SetupCellFormatting(DataGridView dgv)
        {
            dgv.CellFormatting += (sender, e) =>
            {
                if (e.Value == null) return;
                var colName = dgv.Columns[e.ColumnIndex].Name;

                if (colName == "MaCanHo" || colName == "Phong")
                {
                    string val = e.Value.ToString();
                    if (!string.IsNullOrEmpty(val))
                    {
                        e.Value = GetCanHoName(val);
                        e.FormattingApplied = true;
                    }
                }
                else if (colName == "MaToa" || colName == "Toa")
                {
                    string val = e.Value.ToString();
                    if (!string.IsNullOrEmpty(val))
                    {
                        e.Value = GetToaName(val);
                        e.FormattingApplied = true;
                    }
                }
            };
        }
    }
}
