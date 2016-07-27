using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformSolution.Perfmon.Api
{
    public interface IPerfmon
    {
        string Name { get; }
        string Code { get; }
        bool SetMaxTimes(int maxTimes);
        List<PerfmonModel> GetPerfmons();
    }
}
