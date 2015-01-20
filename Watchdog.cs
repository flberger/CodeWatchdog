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
/// </summary>
namespace CodeWatchdog
{
    /// <summary>
    /// The abstract logic for parsing code files and calling handlers for code fragments.
    /// A new Watchdog instance is expected to be created for parsing a project.
    /// </summary>
    public class Watchdog
    {
        protected char STATEMENT_DELIMTER;
        protected char START_BLOCK_DELIMITER;
        protected char END_BLOCK_DELIMITER;
        protected List<char> STRING_DELIMITERS;
        protected char STRING_ESCAPE;
        protected string START_COMMENT_DELIMITER;
        protected string END_COMMENT_DELIMITER;
        
        public delegate void StringHandler(string input);

        #region Delegates
                        
        /// <summary>
        /// Called when a statement is encountered.
        /// Add callbacks for statement handling here.
        /// </summary>
        protected StringHandler StatementHandler;        
        
        /// <summary>
        /// Called when the beginning of a block is encountered.
        /// Add callbacks for start block handling here.
        /// </summary>
        protected StringHandler StartBlockHandler;
        
        /// <summary>
        /// Called when a comment is encountered.
        /// Add callbacks for comment handling here.
        /// </summary>
        protected StringHandler CommentHandler;
        
        /// <summary>
        /// Called when an error is to be output for human consideration.
        /// Add callbacks with implementations suiting your environment.
        /// </summary>
        public StringHandler Woff;
        
        #endregion
        
        /// <summary>
        /// Translate error codes to human readable complaints.
        /// This is meant to be filled by subclasses implementing specific parsers.
        /// </summary>
        protected Dictionary<int, string> ErrorCodeStrings;
        
        // Variables for processing a project
        //
        protected int CheckedLinesOfCode;
        protected int CommentLines;
        protected Dictionary<int, int> ErrorCodeCount;
        
        const double MaxCodeScore = 10.0;
        
        /// <summary>
        /// Initialise this CodeWatchdog instance.
        /// Call this before parsing a new project.
        /// </summary>
        public virtual void Init()
        {
            CheckedLinesOfCode = 0;
            
            CommentLines = 0;
            
            ErrorCodeCount = new Dictionary<int, int>();
            
            return;
        }

        /// <summary>
        /// Check a single source code file.
        /// </summary>
        /// <param name="filepath">The path to the file.</param>
        public void Check(string filepath)
        {
            // Local variables needed for this scan only
            //
            StreamReader sr = new StreamReader(filepath, true);

            StringBuilder sb = new StringBuilder();
            
            StringBuilder commentSb = new StringBuilder();

            Nullable<char> previousChar = null;
            
            // TODO: stringRunning, comments ... this calls for a state machine.

            bool stringRunning = false;
            
            // -1 = not scanning for further delimiters
            // 0..n = index in START_COMMENT_DELIMITER that has been found
            //
            int foundStartCommentDelimiters = -1;

            // See above.
            //
            int foundEndCommentDelimiters = -1;
            
            bool commentRunning = false;
            
            // Let's go!
            //
            int character = sr.Read();

            while (character != -1)
            {
                //Logging.Debug(string.Format("Parsing char '{0}'", (char)character));
                
                if ((char)character == '\n')
                {
                    CheckedLinesOfCode += 1;
                }
                
                // Comments need special handling since there might be
                // multi-character comment delimiters.

                if (! commentRunning && START_COMMENT_DELIMITER.Contains(((char)character).ToString()))
                {
                    // Gotcha. This needs further attention.
                    
                    Logging.Debug(string.Format("Found possible start comment delimiter character: '{0}'", (char)character));
                    
                    if (foundStartCommentDelimiters == -1)
                    {
                        // Not scanning for further start comment characters
                        
                        if ((char)character == char.Parse(START_COMMENT_DELIMITER.Substring(0, 1)))
                        
                            if (START_COMMENT_DELIMITER.Length == 1)
                            {
                                Logging.Debug(string.Format("Start comment delimiter '{0}' complete in: '{1}'",  START_COMMENT_DELIMITER, sb.ToString() + ((char)character).ToString()));
                            
                                commentRunning = true;
                            }
                            else
                            {
                                Logging.Debug("Will scan for more start comment delimiter characters");
                                
                                foundStartCommentDelimiters = 0;
                            }
                            
                        // No 'else' - not interested in this character
                    }    
                    else
                    {
                        Logging.Debug("Already scanning for start comment characters");
                        
                        if ((char)character == char.Parse(START_COMMENT_DELIMITER.Substring(foundStartCommentDelimiters + 1, 1)))
                        {
                            Logging.Debug(string.Format("Next possible start comment delimiter character found: '{0}'", (char)character));
                            
                            // Compensate 0 index offset
                            //
                            if (foundStartCommentDelimiters + 2 == START_COMMENT_DELIMITER.Length)
                            {
                                Logging.Debug(string.Format("Start comment delimiter '{0}' complete in: '{1}'",  START_COMMENT_DELIMITER, sb.ToString() + ((char)character).ToString()));
                                
                                foundStartCommentDelimiters = -1;
                                
                                commentRunning = true;
                                
                                // Remove beginning delimiter from ordinary string.
                                // The final char is already omitted.
                                // !!!
                                sb.Remove(sb.Length - (START_COMMENT_DELIMITER.Length - 1),
                                          START_COMMENT_DELIMITER.Length - 1);
                            }    
                            else
                            {
                                Logging.Debug("Will scan for more start comment delimiter characters");
                                
                                foundStartCommentDelimiters += 1;
                            }
                        }
                        else
                        {
                            Logging.Debug(string.Format("Character '{0}' is not the next possible start comment delimiter, stopping scan", (char)character));

                            foundStartCommentDelimiters = -1;
                        }
                    }
                }
                else if (foundStartCommentDelimiters > -1)
                {
                    Logging.Debug(string.Format("Character '{0}' is not the next possible start comment delimiter, stopping scan", (char)character));
                    
                    foundStartCommentDelimiters = -1;
                }

                if (commentRunning && END_COMMENT_DELIMITER.Contains(((char)character).ToString()))
                {
                    // Gotcha. This needs further attention.
                    
                    Logging.Debug(string.Format("Found possible end comment delimiter character: '{0}'", (char)character));
                    
                    if (foundEndCommentDelimiters == -1)
                    {
                        // Not scanning for further end comment characters
                        
                        if ((char)character == char.Parse(END_COMMENT_DELIMITER.Substring(0, 1)))
                            
                        if (END_COMMENT_DELIMITER.Length == 1)
                        {
                            string removedChar = "";
                            
                            if (START_COMMENT_DELIMITER.Length > 1)
                            {
                                removedChar = START_COMMENT_DELIMITER.Substring(0, 1);
                            }
                            
                            Logging.Info(string.Format("Comment complete in: '{0}'", removedChar + commentSb.ToString() + ((char)character).ToString()));

                            if (CommentHandler != null)
                            {
                                CommentHandler(removedChar + commentSb.ToString());
                            }
                            
                            commentSb.Clear();
                            
                            CommentLines += 1;
                            
                            commentRunning = false;
                        }
                        else
                        {
                            Logging.Debug("Will scan for more end comment delimiter characters");
                            
                            foundEndCommentDelimiters = 0;
                        }
                        
                        // No 'else' - not interested in this character
                    }    
                    else
                    {
                        Logging.Debug("Already scanning for end comment characters");
                        
                        if ((char)character == char.Parse(END_COMMENT_DELIMITER.Substring(foundStartCommentDelimiters + 1, 1)))
                        {
                            Logging.Debug(string.Format("Next possible end comment delimiter character found: '{0}'", (char)character));
                            
                            // Compensate 0 index offset
                            //
                            if (foundEndCommentDelimiters + 2 == END_COMMENT_DELIMITER.Length)
                            {
                                string removedChar = "";
                                
                                if (START_COMMENT_DELIMITER.Length > 1)
                                {
                                    removedChar = START_COMMENT_DELIMITER.Substring(0, 1);
                                }
                                
                                Logging.Info(string.Format("Comment complete in: '{0}'", removedChar + commentSb.ToString() + ((char)character).ToString()));
                                
                                if (CommentHandler != null)
                                {
                                    CommentHandler(removedChar + commentSb.ToString());
                                }
                                
                                commentSb.Clear();
                                
                                CommentLines += 1;
                                
                                commentRunning = false;
                                
                                foundEndCommentDelimiters = -1;
                            }    
                            else
                            {
                                Logging.Debug("Will scan for more end comment delimiter characters");
                                
                                foundEndCommentDelimiters += 1;
                            }
                        }
                        else
                        {
                            Logging.Debug(string.Format("Character '{0}' is not the next possible end comment delimiter, endping scan", (char)character));
                            
                            foundEndCommentDelimiters = -1;
                        }
                    }
                }
                else if (foundEndCommentDelimiters > -1)
                {
                    Logging.Debug(string.Format("Character '{0}' is not the next possible end comment delimiter, stopping scan", (char)character));
                    
                    foundEndCommentDelimiters = -1;
                }

                if (! commentRunning && !stringRunning && (char)character == STATEMENT_DELIMTER)
                {
                    Logging.Info(string.Format("Found statement: '{0}'", sb));

                    if (StatementHandler != null)
                    {
                        StatementHandler(sb.ToString());
                    }
 
                    sb.Clear();
                }
                else if (! commentRunning && !stringRunning && (char)character == START_BLOCK_DELIMITER)
                {
                    Logging.Info(string.Format("Found start block: '{0}'", sb));

                    if (StartBlockHandler != null)
                    {
                        StartBlockHandler(sb.ToString());
                    }

                    // TODO: Set active block to block type (stack)

                    sb.Clear();
                }
                else if (! commentRunning && !stringRunning && (char)character == END_BLOCK_DELIMITER)
                {
                    Logging.Info(string.Format("Ending block"));

                    // TODO: Run end block checks

                    // TODO: Pop active block from stack
                }
                else if (! commentRunning && STRING_DELIMITERS.Contains((char)character))
                {
                    if (!stringRunning)
                    {
                        Logging.Debug(string.Format("Starting string with: '{0}'", (char)character));

                        stringRunning = true;
                    }
                    else if (previousChar != STRING_ESCAPE)
                    {
                        Logging.Debug(string.Format("Ending string: with: '{0}'", (char)character));

                        stringRunning = false;
                    }

                    sb.Append((char)character);
                }
                else
                {
                    if (commentRunning)
                    {
                        commentSb.Append((char)character);
                    }
                    else
                    {
                        sb.Append((char)character);
                    }
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
        
        /// <summary>
        /// Return a human-readable summary of all Watchdog.Check() runs done so far.
        /// </summary>
        public string Summary()
        {
            // Using Markdown
            //
            StringBuilder summary = new StringBuilder("\nSUMMARY\n=======\n\n");
            
            summary.AppendLine(string.Format("Checked {0} line(s) of code.", CheckedLinesOfCode));
            
            // TODO: Add comment lines value to score formula
            //
            summary.AppendLine(string.Format("Found {0} comment lines.", CommentLines));
            
            int count = 0;
            
            foreach (int errorcount in ErrorCodeCount.Values)
            {
                count += errorcount;
            }
            
            summary.AppendLine(string.Format("Found {0} error(s).", count));

            // TODO: Add a nice table reporting the error types, sorted by frequency.

            // TODO: Use a fancy rating function, in which little errors quickly provide a bad score
            //
            double score = MaxCodeScore - (((double)count / (double)CheckedLinesOfCode) * MaxCodeScore);

            summary.AppendLine(string.Format("Your code is rated {0:0.##} / {1:0.##}.", score, MaxCodeScore));
            
            return summary.ToString();
        }
    }
}
