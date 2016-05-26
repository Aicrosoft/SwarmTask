using System.Linq;
using System.Configuration;
using System.Reflection;
using System;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace DNSPod
{
    public class DnsPodBase
    {
        private DnsPodConfiguration _dnsPodInfo = DnsPodConfiguration.GetConfiguration();

        /// <summary>
        /// 以POST方式请求DNSPod API
        /// </summary>
        /// <param name="address">API地址,不包含域名,如:(https://dnsapi.cn/Info.Version)中的Info.Version</param>
        /// <param name="param">API所需的参数,不包含公共参数部分</param>
        /// <returns></returns>
        protected dynamic PostApiRequest(string address, object param = null)
        {
            dynamic obj = null;
            using (var client = new HttpClient())
            {
                string apiUrl = _dnsPodInfo.Host + address;
                #region 封装公众参数
                object commonParam = null;
                //公共参数
                if (!string.IsNullOrWhiteSpace(_dnsPodInfo.Token))
                {
                    //若Token不为空，使用token验证方式
                    commonParam = new
                    {
                        login_token = _dnsPodInfo.Token,
                        format = _dnsPodInfo.Format,
                        lang = _dnsPodInfo.Lang,
                        error_on_empty = _dnsPodInfo.ErrorOnEmpty
                    };
                }
                else
                {
                    //Token为空，使用邮箱+密码验证方式
                    commonParam = new
                    {
                        login_email = _dnsPodInfo.Email,
                        login_password = _dnsPodInfo.Password,
                        format = _dnsPodInfo.Format,
                        lang = _dnsPodInfo.Lang,
                        error_on_empty = _dnsPodInfo.ErrorOnEmpty
                    };
                }
                #endregion

                var postParameters = GetApiParamDataString(commonParam);

                //若有额外参数，则将公共参数添加至参数请求中
                if (param != null)
                {
                    postParameters = string.Format("{0}&{1}", postParameters, GetApiParamDataString(param));
                }

                //client.DefaultRequestHeaders.Add("content", "text/html; charset=UTF-8");
                //设置UserAgent，参考API开发规范：https://www.dnspod.cn/docs/info.html#specification
                client.DefaultRequestHeaders.Add("User-Agent", _dnsPodInfo.UserAgent);

                ////因目前DNSPod API不支持识别json字符串，暂时注释掉
                //HttpContent content = new StringContent(JsonConvert.SerializeObject(param), Encoding.UTF8, "application/json");

                HttpContent content = new StringContent(postParameters, Encoding.UTF8, "application/x-www-form-urlencoded");
                var result = client.PostAsync(apiUrl, content).Result;

                if (result.IsSuccessStatusCode)
                {
                    var resultContent = result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(resultContent))
                    {
                        obj = JObject.Parse(resultContent);
                    }
                }
            }
            return obj ?? new { status = new { code = 0, message = "DNSPod服务器接口异常，无法正常调用！" } };
        }

        /// <summary>
        /// 将API参数以字符串形式返回，参数之间以"&amp;"操作符连接
        /// </summary>
        /// <param name="data">API参数对象</param>
        /// <returns>参数字符串</returns>
        private static string GetApiParamDataString(object data)
        {
            var dataString = string.Empty;
            if (data == null) return dataString;

            //利用反复取可读属性或实例，返回属性集合
            var objProperties = (from x in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) where x.CanRead select x).ToList();

            //将属性名与值拼接，返回string类型的数组
            var arrProValue = (from y in objProperties select string.Format("{0}={1}", y.Name, y.GetValue(data, new object[0]))).ToArray();

            //将数组以“&”连接
            dataString = string.Join("&", arrProValue);
            return dataString;
        }
    }

}
