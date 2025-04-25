using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_PDF3024
{
    public class HIMSG1_PDF_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HIMSG1\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HIMSG1_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令


            //版本号
            Command_SetVersion = new RelayCommand(
                execute: () => VersionOperation(),
                canExecute: () => Validate(nameof(Version_Inputs)) && !Version_IsWorking // 增加处理状态检查
            );
            //PFC工作状态
            Command_SetReleaseDate = new RelayCommand(
               execute: () => ReleaseDateOperation(),
               canExecute: () => Validate(nameof(ReleaseDate_Inputs)) && !ReleaseDate_IsWorking // 增加处理状态检查
            );
            //程序完整性
            Command_SetSWIntegrity = new RelayCommand(
               execute: () => SWIntegrityOperation(),
               canExecute: () => Validate(nameof(SWIntegrity_Inputs)) && !SWIntegrity_IsWorking // 增加处理状态检查
            );

            #endregion
        }



        #region 版本号
        private string _Version;

        public string Version
        {
            get { return _Version; }
            set
            {
                _Version = value;
                this.RaiseProperChanged(nameof(Version));
            }
        }


        private bool Version_IsWorking;


        //设置值
        private string _Version_Inputs;

        public string Version_Inputs
        {
            get { return _Version_Inputs; }
            set
            {
                _Version_Inputs = value;
                this.RaiseProperChanged(nameof(Version_Inputs));
                Command_SetVersion.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetVersion { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void VersionOperation()
        {
            try
            {
                Version_IsWorking = true;
                // 禁用按钮
                Command_SetVersion.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState("正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", Version_Inputs);

                })
                , timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                AddLog("特殊操作执行超时");
            }
            finally
            {
                // 恢复后台线程
                _pauseEvent.Set();
                AddLog("恢复后台通信");
                Version_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetVersion.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 发布日期

        private string _ReleaseDate;

        public string ReleaseDate
        {
            get { return _ReleaseDate; }
            set
            {
                _ReleaseDate = value;
                this.RaiseProperChanged(nameof(ReleaseDate));
            }
        }


        private bool ReleaseDate_IsWorking;


        //设置值
        private string _ReleaseDate_Inputs;

        public string ReleaseDate_Inputs
        {
            get { return _ReleaseDate_Inputs; }
            set
            {
                _ReleaseDate_Inputs = value;
                this.RaiseProperChanged(nameof(ReleaseDate_Inputs));
                Command_SetReleaseDate.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetReleaseDate { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ReleaseDateOperation()
        {
            try
            {
                ReleaseDate_IsWorking = true;
                // 禁用按钮
                Command_SetReleaseDate.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState("正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ReleaseDate_Inputs);

                })
                , timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                AddLog("特殊操作执行超时");
            }
            finally
            {
                // 恢复后台线程
                _pauseEvent.Set();
                AddLog("恢复后台通信");
                ReleaseDate_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetReleaseDate.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 程序完整性

        private string _SWIntegrity;

        public string SWIntegrity
        {
            get { return _SWIntegrity; }
            set
            {
                if (value=="1")
                {
                    _SWIntegrity = "完整";
                }else if (value == "0") { _SWIntegrity = "不完整"; }
                else
                _SWIntegrity = value;
                this.RaiseProperChanged(nameof(SWIntegrity));
            }
        }


        private bool SWIntegrity_IsWorking;


        //设置值
        private string _SWIntegrity_Inputs;

        public string SWIntegrity_Inputs
        {
            get { return _SWIntegrity_Inputs; }
            set
            {
                _SWIntegrity_Inputs = value;
                this.RaiseProperChanged(nameof(SWIntegrity_Inputs));
                Command_SetSWIntegrity.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetSWIntegrity { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void SWIntegrityOperation()
        {
            try
            {
                SWIntegrity_IsWorking = true;
                // 禁用按钮
                Command_SetSWIntegrity.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState("正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", SWIntegrity_Inputs);

                })
                , timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                AddLog("特殊操作执行超时");
            }
            finally
            {
                // 恢复后台线程
                _pauseEvent.Set();
                AddLog("恢复后台通信");
                SWIntegrity_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetSWIntegrity.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 通用方法

        private bool Validate(string value)
        {
            switch (value)
            {
                //版本号    
                case "Version_Inputs":
                    return !string.IsNullOrWhiteSpace(Version_Inputs);
                //PFC工作状态   
                case "ReleaseDate_Inputs":
                    return !string.IsNullOrWhiteSpace(ReleaseDate_Inputs);
                //程序完整性   
                case "SWIntegrity_Inputs":
                    return !string.IsNullOrWhiteSpace(SWIntegrity_Inputs);
                ////温度限制 
                //case "TempLimit_Inputs":
                //    return !string.IsNullOrWhiteSpace(TempLimit_Inputs);
                ////温度限制 
                //case "MaxInvPower_Inputs":
                //    return !string.IsNullOrWhiteSpace(MaxInvPower_Inputs);


                default:
                    return false;

            }
        }


        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalysisStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //母线电压
                Version = Values[0].Substring(1,7);
                //PFC工作状态
                ReleaseDate = Values[1];
                //程序完整性
                SWIntegrity=Values[2].Substring(0,1);

            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }
}
