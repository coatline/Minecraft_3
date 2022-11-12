using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Getter<T>
{
    Dictionary<string, T> dict;
    T[] ts;

    public int Length { get; private set; }

    public Getter(T[] ts)
    {
        dict = new Dictionary<string, T>();

        this.ts = ts;
        Length = ts.Length;

        for (int i = 0; i < ts.Length; i++)
        {
            if (ts[i] == null) { Debug.Log("Null Data!"); continue; }
            dict.Add((ts[i] as Object).name, ts[i]);
        }
    }

    public T this[string n]
    {
        get
        {
            dict.TryGetValue(n, out T j);
            if (j == null) { Debug.LogError($"Couldn't get {n}"); }
            return j;
        }
    }

    public T this[int n]
    {
        get
        {
            return ts[n];
        }
    }
}
