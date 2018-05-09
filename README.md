# RabbitMqSolution
.net core 2.0 RabbitMq 示例

主要针对三种exchange的理解（Direct、Fanout、Topic）


#仿Hangfire Job，实现发布与订阅（方法体）
//Hangfire
Hangfire.BackgroundJob.Enqueue(() => new UserService().PrintUser(userModel, userModel.UserId));

//Demo=》托管一个Job
var job = Job.FromExpression(() => new UserService().PrintUser(userModel, userModel.UserId));
