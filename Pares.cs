using System;
using System.Collections;

namespace Map_Editor_HoD.Code.Models
{
    [Serializable]
    [Stride.Core.DataContract]
    public class Pares<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Pares(T1 item1 = default(T1), T2 item2 = default(T2))
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public Pares()
        {
        }

        #region ForEach Compatibility
        /*public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();*/
        #endregion
    }
}
