using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsmResolver.DotNet.Signatures.Types.Parsing
{
    internal class TypeNameLexer
    {
        private static readonly ISet<char> RservedChars = new HashSet<char>("*+=.,&[]…");
        
        private readonly TextReader _reader;
        private readonly StringBuilder _buffer = new StringBuilder();
        private TypeNameToken? _bufferedToken;

        public TypeNameLexer(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }
        
        public TypeNameToken Peek()
        {
            _bufferedToken ??= ReadNextToken();
            return _bufferedToken.GetValueOrDefault();
        }

        public TypeNameToken Next()
        {
            if (_bufferedToken.HasValue)
            {
                var next = _bufferedToken.Value;
                _bufferedToken = null;
                return next;
            }

            return ReadNextToken() ?? throw new EndOfStreamException();
        }
        
        private TypeNameToken? ReadNextToken()
        {
            SkipWhitespaces();
            _buffer.Clear();
            
            int c = _reader.Peek();
            if (c == -1)
                return null;

            char currentChar = (char) c;
            return currentChar switch
            {
                '*' => ReadSymbolToken(TypeNameTerminal.Star),
                '+' => ReadSymbolToken(TypeNameTerminal.Plus),
                '=' => ReadSymbolToken(TypeNameTerminal.Equals),
                '.' => ReadDotToken(),
                ',' => ReadSymbolToken(TypeNameTerminal.Comma),
                '&' => ReadSymbolToken(TypeNameTerminal.Ampersand),
                '[' => ReadSymbolToken(TypeNameTerminal.OpenBracket),
                ']' => ReadSymbolToken(TypeNameTerminal.CloseBracket),
                '…' => ReadSymbolToken(TypeNameTerminal.Ellipsis),
                _ => char.IsDigit(currentChar) ? ReadNumberOrIdentifierToken() : ReadIdentifierToken()
            };
        }

        private TypeNameToken ReadDotToken()
        {
            // Consume first dot.
            _reader.Read();
            
            // See if there's a second one.
            if (_reader.Peek() == '.')
            {
                _reader.Read();
                return new TypeNameToken(TypeNameTerminal.DoubleDot, "..");
            }

            return new TypeNameToken(TypeNameTerminal.Dot, ".");
        }

        private TypeNameToken ReadNumberOrIdentifierToken()
        {
            TypeNameTerminal terminal = TypeNameTerminal.Number;
            while (true)
            {
                int c = _reader.Peek();
                if (c == -1)
                    break;
                
                char currentChar = (char) c;
                if (char.IsWhiteSpace(currentChar) || RservedChars.Contains(currentChar))
                    break;
                if (!char.IsDigit(currentChar))
                    terminal = TypeNameTerminal.Identifier;

                _reader.Read();
                _buffer.Append(currentChar);
            }
            
            return new TypeNameToken(terminal, _buffer.ToString());
        }

        private TypeNameToken ReadIdentifierToken()
        {           
            while (true)
            {
                int c = _reader.Peek();
                if (c == -1)
                    break;
                
                char currentChar = (char) c;
                if (char.IsWhiteSpace(currentChar) || RservedChars.Contains(currentChar))
                    break;

                _reader.Read();
                _buffer.Append(currentChar);
            }
            
            return new TypeNameToken(TypeNameTerminal.Identifier, _buffer.ToString());
        }

        private TypeNameToken ReadSymbolToken(TypeNameTerminal terminal)
        {
            string text = ((char) _reader.Read()).ToString();
            return new TypeNameToken(terminal, text);
        }

        private void SkipWhitespaces()
        {
            while (true)
            {
                int c = _reader.Peek();
                if (c == -1 || !char.IsWhiteSpace((char) c))
                    break;
                _reader.Read();
            }
        }
        
    }
}