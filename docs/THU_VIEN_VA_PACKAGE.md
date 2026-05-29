# Thư viện · NuGet Package · Extension

## A. NuGet Packages theo từng Project

| Project | Package | Phiên bản | Mục đích |
|---|---|---|---|
| **DAL** | `EntityFramework` | 6.4.4 | ORM kết nối SQL Server (EF6) |
| **BLL** | `BCrypt.Net-Next` | 4.0.3 | **Băm & xác thực mật khẩu (bảo mật)** |
| **GUI** | `MaterialSkin.2` | 2.3.1 | **Giao diện Material Design (UI)** |
| **GUI** | `ClosedXML` | 0.102.2 | **Xuất báo cáo Excel (báo cáo)** |
| **GUI** | `DocumentFormat.OpenXml` | 2.19.0 | Dependency của ClosedXML |

### Cài thêm nếu cần biểu đồ
```
PM> Install-Package System.Windows.Forms.DataVisualization -Project QuanLyChoThueNha.GUI
```
(Hoặc dùng `Chart` control có sẵn trong .NET Framework Toolbox)

---

## B. Phân loại theo yêu cầu của cô (3 nhóm)

### 1. Thư viện Giao diện (UI)
**`MaterialSkin.2`** — Tạo giao diện Material Design hiện đại cho WinForms.

```csharp
// Cách bật MaterialSkin cho một Form:
using MaterialSkin;
using MaterialSkin.Controls;

public class frmCuaToi : MaterialForm   // ← kế thừa MaterialForm, không phải Form
{
    public frmCuaToi()
    {
        var skin = MaterialSkinManager.Instance;
        skin.AddFormToManage(this);
        skin.Theme = MaterialSkinManager.Themes.LIGHT;
        skin.ColorScheme = new ColorScheme(
            Primary.Blue600, Primary.Blue700,
            Primary.Blue200, Accent.LightBlue200,
            TextShade.WHITE);
    }
}
// Dùng: MaterialTextBox, MaterialButton, MaterialLabel, MaterialListView...
```

### 2. Thư viện Báo cáo
**`ClosedXML`** — Xuất file Excel `.xlsx` không cần cài Office.

```csharp
using ClosedXML.Excel;

// Xuất báo cáo doanh thu ra Excel:
using var wb = new XLWorkbook();
var ws = wb.Worksheets.Add("Doanh Thu");
ws.Cell(1,1).Value = "Tháng";
ws.Cell(1,2).Value = "Tổng Thu (VNĐ)";
ws.Cell(1,3).Value = "Số Hóa Đơn";
// Định dạng header
ws.Row(1).Style.Font.Bold = true;
ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;
// Điền dữ liệu
int r = 2;
foreach (var item in dsDoanhThu) {
    ws.Cell(r,1).Value = item.Thang;
    ws.Cell(r,2).Value = (double)item.TongThu;
    ws.Cell(r,3).Value = item.SoHoaDon;
    r++;
}
// Auto-fit cột
ws.Columns().AdjustToContents();
wb.SaveAs("BaoCao.xlsx");
```

### 3. Thư viện Bảo mật
**`BCrypt.Net-Next`** — Băm mật khẩu với thuật toán BCrypt (có salt tự động).

```csharp
using BCrypt.Net;

// Khi tạo tài khoản — lưu hash vào DB, KHÔNG lưu mật khẩu thô:
string hash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12);
// → "$2a$12$92IXUNpkjO0rOQ5byMi.Ye..."

// Khi đăng nhập — so khớp mật khẩu người dùng nhập với hash trong DB:
bool hopLe = BCrypt.Net.BCrypt.Verify("Admin@123", hashTrongDB);
// → true / false
```

**Đã đóng gói sẵn trong `PasswordHelper.cs`:**
```csharp
string hash = PasswordHelper.Hash("Admin@123");
bool ok = PasswordHelper.Verify("Admin@123", hash);
```

---

## C. Extension Visual Studio nên cài

| Extension | Lợi ích |
|---|---|
| **EF Power Tools** | Tạo Entity từ DB có sẵn (Scaffold) |
| **Markdown Editor** | Đọc file .md ngay trong VS |
| **GitHub Extension** | Commit, push, pull request |
| **Productivity Power Tools** | Format code nhanh |

---

## D. Công cụ ngoài

- **SSMS** — Chạy SQL script, xem dữ liệu, debug query
- **Git** + **GitHub** — Quản lý mã nguồn nhóm
- **Postman** *(tùy chọn)* — Test nếu sau này thêm Web API

---

## E. Quy trình Test Style (theo yêu cầu cô)

Sau khi hoàn thành từng chức năng, TV phụ trách kiểm thử:

1. **Test thêm dữ liệu hợp lệ** — Kỳ vọng: thêm thành công, hiển thị trên grid
2. **Test dữ liệu rỗng/sai** — Kỳ vọng: hiện thông báo lỗi, không lưu DB
3. **Test trùng dữ liệu** — Kỳ vọng: thông báo "đã tồn tại"
4. **Test nghiệp vụ** — VD: đặt cọc căn hộ đang thuê → báo lỗi
5. **Test phân quyền** — Menu ẩn/hiện đúng theo vai trò

Ghi kết quả vào bảng kiểm thử trong báo cáo (Chương 4 kiểm thử).
