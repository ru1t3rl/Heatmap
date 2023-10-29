using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ru1t3rl.StateRecorder.Utilities
{
    public abstract class BaseDataHandler
    {
        protected static void ValidateFile(ref string path, bool creatIfNotExists = false)
        {
            if (!Regex.IsMatch(path, @"[A-Z]:\\"))
            {
                path = Path.Combine(Application.dataPath, path);
            }

            if (!File.Exists(path))
            {
                if (!creatIfNotExists)
                    throw new FileNotFoundException($"The file specified could not be found!\n{path}");
                else
                {
                    var pathSplit = path.Split('\\').ToList();
                    pathSplit.RemoveAt(pathSplit.Count - 1);
                    Directory.CreateDirectory(Path.Combine(pathSplit.ToArray()));
                    File.Create(path);
                }
            }
        }
    }
}