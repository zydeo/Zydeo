using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace ZD.AU
{
    internal class ZydeoServiceController : ServiceController
    {
        private string m_ImagePath;
        private ServiceController[] m_DependentServices;
        private ServiceController[] m_ServicesDependedOn;

        public ZydeoServiceController()
            : base()
        {
        }

        public ZydeoServiceController(string name)
            : base(name)
        {
        }

        public ZydeoServiceController(string name, string machineName)
            : base(name, machineName)
        {
        }

        public string ImagePath
        {
            get
            {
                if (m_ImagePath == null) m_ImagePath = GetImagePath();
                return m_ImagePath;
            }
        }

        public new ServiceController[] DependentServices
        {
            get
            {
                if (m_DependentServices == null)
                    m_DependentServices = ZydeoServiceController.GetServices(base.DependentServices);
                return m_DependentServices;
            }
        }

        public new ServiceController[] ServicesDependedOn
        {
            get
            {
                if (m_ServicesDependedOn == null)
                    m_ServicesDependedOn = ZydeoServiceController.GetServices(base.ServicesDependedOn);
                return m_ServicesDependedOn;
            }
        }

        public static new ServiceController[] GetServices()
        {
            return GetServices(".");
        }

        public static new ServiceController[] GetServices(string machineName)
        {
            return GetServices(System.ServiceProcess.ServiceController.GetServices(machineName));
        }

        private string GetImagePath()
        {
            string registryPath = @"SYSTEM\CurrentControlSet\Services\" + ServiceName;
            RegistryKey keyHKLM = Registry.LocalMachine;

            RegistryKey key;
            if (MachineName != "" && MachineName != ".")
                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, this.MachineName).OpenSubKey(registryPath);
            else key = keyHKLM.OpenSubKey(registryPath);

            string value = key.GetValue("ImagePath").ToString();
            key.Close();
            return ExpandEnvironmentVariables(value);
        }

        private string ExpandEnvironmentVariables(string path)
        {
            if (MachineName == "" || MachineName == ".")
                return Environment.ExpandEnvironmentVariables(path);
            else
            {
                string systemRootKey = @"Software\Microsoft\Windows NT\CurrentVersion\";

                RegistryKey key = RegistryKey.OpenRemoteBaseKey
                     (RegistryHive.LocalMachine, MachineName).OpenSubKey(systemRootKey);
                string expandedSystemRoot = key.GetValue("SystemRoot").ToString();
                key.Close();

                path = path.Replace("%SystemRoot%", expandedSystemRoot);
                return path;
            }
        }

        private static ServiceController[] GetServices(System.ServiceProcess.ServiceController[] systemServices)
        {
            List<ServiceController> services = new List<ServiceController>
                (systemServices.Length);
            foreach (System.ServiceProcess.ServiceController service in systemServices)
            {
                services.Add(new ServiceController(service.ServiceName, service.MachineName));
            }
            return services.ToArray();
        }
    }
}
