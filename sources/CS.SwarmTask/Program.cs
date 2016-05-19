#region copyright info
//------------------------------------------------------------------------------
// <copyright company="ChaosStudio">
//     Copyright (c) 2002-2010 巧思工作室.  All rights reserved.
//     Contact:		MSN:zhouyu@cszi.com , QQ:478779122
//		Link:		http://www.cszi.com
// </copyright>
//------------------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using CS.Cryptography;
using CS.Diagnostics;
using CS.TaskScheduling;


namespace CS.SwarmTask
{
    /// <summary>
    /// 基于CS.Scheduling.Framework架构的Windwos服务
    /// <remarks>
    /// 
    /// </remarks>
    /// <description>
    /// zhouyu 2010.3.10 created.
    /// zhouyu 2010-5-31 支持暂停，恢复功能。
    /// </description>
    /// </summary>
    public class TaskService : ServiceBase
    {
        /// <summary>
        /// 服务管理器
        /// </summary>
        private Task4WinService _taskService;

        ///// <summary>
        ///// 运行日志
        ///// </summary>
        //private static readonly ILog logRun = LogManager.GetLogger(typeof(TaskService));

        private static readonly ITracer log = CS.Diagnostics.Logger.GetSysLog(typeof(TaskService));

        #region 应用程序入口

        /// <summary>
        /// 应用程序入口
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {

            log.DebugFormat("args:[{0}]", string.Join(" ", args));

            try
            {
                var newMutexCreated = false;
                //var reg = new Regex("[^A-Za-z0-9_]", RegexOptions.Compiled);    //移去非字符数字的字符
                var mutexName =
                    Md5.Encrypt(
                        $"{AppDomain.CurrentDomain.BaseDirectory}-{Assembly.GetExecutingAssembly().GetName().FullName}"); //唯一的名称
                try
                {
                    var obj = new Mutex(false, mutexName, out newMutexCreated);
                }
                catch (Exception ex)
                {
                    log.Error($"创建互斥体[mutexName = {mutexName}]异常，程序退出", ex);
                    Environment.Exit(1);
                }
                if (newMutexCreated)
                {
                    log.DebugFormat("创建互斥体[mutexName = {0}]成功，开始创建服务", mutexName);

                    //无参数时直接运行服务
                    if ((!Environment.UserInteractive))
                    {
                        log.DebugFormat("RunAsService");
                        RunAsService();
                        return;
                    }

                    if (args != null && args.Length > 0)
                    {
                        if (args[0].Equals("-i", StringComparison.OrdinalIgnoreCase))
                        {
                            log.InfoFormat("Install the service...");
                            SelfInstaller.InstallMe();
                            return;
                        }
                        if (args[0].Equals("-u", StringComparison.OrdinalIgnoreCase))
                        {
                            log.InfoFormat("Uninstall the service...");
                            SelfInstaller.UninstallMe();
                            return;
                        }
                        if (args[0].Equals("-t", StringComparison.OrdinalIgnoreCase) ||
                            args[0].Equals("-c", StringComparison.OrdinalIgnoreCase))
                        {
                            log.InfoFormat("Run as Console.[{0}]", Assembly.GetExecutingAssembly().Location);
                            RunAsConsole(args);
                            return;
                        }
                        const string tip =
                            "Invalid argument! note:\r\n -i is install the service.;\r\n -u is uninstall the service.;\r\n -t or -c is run the service on console.";
                        log.DebugFormat(tip);
                        Console.WriteLine(tip);
                        Console.ReadLine();
                    }
                    else
                    {
#if DEBUG
                        log.InfoFormat("Run as Console.[{0}]", Assembly.GetExecutingAssembly().Location);
                        RunAsConsole(args);
#endif
                    }
                }
                else
                {
                    log.Error("有一个实例正在运行，如要调试，请先停止其它正在运行的实例如WindowsService，程序退出。");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                log.Error("启动服务异常", ex);
                //throw;
            }
        }

        #endregion


        private static void RunAsConsole(string[] args)
        {
            //var x = new ShipServicesHost();
            //x.Start(args);
            var service = new TaskService();
            service.OnStart(null);
            Console.ReadLine();
        }

        private static void RunAsService()
        {
            //var servicesToRun = new ServiceBase[] { new ShipServicesHost() };
            //ServiceBase.Run(servicesToRun);
            var service = new TaskService();
            Run(service);
        }

        private static bool IsMono => Type.GetType("Mono.Runtime") != null;

        private readonly string _displayName;

        /// <summary>
        /// 默认构造，相关初始化
        /// </summary>
        public TaskService()
        {
            _displayName = ConfigurationManager.AppSettings["ServiceDisplayName"].Trim();

            CanPauseAndContinue = true;
            ServiceName = ConfigurationManager.AppSettings["ServiceName"].Trim();
            log.DebugFormat("服务类初始化完成");
        }


        #region Start & Stop  &   Pause & Continue

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {

            try
            {
                if (_taskService == null)
                    _taskService = new Task4WinService();
                log.InfoFormat("try run the {0}.", _displayName);
                _taskService.Start();
                log.InfoFormat("{0} is runing.", _displayName);
            }
            catch (Exception ex)
            {
                log.FatalFormat("启动服务时发生异常", ex);
                throw;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            log.InfoFormat("try stop the {0}.", _displayName);
            _taskService.Stop();
            log.InfoFormat("{0} is stoped.", _displayName);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        protected override void OnPause()
        {
            log.InfoFormat("try pause the {0}", _displayName);
            _taskService.Pause();
            log.InfoFormat("{0} is pauseed\n", _displayName);
        }

        /// <summary>
        /// 继续执行
        /// </summary>
        protected override void OnContinue()
        {
            log.InfoFormat("try continue the {0}", _displayName);
            _taskService.Resume();
            log.InfoFormat("{0} is continued\n", _displayName);
        }



        #endregion

    }



    /// <summary>
    /// 服务自安装
    /// </summary>
    public class SelfInstaller
    {
        private static readonly string exePath = Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// Install service
        /// </summary>
        /// <returns></returns>
        public static bool InstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Uninstall service
        /// </summary>
        /// <returns></returns>
        public static bool UninstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { "/u", exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }
    }


    #region 服务安装配置

    /// <summary>
    /// 服务安装，安装后立即启动
    /// </summary>
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private static readonly string codeBase = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
        protected string ConfigServiceName = codeBase;
        protected string ConfigDescription = null;
        protected string DisplayName = codeBase;

        /// <summary>
        /// 安装
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent(); //在安装中取得配置中的名字必须。
            Committed += ServiceInstallerCommitted;

            var serviceName = ConfigurationManager.AppSettings["ServiceName"].Trim();
            var displayName = ConfigurationManager.AppSettings["ServiceDisplayName"].Trim();
            var desc = ConfigurationManager.AppSettings["ServiceDescription"].Trim();
            if (!string.IsNullOrEmpty(serviceName)) ConfigServiceName = serviceName;
            if (!string.IsNullOrEmpty(displayName)) DisplayName = displayName;
            if (!string.IsNullOrEmpty(desc)) ConfigDescription = desc;

            var processInstaller = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
            var serviceInstaller = new ServiceInstaller
            {
                //自动启动服务，手动的话，每次开机都要手动启动。
                StartType = ServiceStartMode.Automatic,

                DisplayName = DisplayName,
                ServiceName = ConfigServiceName,
                Description = ConfigDescription
            };


            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);

        }


        /// <summary>
        /// 服务安装完成后自动启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServiceInstallerCommitted(object sender, InstallEventArgs e)
        {
            var controller = new ServiceController(ConfigServiceName);
            controller.Start();
        }

        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

        }

        #endregion


    }


    #endregion

}

