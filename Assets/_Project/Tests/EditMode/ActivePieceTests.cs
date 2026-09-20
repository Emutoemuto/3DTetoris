using System.Collections.Generic;
using NUnit.Framework;
using ThreeDTetris.Core;
using UnityEngine;

namespace ThreeDTetris.Tests.EditMode
{
    /// <summary>
    /// 盤面上での位置の扱いの検証（仕様書 5.3）。
    ///
    /// 「回転しただけでピースが盤面上を移動してしまう」は遊んで初めて気づく類のバグなので、
    /// ここで潰しておく。
    /// </summary>
    public class ActivePieceTests
    {
        static readonly Vector3Int SomeWhere = new Vector3Int(1, 5, 1);

        static IEnumerable<PieceId> AllPieces
        {
            get { return PieceDefinitions.AllIds; }
        }

        [TestCaseSource("AllPieces")]
        public void 回転しても中心は動かない(PieceId id)
        {
            var shape = PieceShapeTable.Default.Get(id);

            for (int o = 0; o < shape.OrientationCount; o++)
            {
                var piece = PieceGeometry.CreateAtOrigin(shape, o, SomeWhere);

                for (int step = 0; step < RotationMath.StepKinds; step++)
                {
                    var axis = (RotationAxis)(step % 3);
                    bool inverse = step >= 3;

                    var rotated = PieceGeometry.Rotated(shape, piece, axis, inverse);

                    Assert.That(rotated.CenterDoubled, Is.EqualTo(piece.CenterDoubled),
                        id + " 姿勢" + o + " を軸" + axis + (inverse ? "(逆)" : "") + "に回したら中心が動いた");
                }
            }
        }

        [TestCaseSource("AllPieces")]
        public void 同じ軸に4回まわすと完全に元へ戻る(PieceId id)
        {
            // 中心を保つために切り捨てが入るので、それが蓄積しないことを確かめる。
            // 最小隅を持ち回す実装だと、ここでずれていく。
            var shape = PieceShapeTable.Default.Get(id);

            for (int axisIndex = 0; axisIndex < 3; axisIndex++)
            {
                var start = PieceGeometry.CreateAtOrigin(shape, 0, SomeWhere);
                var piece = start;

                for (int i = 0; i < 4; i++)
                {
                    piece = PieceGeometry.Rotated(shape, piece, (RotationAxis)axisIndex, false);
                }

                Assert.That(piece, Is.EqualTo(start), id + " 軸" + axisIndex + " でずれた");
                CollectionsAreEqual(
                    PieceGeometry.CellsOf(shape, start),
                    PieceGeometry.CellsOf(shape, piece),
                    id + " 軸" + axisIndex + " のセル配置");
            }
        }

        [TestCaseSource("AllPieces")]
        public void 移動すると全セルが同じだけずれる(PieceId id)
        {
            var shape = PieceShapeTable.Default.Get(id);
            var delta = new Vector3Int(1, -2, 3);

            var piece = PieceGeometry.CreateAtOrigin(shape, 0, SomeWhere);
            var moved = piece.MovedBy(delta);

            var before = PieceGeometry.CellsOf(shape, piece);
            var after = PieceGeometry.CellsOf(shape, moved);

            for (int i = 0; i < before.Length; i++)
            {
                Assert.That(after[i], Is.EqualTo(before[i] + delta));
            }
        }

        [TestCaseSource("AllPieces")]
        public void 最小隅を指定して作るとその位置に置かれる(PieceId id)
        {
            var shape = PieceShapeTable.Default.Get(id);

            for (int o = 0; o < shape.OrientationCount; o++)
            {
                var piece = PieceGeometry.CreateAtOrigin(shape, o, SomeWhere);
                Assert.That(PieceGeometry.OriginOf(shape, piece), Is.EqualTo(SomeWhere),
                    id + " 姿勢" + o);
            }
        }

        [Test]
        public void 棒を縦にしても中心の位置に留まる()
        {
            // 横4マスから縦4マスへ。最小隅をそのまま使う実装だと、ここで箱の角へ飛ぶ。
            var bar = PieceShapeTable.Default.Get(PieceId.I);
            var horizontal = PieceGeometry.CreateAtOrigin(bar, 0, new Vector3Int(0, 5, 1));
            var vertical = PieceGeometry.Rotated(bar, horizontal, RotationAxis.Z, false);

            Assert.That(vertical.CenterDoubled, Is.EqualTo(horizontal.CenterDoubled));

            // 長辺が向いている軸が入れ替わっていることも確認する（本当に回っているか）
            Assert.That(bar.Size(horizontal.Orientation), Is.EqualTo(new Vector3Int(4, 1, 1)));
            Assert.That(bar.Size(vertical.Orientation), Is.EqualTo(new Vector3Int(1, 4, 1)));
        }

        static void CollectionsAreEqual(Vector3Int[] a, Vector3Int[] b, string label)
        {
            Assert.That(a.Length, Is.EqualTo(b.Length), label);
            for (int i = 0; i < a.Length; i++)
            {
                Assert.That(b[i], Is.EqualTo(a[i]), label);
            }
        }
    }
}
