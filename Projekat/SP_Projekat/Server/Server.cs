using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SP_Projekat.IQAirApi;

namespace SP_Projekat.Server
{
    internal class Server
    {
        
        private Cache cache;
        private static IQAirApi.IQAirService api;
        private readonly string TAG = "[Server]";

        //Ako nesto treba da se loguje, samo zovi Logger.Info
        //                                              .Error
        //                                              .Warning(TAG,message)
        //LOGUJ SVE! Kada uvatis gresku, da li se pocinje prerada, da li se zove API itd.
       

        public Server(int velicinaKesa)
        {
            cache = new Cache(velicinaKesa);
            api = new IQAirService();
        }

        public void preradiRequest(string city, string state,string country)
        {
            //otprilike ovako da izgleda

            object result = null;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                result = api.vratiZagadjenostGrada(city,state,country);
            }
            );
            string resultString = (string)result;

            //radi sa resultString posle sta oces.Mozda moze i unutar api da se formatira izlaz
        }
    }
}
