using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharedMemoryCSharp
{
    public partial class frmMain : Form
    {
        #region Win32 DLL Import

        /// <summary>
        /// Memory Protection Constants
        /// http://msdn.microsoft.com/en-us/library/aa366786.aspx
        /// </summary>
        [Flags]
        public enum FileProtection : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
            SEC_FILE = 0x800000,
            SEC_IMAGE = 0x1000000,
            SEC_RESERVE = 0x4000000,
            SEC_COMMIT = 0x8000000,
            SEC_NOCACHE = 0x10000000
        }

        /// <summary>
        /// Access rights for file mapping objects
        /// http://msdn.microsoft.com/en-us/library/aa366559.aspx
        /// </summary>
        [Flags]
        public enum FileMapAccess
        {
            FILE_MAP_COPY = 0x0001,
            FILE_MAP_WRITE = 0x0002,
            FILE_MAP_READ = 0x0004,
            FILE_MAP_ALL_ACCESS = 0x000F001F
        }

        public class FileMappingNative
        {
            public const int INVALID_HANDLE_VALUE = -1;
            /// <summary>
            /// Creates or opens a named or unnamed file mapping object for a 
            /// specified file.
            /// </summary>
            /// <param name="hFile">
            /// A handle to the file from which to create a file mapping object.
            /// </param>
            /// <param name="lpAttributes">
            /// A pointer to a SECURITY_ATTRIBUTES structure that determines whether 
            /// a returned handle can be inherited by child processes.
            /// </param>
            /// <param name="flProtect">
            /// Specifies the page protection of the file mapping object. All mapped 
            /// views of the object must be compatible with this protection.
            /// </param>
            /// <param name="dwMaximumSizeHigh">
            /// The high-order DWORD of the maximum size of the file mapping object.
            /// </param>
            /// <param name="dwMaximumSizeLow">
            /// The low-order DWORD of the maximum size of the file mapping object.
            /// </param>
            /// <param name="lpName">
            /// The name of the file mapping object.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is a handle to the newly 
            /// created file mapping object.
            /// </returns>
            [DllImport("Kernel32.dll", SetLastError = true)]
            public static extern IntPtr CreateFileMapping(
                IntPtr hFile,                   // Handle to the file
                IntPtr lpAttributes,            // Security Attributes
                FileProtection flProtect,       // File protection
                uint dwMaximumSizeHigh,         // High-order DWORD of size
                uint dwMaximumSizeLow,          // Low-order DWORD of size
                string lpName                   // File mapping object name
                );

            /// <summary>
            /// Maps a view of a file mapping into the address space of a calling
            /// process.
            /// </summary>
            /// <param name="hFileMappingObject">
            /// A handle to a file mapping object. The CreateFileMapping and 
            /// OpenFileMapping functions return this handle.
            /// </param>
            /// <param name="dwDesiredAccess">
            /// The type of access to a file mapping object, which determines the 
            /// protection of the pages.
            /// </param>
            /// <param name="dwFileOffsetHigh">
            /// A high-order DWORD of the file offset where the view begins.
            /// </param>
            /// <param name="dwFileOffsetLow">
            /// A low-order DWORD of the file offset where the view is to begin.
            /// </param>
            /// <param name="dwNumberOfBytesToMap">
            /// The number of bytes of a file mapping to map to the view. All bytes 
            /// must be within the maximum size specified by CreateFileMapping.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is the starting address 
            /// of the mapped view.
            /// </returns>
            [DllImport("Kernel32.dll", SetLastError = true)]
            public static extern IntPtr MapViewOfFile(
                IntPtr hFileMappingObject,      // Handle to the file mapping object
                FileMapAccess dwDesiredAccess,  // Desired access to file mapping object
                uint dwFileOffsetHigh,          // High-order DWORD of file offset
                uint dwFileOffsetLow,           // Low-order DWORD of file offset
                uint dwNumberOfBytesToMap       // Number of bytes map to the view
                );

            /// <summary>
            /// Opens a named file mapping object.
            /// </summary>
            /// <param name="dwDesiredAccess">
            /// The access to the file mapping object. This access is checked against 
            /// any security descriptor on the target file mapping object.
            /// </param>
            /// <param name="bInheritHandle">
            /// If this parameter is TRUE, a process created by the CreateProcess 
            /// function can inherit the handle; otherwise, the handle cannot be 
            /// inherited.
            /// </param>
            /// <param name="lpName">
            /// The name of the file mapping object to be opened.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is an open handle to the 
            /// specified file mapping object.
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr OpenFileMapping(
              FileMapAccess dwDesiredAccess,    // Access mode
              bool bInheritHandle,              // Inherit flag
              string lpName                     // File mapping object name
              );

            /// <summary>
            /// Unmaps a mapped view of a file from the calling process's address space.
            /// </summary>
            /// <param name="lpBaseAddress">
            /// A pointer to the base address of the mapped view of a file that is to
            /// be unmapped.
            /// </param>
            /// <returns></returns>
            [DllImport("Kernel32.dll", SetLastError = true)]
            public static extern bool UnmapViewOfFile(
                IntPtr lpBaseAddress             // Base address of mapped view
                );

            /// <summary>
            /// Closes an open object handle.
            /// </summary>
            /// <param name="hHandle">Handle to an open object.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr hHandle);

            /// <summary>
            /// Retrieves the calling thread's last-error code value.
            /// </summary>
            /// <returns>
            /// The return value is the calling thread's last-error code value.
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint GetLastError();
        }
        #endregion

        char workType = 'C';
        System.Windows.Forms.Timer memoryTimer = new System.Windows.Forms.Timer();
        const uint BUFFER_SIZE = 256;
        IntPtr pBuf;
        IntPtr hMapFile;
        string strMapFileName = "SharedMemory_CPP_CSHARP";
        Random rand = new Random(DateTime.Now.Millisecond);

        public frmMain()
        {
            InitializeComponent();

            memoryTimer.Interval = 1000 / 1;
            memoryTimer.Tick += MemoryTimer_Tick;
        }

        private void MemoryTimer_Tick(object sender, EventArgs e)
        {
            if (workType == 'S')
            {
                string strMessage = rand.Next(100).ToString("00");
                byte[] bMessage = Encoding.Unicode.GetBytes(strMessage);
                Marshal.Copy(bMessage, 0, pBuf, bMessage.Length);

                txtMsg.AppendText("WriteMemory: " + strMessage + "\r\n");
            }
            else if (workType == 'C')
            {
                string strMessage = Marshal.PtrToStringUni(pBuf);

                txtMsg.AppendText("ReadMemory: " + strMessage + "\r\n");
            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "START")
            {
                btnStart.Text = "STOP";

                memoryTimer.Stop();

                rbServer.Enabled = true;
                rbClient.Enabled = true;

                Thread.Sleep(100);

                FileMappingNative.UnmapViewOfFile(pBuf);
                FileMappingNative.CloseHandle(hMapFile);
            }
            else
            {
                btnStart.Text = "START";

                rbServer.Enabled = false;
                rbClient.Enabled = false;

                if (workType == 'S')
                {
                    // Create the file mapping object
                    hMapFile = FileMappingNative.CreateFileMapping(
                        (IntPtr)FileMappingNative.INVALID_HANDLE_VALUE,
                        IntPtr.Zero,
                        FileProtection.PAGE_READWRITE,
                        0,
                        BUFFER_SIZE,
                        strMapFileName);

                    if (hMapFile == IntPtr.Zero)
                    {
                        Console.WriteLine("Unable to create file mapping object w/err 0x{0:X}",
                            FileMappingNative.GetLastError());
                        return;
                    }
                    Console.WriteLine("The file mapping object, {0}, is created.",
                        strMapFileName);


                    /////////////////////////////////////////////////////////////////////
                    // Maps the view of the file mapping into the address space of the 
                    // current process.
                    // 

                    // Create file view from the file mapping object.
                    pBuf = FileMappingNative.MapViewOfFile(
                        hMapFile,
                        FileMapAccess.FILE_MAP_ALL_ACCESS,
                        0,
                        0,
                        BUFFER_SIZE);

                    if (pBuf == IntPtr.Zero)
                    {
                        Console.WriteLine("Unable to map view of file w/err 0x{0:X}",
                            FileMappingNative.GetLastError());
                        return;
                    }
                    Console.WriteLine("The file view is created.");

                    memoryTimer.Start();
                }
                else if (workType == 'C')
                {
                    // Open the named file mapping.
                    hMapFile = FileMappingNative.OpenFileMapping(
                        FileMapAccess.FILE_MAP_ALL_ACCESS,
                        false,
                        strMapFileName);

                    if (hMapFile == IntPtr.Zero)
                    {
                        Console.WriteLine("Unable to open file mapping object w/err 0x{0:X}",
                            FileMappingNative.GetLastError());
                        return;
                    }
                    Console.WriteLine("The file mapping object, {0}, is opened.",
                        strMapFileName);

                    pBuf = FileMappingNative.MapViewOfFile(
                        hMapFile,
                        FileMapAccess.FILE_MAP_ALL_ACCESS,
                        0,
                        0,
                        BUFFER_SIZE);

                    if (pBuf == IntPtr.Zero)
                    {
                        Console.WriteLine("Unable to map view of file w/err 0x{0:X}",
                            FileMappingNative.GetLastError());
                        FileMappingNative.CloseHandle(hMapFile);
                        return;
                    }
                    Console.WriteLine("The file view is created.");

                    memoryTimer.Start();
                }
            }
        }

        private void rbServer_CheckedChanged(object sender, EventArgs e)
        {
            workType = 'S';
        }

        private void rbClient_CheckedChanged(object sender, EventArgs e)
        {
            workType = 'C';
        }
    }
}
