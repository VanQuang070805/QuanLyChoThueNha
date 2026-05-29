using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.Auth
{
    public class frmQuanLyTaiKhoan : CrudFormBase<TaiKhoan>
    {
        private readonly TaiKhoanService _service = new TaiKhoanService();

        public frmQuanLyTaiKhoan() : base("Quan ly Tai khoan", Fields())
        {
            AddCommandButton("Tao moi", BtnTaoMoi_Click);
            AddCommandButton("Khoa", BtnKhoa_Click);
            AddCommandButton("Mo khoa", BtnMoKhoa_Click);
            SetEditorValue("MaTaiKhoan", _service.LayMaTaiKhoanTiepTheo());
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            return new[]
            {
                new FieldDefinition("MaTaiKhoan", "Ma tai khoan", typeof(string), true),
                new FieldDefinition("TenDangNhap", "Ten dang nhap"),
                new FieldDefinition("Email", "Email"),
                new FieldDefinition("SoDienThoai", "So dien thoai"),
                new FieldDefinition("VaiTro", "Vai tro", typeof(string), false,
                    new[] { "Admin", "NhanVien" }),
                new FieldDefinition("TrangThai", "Dang hoat dong", typeof(bool)),
                new FieldDefinition("NgayTao", "Ngay tao", typeof(DateTime), true)
            };
        }

        // Hien thi Admin/NhanVien kem ma nguoi dung (MaAdmin/MaNhanVien). KhachThue quan ly rieng.
        protected override IEnumerable<TaiKhoan> GetItems()
        {
            return _service.LayTatCaVoiMaNguoiDung();
        }

        protected override void AfterGridBound()
        {
            // An cot hash mat khau - khong can hien thi
            if (Grid.Columns.Contains("MatKhauHash"))
                Grid.Columns["MatKhauHash"].Visible = false;
        }

        protected override bool AddItem(TaiKhoan item, out string error)
        {
            return _service.TaoTaiKhoan(item.TenDangNhap, "Admin@123", item.Email,
                item.SoDienThoai, item.VaiTro, out error);
        }

        protected override bool UpdateItem(TaiKhoan item, out string error)
        {
            return _service.CapNhatTaiKhoan(item, SessionContext.VaiTro, out error);
        }

        protected override bool DeleteItem(TaiKhoan item, out string error)
        {
            error = "Khong xoa tai khoan truc tiep. Hay khoa tai khoan neu khong con su dung.";
            return false;
        }

        private void BtnTaoMoi_Click(object sender, EventArgs e)
        {
            GoToAddMode();
        }

        protected override void OnAfterAdd()
        {
            DatMaTaiKhoanMoi();
        }

        private void DatMaTaiKhoanMoi()
        {
            SetEditorValue("MaTaiKhoan", _service.LayMaTaiKhoanTiepTheo());
            SetEditorValue("VaiTro", "NhanVien");
            SetEditorValue("TrangThai", true);
        }

        private void BtnKhoa_Click(object sender, EventArgs e)
        {
            ChangeStatus(false);
        }

        private void BtnMoKhoa_Click(object sender, EventArgs e)
        {
            ChangeStatus(true);
        }

        private void ChangeStatus(bool status)
        {
            var item = CurrentItem;
            if (item == null)
            {
                ShowError("Chon tai khoan.");
                return;
            }
            string error;
            if (!_service.DoiTrangThai(item.MaTaiKhoan, status, SessionContext.VaiTro, out error))
            {
                ShowError(error);
                return;
            }
            ReloadData();
        }
    }
}
