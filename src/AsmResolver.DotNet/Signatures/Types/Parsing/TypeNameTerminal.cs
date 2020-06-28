namespace AsmResolver.DotNet.Signatures.Types.Parsing
{
    internal enum TypeNameTerminal
    {
        Eof,
        Identifier,
        Number,
        Star,
        Plus,
        Equals,
        Dot,
        DoubleDot,
        Comma,
        Ampersand, 
        OpenBracket,
        CloseBracket,
        Ellipsis,
    }
}