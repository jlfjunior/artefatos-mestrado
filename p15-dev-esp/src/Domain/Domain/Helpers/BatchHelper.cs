﻿public static class BatchHelper
{
    public static IEnumerable<List<T>> SplitIntoBatches<T>(List<T> source, int batchSize)
    {
        for (int i = 0; i < source.Count; i += batchSize)
        {
            yield return source.Skip(i).Take(batchSize).ToList();
        }
    }
}
