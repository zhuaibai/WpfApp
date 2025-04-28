using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfApp1.Convert;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_PDF3024
{
    public class HEEP1_PDF_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HEEP1\r";
        public string Command { get { return command; } }

        public string MachineType;

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志
        public HEEP1_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;

            #region 初始化命令
            //设置工作模式
            Command_SetWorkingMode = new RelayCommand(
                execute: () => WorkingModeOperation(),
                canExecute: () => Validate(nameof(WorkingModeSelectedOption)) && !_WorkingModeIsWorking // 增加处理状态检查
            );
            //设置低电锁机电压
            Command_SetLowPowerLock = new RelayCommand(
                execute: () => LowPowerLockOperation(),
                canExecute: () => Validate(nameof(LowPowerLock_Inputs)) && !_LowPowerLockIsWorking
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
            //LCD背光
            Command_SetLCD_Backlight = new RelayCommand(
                execute: () => LCD_BacklightOperation(),
                canExecute: () => Validate(nameof(LCD_Backlight_Inputs)) && !LCD_Backlight_IsWorking
            );
            //BMS开关
            Command_SetBMS_CommunicationControlFunction = new RelayCommand(
                execute: () => BMS_CommunicationControlFunctionOperation(),
                canExecute: () => Validate(nameof(BMS_CommunicationControlFunction_Inputs)) && !BMS_CommunicationControlFunction_IsWorking
            );
            //输出模式
            Command_SetOutputMode = new RelayCommand(
                execute: () => OutputModeOperation(),
                canExecute: () => Validate(nameof(OutputMode_Inputs)) && !OutputMode_IsWorking
            );
            //市电输入范围
            Command_SetAC_InputRange = new RelayCommand(
                execute: () => AC_InputRangeOperation(),
                canExecute: () => Validate(nameof(AC_InputRange_Inputs)) && !AC_InputRange_IsWorking
            );
            //充电模式
            Command_SetChargingPriority = new RelayCommand(
                execute: () => ChargingPriorityOperation(),
                canExecute: () => Validate(nameof(ChargingPriority_Inputs)) && !ChargingPriority_IsWorking
            );
            //BMS锁机电池容量
            Command_SetBMS_LowPower_SOC = new RelayCommand(
                execute: () => BMS_LowPower_SOCOperation(),
                canExecute: () => Validate(nameof(BMS_LowPower_SOC_Inputs)) && !BMS_LowPower_SOC_IsWorking
            );
            //AC充电电池容量
            Command_SetBMSreturns_to_AC_mode_SOC = new RelayCommand(
                execute: () => BMSreturns_to_AC_mode_SOCOperation(),
                canExecute: () => Validate(nameof(BMSreturns_to_AC_mode_SOC_Inputs)) && !BMSreturns_to_AC_mode_SOC_IsWorking
            );
            //恢复电池放电电池容量
            Command_SetBMS_returns_to_battery_mode_SOC = new RelayCommand(
                execute: () => BMS_returns_to_battery_mode_SOCOperation(),
                canExecute: () => Validate(nameof(BMS_returns_to_battery_mode_SOC_Inputs)) && !BMS_returns_to_battery_mode_SOC_IsWorking
            );

            //逆变开机电池容量
            Command_SetBMS_automatically_turns_on_after_low_power_SOC = new RelayCommand(
                execute: () => BMS_automatically_turns_on_after_low_power_SOCOperation(),
                canExecute: () => Validate(nameof(BMS_automatically_turns_on_after_low_power_SOC_Inputs)) && !BMS_automatically_turns_on_after_low_power_SOC_IsWorking
            );
            //过载转接旁路
            Command_SetOverloadByPassFunction = new RelayCommand(
               execute: () => OverloadByPassFunctionOperation(),
               canExecute: () => Validate(nameof(OverloadByPassFunction_Inputs)) && !OverloadByPassFunction_IsWorking
            );
            //系统频率
            Command_SetOutputSettingFrequency = new RelayCommand(
               execute: () => OutputSettingFrequencyOperation(),
               canExecute: () => Validate(nameof(OutputSettingFrequency_Inputs)) && !OutputSettingFrequency_IsWorking
            );
            //并网电流
            Command_SetGridCurrent = new RelayCommand(
              execute: () => GridCurrentOperation(),
              canExecute: () => Validate(nameof(GridCurrent_Inputs)) && !GridCurrent_IsWorking
             );

            //并网功能
            Command_SetGridConnectedFunction = new RelayCommand(
              execute: () => GridConnectedFunctionOperation(),
              canExecute: () => Validate(nameof(GridConnectedFunction_Inputs)) && !GridConnectedFunction_IsWorking
             );
            //并网协议
            Command_SetPV_GridConnectionProtocol = new RelayCommand(
              execute: () => PV_GridConnectionProtocolOperation(),
              canExecute: () => Validate(nameof(PV_GridConnectionProtocol_Inputs)) && !PV_GridConnectionProtocol_IsWorking
             );
            //PV馈能优先级
            Command_SetPV_FeedPriority = new RelayCommand(
              execute: () => PV_FeedPriorityOperation(),
              canExecute: () => Validate(nameof(PV_FeedPriority_Inputs)) && !PV_FeedPriority_IsWorking
             );
            //CT功能开关
            Command_SetCT_Enable = new RelayCommand(
              execute: () => CT_EnableOperation(),
              canExecute: () => Validate(nameof(CT_Enable_Inputs)) && !CT_Enable_IsWorking
             );
            //自动返回首页
            Command_SetAutoReturnHome = new RelayCommand(
              execute: () => AutoReturnHomeOperation(),
              canExecute: () => Validate(nameof(AutoReturnHome_Inputs)) && !AutoReturnHome_IsWorking
             );
            //输入源提示
            Command_SetInputPrompt = new RelayCommand(
              execute: () => InputPromptOperation(),
              canExecute: () => Validate(nameof(InputPrompt_Inputs)) && !InputPrompt_IsWorking
             );
            

            #endregion
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
                if (SerialCommunicationService.MachineType == "A" || SerialCommunicationService.MachineType == "C")
                {
                    if (value == "(0") { _WorkingMode = "UTI"; }
                    else if (value == "(1") { _WorkingMode = "SUB"; }
                    else if (value == "(2") { _WorkingMode = "SBU"; }
                }
                else if (SerialCommunicationService.MachineType == "B" || SerialCommunicationService.MachineType == "D")
                {
                    if (value == "1") { _WorkingMode = "PV"; }
                    else if (value == "0") { _WorkingMode = "VOLTAGE"; }
                }
                else
                    _WorkingMode = value;
                RaiseProperChanged(nameof(WorkingMode));

            }
        }

        /// <summary>
        /// 工作模式可选项
        /// </summary>
        private List<string> _WorkingModeOptions = new List<string> { "PV", "VOLTAGE" };

        public List<string> WorkingModeOptions
        {
            get { return _WorkingModeOptions; }
            set
            {
                _WorkingModeOptions = value;
                RaiseProperChanged(nameof(WorkingModeOptions));
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
                    Thread.Sleep(1000);
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
                RaiseProperChanged(nameof(LowPowerLock));
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
                RaiseProperChanged(nameof(LowPowerLock_Inputs));
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
                if (value == "0") { _BatteryType = "AGM"; }
                else if (value == "1") { _BatteryType = "FLD"; }
                else if (value == "2") { _BatteryType = "USE"; }
                else if (value == "3") { _BatteryType = "LIA"; }
                else if (value == "4") { _BatteryType = "PYL"; }
                else if (value == "5") { _BatteryType = "TQF"; }
                else if (value == "6") { _BatteryType = "GRO"; }
                else if (value == "7") { _BatteryType = "LIB"; }
                else if (value == "8") { _BatteryType = "LIC"; }


                else
                    _BatteryType = value;
                RaiseProperChanged(nameof(BatteryType));
            }
        }
        private bool _BatteryIsWorking;


        //电池类型可选项
        private List<string> _BatteryTypeOptions = new List<string> { "AGM", "FLD", "USE", "LIA", "PYL", "TQF", "GRO", "LIB", "LIC" };

        public List<string> BatteryTypeOptions
        {
            get { return _BatteryTypeOptions; }
            set
            {
                _BatteryTypeOptions = value;
                RaiseProperChanged(nameof(BatteryTypeOptions));
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
                RaiseProperChanged(nameof(BatteryTypeSelectedOption));
                Command_SetBatteryType.RaiseCanExecuteChanged();
            }
        }
        //设置命令
        public RelayCommand Command_SetBatteryType { get; set; }

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
                    Thread.Sleep(1000);
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
                RaiseProperChanged(nameof(StrongChargeVoltage));
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
                RaiseProperChanged(nameof(StrongChargeVoltage_Inputs));
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
                    Thread.Sleep(1000);
                    string receive = SerialCommunicationService.SendSettingCommand("PCVV", StrongChargeVoltage_Inputs);

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
                RaiseProperChanged(nameof(FloatChargeVolage));
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
                RaiseProperChanged(nameof(FloatChargeVolage_Inputs));
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
                    Thread.Sleep(1000);
                    string receive = SerialCommunicationService.SendSettingCommand("PBFT", FloatChargeVolage_Inputs);

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
                RaiseProperChanged(nameof(TotalChargeCurrent));
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
                RaiseProperChanged(nameof(TotalChargeCurrent_Inputs));
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
                    Thread.Sleep(1000);
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
                RaiseProperChanged(nameof(AC_ChargingCurrent));
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
                RaiseProperChanged(nameof(AC_ChargingCurrent_Inputs));
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
                    Thread.Sleep(1000);
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

        #region 蜂鸣器状态

        private string _BuzzerStatus;

        public string BuzzerStatus
        {
            get { return _BuzzerStatus; }
            set
            {
                if (value == "1")
                {

                    _BuzzerStatus = App.GetText("开启");
                }
                else if (value == "0")
                {
                    _BuzzerStatus = App.GetText("关闭");
                }
                else
                {
                    _BuzzerStatus = "";
                }
                RaiseProperChanged(nameof(BuzzerStatus));
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
                RaiseProperChanged(nameof(BuzzerStatus_Inputs));
                Command_SetBuzzerStatus.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _BuzzerStatusOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> BuzzerStatusOptions
        {
            get { return _BuzzerStatusOptions; }
            set
            {
                _BuzzerStatusOptions = value;
                RaiseProperChanged(nameof(BuzzerStatusOptions));
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
                    Thread.Sleep(1000);
                    string receive = SerialCommunicationService.SendSettingCommand("", getSelectedToCommad(nameof(BuzzerStatus_Inputs)));

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
                if (value == "0")
                {
                    _OverloadRestart = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _OverloadRestart = App.GetText("开启");
                }
                else
                    _OverloadRestart = value;
                RaiseProperChanged(nameof(OverloadRestart));
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
                RaiseProperChanged(nameof(OverloadRestart_Inputs));
                Command_SetOverloadRestart.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OverloadRestartOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> OverloadRestartOptions
        {
            get { return _OverloadRestartOptions; }
            set
            {
                _OverloadRestartOptions = value;
                RaiseProperChanged(nameof(OverloadRestartOptions));
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
                    Thread.Sleep(1000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("", getSelectedToCommad(nameof(OverloadRestart_Inputs)));

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

        #region 输入源提示

        private string _InputPrompt;

        public string InputPrompt
        {
            get { return _InputPrompt; }
            set
            {
                if (value == "0")
                {
                    _InputPrompt = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _InputPrompt = App.GetText("开启");
                }
                else
                    _InputPrompt = value;
                this.RaiseProperChanged(nameof(InputPrompt));
            }
        }


        private bool InputPrompt_IsWorking;


        //设置值
        private string _InputPrompt_Inputs;

        public string InputPrompt_Inputs
        {
            get { return _InputPrompt_Inputs; }
            set
            {
                _InputPrompt_Inputs = value;
                this.RaiseProperChanged(nameof(InputPrompt_Inputs));
                Command_SetInputPrompt.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _InputPromptOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> InputPromptOptions
        {
            get { return _InputPromptOptions; }
            set
            {
                _InputPromptOptions = value;
                this.RaiseProperChanged(nameof(InputPromptOptions));
            }
        }

        public RelayCommand Command_SetInputPrompt { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InputPromptOperation()
        {
            try
            {
                InputPrompt_IsWorking = true;
                // 禁用按钮
                Command_SetInputPrompt.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("",getSelectedToCommad(nameof(InputPrompt_Inputs)));

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
                InputPrompt_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInputPrompt.RaiseCanExecuteChanged();
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
                    _OverTempperatureRestart = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _OverTempperatureRestart = App.GetText("开启");
                }
                else
                    _OverTempperatureRestart = value;
                RaiseProperChanged(nameof(OverTemperatureRestart));
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
                RaiseProperChanged(nameof(OverTemperatureRestart_Inputs));
                Command_SetOverTemperatureRestart.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OverTemperatureRestartOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> OverTemperatureRestartOptions
        {
            get { return _OverTemperatureRestartOptions; }
            set
            {
                _OverTemperatureRestartOptions = value;
                RaiseProperChanged(nameof(OverTemperatureRestartOptions));
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
                    Thread.Sleep(1000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("", getSelectedToCommad(nameof(OverTemperatureRestart_Inputs)));

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

        #region LCD背光开启

        private string _LCD_Backlight;

        public string LCD_Backlight
        {
            get { return _LCD_Backlight; }
            set
            {
                if (value == "0")
                {
                    _LCD_Backlight = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _LCD_Backlight = App.GetText("开启");
                }
                else
                    _LCD_Backlight = value;
                RaiseProperChanged(nameof(LCD_Backlight));
            }
        }


        private bool LCD_Backlight_IsWorking;


        //设置值
        private string _LCD_Backlight_Inputs;

        public string LCD_Backlight_Inputs
        {
            get { return _LCD_Backlight_Inputs; }
            set
            {
                _LCD_Backlight_Inputs = value;
                RaiseProperChanged(nameof(LCD_Backlight_Inputs));
                Command_SetLCD_Backlight.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _LCD_BacklightOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> LCD_BacklightOptions
        {
            get { return _LCD_BacklightOptions; }
            set
            {
                _LCD_BacklightOptions = value;
                RaiseProperChanged(nameof(LCD_BacklightOptions));
            }
        }

        public RelayCommand Command_SetLCD_Backlight { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void LCD_BacklightOperation()
        {
            try
            {
                LCD_Backlight_IsWorking = true;
                // 禁用按钮
                Command_SetLCD_Backlight.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("", getSelectedToCommad(nameof(LCD_Backlight_Inputs)));

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
                LCD_Backlight_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetLCD_Backlight.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 输出模式

        private string _OutputMode;

        public string OutputMode
        {
            get { return _OutputMode; }
            set
            {
                if (value == "0")
                {
                    _OutputMode = "SIG";
                }
                else if (value == "1")
                {
                    _OutputMode = "PAL";
                }
                else if (value == "2")
                {
                    _OutputMode = "3P1";
                }
                else if (value == "3") { _OutputMode = "3P2"; }
                else if (value == "4") { _OutputMode = "3P3"; }
                else
                    _OutputMode = value;
                RaiseProperChanged(nameof(OutputMode));
            }
        }


        private bool OutputMode_IsWorking;


        //设置值
        private string _OutputMode_Inputs;

        public string OutputMode_Inputs
        {
            get { return _OutputMode_Inputs; }
            set
            {
                _OutputMode_Inputs = value;
                RaiseProperChanged(nameof(OutputMode_Inputs));
                Command_SetOutputMode.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OutputModeOptions = new List<string> { "SIG", "PAL", "3P1", "3P2", "3P3" };

        public List<string> OutputModeOptions
        {
            get { return _OutputModeOptions; }
            set
            {
                _OutputModeOptions = value;
                RaiseProperChanged(nameof(OutputModeOptions));
            }
        }

        public RelayCommand Command_SetOutputMode { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OutputModeOperation()
        {
            try
            {
                OutputMode_IsWorking = true;
                // 禁用按钮
                Command_SetOutputMode.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("POPM", getSelectedToCommad(nameof(OutputMode_Inputs)));

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
                OutputMode_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOutputMode.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region BMS开关

        //BMS通信控制功能
        private string _BMS_CommunicationControlFunction;

        public string BMS_CommunicationControlFunction
        {
            get { return _BMS_CommunicationControlFunction; }
            set
            {

                if (value == "0")
                {
                    _BMS_CommunicationControlFunction = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _BMS_CommunicationControlFunction = App.GetText("开启");
                }
                else
                    _BMS_CommunicationControlFunction = value;
                RaiseProperChanged(nameof(BMS_CommunicationControlFunction));
            }
        }


        private bool BMS_CommunicationControlFunction_IsWorking;


        //设置值
        private string _BMS_CommunicationControlFunction_Inputs;

        public string BMS_CommunicationControlFunction_Inputs
        {
            get { return _BMS_CommunicationControlFunction_Inputs; }
            set
            {
                _BMS_CommunicationControlFunction_Inputs = value;
                RaiseProperChanged(nameof(BMS_CommunicationControlFunction_Inputs));
                Command_SetBMS_CommunicationControlFunction.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _BMS_CommunicationControlFunctionOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> BMS_CommunicationControlFunctionOptions
        {
            get { return _BMS_CommunicationControlFunctionOptions; }
            set
            {
                _BMS_CommunicationControlFunctionOptions = value;
                RaiseProperChanged(nameof(BMS_CommunicationControlFunctionOptions));
            }
        }

        public RelayCommand Command_SetBMS_CommunicationControlFunction { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BMS_CommunicationControlFunctionOperation()
        {
            try
            {
                BMS_CommunicationControlFunction_IsWorking = true;
                // 禁用按钮
                Command_SetBMS_CommunicationControlFunction.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("BMSC", getSelectedToCommad(nameof(BMS_CommunicationControlFunction_Inputs)));

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
                BMS_CommunicationControlFunction_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBMS_CommunicationControlFunction.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 市电输入范围
        //市电输入范围
        private string _AC_InputRange;

        public string AC_InputRange
        {
            get { return _AC_InputRange; }
            set
            {
                if (value == "0")
                {
                    _AC_InputRange = "APL(宽范围)";
                }
                else if (value == "1")
                {
                    _AC_InputRange = "UPL(窄范围)";
                }
                else
                    _AC_InputRange = value;
                RaiseProperChanged(nameof(AC_InputRange));
            }
        }


        private bool AC_InputRange_IsWorking;


        //设置值
        private string _AC_InputRange_Inputs;

        public string AC_InputRange_Inputs
        {
            get { return _AC_InputRange_Inputs; }
            set
            {
                _AC_InputRange_Inputs = value;
                RaiseProperChanged(nameof(AC_InputRange_Inputs));
                Command_SetAC_InputRange.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _AC_InputRangeOptions = new List<string> { "APL(宽范围)", "UPS(窄范围)" };

        public List<string> AC_InputRangeOptions
        {
            get { return _AC_InputRangeOptions; }
            set
            {
                _AC_InputRangeOptions = value;
                RaiseProperChanged(nameof(AC_InputRangeOptions));
            }
        }

        public RelayCommand Command_SetAC_InputRange { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void AC_InputRangeOperation()
        {
            try
            {
                AC_InputRange_IsWorking = true;
                // 禁用按钮
                Command_SetAC_InputRange.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PGR", getSelectedToCommad(nameof(AC_InputRange_Inputs)));

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
                AC_InputRange_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetAC_InputRange.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 充电模式

        //充电模式
        private string _ChargingPriority;

        public string ChargingPriority
        {
            get { return _ChargingPriority; }
            set
            {

                if (SerialCommunicationService.MachineType == "A" || SerialCommunicationService.MachineType == "C")
                {
                    if (value == "0")
                    {
                        _ChargingPriority = "CUT";
                    }
                    else if (value == "1") { _ChargingPriority = "CSO"; }
                    else if (value == "2") { _ChargingPriority = "SUN"; }
                    else if (value == "3") { _ChargingPriority = "OSO"; }
                    else { _ChargingPriority = value; }
                }
                else if (SerialCommunicationService.MachineType == "B" || SerialCommunicationService.MachineType == "D")
                {
                    if (value == "0")
                    {
                        _ChargingPriority = "CSO";
                    }
                    else if (value == "1") { _ChargingPriority = "SUN"; }
                    else if (value == "2") { _ChargingPriority = "OSO"; }
                    else if (value == "3") { _ChargingPriority = "OSO"; }
                    else { _ChargingPriority = value; }
                }

                else
                    _ChargingPriority = value;
                RaiseProperChanged(nameof(ChargingPriority));
            }
        }


        private bool ChargingPriority_IsWorking;


        //设置值
        private string _ChargingPriority_Inputs;

        public string ChargingPriority_Inputs
        {
            get { return _ChargingPriority_Inputs; }
            set
            {
                _ChargingPriority_Inputs = value;
                RaiseProperChanged(nameof(ChargingPriority_Inputs));
                Command_SetChargingPriority.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _ChargingPriorityOptions = new List<string> { "CUT", "CSO", "SNU", "OSO" };

        public List<string> ChargingPriorityOptions
        {
            get { return _ChargingPriorityOptions; }
            set
            {
                _ChargingPriorityOptions = value;
                RaiseProperChanged(nameof(ChargingPriorityOptions));
            }
        }

        public RelayCommand Command_SetChargingPriority { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ChargingPriorityOperation()
        {
            try
            {
                ChargingPriority_IsWorking = true;
                // 禁用按钮
                Command_SetChargingPriority.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PCP", getSelectedToCommad(nameof(ChargingPriority_Inputs)));

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
                ChargingPriority_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetChargingPriority.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region BMS锁机电池容量(%)

        //BMS锁机电池容量
        private string _BMS_LowPower_SOC;

        public string BMS_LowPower_SOC
        {
            get { return _BMS_LowPower_SOC; }
            set
            {
                _BMS_LowPower_SOC = value;
                RaiseProperChanged(nameof(BMS_LowPower_SOC));
            }
        }


        private bool BMS_LowPower_SOC_IsWorking;


        //设置值
        private string _BMS_LowPower_SOC_Inputs;

        public string BMS_LowPower_SOC_Inputs
        {
            get { return _BMS_LowPower_SOC_Inputs; }
            set
            {
                _BMS_LowPower_SOC_Inputs = value;
                RaiseProperChanged(nameof(BMS_LowPower_SOC_Inputs));
                Command_SetBMS_LowPower_SOC.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBMS_LowPower_SOC { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BMS_LowPower_SOCOperation()
        {
            try
            {
                BMS_LowPower_SOC_IsWorking = true;
                // 禁用按钮
                Command_SetBMS_LowPower_SOC.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("BMSSDC", BMS_LowPower_SOC_Inputs);

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
                BMS_LowPower_SOC_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBMS_LowPower_SOC.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region AC充电电池容量

        //AC充电电池容量
        private string _BMSreturns_to_AC_mode_SOC;

        public string BMSreturns_to_AC_mode_SOC
        {
            get { return _BMSreturns_to_AC_mode_SOC; }
            set
            {
                _BMSreturns_to_AC_mode_SOC = value;
                RaiseProperChanged(nameof(BMSreturns_to_AC_mode_SOC));
            }
        }


        private bool BMSreturns_to_AC_mode_SOC_IsWorking;


        //设置值
        private string _BMSreturns_to_AC_mode_SOC_Inputs;

        public string BMSreturns_to_AC_mode_SOC_Inputs
        {
            get { return _BMSreturns_to_AC_mode_SOC_Inputs; }
            set
            {
                _BMSreturns_to_AC_mode_SOC_Inputs = value;
                RaiseProperChanged(nameof(BMSreturns_to_AC_mode_SOC_Inputs));
                Command_SetBMSreturns_to_AC_mode_SOC.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBMSreturns_to_AC_mode_SOC { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BMSreturns_to_AC_mode_SOCOperation()
        {
            try
            {
                BMSreturns_to_AC_mode_SOC_IsWorking = true;
                // 禁用按钮
                Command_SetBMSreturns_to_AC_mode_SOC.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("BMSB2UC", BMSreturns_to_AC_mode_SOC_Inputs);

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
                BMSreturns_to_AC_mode_SOC_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBMSreturns_to_AC_mode_SOC.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 电池放电电池容量

        private string _BMS_returns_to_battery_mode_SOC;

        public string BMS_returns_to_battery_mode_SOC
        {
            get { return _BMS_returns_to_battery_mode_SOC; }
            set
            {
                _BMS_returns_to_battery_mode_SOC = value;
                RaiseProperChanged(nameof(BMS_returns_to_battery_mode_SOC));
            }
        }

        //验证是否在执行
        private bool BMS_returns_to_battery_mode_SOC_IsWorking;


        //设置值
        private string _BMS_returns_to_battery_mode_SOC_Inputs;

        public string BMS_returns_to_battery_mode_SOC_Inputs
        {
            get { return _BMS_returns_to_battery_mode_SOC_Inputs; }
            set
            {
                _BMS_returns_to_battery_mode_SOC_Inputs = value;
                RaiseProperChanged(nameof(BMS_returns_to_battery_mode_SOC_Inputs));
                Command_SetBMS_returns_to_battery_mode_SOC.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBMS_returns_to_battery_mode_SOC { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BMS_returns_to_battery_mode_SOCOperation()
        {
            try
            {
                BMS_returns_to_battery_mode_SOC_IsWorking = true;
                // 禁用按钮
                Command_SetBMS_returns_to_battery_mode_SOC.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("BMSU2BC", BMS_returns_to_battery_mode_SOC_Inputs);

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
                BMS_returns_to_battery_mode_SOC_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBMS_returns_to_battery_mode_SOC.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 逆变开机电池容量

        //逆变开机电池容量
        private string _BMS_automatically_turns_on_after_low_power_SOC;

        public string BMS_automatically_turns_on_after_low_power_SOC
        {
            get { return _BMS_automatically_turns_on_after_low_power_SOC; }
            set
            {
                _BMS_automatically_turns_on_after_low_power_SOC = value;
                RaiseProperChanged(nameof(BMS_automatically_turns_on_after_low_power_SOC));
            }
        }


        private bool BMS_automatically_turns_on_after_low_power_SOC_IsWorking;


        //设置值
        private string _BMS_automatically_turns_on_after_low_power_SOC_Inputs;

        public string BMS_automatically_turns_on_after_low_power_SOC_Inputs
        {
            get { return _BMS_automatically_turns_on_after_low_power_SOC_Inputs; }
            set
            {
                _BMS_automatically_turns_on_after_low_power_SOC_Inputs = value;
                RaiseProperChanged(nameof(BMS_automatically_turns_on_after_low_power_SOC_Inputs));
                Command_SetBMS_automatically_turns_on_after_low_power_SOC.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBMS_automatically_turns_on_after_low_power_SOC { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BMS_automatically_turns_on_after_low_power_SOCOperation()
        {
            try
            {
                BMS_automatically_turns_on_after_low_power_SOC_IsWorking = true;
                // 禁用按钮
                Command_SetBMS_automatically_turns_on_after_low_power_SOC.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("BMSSRC", BMS_automatically_turns_on_after_low_power_SOC_Inputs);

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
                BMS_automatically_turns_on_after_low_power_SOC_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBMS_automatically_turns_on_after_low_power_SOC.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 市电过载转旁路

        //市电过载转旁路
        private string _OverloadByPassFunction;

        public string OverloadByPassFunction
        {
            get { return _OverloadByPassFunction; }
            set
            {
                if (value == "0")
                {
                    _OverloadByPassFunction = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _OverloadByPassFunction = App.GetText("开启");
                }
                else
                    _OverloadByPassFunction = value;
                RaiseProperChanged(nameof(OverloadByPassFunction));
            }
        }


        private bool OverloadByPassFunction_IsWorking;


        //设置值
        private string _OverloadByPassFunction_Inputs;

        public string OverloadByPassFunction_Inputs
        {
            get { return _OverloadByPassFunction_Inputs; }
            set
            {
                _OverloadByPassFunction_Inputs = value;
                RaiseProperChanged(nameof(OverloadByPassFunction_Inputs));
                Command_SetOverloadByPassFunction.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OverloadByPassFunctionOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> OverloadByPassFunctionOptions
        {
            get { return _OverloadByPassFunctionOptions; }
            set
            {
                _OverloadByPassFunctionOptions = value;
                RaiseProperChanged(nameof(OverloadByPassFunctionOptions));
            }
        }

        public RelayCommand Command_SetOverloadByPassFunction { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OverloadByPassFunctionOperation()
        {
            try
            {
                OverloadByPassFunction_IsWorking = true;
                // 禁用按钮
                Command_SetOverloadByPassFunction.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("", getSelectedToCommad(nameof(OverloadByPassFunction_Inputs)));

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
                OverloadByPassFunction_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOverloadByPassFunction.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 系统频率

        //系统频率
        private string _OutputSettingFrequency;

        public string OutputSettingFrequency
        {
            get { return _OutputSettingFrequency; }
            set
            {
                if (value == "0")
                {
                    _OutputSettingFrequency = "50";
                }
                else if (value == "1")
                {
                    _OutputSettingFrequency = "60";
                }
                else
                    _OutputSettingFrequency = value;
                RaiseProperChanged(nameof(OutputSettingFrequency));
            }
        }


        private bool OutputSettingFrequency_IsWorking;


        //设置值
        private string _OutputSettingFrequency_Inputs;

        public string OutputSettingFrequency_Inputs
        {
            get { return _OutputSettingFrequency_Inputs; }
            set
            {
                _OutputSettingFrequency_Inputs = value;
                RaiseProperChanged(nameof(OutputSettingFrequency_Inputs));
                Command_SetOutputSettingFrequency.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OutputSettingFrequencyOptions = new List<string> { "50", "60" };

        public List<string> OutputSettingFrequencyOptions
        {
            get { return _OutputSettingFrequencyOptions; }
            set
            {
                _OutputSettingFrequencyOptions = value;
                RaiseProperChanged(nameof(OutputSettingFrequencyOptions));
            }
        }

        public RelayCommand Command_SetOutputSettingFrequency { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OutputSettingFrequencyOperation()
        {
            try
            {
                OutputSettingFrequency_IsWorking = true;
                // 禁用按钮
                Command_SetOutputSettingFrequency.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("F", OutputSettingFrequency_Inputs);

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
                OutputSettingFrequency_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOutputSettingFrequency.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 自动返回首页

        private string _AutoReturnHome;

        public string AutoReturnHome
        {
            get { return _AutoReturnHome; }
            set
            {
                if (value == "0")
                {
                    _AutoReturnHome = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _AutoReturnHome = App.GetText("开启");
                }
                else
                    _AutoReturnHome = value;
                this.RaiseProperChanged(nameof(AutoReturnHome));
            }
        }


        private bool AutoReturnHome_IsWorking;


        //设置值
        private string _AutoReturnHome_Inputs;

        public string AutoReturnHome_Inputs
        {
            get { return _AutoReturnHome_Inputs; }
            set
            {
                _AutoReturnHome_Inputs = value;
                this.RaiseProperChanged(nameof(AutoReturnHome_Inputs));
                Command_SetAutoReturnHome.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _AutoReturnHomeOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> AutoReturnHomeOptions
        {
            get { return _AutoReturnHomeOptions; }
            set
            {
                _AutoReturnHomeOptions = value;
                this.RaiseProperChanged(nameof(AutoReturnHomeOptions));
            }
        }

        public RelayCommand Command_SetAutoReturnHome { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void AutoReturnHomeOperation()
        {
            try
            {
                AutoReturnHome_IsWorking = true;
                // 禁用按钮
                Command_SetAutoReturnHome.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("",getSelectedToCommad(nameof(AutoReturnHome_Inputs)));

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
                AutoReturnHome_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetAutoReturnHome.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 并网电流

        //并网电流
        private string _GridCurrent;

        public string GridCurrent
        {
            get { return _GridCurrent; }
            set
            {
                _GridCurrent = Tools.RemoveLeadingZeros(value);
                RaiseProperChanged(nameof(GridCurrent));
            }
        }


        private bool GridCurrent_IsWorking;


        //设置值
        private string _GridCurrent_Inputs;

        public string GridCurrent_Inputs
        {
            get { return _GridCurrent_Inputs; }
            set
            {
                _GridCurrent_Inputs = value;
                RaiseProperChanged(nameof(GridCurrent_Inputs));
                Command_SetGridCurrent.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetGridCurrent { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void GridCurrentOperation()
        {
            try
            {
                GridCurrent_IsWorking = true;
                // 禁用按钮
                Command_SetGridCurrent.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PGFC", GridCurrent_Inputs);

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
                GridCurrent_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetGridCurrent.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 并网功能

        //并网功能
        private string _GridConnectedFunction;

        public string GridConnectedFunction
        {
            get { return _GridConnectedFunction; }
            set
            {
                if (value == "0")
                {
                    _GridConnectedFunction = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _GridConnectedFunction = App.GetText("开启");
                }
                else
                    _GridConnectedFunction = value;
                RaiseProperChanged(nameof(GridConnectedFunction));
            }
        }


        private bool GridConnectedFunction_IsWorking;


        //设置值
        private string _GridConnectedFunction_Inputs;

        public string GridConnectedFunction_Inputs
        {
            get { return _GridConnectedFunction_Inputs; }
            set
            {
                _GridConnectedFunction_Inputs = value;
                RaiseProperChanged(nameof(GridConnectedFunction_Inputs));
                Command_SetGridConnectedFunction.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _GridConnectedFunctionOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> GridConnectedFunctionOptions
        {
            get { return _GridConnectedFunctionOptions; }
            set
            {
                _GridConnectedFunctionOptions = value;
                RaiseProperChanged(nameof(GridConnectedFunctionOptions));
            }
        }

        public RelayCommand Command_SetGridConnectedFunction { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void GridConnectedFunctionOperation()
        {
            try
            {
                GridConnectedFunction_IsWorking = true;
                // 禁用按钮
                Command_SetGridConnectedFunction.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("GRIDFEED", getSelectedToCommad(nameof(GridConnectedFunction_Inputs)));

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
                GridConnectedFunction_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetGridConnectedFunction.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 并网协议

        //并网协议
        private string _PV_GridConnectionProtocol;

        public string PV_GridConnectionProtocol
        {
            get { return _PV_GridConnectionProtocol; }
            set
            {
                if (value == "0") { _PV_GridConnectionProtocol = "India"; }
                else if (value == "1") { _PV_GridConnectionProtocol = "Germen"; }
                else if (value == "2") { _PV_GridConnectionProtocol = "SouthAmerica"; }
                else if (value == "3") { _PV_GridConnectionProtocol = "Pakistan"; }
                else
                    _PV_GridConnectionProtocol = value;
                RaiseProperChanged(nameof(PV_GridConnectionProtocol));
            }
        }


        private bool PV_GridConnectionProtocol_IsWorking;


        //设置值
        private string _PV_GridConnectionProtocol_Inputs;

        public string PV_GridConnectionProtocol_Inputs
        {
            get { return _PV_GridConnectionProtocol_Inputs; }
            set
            {
                _PV_GridConnectionProtocol_Inputs = value;
                RaiseProperChanged(nameof(PV_GridConnectionProtocol_Inputs));
                Command_SetPV_GridConnectionProtocol.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _PV_GridConnectionProtocolOptions = new List<string> { "India", "Germen", "SouthAmerica", "Pakistan" };

        public List<string> PV_GridConnectionProtocolOptions
        {
            get { return _PV_GridConnectionProtocolOptions; }
            set
            {
                _PV_GridConnectionProtocolOptions = value;
                RaiseProperChanged(nameof(PV_GridConnectionProtocolOptions));
            }
        }

        public RelayCommand Command_SetPV_GridConnectionProtocol { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void PV_GridConnectionProtocolOperation()
        {
            try
            {
                PV_GridConnectionProtocol_IsWorking = true;
                // 禁用按钮
                Command_SetPV_GridConnectionProtocol.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("^S???RS", getSelectedToCommad(nameof(PV_GridConnectionProtocol_Inputs)));

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
                PV_GridConnectionProtocol_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetPV_GridConnectionProtocol.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }
        #endregion

        #region PV馈能优先级

        //PV馈能优先级
        private string _PV_FeedPriority;

        public string PV_FeedPriority
        {
            get { return _PV_FeedPriority; }
            set
            {
                if (value == "0")
                {
                    _PV_FeedPriority = "BLU";
                }
                else if (value == "1")
                {
                    _PV_FeedPriority = "LBU";
                }
                else
                    _PV_FeedPriority = value;
                RaiseProperChanged(nameof(PV_FeedPriority));
            }
        }


        private bool PV_FeedPriority_IsWorking;


        //设置值
        private string _PV_FeedPriority_Inputs;

        public string PV_FeedPriority_Inputs
        {
            get { return _PV_FeedPriority_Inputs; }
            set
            {
                _PV_FeedPriority_Inputs = value;
                RaiseProperChanged(nameof(PV_FeedPriority_Inputs));
                Command_SetPV_FeedPriority.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _PV_FeedPriorityOptions = new List<string> { "BLU", "LBU" };

        public List<string> PV_FeedPriorityOptions
        {
            get { return _PV_FeedPriorityOptions; }
            set
            {
                _PV_FeedPriorityOptions = value;
                RaiseProperChanged(nameof(PV_FeedPriorityOptions));
            }
        }

        public RelayCommand Command_SetPV_FeedPriority { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void PV_FeedPriorityOperation()
        {
            try
            {
                PV_FeedPriority_IsWorking = true;
                // 禁用按钮
                Command_SetPV_FeedPriority.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PVENGUSE", getSelectedToCommad(nameof(PV_FeedPriority_Inputs)));

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
                PV_FeedPriority_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetPV_FeedPriority.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region CT功能开关
        private string _CT_Enable;

        public string CT_Enable
        {
            get { return _CT_Enable; }
            set
            {
                if (value == "0")
                {
                    _CT_Enable = App.GetText("关闭");
                }
                else if (value == "1")
                {
                    _CT_Enable = App.GetText("开启");
                }
                else
                    _CT_Enable = value;
                this.RaiseProperChanged(nameof(CT_Enable));
            }
        }


        private bool CT_Enable_IsWorking;


        //设置值
        private string _CT_Enable_Inputs;

        public string CT_Enable_Inputs
        {
            get { return _CT_Enable_Inputs; }
            set
            {
                _CT_Enable_Inputs = value;
                this.RaiseProperChanged(nameof(CT_Enable_Inputs));
                Command_SetCT_Enable.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _CT_EnableOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> CT_EnableOptions
        {
            get { return _CT_EnableOptions; }
            set
            {
                _CT_EnableOptions = value;
                this.RaiseProperChanged(nameof(CT_EnableOptions));
            }
        }

        public RelayCommand Command_SetCT_Enable { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void CT_EnableOperation()
        {
            try
            {
                CT_Enable_IsWorking = true;
                // 禁用按钮
                Command_SetCT_Enable.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", CT_Enable_Inputs);

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
                CT_Enable_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetCT_Enable.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }
        #endregion

        #region 自动返回首页



        #endregion

        #region 通用方法
        // 输入验证&选择验证
        private bool Validate(string value)
        {
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
                case "LCD_Backlight_Inputs":
                    return !string.IsNullOrWhiteSpace(LCD_Backlight_Inputs);
                case "BMS_CommunicationControlFunction_Inputs":
                    return !string.IsNullOrWhiteSpace(BMS_CommunicationControlFunction_Inputs);
                case "OutputMode_Inputs":
                    return !string.IsNullOrWhiteSpace(OutputMode_Inputs);
                case "AC_InputRange_Inputs":
                    return !string.IsNullOrWhiteSpace(AC_InputRange_Inputs);
                case "ChargingPriority_Inputs":
                    return !string.IsNullOrWhiteSpace(ChargingPriority_Inputs);
                case "BMS_LowPower_SOC_Inputs":
                    return !string.IsNullOrWhiteSpace(BMS_LowPower_SOC_Inputs);
                case "BMSreturns_to_AC_mode_SOC_Inputs":
                    return !string.IsNullOrWhiteSpace(BMSreturns_to_AC_mode_SOC_Inputs);
                case "BMS_returns_to_battery_mode_SOC_Inputs":
                    return !string.IsNullOrWhiteSpace(BMS_returns_to_battery_mode_SOC_Inputs);
                case "BMS_automatically_turns_on_after_low_power_SOC_Inputs":
                    return !string.IsNullOrWhiteSpace(BMS_automatically_turns_on_after_low_power_SOC_Inputs);
                case "OverloadByPassFunction_Inputs":
                    return !string.IsNullOrWhiteSpace(OverloadByPassFunction_Inputs);
                case "OutputSettingFrequency_Inputs":
                    return !string.IsNullOrWhiteSpace(OutputSettingFrequency_Inputs);
                case "GridCurrent_Inputs":
                    return !string.IsNullOrWhiteSpace(GridCurrent_Inputs);
                case "GridConnectedFunction_Inputs":
                    return !string.IsNullOrWhiteSpace(GridConnectedFunction_Inputs);
                case "PV_GridConnectionProtocol_Inputs":
                    return !string.IsNullOrWhiteSpace(PV_GridConnectionProtocol_Inputs);
                case "PV_FeedPriority_Inputs":
                    return !string.IsNullOrWhiteSpace(PV_FeedPriority_Inputs);
                case "CT_Enable_Inputs":
                    return !string.IsNullOrWhiteSpace(CT_Enable_Inputs);
                case "AutoReturnHome_Inputs":
                    return !string.IsNullOrWhiteSpace(AutoReturnHome_Inputs);
                case "InputPrompt_Inputs":
                    return !string.IsNullOrWhiteSpace(InputPrompt_Inputs);
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
                WorkingMode = Values[8];
                //最大总充电流
                TotalChargeCurrent = Values[1];
                //市电总充电流
                AC_ChargingCurrent = Values[2];
                //市电输入范围
                AC_InputRange = Values[3].Substring(0, 1);
                //PV并网协议
                PV_GridConnectionProtocol = Values[3].Substring(1, 1);
                //电池类型
                BatteryType = Values[3].Substring(2, 1);
                //PV馈能优先级
                PV_FeedPriority = Values[3].Substring(3, 1);
                //过载重启
                OverloadRestart = Values[3].Substring(4, 1);
                //输入源提示
                InputPrompt = Values[3].Substring(5, 1);
                //过温重启
                OverTemperatureRestart = Values[3].Substring(6, 1);
                //CT功能开关
                CT_Enable = Values[3].Substring(7, 1);
                //系统频率
                OutputSettingFrequency = Values[4].Substring(0, 1);
                //自动返回首页
                AutoReturnHome = Values[4].Substring(1, 1);
                //充电优先顺序(充电模式)
                ChargingPriority = Values[4].Substring(2, 1);
                //蜂鸣器状态
                BuzzerStatus = Values[5];
                //LCD背光
                LCD_Backlight = Values[6];
                //过载转接旁路
                OverloadByPassFunction = Values[7];
                //输出模式
                OutputMode = Values[8];
                //BMS通讯控制功能
                BMS_CommunicationControlFunction = Values[9];
                //BMS锁机电池容量
                BMS_LowPower_SOC = Values[10];
                //BMS返回市电模式SOC(AC充电电池容量)
                BMSreturns_to_AC_mode_SOC = Values[11];
                //恢复电池放电电池容量
                BMS_returns_to_battery_mode_SOC = Values[12];
                //BMS低电压自动开机(逆变开机电池容量)
                BMS_automatically_turns_on_after_low_power_SOC = Values[13];
                //强充电压
                StrongChargeVoltage = Values[14];
                //浮充电压
                FloatChargeVolage = Values[15];
                //低电锁机电压
                LowPowerLock = Values[16];
                //并网电流
                GridCurrent = Values[17];
                //并网功能
                GridConnectedFunction = Values[18].Substring(0,1);
            }
            catch (Exception ex)
            {
                //异常
                return;
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
                case "WorkingModeSelectedOption":
                    if (SerialCommunicationService.MachineType == "A" || SerialCommunicationService.MachineType == "C")
                    {
                        if (string.IsNullOrWhiteSpace(WorkingModeSelectedOption))
                        {
                            return string.Empty;
                        }
                        else if (WorkingModeSelectedOption == "UTI")
                        {
                            return "00";
                        }
                        else if (WorkingModeSelectedOption == "SUB")
                        {
                            return "01";
                        }
                        else if (WorkingModeSelectedOption == "SBU") { return "02"; }

                    }
                    else if (SerialCommunicationService.MachineType == "B" || SerialCommunicationService.MachineType == "D")
                    {
                        if (string.IsNullOrWhiteSpace(WorkingModeSelectedOption))
                        {
                            return string.Empty;
                        }
                        else if (WorkingModeSelectedOption == "PV")
                        {
                            return "01";
                        }
                        else if (WorkingModeSelectedOption == "VOLTAGE") { return "00"; }
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
                    if (string.IsNullOrWhiteSpace(BuzzerStatus_Inputs)) { return string.Empty; }
                    else if (BuzzerStatus_Inputs == "开启/On") { return "PEa"; }
                    else if (BuzzerStatus_Inputs == "关闭/Off") { return "PDa"; }
                    else
                        return "";
                //过载重启
                case "OverloadRestart_Inputs":
                    if (string.IsNullOrWhiteSpace(OverloadRestart_Inputs)) { return string.Empty; }
                    else if (OverloadRestart_Inputs == "开启/On") { return "PEu"; }
                    else if (OverloadRestart_Inputs == "关闭/Off") { return "PDu"; }
                    else
                        return "";
                //过温重启
                case "OverTemperatureRestart_Inputs":
                    if (string.IsNullOrWhiteSpace(OverTemperatureRestart_Inputs)) { return string.Empty; }
                    else if (OverTemperatureRestart_Inputs == "开启/On") { return "PEv"; }
                    else if (OverTemperatureRestart_Inputs == "关闭/Off") { return "PDv"; }
                    else
                        return "";
                //LCD背光开启
                case "LCD_Backlight_Inputs":
                    if (string.IsNullOrWhiteSpace(LCD_Backlight_Inputs)) { return string.Empty; }
                    else if (LCD_Backlight_Inputs == "开启/On") { return "PEx"; }
                    else if (LCD_Backlight_Inputs == "关闭/Off") { return "PDx"; }
                    else
                        return "";
                //BMS开关
                case "BMS_CommunicationControlFunction_Inputs":
                    if (string.IsNullOrWhiteSpace(BMS_CommunicationControlFunction_Inputs)) { return string.Empty; }
                    else if (BMS_CommunicationControlFunction_Inputs == "开启/On") { return "01"; }
                    else if (BMS_CommunicationControlFunction_Inputs == "关闭/Off") { return "00"; }
                    else
                        return "";
                //输出模式
                case "OutputMode_Inputs":
                    if (string.IsNullOrWhiteSpace(OutputMode_Inputs)) { return string.Empty; }
                    else if (OutputMode_Inputs == "SIG") { return "00"; }
                    else if (OutputMode_Inputs == "PAL") { return "01"; }
                    else if (OutputMode_Inputs == "3P1") { return "02"; }
                    else if (OutputMode_Inputs == "3P2") { return "03"; }
                    else if (OutputMode_Inputs == "3P3") { return "04"; }
                    else
                        return "";
                //市电输入范围
                case "AC_InputRange_Inputs":
                    if (string.IsNullOrWhiteSpace(AC_InputRange_Inputs)) { return string.Empty; }
                    else if (AC_InputRange_Inputs == "APL(宽范围)") { return "00"; }
                    else if (AC_InputRange_Inputs == "UPS(窄范围)") { return "01"; }
                    else return AC_InputRange_Inputs;
                //充电模式
                case "ChargingPriority_Inputs":
                    if (SerialCommunicationService.MachineType == "A" || SerialCommunicationService.MachineType == "C")
                    {
                        if (ChargingPriority_Inputs == "CUT") { return "00"; }
                        else if (ChargingPriority_Inputs == "CSO") { return "01"; }
                        else if (ChargingPriority_Inputs == "SNU") { return "02"; }
                        else if (ChargingPriority_Inputs == "OSO") { return "03"; }
                    }
                    else if (SerialCommunicationService.MachineType == "B" || SerialCommunicationService.MachineType == "D")
                    {
                        if (ChargingPriority_Inputs == "CUT") { return ""; }
                        else if (ChargingPriority_Inputs == "CSO") { return "00"; }
                        else if (ChargingPriority_Inputs == "SNU") { return "01"; }
                        else if (ChargingPriority_Inputs == "OSO") { return "02"; }
                    }
                    return ChargingPriority_Inputs;
                //过载转接旁路
                case "OverloadByPassFunction_Inputs":
                    if (string.IsNullOrWhiteSpace(OverloadByPassFunction_Inputs)) { return string.Empty; }
                    else if (OverloadByPassFunction_Inputs == "开启/On") { return "PEb"; }
                    else if (OverloadByPassFunction_Inputs == "关闭/Off") { return "PDb"; }
                    else
                        return OverloadByPassFunction_Inputs;
                //系统频率
                case "OutputSettingFrequency_Inputs":
                    if (string.IsNullOrWhiteSpace(OutputSettingFrequency_Inputs)) { return string.Empty; }
                    else if (OutputSettingFrequency_Inputs == "50") { return "1"; }
                    else if (OutputSettingFrequency_Inputs == "60") { return "0"; }
                    else
                        return OutputSettingFrequency_Inputs;
                //并网功能
                case "GridConnectedFunction_Inputs":
                    if (string.IsNullOrWhiteSpace(GridConnectedFunction_Inputs)) { return string.Empty; }
                    else if (GridConnectedFunction_Inputs == "开启/On") { return "01"; }
                    else if (GridConnectedFunction_Inputs == "关闭/Off") { return "00"; }
                    else
                        return OutputSettingFrequency_Inputs;
                //PV并网协议
                case "PV_GridConnectionProtocol_Inputs":
                    if (string.IsNullOrWhiteSpace(PV_GridConnectionProtocol_Inputs)) { return string.Empty; }
                    else if (PV_GridConnectionProtocol_Inputs == "India") { return "00"; }
                    else if (PV_GridConnectionProtocol_Inputs == "Germen") { return "01"; }
                    else if (PV_GridConnectionProtocol_Inputs == "SouthAmerica") { return "02"; }
                    else if (PV_GridConnectionProtocol_Inputs == "Pakistan") { return "03"; }
                    else
                        return OutputSettingFrequency_Inputs;
                //PV馈能优先级
                case "PV_FeedPriority_Inputs":
                    if (string.IsNullOrWhiteSpace(PV_FeedPriority_Inputs)) { return string.Empty; }
                    else if (PV_FeedPriority_Inputs == "BLU") { return "00"; }
                    else if (PV_FeedPriority_Inputs == "LBU") { return "01"; }
                    else
                        return OutputSettingFrequency_Inputs;
                //自动返回首页
                case "AutoReturnHome_Inputs":
                    if(AutoReturnHome_Inputs == "开启/On")
                    {
                        return "PEk";
                    }else if(AutoReturnHome_Inputs == "关闭/Off")
                    {
                        return "PDk";
                    }else
                        return AutoReturnHome_Inputs;
                //输入源提示
                case "InputPrompt_Inputs":
                    if(AutoReturnHome_Inputs == "开启/On")
                    {
                        return "PEy";
                    }else if(AutoReturnHome_Inputs == "关闭/Off")
                    {
                        return "PDy";
                    }else
                        return AutoReturnHome_Inputs;
                default:
                    return "";


            }
        }

        #endregion

    }
}
