using Applets.Common;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImgUtil.WPFUI.Utils
{
    [Flags]
    public enum TASKDIALOG_COMMON_BUTTON_FLAGS
    {
        None = 0,
        Ok = 0x0001,
        Yes = 0x0002,
        No = 0x0004,
        Cancel = 0x0008,
        Retry = 0x0010,
        Close = 0x0020,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct TASKDIALOG_BUTTON
    {
        public int id;
        public string text;
    }

    public struct TaskDialogResult
    {
        public bool verificationChecked;
        public int radioButtonResult;
        public DialogResult dialogResult;
    }

    public class TaskDialog
    {
        private string windowTitle;
        private string mainInstruction;
        private string content;
        private string verificationText;

        private TASKDIALOG_BUTTON[] buttons;
        private TASKDIALOG_BUTTON[] radioButtons;
        private int defaultButton = 0;
        private int defaultRadioButton = 0;

        private Icon mainIcon = null;

        private TASKDIALOG_COMMON_BUTTON_FLAGS commonButtonFlags = TASKDIALOG_COMMON_BUTTON_FLAGS.None;
        private NativeMethods.TASKDIALOG_FLAGS flags = 0;

        // populate these field if needed
        private Icon footerIcon = null;
        private string footer = null;
        private string expandedInformation = null;
        private string expandedControlText = null;
        private string collapsedControlText = null;
        private IntPtr callback = IntPtr.Zero;
        private IntPtr callbackData = IntPtr.Zero;
        private int width;

        #region Constructors
        // Add more Constructor if needed
        public TaskDialog(string Title, string MainInstruction, string Content, string VerificationText, TASKDIALOG_BUTTON[] CommandButtons, Icon MainIcon, int Width = 0) :
            this(Title, MainInstruction, Content, null, null, VerificationText, CommandButtons, null, TASKDIALOG_COMMON_BUTTON_FLAGS.None, MainIcon, null, Width)
        {
        }

        public TaskDialog(string Title, string MainInstruction, string Content, string ExpandedInfo, string Footer, string VerificationText,
            TASKDIALOG_BUTTON[] CommandButtons, TASKDIALOG_BUTTON[] RadioButtons,
            TASKDIALOG_COMMON_BUTTON_FLAGS CommonButtonFlags, Icon MainIcon, Icon FooterIcon, int Width)
        {
            this.windowTitle = Title;
            this.content = Content;
            this.expandedInformation = ExpandedInfo;
            this.verificationText = VerificationText;

            this.buttons = CommandButtons;
            this.radioButtons = RadioButtons;
            this.commonButtonFlags = CommonButtonFlags;

            this.mainInstruction = MainInstruction;
            this.mainIcon = MainIcon;

            this.footer = Footer;
            this.footerIcon = FooterIcon;
            this.width = Width;

            // Set flags
            this.AllowDialogCancellation = true;
            this.PositionRelativeToWindow = true;
            this.UseCommandLinks = (CommandButtons != null && CommandButtons.Length > 0);
            this.UseCommandLinksNoIcon = false;
            this.CanBeMinimized = false;
        }
        #endregion

        public TaskDialogResult Show(IntPtr hwndOwner)
        {
            TaskDialogResult result = default(TaskDialogResult);
            result.dialogResult = NativeShow(hwndOwner, out result.verificationChecked, out result.radioButtonResult);
            return result;
        }

        private DialogResult NativeShow(IntPtr hwndOwner, out bool verificationFlagChecked, out int radioButtonResult)
        {
            NativeMethods.TASKDIALOGCONFIG config = new NativeMethods.TASKDIALOGCONFIG();
            try
            {
                config.size = Marshal.SizeOf(typeof(NativeMethods.TASKDIALOGCONFIG));
                config.parent = hwndOwner;
                config.flags = this.flags;
                config.commonButtonFlags = commonButtonFlags;

                if (!string.IsNullOrEmpty(this.windowTitle))
                {
                    config.title = this.windowTitle;
                }

                if (this.mainIcon != null)
                {
                    config.flags |= NativeMethods.TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN;
                    config.mainIcon = this.mainIcon.Handle;
                }
                if (!string.IsNullOrEmpty(this.mainInstruction))
                {
                    config.mainInstruction = this.mainInstruction;
                }
                if (!string.IsNullOrEmpty(this.content))
                {
                    config.content = this.content;
                }

                config.buttons = InitTaskDialogButton(buttons);
                config.buttonCount = buttons != null ? buttons.Length : 0;
                config.radioButtons = InitTaskDialogButton(radioButtons);
                config.radioButtonCount = radioButtons != null ? radioButtons.Length : 0;

                config.defaultButton = this.defaultButton;
                config.defaultRadioButton = this.defaultRadioButton;

                if (!string.IsNullOrEmpty(this.verificationText))
                {
                    config.verificationText = this.verificationText;
                }
                if (!string.IsNullOrEmpty(this.expandedInformation))
                {
                    config.expandedInformation = this.expandedInformation;
                }
                if (!string.IsNullOrEmpty(this.expandedControlText))
                {
                    config.expandedControlText = this.expandedControlText;
                }
                if (!string.IsNullOrEmpty(this.collapsedControlText))
                {
                    config.collapsedControlText = this.collapsedControlText;
                }

                if (this.footerIcon != null)
                {
                    config.flags |= NativeMethods.TASKDIALOG_FLAGS.TDF_USE_HICON_FOOTER;
                    config.footerIcon = this.footerIcon.Handle;
                }
                if (!string.IsNullOrEmpty(this.footer))
                {
                    config.footer = this.footer;
                }

                config.callback = this.callback;
                config.callbackData = this.callbackData;

                config.width = this.width;

                int buttonId = 0;
                verificationFlagChecked = false;
                radioButtonResult = 0;
                int hr = NativeMethods.TaskDialogIndirect(ref config, out buttonId, out radioButtonResult, out verificationFlagChecked);
                return hr == 0 ? (DialogResult)buttonId : DialogResult.Cancel;
            }
            finally
            {
                // Free the unmanged memory needed for the button arrays.
                FreeTaskDialogButton(config.buttons);
                FreeTaskDialogButton(config.radioButtons);
            }
        }

        #region Properties
        public bool EnableHyperlinks
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS, value); }
        }

        public bool AllowDialogCancellation
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION, value); }
        }

        public bool UseCommandLinks
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS, value); }
        }

        public bool UseCommandLinksNoIcon
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON, value); }
        }

        public bool ExpandFooterArea
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA, value); }
        }

        public bool ExpandedByDefault
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT, value); }
        }

        public bool VerificationFlagChecked
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED, value); }
        }

        public bool ShowProgressBar
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR, value); }
        }

        public bool ShowMarqueeProgressBar
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR, value); }
        }

        public bool CallbackTimer
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER, value); }
        }

        public bool PositionRelativeToWindow
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW, value); }
        }

        public bool RightToLeftLayout
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_RTL_LAYOUT) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_RTL_LAYOUT, value); }
        }

        public bool NoDefaultRadioButton
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON, value); }
        }

        public bool CanBeMinimized
        {
            get { return (this.flags & NativeMethods.TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED) != 0; }
            set { this.SetFlag(NativeMethods.TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED, value); }
        }
        #endregion

        private void SetFlag(NativeMethods.TASKDIALOG_FLAGS flag, bool value)
        {
            if (value)
            {
                this.flags |= flag;
            }
            else
            {
                this.flags &= ~flag;
            }
        }

        private IntPtr InitTaskDialogButton(TASKDIALOG_BUTTON[] customButtons)
        {
            if (customButtons != null && customButtons.Length > 0)
            {
                int buttonSize = Marshal.SizeOf(typeof(TASKDIALOG_BUTTON));
                IntPtr buttonPtr = Marshal.AllocHGlobal(buttonSize * customButtons.Length);
                for (int i = 0; i < buttons.Length; i++)
                {
                    Marshal.StructureToPtr(buttons[i], buttonPtr.Offset(i * buttonSize), false);
                }
                return buttonPtr;
            }
            return IntPtr.Zero;
        }

        private void FreeTaskDialogButton(IntPtr buttonPtr)
        {
            if (buttonPtr != IntPtr.Zero)
            {
                int buttonSize = Marshal.SizeOf(typeof(TASKDIALOG_BUTTON));
                for (int i = 0; i < buttons.Length; i++)
                {
                    Marshal.DestroyStructure(buttonPtr.Offset(i * buttonSize), typeof(TASKDIALOG_BUTTON));
                }
                Marshal.FreeHGlobal(buttonPtr);
            }
        }
    }
}
