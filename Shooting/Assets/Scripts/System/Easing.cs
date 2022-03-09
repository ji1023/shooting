using UnityEngine;
using UnityEditor;

public static class Easing
{
    public static float Linear(float ratio)
    {
        return ratio;
    }

    public static class Quad
    {
        public static float In(float ratio)
        {
            return ratio * ratio;
        }

        public static float Out(float ratio)
        {
            return 1.0f - In(1.0f - ratio);
        }

        public static float InOut(float ratio)
        {
            if (ratio < 0.5f)
            {
                return In(ratio * 2.0f);
            }
            else
            {
                return Out((ratio - 0.5f) * 2.0f);
            }
        }
    }
}