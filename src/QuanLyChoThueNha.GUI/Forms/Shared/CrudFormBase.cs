using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.GUI.Helpers;

namespace QuanLyChoThueNha.GUI.Forms.Shared
{
    public class FieldDefinition
    {
        public FieldDefinition(string propertyName, string caption, Type valueType = null,
            bool readOnly = false, string[] options = null, bool multiline = false,
            IEnumerable<ComboOption> lookupOptions = null, bool addOnly = false)
        {
            PropertyName = propertyName;
            Caption = caption;
            ValueType = valueType;
            ReadOnly = readOnly;
            Options = options;
            Multiline = multiline;
            LookupOptions = lookupOptions == null ? null : lookupOptions.ToList();
            AddOnly = addOnly;
        }

        public string PropertyName { get; private set; }
        public string Caption { get; private set; }
        public Type ValueType { get; private set; }
        public bool ReadOnly { get; private set; }
        public string[] Options { get; private set; }
        public bool Multiline { get; private set; }
        public IList<ComboOption> LookupOptions { get; private set; }
        public bool AddOnly { get; private set; }

        public static FieldDefinition Lookup(string propertyName, string caption,
            IEnumerable<ComboOption> options, bool readOnly = false)
        {
            return new FieldDefinition(propertyName, caption, typeof(string), readOnly, null, false, options);
        }
    }

    public class ComboOption
    {
        public ComboOption(string value, string display)
        {
            Value = value;
            Display = string.IsNullOrWhiteSpace(display) ? value : display;
        }

        public string Value { get; private set; }
        public string Display { get; private set; }

        public override string ToString()
        {
            return Display;
        }
    }

    public abstract class CrudFormBase<T> : MaterialForm where T : class, new()
    {
        private enum FormMode { View, Adding, Editing }

        private readonly List<FieldDefinition> _fields;
        private readonly Dictionary<string, Control> _editors = new Dictionary<string, Control>();
        private readonly FlowLayoutPanel _commandPanel = new FlowLayoutPanel();
        private readonly ErrorProvider _errorProvider = new ErrorProvider();
        protected readonly DataGridView Grid = new DataGridView();
        protected readonly TextBox TxtSearch = new PlaceholderTextBox();
        protected readonly Label LblStatus = new Label();
        protected readonly FlowLayoutPanel FiltersPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0),
            Padding = new Padding(0),
            AutoSize = true
        };

        private FormMode _mode = FormMode.View;
        private RoundedButton _btnAdd;
        private RoundedButton _btnUpdate;
        private RoundedButton _btnDelete;
        private RoundedButton _btnRefresh;
        private Label _lblError;

        protected CrudFormBase(string title, IEnumerable<FieldDefinition> fields)
        {
            Text = title;
            Size = new Size(1180, 680);
            StartPosition = FormStartPosition.CenterScreen;
            _fields = fields.ToList();
            _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            BuildLayout();
            Load += delegate { ReloadData(); EnterAddMode(); };
        }

        protected abstract IEnumerable<T> GetItems();
        protected abstract bool AddItem(T item, out string error);
        protected abstract bool UpdateItem(T item, out string error);
        protected abstract bool DeleteItem(T item, out string error);

        protected virtual void AfterGridBound() { }

        protected virtual IEnumerable<string> GridColumnNames()
        {
            return _fields.Select(f => f.PropertyName);
        }

        // Goi khi vao trang thai Add (sau save thanh cong, khi bam "Them", khi form load).
        // Override de reset ma tu sinh hoac dien san cac truong co dinh.
        protected virtual void OnAfterAdd() { }
        protected virtual void OnAfterEdit() { }

        protected void AddCommandButton(string text, EventHandler handler)
        {
            var btn = CreateButton(text);
            btn.Click += handler;
            _commandPanel.Controls.Add(btn);
        }

        protected void AddCommandControl(Control control)
        {
            if (control == null) return;
            _commandPanel.Controls.Add(control);
        }

        protected void AddSearchFilter(string headerText, ComboBox comboBox, int width = 140)
        {
            var filterBox = new RoundedPanel
            {
                Width = width,
                Height = 60,
                Radius = 12,
                BorderColor = Color.FromArgb(226, 232, 240),
                Padding = new Padding(10, 4, 10, 6),
                BackColor = Color.White,
                Margin = new Padding(8, 0, 0, 0)
            };

            var layout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                RowCount = 2, 
                ColumnCount = 1,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = headerText,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0)
            };

            comboBox.Dock = DockStyle.Fill;
            comboBox.Font = new Font("Segoe UI", 9.5F);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = Color.White;
            comboBox.Margin = new Padding(0, 2, 0, 0);

            layout.Controls.Add(label, 0, 0);
            layout.Controls.Add(comboBox, 0, 1);
            filterBox.Controls.Add(layout);

            FiltersPanel.Controls.Add(filterBox);
        }

        protected void HideDeleteButton()
        {
            if (_btnDelete != null)
                _btnDelete.Visible = false;
        }

        protected T CurrentItem
        {
            get
            {
                if (Grid.CurrentRow == null) return null;
                return Grid.CurrentRow.DataBoundItem as T;
            }
        }

        protected void ReloadData()
        {
            try
            {
                GridFormatterHelper.ClearNamesCache();
                var items = GetItems().ToList();
                var keyword = TextFormatHelper.NormalizeSearch(TxtSearch.Text);
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    items = items.Where(x => _fields.Any(f =>
                    {
                        var value = GetProperty(f.PropertyName).GetValue(x, null);
                        return value != null && TextFormatHelper.ContainsNormalized(value.ToString(), keyword);
                    })).ToList();
                }

                Grid.DataSource = new BindingList<T>(items);
                AfterGridBound();
                LblStatus.Text = string.Format("So dong: {0}", items.Count);
            }
            catch (Exception ex)
            {
                Grid.DataSource = null;
                LblStatus.Text = "Khong tai du lieu: " + ex.Message;
            }
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(12, 18, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            var searchBox = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                BorderColor = Color.FromArgb(226, 232, 240),
                Padding = new Padding(12, 4, 12, 6),
                BackColor = Color.White
            };
            var searchLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.White };
            searchLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            searchLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            searchLayout.Controls.Add(new Label
            {
                Text = "Tìm kiếm",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            TxtSearch.Dock = DockStyle.Fill;
            TxtSearch.BorderStyle = BorderStyle.None;
            TxtSearch.Font = new Font("Segoe UI", 10.5F);
            TxtSearch.BackColor = Color.White;
            var placeholder = TxtSearch as PlaceholderTextBox;
            if (placeholder != null) placeholder.Placeholder = "Nhập từ khóa tìm kiếm...";
            TxtSearch.TextChanged += delegate { ReloadData(); };
            searchLayout.Controls.Add(TxtSearch, 0, 1);
            searchBox.Controls.Add(searchLayout);

            var topSearchPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            topSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            topSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topSearchPanel.Controls.Add(searchBox, 0, 0);
            topSearchPanel.Controls.Add(FiltersPanel, 1, 0);
            left.Controls.Add(topSearchPanel, 0, 0);

            Grid.Dock = DockStyle.Fill;
            Grid.AutoGenerateColumns = false;
            Grid.AllowUserToAddRows = false;
            Grid.AllowUserToDeleteRows = false;
            Grid.ReadOnly = true;
            Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Grid.MultiSelect = false;
            Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Grid.BackgroundColor = Color.White;
            Grid.BorderStyle = BorderStyle.FixedSingle;
            Grid.EnableHeadersVisualStyles = false;
            Grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            Grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175);
            Grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            Grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            Grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
            Grid.RowTemplate.Height = 30;
            Grid.RowHeadersVisible = false;
            Grid.DataBindingComplete += delegate { Grid.ClearSelection(); };
            Grid.SelectionChanged += delegate { BindCurrentToInputs(); };
            TaoCotGrid();
            GridFormatterHelper.SetupCellFormatting(Grid);
            left.Controls.Add(Grid, 0, 1);

            LblStatus.Dock = DockStyle.Fill;
            LblStatus.TextAlign = ContentAlignment.MiddleLeft;
            left.Controls.Add(LblStatus, 0, 2);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            right.TabStop = true;
            right.MouseEnter += delegate { right.Focus(); };

            foreach (var field in _fields)
            {
                right.Controls.Add(new Label
                {
                    Text = field.Caption,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Margin = new Padding(0, 6, 0, 2)
                });

                var editor = CreateEditor(field);
                _editors[field.PropertyName] = editor;
                editor.MouseEnter += delegate { right.Focus(); };
                right.Controls.Add(editor);
            }

            _commandPanel.Dock = DockStyle.Top;
            _commandPanel.AutoSize = true;
            _commandPanel.WrapContents = true;
            _commandPanel.Margin = new Padding(0, 12, 0, 0);

            _btnAdd     = CreateButton("Thêm",    BtnAdd_Click);
            _btnUpdate  = CreateButton("Sửa",     BtnUpdate_Click);
            _btnDelete  = CreateButton("Xóa",     BtnDelete_Click);
            _btnRefresh = CreateButton("Làm mới", BtnRefresh_Click);
            _commandPanel.Controls.Add(_btnAdd);
            _commandPanel.Controls.Add(_btnUpdate);
            _commandPanel.Controls.Add(_btnDelete);
            _commandPanel.Controls.Add(_btnRefresh);
            UpdateButtonStyles();

            right.Controls.Add(_commandPanel);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);

            _lblError = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                Font = new Font("Segoe UI", 9f),
                Visible = false
            };

            Controls.Add(root);
            Controls.Add(_lblError);
        }

        private Control CreateEditor(FieldDefinition field)
        {
            var type = Nullable.GetUnderlyingType(field.ValueType ?? GetProperty(field.PropertyName).PropertyType)
                ?? (field.ValueType ?? GetProperty(field.PropertyName).PropertyType);

            Control ctrl;

            if (field.LookupOptions != null && field.LookupOptions.Count > 0)
            {
                var combo = new ComboBox
                {
                    Dock = DockStyle.Top,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Height = 32,
                    Enabled = false,
                    DisplayMember = "Display",
                    ValueMember = "Value"
                };
                combo.DataSource = field.LookupOptions.ToList();
                ctrl = combo;
            }
            else if (field.Options != null && field.Options.Length > 0)
            {
                var combo = new ComboBox
                {
                    Dock = DockStyle.Top,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Height = 32,
                    Enabled = false
                };
                combo.Items.AddRange(field.Options);
                if (combo.Items.Count > 0) combo.SelectedIndex = 0;
                ctrl = combo;
            }
            else if (type == typeof(bool))
            {
                ctrl = new CheckBox { Dock = DockStyle.Top, Height = 28, Enabled = false };
            }
            else if (type == typeof(DateTime))
            {
                ctrl = new DateTimePicker
                {
                    Dock = DockStyle.Top,
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "dd/MM/yyyy",
                    ShowCheckBox = Nullable.GetUnderlyingType(GetProperty(field.PropertyName).PropertyType) != null,
                    Enabled = false
                };
            }
            else if (type == typeof(int) || type == typeof(decimal) || type == typeof(float) || type == typeof(double))
            {
                var num = new KeyboardOnlyNumericUpDown
                {
                    Dock = DockStyle.Top,
                    Maximum = 1000000000000,
                    Minimum = -1000000000000,
                    DecimalPlaces = type == typeof(int) ? 0 : 2,
                    ThousandsSeparator = true,
                    Enabled = false
                };
                ctrl = num;
            }
            else
            {
                ctrl = new TextBox
                {
                    Dock = DockStyle.Top,
                    ReadOnly = true,
                    Multiline = field.Multiline,
                    UseSystemPasswordChar = field.PropertyName.IndexOf("MatKhau", StringComparison.OrdinalIgnoreCase) >= 0,
                    Height = field.Multiline ? 66 : 28,
                    ScrollBars = field.Multiline ? ScrollBars.Vertical : ScrollBars.None
                };
            }

            ctrl.Font = new Font("Segoe UI", 10F);
            return ctrl;
        }

        private void TaoCotGrid()
        {
            Grid.Columns.Clear();
            var fieldByName = _fields.ToDictionary(f => f.PropertyName);
            foreach (var propertyName in GridColumnNames())
            {
                FieldDefinition field;
                if (!fieldByName.TryGetValue(propertyName, out field)) continue;

                var headerText = field.Caption;
                if (field.PropertyName == "MaCanHo" || field.PropertyName == "Phong")
                {
                    headerText = "Tên căn hộ";
                }
                else if (field.PropertyName == "MaToa" || field.PropertyName == "Toa")
                {
                    headerText = "Tên tòa";
                }
                else if (field.PropertyName == "MaKhach" || field.PropertyName == "KhachThue")
                {
                    headerText = "Tên khách";
                }

                var column = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = field.PropertyName,
                    Name = field.PropertyName,
                    HeaderText = headerText,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    MinimumWidth = 90
                };

                var type = Nullable.GetUnderlyingType(field.ValueType ?? GetProperty(field.PropertyName).PropertyType)
                    ?? (field.ValueType ?? GetProperty(field.PropertyName).PropertyType);
                if (type == typeof(DateTime))
                    column.DefaultCellStyle.Format = "dd/MM/yyyy";
                else if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
                {
                    column.DefaultCellStyle.Format = "N0";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                Grid.Columns.Add(column);
            }
        }

        private RoundedButton CreateButton(string text, EventHandler handler = null)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Width = 90,
                Height = 36,
                Radius = 10,
                Margin = new Padding(0, 4, 6, 4)
            };
            if (handler != null) btn.Click += handler;
            return btn;
        }

        // ── Mode management ──────────────────────────────────────────────────────

        private void EnterAddMode()
        {
            _mode = FormMode.Adding;
            HideMessage();
            ClearInputs();               // xoa trang, bo chon grid
            SetEditorsEnabled(true);     // bat cac truong co the sua
            _btnAdd.Text     = "Lưu";    _btnAdd.Enabled    = true;
            _btnUpdate.Text  = "Sửa";    _btnUpdate.Enabled = false;
            _btnDelete.Enabled = false;
            _btnRefresh.Text = "Làm mới";
            UpdateButtonStyles();
            OnAfterAdd();               // reset ma tu sinh, dien san truong co dinh
        }

        private void EnterViewMode()
        {
            _mode = FormMode.View;
            HideMessage();
            SetEditorsEnabled(false);   // tat het (chi xem)
            _btnAdd.Text     = "Thêm";  _btnAdd.Enabled    = true;
            _btnUpdate.Text  = "Sửa";   _btnUpdate.Enabled = true;
            _btnDelete.Enabled = true;
            _btnRefresh.Text = "Làm mới";
            UpdateButtonStyles();
        }

        private void EnterEditMode()
        {
            _mode = FormMode.Editing;
            HideMessage();
            SetEditorsEnabled(true);    // bat cac truong duoc phep sua
            _btnAdd.Text     = "Thêm";  _btnAdd.Enabled    = false;
            _btnUpdate.Text  = "Lưu";   _btnUpdate.Enabled = true;
            _btnDelete.Enabled = false;
            _btnRefresh.Text = "Hủy";
            UpdateButtonStyles();
            OnAfterEdit();
        }

        // Bat/tat cac editor KHONG ReadOnly theo mode.
        // Truong ReadOnly luon disabled (chi hien thi).
        private void SetEditorsEnabled(bool enabled)
        {
            foreach (var field in _fields)
            {
                if (field.ReadOnly) continue;
                var editor = _editors[field.PropertyName];
                var fieldEnabled = enabled && (!field.AddOnly || _mode == FormMode.Adding);
                if (editor is TextBox)
                    ((TextBox)editor).ReadOnly = !fieldEnabled;
                else
                    editor.Enabled = fieldEnabled;
            }
        }

        // ── Button handlers ──────────────────────────────────────────────────────

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View || _mode == FormMode.Editing)
            {
                EnterAddMode();
                return;
            }
            // Adding mode -> luu ban ghi moi
            var item = ReadInputs(new T(), false);
            if (!ValidateBeforeSave(item)) return;
            string error;
            try
            {
                if (!AddItem(item, out error)) { ShowError(error); return; }
            }
            catch (Exception ex) { ShowError(LayLoiSauCung(ex)); return; }
            ReloadData();
            EnterAddMode();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View)
            {
                if (CurrentItem == null) { ShowError("Chon dong can sua."); return; }
                EnterEditMode();
                return;
            }
            if (_mode == FormMode.Editing)
            {
                var item = CurrentItem;
                if (item == null) { ShowError("Mat lua chon. Bam 'Huy' va chon lai."); return; }
                ReadInputs(item, true);
                if (!ValidateBeforeSave(item)) return;
                string error;
                try
                {
                    if (!UpdateItem(item, out error)) { ShowError(error); return; }
                }
                catch (Exception ex) { ShowError(LayLoiSauCung(ex)); return; }
                ReloadData();
                EnterAddMode();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var item = CurrentItem;
            if (item == null) { ShowError("Chon dong can xoa."); return; }
            if (MessageBox.Show("Xoa ban ghi dang chon?", "Xac nhan", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
            string error;
            try
            {
                if (!DeleteItem(item, out error)) { ShowError(error); return; }
            }
            catch (Exception ex) { ShowError(LayLoiSauCung(ex)); return; }
            ReloadData();
            EnterAddMode();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.Editing)
            {
                // Huy sua: khoi phuc du lieu goc, quay ve xem
                var item = CurrentItem;
                if (item != null) BindRowToInputs(item);
                EnterViewMode();
                return;
            }
            OnRefreshRequested();
        }

        protected virtual void OnRefreshRequested()
        {
            ReloadData();
            EnterAddMode();
        }

        // ── Data binding ─────────────────────────────────────────────────────────

        private void BindCurrentToInputs()
        {
            var item = CurrentItem;
            if (item == null) return;   // khong co lua chon, giu nguyen mode
            BindRowToInputs(item);
            EnterViewMode();            // co row duoc chon -> che do xem
        }

        private void BindRowToInputs(T item)
        {
            foreach (var field in _fields)
            {
                var property = GetProperty(field.PropertyName);
                var value = property.GetValue(item, null);
                var editor = _editors[field.PropertyName];

                if (editor is ComboBox)
                {
                    var combo = (ComboBox)editor;
                    if (combo.DataSource != null && value != null)
                        combo.SelectedValue = value.ToString();
                    else
                        combo.SelectedItem = value == null ? null : value.ToString();
                }
                else if (editor is CheckBox)
                {
                    ((CheckBox)editor).Checked = value != null && (bool)value;
                }
                else if (editor is DateTimePicker)
                {
                    var picker = (DateTimePicker)editor;
                    if (value == null) picker.Checked = false;
                    else
                    {
                        picker.Checked = true;
                        SetDatePickerValueSafe(picker, (DateTime)value);
                    }
                }
                else if (editor is NumericUpDown)
                {
                    ((NumericUpDown)editor).Value = value == null ? 0 : Convert.ToDecimal(value);
                }
                else
                {
                    ((TextBox)editor).Text = value == null ? string.Empty : value.ToString();
                }
            }
        }

        private static void SetDatePickerValueSafe(DateTimePicker picker, DateTime value)
        {
            var date = value.Date;
            if (date < picker.MinDate)
                picker.MinDate = date;
            if (date > picker.MaxDate)
                picker.MaxDate = date;
            picker.Value = date;
        }

        private T ReadInputs(T item, bool updateMode)
        {
            foreach (var field in _fields)
            {
                if (field.ReadOnly) continue;   // luon bo qua truong ReadOnly
                if (updateMode && field.AddOnly) continue;
                var property = GetProperty(field.PropertyName);
                var value = ReadEditorValue(field, property.PropertyType);
                property.SetValue(item, value, null);
            }
            return item;
        }

        private object ReadEditorValue(FieldDefinition field, Type propertyType)
        {
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var editor = _editors[field.PropertyName];

            if (editor is ComboBox)
            {
                var combo = (ComboBox)editor;
                if (combo.SelectedItem is ComboOption)
                    return ((ComboOption)combo.SelectedItem).Value;
                return combo.SelectedItem == null ? null : combo.SelectedItem.ToString();
            }
            if (editor is CheckBox) return ((CheckBox)editor).Checked;
            if (editor is DateTimePicker)
            {
                var picker = (DateTimePicker)editor;
                if (Nullable.GetUnderlyingType(propertyType) != null && !picker.Checked) return null;
                return picker.Value.Date;
            }
            if (editor is NumericUpDown)
            {
                var value = ((NumericUpDown)editor).Value;
                if (type == typeof(int)) return Convert.ToInt32(value);
                if (type == typeof(float)) return Convert.ToSingle(value);
                if (type == typeof(double)) return Convert.ToDouble(value);
                return value;
            }
            return ((TextBox)editor).Text.Trim();
        }

        private void ClearInputs()
        {
            foreach (var field in _fields)
            {
                var editor = _editors[field.PropertyName];
                if (editor is TextBox) ((TextBox)editor).Clear();
                else if (editor is NumericUpDown) ((NumericUpDown)editor).Value = 0;
                else if (editor is CheckBox) ((CheckBox)editor).Checked = false;
                else if (editor is DateTimePicker)
                {
                    var picker = (DateTimePicker)editor;
                    picker.Value = DateTime.Today;
                    picker.Checked = !picker.ShowCheckBox;
                }
                else if (editor is ComboBox && ((ComboBox)editor).Items.Count > 0)
                    ((ComboBox)editor).SelectedIndex = 0;
            }
            Grid.ClearSelection();
            ClearErrors();
        }

        private PropertyInfo GetProperty(string propertyName)
        {
            return typeof(T).GetProperty(propertyName);
        }

        protected void ShowError(string error)
        {
            _lblError.Text = string.IsNullOrWhiteSpace(error) ? "Thao tac khong thanh cong." : error;
            _lblError.BackColor = Color.MistyRose;
            _lblError.ForeColor = Color.DarkRed;
            _lblError.Visible = true;
        }

        protected void ShowInfo(string msg)
        {
            _lblError.Text = msg;
            _lblError.BackColor = Color.FromArgb(220, 240, 255);
            _lblError.ForeColor = Color.Navy;
            _lblError.Visible = true;
        }

        private void HideMessage() { _lblError.Visible = false; }

        private static string LayLoiSauCung(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }

        protected Control GetEditor(string propertyName)
        {
            return _editors.ContainsKey(propertyName) ? _editors[propertyName] : null;
        }

        protected void SetEditorValue(string propertyName, object value)
        {
            var editor = GetEditor(propertyName);
            if (editor == null) return;

            if (editor is TextBox) ((TextBox)editor).Text = value == null ? string.Empty : value.ToString();
            else if (editor is NumericUpDown) ((NumericUpDown)editor).Value = value == null ? 0 : Convert.ToDecimal(value);
            else if (editor is CheckBox) ((CheckBox)editor).Checked = value != null && Convert.ToBoolean(value);
            else if (editor is ComboBox)
            {
                var combo = (ComboBox)editor;
                if (combo.DataSource != null && value != null) combo.SelectedValue = value.ToString();
                else combo.SelectedItem = value == null ? null : value.ToString();
            }
        }

        protected void ClearFormInputs()
        {
            ClearInputs();
        }

        // Cho phep subclass kich hoat Add mode (VD: nut "Tao moi" tuy chinh).
        protected void GoToAddMode()
        {
            EnterAddMode();
        }

        private bool ValidateBeforeSave(T item)
        {
            ClearErrors();

            var context = new ValidationContext(item, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(item, context, results, true);
            results = results
                .Where(r => !r.MemberNames.Any() || r.MemberNames.Any(m => _editors.ContainsKey(m)))
                .ToList();

            if (results.Count == 0) return true;

            foreach (var result in results)
            {
                var member = result.MemberNames.FirstOrDefault();
                if (!string.IsNullOrEmpty(member) && _editors.ContainsKey(member))
                    _errorProvider.SetError(_editors[member], result.ErrorMessage);
            }

            ShowError(string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage)));
            return false;
        }

        private void ClearErrors()
        {
            foreach (var editor in _editors.Values)
                _errorProvider.SetError(editor, string.Empty);
        }

        private void UpdateButtonStyles()
        {
            ApplyButtonStyle(_btnAdd);
            ApplyButtonStyle(_btnUpdate);
            ApplyButtonStyle(_btnDelete);
            ApplyButtonStyle(_btnRefresh);
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
            else // "Làm mới" or other
            {
                btn.BackColor = Color.FromArgb(107, 114, 128); // gray-500
                btn.BorderColor = Color.FromArgb(75, 85, 99); // gray-600
                btn.ForeColor = Color.White;
            }
        }
    }
}
