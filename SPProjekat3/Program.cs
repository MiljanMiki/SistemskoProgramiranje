using SPProjekat3.TextProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using SPProjekat3.ServerSide;


/*
Koristeći principe Reaktivnog programiranja i News API,
implementirati aplikaciju za prikaz opisa za određene članke (description property).
Koristiti /v2/everything endpoint. Prilikom poziva proslediti odgovarajuću
ključnu reč (keyword). Za prikupljene opise implementirati Topic Modeling 
koristeći SharpEntropy biblioteku. Članke sortirati po popularnosti. Prikazati dobijene rezultate.
 */


namespace SPProjekat3
{
    
    internal class Program
    {
        
        static void tmTest()
        {
            TopicModeler tm = new TopicModeler(null);

            string imeModela = "modelSamoDescription";
            tm.treniranje(imeModela);
            Console.WriteLine("Gotovo treniranje modela!");
            //tm.testiranje(imeModela, new Data("Did a single whale disrupt the crypto ocean?"));
        }

        static void lemmatizedTest()
        {
            TopicModeler tm = new TopicModeler(null);

            string imeModela = "modelPreLemmatized";
            //tm.treniranjeLemmatized(imeModela);
            //Console.WriteLine("Gotovo treniranje modela!");
            tm.testiranje(imeModela, new Data("Did a single whale disrupt the crypto ocean?"));
        }

        static void modelBuilderTestTSV()
        {
            string path = @"C:\Users\Korisnik\Desktop\Faks\Semestar6\Sistemsko Programiranje\SistemskoProgramiranje\SPProjekat3\TrainingData\archive\News_Category_Dataset_v3.json";
            string output = @"C:\Users\Korisnik\Desktop\Faks\Semestar6\Sistemsko Programiranje\SistemskoProgramiranje\SPProjekat3\TrainingData\archive\labeledData.txt";
            SredjivanjePodataka.srediJsonTabSeperatedLabeled(path, output);
        }

        static void modelBuilderTestCSV()
        {
            string path = @"C:\Users\Korisnik\Desktop\Faks\Semestar6\Sistemsko Programiranje\SistemskoProgramiranje\SPProjekat3\TrainingData\archive\News_Category_Dataset_v3.json";
            string output = @"C:\Users\Korisnik\Desktop\Faks\Semestar6\Sistemsko Programiranje\SistemskoProgramiranje\SPProjekat3\TrainingData\archive\labeledData.csv";
            SredjivanjePodataka.srediJsonCSV(path, output);
        }

        static void testirajKes(ServerSaKesomZaTaskove server)
        {
            List<string> urls = new List<string>();
            urls.Add("/bitcoin");
            urls.Add("/bitcoin");
            urls.Add("/ElonMusk");
            urls.Add("/japan");
            urls.Add("/serbia");
            urls.Add("/russia");
            urls.Add("/bitcoin");
            urls.Add("/ElonMusk");

            for (int i = 0; i < 100; ++i)
            {
                foreach (string url in urls)
                {
                    //Thread.Sleep(5000);
                    Task.Run(async () =>
                    {

                        server.testKes(url);

                    });
                }
            }
        }

        static void Main(string[] args)
        {
            //tmTest();
            //lemmatizedTest();
            //modelBuilderTestTSV();
            //modelBuilderTestCSV();

            //zahtev oblika http://localhost:5050/keyword
            ServerSaKesomZaTaskove s = new ServerSaKesomZaTaskove(5, "modelPreLemmatized");
            //testirajKes(s);
            s.StartServer();

            Console.ReadLine();

            s.StopServer();

        }
    }
}
