using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSPod
{
    /// <summary>
    /// 批量操作相关
    /// </summary>
    public class DNSPodBatch : DnsPodBase
    {
        /// <summary>
        /// 批量添加域名
        /// </summary>
        /// <param name="domains">域名数组</param>
        /// <param name="recordValue">为每个域名添加 @ 和 www 的 A 记录值，记录值为IP，可选，如果不传此参数或者传空，将只添加域名，不添加记录</param>
        /// <returns></returns>
        public dynamic DomainCreate(string[] domains, string recordValue = "")
        {
            return PostApiRequest("Batch.Domain.Create", new { domains = string.Join(",", domains), record_value = recordValue });
        }

        /// <summary>
        /// 批量添加记录
        /// </summary>
        /// <param name="domains">域名数组</param>
        /// <param name="recordValue">待批量添加的记录详情，JSON 字符串</param>
        /// <returns></returns>
        public dynamic RecordCreate(string[] domainIds, object records)
        {
            return PostApiRequest("Batch.Record.Create", new { domain_id = string.Join(",", domainIds), records = JsonConvert.SerializeObject(records) });
        }

        /// <summary>
        /// 批量修改记录
        /// </summary>
        /// <param name="paramObject"></param>
        /// <returns></returns>
        public dynamic RecordModify(object paramObject)
        {
            return PostApiRequest("Batch.Record.Modify", paramObject);
        }

        /// <summary>
        /// 获取任务详情
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <returns></returns>
        public dynamic Detail(int jobId)
        {
            return PostApiRequest("Batch.Detail", new { job_id = jobId });
        }
    }
}
