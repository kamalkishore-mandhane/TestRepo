using Microsoft.Win32;
using SBkUpUI.WPFUI.Utils;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SBkUpUI.WPFUI.View
{
    /// <summary>
    /// Interaction logic for CreateSBkUpView.xaml
    /// </summary>
    public partial class CreateSBkUpView : BaseWindow
    {
        private Swjf _swjf = new Swjf();
        private bool _isModify;
        private bool _autoCreate;

        public CreateSBkUpView()
        {
            InitializeComponent();
            encryptCheckBox.IsEnabled = WinZipMethods.AllowedEncrypt(new WindowInteropHelper(this).Handle);
            encryptButton.IsEnabled = encryptCheckBox.IsEnabled;
            if (WinZipMethods.IsAlwaysEncrypt(new WindowInteropHelper(this).Handle))
            {
                encryptCheckBox.IsChecked = true;
                encryptCheckBox.IsEnabled = false;
            }
            _isModify = false;
        }

        public void InitModifyMode(Swjf swjf)
        {
            _isModify = true;
            _swjf = swjf.CloneSwjf();
            this.Title = Properties.Resources.MODIFY_TITLE;
            SelectBackupFolderButton.Visibility = Visibility.Hidden;
            storeFolderTextBox.Text = Util.GetTrimmedString(Util.GetDisplayName(_swjf.storeFolder), storeFolderTextBox.Width, storeFolderTextBox);
            backupFolderTextBox.Text = Util.GetTrimmedString(Util.GetDisplayName(_swjf.backupFolder), backupFolderTextBox.Width, backupFolderTextBox);
            backupFolderTextBox.BorderBrush = this.Background; // Not editable, no border
            if (encryptCheckBox.IsEnabled)
            {
                encryptCheckBox.IsChecked = _swjf.encrypt;
                encryptCheckBox.Checked += encryptCheckBox_Checked;
            }
            keepCheckBox.IsChecked = _swjf.limitMaxBackupNumber;
            numericUpDown.Value = _swjf.maxBackupNumber;
            numericUpDown.ValueChanged += numericUpDown_ValueChanged;
        }

        public void InitCreateMode(string path = null)
        {
            numericUpDown.ValueChanged += numericUpDown_ValueChanged;
            if (encryptCheckBox.IsEnabled)
            {
                encryptCheckBox.Checked += encryptCheckBox_Checked;
            }
            _swjf.storeFolder = WinZipMethods.InitCloudItemFromPath(Path.Combine(Util.GetCannedPath(CannedSwjf.Documents), Properties.Resources.MY_WINZIP_FILES));
            _swjf.backupFolder = WinZipMethods.InitCloudItemFromPath(string.IsNullOrEmpty(path) ? Util.GetCannedPath(CannedSwjf.Documents) : path);
            storeFolderTextBox.Text = _swjf.storeFolder.path;
            if (!string.IsNullOrEmpty(path))
            {
                backupFolderTextBox.Text = _swjf.backupFolder.path;
                keepCheckBox.IsChecked = true;
                _autoCreate = true;
            }

            if (_autoCreate)
            {
                this.ContentRendered += (s, e) =>
                {
                    OkButton_Click(null, null);
                };
            }
        }

        public Swjf CurrentSwjf
        {
            get
            {
                return _swjf;
            }
            set
            {
                _swjf = value;
            }
        }
        public string FileName
        {
            get; set;
        }

        private void SelectStoreFolderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectStoreFolderButton.Click -= SelectStoreFolderButton_Click;
            var tempFolder = _swjf.storeFolder;
            if (WinZipMethods.SelectFolder(new WindowInteropHelper(this).Handle, ref tempFolder))
            {
                _swjf.storeFolder = tempFolder;
                storeFolderTextBox.Text = Util.GetTrimmedString(Util.GetDisplayName(_swjf.storeFolder), storeFolderTextBox.ActualWidth, storeFolderTextBox);
            }
            SelectStoreFolderButton.Click += SelectStoreFolderButton_Click;
        }

        private void SelectBackupFolderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectBackupFolderButton.Click -= SelectBackupFolderButton_Click;
            var tempFolder = _swjf.backupFolder;
            if (WinZipMethods.SelectFolder(new WindowInteropHelper(this).Handle, ref tempFolder))
            {
                if (WinZipMethods.IsCloudItem(tempFolder.profile.Id) && (string.IsNullOrEmpty(tempFolder.parentId) || string.IsNullOrEmpty(tempFolder.name)))
                {
                    FlatMessageBox.ShowWarning(this, Properties.Resources.WARNING_LIMITATIONS_CLOUD_ROOT);
                    return;
                }
                _swjf.backupFolder = tempFolder;
                backupFolderTextBox.Text = Util.GetTrimmedString(Util.GetDisplayName(_swjf.backupFolder), backupFolderTextBox.ActualWidth, backupFolderTextBox);
            }
            SelectBackupFolderButton.Click += SelectBackupFolderButton_Click;
        }

        private void PasswordDlg_Click(object sender, RoutedEventArgs e)
        {
            string password = _swjf.password;
            WinZipMethods.Encryption encryption = _swjf.encryption;
            if (WinZipMethods.PasswordDlg(new WindowInteropHelper(this).Handle, true, ref encryption, out password))
            {
                _swjf.password = password;
                _swjf.encryption = encryption;
                encryptCheckBox.Checked -= encryptCheckBox_Checked;
                encryptCheckBox.IsChecked = true;
                encryptCheckBox.Checked += encryptCheckBox_Checked;
            }
        }

        private void encryptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordDlg_Click(null, null);
        }

        public void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (finishCreate())
            {
                this.DialogResult = true;
            }
        }

        public bool finishCreate()
        {
            if (string.IsNullOrEmpty(backupFolderTextBox.Text))
            {
                FlatMessageBox.ShowWarning(this, Properties.Resources.SPECIFY_BACKUP_FOLDER);
                SelectBackupFolderButton.Focus();
                SelectBackupFolderButton_Click(SelectBackupFolderButton, null);
                return false;
            }
            if (string.IsNullOrEmpty(storeFolderTextBox.Text))
            {
                FlatMessageBox.ShowWarning(this, Properties.Resources.SPECIFY_STORE_FOLDER);
                SelectStoreFolderButton.Focus();
                SelectStoreFolderButton_Click(SelectStoreFolderButton, null);
                return false;
            }
            if (encryptCheckBox.IsChecked.HasValue && encryptCheckBox.IsChecked.Value && string.IsNullOrEmpty(_swjf.password))
            {
                FlatMessageBox.ShowWarning(this, Properties.Resources.SPECIFY_ENCRYPTION_METHOD);
                PasswordDlg_Click(null, null);
                return false;
            }
            else
            {
                if (!_isModify)
                {
                    if (this.IsActive)
                    {
                        FileName = GetFileName();

                        if (DuplicateNameView.NameDuplicate(FileName))
                        {
                            var view = new DuplicateNameView(FileName);
                            view.Owner = this;
                            view.ShowDialog();
                            if (view.DialogResult != true)
                            {
                                return false;
                            }
                            FileName = view.FileName;
                        }
                    }

                    if (FileName.Length > 150)
                    {
                        FileName = FileName.Substring(0, 150);
                    }
                    _swjf.zipName = FileName + ".zipx";
                }

                _swjf.encrypt = encryptCheckBox.IsChecked.HasValue && encryptCheckBox.IsChecked.Value;
                _swjf.limitMaxBackupNumber = keepCheckBox.IsChecked.HasValue && keepCheckBox.IsChecked.Value;
                _swjf.maxBackupNumber = numericUpDown.Value;
                if (!_swjf.encrypt)
                {
                    _swjf.encryption = WinZipMethods.Encryption.CRYPT_NONE;
                }

                return true;
            }
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            keepCheckBox.IsChecked = true;
        }

        private void CreateSBkUpView_Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging += UserPreferenceChanging;
            if (SystemParameters.HighContrast)
            {
                SetHighContrastTheme(SystemParameters.HighContrast);
            }
        }

        private void CreateSBkUpView_UnLoaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanging -= UserPreferenceChanging;
        }

        private void CreateSBkUpViewWindow_KeyDown(object sender, KeyEventArgs e)
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

        public string GetFileName()
        {
            string name = _swjf.backupFolder.name;
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar.ToString(), string.Empty);
            }
            return name;
        }

        public string GetUniqueFileName()
        {
            var fileName = GetFileName();
            if (!DuplicateNameView.NameDuplicate(fileName))
            {
                return fileName;
            }

            int i = 1;
            var filenameFormat = string.Empty;
            var match = Regex.Match(fileName, @"\(+\d+\)");
            if (match.Success)
            {
                i = int.Parse(match.Value.Substring(1, match.Length - 2));
                filenameFormat = fileName.Replace(match.Value, "{0}");
            }
            else
            {
                filenameFormat = fileName + "{0}";
            }

            while (DuplicateNameView.NameDuplicate(fileName))
            {
                fileName = string.Format(filenameFormat, "(" + (++i) + ")");
            }

            return fileName;
        }
    }
}
