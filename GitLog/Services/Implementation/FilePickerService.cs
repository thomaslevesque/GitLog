using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Interop;

namespace GitLog.Services.Implementation
{
    [Export(typeof(IFilePickerService))]
    class FilePickerService : IFilePickerService
    {
        public string PickFolder(string current = null)
        {
            var browser = new FolderBrowser();
            if (!string.IsNullOrEmpty(current))
                browser.InitialPath = current;
            browser.ShowEditBox = true;
            IntPtr hwnd = GetParentWindowHandle();
            if (browser.ShowDialog(hwnd) == true)
                return browser.SelectedPath;
            return null;
        }

        private static IntPtr GetParentWindowHandle()
        {
            var window = Application.Current.MainWindow;
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }
}
