using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quartz;
using Quartz.Impl;
using NFC_DL_WebService.Models;

namespace NFC_DL_WebService.Controllers
{
    public class Scheduler : ApiController
    {
    }
    public class FbFilePathChanger : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            dbModule.dbName = "";
        }
    }
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<FbFilePathChanger>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                  )
                .Build();
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
