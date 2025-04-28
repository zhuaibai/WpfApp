using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Convert;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_PDF3024
{
    public class HGRID_PDF_ViewModel : BaseViewModel
    {
        //指令
        private string command = "HGRID\r";
        public string Command { get { return command; } }

        public string MachineType;

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HGRID_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;


            #region 初始化指令


            //市电电压
            Command_SetMainsVoltage = new RelayCommand(
                execute: () => MainsVoltageOperation(),
                canExecute: () => Validate(nameof(MainsVoltage_Inputs)) && !MainsVoltage_IsWorking // 增加处理状态检查
            );
            //市电频率
            Command_SetMainsFrequency = new RelayCommand(
                execute: () => MainsFrequencyOperation(),
                canExecute: () => Validate(nameof(MainsFrequency_Inputs)) && !MainsFrequency_IsWorking // 增加处理状态检查
            );
            //市电功率
            Command_SetACPower = new RelayCommand(
                execute: () => ACPowerOperation(),
                canExecute: () => Validate(nameof(ACPower_Inputs)) && !ACPower_IsWorking // 增加处理状态检查
            );
            //温度限制
            Command_SetTempLimit = new RelayCommand(
                execute: () => TempLimitOperation(),
                canExecute: () => Validate(nameof(TempLimit_Inputs)) && !TempLimit_IsWorking // 增加处理状态检查
            );
            //当前允许最大功率
            Command_SetMaxInvPower = new RelayCommand(
               execute: () => MaxInvPowerOperation(),
               canExecute: () => Validate(nameof(MaxInvPower_Inputs)) && !MaxInvPower_IsWorking // 增加处理状态检查
           );


            #endregion
        }

        #region 市电电压

        //市电电压
        private string _MainsVoltage;

        public string MainsVoltage
        {
            get { return _MainsVoltage; }
            set
            {
                _MainsVoltage = Tools.RemoveLeadingZeros(value);
                RaiseProperChanged(nameof(MainsVoltage));
            }
        }


        private bool MainsVoltage_IsWorking;


        //设置值
        private string _MainsVoltage_Inputs;

        public string MainsVoltage_Inputs
        {
            get { return _MainsVoltage_Inputs; }
            set
            {
                _MainsVoltage_Inputs = value;
                RaiseProperChanged(nameof(MainsVoltage_Inputs));
                Command_SetMainsVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetMainsVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void MainsVoltageOperation()
        {
            try
            {
                MainsVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetMainsVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", MainsVoltage_Inputs);

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
                MainsVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetMainsVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 市电频率

        //市电频率
        private string _MainsFrequency;

        public string MainsFrequency
        {
            get { return _MainsFrequency; }
            set
            {
                _MainsFrequency = value;
                RaiseProperChanged(nameof(MainsFrequency));
            }
        }


        private bool MainsFrequency_IsWorking;


        //设置值
        private string _MainsFrequency_Inputs;

        public string MainsFrequency_Inputs
        {
            get { return _MainsFrequency_Inputs; }
            set
            {
                _MainsFrequency_Inputs = value;
                RaiseProperChanged(nameof(MainsFrequency_Inputs));
                Command_SetMainsFrequency.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetMainsFrequency { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void MainsFrequencyOperation()
        {
            try
            {
                MainsFrequency_IsWorking = true;
                // 禁用按钮
                Command_SetMainsFrequency.RaiseCanExecuteChanged();

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
                    Thread.Sleep(1000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", MainsFrequency_Inputs);

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
                MainsFrequency_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetMainsFrequency.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 市电功率

        //市电功率
        private string _ACPower;

        public string ACPower
        {
            get { return _ACPower; }
            set
            {
                _ACPower = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(ACPower));
            }
        }


        private bool ACPower_IsWorking;


        //设置值
        private string _ACPower_Inputs;

        public string ACPower_Inputs
        {
            get { return _ACPower_Inputs; }
            set
            {
                _ACPower_Inputs = value;
                this.RaiseProperChanged(nameof(ACPower_Inputs));
                Command_SetACPower.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetACPower { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ACPowerOperation()
        {
            try
            {
                ACPower_IsWorking = true;
                // 禁用按钮
                Command_SetACPower.RaiseCanExecuteChanged();

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
                    Thread.Sleep(1000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ACPower_Inputs);

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
                ACPower_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetACPower.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 温度限制

        //温度限制
        private string _TempLimit;

        public string TempLimit
        {
            get { return _TempLimit; }
            set
            {
                if (value == "0") { _TempLimit = App.GetText("关闭"); }
                else if (value == "1") { _TempLimit = App.GetText("开启"); }
                else
                _TempLimit = value;
                this.RaiseProperChanged(nameof(TempLimit));
            }
        }


        private bool TempLimit_IsWorking;


        //设置值
        private string _TempLimit_Inputs;

        public string TempLimit_Inputs
        {
            get { return _TempLimit_Inputs; }
            set
            {
                _TempLimit_Inputs = value;
                this.RaiseProperChanged(nameof(TempLimit_Inputs));
                Command_SetTempLimit.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _TempLimitOptions = new List<string> { "开启", "关闭" };

        public List<string> TempLimitOptions
        {
            get { return _TempLimitOptions; }
            set
            {
                _TempLimitOptions = value;
                this.RaiseProperChanged(nameof(TempLimitOptions));
            }
        }

        public RelayCommand Command_SetTempLimit { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void TempLimitOperation()
        {
            try
            {
                TempLimit_IsWorking = true;
                // 禁用按钮
                Command_SetTempLimit.RaiseCanExecuteChanged();

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
                    Thread.Sleep(1000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand(GetSelectedToCommand("TempLimit_Inputs"), TempLimit_Inputs);

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
                TempLimit_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetTempLimit.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 当前允许最大功率

        //当前允许最大功率
        private string _MaxInvPower;

        public string MaxInvPower
        {
            get { return _MaxInvPower; }
            set
            {
                _MaxInvPower = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(MaxInvPower));
            }
        }


        private bool MaxInvPower_IsWorking;


        //设置值
        private string _MaxInvPower_Inputs;

        public string MaxInvPower_Inputs
        {
            get { return _MaxInvPower_Inputs; }
            set
            {
                _MaxInvPower_Inputs = value;
                this.RaiseProperChanged(nameof(MaxInvPower_Inputs));
                Command_SetMaxInvPower.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetMaxInvPower { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void MaxInvPowerOperation()
        {
            try
            {
                MaxInvPower_IsWorking = true;
                // 禁用按钮
                Command_SetMaxInvPower.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", MaxInvPower_Inputs);

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
                MaxInvPower_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetMaxInvPower.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        /// <summary>
        /// 判断输入是否正确
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool Validate(string value)
        {
            switch (value)
            {
                //市电电压       
                case "MainsVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(MainsVoltage_Inputs);
                //市电频率   
                case "MainsFrequency_Inputs":
                    return !string.IsNullOrWhiteSpace(MainsFrequency_Inputs); 
                //市电功率   
                case "ACPower_Inputs":
                    return !string.IsNullOrWhiteSpace(ACPower_Inputs);
                //温度限制 
                case "TempLimit_Inputs":
                    return !string.IsNullOrWhiteSpace(TempLimit_Inputs);
                //温度限制 
                case "MaxInvPower_Inputs":
                    return !string.IsNullOrWhiteSpace(MaxInvPower_Inputs);
               

                default:
                    return false;

            }
        }

        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalyseStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            string[] Values = value.Split(" ");
            try
            {
                //市电电压
                MainsVoltage = Values[0].Substring(1, 5);
                //市电频率
                MainsFrequency = Values[1];
                //市电功率
                ACPower = Values[2];
                //温度限制
                TempLimit = Values[3];
                //当前允许最大逆变功率
                MaxInvPower = Values[4];
                ////逆变电流
                //InverterCurrent = Values[7];
                ////Mos管电流
                //MOS_TubeCurrent = Values[8];

            }
            catch (Exception ex)
            {

            }
        }
        
        /// <summary>
        /// 把选项转化为指令格式
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        private string GetSelectedToCommand(string selected)
        {
            switch (selected)
            {
                //输出电压
                case "TempLimit_Inputs":
                    if (string.IsNullOrWhiteSpace(TempLimit_Inputs))
                    {
                        return string.Empty;
                    }
                    else if (TempLimit_Inputs == "关闭")
                    {
                        return "00";
                    }
                    else if (TempLimit_Inputs == "开启")
                    {
                        return "01";
                    }
                    return "";
                default:
                    return string.Empty;

            }

        }
    }
}
