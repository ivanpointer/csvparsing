/*
 * (C)2015 Ivan Andrew Pointer (ivan@pointerplace.us)
 * Date: 02/16/2015
 * License: Apache License 2 (https://raw.githubusercontent.com/ivanpointer/csvparsing/master/LICENSE)
 * GitHub: https://github.com/ivanpointer/csvparsing
 */

namespace PointerPlace.CSVParsing
{
	/// <summary>
	/// Defines the different types to be parsed
	/// from the CSV file.  Mainly used for parsing
	/// formatted text such as DateTime objects
	/// </summary>
	public enum CSVTypeHint
	{
		/// <summary>
		/// Will be treated as a string;
		///  this is the default behavior
		/// </summary>
		String,

		/// <summary>
		/// Will be treated as a DateTime
		/// </summary>
		DateTime
	}
}
