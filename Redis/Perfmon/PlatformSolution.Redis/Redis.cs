using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformSolution.Redis
{

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
