using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using LaserHarp.Properties;
using PixyUSBNet;

namespace LaserHarp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThreadInvoker _threadInvoker;
        private DateTime? _dataBlockLastRecievedTimestamp;
        private bool _isClosing;
        private bool _pixyDisconnectedHaventRetriedYet;


        public MainWindow()
        {
            InitializeComponent();
            _threadInvoker = ThreadInvoker.Instance;
            _threadInvoker.InitDispacter(Dispatcher.CurrentDispatcher);
            StartAndMonitorPixyConnection();
            
        }

        private void LogPixy(string s)
        {
            ThreadInvoker.Instance.RunByUiThread(() =>
            {
                pixieLabel.Content = s;
            });
        }

        private void StartAndMonitorPixyConnection()
        {
            ThreadInvoker.Instance.RunByNewThread(() =>
            {
                StartPixyConnection(0);

                while (!_isClosing)
                {
                    if (_pixyDisconnectedHaventRetriedYet)
                    {
                        _pixyDisconnectedHaventRetriedYet = false;

                        StartPixyConnection(1000);
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        private void StartPixyConnection(int delayAfterConnection)
        {
            ThreadInvoker.Instance.RunByNewThread(() =>
            {
                PixyConnection pixy = null;
                while (pixy == null)
                {
                    LogPixy("Attempting to connect to Pixy...");
                    pixy = AppContext.GetContext().TryPixieConnection();
                    Thread.Sleep(2000);
                    if (pixy == null)
                    {
                        LogPixy("Pixy camera not found");
                        Thread.Sleep(7000);
                    }
                    Thread.Sleep(delayAfterConnection);
                }
                return pixy;
            }, ConnectionSuccessCallback);
        }

        private void ConnectionSuccessCallback(PixyConnection conn)
        {
            if (conn != null)
            {
                LogPixy("Connected to Pixy!");
                bool dataBlockAvailable = false;
                while (DataBlockRecievedInAcceptableWindow(AppContext.GetContext().GetPixyData(conn, textBlock, infoTextBlock)) && !this._isClosing)
                {
                    Thread.Sleep(1);
                }
                 _pixyDisconnectedHaventRetriedYet = true;
                _dataBlockLastRecievedTimestamp = null;
                LogPixy("Pixy camera disconnected. Will Retry...");
            }
        }

        private bool DataBlockRecievedInAcceptableWindow(bool dataBlockAvailable)
        {
            if (dataBlockAvailable)
            {
                this._dataBlockLastRecievedTimestamp = DateTime.Now;
                return true;
            }
            else
            {
                if (this._dataBlockLastRecievedTimestamp.HasValue)
                {
                    return DateTime.Now.AddSeconds(-5) < _dataBlockLastRecievedTimestamp;
                }
                else
                {
                    return true;
                }
            }
            
        }

        private void MidiDeviceListData_OnDataChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
            AppContext.GetContext().DestroyContext();
        }

        private void midiRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDeviceBefore = midiDeviceComboBox.SelectedValue?.ToString();

            var midilistObjectProvider = (ObjectDataProvider)Resources["MidiDeviceListData"];
            var midilistData = (MidiDeviceListData)midilistObjectProvider.ObjectInstance;
            midilistData.GetListData();

            if (!string.IsNullOrEmpty(selectedDeviceBefore))
            {
                for (int i = 0; i < midiDeviceComboBox.Items.Count; i++)
                {
                    var item = midiDeviceComboBox.Items.GetItemAt(i);
                    if (item.ToString() == selectedDeviceBefore)
                    {
                        midiDeviceComboBox.SelectedValue = item;
                    }
                } 
            }
        }

        private void MidiDeviceComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                AppContext.GetContext().MidiControl.SelectDeviceByName((string)e.AddedItems[0]);
            }   
        }

        private void InstrumentComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string instStr = (string)e.AddedItems[0];
            int inst = int.Parse(instStr.Split(' ')[0]);
            AppContext.GetContext().MidiControl.SelectInstrument(inst);
        }
    }
}
