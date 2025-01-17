using dbf2sql.Options;
using DbfDataReader;
using Microsoft.Data.SqlClient;
using System.Text;

namespace dbf2sql.VerbRuns
{
    public class SqlVerbRun
    {
        public void Run(SqlOptions sql) 
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
            var options = new InfoOptions { Filename = filePath, SkipDeleted = true };
            string query = GetSchema(options);

            Console.WriteLine(query);

            if (sql.Execute)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine("CREATE TABLE ejecutado correctamente.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al ejecutar el CREATE TABLE: {ex.Message}");
                }
            }
        }

        private static string GetSchema(InfoOptions options)
        {
            var encoding = Encoding.GetEncoding(1252);
            string query;

            using (var dbfTable = new DbfTable(options.Filename, encoding))
            {
                var tableName = Path.GetFileNameWithoutExtension(options.Filename);
                var schemaBuilder = new StringBuilder();
                schemaBuilder.AppendLine($"CREATE TABLE [dbo].[{tableName}]");
                schemaBuilder.AppendLine("(");

                //ddl for columns
                var columns = new List<string>();
                var primaryKeys = new List<string>();

                foreach (var dbfColumn in dbfTable.Columns)
                {
                    var columnSchema = ColumnSchema(dbfColumn);
                    columns.Add(columnSchema);

                    if (dbfColumn.IsKey == true)
                    {
                        primaryKeys.Add($"[{dbfColumn.ColumnName.ToLowerInvariant()}]");
                    }
                }

                if (!options.SkipDeleted)
                {
                    columns.Add("[deleted] [bit] NULL DEFAULT ((0))");
                }
                schemaBuilder.AppendLine(string.Join(",\n", columns));
                //end ddl for columns

                if (primaryKeys.Any())
                {
                    schemaBuilder.AppendLine($",\nCONSTRAINT [PK_{tableName}] PRIMARY KEY ({string.Join(", ", primaryKeys)})");
                }

                schemaBuilder.AppendLine(")");
                query = schemaBuilder.ToString();
            }

            return query;
        }

        private static string ColumnSchema(DbfColumn dbfColumn)
        {
            var schema = string.Empty;
            var columnName = dbfColumn.ColumnName.ToLowerInvariant();
            //var nullable = (dbfColumn.AllowDBNull == true) ? "NULL" : "NOT NULL";
            var nullable = "NULL";

            switch (dbfColumn.ColumnType)
            {
                case DbfColumnType.Boolean:
                    schema = $"[{columnName}] [bit] {nullable}";
                    break;
                case DbfColumnType.Character:
                    schema = $"[{columnName}] [nvarchar]({dbfColumn.Length}) {nullable}";
                    break;
                case DbfColumnType.Currency:
                    schema =
                        $"[{columnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) {nullable}";
                    break;
                case DbfColumnType.Date:
                    schema = $"[{columnName}] [date] {nullable}";
                    break;
                case DbfColumnType.DateTime:
                    schema = $"[{columnName}] [datetime] {nullable}";
                    break;
                case DbfColumnType.Double:
                    schema =
                        $"[{columnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) {nullable}";
                    break;
                case DbfColumnType.Float:
                    schema =
                        $"[{columnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) {nullable}";
                    break;
                case DbfColumnType.General:
                    schema = $"[{columnName}] [nvarchar]({dbfColumn.Length})  {nullable}";
                    break;
                case DbfColumnType.Memo:
                    schema = $"[{columnName}] [ntext]  {nullable}";
                    break;
                case DbfColumnType.Number:
                    if (dbfColumn.DecimalCount > 0)
                        schema =
                            $"[{columnName}] [decimal]({dbfColumn.Length + dbfColumn.DecimalCount},{dbfColumn.DecimalCount}) {nullable}";
                    else
                        schema = $"[{columnName}] [int] {nullable}";
                    break;
                case DbfColumnType.SignedLong:
                    schema = $"[{columnName}] [int] {nullable}";
                    break;
            }

            return $"\t{schema}";
        }
    }
}
