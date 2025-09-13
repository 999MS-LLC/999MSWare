// References
using DiscordRPC;
using DiscordRPC.Logging;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MainDabRedo.Classes.Cosmetic;
using MainDabRedo.Execution;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NguyenNhatAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using VelocityAPI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Path = System.IO.Path;

namespace MainDabRedo
{

    public partial class MainWindow : Window
    {
        // VARIABLES //
        string CurrentVersion = "0.0.8"; // The version for this specific build

        // The default text editor text
        string DefaultTextEditorText = "--[[\r\nWelcome to QAMSWare\r\nMake sure to join QAMSWARE Discord at discord.gg/ZsHEZS8Zta\r\nIf you need help, join our Discord!\r\n--]]\r\n-- Paste in your text below this comment.\r\n\r\nlocal executor = identifyexecutor and identifyexecutor() or \"Unknown\"\r\n\r\nlocal message = \"\"\r\n\r\nif typeof(executor) == \"table\" then\r\n    message = \"Hello! You're using executor: \" .. tostring(executor[1])\r\nelse\r\n    message = \"Hello! You're using executor: \" .. tostring(executor)\r\nend\r\n\r\ngame.StarterGui:SetCore(\"SendNotification\", {\r\n    Title = \"Executor Detected\";\r\n    Text = message;\r\n    Duration = 5;\r\n})\r\n";

        // Variables relating to injection
        bool InjectionInProgress = false; // When injection is in progress, self explanatory
        bool OxygenInjected = false; // This function is here just so the status text shows whether Oxygen is injecting or not

        // Animation variables
        private bool CloseCompleted = false; // Window fade-in

        // Scripthub stuff
        bool IsScriptHubOpened = false;
        bool IsGameHubOpened = false;

        // Theme
        bool IsDefaultTheme = true; // So it won't apply on startup smh
        bool IsAvalonLoaded = false; // Prevent more errors from occuring

        // Variables for the theming, you'll see it later on
        string CurrentLuaXSHDLocation = ""; // The path for the AvalonEdit syntax highlighting. Will be needed for future theming

        string LeftRGB = ""; // Top left gradient
        string RightRGB = ""; // Bottom right gradient
        string BGImageURL = ""; // Background image URL
        string BGTransparency = ""; // Background image transparency, on a 0 to 1 scale

        // Variables for the AvalonEdit background colours, there's probably a much better way
        static int AvalonEditBGA = 6; // A (Transparency)
        static int AvalonEditBGR = 47; // R
        static int AvalonEditBGG = 47; // G
        static int AvalonEditBGB = 49; // B

        string AvalonEditFont = "Consolas"; // Font to be used for editor, this is for future customisation options

        bool IsInjected = false;
        private readonly Dictionary<string, int> notifications = new Dictionary<string, int>();

        // Variables for custom icons (to be implemented in the future)
        Array scripts = null;
        Array gamescripts = null;

        private const string ClientId = "1415038205698510868";
        private const string ClientSecret = "yycgegyI1P5ZnGcafKd-Wro05zTXAIZs";
        private const string RedirectUri = "http://localhost:8000/callback";
        private static readonly string UserInfoFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"bin","user","user_info.json");

        private Control currentPanel;

        private class UserInfo
        {
            public string Username { get; set; }
            public string UserId { get; set; }
            public string AvatarUrl { get; set; }
            public DateTime LoginTime { get; set; }
        }

        private readonly string[] descriptions = new string[]
    {
        "Gọi em là công chúa vì hoàng tử đang đứng chờ em nè!",
        "Chưa được sự cho phép mà đã tự ý thích em. Anh xin lỗi nhé công chúa!",
        "Em nhìn rất giống người họ hàng của anh. Đó chính là con dâu của mẹ anh!",
        "Trái Đất quay quanh Mặt Trời. Còn em thì quay mãi trong tâm trí anh!",
        "Vector chỉ có một chiều. Anh dân chuyên toán chỉ yêu một người.",
        "Anh béo thế này là bởi vì trong lòng anh có em nữa.",
        "Nghe đây! Em đã bị bắt vì tội quá xinh đẹp.",
        "Anh chỉ muốn bên cạnh em hai lần đó là bây giờ và mãi mãi.",
        "Bao nhiêu cân thính cho vừa? Bao nhiêu cân bả mới lừa được em?",
        "Vũ trụ của người ta là màu đen huyền bí. Còn vũ trụ của anh bé tí, thu nhỏ lại là em.",
        "Anh rất yêu thành phố này. Không phải vì nó có gì, mà vì nó có em.",
        "Anh bận với tất cả mọi điều. Nhưng vẫn luôn rảnh để nhớ đến em.",
        "Cành cây còn có lá. Chú cá vẫn đang bơi. Sao em cứ mải chơi. Chẳng chịu yêu anh thế!",
        "Em nhà ở đâu thế? Cứ tới lui trong tim anh không biết đường về nhà à?",
        "Cuộc đời anh vốn là một đường thẳng, chỉ vì gặp em mà rẽ ngang.",
        "Với thế giới em chỉ là một người. Nhưng với anh, em là cả thế giới.",
        "Em có thể đừng cười nữa được không? Da anh đen hết rồi.",
        "Anh đây chẳng thích nhiều lời. Nhìn em là biết cả đời của anh.",
        "Cảm lạnh có thể do gió, nhưng, cảm nắng thì chắc chắn do em.",
        "Trứng rán cần mỡ, bắp cần bơ. Yêu không cần cớ, cần em cơ!",
        "Cafe đắng thêm đường sẽ ngọt, còn cuộc đời anh thêm em sẽ hạnh phúc.",
        "Giữa cuộc đời hàng ngàn cám dỗ, nhưng, anh vẫn chỉ cần bến đỗ là em.",
        "Có người rủ anh đi ăn tối, nhưng anh từ chối vì thực đơn không có em.",
        "Em có biết vì sao đầu tuần lại bắt đầu bằng thứ hai không? Bởi vì em là thứ nhất!",
        "Oxy là nguồn sống của nhân loại. Còn em chính là nguồn sống của anh.",
        "Em bị cận thị à? Nếu không tại sao không nhìn thấy anh thích em chứ?",
        "Hôm qua anh gặp ác mộng vì trong giấc mộng đó không có em.",
        "Uống nhầm một ánh mắt, cơn say theo cả đời. Thương nhầm một nụ cười, cả một đời phiêu lãng.",
        "Dạo này em có thấy mỏi chân không? Sao cứ đi mãi trong đầu anh thế?",
        "Hình như em thích trà sữa lắm phải không? Anh cũng thích em như thế đấy.",
        "Nếu em là nước mắt thì anh sẽ không bao giờ khóc để lạc mất em đâu.",
        "Đôi mắt em còn xanh hơn cả Đại Tây Dương và anh thì bị lạc trên biển cả mất rồi.",
        "Nếu nụ hôn là những bông tuyết thì anh sẽ gửi đến em một cơn bão tuyết",
        "Phải chăng em là một ảo thuật gia? Bởi mỗi khi anh nhìn em là mọi thứ xung quanh đều biến mất.",
        "Anh có thể chụp ảnh em được không? Để chứng minh với lũ bạn rằng thiên thần là có thật.",
        "Anh có thể đi theo em được không? Bởi anh được bố mẹ dạy rằng phải theo đuổi giấc mơ của mình.",
        "Nếu khi anh nghĩ đến em mà có một ngôi sao biến mất. Vậy chắc cả bầu trời này không còn sao."
    };

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Random _rand = new Random();
        private readonly VelAPI Velocity = new VelAPI();
        private bool isNotifying;
        private Storyboard currentNotification;
        private Classes.Advertisement currentAdvertisement;

        // WebClient Creation
        WebClient WebStuff = new WebClient(); // Create a new generally used WebClient

        // Console handle - https://stackoverflow.com/questions/3571627/show-hide-the-console-window-of-a-c-sharp-console-application
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public MainWindow()
        {
            AllocConsole();

            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, SW_SHOW);
            }
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"\r\n▒█▀▀█ ░█▀▀█ ░ ▒█▀▄▀█ ▒█▀▀▀█ ▒█░░▒█ ░█▀▀█ ▒█▀▀█ ▒█▀▀▀ \r\n▒█░▒█ ▒█▄▄█ ▄ ▒█▒█▒█ ░▀▀▀▄▄ ▒█▒█▒█ ▒█▄▄█ ▒█▄▄▀ ▒█▀▀▀ \r\n░▀▀█▄ ▒█░▒█ █ ▒█░░▒█ ▒█▄▄▄█ ▒█▄▀▄█ ▒█░▒█ ▒█░▒█ ▒█▄▄▄\r\n- Welcome to QAMSWARE.\r\n- This is a console debug version, do not close it, if you close it, the interface will be closed.\r\n- Join discord: discord.gg/ZsHEZS8Zta | subscribe to yt channel: Dau Hu Yummy.\r\n- Made with <3 Nguyen Minh Nhat | API By Velocity (Custom)\r\n- Current QAMSWARE Version: {CurrentVersion}.\r\n===========================================================================================");
            InitializeComponent();
            MainWin.WindowStartupLocation = WindowStartupLocation.CenterScreen; // Center MainDab to the middle of the screen
            // UPDATE SYSTEM //

            // First, we want to check and see if the updater is still there
            if (File.Exists("QAMSWAREBootstrapper.exe"))
            {
                Console.WriteLine("[ℹ️] QAMSWARE Downloader found, deleting");
                File.Delete("QAMSWAREBootstrapper.exe"); // If it is, we should delete it
            }

            Console.WriteLine("[ℹ️] Checking to see if QAMSWARE is up to date");
            string Version = WebStuff.DownloadString("https://raw.githubusercontent.com/999MS-LLC/999MSWare/refs/heads/main/HeThong/check-update");
            WebStuff.Dispose(); // Remember to dispose the WebClient! Or someone will scold me for it
            Information("Checking for updates...", 3); // Show a notification that we are checking for updates

            // .FirstOrDefault() is nessesary since GitHub always adds an extra line for some reason
            // If I don't do this, then the string that would return is "MainDab 14.3/n" rather than "MainDab 14.3", so basically an additional unwanted line!
            string OnlineVersion = Version.Split(new[] { '\r', '\n' }).FirstOrDefault();

            if (CurrentVersion != OnlineVersion) // If the current version is not equal to the value online
            {
                // Downloading MainDab's Updater
                Console.WriteLine("[ℹ️] QAMSWARE not up to date, downloading new version");

                WebStuff.DownloadFile("https://github.com/999MS-LLC/999MSWare/releases/download/qaxiuoi/QAMSWAREBootstrapper.exe", "QAMSWAREBootstrapper.exe");
                WebStuff.Dispose();

                // Downloading MainDab's Updater

                // We have to set it like this since the updater needs the right startup path to run correctly
                Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
                Process.Start("QAMSWAREBootstrapper.exe"); // Run the updater
                Environment.Exit(0);
                // Note : The updater automatically deletes MainDab.exe
            }

            // SETUP //

            // This can be written in a shorter way, but I'll just leave it like this

            Console.WriteLine("[ℹ️] Checking to see if directories exist");

            if (!Directory.Exists("Applications")) // Tools
            {
                Console.WriteLine("[ℹ️] Created new directory: Applications");
                Directory.CreateDirectory("Applications");
            }
            if (!Directory.Exists("EditorThemes"))
            {
                Console.WriteLine("[ℹ️] Created new directory: EditorThemes");
                Directory.CreateDirectory("EditorThemes");
            }
            if (!Directory.Exists("Scripts"))
            {
                Console.WriteLine("[ℹ️] Created new directory: Scripts");
                Directory.CreateDirectory("Scripts");
            }
            if (!Directory.Exists("Themes"))
            {
                Console.WriteLine("[ℹ️] Created new directory: Themes");
                Directory.CreateDirectory("Themes");
            }
            if (!Directory.Exists("Workspace"))
            {
                Console.WriteLine("[ℹ️] Created new directory: Workspace");
                Directory.CreateDirectory("Workspace");
            }

            // check the WRD wrapper version first & update if req

            //if (!File.Exists("VelocityAPI.dll"))
            //{
            //    Console.WriteLine("Downloading VelocityAPI.dll, please wait...");
            //    WebStuff.DownloadFile("https://github.com/NguyenNhatIT/QAMSWARE/raw/refs/heads/main/resourceasset/API/VelocityAPI.dll", "VelocityAPI.dll");
            //}
                        
            // Theme checking for Avalon stuff, etc
            Console.WriteLine("[ℹ️] Updating Avalon theme definitions (text editor syntax highlighting)");
            if (File.Exists("EditorThemes\\lua_md_default.xshd"))
            {
                File.Delete("EditorThemes\\lua_md_default.xshd"); // We want to update default theme regardless lol
            }
            string penis = WebStuff.DownloadString("https://raw.githubusercontent.com/999MS-LLC/999MSWare/refs/heads/main/HeThong/resource/lua_md_default.xshd");
            File.WriteAllText("EditorThemes\\lua_md_default.xshd", penis);
            
            CurrentLuaXSHDLocation = "EditorThemes\\lua_md_default.xshd";
            IsAvalonLoaded = true;

            // Finally, load scripthub data
            this.Dispatcher.Invoke(async () => // Prevent error from this being done on "another thread"
            {
                Console.WriteLine("[ℹ️] Loading script hub data...");
                scripts = await ScriptHub.MainDabSC.GetSCData(); // Extract data from json file
                gamescripts = await ScriptHub.MainDabGSC.GetGSCData(); // Extract data from json file
            });

           
            Console.Title = "QAMSWARE | Console";
            this.Velocity.StartCommunication();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += UpdateBioText;
            _timer.Start();
            InitializeAds();
            LoadUserInfoFromFile();
            Console.WriteLine("[✅] Everything loaded successfully, ready to use.");
            PopupNotification($"Welcome back, QAMSWARE Loaded !", 7);
        }

        // Uodate Bio In 10s
        private async void UpdateBioText(object sender, EventArgs e)
        {
            int index = _rand.Next(descriptions.Length);
            string newText = descriptions[index];
            while (bio.Text.Length > 0)
            {
                bio.Text = bio.Text.Substring(0, bio.Text.Length - 1);
                await Task.Delay(20);
            }

            foreach (char c in newText)
            {
                bio.Text += c;
                await Task.Delay(50);
            }
        }

        // Make MainDab actually draggable
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Draggable top window
            DragMove();
        }

        // Theme settings
        private void ThemeLoading(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARETheme");
                if (SettingReg != null)
                {
                    string Thingy = SettingReg.GetValue("ISDEFAULT").ToString();
                    if (Thingy == "No")
                    {
                        string LeftRGBVal = SettingReg.GetValue("LEFTRGB").ToString();
                        string RightRGBVal = SettingReg.GetValue("RIGHTRGB").ToString();
                        string BGImageURLVal = SettingReg.GetValue("BGIMAGEURL").ToString();
                        string BGImageTransparencyVal = SettingReg.GetValue("BGTRANSPARENCY").ToString();
                        float transparencyval = float.Parse(BGImageTransparencyVal);
                        transparencyval = transparencyval / 100;

                        // Border
                        var conv = new ColorConverter();
                        LinearGradientBrush brushy = new LinearGradientBrush();
                        brushy.StartPoint = new Point(0, 0);
                        brushy.EndPoint = new Point(1, 1);
                        brushy.GradientStops.Add(new GradientStop((Color)conv.ConvertFrom(LeftRGBVal), 0.0));
                        brushy.GradientStops.Add(new GradientStop((Color)conv.ConvertFrom(RightRGBVal), 0.0));
                        WindowBorder.BorderBrush = brushy;

                        ImageBrush bb = new ImageBrush();
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(BGImageURLVal));
                        image.Opacity = transparencyval;
                        bb.ImageSource = image.Source;
                        bb.Opacity = transparencyval;
                        MainGrid.Background = bb;
                        MainGrid.Background.Opacity = transparencyval;

                        IsDefaultTheme = false;
                    }
                }
            }
            catch
            {
                // Prevent themes from breaking MainDab on startup
            }

        }

        // TAB CREATION FUNCTIONS //
        // Honestly these functions were actually pasted off other sources A LONG TIME AGO
        // http://avalonedit.net/documentation/ for any other references
        // Note that these are also "Sentinel" tabs

        public TextEditor CreateNewTab()
        {
            TextEditor textEditor = new TextEditor // Here, we make a new Avalon editor
            {
                // Setup some settings
                LineNumbersForeground = new SolidColorBrush(Color.FromRgb(199, 197, 197)),
                ShowLineNumbers = true,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Background = new SolidColorBrush(Color.FromArgb((byte)AvalonEditBGA, (byte)AvalonEditBGR, (byte)AvalonEditBGB, (byte)AvalonEditBGG)),
                FontFamily = new FontFamily(AvalonEditFont),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                WordWrap = true
            };

            // Set some settings regarding the editor
            textEditor.Options.EnableEmailHyperlinks = false;
            textEditor.Options.EnableHyperlinks = false; // The URL looks ugly
            textEditor.Options.AllowScrollBelowDocument = false;

            // Loop until Avalon has loaded
            while (IsAvalonLoaded == false)
            {
                Thread.Sleep(100);
            }

            // This one is for the theming for Avalon
            Stream xshd_stream = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xshd_reader = new XmlTextReader(xshd_stream);
            textEditor.SyntaxHighlighting = HighlightingLoader.Load(xshd_reader, HighlightingManager.Instance); // Now finally set it

            xshd_reader.Close();
            xshd_stream.Close();
            return textEditor;
        }

        // Get the text from the current texteditor
        // You can call this using CurrentTabWithStuff()
        // For example, to set the texteditor text, CurrentTabWithStuff().Text = "Text here";
        public TextEditor CurrentTabWithStuff() 
        {
            return this.TabControl.SelectedContent as TextEditor;
        }
        
        // Create a new tab
        public TabItem CreateTab(string text = "", string title = "Tab")
        {
            title = title + " " + TabControl.Items.Count.ToString(); // Counts the amount of tabs
            bool loaded = false; // Some weird bugs have been occuring without this here

            // Calls the function CreateNewTab, which is found above
            TextEditor textEditor = CreateNewTab();
            textEditor.Text = text;
            TabItem tab = new TabItem
            {
                Content = textEditor,
                Style = TryFindResource("Tab2") as Style, // Declared in the XAML stuff, https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.tryfindresource?view=net-5.0
                AllowDrop = true,
                Header = title // From the function

            };

            tab.Loaded += delegate (object source, RoutedEventArgs e) // Function for when the tab is loaded
            {
                if (loaded)
                {
                    return;
                }

                loaded = true; // Prevents some weird bug from occuring, though I doubt this is needed
            };

            // This is the function for the "X" icon, basically close tab button
            tab.MouseDown += delegate (object sender, MouseButtonEventArgs farded)
            {
                if (farded.OriginalSource is Border) // First of all actually check it
                {
                    if (farded.MiddleButton == MouseButtonState.Pressed) // MiddleButton = Left click here
                    {
                        this.TabControl.Items.Remove(tab); // Remove the tab
                        return;
                    }
                }
            };

            // A second loaded function, in order to actually make the magic work
            tab.Loaded += delegate (object seggs, RoutedEventArgs daddy)
            {
                tab.GetTemplateItem<System.Windows.Controls.Button>("CloseButton").Click += delegate (object sjdfaskdfasklf, RoutedEventArgs efdsn)
                {
                    this.TabControl.Items.Remove(tab);
                };

                loaded = true;
            };

            // Now finally set the title
            string oldHeader = title;
            this.TabControl.SelectedIndex = this.TabControl.Items.Add(tab);

            // This is the text that we start off with
            CurrentTabWithStuff().Text = DefaultTextEditorText;
            Console.WriteLine($"[✅] Created new tab: {title}");
            Success("Created new tab: " + title, 3); // Show a notification that the tab was created
            return tab;
        }

        // Now when the TabControl is loaded
        private void TextEditorLoad(object sender, RoutedEventArgs e)
        {
            // Loop until Avalon has loaded
            while (IsAvalonLoaded == false)
            {
                Thread.Sleep(100);
            }

            // Now, let's load up the theme
            Stream input = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xmlTextReader = new XmlTextReader(input);
            TextEditor.SyntaxHighlighting = HighlightingLoader.Load(xmlTextReader, HighlightingManager.Instance);
                        
            // Now actually set it
            Stream nya = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xml = new XmlTextReader(nya);
            TextEditor.SyntaxHighlighting = HighlightingLoader.Load(xml, HighlightingManager.Instance); 

            CurrentTabWithStuff().Text = DefaultTextEditorText; // Scroll all the way up to the top of this source code to set it

            // The template is defined in the xaml code
            this.TabControl.GetTemplateItem<System.Windows.Controls.Button>("AddTabButton").Click += delegate (object s, RoutedEventArgs f)
            {
                this.CreateTab("", "Tab");
            };

            // More theming
            foreach (TabItem tab in TabControl.Items)
            {
                tab.GetTemplateItem<System.Windows.Controls.Button>("CloseButton").Width = 0;
            }
        }

        // This is similar to the function above, but it's actually for the texteditor that first spawns in
        private void TextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // Set some settings regarding the editor
            TextEditor.Options.EnableEmailHyperlinks = false;
            TextEditor.Options.EnableHyperlinks = false; // The URL looks ugly
            TextEditor.Options.AllowScrollBelowDocument = false;

            // Set some stuff first
            MainWin.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            // Loop until Avalon has loaded
            while (IsAvalonLoaded == false)
            {
                Thread.Sleep(100);
            }

            // Load file
            Stream input = File.OpenRead(CurrentLuaXSHDLocation);
            XmlTextReader xmlTextReader = new XmlTextReader(input);
            CurrentTabWithStuff().SyntaxHighlighting = HighlightingLoader.Load(xmlTextReader, HighlightingManager.Instance);
        }

        // ADDITIONAL FUNCTIONS //
        // These are where additional functions go

        private void LoadUserInfoFromFile()
        {
            if (File.Exists(UserInfoFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(UserInfoFilePath);
                    // Sử dụng Newtonsoft.Json thay vì System.Text.Json
                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(jsonString);

                    if (userInfo != null)
                    {
                        // Update UI controls
                        if (usernameText != null)
                            usernameText.Text = userInfo.Username;

                        if (iddiscordText != null)
                            iddiscordText.Text = userInfo.UserId;

                        // Load avatar
                        if (!string.IsNullOrEmpty(userInfo.AvatarUrl) && avatarImage != null)
                        {
                            LoadAvatarImage(userInfo.AvatarUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write($"[❌] Error loading user info: {ex.Message}");
                }
            }
        }

        private async void LoadAvatarImage(string imageUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    byte[] imageData = await client.GetByteArrayAsync(imageUrl);

                    using (var stream = new MemoryStream(imageData))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        avatarImage.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write($"[❌] Error loading avatar: {ex.Message}");
                SetDefaultAvatarImage(); // Gọi phương thức đã được đổi tên
            }
        }

        // Phương thức set avatar mặc định
        private void SetDefaultAvatarImage()
        {
            try
            {
                // Tạo avatar mặc định đơn giản
                var ellipse = new Ellipse
                {
                    Width = 100,
                    Height = 100,
                    Fill = Brushes.Gray
                };

                ellipse.Measure(new Size(100, 100));
                ellipse.Arrange(new Rect(0, 0, 100, 100));

                var renderTarget = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(ellipse);

                avatarImage.Source = renderTarget;
            }
            catch (Exception ex)
            {
                Console.Write($"[❌] Error setting default avatar: {ex.Message}");
            }
        }

        // Ads System //
        private void ShowAdPane()
        {
            if (AdBorder.Visibility == Visibility.Collapsed)
            {
                MainWin.Margin = new Thickness(4.0, 4.0, 4.0, 124.0);
                base.Height += 120.0;
                AdBorder.Visibility = Visibility.Visible;
            }
            base.MinHeight = 570.0;
        }

        private void HideAdPane()
        {
            base.MinHeight = 450.0;
            if (AdBorder.Visibility == Visibility.Visible)
            {
                AdBorder.Visibility = Visibility.Collapsed;
                MainWin.Margin = new Thickness(4.0, 4.0, 4.0, 4.0);
                base.Height -= 120.0;
            }
        }

        private async Task LoadAdAsync()
        {
            await Classes.Advertisements.LoadAsync();

            currentAdvertisement = Classes.Advertisements.GetRandomAd();
            if (currentAdvertisement == null) return;

            ImageBrush brush = new ImageBrush();

            try
            {
                if (currentAdvertisement.imageUrl.StartsWith("http"))
                {
                    brush.ImageSource = new BitmapImage(new Uri(currentAdvertisement.imageUrl));
                    Console.WriteLine($"[ℹ️] Loaded advertisement from ID: {currentAdvertisement.uuid}");
                }
                else
                {
                    brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/QAMSWARELauncher;component" + currentAdvertisement.imageUrl, UriKind.Absolute));
                }

                brush.Stretch = Stretch.UniformToFill;
                AdBorder.Background = brush;
                ShowAdPane();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị quảng cáo: " + ex.Message);
            }
        }

        private void AdBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(currentAdvertisement.redirectLink);
        }

        private void InitializeAds()
        {
            _ = LoadAdAsync();

            System.Timers.Timer timer = new System.Timers.Timer(30000);
            timer.Elapsed += (sender, e) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _ = LoadAdAsync();
                });
            };
            timer.AutoReset = true;
            timer.Start();
        }

        // Run Discord's RPC, this will reflect in setting options later
        public void DiscordRPC()
        {
            try
            {
                DiscordRpcClient client;
                client = new DiscordRpcClient("1396328932403576905") // The ID of the client
                {
                    Logger = new ConsoleLogger
                    {
                        Level = LogLevel.Warning
                    }
                };

                client.OnReady += delegate { };
                client.OnPresenceUpdate += delegate { };

                if (!client.Initialize()) // Kiểm tra khởi tạo
                {
                    Console.WriteLine("[❌] Không thể khởi động Discord RPC (Discord chưa mở hoặc chưa cài).");
                    return; // Thoát hàm, không crash
                }

                client.SetPresence(new RichPresence()
                {
                    Details = "Using " + CurrentVersion,
                    State = "Keyless Roblox Exploit Free For Everyone...",

                    Timestamps = new Timestamps
                    {
                        Start = DateTime.UtcNow,
                    },

                    Assets = new Assets
                    {
                        LargeImageKey = "QAMSWARE",
                        LargeImageText = "QamsWare Roblox Exploit",
                        SmallImageKey = "nguyen-nhat"
                    },

                    Buttons = new DiscordRPC.Button[]
                    {
                new DiscordRPC.Button() { Label = "Join Discord", Url = "https://discord.gg/ZsHEZS8Zta" },
                new DiscordRPC.Button() { Label = "Get QAMSWARE Now", Url = "https://999msllc.pages.dev" }
                    },
                });

                Console.WriteLine("[✅] Discord RPC started successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[❌] Discord RPC bị lỗi: " + ex.Message);
            }
        }

        // ANIMATIONS //
        // All of these animations are just basic fade in/out anims
        // Oh and these are found in the XAML code

        // Sidebar animations
        // Home icon animation
        private void HomePage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("HomeOpen") as Storyboard;
            sb.Begin();
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Visible;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        // Execution icon animation
        private void ExecutorPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ExecutionOpen") as Storyboard;
            sb.Begin();
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Visible;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        // Scripthub page icon
        private void ScriptHubPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ScriptHubOpen") as Storyboard;
            sb.Begin();
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Visible;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        // Utilities page icon
        private void UtilitiesPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ToolsOpen") as Storyboard;
            sb.Begin();
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Visible;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        // Settings page icon
        private void SettingsPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("SettingsOpen") as Storyboard;
            sb.Begin();
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Visible;
        }

        // Settings animations

        // API Selection button
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("APISelection1") as Storyboard;
            sb.Begin();
            APISelection.Visibility = Visibility.Visible;
            GeneralOptions.Visibility = Visibility.Hidden;
            ThemeSelection.Visibility = Visibility.Hidden;
        }

        // General options button
        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("GeneralOptions1") as Storyboard;
            sb.Begin();
            APISelection.Visibility = Visibility.Hidden;
            GeneralOptions.Visibility = Visibility.Visible;
            ThemeSelection.Visibility = Visibility.Hidden;
        }

        // Theme selection button
        private void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("ThemeSelection1") as Storyboard;
            sb.Begin();
            APISelection.Visibility = Visibility.Hidden;
            GeneralOptions.Visibility = Visibility.Hidden;
            ThemeSelection.Visibility = Visibility.Visible;
        }

        // TOP BAR FUNCTIONS //

        private void DoNotification()
        {
            isNotifying = true;
            KeyValuePair<string, int> keyValuePair = notifications.First();
            notifications.Remove(keyValuePair.Key);
            NotificationContent.Text = keyValuePair.Key;
            DurationIndicator.Width = 0.0;
            currentNotification = Animation.Animate(new AnimationPropertyBase(NotificationBorder)
            {
                Property = FrameworkElement.WidthProperty,
                To = 280
            }, new AnimationPropertyBase(DurationIndicator)
            {
                Property = FrameworkElement.WidthProperty,
                To = 278,
                Duration = new System.Windows.Duration(TimeSpan.FromMilliseconds(keyValuePair.Value)),
                DisableEasing = true
            });
            currentNotification.Completed += delegate
            {
                CloseNotification();
            };
        }

        private void CloseNotification()
        {
            Animation.Animate(new AnimationPropertyBase(NotificationBorder)
            {
                Property = FrameworkElement.WidthProperty,
                To = 0
            }, new AnimationPropertyBase(DurationIndicator)
            {
                Property = FrameworkElement.WidthProperty,
                To = 0
            }).Completed += async delegate
            {
                if (notifications.Count > 0)
                {
                    await Task.Delay(250);
                    DoNotification();
                }
                else
                {
                    isNotifying = false;
                }
            };
        }

        private void PopupNotification(string message, int duration = 2500)
        {
            notifications[message] = duration; // vừa thêm mới vừa cập nhật nếu đã tồn tại

            if (!isNotifying)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(delegate
                {
                    DoNotification();
                });
            }
        }


        private void CloseNotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentNotification != null && currentNotification.GetCurrentState() == ClockState.Active)
            {
                currentNotification.Stop();
                CloseNotification();
            }
            else
            {
                // fallback: chỉ cần đóng notification mà không quan tâm animation
                CloseNotification();
            }
        }

        private void Success(string text, int time)
        {
            var notification = new MainDabRedo.controls.Notifications
            {
                BackgroundColor = Colors.White,
                ProgressColor = Colors.SeaGreen,
                BProgressColor = Colors.LightGreen,
                NotificationImage = new BitmapImage(new Uri("pack://application:,,,/images/succeded.png")),
                NotificationText = text,
                Time = time
            };

            NotificationsContainer.Children.Add(notification);
            notification.StartNotification();
        }

        private void Error(string text, int time)
        {
            var notification = new MainDabRedo.controls.Notifications
            {
                BackgroundColor = Colors.White,
                ProgressColor = Colors.DarkRed,
                BProgressColor = Colors.LightPink,
                NotificationImage = new BitmapImage(new Uri("pack://application:,,,/images/errored.png")),
                NotificationText = text,
                Time = time
            };

            NotificationsContainer.Children.Add(notification);
            notification.StartNotification();
        }

        private void Warning(string text, int time)
        {
            var notification = new MainDabRedo.controls.Notifications
            {
                BackgroundColor = Colors.White,
                ProgressColor = Colors.Goldenrod,
                BProgressColor = Colors.LightGoldenrodYellow,
                NotificationImage = new BitmapImage(new Uri("pack://application:,,,/images/warned.png")),
                NotificationText = text,
                Time = time
            };

            NotificationsContainer.Children.Add(notification);
            notification.StartNotification();
        }

        private void Information(string text, int time)
        {
            var notification = new MainDabRedo.controls.Notifications
            {
                BackgroundColor = Colors.White,
                ProgressColor = Colors.DodgerBlue,
                BProgressColor = Colors.LightBlue,
                NotificationImage = new BitmapImage(new Uri("pack://application:,,,/images/informed.png")),
                NotificationText = text,
                Time = time
            };

            NotificationsContainer.Children.Add(notification);
            notification.StartNotification();
        }

        // Minimise MainDab
        private void Mini(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Exit functions
        // Now, since we want to play a fade out animation, we will need something extra
        private void ExitMD(object sender, MouseButtonEventArgs e)
        {
            this.Close(); // There will be a function that will run when the window closes, MainWin_Closing
        }
        // Closing function
        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseCompleted)
            {
                FormFadeOut.Begin(); // Start the animation storyboard FormFadeOut
                e.Cancel = true;
            }
        }
        // Now when it's actually done, kill off MainDab process
        private void CloseWindow(object sender, EventArgs e)
        {
            CloseCompleted = true;
            // just in case
            try { foreach (Process proc in Process.GetProcessesByName("erto3e4rortoergn")) { proc.Kill(); } } catch { }
            try { foreach (Process proc in Process.GetProcessesByName("Decompiler")) { proc.Kill(); } } catch { }
            Environment.Exit(0);
        }

        // Draggable top window
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Draggable top window
            DragMove();
        }

        // HOME GRID FUNCTIONS //

        // Join Discord button
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            byte[] succ = WebStuff.DownloadData("https://raw.githubusercontent.com/NguyenNhatIT/QAMSWARE/refs/heads/main/resourceasset/discordlink");
            WebStuff.Dispose();
            string discord = Encoding.UTF8.GetString(succ);
            Process.Start(discord);
        }

        // Get help button
        private void Help(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://discord.gg/ZsHEZS8Zta"); // help website
        }

        // Set notice board text
        private void Notice(object sender, RoutedEventArgs e)
        {
            string noticeText = WebStuff.DownloadString("https://raw.githubusercontent.com/999MS-LLC/999MSWare/refs/heads/main/HeThong/thong-bao");
            NoticeBoard.Text = noticeText;
            Console.WriteLine("[✅] Notice board text set successfully.");
        }
        // Set changelog board text
        private void ChangelogBoard(object sender, RoutedEventArgs e)
        {
            string changelogText = WebStuff.DownloadString("https://raw.githubusercontent.com/999MS-LLC/999MSWare/refs/heads/main/HeThong/cap-nhat-co-gi-moi");
            Changelog.Text = changelogText;
            Console.WriteLine("[✅] Changelog board text set successfully.");
        }


        // EXECUTION GRID FUNCTIONS //

        // Execute script icon
        private void Execute(object sender, MouseButtonEventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
            if (processes.Length == 0)
            {
                Console.WriteLine("[ℹ️] Roblox is not running. Please start Roblox before executing scripts.");
                PopupNotification("Roblox is not running. Please start Roblox before executing scripts.", 3000);
                return;
            }

            int procId = processes[0].Id;

            if (this.Velocity.IsAttached(procId))
            {
                Velocity.Execute(this.CurrentTabWithStuff().Text);
            }
            else
            {
                Console.WriteLine("[❌] Velocity is not yet attached to Roblox. Please inject it before executing this command.");
                PopupNotification("Velocity is not yet attached to Roblox. Please inject it before executing this command.", 3000);
            }
        }

        private async void Inject(object sender, MouseButtonEventArgs e)
        {
            var robloxProc = Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault();
            if (robloxProc == null)
            {
                Console.WriteLine("[ℹ️] Roblox is not enabled. Please open Roblox!");
                PopupNotification("Roblox is not enabled. Please open Roblox!", 3000);
                return;
            }

            int procId = robloxProc.Id;

            if (Velocity.IsAttached(procId))
            {
                OnAttachSuccess(procId);
                return;
            }

            try
            {
                InjectionInProgress = true;

                int result = (int)await Velocity.Attach(procId);
                if (result == 1 || result == 0)
                {
                    Console.WriteLine("[✅] Successful injection!");
                    OnAttachSuccess(procId);
                }
                else
                {
                    Console.WriteLine($"[❌] Injection failed. Error code: {result}");
                    PopupNotification($"Injection failed. Error code: {result}", 3000);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Injection error. Send error image to support!");
                Console.WriteLine($"[❌] Error injecting into Roblox: {ex}");
            }
            finally
            {
                InjectionInProgress = false;
            }
        }

        private void OnAttachSuccess(int pid)
        {
            Console.WriteLine($"[✅] Successfully injected into Roblox (PID: {pid})");
            StartRobloxMonitor(pid);
        }

        private DispatcherTimer robloxMonitorTimer;

        private void StartRobloxMonitor(int procId)
        {
            if (robloxMonitorTimer != null)
                return;

            robloxMonitorTimer = new DispatcherTimer();
            robloxMonitorTimer.Interval = TimeSpan.FromSeconds(1);

            robloxMonitorTimer.Tick += (s, e) =>
            {
                var robloxProcesses = Process.GetProcessesByName("RobloxPlayerBeta");

                if (robloxProcesses.Length == 0 || !this.Velocity.IsAttached(procId))
                {
                    robloxMonitorTimer.Stop();
                    robloxMonitorTimer = null;
                    Console.WriteLine("[❌] Roblox Is Down Or Has An Injection Error, Please Try Again!");
                    PopupNotification("Roblox is down or has an injection error, please try again!", 3000);
                }
            };

            robloxMonitorTimer.Start();
        }

        // Clear icon
        private void Clear(object sender, MouseButtonEventArgs e)
        {
            CurrentTabWithStuff().Text = ""; // Clear the currently selected textbox
            Console.WriteLine("[✅] Cleared the current text editor.");
            PopupNotification("Cleared the current text editor.", 2000);
        }

        // Kill Roblox icon (so end process)
        private void KillRoblox(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to kill Roblox?", "QAMSWARE", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                // Just a basic YesNo thing
            }
            else
            {
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("RobloxPlayerBeta")) // We will loop though each process just to find it
                    // Not sure if there's a more efficient way but this does the job quickly
                    {
                        proc.Kill();
                        MessageBox.Show("Roblox process killed", "QAMSWARE");
                        PopupNotification("Roblox process killed", 2000);
                    }

                }
                catch
                {
                    // Just in case the user is stupid or something
                    MessageBox.Show("Roblox process has already been killed, or Roblox isn't running.", "QAMSWARE");
                    PopupNotification("Roblox process has already been killed, or Roblox isn't running.", 2000);
                }
            }
        }

        // Open file function itself
        public static OpenFileDialog openfiledialog = new OpenFileDialog // New OpenFileDialog thing
        {
            Filter = "Text Files and Lua Files (*.txt *.lua)|*.txt;*.lua|All files (*.*)|*.*", // Either any of those will work
            FilterIndex = 1,
            RestoreDirectory = true,
            Title = "Open File"
        };

        // Open file icon
        private void OpenFile(object sender, MouseButtonEventArgs e)
        {
           OpenFileDialog FL = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Text Files and Lua Files (*.txt *.lua)|*.txt;*.lua|All files (*.*)|*.*" // Same filter
            };

            if (FL.ShowDialog() == true)
            {
                CurrentTabWithStuff().Text = File.ReadAllText(FL.FileName);
                Console.WriteLine($"[✅] Opened file: {FL.FileName}");
                PopupNotification($"Opened file: {FL.FileName}", 2000);
            }
        }
        
        // Save file icon
        private void SaveFile(object sender, MouseButtonEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Save File",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Filter = "Text Files and Lua Files(*.txt *.lua) | *.txt; *.lua | All files(*.*) | *.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveDialog.ShowDialog() == true) File.WriteAllText(saveDialog.FileName, CurrentTabWithStuff().Text);
            Console.WriteLine($"[✅] Saved file: {saveDialog.FileName}");
            PopupNotification($"Saved file: {saveDialog.FileName}", 2000);
        }

        // Sets the selected API
        private void SetAPI(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWAREData");
            if (key != null)
            {
                string apishouldbe = (key.GetValue("DLL").ToString());
                CurrentAPILabel.Content = apishouldbe;
                Execution.SelectedAPI.API = apishouldbe;
            }
            else
            {
                CurrentAPILabel.Content = "Using Velocity API";
                Execution.SelectedAPI.API = "Selected API: Velocity API";
            }
        }

        private void StatusCheck(Object o)
        {
            Process[] pname = Process.GetProcessesByName("RobloxPlayerBeta");
            if (pname.Length > 0) // Nếu Roblox đang chạy
            {
                int procId = pname[0].Id; // Lấy PID của Roblox
                if (Execution.SelectedAPI.API == "Selected API: Velocity API")
                {
                    // Kiểm tra nếu Velocity đã được attach vào tiến trình Roblox
                    if (this.Velocity.IsAttached(procId))
                    {
                        ShowWindow(GetConsoleWindow(), 5); // Hiện console để debug
                        this.Dispatcher.Invoke(() =>
                        {
                            InjectionStatus.Content = "Velocity injected";
                            InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 192, 140));
                        });
                    }
                    else
                    {
                        ShowWindow(GetConsoleWindow(), 5); // Hiện console để debug
                        this.Dispatcher.Invoke(() =>
                        {
                            InjectionStatus.Content = "Velocity injection in progress";
                            InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(170, 192, 0));
                        });
                    }
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        InjectionStatus.Content = "Awaiting injection";
                        InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(192, 110, 0));
                    });
                }
            }
            else
            {
                // Dành cho WRD
                try { foreach (Process proc in Process.GetProcessesByName("erto3e4rortoergn")) { proc.Kill(); } } catch { }
                try { foreach (Process proc in Process.GetProcessesByName("Decompiler")) { proc.Kill(); } } catch { }

                this.Dispatcher.Invoke(() =>
                {
                    InjectionStatus.Content = "Roblox not opened";
                    InjectionStatus.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                    InjectionInProgress = false;
                });
            }
        }

        private System.Threading.Timer StatusCheckTimer; // Create timer

        private void StartStatusCheck(object sender, RoutedEventArgs e) // Actual function
        { 
            StatusCheckTimer = new System.Threading.Timer(StatusCheck, null, 1000, 1000); // Run the check every 10 seconds or so
        }


        // SCRIPTHUB GRID FUNCTIONS //

        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overview?view=netdesktop-6.0
        public static readonly DependencyProperty ScriptStringVal = DependencyProperty.Register("ScriptString", typeof(ScriptDetails), typeof(TabThingy));

        public struct ScriptDetails // String for the script itself
        {
            public string ScriptExecute;
        }

        // Get details of script
        public ScriptDetails SetStringValue
        {
            get => (ScriptDetails)GetValue(ScriptStringVal);
            set => SetValue(ScriptStringVal, value);
        }
        
        // When the scripthub panel loads
        private void WrapPanel_LoadedAsync(object sender, RoutedEventArgs e)
        {
            
        }

        // TOOLS GRID FUNCTIONS //

        // Multi Roblox Instance
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Applications\\MultipleRobloxInstances.exe"))
            {
                Process.Start("Applications\\MultipleRobloxInstances.exe");
            }
            else
            {
                MessageBox.Show("Downloading Multiple Roblox Instances. Click OK to continue.", "QAMSWARE");

                WebStuff.DownloadFile("https://github.com/999MS-LLC/999MSWare/releases/download/qaxiuoi/MultipleRobloxInstances.exe", "Applications\\MultipleRobloxInstances.exe");
                WebStuff.Dispose();
                Process.Start("Applications\\MultipleRobloxInstances.exe");
                MessageBox.Show("Multiple Roblox downloaded and started!", "QAMSWARE");
                PopupNotification("Multiple Roblox Instances downloaded and started!", 2000);
            }
        }

        // FPS unlocker
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Applications\\rbxfpsunlocker.exe"))
            {
                Process.Start("Applications\\rbxfpsunlocker.exe");
            }
            else
            {
                MessageBox.Show("Downloading FPS Unlocker. Click OK to continue.", "QAMSWARE");
                // Taken from https://github.com/axstin/rbxfpsunlocker
                WebStuff.DownloadFile("https://github.com/999MS-LLC/999MSWare/releases/download/qaxiuoi/rbxfpsunlocker.exe", "Applications\\rbxfpsunlocker.exe");
                WebStuff.Dispose();
                Process.Start("Applications\\rbxfpsunlocker.exe");
                MessageBox.Show("FPS unlocker downloaded and started!", "QAMSWARE");
                PopupNotification("FPS unlocker downloaded and started!", 2000);
            }
        }

        // SETTINGS GRID FUNCTIONS //
        // There are multiple tabs on this grid //

        // Velocity API selection
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Execution.SelectedAPI.API = "Selected API: Velocity API";
            CurrentAPILabel.Content = "Using Velocity API";
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWAREData");
            key.SetValue("DLL", "Selected API: Velocity API");
            key.Close();
            MessageBox.Show("API set to Velocity", "QAMSWARE");
            Console.WriteLine("[✅] API set to Velocity");
        }

        // Join Discord for help button
        private void DiscordLinkie(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/ZsHEZS8Zta");
        }

        // General options //
        // Most of the code here is pretty much self explanatory
        // Topmost enable/disable
        private void TopmostFunc(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARESettings"); // From the settings we saved
            if (SettingReg != null)
            {
                string TopMostSetting = SettingReg.GetValue("TOPMOST").ToString();
                if (TopMostSetting == "Yes")
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARESettings");
                    key.SetValue("TOPMOST", "No");
                    key.Close();
                    TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 33, 33, 33));
                    TopMostButton.Content = "Disabled";
                    Topmost = false;
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARESettings");
                    key.SetValue("TOPMOST", "Yes");
                    key.Close();
                    TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                    TopMostButton.Content = "Enabled";
                    Topmost = true;
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARESettings");
                key.SetValue("TOPMOST", "Yes");
                key.Close();
                TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                TopMostButton.Content = "Enabled";
                Topmost = true;
            }

        }

        // Load the setting for it
        private void TopmostFuncSetting(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARESettings");
            if (SettingReg != null)
            {
                string TopMostSetting = SettingReg.GetValue("TOPMOST").ToString();
                if (TopMostSetting == "Yes")
                {
                    TopMostButton.Content = "Enabled";
                    TopMostButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                    Topmost = true;
                }
            }

        }

        // Discord RPC enable/disable function
        private void DiscordRPCFunc(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARERPC");
            if (SettingReg != null)
            {
                string TopMostSetting = SettingReg.GetValue("DISCORDRPC").ToString();
                if (TopMostSetting == "Yes")
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARERPC");
                    key.SetValue("DISCORDRPC", "No");
                    key.Close();
                    DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(20, 33, 33, 33));
                    DiscordRPCButton.Content = "Disabled";
                    MessageBox.Show("Discord RPC disabled, QAMSWARE will now close. Please reopen QAMSWARE to apply settings.", "QAMSWARE");
                    Environment.Exit(0);
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARERPC");
                    key.SetValue("DISCORDRPC", "Yes");
                    key.Close();
                    DiscordRPCButton.Content = "Enabled";
                    DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                    MessageBox.Show("Discord RPC enabled, QAMSWARE will now close. Please reopen QAMSWARE to apply settings.", "QAMSWARE");
                    Environment.Exit(0);
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARERPC");
                key.SetValue("DISCORDRPC", "Yes");
                key.Close();
                DiscordRPCButton.Content = "Enabled";
                DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(30, 11, 47, 199));
                MessageBox.Show("Discord RPC enabled, QAMSWARE will now close. Please reopen QAMSWARE to apply settings.", "QAMSWARE");
                Environment.Exit(0);
            }
        }

        // Load setting for Discord RPC
        private void DiscordRPCSetting(object sender, RoutedEventArgs e)
        {
            if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARERPC") != null)
            {
                RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARERPC");
                if (SettingReg != null)
                {
                    if (SettingReg.GetValue("DISCORDRPC").ToString() != null)
                    {
                        string TopMostSetting = SettingReg.GetValue("DISCORDRPC").ToString();
                        if (TopMostSetting == "Yes")
                        {
                            DiscordRPCButton.Content = "Enabled";
                            DiscordRPCButton.Background = new SolidColorBrush(Color.FromArgb(20, 11, 47, 199));
                            DiscordRPC();
                        }
                    }  
                }
                else
                {
                    DiscordRPCButton.Background = new SolidColorBrush(Color.FromRgb(40, 195, 126));
                    DiscordRPCButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    DiscordRPC();
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARERPC");
                key.SetValue("DISCORDRPC", "Yes");
                key.Close();
                DiscordRPCButton.Background = new SolidColorBrush(Color.FromRgb(40, 195, 126));
                DiscordRPCButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                DiscordRPC();
            }
        }

        // Use custom theme function
        private void UseCustomTheme(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARETheme");
            if (SettingReg != null)
            {
                if (IsDefaultTheme == false)
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARETheme"); // Save the settings in registry
                    key.SetValue("ISDEFAULT", "No");
                    key.SetValue("LEFTRGB", LeftRGB);
                    key.SetValue("RIGHTRGB", RightRGB);
                    key.SetValue("BGIMAGEURL", BGImageURL);
                    key.SetValue("BGTRANSPARENCY", BGTransparency);
                    key.Close();
                    MessageBox.Show("Theme settings saved!", "QAMSWARE");
                    Console.WriteLine("[✅] Theme settings saved successfully.");
                }
                else
                {
                    // Do nothing lol
                    MessageBox.Show("Theme settings saved!", "QAMSWARE");
                    Console.WriteLine("[ℹ️] Theme settings saved but not applied as default theme is enabled.");
                    PopupNotification("Theme settings saved but not applied as default theme is enabled.", 2000);
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARETheme");
                key.SetValue("ISDEFAULT", "No");
                key.SetValue("LEFTRGB", LeftRGB);
                key.SetValue("RIGHTRGB", RightRGB);
                key.SetValue("BGIMAGEURL", BGImageURL);
                key.SetValue("BGTRANSPARENCY", BGTransparency);
                key.Close();
                MessageBox.Show("Theme settings saved!", "QAMSWARE");
                Console.WriteLine("[✅] Theme settings saved successfully.");
                PopupNotification("Theme settings saved successfully.", 2000);
            }
        }

        // Reset to default theme function
        private void ResetTheme(object sender, RoutedEventArgs e)
        {
            RegistryKey SettingReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QAMSWARETheme");
            if (SettingReg != null)
            {
                if (IsDefaultTheme == false)
                {
                    IsDefaultTheme = true;
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARETheme");
                    key.SetValue("ISDEFAULT", "Yes");
                    key.Close();
                    MessageBox.Show("Theme settings reset! Please reopen QAMSWARE.", "QAMSWARE");
                    Console.WriteLine("[ℹ️] Theme settings reset to default successfully.");
                    PopupNotification("Theme settings reset to default successfully. Please reopen QAMSWARE.", 2000);
                }
                else
                {
                    // Do nothing lol
                    MessageBox.Show("Theme settings reset! Please reopen QAMSWARE.", "QAMSWARE");
                    Console.WriteLine("[ℹ️] Theme settings reset to default but already in default theme.");
                    PopupNotification("Theme settings reset to default but already in default theme.", 2000);
                    PopupNotification("Please reopen QAMSWARE to apply changes.", 2000);
                }
            }
            else
            {
                IsDefaultTheme = true;
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QAMSWARETheme");
                key.SetValue("ISDEFAULT", "Yes");
                key.Close();
                MessageBox.Show("Theme settings reset! Please reopen QAMSWARE.", "QAMSWARE");
                Console.WriteLine("[ℹ️] Theme settings reset to default successfully.");
                PopupNotification("Theme settings reset to default successfully. Please reopen QAMSWARE.", 2000);
            }
        }

        // Theme options //
        // This part took time to make as well btw

        // Apply theme button
        private void ThemeApply(object sender, RoutedEventArgs e)
        {
            try
            {
                // Border first
                var conv = new ColorConverter();
                var left = (Color)conv.ConvertFrom(LeftGradient.Text);
                var right = (Color)conv.ConvertFrom(RightGradient.Text);
                LinearGradientBrush brushy = new LinearGradientBrush();
                brushy.StartPoint = new Point(0, 0);
                brushy.EndPoint = new Point(1, 1);
                brushy.GradientStops.Add(new GradientStop(left, 0.0));
                brushy.GradientStops.Add(new GradientStop(right, 1.0));
                WindowBorder.BorderBrush = brushy;

                // Image now alongside transparency
                float transparencyval = float.Parse(ImageTransparency.Text);
                transparencyval = transparencyval / 100;
                ImageBrush bb = new ImageBrush();
                Image image = new Image();
                image.Source = new BitmapImage(
                    new Uri(
                       ImageLink.Text));
                image.Opacity = transparencyval;
                bb.ImageSource = image.Source;
                bb.Opacity = transparencyval;
                MainGrid.Background = bb;
                MainGrid.Background.Opacity = transparencyval;

                // If user wants to set it as default theme
                IsDefaultTheme = false;
                LeftRGB = LeftGradient.Text;
                RightRGB = RightGradient.Text;
                BGImageURL = ImageLink.Text;
                BGTransparency = ImageTransparency.Text;

                MessageBox.Show("Theme set!", "QAMSWARE");
                Console.WriteLine("[✅] Theme applied successfully.");
                PopupNotification("Theme applied successfully.", 2000);
            }
            catch
            { 
                MessageBox.Show("Did you put in the right hex code format and image url?", "QAMSWARE");
                Console.WriteLine("[❌] Error applying theme. Please check your inputs.");
                PopupNotification("Error applying theme. Please check your inputs.", 2000);
            }
            
        }

        // To be removed soon
        private void ThemeDemo1(object sender, RoutedEventArgs e)
        {
            LeftGradient.Text = "#4C464646";
            RightGradient.Text = "#4C464646";
            ImageTransparency.Text = "20";
            CreatorName.Text = "Nguyen Minh Nhat";
            ImageLink.Text = "https://art.pixilart.com/b7875a3999e9a79.gif";
            var conv = new BrushConverter();
            WindowBorder.BorderBrush = (Brush)conv.ConvertFrom("#4C464646");
            ImageBrush bb = new ImageBrush();
            Image image = new Image();
            image.Source = new BitmapImage(
                new Uri(
                   "https://art.pixilart.com/b7875a3999e9a79.gif"));
            image.Opacity = 0.2;
            bb.ImageSource = image.Source;
            bb.Opacity = 0.2;
            MainGrid.Background = bb;
            MainGrid.Background.Opacity = 0.2;

            IsDefaultTheme = false;
            LeftRGB = "#4C464646";
            RightRGB = "#4C464646";
            BGImageURL = "https://art.pixilart.com/b7875a3999e9a79.gif";
            BGTransparency = "20";
        }

        // I'm just keeping this function here for now
        private void JSON(object sender, RoutedEventArgs e)
        {    
            try
            {
                // Deserialise first
                // Might be a bad way but works for such a small purpose
                var jsonfile = File.ReadAllText("Themes\\theme.json");
                var stuff = JObject.Parse(jsonfile);

                // Declare
                var conv = new ColorConverter();

                // Border setting first
                var left = (Color)conv.ConvertFrom(stuff.SelectToken("TopLeftBorderColour").Value<string>());
                var right = (Color)conv.ConvertFrom(stuff.SelectToken("BottomRightBorderColour").Value<string>());

                // Actual setting
                LinearGradientBrush brushy = new LinearGradientBrush();
                brushy.StartPoint = new Point(0, 0);
                brushy.EndPoint = new Point(1, 1);
                brushy.GradientStops.Add(new GradientStop(left, 0.0));
                brushy.GradientStops.Add(new GradientStop(right, 1.0));
                WindowBorder.BorderBrush = brushy;

                // Image transparency
                float transparencyval = float.Parse(stuff.SelectToken("BackgroundImageTransparency").Value<string>());
                transparencyval = transparencyval / 100;

                // Image stuff
                ImageBrush bb = new ImageBrush();
                Image image = new Image();

                image.Source = new BitmapImage(new Uri(stuff.SelectToken("BackgroundImageURL").Value<string>()));
                image.Opacity = transparencyval; // Just in case

                bb.ImageSource = image.Source;
                bb.Opacity = transparencyval; // Just in case

                MainGrid.Background = bb;
                MainGrid.Background.Opacity = transparencyval; // Just in case

                //MessageBox.Show("Theme set!", "MainDab");
            }
            catch
            {
                MessageBox.Show("Did you put in the right hex code format and image url?", "QAMSWARE");
                Console.WriteLine("[❌] Error applying theme from JSON. Please check your inputs.");
                PopupNotification("Error applying theme from JSON. Please check your inputs.", 2000);
            }
        }

        // When listbox loaded
        private void ThemeListBox_Loaded(object sender, RoutedEventArgs e)
        {
            string[] fileEntries = Directory.GetFiles("Themes"); // Get stuff from folder
            foreach (string fileName in fileEntries)
                ThemeListBox.Items.Add(fileName.Remove(0, 7));
        }

        private void ListBoxSelect(object sender, SelectionChangedEventArgs e) // When something is selected on the list
        {
            object item = ThemeListBox.SelectedItem;

            if (item == null)
            {

            }
            else
            {
                string filepath = item.ToString();
                filepath = "Themes\\" + filepath;
                try
                {

                    // Deserialise first
                    // Might be a bad way but works for such a small purpose
                    // MessageBox.Show(filepath);

                    var jsonfile = File.ReadAllText(filepath);
                    var stuff = JObject.Parse(jsonfile);

                    // Declare
                    var conv = new ColorConverter();

                    // Name
                    CreatorName.Text = stuff.SelectToken("MadeBy").Value<string>();
                    

                    // Border setting first
                    var left = (Color)conv.ConvertFrom(stuff.SelectToken("TopLeftBorderColour").Value<string>());
                    var right = (Color)conv.ConvertFrom(stuff.SelectToken("BottomRightBorderColour").Value<string>());

                    LeftGradient.Text = stuff.SelectToken("TopLeftBorderColour").Value<string>();
                    RightGradient.Text = stuff.SelectToken("BottomRightBorderColour").Value<string>();

                    // Actual setting
                    LinearGradientBrush brushy = new LinearGradientBrush();
                    brushy.StartPoint = new Point(0, 0);
                    brushy.EndPoint = new Point(1, 1);
                    brushy.GradientStops.Add(new GradientStop(left, 0.0));
                    brushy.GradientStops.Add(new GradientStop(right, 1.0));
                    WindowBorder.BorderBrush = brushy;

                    // Image transparency
                    float transparencyval = float.Parse(stuff.SelectToken("BackgroundImageTransparency").Value<string>());
                    ImageTransparency.Text = transparencyval.ToString();

                    transparencyval = transparencyval / 100;

                    // Image stuff
                    ImageBrush bb = new ImageBrush();
                    Image image = new Image();

                    image.Source = new BitmapImage(new Uri(stuff.SelectToken("BackgroundImageURL").Value<string>()));
                    image.Opacity = transparencyval; // Just in case
                    ImageLink.Text = stuff.SelectToken("BackgroundImageURL").Value<string>();

                    bb.ImageSource = image.Source;
                    bb.Opacity = transparencyval; // Just in case

                    MainGrid.Background = bb;
                    MainGrid.Background.Opacity = transparencyval; // Just in case

                    // Default theme
                    IsDefaultTheme = false;
                    LeftRGB = stuff.SelectToken("TopLeftBorderColour").Value<string>();
                    RightRGB = stuff.SelectToken("BottomRightBorderColour").Value<string>();
                    BGImageURL = stuff.SelectToken("BackgroundImageURL").Value<string>();
                    BGTransparency = stuff.SelectToken("BackgroundImageTransparency").Value<string>();

                    //MessageBox.Show("Theme set!", "MainDab");
                }
                catch
                {
                   // MessageBox.Show("The theme file is invalid!", "MainDab");
                }
            }
        }

        private void ThemeSave(object sender, RoutedEventArgs e)
        {
            // Instead of properly sterialising the json file properly, we can manually write it ourselves
            string FinalJson = ("{\n" +
                "  \"MadeBy\" : \"" + CreatorName.Text + "\",\n" +
                "  \"TopLeftBorderColour\" : \"" + LeftGradient.Text + "\",\n" +
                "  \"BottomRightBorderColour\" : \"" + RightGradient.Text + "\",\n" +
                "  \"BackgroundImageURL\" : \"" + ImageLink.Text + "\",\n" +
                "  \"BackgroundImageTransparency\" : \"" + ImageTransparency.Text + "\"\n" +
                "}"); // I didn't want to go though so much trouble figuring out steralisation, bear with this

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON file (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, FinalJson); // Save file

        }

        // Download themes button
        private void ThemeDownload(object sender, RoutedEventArgs e)
        {
            ThemeListBox.Items.Clear();
            DownloadMessage.Visibility = Visibility.Visible;

            if (!Directory.Exists("Themes"))
                Directory.CreateDirectory("Themes");

            // Load existing themes
            string[] localThemes = Directory.GetFiles("Themes");
            foreach (string filePath in localThemes)
            {
                string themeName = Path.GetFileName(filePath);
                ThemeListBox.Items.Add(themeName);
            }

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                this.Dispatcher.Invoke(() =>
                {
                    DownloadMessageTextBox.Text = "Downloading themes from theme repository...";
                    Console.WriteLine("[ℹ️] Downloading themes from theme repository...");
                    PopupNotification("Downloading themes from theme repository...", 2000);

                    if (TryFindResource("FadeInListBox") is Storyboard sbFadeIn)
                        sbFadeIn.Begin();
                });

                try
                {
                    string json = WebStuff.DownloadString("https://raw.githubusercontent.com/999MS-LLC/999MSWare/refs/heads/main/HeThong/resource/API/ThemeList.json");
                    dynamic themeList = JsonConvert.DeserializeObject(json);

                    foreach (var theme in themeList)
                    {
                        string fileName = theme.filename;
                        string url = theme.themeurl;
                        string filePath = Path.Combine("Themes", fileName);

                        try
                        {
                            if (File.Exists(filePath))
                                File.Delete(filePath); // Replace old version

                            WebStuff.DownloadFile(url, filePath);

                            this.Dispatcher.Invoke(() =>
                            {
                                if (!ThemeListBox.Items.Contains(fileName))
                                    ThemeListBox.Items.Add(fileName);
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[❌] Failed to download theme: {fileName}. Error: {ex.Message}");
                            PopupNotification($"Failed to download theme: {fileName}. Error: {ex.Message}", 2000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        DownloadMessageTextBox.Text = "[❌] Failed to download theme list.";
                        PopupNotification("Failed to download theme list.", 2000);
                        MessageBox.Show("Download failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    return;
                }

                this.Dispatcher.Invoke(() =>
                {
                    DownloadMessageTextBox.Text = "✅ Download completed!";
                    PopupNotification("Download completed!", 2000);
                    Console.WriteLine("[✅] Download completed!");

                    if (TryFindResource("FadeOutListBox") is Storyboard sbFadeOut)
                    {
                        sbFadeOut.Completed += new EventHandler(Done);
                        sbFadeOut.Begin();
                    }
                });
            }).Start();
        }


        private void Done(object sender, EventArgs e)
        {
            DownloadMessage.Visibility = Visibility.Hidden; // For the animation
        }

        // OTHER FUNCTIONS //

        private void LoadingScreenLoaded(object sender, RoutedEventArgs e)
        {
           
        }

       
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            // This is likely a very inefficient method of searching, if there are better ways, I would like to know
            // The CPU usage rises while searching
            if (IsScriptHubOpened == true)
            {
                new Thread(() =>
                {
                    this.Dispatcher.Invoke(() => // Prevent error from this being done on "another thread"
                    {
                        WP.Children.Clear();

                        foreach (var scriptData in scripts)
                        {
                            var obj = new TabThingy
                            {
                                Script = (ScriptHub.ScriptData)scriptData
                            };

                            if (GeneralScriptSearch.Text == "" || GeneralScriptSearch.Text == "Search for a script here" || GeneralScriptSearch.IsFocused == false || GeneralScriptSearch.IsKeyboardFocused == false)
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                            else if (obj.ScriptTitle.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Description.Text.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Credit.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true)
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                        }
                        GC.Collect();

                    });

                })

                { }.Start();
            }
        }

        private void GeneralScriptSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "Search for a script here")
            {
                GeneralScriptSearch.Text = "";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }
        private void GeneralScriptSearch_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "Search for a script here")
            {
                GeneralScriptSearch.Text = "";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }

        private void GeneralScriptSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "")
            {
                GeneralScriptSearch.Text = "Search for a script here";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void GeneralScriptSearch_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GeneralScriptSearch.Text == "")
            {
                GeneralScriptSearch.Text = "Search for a script here";
                GeneralScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void WP1_LoadedAsync(object sender, RoutedEventArgs e)
        {
           
        
        }

        private void GameScriptTextChanged(object sender, TextChangedEventArgs e)
        {
            
            
        }

        private void GameScriptSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (GameScriptSearch.Text == "Search for a script here")
            {
                GameScriptSearch.Text = "";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }

        private void GameScriptSearch_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GameScriptSearch.Text == "Search for a script here")
            {
                GameScriptSearch.Text = "";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(100, 137, 137, 137));
            }
        }

        private void GameScriptSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (GameScriptSearch.Text == "")
            {
                GameScriptSearch.Text = "Search for a script here";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void GameScriptSearch_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (GameScriptSearch.Text == "")
            {
                GameScriptSearch.Text = "Search for a script here";
                GameScriptSearch.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 137, 137, 137));
            }
        }

        private void GameHubOpen(object sender, MouseButtonEventArgs e)
        {
            Storyboard sb = TryFindResource("GameHubOpen") as Storyboard;
            sb.Begin();

            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Visible;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void HomeRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Visible;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void ExecutorRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Visible;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void ScriptHubRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsScriptHubOpened == false)
            {
                IsScriptHubOpened = true;
                WP.Children.Clear();
                foreach (var scriptData in scripts)
                {
                    var obj = new TabThingy
                    {
                        Script = (ScriptHub.ScriptData)scriptData
                    };
                    // Functions for buttons
                    obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                    obj.CopyScript += (_, _) => Clipboard.SetDataObject(obj.Script.Script);
                    WP.Children.Add(obj); // Add objects into scripthub panel
                }
            }

          
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Visible;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void GameHubRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsGameHubOpened == false)
            {
                IsGameHubOpened = true;
                WP1.Children.Clear();
                foreach (var scriptData in gamescripts)
                {
                    var obj = new GameTab()
                    {
                        Script = (ScriptHub.GameScriptData)scriptData
                    };
                    // Functions for buttons
                    obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                    obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                    WP1.Children.Add(obj); // Add objects into scripthub panel
                }
            }
            

            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Visible;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void ToolsRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Visible;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void SettingsRadioButtonClick(object sender, RoutedEventArgs e)
        {
            // This is pretty damn stupid to do but oh well
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Visible;
        }

        private void HomeRadioButtonLoaded(object sender, RoutedEventArgs e)
        {
            HomeRadioButton.IsChecked = true;
        }

        private void SearchGameHub(object sender, RoutedEventArgs e)
        {
            // This is likely a very inefficient method of searching, if there are better ways, I would like to know
            // The CPU usage rises while searching
            
                new Thread(() =>
                {
                    this.Dispatcher.Invoke(() => // Prevent error from this being done on "another thread"
                    {
                        WP1.Children.Clear();

                        foreach (var scriptData in gamescripts)
                        {
                            var obj = new GameTab
                            {
                                Script = (ScriptHub.GameScriptData)scriptData
                            };

                            if (GameScriptSearch.Text == "" | GameScriptSearch.Text == "Search for a script here" | string.IsNullOrEmpty(GameScriptSearch.Text))
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP1.Children.Add(obj); // Add objects into scripthub panel
                            }

                            else if (obj.ScriptTitle.Content.ToString().ToLower().Contains(GameScriptSearch.Text.ToLower()) == true | obj.Description.Text.ToString().ToLower().Contains(GameScriptSearch.Text.ToLower()) == true | obj.Credit.Content.ToString().ToLower().Contains(GameScriptSearch.Text.ToLower()) == true)
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP1.Children.Add(obj); // Add objects into scripthub panel
                            }
                        }
                    });
                })
                { }.Start();
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.F5))
             {
                if(BlurTextbox.Visibility == Visibility.Visible)
                {
                    BlurTextbox.Visibility = Visibility.Hidden;
                }
                else
                {
                    BlurTextbox.Visibility = Visibility.Visible;
                }
            }
        }

        private void SearchScriptHub(object sender, RoutedEventArgs e)
        {
            // This is likely a very inefficient method of searching, if there are better ways, I would like to know
            // The CPU usage rises while searching
            if (IsScriptHubOpened == true)
            {
                new Thread(() =>
                {
                    this.Dispatcher.Invoke(() => // Prevent error from this being done on "another thread"
                    {
                        WP.Children.Clear();

                        foreach (var scriptData in scripts)
                        {
                            var obj = new TabThingy
                            {
                                Script = (ScriptHub.ScriptData)scriptData
                            };

                            if (GeneralScriptSearch.Text == "" | GeneralScriptSearch.Text == "Search for a script here" | string.IsNullOrEmpty(GeneralScriptSearch.Text))
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                            else if (obj.ScriptTitle.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Description.Text.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true || obj.Credit.Content.ToString().ToLower().Contains(GeneralScriptSearch.Text.ToLower()) == true)
                            {
                                // Functions for buttons
                                obj.Executed += (_, _) => Velocity.Execute(obj.Script.Script);
                                obj.CopyScript += (_, _) => Clipboard.SetText(obj.Script.Script);
                                WP.Children.Add(obj); // Add objects into scripthub panel
                            }

                        }
                        GC.Collect();

                    });

                })

                { }.Start();
            }
         }

        private void CustomisationRadioButtonClick(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Hidden;
            ExecutorGrid.Visibility = Visibility.Hidden;
            ScriptHubGrid.Visibility = Visibility.Hidden;
            GameHubGrid.Visibility = Visibility.Hidden;
            ToolsGrid.Visibility = Visibility.Hidden;
            CustomisationGrid.Visibility = Visibility.Visible;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void WRDStatus_Loaded_1(object sender, RoutedEventArgs e)
        {
            // wrd status checker
            string WRDStatusDl = WebStuff.DownloadString("https://raw.githubusercontent.com/999MS-LLC/999MSWare/refs/heads/main/HeThong/trang-thai-velocityapi.json");
            dynamic WRDStatusFormat = JsonConvert.DeserializeObject(WRDStatusDl);
            bool IsWRDPatched = WRDStatusFormat.patched;

            if (!IsWRDPatched) // not patch
            {
                WRDStatus.Fill = new SolidColorBrush(Color.FromRgb(40, 195, 126));
                WRDStatusText.Text = "Unpatched and working!";
            }
            else
            {
                WRDStatus.Fill = new SolidColorBrush(Color.FromRgb(255, 30, 30));
                WRDStatusText.Text = "Currently patched";
            }
        }
    }
}