using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Covid19Lib.Parser.Base
{
    /// <summary>
    /// The BaseParser inherits from the TextFieldParser and only defines the text field type and the delimiter.
    /// </summary>
    public class BaseParser : TextFieldParser
    {
        public BaseParser(string filename) : base(filename)
        {
            TextFieldType = FieldType.Delimited;
            Delimiters = new string[] { "," };
        }
    }
}
