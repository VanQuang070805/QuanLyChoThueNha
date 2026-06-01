# Huong dan viet lai giao dien SmartApart theo phong cach dashboard nghiep vu

Tai lieu nay dung de tao mot ban giao dien moi cho he thong QuanLyChoThueNha/SmartApart, lay cam hung tu project `QuanLyCuaHangGame`: WinForms + MaterialSkin.2 + helper UI tu viet + dashboard theo nghiep vu.

Muc tieu khong phai copy y nguyen giao dien cua project game, ma la ap dung cach to chuc UI/UX cua ho vao dung bai toan quan ly cho thue nha.

## 1. Muc tieu cua ban giao dien moi

Ban giao dien moi can dat cac muc tieu sau:

- Giao dien nhin thong nhat hon: cung mau, cung font, cung kieu nut, cung kieu bang.
- Dieu huong ro rang: top bar + left drawer/menu trai + vung noi dung chinh.
- Moi man hinh nghiep vu co cau truc giong nhau: thanh hanh dong, bo loc, bang du lieu, panel chi tiet.
- Giam nhap ma thu cong: cac khoa ngoai dung combobox/dropdown.
- Bang du lieu hien thong tin nguoi dung doc duoc: ten khach, ten toa, ten can ho thay vi chi hien ma.
- Dashboard co bieu do, KPI card, bo loc thoi gian va trang thai.
- Form khach tim tro co card responsive, tag trang thai, tim kiem co dau/khong dau/giao nghia co ban.

## 2. Thu vien nen dung

Nen giu tech stack WinForms hien tai de tranh phai viet lai toan bo nghiep vu.

Thu vien khuyen nghi:

- `MaterialSkin.2`: dung cho form, button, textbox, theme mau xanh.
- `LiveCharts.WinForms`: dung cho line chart, bar chart, donut/pie chart neu muon dep hon chart tu ve.
- `Microsoft.Web.WebView2`: dung cho Leaflet/OpenStreetMap o man tim tro.
- `ClosedXML`: giu cho xuat Excel.
- `EntityFramework`: giu tang DAL/BLL hien tai.

Khong nen dua them qua nhieu UI library moi, vi WinForms de bi xung dot style. Cach tot hon la tao mot lop `UIHelper` rieng va dung lai o moi form.

## 3. Kien truc UI moi de xuat

Nen tao them thu muc:

```text
src/QuanLyChoThueNha.GUI/
  UI/
    SmartTheme.cs
    SmartLayout.cs
    SmartGridStyler.cs
    SmartButtonFactory.cs
    SmartCard.cs
    SmartDrawer.cs
    SmartMessageBox.cs
    SmartChartFactory.cs
```

Vai tro tung file:

- `SmartTheme.cs`: khai bao mau, font, spacing, radius, shadow gia lap.
- `SmartLayout.cs`: helper tao root layout, split view, detail panel, toolbar.
- `SmartGridStyler.cs`: style DataGridView/ListView, doi header, format tien, format ngay.
- `SmartButtonFactory.cs`: tao button chinh/phu/nguy hiem/icon button.
- `SmartCard.cs`: panel bo goc dung cho KPI, form group, card phong.
- `SmartDrawer.cs`: menu trai mo/dong, item active, overlay neu can.
- `SmartMessageBox.cs`: hop thoai thong bao dep va dong nhat.
- `SmartChartFactory.cs`: tao chart doanh thu, donut, bar, line.

## 4. Mau layout tong the

Ung dung sau dang nhap nen co 4 vung:

```text
+------------------------------------------------------+
| TopBar: logo + ten he thong + nut menu + ten vai tro |
+------------------+-----------------------------------+
| LeftDrawer       | Content Area                      |
| - Tong quan      | - Form hien tai                   |
| - Tai san        | - Dashboard / CRUD / Detail       |
| - Hop dong       |                                   |
| - Thanh toan     |                                   |
| - Bao cao        |                                   |
| - Quan tri       |                                   |
+------------------+-----------------------------------+
| Footer: nguoi dung, ngay, so can ho, doanh thu...    |
+------------------------------------------------------+
```

Hanh vi:

- Mac dinh chi hien top bar.
- Bam nut menu hoac bam nhom menu tren top bar thi mo left drawer.
- Bam vao item trong drawer thi mo trang vao content area.
- Drawer tu dong dong sau khi chon trang.
- Drawer co item dang active voi nen xanh nhat.
- Admin thay tat ca muc.
- Nhan vien chi thay muc duoc phan quyen.
- Khach thue khong vao dashboard noi bo, chi vao trang khach.

## 5. Mau layout cho man CRUD

Tat ca cac man quan ly nhu Khu vuc, Toa nha, Can ho, Khach thue, Hop dong, Hoa don nen theo mot khung:

```text
+--------------------------------------------------------+
| Title + mo ta ngan                                     |
| [Them moi] [Sua] [Xoa] [Lam moi] [Xuat Excel]          |
+--------------------------------------------------------+
| Search/filter card                                     |
| Tim kiem | Trang thai | Toa | Khu vuc | Gia tu/den     |
+-------------------------------+------------------------+
| Data grid                     | Detail panel           |
| - cot da rut gon              | - field editable       |
| - badge trang thai            | - dropdown khoa ngoai  |
| - format tien/ngay            | - nut luu/huy          |
+-------------------------------+------------------------+
```

Quy tac:

- Bang ben trai chi hien cot quan trong, khong show tat ca cot database.
- Panel ben phai hien chi tiet ban ghi dang chon.
- Khi them/sua moi bat field can sua.
- Cac truong he thong nhu `Ma...`, `TrangThai` tinh tu dong thi readonly.
- Khoa ngoai phai la combobox:
  - `MaKhach` hien `HoTen - CCCD/SDT`.
  - `MaCanHo` hien `TenToa - Can - Tang - TrangThai`.
  - `MaHopDong` hien `HopDong - TenKhach - CanHo`.
  - `MaPhieuDatTruoc` hien `Phieu - TenKhach - CanHo - TrangThai`.

## 6. Mau dashboard moi

Dashboard nen chia thanh:

- KPI cards: Tong can ho, Dang thue, Cho coc, Doanh thu, Cong no, Hop dong sap het han.
- Bo loc thoi gian: Ngay / Tuan / Thang / Nam bang nut bam.
- Bieu do:
  - Doanh thu vs cong no theo thoi gian.
  - Co cau trang thai can ho.
  - Top can ho doanh thu cao.
  - Loai phong duoc thue nhieu.
- Bang can xu ly:
  - Phieu cho coc.
  - Phieu cho ky.
  - Hoa don qua han.
  - Hop dong sap het han.

UI nen hoc tu project game:

- KPI card co border/shadow nhe.
- So lieu lon, label nho.
- Mau theo y nghia:
  - Xanh duong: tong quan.
  - Xanh la: tot/da tra/dang thue.
  - Cam: cho xu ly/sap het han.
  - Do: no/qua han/loi.
- Footer hien thong tin tong hop ngay hien tai.

## 7. Mau trang tim tro public

Trang tim tro nen tach ro:

- Header: logo, ten SmartApart, nut Tai khoan khach, Cong noi bo.
- Search/filter card:
  - Tim kiem dia chi/toa/can.
  - Khu vuc.
  - Ban kinh.
  - Toa.
  - Loai phong.
  - Gia tu/den, co don vi VND.
- Grid card phong responsive:
  - 1 cot tren man nho.
  - 2 cot tren man vua.
  - 3-4 cot tren man rong.
- Card phong:
  - Anh/toa nha placeholder hoac anh that.
  - Tag trang thai goc phai.
  - Ten toa - can.
  - Dien tich, tang, tien nghi.
  - Gia thue VND, coc VND.
  - Nut `Dat nhanh`.
  - Nut `Chi tiet`.
- Popup chi tiet:
  - Ben trai thong tin chia thanh card nho.
  - Ben phai ban do Leaflet/WebView2.
  - Bam ngoai popup thi dong.

## 8. Mau trang dang nhap

Nen viet lai form dang nhap theo kieu card trung tam:

```text
+--------------------------------+
| Logo SmartApart                |
| Quan ly cho thue nha           |
|                                |
| Ten dang nhap                  |
| [____________________]         |
| Mat khau                       |
| [____________________]         |
| [Dang nhap] [Thoat]            |
+--------------------------------+
```

Quy tac:

- Cong khach va cong noi bo tach ro.
- Cong khach chi cho role `KhachThue`.
- Cong noi bo chi cho `Admin`, `NhanVien`.
- Neu sai cong, bao ro: "Tai khoan nay khong thuoc cong dang nhap nay".
- Logo load tu `logo.png` va copy vao output khi build.

## 9. Quy tac DataGridView/ListView

Nen tao `SmartGridStyler.Apply(DataGridView grid)` de dung chung.

Style:

- Header nen xanh nhat hoac xanh dam, chu dam.
- Row cao 32-36 px.
- Alternating row mau rat nhe.
- Khong hien row header neu khong can.
- Selection mau xanh nhat, chu den.
- Tien format `N0` + `VND` neu hien text.
- Ngay format `dd/MM/yyyy`.
- Trang thai hien badge mau neu co the custom paint.

Cot nen an:

- Audit ky thuat neu khong phai man chi tiet.
- Ma noi bo khong can cho nguoi dung.
- Cac cot FK nen thay bang ten hien thi.

Vi du:

- Khong hien `MaKhach`, hien `TenKhach`.
- Khong hien `MaToa`, hien `TenToa`.
- Khong hien `MaLoai`, hien `TenLoai`.
- Khong hien `MaNhanVien`, hien `TenNhanVien`.

## 10. Cach viet helper UI mau

### SmartTheme

Nen chua:

- `Primary = Color.FromArgb(25, 118, 210)`
- `PrimaryDark = Color.FromArgb(13, 71, 161)`
- `Background = Color.FromArgb(248, 250, 252)`
- `Card = Color.White`
- `Border = Color.FromArgb(226, 232, 240)`
- `Text = Color.FromArgb(15, 23, 42)`
- `Muted = Color.FromArgb(100, 116, 139)`
- `Success`, `Warning`, `Danger`

### SmartCard

Dung `Panel` tu ve rounded rectangle trong `OnPaint`, tuong tu cach project game co `GetRoundedRectPath`.

Can ho tro:

- Radius.
- BorderColor.
- BorderThickness.
- BackColor.
- Padding.

### SmartDrawer

Can co:

- `ShowGroup(string title, IEnumerable<MenuItem>)`.
- `Hide()`.
- `SetActive(string key)`.
- Event click item.

Khong nen moi form tu tao drawer rieng; chi co `frmMain` quan ly drawer.

## 11. Thu tu thuc hien de it loi

Nen lam theo 8 giai doan:

### Giai doan 1: Dong bo theme

- Tao `SmartTheme`.
- Tao `SmartCard`.
- Tao `SmartGridStyler`.
- Ap dung vao 1-2 form mau truoc: `frmDashboard`, `frmCanHo`.

### Giai doan 2: Viet lai shell/main layout

- Sua `frmMain` thanh top bar + drawer + content area + footer.
- Dam bao phan quyen menu van dung.
- Dam bao logout dung.

### Giai doan 3: Viet lai dashboard

- Tao KPI cards.
- Them filter Ngay/Tuan/Thang/Nam.
- Chuyen chart sang LiveCharts neu muon dep hon.
- Bang can xu ly chi hien ten khach, ten can, trang thai.

### Giai doan 4: Chuan hoa CRUD base

- Refactor `CrudFormBase`.
- Tat ca form quan ly dung chung layout:
  - Search card.
  - Grid.
  - Detail panel.
  - Command buttons.
- Chuan hoa dropdown cho khoa ngoai.

### Giai doan 5: Viet lai trang tim tro

- Card responsive.
- Tag trang thai.
- Popup chi tiet + map.
- Dat nhanh.
- Validate gia, coc, email, sdt.

### Giai doan 6: Viet lai man nhan vien

- Trang cong viec: Cho coc, Cho ky, Hop dong sap het han, Hoa don can xu ly.
- Nut nghiep vu nhanh: Xac nhan coc, Lap hop dong, Lap hoa don.

### Giai doan 7: Viet lai trang khach hang

- Cac tab: Can ho da dat, Hop dong, Hoa don, Vi pham, Tai khoan.
- Hoa don co QR.
- Trang thai dong bo voi admin.

### Giai doan 8: Kiem thu va polish

- Build 0 warning.
- Test tung role.
- Test resize man hinh.
- Test font tieng Viet.
- Test database sandbox.

## 12. Checklist nghiep vu khong duoc pha

Khi thay UI, khong duoc lam sai cac luong sau:

- Khach xem tro khong can dang nhap.
- Khach dat phong thi moi tao tai khoan/phieu.
- Phieu dat truoc:
  - `ChoThanhToanCoc`
  - `DaThanhToanCoc`
  - `ChoKy`
  - `DaKyHD`
  - `Huy`
  - `HetHan`
- Nhan vien xac nhan coc moi chuyen phong sang cho ky/dat coc.
- Qua 24h chua coc thi het han va phong trong lai.
- Hop dong chi tao voi can ho trong hoac co phieu cho ky hop le.
- Hoa don trang thai tu tinh, khong cho nhap tay.
- Admin co bao cao va quan tri tai khoan.
- Nhan vien chi thay chuc nang duoc phan quyen.
- Khach chi thay du lieu cua chinh minh.

## 13. Tieu chuan pass UI moi

Mot man hinh dat yeu cau khi:

- Khong con chu mojibake/lien quan dau tieng Viet bi loi.
- Resize khong vo layout.
- Cac nut khong mat pixel/khong bi cat chu.
- Grid khong hien cot thua.
- Field FK la dropdown, khong bat go ma.
- Search co dau/khong dau hoat dong.
- Enter trong filter khong lam reset sai.
- Trang thai hien bang tag/badge de nhin.
- Build pass 0 warning, 0 error.

## 14. Kien truc de khong bi roi code

Nen giu quy tac:

- `Model`: chi entity.
- `DAL`: repository/dbcontext.
- `BLL`: nghiep vu, validation, trang thai.
- `GUI`: chi hien thi va goi service.
- Khong viet SQL truc tiep trong form.
- Khong tinh trang thai nghiep vu phuc tap trong UI.
- UI chi goi cac ham nhu:
  - `HopDongService.TrangThaiHienThi`
  - `HoaDonService.TinhTrangThai`
  - `PhieuDatTruocService.XacNhanDaNhanCoc`

## 15. Phuong an nang cap cao hon

Neu muon giao dien dep hon nua, co 3 huong:

### Phuong an A: WinForms nang cap vua phai

- Giu WinForms.
- Dung MaterialSkin + helper tu viet.
- Them LiveCharts.
- It rui ro nhat, phu hop bai .NET WinForms.

### Phuong an B: WinForms + WebView2 UI tung phan

- Cac man nhu tim tro, dashboard co the render HTML/CSS trong WebView2.
- BLL/DB van C#.
- UI dep hon nhung phuc tap hon.

### Phuong an C: Tach thanh Web App

- Backend .NET API.
- Frontend React/Vue/Blazor.
- UX tot nhat, nhung phai viet lai nhieu.

Voi bai hien tai, nen chon phuong an A truoc. Sau khi nghiep vu on dinh moi can tinh den B/C.

