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
using System.Xml;

namespace CS.TaskScheduling
{
    /// <summary>
    ///   任务执行必须的上下文
    /// </summary>
    /// 
    /// <description class = "CS.WinService.TaskContext">
    ///  Note: 暂时没有用上，下一步改造用？
    /// </description>
    /// 
    /// <history>
    ///   2010-3-9 13:02:26 , zhouyu ,  创建	     
    ///  </history>
    public class TaskContext
    {
        /// <summary>
        /// 任务友好名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 任务开始时间，null时立即开始
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间，null时永不结束
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 执行延时[毫秒]，防止启动后立即执行。
        /// </summary>
        public int DelaySecond { get; set; }

        /// <summary>
        /// 上次任务结束至下次任务开始的间隔
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// 每天要运行的成功次数
        /// </summary>
        public int RunTimesPerDay { get; set; }


        /// <summary>
        /// 工作时间区间-开始
        /// </summary>
        public TimeSpan WorkAreaBegin { get; set; }
        /// <summary>
        /// 工作时间区间-结束
        /// </summary>
        public TimeSpan WorkAreaEnd { get; set; }


        //Todo:?还有一个周几的特定日期运行，暂没有实现


        /// <summary>
        /// 运行状态节点
        /// </summary>
        public XmlNode Execution { get; set; }

        /// <summary>
        /// 扩展的配置，供本任务使用
        /// </summary>
        public XmlNode Extend { get; set; }



    }
}
