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
using System.Xml.Serialization;

namespace CS.TaskScheduling
{
    /// <summary>
    ///   默认的扩展
    /// </summary>
    /// 
    /// <description class = "CS.WinService.ExtendInfo">
    ///   简单的扩展，方便加入一些必要的参数
    /// </description>
    /// 
    /// <history>
    ///   2010-7-9 10:03:13 , zhouyu ,  创建	     
    ///  </history>
    [Serializable]
    [XmlRoot("extend")]
    public class ExtendInfo : TaskExtendBase
    {
        [XmlArrayItem(ElementName = "add")]
        public ParamCollection Settings { get; set; }
    }


}
