using System;
using System.Xml.Serialization;
using CS.Attribute;

namespace CS.TaskScheduling
{
    /// <summary>
    /// 任务执行结果
    /// </summary>
    public class TaskResult
    {
        /// <summary>
        /// 运行结果枚举
        /// </summary>
        public TaskResultType Result { get; set; }

        /// <summary>
        /// 任务返回的消息
        /// </summary>
        public string Message { get; set; }

        [XmlIgnore]
        public object ExtendMessage { get; set; }
    }

    /// <summary>
    /// 运行结果的枚举
    /// (注意:不能正负混用)
    /// <para>这个关系到任务的运行结果</para>
    /// </summary>
    [Flags]
    public enum TaskResultType : uint
    {
        /// <summary>
        /// 未知，本次执行结果将被忽略
        /// </summary>
        Unknow = 0,

        /// <summary>
        /// 执行成功，全部成功后(Finnished)才会更新最后成功执行日期
        /// </summary>
        Succeed = 1,

        /// <summary>
        /// 执行失败
        /// </summary>
        Failed = 2,

        #region 用于任务被全的枚举结果

        /// <summary>
        /// 今天运行完成
        /// </summary>
        TodayComplete = 256,

        /// <summary>
        /// 今天未正常运行完成，如果要补全的任务的话就要启用补全功能了
        /// </summary>
        TodayNotComplete = 512,

        #endregion

        /// <summary>
        /// 异常或错误
        /// <remarks>
        /// 当含有该信息结果时系统将进行停止，休眠，通知等相关操作
        /// </remarks>
        /// </summary>
        Error = 536870912,

        /// <summary>
        /// 任务完成，再也不用执行了， [超出运行设定：如 返回的结果给出了明确的结果 或 正确执行了设定的次数，或是时间触发器已过期]
        /// </summary>
        Finished = 1073741824,
    }

}