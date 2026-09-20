namespace ThreeDTetris.Core
{
    /// <summary>ピースの種類。4セルのテトラキューブ8種（仕様書 4.1）。</summary>
    /// <remarks>enumはあくまで型宣言なのでこれでつくる変数はPieceId型になる。</remarks>
    public enum PieceId
    {
        I = 0,
        O = 1,
        L = 2,
        S = 3,
        T = 4,
        Tripod = 5,
        ScrewR = 6,
        ScrewL = 7,
    }

    /// <summary>回転させる軸。いずれもワールド固定（仕様書 10.3）。</summary>
    public enum RotationAxis
    {
        X = 0,
        Y = 1,
        Z = 2,
    }
}
