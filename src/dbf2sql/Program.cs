using CommandLine;
using dbf2sql.Options;
using DbfDataReader;
using DS.Foundation.Configuration;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace dbf2sql
{
    public class Program
    {

        private static readonly char[] newLineChars = { '\r', '\n' };

        /// <summary>
        /// el comando va a tener varios verbos
        /// 
        /// git status 
        /// git log 
        /// git push
        /// git commit
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Parser.Default.ParseArguments<SqlOptions, DataOptions>(args)
                .WithParsed<SqlOptions>(sql =>
                {
                    var engine = sql.Engine;
                    var connectionString = sql.ConnectionString;
                    var filePath = sql.FilePath;

                    /*if (sql.Engine == DbEngine.None)
                    {
                        Console.WriteLine("Debe especificar un motor SQL usando --engine.");
                        return;
                    }*/

                    if (sql.Execute && string.IsNullOrEmpty(connectionString))
                    {
                        Console.WriteLine("Debe especificar una cadena de conexión usando --connectionstring para ejecutar el CREATE TABLE.");
                        return;
                    }

                    // Generar el CREATE TABLE a partir del archivo .dbf
                    var options = new DbfFileOptions { Filename = filePath, SkipDeleted = false };
                    string querry = GetSchema(options);
                    
                    Console.WriteLine(querry);

                    if (sql.Execute)
                    {
                        try
                        {
                            using var connection = new SqlConnection(connectionString);
                            connection.Open();

                            using var command = new SqlCommand(querry, connection);
                            command.ExecuteNonQuery();

                            Console.WriteLine("Tabla creada exitosamente en el SQL Server.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al ejecutar el CREATE TABLE: {ex.Message}");
                        }
                    }
                })
                .WithParsed<DataOptions>(data =>
                {
                    var size = data.BatchSize;

                    /*
                        var options = new DbfDataReaderOptions
                        {
                        SkipDeletedRecords = true,
                        Encoding = DbfDataReader.EncodingProvider.GetEncoding(1252)
                        };

                        using (var dbfDataReader = new DbfDataReader.DbfDataReader(source.FullName, options))
                        {
                        while (dbfDataReader.Read())
                        {
                        var valueCol1 = dbfDataReader.GetInt32(0);
                        var valueCol2 = dbfDataReader.GetDecimal(1);
                        var valueCol3 = dbfDataReader.GetDateTime(2);
                        var valueCol4 = dbfDataReader.GetInt32(3);
                        }
                        }
                     */

                });

            //chain
            //chainning methods

        }


        private static void PrintSummaryInfo(DbfFileOptions options)
        {
            var encoding = GetEncoding();
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

        private static void PrintCsv(DbfFileOptions options)
        {
            var encoding = GetEncoding();
            using (var dbfTable = new DbfTable(options.Filename, encoding))
            {
                var columnNames = string.Join(",", dbfTable.Columns.Select(c => c.ColumnName));
                if (!options.SkipDeleted) columnNames += ",Deleted";

                Console.WriteLine(columnNames);

                var dbfRecord = new DbfRecord(dbfTable);

                while (dbfTable.Read(dbfRecord))
                {
                    if (options.SkipDeleted && dbfRecord.IsDeleted) continue;

                    var values = string.Join(",", dbfRecord.Values.Select(v => EscapeValue(v)));
                    if (!options.SkipDeleted) values += $",{dbfRecord.IsDeleted}";

                    Console.WriteLine(values);
                }
            }
        }

        private static string GetSchema(DbfFileOptions options)
        {
            var encoding = GetEncoding();
            string createTableSql;

            using (var dbfTable = new DbfTable(options.Filename, encoding))
            {
                var tableName = Path.GetFileNameWithoutExtension(options.Filename);
                var schemaBuilder = new StringBuilder();
                schemaBuilder.AppendLine($"CREATE TABLE [dbo].[{tableName}]");
                schemaBuilder.AppendLine("(");

                foreach (var dbfColumn in dbfTable.Columns)
                {
                    var columnSchema = ColumnSchema(dbfColumn);
                    schemaBuilder.AppendLine($"  {columnSchema},");

                    // NO SE QUE HACE ESTO PERO LLENA DE COMAS LA QUERRY
                    /*if (dbfColumn.ColumnOrdinal < dbfTable.Columns.Count ||
                        !options.SkipDeleted)
                        schemaBuilder.AppendLine(",");*/
                }

                if (!options.SkipDeleted) schemaBuilder.AppendLine("  [deleted] [bit] NULL DEFAULT ((0))");

                schemaBuilder.AppendLine(")");
                createTableSql = schemaBuilder.ToString();
            }

            return createTableSql;
        }

        private static Encoding GetEncoding()
        {
            return Encoding.GetEncoding(1252);
        }

        private static string ColumnSchema(DbfColumn dbfColumn)
        {
            var schema = string.Empty;
            switch (dbfColumn.ColumnType)
            {
                case DbfColumnType.Boolean:
                    schema = $"[{dbfColumn.ColumnName}] [bit] NULL DEFAULT ((0))";
                    break;
                case DbfColumnType.Character:
                    schema = $"[{dbfColumn.ColumnName}] [nvarchar]({dbfColumn.Length})  NULL";
                    break;
                case DbfColumnType.Currency:
                    schema =
                        $"[{dbfColumn.ColumnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) NULL DEFAULT (NULL)";
                    break;
                case DbfColumnType.Date:
                    schema = $"[{dbfColumn.ColumnName}] [date] NULL DEFAULT (NULL)";
                    break;
                case DbfColumnType.DateTime:
                    schema = $"[{dbfColumn.ColumnName}] [datetime] NULL DEFAULT (NULL)";
                    break;
                case DbfColumnType.Double:
                    schema =
                        $"[{dbfColumn.ColumnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) NULL DEFAULT (NULL)";
                    break;
                case DbfColumnType.Float:
                    schema =
                        $"[{dbfColumn.ColumnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) NULL DEFAULT (NULL)";
                    break;
                case DbfColumnType.General:
                    schema = $"[{dbfColumn.ColumnName}] [nvarchar]({dbfColumn.Length})  NULL";
                    break;
                case DbfColumnType.Memo:
                    schema = $"[{dbfColumn.ColumnName}] [ntext]  NULL";
                    break;
                case DbfColumnType.Number:
                    if (dbfColumn.DecimalCount > 0)
                        schema =
                            $"[{dbfColumn.ColumnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) NULL DEFAULT (NULL)";
                    else
                        schema = $"[{dbfColumn.ColumnName}] [int] NULL DEFAULT (NULL)";
                    break;
                case DbfColumnType.SignedLong:
                    schema = $"[{dbfColumn.ColumnName}] [int] NULL DEFAULT (NULL)";
                    break;
            }

            return schema;
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
