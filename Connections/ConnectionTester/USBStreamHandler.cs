namespace ConnectionTester
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class UsbStreamHandler : Stream
    {
        SECURITY_ATTRIBUTES lpSecurityAttributes = new SECURITY_ATTRIBUTES();
        // Constants for Windows API functions and flags
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint GENERIC_READ_WRITE = GENERIC_READ | GENERIC_WRITE;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_READ_WRITE = FILE_SHARE_READ | FILE_SHARE_WRITE;

        private const uint OPEN_EXISTING = 3;

        // Define IOCTL codes
        private static uint IOCTL_CMWDMUSB_SELECT_BOOT_CFG = CMWDMUSB_BUFFERED_CTL(10);
        private static uint IOCTL_CMWDMUSB_GET_STRING_DESC = CMWDMUSB_BUFFERED_CTL(7);

        // Define CMWDMUSB_BUFFERED_CTL macro
        public static uint CMWDMUSB_BUFFERED_CTL(uint Function)
        {
            return ((0x22U << 16) | (0U << 14) | ((Function + 2048U) << 2) | (0U));
        }
        private string deviceName;
        private IntPtr h;

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override long Length => throw new NotImplementedException();

        public override bool CanWrite => true;

        public override bool CanSeek => false;

        public override bool CanRead => true;

        public bool IsOpened { get; set; } = false;
        private void CleanUp()
        {
            if (h == IntPtr.Zero) return;

            try
            {
                if (h != IntPtr.Zero)
                {
                    CloseHandle(h);
                    h = IntPtr.Zero;
                    IsOpened = false;
                }
            }
            catch
            {
                // Handle any cleanup exceptions if necessary
            }
        }

        private void ThrowWin32Error()
        {
            CleanUp();
            var ex = new Win32Exception(Marshal.GetLastWin32Error());
            throw new IOException($"{deviceName}: {ex.Message}", ex);
        }

        public UsbStreamHandler(string deviceName0)
        {
            try
            {
                h = CreateFileW(
                    deviceName0,
                    GENERIC_READ_WRITE,
                    FILE_SHARE_READ_WRITE,
                   ref lpSecurityAttributes,
                    OPEN_EXISTING,
                    0,
                    IntPtr.Zero
                );

                int errorCode = Marshal.GetLastWin32Error();

                if (h == IntPtr.Zero || h == new IntPtr(-1) || errorCode != 0)
                {
                    ThrowWin32Error();
                }

                // Example of using DeviceIoControl to retrieve string descriptors
                byte[] buf = new byte[256];
                uint bytesRead;
                for (int stridx = 1; stridx < 6; stridx++)
                {
                    bool success = DeviceIoControl(
                        h,
                        IOCTL_CMWDMUSB_GET_STRING_DESC,
                        Marshal.UnsafeAddrOfPinnedArrayElement(new byte[] { (byte)stridx }, 0),
                        1,
                        buf,
                        (uint)buf.Length,
                        out bytesRead,
                        IntPtr.Zero
                    );

                    if (!success)
                    {
                        ThrowWin32Error();
                    }

                }

                // Example of using DeviceIoControl to select boot configuration
                uint dwBytes;
                if (!DeviceIoControl(
                    h,
                    IOCTL_CMWDMUSB_SELECT_BOOT_CFG,
                    IntPtr.Zero,
                    0,
                    null,
                    0,
                    out dwBytes,
                    IntPtr.Zero
                ))
                {
                    ThrowWin32Error();
                }
                IsOpened = true;
            }
            catch
            {
                Dispose(true);
                throw;
            }
        }

        public override void Close()
        {
            base.Close();
        }

        public override void Flush()
        {
            // No operation needed
        }

        public override long Seek(long pos, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long len)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || buffer.Length - offset < count) throw new ArgumentException("Invalid offset or count.");

            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr bufferPtr = bufferHandle.AddrOfPinnedObject() + offset;
                if (!ReadFile(h, bufferPtr, (uint)count, out uint bytesRead, IntPtr.Zero))
                {
                    ThrowWin32Error();
                }
                return (int)bytesRead;
            }
            finally
            {
                bufferHandle.Free();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || buffer.Length - offset < count) throw new ArgumentException("Invalid offset or count.");

            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr bufferPtr = bufferHandle.AddrOfPinnedObject() + offset;
                if (!WriteFile(h, bufferPtr, (uint)count, out uint bytesWritten, IntPtr.Zero))
                {
                    ThrowWin32Error();
                }
                if (bytesWritten < (uint)count)
                {
                    throw new InternalBufferOverflowException();
                }
            }
            finally
            {
                bufferHandle.Free();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanUp();
            }
            base.Dispose(disposing);
        }
        
        ~UsbStreamHandler()
        {
            Dispose(false);
        }

        #region P/Invoke Declarations


        private const uint IOCTL_USB_GET_DESCRIPTOR = 0x0000C004; // Example IOCTL value, replace with actual value
        private const uint IOCTL_USB_RESET_DEVICE = 0x0000C008; // Example IOCTL value, replace with actual value

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
       string lpFileName,
       uint dwDesiredAccess,
        uint dwShareMode,
       ref SECURITY_ATTRIBUTES lpSecurityAttributes,
       uint dwCreationDisposition,
       uint dwFlagsAndAttributes,
       IntPtr hTemplateFile
   );
        // Define SECURITY_ATTRIBUTES structure
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            byte[] lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion
    }
}
