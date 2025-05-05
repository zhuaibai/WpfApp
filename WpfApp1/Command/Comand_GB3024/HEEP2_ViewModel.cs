using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Command;
using WpfApp1.Convert;
using WpfApp1.Services;
using WpfApp1.ViewModels;


namespace WpfApp1.Command.Comand_GB3024
{
    public class HEEP2_ViewModel : BaseViewModel
    {
        
        //指令
        private string command = "HEEP2\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志
        public HEEP2_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;

            //返回市电电池电压
            Command_SetReturnMainsBatteryVoltage = new RelayCommand(
                execute: () => ReturnMainsBatteryVoltageOperation(),
                canExecute: () => Validate(nameof(ReturnMainsBatteryVoltage_Inputs)) && !ReturnMainsBatteryVoltage_IsWorking // 增加处理状态检查
            );
            //返回电池模式电压
            Command_SetReturnBatteryModeVoltage = new RelayCommand(
                execute: () => ReturnBatteryModeVoltageOperation(),
                canExecute: () => Validate(nameof(ReturnBatteryModeVoltage_Inputs)) && !ReturnBatteryModeVoltage_IsWorking // 增加处理状态检查
            );
            //电池均衡间隔时间(Day)
            Command_SetBatteryBalancingInterval = new RelayCommand(
                execute: () => BatteryBalancingIntervalOperation(),
                canExecute: () => Validate(nameof(BatteryBalancingInterval_Inputs)) && !BatteryBalancingInterval_IsWorking // 增加处理状态检查
            );
            //第二输出放电时长(Min)
            Command_SetSecondOutputDischargeTime = new RelayCommand(
                execute: () => SecondOutputDischargeTimeOperation(),
                canExecute: () => Validate(nameof(SecondOutputDischargeTime_Inputs)) && !SecondOutputDischargeTime_IsWorking // 增加处理状态检查
            );
            //恢复第二输出的延时时间(Min)
            Command_SetDelayTimeToRestoreTheSecondOutput = new RelayCommand(
                execute: () => DelayTimeToRestoreTheSecondOutputOperation(),
                canExecute: () => Validate(nameof(DelayTimeToRestoreTheSecondOutput_Inputs)) && !DelayTimeToRestoreTheSecondOutput_IsWorking // 增加处理状态检查
            );
            //恢复第二输出的电池容量(%)
            Command_SetRestoreBatteryCapacityOfSecondOutput = new RelayCommand(
                execute: () => RestoreBatteryCapacityOfSecondOutputOperation(),
                canExecute: () => Validate(nameof(RestoreBatteryCapacityOfSecondOutput_Inputs)) && !RestoreBatteryCapacityOfSecondOutput_IsWorking // 增加处理状态检查
            );
            //恢复第二输出的电池容量(%)
            Command_SetRestoreBatteryVoltageOfSecondOutput = new RelayCommand(
                execute: () => RestoreBatteryVoltageOfSecondOutputOperation(),
                canExecute: () => Validate(nameof(RestoreBatteryVoltageOfSecondOutput_Inputs)) && !RestoreBatteryVoltageOfSecondOutput_IsWorking // 增加处理状态检查
            );
            //双输出模式
            Command_SetDualOutputMode = new RelayCommand(
              execute: () => DualOutputModeOperation(),
              canExecute: () => Validate(nameof(DualOutputMode_Inputs)) && !DualOutputMode_IsWorking // 增加处理状态检查
           );
            //并机模式关闭电压
            Command_SetParallelModeShutdownVoltage = new RelayCommand(
              execute: () => ParallelModeShutdownVoltageOperation(),
              canExecute: () => Validate(nameof(ParallelModeShutdownVoltage_Inputs)) && !ParallelModeShutdownVoltage_IsWorking // 增加处理状态检查
            );
            //并机模式关闭SOC
            Command_SetParallelModeShutdownSOC = new RelayCommand(
              execute: () => ParallelModeShutdownSOCOperation(),
              canExecute: () => Validate(nameof(ParallelModeShutdownSOC_Inputs)) && !ParallelModeShutdownSOC_IsWorking // 增加处理状态检查
            );
            //电池均衡模式
            Command_SetBatteryBalancingMmode = new RelayCommand(
              execute: () => BatteryBalancingMmodeOperation(),
              canExecute: () => Validate(nameof(BatteryBalancingMmode_Inputs)) && !BatteryBalancingMmode_IsWorking // 增加处理状态检查
            );
            //电池均衡电压
            Command_SetBatteryBalancingVoltage = new RelayCommand(
              execute: () => BatteryBalancingVoltageOperation(),
              canExecute: () => Validate(nameof(BatteryBalancingVoltage_Inputs)) && !BatteryBalancingVoltage_IsWorking // 增加处理状态检查
            );
            //电池均衡时间
            Command_SetBatteryBalancingTime = new RelayCommand(
              execute: () => BatteryBalancingTimeOperation(),
              canExecute: () => Validate(nameof(BatteryBalancingTime_Inputs)) && !BatteryBalancingTime_IsWorking // 增加处理状态检查
            );
            //电池均衡超时值
            Command_SetBatteryBalancingTimeoutValue = new RelayCommand(
              execute: () => BatteryBalancingTimeoutValueOperation(),
              canExecute: () => Validate(nameof(BatteryBalancingTimeoutValue_Inputs)) && !BatteryBalancingTimeoutValue_IsWorking // 增加处理状态检查
            );
            //电池均衡超时值
            Command_SetBattLowAlarmVolt = new RelayCommand(
              execute: () => BattLowAlarmVoltOperation(),
              canExecute: () => Validate(nameof(BattLowAlarmVolt_Inputs)) && !BattLowAlarmVolt_IsWorking // 增加处理状态检查
            );

        }

        #region 双输出模式(并机模式)

        //双输出模式
        private string _DualOutputMode;

        public string DualOutputMode
        {
            get { return _DualOutputMode; }
            set
            {
                if (value == "0") { _DualOutputMode = App.GetText("关闭"); }
                else if (value == "1") { _DualOutputMode = App.GetText("开启"); }
                else
                    _DualOutputMode = value;
                RaiseProperChanged(nameof(DualOutputMode));
            }
        }


        private bool DualOutputMode_IsWorking;


        //设置值
        private string _DualOutputMode_Inputs;

        public string DualOutputMode_Inputs
        {
            get { return _DualOutputMode_Inputs; }
            set
            {
                _DualOutputMode_Inputs = value;
                RaiseProperChanged(nameof(DualOutputMode_Inputs));
                Command_SetDualOutputMode.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _DualOutputModeOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> DualOutputModeOptions
        {
            get { return _DualOutputModeOptions; }
            set
            {
                _DualOutputModeOptions = value;
                RaiseProperChanged(nameof(DualOutputModeOptions));
            }
        }

        public RelayCommand Command_SetDualOutputMode { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void DualOutputModeOperation()
        {
            try
            {
                DualOutputMode_IsWorking = true;
                // 禁用按钮
                Command_SetDualOutputMode.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDAULC", getSelectedToCommad(nameof(DualOutputMode_Inputs)));

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
                DualOutputMode_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetDualOutputMode.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 并机模式关闭电压(第二输出关闭电压)

        //并机模式关闭电压
        private string _ParallelModeShutdownVoltage;

        public string ParallelModeShutdownVoltage
        {
            get { return _ParallelModeShutdownVoltage; }
            set
            {
                _ParallelModeShutdownVoltage = value;
                RaiseProperChanged(nameof(ParallelModeShutdownVoltage));
            }
        }


        private bool ParallelModeShutdownVoltage_IsWorking;


        //设置值
        private string _ParallelModeShutdownVoltage_Inputs;

        public string ParallelModeShutdownVoltage_Inputs
        {
            get { return _ParallelModeShutdownVoltage_Inputs; }
            set
            {
                _ParallelModeShutdownVoltage_Inputs = value;
                RaiseProperChanged(nameof(ParallelModeShutdownVoltage_Inputs));
                Command_SetParallelModeShutdownVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetParallelModeShutdownVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ParallelModeShutdownVoltageOperation()
        {
            try
            {
                ParallelModeShutdownVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetParallelModeShutdownVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDSDV", ParallelModeShutdownVoltage_Inputs);

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
                ParallelModeShutdownVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetParallelModeShutdownVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 并机模式关闭SOC

        //并机模式关闭SOC
        private string _ParallelModeShutdownSOC;

        public string ParallelModeShutdownSOC
        {
            get { return _ParallelModeShutdownSOC; }
            set
            {
                _ParallelModeShutdownSOC = value;
                RaiseProperChanged(nameof(ParallelModeShutdownSOC));
            }
        }


        private bool ParallelModeShutdownSOC_IsWorking;


        //设置值
        private string _ParallelModeShutdownSOC_Inputs;

        public string ParallelModeShutdownSOC_Inputs
        {
            get { return _ParallelModeShutdownSOC_Inputs; }
            set
            {
                _ParallelModeShutdownSOC_Inputs = value;
                RaiseProperChanged(nameof(ParallelModeShutdownSOC_Inputs));
                Command_SetParallelModeShutdownSOC.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetParallelModeShutdownSOC { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ParallelModeShutdownSOCOperation()
        {
            try
            {
                ParallelModeShutdownSOC_IsWorking = true;
                // 禁用按钮
                Command_SetParallelModeShutdownSOC.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDSDS", ParallelModeShutdownSOC_Inputs);

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
                ParallelModeShutdownSOC_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetParallelModeShutdownSOC.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 电池低电告警电压

        private string _BattLowAlarmVolt;

        public string BattLowAlarmVolt
        {
            get { return _BattLowAlarmVolt; }
            set
            {
                _BattLowAlarmVolt = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(BattLowAlarmVolt));
            }
        }


        private bool BattLowAlarmVolt_IsWorking;


        //设置值
        private string _BattLowAlarmVolt_Inputs;

        public string BattLowAlarmVolt_Inputs
        {
            get { return _BattLowAlarmVolt_Inputs; }
            set
            {
                _BattLowAlarmVolt_Inputs = value;
                this.RaiseProperChanged(nameof(BattLowAlarmVolt_Inputs));
                Command_SetBattLowAlarmVolt.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBattLowAlarmVolt { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BattLowAlarmVoltOperation()
        {
            try
            {
                BattLowAlarmVolt_IsWorking = true;
                // 禁用按钮
                Command_SetBattLowAlarmVolt.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", BattLowAlarmVolt_Inputs);

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
                BattLowAlarmVolt_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBattLowAlarmVolt.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 返回市电电池电压

        //返回市电电池电压
        private string _ReturnMainsBatteryVoltage;

        public string ReturnMainsBatteryVoltage
        {
            get { return _ReturnMainsBatteryVoltage; }
            set
            {
                _ReturnMainsBatteryVoltage = value;
                RaiseProperChanged(nameof(ReturnMainsBatteryVoltage));
            }
        }


        private bool ReturnMainsBatteryVoltage_IsWorking;


        //设置值
        private string _ReturnMainsBatteryVoltage_Inputs;

        public string ReturnMainsBatteryVoltage_Inputs
        {
            get { return _ReturnMainsBatteryVoltage_Inputs; }
            set
            {
                _ReturnMainsBatteryVoltage_Inputs = value;
                RaiseProperChanged(nameof(ReturnMainsBatteryVoltage_Inputs));
                Command_SetReturnMainsBatteryVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetReturnMainsBatteryVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ReturnMainsBatteryVoltageOperation()
        {
            try
            {
                ReturnMainsBatteryVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetReturnMainsBatteryVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBCV", ReturnMainsBatteryVoltage_Inputs);

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
                ReturnMainsBatteryVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetReturnMainsBatteryVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 返回电池模式电压    

        //返回电池模式电压
        private string _ReturnBatteryModeVoltage;

        public string ReturnBatteryModeVoltage
        {
            get { return _ReturnBatteryModeVoltage; }
            set
            {
                _ReturnBatteryModeVoltage = value;
                RaiseProperChanged(nameof(ReturnBatteryModeVoltage));
            }
        }


        private bool ReturnBatteryModeVoltage_IsWorking;


        //设置值
        private string _ReturnBatteryModeVoltage_Inputs;

        public string ReturnBatteryModeVoltage_Inputs
        {
            get { return _ReturnBatteryModeVoltage_Inputs; }
            set
            {
                _ReturnBatteryModeVoltage_Inputs = value;
                RaiseProperChanged(nameof(ReturnBatteryModeVoltage_Inputs));
                Command_SetReturnBatteryModeVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetReturnBatteryModeVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ReturnBatteryModeVoltageOperation()
        {
            try
            {
                ReturnBatteryModeVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetReturnBatteryModeVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBDV", ReturnBatteryModeVoltage_Inputs);

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
                ReturnBatteryModeVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetReturnBatteryModeVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 电池均衡模式

        //电池均衡模式
        private string _BatteryBalancingMmode;

        public string BatteryBalancingMmode
        {
            get { return _BatteryBalancingMmode; }
            set
            {
                if (value == "0") { _BatteryBalancingMmode = App.GetText("关闭"); }
                else if (value == "1") { _BatteryBalancingMmode = App.GetText("开启"); }
                else
                    _BatteryBalancingMmode = value;
                RaiseProperChanged(nameof(BatteryBalancingMmode));
            }
        }


        private bool BatteryBalancingMmode_IsWorking;


        //设置值
        private string _BatteryBalancingMmode_Inputs;

        public string BatteryBalancingMmode_Inputs
        {
            get { return _BatteryBalancingMmode_Inputs; }
            set
            {
                _BatteryBalancingMmode_Inputs = value;
                RaiseProperChanged(nameof(BatteryBalancingMmode_Inputs));
                Command_SetBatteryBalancingMmode.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _BatteryBalancingMmodeOptions = new List<string> { "开启/On", "关闭/Off" };

        public List<string> BatteryBalancingMmodeOptions
        {
            get { return _BatteryBalancingMmodeOptions; }
            set
            {
                _BatteryBalancingMmodeOptions = value;
                RaiseProperChanged(nameof(BatteryBalancingMmodeOptions));
            }
        }

        public RelayCommand Command_SetBatteryBalancingMmode { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BatteryBalancingMmodeOperation()
        {
            try
            {
                BatteryBalancingMmode_IsWorking = true;
                // 禁用按钮
                Command_SetBatteryBalancingMmode.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBEQE", getSelectedToCommad(nameof(BatteryBalancingMmode_Inputs)));

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
                BatteryBalancingMmode_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBatteryBalancingMmode.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 电池均衡电压

        //电池均衡电压
        private string _BatteryBalancingVoltage;

        public string BatteryBalancingVoltage
        {
            get { return _BatteryBalancingVoltage; }
            set
            {
                _BatteryBalancingVoltage = value;
                RaiseProperChanged(nameof(BatteryBalancingVoltage));
            }
        }


        private bool BatteryBalancingVoltage_IsWorking;


        //设置值
        private string _BatteryBalancingVoltage_Inputs;

        public string BatteryBalancingVoltage_Inputs
        {
            get { return _BatteryBalancingVoltage_Inputs; }
            set
            {
                _BatteryBalancingVoltage_Inputs = value;
                RaiseProperChanged(nameof(BatteryBalancingVoltage_Inputs));
                Command_SetBatteryBalancingVoltage.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBatteryBalancingVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BatteryBalancingVoltageOperation()
        {
            try
            {
                BatteryBalancingVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetBatteryBalancingVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBEQV", BatteryBalancingVoltage_Inputs);

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
                BatteryBalancingVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBatteryBalancingVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 电池均衡时间

        //电池均衡时间
        private string _BatteryBalancingTime;

        public string BatteryBalancingTime
        {
            get { return _BatteryBalancingTime; }
            set
            {
                _BatteryBalancingTime = value;
                RaiseProperChanged(nameof(BatteryBalancingTime));
            }
        }


        private bool BatteryBalancingTime_IsWorking;


        //设置值
        private string _BatteryBalancingTime_Inputs;

        public string BatteryBalancingTime_Inputs
        {
            get { return _BatteryBalancingTime_Inputs; }
            set
            {
                _BatteryBalancingTime_Inputs = value;
                RaiseProperChanged(nameof(BatteryBalancingTime_Inputs));
                Command_SetBatteryBalancingTime.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBatteryBalancingTime { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BatteryBalancingTimeOperation()
        {
            try
            {
                BatteryBalancingTime_IsWorking = true;
                // 禁用按钮
                Command_SetBatteryBalancingTime.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBEQT", BatteryBalancingTime_Inputs);

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
                BatteryBalancingTime_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBatteryBalancingTime.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 电池均衡超时值

        //电池均衡超时值
        private string _BatteryBalancingTimeoutValue;

        public string BatteryBalancingTimeoutValue
        {
            get { return _BatteryBalancingTimeoutValue; }
            set
            {
                _BatteryBalancingTimeoutValue = value;
                RaiseProperChanged(nameof(BatteryBalancingTimeoutValue));
            }
        }


        private bool BatteryBalancingTimeoutValue_IsWorking;


        //设置值
        private string _BatteryBalancingTimeoutValue_Inputs;

        public string BatteryBalancingTimeoutValue_Inputs
        {
            get { return _BatteryBalancingTimeoutValue_Inputs; }
            set
            {
                _BatteryBalancingTimeoutValue_Inputs = value;
                RaiseProperChanged(nameof(BatteryBalancingTimeoutValue_Inputs));
                Command_SetBatteryBalancingTimeoutValue.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBatteryBalancingTimeoutValue { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BatteryBalancingTimeoutValueOperation()
        {
            try
            {
                BatteryBalancingTimeoutValue_IsWorking = true;
                // 禁用按钮
                Command_SetBatteryBalancingTimeoutValue.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBEQOT", BatteryBalancingTimeoutValue_Inputs);

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
                BatteryBalancingTimeoutValue_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBatteryBalancingTimeoutValue.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 电池均衡间隔时间(Day)

        //电池均衡间隔时间 天
        private string _BatteryBalancingInterval;

        public string BatteryBalancingInterval
        {
            get { return _BatteryBalancingInterval; }
            set
            {
                _BatteryBalancingInterval = value;
                RaiseProperChanged(nameof(BatteryBalancingInterval));
            }
        }


        private bool BatteryBalancingInterval_IsWorking;


        //设置值
        private string _BatteryBalancingInterval_Inputs;

        public string BatteryBalancingInterval_Inputs
        {
            get { return _BatteryBalancingInterval_Inputs; }
            set
            {
                _BatteryBalancingInterval_Inputs = value;
                RaiseProperChanged(nameof(BatteryBalancingInterval_Inputs));
                Command_SetBatteryBalancingInterval.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBatteryBalancingInterval { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BatteryBalancingIntervalOperation()
        {
            try
            {
                BatteryBalancingInterval_IsWorking = true;
                // 禁用按钮
                Command_SetBatteryBalancingInterval.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PBEQP", BatteryBalancingInterval_Inputs);

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
                BatteryBalancingInterval_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBatteryBalancingInterval.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 输出第二放电时间

        //第二输出放电时间
        private string _SecondOutputDischargeTime;

        public string SecondOutputDischargeTime
        {
            get { return _SecondOutputDischargeTime; }
            set
            {
                _SecondOutputDischargeTime = value;
                RaiseProperChanged(nameof(SecondOutputDischargeTime));
            }
        }


        private bool SecondOutputDischargeTime_IsWorking;


        //设置值
        private string _SecondOutputDischargeTime_Inputs;

        public string SecondOutputDischargeTime_Inputs
        {
            get { return _SecondOutputDischargeTime_Inputs; }
            set
            {
                _SecondOutputDischargeTime_Inputs = value;
                RaiseProperChanged(nameof(SecondOutputDischargeTime_Inputs));
                Command_SetSecondOutputDischargeTime.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetSecondOutputDischargeTime { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void SecondOutputDischargeTimeOperation()
        {
            try
            {
                SecondOutputDischargeTime_IsWorking = true;
                // 禁用按钮
                Command_SetSecondOutputDischargeTime.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDDCGT", SecondOutputDischargeTime_Inputs);

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
                SecondOutputDischargeTime_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetSecondOutputDischargeTime.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 恢复第二输出的延时时间

        //第二输出的延时时间
        private string _DelayTimeToRestoreTheSecondOutput;

        public string DelayTimeToRestoreTheSecondOutput
        {
            get { return _DelayTimeToRestoreTheSecondOutput; }
            set
            {
                _DelayTimeToRestoreTheSecondOutput = value;
                RaiseProperChanged(nameof(DelayTimeToRestoreTheSecondOutput));
            }
        }


        private bool DelayTimeToRestoreTheSecondOutput_IsWorking;


        //设置值
        private string _DelayTimeToRestoreTheSecondOutput_Inputs;

        public string DelayTimeToRestoreTheSecondOutput_Inputs
        {
            get { return _DelayTimeToRestoreTheSecondOutput_Inputs; }
            set
            {
                _DelayTimeToRestoreTheSecondOutput_Inputs = value;
                RaiseProperChanged(nameof(DelayTimeToRestoreTheSecondOutput_Inputs));
                Command_SetDelayTimeToRestoreTheSecondOutput.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetDelayTimeToRestoreTheSecondOutput { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void DelayTimeToRestoreTheSecondOutputOperation()
        {
            try
            {
                DelayTimeToRestoreTheSecondOutput_IsWorking = true;
                // 禁用按钮
                Command_SetDelayTimeToRestoreTheSecondOutput.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDDLYT", DelayTimeToRestoreTheSecondOutput_Inputs);

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
                DelayTimeToRestoreTheSecondOutput_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetDelayTimeToRestoreTheSecondOutput.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region 恢复第二输出的电池电压

        //第二输出的电池电压
        private string _RestoreBatteryVoltageOfSecondOutput;

        public string RestoreBatteryVoltageOfSecondOutput
        {
            get { return _RestoreBatteryVoltageOfSecondOutput; }
            set
            {
                _RestoreBatteryVoltageOfSecondOutput = value;
                RaiseProperChanged(nameof(RestoreBatteryVoltageOfSecondOutput));
            }
        }


        private bool RestoreBatteryVoltageOfSecondOutput_IsWorking;


        //设置值
        private string _RestoreBatteryVoltageOfSecondOutput_Inputs;

        public string RestoreBatteryVoltageOfSecondOutput_Inputs
        {
            get { return _RestoreBatteryVoltageOfSecondOutput_Inputs; }
            set
            {
                _RestoreBatteryVoltageOfSecondOutput_Inputs = value;
                RaiseProperChanged(nameof(RestoreBatteryVoltageOfSecondOutput_Inputs));
                Command_SetRestoreBatteryVoltageOfSecondOutput.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetRestoreBatteryVoltageOfSecondOutput { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void RestoreBatteryVoltageOfSecondOutputOperation()
        {
            try
            {
                RestoreBatteryVoltageOfSecondOutput_IsWorking = true;
                // 禁用按钮
                Command_SetRestoreBatteryVoltageOfSecondOutput.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDSRV", RestoreBatteryVoltageOfSecondOutput_Inputs);

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
                RestoreBatteryVoltageOfSecondOutput_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetRestoreBatteryVoltageOfSecondOutput.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 恢复第二输出的电池容量

        //第二输出的电池容量
        private string _RestoreBatteryCapacityOfSecondOutput;

        public string RestoreBatteryCapacityOfSecondOutput
        {
            get { return _RestoreBatteryCapacityOfSecondOutput; }
            set
            {
                _RestoreBatteryCapacityOfSecondOutput = value;
                RaiseProperChanged(nameof(RestoreBatteryCapacityOfSecondOutput));
            }
        }


        private bool RestoreBatteryCapacityOfSecondOutput_IsWorking;


        //设置值
        private string _RestoreBatteryCapacityOfSecondOutput_Inputs;

        public string RestoreBatteryCapacityOfSecondOutput_Inputs
        {
            get { return _RestoreBatteryCapacityOfSecondOutput_Inputs; }
            set
            {
                _RestoreBatteryCapacityOfSecondOutput_Inputs = value;
                RaiseProperChanged(nameof(RestoreBatteryCapacityOfSecondOutput_Inputs));
                Command_SetRestoreBatteryCapacityOfSecondOutput.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetRestoreBatteryCapacityOfSecondOutput { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void RestoreBatteryCapacityOfSecondOutputOperation()
        {
            try
            {
                RestoreBatteryCapacityOfSecondOutput_IsWorking = true;
                // 禁用按钮
                Command_SetRestoreBatteryCapacityOfSecondOutput.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("PDSRS", RestoreBatteryCapacityOfSecondOutput_Inputs);

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
                RestoreBatteryCapacityOfSecondOutput_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetRestoreBatteryCapacityOfSecondOutput.RaiseCanExecuteChanged();
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
            switch (value)
            {
                //返回市电电池电压
                case "ReturnMainsBatteryVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(ReturnMainsBatteryVoltage_Inputs);
                //返回电池模式电压
                case "ReturnBatteryModeVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(ReturnBatteryModeVoltage_Inputs);
                //均衡间隔时间(Day)
                case "BatteryBalancingInterval_Inputs":
                    return !string.IsNullOrWhiteSpace(BatteryBalancingInterval_Inputs);
                //第二输出放电时间(Min)
                case "SecondOutputDischargeTime_Inputs":
                    return !string.IsNullOrWhiteSpace(SecondOutputDischargeTime_Inputs);
                //恢复第二输出的延时时间(Min)
                case "DelayTimeToRestoreTheSecondOutput_Inputs":
                    return !string.IsNullOrWhiteSpace(DelayTimeToRestoreTheSecondOutput_Inputs);
                //恢复第二输出的电池容量(%)
                case "RestoreBatteryCapacityOfSecondOutput_Inputs":
                    return !string.IsNullOrWhiteSpace(RestoreBatteryCapacityOfSecondOutput_Inputs);
                //恢复第二输出的电池电压(V)
                case "RestoreBatteryVoltageOfSecondOutput_Inputs":
                    return !string.IsNullOrWhiteSpace(RestoreBatteryVoltageOfSecondOutput_Inputs);
                case "DualOutputMode_Inputs":
                    return !string.IsNullOrWhiteSpace(DualOutputMode_Inputs);
                case "ParallelModeShutdownVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(ParallelModeShutdownVoltage_Inputs);
                case "ParallelModeShutdownSOC_Inputs":
                    return !string.IsNullOrWhiteSpace(ParallelModeShutdownSOC_Inputs);
                case "BatteryBalancingMmode_Inputs":
                    return !string.IsNullOrWhiteSpace(BatteryBalancingMmode_Inputs);
                case "BatteryBalancingVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(BatteryBalancingVoltage_Inputs);
                case "BatteryBalancingTime_Inputs":
                    return !string.IsNullOrWhiteSpace(BatteryBalancingTime_Inputs);
                case "BatteryBalancingTimeoutValue_Inputs":
                    return !string.IsNullOrWhiteSpace(BatteryBalancingTimeoutValue_Inputs); 
                case "BattLowAlarmVolt_Inputs":
                    return !string.IsNullOrWhiteSpace(BattLowAlarmVolt_Inputs);
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

                //双输出模式(并机模式)
                DualOutputMode = Values[0].Substring(1, 1);
                //并机模式关闭电压
                ParallelModeShutdownVoltage = Values[1];
                //并机模式关闭SOC
                ParallelModeShutdownSOC = Values[2];
                //电池低电告警电压
                BattLowAlarmVolt = Values[3];
                //返回市电模式电压
                ReturnMainsBatteryVoltage = Values[4];
                //返回电池模式电压
                ReturnBatteryModeVoltage = Values[5];
                //电池均衡模式
                BatteryBalancingMmode = Values[6];
                //电池均衡电压
                BatteryBalancingVoltage = Values[7];
                //电池均衡时间
                BatteryBalancingTime = Values[8];
                //电池均衡超时值
                BatteryBalancingTimeoutValue = Values[9];
                //电池均衡间隔时间(Day)
                BatteryBalancingInterval = Values[10];
               
                //恢复第二输出的延时时间
                DelayTimeToRestoreTheSecondOutput = Values[13];
                //恢复第二输出的电电池电压
                RestoreBatteryVoltageOfSecondOutput = Values[15];
                //恢复第二输出的电池容量
                RestoreBatteryCapacityOfSecondOutput = Values[16].Substring(0, 2);
                //第二输出放电时间
                SecondOutputDischargeTime = Values[16].Substring(2, 3);
                ////强充电压
                //StrongChargeVoltage = Values[15];
                ////浮充电压
                //FloatChargeVolage = Values[16];
                ////低电锁机电压
                //LowPowerLock = Values[17];
            }
            catch (Exception ex)
            {
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
                //设置双输出模式(并机模式)
                case "DualOutputMode_Inputs":
                    if (string.IsNullOrWhiteSpace(DualOutputMode_Inputs))
                    {
                        return string.Empty;
                    }
                    else if (DualOutputMode_Inputs == "开启/On")
                    {
                        return "01";
                    }
                    else if (DualOutputMode_Inputs == "关闭/Off")
                    {
                        return "00";
                    }
                    return DualOutputMode_Inputs;
                //电池均衡模式
                case "BatteryBalancingMmode_Inputs":

                    if (string.IsNullOrWhiteSpace(BatteryBalancingMmode_Inputs))
                    {
                        return string.Empty;
                    }
                    else if (BatteryBalancingMmode_Inputs == "开启/On")
                    {
                        return "1";
                    }
                    else if (BatteryBalancingMmode_Inputs == "关闭Off")
                    {
                        return "0";
                    }
                    return DualOutputMode_Inputs;
                default:
                    return "";
            }
        }

        #endregion
    }
}
