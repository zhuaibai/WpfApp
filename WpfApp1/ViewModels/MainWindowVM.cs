using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Command;
using WpfApp1.Models;
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
            // 示例：添加一些初始日志
            AddLog("程序启动");

            #endregion
        }

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


        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="logMessage"></param>
        public void AddLog(string logMessage)
        {
            string timestamp = DateTime.Now.ToString("yyyy - MM - dd HH:mm:ss");
            string formattedLog = $"[{timestamp}] {logMessage}";
            //LogMessages.Add(formattedLog);
            LogMessages.Insert(0, formattedLog);

            if (IsScrollingEnabled)
            {
                // 这里可以通过事件或其他方式通知视图滚动到最新日志
            }
            SaveLogToFile(formattedLog);
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

    }
}
