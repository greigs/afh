using System.Collections.ObjectModel;

namespace LaserHarp
{
    public class MidiInstrumentListData
    {
        private readonly ObservableCollection<string> _collection;

        public MidiInstrumentListData()
        {
            this._collection = new ObservableCollection<string>();
        }

        public ObservableCollection<string> GetListData()
        {
            _collection.Clear();

            for (int i = 0; i < MidiInstruments.InstrumentNames.Length; i++)
            {
                _collection.Add(i + 1 + "  " + MidiInstruments.InstrumentNames[i]);
            }

            return _collection;
        }
    }
}