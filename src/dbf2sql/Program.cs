using CommandLine;
using dbf2sql.Options;
using dbf2sql.VerbRuns;
using DbfDataReader;
using DS.Foundation.Configuration;
using FluentMigrator.Builders.Alter.Column;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using System.Globalization;
using System.Text;

namespace dbf2sql
{
    public class Program
    {
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
                    var runner = new SqlVerbRun();
                    runner.Run(sql);
                })
                .WithParsed<DataOptions>(data =>
                {
                    var runner = new DataVerbRun();

                    if (data.Csv)
                    {
                        runner.ExportToCsv(data, data.OutputCsvPath);
                    }
                    else
                    { 
                        runner.Run(data);
                    }

                })
                .WithParsed<InfoOptions>(info =>
                {
                    var runner = new InfoVerbRun();
                    runner.Run(info);
                });

            //chain
            //chainning methods

        }
    }
}
