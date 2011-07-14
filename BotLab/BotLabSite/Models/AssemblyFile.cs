using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Compete.Site.Models
{
    public class AssemblyFileRepository
    {
        public static string Directory = RockPaperAzure.AzureHelper.GetLocalFolder();

        public void Add(HttpPostedFileBase file, string fileName)
        {
            DirectoryHelpers.CreateIfNecessary(Directory);
            DirectoryHelpers.CreateIfNecessary(Directory, "bots");
            string savedFileName = Path.Combine(Path.Combine(Directory, "bots"), fileName);
            File.Delete(savedFileName);
            file.SaveAs(savedFileName);
        }

        public void Add(String srcPath, string fileName)
        {
            DirectoryHelpers.CreateIfNecessary(Directory);
            DirectoryHelpers.CreateIfNecessary(Directory, "bots");
            File.Copy(srcPath, Path.Combine(Path.Combine(Directory, "bots"), fileName), true);
        }

        public void Remove(string fileName)
        {
            string savedFileName = Path.Combine(Path.Combine(Directory, "bots"), fileName);
            File.Delete(savedFileName);
        }

        public ICollection<AssemblyFile> FindAllPlayers()
        {
            List<AssemblyFile> files = new List<AssemblyFile>();
            files.AddRange(FindAllDlls(Path.Combine(Directory, "bots")).ToArray());
            return files;
        }

        private static IEnumerable<AssemblyFile> FindAllDlls(string directory)
        {
            foreach (string path in System.IO.Directory.GetFiles(directory, "*.dll"))
            {
                yield return new AssemblyFile(path);
            }
        }
    }

    public static class DirectoryHelpers
    {
        public static void CreateIfNecessary(string full)
        {
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
            }
        }
        public static void CreateIfNecessary(string path, string relative)
        {
            string full = Path.Combine(path, relative);
            CreateIfNecessary(full);
        }
    }

    [Serializable]
    public class AssemblyFile
    {
        readonly string _path;

        public string Path
        {
            get { return _path; }
        }

        public AssemblyFile(string path)
        {
            _path = path;
        }

        public bool Equals(AssemblyFile obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj._path, _path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(AssemblyFile)) return false;
            return Equals((AssemblyFile)obj);
        }

        public override Int32 GetHashCode()
        {
            return _path.GetHashCode();
        }
    }
}
