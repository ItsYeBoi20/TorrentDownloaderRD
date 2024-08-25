namespace MediaDownloader
{
    public class TorrentInfo
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public string Date { get; set; }
        public string Size { get; set; }
        public string Description { get; set; }
        public long SizeInBytes { get; set; }
        public string Magnet { get; set; }
        public string Status { get; internal set; }
    }
}
