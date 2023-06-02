using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Yun.Common;

namespace MonItemsMerge
{
    public partial class ImportMonItems : Form
    {
        public ImportMonItems()
        {
            InitializeComponent();
        }
        public bool saveLog = false;

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!XDRadioButton.Checked && !GeeRadioButton.Checked && !SJradioButton.Checked)
            {
                MessageBox.Show("3种转换格式请选择1个");
                return;
            }
            try
            {
                Config config = new Config(System.AppDomain.CurrentDomain.BaseDirectory);
                if ((DateTime.Now - config.lastDate).Minutes < 5)
                {
                    this.saveLog = false;
                }

                config.ImportPath = txt_Source.Text;
                config.targetPath = txt_Target.Text;
                config.lastDate = DateTime.Now;
                config.showLog = checkBox1.Checked;
                config.xdExport = XDRadioButton.Checked;
                config.geeExport = GeeRadioButton.Checked;
                config.Save();
            }
            catch (Exception)
            {
                MessageBox.Show("发生无法保存配置的意外");
                return;
            }
            var excel = new Yun.Common.Excel();
            if (!System.IO.File.Exists(txt_Source.Text))
            {
                MessageBox.Show("请选择要再次操作的文件");
                return;
            }
            IList<ImportItem> packageDt = null;
            var fileName = txt_Source.Text;
            fileName = System.IO.Path.GetFileName(fileName);
            var path = System.IO.Path.GetTempPath() + fileName;
            System.IO.File.Copy(txt_Source.Text, path, true);
            if (System.IO.Path.GetExtension(path) == ".js")
            {
                using (var st = new System.IO.StreamReader(path))
                {
                    try
                    {
                        packageDt = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ImportItem>>(st.ReadToEnd());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("load info error!ex:" + ex.Message);
                        return;
                    }
                }
            }
            else
            {
                try
                {
                    var ds = excel.ExcelToDataTables(path);
                    packageDt = new List<ImportItem>();

                    foreach (var dt in ds.Tables.Cast<DataTable>())
                    {
                        Dictionary<string, string> columnName = new Dictionary<string, string>() {
                            { "monPackage","怪物包"},
                            { "mdetail","怪物名称"},
                            { "mprate","怪物爆率"},
                            { "itemPackage","物品包"},
                            { "idetail","物品名称"},
                            { "iprate","物品爆率"},
                            { "random","是否新爆率"},
                            { "monitemPack","爆率怪物包"},
                            { "items","爆率物品包"},
                            { "mirate","初始爆率"}
                        };
                        //筛选目标列
                        var checkList = columnName.SelectMany(s => new string[] { s.Key, s.Value }).Where(s => !string.IsNullOrEmpty(s));
                        if (!dt.Columns.Cast<DataColumn>().Where(s => checkList.Contains(s.ColumnName)).Any())
                        {
                            continue;
                        }

                        //封装获取行数据的方法
                        Func<Dictionary<string, string>, DataRow, string, object> getRowValue = (col, row, key) =>
                         {

                             if (row.Table.Columns.Contains(key))
                             {
                                 return row[key];
                             }
                             else if (row.Table.Columns.Contains(col[key]))
                             {
                                 return row[col[key]];
                             }
                             else
                             {
                                 return null;
                             }
                         };

                        //按行读取掉落表数据
                        packageDt = packageDt.Concat(dt.Rows.Cast<DataRow>().Select((s, i) =>
                            new ImportItem()
                            {
                                rowIndex = i,
                                monPackage = ConvertClass.StringWrapper(getRowValue(columnName, s, "monPackage")).Trim(),
                                mdetail = ConvertClass.StringWrapper(getRowValue(columnName, s, "mdetail")).Trim(),
                                mprate = ConvertClass.StringWrapper(getRowValue(columnName, s, "mprate")).Trim(),
                                itemPackage = ConvertClass.StringWrapper(getRowValue(columnName, s, "itemPackage")).Trim(),
                                idetail = ConvertClass.StringWrapper(getRowValue(columnName, s, "idetail")).Trim(),
                                iprate = ConvertClass.StringWrapper(getRowValue(columnName, s, "iprate")).Trim(),
                                random = ConvertClass.IntegerWrapper(getRowValue(columnName, s, "random")),
                                monitemPack = ConvertClass.StringWrapper(getRowValue(columnName, s, "monitemPack")).Trim(),
                                items = ConvertClass.StringWrapper(getRowValue(columnName, s, "items")).Trim(),
                                mirate = ConvertClass.StringWrapper(getRowValue(columnName, s, "mirate")).Trim()
                            })).ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("load info error!ex:" + ex.Message);
                    return;
                }
            }
            if (packageDt == null)
            {
                MessageBox.Show("load info error!");
                return;
            }

            //怪物包
            var monpacke = new Dictionary<string, PackageItem>();
            //物品包
            var itempacke = new Dictionary<string, PackageRandomItem>();
            //爆率包
            var monitemPack = new Dictionary<string, PackageItem>();

            if (packageDt != null)
            {
                PackageItem lastmonpacke = null;
                PackageRandomItem lastitempacke = null;
                PackageItem lastmonitemPack = null;
                string 路径 = "";
                foreach (var item in packageDt)
                {
                    //爆率包解析
                    if (!(string.IsNullOrEmpty(item.monitemPack) && string.IsNullOrEmpty(item.items)))
                    {
                        //筛选爆率包名
                        if (!string.IsNullOrEmpty(item.monitemPack))
                        {
                            if (monitemPack.ContainsKey(item.monitemPack))
                            {
                                lastmonitemPack = monitemPack[item.monitemPack];
                            }
                            else
                            {
                                if (lastmonitemPack != null)
                                {
                                    monitemPack.Add(lastmonitemPack.name, lastmonitemPack);
                                    lastmonitemPack = null;
                                }
                                lastmonitemPack = new PackageItem
                                {
                                    name = item.monitemPack,
                                    rate = ConvertClass.DecimalWrapper(item.mirate, 1),
                                    detail = new List<PackageDetail>()
                                };
                            }
                        }
                        //筛选爆率包爆率
                        if (string.IsNullOrEmpty(item.monitemPack) && !string.IsNullOrEmpty(item.items) && lastmonitemPack != null)
                        {
                            //默认爆率倍数
                            if (string.IsNullOrEmpty(item.mirate))
                            {
                                lastmonitemPack.detail.Add(new PackageDetail()
                                {
                                    name = item.items,
                                    rate = 1
                                });
                            }
                            //读取配置爆率
                            else
                            {
                                var tmps = item.mirate.Split(',').Select(s => new PackageDetail()
                                {
                                    name = item.items,
                                    rate = ConvertClass.DecimalWrapper(s)
                                }).Where(s => s.rate != 0);
                                if (tmps.Any())
                                {
                                    foreach (var t in tmps)
                                    {
                                        lastmonitemPack.detail.Add(t);
                                    }
                                }
                                else
                                {
                                    lastmonitemPack.detail.Add(new PackageDetail()
                                    {
                                        name = item.items,
                                        rate = 1
                                    });
                                }
                            }
                        }
                    }

                    //装备包解析
                    if (!(string.IsNullOrEmpty(item.itemPackage) && string.IsNullOrEmpty(item.idetail)))
                    {
                        if (!string.IsNullOrEmpty(item.itemPackage))
                        {
                            if (itempacke.ContainsKey(item.itemPackage))
                            {
                                lastitempacke = itempacke[item.itemPackage];
                            }
                            else
                            {
                                if (lastitempacke != null)
                                {
                                    itempacke.Add(lastitempacke.name, lastitempacke);
                                    lastitempacke = null;
                                }
                                lastitempacke = new PackageRandomItem
                                {
                                    name = item.itemPackage,
                                    rate = ConvertClass.DecimalWrapper(item.iprate, 1),
                                    random = item.random,
                                    detail = new List<PackageDetail>()
                                };
                            }

                        }
                        if (string.IsNullOrEmpty(item.itemPackage) && !string.IsNullOrEmpty(item.idetail) && lastitempacke != null)
                        {
                            if (string.IsNullOrEmpty(item.iprate))
                            {
                                lastitempacke.detail.Add(new PackageDetail()
                                {
                                    name = item.idetail,
                                    rate = 1
                                });
                            }
                            else
                            {
                                var tmps = item.iprate.Split(',').Select(s => new PackageDetail()
                                {
                                    name = item.idetail,
                                    rate = ConvertClass.DecimalWrapper(s)
                                }).Where(s => s.rate != 0);
                                if (tmps.Any())
                                {
                                    foreach (var t in tmps)
                                    {
                                        lastitempacke.detail.Add(t);
                                    }
                                }
                                else
                                {
                                    lastitempacke.detail.Add(new PackageDetail()
                                    {
                                        name = item.idetail,
                                        rate = 1
                                    });
                                }
                            }
                        }
                    }

                    //怪物包解析
                    if (!(string.IsNullOrEmpty(item.monPackage) && string.IsNullOrEmpty(item.mdetail)))
                    {
                        if (!string.IsNullOrEmpty(item.monPackage))
                        {
                            if (monpacke.ContainsKey(item.monPackage))
                            {
                                lastmonpacke = monpacke[item.monPackage];
                            }
                            else
                            {
                                if (lastmonpacke != null)
                                {
                                    monpacke.Add(lastmonpacke.name, lastmonpacke);
                                    lastmonpacke = null;
                                }
                                lastmonpacke = new PackageItem
                                {
                                    name = item.monPackage,
                                    rate = ConvertClass.DecimalWrapper(item.mprate, 1),
                                    detail = new List<PackageDetail>()
                                };
                            }
                        }
                        if (string.IsNullOrEmpty(item.monPackage) && !string.IsNullOrEmpty(item.mdetail) && lastmonpacke != null)
                        {

                            if (string.IsNullOrEmpty(item.mprate))
                            {
                                lastmonpacke.detail.Add(new PackageDetail()
                                {
                                    name = item.mdetail,
                                    rate = 1
                                });
                            }
                            else
                            {
                                var tmps = item.mprate.Split(',').Select(s => new PackageDetail()
                                {
                                    name = item.mdetail,
                                    rate = ConvertClass.DecimalWrapper(s)
                                }).Where(s => s.rate != 0);
                                if (tmps.Any())
                                {
                                    foreach (var t in tmps)
                                    {
                                        lastmonpacke.detail.Add(t);
                                    }
                                }
                                else
                                {
                                    lastmonpacke.detail.Add(new PackageDetail()
                                    {
                                        name = item.mdetail,
                                        rate = 1
                                    });
                                }
                            }
                        }
                    }
                }

                //添加到爆率包
                if (lastmonitemPack != null)
                {
                    if (!monitemPack.ContainsKey(lastmonitemPack.name))
                    {
                        monitemPack.Add(lastmonitemPack.name, lastmonitemPack);
                        lastmonitemPack = null;
                    }
                }

                //添加到物品包
                if (lastitempacke != null)
                {
                    if (!itempacke.ContainsKey(lastitempacke.name))
                    {
                        itempacke.Add(lastitempacke.name, lastitempacke);
                        lastitempacke = null;
                    }
                }

                //添加到怪物包
                if (lastmonpacke != null)
                {
                    if (!monpacke.ContainsKey(lastmonpacke.name))
                    {
                        monpacke.Add(lastmonpacke.name, lastmonpacke);
                        lastmonpacke = null;
                    }
                }

                //实例化写入文件类
                var write = new WriteFile(this.txt_Target.Text);
                //处理数据包
                Action<int> act2 = (i) =>
                {
                    this.progressBar1.Value = i;
                    this.Text = $"格式化数量 {i}%";
                };
                progressBar1.Maximum = monitemPack.Count();
                var paks = AdaptPackage(monpacke, itempacke, monitemPack, act2);

              

                //开始写爆率文件
                Action<int> act = (i) =>
                {
                    this.progressBar1.Value = i;
                    this.Text = "导出数量 " + i.ToString();
                };
                progressBar1.Maximum = paks.Count();

                if (paks != null && paks.Any() && !checkBox2.Checked)
                {
                    try
                    {
                        write.Push(paks, checkBox1.Checked, XDRadioButton.Checked, GeeRadioButton.Checked,SJradioButton.Checked ,(i) => this.Invoke(act, i));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        //source装备包项目 rate根物品包全局掉率
        private IEnumerable<FilePackageItem> getFilePackageItem(string str, PackageRandomItem source, decimal rate, int level)
        {
            //;10 0.9 0.8 0.7 0.6 0.55
            foreach (var item in source.detail)
            {
                //递归处理嵌套得物品包
                if (item is PackageRandomItem)
                {
                    var stdPak = item as PackageRandomItem;
                    //source.rate 嵌套物品包原始掉率 - 根掉率 * 嵌套条目掉率作为下次嵌套的根包掉率
                    var items = getFilePackageItem(str + ";--> itemPackage:" + item.name + " rate:" + item.rate, stdPak, rate * source.rate, level++)
                        .Select(s => s)
                        .ToList();
                    FilePackageItem target = null;
                    //随机分组掉率包
                    if (stdPak.random != 0)
                    {
                        target = new FilePackageItem()
                        {
                            name = item.name,
                            random = stdPak.random,
                            rate = rate * item.rate * source.rate, //根物品包掉率 * 嵌套物品包条目掉率 * 嵌套物品包原始掉率
                            detail = new List<PackageDetail>()
                        }.addDesc(str + "-->rate : " + item.rate);
                    }
                    foreach (var p in items)
                    {
                        if (target == null)
                        {
                            yield return p;
                        }
                        else
                        {
                            target.detail.Add(p);
                        }
                    }
                    if (target != null)
                    {
                        yield return target;
                    }
                }
                else
                {
                    yield return new FilePackageItem()
                    {
                        name = item.name,
                        random = 0,
                        rate = rate * item.rate * source.rate,
                        detail = new List<PackageDetail>()
                    }.addDesc(str + "-->rate : " + item.rate);
                }
            }
        }

        //解析怪物包对应的掉落信息
        private IEnumerable<FilePackage> getFileMonsterItem(string str, PackageItem monsterPak, decimal rate, int level)
        {
            var monPak = monsterPak;
            if (monPak == null)
            {
                yield return null;
            }
            if (monPak.detail == null || !monPak.detail.Any())
            {
                yield return null;
            }
            foreach (var item in monPak.detail)
            {
                if (item is PackageItem)
                {
                    var items = getFileMonsterItem(str + ";-->monpak:" + item.name + " rate:" + item.rate, item as PackageItem, rate * item.rate, level++);
                    foreach (var i in items)
                    {
                        yield return new FilePackage() { name = i.name, rate = rate * i.rate, items = new List<FilePackageItem>() }.addDesc(str + ";-->monpak:" + i.name + " rate:" + i.rate);
                    }
                }
                else
                {
                    yield return new FilePackage() { name = item.name, rate = rate * item.rate, items = new List<FilePackageItem>() }.addDesc(str + ";-->monpak:" + item.name + " rate:" + item.rate);
                }
            }
        }

        private Dictionary<string, List<FilePackage>> AdaptPackage(
            Dictionary<string, PackageItem> monsterPak,         //怪物包
            Dictionary<string, PackageRandomItem> stdItemsPak,  //物品包
            Dictionary<string, PackageItem> monitemsPak,        //爆率包
            Action<int> act)
        {
            //实例化写入文件类
            var write = new WriteFile(this.txt_Target.Text);
            Dictionary<string, List<FilePackage>> response = new Dictionary<string, List<FilePackage>>();
            Dictionary<string, List<FilePackage>> 临时response = new Dictionary<string, List<FilePackage>>();
            //解析怪物包/物品包
            AdaptPak(monsterPak);
            AdaptPak(stdItemsPak);
            var page = 0;
            foreach (var item in monitemsPak)
            {
                //此爆率包的所有掉落信息
                var monFilePaks = new List<FilePackageItem>();
                //遍历爆率包-处理每个包的所有掉落物品信息
                //包内物品包
                foreach (var mid in item.Value.detail)
                {
                    //处理物品包得嵌套
                    if (stdItemsPak.ContainsKey(mid.name))
                    {
                        //取出物品包
                        var stdPak = stdItemsPak[mid.name];
                        FilePackageItem target = null;
                        //随机组物品包
                        if (stdPak.random != 0)
                        {
                            target = new FilePackageItem()
                            {
                                name = stdPak.name,
                                random = stdPak.random,
                                rate = stdPak.rate * mid.rate,
                                detail = new List<PackageDetail>()
                            }.addDesc(";--> itemPackage:" + mid.name + " rate: " + mid.rate + "-- >rate : " + stdPak.rate);
                        }
                        //获取包内物品
                        var items = getFilePackageItem("; --> itemPackage:" + mid.name + " rate:" + mid.rate, stdPak, mid.rate, 0);

                        foreach (var i in items)
                        {
                            if (target != null)
                            {
                                target.detail.Add(i);
                            }
                            else
                            {
                                monFilePaks.Add(i);
                            }
                        }
                        if (target != null)
                        {
                            monFilePaks.Add(target);
                        }
                    }
                    //添加单独得物品掉落
                    else
                    {
                        monFilePaks.Add(new FilePackageItem()
                        {
                            name = mid.name,
                            random = 0,
                            rate = mid.rate
                        });
                    }
                }
                //处理怪物包内怪物指定爆率信息
                List<FilePackage> files = new List<FilePackage>();
                //如果怪物包存在则解析怪物包对应的掉落信息
                if (monsterPak.ContainsKey(item.Key))
                {
                    var monster = getFileMonsterItem(";-->root:" + item.Value.rate + " ;-->monpak:" + monsterPak[item.Key].name + " rate:" + monsterPak[item.Key].rate, monsterPak[item.Key], item.Value.rate * monsterPak[item.Key].rate, 0)
                        .Where(s => s != null)
                        .ToList();
                    monster.ForEach(s => s.items = monFilePaks);
                    files.AddRange(monster);
                }
                //否则视爆率包的怪物包名为怪物名直接配置掉落物品
                else
                {
                    files.Add(new FilePackage()
                    {
                        name = item.Key,
                        rate = item.Value.rate,
                        items = monFilePaks
                    }.addDesc(";-->root:" + item.Value.rate));
                }
                if (files.Any())
                {
                    //整合同名怪物到同一个配置组下
                    foreach (var g in files.GroupBy(s => s.name, s => s))
                    {
                        if (response.ContainsKey(g.Key))
                        {
                            response[g.Key].AddRange(g);
                        }
                        else
                        {
                            response.Add(g.Key, g.ToList());
                        }
                        if (临时response.ContainsKey(g.Key))
                        {
                            临时response[g.Key].AddRange(g);
                        }
                        else
                        {
                            临时response.Add(g.Key, g.ToList());
                        }
                    }
                }
                //----每个怪物包单独保存
                if (checkBox2.Checked)
                {
                    write.PushAsync(临时response, checkBox1.Checked, XDRadioButton.Checked, GeeRadioButton.Checked, SJradioButton.Checked, (i) => this.Invoke(act, i), monsterPak.ContainsKey(item.Key) ? monsterPak[item.Key].name : "");
                    临时response.Clear();
                }
                act(page++);
            }

            return response;
        }

        //解析物品包
        private void AdaptPak(Dictionary<string, PackageRandomItem> paks)
        {
            //物品包
            foreach (var pak in paks)
            {
                var insertDetails = new List<PackageDetail>();
                var delDetails = new List<string>();
                //包内物品
                foreach (var d in pak.Value.detail)
                {
                    //物品名
                    var name = d.name;
                    if (name.StartsWith("$"))
                    {
                        name = name.Substring(1);
                    }
                    //处理嵌套得物品包
                    if (paks.ContainsKey(name))
                    {
                        insertDetails.Add(new PackageRandomItem()
                        {
                            rate = paks[name].rate * d.rate,
                            name = paks[name].name,
                            detail = paks[name].detail,
                            random = paks[name].random
                        });
                        delDetails.Add(d.name);
                    }
                    else if (!name.Equals(d.name))
                    {
                        //移除连接
                        delDetails.Add(d.name);
                    }
                }
                //剔除删除得掉率/增加引用包
                if (insertDetails.Any() || delDetails.Any())
                {
                    pak.Value.detail = pak.Value.detail.Where(s => !delDetails.Contains(s.name)).Concat(insertDetails).ToList();
                }
            }
        }

        //解析怪物包
        private void AdaptPak(Dictionary<string, PackageItem> paks)
        {
            //怪物包
            foreach (var pak in paks)
            {
                var insertDetails = new List<PackageDetail>();
                var delDetails = new List<string>();
                //包内怪物
                foreach (var d in pak.Value.detail)
                {
                    var name = d.name;
                    if (name.StartsWith("$"))
                    {
                        name = name.Substring(1);
                    }
                    //处理嵌套得怪物包
                    if (paks.ContainsKey(name))
                    {
                        insertDetails.Add(new PackageItem()
                        {
                            rate = paks[name].rate * d.rate,
                            name = paks[name].name,
                            detail = paks[name].detail
                        });
                        delDetails.Add(d.name);
                    }
                    else if (!name.Equals(d.name))
                    {
                        //移除连接
                        delDetails.Add(d.name);
                    }
                }
                if (insertDetails.Any() || delDetails.Any())
                {
                    pak.Value.detail = pak.Value.detail.Where(s => !delDetails.Contains(s.name)).Concat(insertDetails).ToList();
                }
            }
        }

        private void btnOpenTarget_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                this.txt_Target.Text = foldPath;
            }
        }

        private void btnOpenSource_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            openFileDialog.Filter = "xlsx(excel 2007)|*.xlsx|xls(execl 2003)|*.xls|json(import json)|*.js";

            openFileDialog.RestoreDirectory = true;

            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void ImportMonItems_Load(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            ReaderConfig();
            OpenFile(txt_Source.Text);
        }

        private void OpenFile(string file)
        {
            txt_Source.Text = file;
            if (!string.IsNullOrEmpty(txt_Source.Text))
            {
                btnStart.Enabled = true;
            }
        }

        private void ReaderConfig()
        {
            try
            {
                Config config = new Config(System.AppDomain.CurrentDomain.BaseDirectory);
                this.txt_Source.Text = config.ImportPath ?? string.Empty;
                this.txt_Target.Text = config.targetPath ?? string.Empty;
                this.checkBox1.Checked = config.showLog;
                this.XDRadioButton.Checked = config.xdExport;
                this.GeeRadioButton.Checked = config.geeExport;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int ReaderFiles(string text, int v)
        {
            var ds = new Excel().ExcelToDataTable(txt_Source.Text, "baseInfo");
            var count = ds.Rows.Cast<DataRow>().Select(s => ConvertClass.StringWrapper(s["monsters"])).Where(s => !string.IsNullOrEmpty(s))?.Count();

            return count ?? 0;
        }

    }
}