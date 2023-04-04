using System.IO;

namespace MonItemsMerge
{
    public class File
    {
        public string Path
        {
            get
            {
                if (Info != null)
                {
                    return Info.FullName;
                }
                else
                    return string.Empty;
            }
        }

        public string Name
        {
            get
            {
                if (Info != null)
                {
                    return System.IO.Path.GetFileNameWithoutExtension(Info.FullName);
                }
                else
                    return string.Empty;
            }
        }

        public FileInfo Info { get; private set; }

        public File(string path)
        {
            Info = new FileInfo(path);
        }
    }
}