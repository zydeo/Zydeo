using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using Microsoft.Win32;

namespace ZD.AU
{
    /// <summary>
    /// Encapsulates generation and storage of salt in registry.
    /// </summary>
    internal static class Salt
    {
        /// <summary>
        /// Makes sure salt is stored in registry; stores if not there yet.
        /// </summary>
        public static void EnsureSalt()
        {
            // Create/open Zydeo key in HKLM\Software
            string keyName = @"SOFTWARE\" + Magic.ZydeoSoftwareRegKey;
            using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey(keyName))
            {
                // Check if salt value is present and has the right type
                string[] valueNames = regKey.GetValueNames();
                string vx = valueNames.FirstOrDefault(x => x.ToLowerInvariant() == Magic.ZydeoSaltRegVal.ToLowerInvariant());
                // Value does not exist: create now
                if (vx == null)
                    regKey.SetValue(Magic.ZydeoSaltRegVal, getRandomSalt(), RegistryValueKind.DWord);
                // Value is there: is it a DWOD?
                else
                {
                    RegistryValueKind vkind = regKey.GetValueKind(vx);
                    // If not DWORD, delete and recreate
                    if (vkind != RegistryValueKind.DWord)
                    {
                        regKey.DeleteValue(vx);
                        regKey.SetValue(Magic.ZydeoSaltRegVal, getRandomSalt(), RegistryValueKind.DWord);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the salt, or 0 if there is no stored salt.
        /// </summary>
        /// <returns></returns>
        public static int GetSalt()
        {
            // Ppen Zydeo key in HKLM\Software
            string keyName = @"SOFTWARE\" + Magic.ZydeoSoftwareRegKey;
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(keyName))
            {
                // If key does not exist, salt is 0.
                if (regKey == null) return 0;
                // Get value. This returns null if value is missing.
                object sval = regKey.GetValue(Magic.ZydeoSaltRegVal);
                if (sval == null) return 0;
                // Verify type of value
                RegistryValueKind vkind = regKey.GetValueKind(Magic.ZydeoSaltRegVal);
                if (vkind != RegistryValueKind.DWord) return 0;
                // Got it. It's an int.
                return (int)sval;
            }
        }

        /// <summary>
        /// Generates a random, non-zero integer for salt.
        /// </summary>
        private static int getRandomSalt()
        {
            int salt = 0;
            while (salt == 0 || salt == 1)
            {
                Guid g = Guid.NewGuid();
                salt = g.GetHashCode();
            }
            return salt;
        }
    }
}
