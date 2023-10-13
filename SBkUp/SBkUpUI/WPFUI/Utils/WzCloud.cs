using SBkUpUI.WPFUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SBkUpUI.WPFUI.Utils
{
    class WzCloud
    {
        public static string Save(string folder, Backup bk)
        {
            var path = Path.Combine(folder, Path.GetFileNameWithoutExtension(bk.Item.name) + ".wzcloud");
            XmlWriter writer = XmlWriter.Create(path);

            writer.WriteStartDocument();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteStartElement("mru-item");
            writer.WriteWhitespace(Environment.NewLine);

            writer.WriteStartElement("provider");
            writer.WriteAttributeString("spid", ((int)bk.Item.profile.Id).ToString());
            writer.WriteAttributeString("name", WinZipMethods.GetShortDescription(IntPtr.Zero, bk.Item.profile.Id));
            writer.WriteAttributeString("profile", bk.Item.profile.name);
            writer.WriteAttributeString("authId", bk.Item.profile.authId);
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteWhitespace(Environment.NewLine);

            writer.WriteStartElement("folder");
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteStartElement("name");
            writer.WriteCData(Util.GetFolderPath(bk.Item.path));
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteStartElement("id");
            writer.WriteCData(bk.Item.parentId);
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteWhitespace(Environment.NewLine);

            writer.WriteStartElement("file");
            writer.WriteAttributeString("size", bk.Item.length.ToString());
            writer.WriteAttributeString("mod-time", WinZipMethods.SYSTEMTIMEToDateTime(bk.Item.modified).ToString("yyyy/MM/dd HH:mm:ss.f"));
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteStartElement("name");
            writer.WriteCData(bk.Item.name);
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteStartElement("id");
            writer.WriteCData(bk.Item.itemId);
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);
            writer.WriteWhitespace(Environment.NewLine);

            writer.WriteStartElement("uri");
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);

            writer.WriteStartElement("timestamp");
            writer.WriteString(DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss.f"));
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);

            writer.WriteStartElement("state");
            writer.WriteString("Normal");
            writer.WriteEndElement();
            writer.WriteWhitespace(Environment.NewLine);

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();

            return path;
        }
    }
}
