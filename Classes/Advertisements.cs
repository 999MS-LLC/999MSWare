using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MainDabRedo.Classes
{
    internal class Advertisement
    {
        public string imageUrl { get; set; }
        public string redirectLink { get; set; }

        public string uuid { get; set; }
    }

    internal static class Advertisements
    {
        private static List<Advertisement> ads = new List<Advertisement>();
        private static readonly string jsonUrl = "https://raw.githubusercontent.com/NguyenNhatIT/GodsWave/refs/heads/main/hethong/he-thong-quang-cao.json";

        public static async Task LoadAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(jsonUrl);
                    ads = JsonConvert.DeserializeObject<List<Advertisement>>(json);
                }
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                System.Diagnostics.Debug.WriteLine("Lỗi tải quảng cáo: " + ex.Message);
            }
        }

        public static Advertisement GetRandomAd()
        {
            if (ads.Count == 0) return null;
            return ads[new Random().Next(ads.Count)];
        }
    }
}
