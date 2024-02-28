namespace youtube_converter_mp4.Models
{
    public class VideoInfo
    {
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; }
    }
}
