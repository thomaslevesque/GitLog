using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GitLog.Services.Implementation
{
    class FolderBrowser
    {
        public FolderBrowser()
        {
            ShowNewFolderButton = true;
            RootFolder = FolderBrowserRoot.Desktop;
        }

        public string Caption { get; set; }
        public string Title { get; set; }
        public string InitialPath { get; set; }
        public string SelectedPath { get; private set; }

        public bool ShowNewFolderButton { get; set; }
        public bool IncludeFiles { get; set; }
        public bool ShowEditBox { get; set; }
        public FolderBrowserRoot RootFolder { get; set; }

        public bool? ShowDialog()
        {
            return ShowDialog(IntPtr.Zero);
        }

        public bool? ShowDialog(IntPtr parentHandle)
        {
            StringBuilder sb = new StringBuilder(256);
            IntPtr pidl = IntPtr.Zero;
            BROWSEINFO bi = new BROWSEINFO();

            bi.hwndOwner = parentHandle;

            bi.pidlRoot = IntPtr.Zero;
            if (RootFolder != FolderBrowserRoot.Desktop)
            {
                var hr = SHGetFolderLocation(IntPtr.Zero, (int)RootFolder, IntPtr.Zero, 0, out bi.pidlRoot);
                if (hr.IsError)
                    throw hr.Exception;
            }

            bi.lpszTitle = Caption;
            bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;
            if (IncludeFiles)
                bi.ulFlags |= BIF_BROWSEINCLUDEFILES;
            if (!ShowNewFolderButton)
                bi.ulFlags |= BIF_NONEWFOLDERBUTTON;
            if (ShowEditBox)
                bi.ulFlags |= BIF_EDITBOX;
            bi.lpfn = OnBrowseEvent;
            bi.lParam = IntPtr.Zero;
            bi.iImage = 0;

            try
            {
                pidl = SHBrowseForFolder(ref bi);

                if (pidl == IntPtr.Zero)
                    return false;

                if (SHGetPathFromIDList(pidl, sb))
                    SelectedPath = sb.ToString();
            }
            finally
            {
                // Caller is responsible for freeing this memory.
                Marshal.FreeCoTaskMem(pidl);
            }

            return true;
        }

        private int OnBrowseEvent(IntPtr hwnd, int msg, IntPtr lp, IntPtr lpData)
        {
            switch (msg)
            {
                case BFFM_INITIALIZED: // Required to set initialPath
                {
                    // Use BFFM_SETSELECTIONW if passing a Unicode string, i.e. native CLR Strings.
                    if (!String.IsNullOrEmpty(InitialPath))
                        SendMessage(new HandleRef(null, hwnd), BFFM_SETSELECTIONW, 1, InitialPath);
                    if (!String.IsNullOrEmpty(Title))
                        SetWindowText(hwnd, Title);
                    break;
                }
                case BFFM_SELCHANGED:
                {
                    StringBuilder sb = new StringBuilder(260);
                    if (SHGetPathFromIDList(lp, sb))
                        SendMessage(new HandleRef(null, hwnd), BFFM_SETSTATUSTEXTW, 0, sb.ToString());
                    break;
                }
            }

            return 0;
        }

        #region Interop

        // Resharper disable InconsistentNaming

        public const int WM_USER = 0x400;
        public const int BFFM_INITIALIZED = 1;
        public const int BFFM_SELCHANGED = 2;
        public const int BFFM_VALIDATEFAILEDA = 3;
        public const int BFFM_VALIDATEFAILEDW = 4;
        public const int BFFM_IUNKNOWN = 5; // provides IUnknown to client. lParam: IUnknown*
        public const int BFFM_SETSTATUSTEXTA = WM_USER + 100;
        public const int BFFM_ENABLEOK = WM_USER + 101;
        public const int BFFM_SETSELECTIONA = WM_USER + 102;
        public const int BFFM_SETSELECTIONW = WM_USER + 103;
        public const int BFFM_SETSTATUSTEXTW = WM_USER + 104;
        public const int BFFM_SETOKTEXT = WM_USER + 105; // Unicode only
        public const int BFFM_SETEXPANDED = WM_USER + 106; // Unicode only

        // Browsing for directory.
        public const uint BIF_RETURNONLYFSDIRS = 0x0001;  // For finding a folder to start document searching
        public const uint BIF_DONTGOBELOWDOMAIN = 0x0002;  // For starting the Find Computer
        public const uint BIF_STATUSTEXT = 0x0004;  // Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and one line if
        // this flag is set.  Passing the message BFFM_SETSTATUSTEXTA to the hwnd can set the
        // rest of the text.  This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
        // all three lines of text.
        public const uint BIF_RETURNFSANCESTORS = 0x0008;
        public const uint BIF_EDITBOX = 0x0010;   // Add an editbox to the dialog
        public const uint BIF_VALIDATE = 0x0020;   // insist on valid result (or CANCEL)

        public const uint BIF_NEWDIALOGSTYLE = 0x0040;   // Use the new dialog layout with the ability to resize
        // Caller needs to call OleInitialize() before using this API
        public const uint BIF_USENEWUI = 0x0040 + 0x0010; //(BIF_NEWDIALOGSTYLE | BIF_EDITBOX);

        public const uint BIF_BROWSEINCLUDEURLS = 0x0080;   // Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
        public const uint BIF_UAHINT = 0x0100;   // Add a UA hint to the dialog, in place of the edit box. May not be combined with BIF_EDITBOX
        public const uint BIF_NONEWFOLDERBUTTON = 0x0200;   // Do not add the "New Folder" button to the dialog.  Only applicable with BIF_NEWDIALOGSTYLE.
        public const uint BIF_NOTRANSLATETARGETS = 0x0400;  // don't traverse target as shortcut

        public const uint BIF_BROWSEFORCOMPUTER = 0x1000;  // Browsing for Computers.
        public const uint BIF_BROWSEFORPRINTER = 0x2000;// Browsing for Printers
        public const uint BIF_BROWSEINCLUDEFILES = 0x4000; // Browsing for Everything
        public const uint BIF_SHAREABLE = 0x8000;  // sharable resources displayed (remote shares, requires BIF_USENEWUI)

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern HRESULT SHGetFolderLocation(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwReserved, out IntPtr ppidl);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);

        public delegate int BrowseCallBackProc(IntPtr hwnd, int msg, IntPtr lp, IntPtr wp);
        public struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public BrowseCallBackProc lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HRESULT
        {
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            private int _hresult;
            // ReSharper restore FieldCanBeMadeReadOnly.Local

            public Exception Exception
            {
                get
                {
                    return Marshal.GetExceptionForHR(_hresult, (IntPtr)(-1));
                }
            }

            public bool IsSuccess
            {
                get { return _hresult >= 0; }
            }

            public bool IsError
            {
                get { return _hresult < 0; }
            }
        }

        // Resharper restore InconsistentNaming

        #endregion

    }

    enum FolderBrowserRoot
    {
        Desktop = 0x0000,         // <desktop>
        Internet = 0x0001,         // Internet Explorer (icon on desktop)
        Programs = 0x0002,         // Start Menu\Programs
        ControlPanel = 0x0003,         // My Computer\Control Panel
        Printers = 0x0004,         // My Computer\Printers
        Personal = 0x0005,         // My Documents
        Favorites = 0x0006,         // <user name>\Favorites
        Startup = 0x0007,         // Start Menu\Programs\Startup
        Recent = 0x0008,         // <user name>\Recent
        SendTo = 0x0009,         // <user name>\SendTo
        RecycleBin = 0x000a,         // <desktop>\Recycle Bin
        StartMenu = 0x000b,         // <user name>\Start Menu
        MyDocuments = Personal,  //  Personal was just a silly name for My Documents
        MyMusic = 0x000d,         // "My Music" folder
        MyVideos = 0x000e,         // "My Videos" folder
        DesktopDirectory = 0x0010,         // <user name>\Desktop
        Drives = 0x0011,         // My Computer
        Network = 0x0012,         // Network Neighborhood (My Network Places)
        NetHood = 0x0013,         // <user name>\nethood
        Fonts = 0x0014,         // windows\fonts
        Templates = 0x0015,
        CommonStartMenu = 0x0016,         // All Users\Start Menu
        CommonPrograms = 0X0017,         // All Users\Start Menu\Programs
        CommonStartup = 0x0018,         // All Users\Startup
        CommonDesktopDirectory = 0x0019,         // All Users\Desktop
        AppData = 0x001a,         // <user name>\Application Data
        PrintHood = 0x001b,         // <user name>\PrintHood
        LocalAppData = 0x001c,         // <user name>\Local Settings\Applicaiton Data (non roaming)
        AltStartup = 0x001d,         // non localized startup
        CommonAltStartup = 0x001e,         // non localized common startup
        CommonFavorites = 0x001f,
        InternetCache = 0x0020,
        Cookies = 0x0021,
        History = 0x0022,
        CommonAppData = 0x0023,         // All Users\Application Data
        Windows = 0x0024,         // GetWindowsDirectory()
        System = 0x0025,         // GetSystemDirectory()
        ProgramFiles = 0x0026,         // C:\Program Files
        MyPictures = 0x0027,         // C:\Program Files\My Pictures
        Profile = 0x0028,         // USERPROFILE
        Systemx86 = 0x0029,         // x86 system directory on RISC
        ProgramFilesx86 = 0x002a,         // x86 C:\Program Files on RISC
        ProgramFilesCommon = 0x002b,         // C:\Program Files\Common
        ProgramFilesCommonx86 = 0x002c,         // x86 Program Files\Common on RISC
        CommonTemplates = 0x002d,         // All Users\Templates
        CommonDocuments = 0x002e,         // All Users\Documents
        CommonAdminTools = 0x002f,         // All Users\Start Menu\Programs\Administrative Tools
        AdminTools = 0x0030,         // <user name>\Start Menu\Programs\Administrative Tools
        Connections = 0x0031,         // Network and Dial-up Connections
        CommonMusic = 0x0035,         // All Users\My Music
        CommonPictures = 0x0036,         // All Users\My Pictures
        CommonVideos = 0x0037,         // All Users\My Video
        Resources = 0x0038,         // Resource Direcotry
        ResourcesLocalized = 0x0039,         // Localized Resource Direcotry
        CommonOemLinks = 0x003a,         // Links to All Users OEM specific apps
        CDBurnArea = 0x003b,         // USERPROFILE\Local Settings\Application Data\Microsoft\CD Burning
        ComputersNearMe = 0x003d,         // Computers Near Me (computered from Workgroup membership)
    }

}