using System.Collections.ObjectModel;

namespace LaserHarp
{
    public class MidiDeviceListData
    {
        private readonly ObservableCollection<string> _collection;

        public MidiDeviceListData()
        {
            this._collection = new ObservableCollection<string>();
        }

        public ObservableCollection<string> GetListData()
        {
            _collection.Clear();

            var context = AppContext.GetContext();
            
            var names = context.MidiControl.GetMidiDeviceNames();

            foreach (var name in names)
            {
                _collection.Add(name);
            }

            return _collection;
        }
    }
}