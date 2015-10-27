using log4net;

namespace CS.Diagnostics
{
    /// <summary>
    /// 应用日志
    /// <remarks>
    /// 应用日志，用于记录业务相关的日志
    /// </remarks>
    /// </summary>
    public class AppLog:Logger
    {
        public AppLog()
        {
            Log = LogManager.GetLogger("App");
        }
    }
}