using System;
using Godot;

public static class JsonUtil
{
    public static string EncodeVector(Vector2 vec)
    {
        return $"{vec.X},{vec.Y}";
    }

    public static Vector2 DecodeVector(string vec)
    {
        string[] split = vec.Split(",");
        if (split.Length != 2 || !float.TryParse(split[0], out float x) || !float.TryParse(split[1], out float y)) return Vector2.Zero;
        return new Vector2(x, y);
    }
}
