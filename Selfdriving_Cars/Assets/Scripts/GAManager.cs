using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public struct FitnessRank
{
    public float Min;
    public float Max;
}
public class GAManager : MonoBehaviour
{
    public UnityEvent episode_ended;
    public int PopulationSize = 80;
    public GameObject popPrefab;
    Dictionary<ICandidate, float> Population;
    float passedTime = 0;
    int selectivePressure = 10;
    bool started = false;
    // Use this for initialization
    void Start()
    {

    }

    public void StartAlgorithm()
    {
        Debug.Log("Starting GA");
        started = true;
        Population = new Dictionary<ICandidate, float>();
        for (int i = 0; i < PopulationSize; i++)
        {
            GameObject candidate = Instantiate(popPrefab);
            ICandidate candidateCom = candidate.GetComponent<ICandidate>();
            candidateCom.InitCandidate(null);
            Population[candidateCom] = 0;
        }
    }

    public void NewEpisode()
    {
        newGeneration();
        passedTime = 0;
        started = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!started)
        {
            return;
        }

        passedTime += Time.deltaTime;
        if (passedTime > 30)
        {
            started = false;
            episode_ended.Invoke();
        }
        else
        {
            foreach (ICandidate pop in Population.Keys)
            {
                if (pop.EvaluateFitness() >= 16)
                {
                    started = false;
                    episode_ended.Invoke();
                }
            }
            foreach (ICandidate pop in Population.Keys)
            {
                if (!pop.Dead)
                {
                    return;
                }
            }
            started = false;
            episode_ended.Invoke();
        }
    }

    ICandidate sampleParent()
    {
        List<ICandidate> candidates = new List<ICandidate>(Population.Keys);
        int x = Random.Range(0, candidates.Count);
        ICandidate best = candidates[x];
        float bestScore = Population[candidates[x]];
        for (int i=0; i<selectivePressure; i++)
        {
            x = Random.Range(0, candidates.Count);
            if(Population[candidates[x]] > bestScore) {
                best = candidates[x];
                bestScore = Population[candidates[x]];
            }
        }
        return best;
        throw new System.Exception("Found no suitable parent!");
    }

    ICandidate spawnChild(double[] gene)
    {
        GameObject candidate = Instantiate(popPrefab);
        ICandidate child = candidate.GetComponent<ICandidate>();
        child.InitCandidate(gene);

        return child;
    }
    void printGene(double[] gene)
    {
        string a = "";
        foreach(double x in gene)
        {
            a = a + x + " ";
        }
        Debug.Log(a);
    }
    void newGeneration()
    {
        List<ICandidate> candidates = new List<ICandidate>(Population.Keys);
        ICandidate best = candidates[0];
        float bestFitness = 0;
        foreach(ICandidate candidate in candidates)
        {
            float fitness = candidate.EvaluateFitness();
            if (fitness > bestFitness) {
                bestFitness = fitness;
                best = candidate;
            }
            if (System.Math.Abs(fitness) < 0.1f)
            {
                fitness = 0.1f;
            }
            Population[candidate] = fitness;
        }

        Dictionary<ICandidate, float> newPopulation = new Dictionary<ICandidate, float>();
        best = spawnChild(best.Gene);
        best.getGameObject().GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
        newPopulation[best] = 0;
        Debug.Log(bestFitness);
        while (newPopulation.Count < PopulationSize)
        {
            ICandidate parent1 = sampleParent();
            //ICandidate parent2 = sampleParent(currentFitness);

            //double[] gene = crossover(parent1.Gene, parent2.Gene);
            double[] gene = mutate(parent1.Gene);
            
            newPopulation[spawnChild(gene)] = 0;
        }

        for(int i= candidates.Count - 1; i >= 0; i--)
        {
            ICandidate toDelete = candidates[i];
            Population.Remove(toDelete);
            candidates.Remove(toDelete);
            Destroy(toDelete.getGameObject());
        }
        Population = newPopulation;

    }

    double[] crossover(double[] parent1, double[] parent2)
    {
        double[] gene = new double[parent1.Length];
        for(int i=0; i < parent1.Length; i++)
        {
            int x = Random.Range(0, 2);
            if(x == 0)
            {
                gene[i] = parent1[i];
            }
            else
            {
                gene[i] = parent2[i];
            }
        }
        return gene;
    }

    double randomMutation(double x)
    {
        float op = Random.Range(0, 5);

        switch (op)
        {
            case 0:
                x = Random.Range(-1.0f, 1.0f);
                break;
            case 1:
                x = x + Random.Range(-0.25f, 0.25f);
                break;
            case 2:
                x = x * Random.Range(0.5f, 1.5f);
                break;
            case 3:
                x = -x;
                break;
            case 4:
                x = 0;
                break;
            default:
                x = x + Random.Range(-0.25f, 0.25f);
                break;
        }
        return x;
    }
    double[] mutate(double[] gene)
    {
        float mutateProb = 0.1f;
        double[] newGene = new double[gene.Length];
        for (int i = 0; i < gene.Length; i++)
        {
            float x = Random.Range(0.0f, 1.0f);
            if(x<mutateProb)
            {
                newGene[i] = randomMutation(gene[i]);
            }
            else
            {
                newGene[i] = gene[i];
            }
        }
        return newGene;
    }
}
