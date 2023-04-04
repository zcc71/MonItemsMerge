using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Yun.Common;

namespace MonItemsMerge
{
    public class Config
    {
        public string Path { get; set; }

        public Config(string path)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                return;
            this.Path = path;

            var config = dir.GetFiles("config.xml");
            var first = config.FirstOrDefault();
            if (first == null)
            {
                Save();
                config = dir.GetFiles("config.xml");
                first = config.FirstOrDefault();
            }

            path = first.FullName;

            if (!System.IO.File.Exists(path))
                return;

            XElement xd = XDocument.Load(path).Element("root");
            sourcePath = xd.Element("source")?.Value;
            ImportPath = xd.Element("import")?.Value;
            targetPath = xd.Element("target")?.Value;
            showLog = ConvertClass.BooleanWrapper(xd.Element("showLog")?.Value);
            geeExport = ConvertClass.BooleanWrapper(xd.Element("geeExport")?.Value);
            xdExport = ConvertClass.BooleanWrapper(xd.Element("xdExport")?.Value);
            lastDate = ConvertClass.DateWrapper(xd.Element("lastDate")?.Value);
            ext = xd.Element("ext")?.Value;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(this.Path))
                return;

            var dir = new DirectoryInfo(this.Path);
            if (!dir.Exists)
                return;

            var path = this.Path + @"\config.xml";

            //if (!System.IO.File.Exists(path))
            //{
            //    System.IO.File.Create(path).Dispose();
            //}

            //实例化XDocument对象
            XDocument xdoc = new XDocument();

            //创建根节点
            XElement root = new XElement("root");
            root.Add(new XElement("import", ImportPath ?? string.Empty));
            root.Add(new XElement("source", sourcePath ?? string.Empty));
            root.Add(new XElement("target", targetPath ?? string.Empty));
            root.Add(new XElement("showLog", showLog));
            root.Add(new XElement("geeExport", geeExport));
            root.Add(new XElement("xdExport", xdExport));
            root.Add(new XElement("lastDate", lastDate.ToString("yyyy-MM-dd HH:mm:ss")));
            root.Add(new XElement("ext", ext ?? string.Empty));
            xdoc.Add(root);
            xdoc.Save(path);
        }

        public bool geeExport { get; set; }
        public bool xdExport { get; set; }
        public bool showLog { get; set; }
        public string sourcePath { get; set; }
        public string targetPath { get; set; }
        public string ext { get; set; }

        public string ImportPath { get; set; }
        public DateTime lastDate { get; set; }
    }
}