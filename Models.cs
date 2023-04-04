using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonItemsMerge
{
    internal class FilePackage:PackageDetail
    {
        private StringBuilder desc { get; set; } = new StringBuilder();
        public string OutPutDesc { get { return desc.ToString(); } }
        public FilePackage addDesc(string str)
        {
            desc.Append(str);
            return this;
        }
        public override string ToString()
        {
            return desc.ToString();
        }
        public List<FilePackageItem> items { get; set; }

    }

    internal class FilePackageItem: PackageRandomItem
    {
        private StringBuilder desc { get; set; }=new StringBuilder();
        public string OutPutDesc { get { return desc.ToString(); } }
        public FilePackageItem addDesc(string str)
        {
            desc.Append(str);
            return this;
        }
        public override string ToString()
        {
            return desc.ToString();
        }
    }

    internal class PackageItem : PackageDetail
    {
        public IList<PackageDetail> detail { get; set; }
    }

    internal class PackageRandomItem : PackageItem
    {
        public int random { get; set; }
    }

    internal class PackageDetail
    {
        public string name { get; set; }
        public decimal rate { get; set; }
    }

    internal class ImportItem
    {
        public int rowIndex { get; set; }
        public string monPackage { get; set; }
        public string mdetail { get; set; }
        public string mprate { get; set; }
        public string itemPackage { get; set; }
        public string idetail { get; set; }
        public string iprate { get; set; }
        public int random { get; set; }
        public string monitemPack { get; set; }
        public string items { get; set; }
        public string mirate { get; set; }
    }
}
