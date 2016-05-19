using System;
using log4net;

namespace CS.Diagnostics
{
    /// <summary>
    /// 系统日志
    /// <remarks>
    /// 输出仅关于系统和程序诊断的相关日志
    /// </remarks>
    /// </summary>
    public class SysLog:Logger
    {
        public SysLog(Type type)
        {
            Log = LogManager.GetLogger(type);
        }
    }
}