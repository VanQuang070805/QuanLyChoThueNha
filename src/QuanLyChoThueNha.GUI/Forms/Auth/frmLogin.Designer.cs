namespace QuanLyChoThueNha.GUI.Forms.Auth
{
    partial class frmLogin
    {
        private System.ComponentModel.IContainer components = null;

        private MaterialSkin.Controls.MaterialTextBox txtTenDangNhap;
        private MaterialSkin.Controls.MaterialTextBox txtMatKhau;
        private MaterialSkin.Controls.MaterialButton btnDangNhap;
        private MaterialSkin.Controls.MaterialButton btnThoat;
        private MaterialSkin.Controls.MaterialLabel lblTieuDe;
        private MaterialSkin.Controls.MaterialLabel lblTenDangNhap;
        private MaterialSkin.Controls.MaterialLabel lblMatKhau;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtTenDangNhap = new MaterialSkin.Controls.MaterialTextBox();
            this.txtMatKhau     = new MaterialSkin.Controls.MaterialTextBox();
            this.btnDangNhap    = new MaterialSkin.Controls.MaterialButton();
            this.btnThoat       = new MaterialSkin.Controls.MaterialButton();
            this.lblTieuDe      = new MaterialSkin.Controls.MaterialLabel();
            this.lblTenDangNhap = new MaterialSkin.Controls.MaterialLabel();
            this.lblMatKhau     = new MaterialSkin.Controls.MaterialLabel();
            this.SuspendLayout();

            // lblTieuDe
            this.lblTieuDe.AutoSize  = true;
            this.lblTieuDe.Depth     = 0;
            this.lblTieuDe.Font      = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            this.lblTieuDe.Location  = new System.Drawing.Point(120, 100);
            this.lblTieuDe.Text      = "QUẢN LÝ CHO THUÊ NHÀ";

            // lblTenDangNhap
            this.lblTenDangNhap.AutoSize  = true;
            this.lblTenDangNhap.Depth     = 0;
            this.lblTenDangNhap.Font      = new System.Drawing.Font("Roboto", 11F);
            this.lblTenDangNhap.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            this.lblTenDangNhap.Location  = new System.Drawing.Point(80, 155);
            this.lblTenDangNhap.Text      = "Tên đăng nhập";

            // txtTenDangNhap
            this.txtTenDangNhap.Depth    = 0;
            this.txtTenDangNhap.Location = new System.Drawing.Point(80, 175);
            this.txtTenDangNhap.Size     = new System.Drawing.Size(340, 50);
            this.txtTenDangNhap.Hint     = "Nhập tên đăng nhập";
            this.txtTenDangNhap.Text     = "admin";  // mặc định để test

            // lblMatKhau
            this.lblMatKhau.AutoSize  = true;
            this.lblMatKhau.Depth     = 0;
            this.lblMatKhau.Font      = new System.Drawing.Font("Roboto", 11F);
            this.lblMatKhau.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            this.lblMatKhau.Location  = new System.Drawing.Point(80, 240);
            this.lblMatKhau.Text      = "Mật khẩu";

            // txtMatKhau
            this.txtMatKhau.Depth        = 0;
            this.txtMatKhau.Location     = new System.Drawing.Point(80, 260);
            this.txtMatKhau.Size         = new System.Drawing.Size(340, 50);
            this.txtMatKhau.Hint         = "Nhập mật khẩu";
            this.txtMatKhau.Password     = true;
            this.txtMatKhau.Text         = "Admin@123";  // mặc định để test
            this.txtMatKhau.KeyDown     += new System.Windows.Forms.KeyEventHandler(this.txtMatKhau_KeyDown);

            // btnDangNhap
            this.btnDangNhap.AutoSize    = true;
            this.btnDangNhap.Depth       = 0;
            this.btnDangNhap.HighEmphasis = true;
            this.btnDangNhap.Location    = new System.Drawing.Point(80, 340);
            this.btnDangNhap.Size        = new System.Drawing.Size(160, 36);
            this.btnDangNhap.Text        = "ĐĂNG NHẬP";
            this.btnDangNhap.Type        = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnDangNhap.Click      += new System.EventHandler(this.btnDangNhap_Click);

            // btnThoat
            this.btnThoat.AutoSize  = true;
            this.btnThoat.Depth     = 0;
            this.btnThoat.Location  = new System.Drawing.Point(260, 340);
            this.btnThoat.Size      = new System.Drawing.Size(160, 36);
            this.btnThoat.Text      = "THOÁT";
            this.btnThoat.Type      = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnThoat.Click    += new System.EventHandler(this.btnThoat_Click);

            // frmLogin
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize    = new System.Drawing.Size(500, 420);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTieuDe, this.lblTenDangNhap, this.txtTenDangNhap,
                this.lblMatKhau, this.txtMatKhau, this.btnDangNhap, this.btnThoat
            });
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "frmLogin";
            this.Sizable         = false;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "Đăng nhập";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
