/*
 * (C)2015 Ivan Andrew Pointer (ivan@pointerplace.us)
 * Date: 02/16/2015
 * License: Apache License 2 (https://raw.githubusercontent.com/ivanpointer/csvparsing/master/LICENSE)
 * GitHub: https://github.com/ivanpointer/csvparsing
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PointerPlace.CSVParsing.Test.Models;
using System;
using System.IO;

namespace PointerPlace.CSVParsing.Test
{
	[TestClass]
	public class CSVParsingTests
	{
		[TestMethod]
		public void TestParsePOCO()
		{
			using (var textReader = File.OpenText(@"Dat\MOCK_DATA.csv"))
			{
				var mockData = CSVParser.Parse<MockData>(textReader);
				Assert.IsNotNull(mockData);
				foreach (var entry in mockData)
					if (entry != null)
						Console.WriteLine(String.Format("{0} | {1} {2}", entry.ID, entry.FirstName, entry.LastName));
					else
						Assert.Fail("There shouldn't be any empty records!");
			}
		}
	}
}
