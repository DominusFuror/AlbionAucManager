using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbionAucManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var devices = CaptureDeviceList.Instance;
            var device = devices[2];

            device.OnPacketArrival += OnPacketArrival;


            device.Open(DeviceModes.Promiscuous, 1000);
            // начинаем захват пакетов
            device.Capture();



        }

        public static void OnPacketArrival( object sender ,PacketCapture e)
        {

       
            // получение только TCP пакета из всего фрейма
            var tcpPacket = TcpPacket.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            // получение только IP пакета из всего фрейма
    
            if (tcpPacket != null )
            {
                DateTime time = e.GetPacket().Timeval.Date;
                int len = e.GetPacket().Data.Length;
       
                // данные пакета
                var data = tcpPacket.PayloadPacket;
                Console.WriteLine(UTF8Encoding.UTF8.GetString(e.Data.ToArray()));
            }
        }

     
    }
}
