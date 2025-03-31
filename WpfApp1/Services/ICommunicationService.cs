using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    interface ICommunicationService
    {
        
        void InitiateCom();//初始化串口
        bool OpenCom(); //打开串口
        bool CloseCom();//关闭串口

        string SendCommand(string command,int returnCount);//发送指令并返回接受字符串

        string SendSettingCommand(string frontComand, string setValue);//发送设置指令，返回接收值

        bool IsOpen();//串口是否开启

        string getComName();//获取串口名字

    }
}
