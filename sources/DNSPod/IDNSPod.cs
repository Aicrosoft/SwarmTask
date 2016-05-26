
namespace DNSPod
{
    interface IDnsPod
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="paramObject"></param>
        /// <returns></returns>
        int Create(object paramObject);

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="paramObject"></param>
        /// <returns></returns>
        dynamic List(object paramObject);

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="paramObject"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        bool Remove(object paramObject);

        dynamic Info(object paramObject);
    }
}
