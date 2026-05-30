param(
    [string]$SqlServer = ".\SQLEXPRESS"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$bin = Join-Path $root "src\QuanLyChoThueNha.GUI\bin\Debug"
$exeConfig = Join-Path $bin "QuanLyChoThueNha.GUI.exe.config"

function Reset-AppConfig {
    Add-Type -AssemblyName System.Configuration
    [AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", $exeConfig)
    try {
        $cm = [System.Configuration.ConfigurationManager]
        $field = $cm.GetField("s_initState", [System.Reflection.BindingFlags] "NonPublic, Static")
        if ($field) { $field.SetValue($null, 0) }
        $field = $cm.GetField("s_configSystem", [System.Reflection.BindingFlags] "NonPublic, Static")
        if ($field) { $field.SetValue($null, $null) }
    } catch {
        # Older .NET Framework builds may not expose the same private fields.
    }
}

function Load-AppAssemblies {
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing
    Add-Type -AssemblyName System.Configuration

    [AppDomain]::CurrentDomain.add_AssemblyResolve({
        param($sender, $args)
        $name = New-Object System.Reflection.AssemblyName($args.Name)
        $candidate = Join-Path $bin ($name.Name + ".dll")
        if (Test-Path $candidate) {
            return [System.Reflection.Assembly]::LoadFrom($candidate)
        }
        return $null
    }) | Out-Null

    foreach ($dependency in @("System.Buffers.dll", "System.Memory.dll", "System.Numerics.Vectors.dll", "System.Runtime.CompilerServices.Unsafe.dll")) {
        $path = Join-Path $bin $dependency
        if (Test-Path $path) {
            [System.Reflection.Assembly]::LoadFrom($path) | Out-Null
        }
    }

    $script:GuiAssembly = [System.Reflection.Assembly]::LoadFrom((Join-Path $bin "QuanLyChoThueNha.GUI.exe"))
    [System.Reflection.Assembly]::LoadFrom((Join-Path $bin "QuanLyChoThueNha.BLL.dll")) | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Join-Path $bin "QuanLyChoThueNha.DAL.dll")) | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Join-Path $bin "QuanLyChoThueNha.Model.dll")) | Out-Null

    [System.Windows.Forms.Application]::EnableVisualStyles()
    [System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)
}

function Login-As([string]$UserName) {
    $auth = New-Object QuanLyChoThueNha.BLL.Services.AuthService
    $errorMessage = ""
    $ok = $auth.DangNhap($UserName, "Admin@123", [ref]$errorMessage)
    if (-not $ok) {
        throw ("Login failed for " + $UserName + ": " + $errorMessage)
    }
}

function Show-Form([string]$TypeName) {
    $type = $script:GuiAssembly.GetType($TypeName, $true)
    $form = [Activator]::CreateInstance($type)
    $form.Show()
    [System.Windows.Forms.Application]::DoEvents()
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.Application]::DoEvents()
    return $form
}

function Get-PrivateField($Object, [string]$FieldName) {
    $field = $Object.GetType().GetField($FieldName, [System.Reflection.BindingFlags] "NonPublic, Instance")
    if (-not $field) {
        throw "Missing private field $FieldName on $($Object.GetType().FullName)"
    }
    return $field.GetValue($Object)
}

function Assert-DefaultForm([string]$UserName, [string]$ExpectedFormName) {
    Login-As $UserName
    $main = Show-Form "QuanLyChoThueNha.GUI.Forms.frmMain"
    try {
        $panel = Get-PrivateField $main "panelNoidung"
        if ($panel.Controls.Count -eq 0) {
            throw "No default child form was opened for $UserName"
        }
        $actual = $panel.Controls[0].GetType().Name
        if ($actual -ne $ExpectedFormName) {
            throw "Default form mismatch for $UserName. Expected $ExpectedFormName, actual $actual"
        }
    } finally {
        $main.Close()
        $main.Dispose()
    }
}

function Assert-MenuVisibility([string]$UserName, [hashtable]$Expected) {
    Login-As $UserName
    $main = Show-Form "QuanLyChoThueNha.GUI.Forms.frmMain"
    try {
        foreach ($key in $Expected.Keys) {
            $control = Get-PrivateField $main $key
            if ($control.Visible -ne $Expected[$key]) {
                throw "Visibility mismatch for $UserName/$key. Expected $($Expected[$key]), actual $($control.Visible)"
            }
        }
    } finally {
        $main.Close()
        $main.Dispose()
    }
}

function Assert-CreateCustomerLogin {
    $user = "khach_flow_test"
    $password = "Admin@123"

    Cleanup-TempCustomer $user

    $khachSvc = New-Object QuanLyChoThueNha.BLL.Services.KhachThueService
    $authSvc = New-Object QuanLyChoThueNha.BLL.Services.AuthService
    $khach = New-Object QuanLyChoThueNha.Model.Entities.KhachThue
    $khach.HoTen = "Khach Flow Test"
    $khach.SoCMND = "091234567890"
    $khach.DiaChi = "Ha Noi"
    $khach.NgaySinh = [DateTime]"1997-01-01"

    $errorMessage = ""
    $ok = $khachSvc.TaoKhachKemTaiKhoan($khach, $user, $password, "khach.flow.test@example.com", "0912345678", [ref]$errorMessage)
    if (-not $ok) {
        throw "Create customer account failed: $errorMessage"
    }

    $errorMessage = ""
    $ok = $authSvc.DangNhap($user, $password, [ref]$errorMessage)
    if (-not $ok) {
        throw "New customer login failed: $errorMessage"
    }

    Cleanup-TempCustomer $user
}

function Assert-PublicSearchForm {
    $form = Show-Form "QuanLyChoThueNha.GUI.Forms.KhachHang.frmTimTroPublic"
    try {
        $roomCards = Get-PrivateField $form "_roomCards"
        if ($roomCards.Controls.Count -lt 1) {
            throw "Public room search did not render any room card or empty-state label"
        }
    } finally {
        $form.Close()
        $form.Dispose()
    }
}

function Assert-BookingFlow {
    $canHoSvc = New-Object QuanLyChoThueNha.BLL.Services.CanHoService
    $phieuSvc = New-Object QuanLyChoThueNha.BLL.Services.PhieuDatTruocService
    $canHo = @($canHoSvc.LayTheoTinhTrang("Trong") | Select-Object -First 1)[0]
    if ($null -eq $canHo) {
        Write-Host "SKIP booking flow: no empty room/apartment"
        return
    }

    $phieu = New-Object QuanLyChoThueNha.Model.Entities.PhieuDatTruoc
    $phieu.MaCanHo = $canHo.MaCanHo
    $phieu.MaKhach = "KH903"
    if ($canHo.TienCocNiemYet -gt 0) {
        $phieu.SoTienDatCoc = $canHo.TienCocNiemYet
    } else {
        $phieu.SoTienDatCoc = $canHo.GiaThueNiemYet
    }
    $phieu.NgayHetHan = [DateTime]::Today.AddDays(3)
    $phieu.PhuongThucThanhToan = "SmokeTest"
    $phieu.GhiChu = "Smoke test card booking"

    $errorMessage = ""
    $ok = $phieuSvc.TaoPhieu($phieu, [ref]$errorMessage)
    if (-not $ok) {
        throw "Booking failed: $errorMessage"
    }

    $created = @($phieuSvc.LayTatCa() |
        Where-Object { $_.MaKhach -eq "KH903" -and $_.MaCanHo -eq $canHo.MaCanHo -and $_.TrangThai -eq "ChoKy" } |
        Sort-Object NgayDatCoc -Descending |
        Select-Object -First 1)[0]

    if ($null -eq $created) {
        throw "Created reservation was not found"
    }

    $ok = $phieuSvc.HuyPhieu($created.MaPhieuDatTruoc, [ref]$errorMessage)
    if (-not $ok) {
        throw "Booking cleanup failed: $errorMessage"
    }
}

function Cleanup-TempCustomer([string]$UserName) {
    $query = @"
DECLARE @MaTaiKhoan VARCHAR(50);
SELECT @MaTaiKhoan = MaTaiKhoan FROM TaiKhoan WHERE TenDangNhap = '$UserName';
DELETE FROM KhachThue WHERE MaTaiKhoan = @MaTaiKhoan;
DELETE FROM TaiKhoan WHERE MaTaiKhoan = @MaTaiKhoan;
"@
    sqlcmd -S $SqlServer -E -d QuanLyChoThueNha -Q $query | Out-Null
}

function Cleanup-SmokeReservations {
    sqlcmd -S $SqlServer -E -d QuanLyChoThueNha -Q "DELETE FROM PhieuDatTruoc WHERE PhuongThucThanhToan = 'SmokeTest';" | Out-Null
}

Reset-AppConfig
Load-AppAssemblies

Assert-PublicSearchForm

Assert-DefaultForm "admin_test" "frmDashboard"
Assert-DefaultForm "nv_test" "frmNhanVienHome"
Assert-DefaultForm "khach_test" "frmKhachHangHome"

Assert-MenuVisibility "admin_test" @{
    btnBaoCao = $true
    btnQuanLyTaiKhoan = $true
    btnKhachHangHome = $false
}
Assert-MenuVisibility "nv_test" @{
    btnBaoCao = $false
    btnQuanLyTaiKhoan = $false
    btnHopDong = $true
}
Assert-MenuVisibility "khach_test" @{
    btnKhachHangHome = $true
    btnCanHo = $false
    btnHopDong = $false
}

Login-As "khach_test"
$customerHome = Show-Form "QuanLyChoThueNha.GUI.Forms.KhachHang.frmKhachHangHome"
try {
    $roomCards = Get-PrivateField $customerHome "roomCards"
    if ($roomCards.Controls.Count -lt 1) {
        throw "Customer home did not render any room card or empty-state label"
    }
} finally {
    $customerHome.Close()
    $customerHome.Dispose()
}

Login-As "nv_test"
$employeeHome = Show-Form "QuanLyChoThueNha.GUI.Forms.NhanVien.frmNhanVienHome"
$employeeHome.Close()
$employeeHome.Dispose()

Assert-CreateCustomerLogin
Assert-BookingFlow
Cleanup-SmokeReservations

Write-Host "OK smoke test: roles, default forms, customer account, booking flow"
