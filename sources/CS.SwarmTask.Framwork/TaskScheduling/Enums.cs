using System;

namespace CS.TaskScheduling
{

    #region 错误发生时后续操作枚举 ErrorWayType

    /// <summary>
    /// 错误发生时后续操作枚举
    /// </summary>
    public enum ErrorWayType
    {
        /// <summary>
        /// 默认，一直执行下去，直到执行成功完成任务。
        /// </summary>
        Default = 0,

        /// <summary>
        /// 休眠后执行，直到失败次数执行完成后停止，如果失败次数为设定值后停止
        /// </summary>
        TryAndStop = 1,

        /// <summary>
        /// 停止执行
        /// </summary>
        Stop = 2,
    }

    #endregion


    #region 任务执行状态 TaskRunStatusType

    /// <summary>
    /// 任务运行行状态
    /// </summary>
    public enum TaskRunStatusType
    {
        /// <summary>
        /// 默认，需要计算后判断是否要执行
        /// </summary>
        Default = 0,

        /// <summary>
        /// 开始运行
        /// </summary>
        Running,
        /// <summary>
        /// 开始执行任务
        /// </summary>
        Working,

        /// <summary>
        /// 本次任务执行完成
        /// </summary>
        Worked,

        /// <summary>
        /// 正在以错误状态运行休眠中
        /// </summary>
        Sleeping ,

        /// <summary>
        /// 正在暂停
        /// </summary>
        Pausing ,

        /// <summary>
        /// 任务被暂停
        /// </summary>
        Paused ,

        /// <summary>
        /// 正在停止
        /// </summary>
        Stoping ,

        /// <summary>
        /// 任务停止了
        /// </summary>
        Stoped ,

        /// <summary>
        /// 该任务需要被移除，比如：配置变更，其新的同样任务已经加入
        /// </summary>
        Removing ,
    }

    #endregion


    ///// <summary>
    ///// 任务服务状态
    ///// </summary>
    //public enum ServiceStatusType
    //{
    //    /// <summary>
    //    /// 未知
    //    /// </summary>
    //    Unknow = 0,

    //    /// <summary>
    //    /// 已停止
    //    /// </summary>
    //    Stoped = 1,

    //    /// <summary>
    //    /// 运行中
    //    /// </summary>
    //    Runing = 2,

    //    /// <summary>
    //    /// 暂停中
    //    /// </summary>
    //    Suspend = 3,

    //}
}