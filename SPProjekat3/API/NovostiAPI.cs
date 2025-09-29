using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using SPProjekat3.ServerSide;

namespace SPProjekat3.API
{
    internal class NovostiAPI
    {

        private const string apiKey= "2e257ba2ba314aaa9ab176319564a177";
        private const string TAG = "[NewsAPI]";

        public NovostiAPI() { }
        public async Task<List<string>> vratiNajpopularnijeClankoveAsync(string keyword)
        {
            //string request = $"https://newsapi.org/v2/everything?q={keyword}&sortBy=popularity&apiKey={apiKey}";

            
            try
            {
                var newsApiClient = new NewsApiClient(apiKey);

                var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
                {
                    Q = keyword,
                    SortBy = SortBys.Popularity,
                    Language = Languages.EN
                });

                if (articlesResponse.Status == Statuses.Ok)
                {
                    //u principu sve sto meni treba je article.Description i nista vise

                    List<String> povratnaVrednost = new List<string>(articlesResponse.TotalResults);
                    foreach (var article in articlesResponse.Articles)
                    {
                        povratnaVrednost.Add(article.Description);
                    }

                    return povratnaVrednost;
                }
                else
                {
                    throw new Exception(TAG+"Greska prilikom pozivanja NewsAPI-ja!");
                }
            }
            catch(Exception e)
            {
                Logger.Error(TAG, e.Message);
                return null;
            }
        }

       
    }
}
