using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsoleApp.Recv2
{
    class Program
    {
        private static string QUEUE_NAME = "test_queue_topic21";
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
                    //声明通道  
                    channel.QueueDeclare(QUEUE_NAME, false, false, false, null);
                    //绑定exchange  
                    channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, "item.#"); //使用item.# 匹配所有的以item开头的  
                    //同一时刻服务器只能发送一条消息给消费者；  
                    channel.BasicQos(0, 1, false);
                    //声明消费者  
                    var consumer = new QueueingBasicConsumer(channel);
                    //var customer = new EventingBasicConsumer(channel);
                    //监控队列，设置手动完成  
                    channel.BasicConsume(QUEUE_NAME, false, consumer);
                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();
                        var deliveryTag = ea.DeliveryTag;
                        byte[] bytes = ea.Body;
                        string message = Encoding.UTF8.GetString(bytes);

                        Console.WriteLine("插入商品 '" + message + "'");
                        Thread.Sleep(10);
                        channel.BasicAck(deliveryTag, false);
                    }
                }
            }
        }
    }
}
