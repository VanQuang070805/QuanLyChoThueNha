# Nhat Ky Thay Doi Trong Session

Ngay cap nhat: 01/06/2026

Nhanh tao moi: `Phieu/change_log`

Tai lieu nay tong hop cac thay doi dang co trong source code hien tai so voi ban truoc tren nhanh goc.

## 1. Bao cao va Dashboard

- Sua cach tinh doanh thu trong `BaoCaoService`:
  - Doanh thu thang, doanh thu theo ky, tong doanh thu va top can ho doanh thu tinh theo `NgayDaoHan` va `SoTienPhaiTra`.
  - Khong phu thuoc vao `NgayThanhToan` nen hoa don chua thanh toan van duoc ghi nhan vao bao cao doanh thu.
- Dashboard bo sung/tinh lai:
  - Tong doanh thu.
  - So hoa don.
  - Doanh thu thang.
  - Hoa don chua tra.
- Bieu do bao cao duoc gian nhan truc X de tranh chong chu khi hien thi nhieu ngay.

## 2. Xoa du lieu va khoa ngoai

- Them xoa an toan cho:
  - Can ho.
  - Khu vuc.
  - Toa nha.
  - Loai can ho.
  - Tien nghi.
- Khi xoa can ho:
  - Phieu dat truoc dang xu ly van chan xoa.
  - Phieu dat truoc da `Huy` hoac `HetHan` khong con chan xoa.
  - Tu dong don phieu dat truoc da dong va email log lien quan truoc khi xoa can ho.
- An nut xoa trong man hinh quan ly tai khoan de tranh loi khoa ngoai.

## 3. Quan ly can ho va tien nghi

- Tinh trang can ho duoc dong bo theo nghiep vu:
  - Co hop dong hieu luc thi `DangThue`.
  - Co phieu dat truoc dang xu ly thi `DaDatCoc`.
  - Phieu dat truoc huy/het han thi phong ve trang thai `Trong` neu khong con rang buoc khac.
- Khong cho chon thu cong `DaDatCoc` va `DangThue` trong quan ly can ho.
- Tien nghi cua can ho doi tu combobox chon 1 sang checklist chon nhieu.
- Nut `Gan tien nghi` dong bo tat ca tien nghi dang tick cho can ho.
- Nut `Go tien nghi` bo toan bo tien nghi dang gan cua can ho.

## 4. Hoa don, phieu tra nha, xu ly vi pham

- DataGridView hien thi them:
  - Ten nhan vien.
  - Ten toa.
  - Ten can ho.
- An cot ma nhan vien trong DataGridView, thay bang ten nhan vien.
- Du lieu hien thi duoc nap san truoc khi gan `DataSource`, khong can bam `Lam moi` moi thay day du.
- Them helper `GridDisplayHelper` de can bang cot, dinh dang cot tien va an/hien cot dung chung.
- Them cac field `[NotMapped]` cho entity hien thi:
  - `HoaDonThanhToan`.
  - `PhieuTraNha`.
  - `PhieuXuLyViPham`.

## 5. Hop dong, gia han, phieu dat truoc

- Quan ly gia han hop dong:
  - Them cot ten nhan vien, ten toa, ten can ho.
  - An cot ma nhan vien.
  - Du lieu hien thi ngay khi mo form.
  - Bo nut `Chap thuan` va `Tu choi` tren giao dien.
- Quan ly phieu dat truoc:
  - Sua loi cot ten toa khong hien dung.
  - Them ten can ho va ten nhan vien.
  - An ma can ho va ma nhan vien tren DataGridView.
  - Khong can bam `Lam moi` moi hien du lieu.
- Them cac field `[NotMapped]` cho entity:
  - `GiaHanHopDong`.
  - `PhieuDatTruoc`.

## 6. Quan ly khach thue va tai khoan khach

- Quan ly khach thue:
  - Click vao DataGridView se hien thong tin sang panel ben phai.
  - Co the bam `Sua thong tin` sau khi chon dong.
  - Ep refresh textbox sau khi bind du lieu de khong can re chuot vao textbox moi thay noi dung.
- Quan ly tai khoan khach:
  - Ep refresh textbox sau khi bind du lieu.
  - Xu ly loi `BeginInvoke` khi handle cua form chua duoc tao bang cach kiem tra `IsHandleCreated`.

## 7. Cau hinh va project

- Them file helper moi vao project WinForms:
  - `src/QuanLyChoThueNha.GUI/Forms/Shared/GridDisplayHelper.cs`
- Sua loi typo trong `UnitOfWork.Complete()`:
  - `phi_context.SaveChanges()` thanh `_context.SaveChanges()`.
- App config co thay doi theo moi truong hien tai cua project.

## 8. Kiem tra

Lenh da chay:

```powershell
dotnet build .\QuanLyChoThueNha.sln -p:OutDir=.\build-check\
```

Ket qua gan nhat:

- Build thanh cong.
- `0 Warning`.
- `0 Error`.

