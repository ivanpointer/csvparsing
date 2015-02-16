/*
 * (C)2015 Ivan Andrew Pointer (ivan@pointerplace.us)
 * Date: 02/16/2015
 * License: Apache License 2 (https://raw.githubusercontent.com/ivanpointer/csvparsing/master/LICENSE)
 * GitHub: https://github.com/ivanpointer/csvparsing
 */

using System;

namespace PointerPlace.CSVParsing
{
	/// <summary>
	/// An exception specific to the CSV parsing process
	/// </summary>
	public class CSVException : Exception
	{
		/// <summary>
		/// The column within which the exception occurred
		/// </summary>
		public ColumnDefinition Column { get; set; }
		/// <summary>
		/// The row within which the exception occurred
		/// </summary>
		public int? RowNumber { get; set; }

		/// <summary>
		/// An exception specific to the CSV parsing process
		/// </summary>
		public CSVException() : base() { }

		/// <summary>
		/// An exception specific to the CSV parsing process
		/// </summary>
		public CSVException(string message) : base(message) { }

		/// <summary>
		/// An exception specific to the CSV parsing process
		/// </summary>
		public CSVException(Exception cause) : base(null, cause) { }

		/// <summary>
		/// An exception specific to the CSV parsing process
		/// </summary>
		public CSVException(string message, Exception cause) : base(message, cause) { }
	}
}
