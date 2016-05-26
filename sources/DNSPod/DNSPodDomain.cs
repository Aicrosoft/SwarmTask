using System;

namespace DNSPod
{
    public class DnsPodDomain : DnsPodBase, IDnsPod
    {
        public int Create(object paramObject)
        {
            dynamic result = PostApiRequest("Domain.Create", paramObject);
            if (result != null && Convert.ToInt32(result.status.code) == 1)
            {
                return Convert.ToInt32(result.domain.id);
            }
            return 0;
        }

        public dynamic List(object paramObject)
        {
            return PostApiRequest("Domain.List", paramObject);
        }

        /// <summary>
        /// 根据指定域名名称获取域名ID,若不存在,则返回0
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        public int GetDomainIdByKeywords(string domainName)
        {
            dynamic result = this.List(new { keyword = domainName });
            return Convert.ToInt32(result.status.code) == 1 ? Convert.ToInt32(result.domains[0].id) : 0;
        }

        public bool Remove(string domain)
        {
            return Remove(new { domain = domain });
        }

        public bool Remove(int domainId)
        {
            return Remove(new { domain_id = domainId });
        }

        public bool Remove(object paramObject)
        {
            dynamic result = PostApiRequest("Domain.remove", paramObject);
            if (result != null && Convert.ToInt32(result.status.code) == 1)
            {
                return true;
            }
            return false;
        }

        public dynamic Info(object paramObject)
        {
            throw new NotImplementedException();
        }
    }
}
