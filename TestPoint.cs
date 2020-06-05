using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ETRU_TestBench.DataGridClass;

namespace ETRU_TestBench
{
   public static class TestPoint
    {
        public static ICollection<DataGridClass> CreateTestCourse()
        {
            ObservableCollection<DataGridClass> myCourse = new ObservableCollection<DataGridClass>();
            myCourse.Add(new DataGridClass(1, TestState.TestRunning));
            myCourse.Add(new DataGridClass(2, TestState.TestRunning));
            myCourse.Add(new DataGridClass(3, TestState.TestRunning));
            myCourse.Add(new DataGridClass(4, TestState.TestRunning));
            myCourse.Add(new DataGridClass(5, TestState.TestRunning));
            myCourse.Add(new DataGridClass(6, TestState.TestRunning));
            myCourse.Add(new DataGridClass(7, TestState.TestRunning));
            myCourse.Add(new DataGridClass(8, TestState.TestRunning));
            return myCourse;

        }
    }
}
