using UnityEngine;
using UnityEditor;

public static class Easing
{
    public static class Funcs
    {
        public abstract class Base
        {
            public abstract float In(float ratio);

            public float Out(float ratio)
            {
                return 1.0f - In(1.0f - ratio);
            }

            public float InOut(float ratio)
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

        public class Sine : Base
        {
            public override float In(float ratio)
            {
                return 1.0f + -Mathf.Cos(ratio * (Mathf.PI * 0.5f));
            }
        }

        public class Quad : Base
        {
            public override float In(float ratio)
            {
                return ratio * ratio;
            }
        }

        public class Cubic : Base
        {
            public override float In(float ratio)
            {
                return ratio * ratio * ratio;
            }
        }

        public class Quart : Base
        {
            public override float In(float ratio)
            {
                return ratio * ratio * ratio * ratio;
            }
        }

        public class Quint : Base
        {
            public override float In(float ratio)
            {
                return ratio * ratio * ratio * ratio * ratio;
            }
        }
    }

    /// <summary>
    /// 最もゆるやか
    /// </summary>
    public static readonly Funcs.Base Sine = new Funcs.Sine();

    /// <summary>
    /// Sineの次にゆるやか
    /// </summary>
    public static readonly Funcs.Base Quad = new Funcs.Quad();

    /// <summary>
    /// Quadの次に急
    /// </summary>
    public static readonly Funcs.Base Cubic = new Funcs.Cubic();

    /// <summary>
    /// Cubicの次に急
    /// </summary>
    public static readonly Funcs.Base Quart = new Funcs.Quart();

    /// <summary>
    /// Quartの次に急
    /// </summary>
    public static readonly Funcs.Base Quint = new Funcs.Quint();
}