﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Reusable.Shelly
{
    public class CommandLineTokenizer
    {
        public static IList<string> Tokenize(string text, char? nameValueSeparator = null)
        {
            var escapableChars = new HashSet<char> { '\\', '"' };
            var separators = new HashSet<char> { ' ' };
            var tokens = new List<string>();
            var token = new StringBuilder();
            var escapeMode = false;
            var quoted = false;

            if (nameValueSeparator.HasValue)
            {
                escapableChars.Add(nameValueSeparator.Value);
                separators.Add(nameValueSeparator.Value);
            }

            bool IsUnquotedSeparator(char c) => separators.Contains(c) && !quoted;

            foreach (var c in text ?? throw new ArgumentNullException(nameof(text)))
            {
                switch (c)
                {
                    case '\\' when !escapeMode:
                        escapeMode = true;
                        // Don't eat escape-char yet.
                        break;

                    case '"':
                        quoted = !quoted;
                        // Don't eat quotes.
                        break;

                    default:

                        switch (escapeMode)
                        {
                            case true:
                                switch (!escapableChars.Contains(c))
                                {
                                    case true:
                                        token.Append(escapeMode);
                                        break;
                                }
                                token.Append(c);
                                escapeMode = false;
                                // Escape-char already eaten.
                                break;

                            default:
                                switch (IsUnquotedSeparator(c))
                                {
                                    case true when token.Length > 0:
                                        tokens.Add(token.ToString());
                                        token.Clear();
                                        break;

                                    case true:
                                        // Don't eat separators.
                                        break;

                                    default:
                                        token.Append(c);
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }

            switch (token)
            {
                case StringBuilder b when b.Length > 0:
                    tokens.Add(token.ToString());
                    break;
            }

            return tokens;
        }
    }
}