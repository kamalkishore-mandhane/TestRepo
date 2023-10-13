using Microsoft.Win32;
using SBkUpUI.WPFUI.Utils;
using SBkUpUI.WPFUI.View;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static SBkUpUI.WPFUI.Utils.WinZipMethods;

namespace SBkUpUI.WPFUI.Controls
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : BaseWindow
    {
        private SBkUpView _view;

        private bool isSubs = false;

        private string ProductID;
        private string SoftwareVersion;
        private string VID;

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        public AboutDialog(SBkUpView view)
        {
            InitializeComponent();
            _view = view;
            if (view.IsLoaded)
            {
                Owner = view;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitAboutInfo();
        }

        private void AboutDialog_SourceInitialized(object sender, EventArgs e)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            NativeMethods.SendMessage(hwnd, NativeMethods.WM_SETICON, new IntPtr(NativeMethods.ICON_SMALL), IntPtr.Zero);
            NativeMethods.SendMessage(hwnd, NativeMethods.WM_SETICON, new IntPtr(NativeMethods.ICON_BIG), IntPtr.Zero);

            // Change the extended window style to not show a window icon
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, extendedStyle | NativeMethods.WS_EX_DLGMODALFRAME);
        }

        private void AboutDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void AboutDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void InitAboutInfo()
        {
            string version = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
            string[] arrVersion = version.Split('.');
            string buildVersionInfo = arrVersion[2];
            string exeVersion = arrVersion[0] + '.' + arrVersion[1];

            string[] arrWinZipVersion = RegeditOperation.GetWinZipVersion().Split('.');
            string winzipVersion = arrWinZipVersion[0] + '.' + arrWinZipVersion[1];

            bool isEnterprise = RegeditOperation.GetIsEnterprise();
            string name, sn;
            WinZipMethods.GetAuthNameAndCode(out name, out sn);

            bool is64bit = RegeditOperation.IsWinZip64Exe();
            string type;
            string exeBit = is64bit ? "- 64-bit" : "- 32-bit";
            string test = "";
            isSubs = RegeditOperation.GetIsXAT().Equals("subs");

            int licenseType = -1;
            WinZipMethods.GetLicenseStatus(_view.WindowHandle, ref licenseType);
            bool isEval = licenseType == (int)WINZIP_LICENSED_VERSIONS.WLV_TRIAL_VERSION;

            switch (licenseType)
            {
                case (int)WINZIP_LICENSED_VERSIONS.WLV_STD_VERSION:
                    type = "Std";
                    break;

                case (int)WINZIP_LICENSED_VERSIONS.WLV_PRO_VERSION:
                    type = isEnterprise ? " Enterprise" : " Pro";
                    break;

                case (int)WINZIP_LICENSED_VERSIONS.WLV_TRIAL_VERSION:
                default:
                    type = "";
                    break;
            }
#if DEBUG
            test = " TEST";
#endif

            var ProductInfoPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            ProductInfoPath = System.IO.Path.Combine(ProductInfoPath, "WinZip", "WZPRODINFO.DAT");
            bool bFileExists = File.Exists(ProductInfoPath);
            if (bFileExists)
            {
                string Section = string.Format("WinZip{0}", Convert.ToInt32(arrVersion[0]) * 10);
                var sb1 = new StringBuilder(256);
                var sb2 = new StringBuilder(256);
                var sb3 = new StringBuilder(256);
                GetPrivateProfileString(Section, "ProductID", string.Empty, sb1, 1024, ProductInfoPath);
                GetPrivateProfileString(Section, "SoftwareVersion", string.Empty, sb2, 1024, ProductInfoPath);
                GetPrivateProfileString(Section, "VID", string.Empty, sb3, 1024, ProductInfoPath);
                if (!sb1.ToString().Equals(string.Empty))
                {
                    ProductID = sb1.ToString();
                    SoftwareVersion = sb2.ToString();
                    VID = sb3.ToString();
                }
            }

            TextBolck_VersionInfo_1.Text = Properties.Resources.WINZIP + "® " + Properties.Resources.SECURE_BACKUP_FRIENDLY_NAME + " " + exeVersion + " (" + buildVersionInfo + ") " + exeBit;
            TextBolck_VersionInfo_2.Text = Properties.Resources.WINZIP + "® " + winzipVersion + type + test + " (" + buildVersionInfo + ") " + exeBit;
            TextBolck_L2.Text = name;
            TextBolck_L4.Text = sn;
            Grid_Suite.Visibility = isEval ? Visibility.Collapsed : Visibility.Visible;

            if (isEval)
            {
                GridLicenseEvaInfo.Visibility = Visibility.Visible;
                GridLicenseInfo.Visibility = Visibility.Hidden;
            }
            else
            {
                GridLicenseEvaInfo.Visibility = Visibility.Hidden;
                GridLicenseInfo.Visibility = Visibility.Visible;
                if (isEnterprise)
                {
                    TextBlock_Suite.Text = "WinZip Enterprise Suite";
                }
                else if (licenseType == (int)WINZIP_LICENSED_VERSIONS.WLV_STD_VERSION)
                {
                    TextBlock_Suite.Text = "WinZip Standard Suite";
                }
                else if (licenseType == (int)WINZIP_LICENSED_VERSIONS.WLV_PRO_VERSION)
                {
                    TextBlock_Suite.Text = "WinZip Pro Suite";
                }
                TextBlock_name.Text = name;
                if (isSubs)
                {
                    TextBlock_renews.Text = Properties.Resources.ABOUTBOX_EXPRIESDATE + " " + GetSuitSed();
                }
                else
                {
                    TextBlock_renews.Text = sn;
                }
            }

            if (isEval)
            {
                this.Height = 380;
            }

#if WZ_APPX
            GridLicenseEvaInfo.Visibility = Visibility.Hidden;
            GridLicenseInfo.Visibility = Visibility.Hidden;
#endif
        }

        private void Button_KnowledgebaseClick(object sender, RoutedEventArgs e)
        {
            var knowledgebaseUrl = string.Format("http://www.winzip.com/wzgate.cgi?lang={0}&x-at={1}&url=kb.winzip.com/kb/&wzbits={2}&osbits={3}", MapWinzipLangToEcomm(RegeditOperation.GetWinZipInstalledUILangID()), RegeditOperation.GetIsXAT(), RegeditOperation.IsWinZip64Exe() ? "64" : "32", Environment.Is64BitOperatingSystem ? "64" : "32");
            Process.Start(knowledgebaseUrl);
        }

        private void Button_LicenseClick(object sender, RoutedEventArgs e)
        {
            var licenseUrl = string.Format("http://www.winzip.com/help.cgi?prod={0}&vid={1}&lang={2}&ver={3}&topic=HELP_SHAREWARE_LICENSE.htm&hlpprm=", ProductID, VID, MapWinzipLangToEcomm(RegeditOperation.GetWinZipInstalledUILangID()), SoftwareVersion);
            Process.Start(licenseUrl);
        }

        private void Button_HomePageClick(object sender, RoutedEventArgs e)
        {
            var param = string.Format("ver={0}&vid={1}&x-at={2}&wzbits={3}&osbits={4}", SoftwareVersion, VID, RegeditOperation.GetIsXAT(), RegeditOperation.IsWinZip64Exe() ? "64" : "32", Environment.Is64BitOperatingSystem ? "64" : "32");
            param = HttpUtility.UrlEncode(param);
            var homePageUrl = string.Format("http://www.winzip.com//wzgate.cgi?lang={0}&x-at={1}&url=www.winzip.com/&param={2}", MapWinzipLangToEcomm(RegeditOperation.GetWinZipInstalledUILangID()), RegeditOperation.GetIsXAT(), param);
            Process.Start(homePageUrl);
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    e.Handled = true;
                    break;
                default:
                    return;
            }
        }

        private string GetSuitSed()
        {
            byte[] protectedSed = RegeditOperation.GetSuiteSED();
            if (protectedSed != null)
            {
                string data = Encoding.Unicode.GetString(ProtectedData.Unprotect(protectedSed, null, DataProtectionScope.CurrentUser)).Replace("\0", string.Empty);
                DateTime dt;
                if (DateTime.TryParse(data, out dt))
                    return dt.ToLongDateString();
            }

            return string.Empty;
        }

        private static string MapWinzipLangToEcomm(int langId)
        {
            switch (langId)
            {
                case 1028:
                    return "ct";    // Chinese Traditional
                case 1029:
                    return "cz";    // Czech
                case 1030:
                    return "da";    // Dannish
                case 1031:
                    return "de";    // German
                case 1033:
                    return "en";    // English
                case 1034:          // Traditional Spanish
                case 3082:          // Modern Spanish
                case 2058:
                    return "es";    // Spanish or Mexican
                case 1035:
                    return "su";    // Finnish
                case 1036:
                    return "fr";    // French
                case 1040:
                    return "it";    // Italian
                case 1041:
                    return "jp";    // Japanese
                case 1042:
                    return "kr";    // Korean
                case 1043:
                    return "nl";    // Dutch
                case 1044:
                    return "no";    // Norwegian
                case 1046:
                    return "br";    // Brazilian Portuguese
                case 1049:
                    return "ru";    // Russian
                case 1053:
                    return "sv";    // Swedish
                case 2052:
                    return "cs";    // Chinese Simplified
                default:
                    return "en";
            }
        }
    }
}
