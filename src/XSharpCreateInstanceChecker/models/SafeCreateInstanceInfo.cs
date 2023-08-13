using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharpSafeCreateInstanceAnalzyer.models
{
    public class SafeCreateInstanceInfo : IInfo
    {
        public string ClassName { get; set; }

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        public int StartLineOffset { get; set; }

        public int EndLineOffset { get; set; }

        public override string ToString()
        {
            return ClassName + "," + StartLine + "," + EndLine + "," + StartLineOffset + "," + EndLineOffset;
        }

    }
}
