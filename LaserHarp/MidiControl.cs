using System.Collections.Generic;
using System.Linq;
using NAudio.Midi;

namespace LaserHarp
{
    public class MidiControl
    {

        private int _selectedInstrument = 1;

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
            var mm = MidiMessage.StartNote(i, 127, _selectedInstrument);
            MidiOut.Send(mm.RawData);
        }

        public void NoteOff(int i)
        {
            var mm = MidiMessage.StopNote(i, 127, _selectedInstrument);
            MidiOut.Send(mm.RawData);
        }

        public void SelectInstrument(int inst)
        {
            _selectedInstrument = inst;
        }

        public void SelectDeviceByName(string device)
        {
            MidiOut?.Close();

            int deviceno;
            FindOutMidiDevice(GetMidiDevices(), device, out deviceno);
            MidiOut = new MidiOut(deviceno);
        }
    }
}