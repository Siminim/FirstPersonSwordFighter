using Godot;
using System;

public static class Delegates
{
    public delegate void VoidDelegate();

    public delegate float FloatReturnDelegate();
    public delegate bool BoolReturnDelegate();

    public delegate void FloatParameterDelegate(float value);
    public delegate void DoubleParameterDelegate(double value);
    public delegate void Node3DParameterDelegate(Node3D node);
    public delegate void BoolParameterDelegate(bool value);

    public delegate void Vector2DoubleParameterDelegate(Vector2 direction, double delta);
    public delegate void MouseMotionDelegate(InputEventMouseMotion mouseMotion);
}

public struct FloatReturnDelegateModifiers
{
    public Delegates.FloatReturnDelegate Additive;
    public Delegates.FloatReturnDelegate Subtractive;
    public Delegates.FloatReturnDelegate Multiplier;
    public Delegates.FloatReturnDelegate Divider;

    private float GetAddedTotal(Delegates.FloatReturnDelegate floatReturnDelegate, float failsafe = 1.0f)
    {
        if (floatReturnDelegate == null || floatReturnDelegate.GetInvocationList().Length == 0)
            return failsafe;

        float total = 0.0f;

        foreach (Delegates.FloatReturnDelegate floatDelegate in floatReturnDelegate.GetInvocationList())
        {
            total += floatDelegate.Invoke();
        }

        return total;
    }

    public float GetFinal()
    {
        return (GetAddedTotal(Additive) - GetAddedTotal(Subtractive)) * GetAddedTotal(Multiplier) / GetAddedTotal(Divider);
    }

    public float GetSafeFinalModifier()
    {
        float add = GetAddedTotal(Additive, 0.0f);
        float sub = GetAddedTotal(Subtractive, 0.0f);

        // Negative numbers here can cause issues
        float sum = Mathf.Max(0.0f, add - sub);

        // Make sure that the final value is at least 1
        float mul = Mathf.Max(1.0f, GetAddedTotal(Multiplier));
        float div = Mathf.Max(1.0f, GetAddedTotal(Divider));

        return sum * mul / div;
    }
}