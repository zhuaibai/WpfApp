using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Command;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.GB3024C_Comand
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
                execute: () => ReturnMainsBatteryVoltageOperation(),
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
        }

        #region 返回市电电池电压

        //返回市电电池电压
        private string _ReturnMainsBatteryVoltage;

        public string ReturnMainsBatteryVoltage
        {
            get { return _ReturnMainsBatteryVoltage; }
            set
            {
                _ReturnMainsBatteryVoltage = value;
                this.RaiseProperChanged(nameof(ReturnMainsBatteryVoltage));
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
                this.RaiseProperChanged(nameof(ReturnMainsBatteryVoltage_Inputs));
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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ReturnMainsBatteryVoltage_Inputs);

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
                this.RaiseProperChanged(nameof(ReturnBatteryModeVoltage));
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
                this.RaiseProperChanged(nameof(ReturnBatteryModeVoltage_Inputs));
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
                    //Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ReturnBatteryModeVoltage_Inputs);

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

        #region 电池均衡间隔时间(Day)

        //电池均衡间隔时间 天
        private string _BatteryBalancingInterval;

        public string BatteryBalancingInterval
        {
            get { return _BatteryBalancingInterval; }
            set
            {
                _BatteryBalancingInterval = value;
                this.RaiseProperChanged(nameof(BatteryBalancingInterval));
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
                this.RaiseProperChanged(nameof(BatteryBalancingInterval_Inputs));
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
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", BatteryBalancingInterval_Inputs);

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
                this.RaiseProperChanged(nameof(SecondOutputDischargeTime));
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
                this.RaiseProperChanged(nameof(SecondOutputDischargeTime_Inputs));
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
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", SecondOutputDischargeTime_Inputs);

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
                this.RaiseProperChanged(nameof(DelayTimeToRestoreTheSecondOutput));
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
                this.RaiseProperChanged(nameof(DelayTimeToRestoreTheSecondOutput_Inputs));
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
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", DelayTimeToRestoreTheSecondOutput_Inputs);

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
                this.RaiseProperChanged(nameof(RestoreBatteryVoltageOfSecondOutput));
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
                this.RaiseProperChanged(nameof(RestoreBatteryVoltageOfSecondOutput_Inputs));
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
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", RestoreBatteryVoltageOfSecondOutput_Inputs);

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
            get { return  _RestoreBatteryCapacityOfSecondOutput; }
            set
            {
                 _RestoreBatteryCapacityOfSecondOutput = value;
                this.RaiseProperChanged(nameof(RestoreBatteryCapacityOfSecondOutput));
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
                this.RaiseProperChanged(nameof(RestoreBatteryCapacityOfSecondOutput_Inputs));
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
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", RestoreBatteryCapacityOfSecondOutput_Inputs);

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
                ////工作模式
                //WorkingMode = Values[0];
                ////总充电流
                //TotalChargeCurrent = Values[1];
                ////市电总充电流
                //AC_ChargingCurrent = Values[2];
                ////电池类型
                //BatteryType = Values[5];
                //电池均衡间隔时间(Day)
                BatteryBalancingInterval = Values[10];
                //第二输出放电时间
                SecondOutputDischargeTime = Values[16].Substring(2, 3);
                //恢复第二输出的延时时间
                DelayTimeToRestoreTheSecondOutput = Values[13];
                //恢复第二输出的电电池电压
                RestoreBatteryVoltageOfSecondOutput = Values[15];
                //恢复第二输出的电池容量
                RestoreBatteryCapacityOfSecondOutput = Values[16].Substring(0, 2);
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
                case "SBU":
                    return "";
                default:
                    return "";
            }
        }

        #endregion
    }
}
