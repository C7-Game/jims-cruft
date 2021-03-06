/*

no references as of commit 34ddd0efea77255cb8adc7aa6bc7b85acb01d1b1

This code was to generate a noise map using an OpenSimplexNoise generator
from https://gist.github.com/digitalshadow/134a3a02b67cecd72181 instead
of the one provided in Godot, so the map gen code doesn't depend on Godot.

Actually, now that I think of it, I'm leaving this in, but I'll keep it
here in cruft, too. It's a very useful noise generator able to wrap x, y,
both, or neither, and it can be used in many ways.

*/

using System;
using System.Collections.Generic;

class RemovedNoiseMapGenCode {
    // Inputs: noise field width and height, bool whether noise should smoothly wrap X or Y
    // Actual fake-isometric map will have different shape, but for noise we'll go straight 2d matrix
    // NOTE: Apparently this OpenSimplex implementation doesn't do octaves, including persistance or lacunarity
    //  Might be able to implement them, use https://www.youtube.com/watch?v=MRNFcywkUSA&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=4 as reference
    // TODO: Parameterize octaves, persistence, scale/period; compare this generator to Godot's
    // NOTE: Godot's OpenSimplexNoise returns -1 to 1; this one seems to be from 0 to 1 like most Simplex/Perlin implementations
    public static double[,] tempMapGenPrototyping(int width, int height, bool wrapX = true, bool wrapY = false)
    {
        // TODO: I think my octaves implementation is broken; specifically it needs normalizing I think as additional octaves drive more extreme values
        int octaves = 1;
        double persistence = 0.5;
        // The public domain OpenSiplex implementation always
        //   seems to be 0 at 0,0, so let's offset from it.
        double originOffset = 1000;
        double scale = 0.03;
        double xRadius = (double)width / (System.Math.PI * 2);
        double yRadius = (double)height / (System.Math.PI * 2);
        OpenSimplexNoise noise = new OpenSimplexNoise();
        double[,] noiseField = new double[width, height];

        for (int x=0; x < width; x++)
        {
            double oX = originOffset + (scale * x);
            // Set up cX,cY to make one circle as a function of x
            double theta = ((double)x / (double)width) * (System.Math.PI * 2);
            double cX = originOffset + (scale * xRadius * System.Math.Sin(theta));
            double cY = originOffset + (scale * xRadius * System.Math.Cos(theta));
            for (int y=0; y < height; y++)
            {
                double oY = originOffset + (scale * y);
                // Set up ycX,ycY to make one circle as a function of y
                double yTheta = ((double)y / (double)height) * (System.Math.PI * 2);
                double ycX = originOffset + (scale * yRadius * System.Math.Sin(yTheta));
                double ycY = originOffset + (scale * yRadius * System.Math.Cos(yTheta));

                // No wrapping, just yoink values at scaled coordinates
                if (!(wrapX || wrapY))
                {
                    // noiseField[x,y] = noise.Evaluate(oX, oY);
                    for (int i=0;i<octaves;i++)
                    {
                        double offset = i * 1.5 * System.Math.Max(width, height) * scale;
                        noiseField[x,y] += (octaves - i) * persistence * noise.Evaluate(oX + offset, oY + offset);
                    }
                    continue;
                }
                // Bi-axis wrapping requires two extra dimensions and circling through each
                if (wrapX && wrapY)
                {
                    for (int i=0;i<octaves;i++)
                    {
                        double offset = i * 1.5 * System.Math.Max(width, height) * scale;
                        double a = cX + offset;
                        double b = cY + offset;
                        double c = ycX + offset;
                        double d = ycY + offset;
                        noiseField[x,y] += (octaves - i) * persistence * noise.Evaluate(a, b, c, d);
                    }
                    // Skip the below tests, go to next loop iteration
                    continue;
                }
                // Y wrapping as Y increments it instead traces a circle in a third dimension to match up its ends
                if (wrapY)
                {
                    for (int i=0;i<octaves;i++)
                    {
                        double offset = i * 1.5 * System.Math.Max(width, height) * scale;
                        double a = ycX + offset;
                        double b = ycY + offset;
                        double c = oX + offset;
                        noiseField[x,y] += (octaves - i) * persistence * noise.Evaluate(a, b, c);
                    }
                    continue;
                }
                // Similar to Y wrapping
                if (wrapX)
                {
                    for (int i=0;i<octaves;i++)
                    {
                        double offset = i * 1.5 * System.Math.Max(width, height) * scale;
                        double a = cX + offset;
                        double b = cY + offset;
                        double c = oY + offset;
                        noiseField[x,y] += (octaves - i) * persistence * noise.Evaluate(a, b, c);
                    }
                }
            }
        }
        return noiseField;
    }
}