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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using CS.Diagnostics;

namespace CS.TaskScheduling
{
    /// <summary>
    ///   任务管理器
    ///   专为WindowsService服务
    ///   Task 计划的伤务 Ver:0.3.1
    /// </summary>
    /// 
    /// <description class = "CS.TaskScheduling.Task4WinService">
    ///   
    /// </description>
    /// 
    /// <history>
    ///   2010-3-1 14:55:12 , zhouyu ,  创建
    ///   2010-7-7 14:32  , zhouyu , 加上配置初始化 行230
    ///  </history>
    public sealed class Task4WinService : IDisposable
    {
        #region 相关属性设定

        /// <summary>
        /// 程序运行日志 
        /// </summary>
        private readonly ITracer _log = CS.Diagnostics.Logger.GetSysLog(typeof(Task4WinService));

        /// <summary>
        /// 所有正在运行的任务集合
        /// </summary>
        internal IList<TaskProvider> Tasks { get; private set; }
        
        /// <summary>
        /// 监视工人(轮询工作计时器)
        /// </summary>
        private Timer _watcher;

        /// <summary>
        /// 运行次数
        /// </summary>
        private int _workTimes;

        /// <summary>
        /// 任务配置
        /// </summary>
        internal TaskConfig TaskSetting { get; private set; }


        #endregion


        #region 工作回调

        /// <summary>
        /// 监视轮询工作
        /// 发现有可执行的任务时，启动该任务所在的驱动以使其执行。
        /// Note:Working时就要每次重读配置，释放后的任务再也回不来了。
        /// </summary>
        public void Working(object sender, ElapsedEventArgs args)
        {
            //Note:线程锁定
            lock (this)
            {
                _watcher.Stop();
                _log.Debug($"↓---- 任务监视轮询开始 ----    [下次轮询延时: {_watcher.Interval/1000}秒 ; 当前时间:{DateTime.Now.ToString("HH:mm:ss ffffff")}]");

                TryRemoveTask();

                StartSettingTasks();
               
                _watcher.Interval = ((TimeSpan)TaskSetting.WatchTimer.WorkingInterval).TotalMilliseconds;
                _workTimes++;

                _log.DebugFormat($"↑---- 任务监视轮询结束[Times:{_workTimes}] ----  [当前时间:{DateTime.Now.ToString("HH:mm:ss ffffff")}]");
                _watcher.Start();
            }
        }


        /// <summary>
        /// 启动设置中的任务
        /// </summary>
        private void StartSettingTasks()
        {
            var tasks = TaskSetting.Tasks;
            foreach (var task in tasks.Select(GetTask).Where(task => task!=null))
            {
                Tasks.Add(task);
            }
        }

        /// <summary>
        /// 尝试移除待移除的任务
        /// </summary>
        private void TryRemoveTask()
        {
            for (int i = 0; i < Tasks.Count; i++)
            {
                var task = Tasks[i];
                if (task.Task.Execution.RunStatus != TaskRunStatusType.Removing) continue;
                task.Dispose();
                var val = Tasks.Remove(task);
                _log.Info($"[{task}] 的移除结果：{val}。");
            }
        }


        ///// <param name="config">原始配置</param>
        /// <summary>
        /// 初始化一个任务的驱动实例
        /// <remarks>
        /// 如果任务为空，表示没有可用的任务
        /// </remarks>
        /// </summary>
        /// <param name="taskSetting">任务配置信息</param>
        private TaskProvider GetTask(TaskInfo taskSetting)
        {
            var job = Tasks.FirstOrDefault(x => x.Task.Meta.Id == taskSetting.Meta.Id);
            if (job != null) return null;
            if (!taskSetting.Enable) return null;
            var now = SystemTime.Now();
            //var runTime = taskSetting.GetNextRunTime(now);
            var nextRunInterval = taskSetting.GetNextInterval();
            if (nextRunInterval == null)
            {
                _log.Warn($"○ [{taskSetting}] 下次启动时间为null ，请检查触发的表达式。或者，该任务已经完成设定次数。");
                return null;
            }
            var delayMilSeconds = nextRunInterval.Value.TotalMilliseconds;
            var interval = (TimeSpan) TaskSetting.WatchTimer.WorkingInterval;
            if (delayMilSeconds >  interval.TotalMilliseconds *   3)
            {
                //_log.Warn($"delayMilSeconds:{delayMilSeconds};_watcher.Interval:{interval}");
                _log.Debug($"○ [{taskSetting}] 下次启动时间间隔为:{nextRunInterval}，远小于监视线程间隔，暂不执行。");
                return null;
            }

            //实例化驱动实现
            try
            {
                _log.Debug($"[{taskSetting}] 开始初始化...");
                var typeInfo = taskSetting.Type.Split(',');
                var assembly = Assembly.Load(typeInfo[1].Trim()); //如果错误，这儿会引发异常，下面就不用执行了
                var type = assembly.GetType(typeInfo[0].Trim());
                if (type == null || !typeof (TaskProvider).IsAssignableFrom(type))
                {
                    var msg = $"[{taskSetting}] 的type属性[{taskSetting.Type}]无效，请使用实现了TaskProvider的类，服务中止。";
                    //log.Error(sb);
                    throw new ConfigurationErrorsException(msg);
                }
                var task = assembly.CreateInstance(typeInfo[0]) as TaskProvider;
                if (task != null)
                {
                    task.WorkerInterval = ((TimeSpan) TaskSetting.WatchTimer.WorkingInterval).TotalMilliseconds;
                    task.Task = taskSetting;
                    task.Resources = TaskSetting.Resources; //公有资源引用
                    //task.InitPreExetend();
                    task.InitExtend(); //由于非new方式创建实现，无法在构造中获得配置
                    task.Start();
                    _log.Info($"[{taskSetting}] 实例化成功。");
                }
                else
                {
                    _log.Error($"[{taskSetting}] 实例化为null。");
                }
                return task;
            }
            catch (Exception ex)
            {
                var msg = $"[{taskSetting}] 的type属性[{taskSetting.Type}]无效，实例化异常，该任务将被跳过。";
                _log.Error(msg, ex);
                //throw new ConfigurationErrorsException(msg);
                return null;
            }
        }

        #endregion

        #region 监工工作 开始 暂停 配置变更

        /// <summary>
        /// 开始执行全部。
        /// </summary>
        public void Start()
        {
            //Note:线程锁定
            lock (this)
            {
                _log.DebugFormat("----- 服务启动开始 -----");

                //初始化任务集合
                Tasks = new List<TaskProvider>();

                //载入所有配置
                TaskSetting = TaskConfig.GetInstance();
                TaskSetting.Changed += TaskConfigChanged; //只保留一次的事件

                //Note:初始化计时器
                _watcher = new Timer(TaskSetting.WatchTimer.DelayMillisecond); //Note:第一次运行不按间隔时间执行。
                _watcher.Elapsed += Working;
                _watcher.Start(); //启动工作回调
                _log.Debug($"监视工人第一次工作将于{TimeSpan.FromMilliseconds(_watcher.Interval)}后执行");
                _log.Debug("----- 服务启动完成 -----");
            }
        }

        #region 监督配置事件 tasks.config

        /// <summary>
        /// 发生配置变化后约30秒延迟时间才会全部重启完成。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TaskConfigChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Note:线程锁定
                lock (this)
                {
                    _log.Info($"■■■■■■ 文件{e.Name}发生变化。变化类型:{e.ChangeType} ■■■■■■");
                    _watcher.Stop();

                    //原任务全部移除
                    foreach (var task in Tasks)
                    {
                        task.ChangeStatus(TaskRunStatusType.Removing);
                    }
                    TryRemoveTask();

                    TaskSetting = TaskConfig.GetInstance(true);
                    TaskSetting.Changed += TaskConfigChanged; //只保留一次的事件

                    //Note:初始化计时器
                    //_watcher = new Timer(cfg.WatchTimer.DelayMillisecond); //Note:第一次运行不按间隔时间执行。
                    _watcher.Interval = ((TimeSpan)TaskSetting.WatchTimer.WorkingInterval).TotalMilliseconds;
                    _watcher.Start(); //启动工作回调
                    _log.Info($"■■■■■■ 更新配置后的工作间隔为{TimeSpan.FromMilliseconds(_watcher.Interval)}后 ■■■■■■");
                }
            }
            catch (Exception ex)
            {
                _log.Debug("重载任务配置发生异常", ex);
                throw;
            }
        }

        #endregion

        ///// <summary>
        ///// 移除可能过期的任务
        ///// </summary>
        ///// <param name="item"></param>
        //public void TryRemoveJob(TaskProvider item)
        //{
        //    if (item.Task.Meta.Execution.RunStatus != (TaskRunStatusType.Removing)) return;
        //    item.Dispose();
        //    var rst = Tasks.Remove(item);
        //    _log.Info($"任务[{item.Task.Meta.Id}:{item.Task.Meta.Name}]任务将移除结果：{rst}。");
        //}

        /// <summary>
        /// 开始停止全部。
        /// 并清空集合
        /// TODO:这儿的停止要把所有的任务全部停掉后才能真正的停止,,有待检测。
        /// </summary>
        public void Stop()
        {
            //Note:线程锁定
            lock (this)
            {
                if (_watcher == null) return;
                _log.DebugFormat("----- 服务停止开始 -----");
                _watcher.Stop();
                if (Tasks.Count > 0)
                {
                    //开始通知所有任务，让其终止。
                    foreach (var task in Tasks)
                    {
                        task?.Stop();
                    }
                    Tasks = null;
                }
                //所有任务停止后 释放监工计时器
                _watcher.Dispose();
                _watcher = null; //干掉引用
                //RunStatus = () => ServiceStatusType.Stoped;
                _log.DebugFormat("----- 服务停止完成 -----");
            }
        }

        /// <summary>
        /// 暂停所有任务。
        /// </summary>
        public void Pause()
        {
            //Note:线程锁定
            lock (this)
            {
                _log.DebugFormat("----- 服务暂停开始 -----");
                _watcher.Stop();
                foreach (var task in Tasks)
                {
                    task.Pause();
                }
                //RunStatus = () => ServiceStatusType.Suspend;
                _log.DebugFormat("----- 服务暂停结束 -----");
            }
        }

        /// <summary>
        /// 从暂停处重新执行。
        /// </summary>
        public void Resume()
        {
            //Note:线程锁定
            lock (this)
            {
                _log.DebugFormat("----- 服务恢复开始 -----");
                foreach (var task in Tasks)
                {
                    task.Resume();
                }
                _watcher.Start();
                //RunStatus =() => ServiceStatusType.Runing;
                _log.DebugFormat("----- 服务恢复结束 -----");
            }
        }

        #endregion

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            _log.DebugFormat("----- 服务资源释放开始 -----");

            Stop();

            _log.DebugFormat("----- 服务资源释放结束 -----");
        }

       
    }

    
}