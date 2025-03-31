using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using WpfApp1.Command;
using WpfApp1.GB3024C_Comand;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.UserControls;

namespace WpfApp1.ViewModels
{
    public class MainWindowVM:BaseViewModel
    {
		public MainWindowVM() 
		{

            #region 初始化PV信息
            //初始化PV信息
            PVList = new List<PV_Model>();
			PVList.Add(new PV_Model() { PV_Name = "电压(V)", PV1 = "0", PV2 = "0" });
			PVList.Add(new PV_Model() { PV_Name = "电流(A)", PV1 = "0", PV2 = "0" });
			PVList.Add(new PV_Model() { PV_Name = "功率(W)", PV1 = "0", PV2 = "0", PV_Count="0"});
            #endregion

            #region 初始化电量信息
            //初始化电量信息
            PV_Info_List = new List<PV_InfoModel>();
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="PV日电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="PV总电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="日充电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="总充电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="日放电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="总放电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="电网日取电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="电网总取电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="电网日馈电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="电网总馈电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="负载日用电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="负载总用电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="系统产出日发电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="系统产出总发电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="自发自用日发电量:",PV_Info_Value="0"});
			PV_Info_List.Add(new PV_InfoModel() {PV_Info_Name="自发自用总发电量:",PV_Info_Value="0"});
            #endregion

            #region 日志界面
            // 初始化命令
            ClearLogCommand = new DelegateCommand(ClearLog);
            SaveLogCommand = new DelegateCommand(SaveLog);
            AddLogCommand = new DelegateCommand(AddMessage);
            // 示例：添加一些初始日志
            AddLog("程序启动");

            #endregion

            #region 后台线程
            StartCommand = new RelayCommand(StartBackgroundThread);
            StopCommand = new RelayCommand(StopBackgroundThread);
            

            #endregion


            //初始化串口信息
            IniCom();

            hopViewModel = new HOPViewModel(_pauseEvent,_semaphore,AddLog,UpdateState,communicationService);
            
            OpenCom = new RelayCommand(openCom);
            // 改进的命令初始化
            Button1Command = new RelayCommand(
                execute: () => ExecuteButton1Operation(),
                canExecute: () => ValidateInput1() && !IsWorking // 增加处理状态检查
            );
            
        }

        #region 串口工具
        //modbus通讯工具
        SerialCommunicationService communicationService;
        //串口实体类
        private SerialPortSettings serialPortSettings;
        //初始化串口工具
        private void IniCom()
        {
            serialPortSettings = LoadCom();
            communicationService = new SerialCommunicationService(serialPortSettings);
        }

        



        /// <summary>
        /// 从文件加载串口通讯信息
        /// </summary>
        /// <returns></returns>
        private SerialPortSettings LoadCom()
        {
            try
            {
                if (File.Exists("serialSettings.xml"))
                {
                    var serializer = new XmlSerializer(typeof(SerialPortSettings));
                    using (var reader = new StreamReader("serialSettings.xml"))
                    {
                        return (SerialPortSettings)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载设置时出错: {ex.Message}");
            }
            return new SerialPortSettings();
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        public void openCom()
        {
            if (communicationService.IsOpen())
            {
                MessageBoxResult dialogResult = MessageBox.Show("串口已打开，是否重新打开！", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (dialogResult == MessageBoxResult.OK)
                {
                    AddLog("准备关闭通信");
                    //停止后台通信(如果有)
                    StopBackgroundThread();
                    
                    //关闭串口
                    AddLog("串口已关闭");
                    communicationService.CloseCom();
                    
                    
                    //重新加载配置文件
                    IniCom();
                    if (!communicationService.OpenCom())
                    {
                        MessageBox.Show("串口打开失败！");
                        return;
                    };
                }
                else
                {
                    return;
                }
                
            }
            else
            {
                IniCom();
                if (!communicationService.OpenCom())
                {
                    MessageBox.Show("串口打开失败！");
                    return ;
                };
            }
            UpdateState("串口已打开(点击开始进行通讯)");
            comStateColor(true);
            AddLog($"打开串口{communicationService.getComName()}成功");
        }
        #endregion

        #region 信息显示窗口
        /// <summary>
        /// 内容窗口
        /// </summary>
        private UserControl contentUC;
		public UserControl ContentUC
		{
			get 
			{ 
				if(contentUC ==null)
				{
					 return contentUC = new MonitorUC(); 
				}
				return contentUC; }
			set { contentUC = value; RaiseProperChanged(nameof(ContentControl)); }
		}
        #endregion

        #region PV信息
        /// <summary>
        /// PV信息
        /// </summary>
        private List<PV_Model> _PVList;
		public List<PV_Model> PVList
		{
			get { return _PVList; }
			set { _PVList = value; RaiseProperChanged(nameof(PVList)); }
		}

        #endregion

        #region 电量信息
        /// <summary>
        /// 电量信息
        /// </summary>
        private List<PV_InfoModel> _PV_InfoList;

		public List<PV_InfoModel> PV_Info_List
		{
			get { return _PV_InfoList; }
			set
			{
				_PV_InfoList = value;
				this.RaiseProperChanged(nameof(PV_Info_List));
			}
		}
        #endregion

        #region 日志界面

        /// <summary>
        /// 日志信息
        /// </summary>
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();
        public ObservableCollection<string> LogMessages
        {
            get { return _logMessages; }
            set
            {
                if (_logMessages != value)
                {
                    _logMessages = value;
                    RaiseProperChanged(nameof(LogMessages));
                }
            }
        }

        private bool _isScrollingEnabled = true;
        public bool IsScrollingEnabled
        {
            get { return _isScrollingEnabled; }
            set
            {
                if (_isScrollingEnabled != value)
                {
                    _isScrollingEnabled = value;
                    RaiseProperChanged(nameof(IsScrollingEnabled));
                }
            }
        }

        public ICommand ClearLogCommand { get; }
        public ICommand SaveLogCommand { get; }

        public ICommand AddLogCommand { get; }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="logMessage"></param>
        public void AddLog(string logMessage)
        {
            string timestamp = DateTime.Now.ToString("yyyy - MM - dd HH:mm:ss");
            string formattedLog = $"[{timestamp}] {logMessage}";

            Application.Current.Dispatcher.Invoke(() =>
            {
                
                LogMessages.Insert(0, formattedLog);
                if (LogMessages.Count > 100) LogMessages.RemoveAt(99);
            });
            //从上到下
            //LogMessages.Add(formattedLog);

            //从下到上的添加日志
            

            if (IsScrollingEnabled)
            {
                // 这里可以通过事件或其他方式通知视图滚动到最新日志
            }
            SaveLogToFile(formattedLog);
        }

        private void AddMessage(object p)
        {
            string timestamp = DateTime.Now.ToString("yyyy - MM - dd HH:mm:ss");
            string formattedLog = $"[{timestamp}] 新增数据";

            //从上到下
            //LogMessages.Add(formattedLog);

            //从下到上的添加日志
            LogMessages.Insert(0, formattedLog);
        }

        private void ClearLog(object parameter)
        {
            LogMessages.Clear();
        }

        /// <summary>
        /// 保存所有日志
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveLog(object parameter)
        {
            try
            {
                foreach (var log in LogMessages)
                {
                    SaveLogToFile(log);
                }
                AddLog("日志已保存");
            }
            catch (Exception ex)
            {
                AddLog($"保存日志时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存一条日志到本地
        /// </summary>
        /// <param name="log"></param>
        private void SaveLogToFile(string log)
        {
            DateTime now = DateTime.Now;
            string logName = "日志";
            string yearMonth = now.ToString("yyyy-MM");
            string DayFolder = now.ToString("dd");

            // 构建日志文件夹路径
            string logFolder = Path.Combine(Directory.GetCurrentDirectory(), logName,yearMonth, DayFolder);
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            // 构建日志文件路径
            string logFilePath = Path.Combine(logFolder, $"log_{now:yyyyMMdd}.txt");
            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine(log);
                }
            }
            catch (Exception ex)
            {
                AddLog($"写入日志文件时出错: {ex.Message}");
            }
        }
        #endregion

        #region 后台通讯线程
        private readonly object _syncLock = new object();
        private CancellationTokenSource _cts = new CancellationTokenSource();//取消线程专用
        private ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);//暂停线程专用

        // 后台线程是否正在运行
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => OnPropertyChanged();
        }

        // 状态信息
        private string _status = "关闭";
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    RaiseProperChanged(nameof(Status));
                }
                
            }
        }


        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="state"></param>
        public void UpdateState(string state)
        {
            Status = state;
        }

        // 工作状态指示
        private bool _isWorking;
        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                if (value == _isWorking) return;
                _isWorking = value;
                RaiseProperChanged(nameof(IsWorking));
            }
        }

        //状态灯指示
        private Brush comStatus = Brushes.Red;
        public Brush ComStatus
        {
            get
            {
                return comStatus;
            }
            set
            {
                comStatus = value;
               OnPropertyChanged();
            }
        }

        /// <summary>
        /// 状态颜色
        /// </summary>
        /// <param name="flag"></param>
        public void comStateColor(bool flag)
        {
            if (flag)
            {
                ComStatus = Brushes.Green;
            }
            else
            {
                ComStatus = Brushes.Red;
            }
        }
        // 日志记录
        //public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        // 命令定义
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ExecuteSpecialCommand { get; }
        public ICommand OpenCom {  get; }
       
        

        /// <summary>
        /// 启动后台通信线程
        /// </summary>
        private void StartBackgroundThread()
        {
            if (IsRunning) return;
            IsRunning = true;
            _cts = new CancellationTokenSource();
            _pauseEvent.Set();
            UpdateState("正在通信");
            comStateColor(true);
            Task.Run(() => BackgroundWorker(_cts.Token));
            AddLog("后台通信线程已启动");
        }


        private HOPViewModel hopVm;

        public HOPViewModel hopViewModel    
        {
            get { return hopVm; }
            set
            {
                hopVm = value;
                this.RaiseProperChanged(nameof(hopViewModel));
            }
        }
            

        /// <summary>
        /// 后台工作线程主循环
        /// </summary>
        private async Task BackgroundWorker(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _pauseEvent.Wait(token); // 等待暂停或取消信号

                    //发送HOP指令
                    string receive = communicationService.SendCommand(hopViewModel.Command, 40);
                    //解析返回命令
                    hopViewModel.AnalysisStringToElement(receive);

                    AddLog($"{"Name:"+hopViewModel.Name +"Kanme:"+ hopViewModel.Kame}");
                    // 模拟常规通信
                    await Task.Delay(1000, token);
                    AddLog($"[后台] 常规通信: {DateTime.Now:HH:mm:ss.fff}");
                }
            }
            catch (OperationCanceledException)
            {
                AddLog("后台通信已正常终止");
                UpdateState("已停止通信");
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// 停止后台通信
        /// </summary>
        private void StopBackgroundThread()
        {
            _cts.Cancel();
            AddLog("后台通信停止请求已发送");
        }

       

        ///// <summary>
        ///// 执行特殊操作（暂停后台线程后执行）
        ///// </summary>
        //private async void ExecuteSpecialOperation()
        //{
        //    try
        //    {
        //        IsWorking = true;
        //        Status = "正在执行特殊操作...";

        //        // 暂停后台线程
        //        _pauseEvent.Reset();
        //        AddLog("已暂停后台通信");

        //        // 执行特殊操作（带超时保护）
        //        using var timeoutCts = new CancellationTokenSource(5000);
        //        await Task.Run(new Action (Test)
        //        , timeoutCts.Token);
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        AddLog("特殊操作执行超时");
        //    }
        //    finally
        //    {
        //        // 恢复后台线程
        //        _pauseEvent.Set();
        //        IsWorking = false;
        //        Status = "就绪";
        //    }
        //}

        //public void Test()
        //{

        //}
        ///// <summary>
        ///// 线程安全的日志添加方法
        ///// </summary>
        //private void AddLog(string message)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        Logs.Add($"{DateTime.Now:HH:mm:ss.fff} {message}");
        //        if (Logs.Count > 100) Logs.RemoveAt(0);
        //    });
        //}

        // INotifyPropertyChanged 实现
        

        #endregion

        #region 设置指令测试

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // 异步竞争
        // 示例1的属性和命令
        private string _input1 = string.Empty;
        public string Input1
        {
            get => _input1;
            set
            {
                _input1 = value;
                OnPropertyChanged();
                // 输入变化时更新按钮可用状态
                Button1Command.RaiseCanExecuteChanged();
            }
        }
        // 示例操作1（模拟耗时操作）
        private async void ExecuteButton1Operation()
        {
            try
            {
                IsWorking = true;
                // 禁用按钮
                Button1Command.RaiseCanExecuteChanged();
                
                // 异步等待锁
                await _semaphore.WaitAsync();
                Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    Thread.Sleep(2000);
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
                IsWorking = false;
                Status = "就绪";
                // 重新启用按钮
                Button1Command.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release(); 
            }


        }

        

        public RelayCommand Button1Command { get; }
        // 输入验证方法1：简单非空验证
        private bool ValidateInput1()
        {
            return !string.IsNullOrWhiteSpace(Input1);
        }
       
        #endregion

    }
}
