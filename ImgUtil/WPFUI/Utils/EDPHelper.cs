using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace ImgUtil.WPFUI.Utils
{
    public static class EDPAPIHelper
    {
        #region 32 API

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetManagedEnterpriseId", ExactSpelling = true)]
        private static extern bool GetManagedEnterpriseId32(StringBuilder enterpriseId, int count);

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetEnterpriseIdByPath", ExactSpelling = true)]
        private static extern bool GetEnterpriseIdByPath32(string path, StringBuilder enterpriseId, int count);

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "DomainIsIdentityManaged", ExactSpelling = true)]
        private static extern bool DomainIsIdentityManaged32(string domain);

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "ProtectNewItem", ExactSpelling = true)]
        private static extern bool ProtectNewItem32(string filePath);

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "UnProtectItem", ExactSpelling = true)]
        private static extern bool UnProtectItem32(string filePath);

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetTempEnterpriseId", ExactSpelling = true)]
        private static extern bool SetTempEnterpriseId32(string tempEnterpriseId);

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "ApplyProcessUIPolicy", ExactSpelling = true)]
        private static extern bool ApplyProcessUIPolicy32(string enterpriseId);

        [DllImport("WzEDPHelper32.dll", CharSet = CharSet.Unicode, EntryPoint = "ClearProcessUIPolicy", ExactSpelling = true)]
        private static extern bool ClearProcessUIPolicy32();

        #endregion

        #region 64 API

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetManagedEnterpriseId", ExactSpelling = true)]
        private static extern bool GetManagedEnterpriseId64(StringBuilder enterpriseId, int count);

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetEnterpriseIdByPath", ExactSpelling = true)]
        public static extern bool GetEnterpriseIdByPath64(string path, StringBuilder enterpriseId, int count);

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "DomainIsIdentityManaged", ExactSpelling = true)]
        private static extern bool DomainIsIdentityManaged64(string domain);

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "ProtectNewItem", ExactSpelling = true)]
        private static extern bool ProtectNewItem64(string filePath);

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "UnProtectItem", ExactSpelling = true)]
        private static extern bool UnProtectItem64(string filePath);

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "SetTempEnterpriseId", ExactSpelling = true)]
        private static extern bool SetTempEnterpriseId64(string tempEnterpriseId);

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "ApplyProcessUIPolicy", ExactSpelling = true)]
        private static extern bool ApplyProcessUIPolicy64(string enterpriseId);

        [DllImport("WzEDPHelper64.dll", CharSet = CharSet.Unicode, EntryPoint = "ClearProcessUIPolicy", ExactSpelling = true)]
        private static extern bool ClearProcessUIPolicy64();

        #endregion

        public static List<string> EDPAllowedSharedLinkList = new List<string>();

        public static bool GetManagedEnterpriseId(StringBuilder enterpriseId, int count)
        {
            if (IntPtr.Size == 4)
            {
                return GetManagedEnterpriseId32(enterpriseId, count);
            }
            else
            {
                return GetManagedEnterpriseId64(enterpriseId, count);
            }
        }

        private static bool GetEnterpriseIdByPath(string path, StringBuilder enterpriseId, int count)
        {
            if (IntPtr.Size == 4)
            {
                return GetEnterpriseIdByPath32(path, enterpriseId, count);
            }
            else
            {
                return GetEnterpriseIdByPath64(path, enterpriseId, count);
            }
        }

        public static string GetEnterpriseId(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            try
            {
                StringBuilder stringBuilder = new StringBuilder(259);
                EDPAPIHelper.GetEnterpriseIdByPath(path, stringBuilder, stringBuilder.Capacity);
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetEnterpriseId()
        {
            StringBuilder enterpriseId = new StringBuilder(259);
            try
            {
                GetManagedEnterpriseId(enterpriseId, enterpriseId.Capacity);
            }
            catch
            {
                return string.Empty;
            }
            return enterpriseId.ToString();
        }

        public static bool IsProcessProtectedByEDP()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder(259);
                return GetManagedEnterpriseId(stringBuilder, 259);
            }
            catch
            {
                return false;
            }
        }

        //Always return false when WinZip is not in programs allowed list
        public static bool IsIdentityManaged(string strDomain)
        {
            try
            {
                if (IntPtr.Size == 4)
                {
                    return DomainIsIdentityManaged32(strDomain);
                }
                else
                {
                    return DomainIsIdentityManaged64(strDomain);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ProtectNewItem(string filePath)
        {
            try
            {
                if (IntPtr.Size == 4)
                {
                    return ProtectNewItem32(filePath);
                }
                else
                {
                    return ProtectNewItem64(filePath);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool UnProtectItem(string filePath)
        {
            try
            {
                if (IntPtr.Size == 4)
                {
                    return UnProtectItem32(filePath);
                }
                else
                {
                    return UnProtectItem64(filePath);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool SetTempEnterpriseId(string tempEnterpriseId)
        {
            try
            {
                if (IntPtr.Size == 4)
                {
                    return SetTempEnterpriseId32(tempEnterpriseId);
                }
                else
                {
                    return SetTempEnterpriseId64(tempEnterpriseId);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ApplyProcessUIPolicy(string enterpriseId)
        {
            try
            {
                if (IntPtr.Size == 4)
                {
                    return ApplyProcessUIPolicy32(enterpriseId);
                }
                else
                {
                    return ApplyProcessUIPolicy64(enterpriseId);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ClearProcessUIPolicy()
        {
            try
            {
                if (IntPtr.Size == 4)
                {
                    return ClearProcessUIPolicy32();
                }
                else
                {
                    return ClearProcessUIPolicy64();
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class EDPAutoRestoreTempEnterpriseID : System.IDisposable
    {
        bool isSetTempEnterpriseId = false;

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (isSetTempEnterpriseId)
            {
                EDPAPIHelper.SetTempEnterpriseId(null);
            }
        }

        ~EDPAutoRestoreTempEnterpriseID()
        {
            Dispose(false);
        }

        public EDPAutoRestoreTempEnterpriseID(string tempEnterpriseId)
        {
            isSetTempEnterpriseId = EDPAPIHelper.SetTempEnterpriseId(tempEnterpriseId);
        }
    }

    public static class EDPHelper
    {
        public static void FileCopy(string sourceFileName, string destFileName, bool overwrite = true)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
            SyncEnterpriseId(sourceFileName, destFileName);
        }

        public static void SyncEnterpriseId(string sourceFileName, string destFileName, int delayTime = 0)
        {
            if (EDPAPIHelper.IsProcessProtectedByEDP())
            {
                var sourceEnterpriseId = EDPAPIHelper.GetEnterpriseId(sourceFileName);
                if (sourceEnterpriseId != EDPAPIHelper.GetEnterpriseId())
                {
                    UnProtectItemDelay(destFileName, delayTime);
                }
            }
        }

        public static void UnProtectItemDelay(string destFileName, int delayTime = 0)
        {
            if (delayTime == 0)
            {
                EDPAPIHelper.UnProtectItem(destFileName);
            }
            else
            {
                var timer = new Timer((double)delayTime);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    EDPAPIHelper.UnProtectItem(destFileName);
                };
                timer.Start();
            }
        }
    }
}
