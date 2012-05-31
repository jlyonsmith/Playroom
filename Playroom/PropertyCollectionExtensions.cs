using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public static class PropertyCollectionExtensions
    {
        public static string ReplaceVariables(this PropertyCollection properties, string s)
        {
            return s.ReplaceTags("$(", ")", properties.AsReadOnlyDictionary());
        }

        public static void AddWellKnownProperties(
            this PropertyCollection properties, 
            ParsedPath buildContentInstallDir,
            ParsedPath contentFileDir)
        {
            properties["BuildContentInstallDir"] = buildContentInstallDir.ToString();
            properties["InputRootDir"] = contentFileDir.ToString();
            properties["OutputRootDir"] = contentFileDir.ToString();
        }

        public static void AddFromTupleList(
            this PropertyCollection properties, List<Tuple<string, string>> tuples)
        {
            foreach (var tuple in tuples)
            {
                properties[tuple.Item1] = tuple.Item2;
            }
        }
    }
}
