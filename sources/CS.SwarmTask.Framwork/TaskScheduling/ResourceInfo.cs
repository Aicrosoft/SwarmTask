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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CS.TaskScheduling
{

    #region 公有资源

    /// <summary>
    ///  公有资源信息
    /// </summary>
    /// 
    /// <description class = "CS.WinService.ResourceInfo">
    ///   
    /// </description>
    /// 
    /// <history>
    ///   2010-7-8 9:50:45 , zhouyu ,  创建	     
    ///  </history>
    [Serializable]
    public class ResourceInfo
    {
        /// <summary>
        /// 资源名称，调用资源的key
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// 资源类型
        /// </summary>
        [XmlAttribute(AttributeName = "type")]
        public ResourceType Type { get; set; }

        /// <summary>
        /// 资源的值，对应于Name的值的调用
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// 资源参数集合
        /// </summary>
        [XmlArrayItem(ElementName = "add")]
        public ParamCollection Params { get; set; }

    }

    /// <summary>
    /// 资源的参数的键值对
    /// <example>
    ///  <add key="Sender" value="">
    ///	  <text>
    ///	  <![CDATA[
    ///       带格式的文本内容
    ///       ]]>
    ///	  </text>
    /// </example>
    /// </summary>
    [Serializable]
    public struct ParamInfo
    {

        public ParamInfo(string key, string value) : this(key, value, string.Empty)
        {
        }

        public ParamInfo(string key, string value,string text)
        {
            _key = key;
            _value = value;
            _text = text;
        }

        private string _key;
        /// <summary>
        /// 键名
        /// </summary>
        [XmlAttribute(AttributeName = "key")]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        private string _value;
        /// <summary>
        /// 值内容
        /// Note:Text的内容会覆盖value原来的值
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public string Value
        {
            get { return _text != null && !string.IsNullOrEmpty(_text.Trim()) ? _text : _value; }
            set { _value = value; }
        }

        private string _text;
        /// <summary>
        /// 有特殊字符的大块文本
        /// </summary>
        [XmlElement("text")]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

    }



    /// <summary>
    /// 资源类型
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// 未知的
        /// </summary>
        Unknow= 0,

        /// <summary>
        /// 本地资源
        /// </summary>
        Local,

        /// <summary>
        /// Http型源型
        /// </summary>
        Http,

        /// <summary>
        /// Ftp协议资源
        /// </summary>
        Ftp,

        /// <summary>
        /// MsSqlServer数据库
        /// </summary>
        SqlServer,

        /// <summary>
        /// Web服务
        /// </summary>
        WebService,

        /// <summary>
        /// Email服务器
        /// </summary>
        EmailService,

    }


    /// <summary>
    /// 参数键值对集合
    /// </summary>
    [Serializable]
    public class ParamCollection : List<ParamInfo>
    {
        /// <summary>
        /// 按名称索引
        /// </summary>
        /// <param name="key">参数名称</param>
        /// <returns>参数的值</returns>
        public ParamInfo this[string key]
        {
            get { return Find(x => x.Key == key); }
        }
    }

    /// <summary>
    /// 公有资源集合
    /// </summary>
    [Serializable]
    public class ResourceCollection:List<ResourceInfo>
    {
        /// <summary>
        /// 按名称索引
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <returns>资源内容</returns>
        public ResourceInfo this[string name]
        {
            get { return Find(x => x.Name == name); }
        }
    }

    #endregion


    #region 任务内部对公有资源的引用

    /// <summary>
    /// 预定义的扩展
    /// <para>预定义的内部扩展</para>
    /// </summary>
    public class PreExtend
    {
        /// <summary>
        /// 公有资源信息
        /// </summary>
        [XmlArrayItem(ElementName = "ref")]
        public RefCollection Refs { get; set; }

        /// <summary>
        /// 初始化引用资源的信息
        /// </summary>
        /// <param name="resources"></param>
        public void InitRefResource(ResourceCollection resources)
        {
            foreach (var reference in Refs)
            {
                reference.Resource = resources[reference.ResName];
            }
        }

    }


    /// <summary>
    /// 公有资源的引用信息
    /// </summary>
    [Serializable]
    public class RefInfo
    {
        /// <summary>
        /// 本任务内容的名称，调用资源的key
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        ///// <summary>
        ///// 资源类型
        ///// </summary>
        //[XmlAttribute(AttributeName = "type")]
        //public ResourceType Type { get; set; }

        /// <summary>
        /// 引用的公有资源名称
        /// </summary>
        [XmlAttribute(AttributeName = "resName")]
        public string ResName { get; set; }

        /// <summary>
        /// 引用的公有资源
        /// </summary>
        [XmlIgnore]
        public ResourceInfo Resource { get; set; }
        //{
        //    get { return Resources[ResName]; }
        //}
        ///// <summary>
        ///// 公有资源的引用
        ///// </summary>
        //[XmlIgnore]
        //internal ResourceCollection Resources { set; private get;}

        /// <summary>
        /// 针对该任务，该资源的参数的集合
        /// </summary>
        [XmlArrayItem(ElementName = "add")]
        public ParamCollection Params { get; set; }

    }

    /// <summary>
    /// 该任务内的资源引用集合
    /// </summary>
    [Serializable]
    public class RefCollection : List<RefInfo>
    {
        /// <summary>
        /// 按名称索引
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <returns>资源内容</returns>
        public RefInfo this[string name]
        {
            get { return Find(x => x.Name == name); }
        }
    }


    #endregion

}
