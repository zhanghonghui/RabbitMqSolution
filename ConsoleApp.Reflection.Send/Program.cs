using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Reflection.Services;
using Reflection.Services.Common;
using Reflection.Services.Storage;

namespace ConsoleApp.Reflection.Send
{
    class Program
    {
        private static string EXCHANGE_NAME = "test_exchange_direct_job";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            UserModel userModel = new UserModel();
            userModel.UserId = DateTime.Now.Second;
            userModel.UserName = "JayZhang";

            //托管一个Job
            var job = Job.FromExpression(() => new UserService().PrintUser(userModel, userModel.UserId));

            //生产一个存储对象：Storage
            var invocationData = InvocationData.Serialize(job);
            var jobStr = JobHelper.ToJson(invocationData);

            //发布到RabbitMq某队列
            Publish(jobStr);

            Console.WriteLine("DateTime is " + DateTime.Now);

            Console.Read();
        }

        private static void Publish(string message)
        {
            var rabbitClientHelper = new RabbitMQ.Base.RabbitClientHelper();

            //获取到连接以及mq通道  
            RabbitMQ.Client.IConnection connection = rabbitClientHelper.GetConnection();

            if (connection != null)
            {
                using (IModel channel = connection.CreateModel())
                {
                    //声明exchange  
                    channel.ExchangeDeclare(EXCHANGE_NAME, "direct", true);

                    channel.BasicPublish(EXCHANGE_NAME, "job", null, System.Text.Encoding.UTF8.GetBytes(message));

                    channel.Close();
                    connection.Close();
                }
            }
        }
    }
}
