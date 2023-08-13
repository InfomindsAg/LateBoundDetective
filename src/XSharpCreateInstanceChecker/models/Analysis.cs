using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharpSafeCreateInstanceAnalzyer.models
{
    public class Analysis
    {
        public IEnumerable<SafeCreateInstanceInfo>? SafeCreateInstanceInfos { get; set; }
        
        public IEnumerable<GetRegServerRealInfo>? GetRegServerRealInfos { get; set; }

    }
}
