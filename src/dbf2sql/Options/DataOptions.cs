using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbf2sql.Options
{
    [Verb("data", HelpText = "permite indicar el manejo de datos")]
    public class DataOptions
    {
        /*[Option("source", Required = false, HelpText = "Carpeta donde estan los archivos .dbf")]
        public string SourceFolder { get; set; } = "";

        [Option("batch-size", Required = false, HelpText = "cantidad de registros a copiar por vez")]
        public int BatchSize { get; set; } = 10;*/

        [Option("connectionstring", Required = false, HelpText = "Cadena de conexión a la base de datos")]
        public string ConnectionString { get; set; } = string.Empty;

        [Option("filepath", Required = true, HelpText = "Ruta del archivo .dbf")]
        public string FilePath { get; set; } = "";
    }
}
