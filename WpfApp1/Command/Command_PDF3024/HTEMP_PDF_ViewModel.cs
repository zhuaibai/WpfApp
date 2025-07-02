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
    public class HTEMP_PDF_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HTEMP\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HTEMP_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令

            //逆变温度
            Command_SetInvTemp = new RelayCommand(
                execute: () => InvTempOperation(),
                canExecute: () => Validate(nameof(InvTemp_Inputs)) && !InvTemp_IsWorking // 增加处理状态检查
            );
            //当前最高温度
            Command_SetMaxTemp = new RelayCommand(
               execute: () => MaxTempOperation(),
               canExecute: () => Validate(nameof(MaxTemp_Inputs)) && !MaxTemp_IsWorking // 增加处理状态检查
            );
            //风扇转速
            Command_SetFanSpeed = new RelayCommand(
               execute: () => FanSpeedOperation(),
               canExecute: () => Validate(nameof(FanSpeed_Inputs)) && !FanSpeed_IsWorking // 增加处理状态检查
            );
            //风扇开关
            Command_SetFanEnable = new RelayCommand(
               execute: () => FanEnableOperation(),
               canExecute: () => Validate(nameof(FanEnable_Inputs)) && !FanEnable_IsWorking // 增加处理状态检查
            );

            #endregion
        }

        #region PV温度

        private string _PVTemp;

        public string PVTemp
        {
            get { return _PVTemp; }
            set
            {
                _PVTemp = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(PVTemp));
            }
        }

        #endregion

        #region 逆变温度

        //逆变温度
        private string _InvTemp;

        public string InvTemp
        {
            get { return _InvTemp; }
            set
            {
                _InvTemp = Tools.RemoveLeadingZeros(value) + "℃";
                this.RaiseProperChanged(nameof(InvTemp));
            }
        }


        private bool InvTemp_IsWorking;


        //设置值
        private string _InvTemp_Inputs;

        public string InvTemp_Inputs
        {
            get { return _InvTemp_Inputs; }
            set
            {
                _InvTemp_Inputs = value;
                this.RaiseProperChanged(nameof(InvTemp_Inputs));
                Command_SetInvTemp.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetInvTemp { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InvTempOperation()
        {
            try
            {
                InvTemp_IsWorking = true;
                // 禁用按钮
                Command_SetInvTemp.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", InvTemp_Inputs);

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
                InvTemp_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInvTemp.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }




        #endregion

        #region 升压温度

        private string _BoostTemp;

        public string BoostTemp
        {
            get { return _BoostTemp; }
            set
            {
                _BoostTemp = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(BoostTemp));
            }
        }

        #endregion

        #region 变压器温度

        private string _XfmrTemp;

        public string XfmrTemp
        {
            get { return _XfmrTemp; }
            set
            {
                _XfmrTemp = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(XfmrTemp));
            }
        }

        #endregion

        #region 当前最高温度

        //当前最高温度
        private string _MaxTemp;

        public string MaxTemp
        {
            get { return _MaxTemp; }
            set
            {
                _MaxTemp = Tools.RemoveLeadingZeros(value) + "℃";
                this.RaiseProperChanged(nameof(MaxTemp));
            }
        }


        private bool MaxTemp_IsWorking;


        //设置值
        private string _MaxTemp_Inputs;

        public string MaxTemp_Inputs
        {
            get { return _MaxTemp_Inputs; }
            set
            {
                _MaxTemp_Inputs = value;
                this.RaiseProperChanged(nameof(MaxTemp_Inputs));
                Command_SetMaxTemp.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetMaxTemp { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void MaxTempOperation()
        {
            try
            {
                MaxTemp_IsWorking = true;
                // 禁用按钮
                Command_SetMaxTemp.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", MaxTemp_Inputs);

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
                MaxTemp_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetMaxTemp.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 风扇转速

        private string _FanSpeed;

        public string FanSpeed
        {
            get { return _FanSpeed; }
            set
            {
                if(FanEnable=="开启"||FanEnable == "On") { _FanSpeed = Tools.RemoveLeadingZeros(value); }
                else 
                _FanSpeed = "0";
                this.RaiseProperChanged(nameof(FanSpeed));
            }
        }


        private bool FanSpeed_IsWorking;


        //设置值
        private string _FanSpeed_Inputs;

        public string FanSpeed_Inputs
        {
            get { return _FanSpeed_Inputs; }
            set
            {
                _FanSpeed_Inputs = value;
                this.RaiseProperChanged(nameof(FanSpeed_Inputs));
                Command_SetFanSpeed.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetFanSpeed { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void FanSpeedOperation()
        {
            try
            {
                FanSpeed_IsWorking = true;
                // 禁用按钮
                Command_SetFanSpeed.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", FanSpeed_Inputs);

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
                FanSpeed_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetFanSpeed.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 风扇开关

        private string _FanEnable;

        public string FanEnable
        {
            get { return _FanEnable; }
            set
            {
                if(value == "0") { _FanEnable = App.GetText("关闭"); }
                else if (value == "1") { _FanEnable = App.GetText("开启"); }
                else
                _FanEnable = value;
                this.RaiseProperChanged(nameof(FanEnable));
            }
        }


        private bool FanEnable_IsWorking;


        //设置值
        private string _FanEnable_Inputs;

        public string FanEnable_Inputs
        {
            get { return _FanEnable_Inputs; }
            set
            {
                _FanEnable_Inputs = value;
                this.RaiseProperChanged(nameof(FanEnable_Inputs));
                Command_SetFanEnable.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _FanEnableOptions = new List<string> { "开启", "关闭" };

        public List<string> FanEnableOptions
        {
            get { return _FanEnableOptions; }
            set
            {
                _FanEnableOptions = value;
                this.RaiseProperChanged(nameof(FanEnableOptions));
            }
        }

        public RelayCommand Command_SetFanEnable { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void FanEnableOperation()
        {
            try
            {
                FanEnable_IsWorking = true;
                // 禁用按钮
                Command_SetFanEnable.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", FanEnable_Inputs);

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
                FanEnable_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetFanEnable.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        
        #region 通用方法

        private bool Validate(string value)
        {
            switch (value)
            {
                //逆变电压     
                case "InvTemp_Inputs":
                    return !string.IsNullOrWhiteSpace(InvTemp_Inputs);
                //当前最大温度   
                case "MaxTemp_Inputs":
                    return !string.IsNullOrWhiteSpace(MaxTemp_Inputs);
                //风扇转速   
                case "FanSpeed_Inputs":
                    return !string.IsNullOrWhiteSpace(FanSpeed_Inputs);
                //风扇开关 
                case "FanEnable_Inputs":
                    return !string.IsNullOrWhiteSpace(FanEnable_Inputs);
                ////温度限制 
                //case "MaxInvPower_Inputs":
                //    return !string.IsNullOrWhiteSpace(MaxInvPower_Inputs);


                default:
                    return false;

            }
        }


        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalysisStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                ReceiveException("空");
                return;
            }
            if(value == "-1")
            {
                ReceiveException("CRC异常");
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //PV温度
                PVTemp = Values[0].Substring(1);
                //逆变温度
                InvTemp = Values[1];
                //升压温度
                FanSpeed = Values[2];
                //变压器温度
                XfmrTemp = Values[3];
                //当前最大温度
                MaxTemp = Values[4];
                //风扇开关
                FanEnable = Values[7].Substring(0, 1);
                //风扇转速
                FanSpeed = Values[5];

            }
            catch (Exception ex)
            {
                ReceiveException("HTEMP异常");
            }
        }

        /// <summary>
        /// 接收异常使用方法
        /// </summary>
        /// <param name="exceptionDescription"></param>
        private void ReceiveException(string exceptionDescription)
        {
            //PV温度
            PVTemp = exceptionDescription;
            //逆变温度
            InvTemp = exceptionDescription;
            //升压温度
            FanSpeed = exceptionDescription;
            //变压器温度
            XfmrTemp = exceptionDescription;
            //当前最大温度
            MaxTemp = exceptionDescription;
            //风扇转速
            FanSpeed = exceptionDescription;
            //风扇开关
            FanEnable = exceptionDescription;
        }
        #endregion
    }
}
