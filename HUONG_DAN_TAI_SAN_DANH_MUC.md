# Hướng dẫn học & báo cáo: Phần Tài sản & Danh mục

> Tài liệu này giải thích chi tiết từng đoạn code cho phần quản lý tài sản và danh mục trong dự án **QuanLyChoThueNha**.

---

## MỤC LỤC

1. [Tổng quan kiến trúc 3 lớp](#1-tổng-quan-kiến-trúc-3-lớp)
2. [Model Entities — Các bảng dữ liệu](#2-model-entities---các-bảng-dữ-liệu)
3. [DAL — Unit of Work & Repository](#3-dal---unit-of-work--repository)
4. [BLL — BaseService & các Helper](#4-bll---baseservice--các-helper)
5. [CrudFormBase — Mẫu Form CRUD chung](#5-crudformbase---mẫu-form-crud-chung)
6. [frmKhuVuc — Quản lý Khu vực](#6-frmkhuvuc---quản-lý-khu-vực)
7. [frmToa — Quản lý Tòa nhà](#7-frmtoa---quản-lý-tòa-nhà)
8. [frmLoaiCanHo — Quản lý Loại căn hộ](#8-frmloaicanho---quản-lý-loại-căn-hộ)
9. [frmTienNghi — Quản lý Tiện nghi](#9-frmtiennghi---quản-lý-tiện-nghi)
10. [frmGiaDichVu — Quản lý Giá dịch vụ](#10-frmgiadichvu---quản-lý-giá-dịch-vụ)
11. [frmCanHo — Quản lý Căn hộ (Form phức tạp nhất)](#11-frmcanho---quản-lý-căn-hộ)
12. [Luồng dữ liệu tổng quát](#12-luồng-dữ-liệu-tổng-quát)
13. [Câu hỏi báo cáo thường gặp](#13-câu-hỏi-báo-cáo-thường-gặp)

---

## 1. Tổng quan kiến trúc 3 lớp

Toàn bộ dự án được chia thành **3 project (layer)**:

```
┌─────────────────────────────────────────────────────┐
│  GUI  (QuanLyChoThueNha.GUI)                        │
│  Forms/TaiSan/frmCanHo.cs, frmKhuVuc.cs, ...       │
│  → Chịu trách nhiệm hiển thị, nhận sự kiện người dùng
└────────────────────┬────────────────────────────────┘
                     │ gọi Service
┌────────────────────▼────────────────────────────────┐
│  BLL  (QuanLyChoThueNha.BLL)                        │
│  Services/CanHoService.cs, KhuVucService.cs, ...   │
│  → Xử lý nghiệp vụ, kiểm tra dữ liệu, tạo mã      │
└────────────────────┬────────────────────────────────┘
                     │ gọi Repository qua UnitOfWork
┌────────────────────▼────────────────────────────────┐
│  DAL  (QuanLyChoThueNha.DAL)                        │
│  Repositories/UnitOfWork.cs, Repository.cs         │
│  → Trực tiếp đọc/ghi vào SQL Server bằng EF6       │
└─────────────────────────────────────────────────────┘
         ↕  Entity Framework 6 (Code First)
┌─────────────────────────────────────────────────────┐
│  Model  (QuanLyChoThueNha.Model)                    │
│  Entities/CanHo.cs, KhuVuc.cs, Toa.cs, ...        │
│  → Các class ánh xạ trực tiếp với bảng SQL         │
└─────────────────────────────────────────────────────┘
```

**Ý nghĩa:**
- **GUI** không được phép truy cập database trực tiếp — chỉ gọi BLL.
- **BLL** không biết giao diện — chỉ nhận dữ liệu, kiểm tra, rồi gọi DAL.
- **DAL** không biết nghiệp vụ — chỉ biết cách lưu/đọc Entity.

---

## 2. Model Entities — Các bảng dữ liệu

### 2.1 CanHo.cs
```csharp
// File: src/QuanLyChoThueNha.Model/Entities/CanHo.cs

[Table("CanHo")]               // → ánh xạ với bảng CanHo trong SQL Server
public class CanHo
{
    [Key, Column("MaCanHo"), MaxLength(50)]
    public string MaCanHo { get; set; }       // Khóa chính, VD: CH001

    [Required, MaxLength(50)]
    public string MaToa { get; set; }         // Khóa ngoại → bảng Toa

    [Required, MaxLength(50)]
    public string MaLoai { get; set; }        // Khóa ngoại → bảng LoaiCanHo

    [MaxLength(50)]
    public string MaNhanVien { get; set; }    // Nhân viên quản lý căn hộ này

    [MaxLength(50)]
    public string MaNguoiThaoTac { get; set; }   // Audit: ai đã thao tác cuối

    [MaxLength(50)]
    public string VaiTroNguoiThaoTac { get; set; } // Audit: vai trò người thao tác

    public double DienTich { get; set; }           // m²

    [Column(TypeName = "decimal")]
    public decimal GiaThueNiemYet { get; set; }    // Giá thuê gốc (niêm yết)

    [Column(TypeName = "decimal")]
    public decimal TienCocNiemYet { get; set; }    // Tiền cọc gốc

    public int SoCanHo { get; set; }    // Số hiệu phòng (101, 202...)
    public int TangSo { get; set; }     // Tầng nào

    [Required, MaxLength(50)]
    public string TinhTrang { get; set; } = "Trong";
    // Tình trạng: "Trong" | "BaoTri" | "DaDatCoc" | "DangThue"
    // "DaDatCoc" và "DangThue" → hệ thống tự cập nhật, người dùng KHÔNG sửa được

    [MaxLength(1000)]
    public string MoTa { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;
}
```

**Điểm quan trọng:** `TinhTrang` có 4 giá trị. Trong đó `DaDatCoc` và `DangThue` là **trạng thái hệ thống** — được tự động cập nhật từ PhieuDatTruoc và HopDong, người dùng không thể tự gán.

### 2.2 KhuVuc.cs
```csharp
[Table("KhuVuc")]
public class KhuVuc
{
    [Key, Column("MaKhuVuc"), MaxLength(50)]
    public string MaKhuVuc { get; set; }   // VD: KV001

    [MaxLength(50)]
    public string MaAdmin { get; set; }    // Admin tạo khu vực này

    [Required, MaxLength(150)]
    public string TenKhuVuc { get; set; }  // VD: "Khu Cầu Giấy"

    [MaxLength(100)]
    public string Quan { get; set; }       // Quận/Huyện

    [MaxLength(100)]
    public string ThanhPho { get; set; }   // Thành phố

    public double? ViDo { get; set; }      // Latitude — nullable (có thể chưa có)
    public double? KinhDo { get; set; }    // Longitude
}
```

**Sơ đồ quan hệ (ERD rút gọn):**
```
KhuVuc (1) ──────── (N) Toa ──────────── (N) CanHo
                                              │
                               TienNghiCuaCanHo (bảng trung gian)
                                              │
                                        TienNghi
```

---

## 3. DAL — Unit of Work & Repository

### 3.1 Pattern Repository + Unit of Work

**Repository** bọc lấy một Entity, cung cấp các hàm: `GetAll()`, `GetById()`, `Find()`, `Add()`, `Update()`, `Remove()`.

**Unit of Work (UnitOfWork.cs)** giữ **một DbContext duy nhất**, cho phép nhiều Repository dùng chung một transaction:

```csharp
// File: src/QuanLyChoThueNha.DAL/Repositories/UnitOfWork.cs

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;  // Chỉ 1 DbContext duy nhất!

    // Lazy-init: repository chỉ được tạo khi lần đầu dùng tới
    private IRepository<KhuVuc> _khuVucs;
    public IRepository<KhuVuc> KhuVucs
        => _khuVucs ?? (_khuVucs = new Repository<KhuVuc>(_context));

    // Tương tự cho CanHos, Toas, TienNghis, GiaDichVus...

    // Lưu tất cả thay đổi vào DB (= SaveChanges)
    public int Complete() => _context.SaveChanges();

    // Hỗ trợ Transaction
    public void BeginTransaction()    => _transaction = _context.Database.BeginTransaction();
    public void CommitTransaction()   => _transaction?.Commit();
    public void RollbackTransaction() => _transaction?.Rollback();
}
```

**Tại sao dùng Unit of Work?**
→ Khi thêm CanHo + gán TienNghi cùng lúc, nếu bước 2 lỗi, bước 1 cũng bị rollback — toàn bộ là 1 atomic operation.

---

## 4. BLL — BaseService & các Helper

### 4.1 BaseService\<T\>

```csharp
// File: src/QuanLyChoThueNha.BLL/Services/BaseService.cs

public abstract class BaseService<T> where T : class
{
    protected readonly IUnitOfWork _uow;  // Dùng chung UoW

    protected BaseService()
    {
        _uow = new UnitOfWork();  // Mỗi Service tạo 1 UoW riêng
    }

    // Mỗi class con PHẢI khai báo nó thuộc Repository nào
    protected abstract IRepository<T> Repo { get; }

    // CRUD cơ bản — Service con có thể override nếu cần thêm nghiệp vụ
    public virtual T LayTheoMa(object ma)            => Repo.GetById(ma);
    public virtual IEnumerable<T> LayTatCa()         => Repo.GetAll();
    public virtual IEnumerable<T> Tim(Expression...)  => Repo.Find(dkLoc);
    public virtual void Them(T entity) { GanAudit(entity); Repo.Add(entity); _uow.Complete(); }
    public virtual void Sua(T entity)  { GanAudit(entity); Repo.Update(entity); _uow.Complete(); }
    public virtual void Xoa(T entity)  { Repo.Remove(entity); _uow.Complete(); }

    // Tự động gán MaNguoiThaoTac và VaiTroNguoiThaoTac nếu Entity có trường đó
    private static void GanAuditNeuCo(T entity)
    {
        if (!SessionContext.DaXacThuc) return;
        SetIfExists(entity, "MaNguoiThaoTac", SessionContext.MaNguoiDung);
        SetIfExists(entity, "VaiTroNguoiThaoTac", SessionContext.VaiTro);
    }
}
```

**Điểm quan trọng:** Kỹ thuật dùng **Reflection** (`typeof(T).GetProperty(...)`) để tự động gán Audit mà không cần viết thủ công cho từng entity.

### 4.2 MaGenerator — Tự động sinh mã khóa chính

```csharp
// File: src/QuanLyChoThueNha.BLL/Helpers/MaGenerator.cs

public static class MaGenerator
{
    // Sinh mã tiếp theo. VD: "CH" + max=5 → "CH006"
    public static string Sinh(string prefix, int currentMax, int padding = 3)
    {
        int next = currentMax + 1;
        return prefix + next.ToString("D" + padding); // "D3" = 3 chữ số có padding 0
    }

    // Lấy số thứ tự từ mã. VD: "CH007" → 7 (bỏ 2 ký tự prefix "CH")
    public static int LaySoThuTu(string ma, int prefixLength)
    {
        if (int.TryParse(ma.Substring(prefixLength), out int so))
            return so;
        return 0;
    }
}
```

**Cách dùng trong Service:**
```csharp
// Trong KhuVucService
private string SinhMa()
{
    int max = 0;
    foreach (var x in _uow.KhuVucs.GetAll())
        max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaKhuVuc, 2)); // 2 = độ dài "KV"
    return MaGenerator.Sinh("KV", max); // "KV001", "KV002"...
}
```

**Bảng prefix mã cho phần Tài sản:**

| Entity | Prefix | Ví dụ |
|--------|--------|-------|
| KhuVuc | KV | KV001 |
| Toa | TO | TO001 |
| CanHo | CH | CH001 |
| LoaiCanHo | LC | LC001 |
| TienNghi | TN | TN001 |
| GiaDichVu | GDV | GDV001 |
| HinhAnhNha | HA | HA001 |

### 4.3 ValidationHelper

```csharp
// File: src/QuanLyChoThueNha.BLL/Helpers/ValidationHelper.cs

// Kiểm tra không rỗng — dùng ở mọi nơi
public static bool KhongRong(string value, string tenTruong, out string loi)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        loi = $"{tenTruong} không được để trống.";
        return false;   // trả về false = có lỗi
    }
    loi = string.Empty;
    return true;        // trả về true = OK
}

// Kiểm tra số dương (cho giá tiền, diện tích)
public static bool SoDuong(decimal value, string tenTruong, out string loi)
{
    if (value <= 0) { loi = $"{tenTruong} phải là số dương."; return false; }
    loi = string.Empty;
    return true;
}
```

**Pattern `out string loi`:** Thay vì throw Exception, hàm trả về `bool` + thông báo lỗi qua tham số `out`. GUI nhận lỗi này và hiện lên màn hình.

---

## 5. CrudFormBase — Mẫu Form CRUD chung

### 5.1 Tại sao có class này?

4 form đơn giản (Toa, LoaiCanHo, TienNghi, GiaDichVu) đều có cùng layout và logic: danh sách bên trái, form nhập bên phải, nút Thêm/Sửa/Xóa. Thay vì viết lại 4 lần, ta tạo `CrudFormBase<T>` làm "khuôn mẫu".

```csharp
// File: src/QuanLyChoThueNha.GUI/Forms/Shared/CrudFormBase.cs

public abstract class CrudFormBase<T> : MaterialForm where T : class, new()
{
    // Ba trạng thái của form
    private enum FormMode { View, Adding, Editing }
    private FormMode _mode = FormMode.View;

    // Dictionary lưu tên field → Control tương ứng
    // VD: "TenToa" → TextBox, "MaKhuVuc" → ComboBox
    private readonly Dictionary<string, Control> _editors = new Dictionary<string, Control>();

    protected readonly DataGridView Grid;  // Bảng danh sách bên trái
    protected readonly TextBox TxtSearch; // Ô tìm kiếm
```

### 5.2 FieldDefinition — Mô tả cột/trường

```csharp
public class FieldDefinition
{
    public string PropertyName { get; }  // Tên property trong entity, VD: "TenToa"
    public string Caption { get; }       // Nhãn hiển thị, VD: "Tên tòa"
    public Type ValueType { get; }       // Kiểu dữ liệu: string, int, decimal, DateTime...
    public bool ReadOnly { get; }        // true = chỉ hiển thị, không sửa (VD: mã tự sinh)
    public bool Multiline { get; }       // true = TextBox cao (cho Mô tả)
    public IList<ComboOption> LookupOptions { get; } // null = TextBox, != null = ComboBox
}
```

**Ví dụ tạo fields cho frmToa:**
```csharp
new FieldDefinition("MaToa",     "Mã tòa",   typeof(string), readOnly: true),  // Chỉ đọc
FieldDefinition.Lookup("MaKhuVuc", "Khu vực", khuVucOptions),                  // ComboBox
new FieldDefinition("TenToa",    "Tên tòa"),                                    // TextBox bình thường
new FieldDefinition("SoTang",    "Số tầng",  typeof(int)),                      // NumericUpDown
new FieldDefinition("MoTa",      "Mô tả",   typeof(string), false, null, true), // Multiline TextBox
```

### 5.3 CreateEditor — Tự động tạo Control phù hợp kiểu dữ liệu

```csharp
private Control CreateEditor(FieldDefinition field)
{
    var type = /* lấy kiểu thực của property, xử lý Nullable */;

    if (field.LookupOptions != null && field.LookupOptions.Count > 0)
        return new ComboBox { DataSource = field.LookupOptions };   // Có options → ComboBox

    if (type == typeof(bool))
        return new CheckBox();                   // bool → CheckBox

    if (type == typeof(DateTime))
        return new DateTimePicker();             // DateTime → DateTimePicker

    if (type == typeof(int) || type == typeof(decimal) || ...)
        return new KeyboardOnlyNumericUpDown();  // Số → NumericUpDown

    return new TextBox();                        // Còn lại → TextBox
}
```

**Kết quả:** Thêm 1 field vào `Fields()` là tự động có Control đúng kiểu, không cần kéo thả Designer.

### 5.4 Ba trạng thái Form (State Machine)

```
          ┌────────────────────────────────────────────┐
          │           Sơ đồ trạng thái Form            │
          └────────────────────────────────────────────┘

  ┌─────────────────────┐
  │     VIEW MODE       │  ← Trạng thái mặc định
  │  Thêm ✓  Sửa ✓  Xóa ✓│
  │  Lưu ✗   Hủy ✗       │
  │  Các field: ReadOnly  │
  └─────┬──────────┬──────┘
        │ Bấm Thêm  │ Bấm Sửa
        ▼           ▼
  ┌──────────┐ ┌──────────────┐
  │  ADDING  │ │   EDITING    │
  │ Form rỗng│ │ Form có data │
  │ Lưu ✓   │ │ Lưu ✓        │
  │ Hủy ✓   │ │ Hủy ✓        │
  └──────────┘ └──────────────┘
      ↕ Lưu/Hủy     ↕ Lưu/Hủy
         quay về VIEW MODE
```

**Code thực hiện:**
```csharp
private void EnterViewMode()
{
    _mode = FormMode.View;
    SetEditorsEnabled(false);  // Tắt toàn bộ editor

    bool hasRow = CurrentItem != null;
    _btnAdd.Visible    = true;  _btnAdd.Enabled    = true;
    _btnUpdate.Visible = true;  _btnUpdate.Enabled = hasRow; // Chỉ enable nếu có dòng chọn
    _btnDelete.Visible = true;  _btnDelete.Enabled = hasRow;
    _btnSave.Visible   = false; // Ẩn Lưu
    _btnCancel.Visible = false; // Ẩn Hủy
}

private void EnterAddMode()
{
    _mode = FormMode.Adding;
    ClearInputs();         // Xóa trắng toàn bộ
    SetEditorsEnabled(true);

    _btnAdd.Visible    = false; // Ẩn Thêm
    _btnUpdate.Visible = false; // Ẩn Sửa
    _btnDelete.Visible = false; // Ẩn Xóa
    _btnSave.Visible   = true;  // Hiện Lưu
    _btnCancel.Visible = true;  // Hiện Hủy
}
```

### 5.5 BindRowToInputs — Đổ dữ liệu từ Entity vào Form

```csharp
private void BindRowToInputs(T item)
{
    foreach (var field in _fields)
    {
        var property = GetProperty(field.PropertyName); // Reflection lấy PropertyInfo
        var value = property.GetValue(item, null);       // Lấy giá trị từ object
        var editor = _editors[field.PropertyName];       // Lấy Control tương ứng

        if (editor is ComboBox)
            ((ComboBox)editor).SelectedValue = value?.ToString();
        else if (editor is CheckBox)
            ((CheckBox)editor).Checked = (bool)value;
        else if (editor is DateTimePicker)
            ((DateTimePicker)editor).Value = (DateTime)value;
        else if (editor is NumericUpDown)
            ((NumericUpDown)editor).Value = Convert.ToDecimal(value);
        else
            ((TextBox)editor).Text = value?.ToString() ?? string.Empty;
    }
}
```

**Kỹ thuật Reflection:** `typeof(T).GetProperty("TenToa")` tìm property theo tên string — không cần biết kiểu T cụ thể là gì.

### 5.6 Validation tự động bằng DataAnnotations

```csharp
private bool ValidateBeforeSave(T item)
{
    var context = new ValidationContext(item, null, null);
    var results = new List<ValidationResult>();

    // Validator.TryValidateObject kiểm tra các [Required], [MaxLength], [Range]...
    // trong Model Entity mà không cần viết thủ công
    Validator.TryValidateObject(item, context, results, true);

    if (results.Count == 0) return true;  // Hợp lệ

    foreach (var result in results)
        _errorProvider.SetError(_editors[result.MemberNames.First()], result.ErrorMessage);

    ShowError(string.Join("\n", results.Select(r => r.ErrorMessage)));
    return false;
}
```

---

## 6. frmKhuVuc — Quản lý Khu vực

### 6.1 Kiến trúc form này

`frmKhuVuc` là form **tự xây layout** (không kế thừa CrudFormBase) vì có logic đặc biệt: ComboBox Thành phố ↔ Quận liên động, và tính năng lấy tọa độ từ OpenStreetMap.

```csharp
// File: src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmKhuVuc.cs

public class frmKhuVuc : MaterialForm
{
    private readonly KhuVucService _svc = new KhuVucService();

    // Dữ liệu tĩnh: mapping Thành phố → danh sách Quận
    private readonly Dictionary<string, string[]> _districtsByCity = new Dictionary<string, string[]>
    {
        { "Ha Noi", new[] { "Ba Dinh", "Hoan Kiem", "Cau Giay", ... } },
        { "TP Ho Chi Minh", new[] { "Quan 1", "Quan 3", ... } },
        { "Da Nang", new[] { "Hai Chau", "Son Tra", ... } }
        // ...
    };
```

### 6.2 ComboBox liên động (Cascade ComboBox)

```csharp
// Khi form khởi tạo, nạp danh sách thành phố
private void NapThanhPho()
{
    cboThanhPho.Items.Clear();

    // Thêm các thành phố cố định từ dictionary
    foreach (var city in _districtsByCity.Keys.OrderBy(x => x))
        cboThanhPho.Items.Add(city);

    // Thêm thêm các thành phố từ DB (nếu có thành phố lạ đã nhập trước)
    foreach (var city in _svc.LayTatCa().Select(k => k.ThanhPho).Distinct())
        if (!cboThanhPho.Items.Contains(city)) cboThanhPho.Items.Add(city);

    if (cboThanhPho.Items.Count > 0) cboThanhPho.SelectedIndex = 0;
}

// Sự kiện: khi chọn Thành phố → cập nhật danh sách Quận
private void NapQuanTheoThanhPho()
{
    var city = cboThanhPho.SelectedItem?.ToString() ?? string.Empty;
    cboQuan.Items.Clear();

    // Thêm quận từ dictionary tĩnh
    string[] districts;
    if (_districtsByCity.TryGetValue(city, out districts))
        cboQuan.Items.AddRange(districts);

    // Thêm quận từ DB (nếu đã nhập quận lạ trước đó)
    foreach (var d in _svc.LayTatCa()
        .Where(k => k.ThanhPho == city)
        .Select(k => k.Quan).Distinct())
        if (!cboQuan.Items.Contains(d)) cboQuan.Items.Add(d);
}
```

**Kết nối sự kiện:**
```csharp
// Dòng này ở BuildLayout(), đăng ký event handler
cboThanhPho.SelectedIndexChanged += delegate { NapQuanTheoThanhPho(); };
```

### 6.3 Tính năng lấy tọa độ từ OpenStreetMap

```csharp
private void btnLayToaDo_Click(object sender, EventArgs e)
{
    // Ghép địa chỉ tìm kiếm
    var address = string.Format("{0}, {1}, {2}, Viet Nam",
        txtTen.Text,
        cboQuan.SelectedItem?.ToString(),
        cboThanhPho.SelectedItem?.ToString());

    try
    {
        using (var client = new WebClient())
        {
            // Phải gửi User-Agent vì Nominatim API yêu cầu
            client.Headers[HttpRequestHeader.UserAgent] = "QuanLyChoThueNha/1.0";

            // Gọi Nominatim (geocoding API miễn phí của OpenStreetMap)
            var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&q="
                + Uri.EscapeDataString(address);
            var json = client.DownloadString(url);

            // Parse JSON thủ công bằng Regex (không dùng thư viện JSON)
            var lat = DocGiaTriJson(json, "lat");
            var lon = DocGiaTriJson(json, "lon");

            numViDo.Value = (decimal)double.Parse(lat, CultureInfo.InvariantCulture);
            numKinhDo.Value = (decimal)double.Parse(lon, CultureInfo.InvariantCulture);
        }
    }
    catch (Exception ex) { /* hiện thông báo lỗi */ }
}

// Hàm parse JSON đơn giản bằng Regex
private string DocGiaTriJson(string json, string key)
{
    // Tìm pattern: "lat": "21.028511"
    var match = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\"([^\"]+)\"");
    return match.Success ? match.Groups[1].Value : string.Empty;
}
```

### 6.4 KhuVucService — Nghiệp vụ

```csharp
// File: src/QuanLyChoThueNha.BLL/Services/KhuVucService.cs

public class KhuVucService : BaseService<KhuVuc>
{
    protected override IRepository<KhuVuc> Repo => _uow.KhuVucs;

    public bool Them(string tenKhuVuc, string quan, string thanhPho, string maAdmin, out string loi,
        double? viDo = null, double? kinhDo = null)
    {
        loi = string.Empty;
        if (!ValidationHelper.KhongRong(tenKhuVuc, "Tên khu vực", out loi)) return false;

        // Kiểm tra giới hạn địa lý (vĩ độ -90 đến 90, kinh độ -180 đến 180)
        if (viDo.HasValue && (viDo.Value < -90 || viDo.Value > 90))
        { loi = "Vi do phai nam trong khoang -90 den 90."; return false; }

        var kv = new KhuVuc
        {
            MaKhuVuc  = SinhMa(),        // KV001, KV002...
            TenKhuVuc = tenKhuVuc.Trim(),
            Quan      = quan?.Trim(),
            ThanhPho  = thanhPho?.Trim(),
            MaAdmin   = maAdmin,
            ViDo = viDo, KinhDo = kinhDo
        };
        base.Them(kv);  // Gọi BaseService.Them → Add + SaveChanges
        return true;
    }

    // Không xóa được nếu còn tòa nhà thuộc khu vực này
    public bool XoaKhuVuc(string maKhuVuc, out string loi)
    {
        if (_uow.Toas.Any(t => t.MaKhuVuc == maKhuVuc))
        {
            loi = "Khong the xoa khu vuc vi dang co toa nha thuoc khu vuc nay.";
            return false;
        }
        Xoa(khuVuc);
        return true;
    }
}
```

---

## 7. frmToa — Quản lý Tòa nhà

`frmToa` kế thừa `CrudFormBase<Toa>` — chỉ cần **59 dòng code** nhờ base class:

```csharp
// File: src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmToa.cs

public class frmToa : CrudFormBase<Toa>
{
    private readonly ToaService _service = new ToaService();

    // Constructor: truyền tiêu đề và danh sách field vào base
    public frmToa() : base("Quản lý Tòa nhà", Fields()) { }

    // Khai báo các cột/trường cần quản lý
    private static IEnumerable<FieldDefinition> Fields()
    {
        // Phải lấy danh sách KhuVuc ở đây để nạp ComboBox
        var khuVucOptions = new KhuVucService().LayTatCa()
            .OrderBy(k => k.ThanhPho)
            .Select(k => new ComboOption(k.MaKhuVuc,
                $"{k.TenKhuVuc} - {k.Quan}, {k.ThanhPho}")) // Format hiển thị
            .ToList();

        return new[]
        {
            new FieldDefinition("MaToa",    "Mã tòa",   typeof(string), readOnly: true),
            FieldDefinition.Lookup("MaKhuVuc", "Khu vực", khuVucOptions),
            new FieldDefinition("TenToa",   "Tên tòa"),
            new FieldDefinition("DiaChi",   "Địa chỉ"),
            new FieldDefinition("SoTang",   "Số tầng", typeof(int)),
            new FieldDefinition("MoTa",     "Mô tả", typeof(string), false, null, multiline: true)
        };
    }

    // Override 4 hàm abstract — base class lo phần còn lại
    protected override IEnumerable<Toa> GetItems() { return _service.LayTatCa(); }

    protected override bool AddItem(Toa item, out string error)
    {
        // Gọi Service với từng tham số, không phải truyền object nguyên
        return _service.Them(item.TenToa, item.MaKhuVuc, item.DiaChi, item.SoTang, item.MoTa, out error);
    }

    protected override bool UpdateItem(Toa item, out string error)
    {
        error = string.Empty;
        _service.Sua(item);  // Sửa không cần kiểm tra thêm
        return true;
    }

    protected override bool DeleteItem(Toa item, out string error)
    {
        return _service.XoaToa(item.MaToa, out error); // Service tự kiểm tra FK
    }
}
```

**ToaService — nghiệp vụ kiểm tra:**
```csharp
public bool XoaToa(string maToa, out string loi)
{
    // Không xóa nếu còn CanHo thuộc tòa này
    if (_uow.CanHos.Any(c => c.MaToa == maToa))
    { loi = "Khong the xoa toa nha vi dang co can ho."; return false; }

    // Không xóa nếu còn bảng GiaDichVu liên quan
    if (_uow.GiaDichVus.Any(g => g.MaToa == maToa))
    { loi = "Khong the xoa toa nha vi dang co bang gia dich vu."; return false; }

    Xoa(toa);
    return true;
}
```

---

## 8. frmLoaiCanHo — Quản lý Loại căn hộ

Tương tự frmToa, kế thừa CrudFormBase. Chỉ có 4 trường đơn giản:

```csharp
// File: src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmLoaiCanHo.cs

private static IEnumerable<FieldDefinition> Fields()
{
    return new[]
    {
        new FieldDefinition("MaLoai",  "Mã loại",  typeof(string), readOnly: true),
        new FieldDefinition("MaAdmin", "Mã admin", typeof(string), readOnly: true),
        new FieldDefinition("TenLoai", "Tên loại"),
        new FieldDefinition("MoTa",    "Mô tả", typeof(string), false, null, multiline: true)
    };
}

protected override bool AddItem(LoaiCanHo item, out string error)
{
    // Lấy mã admin từ SessionContext (người đang đăng nhập)
    var maAdmin = SessionContext.LaAdmin ? SessionContext.MaNguoiDung : null;
    return _service.Them(item.TenLoai, item.MoTa, maAdmin, out error);
}
```

**LoaiCanHoService:**
```csharp
public bool Them(string tenLoai, string moTa, string maAdmin, out string loi)
{
    if (!ValidationHelper.KhongRong(tenLoai, "Tên loại căn hộ", out loi)) return false;
    var loai = new LoaiCanHo { MaLoai = SinhMa(), TenLoai = tenLoai.Trim(), ... };
    base.Them(loai);
    return true;
}

// Không xóa nếu có CanHo đang dùng loại này
public bool XoaLoaiCanHo(string maLoai, out string loi)
{
    if (_uow.CanHos.Any(c => c.MaLoai == maLoai))
    { loi = "Khong the xoa loai can ho vi dang co can ho su dung loai nay."; return false; }
    Xoa(loai); return true;
}
```

**Ví dụ loại căn hộ:** Studio, 1 Phòng ngủ, 2 Phòng ngủ, Penthouse...

---

## 9. frmTienNghi — Quản lý Tiện nghi

Cấu trúc gần giống frmLoaiCanHo. Đây là danh mục các tiện nghi như: WiFi, Máy lạnh, Bãi đỗ xe, Hồ bơi...

```csharp
// File: src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmTienNghi.cs

private static IEnumerable<FieldDefinition> Fields()
{
    return new[]
    {
        new FieldDefinition("MaTienNghi", "Mã tiện nghi", typeof(string), readOnly: true),
        new FieldDefinition("MaAdmin",    "Mã admin",     typeof(string), readOnly: true),
        new FieldDefinition("TenTienNghi","Tên tiện nghi"),
        new FieldDefinition("MoTa",       "Mô tả", typeof(string), false, null, multiline: true)
    };
}
```

**TienNghiService — kiểm tra trước khi xóa:**
```csharp
public bool XoaTienNghi(string maTienNghi, out string loi)
{
    // Không xóa nếu tiện nghi đang được gán cho căn hộ nào đó
    if (_uow.TienNghiCuaCanHos.Any(t => t.MaTienNghi == maTienNghi))
    {
        loi = "Khong the xoa tien nghi vi dang duoc gan cho can ho.";
        return false;
    }
    Xoa(tienNghi);
    return true;
}
```

**Quan hệ nhiều-nhiều CanHo ↔ TienNghi:**

```
CanHo              TienNghiCuaCanHo (bảng trung gian)    TienNghi
─────              ────────────────────────────────       ────────
MaCanHo ──────────→ MaCanHo                              MaTienNghi ←── MaTienNghi
                    MaTienNghi ───────────────────────→
                    GhiChu
```

---

## 10. frmGiaDichVu — Quản lý Giá dịch vụ

Phức tạp hơn một chút vì có logic **chỉ có 1 bảng giá đang áp dụng** mỗi tòa:

```csharp
// File: src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmGiaDichVu.cs

private static IEnumerable<FieldDefinition> Fields()
{
    var toaOptions = new ToaService().LayTatCa()
        .Select(t => new ComboOption(t.MaToa, t.TenToa))
        .ToList();

    return new[]
    {
        new FieldDefinition("MaGiaDichVu",      "Mã giá",            typeof(string), readOnly: true),
        FieldDefinition.Lookup("MaToa",         "Tòa nhà",           toaOptions),
        new FieldDefinition("GiaDien",          "Giá điện",          typeof(decimal)),
        new FieldDefinition("GiaNuoc",          "Giá nước",          typeof(decimal)),
        new FieldDefinition("GiaDichVuChung",   "Giá dịch vụ chung", typeof(decimal)),
        new FieldDefinition("NgayApDung",       "Ngày áp dụng",      typeof(DateTime)),
        new FieldDefinition("NgayKetThuc",      "Ngày kết thúc",     typeof(DateTime?)), // Nullable
        new FieldDefinition("DangApDung",       "Đang áp dụng",      typeof(bool))
    };
}

// Override để tô màu dòng đang áp dụng
protected override void AfterGridBound()
{
    foreach (DataGridViewRow row in Grid.Rows)
    {
        var gdv = row.DataBoundItem as GiaDichVu;
        if (gdv != null && gdv.DangApDung)
            row.DefaultCellStyle.BackColor = Color.Honeydew; // Xanh nhạt
    }
}
```

**GiaDichVuService — tự động vô hiệu hóa bảng giá cũ:**
```csharp
public bool Them(GiaDichVu gdv, out string loi)
{
    // Kiểm tra giá trị hợp lệ
    if (gdv.GiaDien <= 0)  { loi = "Gia dien phai > 0."; return false; }
    if (gdv.GiaNuoc <= 0)  { loi = "Gia nuoc phai > 0."; return false; }
    if (gdv.GiaDichVuChung < 0) { loi = "Gia dich vu chung khong duoc am."; return false; }

    // TỰ ĐỘNG: tắt tất cả bảng giá cũ đang áp dụng của cùng tòa nhà
    foreach (var old in _uow.GiaDichVus.Find(g => g.MaToa == gdv.MaToa && g.DangApDung))
    {
        old.DangApDung = false;
        old.NgayKetThuc = DateTime.Today;  // Đặt ngày kết thúc = hôm nay
        _uow.GiaDichVus.Update(old);
    }

    // Thêm bảng giá mới, luôn là đang áp dụng
    gdv.MaGiaDichVu = SinhMa();
    gdv.DangApDung  = true;
    _uow.GiaDichVus.Add(gdv);
    _uow.Complete();
    return true;
}

// Lấy bảng giá hiện hành của tòa nhà
public GiaDichVu LayGiaHienTai(string maToa)
{
    return TimMotBan(g => g.MaToa == maToa && g.DangApDung);
}
```

---

## 11. frmCanHo — Quản lý Căn hộ

Đây là form **phức tạp nhất** trong phần Tài sản vì: tự xây layout, quản lý hình ảnh, quản lý tiện nghi qua CheckedListBox, và có logic đặc biệt về tình trạng.

### 11.1 Tổng quan cấu trúc

```csharp
// File: src/QuanLyChoThueNha.GUI/Forms/TaiSan/frmCanHo.cs

public class frmCanHo : MaterialForm
{
    private enum FormMode { View, Adding, Editing }

    // Services — mỗi service quản lý 1 loại dữ liệu
    private readonly CanHoService    _canHoSvc   = new CanHoService();
    private readonly ToaService      _toaSvc     = new ToaService();
    private readonly LoaiCanHoService _loaiSvc   = new LoaiCanHoService();
    private readonly TienNghiService _tienNghiSvc = new TienNghiService();

    private string _tinhTrangHienTai = "Trong"; // Lưu tình trạng thực (từ DB) để so sánh
    // ...
```

### 11.2 Layout: TableLayoutPanel 68/32

```csharp
private void BuildLayout()
{
    var root = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill };
    root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68)); // Cột trái 68%
    root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32)); // Cột phải 32%
    // Trái: SearchPanel + DataGridView
    // Phải: Form nhập liệu + Buttons
}
```

### 11.3 NapComboBox — Nạp dữ liệu vào ComboBox và CheckedListBox

```csharp
private void NapComboBox()
{
    // ComboBox Tòa nhà
    cboToa.DisplayMember = "TenToa";  // Hiển thị tên tòa
    cboToa.ValueMember   = "MaToa";   // Lưu mã tòa (key)
    cboToa.DataSource    = _toaSvc.LayTatCa().OrderBy(t => t.TenToa).ToList();

    // ComboBox Loại căn hộ
    cboLoai.DisplayMember = "TenLoai";
    cboLoai.ValueMember   = "MaLoai";
    cboLoai.DataSource    = _loaiSvc.LayTatCa().OrderBy(l => l.TenLoai).ToList();

    // CheckedListBox Tiện nghi (có thể tick nhiều)
    lstTienNghi.DisplayMember = "TenTienNghi";
    lstTienNghi.ValueMember   = "MaTienNghi";
    lstTienNghi.Items.Clear();
    foreach (var tn in _tienNghiSvc.LayTatCa().OrderBy(t => t.TenTienNghi))
        lstTienNghi.Items.Add(tn, false); // false = chưa tick
}
```

### 11.4 CapNhatGrid — Hiển thị dữ liệu lên bảng

```csharp
private void CapNhatGrid(List<CanHo> canHos)
{
    // Tạo Dictionary để tra tên nhanh (tránh query DB nhiều lần)
    var toas  = _toaSvc.LayTatCa().ToDictionary(t => t.MaToa, t => t.TenToa);
    var loais = _loaiSvc.LayTatCa().ToDictionary(l => l.MaLoai, l => l.TenLoai);

    // Chiếu (Project) sang anonymous type để hiển thị tên thay vì mã
    var list = canHos.Select(c => new
    {
        c.MaCanHo,
        TenCanHo = "Căn " + c.SoCanHo,       // Tên hiển thị = "Căn 101"
        TenToa   = toas.ContainsKey(c.MaToa)  // Tra tên tòa từ dictionary
                   ? toas[c.MaToa] : c.MaToa,
        TenLoai  = loais.ContainsKey(c.MaLoai)
                   ? loais[c.MaLoai] : c.MaLoai,
        c.DienTich, c.GiaThueNiemYet, c.TienCocNiemYet,
        c.SoCanHo, c.TangSo, c.TinhTrang
    }).ToList();

    dgv.DataSource = new BindingList<object>(list.Cast<object>().ToList());
}
```

### 11.5 Logic tình trạng căn hộ — Phần quan trọng nhất

```csharp
// Chỉ cho phép người dùng chọn Trong / BaoTri
// DaDatCoc / DangThue là do HỆ THỐNG tự cập nhật
private void SetEditorsEnabled(bool enabled)
{
    // ...
    // Nếu tình trạng hiện tại là DaDatCoc hoặc DangThue → khóa ComboBox tình trạng
    cboTinhTrang.Enabled = enabled
        && _tinhTrangHienTai != "DaDatCoc"
        && _tinhTrangHienTai != "DangThue";
}
```

**Trong CanHoService, tình trạng được tự động tính:**
```csharp
private string TinhTrangTheoNghiepVu(CanHo canHo)
{
    // Ưu tiên 1: Có HopDong HieuLuc đang trong khoảng ngày → DangThue
    var coHopDong = _uow.HopDongs.Any(h =>
        h.MaCanHo == canHo.MaCanHo &&
        h.TrangThai == "HieuLuc" &&
        h.NgayBatDau <= DateTime.Today &&
        h.NgayKetThuc >= DateTime.Today);
    if (coHopDong) return "DangThue";

    // Ưu tiên 2: Có PhieuDatTruoc mở chưa hết hạn → DaDatCoc
    var coDatTruoc = _uow.PhieuDatTruocs.Any(p =>
        p.MaCanHo == canHo.MaCanHo &&
        p.NgayHetHan >= DateTime.Now &&
        (p.TrangThai == "ChoThanhToanCoc" || p.TrangThai == "DaThanhToanCoc" || p.TrangThai == "ChoKy"));
    if (coDatTruoc) return "DaDatCoc";

    // Ưu tiên 3: Nếu đang là trạng thái hệ thống → đặt lại Trong
    return LaTrangThaiHeThong(canHo.TinhTrang) ? "Trong" : canHo.TinhTrang;
}

// Mỗi lần LayTatCa(), đồng bộ tình trạng
public override IEnumerable<CanHo> LayTatCa()
{
    DongBoTinhTrangTheoNghiepVu(); // Cập nhật trước khi trả về
    return base.LayTatCa();
}
```

### 11.6 Quản lý Tiện nghi — DongBoTienNghi

```csharp
// Lấy danh sách MaTienNghi đang được tick trong CheckedListBox
private IEnumerable<string> LayTienNghiDangTick()
{
    return lstTienNghi.CheckedItems
        .Cast<TienNghi>()
        .Select(t => t.MaTienNghi)
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .ToList();
}

// Sau khi lưu căn hộ, gọi hàm này để đồng bộ tiện nghi
_canHoSvc.DongBoTienNghi(canHo.MaCanHo, LayTienNghiDangTick());
```

**DongBoTienNghi trong Service:**
```csharp
public void DongBoTienNghi(string maCanHo, IEnumerable<string> maTienNghis)
{
    var selected = new HashSet<string>(maTienNghis); // Danh sách MỚI (từ UI)
    var current  = _uow.TienNghiCuaCanHos
        .Find(t => t.MaCanHo == maCanHo).ToList(); // Danh sách CŨ (từ DB)

    // Xóa những tiện nghi không còn được tick
    foreach (var item in current.Where(t => !selected.Contains(t.MaTienNghi)))
        _uow.TienNghiCuaCanHos.Remove(item);

    // Thêm những tiện nghi mới được tick
    var currentIds = new HashSet<string>(current.Select(t => t.MaTienNghi));
    foreach (var maTN in selected.Where(id => !currentIds.Contains(id)))
        _uow.TienNghiCuaCanHos.Add(new TienNghiCuaCanHo { MaCanHo = maCanHo, MaTienNghi = maTN });

    _uow.Complete(); // Lưu tất cả thay đổi một lần
}
```

**Ý tưởng:** So sánh tập cũ (DB) và tập mới (UI), rồi chỉ Xóa/Thêm phần chênh lệch — tương tự git diff.

### 11.7 Quản lý Hình ảnh

```csharp
private void btnThemAnh_Click(object sender, EventArgs e)
{
    // 1. Chọn file ảnh
    using (var dialog = new OpenFileDialog())
    {
        dialog.Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*";
        if (dialog.ShowDialog() != DialogResult.OK) return;

        // 2. Kiểm tra ảnh hợp lệ trước khi lưu
        Image preview;
        string loi;
        if (!TryCreatePreviewImage(dialog.FileName, out preview, out loi))
        { ShowError(loi); return; }

        // 3. Gọi Service lưu đường dẫn vào DB
        _canHoSvc.ThemAnh(txtMa.Text, dialog.FileName, "Ảnh phòng");

        // 4. Cập nhật PictureBox trên UI
        picAnh.Image = preview;
    }
}

// Tạo ảnh preview chất lượng cao (dùng HighQualityBicubic)
private bool TryCreatePreviewImage(string path, out Image preview, out string error)
{
    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    using (var original = Image.FromStream(stream))
    {
        // Tính tỷ lệ để fit vào PictureBox mà không méo
        var ratio = Math.Min((float)picAnh.Width / original.Width,
                             (float)picAnh.Height / original.Height);
        var width  = (int)(original.Width  * ratio);
        var height = (int)(original.Height * ratio);

        var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic; // Chất lượng cao
            g.DrawImage(original, 0, 0, width, height);
        }
        preview = bitmap;
        return true;
    }
}
```

**ThemAnh trong Service:**
```csharp
public void ThemAnh(string maCanHo, string duongDanAnh, string moTa = null)
{
    var anhHienCo = _uow.HinhAnhNhas.Find(h => h.MaCanHo == maCanHo).ToList();

    if (anhHienCo.Count > 0)
    {
        // ĐÃ CÓ ảnh → cập nhật ảnh đại diện, xóa ảnh thừa
        var anhDaiDien = anhHienCo[0];
        anhDaiDien.DuongDanAnh = duongDanAnh;
        anhDaiDien.NgayTaiLen  = DateTime.Now;
        _uow.HinhAnhNhas.Update(anhDaiDien);

        foreach (var anhPhu in anhHienCo.Skip(1))
            _uow.HinhAnhNhas.Remove(anhPhu); // Xóa ảnh thừa
    }
    else
    {
        // CHƯA CÓ ảnh → tạo mới
        _uow.HinhAnhNhas.Add(new HinhAnhNha {
            MaHinhAnh = SinhMaHinhAnh(),
            MaCanHo = maCanHo,
            DuongDanAnh = duongDanAnh,
            NgayTaiLen = DateTime.Now
        });
    }
    _uow.Complete();
}
```

### 11.8 XoaCanHo — Kiểm tra toàn diện trước khi xóa

```csharp
public bool XoaCanHo(string maCanHo, out string loi)
{
    // Kiểm tra 1: Còn phiếu đặt trước đang xử lý?
    if (phieusDatTruoc.Any(p => !LaPhieuDatTruocDaDong(p.TrangThai)))
    { loi = "Khong the xoa vi da co phieu dat truoc dang xu ly."; return false; }

    // Kiểm tra 2: Phiếu đặt trước đã dùng để ký hợp đồng?
    if (phieusDatTruoc.Any(p => _uow.HopDongs.Any(h => h.MaPhieuDatTruoc == p.MaPhieuDatTruoc)))
    { loi = "Khong the xoa vi phieu dat truoc da duoc ky hop dong."; return false; }

    // Kiểm tra 3: Có hợp đồng?
    if (_uow.HopDongs.Any(h => h.MaCanHo == maCanHo))
    { loi = "Khong the xoa vi da co hop dong lien quan."; return false; }

    // Kiểm tra 4: Có hình ảnh?
    if (_uow.HinhAnhNhas.Any(h => h.MaCanHo == maCanHo))
    { loi = "Khong the xoa vi dang co hinh anh lien quan."; return false; }

    // Kiểm tra 5: Có tiện nghi được gán?
    if (_uow.TienNghiCuaCanHos.Any(t => t.MaCanHo == maCanHo))
    { loi = "Khong the xoa vi dang co tien nghi duoc gan."; return false; }

    // Tất cả OK → dùng Transaction để xóa có rollback
    _uow.BeginTransaction();
    try
    {
        // Xóa EmailLog liên quan (cascade thủ công vì EF không tự làm)
        foreach (var phieu in phieusDatTruoc)
        {
            var emailLogs = _uow.EmailLogs.Find(e => e.MaPhieuDatTruoc == phieu.MaPhieuDatTruoc);
            _uow.EmailLogs.RemoveRange(emailLogs);
            _uow.PhieuDatTruocs.Remove(phieu);
        }
        _uow.CanHos.Remove(canHo);
        _uow.Complete();
        _uow.CommitTransaction();
        return true;
    }
    catch (Exception ex)
    {
        _uow.RollbackTransaction(); // Nếu lỗi → hoàn tác tất cả
        loi = ex.Message;
        return false;
    }
}
```

---

## 12. Luồng dữ liệu tổng quát

### 12.1 Luồng THÊM căn hộ

```
Người dùng điền form → Bấm "Lưu"
    ↓
frmCanHo.btnThem_Click()
    ↓
ValidateForm() → kiểm tra UI (Tòa, Loại, DienTich, GiaThue)
    ↓ (OK)
DocForm() → tạo object CanHo từ các Control
    ↓
CanHoService.Them(canHo, out loi)
    ├── ValidationHelper.KhongRong(MaToa)
    ├── ValidationHelper.KhongRong(MaLoai)
    ├── Kiểm tra Toa tồn tại trong DB
    ├── Kiểm tra LoaiCanHo tồn tại trong DB
    ├── canHo.MaCanHo = SinhMa() → "CH001"
    ├── canHo.NgayTao = DateTime.Now
    ├── AuditHelper.GanNguoiThaoTac(canHo)
    └── base.Them(canHo) → Repo.Add() + _uow.Complete()
    ↓
DongBoTienNghi(canHo.MaCanHo, tienNghiDangTick)
    ↓
TaiDuLieu() → làm mới DataGridView
```

### 12.2 Luồng hiển thị tình trạng

```
User mở form frmCanHo
    ↓
TaiDuLieu()
    ↓
CanHoService.LayTatCa()
    ↓
DongBoTinhTrangTheoNghiepVu() (duyệt toàn bộ căn hộ)
    ├── Mỗi căn hộ: gọi TinhTrangTheoNghiepVu()
    │     ├── Có HopDong HieuLuc? → "DangThue"
    │     ├── Có PhieuDatTruoc còn hạn? → "DaDatCoc"
    │     └── Không có gì → giữ nguyên hoặc "Trong"
    └── Nếu khác → Update DB
    ↓
base.LayTatCa() → trả về danh sách đã đồng bộ
    ↓
CapNhatGrid() → hiển thị lên DataGridView
```

---

## 13. Câu hỏi báo cáo thường gặp

**Q: Tại sao có CrudFormBase mà frmCanHo lại không dùng?**
> `CrudFormBase` phù hợp với form đơn giản. `frmCanHo` cần: CheckedListBox tiện nghi, PictureBox ảnh, logic tình trạng phức tạp, NumericUpDown tùy chỉnh — nên tự xây layout để kiểm soát hoàn toàn.

**Q: DaDatCoc và DangThue được cập nhật như thế nào?**
> Mỗi lần gọi `LayTatCa()` hoặc `LayTheoMa()`, `CanHoService` tự gọi `DongBoTinhTrangTheoNghiepVu()` để kiểm tra HopDong và PhieuDatTruoc trong DB và cập nhật nếu cần.

**Q: Tại sao dùng transaction khi xóa CanHo?**
> Xóa CanHo cần xóa theo thứ tự: EmailLog → PhieuDatTruoc → CanHo. Nếu bước giữa lỗi mà không có transaction, DB sẽ bị dữ liệu nửa chừng. Transaction đảm bảo "xóa hết hoặc không xóa gì".

**Q: DongBoTienNghi hoạt động ra sao?**
> Nhận danh sách MaTienNghi mới từ UI, so sánh với DB: xóa những cái không còn tick, thêm những cái mới tick. Hiệu quả hơn xóa-tất-cả rồi thêm lại vì chỉ thao tác phần chênh lệch.

**Q: Nominatim API là gì?**
> Dịch vụ geocoding miễn phí của OpenStreetMap. Nhận địa chỉ text, trả về JSON có `lat` (vĩ độ) và `lon` (kinh độ). Dự án dùng WebClient để gọi HTTP và Regex để parse JSON thủ công (không cần thư viện Newtonsoft).

**Q: Sự khác nhau giữa GiaThueNiemYet và giá thực tế?**
> `GiaThueNiemYet` trong bảng CanHo là giá **niêm yết** (giá gốc). Khi ký HopDong, có thể thỏa thuận giá khác. GiaDichVu (điện, nước) là riêng, được tra theo MaToa.

**Q: KeyboardOnlyNumericUpDown là gì?**
> Custom control kế thừa `NumericUpDown`, chỉ cho phép nhập số qua bàn phím (block mouse scroll để tránh vô tình thay đổi giá tiền). Dùng cho các trường giá tiền, diện tích.

---

## Tóm tắt sơ đồ phụ thuộc

```
frmKhuVuc ──────────────→ KhuVucService
frmToa ─────────────────→ ToaService (phụ thuộc KhuVucService)
frmLoaiCanHo ───────────→ LoaiCanHoService
frmTienNghi ────────────→ TienNghiService
frmGiaDichVu ───────────→ GiaDichVuService (phụ thuộc ToaService)
frmCanHo ───────────────→ CanHoService
                           ├── ToaService
                           ├── LoaiCanHoService
                           └── TienNghiService

Tất cả Service ──────────→ BaseService<T>
                           └── UnitOfWork (1 DbContext dùng chung)
                               └── Repository<T> (EF6 CRUD)
                                   └── AppDbContext → SQL Server
```

---

*Tài liệu được tạo tự động từ source code — phiên bản 2026-06-03*
