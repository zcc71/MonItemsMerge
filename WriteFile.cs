using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yun.Common;
using static MonItemsMerge.ImportMonItems;

namespace MonItemsMerge
{
    public class WriteFile
    {
        public string sourcePath { get; set; }
        public string targetPath { get; set; }
        public string ext { get; set; }
        public bool xdChecked { get; set; }
        public bool geeChecked { get; set; }
        public bool SjChecked { get; set; }
        public WriteFile(string sourcePath, string targetPath, string ext)
        {
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
            this.ext = ext;
        }

        public WriteFile(string targetPath)
        {
            this.targetPath = targetPath;
        }

        private Dictionary<string, MonItem> OutFiles { get; set; }
        private Dictionary<string, LinkFile> LinkToFiles { get; set; }

        internal void Push(StringBuilder sb, string item, IEnumerable<PackageItem> monpacke, IEnumerable<PackageRandomItem> itempacke, IEnumerable<PackageItem> monitempack)
        {
            var monp = monpacke.Where(s => s.detail.Where(p => p.name == item).Any());
            foreach (var packs in monitempack)
            {
                if (packs.name == item)//如果是怪物本身
                {
                    PushPackMon(sb, packs, 1, itempacke, false);
                    continue;
                }
                var mpitems = monp.Where(p => p.name == packs.name);
                foreach (var p in mpitems)
                {
                    var rate = p.detail.Where(s => s.name == item).Select(s => s.rate).FirstOrDefault();
                    if (rate == 0)
                        continue;
                    PushPackMon(sb, new PackageItem()
                    {
                        name = p.name,
                        rate = p.rate,
                        detail = packs.detail
                    }, 1 * packs.rate * rate, itempacke, false);
                }
            }
        }

        internal void Push(string[] mons, IEnumerable<PackageItem> monpacke, IEnumerable<PackageRandomItem> itempacke, IEnumerable<PackageItem> monitempack, Action<int> action)
        {
            Queue<string> que = new Queue<string>(mons);
            for (int index = 0; index < mons.Length; index++)
            {
                Thread newThread = new Thread(() =>
                {
                    var sb = new StringBuilder();
                    var mon = que.Dequeue();
                    Push(sb, mon, monpacke, itempacke, monitempack);
                    if (sb.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            if (!System.IO.Directory.Exists(targetPath))
                            {
                                System.IO.Directory.CreateDirectory(targetPath);
                            }
                            using (var s = new StreamWriter(System.IO.Path.Combine(this.targetPath, mon + ".txt"), false, Encoding.UTF8))
                            {
                                s.Write(sb);
                            }
                        }
                    }
                    action(index);
                });
                newThread.Start();
            };
        }

        //从数据生成爆率文件
        internal void Push(Dictionary<string, List<FilePackage>> paks, bool showInfo, bool xdChecked, bool geeChecked,bool sjChecked, Action<int> action)
        {
            this.xdChecked = xdChecked;
            this.geeChecked = geeChecked;
            this.SjChecked = sjChecked;

            var que = new Queue<KeyValuePair<string, List<FilePackage>>>(paks);
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(10, 10);
            for (int index = 0; index < paks.Count(); index++)
            {

                WaitCallback act = (o) =>
                {
                    var sb = new StringBuilder();
                    //取出单个怪掉落信息
                    var mon = que.Dequeue();
                    //解析掉落字符串
                    foreach (var item in mon.Value)
                    {
                        PushMonItemPak(sb, item, showInfo);
                    }
                    //将字符串写入文件
                    if (sb.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            if (!Directory.Exists(targetPath ))
                            {
                                Directory.CreateDirectory(targetPath);
                            }
                            using (var s = new StreamWriter(Path.Combine(this.targetPath , mon.Key + ".txt"), false, Encoding.UTF8))
                            {
                                s.Write(sb);
                            }
                        }
                    }
                    action(index);
                };
                ThreadPool.UnsafeQueueUserWorkItem(act, null);
            };
        }

        internal async Task PushAsync(Dictionary<string, List<FilePackage>> paks, bool showInfo, bool xdChecked, bool geeChecked, bool sjChecked, Action<int> action, string 路径)
        {
            this.xdChecked = xdChecked; this.geeChecked = geeChecked; this.SjChecked = sjChecked;

            var que = new Queue<KeyValuePair<string, List<FilePackage>>>(paks);

            for (int index = 0; index < paks.Count(); index++)
            {
                var sb = new StringBuilder();
                //取出单个怪掉落信息
                var mon = que.Dequeue();
                //解析掉落字符串
                foreach (var item in mon.Value)
                {
                    PushMonItemPak(sb, item, showInfo);
                }
                //将字符串写入文件 mon.Value[0].OutPutDesc ;-->root:1 ;-->monpak:怪物包-1级 rate:1;-->monpak:稻草人 rate:1
                if (sb.Length > 0)
                {
                    var filePath = $"{this.targetPath}/{路径}/";
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        using (var s = new StreamWriter(Path.Combine(filePath, mon.Key + ".txt"), false, Encoding.UTF8))
                        {
                            await s.WriteAsync(sb.ToString());
                        }
                    }
                }
                action(index);
            };
        }

        //解析掉落字符串
        private void PushMonItemPak(StringBuilder sb, FilePackage pak, bool showInfo)
        {
            //无数据
            if (pak.items == null || !pak.items.Any())
            {
                sb.AppendLine(";暂无掉落配置");
                return;
            }
            sb.AppendLine(";------------怪物掉落配置自动生成请勿手动更改------------");
            sb.AppendLine();
            //日志信息
            if (showInfo && !string.IsNullOrEmpty(pak.OutPutDesc.ToString()))
            {
                sb.AppendLine(";" + pak.OutPutDesc);
                sb.AppendLine();
            }
            //掉落信息
            foreach (var item in pak.items)
            {
                //日志信息
                if (showInfo && !string.IsNullOrEmpty(item.OutPutDesc.ToString()))
                {
                    sb.AppendLine(";" + item.OutPutDesc);
                }
                //随机分组掉率
                if (item.detail != null && item.detail.Any())
                {
                    WriteItemPak(sb, item, pak);
                }
                //独立物品掉率
                else
                {
                    WriteItem(sb, item, 1, pak, false);
                }
                sb.AppendLine();
            }
        }

        //独立物品掉率
        private void WriteItem(StringBuilder sb, PackageDetail detail, int level, FilePackage pak, bool rateOne)
        {
            if (detail.name.StartsWith("$"))
            {
                throw new Exception("line not adapt");
            }

            var rate = rateOne ? 1 : GetRate(detail.rate * pak.rate);
            if (rate < 1)
                rate = 1;

            //心动引擎
            if (xdChecked || SjChecked)
            {
                var ItemInfo = ParseItemDroupName(detail.name);
                sb.AppendLine($"{(level <= 1 ? "" : " ")}1/{rate} {ItemInfo.Key} {ItemInfo.Value}");
            }
            //GEE引擎
            else if (geeChecked )
            {
                sb.AppendLine($"{(level <= 1 ? "" : " ")}1/{rate} {detail.name}");
            }
        }

        //从物品名解析数量
        private KeyValuePair<string, string> ParseItemDroupName(string messName)
        {
            var ItemInfo = messName.Split(',');
            var ItemName = ItemInfo[0];
            var ValueInfo = string.Empty;
            if (ItemInfo.Length > 1)
            {
                if (ItemInfo[1].ToUpper().Equals("Q"))
                    ValueInfo = "Q";
                else
                    ValueInfo = Math.Max(ConvertClass.IntegerWrapper(ItemInfo[1]), 1).ToString();
            }
            else
            {
                ValueInfo = "1";
            }
            return new KeyValuePair<string, string>(ItemName, ValueInfo);
        }

        //随机分组掉率
        private void WriteItemPak(StringBuilder sb, FilePackageItem detail, FilePackage pak)
        {
            var rate = GetRate(detail.rate * pak.rate);
            if (rate < 1)
                rate = 1;

            //心动引擎
            if (xdChecked)
            {
                if (detail.random != 0)
                {
                    var Names = detail.detail.Select(item => ParseItemDroupName(item.name).Key).Where(name => !string.IsNullOrEmpty(name));
                    var NameConnect = string.Join("|", Names.ToArray());
                    sb.AppendLine($"1/{rate} {NameConnect} {detail.random}");
                }
                else
                {
                    foreach (var item in detail.detail)
                    {
                        WriteItem(sb, item, 1, pak, detail.random != 0);
                    }
                }
            }
            //GEE引擎
            else if (geeChecked)
            {
                if (detail.random == 1)
                {
                    sb.AppendLine($"#CHILD 1/{rate} RANDOM");
                }
                else if (detail.random == 2)
                {
                    sb.AppendLine($"#CHILD 1/{rate}");
                }
                if (detail.random != 0)
                {
                    sb.AppendLine("(");
                }
                foreach (var item in detail.detail)
                {
                    WriteItem(sb, item, 2, pak, detail.random != 0);
                }
                if (detail.random != 0)
                {
                    sb.AppendLine(")");
                }
            }
            //水晶引擎
            else if (SjChecked)
            {
                if (detail.random == 1) //随机爆1件
                {
                    sb.AppendLine($"1/{rate} GROUP*"); 
                }
                else if (detail.random > 1)//全爆
                {
                    sb.AppendLine($"1/{rate} GROUP");
                }
                if (detail.random != 0)
                {
                    sb.AppendLine("{");
                }
                foreach (var item in detail.detail)
                {
                    WriteItem(sb, item, 2, pak, detail.random != 0);
                }
                if (detail.random != 0)
                {
                    sb.AppendLine("}");
                }
            }
        }

        internal void PushPackMon(StringBuilder sb, PackageItem monItem, decimal rate, IEnumerable<PackageRandomItem> itempacke, bool isPackage)
        {
            sb.AppendLine($";monpack => " + monItem.name);
            rate = rate * monItem.rate;
            foreach (var item in monItem.detail)
            {
                var itempacks = itempacke.Where(s => s.name == item.name);
                if (itempacks.Any())//pack
                {
                    foreach (var p in itempacks)
                    {
                        PushPackItem(sb, p, rate * item.rate);
                    }
                }
                else
                {
                    PushPackItem(sb, item, rate);
                }
            }
        }

        private int GetRate(decimal rate)
        {
            rate = Math.Ceiling(rate);
            if (rate < 0)
                return 1;
            return (int)rate;
        }

        internal void PushPackItem(StringBuilder sb, PackageRandomItem package, decimal rate)
        {
            rate = rate * package.rate;
            sb.AppendLine($";itempack => " + package.name);
            if (package.random == 1)
            {
                sb.AppendLine($"#CHILD 1/{GetRate(rate)} RANDOM");
            }
            else if (package.random == 2)
            {
                sb.AppendLine($"#CHILD 1/{GetRate(rate)}");
            }
            if (package.random != 0)
            {
                sb.AppendLine("(");
            }
            foreach (var item in package.detail)
            {
                PushPackItem(sb, item, package.random == 1 ? -1 : package.random == 2 ? 1 : rate);
            }
            if (package.random != 0)
            {
                sb.AppendLine(")");
            }
        }

        internal void PushPackItem(StringBuilder sb, PackageDetail package, decimal rate)
        {
            rate = rate * package.rate;
            sb.AppendLine($"1/{GetRate(rate)} {package.name}");
        }

        internal void PushPackMonEx(StringBuilder sb, PackageItem monItem, decimal rate, IEnumerable<PackageRandomItem> itempacke)
        {
            foreach (var d in monItem.detail ?? new PackageDetail[0])
            {
                if (itempacke.Where(s => s.name == d.name).Any())//package
                {
                    foreach (var item in itempacke.Where(s => s.name == d.name))
                    {
                        if (item.random != 0)
                        {
                            rate = GetRate(monItem.rate * d.rate * rate * item.rate);
                            sb.AppendLine($";" + item.name);
                            sb.AppendLine($"#CHILD 1/{rate} RANDOM");
                            sb.AppendLine("(");
                            PushPackMonEx(sb, item, -1, itempacke);
                            sb.AppendLine(")");
                        }
                        else
                        {
                            sb.AppendLine($";" + item.name);
                            PushPackMonEx(sb, item, rate * monItem.rate * item.rate, itempacke);
                        }
                    }
                }
                else
                {
                    rate = GetRate(monItem.rate * d.rate * rate);
                    if (rate < 1)
                        rate = 1;
                    sb.AppendLine($"1/{rate} {d.name}");
                }
            }
        }

        public void Push(Action<int> action)
        {
            LinkToFiles = new Dictionary<string, LinkFile>();
            OutFiles = new Dictionary<string, MonItem>();
            var files = new List<string>();
            ReaderFiles(sourcePath, files);
            //try
            //{
            foreach (var item in files)
            {
                var items = GetMonItems(item);
                if (items != null)
                    OutFiles.Add(item, items);
            }
            var i = 1;
            foreach (var item in OutFiles)
            {
                var file = item.Key.Replace(this.sourcePath, this.targetPath);
                if (file == item.Key)
                {
                    System.Windows.Forms.MessageBox.Show("not move files");
                    return;
                }
                if (!System.IO.File.Exists(file))
                {
                    System.IO.File.Create(file).Dispose();
                }
                else
                {
                    System.IO.File.WriteAllText(file, "");
                }

                using (var s = new StreamWriter(file, false, Encoding.UTF8))
                {
                    foreach (var v in item.Value.Items)
                    {
                        v.Write(s, 1);
                    }
                    action(i);
                }
                i++;
            }
            // }
            //catch (Exception ex )
            //{
            //    System.Windows.Forms.MessageBox.Show(ex.Message);
            //}
        }

        private void ReaderFiles(string dir, List<string> sfiles)
        {
            if (Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir, "*." + ext);
                string[] dirs = Directory.GetDirectories(dir);
                foreach (var item in dirs)
                {
                    ReaderFiles(item, sfiles);
                }
                sfiles.AddRange(files);
            }
        }

        private MonItem GetMonItems(string info)
        {
            if (System.IO.File.Exists(info))
            {
                var monItems = new MonItem();
                monItems.Items = new List<Item>();
                var str = string.Empty;
                using (var s = new StreamReader(info, Encoding.Default))
                {
                    str = s.ReadToEnd();
                }
                var sp = str.Split(Environment.NewLine.ToCharArray()).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (sp.Any())
                {
                    foreach (var item in sp)
                    {
                        if (string.IsNullOrEmpty(item))
                            continue;

                        if (item.ToLower().IndexOf("#call") >= 0)
                        {
                            var i = GetLinkItem(info, item);
                            if (i == null)
                            {
                                System.Windows.Forms.MessageBox.Show("path:" + info + ",adapt error,str:" + item);
                                throw new Exception("path:" + info + ",adapt error,str:" + item);
                            }
                            monItems.Items.Add(i);
                        }
                        else
                        {
                            var i = GetItem(item);
                            if (i == null)
                            {
                                System.Windows.Forms.MessageBox.Show("path:" + info + ",adapt error,str:" + item);
                                throw new Exception("path:" + info + ",adapt error,str:" + item);
                            }
                            monItems.Items.Add(i);
                        }
                    }
                }
                return monItems.Items.Any() ? monItems : null;
            }
            return null;
        }

        private Item GetLinkItem(string path, string str)
        {
            str = str.Trim();
            string[] stringSeparators = new string[] { " ", "\t" };
            var sp = str.Split(stringSeparators, StringSplitOptions.None)?.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (sp.Length < 3)
            {
                return null;
            }
            if (sp[1].Length < 2)
            {
                return null;
            }
            path = System.IO.Path.Combine(new FileInfo(path).DirectoryName, sp[1].Substring(1, sp[1].Length - 2));
            var r = GetLinkItemByKey(path, sp[2]);

            if (r != null)
            {
                r = new LinkFile()
                {
                    file = r.file,
                    Items = r.Items,
                    label = r.label
                };
                if (sp.Length > 3)
                {
                    r.rate = ConvertDecimal(sp[3].StartsWith(".") ? ("0" + sp[3]) : sp[3]);
                }
                else
                    r.rate = 1;
            }
            else
            {
            }
            return r;
        }

        private LinkFile GetLinkItemByKey(string path, string key)
        {
            path = new FileInfo(path).FullName;
            var tkey = (path + "$&$" + key).ToLower();

            if (!LinkToFiles.ContainsKey(tkey))
            {
                InsertLinkItem(path);
            }
            if (LinkToFiles.ContainsKey(tkey))
            {
                return LinkToFiles[tkey];
            }

            return null;
        }

        private void InsertLinkItem(string path)
        {
            if (System.IO.File.Exists(path))
            {
                var str = string.Empty;
                using (var s = new StreamReader(path, Encoding.Default))
                {
                    str = s.ReadToEnd();
                }
                var sp = str.Split(Environment.NewLine.ToCharArray()).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
                LinkFile item = null;
                NewRateItem newitem = null;
                var f = false;
                var f1 = false;
                if (sp.Any())
                {
                    foreach (var s in sp)
                    {
                        if (string.IsNullOrEmpty(s))
                            continue;

                        if (s.IndexOf("[@") >= 0)
                        {
                            item = new LinkFile()
                            {
                                file = path,
                                label = s.Length < 4 ? string.Empty : s.Substring(1, s.Length - 2),
                                Items = new List<Item>()
                            };
                            continue;
                        }
                        if (s.IndexOf("{") >= 0)
                        {
                            f = true;
                            continue;
                        }
                        if (s.IndexOf("}") >= 0)
                        {
                            f = false;
                            if (item != null)
                            {
                                var tkey = (path + "$&$" + item.label).ToLower();
                                if (!LinkToFiles.ContainsKey(tkey))
                                    LinkToFiles.Add(tkey, item);
                                item = null;
                            }
                            continue;
                        }
                        if (s.IndexOf("(") >= 0)
                        {
                            f1 = true;
                            continue;
                        }
                        if (s.IndexOf(")") >= 0)
                        {
                            f1 = false;
                            if (newitem != null)
                            {
                                item.Items.Add(newitem);
                            }
                            newitem = null;
                            continue;
                        }
                        if (item == null || !f)
                        {
                            continue;
                        }
                        if (newitem != null && f1)
                        {
                            var i = GetItem(s);
                            if (i == null)
                            {
                                System.Windows.Forms.MessageBox.Show("path:" + path + ",adapt error,str:" + s);
                                throw new Exception("path:" + path + ",adapt error,str:" + s);
                            }
                            newitem.RateItems.Add((RateItem)i);
                            continue;
                        }

                        if (s.ToLower().IndexOf("#child") >= 0)
                        {
                            newitem = GetNewRateItemHeader(s);
                            if (newitem == null)
                            {
                                System.Windows.Forms.MessageBox.Show("path:" + path + ",adapt error,str:" + s);
                                throw new Exception("path:" + path + ",adapt error,str:" + s);
                            }
                        }
                        else
                        {
                            var i = GetItem(s);
                            if (i == null)
                            {
                                System.Windows.Forms.MessageBox.Show("path:" + path + ",adapt error,str:" + s);
                                throw new Exception("path:" + path + ",adapt error,str:" + s);
                            }
                            item.Items.Add(i);
                        }
                    }
                }
            }
        }

        private NewRateItem GetNewRateItemHeader(string str)
        {
            var sp = str.Trim().Split(' ').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (sp.Length < 2)
            {
                return null;
            }
            var le = sp[1].Split('/');
            var ret = new NewRateItem();
            ret.CanChild = !(str.ToLower().IndexOf("random") >= 0);
            ret.min = le.Length > 0 ? ConvertInt(le[0]) : 0;
            ret.max = le.Length > 0 ? ConvertInt(le[1]) : 0;
            ret.RateItems = new List<RateItem>();
            if (ret.min == 0 || ret.max == 0)
                return null;
            return ret;
        }

        private Item GetItem(string str)
        {
            str = str.Trim();
            string[] stringSeparators = new string[] { " ", "\t" };
            var sp = str.Split(stringSeparators, StringSplitOptions.None)?.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (sp.Length != 2)
            {
                return null;
            }
            var le = sp[0].Split('/');
            if (string.IsNullOrEmpty(sp[1]) || string.IsNullOrEmpty(le[0]) || string.IsNullOrEmpty(le[0]))
            {
                return null;
            }
            return new RateItem()
            {
                name = sp[1],
                min = ConvertInt(le[0]),
                max = ConvertInt(le[1])
            };
        }

        private decimal ConvertDecimal(object obj)
        {
            if (obj == null)
                return 0M;
            if (obj is decimal)
                return (decimal)obj;
            var t = 0M;
            if (!decimal.TryParse(obj.ToString(), out t))
            {
                return 0;
            }
            else
                return t;
        }

        private int ConvertInt(object obj)
        {
            if (obj == null)
                return 0;
            if (obj is int)
                return (int)obj;
            var t = 0M;
            if (!decimal.TryParse(obj.ToString(), out t))
            {
                return 0;
            }
            else
                return (int)t;
        }

    }

    public class MonItem
    {
        public IList<Item> Items { get; set; }
    }

    public interface Item
    {
        void Write(StreamWriter sw, decimal rate);
    }

    public class RateItem : Item
    {
        public int min { get; set; }
        public int max { get; set; }
        public string name { get; set; }

        public virtual void Write(StreamWriter sw, decimal rate = 1)
        {
            var tmax = max * rate / min * 10;
            if (tmax < 10)
            {
                tmax = 10;
            }
            sw.WriteLine($"10/{ Math.Ceiling(tmax)} {name}");
        }
    }

    public class LinkFile : Item
    {
        public string file { get; set; }
        public string label { get; set; }
        public decimal rate { get; set; }
        public IList<Item> Items { get; set; }

        public void Write(StreamWriter sw, decimal rate)
        {
            foreach (var item in Items)
            {
                item.Write(sw, this.rate);
            }
        }
    }

    public class NewRateItem : RateItem
    {
        public bool CanChild { get; set; }
        public IList<RateItem> RateItems { get; set; }

        public override void Write(StreamWriter sw, decimal rate)
        {
            if (CanChild)
            {
                sw.WriteLine($"#CHILD {min}/{max} {(CanChild ? "" : "Random")}");
                sw.WriteLine("(");
                foreach (var item in RateItems)
                {
                    item.Write(sw, rate);
                }
                sw.WriteLine(")");
            }
            else
            {
                var tmax = max * rate / min * 10;
                if (tmax < 10)
                {
                    tmax = 10;
                }
                sw.WriteLine($"#CHILD 10/{Math.Ceiling(tmax)} {(CanChild ? "" : "Random")}");
                sw.WriteLine("(");
                foreach (var item in RateItems)
                {
                    item.Write(sw, rate);
                }
                sw.WriteLine(")");
            }
        }
    }
}