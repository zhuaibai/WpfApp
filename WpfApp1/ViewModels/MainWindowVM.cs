﻿using System;
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
using WpfApp1.Command.Comand_GB3024;
using WpfApp1.Command.Command_VQ3024;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.UserControls;

namespace WpfApp1.ViewModels
{
    public class MainWindowVM:BaseViewModel
    {
		public MainWindowVM() 
		{

            

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
            

            


            //初始化串口信息
            IniCom();
            //发送帧，接收帧
            SerialCountVM = new SerialCountVM();
            //绑定委托
            SerialCommunicationService.AddReceiveFrame=SerialCountVM.AddReceiveFrame;
            SerialCommunicationService.AddSendFrame=SerialCountVM.AddSendFrame;
            
            //初始化  GB   ViewModel
            HOP = new HOPViewModel(_pauseEvent,_semaphore,AddLog,UpdateState);
            HEEP1 = new HEEP1_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HEEP2 = new HEEP2_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HGEN = new HGEN_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            SpecialCommand = new Special_Command(_pauseEvent, _semaphore, AddLog, UpdateState);
            OpenCom = new RelayCommand(openCom);
            //初始化  VQ   VIew
            HOP_VQ = new HOP_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HGRID_VQ = new HGRID_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            //初始化实时时间ViewModel
            Clock = new ClockViewModel();
        }
        #endregion

        #region 下拉框选择机器类型

        private string? _selectedItem ="GB3024";
        public string? SelectedMachineItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

     

        public ObservableCollection<string> MachineItems { get; } = new(){
        "GB3024",
        "VQ3024",
        "PTF"
         };

        //切换指令
        public ICommand SelectionChangedCommand 
        { 
            get
            {
                return new RelayCommand(OnSelectionChanged);
            } 
        }

        //选中项改变
        private void OnSelectionChanged()
        { 
            // 这里处理选择变更逻辑
            System.Diagnostics.Debug.WriteLine($"选中项: {SelectedMachineItem}");

            // 如果需要，可以在这里添加业务逻辑
            if (SelectedMachineItem != null)
            {
                // 执行你的命令逻辑
                SwitchViewToVQorGB(SelectedMachineItem);
            }
        }

        /// <summary>
        /// 切换界面到VQ
        /// </summary>
        public void SwitchViewToVQorGB(string view)
        {

            if (view == "VQ3024")
            {
                ContentUC = new AC_Monitor();
            }
            else if(view =="GB3024")
            {
                ContentUC = new MonitorUC();
            }
            else
            {
                ContentUC = new PTF_Monitor();
            }
        }
        #endregion

        #region 实时时间
        /// <summary>
        /// 实时时间
        /// </summary>
        private ClockViewModel _Clock;

        public ClockViewModel Clock 
        {
            get { return _Clock; }
            set
            {
                _Clock = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region GB指令ViewModel

        private HOPViewModel hopVm;
        public HOPViewModel HOP
        {
            get { return hopVm; }
            set
            {
                hopVm = value;
                this.RaiseProperChanged(nameof(HOP));
            }
        }

        private HEEP1_ViewModel hEEP1_ViewModel;

        public HEEP1_ViewModel HEEP1
        {
            get { return hEEP1_ViewModel; }
            set
            {
                hEEP1_ViewModel = value;
                this.RaiseProperChanged(nameof(HEEP1));
            }
        }

        private HEEP2_ViewModel hEEP2_ViewModel;

        public HEEP2_ViewModel HEEP2
        {
            get { return hEEP2_ViewModel; }
            set
            {
                hEEP2_ViewModel = value;
                this.RaiseProperChanged(nameof(HEEP2));
            }
        }

        private HGEN_ViewModel _HGEN;

        public HGEN_ViewModel HGEN
        {
            get { return _HGEN; }
            set { _HGEN = value; }
        }


        private Special_Command special_Command;

        public Special_Command SpecialCommand
        {
            get { return special_Command; }
            set
            {
                special_Command = value;
                this.RaiseProperChanged(nameof(SpecialCommand));
            }
        }

        #endregion

        #region VQ指令ViewModel

        private HOP_ViewModel _HOP_VQ;

        public HOP_ViewModel HOP_VQ
        {
            get { return _HOP_VQ; }
            set
            {
                _HOP_VQ = value;
                this.RaiseProperChanged(nameof(HOP_VQ));
            }
        }

        private HGRID_ViewModel _HGRID_VQ ;

        public HGRID_ViewModel HGRID_VQ
        {
            get { return _HGRID_VQ; }
            set
            {
                _HGRID_VQ = value;
                this.RaiseProperChanged(nameof(HGRID_VQ));
            }
        }




        #endregion


        #region 串口工具

        //串口实体类
        private SerialPortSettings serialPortSettings;

        /// <summary>
        /// 初始化串口工具
        /// </summary>
        private void IniCom()
        {
            //加载串口配置文件
            serialPortSettings = LoadCom();
            //初始化串口通讯工具
            SerialCommunicationService.InitiateCom(serialPortSettings);
        }

        //串口打开图标
        private Visibility _ComIconOpen  = Visibility.Visible;

        public Visibility ComIconOpen
        {
            get { return _ComIconOpen; }
            set
            {
                _ComIconOpen = value;
                this.RaiseProperChanged(nameof(ComIconOpen));
            }
        }

        //串口关闭图标
        private Visibility _ComIconClose = Visibility.Collapsed;

        public Visibility ComIconClose
        {
            get { return _ComIconClose ; }
            set
            {
                _ComIconClose = value;
                this.RaiseProperChanged(nameof(ComIconClose));
            }
        }


        /// <summary>
        /// 改变串口打开图标
        /// </summary>
        /// <param name="flag"></param>
        private void ChangeComIcon(bool flag)
        {
            if (flag)
            {
                //串口打开
                ComIconClose = Visibility.Visible;
                ComIconOpen = Visibility.Collapsed;
            }
            else
            {
                //串口关闭
                ComIconClose = Visibility.Collapsed;
                ComIconOpen = Visibility.Visible;
            }
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
            if (SerialCommunicationService.IsOpen())
            {
                MessageBoxResult dialogResult = MessageBox.Show(App.GetText("串口已打开，是否关闭?"), "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (dialogResult == MessageBoxResult.OK)
                {
                    try
                    {
                        AddLog("准备关闭通信");
                        //停止后台通信(如果有)
                        StopBackgroundThread();

                        //关闭串口
                        AddLog("串口已关闭");
                        SerialCommunicationService.CloseCom();

                        ChangeComIcon(false);

                        ////重新加载配置文件
                        //IniCom();
                        //if (!SerialCommunicationService.OpenCom())
                        //{
                        //    MessageBox.Show("串口打开失败！");
                        //    return;
                        //};
                        UpdateState(App.GetText("串口已关闭"));
                        comStateColor(false);
                        AddLog($"关闭串口{SerialCommunicationService.getComName()}成功");
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    return;
                }
                
            }
            else
            {
                IniCom();
                if (!SerialCommunicationService.OpenCom())
                {
                    MessageBox.Show("串口打开失败！");
                    return ;
                };
                ChangeComIcon(true);
                UpdateState(App.GetText("串口已打开(点击开始进行通讯)"));
                comStateColor(true);
                AddLog($"打开串口{SerialCommunicationService.getComName()}成功");
            }
           
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
			set { contentUC = value; RaiseProperChanged(nameof(ContentUC)); }
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
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // 异步竞争

        //帧计数
        private SerialCountVM _SerialCountVM;

        public SerialCountVM SerialCountVM
        {
            get { return _SerialCountVM; }
            set
            {
                _SerialCountVM = value;
                this.RaiseProperChanged(nameof(SerialCountVM));
            }
        }




        // 后台线程是否正在运行
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
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

        //状态灯
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
        /// 设置状态灯颜色
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
           
            if (!SerialCommunicationService.IsOpen())
            {
                MessageBox.Show(App.GetText("请先打开串口!"), "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (IsRunning) { return; } 
            else
            {
                IsRunning = true;
                _cts = new CancellationTokenSource();
                _pauseEvent.Set();
                UpdateState(App.GetText("正在通信"));
                comStateColor(true);
                Task.Run(() => BackgroundWorker(_cts.Token));
                AddLog("后台通信线程已启动");
            }
            
        }

       

        /// <summary>
        /// 后台工作线程主循环
        /// </summary>
        private async Task BackgroundWorker(CancellationToken token)
        {
            try
            {
                //COM通讯
                while (!token.IsCancellationRequested)
                {
                    if (SelectedMachineItem == "GB3024")
                    {
                        //GB3024通讯
                        CommunicationWithGB3024(token);
                    }
                    if (SelectedMachineItem == "VQ3024")
                    {
                        //VQ3024通讯
                        CommunicationWithVQ3024(token);
                    }
                    // 模拟常规通信
                    await Task.Delay(1000, token);
                    //AddLog($"[后台] 常规通信: {DateTime.Now:HH:mm:ss.fff}");

                    
                }
            }
            catch (OperationCanceledException)
            {
                AddLog("后台通信已终止");
                UpdateState(App.GetText("已停止通信!"));
                IsRunning = false;
            }
            finally
            {
                
            }
        }
        /// <summary>
        /// GB3024通讯
        /// </summary>
        /// <param name="token"></param>
        private void CommunicationWithGB3024(CancellationToken token)
        {
            // 等待暂停或取消信号
            _pauseEvent.Wait(token);
            //发送查询机器指令
            string receive_MachineType = SerialCommunicationService.SendCommand(SpecialCommand.QueryMachineType, 10);
            //解析指令
            SerialCommunicationService.MachineType = receive_MachineType;

            _pauseEvent.Wait(token); // 等待暂停或取消信号
                                     //发送HOP指令
            string receive = SerialCommunicationService.SendCommand(HOP.Command, 40);
            //解析返回命令
            HOP.AnalysisStringToElement(receive);

            _pauseEvent.Wait(token); // 等待暂停或取消信号
                                     //发送HEEP1指令
            string receiveHEEP1 = SerialCommunicationService.SendCommand(HEEP1.Command, 80);
            //解析返回指令
            HEEP1.AnalyseStringToElement(receiveHEEP1);

            _pauseEvent.Wait(token); // 等待暂停或取消信号
                                     //发送HEEP2指令
            string receive_HEEP2 = SerialCommunicationService.SendCommand(HEEP2.Command, 80);
            //解析返回指令
            HEEP2.AnalyseStringToElement(receive_HEEP2);

            _pauseEvent.Wait(token); // 等待暂停或取消信号
                                     //发送HEEP2指令
            string receive_HOP = SerialCommunicationService.SendCommand(HOP.Command, 50);
            //解析返回指令
            HOP.AnalysisStringToElement(receive_HOP);

            _pauseEvent.Wait(token); // 等待暂停或取消信号
                                     //发送HEEP2指令
            string receive_HGEN = SerialCommunicationService.SendCommand(HGEN.Command, 60);
            //解析返回指令
            HGEN.AnalyseStringToElement(receive_HGEN);
        }

        /// <summary>
        /// GB3024通讯
        /// </summary>
        /// <param name="token"></param>
        private void CommunicationWithVQ3024(CancellationToken token)
        {
            //发送VQ2024
            string Receive = "";
            _pauseEvent.Wait(token); // 等待暂停或取消信号
                                     //发送HOP指令
            Receive = SerialCommunicationService.SendCommand(HOP_VQ.Command, 60);
            //解析返回指令
            HGEN.AnalyseStringToElement(Receive);

            _pauseEvent.Wait(token); // 等待暂停或取消信号
                                     //发送HEEP2指令
            Receive = SerialCommunicationService.SendCommand(HGRID_VQ.Command, 60);
            //解析返回指令
            HGEN.AnalyseStringToElement(Receive);
        }
        /// <summary>
        /// 停止后台通信
        /// </summary>
        private void StopBackgroundThread()
        {
            _cts.Cancel();
            AddLog("后台通信停止请求已发送");
        }
        #endregion


    }
}
