/*
 * (C)2015 Ivan Andrew Pointer (ivan@pointerplace.us)
 * Date: 02/16/2015
 * License: Apache License 2 (https://raw.githubusercontent.com/ivanpointer/csvparsing/master/LICENSE)
 * GitHub: https://github.com/ivanpointer/csvparsing
 */

using System.Reflection;
using System.Text.RegularExpressions;

namespace PointerPlace.CSVParsing
{
	/// <summary>
	/// Defines a column in a CSV table
	/// </summary>
	public class ColumnDefinition
	{
		/// <summary>
		/// The index of the column in the CSV
		/// </summary>
		public int? Index { get; set; }
		/// <summary>
		/// The header of the column in the CSV
		/// </summary>
		public string Header { get; set; }

		/// <summary>
		/// Indicates that a value is required in
		/// the column
		/// </summary>
		public bool Required { get; set; }
		/// <summary>
		/// An optional regex pattern to be used to
		/// validate the value in the column
		/// </summary>
		public Regex Regex { get; set; }
		/// <summary>
		/// Hints at the type of the text in the column.
		/// Used mainly for format parsing.
		/// </summary>
		public CSVTypeHint TypeHint { get; set; }
		/// <summary>
		/// A format used for parsing the value in the
		/// column
		/// </summary>
		public string Format { get; set; }
		/// <summary>
		/// An optional value which indicates that
		/// the record is an EOF indicator.  This is
		/// an "exact match" value, meaning this value
		/// must be found in the column in the last
		/// record of the CSV
		/// </summary>
		public string EOFCondition { get; set; }

		/// <summary>
		/// A reference to the property in the POCO that
		/// the column is associated with
		/// </summary>
		public PropertyInfo Property { get; set; }
	}
}
