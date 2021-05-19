using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;


namespace UDPTestToolArcNet
{
    class ArtNet
    {
        UdpClient udpClient, receiver;
        const string                           
            ARTNETHEADER = "4172742d4e657400", 
            ARTNET_OP_CODE_DMX_SET = "0050",
            ARTNET_OP_CODE_TOD_REQUEST = "0080",
            ARTNET_ARTTOD_REQ_PACKET =  "0000000000000000000000010100000000000000000000000000000000000000000000000000000000000000",
            ARTNET_ARTTOD_REQ_PACKET2=   "0000000000000000000000000100000000000000000000000000000000000000000000000000000000000000",
            ARTNET_OP_CODE_CONTROL_PACKET = "0082",
            ARTNET_ARTTOD_CONTROL_PACKET = "000000000000000000000101",
            ARTNET_PROT_VERSION = "000e",
            ARTNET_PHYSICAL = "00", 
            HEX_FORMAT_2_BYTES = "D2", 
            HEX_FORMAT_4_BYTES = "D4",
            ARTNET_LENGHT_BYTE0 = "02",
            ARTNET_LENGHT_BYTE1 = "00",
            ARTNET_UNIVERSE = "0100";
        static byte _sequenceCounter = 0;
        string _packet = "";
        const int CHANNEL_NUMBER = 512, CHANNEL_BYTES_NUMBER = 2;
        byte[] dataBytes = null;
        public string message;
        public ArtNet(int port) 
        {
                        
        }
              
        public void ConnectToDevice(string ip, int port) 
        {
            IPEndPoint localpt = new IPEndPoint(IPAddress.Any, 6454);
            udpClient = new UdpClient(ip,port);
            //udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Connect(ip, port);

            //receiver = new UdpClient();
            //udpClient.EnableBroadcast = true;
            //receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //receiver.Client.Bind(localpt);
        }
        
        //Формирование и отправка DMX сообщения для управления светом. 
        public void SendDMX(byte[] data)
        {
            _sequenceCounter++;
            string sequence = _sequenceCounter.ToString(HEX_FORMAT_2_BYTES);
            //Формирование АртНет составляющей пакета
            _packet = ARTNETHEADER + ARTNET_OP_CODE_DMX_SET + ARTNET_PROT_VERSION + sequence + ARTNET_PHYSICAL + ARTNET_UNIVERSE + ARTNET_LENGHT_BYTE0 + ARTNET_LENGHT_BYTE1;
           
            byte[] bytes = StringToByteArray(_packet);
            byte[] query = new byte[bytes.Length + CHANNEL_NUMBER];

            bytes.CopyTo(query, 0);
            data.CopyTo(query, bytes.Length);

            try
            {
                udpClient.Send(query, query.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length / 2).Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16)).ToArray();            
        }

        private void DiscoverySender(string header) {
            byte[] bytes = StringToByteArray(header);
            try
            {
                udpClient.Send(bytes, bytes.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
        }
        public void Discover() {
            //Формирование АртНет пакета
            DiscoverySender(ARTNETHEADER + ARTNET_OP_CODE_TOD_REQUEST + ARTNET_PROT_VERSION + ARTNET_ARTTOD_REQ_PACKET);
            DiscoverySender(ARTNETHEADER + ARTNET_OP_CODE_CONTROL_PACKET + ARTNET_PROT_VERSION + ARTNET_ARTTOD_CONTROL_PACKET);            
        }
        IPEndPoint remoteIp = null;
        public void ReceiveMessage()
        {

            //new IPEndPoint(IPAddress.Parse("2.188.104.63"), 6454); // адрес входящего подключения
            try
            {
                dataBytes = udpClient.Receive(ref remoteIp);// (ref remoteIp); // получаем данные
                //message = Encoding.Unicode.GetString(dataBytes);
               // MessageBox.Show(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Вот это сообщение");
            }
            finally
            {
                udpClient.Close();
            }
            if (dataBytes == null)
            {
                dataBytes = new byte[] { 0x00 };
            }
            message = BitConverter.ToString(dataBytes);
            //return message;
           
        }
        public void Close()
        {
            udpClient.Close();
        }
    }
}
