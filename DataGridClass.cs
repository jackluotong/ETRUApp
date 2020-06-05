using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ETRU_TestBench.MainWindow;

namespace ETRU_TestBench
{

    public class DataGridClass: INotifyPropertyChanged
    {
        public enum TestState
        {
            TestFailed = 0,
            TestOK = 1,
            TestNotDone = 2,
            TestRunning = 3,
            TestNotRequired = 4
        }

        public event PropertyChangedEventHandler PropertyChanged;
         
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }

            remove
            {
                PropertyChanged -= value;
            }
        }

        private TestState state;

        public  TestState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        public int ID { get; set; }

        public void OnPropertyChanged(string PropertyName)
        {
            //PropertyChangedEventHandler tempHandler = PropertyChanged;
            //if (tempHandler != null)
            //    tempHandler(this, new PropertyChangedEventArgs(PropertyName)); 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public DataGridClass(int id,TestState state)
        {
            this.ID = id;
            this.State = state;
        }
      
    }
}
