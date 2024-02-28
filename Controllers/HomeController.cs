using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using youtube_converter_mp4.Models;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly YoutubeClient _youtubeClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _youtubeClient = new YoutubeClient();
        }




        [HttpPost]
        public async Task<IActionResult> GetVideoInfo(string videoUrl)
        {
            try
            {
                var videoId = ParseVideoId(videoUrl);
                var video = await _youtubeClient.Videos.GetAsync(videoId);

                var videoInfo = new VideoInfo
                {
                    Title = video.Title,
                    Duration = video.Duration ?? TimeSpan.Zero,
                    ThumbnailUrl = $"https://img.youtube.com/vi/{videoId}/default.jpg",
                    VideoUrl = $"https://www.youtube.com/watch?v={videoId}"
                };

                return View("Index", videoInfo);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao obter informações do vídeo: {ex.Message}");
                return View("Index");
            }
        }
        public static string ParseVideoId(string videoUrl)
        {

            if (string.IsNullOrEmpty(videoUrl))
            {
                throw new ArgumentException("O URL do vídeo não pode ser nulo ou vazio.", nameof(videoUrl));
            }


            var match = Regex.Match(videoUrl, @"(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new ArgumentException("O URL do vídeo do YouTube não é válido.", nameof(videoUrl));
            }
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DownloadVideo(string videoUrl)
        {
            try
            {
                var videoId = ParseVideoId(videoUrl);
                var video = await _youtubeClient.Videos.GetAsync(videoId);


                var streamInfoSet = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);


                var streamInfo = streamInfoSet.GetMuxedStreams().GetWithHighestVideoQuality();

                if (streamInfo != null)
                {

                    var stream = await _youtubeClient.Videos.Streams.GetAsync(streamInfo);


                    var fileName = $"{video.Title}.mp4";


                    return File(stream, "video/mp4", fileName);
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível encontrar um vídeo para download.");
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao fazer download do vídeo: {ex.Message}");
                return View("Index");
            }
        }


    }
}

