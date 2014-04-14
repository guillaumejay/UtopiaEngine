using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace UE.Core.Architecture
{
    [DebuggerDisplay("IsEmpty ={IsEmpty},ISFull= {IsFull},T:{Top},B:{Bottom},F:{ForcedValue}")]
    public class Column
    {
        public Column()
        { }

        public Column(int top, int bottom)
        {
            Top = top;
            Bottom = bottom;
        }

		/// <summary>
		/// Cells the value.
		/// </summary>
		/// <returns>The value, Empty string if 0</returns>
		/// <param name="row">Row.</param>
		public string CellValue (int row)
		{
			int value = (row == 0) ? Top : Bottom;
			if (value == 0)
				return String.Empty;
			return value.ToString ();
		}

        /// <summary>
        /// Calculated value for 
        /// </summary>
        public int ActivationValue
        {
            get
            {
                if (ForcedValue != 0)
                    return ForcedValue;
                return Top - Bottom;
            }
        }

        [XmlAttribute]
        public Int32 Bottom { get;  set; }

        [XmlIgnore]
        public string BottomNumber { get { return AsString(Bottom); } }

        /// <summary>
        /// Energy point value (0=lock, -1 = reset);
        /// </summary>
        public int EnergyPointValue
        {
            get
            {
                switch (ActivationValue)
                {
                    case 5:
                        return 2;
                    case 4:
                        return 1;
                    case 0:
                        return 0;
                    default:
                        return -1;
                }

            }
        }

        /// <summary>
        /// If >0, Forced value for the column, without looking at topébottom
        /// </summary>
        [XmlAttribute]
        public int ForcedValue { get; set; }

        [XmlIgnore]
        public bool IsEmpty { get { return Top == 0 && Bottom == 0; } }

        public bool IsFull { get { return (ForcedValue != 0) || (Top != 0 && Bottom != 0); } }

        [XmlAttribute]
        public Int32 Top { get; set; }

        [XmlIgnore]
		/// <summary>
		/// Gets the top number as a string
		/// </summary>
		/// <value>Top Number, "" if  0</value>
        public string TopNumber { get { return AsString(Top); } }
       
        public int UnfilledCount
        {
            get
            {
                return ((Top == 0) ? 1 : 0) + ((Bottom == 0) ? 1 : 0);
            }
        }

        internal void Clear()
        {
            Top = 0;
            Bottom = 0;
            ForcedValue = 0;
        }

        internal void PlaceNumber(bool onTop, int value)
        {
            if (onTop)
                Top = value;
            else
            {
                Bottom = value;
            }
        }

        private string AsString(int value)
        {
            if (value == 0)
                return "";
            return value.ToString();
        }
    }
}
