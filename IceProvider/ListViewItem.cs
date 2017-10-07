namespace IceProvider
{
    public class ListViewItem : IIceEpisode
    {
        public ListViewItem()
        {
        }

        public ListViewItem(params object[] o)
        {
            Add((string[]) o);
        }

        public bool Checked
        {
            get => Selected;
            set => Selected = value;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public bool Selected { get; set; }
        public string FileName { get; set; }
        public void Add(params string[] a)
        {
            Name = a[0];
            Url = a[1];
        }

        public void Add(string name, string url, bool selected = false)
        {
            Url = url;
            Name = name;
            Checked = selected;
        }
    }
}