using SafeShare.Util;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace SafeShare
{
    internal class ProcessCommand
    {
        private const string PdfUtil = "-cpdfutil";
        private const string ImgUtil = "-cimgutil";
        private const string ShellMenu = "-cshellmenu";
        private string Open_SafeShare_From_PdfUtil_EVENT_NAME = "opensafesharefrompdfutil_event_{0}";
        private string Open_SafeShare_From_ImgUtil_EVENT_NAME = "opensafesharefromimgutil_event_{0}";

        private string _processID;
        private IntPtr _handle;
        private SafeShareView _safeshareView;
        private List<string> _sourceFiles;

        public ProcessCommand(SafeShareView view)
        {
            _safeshareView = view;
        }

        public void Process(string[] args)
        {
            var viewModel = _safeshareView.DataContext as SafeShareViewModel;
            bool getProcessId = false;
            _sourceFiles = new List<string>();
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (arg.StartsWith("-h:"))
                    {
                        string handeLong = arg.Substring(3);
                        _handle = new IntPtr(long.Parse(handeLong));
                        continue;
                    }
                    if (arg.StartsWith("-cmd:", StringComparison.OrdinalIgnoreCase))
                    {
                        // The "-cmd:" must be at the end of commands
                        break;
                    }

                    if (arg == PdfUtil)
                    {
                        viewModel.CallFrom = ExeFrom.PDFUTIL;
                    }
                    else if (arg == ImgUtil)
                    {
                        viewModel.CallFrom = ExeFrom.IMGUTIL;
                    }
                    else if (arg == ShellMenu)
                    {
                        viewModel.CallFrom = ExeFrom.SHELLMENU;
                    }
                }
                else if (arg.StartsWith("&"))
                {
                    string filePath = arg.Substring(1);

                    if (Path.GetExtension(filePath).ToLower() == ".tmp")
                    {
                        ParseAndGetSourceFiles(filePath, ref _sourceFiles);
                    }
                    else
                    {
                        _sourceFiles.Add(filePath);
                    }
                }
                else if (arg == "/processid")
                {
                    getProcessId = true;
                }
                else if (getProcessId)
                {
                    _processID = arg;
                    getProcessId = false;
                }
                else if (Path.IsPathRooted(arg) && (File.Exists(arg) || Directory.Exists(arg)))
                {
                    viewModel.CallFrom = ExeFrom.DRAG;
                    _sourceFiles.Add(arg);
                }
            }

            if (!EDPHelper.CheckProtectedFiles(_sourceFiles.ToArray()))
            {
                return;
            }
        }

        public void ProcessCall()
        {
            var viewModel = _safeshareView.DataContext as SafeShareViewModel;
            switch (viewModel.CallFrom)
            {
                case ExeFrom.PDFUTIL:
                    ExecuteCommandsCalledByPdfUtil(_sourceFiles.ToArray());
                    break;

                case ExeFrom.IMGUTIL:
                    ExecuteCommandsCalledByImgUtil(_sourceFiles.ToArray());
                    break;

                case ExeFrom.SHELLMENU:
                case ExeFrom.DRAG:
                    ExecuteCommandsWithFiles(_sourceFiles.ToArray());
                    break;
            }
        }

        private void ExecuteCommandsCalledByPdfUtil(string[] sourceFiles)
        {
            string eventName = string.Format(Open_SafeShare_From_PdfUtil_EVENT_NAME, _processID);
            IntPtr hEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, eventName);

            if (hEvent != IntPtr.Zero)
            {
                NativeMethods.SetEvent(hEvent);
            }

            if (_handle != IntPtr.Zero)
            {
                _safeshareView.ParentWndHandle = _handle;
                NativeMethods.EnableWindow(_handle, false);
            }

            _safeshareView.LaunchWithFile(sourceFiles);
            _safeshareView.ShowDialog();
        }

        private void ExecuteCommandsCalledByImgUtil(string[] sourceFiles)
        {
            string eventName = string.Format(Open_SafeShare_From_ImgUtil_EVENT_NAME, _processID);
            IntPtr hEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, eventName);

            if (hEvent != IntPtr.Zero)
            {
                NativeMethods.SetEvent(hEvent);
            }

            if (_handle != IntPtr.Zero)
            {
                _safeshareView.ParentWndHandle = _handle;
                NativeMethods.EnableWindow(_handle, false);
            }

            _safeshareView.LaunchWithFile(sourceFiles);
            _safeshareView.ShowDialog();
        }

        private void ExecuteCommandsWithFiles(string[] sourceFiles)
        {
            _safeshareView.LaunchWithFile(sourceFiles);

            var viewModel = _safeshareView.DataContext as SafeShareViewModel;
            if (viewModel.CallFrom == ExeFrom.SHELLMENU)
            {
                TrackHelper.LogShellMenuEvent("safeshare");
            }

            _safeshareView.ShowDialog();
        }

        private void ParseAndGetSourceFiles(string file, ref List<string> sourceFiles)
        {
            using (var reader = new StreamReader(file))
            {
                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    if (line != string.Empty)
                    {
                        sourceFiles.Add(line);
                    }
                }
            }
        }
    }
}