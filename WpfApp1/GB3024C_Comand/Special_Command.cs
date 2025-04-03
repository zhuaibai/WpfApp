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
    public class Special_Command:BaseViewModel
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

        }


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
                this.RaiseProperChanged(nameof(ConstantVoltageCV));
            }
        }

        private bool _ConstantVoltageCVIsWorking;

        /// <summary>
        /// 设置值
        /// </summary>
        private string _ConstantVoltageCV_Inputs    ;

        public string ConstantVoltageCV_Inputs
        {
            get { return _ConstantVoltageCV_Inputs; }
            set
            {
                _ConstantVoltageCV_Inputs = value;
                this.RaiseProperChanged(nameof(ConstantVoltageCV_Inputs));
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

        private string QBFT_command  = "QBFT";

        public string QBFT_Command
        {
            get { return QBFT_command ; }
            
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
                this.RaiseProperChanged(nameof(FloatChargeCV));
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
                this.RaiseProperChanged(nameof(FloatChargeCV_Inputs));
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
                this.RaiseProperChanged(nameof(AC_ChargingVoltage));
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
                this.RaiseProperChanged(nameof(AC_ChargingVoltage_Inputs));
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
                this.RaiseProperChanged(nameof(BatteryDischargeVoltage));
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
                this.RaiseProperChanged(nameof(BatteryDischargeVoltage_Inputs));
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
                //设置工作模式
                //case "WorkingModeSelectedOption":
                //    if (string.IsNullOrWhiteSpace(WorkingModeSelectedOption))
                //    {
                //        return string.Empty;
                //    }
                //    else if (WorkingModeSelectedOption == "UTI")
                //    {
                //        return "00";
                //    }
                //    else if (WorkingModeSelectedOption == "SUB")
                //    {
                //        return "01";
                //    }
                //    return "";
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
