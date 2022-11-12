using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static int[] GetTriangles(int verticeCount)
    {
        int tl = verticeCount - 4;
        return new int[] { tl, tl + 1, tl + 2, tl + 2, tl + 1, tl + 3 };
    }

    public static Vector3[] topFaces = new Vector3[4] {
        new Vector3(0, 1, 0),
        new Vector3(0, 1, 1),
        new Vector3(1, 1, 0),
        new Vector3(1, 1, 1)
    };

    public static Vector3[] bottomFaces = new Vector3[4] {
        new Vector3(0, 0, 1),
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 1),
        new Vector3(1, 0, 0)
    };

    public static Vector3[] leftFaces = new Vector3[4] {
        new Vector3(0, 0, 1),
        new Vector3(0, 1, 1),
        new Vector3(0, 0, 0),
        new Vector3(0, 1, 0)
    };

    public static Vector3[] rightFaces = new Vector3[4] {
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1)
    };

    public static Vector3[] backFaces = new Vector3[4] {
        new Vector3(0, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0)
    };

    public static Vector3[] frontFaces = new Vector3[4] {
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 0, 1),
        new Vector3(0, 1, 1)
    };
}
