using DiscordRPC;
using DiscordRPC.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NguyenNhatAPI
{
    public static class Api
    {
        public static class misc
        {
            public static string editorUri = "https://getcloudy.xyz/Editor";
            private static readonly HttpClient client = new HttpClient();
            private static DiscordRpcClient clients;
            private static DiscordRpcClient clientas;

            static misc()
            {
                Api.misc.client.DefaultRequestHeaders.Add("Authorization", "v1USERFREE");
            }

            private static string GetUserIdFromUsername(string username)
            {
                string address = "https://users.roblox.com/v1/usernames/users";
                string data = $"{{\"usernames\": [\"{username}\"]}}";
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                        JObject jobject = JObject.Parse(webClient.UploadString(address, "POST", data));
                        if (jobject["data"] != null && jobject["data"].HasValues)
                            return jobject["data"][(object)0][(object)"id"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show($"Lỗi khi lấy ID người dùng cho {username}: {ex.Message}", "NguyenNhatAPI");
                }
                return (string)null;
            }

            public static BitmapImage GetAvatar(string username)
            {
                string userIdFromUsername = Api.misc.GetUserIdFromUsername(username);
                if (string.IsNullOrEmpty(userIdFromUsername))
                    return (BitmapImage)null;
                string address = $"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={userIdFromUsername}&size=420x420&format=png";
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        JObject jobject = JObject.Parse(webClient.DownloadString(address));
                        if (jobject["data"] != null && jobject["data"].HasValues)
                            return new BitmapImage(new Uri(jobject["data"][(object)0][(object)"imageUrl"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show($"Lỗi khi tải ảnh đại diện cho {username}: {ex.Message}", "NguyenNhatAPI");
                }
                return (BitmapImage)null;
            }

            public static bool isRobloxOpen()
            {
                return ((IEnumerable<Process>)Process.GetProcessesByName("RobloxPlayerBeta")).Any<Process>();
            }

            public static void SetDiscordRpc(string title, string appid, string img, string state, List<(string label, string url)> buttons = null)
            {
                Api.misc.clients = new DiscordRpcClient(appid);
                Api.misc.clients.OnReady += (sender, e) => { };
                Api.misc.clients.Initialize();

                var presence = new RichPresence()
                {
                    Details = title,
                    State = state,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = img,
                        LargeImageText = title
                    }
                };

                if (buttons != null && buttons.Count > 0)
                {
                    presence.Buttons = buttons.Take(2).Select(b => new DiscordRPC.Button()
                    {
                        Label = b.label,
                        Url = b.url
                    }).ToArray();
                }

                Api.misc.clients.SetPresence(presence);
            }

            [Obsolete]
            public static async Task<string> GetDiscordUsername(string appId)
            {
                TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                Api.misc.clientas = new DiscordRpcClient(appId);
                Api.misc.clientas.OnReady += (OnReadyEvent)((sender, e) => tcs.SetResult($"{e.User.Username}#{e.User.Discriminator}"));
                Api.misc.clientas.Initialize();
                DiscordRpcClient clientas = Api.misc.clientas;
                RichPresence presence = new RichPresence();
                presence.Details = "Xác thực tên người dùng...";
                presence.State = "Vui lòng chờ";
                presence.Timestamps = Timestamps.Now;
                clientas.SetPresence(presence);
                string task = await tcs.Task;
                return task;
            }

            public static string GetUsername()
            {
                string path = $"C:\\\\Users\\\\{Environment.UserName}\\\\AppData\\\\Local\\\\Roblox\\\\LocalStorage\\\\appStorage.json";
                if (!System.IO.File.Exists(path))
                    return (string)null;
                try
                {
                    JObject jobject = JObject.Parse(System.IO.File.ReadAllText(path));
                    if (jobject.ContainsKey("Username"))
                        return jobject["Username"]?.ToString();
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show("Lỗi lấy tên người dùng" + ex.Message, "NguyenNhatAPI");
                }
                return (string)null;
            }

            public static void killRoblox()
            {
                foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Không thể tắt tiến trình: " + ex.Message);
                    }
                }
            }

            public static string cPlaceId()
            {
                try
                {
                    List<string> list = ((IEnumerable<string>)Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs"), "*.log")).OrderByDescending<string, DateTime>((Func<string, DateTime>)(f => new FileInfo(f).LastWriteTime)).ThenByDescending<string, long>((Func<string, long>)(f => new FileInfo(f).Length)).ToList<string>();
                    Regex regex1 = new Regex("placeIds=(\\d+)", RegexOptions.IgnoreCase);
                    Regex regex2 = new Regex("placeid:(\\d+)", RegexOptions.IgnoreCase);
                    foreach (string path in list.Take<string>(5))
                    {
                        using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)fileStream))
                            {
                                string str = (string)null;
                                string input;
                                while ((input = streamReader.ReadLine()) != null)
                                {
                                    Match match1 = regex1.Match(input);
                                    if (match1.Success)
                                        str = match1.Groups[1].Value;
                                    Match match2 = regex2.Match(input);
                                    if (match2.Success)
                                        str = match2.Groups[1].Value;
                                }
                                if (!string.IsNullOrEmpty(str))
                                    return str;
                            }
                        }
                    }
                    return (string)null;
                }
                catch (Exception ex)
                {
                    return "Lỗi: " + ex.Message;
                }
            }

            public static async Task<string> cPlaceName(string placeId)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        string url = "https://www.roblox.com/games/" + placeId;
                        HttpResponseMessage response = await httpClient.GetAsync(url);
                        if (!response.IsSuccessStatusCode)
                            return $"Không thể truy xuất dữ liệu: {response.StatusCode}";
                        string content = await response.Content.ReadAsStringAsync();
                        Match titleMatch = Regex.Match(content, "<title>(.+?) - Roblox</title>", RegexOptions.IgnoreCase);
                        if (!titleMatch.Success)
                            return "Không tìm thấy tên địa danh trong nội dung trang.";
                        string placeName = titleMatch.Groups[1].Value.Trim();
                        return placeName;
                    }
                }
                catch (Exception ex)
                {
                    return "Lỗi: " + ex.Message;
                }
            }

            public static async Task<string> cPlaceImage(string placeId)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        string url = $"https://thumbnails.roblox.com/v1/assets?assetIds={placeId}&format=Png&size=768x432";
                        HttpResponseMessage response = await httpClient.GetAsync(url);
                        if (!response.IsSuccessStatusCode)
                            return $"Không thể lấy hình ảnh: {response.StatusCode}";
                        string json = await response.Content.ReadAsStringAsync();
                        Match match = Regex.Match(json, "\"imageUrl\":\\s*\"(https:[^\"]+)\"");
                        return !match.Success ? "Không tìm thấy URL hình ảnh." : match.Groups[1].Value;
                    }
                }
                catch (Exception ex)
                {
                    return "Lỗi: " + ex.Message;
                }
            }
        }
    }
}
