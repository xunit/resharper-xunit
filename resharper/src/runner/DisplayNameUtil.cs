using System;
using System.Text;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public static class DisplayNameUtil
    {
        public static string Escape(string name)
        {
            var sb = new StringBuilder();
            foreach (var c in name)
                sb.Append(EscapeControlChar(c));

            return sb.ToString();
        }

        private static string EscapeControlChar(char c)
        {
            switch (c)
            {
                case '\0':
                    return "\\0";
                case '\a':
                    return "\\a";
                case '\b':
                    return "\\b";
                case '\f':
                    return "\\f";
                case '\n':
                    return "\\n";
                case '\r':
                    return "\\r";
                case '\t':
                    return "\\t";
                case '\v':
                    return "\\v";

                case '\x0085':
                case '\x2028':
                case '\x2029':
                    return string.Format("\\x{0:X4}", (ushort)c);

                default:
                    return char.IsControl(c)
                             ? String.Format("\\x{0:X4}", (ushort)c)
                             : c.ToString();
            }
        }
    }
}