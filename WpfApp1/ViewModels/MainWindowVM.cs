using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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

    }
}
