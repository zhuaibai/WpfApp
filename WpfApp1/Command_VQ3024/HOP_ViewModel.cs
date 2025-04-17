using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Command;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.VQ3024_Command
{
    public class HOP_ViewModel : BaseViewModel
    {
        //指令
        private string command = "HOP\r";
        public string Command { get { return command; } }

        public string MachineType;

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HOP_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;

            #region 初始化指令

            //输出电压
            Command_SetOutputVoltage = new RelayCommand(
                execute: () => OutputVoltageOperation(),
                canExecute: () => Validate(nameof(OutputVoltage_Inputs)) && !OutputVoltage_IsWorking // 增加处理状态检查
            );
            //输出频率
            Command_SetOutputFrequency = new RelayCommand(
                execute: () => OutputFrequencyOperation(),
                canExecute: () => Validate(nameof(OutputFrequency_Inputs)) && !OutputFrequency_IsWorking // 增加处理状态检查
            );
            //无功功率
            Command_SetReactivePower = new RelayCommand(
                execute: () => ReactivePowerOperation(),
                canExecute: () => Validate(nameof(ReactivePower_Inputs)) && !ReactivePower_IsWorking // 增加处理状态检查
            );
            //有功功率
            Command_SetActivePower = new RelayCommand(
                execute: () => ActivePowerOperation(),
                canExecute: () => Validate(nameof(ActivePower_Inputs)) && !ActivePower_IsWorking // 增加处理状态检查
            );
            //负载百分比
            Command_SetPercentageOfLoad = new RelayCommand(
                execute: () => ActivePowerOperation(),
                canExecute: () => Validate(nameof(PercentageOfLoad_Inputs)) && !PercentageOfLoad_IsWorking // 增加处理状态检查
            );
            //逆变电流
            Command_SetInverterCurrent = new RelayCommand(
                execute: () => InverterCurrentOperation(),
                canExecute: () => Validate(nameof(InverterCurrent_Inputs)) && !InverterCurrent_IsWorking // 增加处理状态检查
            );
            //Mos管电流
            Command_SetInverterCurrent = new RelayCommand(
                execute: () => MOS_TubeCurrentOperation(),
                canExecute: () => Validate(nameof(MOS_TubeCurrent_Inputs)) && !MOS_TubeCurrent_IsWorking // 增加处理状态检查
            );

            #endregion
        }



        #region 输出电压

        //输出电压
        private string _OutputVoltage;

        public string OutputVoltage
        {
            get { return _OutputVoltage; }
            set
            {
                _OutputVoltage = value;
                this.RaiseProperChanged(nameof(OutputVoltage));
            }
        }


        private bool OutputVoltage_IsWorking;


        //设置值
        private string _OutputVoltage_Inputs;

        public string OutputVoltage_Inputs
        {
            get { return _OutputVoltage_Inputs; }
            set
            {
                _OutputVoltage_Inputs = value;
                this.RaiseProperChanged(nameof(OutputVoltage_Inputs));
                Command_SetOutputVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetOutputVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OutputVoltageOperation()
        {
            try
            {
                OutputVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetOutputVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", OutputVoltage_Inputs);

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
                OutputVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOutputVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 输出频率

        //输出频率
        private string _OutputFrequency;

        public string OutputFrequency
        {
            get { return _OutputFrequency; }
            set
            {
                _OutputFrequency = value;
                this.RaiseProperChanged(nameof(OutputFrequency));
            }
        }


        private bool OutputFrequency_IsWorking;


        //设置值
        private string _OutputFrequency_Inputs;

        public string OutputFrequency_Inputs
        {
            get { return _OutputFrequency_Inputs; }
            set
            {
                _OutputFrequency_Inputs = value;
                this.RaiseProperChanged(nameof(OutputFrequency_Inputs));
                Command_SetOutputFrequency.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetOutputFrequency { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OutputFrequencyOperation()
        {
            try
            {
                OutputFrequency_IsWorking = true;
                // 禁用按钮
                Command_SetOutputFrequency.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", OutputFrequency_Inputs);

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
                OutputFrequency_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOutputFrequency.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 无功功率
        //无功功率
        private string _ReactivePower;

        public string ReactivePower
        {
            get { return _ReactivePower; }
            set
            {
                _ReactivePower = value;
                this.RaiseProperChanged(nameof(ReactivePower));
            }
        }


        private bool ReactivePower_IsWorking;


        //设置值
        private string _ReactivePower_Inputs;

        public string ReactivePower_Inputs
        {
            get { return _ReactivePower_Inputs; }
            set
            {
                _ReactivePower_Inputs = value;
                this.RaiseProperChanged(nameof(ReactivePower_Inputs));
                Command_SetReactivePower.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetReactivePower { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ReactivePowerOperation()
        {
            try
            {
                ReactivePower_IsWorking = true;
                // 禁用按钮
                Command_SetReactivePower.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ReactivePower_Inputs);

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
                ReactivePower_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetReactivePower.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 有功功率

        //有功功率
        private string _ActivePower;

        public string ActivePower
        {
            get { return _ActivePower; }
            set
            {
                _ActivePower = value;
                this.RaiseProperChanged(nameof(ActivePower));
            }
        }


        private bool ActivePower_IsWorking;


        //设置值
        private string _ActivePower_Inputs;

        public string ActivePower_Inputs
        {
            get { return _ActivePower_Inputs; }
            set
            {
                _ActivePower_Inputs = value;
                this.RaiseProperChanged(nameof(ActivePower_Inputs));
                Command_SetActivePower.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetActivePower { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ActivePowerOperation()
        {
            try
            {
                ActivePower_IsWorking = true;
                // 禁用按钮
                Command_SetActivePower.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ActivePower_Inputs);

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
                ActivePower_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetActivePower.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 负载百分比

        //负载百分比
        private string _PercentageOfLoad;

        public string PercentageOfLoad
        {
            get { return _PercentageOfLoad; }
            set
            {
                _PercentageOfLoad = value;
                this.RaiseProperChanged(nameof(PercentageOfLoad));
            }
        }


        private bool PercentageOfLoad_IsWorking;


        //设置值
        private string _PercentageOfLoad_Inputs;

        public string PercentageOfLoad_Inputs
        {
            get { return _PercentageOfLoad_Inputs; }
            set
            {
                _PercentageOfLoad_Inputs = value;
                this.RaiseProperChanged(nameof(PercentageOfLoad_Inputs));
                Command_SetPercentageOfLoad.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetPercentageOfLoad { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void PercentageOfLoadOperation()
        {
            try
            {
                PercentageOfLoad_IsWorking = true;
                // 禁用按钮
                Command_SetPercentageOfLoad.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", PercentageOfLoad_Inputs);

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
                PercentageOfLoad_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetPercentageOfLoad.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 逆变电流

        //逆变电流
        private string _InverterCurrent;

        public string InverterCurrent
        {
            get { return _InverterCurrent; }
            set
            {
                _InverterCurrent = value;
                this.RaiseProperChanged(nameof(InverterCurrent));
            }
        }


        private bool InverterCurrent_IsWorking;


        //设置值
        private string _InverterCurrent_Inputs;

        public string InverterCurrent_Inputs
        {
            get { return _InverterCurrent_Inputs; }
            set
            {
                _InverterCurrent_Inputs = value;
                this.RaiseProperChanged(nameof(InverterCurrent_Inputs));
                Command_SetInverterCurrent.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetInverterCurrent { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InverterCurrentOperation()
        {
            try
            {
                InverterCurrent_IsWorking = true;
                // 禁用按钮
                Command_SetInverterCurrent.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", InverterCurrent_Inputs);

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
                InverterCurrent_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInverterCurrent.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region Mos管电流

        //Mos管电流
        private string _MOS_TubeCurrent;

        public string MOS_TubeCurrent
        {
            get { return _MOS_TubeCurrent; }
            set
            {
                _MOS_TubeCurrent = value;
                this.RaiseProperChanged(nameof(MOS_TubeCurrent));
            }
        }


        private bool MOS_TubeCurrent_IsWorking;


        //设置值
        private string _MOS_TubeCurrent_Inputs;

        public string MOS_TubeCurrent_Inputs
        {
            get { return _MOS_TubeCurrent_Inputs; }
            set
            {
                _MOS_TubeCurrent_Inputs = value;
                this.RaiseProperChanged(nameof(MOS_TubeCurrent_Inputs));
                Command_SetMOS_TubeCurrent.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetMOS_TubeCurrent { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void MOS_TubeCurrentOperation()
        {
            try
            {
                MOS_TubeCurrent_IsWorking = true;
                // 禁用按钮
                Command_SetMOS_TubeCurrent.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", MOS_TubeCurrent_Inputs);

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
                MOS_TubeCurrent_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetMOS_TubeCurrent.RaiseCanExecuteChanged();
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
                //输出电压       
                case "OutputVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(OutputVoltage_Inputs);
                //输出频率   
                case "OutputFrequency_Inputs":
                    return !string.IsNullOrWhiteSpace(OutputFrequency_Inputs);
                //无功功率  
                case "ReactivePower_Inputs":
                    return !string.IsNullOrWhiteSpace(ReactivePower_Inputs);
                //有功功率  
                case "ActivePower_Inputs":
                    return !string.IsNullOrWhiteSpace(ActivePower_Inputs);
                //负载百分比
                case "PercentageOfLoad_Inputs":
                    return !string.IsNullOrWhiteSpace(PercentageOfLoad_Inputs);
                //逆变电流
                case "InverterCurrent_Inputs":
                    return !string.IsNullOrWhiteSpace(InverterCurrent_Inputs);
                //Mos管电流
                case "MOS_TubeCurrent_Inputs":
                    return !string.IsNullOrWhiteSpace(MOS_TubeCurrent_Inputs);

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
                //输出电压
                OutputVoltage = Values[0];
                //输出频率
                OutputFrequency = Values[1];
                //无功功率
                ReactivePower = Values[2];
                //有功功率
                ActivePower = Values[3];
                //负载百分比
                PercentageOfLoad = Values[4];
                //逆变电流
                InverterCurrent = Values[7];
                //Mos管电流
                MOS_TubeCurrent = Values[8];

            }
            catch (Exception ex)
            {

            }
        }
    }
}
