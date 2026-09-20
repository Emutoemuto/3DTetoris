using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ThreeDTetris.Core
{
    /// <summary>
    /// 1種類のピースの形。全姿勢のセル配置を起動時に作り置きする（仕様書 5.4）。
    /// 実行時の回転は、ここに持っている遷移表の引き当てだけで済む。
    ///
    /// 各姿勢のセルは「最小値が原点になるよう正規化」し、昇順に並べてある。
    /// 盤面のどこに置くかは <see cref="ActivePiece"/> が持ち、この型は関与しない。
    /// </summary>
    public sealed class PieceShape
    {
        readonly Vector3Int[][] _orientations;
        readonly Vector3Int[] _sizes;
        readonly int[,] _transitions;

        public PieceId Id { get; private set; }

        /// <summary>ユニークな姿勢の数。対称性が高いピースほど24より少なくなる。</summary>
        public int OrientationCount
        {
            get { return _orientations.Length; }
        }

        public PieceShape(PieceId id, IReadOnlyList<Vector3Int> baseCells)
        {
            if (baseCells == null) throw new ArgumentNullException("baseCells");
            if (baseCells.Count == 0) throw new ArgumentException("セルが空です", "baseCells");

            Id = id;

            var indexByKey = new Dictionary<string, int>();
            var unique = new List<Vector3Int[]>();

            // 24姿勢ぶん回して、重複を取り除く。
            // All24 の先頭は恒等変換なので、姿勢0は必ず基準姿勢になる。
            IReadOnlyList<RotationMatrix> all = RotationMath.All24;
            for (int i = 0; i < all.Count; i++)
            {
                Vector3Int[] cells = NormalizeSorted(Rotate(all[i], baseCells));
                string key = KeyOf(cells);
                if (indexByKey.ContainsKey(key)) continue;

                indexByKey.Add(key, unique.Count);
                unique.Add(cells);
            }

            _orientations = unique.ToArray();

            _sizes = new Vector3Int[_orientations.Length];
            for (int i = 0; i < _orientations.Length; i++)
            {
                _sizes[i] = BoundsSizeOf(_orientations[i]);
            }

            // 「この姿勢からこの軸に回したらどの姿勢になるか」を先に全部引いておく。
            _transitions = new int[_orientations.Length, RotationMath.StepKinds];
            for (int i = 0; i < _orientations.Length; i++)
            {
                for (int step = 0; step < RotationMath.StepKinds; step++)
                {
                    Vector3Int[] rotated = NormalizeSorted(Rotate(RotationMath.Step(step), _orientations[i]));
                    _transitions[i, step] = indexByKey[KeyOf(rotated)];
                }
            }
        }

        /// <summary>指定した姿勢のセル配置。最小値が原点に正規化されている。</summary>
        public IReadOnlyList<Vector3Int> Cells(int orientation)
        {
            return _orientations[orientation];
        }

        /// <summary>指定した姿勢のバウンディングボックスの大きさ（セル数）。</summary>
        public Vector3Int Size(int orientation)
        {
            return _sizes[orientation];
        }

        /// <summary>指定した姿勢から1手回した先の姿勢。</summary>
        public int Rotated(int orientation, RotationAxis axis, bool inverse)
        {
            return _transitions[orientation, RotationMath.StepIndex(axis, inverse)];
        }

        static Vector3Int[] Rotate(RotationMatrix m, IReadOnlyList<Vector3Int> cells)
        {
            var result = new Vector3Int[cells.Count];
            for (int i = 0; i < cells.Count; i++)
            {
                result[i] = m.Apply(cells[i]);
            }
            return result;
        }

        /// <summary>最小値を原点に寄せ、昇順に並べ替える。姿勢を比較できる形にするため。</summary>
        static Vector3Int[] NormalizeSorted(Vector3Int[] cells)
        {
            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].x < minX) minX = cells[i].x;
                if (cells[i].y < minY) minY = cells[i].y;
                if (cells[i].z < minZ) minZ = cells[i].z;
            }

            var shifted = new Vector3Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                shifted[i] = new Vector3Int(cells[i].x - minX, cells[i].y - minY, cells[i].z - minZ);
            }

            Array.Sort(shifted, CompareCells);
            return shifted;
        }

        static int CompareCells(Vector3Int a, Vector3Int b)
        {
            if (a.x != b.x) return a.x - b.x;
            if (a.y != b.y) return a.y - b.y;
            return a.z - b.z;
        }

        static Vector3Int BoundsSizeOf(Vector3Int[] normalizedCells)
        {
            int maxX = 0, maxY = 0, maxZ = 0;
            for (int i = 0; i < normalizedCells.Length; i++)
            {
                if (normalizedCells[i].x > maxX) maxX = normalizedCells[i].x;
                if (normalizedCells[i].y > maxY) maxY = normalizedCells[i].y;
                if (normalizedCells[i].z > maxZ) maxZ = normalizedCells[i].z;
            }
            return new Vector3Int(maxX + 1, maxY + 1, maxZ + 1);
        }

        static string KeyOf(Vector3Int[] sortedCells)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < sortedCells.Length; i++)
            {
                sb.Append(sortedCells[i].x).Append(',')
                  .Append(sortedCells[i].y).Append(',')
                  .Append(sortedCells[i].z).Append(';');
            }
            return sb.ToString();
        }
    }
}
