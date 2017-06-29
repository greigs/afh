using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using PixyUSBNet;

namespace LaserHarp
{
    public class AppContext
    {
        public AppContext()
        {
            sb1 = new StringBuilder(550);
            sb2 = new StringBuilder(100);
        }

        private static AppContext Context { get; set; }
        
        public MidiControl MidiControl { get; set; } = new MidiControl();


        private string _expectedMidiOutDeviceName = "VirtualMIDISynth #1";

        private DateTime? timestamp;

        private StringBuilder sb1, sb2;
        private PixyConnection _pixyConnection;

        public static AppContext GetContext()
        {
            return Context ?? (Context = new AppContext());
        }

        public void DestroyContext()
        {
            _pixyConnection?.Close();
            MidiControl?.MidiOut?.Close();
        }

        public PixyConnection TryPixieConnection()
        {
            // ensure existing connection is closed
            _pixyConnection?.Close();
            _pixyConnection?.Dispose();
            //_pixyConnection = null;
            Thread.Sleep(1000);

            if (_pixyConnection == null)
            {
                _pixyConnection = new PixyConnection();
            }
            
            PixyResult result = PixyResult.USB_NOT_FOUND;


            try
            {
                result = _pixyConnection.Initialize();
            }
            catch
            {
                
            }

            // Make sure connection successful..
            if (result == PixyResult.SUCCESSFUL_CONNECTION)
            {
                Thread.Sleep(3000);
                if (_pixyConnection.BlockDataAvailable())
                {
                    return _pixyConnection;
                }
                
            }

            return null;
        }



        public bool GetPixyData(PixyConnection pixyConnection, TextBlock pixieGrid, TextBlock labels)
        {
            var dataBlockAvailable = pixyConnection.BlockDataAvailable();

            if (dataBlockAvailable)
            {
                // Do we have image blocks?
                Block[] blocks = pixyConnection.GetBlocks(20);

                if (blocks != null && blocks.Length > 0)
                {
                    if (!timestamp.HasValue)
                    {
                        timestamp = DateTime.Now;
                    }

                    if (DateTime.Now.AddMilliseconds(-50) > timestamp)
                    {
                        timestamp = DateTime.Now;
                        
                        sb2.Clear();

                        foreach (var block in blocks.OrderBy(x => x.X))
                        {
                            sb2.Append(block.X + ", " + block.Y + "\n");    
                        }
                        
                        sb1.Clear();

                        for (int i = 0; i < 50; i++)
                        {
                            for (int j = 0; j < 50; j++)
                            {
                                bool blockFound = false;
                                foreach (var block in blocks)
                                {
                                    
                                    if (block.X / 4 == j && block.Y / 8 == i)
                                    {
                                        blockFound = true;
                                    }
                                }
                                sb1.Append(blockFound ? '■' : '-');
                            }
                            sb1.Append("\n");
                        }
                            
                        ThreadInvoker.Instance.RunByUiThread(() =>
                        {
                            pixieGrid.Text = sb1.ToString();
                            labels.Text = sb2.ToString();
                        });
                    }
                }
                else
                {
                    // no signatures
                }

            }
            return dataBlockAvailable;
        }
    }
}
