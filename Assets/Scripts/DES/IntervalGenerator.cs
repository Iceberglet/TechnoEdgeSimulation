using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;
//using MathNet.Numerics;
//using MathNet.Numerics.Distributions;

//NOTE: Due to extenuating circumstances, all distributions will have to be written by hand :(
//Known Issues: Exp - bloated average and smaller variance

public interface IntervalGenerator
{
    float next();
}

public class Normal : IntervalGenerator
{
    private float mean;
    private float sd;
    private bool positive;
    private System.Random rand = new System.Random(GlobalConstants.RANDOM_SEED + 1);

    public Normal(float m, float sd, bool positive = true)
    {
        this.mean = m;
        this.sd = sd;
        this.positive = positive;
    }

    public float next()
    {
        float u1 = (float)rand.NextDouble(); //these are uniform(0,1) random  floats
        float u2 = (float)rand.NextDouble();
        float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2)); //random normal(0,1)
        float randNormal =
                     mean + sd * randStdNormal; //random normal(mean,stdDev^2)
        return positive ? Math.Max(randNormal, 0) : randNormal;
    }
}

public class Exp : IntervalGenerator
{
    private float lambda;
    private System.Random rand = new System.Random(GlobalConstants.RANDOM_SEED + 2);
    //private System.RandomNumberGenerator rand = System.RandomNumberGenerator.Create();

    public Exp(float l)
    {
        this.lambda = l;
    }

    public float next()
    {
        if (lambda <= 0)
            throw new Exception("Invalid input for exp distribution: lambda = " + lambda);
        float p = (float)rand.NextDouble();
        return (float)(Math.Log(1 - p) / (-lambda));
    }
}

public class Uniform : IntervalGenerator
{
    private float l;
    private float r;
    private System.Random rand = new System.Random(GlobalConstants.RANDOM_SEED + 3);

    public Uniform(float l, float r)
    {
        this.l = l;
        this.r = r;
    }

    public float next()
    {
        if (l > r)
            throw new Exception("Invalid input for uniform distribution, l > r");
        return (float)rand.NextDouble() * (r - l) + l;
    }
}

public class StudentEntry : IntervalGenerator
{
    int counter = 0;
    private System.Random rand = new System.Random(GlobalConstants.RANDOM_SEED + 4);

    public float next()
    {
        //TODO: Possibly get from a file
        /*
        if(counter >= 10)
        {
            return float.PositiveInfinity;
        }
        counter++;*/

        return 2;
    }
}

public class EatingTime : IntervalGenerator
{
    private System.Random rand = new System.Random(GlobalConstants.RANDOM_SEED + 5);

    public float next()
    {
        //TODO: Possibly get from a file
        return 500;
    }
}


public class GenericDistribution : IntervalGenerator
{
    private System.Random rand = new System.Random(GlobalConstants.RANDOM_SEED + 13);

    private float[] value;
    private float[] probabilities;
    private readonly float sumOfProba;

    public GenericDistribution(float[] values, float[] proba)
    {
        value = values;
        probabilities = proba;
        sumOfProba = proba.Sum();
    }

    //Read from file (csv format)
    //Format: each row must be the value, comma, frequencies
    //Frequencies need not add up to 1, but value must be increasing to make sense
    //e.g:
    //1, 0.2
    //2, 0.4
    //3, 0.8
    //4, 1.3
    public static GenericDistribution createInstanceFromFile(string fileName)
    {
        string path = Utility.Path + "/Input/" + fileName;

        String[] rows = File.ReadAllText(path).Split('\n');
        List<float> values = new List<float>();
        List<float> probas = new List<float>();

        for (int i = 0; i < rows.Length; i++)
        {
            String[] r = rows[i].Trim().Split(',');
            if(r != null && r.Length == 2)
            {
                values.Add(float.Parse(r[0]));
                probas.Add(float.Parse(r[1]));
            }
        }
        return new GenericDistribution(values.ToArray(), probas.ToArray());
    }


    public float next()
    {
        float toSample = (float)rand.NextDouble() * sumOfProba;
        int idx = -1;
        while(toSample > 0)
        {
            idx++;
            toSample -= probabilities[idx];
        }
        return idx == 0 ? 0 : value[idx - 1] + (float)rand.NextDouble()*(value[idx] - value[idx-1]);
    }
}

