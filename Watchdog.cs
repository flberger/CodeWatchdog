using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace code_watchdog
{
    public class Watchdog
    {
        // TODO: Make const
        //
        char STATEMENT_DELIMTER = char.Parse(";");
        char START_BLOCK_DELIMITER = char.Parse("{");
        char END_BLOCK_DELIMITER = char.Parse("}");
        List<char> STRING_DELIMITERS = new List<char>() {char.Parse("\"")};

        public void Check(string filepath)
        {
            StreamReader sr = new StreamReader(filepath, true);

            StringBuilder sb = new StringBuilder();

            bool stringRunning = false;

            int character = sr.Read();

            while (character != -1)
            {
                if ((char)character == STATEMENT_DELIMTER && !stringRunning)
                {
                    Console.WriteLine(string.Format("Found statement: '{0}'", sb));

                    // TODO: Run statement checks

                    sb.Clear();
                }
                else if ((char)character == START_BLOCK_DELIMITER && !stringRunning)
                {
                    Console.WriteLine(string.Format("Found start block: '{0}'", sb));

                    // TODO: Run start block checks on buffer

                    // TODO: Set active block to block type (stack)

                    sb.Clear();
                }
                else if ((char)character == END_BLOCK_DELIMITER && !stringRunning)
                {
                    Console.WriteLine(string.Format("Ending block"));

                    // TODO: Run end block checks

                    // TODO: Pop active block from stack
                }
                else if (STRING_DELIMITERS.Contains((char)character))
                {
                    Console.WriteLine(string.Format("Found string delimiter: '{0}'", (char)character));

                    if (!stringRunning)
                    {
                        Console.WriteLine(string.Format("Starting string"));

                        stringRunning = true;
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Ending string: '{0}'", sb));

                        stringRunning = false;

                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append((char)character);
                }

                character = sr.Read();
            }

            sr.Close();

            return;
        }
    }
}
