using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using Microsoft.Win32;
using System.IO;

namespace Playroom
{
    public static class ToolPaths
    {
        public static ParsedPath InkscapeCom { get; set; }
        public static ParsedPath RSvgConvertExe { get; set; }

        static ToolPaths()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"svgfile\shell\Inkscape\command", false);

                if (key != null)
                {
                    string s = (string)key.GetValue("");

                    if (s != null && s.Length > 0)
                    {
                        if (s[0] == '"')
                            s = s.Substring(1, s.IndexOf('"', 1) - 1);

                        ParsedPath path  = new ParsedPath(s, PathType.File).SetExtension(".com");

                        if (File.Exists(path))
                            InkscapeCom = path;
                    }
                }

                key = Registry.ClassesRoot.OpenSubKey(@"svgfile\shell\Edit with GIMP\command", false);

                if (key != null)
                {
                    string s = (string)key.GetValue("");

                    if (s != null && s.Length > 0)
                    {
                        if (s[0] == '"')
                            s = s.Substring(1, s.IndexOf('"', 1) - 1);

                        ParsedPath path = new ParsedPath(s, PathType.File).SetFileAndExtension("rsvg-convert.exe");

                        if (File.Exists(path))
                            RSvgConvertExe = path;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
