using System;
using UnityEngine;

namespace ThreeDTetris.Core
{
    /// <summary>
    /// 整数の3x3回転行列。浮動小数点を一切使わない（仕様書 5.1）。
    ///
    /// 各軸の90度回転は Unity の Quaternion.Euler と同じ向きに定義してある。
    /// 例えば <see cref="RotateY90"/> は Quaternion.Euler(0, 90, 0) と一致するので、
    /// 見た目の補間に Quaternion を使っても、ロジックとずれた向きにならない。
    /// </summary>
    public struct RotationMatrix : IEquatable<RotationMatrix>
    {
        readonly int _m00, _m01, _m02;
        readonly int _m10, _m11, _m12;
        readonly int _m20, _m21, _m22;

        public RotationMatrix(
            int m00, int m01, int m02,
            int m10, int m11, int m12,
            int m20, int m21, int m22)
        {
            _m00 = m00; _m01 = m01; _m02 = m02;
            _m10 = m10; _m11 = m11; _m12 = m12;
            _m20 = m20; _m21 = m21; _m22 = m22;
        }

        /// <summary>恒等変換。何もしない変換。入れた点がそのまま出てきます。</summary>
        public static RotationMatrix Identity
        {
            get { return new RotationMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1); }
        }

        /// <summary>X軸まわりに +90度。(x, y, z) → (x, -z, y)</summary>
        public static RotationMatrix RotateX90
        {
            get { return new RotationMatrix(1, 0, 0, 0, 0, -1, 0, 1, 0); }
        }

        /// <summary>Y軸まわりに +90度。(x, y, z) → (z, y, -x)</summary>
        public static RotationMatrix RotateY90
        {
            get { return new RotationMatrix(0, 0, 1, 0, 1, 0, -1, 0, 0); }
        }

        /// <summary>Z軸まわりに +90度。(x, y, z) → (-y, x, z)：(1, 0, 0)が(-0, 1, 0)=(0, 1, 0)に変換される</summary>
        public static RotationMatrix RotateZ90
        {
            get { return new RotationMatrix(0, -1, 0, 1, 0, 0, 0, 0, 1); }
        }

        /// <summary>1点に適用する。</summary>
        public Vector3Int Apply(Vector3Int v)
        {
            return new Vector3Int(
                _m00 * v.x + _m01 * v.y + _m02 * v.z,
                _m10 * v.x + _m11 * v.y + _m12 * v.z,
                _m20 * v.x + _m21 * v.y + _m22 * v.z);
        }

        /// <summary>合成。a * b は「b を適用したあとに a を適用する」。RotationMatrixは戻り値の型指定。</summary>
        /// <remarks></remarks>
        public static RotationMatrix operator *(RotationMatrix a, RotationMatrix b)
        {
            return new RotationMatrix(
                a._m00 * b._m00 + a._m01 * b._m10 + a._m02 * b._m20,
                a._m00 * b._m01 + a._m01 * b._m11 + a._m02 * b._m21,
                a._m00 * b._m02 + a._m01 * b._m12 + a._m02 * b._m22,

                a._m10 * b._m00 + a._m11 * b._m10 + a._m12 * b._m20,
                a._m10 * b._m01 + a._m11 * b._m11 + a._m12 * b._m21,
                a._m10 * b._m02 + a._m11 * b._m12 + a._m12 * b._m22,

                a._m20 * b._m00 + a._m21 * b._m10 + a._m22 * b._m20,
                a._m20 * b._m01 + a._m21 * b._m11 + a._m22 * b._m21,
                a._m20 * b._m02 + a._m21 * b._m12 + a._m22 * b._m22);
        }

        /// <summary>行列式。正しい回転なら必ず 1 になる（鏡映が混ざると -1）。</summary>
        public int Determinant
        {
            get
            {
                return _m00 * (_m11 * _m22 - _m12 * _m21)
                     - _m01 * (_m10 * _m22 - _m12 * _m20)
                     + _m02 * (_m10 * _m21 - _m11 * _m20);
            }
        }

        public bool Equals(RotationMatrix o)
        {
            return _m00 == o._m00 && _m01 == o._m01 && _m02 == o._m02
                && _m10 == o._m10 && _m11 == o._m11 && _m12 == o._m12
                && _m20 == o._m20 && _m21 == o._m21 && _m22 == o._m22;
        }

        public override bool Equals(object obj)
        {
            return obj is RotationMatrix && Equals((RotationMatrix)obj);
        }

        // 9個の数字を、1個の整数に畳み込んでいる。
        public override int GetHashCode()
        {
            int h = 17;
            h = h * 31 + _m00; h = h * 31 + _m01; h = h * 31 + _m02;
            h = h * 31 + _m10; h = h * 31 + _m11; h = h * 31 + _m12;
            h = h * 31 + _m20; h = h * 31 + _m21; h = h * 31 + _m22;
            return h;
        }

        // 分かりやすい文字列で返してくれる
        public override string ToString()
        {
            return string.Format("[{0} {1} {2} | {3} {4} {5} | {6} {7} {8}]",
                _m00, _m01, _m02, _m10, _m11, _m12, _m20, _m21, _m22);
        }
    }
}
