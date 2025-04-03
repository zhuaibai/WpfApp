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
                case "ReturnMainsBatteryVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(ReturnMainsBatteryVoltage_Inputs);
                case "ReturnBatteryModeVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(ReturnBatteryModeVoltage_Inputs);
                //case "BatteryTypeSelectedOption":
                //    return !string.IsNullOrWhiteSpace(BatteryTypeSelectedOption);
                //case "TotalChargeCurrent_Inputs":
                //    return !string.IsNullOrWhiteSpace(TotalChargeCurrent_Inputs);
                //case "AC_ChargingCurrent_Inputs":
                //    return !string.IsNullOrWhiteSpace(AC_ChargingCurrent_Inputs);
                //case "StrongChargeVoltage_Inputs":
                //    return !string.IsNullOrWhiteSpace(StrongChargeVoltage_Inputs);
                //case "FloatChargeVolage_Inputs":
                //    return !string.IsNullOrWhiteSpace(FloatChargeVolage_Inputs);
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
                ////设置工作模式
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
