using System;

namespace Ark.Mathematics.Tests
{
    /// <summary>
    /// 種を固定した乱数
    /// </summary>
    /// <remarks>
    /// テストが落ちたとき、必ず同じ入力で再現できるようにするため固定しています。
    /// </remarks>
    internal sealed class Rng
    {
        private uint _State;

        public Rng(uint seed)
        {
            _State = seed == 0u ? 1u : seed;
        }

        /// <summary>
        /// xorshift32
        /// </summary>
        public uint NextUInt()
        {
            _State ^= _State << 13;
            _State ^= _State >> 17;
            _State ^= _State << 5;

            return _State;
        }

        public float NextFloat(float min, float max)
        {
            return min + (max - min) * (NextUInt() / (float)uint.MaxValue);
        }

        public Vec3 NextVec3(float range = 8f)
        {
            return new Vec3(NextFloat(-range, range), NextFloat(-range, range), NextFloat(-range, range));
        }

        /// <summary>
        /// 長さが十分にあるベクトル。零ベクトル近辺で誤差が暴れるのを避けます
        /// </summary>
        public Vec3 NextNonDegenerateVec3()
        {
            for (var i = 0; i < 32; i++)
            {
                var v = NextVec3();

                if (v.Length() > 1f)
                {
                    return v;
                }
            }

            return Vec3.UnitX;
        }
    }

    /// <summary>
    /// 浮動小数点の比較
    /// </summary>
    internal static class Approx
    {
        public const float Epsilon = 1e-4f;

        /// <summary>
        /// 絶対誤差と相対誤差の両方で見ます
        /// </summary>
        /// <remarks>
        /// 絶対誤差だけだと大きな値に厳しすぎ、相対誤差だけだと0付近で破綻します。
        /// </remarks>
        public static bool Close(float a, float b, float epsilon = Epsilon)
        {
            var difference = MathF.Abs(a - b);

            if (difference <= epsilon)
            {
                return true;
            }

            return difference <= epsilon * MathF.Max(MathF.Abs(a), MathF.Abs(b));
        }

        public static bool Same(Vec3 a, Vec3 b, float epsilon = Epsilon)
        {
            return Close(a.X, b.X, epsilon) && Close(a.Y, b.Y, epsilon) && Close(a.Z, b.Z, epsilon);
        }

        /// <summary>
        /// 2つのベクトルがなす角の余弦。直交判定に使います
        /// </summary>
        public static float Cosine(Vec3 a, Vec3 b)
        {
            var denominator = a.Length() * b.Length();

            return denominator > 1e-6f ? Vec3.Dot(a, b) / denominator : 0f;
        }
    }
}
