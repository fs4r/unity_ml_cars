using System;
using UnityEngine;

public class Layer
{
    public int InputLength { get; set; }
    public int NeuronCount { get; set; }
    public double[] Weights { get; set; }
    public string Activation { get; set; }

    public Layer(double[] weights, int inputNeurons, int neurons, string activation = "relu")
    {
        Weights = weights;
        InputLength = inputNeurons;
        NeuronCount = neurons;
        Activation = activation;
    }

    public double[] LayerOutput(double[] inputs)
    {
        int k = 0;
        double[] outputs = new double[NeuronCount];
        for (int i = 0; i < NeuronCount; i++)
        {
            double neuronWeightedSum = 0;
            for (int j = 0; j < inputs.Length + 1; j++)
            {
                if (j == 0)
                {
                    neuronWeightedSum += Weights[k];
                }
                else
                {
                    neuronWeightedSum += Weights[k] * inputs[j-1];
                }

                k += 1;
            }
            if (Activation == "relu")
            {
                outputs[i] = Math.Max(0, neuronWeightedSum);
            }
            else if (Activation == "sigmoid")
            {
                outputs[i] = 1.0 / (1.0 + Math.Exp(-neuronWeightedSum));
            }
            else if (Activation == "tanh")
            {
                outputs[i] = System.Math.Tanh(neuronWeightedSum);
            }
            else if (Activation == "linear")
            {
                outputs[i] = neuronWeightedSum;
            }
        }
        return outputs;
    }
}
