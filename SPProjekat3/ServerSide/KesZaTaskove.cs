using SPProjekat3.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPProjekat3.ServerSide
{
    internal class KesZaTaskove
    {
        private readonly ConcurrentDictionary<string, Lazy<Task<List<string>>>> cache;

        private Cache kesZaResults;
        private readonly NovostiAPI api;

        private const string TAG = "[KesZaTaskove]";

        public KesZaTaskove(Cache c, NovostiAPI api)
        {
            this.api = api;
            this.kesZaResults = c;

            int numProc = Environment.ProcessorCount;

            cache = new ConcurrentDictionary<string, Lazy<Task<List<string>>>>(numProc * 2, c.VelicinaKesa);
            
        }

        public async Task<List<string>> vratiRezultatAsync(string request)
        {
            try
            {
                var results = kesZaResults.vratiResponse(request);
                Logger.Info(TAG, "Cache hit");
                return results;
            }
            catch (ArgumentException argE)
            {
                Logger.Error(TAG, argE.Message);

                var taskZaNalazenjeRezultata = cache.GetOrAdd(request, value =>
                    new Lazy<Task<List<string>>>(async () =>
                    {
                        try
                        {
                            Logger.Info(TAG, $"Thread sa ID:{Environment.CurrentManagedThreadId} zove API");
                            var rezultati = await api.vratiNajpopularnijeClankoveAsync(request);
                            Logger.Info(TAG, $"Thread sa ID:{Environment.CurrentManagedThreadId} vracen rezultat od API");

                            kesZaResults.ubaciUKes(request, rezultati);

                            return rezultati;
                        }
                        catch (ArgumentException e)//ubaciUKes vratio gresku
                        {
                            Logger.Error(TAG, e.Message);
                            return kesZaResults.vratiResponse(request);
                        }
                        catch (Exception ex)//ako api vrati gresku, onda smo gotovi x(
                        {
                            Logger.Error(TAG, ex.Message);
                            return null;
                        }
                        finally
                        {
                            //moze da se izbaci dok neki thread ceka
                            //npr: t1 je u kesZaResults.vratiResponse
                            //a t2 dodje ovde i izbrise se
                            //t1 je uhvatio exception, i umesto da cita iz kesa
                            //on pokrece novi request za api ionako se nalazi u kesu!!!!
                            cache.TryRemove(request, out _);

                        }
                    }
                    ));


                try
                {
                    Logger.Info(TAG, $"Thread sa ID:{Environment.CurrentManagedThreadId} ceka rezultat");
                    var results = await taskZaNalazenjeRezultata.Value;
                    Logger.Info(TAG, $"Thread sa ID:{Environment.CurrentManagedThreadId} primljen rezultat");
                    return results;
                }
                catch (Exception e)
                {
                    Logger.Error(TAG, e.Message);
                    cache.TryRemove(request, out _);

                    throw new Exception(TAG + " Doslo je do nepoznate greske!");
                }

            }

        }
    }
}
