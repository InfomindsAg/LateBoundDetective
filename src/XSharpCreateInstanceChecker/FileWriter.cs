using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharpSafeCreateInstanceAnalzyer
{
    public class FileWriter
    {
        private readonly string filePath;

        public FileWriter(string filePath) {
            this.filePath = filePath;
        }

        public void Write(string text)
        {
            File.WriteAllText(filePath, text);
            
        }

      

       
    }
}
