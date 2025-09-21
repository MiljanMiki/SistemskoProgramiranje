using SPProjekat3.TextProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



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
        static void Main(string[] args)
        {
            tmTest();

        }
    }
}
