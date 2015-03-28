using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ZD.AU
{
    /// <summary>
    /// Wraps a named pipe into a stream.
    /// </summary>
    /// <remarks>http://www.pinvoke.net/default.aspx/kernel32.createnamedpipe</remarks>
    internal class NamedPipeStream : Stream
    {
        [DllImport("kernel32.dll", EntryPoint = "CreateFile", SetLastError = true)]
        private static extern IntPtr CreateFile(String lpFileName,
            UInt32 dwDesiredAccess, UInt32 dwShareMode,
            IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateNamedPipe(
            String lpName,                       // pipe name
            uint dwOpenMode,                     // pipe open mode
            uint dwPipeMode,                     // pipe-specific modes
            uint nMaxInstances,                  // maximum number of instances
            uint nOutBufferSize,                 // output buffer size
            uint nInBufferSize,                  // input buffer size
            uint nDefaultTimeOut,                // time-out interval
            IntPtr pipeSecurityDescriptor        // SD
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DisconnectNamedPipe(IntPtr hHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ConnectNamedPipe(
            IntPtr hHandle,                      // handle to named pipe
            IntPtr lpOverlapped                  // overlapped structure
            );

        [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
        private static extern bool PeekNamedPipe(IntPtr handle,
            byte[] buffer, uint nBufferSize, ref uint bytesRead,
            ref uint bytesAvail, ref uint BytesLeftThisMessage);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr handle,
            byte[] buffer, uint toRead, ref uint read, IntPtr lpOverLapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(IntPtr handle,
            byte[] buffer, uint count, ref uint written, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FlushFileBuffers(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetLastError();

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        //Constants for dwDesiredAccess:
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 GENERIC_WRITE = 0x40000000;

        //Constants for return value:
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        //Constants for dwFlagsAndAttributes:
        private const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        private const UInt32 FILE_FLAG_NO_BUFFERING = 0x20000000;

        //Constants for dwCreationDisposition:
        private const UInt32 OPEN_EXISTING = 3;
    
        /// <summary>
        /// Outbound pipe access.
        /// </summary>
        private const uint PIPE_ACCESS_OUTBOUND = 0x00000002;
        
        /// <summary>
        /// Duplex pipe access.
        /// </summary>
        private const uint PIPE_ACCESS_DUPLEX = 0x00000003;
        
        /// <summary>
        /// Inbound pipe access.
        /// </summary>
        private const uint PIPE_ACCESS_INBOUND = 0x00000001;
        
        /// <summary>
        /// Pipe blocking mode.
        /// </summary>
        private const uint PIPE_WAIT = 0x00000000;
        
        /// <summary>
        /// Pipe non-blocking mode.
        /// </summary>
        private const uint PIPE_NOWAIT = 0x00000001;
        
        /// <summary>
        /// Pipe read mode of type Byte.
        /// </summary>
        private const uint PIPE_READMODE_BYTE = 0x00000000;
        
        /// <summary>
        /// Pipe read mode of type Message.
        /// </summary>
        private const uint PIPE_READMODE_MESSAGE = 0x00000002;
        
        /// <summary>
        /// Byte pipe type.
        /// </summary>
        private const uint PIPE_TYPE_BYTE = 0x00000000;
        
        /// <summary>
        /// Message pipe type.
        /// </summary>
        private const uint PIPE_TYPE_MESSAGE = 0x00000004;
        
        /// <summary>
        /// Pipe client end.
        /// </summary>
        private const uint PIPE_CLIENT_END = 0x00000000;
        
        /// <summary>
        /// Pipe server end.
        /// </summary>
        private const uint PIPE_SERVER_END = 0x00000001;
        
        /// <summary>
        /// Unlimited server pipe instances.
        /// </summary>
        private const uint PIPE_UNLIMITED_INSTANCES = 255;
        
        /// <summary>
        /// Waits indefinitely when connecting to a pipe.
        /// </summary>
        private const uint NMPWAIT_WAIT_FOREVER = 0xffffffff;
        
        /// <summary>
        /// Does not wait for the named pipe.
        /// </summary>
        private const uint NMPWAIT_NOWAIT = 0x00000001;
        
        /// <summary>
        /// Uses the default time-out specified in a call to the CreateNamedPipe method.
        /// </summary>
        private const uint NMPWAIT_USE_DEFAULT_WAIT = 0x00000000;

        private const ulong ERROR_PIPE_CONNECTED = 535;

        /// <summary>
        /// Server side creation options
        /// </summary>
        public enum ServerMode
        {
            InboundOnly = (int)PIPE_ACCESS_INBOUND,
            OutboundOnly = (int)PIPE_ACCESS_OUTBOUND,
            Bidirectional = (int)(PIPE_ACCESS_INBOUND + PIPE_ACCESS_OUTBOUND)
        };

        public enum PeerType
        {
            Client = 0,
            Server = 1
        };

        IntPtr _handle;
        FileAccess _mode;
        PeerType _peerType;

        protected NamedPipeStream()
        {
            _handle = IntPtr.Zero;
            _mode = (FileAccess)0;
            _peerType = PeerType.Server;
        }

        public NamedPipeStream(string pipename, FileAccess mode)
        {
            _handle = IntPtr.Zero;
            _peerType = PeerType.Client;
            Open(pipename, mode);
        }
        public NamedPipeStream(IntPtr handle, FileAccess mode)
        {
            _handle = handle;
            _mode = mode;
            _peerType = PeerType.Client;
        }

        /// <summary>
        /// Opens the client side of a pipe.
        /// </summary>
        /// <param name="pipe">Full path to the pipe, e.g. '\\.\pipe\mypipe'</param>
        /// <param name="mode">Read,Write, or ReadWrite - must be compatible with server-side creation mode</param>
        public void Open(string pipename, FileAccess mode)
        {
            uint pipemode = 0;
            if ((mode & FileAccess.Read) > 0) pipemode |= GENERIC_READ;
            if ((mode & FileAccess.Write) > 0) pipemode |= GENERIC_WRITE;
            IntPtr handle = CreateFile(pipename, pipemode, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (handle == INVALID_HANDLE_VALUE)
            {
                int err = Marshal.GetLastWin32Error();
                throw new Win32Exception(err,
                    string.Format("NamedPipeStream.Open failed, win32 error code {0}, pipename '{1}' ", err, pipename));
            }

            _mode = mode;
            _handle = handle;
        }

        /// <summary>
        /// Create a named pipe instance.
        /// </summary>
        /// <param name="pipeName">Local name (the part after \\.\pipe\)</param>
        public static NamedPipeStream Create(string pipeName, ServerMode mode, FileSecurity fileSec)
        {
            SECURITY_ATTRIBUTES pipeSecurity = new SECURITY_ATTRIBUTES();
            pipeSecurity.nLength = Marshal.SizeOf(pipeSecurity);
            byte[] src = fileSec.GetSecurityDescriptorBinaryForm();
            IntPtr dest = Marshal.AllocHGlobal(src.Length);
            Marshal.Copy(src, 0, dest, src.Length);
            pipeSecurity.lpSecurityDescriptor = dest;
            IntPtr pipeSecPtr = Marshal.AllocHGlobal(pipeSecurity.nLength);
            Marshal.StructureToPtr(pipeSecurity, pipeSecPtr, true);

            IntPtr handle = IntPtr.Zero;
            string name = @"\\.\pipe\" + pipeName;

            handle = CreateNamedPipe(
                name,
                (uint)mode, PIPE_TYPE_MESSAGE | PIPE_WAIT,
                PIPE_UNLIMITED_INSTANCES,
                0,    // outBuffer,
                1024, // inBuffer,
                NMPWAIT_WAIT_FOREVER,
                pipeSecPtr);
            if (handle == INVALID_HANDLE_VALUE)
                throw new Win32Exception("Error creating named pipe " + name + " . Internal error: " + Marshal.GetLastWin32Error().ToString());
            
            // Set members persistently...
            NamedPipeStream self = new NamedPipeStream();
            self._handle = handle;
            switch (mode)
            {
                case ServerMode.InboundOnly:
                    self._mode = FileAccess.Read;
                    break;
                case ServerMode.OutboundOnly:
                    self._mode = FileAccess.Write;
                    break;
                case ServerMode.Bidirectional:
                    self._mode = FileAccess.ReadWrite;
                    break;
            }
            return self;
        }

        /// <summary>
        /// Server only: block until client connects
        /// </summary>
        public bool Listen()
        {
            if (_peerType != PeerType.Server)
                throw new Exception("Listen() is only for server-side streams");
            DisconnectNamedPipe(_handle);
            if (ConnectNamedPipe(_handle, IntPtr.Zero) != true)
            {
                uint lastErr = (uint)Marshal.GetLastWin32Error();
                if (lastErr == ERROR_PIPE_CONNECTED)
                    return true;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Server only: disconnect the pipe.  For most applications, you should just call Listen()
        /// instead, which automatically does a disconnect of any old connection.
        /// </summary>
        public void Disconnect()
        {
            if (_peerType != PeerType.Server)
                throw new Exception("Disconnect() is only for server-side streams");
            DisconnectNamedPipe(_handle);
        }

        /// <summary>
        /// Returns true if client is connected.  Should only be called after Listen() succeeds.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_peerType != PeerType.Server)
                    throw new Exception("IsConnected() is only for server-side streams");

                if (ConnectNamedPipe(_handle, IntPtr.Zero) == false)
                {
                    if ((uint)Marshal.GetLastWin32Error() == ERROR_PIPE_CONNECTED)
                        return true;
                }
                return false;
            }
        }

        public bool DataAvailable
        {
            get
            {
                uint bytesRead = 0, avail = 0, thismsg = 0;

                bool result = PeekNamedPipe(_handle,
                    null, 0, ref bytesRead, ref avail, ref thismsg);
                return (result == true && avail > 0);
            }
        }

        public override bool CanRead
        {
            get { return (_mode & FileAccess.Read) > 0; }
        }

        public override bool CanWrite
        {
            get { return (_mode & FileAccess.Write) > 0; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotSupportedException("NamedPipeStream does not support seeking"); }
        }

        public override long Position
        {
            get { throw new NotSupportedException("NamedPipeStream does not support seeking"); }
            set { }
        }

        public override void Flush()
        {
            if (_handle == IntPtr.Zero)
                throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");
            FlushFileBuffers(_handle);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "The buffer to read into cannot be null");
            if (buffer.Length < (offset + count))
                throw new ArgumentException("Buffer is not large enough to hold requested data", "buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", offset, "Offset cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, "Count cannot be negative");
            if (!CanRead)
                throw new NotSupportedException("The stream does not support reading");
            if (_handle == IntPtr.Zero)
                throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");

            // first read the data into an internal buffer since ReadFile cannot read into a buf at
            // a specified offset
            uint read = 0;

            byte[] buf = buffer;
            if (offset != 0)
            {
                buf = new byte[count];
            }
            bool f = ReadFile(_handle, buf, (uint)count, ref read, IntPtr.Zero);
            if (!f)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "ReadFile failed");
            }
            if (offset != 0)
            {
                for (int x = 0; x < read; x++)
                {
                    buffer[offset + x] = buf[x];
                }

            }
            return (int)read;
        }

        public override void Close()
        {
            CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }

        public override void SetLength(long length)
        {
            throw new NotSupportedException("NamedPipeStream doesn't support SetLength");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "The buffer to write into cannot be null");
            if (buffer.Length < (offset + count))
                throw new ArgumentException("Buffer does not contain amount of requested data", "buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", offset, "Offset cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, "Count cannot be negative");
            if (!CanWrite)
                throw new NotSupportedException("The stream does not support writing");
            if (_handle == IntPtr.Zero)
                throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");

            // copy data to internal buffer to allow writing from a specified offset
            if (offset != 0)
            {
                byte[] buf = new Byte[count];
                for (int x = 0; x < count; x++)
                {
                    buf[x] = buffer[offset + x];
                }
                buffer = buf;
            }
            uint written = 0;
            bool result = WriteFile(_handle, buffer, (uint)count, ref written, IntPtr.Zero);

            if (!result)
            {
                int err = (int)Marshal.GetLastWin32Error();
                throw new Win32Exception(err, "Writing to the stream failed");
            }
            if (written < count)
                throw new IOException("Unable to write entire buffer to stream");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("NamedPipeStream doesn't support seeking");
        }
    }
}
