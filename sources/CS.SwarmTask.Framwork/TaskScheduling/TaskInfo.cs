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
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CS.Diagnostics;
using CS.Serialization;

namespace CS.TaskScheduling
{
    /// <summary>
    ///   单一任务配置信息
    /// </summary>
    /// <history>
    ///   2010/4/4 17:45:21 , atwind ,  创建	  
    ///   2015-10-23 ,atwind,重构   
    ///  </history>
    [Serializable, XmlRoot(ElementName = "task")]
    public class TaskInfo
    {
        /// <summary>
        /// 默认构造
        /// </summary>
        public TaskInfo()
        {
            Meta = new MetaInfo {Id = Guid.NewGuid().ToString()};
            WorkSetting = new WorkSettingInfo
            {
                Times = 0,
                ErrorWay = ErrorWayType.Default,
                SleepInterval = new TimeSpan(1, 1, 1, 1),
            };
            TimeTrigger = "0 0-10 * * * ? ";//默认每10分钟的0秒开始执行
            Extend = new object();
        }

        /// <summary>
        /// 任务是否启用
        /// </summary>
        [XmlAttribute("enable")]
        public bool Enable { get; set; }

        ///// <summary>
        ///// 任务执行次数
        ///// <remarks>
        ///// -1或者大于0,立即实例化TaskProvider，执行时间由TaskProvider自已控制，正确执行大于0的次数后停止（并将移除执行队列），以后再也不会运行
        ///// 0,根据<see cref="TimeTrigger"/>来执行，任务触发由线程监视器启动
        ///// </remarks>
        ///// </summary>
        //[XmlAttribute("times")]
        //public int Times { get; set; }

        /// <summary>
        /// 触发时间表达式
        /// Note: 工作时间触发器:只负责启动触发，执行次数不需设在工作触发器里（即符合该表达式的时间内进行有效）：使用Quartz.net的Cron表达式
        /// </summary>
        [XmlAttribute("timeTrigger")]
        public string TimeTrigger { get; set; }
        
        /// <summary>
        /// 该任务反射Type
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// 任务元 ，基本信息
        /// </summary>
        [XmlElement(ElementName = "meta")]
        public MetaInfo Meta { get; set; }

        /// <summary>
        /// 本任务运行情况
        /// </summary>
        public ExecutionInfo Execution { get; set; }

        /// <summary>
        /// 工作执行时间遇错等相关设定
        /// </summary>
        [XmlElement("workSetting")]
        public WorkSettingInfo WorkSetting { get; set; }


        #region 自定义的扩展配置 GetExtend

        /// <summary>
        /// 结合该任务的扩展配置类，内部使用，请使用GetExtend获取该扩展实例。
        /// </summary>
        [XmlElement("extend")]
        public object Extend { get; set; }

        /// <summary>
        /// 获取已设定的扩展类型实例
        /// </summary>
        /// <typeparam name="T">扩展的类型</typeparam>
        /// <returns>扩展类实例</returns>
        public T GetExtend<T>() where T : class
        {
            return XmlSerializor.Deserialize<T>(ExtendRawXml);
        }

        /// <summary>
        /// Extend扩展的Xml片断
        /// </summary>
        /// <returns></returns>
        protected string ExtendRawXml
        {
            get
            {
                var nodes = Extend as XmlNode[];
                if (nodes == null || nodes.Length == 0)
                    return "<extend />";
                using (var w = new StringWriter())
                {
                    using (XmlWriter writer = new XmlTextWriter(w))
                    {
                        writer.WriteStartElement("extend");
                        foreach (var node in nodes)
                            writer.WriteRaw(node.OuterXml);

                        writer.WriteEndElement();
                        writer.Close();
                    }
                    return w.ToString();
                }
            }
        }

        #endregion

        #region 方法 GetNextLaunchTime

        /// <summary>
        /// 计算任务(单个)的启动时间[当前计算机所处时区]
        /// </summary>
        /// <returns></returns>
        public DateTime? GetNextRunTime(DateTime? lastRunTime=null)
        {
            if (Execution.RunTimes >= WorkSetting.Times && WorkSetting.Times > 0) return null;
            if (lastRunTime == null) lastRunTime = SystemTime.Now();
            var cronExp = new CronExpression(TimeTrigger);
            var runTime = cronExp.GetNextValidTimeAfter(lastRunTime.Value.ToUniversalTime());
            return runTime?.ToLocalTime();
        }

        /// <summary>
        /// 下次启动的时间间隔
        /// </summary>
        /// <param name="lastRunTime"></param>
        /// <returns></returns>
        public TimeSpan? GetNextInterval(DateTime? lastRunTime = null)
        {
            if (Execution.RunTimes >= WorkSetting.Times && WorkSetting.Times > 0) return null;
            if (lastRunTime == null) lastRunTime = SystemTime.Now();
            var cronExp = new CronExpression(TimeTrigger);
            var runTime = cronExp.GetNextValidTimeAfter(lastRunTime.Value.ToUniversalTime());
            return runTime?.ToLocalTime().Subtract(lastRunTime.Value.ToLocalTime());
        }

        #endregion

        /// <summary>
        /// 重写ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Meta.Id}:{Meta.Name}";
        }
    }


    #region 任务元数据相关的基本信息 MetaInfo

    /// <summary>
    /// 任务属性
    /// </summary>
    [Serializable]
    public class MetaInfo
    {
        /// <summary>
        /// 任务Id，必须有
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// 任务是否要自动补全
        /// </summary>
        [XmlAttribute("isPatch")]
        public bool IsPatch { get; set; }

        /// <summary>
        /// 任务Hash，根据Type来
        /// </summary>
        [XmlIgnore]
        public int TaskHash { get; set; }

        ///// <summary>
        ///// 本任务运行情况
        ///// </summary>
        //public ExecutionInfo Execution { get; set; }
    }

    #endregion

    #region 任务运行状态 ExecutionInfo

    /// <summary>
    /// 任务运行状态
    /// </summary>
    [Serializable]
    public class ExecutionInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public ExecutionInfo()
        {
        }

        /// <summary>
        /// 任务配置是否还存在
        /// </summary>
        [XmlIgnore]
        public bool IsExsit { get; set; }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime? LastRun { get; set; }

        /// <summary>
        /// 最后成功运行的时间 
        /// 新任务时为null , 只有补全功能需要时才使用。每次成功运行后更新该值。
        /// Note:主要目的是用于补全时使用的
        /// </summary>
        public DateTime? LastSucceedRun { get; set; }

        /// <summary>
        /// 正确运行的次数
        /// </summary>
        [XmlAttribute]
        public int RunTimes { get; set; }

        /// <summary>
        /// 错误运行时休眠的次数
        /// </summary>
        [XmlAttribute]
        public int SleepTimes { get; set; }

        /// <summary>
        /// 任务状态
        /// Note:状态是给人看的，不是判断任务是否执行的依据。
        /// </summary>
        [XmlAttribute]
        public TaskRunStatusType RunStatus { get; set; }

        ///// <summary>
        ///// 任务执行的时间
        ///// <remarks>
        ///// 如果调用时间过长时可能判断为出了问题
        ///// </remarks>
        ///// </summary>
        //[XmlIgnore]
        //public Stopwatch WorkingWatch { get; set; }


        ////Todo:太诡异了，这两个特性竟然不起作用。
        //[OnDeserialized]
        //internal void OnDeserialized(StreamingContext context)
        //{
        //    log.DebugFormat("反序列化结束... :{0}", RunStatus);
        //  //  RunStatus = TaskRunStatusType.Default;
        //}

        //[OnDeserializing]
        //internal void OnDeserializing(StreamingContext context)
        //{

        //    Console.Write(string.Format("反序列化开始... :{0}", context.State));
        //    //RunStatus = TaskRunStatusType.Default;
        //}

        //private log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExecutionInfo));
    }

    #endregion

    #region 任务运行范围 WorkSettings

    /// <summary>
    /// 该任务所有时间运行执行范围
    /// </summary>
    [Serializable]
    public class WorkSettingInfo
    {
        ///// <summary>
        ///// 需要时的任务延时，秒
        ///// </summary>
        //[XmlAttribute("delaySecond")]
        //public int DelaySecond { get; set; }

        /// <summary>
        /// 任务超时时间，秒
        /// </summary>
        [XmlAttribute("timeout")]
        public int Timeout { get; set; }

        [XmlAttribute("times")]
        public int Times { get; set; }

        /// <summary>
        /// 发生错误时的后续操作
        /// </summary>
        [XmlAttribute("whenErrorHappened")]
        public ErrorWayType ErrorWay { get; set; }

        /// <summary>
        /// 错误时的运行时休眠的间隔
        /// </summary>
        [XmlElement("sleepInterval")]
        public WorkTimeSpan SleepInterval { get; set; }
    }

    #endregion



    #region 任务集合 TaskCollection

    /// <summary>
    /// 任务配置集合，只读
    /// </summary>
    [Serializable]
    public class TaskCollection : List<TaskInfo>
    {
        /// <summary>
        /// 按工作Id的索引
        /// </summary>
        /// <param name="taskId">任务Id</param>
        /// <returns></returns>
        public TaskInfo this[string taskId]
        {
            get { return Find(x => x.Meta.Id == taskId); }
        }
    }

    #endregion

    #region 任务运行状态集合 TaskExecutionCollection

    /// <summary>
    /// 任务运行状态集合
    /// </summary>
    [Serializable]
    public class TaskExecutionCollection : List<MetaInfo>
    {
        /// <summary>
        /// 按工作Id索引返回执行信息
        /// </summary>
        /// <param name="taskId">任务Id</param>
        /// <returns>工作信息</returns>
        public MetaInfo this[string taskId]
        {
            get
            {
                var task = Find(x => x.Id == taskId);
                return task;
            }
            set
            {
                var task = Find(y => y.Id == taskId);
                task = value;
            }
        }
    }

    #endregion

    #region 可序列化的TimeSpan WorkTimeSpan

    /// <summary>
    /// 可以序列化的MyTimeSpan
    /// TimeSpan 不能继承
    /// </summary>
    public class WorkTimeSpan
    {
        private TimeSpan _timeSpan;
        public WorkTimeSpan()
        {
        }

        public WorkTimeSpan(TimeSpan time)
        {
            _timeSpan = time;
        }

        /// <summary>
        /// 执行次数
        /// </summary>
        [XmlAttribute("times")]
        public int Times { get; set; }

        /// <summary>
        /// 仅供序列化使用，不要直接读取该值。
        /// </summary>
        [XmlText]
        public string XmlText
        {
            get { return _timeSpan.ToString(); }
            set { _timeSpan = TimeSpan.Parse(value); }
        }

        /// <summary>
        /// 隐式转换 MyTimeSpan -> TimeSpan
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator TimeSpan(WorkTimeSpan t)
        {
            return t._timeSpan;
        }

        /// <summary>
        /// 隐式转换 TimeSpan -> MyTimeSpan
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator WorkTimeSpan(TimeSpan t)
        {
            return new WorkTimeSpan(t);
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _timeSpan.ToString();
        }
    }

    #endregion

}