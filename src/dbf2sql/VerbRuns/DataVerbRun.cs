using dbf2sql.Options;
using DbfDataReader;
using Microsoft.Data.SqlClient;
using System.Text;

namespace dbf2sql.VerbRuns
{
    public class DataVerbRun
    {
        private static readonly char[] newLineChars = { '\r', '\n' };

        public void Run(DataOptions data) 
        {
            //var size = data.BatchSize;
            var connectionString = data.ConnectionString;
            var filePath = data.FilePath;

            var options = new DbfDataReaderOptions
            {
                SkipDeletedRecords = true,
                Encoding = Encoding.GetEncoding(1252)
            };

            using (var dbfDataReader = new DbfDataReader.DbfDataReader(filePath, options))
            {
                using (var bulkCopy = new SqlBulkCopy(connectionString))
                {
                    bulkCopy.DestinationTableName = Path.GetFileNameWithoutExtension(filePath);

                    try
                    {
                        bulkCopy.WriteToServer(dbfDataReader);
                        Console.WriteLine("BulkCopy ejecutado correctamente");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing: dbf file: '{filePath}', exception: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Exporta los datos del archivo DBF a un archivo CSV.
        /// </summary>
        public void ExportToCsv(DataOptions data, string outputCsvPath)
        {
            var encoding = Encoding.GetEncoding(1252);
            var options = new DbfDataReaderOptions
            {
                SkipDeletedRecords = data.SkipDeleted,
                Encoding = encoding
            };

            try
            {
                using (var dbfTable = new DbfTable(data.FilePath, encoding))
                {
                    using (var writer = new StreamWriter(outputCsvPath, false, encoding))
                    {
                        var columnNames = string.Join(",", dbfTable.Columns.Select(c => c.ColumnName));
                        if (!data.SkipDeleted) columnNames += ",Deleted";
                        writer.WriteLine(columnNames);

                        // Leer y escribir los datos
                        var dbfRecord = new DbfRecord(dbfTable);
                        while (dbfTable.Read(dbfRecord))
                        {
                            if (data.SkipDeleted && dbfRecord.IsDeleted) continue;

                            var values = string.Join(",", dbfRecord.Values.Select(v => EscapeValue(v)));
                            if (!data.SkipDeleted) values += $",{dbfRecord.IsDeleted}";
                            writer.WriteLine(values);
                        }
                    }
                }

                Console.WriteLine($"Datos exportados correctamente al archivo CSV: {outputCsvPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear archivo csv: " +  ex.Message);
            }
        }

        private static string EscapeValue(IDbfValue dbfValue)
        {
            var value = dbfValue.ToString();
            if (dbfValue is DbfValueString)
            {
                if (value.Contains(",") || value.IndexOfAny(newLineChars) > -1)
                {
                    value = value.Replace("\"", "\"\"");
                    value = $"\"{value}\"";
                }
            }
            return value;
        }
    }
}
