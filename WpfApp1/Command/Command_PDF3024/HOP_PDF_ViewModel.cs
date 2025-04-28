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
    public class HOP_PDF_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HOP\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HOP_PDF_ViewModel( ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令
            //输出电压
            Command_SetOutVolt = new RelayCommand(
                execute: () => OutVoltOperation(),
                canExecute: () => Validate(nameof(OutVolt_Inputs)) && !OutVolt_IsWorking // 增加处理状态检查
            );
            //输出频率
            Command_SetOutFreq = new RelayCommand(
                execute: () => OutFreqOperation(),
                canExecute: () => Validate(nameof(OutFreq_Inputs)) && !OutFreq_IsWorking // 增加处理状态检查
            );
            //视在功率
            Command_SetApparentPwr = new RelayCommand(
                execute: () => ApparentPwrOperation(),
                canExecute: () => Validate(nameof(ApparentPwr_Inputs)) && !ApparentPwr_IsWorking // 增加处理状态检查
            );
            //有功功率
            Command_SetActivePwr = new RelayCommand(
                execute: () => ActivePwrOperation(),
                canExecute: () => Validate(nameof(ActivePwr_Inputs)) && !ActivePwr_IsWorking // 增加处理状态检查
            );
            //负载百分比
            Command_SetLoadPercent = new RelayCommand(
                execute: () => LoadPercentOperation(),
                canExecute: () => Validate(nameof(LoadPercent_Inputs)) && !LoadPercent_IsWorking // 增加处理状态检查
            );
            //直流分量
            Command_SetDCOffset = new RelayCommand(
                execute: () => DCOffsetOperation(),
                canExecute: () => Validate(nameof(DCOffset_Inputs)) && !DCOffset_IsWorking // 增加处理状态检查
            );
            //额定功率
            Command_SetRatedPwr = new RelayCommand(
                execute: () => RatedPwrOperation(),
                canExecute: () => Validate(nameof(RatedPwr_Inputs)) && !RatedPwr_IsWorking // 增加处理状态检查
            );
            //电感功率
            Command_SetInductorPwr = new RelayCommand(
                execute: () => InductorPwrOperation(),
                canExecute: () => Validate(nameof(InductorPwr_Inputs)) && !InductorPwr_IsWorking // 增加处理状态检查
            );
            //电感电流
            Command_SetInductorCurr = new RelayCommand(
                execute: () => InductorCurrOperation(),
                canExecute: () => Validate(nameof(InductorCurr_Inputs)) && !InductorCurr_IsWorking // 增加处理状态检查
            );
            #endregion
        }


        #region 输出电压
        //输出电压
        private string _OutVolt;

        public string OutVolt
        {
            get { return _OutVolt; }
            set
            {
                _OutVolt = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(OutVolt));
            }
        }


        private bool OutVolt_IsWorking;


        //设置值
        private string _OutVolt_Inputs;

        public string OutVolt_Inputs
        {
            get { return _OutVolt_Inputs; }
            set
            {
                _OutVolt_Inputs = value;
                this.RaiseProperChanged(nameof(OutVolt_Inputs));
                Command_SetOutVolt.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetOutVolt { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OutVoltOperation()
        {
            try
            {
                OutVolt_IsWorking = true;
                // 禁用按钮
                Command_SetOutVolt.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("V", OutVolt_Inputs);

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
                OutVolt_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOutVolt.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }
        #endregion

        #region 输出频率

        //输出频率
        private string _OutFreq;

        public string OutFreq
        {
            get { return _OutFreq; }
            set
            {
                _OutFreq = value;
                this.RaiseProperChanged(nameof(OutFreq));
            }
        }


        private bool OutFreq_IsWorking;


        //设置值
        private string _OutFreq_Inputs;

        public string OutFreq_Inputs
        {
            get { return _OutFreq_Inputs; }
            set
            {
                _OutFreq_Inputs = value;
                this.RaiseProperChanged(nameof(OutFreq_Inputs));
                Command_SetOutFreq.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetOutFreq { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OutFreqOperation()
        {
            try
            {
                OutFreq_IsWorking = true;
                // 禁用按钮
                Command_SetOutFreq.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("F", OutFreq_Inputs);

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
                OutFreq_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOutFreq.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 视在功率

        //视在功率
        private string _ApparentPwr;

        public string ApparentPwr
        {
            get { return _ApparentPwr; }
            set
            {
                _ApparentPwr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(ApparentPwr));
            }
        }


        private bool ApparentPwr_IsWorking;


        //设置值
        private string _ApparentPwr_Inputs;

        public string ApparentPwr_Inputs
        {
            get { return _ApparentPwr_Inputs; }
            set
            {
                _ApparentPwr_Inputs = value;
                this.RaiseProperChanged(nameof(ApparentPwr_Inputs));
                Command_SetApparentPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetApparentPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ApparentPwrOperation()
        {
            try
            {
                ApparentPwr_IsWorking = true;
                // 禁用按钮
                Command_SetApparentPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ApparentPwr_Inputs);

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
                ApparentPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetApparentPwr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 有功功率

        //有功功率
        private string _ActivePwr;

        public string ActivePwr
        {
            get { return _ActivePwr; }
            set
            {
                _ActivePwr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(ActivePwr));
            }
        }


        private bool ActivePwr_IsWorking;


        //设置值
        private string _ActivePwr_Inputs;

        public string ActivePwr_Inputs
        {
            get { return _ActivePwr_Inputs; }
            set
            {
                _ActivePwr_Inputs = value;
                this.RaiseProperChanged(nameof(ActivePwr_Inputs));
                Command_SetActivePwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetActivePwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ActivePwrOperation()
        {
            try
            {
                ActivePwr_IsWorking = true;
                // 禁用按钮
                Command_SetActivePwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ActivePwr_Inputs);

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
                ActivePwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetActivePwr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region 负载百分比

        //负载百分比
        private string _LoadPercent;

        public string LoadPercent
        {
            get { return _LoadPercent; }
            set
            {
                _LoadPercent = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(LoadPercent));
            }
        }


        private bool LoadPercent_IsWorking;


        //设置值
        private string _LoadPercent_Inputs;

        public string LoadPercent_Inputs
        {
            get { return _LoadPercent_Inputs; }
            set
            {
                _LoadPercent_Inputs = value;
                this.RaiseProperChanged(nameof(LoadPercent_Inputs));
                Command_SetLoadPercent.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetLoadPercent { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void LoadPercentOperation()
        {
            try
            {
                LoadPercent_IsWorking = true;
                // 禁用按钮
                Command_SetLoadPercent.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", LoadPercent_Inputs);

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
                LoadPercent_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetLoadPercent.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 直流分量

        //直流分量
        private string _DCOffset;

        public string DCOffset
        {
            get { return _DCOffset; }
            set
            {
                _DCOffset = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(DCOffset));
            }
        }


        private bool DCOffset_IsWorking;


        //设置值
        private string _DCOffset_Inputs;

        public string DCOffset_Inputs
        {
            get { return _DCOffset_Inputs; }
            set
            {
                _DCOffset_Inputs = value;
                this.RaiseProperChanged(nameof(DCOffset_Inputs));
                Command_SetDCOffset.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetDCOffset { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void DCOffsetOperation()
        {
            try
            {
                DCOffset_IsWorking = true;
                // 禁用按钮
                Command_SetDCOffset.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", DCOffset_Inputs);

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
                DCOffset_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetDCOffset.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 额定功率

        //额定功率
        private string _RatedPwr;

        public string RatedPwr
        {
            get { return _RatedPwr; }
            set
            {
                _RatedPwr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(RatedPwr));
            }
        }


        private bool RatedPwr_IsWorking;


        //设置值
        private string _RatedPwr_Inputs;

        public string RatedPwr_Inputs
        {
            get { return _RatedPwr_Inputs; }
            set
            {
                _RatedPwr_Inputs = value;
                this.RaiseProperChanged(nameof(RatedPwr_Inputs));
                Command_SetRatedPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetRatedPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void RatedPwrOperation()
        {
            try
            {
                RatedPwr_IsWorking = true;
                // 禁用按钮
                Command_SetRatedPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", RatedPwr_Inputs);

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
                RatedPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetRatedPwr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 电感功率

        //电感功率
        private string _InductorPwr;

        public string InductorPwr
        {
            get { return _InductorPwr; }
            set
            {
                _InductorPwr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(InductorPwr));
            }
        }


        private bool InductorPwr_IsWorking;


        //设置值
        private string _InductorPwr_Inputs;

        public string InductorPwr_Inputs
        {
            get { return _InductorPwr_Inputs; }
            set
            {
                _InductorPwr_Inputs = value;
                this.RaiseProperChanged(nameof(InductorPwr_Inputs));
                Command_SetInductorPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetInductorPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InductorPwrOperation()
        {
            try
            {
                InductorPwr_IsWorking = true;
                // 禁用按钮
                Command_SetInductorPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", InductorPwr_Inputs);

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
                InductorPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInductorPwr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 电感电流

        //电感电流
        private string _InductorCurr;

        public string InductorCurr
        {
            get { return _InductorCurr; }
            set
            {
                _InductorCurr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(InductorCurr));
            }
        }


        private bool InductorCurr_IsWorking;


        //设置值
        private string _InductorCurr_Inputs;

        public string InductorCurr_Inputs
        {
            get { return _InductorCurr_Inputs; }
            set
            {
                _InductorCurr_Inputs = value;
                this.RaiseProperChanged(nameof(InductorCurr_Inputs));
                Command_SetInductorCurr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetInductorCurr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InductorCurrOperation()
        {
            try
            {
                InductorCurr_IsWorking = true;
                // 禁用按钮
                Command_SetInductorCurr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", InductorCurr_Inputs);

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
                InductorCurr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInductorCurr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }
        #endregion

        #region 通用方法
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
                case "OutVolt_Inputs":
                    return !string.IsNullOrWhiteSpace(OutVolt_Inputs);
                //输出频率   
                case "OutFreq_Inputs":
                    return !string.IsNullOrWhiteSpace(OutFreq_Inputs);
                //视在功率   
                case "ApparentPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(ApparentPwr_Inputs);
                //有功功率 
                case "ActivePwr_Inputs":
                    return !string.IsNullOrWhiteSpace(ActivePwr_Inputs);
                //负载百分比
                case "LoadPercent_Inputs":
                    return !string.IsNullOrWhiteSpace(LoadPercent_Inputs);
                //直流分量
                case "DCOffset_Inputs":
                    return !string.IsNullOrWhiteSpace(DCOffset_Inputs);
                //额定功率
                case "RatedPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(RatedPwr_Inputs);
                //电感功率
                case "InductorPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(InductorPwr_Inputs);
                //电感功率
                case "InductorCurr_Inputs":
                    return !string.IsNullOrWhiteSpace(InductorCurr_Inputs);


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
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //输出电压
                OutVolt = Values[0].Substring(1,5);
                //输出频率
                OutFreq = Values[1];
                //视在频率
                ApparentPwr= Values[2];
                //有功功率
                ActivePwr = Values[3];
                //负载百分比
                LoadPercent = Values[4];
                //直流分量
                DCOffset = Values[5];
                //额定功率
                RatedPwr = Values[6];
                //电感电流
                InductorCurr = Values[7];
                //电感功率
                InductorPwr = Values[8].Substring(0,5);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }
}
