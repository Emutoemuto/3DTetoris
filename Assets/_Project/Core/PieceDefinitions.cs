using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeDTetris.Core
{
    /// <summary>
    /// 8種のテトラキューブの基準姿勢（仕様書 4.1）。
    ///
    /// ここに書いてあるのは形だけで、色やマテリアルは持たない。
    /// 見た目は Presentation 層の PieceVisualTable が持つ（仕様書 4.2）。
    /// </summary>
    public static class PieceDefinitions
    {
        static readonly PieceId[] s_allIds =
        {
            PieceId.I, PieceId.O, PieceId.L, PieceId.S,
            PieceId.T, PieceId.Tripod, PieceId.ScrewR, PieceId.ScrewL,
        };

        public static IReadOnlyList<PieceId> AllIds
        {
            get { return s_allIds; }
        }

        public static IReadOnlyList<Vector3Int> BaseCellsOf(PieceId id)
        {
            switch (id)
            {
                // 平面に収まる5種（2Dテトロミノの立体版）
                case PieceId.I:
                    return Cells(0, 0, 0,  1, 0, 0,  2, 0, 0,  3, 0, 0);
                case PieceId.O:
                    return Cells(0, 0, 0,  1, 0, 0,  0, 0, 1,  1, 0, 1);
                case PieceId.L:
                    return Cells(0, 0, 0,  1, 0, 0,  2, 0, 0,  2, 0, 1);
                case PieceId.S:
                    return Cells(0, 0, 0,  1, 0, 0,  1, 0, 1,  2, 0, 1);
                case PieceId.T:
                    return Cells(0, 0, 0,  1, 0, 0,  2, 0, 0,  1, 0, 1);

                // 平面に収まらない3種（3D固有）
                case PieceId.Tripod:
                    return Cells(0, 0, 0,  1, 0, 0,  0, 1, 0,  0, 0, 1);
                case PieceId.ScrewR:
                    return Cells(0, 0, 0,  1, 0, 0,  1, 0, 1,  1, 1, 1);
                case PieceId.ScrewL:
                    return Cells(0, 0, 0,  1, 0, 0,  0, 0, 1,  0, 1, 1);

                default:
                    throw new ArgumentOutOfRangeException("id", id, "未知のピースです");
            }
        }

        static Vector3Int[] Cells(params int[] xyz)
        {
            var result = new Vector3Int[xyz.Length / 3];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Vector3Int(xyz[i * 3], xyz[i * 3 + 1], xyz[i * 3 + 2]);
            }
            return result;
        }
    }

    /// <summary>8種の <see cref="PieceShape"/> を保持し、IDで引く。</summary>
    public sealed class PieceShapeTable
    {
        static readonly PieceShapeTable s_default = new PieceShapeTable();

        readonly Dictionary<PieceId, PieceShape> _shapes = new Dictionary<PieceId, PieceShape>();
        readonly PieceShape[] _all;

        /// <summary>ハードコードされた定義から作った標準の表。</summary>
        public static PieceShapeTable Default
        {
            get { return s_default; }
        }

        public PieceShapeTable()
        {
            IReadOnlyList<PieceId> ids = PieceDefinitions.AllIds;
            _all = new PieceShape[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                var shape = new PieceShape(ids[i], PieceDefinitions.BaseCellsOf(ids[i]));
                _shapes.Add(ids[i], shape);
                _all[i] = shape;
            }
        }

        public IReadOnlyList<PieceShape> All
        {
            get { return _all; }
        }

        public PieceShape Get(PieceId id)
        {
            PieceShape shape;
            if (!_shapes.TryGetValue(id, out shape))
            {
                throw new ArgumentOutOfRangeException("id", id, "表に無いピースです");
            }
            return shape;
        }
    }
}
