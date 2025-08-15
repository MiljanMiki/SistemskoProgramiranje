using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

using SP_Projekat.Server;


namespace SP_Projekat.IQAirApi
{
    internal class IQAirService
    {

        private readonly string api_key = "2729db86-ba87-4e48-9a2b-73c01124c64a";

        private readonly string TAG="[IqAirService]";

        public IQAirService() { }

        public string vratiZagadjenostGrada(string city, string state, string country)
        {
            try
            {
                string requestUri = $"http://api.airvisual.com/v2/city" +
                                    $"?city={Uri.EscapeDataString(city)}" +
                                    $"&state={Uri.EscapeDataString(state)}" +
                                    $"&country={Uri.EscapeDataString(country)}" +
                                    $"&key={api_key}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(TAG, ex.Message);
                return null;
            }
        }
    }

}

