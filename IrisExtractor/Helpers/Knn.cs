using System.Collections.Generic;
using System.Linq;

namespace ImageEditor.Helpers
{
    public static class Knn
    {
        public static List<string> ClassifyVector(IEnumerable<(string group, byte[] vec)> trainSample, IEnumerable<byte> vector, int k)
        {
            var distances=new List<(string group,double dist)>();
            foreach (var sample in trainSample)
            {
                var distance = GetEuclideanDistance(sample.vec, vector.ToList());
                distances.Add((sample.group,distance));
            }

            distances = distances.OrderBy(d => d.dist).Select(d => (d.group,d.dist)).Take(k).ToList();
            var bestMatches = distances
                .GroupBy(d => d.group, d => d.dist, (key, count) => new {Group = key, Count = count.Count()})
                .OrderByDescending(d => d.Count).ToList();
            var max = bestMatches.Max(m => m.Count);
            return bestMatches.Where(b=>b.Count==max).Select(b=>b.Group).ToList();
        }
       
        static double GetEuclideanDistance(IList<byte> sample1, IList<byte> sample2)
        {
            var distance = 0.0;
            // assume sample1 and sample2 are valid i.e. same length 

            for (var i = 0; i < sample1.Count; i++)
            {
                var temp = sample1[i] - sample2[i];
                distance += temp * temp;
            }
            return distance;
        }
    }
}