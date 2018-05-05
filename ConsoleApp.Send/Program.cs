using System;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ConsoleApp.Send
{
    class Program
    {
        private static string EXCHANGE_NAME = "test_exchange_topic";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var rabbitClientHelper = new RabbitMQ.Base.RabbitClientHelper();

            //获取到连接以及mq通道  
            RabbitMQ.Client.IConnection connection = rabbitClientHelper.GetConnection();

            if (connection != null)
            {
                using (IModel channel = connection.CreateModel())
                {
                    //声明exchange  
                    channel.ExchangeDeclare(EXCHANGE_NAME, "topic");
                    //消息内容  
                    string message = "插入商品，id=100";
                    //发布消息
                    //JsonSerializerSettings settings = new JsonSerializerSettings();
                    //settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //var json = JsonConvert.SerializeObject(message, settings);
                    //var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(EXCHANGE_NAME, "item.insert", null, System.Text.Encoding.UTF8.GetBytes(message));
                    Console.WriteLine(" [x] Sent '" + message + "'");

                    for(var i=0;i<10;i++)
                    {
                        message += i;
                        channel.BasicPublish(EXCHANGE_NAME, "item.insert" + i, null, System.Text.Encoding.UTF8.GetBytes(message));
                        Console.WriteLine(string.Format(" [{0}] Sent '" + message + "{1}'", i, i));
                    }

                    channel.Close();
                    connection.Close();
                }
            }
        }
    }
}
