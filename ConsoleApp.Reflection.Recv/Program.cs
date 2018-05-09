using System;
using System.Reflection;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Reflection.Services;
using Reflection.Services.Common;
using Reflection.Services.Storage;

namespace ConsoleApp.Reflection.Recv
{
    class Program
    {
        private static string QUEUE_NAME = "test_queue_direct_job";
        private static string EXCHANGE_NAME = "test_exchange_direct_job";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Recv();

            Console.Read();
        }

        private static void Recv()
        {
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
                    //绑定队列到交换机; 
                    channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, "job");
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
                        string message = System.Text.Encoding.UTF8.GetString(bytes);

                        //执行Job方法
                        RunJob(message);

                        System.Threading.Thread.Sleep(10);
                        channel.BasicAck(deliveryTag, false);
                    }
                }
            }
        }

        private static void RunJob(string jobData)
        {
            //从消息队列接收Job
            var jobStr = jobData;

            //反序列化Job对象
            var jobClone = JobHelper.FromJson<InvocationData>(jobStr).Deserialize();

            //执行一个Job
            JobPerform jobPerform = new JobPerform();
            var result = jobPerform.Perform(jobClone);

            Console.WriteLine("执行Job方法时间是:" + DateTime.Now + "，返回值是：" + result);
        }

        public void ReflectionTest()
        {
            #region 反射demo，理解1

            UserModel userModel = new UserModel();
            userModel.UserId = 1;
            userModel.UserName = "JayZhang";

            string userModelStr = Newtonsoft.Json.JsonConvert.SerializeObject(userModel);

            //UserService user = new UserService();
            //var userId = user.PrintUser(userModel);

            ServiceTask serviceTask = new ServiceTask();
            serviceTask.AssemblyName = "Reflection.Services";
            serviceTask.ModuleName = "UserService";
            serviceTask.ClassName = "PrintUser";

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //System.Reflection.Assembly ass = Assembly.LoadFrom(baseDirectory + serviceTask.AssemblyName + ".dll");
            System.Reflection.Assembly ass = Assembly.Load(serviceTask.AssemblyName);
            System.Type t = ass.GetType(serviceTask.AssemblyName + "." + serviceTask.ModuleName);

            //创建实例
            object classObject = System.Activator.CreateInstance(t);

            //获得方法
            MethodInfo method = t.GetMethod(serviceTask.ClassName);

            //获得方法参数
            var methodParaList = method.GetParameters();

            var mpT = methodParaList[0].ParameterType;

            var userModelFromPara = Newtonsoft.Json.JsonConvert.DeserializeObject(userModelStr, mpT);

            //调用方法
            method.Invoke(classObject, new object[] { userModelFromPara, 1 });

            #endregion

            Assembly assemUser = typeof(UserService).Assembly;
            ServiceTask serviceTaskUser = new ServiceTask();
            serviceTaskUser.AssemblyName = assemUser.FullName;
            serviceTaskUser.ModuleName = "UserService";
            serviceTaskUser.ClassName = "PrintUser";

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("主线程代码运行结束");
            });


            //通过GetAssemblies 调用appDomain的所有程序集
             foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                //反射当前程序集的信息
                var fullName = assem.FullName;
            }

        }

    }
}
