//using System.Windows.Forms;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SPProjekat3.TextProcessing;

namespace SPProjekat3.ServerSide
{
    internal class Predictor
    {
        private readonly PredictionEngine<Data, LdaPrediction> engine;



        private static readonly Object locker = new object();

        public Predictor(string imeModela)
        {
            DataViewSchema schema;

            MLContext mlContext = new MLContext();
            var trainedModel = mlContext.Model.Load(imeModela + ".zip", out schema);
            this.engine = mlContext.Model.CreatePredictionEngine<Data, LdaPrediction>(trainedModel);
        }



        public string predict(string input)
        {
            LdaPrediction pred;
            lock (locker)
            {
                pred = engine.Predict(new Data(input));
            }
            int dominant = Array.IndexOf(pred.Topics, pred.Topics.Max());
            var dominantTopic = $"Dominant topic: {dominant}";

            return dominantTopic;
        }

        
    }
}
