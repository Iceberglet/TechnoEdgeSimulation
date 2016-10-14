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
    double next();
}

public class Normal : IntervalGenerator
{
    private double mean;
    private double sd;
    private bool positive;
    private Random rand = new Random();

    public Normal(double m, double sd, bool positive = true)
    {
        this.mean = m;
        this.sd = sd;
        this.positive = positive;
    }

    public double next()
    {
        double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
        double u2 = rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal =
                     mean + sd * randStdNormal; //random normal(mean,stdDev^2)
        return positive ? Math.Max(randNormal, 0) : randNormal;
    }
}

public class Exp : IntervalGenerator
{
    private double lambda;
    private Random rand = new Random();
    //private RandomNumberGenerator rand = RandomNumberGenerator.Create();

    public Exp(float l)
    {
        this.lambda = l;
    }

    public double next()
    {
        if (lambda <= 0)
            throw new Exception("Invalid input for exp distribution: lambda = " + lambda);
        double p = rand.NextDouble();
        /*
        var bytes = new Byte[8];
        rand.GetBytes(bytes);
        // Step 2: bit-shift 11 and 53 based on double's mantissa bits
        var ul = BitConverter.ToUInt64(bytes, 0) / (1 << 11);
        Double p = ul / (Double)(1UL << 53);
        */
        return Math.Log(1 - p) / (-lambda);
    }
}

public class Uniform : IntervalGenerator
{
    private double l;
    private double r;
    private Random rand = new Random();

    public Uniform(double l, double r)
    {
        this.l = l;
        this.r = r;
    }

    public double next()
    {
        if (l > r)
            throw new Exception("Invalid input for uniform distribution, l > r");
        return rand.NextDouble() * (r - l) + l;
    }
}

public class StudentEntry : IntervalGenerator
{
    public double next()
    {
        //TODO: Possibly get from a file
        return 100;
    }
}

