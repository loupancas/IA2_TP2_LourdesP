using System;

[Serializable]
public class WeightedNode<T>
{
    public T Element { get; }
    public float Weight { get; }

    public WeightedNode(T element, float weight)
    {
        Element = element;
        Weight = weight;
    }
}
