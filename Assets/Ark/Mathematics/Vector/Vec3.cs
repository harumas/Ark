using System;
using UnityEngine;

namespace Ark.Mathematics
{
	/// <summary>
	/// 3次元ベクトル
	/// </summary>
	public readonly struct Vec3
	{
		public readonly float X;
		public readonly float Y;
		public readonly float Z;
		
		public Vec3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Vec3 Zero => new Vec3(0f, 0f, 0f);
		public static Vec3 UnitX => new Vec3(1f, 0f, 0f);
		public static Vec3 UnitY => new Vec3(0f, 1f, 0f);
		public static Vec3 UnitZ => new Vec3(0f, 0f, 1f);

		/// <summary>Z成分を0にしたベクトル。XY平面への正射影として使えます</summary>
		public Vec3 XY => new Vec3(X, Y, 0f);

		/// <summary>Y成分を0にしたベクトル。XZ平面への正射影として使えます</summary>
		public Vec3 XZ => new Vec3(X, 0f, Z);

		/// <summary>X成分を0にしたベクトル。YZ平面への正射影として使えます</summary>
		public Vec3 YZ => new Vec3(0f, Y, Z);

		/// <summary>
		/// X成分だけを差し替えた新しいベクトルを返します。自分自身は変化しません
		/// </summary>
		/// <param name="x">新しいX成分</param>
		/// <returns>X成分だけを差し替えたベクトル</returns>
		public Vec3 WithX(float x)
		{
			return new Vec3(x, Y, Z);
		}

		/// <summary>
		/// Y成分だけを差し替えた新しいベクトルを返します。自分自身は変化しません
		/// </summary>
		/// <param name="y">新しいY成分</param>
		/// <returns>Y成分だけを差し替えたベクトル</returns>
		public Vec3 WithY(float y)
		{
			return new Vec3(X, y, Z);
		}

		/// <summary>
		/// Z成分だけを差し替えた新しいベクトルを返します。自分自身は変化しません
		/// </summary>
		/// <param name="z">新しいZ成分</param>
		/// <returns>Z成分だけを差し替えたベクトル</returns>
		public Vec3 WithZ(float z)
		{
			return new Vec3(X, Y, z);
		}

		/// <summary>
		/// 長さの2乗を返します。大小比較だけなら sqrt が要らない分、<see cref="Length"/> より速く済みます
		/// </summary>
		/// <returns>長さの2乗</returns>
		public float SqrLength()
		{
			return X * X + Y * Y + Z * Z;
		}

		/// <summary>
		/// 長さを返します
		/// </summary>
		/// <returns>長さ</returns>
		public float Length()
		{
			return (float)LengthDouble();
		}

		/// <summary>
		/// 長さを double 精度で返します
		/// </summary>
		/// <returns>長さ（double精度）</returns>
		public double LengthDouble()
		{
			return Math.Sqrt(X * X + Y * Y + Z * Z);
		}

		/// <summary>
		/// 長さを1にしたベクトルを返します
		/// </summary>
		/// <returns>長さ1のベクトル。零ベクトルの場合は<see cref="Zero"/></returns>
		public Vec3 Normalize()
		{
			double length = LengthDouble();

			// 長さが0の場合はゼロベクトル
			if (length == 0f)
			{
				return Zero;
			}

			double invLength = 1.0 / length;
			return new Vec3((float)(X * invLength), (float)(Y * invLength), (float)(Z * invLength));
		}

		public static Vec3 operator +(Vec3 a, Vec3 b)
		{
			return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}
		public static Vec3 operator -(Vec3 a, Vec3 b)
		{
			return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vec3 operator -(Vec3 v)
		{
			return new Vec3(-v.X, -v.Y, -v.Z);
		}

		public static Vec3 operator *(Vec3 v, float s)
		{
			return new Vec3(v.X * s, v.Y * s, v.Z * s);
		}

		public static Vec3 operator *(float s, Vec3 v)
		{
			return new Vec3(v.X * s, v.Y * s, v.Z * s);
		}

		public static Vec3 operator /(Vec3 v, float s)
		{
			float d = 1.0f / s;
			return new Vec3(v.X * d, v.Y * d, v.Z * d);
		}

		/// <summary>
		/// 内積を返します。符号は正なら同じ向き、0なら直交、負なら逆向きの目安になります
		/// </summary>
		/// <param name="a">1つ目のベクトル</param>
		/// <param name="b">2つ目のベクトル</param>
		/// <returns>内積</returns>
		public static float Dot(Vec3 a, Vec3 b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}

		/// <summary>
		/// aとbの両方に垂直なベクトルを返します（右手系）
		/// </summary>
		/// <param name="a">1つ目のベクトル</param>
		/// <param name="b">2つ目のベクトル</param>
		/// <returns>aとbに垂直なベクトル</returns>
		public static Vec3 Cross(Vec3 a, Vec3 b)
		{
			return new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		}

		/// <summary>
		/// 2点間の距離を返します
		/// </summary>
		/// <param name="a">1つ目の点</param>
		/// <param name="b">2つ目の点</param>
		/// <returns>距離</returns>
		public static float Distance(Vec3 a, Vec3 b)
		{
			return (b - a).Length();
		}

		/// <summary>
		/// a から b への線形補間を返します
		/// </summary>
		/// <param name="a">t=0のときの値</param>
		/// <param name="b">t=1のときの値</param>
		/// <param name="t">補間の割合。0~1にクランプされません</param>
		/// <returns>補間した値</returns>
		public static Vec3 Lerp(Vec3 a, Vec3 b, float t)
		{
			return a + (b - a) * t;
		}

		/// <summary>
		/// a のうち、normal 方向に沿っている成分を返します
		/// </summary>
		/// <param name="a">分解したいベクトル</param>
		/// <param name="normal">射影先の向き。単位ベクトルでなくても構いません</param>
		/// <returns>normal方向の成分</returns>
		public static Vec3 Project(Vec3 a, Vec3 normal)
		{
			return Dot(a, normal) / normal.SqrLength() * normal;
		}

		/// <summary>
		/// a のうち、normal 方向とは無関係な（垂直な）成分を返します
		/// </summary>
		/// <param name="a">分解したいベクトル</param>
		/// <param name="normal">基準にする向き。単位ベクトルでなくても構いません</param>
		/// <returns>normalに垂直な成分</returns>
		public static Vec3 Reject(Vec3 a, Vec3 normal)
		{
			return a - Project(a, normal);
		}

		/// <summary>
		/// i を normal の面で跳ね返したベクトルを返します
		/// </summary>
		/// <param name="i">入射方向（ボールの速度や光線の向き）</param>
		/// <param name="normal">跳ね返る面の法線。単位ベクトルでなくても構いません</param>
		/// <returns>反射後のベクトル</returns>
		public static Vec3 Reflect(Vec3 i, Vec3 normal)
		{
			return i - 2f * Project(i, normal);
		}

		/// <summary>
		/// a と b のなす角を返します
		/// </summary>
		/// <param name="a">1つ目のベクトル</param>
		/// <param name="b">2つ目のベクトル</param>
		/// <returns>ラジアン単位、0 ~ π の角度</returns>
		public static float Angle(Vec3 a, Vec3 b)
		{
			float dot = Dot(a, b) / (a.Length() * b.Length());
			return (float)Math.Acos(dot);
		}
	}
}