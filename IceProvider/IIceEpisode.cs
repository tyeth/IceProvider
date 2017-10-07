namespace IceProvider
{
    public interface IIceEpisode
    {
        string Name { get; set; }
        string FileName { get; set; }
        string Url { get; set; }
        bool Selected { get; set; }
        void Add(params string[] a);
        void Add(string name, string url, bool selected = false);
    }
}