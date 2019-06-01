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
       
        // vector1 and vector2 must have the same length
        private static double GetEuclideanDistance(IList<byte> vector1, IList<byte> vector2)
        {
            var distance = 0.0;          
            for (var i = 0; i < vector1.Count; i++)
            {
                var temp = vector1[i] - vector2[i];
                distance += temp * temp;
            }
            return distance;
        }
    }
}