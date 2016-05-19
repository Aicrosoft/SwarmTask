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
using CS.Serialization;

namespace CS.TaskScheduling
{

    #region 扩展抽象
        
    /// <summary>
    ///   任务扩展抽象基类
    /// <example>
    /// <code>
    /// <para>[Serializable]</para>
    /// <para>[XmlRoot("extend")] </para>
    /// <para>public class MyExtend : TaskExtendBase</para>
    /// <para>{...}</para>
    /// </code>
    /// </example>
    /// </summary>
    /// <description class = "CS.WinService.TaskExtendBase">
    ///   请在继承的类加上    [Serializable][XmlRoot("extend")] 
    /// </description>
    /// <history>
    ///   2010-4-19 17:14:12 , zhouyu ,  创建	     
    ///  </history>
    [Serializable]
    [XmlRoot("extend")]
    public abstract class TaskExtendBase
    {
        /// <summary>
        /// 返回扩展的Xml配置文本未例
        /// </summary>
        public virtual string ToXml()
        {
            return XmlSerializor.Serialize(this);
        }
    }

    #endregion 
    
    
    #region 内容自定义的扩展实现 PreExtendInfo

    /// <summary>
    ///   默认的预定义扩展
    /// </summary>
    /// 
    /// <description class = "CS.WinService.ExtendInfo">
    ///   预定义的内部扩展，如果不用要重写LoadExtend方法并自已实现扩展
    /// </description>
    /// 
    /// <history>
    ///   2010-7-9 10:03:13 , zhouyu ,  创建	
    ///   2010-7-19 , zhouyu , 增加公有资源引用属性     
    ///  </history>
    [Serializable]
    [XmlRoot("extend")]
    public class PreExtendInfo : TaskExtendBase
    {
        /// <summary>
        /// 初始化引用资源的信息
        /// <para>将公有资源赋值到引用上</para>
        /// </summary>
        /// <param name="resources"></param>
        public void InitRefResource(ResourceCollection resources)
        {
            foreach (var reference in Refs)
            {
                reference.Resource = resources[reference.ResName];
            }
        }

        /// <summary>
        /// 公有资源信息
        /// </summary>
        [XmlArrayItem(ElementName = "ref")]
        public RefCollection Refs { get; set; }


        /// <summary>
        /// 未关联至公有资源的参数集合
        /// </summary>
        [XmlArrayItem(ElementName = "add")]
        public ParamCollection Settings { get; set; }

    }

    #endregion

}
