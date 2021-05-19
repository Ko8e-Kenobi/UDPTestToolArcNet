using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UDPTestToolArcNet
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ArtNet udpConnection;
        const int CHANNEL_NUMBER = 512, DEFAULT_PORT = 6454;
        const string DEFAULT_IP = "2.188.104.63";
        byte[] _dmxData = new byte[CHANNEL_NUMBER], discoveryData;
        int _port;
        string _ip;
        Task receiveTask;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
            
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            udpConnection.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            udpConnection = new ArtNet(DEFAULT_PORT);
            udpConnection.ConnectToDevice(DEFAULT_IP, DEFAULT_PORT);
            Label_IP.Content = DEFAULT_IP;
            Label_PORT.Content = DEFAULT_PORT;
            TextBox_IP.Text = DEFAULT_IP;            
            TextBox_Port.Text = DEFAULT_PORT.ToString();
            receiveTask = new Task(udpConnection.ReceiveMessage);
            receiveTask.Start();
        }
        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {            
            byte value;            
            try
            {
                value = Convert.ToByte(TextBox_Value.Text);                
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный формат количества каналов или значения каналов");
                return;
            }            
            for (int i = 0; i < CHANNEL_NUMBER; i++)
            {
                _dmxData[i] = value;
            }
            udpConnection.SendDMX(_dmxData);
            FillDataGrid(_dmxData);
        }

       
        private void FillDataGrid(byte[] values) 
        {
            DataTable dt = new DataTable();

            string header = "";

            dt.Columns.Add("№x10", typeof(string));

            const int ROWS_NUMBER = 52, COLUMNS_NUMBER = 10;
            for (int i = 0; i < ROWS_NUMBER; i++)
            {                
                dt.Rows.Add();
            }
            for (int i = 0; i < COLUMNS_NUMBER+1; i++)
            {
                header = $"{i}";
                dt.Columns.Add(header, typeof(byte));
            }
            
            int index = 0;
            for (int rowId = 0; rowId < ROWS_NUMBER; rowId++)
            {
                dt.Rows[rowId][0] = $"{rowId}0";
                for (int columnId = 1; columnId < COLUMNS_NUMBER; columnId++)
                {                   
                    if (index >= values.Length)
                    {
                        dt.Rows[rowId][columnId] = 0;
                    }
                    else
                    {
                        dt.Rows[rowId][columnId] = values[index];
                    }                    
                    index++;
                }                
            }            
            dataGrid1.DataContext = dt;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            udpConnection.Discover();
            //TextBox_BytesString.Text.Insert(TextBox_BytesString.MaxLength, discoveryData[28].ToString() + discoveryData[29] + discoveryData[30]);
            //byte TOD1_1 = discoveryData[28];
            //byte TOD1_2 = discoveryData[29];
            //byte TOD1_3 = discoveryData[30];
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _port = int.Parse(TextBox_Port.Text);
            udpConnection.ConnectToDevice(TextBox_IP.Text, _port);
            Label_IP.Content = TextBox_IP.Text;
            Label_PORT.Content = _port.ToString();
        }

        public void updateReceiveMessage()
        {
            TextBox_BytesString.Text = udpConnection.message;
        }
    }
}
