using System.Collections.Specialized;
using System.ComponentModel;

namespace Models
{
    public class ControlInfo : INotifyPropertyChanged
    {
        public string WindowId { get; set; }
        public string WindowTitle { get; set; }

        private string controlId;
        public string ControlId 
        {
            get => controlId;
            set { controlId = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ControlId))); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
