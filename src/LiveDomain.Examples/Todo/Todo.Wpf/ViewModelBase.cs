using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Todo.Wpf
{
    [Serializable]
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            PropertyChanged.Invoke(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
