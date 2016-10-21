using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
//using UnityEngine;
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
    private Random rand = new Random();

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
    private Random rand = new Random();
    //private RandomNumberGenerator rand = RandomNumberGenerator.Create();

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
    private Random rand = new Random();

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
    public float next()
    {
        //TODO: Possibly get from a file
        return 2;
    }
}

public class EatingTime : IntervalGenerator
{
    public float next()
    {
        //TODO: Possibly get from a file
        return 300;
    }
}

