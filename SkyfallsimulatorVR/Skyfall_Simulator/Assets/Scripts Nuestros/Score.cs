using UnityEngine;
using System;
public static class Score
{
    public static event Action<int> OnChanged;
    static int _value;
    public static int Value => _value;

    public static void Add(int v) { _value += v; OnChanged?.Invoke(_value); }
    public static void Reset() { _value = 0; OnChanged?.Invoke(_value); }
}