using System;

namespace DNSPod
{
    public class DnsPodUser : DnsPodBase
    {
        public dynamic Version()
        {
            return PostApiRequest("Info.Version");
        }

        /// <summary>
        /// 获取帐户信息
        /// </summary>
        /// <returns></returns>
        public dynamic UserDetail()
        {
            return PostApiRequest("User.Detail");
        }

        /// <summary>
        /// 修改资料
        /// </summary>
        /// <param name="paramObject"></param>
        /// <returns></returns>
        public bool ModifyUserInfo(dynamic paramObject)
        {
            var result = PostApiRequest("User.Modify", paramObject);
            return Convert.ToInt32(result.status.code) == 1; ;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oldPassword">旧密码</param>
        /// <param name="newPassword">新密码</param>
        /// <returns></returns>
        public bool ModifyUserPassword(string oldPassword, string newPassword)
        {
            var result = PostApiRequest("Userpasswd.Modify",
                new
                {
                    old_password = oldPassword,
                    new_password = newPassword
                }
            );
            var statusCode = Convert.ToInt32(result.status.code);
            if (statusCode != 1)
            {
                throw new ArgumentException(result.status.message);
            }
            return statusCode == 1; ;
        }

        /// <summary>
        /// 修改邮箱地址
        /// </summary>
        /// <param name="oldEmail">旧邮箱</param>
        /// <param name="newEmail">新邮箱</param>
        /// <param name="password">当前密码</param>
        /// <returns></returns>
        public bool ModifyUserEmail(string oldEmail, string newEmail, string password)
        {
            var result = PostApiRequest("Useremail.Modify", new
            {
                old_email = oldEmail,
                new_email = newEmail,
                password
            });
            return Convert.ToInt32(result.status.code) == 1; ;
        }

        /// <summary>
        /// 获取手机验证码
        /// </summary>
        /// <param name="telephone">用户手机号码</param>
        /// <returns></returns>
        public dynamic TelephoneVerifyCode(string telephone)
        {
            return PostApiRequest("Telephoneverify.Code", new { telephone });
        }

        /// <summary>
        /// 获取用户日志
        /// </summary>
        /// <returns></returns>
        public dynamic UserLog()
        {
            return PostApiRequest("User.Log");
        }
    }
}
