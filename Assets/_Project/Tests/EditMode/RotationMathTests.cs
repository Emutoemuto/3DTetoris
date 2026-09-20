using NUnit.Framework;
using ThreeDTetris.Core;
using UnityEngine;

namespace ThreeDTetris.Tests.EditMode
{
    /// <summary>
    /// 回転行列と24姿勢の検証（仕様書 5.1、13章 M1）。
    /// 目で見て確かめるのが難しい領域なので、性質をここで固定する。
    /// </summary>
    public class RotationMathTests
    {
        [Test]
        public void 姿勢はちょうど24通りある()
        {
            Assert.That(RotationMath.All24.Count, Is.EqualTo(24));
        }

        [Test]
        public void 全姿勢に重複がない()
        {
            var seen = new System.Collections.Generic.HashSet<RotationMatrix>();
            foreach (var m in RotationMath.All24)
            {
                Assert.That(seen.Add(m), Is.True, "重複した姿勢がある: " + m);
            }
        }

        [Test]
        public void 先頭は恒等変換である()
        {
            // 姿勢0を基準姿勢として扱うので、ここが崩れると全ピースの初期姿勢がずれる。
            Assert.That(RotationMath.All24[0], Is.EqualTo(RotationMatrix.Identity));
        }

        [Test]
        public void すべての姿勢の行列式が1である()
        {
            // 行列式が -1 になるものは鏡映であり、実際には作れない姿勢になってしまう。
            foreach (var m in RotationMath.All24)
            {
                Assert.That(m.Determinant, Is.EqualTo(1), "鏡映が混ざっている: " + m);
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void 同じ軸に4回まわすと元に戻る(int axisIndex)
        {
            var axis = (RotationAxis)axisIndex;
            var step = RotationMath.Step(axis, false);
            var composed = step * step * step * step;

            Assert.That(composed, Is.EqualTo(RotationMatrix.Identity));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void 逆回転は正回転を打ち消す(int axisIndex)
        {
            var axis = (RotationAxis)axisIndex;
            var forward = RotationMath.Step(axis, false);
            var backward = RotationMath.Step(axis, true);

            Assert.That(backward * forward, Is.EqualTo(RotationMatrix.Identity));
        }

        [Test]
        public void 回転の向きがUnityのQuaternionと一致する()
        {
            // Quaternion.Euler と同じ向きにしてあるので、見た目の補間とロジックがずれない。
            // ここが崩れると、画面上で回った向きと盤面の姿勢が食い違う。
            var x = RotationMatrix.RotateX90;
            var y = RotationMatrix.RotateY90;
            var z = RotationMatrix.RotateZ90;

            Assert.That(x.Apply(new Vector3Int(0, 1, 0)), Is.EqualTo(new Vector3Int(0, 0, 1)), "X90: +Y → +Z");
            Assert.That(y.Apply(new Vector3Int(0, 0, 1)), Is.EqualTo(new Vector3Int(1, 0, 0)), "Y90: +Z → +X");
            Assert.That(z.Apply(new Vector3Int(1, 0, 0)), Is.EqualTo(new Vector3Int(0, 1, 0)), "Z90: +X → +Y");
        }
    }
}
