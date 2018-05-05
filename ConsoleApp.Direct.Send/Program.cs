using System;
using RabbitMQ.Client;

namespace ConsoleApp.Direct.Send
{
    class Program
    {
        private static string EXCHANGE_NAME = "test_exchange_direct";

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
                    channel.ExchangeDeclare(EXCHANGE_NAME, "direct", true);
                    //消息内容  
                    string message = "删除商品，id=200";
                    //发布消息
                    //JsonSerializerSettings settings = new JsonSerializerSettings();
                    //settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //var json = JsonConvert.SerializeObject(message, settings);
                    //var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(EXCHANGE_NAME, "delete", null, System.Text.Encoding.UTF8.GetBytes(message));
                    Console.WriteLine(" [x] Sent '" + message + "'");

                    for (var i = 0; i < 10; i++)
                    {
                        message += i;
                        channel.BasicPublish(EXCHANGE_NAME, "delete", null, System.Text.Encoding.UTF8.GetBytes(message));
                        Console.WriteLine(string.Format(" [{0}] Sent '" + message + "{1}'", i, i));
                    }

                    channel.Close();
                    connection.Close();
                }
            }
        }
    }
}
