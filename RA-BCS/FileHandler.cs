using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA_BCS
{
    internal static class FileHandler
    {
        public static async Task<string[]> GetFilesFromDirectory(string directory_path)
        {
            return await Task.Run(() => Directory.GetFiles(directory_path));
        }
    }
}
