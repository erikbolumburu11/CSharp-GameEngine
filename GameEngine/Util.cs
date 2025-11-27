using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class Util
    {
        public static string GetProjectDir()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            return projectDir;
        }
    }
}
