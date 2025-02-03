using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<WeightedNode<T>> _elements = new List<WeightedNode<T>>();

    public int Count => _elements.Count;
    public bool IsEmpty => _elements.Count == 0;

    public void Enqueue(WeightedNode<T> item)
    {
        _elements.Add(item);
        _elements.Sort((x, y) => x.Weight.CompareTo(y.Weight));
    }

    public WeightedNode<T> Dequeue()
    {
        var item = _elements[0];
        _elements.RemoveAt(0);
        return item;
    }
}
