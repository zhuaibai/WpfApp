using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.ViewModels
{
    public class SerialCountVM:BaseViewModel
    {
        /// <summary>
        /// 发送帧
        /// </summary>
        private int _Send;

        public int SendFrame
        {
            get { return _Send; }
            set
            {
                _Send = value;
                this.RaiseProperChanged(nameof(SendFrame));
            }
        }

        /// <summary>
        /// 接收帧
        /// </summary>
        private int _ReceiveFrame;

        public int ReceiveFrame
        {
            get { return _ReceiveFrame; }
            set
            {
                _ReceiveFrame = value;
                this.RaiseProperChanged(nameof(ReceiveFrame));
            }
        }

        /// <summary>
        /// 发送帧增加
        /// </summary>
        /// <param name="send"></param>
        public void AddSendFrame(int send)
        {
            SendFrame += send;
        }
        /// <summary>
        /// 发送帧减少
        /// </summary>
        /// <param name="receive"></param>
        public void AddReceiveFrame(int receive) { ReceiveFrame += receive; }




        /// <summary>
        /// 置零
        /// </summary>
        public void RemoveAllFrame()
        {
            SendFrame = 0;
            ReceiveFrame = 0;
        }
    }
}
