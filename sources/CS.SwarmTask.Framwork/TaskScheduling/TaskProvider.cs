#region copyright info

//------------------------------------------------------------------------------
// <copyright company="ChaosStudio">
//     Copyright (c) 2002-2015 巧思工作室.  All rights reserved.
//     Contact:		MSN:atwind@cszi.com , QQ:3329091
//		Home:		 http://www.cszi.com
// </copyright>
//------------------------------------------------------------------------------

#endregion

using System;
using System.Threading;
using CS.Diagnostics;

namespace CS.TaskScheduling
{
    /// <summary>
    ///  抽象的单一任务驱动。
    ///  ver:0.3.1
    ///  <para>具体任务必须继承自本类，每个任务单独一个计时器。</para>
    /// <para>PathDate用于补全操作</para>
    /// </summary>
    /// 
    /// <description>
    ///   2010-3-10 , zhouyu , 使用委托调用子类的业务
    ///   2010-4-20 , zhouyu , 配置序列化成对象重每任务在一定时间内置于线程池中执行
    /// </description>
    /// 
    /// <history>
    ///   2010-2-26 16:06:39 , zhouyu ,  创建
    ///  </history>
    public abstract class TaskProvider : IDisposable
    {
        #region 变量 属性

        /// <summary>
        /// 程序运行日志
        /// </summary>
        protected readonly ITracer Log = Logger.GetSysLog(typeof(TaskProvider));

        /// <summary>
        /// 保存运行状态时的互斥体，共享变量，全局唯一
        /// </summary>
        protected static readonly Mutex SaveMutex = new Mutex(false);

        ///// <summary>
        ///// 工作互斥体
        ///// </summary>
        //protected static readonly Mutex WorkingMutex = new Mutex(false);

        /// <summary>
        /// 工作线程调用间隔，毫秒
        /// </summary>
        public double WorkerInterval { get; set; }

        /// <summary>
        /// 任务信息
        /// </summary>
        public TaskInfo Task { get; set; }

        /// <summary>
        /// 共公资源配置信息引用
        /// </summary>
        internal protected ResourceCollection Resources { get; set; }

        /// <summary>
        /// 系统自带的预定义扩展
        /// </summary>
        protected PreExtendInfo Extend { get; set; }

        ///// <summary>
        ///// 宿主服务状态
        ///// </summary>
        //public ServiceStatusType ServiceStatus { get; set; }

        ///// <summary>
        ///// 运行状态配置的引用
        ///// 用来保存运行状态的
        ///// </summary>
        //public ExecutionStatus ExecutionStatus { get; set; }

        /// <summary>
        /// 任务工作
        /// 用以执行本任务的。
        /// </summary>
        protected Timer TaskWorker { get; set; }

        /// <summary>
        /// 补全时间，补全类型任务执行后要更新PatchDate的值
        /// Note:1. 补全任务执行时更新的补全时间，必须从执行触发时间的上一个时间里开始，如10-15号要补全从10-1号的数据，那么任务执行完10-1号任务后该值为10-2，当前补全
        /// Note:2. 配置里的IsPath必须为true，否则不执行补全功能。
        /// </summary>
        public virtual DateTime? PathDate { get; set; }

        /// <summary>
        /// 正在运行的次数
        /// </summary>
        private int _runTimes;

        /// <summary>
        /// 是否正在回调中
        /// </summary>
        private bool _isCallBacking;

        /// <summary>
        /// 停止的委托
        /// </summary>
        private Action _stopTask;

        #endregion

        #region 事件通知

        #endregion

        #region 构造

        protected TaskProvider()
        {
            WorkHandler = () => Work(); //附加该委托即可
            _stopTask = (delegate { }); //默认给一个空的方法
        }

        #endregion

        #region 方法

        ///// <summary>
        ///// 初始化预定义的扩展
        ///// </summary>
        //internal void InitPreExetend()
        //{
        //    PreExtend.InitRefResource(Resources);
        //}

        /// <summary>
        /// 初始化自定义的
        /// <para>自定义了新的扩展配置时一定要重写该方法，并自写扩展代码</para>
        /// </summary>
        public virtual void InitExtend()
        {
            if (Extend == null)
                Extend = Task.GetExtend<PreExtendInfo>();

            Extend.InitRefResource(Resources);
        }


        /// <summary>
        /// 工作委托[返回是否执行]
        /// </summary>
        protected Func<TaskResult> WorkHandler;

        protected bool IsRemoving => Task.Execution.RunStatus == TaskRunStatusType.Removing;

        /// <summary>
        /// 工作运行
        /// Note:两次间隔有100豪秒左右的误差
        /// </summary>
        public virtual void Working()
        {
            try
            {
                TaskWorker.Change(Timeout.Infinite, Timeout.Infinite); //暂停计时。
                if (IsRemoving)
                {
                    Log.Warn("任务被标起为移除，回调中断，且任务不会被再次调用。");
                    return;
                }

                //执行今日任务
                _runTimes++;
                var now = SystemTime.Now();
                //Log.Info($"[{this}] 第{_runTimes}次执行开始。[{now:HH:mm:ss ffff}] ◇");
                ChangeStatus(TaskRunStatusType.Working);
                var val = new TaskResult();
                try
                {
                    val = WorkHandler(); //同步委托，任务执行[可能较耗时]
                }
                catch (Exception ex)
                {
                    Log.Error($"执行任务<{Task}>时发生异常:", ex);
                    //throw;
                    val.Result = TaskResultType.Error;
                    val.ExtendMessage = ex.Message;
                }
                finally
                {
                    ChangeStatus(TaskRunStatusType.Worked);
                    Task.Execution.LastRun = now;
                }
               
                var runSpan = SystemTime.Now() - now;
                Log.Info($"[{this}] 第{_runTimes}次执行结果[{val.Result} : {val.Message}] [Execution:{runSpan}]");

                //Note:工作完成后的状态处理
                //Note:注意，这里的错误次数实际上是执行失败的次数
                if (val.Result.HasFlag(TaskResultType.Error))
                {
                    var sleepInterval = ((TimeSpan)Task.WorkSetting.SleepInterval);
                    Task.Execution.SleepTimes++;
                    Log.Warn($"[{this}] 状态更新[{val.Result}],休眠次数++ ，准备[{sleepInterval}]后再次执行");
                    TaskWorker.Change(sleepInterval, TimeSpan.FromMilliseconds(-1));
                    return;
                }
                else
                {
                    Task.Execution.RunTimes++;
                    var runInterval = Task.GetNextInterval();
                    if (runInterval == null)
                    {
                        ChangeStatus(TaskRunStatusType.Removing);
                        Log.Debug($"[{this}] 下次运行时间为null，当前任务停止。");
                        return;
                    }
                    if (runInterval.Value.TotalMilliseconds > WorkerInterval * 5)
                    {
                        ChangeStatus(TaskRunStatusType.Removing);
                        Log.Debug($"[{this}] 下次运行时间{runInterval}，超过5倍工作线程间隔，暂时移除执行队列。当前任务停止。");
                        return;
                    }

                    //var runInterval = runTime.Value.Subtract(now);
                    SaveExecution();
                    Log.Debug($"[{this}]第{_runTimes}次执行结束。 运行成功[Times:{Task.Execution.RunTimes}] ，准备[{runInterval}]后再次执行 ◆");
                    TaskWorker.Change(runInterval.Value, TimeSpan.FromMilliseconds(-1)); //Note:更改计时器约50多毫秒
                }

                #region 根据任务配置做出相应动作

                //本次任务已完成,Note:只有本次任务达到所设条件才算是正常完成，正常完成后才更新最后成功完成的时间。
                if ((Task.Execution.RunTimes >= Task.WorkSetting.Times && Task.WorkSetting.Times > 0) ||
                    (val.Result.HasFlag(TaskResultType.Finished)))
                {
                    //Task.Meta.Execution.LastSucceedRun = PathDate ?? now;   //Note:可自动补全点
                    //Task.Meta.Execution.RunStatus = TaskRunStatusType.TodayComplete;
                    ChangeStatus(TaskRunStatusType.Removing);
                    Log.Debug($"■ [{this}] ({Task.Execution.LastSucceedRun})完成。■");
                    return;
                }

                //根据设定，一旦有错误发生。立即停止
                if (val.Result.HasFlag(TaskResultType.Error) && Task.WorkSetting.ErrorWay == ErrorWayType.Stop)
                {
                    ChangeStatus(TaskRunStatusType.Removing);
                    Log.Info($"▲ [{this}] 根据设定Stop，发生了错误一次，等待移除。▲");
                    return;
                }

                //根据设定，有错误时。休眠超期后停止
                if (Task.Execution.SleepTimes >= Task.WorkSetting.SleepInterval.Times &&
                    Task.WorkSetting.SleepInterval.Times > 0 && Task.WorkSetting.ErrorWay == ErrorWayType.TryAndStop)
                {
                    ChangeStatus(TaskRunStatusType.Removing);
                    Log.Info($"▲ [{this}] 根据设定Sleep，发生了错误{Task.Execution.SleepTimes}次，等待移除。▲");
                    return;
                }

                #endregion

            }
            catch (Exception ex)
            {
                //Note:异常发生后停止该任务，不管任何原因
                Log.Error($"[{this}] 执行异常，停止执行。", ex);
                Stop();
                //throw;
            }
            finally
            {
            }
        }

        /// <summary>
        /// 线程回调
        /// </summary>
        private void TimerCallback(object state)
        {
            lock (this)
            {
                _isCallBacking = true;
                Working();
                _isCallBacking = false;
            }
            //if (Monitor.TryEnter(this, Task.WorkSetting.Timeout*1000))
            //{
            //    // 得到资源
            //    Monitor.Exit(this);
            //}
            //else
            //{
            //    //获取排它锁失败
            //    Log.Warn($"任务[{Task.Meta.Id}:{Task.Meta.Name}]试图执行时超时");
            //}
        }

        /// <summary>
        /// 状态变更
        /// <remarks>
        /// 正在移除中的状态，不可变更
        /// </remarks>
        /// </summary>
        /// <param name="status"></param>
        public void ChangeStatus(TaskRunStatusType status)
        {
            if (Task.Execution.RunStatus == TaskRunStatusType.Removing) return;
            Task.Execution.RunStatus = status;
        }

        /// <summary>
        /// 运行状态保存
        /// </summary>
        public void SaveExecution()
        {
            SaveMutex.WaitOne();
            //ExecutionStatus.Save();
            SaveMutex.ReleaseMutex();
        }

        /// <summary>
        /// 任务入口，继承类必须实现。
        /// </summary>
        /// <returns>
        /// 执行结果，参见：<see cref="TaskResult"/>
        /// </returns>
        protected abstract TaskResult Work();

        #endregion

        #region 任务驱动器功能

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                ChangeStatus(TaskRunStatusType.Running);

                //校验 计算启动时间。
                var now = SystemTime.Now();
                //var launchTime = Task.GetNextRunTime(now); //计算启动时间
                var launchInterval = Task.GetNextInterval();
                if (launchInterval == null)
                {
                    Log.Warn($"[{this}] 启动时间为null，中断启动。");
                    return;
                }
                Task.Execution.RunTimes = 0;
                Task.Execution.SleepTimes = 0;
                TaskWorker = new Timer(TimerCallback, null, launchInterval.Value /*第一次延时调用*/, TimeSpan.FromMilliseconds(-1) /*Note:回调时会更改调用延时，此周期设为无限*/);
                Log.Debug($"[{this}] 启动成功，将于:{launchInterval}后运行");
            }
        }


        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            //任务回调结束后才可以结束
            if (_isCallBacking)
            {
                Log.Debug($"[{this}]正在回调中，停止方法附加到任务完成后的委托上。");
                _stopTask = StopTask; //使用委托来处理
            }
            else
            {
                StopTask();
            }
        }

        private void StopTask()
        {
            lock (this)
            {
                ChangeStatus(TaskRunStatusType.Stoping);
                TaskWorker.Change(Timeout.Infinite, Timeout.Infinite); //禁止再次回调
                //SaveExecution(); //先保存一次状态
                Task.Execution.RunStatus = TaskRunStatusType.Stoped;
                ChangeStatus(TaskRunStatusType.Stoped);
                Log.Debug($"[{this}] 停止，计时器已释放。");
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            lock (this)
            {
                ChangeStatus(TaskRunStatusType.Pausing);
                //SaveExecution(); //先保存一次状态
                TaskWorker.Change(Timeout.Infinite, Timeout.Infinite); //禁止再次回调
                ChangeStatus(TaskRunStatusType.Paused);
                Log.Debug($"[{this}]暂停，计时器已暂停。(最后一次回调的任务可能还在执行)");
            }
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            lock (this)
            {
                ChangeStatus(TaskRunStatusType.Running);
                var now = SystemTime.Now();
                var nextRun = Task.GetNextRunTime(now);
                if (nextRun == null)
                {
                    Log.Warn($"[{this}] 无法恢复，因为下次执行时间为null");
                    return;
                }
                var dueTime = nextRun.Value.Subtract(now);
                TaskWorker.Change(dueTime/*第一次延时调用*/, TimeSpan.MaxValue); //重启计时器
                Log.Debug($"[{this}] 暂停，计时器已恢复。(最后一次回调的任务可能还在执行)");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop();
                TaskWorker.Dispose();
                TaskWorker = null; //干掉引用
            }
            catch (Exception ex)
            {
                Log.Error("释放资源时发生异常。", ex);
                //throw;
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{Task.Meta.Id}:{Task.Meta.Name}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TaskProviderStatusType
    {
        /// <summary>
        /// 
        /// </summary>
        Working,

        /// <summary>
        /// 要被移除
        /// </summary>
        ToRemove,
    }
}