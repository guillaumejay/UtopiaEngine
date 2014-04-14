using System;
using Cirrious.CrossCore.Converters;
using System.Globalization;
using UE.Core.Architecture;

namespace UE.Core.Converters
{
	public class TableConverter : MvxValueConverter<Table, string>
	{
		protected override string Convert(Table value, Type targetType, object parameter, CultureInfo culture)
		{
			string rowcol=parameter.ToString();
			int row =System.Convert.ToInt32( rowcol.Substring (0, 1));
			int col= System.Convert.ToInt32(rowcol.Substring(1,1));
			return value.CellValue(row,col);
		}

		protected override Table ConvertBack(string value, Type targetType, object parameter, CultureInfo culture)
		{
			string rowcol=parameter.ToString();
            
			int row =System.Convert.ToInt32( rowcol.Substring (0, 1));
			int col= System.Convert.ToInt32(rowcol.Substring(1,1));
			return new Table ();
		}
	}
}

