using MainDabRedo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MainDabRedo
{
    public partial class Login : Window
    {
        private const string ClientId = "1415038205698510868";
        private const string ClientSecret = "yycgegyI1P5ZnGcafKd-Wro05zTXAIZs";
        private const string RedirectUri = "http://localhost:8000/callback";
        private static readonly string UserInfoFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "user", "user_info.json");

        public Login()
        {
            InitializeComponent();
            this.Loaded += Login_Loaded;
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nếu đã có thông tin đăng nhập thì tự động đăng nhập
            if (CheckExistingLogin())
            {
                AutoLogin();
            }
            else
            {

            }
        }

        private bool CheckExistingLogin()
        {
            try
            {
                if (File.Exists(UserInfoFilePath))
                {
                    string jsonString = File.ReadAllText(UserInfoFilePath);
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(jsonString);

                    // Kiểm tra thời gian đăng nhập (ví dụ: không quá 30 ngày)
                    if (userInfo != null && (DateTime.Now - userInfo.LoginTime).TotalDays <= 30)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking existing login: {ex.Message}");
            }
            return false;
        }

        private void AutoLogin()
        {
            // Mở cửa sổ chính
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Hide();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            HintText.Text = "🔄 Waiting for Discord login...";

            bool ok = await LoginWithDiscord();

            if (ok)
            {
                HintText.Text = "✅ Login successful";

                try
                {
                    var sb = (Storyboard)FindResource("MorphToCheck");
                    sb.Begin(this);
                }
                catch { }


                try
                {
                    var anim = new DoubleAnimation(0.0, 0.9, TimeSpan.FromMilliseconds(400));
                    IconGlow.BeginAnimation(DropShadowEffect.OpacityProperty, anim);
                }
                catch { }

                await Task.Delay(1200);
                OpenMainWindow();
            }
            else
            {
                HintText.Text = "❌ Login failed, please try again";
                try
                {
                    var sb = (Storyboard)FindResource("MorphToX");
                    sb.Begin(this);
                }
                catch { }
                LoginButton.IsEnabled = true;
            }
        }

        private async Task<bool> LoginWithDiscord()
        {
            try
            {
                string oauthUrl = $"https://discord.com/api/oauth2/authorize?client_id={ClientId}&redirect_uri={WebUtility.UrlEncode(RedirectUri)}&response_type=code&scope=identify";
                Process.Start(new ProcessStartInfo(oauthUrl) { UseShellExecute = true });

                string code = await StartHttpListener();
                if (string.IsNullOrEmpty(code)) return false;

                var user = await FetchUserInfo(code);
                if (user == null) return false;

                SaveUserInfo(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> StartHttpListener()
        {
            try
            {
                using (var listener = new HttpListener())
                {

                    var prefix = RedirectUri;
                    if (!prefix.EndsWith("/")) prefix += "/";
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    var context = await listener.GetContextAsync();
                    string code = context.Request.QueryString["code"];

                    byte[] buffer = Encoding.UTF8.GetBytes("<html><body style='font-family:Segoe UI;'><h2>Login success !!</h2><p>You can close this window and return to the app.</p></body></html>");
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close();
                    listener.Stop();

                    return code;
                }
            }
            catch
            {
                return null;
            }
        }

        private async Task<UserInfo> FetchUserInfo(string code)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string,string>("client_id", ClientId),
                        new KeyValuePair<string,string>("client_secret", ClientSecret),
                        new KeyValuePair<string,string>("grant_type","authorization_code"),
                        new KeyValuePair<string,string>("code",code),
                        new KeyValuePair<string,string>("redirect_uri",RedirectUri)
                    });

                    var tokenResp = await client.PostAsync("https://discord.com/api/oauth2/token", content);
                    if (!tokenResp.IsSuccessStatusCode) return null;

                    var tokenJson = await tokenResp.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(tokenJson))
                    {
                        var access = doc.RootElement.GetProperty("access_token").GetString();
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);

                        var userResp = await client.GetAsync("https://discord.com/api/users/@me");
                        if (!userResp.IsSuccessStatusCode) return null;

                        var userJson = await userResp.Content.ReadAsStringAsync();
                        using (var udoc = JsonDocument.Parse(userJson))
                        {
                            string id = udoc.RootElement.GetProperty("id").GetString();
                            string avatar = udoc.RootElement.TryGetProperty("avatar", out var a) ? a.GetString() : "";
                            return new UserInfo
                            {
                                Username = udoc.RootElement.GetProperty("username").GetString(),
                                UserId = id,
                                AvatarUrl = string.IsNullOrEmpty(avatar) ? "" : $"https://cdn.discordapp.com/avatars/{id}/{avatar}.png",
                                LoginTime = DateTime.Now
                            };
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private void SaveUserInfo(UserInfo u)
        {
            try
            {
                string json = JsonSerializer.Serialize(u, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UserInfoFilePath, json);
            }
            catch { }
        }

        private void OpenMainWindow()
        {
            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime LoginTime { get; set; }
    }
}