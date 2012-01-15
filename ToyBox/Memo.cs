using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToyBox
{
    public class Memo<T1>
    {
        public Memo()
        {
        }

        public Memo(T1 value1)
        {
            this.Value1 = value1;
        }

        public T1 Value1 { get; set; }
    }

    public class Memo<T1, T2>
    {
        public Memo()
        {
        }

        public Memo(T1 value1, T2 value2)
        {
            this.Value1 = value1;
            this.Value2 = value2;
        }

        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
    }

    public class Memo<T1, T2, T3>
    {
        public Memo()
        {
        }

        public Memo(T1 value1, T2 value2, T3 value3)
        {
            this.Value1 = value1;
            this.Value2 = value2;
            this.Value3 = value3;
        }

        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
        public T3 Value3 { get; set; }
    }
}
