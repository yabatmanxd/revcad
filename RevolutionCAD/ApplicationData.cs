using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    public static class ApplicationData
    {
        public static string FileName { get; set; }
        

        public static bool IsFileExists(string extension, out string error)
        {
            if (File.Exists(Environment.CurrentDirectory + FileName + extension))
            {
                error = "";
                return true;
            }
            error = $"Файл {FileName}{extension} не существует";
            return false;
        }
    }
}
