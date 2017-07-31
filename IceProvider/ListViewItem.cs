namespace IceProvider
{
     
    public class ListViewItem :  IIceEpisode
    {
        public string Name { get; set; }
        public string    Url { get; set; }
        public bool Selected { get; set; } = false;
        public bool Checked {get { return Selected; } set { Selected = value; }} 
        
        public void Add(params string[] a)
        {
            Name = a[0];
            Url = a[1];
            
        }

        public void Add(string name, string url,bool selected = false)
        {
            Url = url;
            Name = name;
            Checked = selected;
        }
    }
}