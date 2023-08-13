using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XSharp.VsParser.Helpers.Parser;
using XSharpSafeCreateInstanceAnalzyer.models;

namespace XSharpSafeCreateInstanceAnalzyer.analysis
{
    public interface IAnalysis<T>
    {
        public IEnumerable<T> GetAnalyses();


    }
}
