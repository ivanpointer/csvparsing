# CSV Parsing Utility
> PointerPlace.CSVParsing

This utility is used to parse CSV data from a string or from a text reader.  I wrote this because the Microsoft Jet OLEDB library is only available in x86 (32 bit) format and I wanted a quick way to read out CSV data.

## Jump Right In
This utility allows you to parse out CSV data using three methods:

1. **IEnumerable&lt;T&gt;** - You can create a POCO and decorate it with the `ColDefAttribute` to be populated by the CSVParser.  The utility will return an `IEnumerable` of your type, automatically doing simple type conversion and format parsing _(currently, only DateTime is included)_.  See the snippet below from the Test project included in the source code:

```cs
using (var textReader = File.OpenText(@"Dat\MOCK_DATA.csv"))
{
	var mockData = CSVParser.Parse<MockData>(textReader);
	Assert.IsNotNull(mockData);
	foreach (var entry in mockData)
		Console.WriteLine(String.Format("{0} | {1} {2}", entry.ID, entry.FirstName, entry.LastName));
}
```

This method enables you to also include validation information using the attributes, such as the boolean "Required" and the string "Regex" pattern allowing to define custom regex patterns.

2. **IEnumerable&lt;IEnumerable&lt;string&gt;&gt;** - A simple list of lists of strings, this is about as simple as the data gets.

3. **IEnumerable&lt;IDictionary&lt;string, string&gt;&gt;** - A list of dictionaries.  The dictionarys' keys are the column headers, where the values are the values of the columns.  This allows you to access the values by their column headers instead of position.

## ColDefAttribute
The `[ColDef]` attribute is used to decorate your classes so that they can be populated by the `CSVParser` utility.  Let's jump right in and take a look at the configurable properties of the `[ColDef]` attribute:

1. **Index:** The position within the CSV data where the column resides, zero based.  For example, if this attribute is decorating the property for the fourth column, this `Index` would be set to `3`.  This superceeds the `Header` property.  
> **Note:** If the CSV data being parsed does not contain headers, `Index` is required, as the `CSVParser` utility will not be able to discern which column goes where otherwise.

2. **Header:** The text header for the column, if this isn't provided, this will be set to the name of the property that the attribute is decorating, for example "FirstName" if it is decorating a `public string FirstName { get; set; }`.  
> **Note:** If neither `Index` nor `Header` are provided, the system will try to line up the columns using the name of the property that the attribute is decorating.

3. **Required:** A boolean indicating whether or not a value must be present in the column for the data to be valid. A value of `true` indicates that data is required in this column.
4. **Regex:** An optional string defining a regular expression to use to validate the data.  The regular expression is only checked if the record column has data.
5. **TypeHint:** Hints to the `CSVParser` utility what type of formatting is performed when parsing the value.  This is used in direct relation with the next property, `Format`.  The only type used right now is `DateTime`.  This defaults to `String`.
6. **Format:** An optional format string used in parsing the value of the column.  Currently, only `DateTime` has any special use of the `Format` setting.
7. **EOFCondition:** An optional setting which defines an EOF value for the CSV data.  If none of these are provided, no EOF checking is performed.  If even a single `EOFCondition` is provided, all columns which do not have an `EOFCondition` defined will not be checked.  This is useful for catching incomplete text files, for example, if being used in an automated process where files are transferred via FTP and a failure causes an incomplete file to be transferred

> **Tip:** Put a string format in a non-string column for the EOF indicater, so that the EOF condition will not occur in normal operations.
 
