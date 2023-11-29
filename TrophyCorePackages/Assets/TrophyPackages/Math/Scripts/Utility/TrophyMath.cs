using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static  class TrophyMath 
{
    public const int INT_ONE = 1;
    public const int INT_TWO = 2;
    public const int INT_THREE = 3;
    public const int INT_FOUR = 4;
    public const int INT_FIVE = 5;

    public const float FLOAT_ONE_AND_A_HALF = 1.5f;
    public const float FLOAT_ONE = 1f;
    public const float FLOAT_FOURFIFTH = 0.8f;
    public const float FLOAT_HALF = 0.5f;
    public const float FLOAT_THIRD = 0.334f;
    public const float FLOAT_TWOTHIRDS = 0.667f;
    public const float FLOAT_FOURTH = 0.25f;
    public const float FLOAT_FIFTH = 0.2f;
    public const float FLOAT_TWOFIFTH = 0.4f;
    public const float FLOAT_THREEFIFTH = 0.6f;
    public const float FLOAT_SIXTH = 0.1667f;
    public const float FLOAT_SEVENTH = 0.1429f;
    public const float FLOAT_EIGHTH = 0.125f;
    public const float FLOAT_NINTH = 0.111f;
    public const float FLOAT_TENTH = 0.1f;
    public const float FLOAT_HUNDRETH = 0.01f;



    public static Vector2[] ToVector2Array(this Vector3[] v3)
    {
        return System.Array.ConvertAll<Vector3, Vector2>(v3, Vector3ToVector2);
    }
    public static Vector2[] ToVector2Array(this List<Vector3> v3)
    {
        return System.Array.ConvertAll<Vector3, Vector2>(v3.ToArray(), Vector3ToVector2);
    }

    public static List<Vector2> ToVector2List(this List<Vector3> v3)
    {
        return System.Array.ConvertAll<Vector3, Vector2>(v3.ToArray(), Vector3ToVector2).ToList();
    }
    public static List<Vector2> ToVector2List(this Vector3[] v3)
    {
        return System.Array.ConvertAll<Vector3, Vector2>(v3, Vector3ToVector2).ToList();
    }

    public static Vector3[] ToVector3Array(this Vector2[] v2)
    {
        return System.Array.ConvertAll<Vector2, Vector3>(v2, Vector2ToVector3);
    }
    public static Vector3[] ToVector3Array(this List<Vector2> v2)
    {
        return System.Array.ConvertAll<Vector2, Vector3>(v2.ToArray(), Vector2ToVector3);
    }

    public static List<Vector3> ToVector3List(this Vector2[] v2)
    {
        return System.Array.ConvertAll<Vector2, Vector3>(v2, Vector2ToVector3).ToList();
    }
    public static List<Vector3> ToVector3List(this List<Vector2> v2)
    {
        return System.Array.ConvertAll<Vector2, Vector3>(v2.ToArray(), Vector2ToVector3).ToList();
    }

    public static Vector2 Vector3ToVector2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }

    public static Vector3 Vector2ToVector3(Vector2 v2)
    {
        return new Vector3(v2.x, v2.y);
    }
    public static Vector3 Vector2ToVector3(Vector2 v2, float z)
    {
        return new Vector3(v2.x, v2.y, z);
    }

    public static Vector2 ScaleRelativeToScreenSize(Vector2 baseVector)
    {
        Vector2 targetSize = new Vector2(1000, 1000);
        var factor = baseVector / targetSize;
        return factor * Screen.safeArea.size;
    }

    public static T[,] Transpose<T>(this T[,] matrix, int xN, int yN)
    {
        T[,] product = new T[xN, yN];
        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                product[i, yN - j - 1] = matrix[i, j];
            }
        }
        return product;
    }

    public static Texture2D Flipped(this Texture2D texture)
    {
        Texture2D flipped = new Texture2D(texture.width, texture.height);

        int xN = texture.width;
        int yN = texture.height;

        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(i, yN - j - 1, texture.GetPixel(i, j));
            }
        }

        flipped.Apply();

        return flipped;
    }
}
