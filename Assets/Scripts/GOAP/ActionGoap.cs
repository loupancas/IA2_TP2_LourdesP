using System;
using System.Collections.Generic;

public class ActionGoap
{
    public string Name { get; private set; }
    public float Cost { get; private set; }
    public Func<WorldState, bool> Precondition { get; private set; }
    public Action<WorldState> Effect { get; private set; }

    public ActionGoap(string name, float cost, Func<WorldState, bool> precondition, Action<WorldState> effect)
    {
        Name = name;
        Cost = cost;
        Precondition = precondition;
        Effect = effect;
    }

    public bool CanExecute(WorldState state) => Precondition(state);
    public void ApplyEffect(WorldState state) => Effect(state);
}
