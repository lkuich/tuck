using TensorFlow;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class ImageTensor
{
    // Labels are pulled from the generated labels.txt from our model training
    private string[] labels { get; set; }

    private byte[] graphModel;
    private TFGraph graph;
    private TFSession session;

    // Use this for initialization
    public ImageTensor(string modelPath, string[] labels)
    {
        graphModel = File.ReadAllBytes(modelPath);
        this.labels = labels.Where(l => !string.IsNullOrEmpty(l)).ToArray();

        graph = new TFGraph();
        graph.Import(graphModel, "");
        session = new TFSession(graph);
    }

    /// <summary>
    /// Partially based off of: https://github.com/migueldeicaza/TensorFlowSharp/blob/master/Examples
    /// </summary>
    /// <param name="tensor"></param>
    public List<Match> Parse(TFTensor tensor)
    {
        var runner = session.GetRunner();
        string inputName = "Placeholder";

        var gimput = graph[inputName];
        var gresult = graph["final_result"];

        if (graph[inputName] == null)
            throw new System.Exception("No input");
        if (graph["final_result"] == null)
            throw new System.Exception("No result");

        runner.AddInput(graph[inputName][0], tensor);
        runner.Fetch(graph["final_result"][0]);

        var output = runner.Run();

        var result = output[0];
        var rshape = result.Shape;
        if (result.NumDims != 2 || rshape[0] != 1)
        {
            var shape = "";
            foreach (var d in rshape)
            {
                shape += $"{d} ";
            }
            shape = shape.Trim();
        }

        var probabilities = ((float[][])result.GetValue(jagged: true))[0];
        
        
        int bestIdx = 0;
        float best = 0;
        for (int i = 0; i < probabilities.Length; i++)
        {
            if (probabilities[i] > best)
            {
                bestIdx = i;
                best = probabilities[i];
            }
        }

        int secondBestIdx = 0;
        float secondBest = 0;
        for (int i = 0; i < probabilities.Length; i++)
        {
            if (probabilities[i] > secondBest && i != bestIdx)
            {
                secondBestIdx = i;
                secondBest = probabilities[i];
            }
        }

        var bestMatch = new Match()
        {
            Name = labels[bestIdx],
            Confidence = best
        };
        var secondBestMatch = new Match()
        {
            Name = labels[secondBestIdx],
            Confidence = secondBest 
        };

        return new List<Match>() {
            bestMatch, secondBestMatch
        };
    }
}

public class Match
{
    public string Name { get; set; }
    public float Confidence { get; set; }
}