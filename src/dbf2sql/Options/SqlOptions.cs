using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbf2sql.Options
{
    [Verb("sql", HelpText = "permite crear sentencias sql")]
    public class SqlOptions
    {
        [Option("engine", Required = false, HelpText = "Motor sql a utilizar")]
        public string Engine { get; set; } = ""; //todo reemplazarlo por DS.Foundation.Data Engine
    }
}
