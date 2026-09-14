# CLAUDE.md

クレーンゲーム型3Dテトリス（Unity 6 / URP）。

**まず `Docs/3DTetris_GameDesign.md` を読むこと。** 仕様・設計はすべてそこにある。このファイルはその要約ではなく、作業時に守る規約だけを書いている。

## 対話

- 日本語で回答する
- 前提が曖昧なら、実装に入る前に質問する
- 堅牢さと拡張性に配慮しつつ、YAGNIで進める。「将来必要かも」で作らない

## 絶対に崩さない設計の約束

1. **`Core` は Unity に依存しない。** MonoBehaviour / Transform / Time / UniTask を持ち込まない。同期的で決定的なままにする。これはEditModeテスト全体の前提
2. **依存は一方向。** `Core` ← `Presentation` / `Input` ← `App`。`Core` は何も参照しない。asmdefで強制済み
3. **座標は2種類。** セル座標（`Vector3Int`、ロジック）とワールド座標（`Vector3`、描画）。変換はセル → ワールドの一方向のみ。逆算は作らない
4. **回転にfloatを混ぜない。** 姿勢は整数の3×3行列。`Quaternion` は見た目の補間だけに使う
5. **盤面配列は `board[x, y, z]` で統一。** 順序を混ぜない
6. **演出を0秒にしてもゲームが同じ結果で進行すること。** ロジックは演出の完了を待たない

## 実装の規約

- 非同期は UniTask。`Presentation` / `App` / `Input` のみ
- `async void` を書かない。戻り値不要なら `UniTaskVoid` + `.Forget()`
- 非同期メソッドには `CancellationToken` を必ず引数に取る
- R3 / UniRx は入れない（判断の根拠は仕様書 11.5）
- ブロックの描画はオブジェクトプール。`Instantiate` / `Destroy` を毎回呼ばない
- 調整用の数値はコードに直接書かず `GameRuleAsset` / `PieceShapeAsset` に置く

## 見た目

- ブロックは角丸・柔らかい質感。着地スクワッシュや揺れは `transform.localScale` とマテリアルの操作のみで表現する
- **見た目の変更がロジックに影響してはいけない。** 演出を全部止めてもゲームの結果は同じ
- 物理エンジン（Rigidbody / ソフトボディ）は使わない。決定性が失われ、EditModeテストが書けなくなる

## 変更したら

- `Core` を触ったら EditMode テストを通す
- 仕様に関わる変更をしたら `Docs/3DTetris_GameDesign.md` を同じコミットで更新する
- 仕様書の「14. 作らないもの」に載っている機能は、明示的に依頼されない限り実装しない
