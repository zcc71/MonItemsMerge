using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MonItemsMerge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            ReaderConfig();
            if (!string.IsNullOrEmpty(txt_Source.Text))
            {
                var count = ReaderFiles(txt_Source.Text, 0);
                this.progressBar1.Maximum = count;
                Text = "查询文件数量:" + count;
                btnStart.Enabled = true;
            }
        }

        private void OpenFile(string file)
        {
            var dir = new DirectoryInfo(file);
            if (dir.Exists)
            {
                txt_Source.Text = file;
                var count = ReaderFiles(file, 0);
                this.progressBar1.Maximum = count;
                Text = "查询文件数量:" + count;
                btnStart.Enabled = true;
            }
        }

        private int ReaderFiles(string dir, int count)
        {
            if (System.IO.Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir, "*." + txt_ext.Text);
                string[] dirs = Directory.GetDirectories(dir);
                foreach (var item in dirs)
                {
                    count += ReaderFiles(item, count);
                }
                count += files.Count();
            }
            return count;
        }

        private void ReaderConfig()
        {
            Config config = new Config(System.AppDomain.CurrentDomain.BaseDirectory);
            this.txt_Source.Text = config.sourcePath ?? string.Empty;
            this.txt_Target.Text = config.targetPath ?? string.Empty;
            this.txt_ext.Text = config.ext ?? "txt";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                Config config = new Config(System.AppDomain.CurrentDomain.BaseDirectory);
                config.sourcePath = txt_Source.Text;
                config.targetPath = txt_Target.Text;
                config.ext = txt_ext.Text;
                config.Save();
            }
            catch (Exception)
            {
                MessageBox.Show("发生无法保存配置的意外");
            }

            var write = new WriteFile(txt_Source.Text, txt_Target.Text, txt_ext.Text);
            Action<int> act = (i) =>
            {
                progressBar1.Value = i;
                Text = $"导出进度 {i}/{progressBar1.Maximum}";
            };
            write.Push((i) => Invoke(act, i));
        }

        private void btnOpenSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                OpenFile(foldPath);
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
    }
}