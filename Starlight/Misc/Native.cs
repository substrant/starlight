using System;
using System.Runtime.InteropServices;

namespace Starlight
{
    public class Native
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        public const int SW_HIDE = 0;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, uint lpProcessAttributes, uint lpThreadAttributes, bool bInheritHandles, ProcessCreationFlags dwCreationFlags, uint lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, [Out] out PROCESS_INFORMATION lpProcessInformation);

        [Flags]
        public enum ProcessCreationFlags : uint // got lazy and didnt rename anything here
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct PROCESS_INFORMATION
        {
            public int hProcess;
            public int hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool CloseHandle(int hThread);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern int SuspendThread(int hThread);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern int ResumeThread(int hThread);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool GetThreadContext(int hThread, ref ThreadContext lpContext);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool SetThreadContext(int hThread, ref ThreadContext lpContext);

        [Flags]
        public enum ThreadAccess : uint
        {
            Terminate = 0x0001,
            SuspendResume = 0x0002,
            GetContext = 0x0008,
            SetContext = 0x0010,
            SetInfo = 0x0020,
            QueryInfo = 0x0040,
            SetToken = 0x0080,
            Impersonate = 0x0100,
            DirectImpersonate = 0x0200,
        }

        [Flags]
        public enum ContextFlags : uint
        {
            I386 = 0x10000,
            Control = I386 | 0x01,
            Integer = I386 | 0x02,
            Segments = I386 | 0x04,
            FloatingPoint = I386 | 0x08,
            DebugRegisters = I386 | 0x10,
            ExtendedRegisers = I386 | 0x20,
            Full = Control | Integer | Segments,
            All = Control | Integer | Segments | FloatingPoint | DebugRegisters | ExtendedRegisers,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XmmSave
        {
            public int ControlWord;
            public int StatusWord;
            public int TagWord;
            public int ErrorOffset;
            public int ErrorSelector;
            public int DataOffset;
            public int DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] RegisterArea;
            public int Cr0NpxState;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ThreadContext
        {
            public ContextFlags Flags;
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            public XmmSave Xmm;
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            public uint Ebp;
            public uint Eip;
            public uint SegCs;
            public uint EFlags;
            public uint Esp;
            public uint SegSs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] ExtendedRegisters;
        }
    }
}