using System;

namespace DNSPod
{
    public class DnsPodRecord : DnsPodBase, IDnsPod
    {
        /// <summary>
        /// 创建记录,默认记录类型为A
        /// </summary>
        /// <param name="domainId">域名ID</param>
        /// <param name="subDomain">二级域名名称</param>
        /// <param name="recordValue">记录值</param>
        /// <returns>记录ID</returns>
        public int Create(int domainId, string subDomain, string recordValue)
        {
            return Create(domainId, subDomain, recordValue, "A", "默认");
        }

        /// <summary>
        /// 创建记录,默认记录类型为A
        /// </summary>
        /// <param name="domainId">域名ID</param>
        /// <param name="subDomain">二级域名名称</param>
        /// <param name="recordValue">记录值</param>
        /// <param name="recordType">记录类型，通过API记录类型获得，大写英文，比如：A</param>
        /// <param name="recordLine">记录线路，通过API记录线路获得，中文，比如：默认</param>
        /// <returns>记录ID</returns>
        public int Create(int domainId, string subDomain, string recordValue, string recordType, string recordLine)
        {
            var recordId = 0;
            object p = new
            {
                domain_id = domainId,
                sub_domain = subDomain,
                record_type = recordType,
                record_line = recordLine,
                value = recordValue
            };
            recordId = Create(p);
            return recordId;
        }

        /// <summary>
        /// 创建记录
        /// domain_id 域名ID, 必选
        /// sub_domain 主机记录, 如 www, 默认@，可选
        /// record_type 记录类型，通过API记录类型获得，大写英文，比如：A, 必选
        /// record_line 记录线路，通过API记录线路获得，中文，比如：默认, 必选
        /// value 记录值, 如 IP:200.200.200.200, CNAME: cname.dnspod.com., MX: mail.dnspod.com., 必选
        /// mx {1-20} MX优先级, 当记录类型是 MX 时有效，范围1-20, MX记录必选
        /// ttl {1-604800} TTL，范围1-604800，不同等级域名最小值不同, 可选
        /// </summary>
        /// <param name="paramObject"></param>
        /// <returns></returns>
        public int Create(object paramObject)
        {
            dynamic result = PostApiRequest("Record.Create", paramObject);
            return Convert.ToInt32(result.status.code) == 1 ? Convert.ToInt32(result.record.id) : -1;
        }

        /// <summary>
        /// 记录列表
        /// </summary>
        /// <param name="domainId">域名ID</param>
        /// <returns></returns>
        public dynamic List(int domainId)
        {
            return List(new { domain_id = domainId });
        }

        public dynamic List(object paramObject)
        {
            return PostApiRequest("Record.List", paramObject);
        }

        public bool Remove(object paramObject)
        {
            dynamic result = PostApiRequest("Record.Remove", paramObject);
            return Convert.ToInt32(result.status.code) == 1;
        }

        public bool Remove(int domainId, int recordId)
        {
            return Remove(new { domain_id = domainId, record_id = recordId });
        }

        public dynamic Info(int domainId, int recordId)
        {
            return Info(new
            {
                domain_id = domainId,
                record_id = recordId
            });
        }

        /// <summary>
        /// 获取记录信息
        /// </summary>
        /// <param name="paramObject"></param>
        /// <returns></returns>
        public dynamic Info(object paramObject)
        {
            return PostApiRequest("Record.Info", paramObject);
        }

        public bool Remark(int domainId, int recordId, string subDomainremark)
        {
            dynamic result = PostApiRequest("Record.Remark", new { });
            return Convert.ToInt32(result.status.code) == 1;
        }

        public bool Modify(object paramObject)
        {
            dynamic result = PostApiRequest("Record.Modify", paramObject);
            return Convert.ToInt32(result.status.code) == 1;
        }

        /// <summary>
        /// 更新DDNS解析，返回完全结果
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="recordId"></param>
        /// <param name="subDomain"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public dynamic UpdateDdns(int domainId, int recordId, string subDomain, string value)
        {
            dynamic result = PostApiRequest("Record.Ddns", new
            {
                domain_id = domainId,
                record_id = recordId,
                sub_domain = subDomain,
                record_line = "默认",
                value
            });
            return result;
        }

        private bool Ddns(object paramObject)
        {
            dynamic result = PostApiRequest("Record.Ddns", paramObject);
            //Console.WriteLine(result);
            return Convert.ToInt32(result.status.code) == 1;
        }

        public bool Ddns(int domainId, int recordId, string subDomain, string value)
        {
            return Ddns(domainId, recordId, subDomain, "默认", value);
        }

        public bool Ddns(int domainId, int recordId, string subDomain, string recordLine, string value)
        {
            return Ddns(new
            {
                domain_id = domainId,
                record_id = recordId,
                sub_domain = subDomain,
                record_line = recordLine,
                value
            });
        }
    }
}