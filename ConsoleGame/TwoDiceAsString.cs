using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    class TwoDiceAsString
    {

        public TwoDiceAsString(string choice)
        {
 
            this.Value = choice;
        }
        public string Value { get; set; }

        public bool IsValid
        {
            get { return Value.Length == 2 && IsValidDiceValue(First) && IsValidDiceValue(Second); }
        }

        private bool IsValidDiceValue(int die)
        {
            return die > +1 && die <= 6;
        }

        public Int32 First { get { return Convert.ToInt32(Value.Substring(0, 1)); } }

        public Int32 Second { get { return Convert.ToInt32(Value.Substring(1, 1)); } }

    }
}
