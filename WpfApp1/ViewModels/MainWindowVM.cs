using System.Collections.ObjectModel;
using System.IO;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using WpfApp1.Command;
using WpfApp1.Command.Comand_GB3024;
using WpfApp1.Command.Command_PDF302;
using WpfApp1.Command.Command_PDF3024;
using WpfApp1.Command.Command_VQ3024;
using WpfApp1.Command.GB_General;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.UserControls;

namespace WpfApp1.ViewModels
{
    public class MainWindowVM : BaseViewModel
    {
        public MainWindowVM()
        {
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
            //绑定发送接收帧计数委托
            SerialCommunicationService.AddReceiveFrame = SerialCountVM.AddReceiveFrame;
            SerialCommunicationService.AddSendFrame = SerialCountVM.AddSendFrame;


            //初始化  GB   ViewModel
            HOP = new HOPViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HEEP1 = new HEEP1_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HEEP2 = new HEEP2_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HGEN = new HGEN_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HGRID_GB = new HGRID_GB_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HSTS_GB = new HSTS_GB_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            SpecialCommand = new Special_Command(_pauseEvent, _semaphore, AddLog, UpdateState);
            HPVB_GB = new HPVB_GB_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            OpenCom = new RelayCommand(openCom);
            //初始化  VQ   VIew
            HOP_VQ = new HOP_VQ_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HBMS1_VQ = new HBMS1_VQ_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HBAT_VQ = new HBAT_VQ_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HEEP1_VQ = new HEEP1_VQ_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HGRID_VQ = new HGRID_VQ_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);

            //初始化  VDF ViewModel
            HBAT_PDF = new HBAT_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HCTMSG1_PDF = new HCTMSG1_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HGRID_PDF = new HGRID_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HEEP1_PDF = new HEEP1_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HEEP3_PDF = new HEEP3_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HIMSG1 = new HIMSG1_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HOP_PDF = new HOP_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HPV_PDF = new HPV_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HTEMP_PDF = new HTEMP_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            HIGSG2_PDF = new HIMSG2_PDF_ViewModel(_pauseEvent, _semaphore, AddLog, UpdateState);
            //初始化实时时间ViewModel
            Clock = new ClockViewModel();
            #endregion
        }

        #region 图标百分比


        //逆变总功率百分比
        private int _PercentValue = 30;

        public int InvTotalPwr
        {
            get { return _PercentValue; }
            set
            {
                _PercentValue = value;
                this.RaiseProperChanged(nameof(InvTotalPwr));
            }
        }

        /// <summary>
        /// MPPT总功率百分比
        /// </summary>
        private int _MPPTTotalPwr;

        public int MPPTTotalPwr
        {
            get { return _MPPTTotalPwr; }
            set
            {
                _MPPTTotalPwr = value;
                RaiseProperChanged(nameof(MPPTTotalPwr));
            }
        }

        /// <summary>
        /// 市电总功率百分比
        /// </summary>
        private int _ACTotalPwr;

        public int ACTotalPwr
        {
            get { return _ACTotalPwr; }
            set
            {
                _ACTotalPwr = value;
                RaiseProperChanged(nameof(ACTotalPwr));
            }
        }

        /// <summary>
        /// 市电功率
        /// </summary>
        private int _ACPower;

        public int ACPowerVM
        {
            get { return _ACPower; }
            set
            {
                _ACPower = value;
                this.RaiseProperChanged(nameof(ACPowerVM));
            }
        }



        /// <summary>
        /// 把数字字符串转换成整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private int StringToIntConversion(string str)
        {
            if (str == null)
            {
                return -1;
            }
            //这是专门应对市电功率的情况
            string tag = str.Substring(0, 1);
            if (tag == "-")
            {
                return 0;
            }
            else if (tag == "+")
            {
                str = str.Substring(1);
            }

            if (int.TryParse(str, out int number))
            {
                Console.WriteLine($"转换后的整数: {number}");
                return number;
            }
            else
            {
                Console.WriteLine("输入的字符串不是有效的整数格式。");
                return 0;
            }
        }

        /// <summary>
        /// 两数相除，返回百分比
        /// </summary>
        /// <param name="front"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        private int CountPercent(int front, int after)
        {
            if (after == 0)
            {
                return 0;
            }
            else
            {
                return (front * 100) / after;
            }
        }

        /// <summary>
        /// 两数相除，返回百分比
        /// </summary>
        /// <param name="front"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        private int CountPercent(string front, string after)
        {
            int newFront = StringToIntConversion(front);
            int newAfter = StringToIntConversion(after);
            if (newAfter == 0 || newAfter == 0)
            {
                return 0;
            }
            else
            {
                return (newFront * 100) / newAfter;
            }
        }


        #endregion

        #region 抗干扰功能

        /// <summary>
        /// 抗干扰按钮开关切换
        /// </summary>
        public ICommand SwitchOpenReceiveCRC
        {
            get
            {
                return new RelayCommand(SwitchCRCReceive);
            }

        }
        //按钮是否打开
        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.RaiseProperChanged(nameof(IsChecked));
            }
        }

        bool OnceOpenCRC = false;
        /// <summary>
        /// 打开或关闭抗干扰功能
        /// </summary>
        /// <param name="parameter"></param>
        private void SwitchCRCReceive()
        {
            // 根据 ToggleButton 的选中状态执行相应逻辑
            if (IsChecked)
            {
                //处理选中状态
                OnceOpenCRC = true;
                SerialCommunicationService.OpenReceiveCRC(true);
                //发送HOSTCRCEN
                Task.Run(new Action(() => SpecialCommand.AntiJamModeOperation(true)));

            }
            else
            {
                // 处理未选中状态
                SerialCommunicationService.OpenReceiveCRC(false);


            }
        }



        #endregion

        #region 下拉框选择机器类型

        private string? _selectedItem = "GB3024";
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
        "GB_04",
        "GB3024",
        "VQ3024",
        "VDF"
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
            switch (view)
            {
                case "GB_04":
                    ContentUC = new GB6042();
                    SelectedMachineItem = "GB_04";
                    break;
                case "GB3024":
                    ContentUC = new GB_MonitorUC();
                    SelectedMachineItem = "GB3024";
                    break;
                case "VQ3024":
                    ContentUC = new VQ_Monitor();
                    SelectedMachineItem = "VQ3024";
                    break;
                case "VDF":
                    ContentUC = new PTF_Monitor();
                    SelectedMachineItem = "VDF";
                    break;
            }

            //if (view == "VQ3024")
            //{
            //    ContentUC = new VQ_Monitor();
            //    SelectedMachineItem = "VQ3024";
            //}
            //else if(view =="GB3024")
            //{
            //    ContentUC = new GB_MonitorUC();
            //    SelectedMachineItem = "GB3024";
            //}
            //else 
            //{
            //    ContentUC = new PTF_Monitor();
            //    SelectedMachineItem = "VDF";
            //}
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

        private HGRID_GB_ViewModel _HGRID_GB;

        public HGRID_GB_ViewModel HGRID_GB
        {
            get { return _HGRID_GB; }
            set
            {
                _HGRID_GB = value;
                this.RaiseProperChanged(nameof(HGRID_GB));
            }
        }

        private HSTS_GB_ViewModel _HSTS_GB;

        public HSTS_GB_ViewModel HSTS_GB
        {
            get { return _HSTS_GB; }
            set
            {
                _HSTS_GB = value;
                this.RaiseProperChanged(nameof(HSTS_GB));
            }
        }

        private HPVB_GB_ViewModel _HPVB_GB;

        public HPVB_GB_ViewModel HPVB_GB
        {
            get { return _HPVB_GB; }
            set
            {
                _HPVB_GB = value;
                this.RaiseProperChanged(nameof(HPVB_GB));
            }
        }



        #endregion

        #region VQ指令ViewModel

        //HOP
        private HOP_VQ_ViewModel _HOP_VQ;

        public HOP_VQ_ViewModel HOP_VQ
        {
            get { return _HOP_VQ; }
            set
            {
                _HOP_VQ = value;
                this.RaiseProperChanged(nameof(HOP_VQ));
            }
        }

        //HBMS1
        private HBMS1_VQ_ViewModel _HBMS1_VQ;

        public HBMS1_VQ_ViewModel HBMS1_VQ
        {
            get { return _HBMS1_VQ; }
            set
            {
                _HBMS1_VQ = value;
                this.RaiseProperChanged(nameof(HBMS1_VQ));
            }
        }

        //HBAT
        private HBAT_VQ_ViewModel _HBAT_VQ;

        public HBAT_VQ_ViewModel HBAT_VQ
        {
            get { return _HBAT_VQ; }
            set
            {
                _HBAT_VQ = value;
                this.RaiseProperChanged(nameof(HBAT_VQ));
            }
        }

        //HEEP1
        private HEEP1_VQ_ViewModel _HEEP1_VQ;

        public HEEP1_VQ_ViewModel HEEP1_VQ
        {
            get { return _HEEP1_VQ; }
            set
            {
                _HEEP1_VQ = value;
                this.RaiseProperChanged(nameof(HEEP1_VQ));
            }
        }

        //HGRID
        private HGRID_VQ_ViewModel _HGRID_VQ;

        public HGRID_VQ_ViewModel HGRID_VQ
        {
            get { return _HGRID_VQ; }
            set
            {
                _HGRID_VQ = value;
                this.RaiseProperChanged(nameof(HGRID_VQ));
            }
        }




        #endregion

        #region VDF指令ViewModel


        //HIMSG2
        private HIMSG2_PDF_ViewModel _HIGSG2_PDF;

        public HIMSG2_PDF_ViewModel HIGSG2_PDF
        {
            get { return _HIGSG2_PDF; }
            set { _HIGSG2_PDF = value; }
        }


        //HGRID
        private HGRID_PDF_ViewModel _HGRID_PDF;
        public HGRID_PDF_ViewModel HGRID_PDF
        {
            get
            {
                return _HGRID_PDF;
            }
            set
            {
                _HGRID_PDF = value;
                OnPropertyChanged();
            }
        }
        //HOP
        private HOP_PDF_ViewModel _HOP_PDF;

        public HOP_PDF_ViewModel HOP_PDF
        {
            get { return _HOP_PDF; }
            set
            {
                _HOP_PDF = value;
                this.RaiseProperChanged(nameof(HOP_PDF));
            }
        }

        //HBAT
        private HBAT_PDF_ViewModel _HBAT_PDF;

        public HBAT_PDF_ViewModel HBAT_PDF
        {
            get { return _HBAT_PDF; }
            set
            {
                _HBAT_PDF = value;
                this.RaiseProperChanged(nameof(HBAT_PDF));
            }
        }

        //HIMSG1
        private HIMSG1_PDF_ViewModel _HIMSG1;

        public HIMSG1_PDF_ViewModel HIMSG1
        {
            get { return _HIMSG1; }
            set
            {
                _HIMSG1 = value;
                this.RaiseProperChanged(nameof(HIMSG1));
            }
        }

        //HPV
        private HPV_PDF_ViewModel _HPV_PDF;

        public HPV_PDF_ViewModel HPV_PDF
        {
            get { return _HPV_PDF; }
            set
            {
                _HPV_PDF = value;
                this.RaiseProperChanged(nameof(HPV_PDF));
            }
        }

        //HTEMP
        private HTEMP_PDF_ViewModel _HTEMP_PDF;

        public HTEMP_PDF_ViewModel HTEMP_PDF
        {
            get { return _HTEMP_PDF; }
            set
            {
                _HTEMP_PDF = value;
                this.RaiseProperChanged(nameof(HTEMP_PDF));
            }
        }

        //HCTMSG1
        private HCTMSG1_PDF_ViewModel _HCTMSG1_PDF;

        public HCTMSG1_PDF_ViewModel HCTMSG1_PDF
        {
            get { return _HCTMSG1_PDF; }
            set
            {
                _HCTMSG1_PDF = value;
                this.RaiseProperChanged(nameof(HCTMSG1_PDF));
            }
        }

        //HEEP3
        private HEEP3_PDF_ViewModel _HEEP3_PDF;

        public HEEP3_PDF_ViewModel HEEP3_PDF
        {
            get { return _HEEP3_PDF; }
            set
            {
                _HEEP3_PDF = value;
                this.RaiseProperChanged(nameof(HEEP3_PDF));
            }
        }

        //HEEP1
        private HEEP1_PDF_ViewModel _HEEP1_PDF;

        public HEEP1_PDF_ViewModel HEEP1_PDF
        {
            get { return _HEEP1_PDF; }
            set
            {
                _HEEP1_PDF = value;
                this.RaiseProperChanged(nameof(HEEP1_PDF));
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

        #region 串口图标
        //串口打开图标
        private Visibility _ComIconOpen = Visibility.Visible;

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
            get { return _ComIconClose; }
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

        #endregion


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
        /// 打开串口(已打开则关闭串口)
        /// </summary>
        public void openCom()
        {
            if (SerialCommunicationService.IsOpen())
            {

                try
                {
                    AddLog("准备关闭通信");
                    //停止后台通信(如果有)
                    StopBackgroundThread();

                    //关闭串口
                    SerialCommunicationService.CloseCom();
                    AddLog("串口已关闭");

                    ChangeComIcon(false);

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
                IniCom();
                if (!SerialCommunicationService.OpenCom())
                {
                    MessageBox.Show("串口打开失败！");
                    return;
                };
                ChangeComIcon(true);
                UpdateState(App.GetText("串口已打开(点击开始进行通讯)"));
                comStateColor(true);
                AddLog($"打开串口{SerialCommunicationService.getComName()}成功");



                string machine;
                //自动识别机器
                if (!AutoSelectedMachineType(out machine))
                {
                    //未识别
                    MessageBoxResult result = MessageBox.Show($"无法识别机器机器类型:{machine},请自行选择机器类型,点击取消则关闭串口", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                    {
                        //关闭串口
                        openCom();
                        return;
                    }
                }
                //开始通讯
                StartBackgroundThread();
            }

        }

        /// <summary>
        /// 自动获取机型
        /// </summary>
        private bool AutoSelectedMachineType(out string machine)
        {
            string receive_MachineType = SerialCommunicationService.SendCommand(SpecialCommand.QueryMachineType, 10);
            if (receive_MachineType == "")
            {
                //判断抗干扰是否打开
                if (IsChecked)
                {
                    IsChecked = false;
                    OnceOpenCRC = false;
                    SerialCommunicationService.OpenReceiveCRC(false);
                    receive_MachineType = SerialCommunicationService.SendCommand(SpecialCommand.QueryMachineType, 10);
                }
            }

            SerialCommunicationService.MachineType = receive_MachineType;
            if (receive_MachineType.Length >= 9)
            {
                if (receive_MachineType.Substring(0, 9) == "(HPVINV05")
                {
                    //切换到PTF界面
                    SwitchViewToVQorGB("VDF");
                    //默认设置抗干扰模式
                    IsChecked = true;
                    OnceOpenCRC = true;
                    SerialCommunicationService.OpenReceiveCRC(true);
                    //返回机器类型
                    machine = receive_MachineType;
                    return true;
                }
                else if (receive_MachineType.Substring(0, 9) == "(HPVINV02")
                {
                    SwitchViewToVQorGB("GB3024");
                    //判断抗干扰是否打开
                    if (IsChecked)
                    {
                        IsChecked = false;
                        OnceOpenCRC = false;
                        SerialCommunicationService.OpenReceiveCRC(false);
                    }
                    //返回机器类型
                    machine = receive_MachineType;
                    return true;
                }
                else if (receive_MachineType.Substring(0, 9) == "(LPVINV02")
                {
                    SwitchViewToVQorGB("VQ3024");
                    //判断抗干扰是否打开
                    if (IsChecked)
                    {
                        IsChecked = false;
                        OnceOpenCRC = false;
                        SerialCommunicationService.OpenReceiveCRC(false);
                    }
                    //返回机器类型
                    machine = receive_MachineType;
                    return true;
                }
                else if (receive_MachineType.Substring(0, 9) == "(HPVINV04")
                {
                    SwitchViewToVQorGB("GB_04");
                    //判断抗干扰是否打开
                    if (IsChecked)
                    {
                        IsChecked = false;
                        OnceOpenCRC = false;
                        SerialCommunicationService.OpenReceiveCRC(false);
                    }
                    //返回机器类型
                    machine = receive_MachineType;
                    return true;
                }
                else
                {
                    //返回机器类型(无法识别
                    machine = receive_MachineType;
                    return false;
                }


            }
            else
            {
                machine = receive_MachineType;
                return false;
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
                if (contentUC == null)
                {
                    return contentUC = new GB_MonitorUC();
                }
                return contentUC;
            }
            set { contentUC = value; RaiseProperChanged(nameof(ContentUC)); }
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
            string logFolder = Path.Combine(Directory.GetCurrentDirectory(), logName, yearMonth, DayFolder);
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
        public ICommand OpenCom { get; }

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
                    if (SelectedMachineItem == "VDF")
                    {
                        //处理选中状态

                        //PTF通讯
                        CommunicationWithPTF3024(token);
                    }
                    if (SelectedMachineItem == "GB_04")
                    {
                        //GB6042通讯
                        CommunicationWithGB6042(token);
                    }
                    // 模拟常规通信
                    await Task.Delay(0, token);
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

        #endregion

        #region 通讯实现方法
        /// <summary>
        /// VDF3024通讯
        /// </summary>
        /// <param name="token"></param>
        private void CommunicationWithPTF3024(CancellationToken token)
        {

            string receive = "";

            //判断是否开启CRC接收校验（抗干扰 默认开启
            if (IsChecked)
            {
                //发送HOSTCRCEN指令
                _pauseEvent.Wait(token);
                receive = SerialCommunicationService.SendSettingCommand("HOSTCRC", "EN");

            }
            else if (OnceOpenCRC)
            {
                //发送HOSTCRDEN指令
                _pauseEvent.Wait(token);
                receive = SerialCommunicationService.SendSettingCommand("HOSTCRC", "DN");
                OnceOpenCRC = false;
            }

            //获取机器型号
            _pauseEvent.Wait(token);
            string receive_MachineType = SerialCommunicationService.SendCommand(SpecialCommand.QueryMachineType, 10);
            SerialCommunicationService.MachineType = receive_MachineType;

            //发送HIMSG2N指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HIGSG2_PDF.Command, 50);
            HIGSG2_PDF.AnalysisStringToElement(receive);

            // 等待暂停或取消信号
            _pauseEvent.Wait(token);
            //发送HGRID指令
            receive = SerialCommunicationService.SendCommand(HGRID_PDF.Command, 50);
            //解析返回命令
            HGRID_PDF.AnalyseStringToElement(receive);
            //显示
            ACPowerVM = StringToIntConversion(HGRID_PDF.ACPower);
            //市电百分比
            ACTotalPwr = CountPercent(HGRID_PDF.ACPower, HIGSG2_PDF.ACTotalPwr);

            // 等待暂停或取消信号
            _pauseEvent.Wait(token);
            //发送HOP指令
            receive = SerialCommunicationService.SendCommand(HOP_PDF.Command, 50);
            //解析返回命令
            HOP_PDF.AnalysisStringToElement(receive);
            //逆变百分比
            InvTotalPwr = StringToIntConversion(HOP_PDF.LoadPercent);

            // 等待暂停或取消信号
            _pauseEvent.Wait(token);
            //发送HBAT指令
            receive = SerialCommunicationService.SendCommand(HBAT_PDF.Command, 50);
            //解析返回命令
            HBAT_PDF.AnalysisStringToElement(receive);

            _pauseEvent.Wait(token); // 等待暂停或取消信号
            //发送HEEP1指令
            receive = SerialCommunicationService.SendCommand(HEEP1_PDF.Command, 80);
            //解析返回指令
            HEEP1_PDF.AnalyseStringToElement(receive);

            _pauseEvent.Wait(token); // 等待暂停或取消信号
            //发送HEEP2指令
            receive = SerialCommunicationService.SendCommand(HEEP2.Command, 80);
            //解析返回指令
            HEEP2.AnalyseStringToElement(receive);

            //发送HEEP3指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HEEP3_PDF.Command, 80);
            HEEP3_PDF.AnalysisStringToElement(receive);

            //发送HIMSG1指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HIMSG1.Command, 21);
            HIMSG1.AnalysisStringToElement(receive);

            //发送HPV指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HPV_PDF.Command, 50);
            HPV_PDF.AnalysisStringToElement(receive);
            //MPPT百分比
            MPPTTotalPwr = CountPercent(HPV_PDF.PVPwr, HIGSG2_PDF.MPPTTotalPwr);

            //发送HTEMP指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HTEMP_PDF.Command, 50);
            HTEMP_PDF.AnalysisStringToElement(receive);

            //发送HCTMSG1指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HCTMSG1_PDF.Command, 80);
            HCTMSG1_PDF.AnalysisStringToElement(receive);

            //发送HGEN指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HGEN.Command, 60);
            HGEN.AnalyseStringToElement(receive);
        }

        /// <summary>
        /// GB3024通讯
        /// </summary>
        /// <param name="token"></param>
        private void CommunicationWithGB3024(CancellationToken token)
        {
            string receive = string.Empty;
            // 等待暂停或取消信号
            _pauseEvent.Wait(token);
            //发送查询机器指令
            string receive_MachineType = SerialCommunicationService.SendCommand(SpecialCommand.QueryMachineType, 10);
            //解析指令
            SerialCommunicationService.MachineType = receive_MachineType;

            //发送HBMS1指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HBMS1_VQ.Command, 70);
            HBMS1_VQ.AnalysisStringToElement(receive);


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


            // 等待暂停或取消信号
            _pauseEvent.Wait(token);
            //发送HOP指令
            receive = SerialCommunicationService.SendCommand(HOP_PDF.Command, 50);
            //解析返回命令
            HOP_PDF.AnalysisStringToElement(receive);
            //逆变百分比
            InvTotalPwr = StringToIntConversion(HOP_PDF.LoadPercent);

            //发送HPV指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HPV_PDF.Command, 50);
            HPV_PDF.AnalysisStringToElement(receive);
            //MPPT百分比
            MPPTTotalPwr = CountPercent(HPV_PDF.PVPwr, HIGSG2_PDF.MPPTTotalPwr);

            //发送HIMSG2N指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HIGSG2_PDF.Command, 50);
            HIGSG2_PDF.AnalysisStringToElement(receive);

            //发送HGRID指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HGRID_GB.Command, 50);
            //解析返回命令
            HGRID_GB.AnalyseStringToElement(receive);
            //显示
            ACPowerVM = StringToIntConversion(HGRID_GB.ACPower);
            //市电百分比
            ACTotalPwr = CountPercent(HGRID_GB.ACPower, HIGSG2_PDF.ACTotalPwr);

            //发送HTEMP指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HTEMP_PDF.Command, 50);
            HTEMP_PDF.AnalysisStringToElement(receive);

            //发送HBAT指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HBAT_VQ.Command, 50);
            HBAT_VQ.AnalysisStringToElement(receive);

            //发送HIMSG1指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HIMSG1.Command, 21);
            HIMSG1.AnalysisStringToElement(receive);


            _pauseEvent.Wait(token); // 等待暂停或取消信号
            //发送Hgen指令
            string receive_HGEN = SerialCommunicationService.SendCommand(HGEN.Command, 60);
            //解析返回指令
            HGEN.AnalyseStringToElement(receive_HGEN);

            //发送HSTS指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HSTS_GB.Command, 40);
            HSTS_GB.AnalyseStringToElement(receive);

        }

        /// <summary>
        /// VQ3024通讯
        /// </summary>
        /// <param name="token"></param>
        private void CommunicationWithVQ3024(CancellationToken token)
        {
            string receive = "";

            //获取机器型号
            _pauseEvent.Wait(token);
            string receive_MachineType = SerialCommunicationService.SendCommand(SpecialCommand.QueryMachineType, 10);
            SerialCommunicationService.MachineType = receive_MachineType;


            //发送HBMS1指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HBMS1_VQ.Command, 70);
            HBMS1_VQ.AnalysisStringToElement(receive);

            //发送HEEP1指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HEEP1_VQ.Command, 80);
            HEEP1_VQ.AnalyseStringToElement(receive);

            //发送HEEP2指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HEEP2.Command, 80);
            HEEP2.AnalyseStringToElement(receive);

            //发送HOP指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HOP_VQ.Command, 50);
            HOP_VQ.AnalyseStringToElement(receive);

            //发送HGRID指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HGRID_VQ.Command, 50);
            //解析返回命令
            HGRID_VQ.AnalyseStringToElement(receive);
            //显示
            ACPowerVM = StringToIntConversion(HGRID_PDF.ACPower);
            //市电百分比
            //ACTotalPwr = CountPercent(HGRID_PDF.ACPower, HIGSG2_PDF.ACTotalPwr);

            //发送HTEMP指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HTEMP_PDF.Command, 50);
            HTEMP_PDF.AnalysisStringToElement(receive);

            //发送HBAT指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HBAT_VQ.Command, 50);
            HBAT_VQ.AnalysisStringToElement(receive);

            //发送HBAT指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HIMSG1.Command, 21);
            HIMSG1.AnalysisStringToElement(receive);

        }

        /// <summary>
        /// GB6042通讯
        /// </summary>
        /// <param name="token"></param>
        private void CommunicationWithGB6042(CancellationToken token)
        {

            string receive = "";

            //获取机器型号
            _pauseEvent.Wait(token);
            string receive_MachineType = SerialCommunicationService.SendCommand(SpecialCommand.QueryMachineType, 10);
            SerialCommunicationService.MachineType = receive_MachineType;


            //发送HBMS1指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HBMS1_VQ.Command, 70);
            HBMS1_VQ.AnalysisStringToElement(receive);

            //发送HEEP1指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HEEP1_PDF.Command, 80);
            HEEP1_PDF.AnalyseStringToElement(receive);

            //发送HEEP2指令
            _pauseEvent.Wait(token); // 等待暂停或取消信号
            receive = SerialCommunicationService.SendCommand(HEEP2.Command, 80);
            HEEP2.AnalyseStringToElement(receive);

            // 等待暂停或取消信号
            _pauseEvent.Wait(token);
            //发送HOP指令
            receive = SerialCommunicationService.SendCommand(HOP_PDF.Command, 50);
            //解析返回命令
            HOP_PDF.AnalysisStringToElement(receive);
            //逆变百分比
            InvTotalPwr = StringToIntConversion(HOP_PDF.LoadPercent);
            

            //发送HPV指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HPV_PDF.Command, 50);
            HPV_PDF.AnalysisStringToElement(receive);
            //MPPT百分比
            MPPTTotalPwr = CountPercent(HPV_PDF.PVPwr, HIGSG2_PDF.MPPTTotalPwr);
           
            //发送HIMSG2N指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HIGSG2_PDF.Command, 50);
            HIGSG2_PDF.AnalysisStringToElement(receive);

            //发送HGRID指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HGRID_GB.Command, 50);
            //解析返回命令
            HGRID_GB.AnalyseStringToElement(receive);
            //显示
            ACPowerVM = StringToIntConversion(HGRID_GB.ACPower);
            //市电百分比
            ACTotalPwr = CountPercent(HGRID_GB.ACPower, HIGSG2_PDF.ACTotalPwr);
            
            //发送HTEMP指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HTEMP_PDF.Command, 50);
            HTEMP_PDF.AnalysisStringToElement(receive);

            //发送HBAT指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HBAT_VQ.Command, 50);
            HBAT_VQ.AnalysisStringToElement(receive);

            //发送HIMSG1指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HIMSG1.Command, 21);
            HIMSG1.AnalysisStringToElement(receive);

            //发送HGEN指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HGEN.Command, 60);
            HGEN.AnalyseStringToElement(receive);

            //发送HSTS指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HSTS_GB.Command, 40);
            HSTS_GB.AnalyseStringToElement(receive);

            //发送HPV指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HPV_PDF.Command, 50);
            HPV_PDF.AnalysisStringToElement(receive);

            //发送HPVB指令
            _pauseEvent.Wait(token);
            receive = SerialCommunicationService.SendCommand(HPVB_GB.Command, 50);
            HPVB_GB.AnalysisStringToElement(receive);

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
