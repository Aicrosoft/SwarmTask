using System;
using System.Configuration;

namespace DNSPod
{
    /// <summary>
    /// 拦截配置文件中dnspod节点的配置信息
    /// </summary>
    public class DnsPodConfiguration : ConfigurationSection
    {
        public static DnsPodConfiguration GetConfiguration()
        {
            DnsPodConfiguration configuration = ConfigurationManager.GetSection("dnspod") as DnsPodConfiguration;
            if (configuration != null)
                return configuration;
            return new DnsPodConfiguration();
        }

        /// <summary>
        /// API主机域名
        /// </summary>
        [ConfigurationProperty("host", DefaultValue = "https://dnsapi.cn/")]
        public string Host
        {
            get
            {
                var value = (string)this["host"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.Host.Value))
                {
                    value = Root.Host.Value;
                }
                if (value != null && !value.EndsWith("/"))
                {
                    value += "/";
                }
                return value;
            }
            set
            {
                this["host"] = value;
            }
        }


        /// <summary>
        /// Token
        /// </summary>
        [ConfigurationProperty("token")]
        public string Token
        {
            get
            {
                var value = (string)this["token"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.Token.Value))
                {
                    return Root.Token.Value;
                }
                return value;
            }
            set
            {
                this["token"] = value;
            }
        }

        /// <summary>
        /// 用户帐号
        /// </summary>
        [ConfigurationProperty("email")]
        public string Email
        {
            get
            {
                var value = (string)this["email"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.Email.Value))
                {
                    return Root.Email.Value;
                }
                return value;
            }
            set
            {
                this["email"] = value;
            }
        }

        /// <summary>
        /// 用户密码
        /// </summary>
        [ConfigurationProperty("password")]
        public String Password
        {
            get
            {
                var value = (string)this["password"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.Password.Value))
                {
                    return Root.Password.Value;
                }
                return value;
            }
            set
            {
                this["password"] = value;
            }
        }

        /// <summary>
        /// 返回的数据格式，可选，默认为xml，建议用json
        /// </summary>
        [ConfigurationProperty("format", DefaultValue = "json")]
        public String Format
        {
            get
            {
                var value = (string)this["format"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.Format.Value))
                {
                    return Root.Format.Value;
                }
                return value;
            }
            set
            {
                this["format"] = value;
            }
        }

        /// <summary>
        /// 返回的错误语言，可选，默认为en，建议用cn
        /// </summary>
        [ConfigurationProperty("lang", DefaultValue = "cn")]
        public String Lang
        {
            get
            {
                var value = (string)this["lang"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.Lang.Value))
                {
                    return Root.Lang.Value;
                }
                return value;
            }
            set
            {
                this["lang"] = value;
            }
        }

        /// <summary>
        /// 没有数据时是否返回错误，可选，默认为yes，建议用no
        /// </summary>
        [ConfigurationProperty("errorOnEmpty", DefaultValue = "no")]
        public String ErrorOnEmpty
        {
            get
            {
                var value = (string)this["errorOnEmpty"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.ErrorOnEmpty.Value))
                {
                    return Root.ErrorOnEmpty.Value;
                }
                return value;
            }
            set
            {
                this["errorOnEmpty"] = value;
            }
        }

        [ConfigurationProperty("userAgent", DefaultValue = "DNSPodForNET/V1.0 (zhengwei@zwsdk.cn)")]
        public String UserAgent
        {

            get
            {
                var value = (string)this["userAgent"];
                if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Root.UserAgent.Value))
                {
                    return Root.UserAgent.Value;
                }
                return value;
            }
            set
            {
                this["userAgent"] = value;
            }
        }

        [ConfigurationProperty("root")]
        private DnsPodRoot Root
        {
            get
            {
                return (DnsPodRoot)this["root"];
            }
            set { this["root"] = value; }
        }
    }

    public class DnsPodRoot : ConfigurationElement
    {
        [ConfigurationProperty("host")]
        public DnsPodHost Host
        {
            get
            {
                return (DnsPodHost)this["host"];
            }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("token")]
        public DnsPodToken Token
        {
            get
            {
                return (DnsPodToken)this["token"];
            }
            set { this["token"] = value; }
        }

        [ConfigurationProperty("email")]
        public DnsPodEmail Email
        {
            get
            {
                return (DnsPodEmail)this["email"];
            }
            set { this["email"] = value; }
        }

        [ConfigurationProperty("password")]
        public DnsPodPassword Password
        {
            get { return (DnsPodPassword)this["password"]; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("format")]
        public DnsPodFormat Format
        {
            get { return (DnsPodFormat)this["format"]; }
            set { this["format"] = value; }
        }

        [ConfigurationProperty("lang")]
        public DnsPodLang Lang
        {
            get { return (DnsPodLang)this["lang"]; }
            set { this["lang"] = value; }
        }

        [ConfigurationProperty("errorOnEmpty")]
        public DnsPodErrorOnEmpty ErrorOnEmpty
        {
            get { return (DnsPodErrorOnEmpty)this["errorOnEmpty"]; }
            set { this["errorOnEmpty"] = value; }
        }

        [ConfigurationProperty("userAgent")]
        public UserAgent UserAgent
        {
            get { return (UserAgent)this["userAgent"]; }
            set { this["userAgent"] = value; }
        }

    }

    public abstract class DnsPodElement : ConfigurationElement
    {
        [ConfigurationProperty("value")]
        public string Value { get { return this["value"].ToString(); } set { this["value"] = value; } }
    }

    public class DnsPodHost : DnsPodElement { }

    public class DnsPodToken : DnsPodElement { }

    public class DnsPodEmail : DnsPodElement { }

    public class DnsPodPassword : DnsPodElement { }

    public class DnsPodFormat : DnsPodElement { }

    public class DnsPodLang : DnsPodElement { }

    public class DnsPodErrorOnEmpty : DnsPodElement { }

    public class UserAgent : DnsPodElement { }
}
