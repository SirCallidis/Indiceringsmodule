using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common
{
    public class Observable : INotifyPropertyChanged
    {
        public bool ValuesChanged;

        //below: empty anonymous method subscribed through delegate, so the subscribers list is never null
        public event PropertyChangedEventHandler PropertyChanged = delegate { };



        //protected and virtual so you could overwrite it in a subclass

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        // = null makes string parameter optional. if none is provided: compiler uses
        //callermembername
        {
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void SetProperty<T>(ref T member, T val, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return; //checks to see if value changed

            member = val; //sets the new value
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); //triggers event
            ValuesChanged = true;
        }
    }
}
