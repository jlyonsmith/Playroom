using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class PrismData
    {
        public List<PrismPinboard> Pinboards { get; set; }
    }

    public class PrismPinboard
    {
        // Set by reader
        public ParsedPath FileName { get; set; }
        public List<PrismCompound> Compounds { get; set; }
        public IList<PrismMapping> Mappings { get; set; }

        // Used for processing
        public PinboardData Pinboard { get; set; }
    }

    public class PrismCompound
    {
        // Set by reader
        public string RectangleName { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public ParsedPath PngFileName { get; set; }

        // Used for processing
        public int LineNumber { get; set; }
        public int NextRow { get; set; }
        public int NextColumn { get; set; }
        public PrismMapping[,] Mappings { get; set; }
        public DateTime NewestSvgFileWriteTime { get; set; }
        public bool PngNeedsCompounding { get; set; }
    }

    public class PrismMapping
    {
        // Set by reader
        public string RectangleName { get; set; }
        public ParsedPath SvgFileName { get; set; }
        public ParsedPath PngFileName { get; set; }
        
        // Used for processing
        public RectangleInfo RectangleInfo { get; set; }
        public int LineNumber { get; set; }
        public PrismCompound Compound { get; set; }
    }
}
