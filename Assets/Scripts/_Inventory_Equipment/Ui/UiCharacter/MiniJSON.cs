// File: MiniJSON.cs
// Namespace optional: MiniJSON

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class MiniJSON
{
    public static object Deserialize(string json)
    {
        if (json == null) return null;
        return Parser.Parse(json);
    }

    sealed class Parser
    {
        const string WORD_BREAK = "{}[],:\"";

        enum TOKEN
        {
            NONE,
            CURLY_OPEN,
            CURLY_CLOSE,
            SQUARED_OPEN,
            SQUARED_CLOSE,
            COLON,
            COMMA,
            STRING,
            NUMBER,
            TRUE,
            FALSE,
            NULL
        }

        StringReader json;

        Parser(string jsonString)
        {
            json = new StringReader(jsonString);
        }

        public static object Parse(string jsonString)
        {
            var instance = new Parser(jsonString);
            return instance.ParseValue();
        }


        public void Dispose() => json.Dispose();

        Dictionary<string, object> ParseObject()
        {
            var table = new Dictionary<string, object>();
            json.Read(); // {
            while (true)
            {
                var token = NextToken;
                if (token == TOKEN.NONE) return null;
                if (token == TOKEN.CURLY_CLOSE) return table;

                var name = ParseString();
                if (name == null) return null;

                if (NextToken != TOKEN.COLON) return null;
                json.Read(); // :

                table[name] = ParseValue();
            }
        }

        List<object> ParseArray()
        {
            var array = new List<object>();
            json.Read(); // [
            var parsing = true;
            while (parsing)
            {
                var token = NextToken;
                if (token == TOKEN.NONE) return null;
                if (token == TOKEN.SQUARED_CLOSE) break;

                var value = ParseValue();
                array.Add(value);
            }
            return array;
        }

        object ParseValue()
        {
            switch (NextToken)
            {
                case TOKEN.STRING: return ParseString();
                case TOKEN.NUMBER: return ParseNumber();
                case TOKEN.CURLY_OPEN: return ParseObject();
                case TOKEN.SQUARED_OPEN: return ParseArray();
                case TOKEN.TRUE: return true;
                case TOKEN.FALSE: return false;
                case TOKEN.NULL: return null;
                default: return null;
            }
        }

        string ParseString()
        {
            var s = new StringBuilder();
            json.Read(); // "

            while (true)
            {
                if (json.Peek() == -1) break;
                var c = (char)json.Read();
                if (c == '"') break;
                s.Append(c);
            }

            return s.ToString();
        }

        object ParseNumber()
        {
            var number = NextWord;
            if (number.IndexOf('.') == -1)
                return int.TryParse(number, out var i) ? i : 0;
            return double.TryParse(number, out var d) ? d : 0.0;
        }

        string NextWord
        {
            get
            {
                var s = new StringBuilder();
                while (!IsWordBreak(json.Peek()))
                    s.Append((char)json.Read());
                return s.ToString();
            }
        }

        TOKEN NextToken
        {
            get
            {
                while (char.IsWhiteSpace((char)json.Peek()))
                    json.Read();

                switch (json.Peek())
                {
                    case -1: return TOKEN.NONE;
                    case '{': return TOKEN.CURLY_OPEN;
                    case '}': return TOKEN.CURLY_CLOSE;
                    case '[': return TOKEN.SQUARED_OPEN;
                    case ']': return TOKEN.SQUARED_CLOSE;
                    case ',': return TOKEN.COMMA;
                    case '"': return TOKEN.STRING;
                    case ':': return TOKEN.COLON;
                    default:
                        var word = NextWord;
                        switch (word)
                        {
                            case "true": return TOKEN.TRUE;
                            case "false": return TOKEN.FALSE;
                            case "null": return TOKEN.NULL;
                            default: return TOKEN.NUMBER;
                        }
                }
            }
        }

        static bool IsWordBreak(int c) => WORD_BREAK.IndexOf((char)c) != -1;
    }
}
