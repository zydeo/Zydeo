using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Text;

namespace ZD.AU
{
    /// <summary>
    /// Wraps up Windows API calls for setting service's start mode and security descriptors.
    /// </summary>
    internal static class ServiceMgr
    {
        /// <summary>
        /// This is the SDDL that needs to be set on the AU service, so that all users will be able to start it
        /// </summary>
        public const string AuSvcDaclSDDL = "D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;LCRP;;;IU)(A;;LCRP;;;SU)";

        [StructLayoutAttribute(LayoutKind.Sequential)]
        struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        private enum SecurityInformation : uint
        {
            OWNER_SECURITY_INFORMATION = 0x00000001,
            GROUP_SECURITY_INFORMATION = 0x00000002,
            DACL_SECURITY_INFORMATION = 0x00000004,
            SACL_SECURITY_INFORMATION = 0x00000008,
            PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,
            PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
            UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
            UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
        };

        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = false)]
        private static extern bool ConvertSecurityDescriptorToStringSecurityDescriptor(
            [In] byte[] SecurityDescriptor,
            [In] int RequestedStringSDRevision,
            [In] SecurityInformation SecurityInformation,
            [Out] out IntPtr StringSecurityDescriptor,
            [Out] out int StringSecurityDescriptorLen
        );


        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = false)]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            [In] string StringSecurityDescriptor,
            [In] uint StringSDRevision,
            [Out] out IntPtr SecurityDescriptor,
            [Out] out int SecurityDescriptorSize
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool QueryServiceObjectSecurity(IntPtr serviceHandle, System.Security.AccessControl.SecurityInfos secInfo, ref SECURITY_DESCRIPTOR lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool QueryServiceObjectSecurity(SafeHandle serviceHandle, System.Security.AccessControl.SecurityInfos secInfo, byte[] lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool SetServiceObjectSecurity(SafeHandle serviceHandle, System.Security.AccessControl.SecurityInfos secInfos, byte[] lpSecDesrBuf);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            UInt32 nServiceType,
            UInt32 nStartType,
            UInt32 nErrorControl,
            String lpBinaryPathName,
            String lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In] char[] lpDependencies,
            String lpServiceStartName,
            String lpPassword,
            String lpDisplayName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(
            IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(
            string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        /// <summary>
        /// Gets a service's security descriptor as SDDL.
        /// </summary>
        public static string GetServiceSDDL(string ServiceName, SecurityInfos SecurityInfos)
        {
            ServiceController sc = new ServiceController(ServiceName);
            byte[] psd = new byte[0];
            uint bufSizeNeeded;
            bool ok = QueryServiceObjectSecurity(sc.ServiceHandle, SecurityInfos, psd, 0, out bufSizeNeeded);
            if (!ok)
            {
                int err = Marshal.GetLastWin32Error();
                if (err == 122) // ERROR_INSUFFICIENT_BUFFER
                {
                    // expected; now we know bufsize
                    psd = new byte[bufSizeNeeded];
                    ok = QueryServiceObjectSecurity(sc.ServiceHandle, SecurityInfos, psd, bufSizeNeeded, out bufSizeNeeded);
                }
                else
                {
                    throw new ApplicationException("error calling QueryServiceObjectSecurity() to get DACL for SeaweedService: error code=" + err);
                }
            }
            if (!ok)
                throw new ApplicationException("error calling QueryServiceObjectSecurity(2) to get DACL for SeaweedService: error code=" + Marshal.GetLastWin32Error());

            return ConvertSDtoStringSD(psd);
        }

        /// <summary>
        /// Sets a service's security descriptor as SDDL.
        /// </summary>
        public static void SetServiceSDDL(string ServiceName, SecurityInfos SecurityInfos, string SDDL)
        {
            ServiceController sc = new ServiceController(ServiceName);
            bool ok = SetServiceObjectSecurity(sc.ServiceHandle, SecurityInfos, ConvertStringSDtoSD(SDDL));
            if (!ok)
                throw new ApplicationException("error calling SetServiceObjectSecurity(); error code=" + Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Converts between two ugly ways of saying "security descriptor".
        /// </summary>
        public static string ConvertSDtoStringSD(byte[] securityDescriptor)
        {
            IntPtr stringSecurityDescriptorPtr = IntPtr.Zero;
            int stringSecurityDescriptorSize = 0;

            bool result =
                ConvertSecurityDescriptorToStringSecurityDescriptor(
                securityDescriptor,
                1,
                SecurityInformation.DACL_SECURITY_INFORMATION |
                SecurityInformation.GROUP_SECURITY_INFORMATION |
                SecurityInformation.OWNER_SECURITY_INFORMATION |
                SecurityInformation.SACL_SECURITY_INFORMATION,
                out stringSecurityDescriptorPtr,
                out stringSecurityDescriptorSize);
            if (!result)
            {
                Console.WriteLine("Fail to convert" +
                    " SD to string SD:");
                throw new Win32Exception(
                    Marshal.GetLastWin32Error());
            }

            string stringSecurityDescriptor =
                Marshal.PtrToStringAuto(
                    stringSecurityDescriptorPtr);

            return stringSecurityDescriptor;
        }

        /// <summary>
        /// Converts between two ugly ways of saying "security descriptor".
        /// </summary>
        public static byte[] ConvertStringSDtoSD(
            string stringSecurityDescriptor)
        {
            IntPtr securityDescriptorPtr = IntPtr.Zero;
            int securityDescriptorSize = 0;

            bool result =
                ConvertStringSecurityDescriptorToSecurityDescriptor(
                stringSecurityDescriptor,
                1,
                out securityDescriptorPtr,
                out securityDescriptorSize);
            if (!result)
            {
                Console.WriteLine(
                    "Fail to convert string SD to SD:");
                throw new Win32Exception(
                    Marshal.GetLastWin32Error());
            }

            byte[] securityDescriptor =
                    new byte[securityDescriptorSize];
            Marshal.Copy(securityDescriptorPtr,
                securityDescriptor, 0, securityDescriptorSize);

            return securityDescriptor;
        }

        /// <summary>
        /// Changes a service's start mode.
        /// </summary>
        public static void ChangeStartMode(string ServiceName, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
                throw new ExternalException("Open Service Manager Error");

            var serviceHandle = OpenService(
                scManagerHandle,
                ServiceName,
                SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            if (serviceHandle == IntPtr.Zero)
                throw new ExternalException("Open Service Error");

            var result = ChangeServiceConfig(
                serviceHandle,
                SERVICE_NO_CHANGE,
                (uint)mode,
                SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                throw new ExternalException("Could not change service start type: "
                    + win32Exception.Message);
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }
    }
}
