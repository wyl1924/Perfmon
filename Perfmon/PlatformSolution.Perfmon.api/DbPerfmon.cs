using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformSolution.Redis;
using System.Configuration;

namespace PlatformSolution.Perfmon.Api
{
    public class PerfmonModel
    {
        public int maxTimes { get; set; }
        public int nowTimes { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string startTime { get; set; }
    }
    public class PerfmonTime
    {
        public string code { get; set; }
        public string name { get; set; }
        public int maxTimes { get; set; }
    }
    public class DbPerfmon : RedisHelper
    {
        public DbPerfmon()
            : base()
        {
            string myredis = ConfigurationManager.AppSettings["Redis_Server_Url_Perfmon"].ToString();
            set_configuration_string(myredis);
        }
      
        public bool SetMaxTimes(string code, string name, int times)
        {
            try
            {
                List<PerfmonTime> list = new List<PerfmonTime>();
                string key = code;
                using (rs)
                {
                    PerfmonTime time = new PerfmonTime()
                    {
                        name = name,
                        code = code,
                        maxTimes = times
                    };
                 ;
                    if (rs.Get<List<PerfmonTime>>("Perfmon") == null)
                        list.Add(time);
                    else if (list.Exists(l => l.code == code && l.name == name))
                    {
                        list = rs.Get<List<PerfmonTime>>("Perfmon");
                        list.Add(time);
                    }
                    else
                    {
                        list = rs.Get<List<PerfmonTime>>("Perfmon");
                        list.RemoveAll(l => l.code == code && l.name == name);
                        list.Add(time);
                    }
                    var flag = rs.Set<List<PerfmonTime>>("Perfmon", list);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;

            }
        }
        public List<PerfmonModel> GetPerfmons()
        {
            List<PerfmonModel> list = new List<PerfmonModel>();
            using (rs)
            {
                var PerfmonTime = rs.Get<List<PerfmonTime>>("Perfmon");
                if (PerfmonTime == null || PerfmonTime.Count < 1)
                    return null;
                PerfmonTime.ForEach(p => list.Add(new PerfmonModel { name = p.name, code = p.code, maxTimes = p.maxTimes, nowTimes = ModulCount(p.code) })
               );
            }
            return list;
        }

        int ModulCount(string ModuleName)
        {
            List<string> keylist = rs.SearchKeys("*");
            //得到集合
            int count = 0;
            IDictionary<string, object> list = rs.GetAll<object>(keylist);
            count = list == null ? 0 : list.Where(l => l.Value.ToString() == ModuleName).Count();
            return count;
        }

        public bool PagePush(string ModuleName, string LoginUserName)
        {
            using (rs)
            {
                var PerfmonTime = rs.Get<List<PerfmonTime>>("Perfmon");
                if (PerfmonTime == null)
                    return false;
                else if (!PerfmonTime.Exists(l => l.code == ModuleName))
                    return false;
                int maxTime = PerfmonTime.Where(p => p.code == ModuleName).ToList()[0].maxTimes;
                //得到所有的Key
                List<string> keylist = rs.SearchKeys("*");
                //得到集合

                IDictionary<string, object> list = rs.GetAll<object>(keylist);
                int count = 0;

                count = list == null ? 0 : list.Where(l => l.Value.ToString() == ModuleName).Count();

                //foreach (KeyValuePair<string, object> kv in list)
                //{
                //    if (kv.Value == ModuleName)
                //    {
                //        count++;
                //    }
                //}
                if (maxTime < count)
                    return false;
                //return rs.Set(LoginUserName, ModuleName, DateTime.Now.AddMinutes(30));
                return rs.Set(LoginUserName, ModuleName);
            }
        }
        public bool PageRemove(string LoginUserName)
        {
            using (rs)
            {
                return rs.Remove(LoginUserName);
            }
        }
    }
}
