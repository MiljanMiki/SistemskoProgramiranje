using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using SPProjekat3.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Snowball;


namespace SPProjekat3.TextProcessing
{
    class LdaPrediction
    {
        // name must match the output column name used for LDA
        [VectorType]
        public float[] Topics { get; set; }
    }

    class Data
    {
        public string Description;

        public Data(string d)
        {
            Description = d;
        }

    }
    internal class TopicModeler
    {
        private readonly NovostiAPI api;
        private readonly Dictionary<float, string> listaTopics;
        public TopicModeler(NovostiAPI api) { this.api = api;listaTopics = new Dictionary<float, string>(40); }



        public void treniranjeLemmatized(string imeModela)
        {
            List<Data> listaData = SredjivanjePodataka.citajLemmatizedData();

            

            trenirajModelLemmatized(listaData,imeModela);
        }

        private void trenirajModelLemmatized(List<Data> data,string imeModela)
        {
            MLContext mlContext = new MLContext();

            var mlData = mlContext.Data.LoadFromEnumerable(data);

            //pise da dataset sadrzi oko ~40 topics 
            int numberOfTopics = 40;

            

            //ovde se sredjuje input data.NgramLenght je ime topic-a
            //dataset kaze da ime 40 topics, a ima nekih koji su sastavljeni od 2 ili vise reci
            var pipeline =
             mlContext.Transforms.Text.NormalizeText("NormalizedText", "Description")
            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
            .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("TokenKeys", "Tokens"))
            .Append(mlContext.Transforms.Text.ProduceNgrams(
                        outputColumnName: "Ngrams",
                        inputColumnName: "TokenKeys",
                        ngramLength: 2,
                        useAllLengths: true,
                        maximumNgramsCount: 50000))

            //zatim se dodaje LDA transformer na sredjen data
            .Append(mlContext.Transforms.Text.LatentDirichletAllocation(
                        outputColumnName: "Topics",
                        inputColumnName: "Ngrams",
                        numberOfTopics: numberOfTopics,
                        maximumNumberOfIterations: 200,
                        numberOfSummaryTermsPerTopic: 15));


            var trainedModel = pipeline.Fit(mlData);

            var transformers = trainedModel as IEnumerable<ITransformer>;
            var lastTransformer = transformers?.Last() as LatentDirichletAllocationTransformer;
            if (lastTransformer == null)
                throw new Exception("Could not get LDA transformer from the pipeline.");

            // Get details (ModelParameters) for the LDA column (column index 0 because we only added one LDA output)
            var ldaDetails = lastTransformer.GetLdaDetails(0);


            for (int t = 0; t < ldaDetails.WordScoresPerTopic.Count; t++)
            {
                Console.WriteLine($"\nTopic {t}:");
                foreach (var wi in ldaDetails.WordScoresPerTopic[t].Take(10))
                {
                    Console.WriteLine($"  {wi.Word} ({wi.Score:F4})");
                    //listaTopics.Add(wi.Score, wi.Word);
                }


            }

            mlContext.Model.Save(trainedModel, mlData.Schema, imeModela + ".zip");
        }


        public void treniranje(string imeModela)
        {
            //normalize → tokenize → remove stopwords
            //→ map tokens to keys → produce n-grams (vocab) → LDA

            //podaci su uzeti od
            //https://www.kaggle.com/datasets/rmisra/news-category-dataset/data 

            List<Data> descriptionsData = SredjivanjePodataka.rucnoStemmovanje();

            treniranjeStemmed(descriptionsData, imeModela);

        }
        private void treniranjeStemmed(List<Data> data,string imeModela)
        {
            MLContext mlContext = new MLContext();

            var mlData= mlContext.Data.LoadFromEnumerable(data);

            //pise da dataset sadrzi oko ~40 topics 
            int numberOfTopics = 40;

            string[] customStopWords = { "you","your","you'll",",","me","did","i'm","day",
                                        "said", "&","says","like","doesn't","."," - " };

            //ovde se sredjuje input data.NgramLenght je ime topic-a
            //dataset kaze da ime 40 topics, a ima nekih koji su sastavljeni od 2 ili vise reci
            var pipeline =
             mlContext.Transforms.Text.NormalizeText("NormalizedText", "Description")
            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
            .Append(mlContext.Transforms.Text.FeaturizeText("FeaturizedText", "Tokens"))
            .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens"))
            //.Append(mlContext.Transforms.Text.RemoveStopWords("Tokens",null,customStopWords))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("TokenKeys", "Tokens"))
            .Append(mlContext.Transforms.Text.ProduceNgrams(
                        outputColumnName: "Ngrams",
                        inputColumnName: "TokenKeys",
                        ngramLength: 1,            
                        useAllLengths: false,
                        maximumNgramsCount: 50000))

            //zatim se dodaje LDA transformer na sredjen data
            .Append(mlContext.Transforms.Text.LatentDirichletAllocation(
                        outputColumnName: "Topics",
                        inputColumnName: "Ngrams",
                        numberOfTopics: numberOfTopics,
                        maximumNumberOfIterations: 200,
                        numberOfSummaryTermsPerTopic: 15));


            var trainedModel = pipeline.Fit(mlData);

            var transformers = trainedModel as IEnumerable<ITransformer>;
            var lastTransformer = transformers?.Last() as LatentDirichletAllocationTransformer;
            if (lastTransformer == null)
                throw new Exception("Could not get LDA transformer from the pipeline.");

            // Get details (ModelParameters) for the LDA column (column index 0 because we only added one LDA output)
            var ldaDetails = lastTransformer.GetLdaDetails(0);


            for (int t = 0; t < ldaDetails.WordScoresPerTopic.Count; t++)
            {
                Console.WriteLine($"\nTopic {t}:");
                foreach (var wi in ldaDetails.WordScoresPerTopic[t].Take(10))
                {
                    Console.WriteLine($"  {wi.Word} ({wi.Score:F4})");
                    //listaTopics.Add(wi.Score, wi.Word);
                }


            }

            mlContext.Model.Save(trainedModel, mlData.Schema,imeModela+".zip");

        }

        public void testiranje(string imeModela,Data input)
        {
            DataViewSchema schema;

            MLContext mlContext = new MLContext();
            var trainedModel = mlContext.Model.Load(imeModela+".zip", out schema);
            var engine = mlContext.Model.CreatePredictionEngine<Data,LdaPrediction >(trainedModel);

            var pred = engine.Predict(input);

            Console.WriteLine("Topic distribution:");
            for (int i = 0; i < pred.Topics.Length; i++)
                Console.WriteLine($"Topic {i}: {pred.Topics[i]:F4}");

            int dominant = Array.IndexOf(pred.Topics, pred.Topics.Max());
            Console.WriteLine($"Dominant topic: {dominant}");

            //string value;
            //listaTopics.TryGetValue(dominant, out value);
            //Console.WriteLine($"Dominant topic: {value}");

        }
    }
}
