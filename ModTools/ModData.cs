using System.Collections.Generic;

namespace ModTools
{
    public class ModData
    {
        public List<string> Orientations { get; set; } = new List<string>();
        public int Resolution { get; set; }
        public int TargetResolution { get; set; }
        public int SliceCount { get; set; }
        public string BaseDirectory { get; set; }
    }

}