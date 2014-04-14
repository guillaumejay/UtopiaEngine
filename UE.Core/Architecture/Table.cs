using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace UE.Core.Architecture
{

    [DebuggerDisplay("IsEmpty ={IsEmpty},IStarted={IsStarted},ISFull= {IsFull}}")]
    public class Table
    {

        public Table()
        {
            Columns = new List<Column>();
        }

        public Table(int numberOfColumn)
        {
            Columns = new List<Column>();
            for (int I = 0; I < numberOfColumn; I++)
                Columns.Add(new Column());
        }

		/// <summary>
		/// Value by row and col
		/// </summary>
		/// <returns>The value, empty string as 0.</returns>
		/// <param name="row">Row.</param>
		/// <param name="col">Col.</param>
		public string CellValue (int row, int col)
		{
			return Columns [col].CellValue (row); 
		}

        [XmlAttribute]
        public int ForcedValue { get; set; }

        public int ColumnCount { get { return Columns.Count; } }

        public List<Column> Columns { get; set; }

        [XmlIgnore]
        public bool IsEmpty
        {
            get { return Columns == null || Columns.All(x => x.IsEmpty); }
        }

        [XmlIgnore]
        public bool IsStarted
        {
            get { return Columns != null && Columns.Any(x => x.IsEmpty == false); }
        }

        [XmlIgnore]
        public bool IsFull
        {
            get
            {
                return (ForcedValue!=0) || (Columns != null && Columns.All(x => x.IsFull));
            }
        }

        public int UnfilledCellCount { get { return Columns.Sum(x => x.UnfilledCount); } }

        /// <summary>
        /// Place a number in the table
        /// </summary>
        /// <param name="indexWhere">1 top Left,2 top next to left, [ColummCount] bottom Right</param>
        /// <param name="value">value</param>
        public void PlaceNumber(int indexWhere, int value)
        {
            int indexCol = ColumnFromPosition(indexWhere);
            Columns[indexCol].PlaceNumber(indexWhere <= ColumnCount, value);
        }

		/// <summary>
		/// Gets or sets the <see cref="UE.Core.Architecture.Table"/> at the specified index.
		/// </summary>
		/// <param name="index">Index. (0based)</param>
		public string this [int index]   // Indexer declaration
		{
			get 
            {
				int indexCol = ColumnFromPosition(index+1);
				Column c = Columns [indexCol];
				return  ((index + 1) <= ColumnCount) ? c.TopNumber : c.BottomNumber;
			}
			set
			{
			    PlaceNumber (index+1, Convert.ToInt32(value));
			}
		}

        public int SearchResult
        {
            get
            {
                if (this.ColumnCount != 3)
                    throw new InvalidOperationException("Trying to use SearchResult with Columns=" + ColumnCount);
                int top = 0, bottom = 0, exp = 100;
                for (int i = 0; i < 3; i++)
                {
                    top += Columns[i].Top * exp;
                    bottom += Columns[i].Bottom * exp;
                    exp /= 10;
                }
                return top - bottom;
            }
        }

        public void Clear()
        {
            ForcedValue = 0;
            foreach (Column column in Columns)
            {
                column.Clear();
            }
        }

        public void Clear(int column)
        {
            Columns[column].Clear();
        }
        internal int ColumnFromPosition(int position)
        {
            int indexCol = position - 1;
            if (indexCol >= ColumnCount)
                indexCol -= ColumnCount;
            return indexCol;
        }

        public int LinkResult
        {
            get
            {
                int value = 0;
                if (ForcedValue != 0)
                    return ForcedValue;
                foreach (Column c in Columns)
                {
                    value += c.ActivationValue;
                }
                return value;
            }
        }

        public int ActivationResult
        {
            get
            {
                int value = 0;
                if (ForcedValue != 0)
                    return ForcedValue;
                foreach (Column c in Columns)
                {
                    if (c.IsFull && c.EnergyPointValue > 0)
                        value += c.EnergyPointValue;
                }
                return value;
            }
        }
    }
}
