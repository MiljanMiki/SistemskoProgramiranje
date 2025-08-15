using SP_Projekat.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



/*
    Web server implementirati kao konzolnu aplikaciju koja loguje sve primljene zahteve i informacije o 
njihovoj obradi (da li je došlo do greške, da li je zahtev uspešno obrađen i ostale ključe detalje). 
Web server treba da kešira u memoriji odgovore na primljene zahteve, tako da u slučaju da stigne 
isti zahtev, prosleđuje se već pripremljeni odgovor. Kao klijentsku aplikaciju možete koristiti Web 
browser ili možete po potrebi kreirati zasebnu konzolnu aplikaciju. Za realizaciju koristiti funkcije 
iz biblioteke System.Threading, uključujući dostupne mehanizme za sinhronizaciju i zaključavanje. 
Dozvoljeno je korišćenje ThreadPool-a. 


Zadatak 21.
    Kreirati Web server koji klijentu omogućava prikaz vrednosti zagađenja vazduha uz pomoć IQ 
    Air API-a. Pretraga se može vršiti pomoću filtera koji se definišu u okviru query-a. Vrednosti 
    zagađenja vazduha se vraćaju kao odgovor (pretragu vršiti po gradu). Svi zahtevi serveru se šalju 
    preko browser-a korišćenjem GET metode. Ukoliko navedene vrednosti zagađenja ne postoje, 
    prikazati grešku klijentu. 


**** API KEY = 2729db86-ba87-4e48-9a2b-73c01124c64a


 */

namespace SP_Projekat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Cache cache = new Cache(1);

            Thread t1 = new Thread(()=> { Console.WriteLine($"{Thread.CurrentThread.Name}" + " trenutno ubacuje u kes");cache.ubaciUKes("1", "asdf"); });
            Thread t2 = new Thread(() => { Console.WriteLine($"{Thread.CurrentThread.Name}" + " trenutno ubacuje u kes"); cache.ubaciUKes("2", "ghjk"); });
            Thread t3 = new Thread(() => { Console.WriteLine($"{Thread.CurrentThread.Name}" + " trenutno proverava kes"); 
                Console.WriteLine($"{Thread.CurrentThread.Name} je procitao iz kesa: "+cache.vratiResponse("1")); });

            t1.Name = "Prvi write thread";
            t2.Name = "Drugi write thread";
            t3.Name = "Prvi read thread";
            t1.Start();
            t3.Start();
            t2.Start();
            
        }
    }
}
