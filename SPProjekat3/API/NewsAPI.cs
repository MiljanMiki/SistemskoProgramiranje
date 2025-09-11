using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;

namespace SPProjekat3.API
{
    internal class NovostiAPI
    {

        private const string apiKey= "2e257ba2ba314aaa9ab176319564a177";

        public void call(string keyword)
        {
            //string request = $"https://newsapi.org/v2/everything?q={keyword}&sortBy=popularity&apiKey={apiKey}";

            var newsApiClient = new NewsApiClient(apiKey);
            var articlesResponse = newsApiClient.GetEverything(new EverythingRequest
            {
                Q = keyword,
                SortBy = SortBys.Popularity,
            });

            if (articlesResponse.Status == Statuses.Ok)
            {
                //u principu sve sto meni treba je article.Description i nista vise

                // total results found
                Console.WriteLine(articlesResponse.TotalResults);
                // here's the first 20
                foreach (var article in articlesResponse.Articles)
                {
                    // title
                    Console.WriteLine(article.Title);
                    // author
                    Console.WriteLine(article.Author);
                    // description
                    Console.WriteLine(article.Description);
                    // url
                    Console.WriteLine(article.Url);
                    // published at
                    Console.WriteLine(article.PublishedAt);
                }
            }
            else
            {
                throw new Exception("Greska prilikom pozivanja NewsAPI-ja!");
            }
        }

    }
}
