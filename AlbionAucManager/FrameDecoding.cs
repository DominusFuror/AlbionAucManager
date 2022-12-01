using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlbionAucManager
{
    class FrameDecoding
    {


        public string nickName = "DominusFuror";
        public int networkDeviceId = 2;

        public string locationID;

   
        ILiveDevice device;
        string marketJSON = "";
        

        byte[] frameData;
        DateTime packetTime;
        public  void StartDecode()
        {


            var devices = CaptureDeviceList.Instance;
            device = devices[networkDeviceId];


            device.OnPacketArrival += OnPacketArrival;


            device.Open(DeviceModes.MaxResponsiveness, 1000);
            device.Capture();

        }
        public  void OnPacketArrival(object sender, PacketCapture e)
        {


            
            var tcpPacket = TcpPacket.ParsePacket(LinkLayers.Raw, e.GetPacket().Data);


            if (tcpPacket != null)
            {

                packetTime = e.GetPacket().Timeval.Date;
                frameData = e.GetPacket().GetPacket().Bytes;

                

                MarketPacketCheck();
                LocationPacketCheck();



            }
        }




        bool checkNextPacket = false;
        void MarketPacketCheck()
        {


            if (frameData.Length < 150) return;


            bool isAlbionMarketPacket = frameData[101] == 0x7b && frameData[102] == 0x22 && frameData[103] == 0x49 && frameData[104] == 0x64;
            int packetDataLength = frameData.Length;


            if (isAlbionMarketPacket)
            {

                for (int i = 0; i < frameData.Length; i++)
                {
                    if (frameData[i] == 0x07d)
                    {
                        try
                        {
                            frameData[i + 1] = 0x20;
                            frameData[i + 2] = 0x20;
                        }
                        catch
                        {


                        }

                    }
                }



                marketJSON += UTF8Encoding.UTF8.GetString(frameData, 101, frameData.Length - 101);

                bool isEnd = frameData[packetDataLength - 1] == 0x4b && frameData[packetDataLength - 2] == 0x00 && frameData[packetDataLength - 3] == 0x6b && frameData[packetDataLength - 4] == 0xfd;

                if (!isEnd)      checkNextPacket = true;
                return;
            }
            if (checkNextPacket)
            {

                for (int i = 0; i < frameData.Length; i++)
                {
                    if (frameData[i] == 0x07d)
                    {
                        try
                        {
                            frameData[i + 1] = 0x20;
                            frameData[i + 2] = 0x20;
                        }
                        catch
                        {


                        }

                    }
                }
            
                bool isEnd = frameData[packetDataLength - 1] == 0x4b && frameData[packetDataLength - 2] == 0x00 && frameData[packetDataLength - 3] == 0x6b && frameData[packetDataLength - 4] == 0xfd;

       

                if (isEnd)
                {
                    checkNextPacket = false;
                    marketJSON += UTF8Encoding.UTF8.GetString(frameData, 86, frameData.Length - 93);


            
                    marketJSON = marketJSON.Replace(",", ",\n");
                    marketJSON = marketJSON.Replace("}", "}\n\n\n\n");

                    
                  
                    Console.WriteLine(marketJSON);

                }
                if (!isEnd)
                {

                    marketJSON += UTF8Encoding.UTF8.GetString(frameData, 86, frameData.Length - 86);

                }
                if (!marketJSON.Contains("AuctionType")) return;



            }
        }


        void LocationPacketCheck()
        {
            if (frameData.Length < 700) return;
  
            


            string strData = UTF8Encoding.UTF8.GetString(frameData);


            byte[] afterNickNameSignature = {

                0x03, 0x62, 0x33, 0x04};
            if (strData.Contains(nickName + UTF8Encoding.UTF8.GetString(afterNickNameSignature)))
            {

                try
                {


                    int index = strData.IndexOf(nickName)  +nickName.Length+32 ;



                    locationID =  strData.Substring(index, 4);
                    Console.WriteLine(locationID);
                }
                catch
                {
                    // not location enter frame

                }
            
            }


        }



    }
}
