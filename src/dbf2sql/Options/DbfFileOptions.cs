﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbf2sql.Options
{
    public class DbfFileOptions
    {
        [Option(Default = true, HelpText = "Print summary information")]
        public bool Summary { get; set; }

        [Option(Default = false, HelpText = "Print as CSV file")]
        public bool Csv { get; set; }

        [Option(Default = false, HelpText = "Print SQL Schema")]
        public bool Schema { get; set; }

        [Option(Default = true, HelpText = "Whether to skip deleted records")]
        public bool SkipDeleted { get; set; }

        [Option(Required = true, HelpText = "Path to the DBF file")]
        public string Filename { get; set; }
    }
}
