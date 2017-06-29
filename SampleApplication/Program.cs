using PixyUSBNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAudio.Midi;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            var mh = SetupMidi();


            // Set up our connection to pixy:
            var pixyConnection = new PixyConnection();

            Console.WriteLine("Attempting to connect to Pixy...");
            var connectionResult = pixyConnection.Initialize();

            // Make sure connection successful..
            if (connectionResult == PixyResult.SUCCESSFUL_CONNECTION)
            {
                Console.WriteLine("Pixie connected!");
                    
                try
                {
                    // Read some Pixy config:
                    Console.WriteLine("Ver:" + pixyConnection.GetFirmwareVersion().ToString());
                    Console.WriteLine();

                    //var result = pixyConnection.SendCommand("cc_setSigRegion 0 1");
                   
                    Console.WriteLine("Listening for data in 2 seconds...");
                    System.Threading.Thread.Sleep(2000);

                    // Press any key to end connection:
                    while (!Console.KeyAvailable)
                    {
                        // Do we have block data we can read?
                        if (pixyConnection.BlockDataAvailable())
                        {
                            // Do we have image blocks?
                            Block[] blocks = pixyConnection.GetBlocks(20);

                            Console.Clear();
                            Console.SetCursorPosition(0, 0);

                            // If yes, display them:
                            if (blocks != null && blocks.Length > 0)
                            {
                                Console.WriteLine($"Press any key to disconnect; Showing {blocks.Length} block[s]:");
                                if (blocks.Length > 0)
                                {
                                    Console.SetCursorPosition(0, 0);
                                    for (int x = 0; x < blocks.Length; x++)
                                    {
                                        if (blocks[x].Y > 10 && blocks[x].Y < 400)
                                        {

                                            Console.SetCursorPosition(blocks[x].X/4, blocks[x].Y/8);
                                            Console.Write("[*]");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Press any key to disconnect; no signatures detected. Bring one within view or set a signature. ");
                            }
                            Thread.Sleep(20);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when reading pixy: " + ex.Message);
                }
                finally
                {
                    Console.WriteLine("Done; attempting to disconnect...");
                    pixyConnection.Close();
                    Console.WriteLine("Disconnected!");
                }
            }
            else
            {
                Console.WriteLine("Could not connect to pixy. :( " + connectionResult.ToString());
            }
            Console.ReadKey(true);
        }

        private static MidiHelper SetupMidi()
        {
            var mh = new MidiHelper();
            // get midi devices
            var expectedMidiOutDeviceName = "VirtualMIDISynth #1"; //"loopMIDI Port";
            var midiDevices = mh.GetMidiDevices();
            int foundMidiDeviceId;
            mh.MidiDevice = mh.FindOutMidiDevice(midiDevices, expectedMidiOutDeviceName, out foundMidiDeviceId);

            if (mh.MidiDevice.ProductName == null)
            {
                throw new Exception("Midi device " + expectedMidiOutDeviceName +
                                    " not found. Check LoopMidi is configured correctly.");
            }

            mh.MidiOut = new MidiOut(foundMidiDeviceId);

            return mh;
        }
    }

    public class MidiHelper
    {
        public IList<MidiOutCapabilities> GetMidiDevices()
        {
            List<MidiOutCapabilities> outDevices = new List<MidiOutCapabilities>();
            var numDevices = MidiOut.NumberOfDevices;
            for (int i = 0; i < numDevices; i++)
            {
                outDevices.Add(MidiOut.DeviceInfo(i));
            }
            return outDevices;
        }

        public IList<string> GetMidiDeviceNames()
        {
            return GetMidiDevices().Select(x => x.ProductName).ToList();
        }


        public MidiOutCapabilities FindOutMidiDevice(IList<MidiOutCapabilities> devices, string deviceName, out int foundMidiDeviceId)
        {
            foundMidiDeviceId = -1;
            var found = devices.Any(x => x.ProductName == deviceName);
            var result = devices.FirstOrDefault(x => x.ProductName == deviceName);
            if (found)
            {
                foundMidiDeviceId = devices.IndexOf(result);    
            }
            return result;
        }

        public MidiOutCapabilities MidiDevice { get; set; }
        public MidiOut MidiOut { get; set; }

        public void NoteOn(int i)
        {
            var mm = MidiMessage.StartNote(i, 127, 1);
            MidiOut.Send(mm.RawData);
        }

        public void NoteOff(int i)
        {
            var mm = MidiMessage.StopNote(i, 127, 1);
            MidiOut.Send(mm.RawData);
        }
    }
}
