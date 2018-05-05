using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQ.Base
{
    public  class RabbitClientHelper
    {
        private MQConnectionPool _pool;

        public RabbitClientHelper(string hostname = "localhost", int port = 5672, string username = "guest", string password = "guest")
        {
            _pool = MQConnectionPool.CreateConnectionPool(hostname, port, username, password, 5, 10000);
        }

        public RabbitClientHelper(string hostname = "localhost", string password = "guest")
        {
            _pool = MQConnectionPool.CreateConnectionPool(hostname, 5672, "guest", password, 5, 10000);
        }

        public RabbitClientHelper()
        {
            RabbitMqConfig rabbitMqConfig = new RabbitMqConfig();
            rabbitMqConfig.IP = "localhost";
            rabbitMqConfig.Port = 5672;
            rabbitMqConfig.UserName = "guest";
            rabbitMqConfig.Password = "guest";
            _pool = MQConnectionPool.CreateConnectionPool(rabbitMqConfig.IP, rabbitMqConfig.Port, rabbitMqConfig.UserName, rabbitMqConfig.Password, 5, 10000);
        }

        public RabbitMQ.Client.IConnection GetConnection()
        {
            return _pool.GetConnection();
        }
    }

    /// <summary>
    /// 消息队列的配置信息
    /// </summary>
    public class RabbitMqConfig
    {
        #region host
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 服务器端口，默认是 5672
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 虚拟主机名称
        /// </summary>
        public string VirtualHost { get; set; }
        #endregion
    }
}
