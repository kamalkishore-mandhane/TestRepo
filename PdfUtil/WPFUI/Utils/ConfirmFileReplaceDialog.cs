using PdfUtil.Properties;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PdfUtil.WPFUI.Utils
{
    class ConfirmFileReplaceDialog
    {
        private const int FORMAT_BYTE_SIZE_LENGTH = 24;

        private TaskDialog dialog;

        private bool _applyToAll = false;
        public bool ApplyToAll
        {
            get { return _applyToAll; }
        }

        private ConfirmFileReplaceDialog(string sourceFileName, string destFileName, bool isMultiFiles)
        {
            string windowTitle = Resources.CONFIRM_DIALOG_TITLE;
            string mainInstruction = Resources.CONFIRM_DIALOG_MAIN_INSTRUCTION;

            string content = string.Format(Resources.CONFIRM_DIALOG_CONTENT, destFileName, sourceFileName);

            string verificationText = string.Empty;
            if (isMultiFiles)
            {
                verificationText = Resources.CONFIRM_DIALOG_VERIFICATION_TEXT;   
            }

            TASKDIALOG_BUTTON[] buttons = new[]
            {
                new TASKDIALOG_BUTTON(){ id = (int)DialogResult.Yes, text = Resources.CONFIRM_DIALOG_BUTTON_YES_TEXT },
                new TASKDIALOG_BUTTON(){ id = (int)DialogResult.No, text = Resources.CONFIRM_DIALOG_BUTTON_NO_TEXT },
                new TASKDIALOG_BUTTON(){ id = (int)DialogResult.OK, text = Resources.CONFIRM_DIALOG_BUTTON_OK_TEXT }
            };
            dialog = new TaskDialog(windowTitle, mainInstruction, content, verificationText, buttons, SystemIcons.Warning);
        }

        private DialogResult ShowDialog(IntPtr owner)
        {
            TaskDialogResult taskDialogResult = dialog.Show(owner);
            _applyToAll = taskDialogResult.verificationChecked;
            return taskDialogResult.dialogResult;
        }

        public static bool NotNeedOverrideConfirm { get; set; } = false;

        public static OverrideConfirmResult Show(IntPtr owner, string fileName, string destFileName, bool isMultiFiles)
        {
            if (NotNeedOverrideConfirm)
            {
                return new OverrideConfirmResult(OverrideConfirmResult.Choice.Override, true);
            }

            ConfirmFileReplaceDialog dialog;
            dialog = new ConfirmFileReplaceDialog(fileName, destFileName, isMultiFiles);
            DialogResult result = dialog.ShowDialog(owner);
            return new OverrideConfirmResult(result, dialog.ApplyToAll);
        }

    }
}
