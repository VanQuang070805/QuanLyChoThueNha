# Kiến trúc 3 lớp — Hướng dẫn thực hành

## Luồng dữ liệu

```
GUI (Form)  ──call──►  BLL (Service)  ──call──►  DAL (UnitOfWork/Repository)  ──►  SQL Server
           ◄──data──                  ◄──data──
```

GUI **không** gọi thẳng DB. BLL **không** biết WinForms. DAL **không** chứa logic nghiệp vụ.

---

## Mẫu Form CRUD theo Validation 3 bước

```csharp
// Theo yêu cầu của cô: 3 bước validation bắt buộc
private void btnThem_Click(object sender, EventArgs e)
{
    // ── Bước 1: Kiểm tra sự tương đồng và tính chính xác ────────
    string loi;
    if (string.IsNullOrWhiteSpace(txtTen.Text))
    {
        // ── Bước 2 (SAI): Cảnh báo lỗi, yêu cầu nhập lại ────────
        MessageBox.Show("Tên không được để trống.", "Lỗi nhập liệu",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtTen.Focus();
        return;
    }

    // Gọi Service — Service tự validate thêm ở tầng BLL
    bool ok = _service.Them(txtTen.Text, out loi);
    if (!ok)
    {
        MessageBox.Show(loi, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // ── Bước 3 (ĐÚNG): Chuyển dữ liệu vào CSDL, làm mới Grid ────
    MessageBox.Show("Thêm thành công!", "Thông báo",
        MessageBoxButtons.OK, MessageBoxIcon.Information);
    TaiDuLieu();
    XoaTrong();
}
```

---

## Mẫu Transaction (nghiệp vụ phức tạp)

```csharp
// Dùng khi cần cập nhật nhiều bảng cùng lúc (VD: ký hợp đồng)
public bool KyHopDong(HopDong hopDong, out string loi)
{
    loi = string.Empty;
    _uow.BeginTransaction();
    try
    {
        _uow.HopDongs.Add(hopDong);

        // Cập nhật trạng thái căn hộ
        var canHo = _uow.CanHos.GetById(hopDong.MaCanHo);
        canHo.TinhTrang = "DangThue";
        _uow.CanHos.Update(canHo);

        _uow.Complete();           // SaveChanges
        _uow.CommitTransaction();  // Commit
        return true;
    }
    catch (Exception ex)
    {
        _uow.RollbackTransaction(); // Rollback nếu lỗi
        loi = ex.Message;
        return false;
    }
}
```

---

## Cách thêm Form mới (dành cho mỗi TV)

1. Tạo file `.cs` trong thư mục module của mình (VD: `Forms/HopDong/frmHopDong.cs`)
2. Kế thừa `MaterialForm`
3. Inject Service tương ứng trong constructor
4. Gắn vào `frmMain.cs` tại dòng `// TODO` phù hợp

```csharp
// frmMain.cs — tìm dòng TODO của module mình rồi thêm vào:
private void btnHopDong_Click(object sender, EventArgs e)
{
    MoForm(new frmHopDong());  // ← TV3 uncomment/sửa dòng này
}
```

---

## Gọi Stored Procedure từ EF6

```csharp
using (var ctx = new AppDbContext())
{
    // Trả về List<DoanhThuTheoThangDto>
    var result = ctx.Database
        .SqlQuery<DoanhThuTheoThangDto>("EXEC sp_DoanhThuTheoThang @p0", 2025)
        .ToList();
}
```

---

## Xuất Excel bằng ClosedXML (TV5)

```csharp
using ClosedXML.Excel;
using var wb = new XLWorkbook();
var ws = wb.Worksheets.Add("Doanh thu");
ws.Cell(1, 1).Value = "Tháng";
ws.Cell(1, 2).Value = "Tổng thu";
ws.Cell(1, 3).Value = "Số hóa đơn";

int row = 2;
foreach (var item in dsDoanhThu)
{
    ws.Cell(row, 1).Value = item.Thang;
    ws.Cell(row, 2).Value = (double)item.TongThu;
    ws.Cell(row, 3).Value = item.SoHoaDon;
    row++;
}

// Định dạng header
var headerRow = ws.Row(1);
headerRow.Style.Font.Bold = true;
headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

// Lưu file
var saveDialog = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = "BaoCaoDoanhThu.xlsx" };
if (saveDialog.ShowDialog() == DialogResult.OK)
    wb.SaveAs(saveDialog.FileName);
```

---

## So sánh mô hình ERD: Chen vs Crow's Foot (theo yêu cầu cô)

| Tiêu chí | Chen | Crow's Foot |
|---|---|---|
| Thực thể | Hình chữ nhật | Hình chữ nhật |
| Thuộc tính | Hình elip nối vào thực thể | Liệt kê trong hộp |
| Quan hệ 1-N | Kim cương + ký hiệu 1 và N | Ký hiệu "chân chim" (crow's foot) |
| Quan hệ N-N | Kim cương | Thực thể trung gian |
| Phù hợp | Hệ thống lớn, học thuật | Thiết kế DB thực tế, gần schema |

**Dự án này dùng Crow's Foot** (biểu đồ lớp HTML đã có).
