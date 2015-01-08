// This file is part of CodeWatchdog.
//
// Copyright (c) 2014, 2015 Florian Berger <mail@florian-berger.de>
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// A coding convention compliance checker written in C#.
// </summary>
namespace CodeWatchdog
{
    /// <summary>
    /// The abstract logic for parsing code files and calling handlers for code fragments.
    /// A new Watchdog instance is expected to be created for parsing a project.
    /// </summary>
    public class Watchdog
    {
        // TODO: Check for and handle comments
        
        protected char STATEMENT_DELIMTER;
        protected char START_BLOCK_DELIMITER;
        protected char END_BLOCK_DELIMITER;
        protected List<char> STRING_DELIMITERS;
        protected char STRING_ESCAPE;
        
        public delegate void StringHandler(string input);
        
        /// <summary>
        /// Called when a statement is encountered.
        /// Add callbacks for statement handling here.
        /// </summary>
        protected StringHandler StatementHandler;
        
        /// <summary>
        /// Called when an error is to be output for human consideration.
        /// Add callbacks with implementations suiting your environment.
        /// </summary>
        public StringHandler Woff;
        
        /// <summary>
        /// Translate error codes to human readable complaints.
        /// This is meant to be filled by subclasses implementing specific parsers.
        /// </summary>
        protected Dictionary<int, string> ErrorCodeStrings;
        
        // Variables for processing a project
        //
        protected int CheckedLinesOfCode;
        protected Dictionary<int, int> ErrorCodeCount;
        
        /// <summary>
        /// Initialise this CodeWatchdog instance.
        /// Call this before parsing a new project.
        /// </summary>
        public virtual void Init()
        {
            CheckedLinesOfCode = 0;
            
            ErrorCodeCount = new Dictionary<int, int>();
            
            return;
        }

        /// <summary>
        /// Check a single source code file.
        /// </summary>
        /// <param name="filepath">The path to the file.</param>
        public void Check(string filepath)
        {
            StreamReader sr = new StreamReader(filepath, true);

            StringBuilder sb = new StringBuilder();

            bool stringRunning = false;

            Nullable<char> previousChar = null;

            int character = sr.Read();

            while (character != -1)
            {
                //Console.WriteLine(string.Format("Parsing char '{0}'", (char)character));

                if ((char)character == STATEMENT_DELIMTER && !stringRunning)
                {
                    Console.WriteLine(string.Format("Found statement: '{0}'", sb));

                    StatementHandler(sb.ToString());

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
                    if (!stringRunning)
                    {
                        Console.WriteLine(string.Format("Starting string with: '{0}'", (char)character));

                        stringRunning = true;
                    }
                    else if (previousChar != STRING_ESCAPE)
                    {
                        Console.WriteLine(string.Format("Ending string: with: '{0}'", (char)character));

                        stringRunning = false;
                    }

                    sb.Append((char)character);
                }
                else
                {
                    sb.Append((char)character);
                }

                if ((char)character == STRING_ESCAPE && previousChar == STRING_ESCAPE)
                {
                    previousChar = null;
                }
                else
                {
                    previousChar = (char)character;
                }

                character = sr.Read();
            }

            sr.Close();

            return;
        }
    }
}
