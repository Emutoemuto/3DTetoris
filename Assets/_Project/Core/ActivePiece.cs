using System;
using UnityEngine;

namespace ThreeDTetris.Core
{
    /// <summary>
    /// 盤面上にいる操作中のピース。
    ///
    /// 位置を「バウンディングボックスの中心（倍座標）」で持つのがこの型の肝（仕様書 5.3）。
    /// 姿勢ごとにバウンディングボックスの大きさが変わるため、最小隅をそのまま持っていると
    /// 回転のたびにピースが盤面上を滑る。中心を持っておけば、回転しても中心は変わらない。
    ///
    /// 倍座標（セル座標の2倍）にしてあるのは、中心が半セルになる場合を整数のまま扱うため。
    /// 例えば横4マスのピースの中心は2.0セル目だが、倍座標なら 4 という整数で表せる。
    /// </summary>
    public struct ActivePiece : IEquatable<ActivePiece>
    {
        public PieceId Id { get; private set; }

        /// <summary><see cref="PieceShape"/> の姿勢添字。</summary>
        public int Orientation { get; private set; }

        /// <summary>バウンディングボックス中心のセル座標を2倍したもの。</summary>
        public Vector3Int CenterDoubled { get; private set; }

        public ActivePiece(PieceId id, int orientation, Vector3Int centerDoubled)
        {
            Id = id;
            Orientation = orientation;
            CenterDoubled = centerDoubled;
        }

        /// <summary>XZ方向などへ1マス単位で動かす。倍座標なので2ずつ動く。</summary>
        public ActivePiece MovedBy(Vector3Int delta)
        {
            return new ActivePiece(Id, Orientation, CenterDoubled + delta * 2);
        }

        /// <summary>姿勢だけを差し替える。中心は動かさない。</summary>
        public ActivePiece WithOrientation(int orientation)
        {
            return new ActivePiece(Id, orientation, CenterDoubled);
        }

        /// <summary>中心だけを差し替える。壁への押し戻しで使う。</summary>
        public ActivePiece WithCenterDoubled(Vector3Int centerDoubled)
        {
            return new ActivePiece(Id, Orientation, centerDoubled);
        }

        public bool Equals(ActivePiece other)
        {
            return Id == other.Id
                && Orientation == other.Orientation
                && CenterDoubled == other.CenterDoubled;
        }

        public override bool Equals(object obj)
        {
            return obj is ActivePiece && Equals((ActivePiece)obj);
        }

        public override int GetHashCode()
        {
            int h = 17;
            h = h * 31 + (int)Id;
            h = h * 31 + Orientation;
            h = h * 31 + CenterDoubled.GetHashCode();
            return h;
        }

        public override string ToString()
        {
            return string.Format("{0} o={1} centerD={2}", Id, Orientation, CenterDoubled);
        }
    }

    /// <summary>
    /// <see cref="ActivePiece"/> と <see cref="PieceShape"/> をつないで、
    /// 盤面上の実際のセル座標を求める。位置に関する計算はここに集約する。
    /// </summary>
    public static class PieceGeometry
    {
        /// <summary>指定の姿勢・位置で、バウンディングボックスの最小隅がどのセルに来るか。</summary>
        public static Vector3Int OriginOf(PieceShape shape, ActivePiece piece)
        {
            Vector3Int size = shape.Size(piece.Orientation);
            Vector3Int d = piece.CenterDoubled - size;

            // 中心が半セルになる姿勢では割り切れない。整数へ落とす必要があるので切り捨てる。
            // int の >> 1 は負数でも床除算になるので、-1 が -1（切り上げ）にならない。
            return new Vector3Int(d.x >> 1, d.y >> 1, d.z >> 1);
        }

        /// <summary>盤面上で占有するセルを書き出す。配列はピースのセル数ぶん必要。</summary>
        public static void WriteCells(PieceShape shape, ActivePiece piece, Vector3Int[] destination)
        {
            if (destination == null) throw new ArgumentNullException("destination");

            var cells = shape.Cells(piece.Orientation);
            if (destination.Length < cells.Count)
            {
                throw new ArgumentException("配列が足りません", "destination");
            }

            Vector3Int origin = OriginOf(shape, piece);
            for (int i = 0; i < cells.Count; i++)
            {
                destination[i] = origin + cells[i];
            }
        }

        /// <summary>盤面上で占有するセルを新しい配列で返す。テストや初期化向け。</summary>
        public static Vector3Int[] CellsOf(PieceShape shape, ActivePiece piece)
        {
            var result = new Vector3Int[shape.Cells(piece.Orientation).Count];
            WriteCells(shape, piece, result);
            return result;
        }

        /// <summary>最小隅の位置を指定してピースを作る。</summary>
        public static ActivePiece CreateAtOrigin(PieceShape shape, int orientation, Vector3Int origin)
        {
            Vector3Int size = shape.Size(orientation);
            return new ActivePiece(shape.Id, orientation, origin * 2 + size);
        }

        /// <summary>
        /// 1手回す。中心を動かさないので、ピースは盤面上でその場に留まる。
        /// 壁からはみ出す場合の押し戻しは、盤面を知っている側の責務（仕様書 5.5）。
        /// </summary>
        public static ActivePiece Rotated(PieceShape shape, ActivePiece piece, RotationAxis axis, bool inverse)
        {
            int next = shape.Rotated(piece.Orientation, axis, inverse);
            return piece.WithOrientation(next);
        }
    }
}
