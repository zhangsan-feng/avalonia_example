using System;
using System.Runtime.InteropServices;
using System.Text;

namespace desktop_application.win32api;

public class ProcessFileLock {
    // 引入必要的 Windows API 函数
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetProcessId(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

    // 错误代码
    private const uint ERROR_SHARING_VIOLATION = 32;
    private const uint ERROR_LOCK_VIOLATION = 33;

    // 文件访问权限
    private const uint GENERIC_READ = 0x80000000;
    private const uint GENERIC_WRITE = 0x40000000;
    private const uint FILE_SHARE_READ = 0x00000001;
    private const uint FILE_SHARE_WRITE = 0x00000002;
    private const uint OPEN_EXISTING = 3;

    public static int? FindProcessHoldingFile(string filePath) {
        IntPtr handle = IntPtr.Zero;
        try {
            // 尝试以独占方式打开文件
            handle = CreateFile(
                filePath,
                GENERIC_READ,
                0, // 不共享
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (handle.ToInt32() == -1) // INVALID_HANDLE_VALUE
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == ERROR_SHARING_VIOLATION || lastError == ERROR_LOCK_VIOLATION) {
                    // 文件被锁定，现在尝试使用 Restart Manager API 查找进程
                    return FindProcessUsingRestartManager(filePath);
                }
            }

            // 如果能成功打开，说明文件没有被占用
            return null;
        }
        finally {
            if (handle != IntPtr.Zero)
                CloseHandle(handle);
        }
    }

    private static int? FindProcessUsingRestartManager(string filePath) {
        try {
            // 使用 Restart Manager API
            uint handle;
            int res = RmStartSession(out handle, 0, Guid.NewGuid().ToString());
            if (res != 0) return null;

            try {
                // 注册资源
                string[] resources = new string[] { filePath };
                res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);
                if (res != 0) return null;

                // 获取进程列表
                uint pnProcInfoNeeded = 0;
                uint pnProcInfo = 0;
                RM_PROCESS_INFO[] processInfo = null;

                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, out uint lpdwRebootReasons);
                if (res == 0 && pnProcInfoNeeded > 0) {
                    processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;
                    res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, out lpdwRebootReasons);
                    if (res == 0) {
                        // 返回第一个找到的进程ID
                        return (int)processInfo[0].Process.dwProcessId;
                    }
                }
            }
            finally {
                RmEndSession(handle);
            }
        }
        catch {
            // 如果 Restart Manager API 不可用，则忽略
        }

        return null;
    }

    // Restart Manager API 结构体和函数
    [StructLayout(LayoutKind.Sequential)]
    private struct RM_UNIQUE_PROCESS {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RM_PROCESS_INFO {
        public RM_UNIQUE_PROCESS Process;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strAppName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string strServiceShortName;

        public RM_APP_TYPE ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)] public bool bRestartable;
    }

    private enum RM_APP_TYPE {
        RmUnknownApp = 0,
        RmMainWindow = 1,
        RmOtherWindow = 2,
        RmService = 3,
        RmExplorer = 4,
        RmConsole = 5,
        RmCritical = 1000
    }

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
    private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint dwSessionHandle);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
    private static extern int RmRegisterResources(uint dwSessionHandle, uint nFiles, string[] rgsFilenames,
        uint nApplications, [In] RM_UNIQUE_PROCESS[] rgApplications, uint nServices, string[] rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
    private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded,
        ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps, out uint lpdwRebootReasons);
}