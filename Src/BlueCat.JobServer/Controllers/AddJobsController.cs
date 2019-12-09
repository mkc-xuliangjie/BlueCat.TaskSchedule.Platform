using BlueCat.Contract;
using BlueCat.Core;
using Hangfire;
using Hangfire.HttpJob.Server;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BlueCat.JobServer.Controllers
{
    [ApiController]
    [Route("api/job")]
    public class AddJobsController : Controller
    {
        /// <summary>
        /// 添加一个队列任务立即被执行
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        [HttpPost, Route("v1/add/background_job")]
        public ResponseModel<AddBackgroundJobResponseModel> AddBackGroundJob([FromBody] HttpJobItem httpJob)
        {
            ResponseModel<AddBackgroundJobResponseModel> responseModel = new ResponseModel<AddBackgroundJobResponseModel>();

            responseModel.ResultData = new AddBackgroundJobResponseModel();

            var addreslut = string.Empty;
            try
            {
                addreslut = BackgroundJob.Enqueue(() => HttpJob.Excute(httpJob, httpJob.JobName, httpJob.QueueName, false, null));

                responseModel.ResultData.JobId = addreslut;
            }
            catch (Exception ex)
            {
                responseModel.ResultCode = ResponseStatusCode.Error;
                responseModel.ResultDesc = ex.Message;

                //return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
            }
            return responseModel;
        }

        /// <summary>
        /// 添加一个周期任务
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        [HttpPost, Route("v1/add_or_update_recurring")]
        public ResponseModel<AddRecurringJobResponseModel> AddOrUpdateRecurringJob([FromBody] HttpJobItem httpJob)
        {
            ResponseModel<AddRecurringJobResponseModel> responseModel = new ResponseModel<AddRecurringJobResponseModel>();

            responseModel.ResultData = new AddRecurringJobResponseModel();

            try
            {
                RecurringJob.AddOrUpdate(httpJob.JobName, () => HttpJob.Excute(httpJob, httpJob.JobName, httpJob.QueueName, httpJob.IsRetry, null), httpJob.Cron, TimeZoneInfo.Local);

                responseModel.ResultData.Result = true;
            }
            catch (Exception ex)
            {
                responseModel.ResultCode = ResponseStatusCode.Error;
                responseModel.ResultDesc = ex.Message;
            }

            return responseModel;
        }

        /// <summary>
        /// 删除一个周期任务
        /// </summary>
        /// <param name="jobname"></param>
        /// <returns></returns>
        [HttpGet, Route("v1/delete_job")]
        public ResponseModel<DeleteJobResponseModel> DeleteJob(string jobname)
        {
            ResponseModel<DeleteJobResponseModel> responseModel = new ResponseModel<DeleteJobResponseModel>();

            responseModel.ResultData = new DeleteJobResponseModel();

            try
            {
                RecurringJob.RemoveIfExists(jobname);

                responseModel.ResultData.Result = true;
            }
            catch (Exception ex)
            {
                responseModel.ResultCode = ResponseStatusCode.Error;
                responseModel.ResultDesc = ex.Message;
            }

            return responseModel;
        }
        /// <summary>
        /// 手动触发一个任务
        /// </summary>
        /// <param name="jobname"></param>
        /// <returns></returns>
        // [HttpGet, Route("v1/job/trigger_recurring_job")]
        //public JsonResult TriggerRecurringJob(string jobname)
        //{
        //    try
        //    {
        //        RecurringJob.Trigger(jobname);
        //    }
        //    catch (Exception ec)
        //    {
        //        return Json(new Message() { Code = false, ErrorMessage = ec.ToString() });
        //    }
        //    return Json(new Message() { Code = true, ErrorMessage = "" });
        //}

        /// <summary>
        /// 添加一个延迟任务
        /// </summary>
        /// <param name="httpJob">httpJob.DelayFromMinutes（延迟多少分钟执行）</param>
        /// <returns></returns>
        [HttpPost, Route("v1/add/schedule_job")]
        public ResponseModel<AddScheduleJobResponseModel> AddScheduleJob([FromBody] HttpJobItem httpJob)
        {
            ResponseModel<AddScheduleJobResponseModel> responseModel = new ResponseModel<AddScheduleJobResponseModel>();

            responseModel.ResultData = new AddScheduleJobResponseModel();

            var reslut = string.Empty;
            try
            {
                reslut = BackgroundJob.Schedule(() => HttpJob.Excute(httpJob, httpJob.JobName, httpJob.QueueName, false, null), TimeSpan.FromMinutes(httpJob.DelayFromMinutes));

                responseModel.ResultData.JobId = reslut;

            }
            catch (Exception ex)
            {
                responseModel.ResultCode = ResponseStatusCode.Error;
                responseModel.ResultDesc = ex.Message;
            }

            return responseModel;
        }

        /// <summary>
        /// 添加连续任务,多个任务依次执行，只执行一次
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        [HttpPost, Route("v1/add/continue_job")]
        public ResponseModel<AddContinueJobResponseModel> AddContinueJob([FromBody] List<HttpJobItem> httpJobItems)
        {

            ResponseModel<AddContinueJobResponseModel> responseModel = new ResponseModel<AddContinueJobResponseModel>();
            responseModel.ResultData = new AddContinueJobResponseModel();


            var reslut = string.Empty;
            var jobid = string.Empty;
            try
            {
                httpJobItems.ForEach(k =>
                {
                    if (!string.IsNullOrEmpty(jobid))
                    {
                        jobid = BackgroundJob.ContinueJobWith(jobid, () => RunContinueJob(k));
                    }
                    else
                    {
                        jobid = BackgroundJob.Enqueue(() => HttpJob.Excute(k, k.JobName, k.QueueName, k.IsRetry, null));
                    }
                });
                reslut = "true";

                responseModel.ResultData.Result = true;
            }
            catch (Exception ex)
            {
                responseModel.ResultCode = ResponseStatusCode.Error;
                responseModel.ResultDesc = ex.Message;
            }

            return responseModel;
        }
        /// <summary>
        /// 执行连续任务
        /// </summary>
        /// <param name="httpJob"></param>
        public void RunContinueJob(HttpJobItem httpJob)
        {
            BackgroundJob.Enqueue(() => HttpJob.Excute(httpJob, httpJob.JobName, httpJob.QueueName, false, null));
        }
    }
}