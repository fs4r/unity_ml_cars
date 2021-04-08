using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class NeuralNetwork
{
    public double[] Weights { get; set; }
    public int InputLength { get; set; }
    public int LayerCount { get; set; }
    public int NeuronCount { get; set; }
    public int OutputCount { get; set; }
    public List<Layer> Layers = new List<Layer>();
    public string OutputMethod { get; set; }

    public NeuralNetwork(int inputLength, int layers, int neurons, int outputs, string outputMethod="linear")
    {
        InputLength = inputLength;
        LayerCount = layers;
        NeuronCount = neurons;
        OutputCount = outputs;
        OutputMethod = outputMethod;
    }

    public void SetWeights(double[] weights)
    {
        int weightLength = GetWeightLength();

        if (weightLength != weights.Length)
        {
            throw new Exception("Weight does not match NeuralNetwork architecture");
        }
        Weights = weights;
    }

    public int GetWeightLength()
    {
        int sum = 0;
        sum += InputLength * NeuronCount + NeuronCount;
        for (int i = 0; i < LayerCount - 1; i++)
        {
            sum += NeuronCount * NeuronCount + NeuronCount;
        }
        sum += NeuronCount * OutputCount + OutputCount;
        return sum;
    }
    public void BuildLayers()
    {
        Layers.Clear();
        if(Weights == null)
        {
            int weightLength = this.GetWeightLength();
            double[] weights = new double[weightLength];
            for (int i = 0; i < weightLength; i++)
            {
                weights[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
            }
            Weights = weights;
        }

        int k = 0;
        for (int i = 0; i < LayerCount + 1; i++)
        {
            Layer layer;
            if (i == 0)
            {
                double[] layerWeights = new double[NeuronCount * InputLength + NeuronCount];
                for (int j = 0; j < NeuronCount * InputLength + NeuronCount; j++)
                {
                    layerWeights[j] = Weights[k];
                    k += 1;
                }
                layer = new Layer(layerWeights, InputLength, NeuronCount);
            }
            else if (i == LayerCount)
            {
                double[] layerWeights = new double[NeuronCount * OutputCount + OutputCount];
                for (int j = 0; j < NeuronCount * OutputCount + OutputCount; j++)
                {
                    layerWeights[j] = Weights[k];
                    k += 1;
                }
                layer = new Layer(layerWeights, NeuronCount, OutputCount, OutputMethod);
            }
            else
            {
                double[] layerWeights = new double[NeuronCount * NeuronCount + NeuronCount];
                for (int j = 0; j < NeuronCount * NeuronCount + NeuronCount; j++)
                {
                    layerWeights[j] = Weights[k];
                    k += 1;
                }
                layer = new Layer(layerWeights, NeuronCount, NeuronCount);
            }
            Layers.Add(layer);
        }
    }
    public void PrintNetwork()
    {
        Debug.Log("Input Length: " + InputLength);
        Debug.Log("Output Length: " + OutputCount);
        for (int i=0; i < Layers.Count; i++) {
            Debug.Log("=========");
            Debug.Log("Layer " + i);
            foreach(double weight in Layers[i].Weights)
            {
                Debug.Log(weight);
            }
        }
    }
    public double[] ForwardPass(double[] input)
    {
        if(input.Length != InputLength)
        {
            throw new Exception("Input length does not match!");
        }
        double[] x = input;
        for (int i = 0; i < Layers.Count; i++)
        {
            x = Layers[i].LayerOutput(x);
        }
        return x;
    }
}
