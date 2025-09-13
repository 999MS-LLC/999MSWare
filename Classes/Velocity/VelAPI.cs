using coms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace VelocityAPI
{
    public class VelAPI
    {
        private HttpClient client = new HttpClient();
        private string current_injector_url = "https://getvelocity.lol/assets/erto3e4rortoergn.exe";
        private string currret_decompiler_url = "https://getvelocity.lol/assets/Decompiler.exe";
        private string current_version_url = "https://getvelocity.lol/assets/current_version.txt";
        private Process decompilerProcess;
        public VelocityStates VelocityStatus = VelocityStates.NotAttached;
        public List<int> injected_pids = new List<int>();
        private Timer CommunicationTimer;

        public static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        public static byte[] Base64Decode(string plainText) => Convert.FromBase64String(plainText);

        private bool IsPidRunning(int pid)
        {
            try
            {
                Process.GetProcessById(pid);
                return true;
            }
            catch (ArgumentException ex)
            {
                return false;
            }
        }

        private void AutoUpdate()
        {
            string result1;
            try
            {
                result1 = this.client.GetStringAsync(this.current_version_url).Result;
            }
            catch (Exception ex)
            {
                return;
            }
            string str = "";
            if (File.Exists("Bin\\current_version.txt"))
                str = File.ReadAllText("Bin\\current_version.txt");
            if (result1 != str)
            {
                if (File.Exists("Bin\\erto3e4rortoergn.exe"))
                    File.Delete("Bin\\erto3e4rortoergn.exe");
                if (File.Exists("Bin\\Decompiler.exe"))
                    File.Delete("Bin\\Decompiler.exe");
                HttpResponseMessage result2 = this.client.GetAsync(this.current_injector_url).Result;
                if (result2.IsSuccessStatusCode)
                    File.WriteAllBytes("Bin\\erto3e4rortoergn.exe", result2.Content.ReadAsByteArrayAsync().Result);
                HttpResponseMessage result3 = this.client.GetAsync(this.currret_decompiler_url).Result;
                if (result3.IsSuccessStatusCode)
                    File.WriteAllBytes("Bin\\Decompiler.exe", result3.Content.ReadAsByteArrayAsync().Result);
            }
            File.WriteAllText("Bin\\current_version.txt", result1);
        }

        public void StartCommunication()
        {
            if (!Directory.Exists("Bin"))
                Directory.CreateDirectory("Bin");
            if (!Directory.Exists("AutoExec"))
                Directory.CreateDirectory("AutoExec");
            if (!Directory.Exists("Workspace"))
                Directory.CreateDirectory("Workspace");
            if (!Directory.Exists("Scripts"))
                Directory.CreateDirectory("Scripts");
            this.AutoUpdate();
            this.StopCommunication();
            this.decompilerProcess = new Process();
            this.decompilerProcess.StartInfo.FileName = "Bin\\Decompiler.exe";
            this.decompilerProcess.StartInfo.UseShellExecute = false;
            this.decompilerProcess.EnableRaisingEvents = true;
            this.decompilerProcess.StartInfo.RedirectStandardError = true;
            this.decompilerProcess.StartInfo.RedirectStandardInput = true;
            this.decompilerProcess.StartInfo.RedirectStandardOutput = true;
            this.decompilerProcess.StartInfo.CreateNoWindow = true;
            this.decompilerProcess.Start();
            this.CommunicationTimer = new Timer(100.0);
            this.CommunicationTimer.Elapsed += (ElapsedEventHandler)((source, e) =>
            {
                foreach (int injectedPid in this.injected_pids)
                {
                    if (!this.IsPidRunning(injectedPid))
                        this.injected_pids.Remove(injectedPid);
                }
                string plainText = $"setworkspacefolder: {Directory.GetCurrentDirectory()}\\Workspace";
                foreach (int injectedPid in this.injected_pids)
                    NamedPipes.LuaPipe(VelAPI.Base64Encode(plainText), injectedPid);
            });
            this.CommunicationTimer.Start();
        }

        public void StopCommunication()
        {
            if (this.CommunicationTimer != null)
            {
                this.CommunicationTimer.Stop();
                this.CommunicationTimer = (Timer)null;
            }
            if (this.decompilerProcess != null)
            {
                this.decompilerProcess.Kill();
                this.decompilerProcess.Dispose();
                this.decompilerProcess = (Process)null;
            }
            this.injected_pids.Clear();
        }

        public bool IsAttached(int pid) => this.injected_pids.Contains(pid);

        public async Task<VelocityStates> Attach(int pid)
        {
            if (this.injected_pids.Contains(pid))
                return VelocityStates.Attached;
            this.VelocityStatus = VelocityStates.Attaching;
            Process.Start(new ProcessStartInfo()
            {
                FileName = "Bin\\erto3e4rortoergn.exe",
                Arguments = $"{pid}",
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = false,
                RedirectStandardOutput = false
            }).WaitForExit();
            this.injected_pids.Add(pid);
            this.VelocityStatus = VelocityStates.Attached;
            await Task.Delay(1500);
            string successLua = @"
local HttpRequest = http_request or request or (syn and syn.request) or (fluxus and fluxus.request)

-- Hook identifyexecutor
if hookfunction and identifyexecutor then
    hookfunction(identifyexecutor, function()
        return '999MS', '0.0.5'
    end)
else
    identifyexecutor = function()
        return '999MS', '0.0.5'
    end
end

-- Override request to include custom User-Agent
local function customRequest(req)
    req.Headers = req.Headers or {}
    req.Headers['User-Agent'] = '999MS/Roblox/NguyenMinhNhat/Đĩ Mẹ Tụi Mày 🤡'
    return HttpRequest(req)
end

getgenv().http_request = customRequest
getgenv().request = customRequest
-- Logs
warn(""Made With 💗 Nguyen Minh Nhat"")
warn([[
▒█▀▀█ ░█▀▀█ ░ ▒█▀▄▀█ ▒█▀▀▀█ ▒█░░▒█ ░█▀▀█ ▒█▀▀█ ▒█▀▀▀ 
▒█░▒█ ▒█▄▄█ ▄ ▒█▒█▒█ ░▀▀▀▄▄ ▒█▒█▒█ ▒█▄▄█ ▒█▄▄▀ ▒█▀▀▀ 
░▀▀█▄ ▒█░▒█ █ ▒█░░▒█ ▒█▄▄▄█ ▒█▄▀▄█ ▒█░▒█ ▒█░▒█ ▒█▄▄▄]])
print(""[✅] API Data Load Successfully, QA.MSWARE Wishes You A Happy Experience !!"")
local MessageBox = loadstring(game:HttpGet(""https://raw.githubusercontent.com/xHeptc/NotificationGUI/main/source.lua""))()
MessageBox.Show({Position = UDim2.new(0.5,0,0.5,0), Text = ""GodsWave?"", Description = ""Successful injection !! \nPls Join My Sever Discord?"", MessageBoxIcon = ""Question"", MessageBoxButtons = ""YesNo"", Result = function(res)
   if (res == ""Yes"") then
       MessageBox.Show({MessageBoxButtons = ""OK"", Description = ""Wow, you said Yes! Thank you, copy link done."", Text = ""YAYYY!""})
       setclipboard(""https://discord.gg/ZsHEZS8Zta"")
   elseif (res == ""No"") then
       MessageBox.Show({MessageBoxButtons = ""OK"", Description = ""Dit Con Me Nha May Thang Bu Lon =))"", Text = ""Di Me May =))""})
   end
end})	
    game:GetService('StarterGui'):SetCore('SendNotification', {
        Title = 'QA.MSWARE',
        Text = 'Successful injection!',
        Duration = 5
    })
";
            this.Execute(successLua);
            return VelocityStates.Attached;
        }

        public VelocityStates Execute(string script)
        {
            if (this.injected_pids.Count.Equals(0))
                return VelocityStates.NotAttached;
            foreach (int injectedPid in this.injected_pids)
                NamedPipes.LuaPipe(VelAPI.Base64Encode(script), injectedPid);
            return VelocityStates.Executed;
        }
    }
}
