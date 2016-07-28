# Perfmon
　由于最近系统访问量过大，相关系统间处理不同步，造成相互等待时间较长，影响系统整体运行性能，造成用户明显感觉响应时间慢、体验不好。所以就对每一个模块的访问人数加一控制。
　
　　进而决定用过滤器与redis。
1.设置最大访问人数：

　在数据存在redis中。当页面打开时如果redis中页面数据为空，则进行数据初始化。没有设置添加页面，具体原因吗？因为控制的模块是固定的，当添加模块式，直接添加redis页面数据即可。有点不合理哈，不过先这样处理吧。


1.1.redis的操作：

　　我喜欢用redis存缓存信息，如果你喜欢用mongodb或其他方式都可以。以前的文档中已经有过类似的操作，在这再写一下。

　1.1.1.redis连接基类　　

　　每次操作redis继承此基类即可.
复制代码

  public abstract class RedisHelper
    {
        private IRedisClient _client;
        private string _configuration_string;

        public RedisHelper()
        {
        }

        public RedisHelper(string configuration_string)
        {
            this._configuration_string = configuration_string;
        }

        public void set_configuration_string(string configuration_string)
        {
            this._configuration_string = configuration_string;
        }

        public IRedisClient rs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._configuration_string))
                {
                    return null;
                }
                if (this._client == null)
                {
                    this._client = new RedisClient(this._configuration_string);
                }
                return this._client;
            }
        }
    }
}

复制代码

　1.1.2.项目中redis的使用

　　（1）配置文件

　　其中路径可以直接写服务器ip地址。我把他写成字符串，是为了开发环境和正是环境都不用修改程序，直接做ip映射就行了。

　　（2）连接redis服务器
复制代码

 public class DbPerfmon : RedisHelper
    {
        public DbPerfmon()
            : base()
        {
            string myredis = ConfigurationManager.AppSettings["Redis_Server_Url_Perfmon"].ToString();
            set_configuration_string(myredis);
        }

复制代码

1.1.3.数据操作【获取各模块访问数据，设置最大访问数，删除指定redis数据】

　　　（1）其中Loginname为redis key值。【一个人同时只能操作一个模块】

　　　（2）模块名称为redis value值

注：简单说一下具体方法：

　　   （1）（bool SetMaxTimes(string code, string name, int times)
　　　　此方法用作设置每个模块访问最大人数，其中code是模块名称。【代码的实现欢迎大家给点中肯的建议】
　　   （2）List<PerfmonModel> GetPerfmons()
　　　　这是上边页面展示数据集的获得，不再累述
　　   （3）bool PagePush(string ModuleName, string LoginUserName)
　　　　核心方法，如果访问人数小于最大访问人数。当前把当前访问人及模块存入redis
　　   （4） bool PageRemove(string LoginUserName)
　　　　当执行方法执行结束时触发，移除redis中访问人信息。
复制代码

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

复制代码

 
2.利用过滤器进行模块监控

　　c#中过滤器在这不再累述。准备在以后c#特性中进行详细叙述。分别在方法执行前与方法执行后进行调用上边的方法。
2.1.OnActionExecuting

　　判断当前访问人数是否大于指定访问人数，不大于则加一，继续action
复制代码

   public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var filer = new DbPerfmon().PagePush(ModuleName, LoginUserName);
            if (filer)
                base.OnActionExecuting(filterContext);
            else
            {
                ContentResult cr = new ContentResult();
                //cr.Content = "<script>window.location.href='" + HttpContext.Current.Request.UrlReferrer.OriginalString + "';</script>";
                cr.Content = "<p style='color:Red;font-weight:bold;clear:both'>同时操作此业务人员较多，请稍候再试。</p>";
                filterContext.Result = cr;
            }

        }

复制代码
2.2.OnResultExecuted

action执行后,删除该访问人数redis信息

 public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            new DbPerfmon().PageRemove(LoginUserName);
        }

2.3.服务系统出现异常时

　　此方法是后续加的，没有过多的测试。
复制代码

 public class ExceptionFilterAttribute : HandleErrorAttribute
        {
            public override void OnException(ExceptionContext filterContext)
            {
                base.OnException(filterContext);
                ContentResult cr = new ContentResult();
                //cr.Content = "<script>window.location.href='" + HttpContext.Current.Request.UrlReferrer.OriginalString + "';</script>";
                cr.Content = "<p style='color:Red;font-weight:bold;clear:both'>此功能页面异常，请联学系校管理员或稍候再试。</p>";
                filterContext.Result = cr;
            }
        }
Perfmon
