#if WZ_APPX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SBkUpUI.Utils
{
    public class UWPHelper
    {
        private static string _helperName = "WzUWPHelperCS.dll";
        private static Dictionary<string, object> _oemPoliciesValue = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private static Assembly _helperAssembly = null;

        private static Assembly HelperAssembly
        {
            get
            {
                if (_helperAssembly == null)
                {
                    string folder = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Directory.FullName;
                    _helperAssembly = Assembly.LoadFrom(Path.Combine(folder, _helperName));
                }

                return _helperAssembly;
            }
        }

        /// <summary>
        /// For OEM versions, reading a Policy requires reading from the Data file first.
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static string GetOemPoliciesValue(string configKey)
        {
            try
            {
                object ret;

                // Read only once and store it
                if (_oemPoliciesValue.ContainsKey(configKey))
                {
                    ret = _oemPoliciesValue[configKey];
                }
                else
                {
                    var type = HelperAssembly.GetType("WzUWPHelperCS.WzUWPHelperCS");
                    ret = type.GetMethod("GetOemPoliciesValue").Invoke(null, new object[] { configKey });

                    _oemPoliciesValue.Add(configKey, ret);
                }

                if (!string.IsNullOrEmpty(ret as string))
                {
                    return ret as string;
                }
            }
            catch
            {

            }

            return string.Empty;
        }
    }
}
#endif