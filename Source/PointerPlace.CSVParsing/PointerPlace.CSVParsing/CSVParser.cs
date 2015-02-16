using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PointerPlace.CSVParsing
{
	/// <summary>
	/// A utility for parsing CSV data into different formats
	/// </summary>
	public class CSVParser
	{

		#region Constants

		// EOF validation statuses
		private const bool EOFValid = true;
		private const bool EOFInvalid = false;

		#endregion

		#region Public Methods

		#region Text Reader

		/// <summary>
		/// Parses the CSV read by the given TextReader into an IEnumerable&lt;T&gt;
		/// 
		/// Uses a "yield return" construct, allowing for processing to occur one line at a time within the CSV data.
		/// </summary>
		/// <typeparam name="T">The type of the object being created from the CSV data</typeparam>
		/// <param name="textReader">The text reader from which to read the CSV data</param>
		/// <param name="csvDef">The CSVDefinition which defines how the CSV data is to be parsed</param>
		/// <param name="exceptionHandler">An optional exception handler, through which exceptions will be passed, instead of bubbled up</param>
		/// <returns>An IEnumerable&lt;T&gt; built from the CSV data</returns>
		public static IEnumerable<T> Parse<T>(TextReader textReader, CSVDefinition csvDef = null, Action<CSVException> exceptionHandler = null) where T : class, new()
		{
			// Get our CSVDefinition
			if (csvDef == null)
				csvDef = new CSVDefinition();

			// Flag shortcuts
			var hasHeaders = !csvDef.Flags.HasFlag(CSVFlags.SourceExcludesHeaders);
			var faultTolerant = csvDef.Flags.HasFlag(CSVFlags.FaultTolerant);
			var columnLenient = csvDef.Flags.HasFlag(CSVFlags.ColumnLeniant);

			// Context variables
			bool eof = false;
			int rowNumber = 0;

			// Read the header row
			if (hasHeaders && csvDef.ColumnHeaders == null)
				csvDef.ColumnHeaders = ReadNextRow(textReader, csvDef, out eof);

			// Make sure we have our column and EOF definitions
			GetColDefs(csvDef, typeof(T), csvDef.ColumnHeaders);
			GetEOFDef(csvDef);
			var colDefs = GetColDefArray(csvDef.ColumnDefinitions);

			// Showtime!
			ICollection<string> row;
			while ((row = ReadNextRow(textReader, csvDef, out eof)) != null)
			{
				// If we aren't at the end of the file, or we don't have an EOF def
				if (!eof || (csvDef.EOFDefinition == null || csvDef.EOFDefinition.Count == 0))
				{
					// The item
					T item = GetInstance<T>();

					try
					{
						// Check our row counts
						if (!columnLenient && row.Count != colDefs.Length)
							throw new CSVException(String.Format("Number of columns inconsistent with column definition, found {0}, expected {1}", row.Count, colDefs.Length)) { RowNumber = rowNumber };

						// Row counts match, now parse out our item
						var colArray = row.ToArray();
						for (int colIndex = 0; colIndex < colArray.Length; colIndex++)
						{
							var colDef = colDefs[colIndex];
							if (colDef != null)
							{
								// Get a hold of our value
								var value = colArray[colIndex];

								// Validate the value
								ValidateValue(rowNumber, colDef, value);

								// Cast and assign the value to our item
								var convertedValue = ConvertValue(value, colDef.Property.PropertyType, colDef.TypeHint, colDef.Format);
								colDef.Property.SetValue(item, convertedValue, null);
							}
						}
					}
					catch (Exception cause)
					{
						HandleException(cause, exceptionHandler, faultTolerant);
					}

					// We have parsed out our item, now return it
					yield return item;

					// Next row...
					rowNumber++;
				}
				else
				{
					// Check our EOF definition
					if (VerifyEOF(row, csvDef.EOFDefinition) == EOFInvalid)
						HandleException(new CSVException("EOF validation failed, last record did not match EOF Condition"), exceptionHandler, faultTolerant);

					// EOF validation passed, nothing more to return!
					break;
				}
			}
		}

		/// <summary>
		/// Parses the CSV read by the given TextReader into an IEnumerable&lt;IEnumerable&lt;string&gt;&gt;
		/// 
		/// Uses a "yield return" construct, allowing for processing to occur one line at a time within the CSV data.
		/// </summary>
		/// <param name="textReader">The text reader from which to read the CSV data</param>
		/// <param name="csvDef">The CSVDefinition which defines how the CSV data is to be parsed</param>
		/// <param name="exceptionHandler">An optional exception handler, through which exceptions will be passed, instead of bubbled up</param>
		/// <returns>An IEnumerable&lt;IEnumerable&lt;string&gt;&gt; built from the CSV data</returns>
		public static IEnumerable<IEnumerable<string>> Parse(TextReader textReader, CSVDefinition csvDef = null, Action<CSVException> exceptionHandler = null)
		{
			// Get our CSVDefinition
			if (csvDef == null)
				csvDef = new CSVDefinition();

			// Flag shortcuts
			var hasHeaders = !csvDef.Flags.HasFlag(CSVFlags.SourceExcludesHeaders);
			var faultTolerant = csvDef.Flags.HasFlag(CSVFlags.FaultTolerant);
			var columnLenient = csvDef.Flags.HasFlag(CSVFlags.ColumnLeniant);

			// Context variables
			bool eof = false;
			int rowNumber = 0;
			int? expectedColumns = null;

			// Read the header row
			if (hasHeaders && csvDef.ColumnHeaders == null)
			{
				csvDef.ColumnHeaders = ReadNextRow(textReader, csvDef, out eof);
				expectedColumns = csvDef.ColumnHeaders.Count;
			}

			// Showtime!
			ICollection<string> row;
			while ((row = ReadNextRow(textReader, csvDef, out eof)) != null)
			{
				// If we aren't at the end of the file, or we don't have an EOF def
				if (!eof || (csvDef.EOFDefinition == null || csvDef.EOFDefinition.Count == 0))
				{
					// Setup
					expectedColumns = expectedColumns ?? row.Count;

					// Check our row counts
					if (!columnLenient && row.Count != expectedColumns)
						throw new CSVException(String.Format("Number of columns inconsistent with expectation, found {0}, expected {1}", row.Count, expectedColumns)) { RowNumber = rowNumber };

					// Row counts line up, return what we have
					yield return row;

					// Next row...
					rowNumber++;
				}
				else
				{
					// Check our EOF definition
					if (VerifyEOF(row, csvDef.EOFDefinition) == EOFInvalid)
						HandleException(new CSVException("EOF validation failed, last record did not match EOF Condition"), exceptionHandler, faultTolerant);

					// EOF validation passed, nothing more to return!
					break;
				}
			}
		}

		/// <summary>
		/// Parses the CSV read by the given TextReader into an IEnumerable&lt;IDictionary&lt;string, string&gt;&gt;
		/// 
		/// Uses a "yield return" construct, allowing for processing to occur one line at a time within the CSV data.
		/// 
		/// The headers must be defined for this to work, either in-file, or in the CSVDefinition.
		/// </summary>
		/// <param name="textReader">The text reader from which to read the CSV data</param>
		/// <param name="csvDef">The CSVDefinition which defines how the CSV data is to be parsed</param>
		/// <param name="exceptionHandler">An optional exception handler, through which exceptions will be passed, instead of bubbled up</param>
		/// <returns>An IEnumerable&lt;IDictionary&lt;string, string&gt;&gt; built from the CSV data</returns>
		public static IEnumerable<IDictionary<string, string>> ParseDictionary(TextReader textReader, CSVDefinition csvDef = null, Action<CSVException> exceptionHandler = null)
		{
			// Get our CSVDefinition
			if (csvDef == null)
				csvDef = new CSVDefinition();

			// Flag shortcuts
			var hasHeaders = !csvDef.Flags.HasFlag(CSVFlags.SourceExcludesHeaders);
			var faultTolerant = csvDef.Flags.HasFlag(CSVFlags.FaultTolerant);
			var columnLenient = csvDef.Flags.HasFlag(CSVFlags.ColumnLeniant);

			// Context variables
			bool eof = false;
			int rowNumber = 0;
			int? expectedColumns = null;

			// Read the header row
			if (hasHeaders && csvDef.ColumnHeaders == null)
			{
				csvDef.ColumnHeaders = ReadNextRow(textReader, csvDef, out eof);
				expectedColumns = csvDef.ColumnHeaders.Count;
			}

			// Check the headers
			if (csvDef.ColumnHeaders == null)
				HandleException(
					new CSVException("Headers are required for parsing CSV into a dictionary"),
					exceptionHandler,
					false);
			var headers = csvDef.ColumnHeaders.ToArray();

			// Showtime!
			ICollection<string> row;
			while ((row = ReadNextRow(textReader, csvDef, out eof)) != null)
			{
				// If we aren't at the end of the file, or we don't have an EOF def
				if (!eof || (csvDef.EOFDefinition == null || csvDef.EOFDefinition.Count == 0))
				{
					// Setup
					var item = new Dictionary<string, string>();
					expectedColumns = expectedColumns ?? row.Count;

					// Check our row counts
					if (!columnLenient && row.Count != expectedColumns)
						throw new CSVException(String.Format("Number of columns inconsistent with expectation, found {0}, expected {1}", row.Count, expectedColumns)) { RowNumber = rowNumber };

					// Build the dictionary
					int colIndex = 0;
					foreach (var col in row)
						if (colIndex < headers.Length)
							item[headers[colIndex++]] = col;
						else
							break;

					// Row counts line up, return what we have
					yield return item;

					// Next row...
					rowNumber++;
				}
				else
				{
					// Check our EOF definition
					if (VerifyEOF(row, csvDef.EOFDefinition) == EOFInvalid)
						HandleException(new CSVException("EOF validation failed, last record did not match EOF Condition"), exceptionHandler, faultTolerant);

					// EOF validation passed, nothing more to return!
					break;
				}
			}
		}

		#endregion

		#region String

		/// <summary>
		/// Parses the CSV in the given string into an IEnumerable&lt;T&gt;
		/// 
		/// Uses a "yield return" construct, allowing for processing to occur one line at a time within the CSV data.
		/// </summary>
		/// <typeparam name="T">The type of the object being created from the CSV data</typeparam>
		/// <param name="csvString">A string containing the CSV data</param>
		/// <param name="csvDef">The CSVDefinition which defines how the CSV data is to be parsed</param>
		/// <param name="exceptionHandler">An optional exception handler, through which exceptions will be passed, instead of bubbled up</param>
		/// <returns>An IEnumerable&lt;T&gt; built from the CSV data</returns>
		public static IEnumerable<T> Parse<T>(string csvString, CSVDefinition csvDef = null, Action<CSVException> exceptionHandler = null) where T : class, new()
		{
			using (var textReader = new StringReader(csvString))
				foreach (var item in Parse<T>(textReader, csvDef, exceptionHandler))
					yield return item;
		}

		/// <summary>
		/// Parses the CSV in the given string into an IEnumerable&lt;IEnumerable&lt;string&gt;&gt;
		/// 
		/// Uses a "yield return" construct, allowing for processing to occur one line at a time within the CSV data.
		/// </summary>
		/// <param name="csvString">A string containing the CSV data</param>
		/// <param name="csvDef">The CSVDefinition which defines how the CSV data is to be parsed</param>
		/// <param name="exceptionHandler">An optional exception handler, through which exceptions will be passed, instead of bubbled up</param>
		/// <returns>An IEnumerable&lt;IEnumerable&lt;string&gt;&gt; built from the CSV data</returns>
		public static IEnumerable<IEnumerable<string>> Parse(string csvString, CSVDefinition csvDef = null, Action<CSVException> exceptionHandler = null)
		{
			using (var textReader = new StringReader(csvString))
				foreach (var item in Parse(textReader, csvDef, exceptionHandler))
					yield return item;
		}

		/// <summary>
		///  Parses the CSV in the given string into an IEnumerable&lt;IDictionary&lt;string, string&gt;&gt;
		/// 
		/// Uses a "yield return" construct, allowing for processing to occur one line at a time within the CSV data.
		/// 
		/// The headers must be defined for this to work, either in-data, or in the CSVDefinition.
		/// </summary>
		/// <param name="csvString">A string containing the CSV data</param>
		/// <param name="csvDef">The CSVDefinition which defines how the CSV data is to be parsed</param>
		/// <param name="exceptionHandler">An optional exception handler, through which exceptions will be passed, instead of bubbled up</param>
		/// <returns>An IEnumerable&lt;IDictionary&lt;string, string&gt;&gt; built from the CSV data</returns>
		public static IEnumerable<IDictionary<string, string>> ParseDictionary(string csvString, CSVDefinition csvDef = null, Action<CSVException> exceptionHandler = null)
		{
			using (var textReader = new StringReader(csvString))
				foreach (var item in ParseDictionary(textReader, csvDef, exceptionHandler))
					yield return item;
		}

		#endregion

		#endregion

		#region Internals

		#region Generic Constructors

		private static readonly IDictionary<Type, ConstructorInfo> _constructorCache = new Dictionary<Type, ConstructorInfo>();
		private static ConstructorInfo GetConstructor(Type t)
		{
			if (_constructorCache.ContainsKey(t) == false)
				lock (_constructorCache)
					if (_constructorCache.ContainsKey(t) == false)
						_constructorCache[t] = t.GetConstructor(new Type[] { });

			return _constructorCache[t];
		}
		private static T GetInstance<T>()
		{
			var constructor = GetConstructor(typeof(T));
			return (T)constructor.Invoke(new object[] { });
		}

		#endregion

		#region Column Definitions

		private static readonly IDictionary<Type, ICollection<ColumnDefinition>> _colDefCache = new Dictionary<Type, ICollection<ColumnDefinition>>();
		private static void GetColDefs(CSVDefinition csvDef, Type t, IEnumerable<string> headers, bool force = false)
		{
			// Short-circuit loading the definitions if they are already loaded
			//  and we aren't flagged to force a reload
			if (csvDef.ColumnDefinitions != null && !force)
				return;

			// If we haven't loaded the types yet,
			//  or if we are flagged to force
			if (_colDefCache.ContainsKey(t) == false || force)
			{
				lock (_colDefCache)
				{
					if (_colDefCache.ContainsKey(t) == false)
					{
						var colDefs = new List<ColumnDefinition>();
						var props = t.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public)
							.Where(_ => _.GetCustomAttributes(typeof(ColDefAttribute), false).Count() > 0);

						foreach (var prop in props)
						{
							var attrib = (ColDefAttribute)prop.GetCustomAttributes(typeof(ColDefAttribute), false).FirstOrDefault();
							if (attrib != null)
							{
								colDefs.Add(new ColumnDefinition
								{
									Index = attrib.Index,
									Header = attrib.Header,

									Required = attrib.Required,
									TypeHint = attrib.TypeHint,
									Format = attrib.Format,
									Regex = String.IsNullOrEmpty(attrib.Regex) == false
										? new Regex(attrib.Regex, RegexOptions.Compiled)
										: null,
									EOFCondition = attrib.EOFCondition,

									Property = prop
								});
							}
						}

						// Populate all headers
						foreach (var colDef in colDefs.Where(_ => _.Index == null && _.Header == null))
							colDef.Header = colDef.Property.Name;

						// Set the indexes based on the provided headers
						if (headers != null)
						{
							var headerIndex = 0;
							foreach (var header in headers)
							{
								var colDef = colDefs.FirstOrDefault(_ => _.Index == null && _.Header == header);
								if (colDef != null)
									colDef.Index = headerIndex;
								headerIndex++;
							}
						}

						// Check to make sure that we have all indexes
						if (colDefs.Any(_ => _.Index == null))
							throw new InvalidOperationException("Unable to determine all column indexes");

						// Assign the column definitions to the cache
						_colDefCache[t] = colDefs;
					}
				}
			}

			// Set the column definitions
			csvDef.ColumnDefinitions = _colDefCache[t];
		}

		private static ColumnDefinition[] GetColDefArray(ICollection<ColumnDefinition> colDefs)
		{
			var index = 0;
			var colDefList = new List<ColumnDefinition>();
			foreach (var colDef in colDefs.OrderBy(_ => _.Index))
			{
				while (index < colDef.Index)
				{
					colDefList.Add(null);
					index++;
				}

				colDefList.Add(colDef);
				index++;
			}

			return colDefList.ToArray();
		}

		#endregion

		#region EOF Definition and Validation

		private static void GetEOFDef(CSVDefinition csvDef, bool force = false)
		{
			if (csvDef != null && csvDef.ColumnDefinitions != null)
			{
				if (csvDef.EOFDefinition == null || force)
				{
					if (csvDef.ColumnDefinitions.Any(_ => String.IsNullOrEmpty(_.EOFCondition) == false))
					{
						var eofDef = new List<string>();
						var colIndex = 0;

						foreach (var colDef in csvDef.ColumnDefinitions
							.Where(_ => String.IsNullOrEmpty(_.EOFCondition) == false)
							.OrderBy(_ => _.Index))
						{
							while (colIndex < colDef.Index)
							{
								eofDef.Add(null);
								colIndex++;
							}

							eofDef.Add(colDef.EOFCondition);
							colIndex++;
						}

						csvDef.EOFDefinition = eofDef;
					}
					else
					{
						csvDef.EOFDefinition = null;
					}
				}
			}
			else
			{
				throw new InvalidOperationException("Cannot build EOF definition without column definitions");
			}
		}

		private static bool VerifyEOF(ICollection<string> row, ICollection<string> eofDef)
		{
			if (row != null && eofDef != null)
			{
				// Convert to array for 1:1 checking on the columns
				var rowArray = row.ToArray();
				var eofDefArray = eofDef.ToArray();

				// Make sure we are working with the same length
				if (rowArray.Length == eofDefArray.Length)
				{
					// Iterate over each, checking for invalid values
					for (int index = 0; index < rowArray.Length; index++)
						if (eofDefArray[index] != null)
							if (rowArray[index] != eofDefArray[index])
								return EOFInvalid;
				}
				else
				{
					throw new InvalidOperationException("EOF definition is not the same length as the EOF row");
				}
			}

			// There is no data, or no EOF defined, or the EOF is valid
			return EOFValid;
		}

		#endregion

		#region Value Conversion and Validation

		private static void ValidateValue(int rowNumber, ColumnDefinition colDef, string value)
		{
			if (String.IsNullOrEmpty(value) == false)
			{
				if (colDef.Regex != null && !colDef.Regex.IsMatch(value))
					throw new CSVException(String.Format("Column [{0}] \"{1}\" value \"{2}\" fails Regex validation", colDef.Index, colDef.Header, value));
			}
			else if (colDef.Required)
			{
				throw new CSVException(String.Format("Column [{0}] \"{1}\" marked as required, and no value found", colDef.Index, colDef.Header)) { RowNumber = rowNumber, Column = colDef };
			}
		}

		private static object ConvertValue(string value, Type type, CSVTypeHint typeHint, string format)
		{
			if (String.IsNullOrEmpty(format) == false)
			{
				switch (typeHint)
				{
					case CSVTypeHint.String:
						return value;
					case CSVTypeHint.DateTime:
						DateTime dt;
						DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
						return dt;
					default:
						throw new NotImplementedException(String.Format("TypeHint for {0} not implemented", typeHint));
				}
			}

			return Convert.ChangeType(value, type);
		}

		#endregion

		#region Text Reader

		private static ICollection<string> ReadNextRow(TextReader textReader, CSVDefinition csvDef, out bool eof)
		{
			// status variables
			bool textCol = false;

			// used for building our column value
			StringBuilder sb = new StringBuilder();

			// The container for the row
			var nextRow = new List<string>();

			// iterate over the lines
			string line;
			bool lastCharText = false;
			string finalCol = null;
			while ((line = textReader.ReadLine()) != null)
			{
				// Text is open, this means we have a column with multi-line text,
				//  we need to append a newline to the value
				if (textCol)
					sb.Append(Environment.NewLine);

				foreach (char ch in line)
				{
					// Check for the text delimiter
					if (ch == csvDef.TextDelimiter)
					{
						// Flip our text mode
						textCol = !textCol;

						// look to see if this is an escaped text delimiter
						if (!lastCharText)
						{
							lastCharText = true;
						}
						else
						{
							sb.Append(ch);
							lastCharText = false;
						}
					}
					else
					{
						// This is not a text delimiter
						lastCharText = false;

						// Check to see if we are not in text, and if this is a column
						//  delimiter
						if (!textCol && ch == csvDef.ColumnDelimiter)
						{
							// This is a column delimiter, end the column and return
							//  what we have and move onto the next column
							nextRow.Add(sb.ToString());
							sb.Clear();
							continue;
						}

						// This is neither a text or column delimimer, add it in
						//  and continue on
						sb.Append(ch);
					}
				}

				// We have reached the end of the line,
				//  we are finished here
				if (!textCol)
				{
					finalCol = sb.ToString();
					break;
				}
			}

			// This will have the last column's data in it,
			//  or null, if something went wrong (like an empty row)
			if (finalCol != null)
				nextRow.Add(finalCol);
			var peek = textReader.Peek();
			eof = peek == -1 || peek == Environment.NewLine[0];
			return nextRow;
		}

		#endregion

		#region Exception Handling

		private static void HandleException(Exception up, Action<CSVException> exceptionHandler, bool faultTolerant)
		{
			var csvException = up is CSVException
				? (CSVException)up
				: new CSVException(up.Message, up);

			if (exceptionHandler != null)
				exceptionHandler(csvException);

			if (!faultTolerant)
				throw up;
		}

		#endregion

		#endregion

	}
}
