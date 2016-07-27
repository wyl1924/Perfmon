using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformSolution.Perfmon.Api
{
    public class PerfmonManage : IPerfmon
    {
        public PerfmonManage(string name, string code)
        {
            _name = name;
            _code = code;
        }
        public PerfmonManage() { }
        private string _name { get; set; }
        private string _code { get; set; }
        public string Name { get { return _name; } }
        public string Code { get { return _code; } }
        public bool SetMaxTimes(int maxTimes)
        {
            return new DbPerfmon().SetMaxTimes(Code, Name, maxTimes);
        }
        public List<PerfmonModel> GetPerfmons()
        {
            return new DbPerfmon().GetPerfmons();
        }
    }
}
