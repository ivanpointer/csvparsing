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
	/// Defines the different flags to control the CSV parsing behavior
	/// </summary>
	[Flags]
	public enum CSVFlags
	{
		/// <summary>
		/// Default behavior; Headers in source file, exceptions are thrown instead of trapped and recorded,
		/// and the number of columns must be consistent for all rows
		/// </summary>
		None = 0,

		/// <summary>
		/// The source file doesn't include the headers on the first row
		/// </summary>
		SourceExcludesHeaders = 1,

		/// <summary>
		/// Continue processing on error
		/// </summary>
		FaultTolerant = 2,

		/// <summary>
		/// The number of columns in each row is allowed to vary.  This doesn't
		/// override any validation applied to the columns.
		/// </summary>
		ColumnLeniant = 4
	}

	public static class CSVFlagsExt
	{
		/// <summary>
		/// Indicates whether "flags" has the flag "hasFlag"
		/// </summary>
		/// <param name="flags">The flags to check</param>
		/// <param name="hasFlag">The flag to check for</param>
		/// <returns>A boolean indicating whether or not "flags" has the flag "hasFlag", true indicates that "flags" contains "hasFlag"</returns>
		private static bool HasFlag(this CSVFlags flags, CSVFlags hasFlag)
		{
			return (flags & hasFlag) == hasFlag;
		}
	}
}
