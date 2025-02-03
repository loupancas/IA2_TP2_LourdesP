using System.Collections.Generic;

public static class CommonUtils
{
    public static IEnumerable<T> CreatePath<T>(Dictionary<T, T> parents, T end)
    {
        var path = new List<T> { end };
        while (parents.ContainsKey(end))
        {
            end = parents[end];
            path.Add(end);
        }
        path.Reverse();
        return path;
    }
}
