using System;

namespace RocketPlus.Utils
{
    public static class ExtensionMethod
    {
        public static float NextFloat(this Random random, float start, float end)
        {
            return (float)(random.NextDouble() * (end - start) + start);
        }
    }
}
