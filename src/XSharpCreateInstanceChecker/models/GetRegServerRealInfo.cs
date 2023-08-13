using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharpSafeCreateInstanceAnalzyer.models
{
    public class GetRegServerRealInfo : IInfo
    {
        public string ClassName { get; set; }

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        public int StartLineOffset { get; set; }

        public int EndLineOffset { get; set;}

        public string SourceFile { get; set; }

        public string MethodName { get; set; }


        public string MethodType {  get; set; }     

        public override string ToString()
        {
            return ClassName + "," + StartLine + "," + EndLine + "," + StartLineOffset + "," + EndLineOffset;
        }
    }
}
