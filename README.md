# BlueCat.TaskSchedule.Platform


Enqueue 方法： 放入队列执行。执行后销毁
BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget"));
 
Schedule方法： 延迟执行。第二个参数设置延迟时间。
BackgroundJob.Schedule(() => Console.WriteLine("Delayed"), TimeSpan.FromDays(1));

AddOrUpdate方法：重复执行。第二个参数设置cronexpression表达式。
参考quartz文档：http://jingyan.baidu.com/article/a3761b2b8e843c1576f9aaac.html
RecurringJob.AddOrUpdate(() => Console.Write("Recurring"), Cron.Daily);


基于队列的任务处理(Fire-and-forget jobs)
基于队列的任务处理是Hangfire中最常用的，客户端使用BackgroundJob类的静态方法Enqueue来调用，传入指定的方法（或是匿名函数），Job Queue等参数.

var jobId = BackgroundJob.Enqueue(
    () => Console.WriteLine("Fire-and-forget!"));
在任务被持久化到数据库之后，Hangfire服务端立即从数据库获取相关任务并装载到相应的Job Queue下，在没有异常的情况下仅处理一次，若发生异常，提供重试机制，异常及重试信息都会被记录到数据库中，通过Hangfire控制面板可以查看到这些信息。

延迟任务执行(Delayed jobs)
延迟（计划）任务跟队列任务相似，客户端调用时需要指定在一定时间间隔后调用：

var jobId = BackgroundJob.Schedule(
    () => Console.WriteLine("Delayed!"),
    TimeSpan.FromDays(7));
定时任务执行(Recurring jobs)
定时（循环）任务代表可以重复性执行多次，支持CRON表达式：

RecurringJob.AddOrUpdate(
    () => Console.WriteLine("Recurring!"),
    Cron.Daily);
延续性任务执行(Continuations)
延续性任务类似于.NET中的Task,可以在第一个任务执行完之后紧接着再次执行另外的任务：

BackgroundJob.ContinueWith(
    jobId,
    () => Console.WriteLine("Continuation!"));