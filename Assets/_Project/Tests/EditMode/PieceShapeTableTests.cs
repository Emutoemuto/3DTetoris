using System.Collections.Generic;
using NUnit.Framework;
using ThreeDTetris.Core;
using UnityEngine;

namespace ThreeDTetris.Tests.EditMode
{
    /// <summary>8種のピース定義と姿勢テーブルの検証（仕様書 4.1、5.4）。</summary>
    public class PieceShapeTableTests
    {
        static IEnumerable<PieceId> AllPieces
        {
            get { return PieceDefinitions.AllIds; }
        }

        [Test]
        public void ピースは8種そろっている()
        {
            Assert.That(PieceShapeTable.Default.All.Count, Is.EqualTo(8));
        }

        [TestCaseSource("AllPieces")]
        public void どの姿勢でもセル数は4のまま(PieceId id)
        {
            var shape = PieceShapeTable.Default.Get(id);
            for (int o = 0; o < shape.OrientationCount; o++)
            {
                Assert.That(shape.Cells(o).Count, Is.EqualTo(4), id + " の姿勢" + o);
            }
        }

        [TestCaseSource("AllPieces")]
        public void 姿勢数は24の約数になる(PieceId id)
        {
            // 姿勢数 = 24 ÷ そのピースの対称性の数。約数にならないなら重複除去が壊れている。
            var shape = PieceShapeTable.Default.Get(id);
            Assert.That(24 % shape.OrientationCount, Is.Zero,
                id + " の姿勢数 " + shape.OrientationCount + " は24の約数ではない");
        }

        [TestCaseSource("AllPieces")]
        public void すべての姿勢が正規化されている(PieceId id)
        {
            // 最小値が原点でないと、姿勢どうしの比較も中心の計算も狂う。
            var shape = PieceShapeTable.Default.Get(id);
            for (int o = 0; o < shape.OrientationCount; o++)
            {
                var min = MinOf(shape.Cells(o));
                Assert.That(min, Is.EqualTo(Vector3Int.zero), id + " の姿勢" + o);
            }
        }

        [TestCase(PieceId.I, 3)]
        [TestCase(PieceId.O, 3)]
        [TestCase(PieceId.Tripod, 8)]
        public void 対称性の高いピースは姿勢数が減る(PieceId id, int expected)
        {
            // I は3方向の棒、O は3平面の板、Tripod は3回対称。手計算できる代表例を固定しておく。
            var shape = PieceShapeTable.Default.Get(id);
            Assert.That(shape.OrientationCount, Is.EqualTo(expected));
        }

        [Test]
        public void ピース8種はどれも回転で一致しない()
        {
            // ScrewR と ScrewL は鏡像どうしなので、3D回転で一致しないことがここで担保される。
            var shapes = PieceShapeTable.Default.All;
            for (int i = 0; i < shapes.Count; i++)
            {
                for (int j = i + 1; j < shapes.Count; j++)
                {
                    Assert.That(SharesAnyOrientation(shapes[i], shapes[j]), Is.False,
                        shapes[i].Id + " と " + shapes[j].Id + " が同じ形になっている");
                }
            }
        }

        [TestCaseSource("AllPieces")]
        public void 回転の遷移表が4回で元の姿勢に戻る(PieceId id)
        {
            var shape = PieceShapeTable.Default.Get(id);
            for (int o = 0; o < shape.OrientationCount; o++)
            {
                for (int axisIndex = 0; axisIndex < 3; axisIndex++)
                {
                    int current = o;
                    for (int i = 0; i < 4; i++)
                    {
                        current = shape.Rotated(current, (RotationAxis)axisIndex, false);
                    }
                    Assert.That(current, Is.EqualTo(o), id + " 姿勢" + o + " 軸" + axisIndex);
                }
            }
        }

        [TestCaseSource("AllPieces")]
        public void 逆回転は正回転を打ち消す(PieceId id)
        {
            var shape = PieceShapeTable.Default.Get(id);
            for (int o = 0; o < shape.OrientationCount; o++)
            {
                for (int axisIndex = 0; axisIndex < 3; axisIndex++)
                {
                    var axis = (RotationAxis)axisIndex;
                    int forward = shape.Rotated(o, axis, false);
                    int back = shape.Rotated(forward, axis, true);
                    Assert.That(back, Is.EqualTo(o), id + " 姿勢" + o + " 軸" + axisIndex);
                }
            }
        }

        static bool SharesAnyOrientation(PieceShape a, PieceShape b)
        {
            for (int i = 0; i < a.OrientationCount; i++)
            {
                for (int j = 0; j < b.OrientationCount; j++)
                {
                    if (SameCells(a.Cells(i), b.Cells(j))) return true;
                }
            }
            return false;
        }

        static bool SameCells(IReadOnlyList<Vector3Int> a, IReadOnlyList<Vector3Int> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        static Vector3Int MinOf(IReadOnlyList<Vector3Int> cells)
        {
            int x = int.MaxValue, y = int.MaxValue, z = int.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].x < x) x = cells[i].x;
                if (cells[i].y < y) y = cells[i].y;
                if (cells[i].z < z) z = cells[i].z;
            }
            return new Vector3Int(x, y, z);
        }
    }
}
