/*
 * (C)2015 Ivan Andrew Pointer (ivan@pointerplace.us)
 * Date: 02/16/2015
 * License: Apache License 2 (https://raw.githubusercontent.com/ivanpointer/csvparsing/master/LICENSE)
 * GitHub: https://github.com/ivanpointer/csvparsing
 */

using System.Collections.Generic;

namespace PointerPlace.CSVParsing
{
	/// <summary>
	/// Defines the settings for parsing a CSV table
	/// </summary>
	public class CSVDefinition
	{
		/// <summary>
		/// The default column delimiter: ','
		/// </summary>
		public const char DefaultColumnDelimiter = ',';
		/// <summary>
		/// The default text delimiter: '"'
		/// </summary>
		public const char DefaultTextDelimiter = '"';

		/// <summary>
		/// Flags indicating how the CSV table should be parsed
		/// </summary>
		public CSVFlags Flags { get; set; }
		/// <summary>
		/// The column delimiter to use for the CSV table
		/// </summary>
		public char ColumnDelimiter { get; set; }
		/// <summary>
		/// The text delimiter to use for the CSV table
		/// </summary>
		public char TextDelimiter { get; set; }

		/// <summary>
		/// The headers for the CSV table
		/// </summary>
		public ICollection<string> ColumnHeaders { get; set; }

		/// <summary>
		/// The column definitions for the CSV table
		/// </summary>
		public ICollection<ColumnDefinition> ColumnDefinitions { get; set; }
		/// <summary>
		/// The EOF definition for the CSV table
		/// </summary>
		public ICollection<string> EOFDefinition { get; set; }

		/// <summary>
		/// The standard constructor which sets the basic settings
		/// for CSV parsing
		/// </summary>
		public CSVDefinition()
		{
			Flags = CSVFlags.None;
			ColumnDelimiter = DefaultColumnDelimiter;
			TextDelimiter = DefaultTextDelimiter;
		}
	}
}
