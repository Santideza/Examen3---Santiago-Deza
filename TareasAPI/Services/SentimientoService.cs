using Microsoft.ML;
using Microsoft.ML.Data;

namespace TareasAPI.Services;

public class SentimientoService
{
    private readonly MLContext _mlContext = new();
    private readonly PredictionEngine<SentimientoData, SentimientoPrediccion> _predictionEngine;

    public SentimientoService()
    {
        var datosPath = Path.Combine(Directory.GetCurrentDirectory(), "MLData", "datos_sentimiento.csv");
        var modeloPath = Path.Combine(Directory.GetCurrentDirectory(), "modelo_sentimiento.zip");

        var datos = _mlContext.Data.LoadFromTextFile<SentimientoData>(
            datosPath,
            hasHeader: true,
            separatorChar: ',');

        var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(SentimientoData.Texto))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: nameof(SentimientoData.Etiqueta),
                featureColumnName: "Features"));

        var modelo = pipeline.Fit(datos);
        _mlContext.Model.Save(modelo, datos.Schema, modeloPath);

        _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimientoData, SentimientoPrediccion>(modelo);
    }

    public string Analizar(string comentario)
    {
        var prediccion = _predictionEngine.Predict(new SentimientoData { Texto = comentario });
        return prediccion.Prediccion ? "Positivo" : "Negativo";
    }
}

public class SentimientoData
{
    [LoadColumn(0)]
    public string Texto { get; set; } = string.Empty;

    [LoadColumn(1)]
    public bool Etiqueta { get; set; }
}

public class SentimientoPrediccion
{
    [ColumnName("PredictedLabel")]
    public bool Prediccion { get; set; }
}
