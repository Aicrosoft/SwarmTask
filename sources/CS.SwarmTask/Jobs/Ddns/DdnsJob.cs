using System;
using System.Diagnostics;
using CS.TaskScheduling;
using DNSPod;

namespace CS.SwarmTask.Jobs.Ddns
{
    /// <summary>
    /// DNSPod的DDNS任务
    /// </summary>
    public class DdnsJob : TaskProvider
    {
        private static string oldIp = "";

        private int _ups = 0;

        protected override TaskResult Work()
        {
            var rst = new TaskResult();
            var helper = new IpHelper(oldIp);
            var ip = helper.GetIp();
            if (helper.IpChanged)
            {
                var sw = new Stopwatch();
                sw.Start();
                var ok = BatchVast(ip);
                sw.Stop();
                if (ok)
                {
                    rst.Result = TaskResultType.Succeed;
                    rst.Message = $"已将所有泛解析更新了IP地址指向[effecteds:{_ups},change: <{oldIp}> -> <{ip}>][elapsed:{sw.Elapsed}]";
                    oldIp = ip;
                }
                else
                {
                    rst.Result = TaskResultType.Failed;
                    rst.Message = $"更新IP地址失败[change: <{oldIp}> -> <{ip}>][elapsed:{sw.Elapsed}]";
                    Log.Warn($"更新地址失败[change: <{oldIp}> -> <{ip}>][elapsed:{sw.Elapsed}]");
                }
            }
            else
            {
                rst.Result = TaskResultType.Unknow;
                rst.Message = "IP地址未变，不用更新。";
            }
            return rst;
        }



        /// <summary>
        /// 批更新 * 的泛解析
        /// 返回为true时表示全部更新成功，否则就是还有没有更新的
        /// </summary>
        /// <param name="ip">新的IP地址</param>
        /// <param name="autoCreate">当 泛解板不存在时是否自动创建</param>
        private bool BatchVast(string ip, bool autoCreate = false)
        {
            //var ip = "127.1.1.1";
            var subDomain = "*";
            //var isAutoCreate = true; //如果没有则创建
            var api = new DnsPodDomain();
            dynamic result = api.List(new { keyword = "" });//此处填写您域名的名称,为确保准确定位,请填写完整的域名,如:baidu.com
            //Console.WriteLine(result);
            if (result.status.code != 1)
            {
                Log.Warn($"查询域名列表时异常:[{result}]");
                return false;
            }
            var domains = result.domains;
            var rst = true; //更新结果为真
            foreach (dynamic domain in domains)
            {
                var domainId = 0;
                domainId = domain.id;
                var record = new DnsPodRecord();
                var rdRst = record.List(new
                {
                    domain_id = domainId,
                    sub_domain = subDomain
                });
                var recordId = 0;
                if (rdRst.info.record_total == 0)
                {
                    //未有记录创建
                    if (autoCreate)
                    {
                        recordId = record.Create(new
                        {
                            domain_id = domainId,
                            sub_domain = subDomain,
                            record_type = "A",
                            record_line = "默认",
                            value = ip,
                            //ttl = 300
                        });
                        Log.Info($"创建了新的泛解析{subDomain}.{domain.name},recordId={recordId}");
                    }
                }
                else
                {
                    _ups = 0;
                    foreach (dynamic rd in rdRst.records)
                    {
                        recordId = rd.id;
                        if (rd.name == subDomain && rd.value != ip)
                        {
                            var dyo = record.UpdateDdns(domainId, recordId, subDomain, ip);
                            var val = Convert.ToInt32(dyo.status.code) == 1;
                            rst = rst && val;
                            if (val)
                            {
                                _ups++;
                            }
                            else
                            {
                                Log.Warn($"更新*.{domain.name}时失败，返回的消息是:[{dyo}]");
                            }
                        }
                        //Console.WriteLine($"did={domainId}    rid={recordId}  subDomain={subDomain}   ip={ip}");
                    }
                }
            }
            return rst;
        }

    }
}