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