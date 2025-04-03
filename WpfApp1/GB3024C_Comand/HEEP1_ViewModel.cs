using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Command;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.GB3024C_Comand
{
    public class HEEP1_ViewModel : BaseViewModel
    {
        //指令
        private string command = "HEEP1\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志
        public HEEP1_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;

            //设置工作模式
            Command_SetWorkingMode = new RelayCommand(
                execute: () => WorkingModeOperation(),
                canExecute: () => Validate(nameof(WorkingModeSelectedOption)) && !_WorkingModeIsWorking // 增加处理状态检查
            );
            //设置低电锁机电压
            Command_SetLowPowerLock = new RelayCommand(
                execute: () => LowPowerLockOperation(),
                canExecute: () => Validate(nameof(LowPowerLock_Inputs))&&!_LowPowerLockIsWorking
            );
            //设置电池类型
            Command_SetBatteryType = new RelayCommand(
                execute: () => BatteryTypeOperation(),
                canExecute: () => Validate(nameof(BatteryTypeSelectedOption)) && !_BatteryIsWorking
            );
            //设置总充电电流
            Command_SetTotalChargeCurrent = new RelayCommand(
                execute: () => TotalChargeCurrentOperation(),
                canExecute: () => Validate(nameof(TotalChargeCurrent_Inputs)) && !TotalChargeCurrent_IsWorking
            );
            //市电充电电压
            Command_SetAC_ChargingCurrent = new RelayCommand(
                execute: () => AC_ChargingCurrentOperation(),
                canExecute: () => Validate(nameof(AC_ChargingCurrent_Inputs)) && !AC_ChargingCurrent_IsWorking
            );
            //强充电压
            Command_SetStrongChargeVoltage = new RelayCommand(
                execute: () => StrongChargeVoltageOperation(),
                canExecute: () => Validate(nameof(StrongChargeVoltage_Inputs)) && !StrongChargeVoltage_IsWorking
            );
            //强充电压
            Command_SetStrongChargeVoltage = new RelayCommand(
                execute: () => StrongChargeVoltageOperation(),
                canExecute: () => Validate(nameof(StrongChargeVoltage_Inputs)) && !StrongChargeVoltage_IsWorking
            );
            //强充电压
            Command_SetFloatChargeVolage = new RelayCommand(
                execute: () => FloatChargeVolageOperation(),
                canExecute: () => Validate(nameof(FloatChargeVolage_Inputs)) && !FloatChargeVolage_IsWorking
            );

            //蜂鸣器状态
            Command_SetBuzzerStatus = new RelayCommand(
                execute: () => BuzzerStatusOperation(),
                canExecute: () => Validate(nameof(BuzzerStatus_Inputs)) && !BuzzerStatus_IsWorking
            );

            //过载重启
            Command_SetOverloadRestart = new RelayCommand(
                execute: () => OverloadRestartOperation(),
                canExecute: () => Validate(nameof(OverloadRestart_Inputs)) && !OverloadRestart_IsWorking
            );
            //过温重启
            Command_SetOverTemperatureRestart = new RelayCommand(
                execute: () => OverTemperatureRestartOperation(),
                canExecute: () => Validate(nameof(OverTemperatureRestart_Inputs)) && !OverTemperatureRestart_IsWorking
            );
        }


        #region 工作模式
        /// <summary>
        /// 工作模式
        /// </summary>
        private string _WorkingMode;

        public string WorkingMode
        {
            get { return _WorkingMode; }
            set
            {
                _WorkingMode = value;
                this.RaiseProperChanged(nameof(WorkingMode));

            }
        }

        /// <summary>
        /// 工作模式可选项
        /// </summary>
        private List<string> _WorkingModeOptions = new List<string> { "UTI", "SUB", "SBU" };

        public List<string> WorkingModeOptions
        {
            get { return _WorkingModeOptions; }
            set
            {
                _WorkingModeOptions = value;
                this.RaiseProperChanged(nameof(WorkingModeOptions));
                Command_SetWorkingMode.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 已选工作模式
        /// </summary>
        private string? _WorkingModeSelectedOption;
        public string? WorkingModeSelectedOption
        {
            get => _WorkingModeSelectedOption;
            set
            {
                _WorkingModeSelectedOption = value;
                OnPropertyChanged();
                // 选择变化时更新按钮可用状态
                Command_SetWorkingMode.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand Command_SetWorkingMode { get; }

        //工作模式正在设置中...
        private bool _WorkingModeIsWorking;

       

        /// <summary>
        /// 点击工作模式设置
        /// </summary>
        private async void WorkingModeOperation()
        {
            try
            {
                _WorkingModeIsWorking = true;
                // 禁用按钮
                Command_SetWorkingMode.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState($"正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    //Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("POP", getSelectedToCommad(nameof(WorkingModeSelectedOption)));

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
                _WorkingModeIsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetWorkingMode.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 锁机电压

        //低电锁机电压
        private string _LowPowerLock;

        public string LowPowerLock
        {
            get { return _LowPowerLock; }
            set
            {
                _LowPowerLock = value;
                this.RaiseProperChanged(nameof(LowPowerLock));
            }
        }

        private bool _LowPowerLockIsWorking;


        //设置值
        private string _LowPowerLock_Inputs;

        public string LowPowerLock_Inputs
        {
            get { return _LowPowerLock_Inputs; }
            set
            {
                _LowPowerLock_Inputs = value;
                this.RaiseProperChanged(nameof(LowPowerLock_Inputs));
                Command_SetLowPowerLock.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetLowPowerLock { get; }

        /// <summary>
        /// 点击工作模式设置
        /// </summary>
        private async void LowPowerLockOperation()
        {
            try
            {
                _LowPowerLockIsWorking = true;
                // 禁用按钮
                Command_SetLowPowerLock.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState($"正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    //Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("PSDV", LowPowerLock_Inputs);

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
                _LowPowerLockIsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetLowPowerLock.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 电池类型

        //电池类型
        private string _BatteryType;

        public string BatteryType
        {
            get { return _BatteryType; }
            set
            {
                _BatteryType = value;
                this.RaiseProperChanged(nameof(BatteryType));
            }
        }
        private bool _BatteryIsWorking;


        //电池类型可选项
        private List<string> _BatteryTypeOptions = new List<string> { "AGM","FLD","USER"};

        public List<string> BatteryTypeOptions
        {
            get { return _BatteryTypeOptions; }
            set
            {
                _BatteryTypeOptions = value;
                this.RaiseProperChanged(nameof(BatteryTypeOptions));
            }
        }

        //电池类型已选项
        private string? _BatteryTypeSelectedOption;

        public string? BatteryTypeSelectedOption
        {
            get { return _BatteryTypeSelectedOption; }
            set
            {
                _BatteryTypeSelectedOption = value;
                this.RaiseProperChanged(nameof(BatteryTypeSelectedOption));
                Command_SetBatteryType.RaiseCanExecuteChanged();
            }
        }
        //设置命令
        public RelayCommand Command_SetBatteryType {  get; set; }

        /// <summary>
        /// 设置电池类型
        /// </summary>
        private async void BatteryTypeOperation()
        {
            try
            {
                _BatteryIsWorking = true;
                // 禁用按钮
                Command_SetBatteryType.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState($"正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    //Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("PBT", getSelectedToCommad(nameof(BatteryTypeSelectedOption)));

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
                _BatteryIsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBatteryType.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 强充电压

        //强充电压
        private string _StrongChargeVoltage;

        public string StrongChargeVoltage
        {
            get { return _StrongChargeVoltage; }
            set
            {
                _StrongChargeVoltage = value;
                this.RaiseProperChanged(nameof(StrongChargeVoltage));
            }
        }


        private bool StrongChargeVoltage_IsWorking;


        //设置值
        private string _StrongChargeVoltage_Inputs;

        public string StrongChargeVoltage_Inputs
        {
            get { return _StrongChargeVoltage_Inputs; }
            set
            {
                _StrongChargeVoltage_Inputs = value;
                this.RaiseProperChanged(nameof(StrongChargeVoltage_Inputs));
                Command_SetStrongChargeVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetStrongChargeVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void StrongChargeVoltageOperation()
        {
            try
            {
                StrongChargeVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetStrongChargeVoltage.RaiseCanExecuteChanged();

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
                    //Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", StrongChargeVoltage_Inputs);

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
                StrongChargeVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetStrongChargeVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 浮充电压

        //浮充电压
        private string _FloatChargeVoltage;

        public string FloatChargeVolage
        {
            get { return _FloatChargeVoltage; }
            set
            {
                _FloatChargeVoltage = value;
                this.RaiseProperChanged(nameof(FloatChargeVolage));
            }
        }


        private bool FloatChargeVolage_IsWorking;


        //设置值
        private string _FloatChargeVolage_Inputs;

        public string FloatChargeVolage_Inputs
        {
            get { return _FloatChargeVolage_Inputs; }
            set
            {
                _FloatChargeVolage_Inputs = value;
                this.RaiseProperChanged(nameof(FloatChargeVolage_Inputs));
                Command_SetFloatChargeVolage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetFloatChargeVolage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void FloatChargeVolageOperation()
        {
            try
            {
                FloatChargeVolage_IsWorking = true;
                // 禁用按钮
                Command_SetFloatChargeVolage.RaiseCanExecuteChanged();

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
                    //Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", FloatChargeVolage_Inputs);

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
                FloatChargeVolage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetFloatChargeVolage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 总充电电流

        //总充电电流
        private string _TotalChargeCurrent;

        public string TotalChargeCurrent
        {
            get { return _TotalChargeCurrent; }
            set
            {
                _TotalChargeCurrent = value;
                this.RaiseProperChanged(nameof(TotalChargeCurrent));
            }
        }

        private bool TotalChargeCurrent_IsWorking;


        //设置值
        private string _TotalChargeCurrent_Inputs;

        public string TotalChargeCurrent_Inputs
        {
            get { return _TotalChargeCurrent_Inputs; }
            set
            {
                _TotalChargeCurrent_Inputs = value;
                this.RaiseProperChanged(nameof(TotalChargeCurrent_Inputs));
                Command_SetTotalChargeCurrent.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetTotalChargeCurrent { get; }

        /// <summary>
        /// 点击工作模式设置
        /// </summary>
        private async void TotalChargeCurrentOperation()
        {
            try
            {
                TotalChargeCurrent_IsWorking = true;
                // 禁用按钮
                Command_SetTotalChargeCurrent.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState($"正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("MNCHGC", TotalChargeCurrent_Inputs);
                    string dsad = receive;
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
                TotalChargeCurrent_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetTotalChargeCurrent.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 市电充电电流

        private string _AC_ChargingCurrent;

        public string AC_ChargingCurrent
        {
            get { return _AC_ChargingCurrent; }
            set
            {
                _AC_ChargingCurrent = value;
                this.RaiseProperChanged(nameof(AC_ChargingCurrent));
            }
        }


        private bool AC_ChargingCurrent_IsWorking;


        //设置值
        private string _AC_ChargingCurrent_Inputs;

        public string AC_ChargingCurrent_Inputs
        {
            get { return _AC_ChargingCurrent_Inputs; }
            set
            {
                _AC_ChargingCurrent_Inputs = value;
                this.RaiseProperChanged(nameof(AC_ChargingCurrent_Inputs));
                Command_SetAC_ChargingCurrent.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetAC_ChargingCurrent { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void AC_ChargingCurrentOperation()
        {
            try
            {
                AC_ChargingCurrent_IsWorking = true;
                // 禁用按钮
                Command_SetAC_ChargingCurrent.RaiseCanExecuteChanged();

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
                    Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("MUCHGC", AC_ChargingCurrent_Inputs);

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
                AC_ChargingCurrent_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetAC_ChargingCurrent.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 凤鸣器状态

        private string _BuzzerStatus;

        public string BuzzerStatus
        {
            get { return _BuzzerStatus; }
            set
            {
                if (value == "1")
                {
                    _BuzzerStatus = "开启";
                }
                else if (value == "0")
                {
                    _BuzzerStatus = "关闭";
                }
                else
                {
                    _BuzzerStatus = "";
                }
                this.RaiseProperChanged(nameof(BuzzerStatus));
            }
        }


        private bool BuzzerStatus_IsWorking;


        //设置值
        private string _BuzzerStatus_Inputs;

        public string BuzzerStatus_Inputs
        {
            get { return _BuzzerStatus_Inputs; }
            set
            {
                _BuzzerStatus_Inputs = value;
                this.RaiseProperChanged(nameof(BuzzerStatus_Inputs));
                Command_SetBuzzerStatus.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _BuzzerStatusOptions = new List<string> { "开启","关闭"};

        public List<string> BuzzerStatusOptions
        {
            get { return _BuzzerStatusOptions; }
            set
            {
                _BuzzerStatusOptions = value;
                this.RaiseProperChanged(nameof(BuzzerStatusOptions));
            }
        }


        public RelayCommand Command_SetBuzzerStatus { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BuzzerStatusOperation()
        {
            try
            {
                BuzzerStatus_IsWorking = true;
                // 禁用按钮
                Command_SetBuzzerStatus.RaiseCanExecuteChanged();

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
                    Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", BuzzerStatus_Inputs);

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
                BuzzerStatus_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBuzzerStatus.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 过载重启

        //过载重启
        private string _OverloadRestart;

        public string OverloadRestart
        {
            get { return _OverloadRestart; }
            set
            {
                if(value == "0")
                {
                    _OverloadRestart = "关闭";
                }else if(value == "1")
                {
                    _OverloadRestart = "开启";
                }else
                _OverloadRestart = value;
                this.RaiseProperChanged(nameof(OverloadRestart));
            }
        }


        private bool OverloadRestart_IsWorking;


        //设置值
        private string _OverloadRestart_Inputs;

        public string OverloadRestart_Inputs
        {
            get { return _OverloadRestart_Inputs; }
            set
            {
                _OverloadRestart_Inputs = value;
                this.RaiseProperChanged(nameof(OverloadRestart_Inputs));
                Command_SetOverloadRestart.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OverloadRestartOptions = new List<string> { "开启", "关闭" };

        public List<string> OverloadRestartOptions
        {
            get { return _OverloadRestartOptions; }
            set
            {
                _OverloadRestartOptions = value;
                this.RaiseProperChanged(nameof(OverloadRestartOptions));
            }
        }

        public RelayCommand Command_SetOverloadRestart { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OverloadRestartOperation()
        {
            try
            {
                OverloadRestart_IsWorking = true;
                // 禁用按钮
                Command_SetOverloadRestart.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", OverloadRestart_Inputs);

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
                OverloadRestart_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOverloadRestart.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 过温重启

        //过温重启
        private string _OverTempperatureRestart;

        public string OverTemperatureRestart
        {
            get { return _OverTempperatureRestart; }
            set
            {
                if (value == "0")
                {
                    _OverTempperatureRestart = "关闭";
                }
                else if (value == "1")
                {
                    _OverTempperatureRestart = "开启";
                }
                else
                    _OverTempperatureRestart = value;
                this.RaiseProperChanged(nameof(OverTemperatureRestart));
            }
        }


        private bool OverTemperatureRestart_IsWorking;


        //设置值
        private string _OverTemperatureRestart_Inputs;

        public string OverTemperatureRestart_Inputs
        {
            get { return _OverTemperatureRestart_Inputs; }
            set
            {
                _OverTemperatureRestart_Inputs = value;
                this.RaiseProperChanged(nameof(OverTemperatureRestart_Inputs));
                Command_SetOverTemperatureRestart.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OverTemperatureRestartOptions = new List<string> { "开启", "关闭" };

        public List<string> OverTemperatureRestartOptions
        {
            get { return _OverTemperatureRestartOptions; }
            set
            {
                _OverTemperatureRestartOptions = value;
                this.RaiseProperChanged(nameof(OverTemperatureRestartOptions));
            }
        }

        public RelayCommand Command_SetOverTemperatureRestart { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OverTemperatureRestartOperation()
        {
            try
            {
                OverTemperatureRestart_IsWorking = true;
                // 禁用按钮
                Command_SetOverTemperatureRestart.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", OverTemperatureRestart_Inputs);

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
                OverTemperatureRestart_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOverTemperatureRestart.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }




        #endregion


        #region 通用方法
        // 输入验证&选择验证
        private bool Validate(string value)
        {
            //if (value== "WorkingModeSelectedOption")
            //{
            //    return !string.IsNullOrWhiteSpace(WorkingModeSelectedOption);
            //}
            //return true;
            switch (value)
            {
                case "WorkingModeSelectedOption":
                    return !string.IsNullOrWhiteSpace(WorkingModeSelectedOption);
                case "LowPowerLock_Inputs":
                    return !string.IsNullOrWhiteSpace(LowPowerLock_Inputs);
                case "BatteryTypeSelectedOption":
                    return !string.IsNullOrWhiteSpace(BatteryTypeSelectedOption);
                case "TotalChargeCurrent_Inputs":
                    return !string.IsNullOrWhiteSpace(TotalChargeCurrent_Inputs);    
                case "AC_ChargingCurrent_Inputs":
                    return !string.IsNullOrWhiteSpace(AC_ChargingCurrent_Inputs);
                case "StrongChargeVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(StrongChargeVoltage_Inputs);
                case "FloatChargeVolage_Inputs":
                    return !string.IsNullOrWhiteSpace(FloatChargeVolage_Inputs);
                case "BuzzerStatus_Inputs":
                    return !string.IsNullOrWhiteSpace(BuzzerStatus_Inputs);
                case "OverloadRestart_Inputs":
                    return !string.IsNullOrWhiteSpace(OverloadRestart_Inputs);
                case "OverTemperatureRestart_Inputs":
                    return !string.IsNullOrWhiteSpace(OverTemperatureRestart_Inputs);
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
                //工作模式
                WorkingMode = Values[0];
                //总充电流
                TotalChargeCurrent = Values[1];
                //市电总充电流
                AC_ChargingCurrent = Values[2];
                //过载重启
                OverloadRestart = Values[3].Substring(4,1);
                //过温重启
                OverTemperatureRestart = Values[3].Substring(6,1);
                //电池类型
                BatteryType = Values[5];
                //蜂鸣器状态
                BuzzerStatus = Values[6];
                //强充电压
                StrongChargeVoltage = Values[15];
                //浮充电压
                FloatChargeVolage = Values[16];
                //低电锁机电压
                LowPowerLock = Values[17];
            }
            catch (Exception ex)
            {
                return ;
            }
            
        }

        /// <summary>
        /// 对属性值进行二次转换，换成命令指令符号
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string getSelectedToCommad(string value)
        {
            switch (value)
            {
                //设置工作模式
                case "BuzzerStatusSelectedOptions":
                    if (string.IsNullOrWhiteSpace(WorkingModeSelectedOption))
                    {
                        return string.Empty;
                    }
                    else if (WorkingModeSelectedOption == "UTI")
                    {
                        return "00";
                    } else if (WorkingModeSelectedOption == "SUB")
                    {
                        return "01";
                    }
                    return "";
                //设置电池类型
                case "BatteryTypeSelectedOption":
                    if (string.IsNullOrWhiteSpace(BatteryTypeSelectedOption)) { return string.Empty; }
                    else if (BatteryTypeSelectedOption == "AGM") { return "00"; }
                    else if (BatteryTypeSelectedOption == "FLD") { return "01"; }
                    else if (BatteryTypeSelectedOption == "USE") { return "02"; }
                    else if (BatteryTypeSelectedOption == "LIA") { return "03"; }
                    else if (BatteryTypeSelectedOption == "PYL") { return "04"; }
                    else if (BatteryTypeSelectedOption == "TQF") { return "05"; }
                    else if (BatteryTypeSelectedOption == "GRO") { return "06"; }
                    else if (BatteryTypeSelectedOption == "LIB") { return "07"; }
                    else if (BatteryTypeSelectedOption == "LIC") { return "08"; }
                    else
                    return "00";
                 //设置蜂鸣器状态
                case "BuzzerStatus_Inputs":
                    if (string.IsNullOrWhiteSpace(BuzzerStatus_Inputs)) {  return string.Empty; }
                    else if (BuzzerStatus_Inputs == "开启") { return "1"; }
                    else if(BuzzerStatus_Inputs == "关闭") { return "0"; }
                    else
                    return "";
                //过载重启
                case "OverloadRestart_Inputs":
                    if (string.IsNullOrWhiteSpace(OverloadRestart_Inputs)) { return string.Empty; }
                    else if (OverloadRestart_Inputs == "开启") { return "1"; }
                    else if (OverloadRestart_Inputs == "关闭") { return "0"; }
                    else
                        return "";
                default:
                    return "";
            }
        }

        #endregion
    }
}
