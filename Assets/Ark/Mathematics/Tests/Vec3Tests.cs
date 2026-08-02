using System;
using NUnit.Framework;

namespace Ark.Mathematics.Tests
{
    // ここに書いてあるのは「どう実装するか」ではなく「正しい実装なら必ず成り立つ事」です。
    // 全部緑になれば、Vec3 は数学的に正しいと言えます。

    internal static class Samples
    {
        public const int Count = 200;
        public const float Pi = 3.14159265f;
    }

    [TestFixture]
    internal sealed class Vec3加法
    {
        [Test]
        public void 交換法則()
        {
            var rng = new Rng(1);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Same(a + b, b + a), Is.True, $"a + b = b + a に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 結合法則()
        {
            var rng = new Rng(2);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3(), c = rng.NextVec3();

                Assert.That(Approx.Same((a + b) + c, a + (b + c)), Is.True,
                    $"(a + b) + c = a + (b + c) に反しました。a={a}, b={b}, c={c}");
            }
        }

        [Test]
        public void 零元を足しても変わらない()
        {
            var rng = new Rng(3);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();

                Assert.That(Approx.Same(a + Vec3.Zero, a), Is.True, $"a + 0 = a に反しました。a={a}");
            }
        }

        [Test]
        public void 逆元を足すと零になる()
        {
            var rng = new Rng(4);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();

                Assert.That(Approx.Same(a + -a, Vec3.Zero), Is.True, $"a + (-a) = 0 に反しました。a={a}");
            }
        }

        [Test]
        public void 減算は逆元の加算と同じ()
        {
            var rng = new Rng(5);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Same(a - b, a + -b), Is.True, $"a - b = a + (-b) に反しました。a={a}, b={b}");
            }
        }
    }

    [TestFixture]
    internal sealed class Vec3スカラー倍
    {
        [Test]
        public void ベクトルの和に分配される()
        {
            var rng = new Rng(6);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();
                var s = rng.NextFloat(-5f, 5f);

                Assert.That(Approx.Same((a + b) * s, a * s + b * s), Is.True,
                    $"(a + b)s = as + bs に反しました。a={a}, b={b}, s={s}");
            }
        }

        [Test]
        public void スカラーの和に分配される()
        {
            var rng = new Rng(7);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                float s = rng.NextFloat(-5f, 5f), t = rng.NextFloat(-5f, 5f);

                Assert.That(Approx.Same(a * (s + t), a * s + a * t), Is.True,
                    $"a(s + t) = as + at に反しました。a={a}, s={s}, t={t}");
            }
        }

        [Test]
        public void 結合法則()
        {
            var rng = new Rng(8);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                float s = rng.NextFloat(-5f, 5f), t = rng.NextFloat(-5f, 5f);

                Assert.That(Approx.Same(a * s * t, a * (s * t)), Is.True,
                    $"(as)t = a(st) に反しました。a={a}, s={s}, t={t}");
            }
        }

        [Test]
        public void 左右どちらから掛けても同じ()
        {
            var rng = new Rng(9);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                var s = rng.NextFloat(-5f, 5f);

                Assert.That(Approx.Same(a * s, s * a), Is.True, $"as = sa に反しました。a={a}, s={s}");
            }
        }

        [Test]
        public void 除算は逆数倍と同じ()
        {
            var rng = new Rng(10);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                var s = rng.NextFloat(1f, 5f);

                Assert.That(Approx.Same(a / s, a * (1f / s)), Is.True, $"a / s = a(1/s) に反しました。a={a}, s={s}");
            }
        }

        [Test]
        public void 一倍しても変わらない()
        {
            var a = new Vec3(3f, -4f, 5f);

            Assert.That(Approx.Same(a * 1f, a), Is.True, $"a * 1 = a に反しました。a={a}");
        }
    }

    [TestFixture]
    internal sealed class Vec3成分の差し替え
    {
        [Test]
        public void 指定した成分が置き換わる()
        {
            var rng = new Rng(49);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                var v = rng.NextFloat(-8f, 8f);

                Assert.That(a.WithX(v).X, Is.EqualTo(v).Within(1e-5f), $"WithX で X が置き換わっていません。a={a}, v={v}");
                Assert.That(a.WithY(v).Y, Is.EqualTo(v).Within(1e-5f), $"WithY で Y が置き換わっていません。a={a}, v={v}");
                Assert.That(a.WithZ(v).Z, Is.EqualTo(v).Within(1e-5f), $"WithZ で Z が置き換わっていません。a={a}, v={v}");
            }
        }

        [Test]
        public void 他の成分は変わらない()
        {
            var rng = new Rng(50);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                var v = rng.NextFloat(-8f, 8f);

                var withX = a.WithX(v);
                Assert.That(withX.Y, Is.EqualTo(a.Y).Within(1e-5f), $"WithX が Y を壊しました。a={a}, v={v}");
                Assert.That(withX.Z, Is.EqualTo(a.Z).Within(1e-5f), $"WithX が Z を壊しました。a={a}, v={v}");

                var withY = a.WithY(v);
                Assert.That(withY.X, Is.EqualTo(a.X).Within(1e-5f), $"WithY が X を壊しました。a={a}, v={v}");
                Assert.That(withY.Z, Is.EqualTo(a.Z).Within(1e-5f), $"WithY が Z を壊しました。a={a}, v={v}");

                var withZ = a.WithZ(v);
                Assert.That(withZ.X, Is.EqualTo(a.X).Within(1e-5f), $"WithZ が X を壊しました。a={a}, v={v}");
                Assert.That(withZ.Y, Is.EqualTo(a.Y).Within(1e-5f), $"WithZ が Y を壊しました。a={a}, v={v}");
            }
        }

        [Test]
        public void 同じ値を入れると元のまま()
        {
            var rng = new Rng(51);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();

                Assert.That(Approx.Same(a.WithX(a.X), a), Is.True, $"WithX(a.X) は a のままです。a={a}");
                Assert.That(Approx.Same(a.WithY(a.Y), a), Is.True, $"WithY(a.Y) は a のままです。a={a}");
                Assert.That(Approx.Same(a.WithZ(a.Z), a), Is.True, $"WithZ(a.Z) は a のままです。a={a}");
            }
        }

        [Test]
        public void 後から入れた値が残る()
        {
            var rng = new Rng(52);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                float p = rng.NextFloat(-8f, 8f), q = rng.NextFloat(-8f, 8f);

                Assert.That(Approx.Same(a.WithX(p).WithX(q), a.WithX(q)), Is.True,
                    $"WithX を重ねたら後の値が残るはずです。a={a}, p={p}, q={q}");
            }
        }

        [Test]
        public void 三つ繋げると新規生成と同じ()
        {
            var rng = new Rng(53);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                float x = rng.NextFloat(-8f, 8f), y = rng.NextFloat(-8f, 8f), z = rng.NextFloat(-8f, 8f);

                Assert.That(Approx.Same(a.WithX(x).WithY(y).WithZ(z), new Vec3(x, y, z)), Is.True,
                    $"全成分を差し替えたら new Vec3(x,y,z) と同じになるはずです。a={a}");
            }
        }

        [Test]
        public void 元のベクトルは変化しない()
        {
            var a = new Vec3(1f, 2f, 3f);

            a.WithX(99f);
            a.WithY(99f);
            a.WithZ(99f);

            Assert.That(Approx.Same(a, new Vec3(1f, 2f, 3f)), Is.True,
                $"With* は新しい値を返すだけで、元を書き換えてはいけません。a={a}");
        }
    }

    [TestFixture]
    internal sealed class Vec3長さと正規化
    {
        [Test]
        public void 既知の長さ()
        {
            Assert.That(new Vec3(3f, 4f, 0f).Length(), Is.EqualTo(5f).Within(1e-4f), "|(3,4,0)| は 5 です");
            Assert.That(new Vec3(2f, 3f, 6f).Length(), Is.EqualTo(7f).Within(1e-4f), "|(2,3,6)| は 7 です");
            Assert.That(Vec3.Zero.Length(), Is.EqualTo(0f).Within(1e-4f), "零ベクトルの長さは 0 です");
        }

        [Test]
        public void 単位ベクトルの長さは1()
        {
            Assert.That(Vec3.UnitX.Length(), Is.EqualTo(1f).Within(1e-4f), "UnitX");
            Assert.That(Vec3.UnitY.Length(), Is.EqualTo(1f).Within(1e-4f), "UnitY");
            Assert.That(Vec3.UnitZ.Length(), Is.EqualTo(1f).Within(1e-4f), "UnitZ");
        }

        [Test]
        public void SqrLengthは長さの2乗()
        {
            var rng = new Rng(11);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();

                Assert.That(Approx.Close(a.SqrLength(), a.Length() * a.Length(), 1e-3f), Is.True,
                    $"SqrLength = Length² に反しました。a={a}, SqrLength={a.SqrLength()}, Length²={a.Length() * a.Length()}");
            }
        }

        [Test]
        public void 正規化すると長さが1になる()
        {
            var rng = new Rng(12);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Close(a.Normalize().Length(), 1f), Is.True,
                    $"|Normalized| = 1 に反しました。a={a}, |Normalized|={a.Normalize().Length()}");
            }
        }

        [Test]
        public void 正規化しても向きは変わらない()
        {
            var rng = new Rng(13);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Same(a.Normalize() * a.Length(), a, 1e-3f), Is.True,
                    $"Normalized * Length = a に反しました。a={a}");
            }
        }

        [Test]
        public void 正のスカラー倍で長さも同じ倍率になる()
        {
            var rng = new Rng(14);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                var s = rng.NextFloat(0.5f, 5f);

                Assert.That(Approx.Close((a * s).Length(), a.Length() * s, 1e-3f), Is.True,
                    $"|as| = |a|s に反しました。a={a}, s={s}");
            }
        }

        [Test]
        public void 三角不等式()
        {
            var rng = new Rng(15);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That((a + b).Length(), Is.LessThanOrEqualTo(a.Length() + b.Length() + 1e-3f),
                    $"|a + b| ≦ |a| + |b| に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 零ベクトルの正規化でNaNにならない()
        {
            var normalized = Vec3.Zero.Normalize();

            Assert.That(float.IsFinite(normalized.X) && float.IsFinite(normalized.Y) && float.IsFinite(normalized.Z),
                Is.True, $"零ベクトルを正規化した結果が {normalized} になりました。0除算を防いでください");
        }
    }

    [TestFixture]
    internal sealed class Vec3内積
    {
        [Test]
        public void 直交する軸の内積は0()
        {
            Assert.That(Vec3.Dot(Vec3.UnitX, Vec3.UnitY), Is.EqualTo(0f).Within(1e-5f), "UnitX·UnitY");
            Assert.That(Vec3.Dot(Vec3.UnitY, Vec3.UnitZ), Is.EqualTo(0f).Within(1e-5f), "UnitY·UnitZ");
            Assert.That(Vec3.Dot(Vec3.UnitZ, Vec3.UnitX), Is.EqualTo(0f).Within(1e-5f), "UnitZ·UnitX");
        }

        [Test]
        public void 同じ向きと逆向き()
        {
            Assert.That(Vec3.Dot(Vec3.UnitX, Vec3.UnitX), Is.EqualTo(1f).Within(1e-5f), "同方向は 1");
            Assert.That(Vec3.Dot(Vec3.UnitX, -Vec3.UnitX), Is.EqualTo(-1f).Within(1e-5f), "逆方向は -1");
        }

        [Test]
        public void 対称()
        {
            var rng = new Rng(16);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Close(Vec3.Dot(a, b), Vec3.Dot(b, a), 1e-3f), Is.True,
                    $"a·b = b·a に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 自己内積は長さの2乗()
        {
            var rng = new Rng(17);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();

                Assert.That(Approx.Close(Vec3.Dot(a, a), a.SqrLength(), 1e-3f), Is.True,
                    $"a·a = |a|² に反しました。a={a}");
            }
        }

        [Test]
        public void 加法に対して線形()
        {
            var rng = new Rng(18);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3(), c = rng.NextVec3();

                Assert.That(Approx.Close(Vec3.Dot(a + b, c), Vec3.Dot(a, c) + Vec3.Dot(b, c), 1e-3f), Is.True,
                    $"(a + b)·c = a·c + b·c に反しました。a={a}, b={b}, c={c}");
            }
        }

        [Test]
        public void スカラー倍に対して線形()
        {
            var rng = new Rng(19);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();
                var s = rng.NextFloat(-3f, 3f);

                Assert.That(Approx.Close(Vec3.Dot(a * s, b), Vec3.Dot(a, b) * s, 1e-3f), Is.True,
                    $"(as)·b = (a·b)s に反しました。a={a}, b={b}, s={s}");
            }
        }

        [Test]
        public void コーシーシュワルツの不等式()
        {
            var rng = new Rng(20);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(MathF.Abs(Vec3.Dot(a, b)), Is.LessThanOrEqualTo(a.Length() * b.Length() + 1e-2f),
                    $"|a·b| ≦ |a||b| に反しました。a={a}, b={b}");
            }
        }
    }

    [TestFixture]
    internal sealed class Vec3外積
    {
        [Test]
        public void 右手系である()
        {
            // ここが逆だと、以降の回転や法線がすべて鏡像になります
            Assert.That(Approx.Same(Vec3.Cross(Vec3.UnitX, Vec3.UnitY), Vec3.UnitZ), Is.True,
                $"UnitX × UnitY は UnitZ です。実際は {Vec3.Cross(Vec3.UnitX, Vec3.UnitY)}");
            Assert.That(Approx.Same(Vec3.Cross(Vec3.UnitY, Vec3.UnitZ), Vec3.UnitX), Is.True,
                $"UnitY × UnitZ は UnitX です。実際は {Vec3.Cross(Vec3.UnitY, Vec3.UnitZ)}");
            Assert.That(Approx.Same(Vec3.Cross(Vec3.UnitZ, Vec3.UnitX), Vec3.UnitY), Is.True,
                $"UnitZ × UnitX は UnitY です。実際は {Vec3.Cross(Vec3.UnitZ, Vec3.UnitX)}");
        }

        [Test]
        public void 自分自身との外積は零()
        {
            var a = new Vec3(1f, 2f, 3f);

            Assert.That(Approx.Same(Vec3.Cross(a, a), Vec3.Zero), Is.True, $"a × a = 0 に反しました。a={a}");
        }

        [Test]
        public void 反交換()
        {
            var rng = new Rng(21);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Same(Vec3.Cross(a, b), -Vec3.Cross(b, a), 1e-3f), Is.True,
                    $"a × b = -(b × a) に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 両方に垂直()
        {
            var rng = new Rng(22);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextNonDegenerateVec3(), b = rng.NextNonDegenerateVec3();
                var cross = Vec3.Cross(a, b);

                if (cross.Length() < 1f)
                {
                    continue;
                }

                Assert.That(Approx.Close(Approx.Cosine(cross, a), 0f, 1e-3f), Is.True,
                    $"(a × b) は a に垂直であるべきです。a={a}, b={b}, a×b={cross}");
                Assert.That(Approx.Close(Approx.Cosine(cross, b), 0f, 1e-3f), Is.True,
                    $"(a × b) は b に垂直であるべきです。a={a}, b={b}, a×b={cross}");
            }
        }

        [Test]
        public void ラグランジュの恒等式()
        {
            var rng = new Rng(23);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();
                var dot = Vec3.Dot(a, b);

                Assert.That(Approx.Close(Vec3.Cross(a, b).SqrLength(), a.SqrLength() * b.SqrLength() - dot * dot, 1e-2f),
                    Is.True, $"|a×b|² = |a|²|b|² − (a·b)² に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 加法に対して分配()
        {
            var rng = new Rng(24);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3(), c = rng.NextVec3();

                Assert.That(Approx.Same(Vec3.Cross(a + b, c), Vec3.Cross(a, c) + Vec3.Cross(b, c), 1e-2f), Is.True,
                    $"(a + b) × c = a×c + b×c に反しました。a={a}, b={b}, c={c}");
            }
        }

        [Test]
        public void 平行なベクトルの外積は零()
        {
            var rng = new Rng(25);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();
                var s = rng.NextFloat(-3f, 3f);

                Assert.That(Vec3.Cross(a, a * s).Length(),
                    Is.LessThanOrEqualTo(1e-2f * MathF.Max(a.SqrLength(), 1f)),
                    $"平行なベクトル同士の外積は零です。a={a}, s={s}");
            }
        }

        [Test]
        public void ヤコビ恒等式()
        {
            var rng = new Rng(26);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3(), c = rng.NextVec3();

                var sum = Vec3.Cross(a, Vec3.Cross(b, c))
                          + Vec3.Cross(b, Vec3.Cross(c, a))
                          + Vec3.Cross(c, Vec3.Cross(a, b));

                Assert.That(sum.Length(), Is.LessThanOrEqualTo(1e-2f * MathF.Max(a.Length() * b.Length() * c.Length(), 1f)),
                    $"a×(b×c) + b×(c×a) + c×(a×b) = 0 に反しました。a={a}, b={b}, c={c}");
            }
        }

        [Test]
        public void スカラー三重積は巡回対称()
        {
            var rng = new Rng(27);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3(), c = rng.NextVec3();

                Assert.That(Approx.Close(Vec3.Dot(a, Vec3.Cross(b, c)), Vec3.Dot(b, Vec3.Cross(c, a)), 1e-2f),
                    Is.True, $"a·(b×c) = b·(c×a) に反しました。a={a}, b={b}, c={c}");
            }
        }
    }

    [TestFixture]
    internal sealed class Vec3射影と直交分解
    {
        [Test]
        public void 射影と残差の和は元に戻る()
        {
            var rng = new Rng(28);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Same(Vec3.Project(a, n) + Vec3.Reject(a, n), a, 1e-2f), Is.True,
                    $"Project + Reject = a に反しました。a={a}, n={n}");
            }
        }

        [Test]
        public void 射影はnと平行()
        {
            var rng = new Rng(29);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();

                Assert.That(Vec3.Cross(Vec3.Project(a, n), n).Length(),
                    Is.LessThanOrEqualTo(1e-2f * MathF.Max(a.Length() * n.Length(), 1f)),
                    $"射影は n と平行であるべきです。a={a}, n={n}");
            }
        }

        [Test]
        public void 残差はnと垂直()
        {
            var rng = new Rng(30);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();
                var rejection = Vec3.Reject(a, n);

                if (rejection.Length() < 0.5f)
                {
                    continue;
                }

                Assert.That(Approx.Close(Approx.Cosine(rejection, n), 0f, 1e-3f), Is.True,
                    $"Reject は n と垂直であるべきです。a={a}, n={n}, Reject={rejection}");
            }
        }

        [Test]
        public void 射影は冪等()
        {
            var rng = new Rng(31);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();
                var projection = Vec3.Project(a, n);

                Assert.That(Approx.Same(Vec3.Project(projection, n), projection, 1e-2f), Is.True,
                    $"2回射影しても変わらないはずです。a={a}, n={n}");
            }
        }

        [Test]
        public void 射影は元より長くならない()
        {
            var rng = new Rng(32);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();

                Assert.That(Vec3.Project(a, n).Length(), Is.LessThanOrEqualTo(a.Length() + 1e-2f),
                    $"|Project(a,n)| ≦ |a| に反しました。a={a}, n={n}");
            }
        }

        [Test]
        public void 軸への射影()
        {
            var projection = Vec3.Project(new Vec3(1f, 2f, 3f), Vec3.UnitY);

            Assert.That(Approx.Same(projection, new Vec3(0f, 2f, 0f)), Is.True,
                $"(1,2,3) を UnitY へ射影すると (0,2,0) です。実際は {projection}");
        }
    }

    [TestFixture]
    internal sealed class Vec3反射
    {
        [Test]
        public void 床で跳ね返る()
        {
            var reflected = Vec3.Reflect(new Vec3(1f, -1f, 0f), Vec3.UnitY);

            Assert.That(Approx.Same(reflected, new Vec3(1f, 1f, 0f)), Is.True,
                $"(1,-1,0) を UnitY の面で反射すると (1,1,0) です。実際は {reflected}");
        }

        [Test]
        public void 長さを保つ()
        {
            var rng = new Rng(33);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Close(Vec3.Reflect(a, n).Length(), a.Length(), 1e-2f), Is.True,
                    $"|Reflect(a,n)| = |a| に反しました。a={a}, n={n}");
            }
        }

        [Test]
        public void 二回反射すると元に戻る()
        {
            var rng = new Rng(34);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Same(Vec3.Reflect(Vec3.Reflect(a, n), n), a, 1e-2f), Is.True,
                    $"2回反射すると元に戻るはずです。a={a}, n={n}");
            }
        }

        [Test]
        public void 法線成分が反転する()
        {
            var rng = new Rng(35);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Same(Vec3.Project(Vec3.Reflect(a, n), n), -Vec3.Project(a, n), 1e-2f), Is.True,
                    $"法線方向の成分だけ符号が反転するはずです。a={a}, n={n}");
            }
        }

        [Test]
        public void 接線成分は変わらない()
        {
            var rng = new Rng(36);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), n = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Same(Vec3.Reject(Vec3.Reflect(a, n), n), Vec3.Reject(a, n), 1e-2f), Is.True,
                    $"面に沿う成分は変わらないはずです。a={a}, n={n}");
            }
        }
    }

    [TestFixture]
    internal sealed class Vec3なす角
    {
        [Test]
        public void 既知の角度()
        {
            Assert.That(Vec3.Angle(Vec3.UnitX, Vec3.UnitX), Is.EqualTo(0f).Within(1e-3f), "同方向は 0");
            Assert.That(Vec3.Angle(Vec3.UnitX, Vec3.UnitY), Is.EqualTo(Samples.Pi * 0.5f).Within(1e-3f), "直交は π/2");
            Assert.That(Vec3.Angle(Vec3.UnitX, -Vec3.UnitX), Is.EqualTo(Samples.Pi).Within(1e-3f), "逆方向は π");
            Assert.That(Vec3.Angle(Vec3.UnitX, new Vec3(1f, 1f, 0f)), Is.EqualTo(Samples.Pi * 0.25f).Within(1e-3f),
                "45度は π/4");
        }

        [Test]
        public void 対称()
        {
            var rng = new Rng(37);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextNonDegenerateVec3(), b = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Close(Vec3.Angle(a, b), Vec3.Angle(b, a), 1e-3f), Is.True,
                    $"Angle(a,b) = Angle(b,a) に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 範囲は0からπ()
        {
            var rng = new Rng(38);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextNonDegenerateVec3(), b = rng.NextNonDegenerateVec3();
                var angle = Vec3.Angle(a, b);

                Assert.That(angle, Is.InRange(-1e-4f, Samples.Pi + 1e-4f),
                    $"角度は 0 ~ π の範囲です。a={a}, b={b}, angle={angle}");
            }
        }

        [Test]
        public void 長さに依らない()
        {
            var rng = new Rng(39);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextNonDegenerateVec3(), b = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Close(Vec3.Angle(a, b), Vec3.Angle(a * 3f, b * 7f), 1e-3f), Is.True,
                    $"長さを変えても角度は変わりません。a={a}, b={b}");
            }
        }

        [Test]
        public void 内積と一致する()
        {
            var rng = new Rng(40);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextNonDegenerateVec3(), b = rng.NextNonDegenerateVec3();

                Assert.That(Approx.Close(Vec3.Dot(a, b), a.Length() * b.Length() * MathF.Cos(Vec3.Angle(a, b)), 1e-2f),
                    Is.True, $"a·b = |a||b|cosθ に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 外積と一致する()
        {
            var rng = new Rng(41);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextNonDegenerateVec3(), b = rng.NextNonDegenerateVec3();

                Assert.That(
                    Approx.Close(Vec3.Cross(a, b).Length(), a.Length() * b.Length() * MathF.Sin(Vec3.Angle(a, b)), 1e-2f),
                    Is.True, $"|a×b| = |a||b|sinθ に反しました。a={a}, b={b}");
            }
        }
    }

    [TestFixture]
    internal sealed class Vec3補間と距離
    {
        [Test]
        public void 両端が一致する()
        {
            var rng = new Rng(42);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Same(Vec3.Lerp(a, b, 0f), a), Is.True, $"t=0 で a になるはずです。a={a}, b={b}");
                Assert.That(Approx.Same(Vec3.Lerp(a, b, 1f), b), Is.True, $"t=1 で b になるはずです。a={a}, b={b}");
            }
        }

        [Test]
        public void 中点()
        {
            var rng = new Rng(43);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Same(Vec3.Lerp(a, b, 0.5f), (a + b) * 0.5f, 1e-3f), Is.True,
                    $"t=0.5 で中点になるはずです。a={a}, b={b}");
            }
        }

        [Test]
        public void 補間結果は線分上にある()
        {
            var rng = new Rng(44);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();
                var t = rng.NextFloat(0f, 1f);
                var point = Vec3.Lerp(a, b, t);

                Assert.That(
                    Approx.Close(Vec3.Distance(a, point) + Vec3.Distance(point, b), Vec3.Distance(a, b), 1e-2f),
                    Is.True, $"補間点は線分上にあるはずです。a={a}, b={b}, t={t}");
            }
        }

        [Test]
        public void 距離は対称()
        {
            var rng = new Rng(45);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Close(Vec3.Distance(a, b), Vec3.Distance(b, a), 1e-3f), Is.True,
                    $"d(a,b) = d(b,a) に反しました。a={a}, b={b}");
            }
        }

        [Test]
        public void 自分との距離は0()
        {
            var rng = new Rng(46);

            for (var i = 0; i < Samples.Count; i++)
            {
                var a = rng.NextVec3();

                Assert.That(Vec3.Distance(a, a), Is.EqualTo(0f).Within(1e-4f), $"d(a,a) = 0 に反しました。a={a}");
            }
        }

        [Test]
        public void 三角不等式()
        {
            var rng = new Rng(47);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3(), c = rng.NextVec3();

                Assert.That(Vec3.Distance(a, c),
                    Is.LessThanOrEqualTo(Vec3.Distance(a, b) + Vec3.Distance(b, c) + 1e-2f),
                    $"d(a,c) ≦ d(a,b) + d(b,c) に反しました。a={a}, b={b}, c={c}");
            }
        }

        [Test]
        public void 距離は差の長さと一致()
        {
            var rng = new Rng(48);

            for (var i = 0; i < Samples.Count; i++)
            {
                Vec3 a = rng.NextVec3(), b = rng.NextVec3();

                Assert.That(Approx.Close(Vec3.Distance(a, b), (a - b).Length(), 1e-3f), Is.True,
                    $"d(a,b) = |a - b| に反しました。a={a}, b={b}");
            }
        }
    }
}
