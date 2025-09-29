using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SPProjekat2.Server;


namespace SPProjekat2.IQAirApi
{
    internal class IQAirService
    {
        private static readonly HttpClient client = new HttpClient();

        private const string api_key = "2729db86-ba87-4e48-9a2b-73c01124c64a";

        private const string TAG = "[IqAirService]";

        public IQAirService() { }

        public async Task<string> vratiZagadjenostGradaAsync(string city, string state, string country)
        {
            string requestUri = $"http://api.airvisual.com/v2/city" +
                                $"?city={Uri.EscapeDataString(city)}" +
                                $"&state={Uri.EscapeDataString(state)}" +
                                $"&country={Uri.EscapeDataString(country)}" +
                                $"&key={api_key}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {

                return await reader.ReadToEndAsync();
            }
            
        }
    }

}

