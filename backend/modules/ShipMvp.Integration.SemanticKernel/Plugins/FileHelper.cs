using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMvp.Integration.SemanticKernel.Plugins
{
    public class FileHelper
    {
        public static string GetFilePathFromPluginsFolder(string subdirs, string fileName)
        {
            string projectRootDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(projectRootDir, "Plugins", subdirs);
            return $"{path}/{fileName}";

        }
    }
}
