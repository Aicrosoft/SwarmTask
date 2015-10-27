#region copyright info
//------------------------------------------------------------------------------
// <copyright company="ChaosStudio">
//     Copyright (c) 2002-2015 巧思工作室.  All rights reserved.
//     Contact:		MSN:atwind@cszi.com , QQ:3329091
//		Home:		 http://www.cszi.com
// </copyright>
//------------------------------------------------------------------------------
#endregion

using System;

namespace CS.TaskScheduling
{
    /// <summary>
    ///   时间委托
    /// </summary>
    /// <description class = "CS.TaskScheduling.SystemTime">
    ///   此处为何要用委托时间? 为了特殊需要。如调试，为了将系统的时间重置为某一个特定的时间，总不能一直调你的Windows时间吧？!
    /// </description>
    /// 
    /// <history>
    ///   2010/4/4 19:18:15 , zhouyu ,  创建	     
    ///  </history>
    public static class SystemTime
    {

        //#region 新用法
        ///// <summary>
        ///// 委托：返回最后一个挂接的方法返回的时间[通过委托挂接到别的函数上，返回的值是最后一个函数的值]。
        ///// <para>Gets the system's current data and time. Only change for  testing scenarios. Use <see cref="Restore"/> to  reset the function to its default implementation.</para>
        ///// </summary>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        //public static Func<DateTime> Now;   //返回时间的一个委托

        //#endregion
        
        //#region 老用法
        ////public delegate DateTimeOffset Func();
        ////public static Func Now;
        //#endregion

        ///// <summary>
        ///// 初始化。
        ///// <para>Inits the <see cref="Now"/> delegate.</para>
        ///// </summary>
        //static SystemTime()
        //{
        //    Restore();
        //}

        ///// <summary>
        ///// 恢复<see cref="Now"/>函数的默认实现<see cref="DateTimeOffset.Now"/>。
        ///// <para>Reverts the <see cref="Now"/> function to its default implementation which just returns <see cref="DateTimeOffset.Now"/>.</para>
        ///// </summary>
        //public static void Restore()
        //{
        //    Now = () => DateTime.Now;  //使用匿名的委托将当前时间挂接到Now方法
        //}


        ///// <summary>
        ///// Return current UTC time via <see cref="Func&lt;T&gt;" />. Allows easier unit testing.
        ///// </summary>
        //public static Func<DateTimeOffset> UtcNow = () => DateTimeOffset.UtcNow;

        ///// <summary>
        ///// Return current time in current time zone via <see cref="Func&lt;T&gt;" />. Allows easier unit testing.
        ///// </summary>
        //public static Func<DateTimeOffset> Now = () => DateTimeOffset.Now;


        /// <summary>
        /// Return current UTC time via <see cref="Func&lt;T&gt;" />. Allows easier unit testing.
        /// </summary>
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;

        /// <summary>
        /// Return current time in current time zone via <see cref="Func&lt;T&gt;" />. Allows easier unit testing.
        /// </summary>
        public static Func<DateTime> Now = () => DateTime.Now;

    }
}