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
        public ParsingParameters parsingParameters;
        
        public delegate void SingleStringHandler(string input);
        public delegate void InjectedSingleStringHandler(string input, Watchdog wd);
        public delegate void DoubleStringHandler(string firstInput, string secondInput);
        public delegate void InjectedDoubleStringHandler(string firstInput, string secondInput, Watchdog wd);
        
        #region Delegates
                        
        /// <summary>
        /// Called when a statement is encountered.
        /// Add callbacks for statement handling here.
        /// </summary>
        protected InjectedSingleStringHandler statementHandler;        
        
        /// <summary>
        /// Called when the beginning of a block is encountered.
        /// Add callbacks for start block handling here.
        /// </summary>
        protected InjectedSingleStringHandler startBlockHandler;
        
        /// <summary>
        /// Called when a comment is encountered, called with the comment
        /// and the current parse buffer which contains preceding text.
        /// Add callbacks for comment handling here.
        /// </summary>
        protected InjectedDoubleStringHandler commentHandler;
        
        /// <summary>
        /// Called when an error is to be output for human consideration.
        /// Add callbacks with implementations suiting your environment.
        /// </summary>
        public SingleStringHandler woff;
        
        #endregion
        
        /// <summary>
        /// Translate error codes to human readable complaints.
        /// This is meant to be filled by subclasses implementing specific parsers.
        /// </summary>
        public Dictionary<int, string> errorCodeStrings;
        
        #region Variables for processing a project

        protected int totalCheckedLines;
        protected int commentLines;
        public Dictionary<int, int> errorCodeCount;
        protected int checkedFiles;
        
        const double MaxCodeScore = 10.0;

        #endregion

        #region Class-accessible variables for each run

        public int checkedLinesThisFile;
        public string previousToken;

        StreamReader sr;
        StringBuilder sb;
        StringBuilder commentSb;
        
        Nullable<char> previousChar;
        
        bool stringRunning;
        
        // -1 = not scanning for further delimiters
        // 0..n = index in START_COMMENT_DELIMITER that has been found
        //
        int foundStartCommentDelimiters;
        int foundEndCommentDelimiters;
        
        bool commentRunning;

        Nullable<DateTime> startTime = null;
        
        #endregion
        
        /// <summary>
        /// Initialise this CodeWatchdog instance.
        /// Call this before parsing a new project.
        /// </summary>
        public virtual void Init()
        {
            totalCheckedLines = 0;
            
            commentLines = 0;
            
            errorCodeCount = new Dictionary<int, int>();
            
            checkedFiles = 0;
            
            return;
        }

        /// <summary>
        /// Check a single source code file.
        /// </summary>
        /// <param name="filepath">The path to the file.</param>
        public void Check(string filepath)
        {
            ResetVariables(filepath);
            
            // Let's go!
            //
            int character = sr.Read();

            while (character != -1)
            {
                Logging.Debug(string.Format("Parsing char '{0}'", (char)character));
                
                if ((char)character == '\n')
                {
                    checkedLinesThisFile += 1;
                    totalCheckedLines += 1;
                }
                
                CheckCommentDelimiters((char)character);

                if (!commentRunning
                    && !stringRunning
                    && (char)character == parsingParameters.statementDelimiter)
                {
                    HandleStatement();
                }
                else if (!commentRunning
                         && !stringRunning
                         && (char)character == parsingParameters.startBlockDelimiter)
                {
                    HandleStartBlock();
                }
                else if (!commentRunning
                         && !stringRunning
                         && (char)character == parsingParameters.endBlockDelimiter)
                {
                    HandleEndBlock();
                }
                else if (!commentRunning
                         && parsingParameters.stringDelimiters.Contains((char)character))
                {
                    HandleString((char)character);
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

                if ((char)character == parsingParameters.stringEscape
                    && previousChar == parsingParameters.stringEscape)
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
            
            checkedFiles += 1;

            return;
        }

        void ResetVariables(string filepath)
        {
            if (startTime == null)
            {
                startTime = DateTime.Now;
            }

            sr = new StreamReader (filepath, true);
            sb = new StringBuilder ();
            commentSb = new StringBuilder ();
            
            previousChar = null;
            
            stringRunning = false;
            
            foundStartCommentDelimiters = -1;
            foundEndCommentDelimiters = -1;
            
            commentRunning = false;
            
            checkedLinesThisFile = 0;
            
            previousToken = "";
            
            return;
        }

        void CheckCommentDelimiters(char character)
        {
            // Comments need special handling since there might be
            // multi-character comment delimiters.
            
            if (!commentRunning
                && !stringRunning
                && parsingParameters.startCommentDelimiter.Contains((character).ToString()))
            {
                // Gotcha. This needs further attention.
                
                HandlePossibleStartCommentDelimiter(character);
            }
            else if (foundStartCommentDelimiters > -1)
            {
                    Logging.Debug(string.Format("Character '{0}' is not the next possible start comment delimiter, stopping scan", (char)character));
                    
                    foundStartCommentDelimiters = -1;
            }
            
            if (commentRunning && parsingParameters.endCommentDelimiter.Contains((character).ToString()))
            {
                // Gotcha. This needs further attention.
                
                HandlePossibleEndCommentDelimiter(character);
            }
            else if (foundEndCommentDelimiters > -1)
            {
                    Logging.Debug(string.Format("Character '{0}' is not the next possible end comment delimiter, stopping scan", (char)character));
                    
                    foundEndCommentDelimiters = -1;
            }
        }

        void HandlePossibleStartCommentDelimiter(char character)
        {
            Logging.Debug (string.Format("Found possible start comment delimiter character: '{0}'", (char)character));
            
            if (foundStartCommentDelimiters == -1)
            {
                // Not scanning for further start comment characters
                
                if (character == char.Parse(parsingParameters.startCommentDelimiter.Substring(0, 1)))
                {
                    if (parsingParameters.startCommentDelimiter.Length == 1)
                    {
                        Logging.Debug (string.Format("Start comment delimiter '{0}' complete in: '{1}'", parsingParameters.startCommentDelimiter, sb.ToString() + ((char)character).ToString()));
                        
                        commentRunning = true;
                    }
                    else
                    {
                        Logging.Debug("Will scan for more start comment delimiter characters");
                        
                        foundStartCommentDelimiters = 0;
                    }
                }
                // No 'else' - not interested in this character
            }
            else
            {
                Logging.Debug("Already scanning for start comment characters");
                
                if (character == char.Parse(parsingParameters.startCommentDelimiter.Substring(foundStartCommentDelimiters + 1, 1)))
                {
                    Logging.Debug(string.Format("Next possible start comment delimiter character found: '{0}'", (char)character));
                    
                    // Compensate 0 index offset
                    //
                    if (foundStartCommentDelimiters + 2 == parsingParameters.startCommentDelimiter.Length)
                    {
                        Logging.Debug(string.Format ("Start comment delimiter '{0}' complete in: '{1}'", parsingParameters.startCommentDelimiter, sb.ToString() + ((char)character).ToString()));
                        
                        foundStartCommentDelimiters = -1;
                        
                        commentRunning = true;
                        
                        // Remove beginning delimiter from ordinary string.
                        // The final char is already omitted.
                        //
                        sb.Remove(sb.Length - (parsingParameters.startCommentDelimiter.Length - 1),
                                  parsingParameters.startCommentDelimiter.Length - 1);
                    }
                    else
                    {
                        Logging.Debug("Will scan for more start comment delimiter characters");
                        
                        foundStartCommentDelimiters += 1;
                    }
                }
                else
                {
                    Logging.Debug (string.Format("Character '{0}' is not the next possible start comment delimiter, stopping scan", (char)character));
                    
                    foundStartCommentDelimiters = -1;
                }
            }
            
            return;
        }

        void HandlePossibleEndCommentDelimiter(char character)
        {
            Logging.Debug (string.Format ("Found possible end comment delimiter character: '{0}'", (char)character));
            
            if (foundEndCommentDelimiters == -1)
            {
                // Not scanning for further end comment characters
                
                if (character == char.Parse (parsingParameters.endCommentDelimiter.Substring (0, 1)))
                {
                    if (parsingParameters.endCommentDelimiter.Length == 1)
                    {
                        string removedChar = "";
                        
                        if (parsingParameters.startCommentDelimiter.Length > 1)
                        {
                            removedChar = parsingParameters.startCommentDelimiter.Substring (0, 1);
                        }
                        
                        Logging.Info (string.Format ("Comment complete in: '{0}'", removedChar + commentSb.ToString() + ((char)character).ToString()));
                        
                        if (commentHandler != null)
                        {
                            commentHandler(removedChar + commentSb.ToString(), sb.ToString(), this);
                        }
                        
                        // NOTE: Set after handler call
                        //
                        previousToken = removedChar + commentSb.ToString();
                        
                        commentSb.Length = 0;
                        
                        commentLines += 1;
                        
                        commentRunning = false;
                    }
                    else
                    {
                        Logging.Debug ("Will scan for more end comment delimiter characters");
                        
                        foundEndCommentDelimiters = 0;
                    }
                }
                // No 'else' - not interested in this character
            }
            else
            {
                Logging.Debug ("Already scanning for end comment characters");
                
                if (character == char.Parse(parsingParameters.endCommentDelimiter.Substring(foundStartCommentDelimiters + 1, 1)))
                {
                    Logging.Debug(string.Format("Next possible end comment delimiter character found: '{0}'", (char)character));
                    
                    // Compensate 0 index offset
                    //
                    if (foundEndCommentDelimiters + 2 == parsingParameters.endCommentDelimiter.Length)
                    {
                        string removedChar = "";
                        
                        if (parsingParameters.startCommentDelimiter.Length > 1)
                        {
                            removedChar = parsingParameters.startCommentDelimiter.Substring(0, 1);
                        }
                        
                        Logging.Info (string.Format("Comment complete in: '{0}'", removedChar + commentSb.ToString() + ((char)character).ToString()));
                        
                        if (commentHandler != null)
                        {
                            commentHandler(removedChar + commentSb.ToString(), sb.ToString(), this);
                        }
                        
                        // NOTE: Set after handler call
                        //
                        previousToken = removedChar + commentSb.ToString();
                        
                        commentSb.Length = 0;
                        
                        commentLines += 1;
                        
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
                    Logging.Debug(string.Format ("Character '{0}' is not the next possible end comment delimiter, endping scan", (char)character));
                    
                    foundEndCommentDelimiters = -1;
                }
            }
            
            return;
        }

        void HandleStatement()
        {
            Logging.Info(string.Format("Found statement: '{0}'", ShowWhitespace(sb.ToString())));
            
            if (statementHandler != null)
            {
                statementHandler(sb.ToString(), this);
            }
            
            // NOTE: Set after handler call
            //
            previousToken = sb.ToString();
            
            Logging.Debug("Resetting StringBuilder");
            
            sb.Length = 0;
            
            return;
        }

        void HandleStartBlock()
        {
            Logging.Info(string.Format("Found start block: '{0}'", ShowWhitespace(sb.ToString())));
            
            if (startBlockHandler != null)
            {
                startBlockHandler(sb.ToString(), this);
            }
            
            // TODO: Set active block to block type (stack)
            // NOTE: Set after handler call
            //
            previousToken = sb.ToString();
            
            Logging.Debug("Resetting StringBuilder");
            
            sb.Length = 0;
            
            return;
        }

        void HandleEndBlock()
        {
            Logging.Info(string.Format("Ending block"));
            
            // TODO: Run end block checks
            
            // TODO: Pop active block from stack
            
            previousToken = "";
            
            Logging.Debug("Resetting StringBuilder");
            
            sb.Length = 0;
            
            return;
        }

        void HandleString(char character)
        {
            if (!stringRunning)
            {
                Logging.Debug(string.Format("Starting string with: '{0}'", character));
                
                stringRunning = true;
            }
            else if (previousChar != parsingParameters.stringEscape)
            {
                Logging.Debug(string.Format("Ending string: with: '{0}'", character));
                
                stringRunning = false;
            }
            
            sb.Append(character);
            
            return;
        }
        
        public void IncreaseCount(int errorCode)
        {
            if (errorCodeCount.ContainsKey(errorCode))
            {
                errorCodeCount[errorCode] += 1;
            }
            else
            {
                errorCodeCount[errorCode] = 1;
            }
            
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
                        
            summary.AppendLine(string.Format("Checked {0} file(s).", checkedFiles));
            
            summary.AppendLine(string.Format("Checked {0} line(s) of code.", totalCheckedLines));
            
            if (startTime != null)
            {
                TimeSpan duration = DateTime.Now - (DateTime)startTime;
                
                summary.AppendLine(string.Format("Processing time: {0:00}:{1:00}.{2}",
                                                 duration.Minutes,
                                                 duration.Seconds,
                                                 duration.Milliseconds));
            }
            
            summary.AppendLine(string.Format("Found {0} comment lines.", commentLines));
            
            int count = 0;
  
            summary.AppendLine("\n" + "------------------------------------------------------------------");
            summary.AppendLine(" Count    Error type");
            summary.AppendLine("------------------------------------------------------------------");
            
            // Sort errors by frequency
            // http://stackoverflow.com/a/292/1132250
            //
            List<KeyValuePair<int, int>> errorCodeCountSorted = new List<KeyValuePair<int, int>>(errorCodeCount);
            
            errorCodeCountSorted.Sort(delegate(KeyValuePair<int, int> firstPair,
                                               KeyValuePair<int, int> nextPair)
                                      {
                                          // Sort reversed
                                          //
                                          return nextPair.Value.CompareTo(firstPair.Value);
                                      });
            
            foreach (KeyValuePair<int, int> errorCodeCountPair in errorCodeCountSorted)
            {
                // Left-pad count for 4 characters.
                // Right-pad description for 56 characters.
                //
                summary.AppendLine(string.Format(" {0,5}    {1, -56}",
                                                 errorCodeCountPair.Value,
                                                 errorCodeStrings[errorCodeCountPair.Key]));
                
                count += errorCodeCountPair.Value;
            }
            
            summary.AppendLine("------------------------------------------------------------------\n");
            
            summary.AppendLine(string.Format("Found {0} error(s).", count));

            // Compute errors per lines of code. This yields a double [0.0, ~infinity],
            // but typically [0.0, 1.0]. 0.0 means no errors.
            //
            double score = (double)count / (double)totalCheckedLines;
            
            // Substract it from 1 to get a [0.0, 1.0] range. Now a value of 1 means
            // no errors.
            // In case of a lot of errors, the score might even be negative, which
            // serves as an additional alert.
            //
            score = 1 - score;
            
            // Now for the psychology part. :-)
            //
            // Apply a quadratic parabola to get quicker to lower values.
            //
            double quadraticScore = Math.Pow(score, 2);
            
            // Weigh MaxCodeScore by the result to get a number that is
            // meaningful to humans. MaxCodeScore means no errors.
            //
            quadraticScore = quadraticScore * MaxCodeScore;
            
            summary.AppendLine(string.Format("Your code is rated {0:0.##} / {1:0.##}.", quadraticScore, MaxCodeScore));
            
            // Alternative:
            //
            // Apply a exponential parabola to get quicker to lower values.
            // Values found by trying.
            //
//            double expScore = Math.Exp(6 * score - 6);
            
            // Weigh MaxCodeScore by the result to get a number that is
            // meaningful to humans. MaxCodeScore means no errors.
            //
//            expScore = expScore * MaxCodeScore;
            
//            summary.AppendLine(string.Format("Your code is rated {0:0.##} / {1:0.##} (exp scale).", expScore, MaxCodeScore));
            
            return summary.ToString();
        }
        
        /// <summary>
        /// Replace whitespace characters in input with printable characters,
        /// for debug display.
        /// </summary>
        string ShowWhitespace(string input)
        {
            input = input.Replace(" ", "~");
            input = input.Replace("\n", @"\n" + "\n");
            
            return input;
        }
    }
}
