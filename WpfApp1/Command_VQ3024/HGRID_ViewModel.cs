using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Command;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command_VQ3024
{
    public class HGRID_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HGRID\r";
        public string Command { get { return command; } }

        public string MachineType;

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pauseEvent">线程暂停控制</param>
        /// <param name="semaphore">资源竞争</param>
        /// <param name="addLog">添加日志日志</param>
        /// <param name="_updateState">更新串口状态</param>
        public HGRID_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
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
            ////无功功率
            //Command_SetReactivePower = new RelayCommand(
            //    execute: () => ReactivePowerOperation(),
            //    canExecute: () => Validate(nameof(ReactivePower_Inputs)) && !ReactivePower_IsWorking // 增加处理状态检查
            //);
            ////有功功率
            //Command_SetActivePower = new RelayCommand(
            //    execute: () => ActivePowerOperation(),
            //    canExecute: () => Validate(nameof(ActivePower_Inputs)) && !ActivePower_IsWorking // 增加处理状态检查
            //);
            ////负载百分比
            //Command_SetPercentageOfLoad = new RelayCommand(
            //    execute: () => ActivePowerOperation(),
            //    canExecute: () => Validate(nameof(PercentageOfLoad_Inputs)) && !PercentageOfLoad_IsWorking // 增加处理状态检查
            //);
            ////逆变电流
            //Command_SetInverterCurrent = new RelayCommand(
            //    execute: () => InverterCurrentOperation(),
            //    canExecute: () => Validate(nameof(InverterCurrent_Inputs)) && !InverterCurrent_IsWorking // 增加处理状态检查
            //);
            ////Mos管电流
            //Command_SetInverterCurrent = new RelayCommand(
            //    execute: () => MOS_TubeCurrentOperation(),
            //    canExecute: () => Validate(nameof(MOS_TubeCurrent_Inputs)) && !MOS_TubeCurrent_IsWorking // 增加处理状态检查
            //);

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
                _MainsVoltage = value;
                this.RaiseProperChanged(nameof(MainsVoltage));
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
                this.RaiseProperChanged(nameof(MainsVoltage_Inputs));
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
                this.RaiseProperChanged(nameof(MainsFrequency));
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
                this.RaiseProperChanged(nameof(MainsFrequency_Inputs));
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
                    Thread.Sleep(2000);//没有这个延时会报错
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
                ////无功功率  
                //case "ReactivePower_Inputs":
                //    return !string.IsNullOrWhiteSpace(ReactivePower_Inputs);
                ////有功功率  
                //case "ActivePower_Inputs":
                //    return !string.IsNullOrWhiteSpace(ActivePower_Inputs);
                ////负载百分比
                //case "PercentageOfLoad_Inputs":
                //    return !string.IsNullOrWhiteSpace(PercentageOfLoad_Inputs);
                ////逆变电流
                //case "InverterCurrent_Inputs":
                //    return !string.IsNullOrWhiteSpace(InverterCurrent_Inputs);
                ////Mos管电流
                //case "MOS_TubeCurrent_Inputs":
                //    return !string.IsNullOrWhiteSpace(MOS_TubeCurrent_Inputs);

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
                MainsVoltage = Values[0].Substring(1,5);
                //市电频率
                MainsFrequency = Values[1];
                ////无功功率
                //ReactivePower = Values[2];
                ////有功功率
                //ActivePower = Values[3];
                ////负载百分比
                //PercentageOfLoad = Values[4];
                ////逆变电流
                //InverterCurrent = Values[7];
                ////Mos管电流
                //MOS_TubeCurrent = Values[8];

            }
            catch (Exception ex)
            {

            }
        }
    }
}
