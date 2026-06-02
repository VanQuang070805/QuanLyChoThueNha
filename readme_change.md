# Bang So Sanh Thay Doi

Tai lieu nay dung de doi chieu nhanh giua ban truoc va ban hien tai trong session sua loi.

| Khu vuc | Ban truoc | Ban hien tai |
| --- | --- | --- |
| Bao cao doanh thu | Chi tinh hoa don da tra theo ngay thanh toan, thang co hoa don chua tra bi hien 0 | Tinh theo ky hoa don/ngay dao han va so tien phai tra, hien du doanh thu va so hoa don |
| Bieu do bao cao | Nhan truc ngay bi chong len nhau | Tu gian nhan truc X de de doc hon |
| Dashboard | Thieu tong doanh thu va so hoa don | Bo sung KPI tong doanh thu, so hoa don, doanh thu thang, hoa don chua tra |
| Xoa can ho | Phieu dat truoc da huy van chan xoa | Chi phieu dang xu ly moi chan xoa; phieu huy/het han duoc don truoc khi xoa |
| Xoa danh muc tai san | De gap loi khoa ngoai khi xoa khu vuc, toa, loai can ho, tien nghi | Kiem tra rang buoc va tra thong bao ro rang truoc khi xoa |
| Quan ly can ho | Co the nham tinh trang he thong; tien nghi chi chon tung cai | Tinh trang `DaDatCoc`/`DangThue` do he thong dong bo; tien nghi chon nhieu bang checklist |
| Hoa don | DataGridView hien ma nhan vien, thieu ten toa/ten can ho khi moi load | Hien ten nhan vien, ten toa, ten can ho ngay khi mo form |
| Phieu tra nha | Thong tin ten nhan vien/toa/can ho phu thuoc bam lam moi | Nap san thong tin hien thi truoc khi bind DataGridView |
| Xu ly vi pham | DataGridView thieu/cap nhat cham thong tin hien thi | Hien du ten nhan vien, ten toa, ten can ho va can cot lai |
| Gia han hop dong | Co nut chap thuan/tu choi; grid thieu ten nhan vien/toa/can ho | Bo nut chap thuan/tu choi; hien ten nhan vien, ten toa, ten can ho |
| Phieu dat truoc | Cot ten toa loi; ma nhan vien hien truc tiep | Sua ten toa, them ten can ho, thay ma nhan vien bang ten nhan vien |
| Quan ly khach thue | Click grid co luc khong hien textbox ben phai, phai re chuot de thay text | Click grid bind sang panel phai va ep refresh textbox ngay |
| Quan ly tai khoan khach | Textbox co luc chi hien sau khi hover; loi BeginInvoke khi handle chua tao | Ep refresh an toan, co kiem tra `IsHandleCreated` |
| Helper UI | Moi form tu xu ly cot grid rieng | Them `GridDisplayHelper` dung chung cho cot grid |
| Entity hien thi | Thieu field rieng cho cot ten trong grid | Them `[NotMapped]` cho cac field hien thi, khong anh huong database |
| Luu DB | `UnitOfWork.Complete()` co typo `phi_context` | Sua ve `_context.SaveChanges()` |

## File chinh da tac dong

| Nhom | File |
| --- | --- |
| BLL services | `BaoCaoService.cs`, `CanHoService.cs`, `KhuVucService.cs`, `LoaiCanHoService.cs`, `TienNghiService.cs`, `ToaService.cs` |
| DAL | `UnitOfWork.cs`, `App.config` |
| GUI shared | `GridDisplayHelper.cs`, `QuanLyChoThueNha.GUI.csproj` |
| GUI auth | `frmQuanLyTaiKhoan.cs`, `frmQuanLyTaiKhoanKhach.cs` |
| GUI bao cao | `frmBaoCao.cs`, `frmDashboard.cs` |
| GUI hop dong | `frmGiaHanHopDong.cs`, `frmKhachThue.cs`, `frmPhieuDatTruoc.cs` |
| GUI tai san | `frmCanHo.cs`, `frmKhuVuc.cs`, `frmLoaiCanHo.cs`, `frmTienNghi.cs`, `frmToa.cs` |
| GUI tra nha/thanh toan | `frmHoaDonThanhToan.cs`, `frmPhieuTraNha.cs`, `frmPhieuXuLyViPham.cs` |
| Model entities | `GiaHanHopDong.cs`, `HoaDonThanhToan.cs`, `PhieuDatTruoc.cs`, `PhieuTraNha.cs`, `PhieuXuLyViPham.cs` |

## Trang thai build

```powershell
dotnet build .\QuanLyChoThueNha.sln -p:OutDir=.\build-check\
```

Ket qua: build thanh cong voi `0 Warning`, `0 Error`.

