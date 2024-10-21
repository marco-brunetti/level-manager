using Newtonsoft.Json;
using System.Collections.Generic;

namespace Layouts
{
    public record LayoutMap
    {
        [JsonProperty("layouts")] public List<LayoutData> Layouts;
    }

    public record LayoutData
    {
        public bool enable;
        public int zone;
        public List<LayoutItem> items;
        public LayoutType type;
        public List<LayoutType> nextTypes;
        public LayoutConfig config;
    }

    public record LayoutItem
    {
        public int id;
    }

    public record LayoutConfig
    {
        
    }
}