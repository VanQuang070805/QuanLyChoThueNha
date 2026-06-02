namespace QuanLyChoThueNha.GUI.Forms.Auth
{
    partial class frmLogin
    {
        private System.ComponentModel.IContainer components = null;

        private MaterialSkin.Controls.MaterialTextBox txtTenDangNhap;
        private MaterialSkin.Controls.MaterialTextBox txtMatKhau;
        private QuanLyChoThueNha.GUI.Controls.RoundedButton btnDangNhap;
        private QuanLyChoThueNha.GUI.Controls.RoundedButton btnThoat;
        private MaterialSkin.Controls.MaterialLabel lblTieuDe;
        private MaterialSkin.Controls.MaterialLabel lblTenDangNhap;
        private MaterialSkin.Controls.MaterialLabel lblMatKhau;
        private System.Windows.Forms.Label lblHuongDan;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtTenDangNhap = new MaterialSkin.Controls.MaterialTextBox();
            this.txtMatKhau = new MaterialSkin.Controls.MaterialTextBox();
            this.btnDangNhap = new QuanLyChoThueNha.GUI.Controls.RoundedButton();
            this.btnThoat = new QuanLyChoThueNha.GUI.Controls.RoundedButton();
            this.lblTieuDe = new MaterialSkin.Controls.MaterialLabel();
            this.lblTenDangNhap = new MaterialSkin.Controls.MaterialLabel();
            this.lblMatKhau = new MaterialSkin.Controls.MaterialLabel();
            this.lblHuongDan = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Depth = 0;
            this.lblTieuDe.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            this.lblTieuDe.Location = new System.Drawing.Point(80, 88);
            this.lblTieuDe.Text = "Cổng nội bộ SmartApart";

            this.lblHuongDan.AutoSize = false;
            this.lblHuongDan.Location = new System.Drawing.Point(80, 120);
            this.lblHuongDan.Size = new System.Drawing.Size(380, 56);
            this.lblHuongDan.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHuongDan.ForeColor = System.Drawing.Color.FromArgb(85, 96, 126);
            this.lblHuongDan.Text = "Dành cho Admin/Nhân viên. Nhập tài khoản nội bộ để quản lý căn hộ, phiếu đặt trước, hợp đồng, hóa đơn và email.";

            this.lblTenDangNhap.AutoSize = true;
            this.lblTenDangNhap.Depth = 0;
            this.lblTenDangNhap.Font = new System.Drawing.Font("Roboto", 11F);
            this.lblTenDangNhap.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            this.lblTenDangNhap.Location = new System.Drawing.Point(80, 188);
            this.lblTenDangNhap.Text = "Tên đăng nhập";

            this.txtTenDangNhap.Depth = 0;
            this.txtTenDangNhap.Location = new System.Drawing.Point(80, 208);
            this.txtTenDangNhap.Size = new System.Drawing.Size(340, 50);
            this.txtTenDangNhap.Hint = "Nhập tên đăng nhập";

            this.lblMatKhau.AutoSize = true;
            this.lblMatKhau.Depth = 0;
            this.lblMatKhau.Font = new System.Drawing.Font("Roboto", 11F);
            this.lblMatKhau.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            this.lblMatKhau.Location = new System.Drawing.Point(80, 273);
            this.lblMatKhau.Text = "Mật khẩu";

            this.txtMatKhau.Depth = 0;
            this.txtMatKhau.Location = new System.Drawing.Point(80, 293);
            this.txtMatKhau.Size = new System.Drawing.Size(340, 50);
            this.txtMatKhau.Hint = "Nhập mật khẩu";
            this.txtMatKhau.Password = true;
            this.txtMatKhau.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMatKhau_KeyDown);

            this.btnDangNhap.Location = new System.Drawing.Point(80, 372);
            this.btnDangNhap.Size = new System.Drawing.Size(160, 36);
            this.btnDangNhap.Text = "ĐĂNG NHẬP";
            this.btnDangNhap.Radius = 10;
            this.btnDangNhap.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnDangNhap.BorderColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnDangNhap.ForeColor = System.Drawing.Color.White;
            this.btnDangNhap.Click += new System.EventHandler(this.btnDangNhap_Click);

            this.btnThoat.Location = new System.Drawing.Point(260, 372);
            this.btnThoat.Size = new System.Drawing.Size(160, 36);
            this.btnThoat.Text = "ĐÓNG CỔNG";
            this.btnThoat.Radius = 10;
            this.btnThoat.BackColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnThoat.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnThoat.ForeColor = System.Drawing.Color.White;
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 460);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTieuDe, this.lblHuongDan, this.lblTenDangNhap, this.txtTenDangNhap,
                this.lblMatKhau, this.txtMatKhau, this.btnDangNhap, this.btnThoat
            });
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmLogin";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
