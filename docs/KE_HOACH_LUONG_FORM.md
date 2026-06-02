# Ke hoach luong form va phan quyen

Tai lieu nay mo ta thu tu thuc hien cac luong chinh trong ung dung WinForms QuanLyChoThueNha.

## 1. Tai khoan thu nghiem

Mat khau mac dinh: `Admin@123`.

| Vai tro | Tai khoan | Muc dich |
|---|---|---|
| Admin | `admin_test` | Kiem thu quan tri tai khoan, bao cao, dashboard day du |
| Nhan vien | `nv_test` | Kiem thu nghiep vu van hanh, khong thay quan tri admin |
| Khach hang | `khach_test` | Kiem thu khach dang nhap va dat phong |
| Khach hang | `khach_demo` | Kiem thu khoa/mo khoa, loc danh sach khach |

Script tao tai khoan: `docs/DATABASE/07_test_accounts.sql`.

## 2. Luong admin

1. Dang nhap bang `admin_test`.
2. Vao dashboard mac dinh sau dang nhap.
3. Xem KPI, doanh thu theo thang, tinh trang can ho, hoa don, hop dong sap het han.
4. Tao tai khoan admin/nhan vien trong form Tai khoan.
5. Tao khach hang kem tai khoan dang nhap trong form Khach thue.
6. Sao chep thong tin dang nhap de gui cho khach hang.
7. Khoa/mo khoa tai khoan khi can.

Admin khong thay trang khach hang ca nhan vi trang do chi danh cho vai tro `KhachThue`.

## 3. Luong nhan vien

1. Dang nhap bang `nv_test`.
2. Vao trang cong viec nhan vien mac dinh sau dang nhap.
3. Chi thay cac nhom nghiep vu duoc cap:
   - Tai san va danh muc.
   - Hop dong.
   - Thanh toan va tra nha.
   - Dashboard van hanh.
4. Khong thay quan tri tai khoan admin/nhan vien.
5. Khong thay bao cao quan tri day du.
6. Tao khach thue kem tai khoan, sao chep thong tin de gui cho khach.
7. Theo doi nhanh cac viec can xu ly: phieu dat phong cho ky hop dong, hop dong sap het han, hoa don can thu.
8. Xu ly phieu dat phong cua khach, ky hop dong, tao hoa don, tra nha/vi pham.

Dashboard cua nhan vien khong hien thi doanh thu thang; thay vao do la thong tin van hanh can theo doi.

## 4. Luong khach hang

1. Dang nhap bang `khach_test` hoac tai khoan moi duoc admin/nhan vien tao.
2. Ung dung mo thang trang khach hang.
3. Loc can ho/phong dang trong theo toa, loai va gia toi da.
4. Chon can ho de xem chi tiet.
5. Bam `Dat phong` de tao phieu dat truoc.
6. Xem lai phieu dat truoc, hop dong va hoa don cua chinh minh.

Khach hang khong thay dashboard admin/nhan vien, danh muc tai san, hop dong toan he thong, quan tri tai khoan, hoac bao cao.

## 5. Thu tu kiem thu

1. Build solution.
2. Neu dung database cu, chay script `08_audit_nguoi_thao_tac.sql`.
3. Chay script `07_test_accounts.sql`.
4. Dang nhap lan luot `admin_test`, `nv_test`, `khach_test`.
5. Kiem tra menu hien/an theo vai tro.
6. Tao mot khach moi kem tai khoan trong form Khach thue.
7. Dang nhap bang tai khoan khach moi.
8. Dat phong tu trang khach hang.
9. Dang nhap nhan vien, xem phieu dat truoc va tiep tuc xu ly nghiep vu.
10. Dang nhap admin, xem dashboard/bieu do va quan tri tai khoan.

Co the chay nhanh cac buoc smoke test tren bang PowerShell:

```powershell
.\docs\SMOKE_TEST_LUONG_FORM.ps1
```

Script nay kiem tra dang nhap 3 vai tro, man hinh mac dinh theo vai tro, an/hien menu, trang phong dang the cua khach, tao tai khoan khach tam, khach dang nhap, va luong tao/huy phieu dat phong.

## 6. Ma tran phan quyen menu

| Chuc nang/menu | Admin | Nhan vien | Khach hang |
|---|---:|---:|---:|
| Dashboard quan tri | Co | Khong | Khong |
| Trang cong viec nhan vien | Khong mac dinh | Co | Khong |
| Trang khach hang dat phong | Khong | Khong | Co |
| Khu vuc / Toa nha / Loai can ho | Co | Co | Khong |
| Can ho / Tien nghi / Gia dich vu | Co | Co | Khong |
| Khach thue | Co | Co | Khong |
| Phieu dat truoc | Co | Co | Chi xem cua minh trong trang khach |
| Hop dong / Gia han | Co | Co | Chi xem cua minh trong trang khach |
| Hoa don / Tra nha / Vi pham | Co | Co | Chi xem hoa don cua minh |
| Quan ly tai khoan admin/nhan vien | Co | Khong | Khong |
| Quan ly tai khoan khach | Co | Khong | Khong |
| Bao cao quan tri | Co | Khong | Khong |

## 7. Kich ban demo de chay tren lop

### Kich ban 1: Admin quan tri va xem bao cao

1. Dang nhap `admin_test / Admin@123`.
2. Xac nhan man hinh mac dinh la Dashboard.
3. Xac nhan co KPI va nhieu bieu do: doanh thu, tinh trang can ho, hoa don, hop dong sap het han.
4. Mo `Tai khoan` de xem tai khoan admin/nhan vien.
5. Mo `Tai khoan khach` de xem khach hang.
6. Mo `Bao cao & thong ke` de xem/xuat bao cao doanh thu.

Ket qua dung: admin thay day du menu quan tri va bao cao.

### Kich ban 2: Nhan vien tao tai khoan gui khach

1. Dang nhap `nv_test / Admin@123`.
2. Xac nhan man hinh mac dinh la Trang cong viec nhan vien.
3. Xac nhan khong thay `Tai khoan`, `Tai khoan khach`, `Bao cao & thong ke`.
4. Mo `Khach thue`.
5. Tao mot khach moi kem ten dang nhap va mat khau tam.
6. Sau khi luu, bam `Copy tai khoan`.
7. Dang xuat nhan vien.
8. Dang nhap bang tai khoan khach vua tao.

Ket qua dung: khach moi dang nhap duoc va chi vao trang khach hang.

### Kich ban 3: Khach hang dat phong

1. Dang nhap `khach_test / Admin@123`.
2. Xac nhan man hinh mac dinh la Trang khach hang.
3. Dung bo loc toa, loai, gia toi da.
4. Chon the phong hoac bam `Dat ngay`.
5. Xac nhan he thong tao phieu dat truoc.
6. Xem phieu trong tab `Phieu dat truoc`.

Ket qua dung: can ho duoc giu cho, phieu co trang thai `ChoKy`, khach khong thay menu quan tri.

### Kich ban 4: Nhan vien tiep tuc xu ly dat phong

1. Dang nhap lai `nv_test`.
2. Vao Trang cong viec nhan vien.
3. Kiem tra phieu dat phong moi xuat hien trong bang `Phieu dat phong cho ky hop dong`.
4. Mo form `Dat truoc` hoac `Hop dong` de xu ly ky hop dong.

Ket qua dung: nhan vien thay viec can xu ly nhung van khong thay bao cao/quan tri admin.

## 8. Tieu chi pass/fail

| Hang muc | Pass | Fail |
|---|---|---|
| Build | Solution build khong loi | Co loi compile hoac thieu reference |
| Tai khoan test | 4 tai khoan test dang nhap duoc | Sai mat khau, bi khoa, thieu lien ket nguoi dung |
| Phan quyen admin | Admin thay quan tri va bao cao | Admin thieu menu quan tri |
| Phan quyen nhan vien | Nhan vien thay nghiep vu, khong thay quan tri/bao cao admin | Nhan vien thay menu quan tri nhay cam |
| Phan quyen khach | Khach chi thay trang khach hang | Khach thay menu noi bo |
| Tao tai khoan khach | Tao xong copy duoc thong tin gui khach, khach dang nhap duoc | Tao loi FK, khach khong dang nhap duoc |
| Dat phong | Tao duoc phieu dat truoc va phong doi trang thai | Khong tao phieu hoac phong van trong |
| Dashboard | Co nhieu bieu do, admin thay doanh thu, nhan vien khong thay doanh thu | Dashboard loi runtime hoac lo thong tin doanh thu cho nhan vien |

## 9. Ghi chu ve file zip giao dien

Workspace hien tai chua co file zip mau giao dien. Khi co file zip, can doi chieu mau sac, bo cuc, typography, icon va khoang cach cua cac trang khach hang/nhan vien/admin theo mau do.
