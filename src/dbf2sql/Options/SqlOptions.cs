using CommandLine;
using DS.Foundation.Configuration;
using DS.Foundation.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbf2sql.Options
{
    [Verb("sql", HelpText = "permite crear sentencias sql")]
    public class SqlOptions
    {
        [Option("engine", Required = false, HelpText = "Motor sql a utilizar")]
        public string Engine { get; set; } = "";
        //public DbEngine Engine { get; set; } = DbEngine.SqlServer; //todo agregarle a DbEngine una opcion NONE
        
        [Option("connectionstring", Required = false, HelpText = "Cadena de conexión a la base de datos")]
        public string ConnectionString { get; set; } = string.Empty;

        [Option("execute", Required = false, Default = false, HelpText = "Indica si se debe ejecutar el CREATE TABLE")]
        public bool Execute { get; set; } = false;
        
        [Option("filepath", Required = true, HelpText = "Ruta del archivo .dbf")]
        public string FilePath { get; set; } = "";
    }
}
