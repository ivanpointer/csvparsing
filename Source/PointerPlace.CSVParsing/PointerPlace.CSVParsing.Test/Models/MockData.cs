/*
 * (C)2015 Ivan Andrew Pointer (ivan@pointerplace.us)
 * Date: 02/16/2015
 * License: Apache License 2 (https://raw.githubusercontent.com/ivanpointer/csvparsing/master/LICENSE)
 * GitHub: https://github.com/ivanpointer/csvparsing
 */

using System;

namespace PointerPlace.CSVParsing.Test.Models
{
	/// <summary>
	/// A test class for parsing the mock CSV data
	/// </summary>
	public class MockData
	{
		[ColDef(Header = "id", Required = true)]
		public int ID { get; set; }
		[ColDef(Header = "first_name", Required = true)]
		public string FirstName { get; set; }
		[ColDef(Header = "last_name", Required = true)]
		public string LastName { get; set; }
		[ColDef(Header = "email", Regex = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,6})+)$")]
		public string Email { get; set; }
		[ColDef(Header = "country", Required = true)]
		public string Country { get; set; }
		[ColDef(Header = "ip_address")]
		public string IPAddress { get; set; }
		[ColDef(Header = "last_login", EOFCondition = "EOF", TypeHint = CSVTypeHint.DateTime, Format = "m/d/yyyy")]
		public DateTime LastLogin { get; set; }
	}
}
