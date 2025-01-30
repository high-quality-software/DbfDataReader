using dbf2sql.Options;
using DbfDataReader;
using System.Text;

namespace dbf2sql.VerbRuns
{
    public class InfoVerbRun
    {
        public void Run(InfoOptions options) 
        {
            if (options.Summary)
            {
                PrintSummaryInfo(options);
            }
            else
            {
                Console.WriteLine("No action has been selected");
            }
        }

        private static void PrintSummaryInfo(InfoOptions options)
        {
            var encoding = Encoding.GetEncoding(1252);
            using (var dbfTable = new DbfTable(options.Filename, encoding))
            {
                var header = dbfTable.Header;

                Console.WriteLine($"Filename: {options.Filename}");
                Console.WriteLine($"Type: {header.VersionDescription}");
                Console.WriteLine($"Memo File: {dbfTable.Memo != null}");
                Console.WriteLine($"Records: {header.RecordCount}");
                Console.WriteLine();
                Console.WriteLine("Fields:");
                Console.WriteLine("Name             Type       Length     Decimal");
                Console.WriteLine("------------------------------------------------------------------------------");

                foreach (var dbfColumn in dbfTable.Columns)
                {
                    var name = dbfColumn.ColumnName;
                    var columnType = ((char)dbfColumn.ColumnType).ToString();
                    var length = dbfColumn.Length.ToString();
                    var decimalCount = dbfColumn.DecimalCount;
                    Console.WriteLine(
                        $"{name.PadRight(16)} {columnType.PadRight(10)} {length.PadRight(10)} {decimalCount}");
                }
            }
        }
    }
}
