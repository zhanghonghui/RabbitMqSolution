using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using RabbitMQ.Client;

namespace RabbitMQ.Base
{
    public class MQConnectionPool
    {
        private static readonly Dictionary<string, MQConnectionPool> _pools = new Dictionary<string, MQConnectionPool>(1);
        private List<IConnection> _connections = new List<IConnection>();

        private ReaderWriterLockSlim _rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private string _serverIp, _userName, _password;
        private int _serverPort, _poolSize, _heartBeat;

        private Random _random;

        public MQConnectionPool(string serverIp, int serverPort,
            string userName = null, string password = null,
            int poolSize = 1, int heartBeat = 2000)
        {
            this._serverIp = serverIp;
            this._serverPort = serverPort;
            this._userName = userName;
            this._password = password;

            //最大连接数
            this._poolSize = poolSize;
            //循环检测时间
            this._heartBeat = heartBeat;

            _random = new Random(poolSize);

            //启动连接检测线程
            ThreadPool.UnsafeQueueUserWorkItem(CheckConnection, null);
        }

        /// <summary>
        /// 线程定时检测当前的连接是否可用
        /// </summary>
        protected void CheckConnection(object state)
        {
            while (true)
            {
                try
                {
                    List<IConnection> errConnections = new List<IConnection>();
                    _rwlock.EnterReadLock();///进入读锁 
                    try
                    {
                        //检测当前的连接池中连是否都是正常的
                        for (int i = 0; i < _connections.Count; i++)
                        {
                            var conn = _connections[i];
                            try
                            {
                                using (var session = conn.CreateModel())
                                {
                                    session.ExchangeDeclare("__Heart", ExchangeType.Fanout, true);
                                }
                            }
                            catch
                            {
                                //连接测试失败，将该连接放入移除的队列
                                errConnections.Add(conn);
                            }
                        }
                    }
                    finally
                    {
                        _rwlock.ExitReadLock();
                    }
                    if (errConnections.Count > 0)
                    {
                        //申接请写锁，删除连池中不可用的连接
                        _rwlock.EnterWriteLock();
                        try
                        {
                            foreach (var conn in errConnections)
                            {
                                _connections.Remove(conn);
                            }
                            //OutputMsg("RabbitMQ 共移除不可用的连接：" + errConnections.Count.ToString() + "个");
                        }
                        finally
                        {
                            _rwlock.ExitWriteLock();
                        }
                    }
                    //保证连接池中有足够可用的连接
                    if (_connections.Count < _poolSize)
                    {
                        _rwlock.EnterWriteLock();
                        try
                        {
                            int addCount = _poolSize - _connections.Count;
                            //每次检测只补充一条新连接，以加快写锁的释放时间
                            //可以避免调用方在申请读锁时导致的超时
                            if (addCount > 0)
                            {
                                CreateNewConnection();
                                //OutputMsg("RabbitMQ 为连接池补充连接：" + _connections.Count.ToString() + "/" + _poolSize.ToString());
                            }
                        }
                        finally
                        {
                            _rwlock.ExitWriteLock();
                        }
                    }

                }
                catch (Exception ex)
                {
                    //OutputMsg("RabbitMQ异常:" + ex);
                }

                Thread.Sleep(_heartBeat);
            }
        }

        protected IConnection CreateNewConnection()
        {
            ConnectionFactory cf = new ConnectionFactory();
            cf.HostName = this._serverIp;
            cf.Port = this._serverPort;
            cf.UserName = this._userName;
            cf.Password = this._password;

            try
            {
                IConnection conn = cf.CreateConnection();

                _connections.Add(conn);

                return conn;
            }
            catch (Exception ex)
            { }
            finally
            { }

            return null;
        }

        public IConnection GetConnection()
        {
            _rwlock.EnterUpgradeableReadLock();///若线程用光了，得写一个连接线程，所以这里用可升级锁
            try
            {
                int index = _random.Next(_connections.Count);
                if (index < _connections.Count)
                {
                    return _connections[index];
                }
                if (_connections.Count == 0)
                {
                    _rwlock.EnterWriteLock();
                    try
                    {
                        if (_connections.Count == 0)
                        {
                            return CreateNewConnection();///这里创建了一个新的连接
                        }
                        else
                        {
                            return _connections[0];
                        }
                    }
                    finally
                    {
                        _rwlock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _rwlock.ExitUpgradeableReadLock();
            }
            return null;
        }

        public static MQConnectionPool CreateConnectionPool(string serverIp, int serverPort,
                    string userName = null, string password = null,
                    int poolSize = 1, int heartBeat = 2000)
        {
            MQConnectionPool ret;
            string key = serverIp + ":" + serverPort.ToString() + ":" + userName + ":" +
                password + ":" + poolSize.ToString() + ":" + heartBeat.ToString();
            if (!_pools.TryGetValue(key, out ret))
            {
                lock (_pools)
                {
                    if (!_pools.TryGetValue(key, out ret))
                    {
                        ret = new MQConnectionPool(serverIp, serverPort,
                            userName, password, poolSize, heartBeat);
                        _pools[key] = ret;
                    }
                }
            }

            return ret;
        }
    }
}
