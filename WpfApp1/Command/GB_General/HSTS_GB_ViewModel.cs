using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Convert;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.GB_General
{
    public class HSTS_GB_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HSTS\r"; //返回40字节 (00 L010100000000 11211001000L002000000

        public string Command { get { return command; } }

        public string MachineType;

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HSTS_GB_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;


            #region 初始化指令

            #endregion
        }

        #region 故障代码
        private string _FaultCode;
        public string FaultCode
        {
            get { return _FaultCode; }
            set
            {
                _FaultCode = value;
                this.RaiseProperChanged(nameof(FaultCode));
            }
        }
        #endregion

        #region 模式
        private string _Mode;
        public string Mode
        {
            get { return _Mode; }
            set
            {
                _Mode = StringToMode(value);
                this.RaiseProperChanged(nameof(Mode));
            }
        }

        /// <summary>
        /// 字符转模式状态
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string StringToMode(string str)
        {
            switch (str)
            {
                case "P":
                    return "初始上电模式";
                case "S":
                    return "待机模式";
                case "L":
                    return "市电模式";
                case "B":
                    return "电池模式";
                case "F":
                    return "故障模式";
                case "D":
                    return "关机模式";
                case "X":
                    return "测试模式";
                default:
                    return str;
            }
        }
        #endregion



        #region AC状态下PV馈能到负载
        private string _PVToLoadAC;
        public string PVToLoadAC
        {
            get { return _PVToLoadAC; }
            set
            {
                if(value == "0")
                {
                    _PVToLoadAC = "无";
                }else if(value == "1")
                {
                    _PVToLoadAC = "有";
                }else
                _PVToLoadAC = value;
                this.RaiseProperChanged(nameof(PVToLoadAC));
            }
        }
        #endregion

        #region 机器是否有输出
        private string _OutputStatus;
        public string OutputStatus
        {
            get { return _OutputStatus; }
            set
            {
                if (value == "0")
                {
                    _OutputStatus = "无";
                }
                else if (value == "1")
                {
                    _OutputStatus = "有";
                }
                else
                    _OutputStatus = value;
                this.RaiseProperChanged(nameof(OutputStatus));
            }
        }
        #endregion

        #region 电池低电报警
        private string _BattLowAlarm;
        public string BattLowAlarm
        {
            get { return _BattLowAlarm; }
            set
            {
                _BattLowAlarm = NumToStr(value);
                this.RaiseProperChanged(nameof(BattLowAlarm));
            }
        }
        #endregion

        #region 电池未接
        private string _BattDisconnected;
        public string BattDisconnected
        {
            get { return _BattDisconnected; }
            set
            {
                _BattDisconnected = NumToStr(value);
                this.RaiseProperChanged(nameof(BattDisconnected));
            }
        }
        #endregion

        #region 输出过载
        private string _OutputOverload;
        public string OutputOverload
        {
            get { return _OutputOverload; }
            set
            {
                _OutputOverload = NumToStr(value);
                this.RaiseProperChanged(nameof(OutputOverload));
            }
        }
        #endregion

        #region 机器过温
        private string _OverTemp;
        public string OverTemp
        {
            get { return _OverTemp; }
            set
            {
                _OverTemp = NumToStr(value);
                this.RaiseProperChanged(nameof(OverTemp));
            }
        }
        #endregion

        #region EEPROM数据异常
        private string _EEPROM_DataErr;
        public string EEPROM_DataErr
        {
            get { return _EEPROM_DataErr; }
            set
            {
                _EEPROM_DataErr = NumToStr(value);
                this.RaiseProperChanged(nameof(EEPROM_DataErr));
            }
        }
        #endregion

        #region EEPROM读写异常
        private string _EEPROM_IOErr;
        public string EEPROM_IOErr
        {
            get { return _EEPROM_IOErr; }
            set
            {
                _EEPROM_IOErr = NumToStr(value);
                this.RaiseProperChanged(nameof(EEPROM_IOErr));
            }
        }
        #endregion

        #region PV功率过低异常
        private string _PVLowPwrFault;
        public string PVLowPwrFault
        {
            get { return _PVLowPwrFault; }
            set
            {
                _PVLowPwrFault = NumToStr(value);
                this.RaiseProperChanged(nameof(PVLowPwrFault));
            }
        }
        #endregion

        #region 输入电压过高
        private string _InputOV;
        public string InputOV
        {
            get { return _InputOV; }
            set
            {
                _InputOV = NumToStr(value);
                this.RaiseProperChanged(nameof(InputOV));
            }
        }
        #endregion

        #region 电池电压过高
        private string _BattOV;
        public string BattOV
        {
            get { return _BattOV; }
            set
            {
                _BattOV = NumToStr(value);
                this.RaiseProperChanged(nameof(BattOV));
            }
        }
        #endregion

        #region 风扇转速异常
        private string _FanSpeedFault;
        public string FanSpeedFault
        {
            get { return _FanSpeedFault; }
            set
            {
                _FanSpeedFault = NumToStr(value);
                this.RaiseProperChanged(nameof(FanSpeedFault));
            }
        }
        #endregion

        #region 并机系统里机器的总数
        private string _ParallelUnits;
        public string ParallelUnits
        {
            get { return _ParallelUnits; }
            set
            {
                _ParallelUnits = value;
                this.RaiseProperChanged(nameof(ParallelUnits));
            }
        }
        #endregion

        #region 并网标志
        private string _GridTieFlag;
        public string GridTieFlag
        {
            get { return _GridTieFlag; }
            set
            {
                if(value == "0")
                {
                    _GridTieFlag = "离网";
                }else if(value == "1")
                {
                    _GridTieFlag = "等待并网";
                }else if (value == "2")
                {
                    _GridTieFlag = "正在并网";
                }else
                _GridTieFlag = value;
                this.RaiseProperChanged(nameof(GridTieFlag));
            }
        }
        #endregion

        #region 并机系统中角色
        private string _ParallelRole;
        public string ParallelRole
        {
            get { return _ParallelRole; }
            set
            {
                _ParallelRole = value;
                this.RaiseProperChanged(nameof(ParallelRole));
            }
        }
        #endregion

        #region 主输出继电器状态
        private string _MainRelayStat;
        public string MainRelayStat
        {
            get { return _MainRelayStat; }
            set
            {
                if (value == "0")
                {
                    _MainRelayStat = "关闭";
                }
                else if (value == "1")
                {
                    _MainRelayStat = "开启";
                }
                else
                    _MainRelayStat = value;
                this.RaiseProperChanged(nameof(MainRelayStat));
            }
        }
        #endregion

        #region 第二输出当前状态
        private string _SecOutStat;
        public string SecOutStat
        {
            get { return _SecOutStat; }
            set
            {
                if(value == "0")
                {
                    _SecOutStat = "关闭";
                }else if(value == "1")
                {
                    _SecOutStat = "开启";
                }else
                _SecOutStat = value;
                this.RaiseProperChanged(nameof(SecOutStat));
            }
        }
        #endregion

        #region BMS通讯异常
        private string _BMS_ComFault;
        public string BMS_ComFault
        {
            get { return _BMS_ComFault; }
            set
            {
                _BMS_ComFault = NumToStr(value);
                this.RaiseProperChanged(nameof(BMS_ComFault));
            }
        }
        #endregion

        #region 温度传感器异常
        private string _TempSensorFault;
        public string TempSensorFault
        {
            get { return _TempSensorFault; }
            set
            {
                _TempSensorFault = NumToStr(value);
                this.RaiseProperChanged(nameof(TempSensorFault));
            }
        }
        #endregion

        #region 市电灯状态
        private string _ACLED;
        public string ACLED
        {
            get { return _ACLED; }
            set
            {
                _ACLED = NumToStr2(value);
                this.RaiseProperChanged(nameof(ACLED));
            }
        }
        #endregion

        #region 逆变灯状态
        private string _InvLED;
        public string InvLED
        {
            get { return _InvLED; }
            set
            {
                _InvLED = NumToStr2(value);
                this.RaiseProperChanged(nameof(InvLED));
            }
        }
        #endregion

        #region 充电灯状态
        private string _ChgLED;
        public string ChgLED
        {
            get { return _ChgLED; }
            set
            {
                _ChgLED = NumToStr2(value);
                this.RaiseProperChanged(nameof(ChgLED));
            }
        }
        #endregion

        #region 报警灯状态
        private string _AlarmLED;
        public string AlarmLED
        {
            get { return _AlarmLED; }
            set
            {
                _AlarmLED = NumToStr2(value);
                this.RaiseProperChanged(nameof(AlarmLED));
            }
        }
        #endregion

        /// <summary>
        /// 根据0/1返回是/否
        /// </summary>
        /// <returns></returns>
        private string NumToStr(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "空";
            }
            if (value == "0")
            {
                return "否";
            } else if (value == "1") {
                return "是";
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// 根据0/1/2返回关闭/常亮/闪烁
        /// </summary>
        /// <returns></returns>
        private string NumToStr2(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "空";
            }
            if (value == "0")
            {
                return "关闭";
            }
            else if (value == "1")
            {
                return "常亮";
            }
            else if (value == "2")
            {   
                return "闪烁";
            }
            else
            {
                return value;
            }
        }

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
                ////市电电压       
                //case "MainsVoltage_Inputs":
                //    return !string.IsNullOrWhiteSpace(MainsVoltage_Inputs);
                ////市电频率   
                //case "MainsFrequency_Inputs":
                //    return !string.IsNullOrWhiteSpace(MainsFrequency_Inputs);
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
        public void AnalyseStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                ReceiveException("空");
                return;
            }
            string[] Values = value.Split(" ");
            try
            {
                //故障代码
                FaultCode = Values[0].Substring(1, 2);
                //模式
                Mode = Values[1].Substring(0,1);
                //AC状态下PV馈能到负载
                PVToLoadAC = Values[1].Substring(1,1);
                //机器是否有输出
                OutputStatus = Values[1].Substring(2,1);
                //电池低电报警
                BattLowAlarm = Values[1].Substring(3,1);
                //电池未接
                BattDisconnected = Values[1].Substring(4,1);
                //输出过载
                OutputOverload = Values[1].Substring(5,1);
                //机器过温
                OverTemp = Values[1].Substring(6,1);
                //EEPROM数据异常
                EEPROM_DataErr = Values[1].Substring(7,1);
                //EEPROM读写异常
                EEPROM_IOErr = Values[1].Substring(8,1);
                //PV功率过低异常
                PVLowPwrFault = Values[1].Substring(9,1);
                //输入电压过高
                InputOV = Values[1].Substring(10,1);
                //电池电压过高
                BattOV = Values[1].Substring(11,1);
                //风扇转速异常
                FanSpeedFault = Values[1].Substring(12,1);
                //并机系统里机器的总数
                ParallelUnits = Values[2].Substring(0,1);
                //并网标志
                GridTieFlag = Values[2].Substring(1,1);
                //并机系统中角色
                ParallelRole = Values[2].Substring(2,1);
                //主输出继电器状态
                MainRelayStat = Values[2].Substring(3,1);
                //第二输出当前状态
                SecOutStat = Values[2].Substring(4,1);
                //BMS通讯异常
                BMS_ComFault = Values[2].Substring(5,1);
                //温度传感器异常
                TempSensorFault = Values[2].Substring(6,1);
                //市电灯状态
                ACLED = Values[2].Substring(7,1);
                //逆变灯状态
                InvLED = Values[2].Substring(8,1);
                //充电灯状态
                ChgLED = Values[2].Substring(9,1);
                //报警灯状态
                AlarmLED = Values[2].Substring(10,1);




            }
            catch (Exception ex)
            {
                ReceiveException("HGRID异常");
            }
        }

        /// <summary>
        /// 接收异常使用方法
        /// </summary>
        /// <param name="exceptionDescription"></param>
        private void ReceiveException(string exceptionDescription)
        {
            //故障代码
            FaultCode = exceptionDescription;
            //模式
            Mode = exceptionDescription;
            //AC状态下PV馈能到负载
            PVToLoadAC = exceptionDescription;
            //机器是否有输出
            OutputStatus = exceptionDescription;
            //电池低电报警
            BattLowAlarm = exceptionDescription;
            //电池未接
            BattDisconnected = exceptionDescription;
            //输出过载
            OutputOverload = exceptionDescription;
            //机器过温
            OverTemp = exceptionDescription;
            //EEPROM数据异常
            EEPROM_DataErr = exceptionDescription;
            //EEPROM读写异常
            EEPROM_IOErr = exceptionDescription;
            //PV功率过低异常
            PVLowPwrFault = exceptionDescription;
            //输入电压过高
            InputOV = exceptionDescription;
            //电池电压过高
            BattOV = exceptionDescription;
            //风扇转速异常
            FanSpeedFault = exceptionDescription;
            //并机系统里机器的总数
            ParallelUnits = exceptionDescription;
            //并网标志
            GridTieFlag = exceptionDescription;
            //并机系统中角色
            ParallelRole = exceptionDescription;
            //主输出继电器状态
            MainRelayStat = exceptionDescription;
            //第二输出当前状态
            SecOutStat = exceptionDescription;
            //BMS通讯异常
            BMS_ComFault = exceptionDescription;
            //温度传感器异常
            TempSensorFault = exceptionDescription;
            //市电灯状态
            ACLED = exceptionDescription;
            //逆变灯状态
            InvLED = exceptionDescription;
            //充电灯状态
            ChgLED = exceptionDescription;
            //报警灯状态
            AlarmLED = exceptionDescription;
        }

        /// <summary>
        /// 把选项转化为指令格式
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        private string GetSelectedToCommand(string selected)
        {
            switch (selected)
            {
                //输出电压
                //case "TempLimit_Inputs":
                //    if (string.IsNullOrWhiteSpace(TempLimit_Inputs))
                //    {
                //        return string.Empty;
                //    }
                //    else if (TempLimit_Inputs == "关闭")
                //    {
                //        return "00";
                //    }
                //    else if (TempLimit_Inputs == "开启")
                //    {
                //        return "01";
                //    }
                //    return "";
                default:
                    return string.Empty;

            }

        }
    }
}
