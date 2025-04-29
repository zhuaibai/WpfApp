using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Command;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Comand_GB3024
{
    public class Special_Command : BaseViewModel
    {



        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public Special_Command(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;

            //设置恒充充电电压
            Command_SetConstantVoltageCV = new RelayCommand(
                execute: () => ConstantVoltageCV_Operation(),
                canExecute: () => Validate(nameof(ConstantVoltageCV_Inputs)) && !_ConstantVoltageCVIsWorking // 增加处理状态检查
            );
            //设置浮充充电电压
            Command_FloatChargeCV = new RelayCommand(
                execute: () => FloatChargeCV_Operation(),
                canExecute: () => Validate(nameof(FloatChargeCV_Inputs)) && !PBFT_IsWorking // 增加处理状态检查
            );
            //设置市电充电电压
            Command_SetAC_ChargingVoltage = new RelayCommand(
                execute: () => AC_ChargingVoltageOperation(),
                canExecute: () => Validate(nameof(AC_ChargingVoltage_Inputs)) && !AC_ChargingVoltage_IsWorking // 增加处理状态检查
            );
            //设置电池放电电压
            Command_SetBatteryDischargeVoltage = new RelayCommand(
               execute: () => AC_ChargingVoltageOperation(),
               canExecute: () => Validate(nameof(BatteryDischargeVoltage_Inputs)) && !BatteryDischargeVoltage_IsWorking // 增加处理状态检查
            );
            //AC充电时间
            Command_SetAC_Charging_Time = new RelayCommand(
               execute: () => AC_Charging_TimeOperation(),
               canExecute: () => Validate(nameof(AC_Charging_Time_Inputs)) && !AC_Charging_Time_IsWorking // 增加处理状态检查
            );
            //系统复位
            Command_SetSystemReset = new RelayCommand(
                execute: () => SystemResetOperation(),
                canExecute: () => !SystemReset_IsWorking // 增加处理状态检查
             );
            //清除发电量
            Command_SetClearPowerGeneration = new RelayCommand(
                execute: () => ClearPowerGenerationOperation(),
                canExecute: () => !ClearPowerGeneration_IsWorking // 增加处理状态检查
             );
            //副输出关
            Command_SetAuxiliaryOutputShutdown = new RelayCommand(
                execute: () => AuxiliaryOutputShutdownOperation(),
                canExecute: () => !AuxiliaryOutputShutdown_IsWorking // 增加处理状态检查
             );
            //副输出开
            Command_SetAuxiliaryOutputOn = new RelayCommand(
                execute: () => AuxiliaryOutputOnOperation(),
                canExecute: () => !AuxiliaryOutputOn_IsWorking // 增加处理状态检查
             );
            //逆变器逆变电压检测值提高n个单位
            Command_SetInvVoltDetectAdjDec = new RelayCommand(
                execute: () => InvVoltDetectAdjDecOperation(),
                canExecute: () => Validate(nameof(InvVoltDetectAdjDec_Inputs)) && !InvVoltDetectAdjDec_IsWorking // 增加处理状态检查
             );
            //逆变器逆变电压检测值降低n个单位
            Command_SetInvVoltDetectAdjInc = new RelayCommand(
                execute: () => InvVoltDetectAdjIncOperation(),
                canExecute: () => Validate(nameof(InvVoltDetectAdjInc_Inputs)) && !InvVoltDetectAdjInc_IsWorking // 增加处理状态检查
             );
            //保存参数设置
            Command_SetSaveConfig = new RelayCommand(
                execute: () => SaveConfigOperation(),
                canExecute: () =>
                !SaveConfig_IsWorking // 增加处理状态检查
             );
            //历史故障保存打开
            Command_SetFaultHistorySave = new RelayCommand(
                execute: () => FaultHistorySaveOperation(),
                canExecute: () =>
                !FaultHistorySave_IsWorking // 增加处理状态检查
             );
            //历史故障保存关闭
            Command_SetFaultHistorySaveOff = new RelayCommand(
                execute: () => FaultHistorySaveOffOperation(),
                canExecute: () =>
                !FaultHistorySaveOff_IsWorking // 增加处理状态检查
             );
            ////抗干扰开关
            //Command_SetAntiJamMode = new RelayCommand(
            //    execute: () => AntiJamModeOperation(),
            //    canExecute: () =>
            //    !FaultHistorySaveOff_IsWorking // 增加处理状态检查
            // );

        }

        #region 获取机器类型

        private string _QueryMachineType = "QPRTL\r";

        public string QueryMachineType
        {
            get { return _QueryMachineType = "QPRTL\r"; }
            set { _QueryMachineType = value; }
        }


        #endregion

        #region 恒压充电电压

        //查询 恒压充电电压 指令
        private string QCVV_command = "QCVV";
        public string QCVV_Command { get { return QCVV_command; } }
        /// <summary>
        /// 恒压充电电压
        /// </summary>
        private string _ConstantVoltageCV;

        public string ConstantVoltageCV
        {
            get { return _ConstantVoltageCV; }
            set
            {
                _ConstantVoltageCV = value;
                RaiseProperChanged(nameof(ConstantVoltageCV));
            }
        }

        private bool _ConstantVoltageCVIsWorking;

        /// <summary>
        /// 设置值
        /// </summary>
        private string _ConstantVoltageCV_Inputs;

        public string ConstantVoltageCV_Inputs
        {
            get { return _ConstantVoltageCV_Inputs; }
            set
            {
                _ConstantVoltageCV_Inputs = value;
                RaiseProperChanged(nameof(ConstantVoltageCV_Inputs));
                Command_SetConstantVoltageCV.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand Command_SetConstantVoltageCV { get; }

        /// <summary>
        /// 设置 恒压充电电压
        /// </summary>
        private async void ConstantVoltageCV_Operation()
        {
            try
            {
                _ConstantVoltageCVIsWorking = true;
                // 禁用按钮
                Command_SetConstantVoltageCV.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PCVV", ConstantVoltageCV_Inputs);

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
                _ConstantVoltageCVIsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetConstantVoltageCV.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        /// <summary>
        /// 对返回的报文进行处理
        /// </summary>
        /// <param name="value"></param>
        public void QCVV_Analayse(string value)
        {
            ConstantVoltageCV = value;
        }

        #endregion

        #region 浮充充电电压

        private string QBFT_command = "QBFT";

        public string QBFT_Command
        {
            get { return QBFT_command; }

        }

        /// <summary>
        /// 浮充充电电压
        /// </summary>
        private string _FloatChargeCV;

        public string FloatChargeCV
        {
            get { return _FloatChargeCV; }
            set
            {
                _FloatChargeCV = value;
                RaiseProperChanged(nameof(FloatChargeCV));
            }
        }

        private bool PBFT_IsWorking;

        private string _FloatChargeCV_Inputs;

        public string FloatChargeCV_Inputs
        {
            get { return _FloatChargeCV_Inputs; }
            set
            {
                _FloatChargeCV_Inputs = value;
                RaiseProperChanged(nameof(FloatChargeCV_Inputs));
                Command_FloatChargeCV.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand Command_FloatChargeCV { get; }

        /// <summary>
        /// 设置 恒压充电电压
        /// </summary>
        private async void FloatChargeCV_Operation()
        {
            try
            {
                PBFT_IsWorking = true;
                // 禁用按钮
                Command_FloatChargeCV.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBFT", FloatChargeCV_Inputs);

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
                PBFT_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_FloatChargeCV.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 电池(市电)充电电压

        //电池充电电压
        private string _AC_ChargingVoltage;

        public string AC_ChargingVoltage
        {
            get { return _AC_ChargingVoltage; }
            set
            {
                _AC_ChargingVoltage = value;
                RaiseProperChanged(nameof(AC_ChargingVoltage));
            }
        }


        private bool AC_ChargingVoltage_IsWorking;


        //设置值
        private string _AC_ChargingVoltage_Inputs;

        public string AC_ChargingVoltage_Inputs
        {
            get { return _AC_ChargingVoltage_Inputs; }
            set
            {
                _AC_ChargingVoltage_Inputs = value;
                RaiseProperChanged(nameof(AC_ChargingVoltage_Inputs));
                Command_SetAC_ChargingVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetAC_ChargingVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void AC_ChargingVoltageOperation()
        {
            try
            {
                AC_ChargingVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetAC_ChargingVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBCV", AC_ChargingVoltage_Inputs);

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
                AC_ChargingVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetAC_ChargingVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 电池放电电压

        //电池放电电压
        private string _BatteryDischargeVoltage;

        public string BatteryDischargeVoltage
        {
            get { return _BatteryDischargeVoltage; }
            set
            {
                _BatteryDischargeVoltage = value;
                RaiseProperChanged(nameof(BatteryDischargeVoltage));
            }
        }


        private bool BatteryDischargeVoltage_IsWorking;


        //设置值
        private string _BatteryDischargeVoltage_Inputs;

        public string BatteryDischargeVoltage_Inputs
        {
            get { return _BatteryDischargeVoltage_Inputs; }
            set
            {
                _BatteryDischargeVoltage_Inputs = value;
                RaiseProperChanged(nameof(BatteryDischargeVoltage_Inputs));
                Command_SetBatteryDischargeVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBatteryDischargeVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BatteryDischargeVoltageOperation()
        {
            try
            {
                BatteryDischargeVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetBatteryDischargeVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBDV", BatteryDischargeVoltage_Inputs);

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
                BatteryDischargeVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBatteryDischargeVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region AC充电时间

        private string _AC_ChargeingTime_Command;

        public string AC_ChargeingTime_Command
        {
            get { return _AC_ChargeingTime_Command; }
            set { _AC_ChargeingTime_Command = value; }
        }


        //AC充电时间
        private string _AC_Charging_Time;

        public string AC_Charging_Time
        {
            get { return _AC_Charging_Time; }
            set
            {
                _AC_Charging_Time = value;
                RaiseProperChanged(nameof(AC_Charging_Time));
            }
        }


        private bool AC_Charging_Time_IsWorking;


        //设置值
        private string _AC_Charging_Time_Inputs;

        public string AC_Charging_Time_Inputs
        {
            get { return _AC_Charging_Time_Inputs; }
            set
            {
                _AC_Charging_Time_Inputs = value;
                RaiseProperChanged(nameof(AC_Charging_Time_Inputs));
                Command_SetAC_Charging_Time.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetAC_Charging_Time { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void AC_Charging_TimeOperation()
        {
            try
            {
                AC_Charging_Time_IsWorking = true;
                // 禁用按钮
                Command_SetAC_Charging_Time.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("^S???ACCT", AC_Charging_Time_Inputs);

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
                AC_Charging_Time_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetAC_Charging_Time.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 系统复位

        //系统复位
        private string _SystemReset;

        public string SystemReset
        {
            get { return _SystemReset; }
            set
            {
                _SystemReset = value;
                RaiseProperChanged(nameof(SystemReset));
            }
        }


        private bool SystemReset_IsWorking;


        //设置值
        private string _SystemReset_Inputs;

        public string SystemReset_Inputs
        {
            get { return _SystemReset_Inputs; }
            set
            {
                _SystemReset_Inputs = value;
                RaiseProperChanged(nameof(SystemReset_Inputs));
                Command_SetSystemReset.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetSystemReset { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void SystemResetOperation()
        {
            try
            {
                SystemReset_IsWorking = true;
                // 禁用按钮
                Command_SetSystemReset.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PF", "");

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
                SystemReset_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetSystemReset.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 清除发电量

        //清除发电量
        private string _ClearPowerGeneration;

        public string ClearPowerGeneration
        {
            get { return _ClearPowerGeneration; }
            set
            {
                _ClearPowerGeneration = value;
                RaiseProperChanged(nameof(ClearPowerGeneration));
            }
        }


        private bool ClearPowerGeneration_IsWorking;


        //设置值
        private string _ClearPowerGeneration_Inputs;

        public string ClearPowerGeneration_Inputs
        {
            get { return _ClearPowerGeneration_Inputs; }
            set
            {
                _ClearPowerGeneration_Inputs = value;
                RaiseProperChanged(nameof(ClearPowerGeneration_Inputs));
                Command_SetClearPowerGeneration.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetClearPowerGeneration { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ClearPowerGenerationOperation()
        {
            try
            {
                ClearPowerGeneration_IsWorking = true;
                // 禁用按钮
                Command_SetClearPowerGeneration.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("^S???CLE", "");

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
                ClearPowerGeneration_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetClearPowerGeneration.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 副输出关

        //副输出关
        private string _AuxiliaryOutputShutdown;

        public string AuxiliaryOutputShutdown
        {
            get { return _AuxiliaryOutputShutdown; }
            set
            {
                _AuxiliaryOutputShutdown = value;
                RaiseProperChanged(nameof(AuxiliaryOutputShutdown));
            }
        }


        private bool AuxiliaryOutputShutdown_IsWorking;


        //设置值
        private string _AuxiliaryOutputShutdown_Inputs;

        public string AuxiliaryOutputShutdown_Inputs
        {
            get { return _AuxiliaryOutputShutdown_Inputs; }
            set
            {
                _AuxiliaryOutputShutdown_Inputs = value;
                RaiseProperChanged(nameof(AuxiliaryOutputShutdown_Inputs));
                Command_SetAuxiliaryOutputShutdown.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetAuxiliaryOutputShutdown { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void AuxiliaryOutputShutdownOperation()
        {
            try
            {
                AuxiliaryOutputShutdown_IsWorking = true;
                // 禁用按钮
                Command_SetAuxiliaryOutputShutdown.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDAULON", "00");

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
                AuxiliaryOutputShutdown_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetAuxiliaryOutputShutdown.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 副输出开

        //副输出开
        private string _AuxiliaryOutputOn;

        public string AuxiliaryOutputOn
        {
            get { return _AuxiliaryOutputOn; }
            set
            {
                _AuxiliaryOutputOn = value;
                RaiseProperChanged(nameof(AuxiliaryOutputOn));
            }
        }


        private bool AuxiliaryOutputOn_IsWorking;


        //设置值
        private string _AuxiliaryOutputOn_Inputs;

        public string AuxiliaryOutputOn_Inputs
        {
            get { return _AuxiliaryOutputOn_Inputs; }
            set
            {
                _AuxiliaryOutputOn_Inputs = value;
                RaiseProperChanged(nameof(AuxiliaryOutputOn_Inputs));
                Command_SetAuxiliaryOutputOn.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetAuxiliaryOutputOn { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void AuxiliaryOutputOnOperation()
        {
            try
            {
                AuxiliaryOutputOn_IsWorking = true;
                // 禁用按钮
                Command_SetAuxiliaryOutputOn.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDAULON", "01");

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
                AuxiliaryOutputOn_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetAuxiliaryOutputOn.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 逆变器逆变电压检测值降低n个单位

        private string _InvVoltDetectAdjDec;

        public string InvVoltDetectAdjDec
        {
            get { return _InvVoltDetectAdjDec; }
            set
            {
                _InvVoltDetectAdjDec = value;
                this.RaiseProperChanged(nameof(InvVoltDetectAdjDec));
            }
        }


        private bool InvVoltDetectAdjDec_IsWorking;


        //设置值
        private string _InvVoltDetectAdjDec_Inputs = "01";

        public string InvVoltDetectAdjDec_Inputs
        {
            get { return _InvVoltDetectAdjDec_Inputs; }
            set
            {
                _InvVoltDetectAdjDec_Inputs = value;
                this.RaiseProperChanged(nameof(InvVoltDetectAdjDec_Inputs));
                Command_SetInvVoltDetectAdjDec.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _InvVoltDetectAdjDecOptions = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09" };

        public List<string> InvVoltDetectAdjDecOptions
        {
            get { return _InvVoltDetectAdjDecOptions; }
            set
            {
                _InvVoltDetectAdjDecOptions = value;
                this.RaiseProperChanged(nameof(InvVoltDetectAdjDecOptions));
            }
        }

        public RelayCommand Command_SetInvVoltDetectAdjDec { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InvVoltDetectAdjDecOperation()
        {
            try
            {
                InvVoltDetectAdjDec_IsWorking = true;
                // 禁用按钮
                Command_SetInvVoltDetectAdjDec.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("IVA-", InvVoltDetectAdjDec_Inputs);

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
                InvVoltDetectAdjDec_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInvVoltDetectAdjDec.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 逆变器逆变电压检测值提高n个单位

        private string _InvVoltDetectAdjInc;

        public string InvVoltDetectAdjInc
        {
            get { return _InvVoltDetectAdjInc; }
            set
            {
                _InvVoltDetectAdjInc = value;
                this.RaiseProperChanged(nameof(InvVoltDetectAdjInc));
            }
        }


        private bool InvVoltDetectAdjInc_IsWorking;


        //设置值
        private string _InvVoltDetectAdjInc_Inputs ="01";

        public string InvVoltDetectAdjInc_Inputs
        {
            get { return _InvVoltDetectAdjInc_Inputs; }
            set
            {
                _InvVoltDetectAdjInc_Inputs = value;
                this.RaiseProperChanged(nameof(InvVoltDetectAdjInc_Inputs));
                Command_SetInvVoltDetectAdjInc.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _InvVoltDetectAdjIncOptions = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09" };

        public List<string> InvVoltDetectAdjIncOptions
        {
            get { return _InvVoltDetectAdjIncOptions; }
            set
            {
                _InvVoltDetectAdjIncOptions = value;
                this.RaiseProperChanged(nameof(InvVoltDetectAdjIncOptions));
            }
        }

        public RelayCommand Command_SetInvVoltDetectAdjInc { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InvVoltDetectAdjIncOperation()
        {
            try
            {
                InvVoltDetectAdjInc_IsWorking = true;
                // 禁用按钮
                Command_SetInvVoltDetectAdjInc.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("IVA+", InvVoltDetectAdjInc_Inputs);

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
                InvVoltDetectAdjInc_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInvVoltDetectAdjInc.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 保存参数设置

        private string _SaveConfig;

        public string SaveConfig
        {
            get { return _SaveConfig; }
            set
            {
                _SaveConfig = value;
                this.RaiseProperChanged(nameof(SaveConfig));
            }
        }


        private bool SaveConfig_IsWorking;


        //设置值
        private string _SaveConfig_Inputs;

        public string SaveConfig_Inputs
        {
            get { return _SaveConfig_Inputs; }
            set
            {
                _SaveConfig_Inputs = value;
                this.RaiseProperChanged(nameof(SaveConfig_Inputs));
                Command_SetSaveConfig.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetSaveConfig { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void SaveConfigOperation()
        {
            try
            {
                SaveConfig_IsWorking = true;
                // 禁用按钮
                Command_SetSaveConfig.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PSAVE", "");

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
                SaveConfig_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetSaveConfig.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 历史故障保存打开

        private string _FaultHistorySave;

        public string FaultHistorySave
        {
            get { return _FaultHistorySave; }
            set
            {
                _FaultHistorySave = value;
                this.RaiseProperChanged(nameof(FaultHistorySave));
            }
        }


        private bool FaultHistorySave_IsWorking;


        //设置值
        private string _FaultHistorySave_Inputs;

        public string FaultHistorySave_Inputs
        {
            get { return _FaultHistorySave_Inputs; }
            set
            {
                _FaultHistorySave_Inputs = value;
                this.RaiseProperChanged(nameof(FaultHistorySave_Inputs));
                Command_SetFaultHistorySave.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetFaultHistorySave { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void FaultHistorySaveOperation()
        {
            try
            {
                FaultHistorySave_IsWorking = true;
                // 禁用按钮
                Command_SetFaultHistorySave.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PEz", "");

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
                FaultHistorySave_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetFaultHistorySave.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 历史故障保存关闭
        private string _FaultHistorySaveOff;

        public string FaultHistorySaveOff
        {
            get { return _FaultHistorySaveOff; }
            set
            {
                _FaultHistorySaveOff = value;
                this.RaiseProperChanged(nameof(FaultHistorySaveOff));
            }
        }


        private bool FaultHistorySaveOff_IsWorking;


        //设置值
        private string _FaultHistorySaveOff_Inputs;

        public string FaultHistorySaveOff_Inputs
        {
            get { return _FaultHistorySaveOff_Inputs; }
            set
            {
                _FaultHistorySaveOff_Inputs = value;
                this.RaiseProperChanged(nameof(FaultHistorySaveOff_Inputs));
                Command_SetFaultHistorySaveOff.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetFaultHistorySaveOff { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void FaultHistorySaveOffOperation()
        {
            try
            {
                FaultHistorySaveOff_IsWorking = true;
                // 禁用按钮
                Command_SetFaultHistorySaveOff.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDz", FaultHistorySaveOff_Inputs);

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
                FaultHistorySaveOff_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetFaultHistorySaveOff.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 抗干扰模式

        private bool _AntiJamMode = false;

        public bool AntiJamMode
        {
            get { return _AntiJamMode; }
            set
            {
                _AntiJamMode = value;
                this.RaiseProperChanged(nameof(AntiJamMode));
            }
        }


        private bool AntiJamMode_IsWorking;


        //设置值
        private string _AntiJamMode_Inputs;

        public string AntiJamMode_Inputs
        {
            get { return _AntiJamMode_Inputs; }
            set
            {
                _AntiJamMode_Inputs = value;
                this.RaiseProperChanged(nameof(AntiJamMode_Inputs));
                Command_SetAntiJamMode.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetAntiJamMode { get; }


        

        /// <summary>
        /// 点击设置
        /// </summary>
        public async void AntiJamModeOperation(bool flag)
        {
            if (flag)
            {
                try
                {
                    AntiJamMode_IsWorking = true;
                    // 禁用按钮
                    //Command_SetAntiJamMode.RaiseCanExecuteChanged();

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
                        string receive = SerialCommunicationService.SendSettingCommand("HOSTCRC", "EN");

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
                    AntiJamMode_IsWorking = false;
                    //Status = "就绪";
                    // 重新启用按钮
                    //Command_SetAntiJamMode.RaiseCanExecuteChanged();
                    // 确保释放锁
                    _semaphore.Release();
                    UpdateState("设置指令已经执行完");
                }
            }
            else
            {
                try
                {
                    AntiJamMode_IsWorking = true;
                    // 禁用按钮
                    //Command_SetAntiJamMode.RaiseCanExecuteChanged();

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
                        string receive = SerialCommunicationService.SendSettingCommand("HOSTCRC", "DN");

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
                    AntiJamMode_IsWorking = false;
                    //Status = "就绪";
                    // 重新启用按钮
                    //Command_SetAntiJamMode.RaiseCanExecuteChanged();
                    // 确保释放锁
                    _semaphore.Release();
                    UpdateState("设置指令已经执行完");
                }
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
                case "ConstantVoltageCV_Inputs":
                    return !string.IsNullOrWhiteSpace(ConstantVoltageCV_Inputs);
                case "FloatChargeCV_Inputs":
                    return !string.IsNullOrWhiteSpace(FloatChargeCV_Inputs);
                case "AC_ChargingVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(AC_ChargingVoltage_Inputs);
                case "BatteryDischargeVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(BatteryDischargeVoltage_Inputs);
                case "AC_Charging_Time_Inputs":
                    return !string.IsNullOrWhiteSpace(AC_Charging_Time_Inputs);
                case "InvVoltDetectAdjDec_Inputs":
                    return !string.IsNullOrWhiteSpace(InvVoltDetectAdjDec_Inputs);
                case "InvVoltDetectAdjInc_Inputs":
                    return !string.IsNullOrWhiteSpace(InvVoltDetectAdjInc_Inputs);

                default:
                    return false;
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

                ////设置电池类型
                //case "BatteryTypeSelectedOption":
                //    if (string.IsNullOrWhiteSpace(BatteryTypeSelectedOption)) { return string.Empty; }
                //    else if (BatteryTypeSelectedOption == "AGM") { return "00"; }
                //    else if (BatteryTypeSelectedOption == "FLD") { return "01"; }
                //    else if (BatteryTypeSelectedOption == "USE") { return "02"; }
                //    else if (BatteryTypeSelectedOption == "LIA") { return "03"; }
                //    else if (BatteryTypeSelectedOption == "PYL") { return "04"; }
                //    else if (BatteryTypeSelectedOption == "TQF") { return "05"; }
                //    else if (BatteryTypeSelectedOption == "GRO") { return "06"; }
                //    else if (BatteryTypeSelectedOption == "LIB") { return "07"; }
                //    else if (BatteryTypeSelectedOption == "LIC") { return "08"; }
                //    else
                //        return "00";
                case "SBU":
                    return "";
                default:
                    return "";
            }
        }




        #endregion
    }
}
