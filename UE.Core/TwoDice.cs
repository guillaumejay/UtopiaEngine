using System;

namespace UE.Core
{
    public class TwoDice
    {
        public TwoDice(int first, int second)
        {
            First = first;
            Second = second;
        }

        public int First { get; set; }
        public int Second { get; set; }

        public override string ToString()
        {
            return String.Format("{0},{1}",First,Second);
        }

        public int Sum { get { return First + Second; } }

        public void ModifyBothDie(int modifier)
        {
            First += modifier;
            Second += modifier;
        }
    }
}
