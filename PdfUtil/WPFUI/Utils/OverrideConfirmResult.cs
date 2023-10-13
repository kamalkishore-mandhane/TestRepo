using PdfUtil.WPFUI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PdfUtil
{
    class OverrideConfirmResult
    {
        public enum Choice
        {
            None = 0,
            Rename = 1,     //DialogResult.OK
            Cancel = 2,     //DialogResult.Cancel
            Override = 6,   //DialogResult.Yes
            Skip = 7,       //DialogResult.No
        }

        public readonly Choice choice;
        public readonly bool applyToAll;

        public OverrideConfirmResult(DialogResult result, bool applyToAll)
        {
            this.applyToAll = applyToAll;
            this.choice = (Choice)result;
        }

        public OverrideConfirmResult(Choice choice, bool applyToAll)
        {
            this.applyToAll = applyToAll;
            this.choice = choice;
        }

        public static string RenameFileName(string filename, WzSvcProviderIDs pickerCreatorId)
        {
            string newFileName;
            string ext = Path.GetExtension(filename);
            int prevIndex = 1;
            string fileNameBody = filename.Substring(0, filename.Length - ext.Length);
            Match match = Regex.Match(fileNameBody, "(.*) \\((\\d+)\\)");

            if (match.Value.Length == fileNameBody.Length && match.Groups.Count > 2)
            {
                fileNameBody = match.Groups[1].Value;
                prevIndex = int.Parse(match.Groups[2].Value);
            }

            string uniqueId = string.Format(@"{0:x4}", FileOperation.LOWORD(DateTime.Now.Ticks) + prevIndex);

            newFileName = string.Format("{0} ({1}-{2}){3}", fileNameBody, uniqueId, prevIndex + 1, ext);

            // https://docs.aws.amazon.com/AmazonS3/latest/dev/BucketRestrictions.html#bucketnamingrules
            // S3Compatible: Bucket names can consist only of lowercase letters, numbers, dots (.), and hyphens (-).
            if (pickerCreatorId == WzSvcProviderIDs.SPID_CLOUD_S3COMPATIBLE)
            {
                fileNameBody = filename.Substring(0, filename.Length - ext.Length);
                match = Regex.Match(fileNameBody, "(.*) \\- (\\d+)");

                if (match.Value.Length == fileNameBody.Length && match.Groups.Count > 2)
                {
                    fileNameBody = match.Groups[1].Value;
                    prevIndex = int.Parse(match.Groups[2].Value);
                }
                newFileName = string.Format("{0} - {1}{2}", fileNameBody, prevIndex + 1, ext);

            }
            return newFileName;
        }
    }
}
