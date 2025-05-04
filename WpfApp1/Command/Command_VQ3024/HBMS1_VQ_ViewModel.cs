using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_VQ3024
{
    public class HBMS1_VQ_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HBMS1\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HBMS1_VQ_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令


           
            #endregion
        }

        #region 协议类型

        private string _ProtocolType;

        public string ProtocolType
        {
            get { return _ProtocolType; }
            set
            {
                _ProtocolType = value;
                this.RaiseProperChanged(nameof(ProtocolType));
            }
        }

        #endregion

        #region BMS通信正常

        private string _BMS_ComOK;

        public string BMS_ComOK
        {
            get { return _BMS_ComOK; }
            set
            {
                _BMS_ComOK = value;
                this.RaiseProperChanged(nameof(BMS_ComOK));
            }
        }

        #endregion

        #region BMS低电报警
        private string _BMS_LowBattAlarm;

        public string BMS_LowBattAlarm
        {
            get { return _BMS_LowBattAlarm; }
            set
            {
                _BMS_LowBattAlarm = value;
                this.RaiseProperChanged(nameof(BMS_LowBattAlarm));
            }
        }


        #endregion

        #region BMS低电故障

        private string _BMS_LowBattFault;

        public string BMS_LowBattFault
        {
            get { return _BMS_LowBattFault; }
            set
            {
                _BMS_LowBattFault = value;
                this.RaiseProperChanged(nameof(BMS_LowBattFault));
            }
        }

        #endregion

        #region BMS允许充电

        private string _BMS_ChgEnable;

        public string BMS_ChgEnable
        {
            get { return _BMS_ChgEnable; }
            set
            {
                _BMS_ChgEnable = value;
                this.RaiseProperChanged(nameof(BMS_ChgEnable));
            }
        }

        #endregion

        #region BMS允许放电

        private string _BMS_DisEnable;

        public string BMS_DisEnable
        {
            get { return _BMS_DisEnable; }
            set
            {
                _BMS_DisEnable = value;
                this.RaiseProperChanged(nameof(BMS_DisEnable));
            }
        }


        #endregion

        #region BMS充电过流

        private string _BMS_ChgOC;

        public string BMS_ChgOC
        {
            get { return _BMS_ChgOC; }
            set
            {
                _BMS_ChgOC = value;
                this.RaiseProperChanged(nameof(BMS_ChgOC));
            }
        }

        #endregion

        #region BMS放电过流
        private string _BMS_DisOC;

        public string BMS_DisOC
        {
            get { return _BMS_DisOC; }
            set
            {
                _BMS_DisOC = value;
                this.RaiseProperChanged(nameof(BMS_DisOC));
            }
        }


        #endregion

        #region BMS温度过低

        private string _BMS_UnderTemp;

        public string BMS_UnderTemp
        {
            get { return _BMS_UnderTemp; }
            set
            {
                _BMS_UnderTemp = value;
                this.RaiseProperChanged(nameof(BMS_UnderTemp));
            }
        }

        #endregion

        #region BMS温度过高

        private string _BMS_OverTemp;

        public string BMS_OverTemp
        {
            get { return _BMS_OverTemp; }
            set
            {
                _BMS_OverTemp = value;
                this.RaiseProperChanged(nameof(BMS_OverTemp));
            }
        }

        #endregion

        #region BMS放电电压限制

        private string _BMS_DisVoltLimit;

        public string BMS_DisVoltLimit
        {
            get { return _BMS_DisVoltLimit; }
            set
            {
                _BMS_DisVoltLimit = value;
                this.RaiseProperChanged(nameof(BMS_DisVoltLimit));
            }
        }

        #endregion

        #region BMS充电电压限制
        private string _BMS_ChgVoltLimit;

        public string BMS_ChgVoltLimit
        {
            get { return _BMS_ChgVoltLimit; }
            set
            {
                _BMS_ChgVoltLimit = value;
                this.RaiseProperChanged(nameof(BMS_ChgVoltLimit));
            }
        }

        #endregion

        #region BMS充电电流限制
        private string _BMS_ChgCurrLimit;

        public string BMS_ChgCurrLimit
        {
            get { return _BMS_ChgCurrLimit; }
            set
            {
                _BMS_ChgCurrLimit = value;
                this.RaiseProperChanged(nameof(BMS_ChgCurrLimit));
            }
        }

        #endregion

        #region BMS当前SOC

        private string _BMS_SOC;

        public string BMS_SOC
        {
            get { return _BMS_SOC; }
            set
            {
                _BMS_SOC = value;
                this.RaiseProperChanged(nameof(BMS_SOC));
            }
        }

        #endregion

        #region BMS充电电流
        private string _BMS_ChgCurr;

        public string BMS_ChgCurr
        {
            get { return _BMS_ChgCurr; }
            set
            {
                _BMS_ChgCurr = value;
                this.RaiseProperChanged(nameof(BMS_ChgCurr));
            }
        }


        #endregion

        #region BMS放电电流

        private string _BMS_DisCurr;

        public string BMS_DisCurr
        {
            get { return _BMS_DisCurr; }
            set
            {
                _BMS_DisCurr = value;
                this.RaiseProperChanged(nameof(BMS_DisCurr));
            }
        }

        #endregion

        #region BMS平均温度(开尔文)

        private string _BMS_AvgTemp;

        public string BMS_AvgTemp
        {
            get { return _BMS_AvgTemp; }
            set
            {
                _BMS_AvgTemp = value;
                this.RaiseProperChanged(nameof(BMS_AvgTemp));
            }
        }
        #endregion

        #region 通用方法

        private bool Validate(string value)
        {
            switch (value)
            {
                //市电电压       
                //case "BusVolt_Inputs":
                //    return !string.IsNullOrWhiteSpace(BusVolt_Inputs);
                ////PFC工作状态   
                //case "PFCStatus_Inputs":
                //    return !string.IsNullOrWhiteSpace(PFCStatus_Inputs);
                ////市电功率   
                //case "ACPower_Inputs":
                //    return !string.IsNullOrWhiteSpace(ACPower_Inputs);
                ////温度限制 
                //case "TempLimit_Inputs":
                //    return !string.IsNullOrWhiteSpace(TempLimit_Inputs);
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
            string[] Values = value.Split(" ");

            try
            {
                //协议类型
                ProtocolType = Values[0].Substring(1);
                //BMS通信正常
                BMS_ComOK = Values[1].Substring(0,1);
                //BMS低电报警
                BMS_LowBattAlarm = Values[1].Substring(1,1);
                //BMS低电报警故障
                BMS_LowBattFault = Values[1].Substring(2,1);
                //BMS允许充电
                BMS_ChgEnable = Values[1].Substring(3,1);
                //BMS允许放电
                BMS_DisEnable = Values[1].Substring(4,1);
                //BMS充电过流
                BMS_ChgOC = Values[1].Substring(5, 1);
                //BMS放电过流
                BMS_DisOC = Values[1].Substring(6, 1);
                //BMC温度过低
                BMS_UnderTemp = Values[1].Substring(7, 1);
                //BMS温度过高
                BMS_OverTemp = Values[1].Substring(8, 1);
                //BMS放电电压限制
                BMS_DisVoltLimit = Values[2];
                //BMS充电电压限制
                BMS_ChgVoltLimit = Values[3];
                //BMS充电电流限制
                BMS_ChgCurrLimit = Values[4];
                //BMS当前SOC
                BMS_SOC = Values[5];
                //BMS充电电流
                BMS_ChgCurr = Values[6];
                //BMS放电电流
                BMS_DisCurr = Values[7];
                //BMS平均温度
                BMS_AvgTemp = Values[8];


            }
            catch (Exception ex)
            {
                ReceiveException("HBAT异常");
            }
        }

        /// <summary>
        /// 接收异常使用方法
        /// </summary>
        /// <param name="exceptionDescription"></param>
        private void ReceiveException(string exceptionDescription)
        {
            //协议类型
            ProtocolType = exceptionDescription;
            //BMS通信正常
            BMS_ComOK = exceptionDescription;
            //BMS低电报警
            BMS_LowBattAlarm = exceptionDescription ;
            //BMS低电故障
            BMS_LowBattFault = exceptionDescription;
            //BMS允许充电
            BMS_ChgEnable = exceptionDescription;
            //BMS允许放电
            BMS_DisEnable = exceptionDescription;
            //BMS充电过流
            BMS_ChgOC = exceptionDescription;
            //BMS放电过流
            BMS_DisOC = exceptionDescription;
            //BMC温度过低
            BMS_UnderTemp = exceptionDescription;
            //BMS温度过高
            BMS_OverTemp = exceptionDescription;
            //BMS放电电压限制
            BMS_DisVoltLimit = exceptionDescription;
            //BMS充电电压限制
            BMS_ChgVoltLimit = exceptionDescription;
            //BMS充电电流限制
            BMS_ChgCurrLimit = exceptionDescription;
            //BMS当前SOC
            BMS_SOC = exceptionDescription;
            //BMS充电电流
            BMS_ChgCurr = exceptionDescription;
            //BMS放电电流
            BMS_DisCurr = exceptionDescription ;
            //BMS平均温度
            BMS_AvgTemp = exceptionDescription;
        }


        #endregion
    }
}
