

namespace QMS.Models
{
    internal class ListItem
    {
        public string Display { get; }
        public string Value { get;  }

        public ListItem(string display, string value)
        {
            this.Display = display;
            this.Value = value;
        }
    }
}