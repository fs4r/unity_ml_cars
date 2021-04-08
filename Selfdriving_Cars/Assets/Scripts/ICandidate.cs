using System;
using UnityEngine;

interface ICandidate
{
    bool Dead { get; set; }
    double[] Gene { get; set; }
    float EvaluateFitness();
    void InitCandidate(double[] gene);
    GameObject getGameObject();
}
