using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.FFMPEG;

namespace MyownconferenceDownloader
{
	class Program
	{
		private static string GetStreamUrl(string url)
		{
			var req = (HttpWebRequest)HttpWebRequest.Create(url);
			req.Method = WebRequestMethods.Http.Get;
			using (var resp = (HttpWebResponse)req.GetResponse())
			{
				if ((int)resp.StatusCode == 301)
					return resp.Headers["location"];
				if (resp.StatusCode == HttpStatusCode.OK)
					return resp.ResponseUri.ToString();
			}
			throw new Exception("Не получили адрес переадрисации");
		}
		private static string GetVideoFileListUrl(string url)
		{
			var req = (HttpWebRequest)HttpWebRequest.Create(url);
			req.Method = WebRequestMethods.Http.Get;
			using (var resp = (HttpWebResponse)req.GetResponse())
			{
				using (var stream = resp.GetResponseStream())
				{
					using (var reader = new StreamReader(stream))
					{
						var content = reader.ReadToEnd();
						var match = Regex.Match(content, @"'[^']*(index.m3u8)[^']*'");
						return match.Value.Replace("'", string.Empty);
					}
				}
			}
			throw new Exception("Не получили адрес списка видео");
		}

		static void Main(string[] args)
		{
			FFMpegOptions.Configure(new FFMpegOptions { RootDirectory = ".\\ffmpeg\\bin" });
			var url = args[0];
			Console.WriteLine($"Загрузка вебинара по ссылке: {url}");
			var streamUrl = GetStreamUrl(url);
			Console.WriteLine($"Получили ссылку на стрим: {streamUrl}");
			var videoFileListUrl = GetVideoFileListUrl(streamUrl);
			Console.WriteLine($"Получили ссылку на плейлист: {streamUrl}");
			var fileName = args[1];
			var encoder = new FFMpeg();
			Console.WriteLine($"Начало записи вебинара в файл: {fileName}");
            var fileInfo = new FileInfo(fileName);
            var videoInfo = encoder.SaveM3U8Stream(new Uri(videoFileListUrl), fileInfo);
			Console.WriteLine($"Вебинар сохранен, размер файла: {fileInfo.Length}, формат: {videoInfo.VideoFormat}");
		}
	}
}
