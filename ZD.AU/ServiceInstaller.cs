using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
using System.Security.AccessControl;
using System.ServiceProcess;

namespace ZD.AU
{
    /// <summary>
    /// Wraps up Windows API calls to install and uninstall services, and to set security descriptors and start mode.
    /// </summary>
    internal static class ServiceInstaller
    {
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);

        [DllImport("Advapi32.dll")]
        public static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName,
        int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName,
        string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

        [DllImport("advapi32.dll")]
        public static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);

        [DllImport("advapi32.dll")]
        public static extern int DeleteService(IntPtr SVHANDLE);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        private const int SC_MANAGER_CREATE_SERVICE = 0x0002;
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        private const int SERVICE_ERROR_NORMAL = 0x00000001;
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int SERVICE_QUERY_CONFIG = 0x0001;
        private const int SERVICE_CHANGE_CONFIG = 0x0002;
        private const int SERVICE_QUERY_STATUS = 0x0004;
        private const int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
        private const int SERVICE_START = 0x0010;
        private const int SERVICE_STOP = 0x0020;
        private const int SERVICE_PAUSE_CONTINUE = 0x0040;
        private const int SERVICE_INTERROGATE = 0x0080;
        private const int SERVICE_USER_DEFINED_CONTROL = 0x0100;
        private const int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
            SERVICE_QUERY_CONFIG |
            SERVICE_CHANGE_CONFIG |
            SERVICE_QUERY_STATUS |
            SERVICE_ENUMERATE_DEPENDENTS |
            SERVICE_START |
            SERVICE_STOP |
            SERVICE_PAUSE_CONTINUE |
            SERVICE_INTERROGATE |
            SERVICE_USER_DEFINED_CONTROL);
        private const int SERVICE_AUTO_START = 0x00000002;
        private const int SERVICE_DEMAND_START = 0x00000003;
        private const int GENERIC_WRITE = 0x40000000;

        /// <summary>
        /// Installs and runs the service in the service control manager.
        /// </summary>
        public static bool InstallService(string svcPath, string svcName, string svcDispName)
        {
            IntPtr sc_handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
            if (sc_handle != IntPtr.Zero)
            {
                IntPtr sv_handle = CreateService(sc_handle, svcName, svcDispName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_AUTO_START, SERVICE_ERROR_NORMAL, svcPath, null, 0, null, null, null);

                if (sv_handle == IntPtr.Zero)
                {
                    CloseServiceHandle(sc_handle);
                    return false;
                }
                else
                {
                    CloseServiceHandle(sc_handle);
                    return true;
                }
            }
            else return false;
        }

        /// <summary>
        /// Uninstalls the service from the service conrol manager.
        /// </summary>
        public static bool UnInstallService(string svcName)
        {
            IntPtr sc_hndl = OpenSCManager(null, null, GENERIC_WRITE);
            if (sc_hndl != IntPtr.Zero)
            {
                int DELETE = 0x10000;
                IntPtr svc_hndl = OpenService(sc_hndl, svcName, DELETE);
                if (svc_hndl != IntPtr.Zero)
                {
                    int i = DeleteService(svc_hndl);

                    if (i != 0)
                    {
                        CloseServiceHandle(sc_hndl);
                        return true;
                    }
                    else
                    {
                        CloseServiceHandle(sc_hndl);
                        return false;
                    }
                }
                else return false;
            }
            else return false;
        }
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