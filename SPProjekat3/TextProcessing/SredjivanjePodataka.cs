using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SPProjekat3.TextProcessing
{
    static internal class SredjivanjePodataka
    {
        public static void srediJsonCSV(string inputPath, string output)
        {
            // Input and output file paths


            using (var reader = new StreamReader(inputPath))
            using (var writer = new StreamWriter(output,false, new System.Text.UTF8Encoding(false)))
            {
                string line;
                writer.WriteLine("short_description,category");

                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(line))
                        {
                            string shortDesc = doc.RootElement.GetProperty("short_description").GetString();
                            string category = doc.RootElement.GetProperty("category").GetString();

                            shortDesc = removePunctuation(shortDesc);
                            category = removePunctuation(category);
                            shortDesc=RemoveEmojis(shortDesc);

                            if (string.IsNullOrWhiteSpace(shortDesc) || string.IsNullOrWhiteSpace(category))
                                continue;


                            //shortDesc = EscapeCsv(shortDesc);

                            //shortDesc = RemoveEmojis(shortDesc);
                            //category = EscapeCsv(category);

                            string pisiOvajString = $"{shortDesc},{category}";

                            writer.WriteLine(pisiOvajString);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Skipping invalid JSON line: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Done! Extracted data saved to {output}");
        }

        static string EscapeCsv(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                field = "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        static string removePunctuation(string field)
        {
            if (field.Contains(","))
                field = field.Replace(",", "");

            if (field.Contains("\n"))
                field = field.Replace("\n", "");

            if (field.Contains("\""))
                field = field.Replace("\"", "");

            if (field.Contains("'"))
                field = field.Replace("'", "");

            if (field.Contains("\t"))
                field = field.Replace("\t", "");


            return field;

        }
        public static void srediJsonTabSeperatedLabeled(string inputPath, string output)
        {
            // Input and output file paths


            using (var reader = new StreamReader(inputPath))
            using (var writer = new StreamWriter(output))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(line))
                        {
                            string shortDesc = doc.RootElement.GetProperty("short_description").GetString();
                            string category = doc.RootElement.GetProperty("category").GetString();

                            if (string.IsNullOrWhiteSpace(shortDesc))
                                continue;

                            shortDesc=EscapeTsv(shortDesc);

                            writer.WriteLine($"{shortDesc}\t{category}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Skipping invalid JSON line: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Done! Extracted data saved to {output}");
        }

        static string EscapeTsv(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Quote field if it contains tab or quote
            if (field.Contains("\t") || field.Contains("\""))
            {
                field = "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }
        

        static string RemoveEmojis(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove surrogate pairs (common emoji encoding in UTF-16)
            return System.Text.RegularExpressions.Regex.Replace(input, @"\p{Cs}", "");
        }

        public static List<Data> citajLemmatizedData()
        {
            string path = @"C:\Users\Korisnik\Desktop\Faks\Semestar6\Sistemsko Programiranje\SistemskoProgramiranje\SPProjekat3\TrainingData\archive\lemmatizedData.txt";

            List<Data> listaData = new List<Data>(10000);

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            try
            {
                using (StreamReader streamReader = new StreamReader(fs))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        listaData.Add(new Data(line));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                fs.Close();
            }


            return listaData;
        }

        public static List<Data> rucnoStemmovanje()
        {
            string path = @"C:\Users\Korisnik\Desktop\Faks\Semestar6\Sistemsko Programiranje\SistemskoProgramiranje\SPProjekat3\TrainingData\archive\News_Category_Dataset_v3.json";

            List<Data> descriptionsData = new List<Data>(10000);

            Snowball.EnglishStemmer stemmer = new Snowball.EnglishStemmer();

            foreach (string line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    JsonDocument doc = JsonDocument.Parse(line);
                    JsonElement root = doc.RootElement;



                    if (root.TryGetProperty("short_description", out JsonElement desc))
                    {
                        string description = desc.GetString();
                        string[] reci = description.Split(' ');

                        string stemmedDescription = "";

                        foreach (var rec in reci)
                        {
                            stemmedDescription += stemmer.Stem(rec);
                            stemmedDescription += " ";
                        }

                        descriptionsData.Add(new Data(stemmedDescription));
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Skipping invalid line: {ex.Message}");
                }
            }

            return descriptionsData;
        }

    }
}

