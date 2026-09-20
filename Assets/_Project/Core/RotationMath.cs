using System.Collections.Generic;

namespace ThreeDTetris.Core
{
    /// <summary>
    /// 立方体の回転対称（全24姿勢）を扱う。
    /// 表は起動時に1度だけ作り、以後は配列の引き当てで済ませる（仕様書 5.4）。
    /// </summary>
    public static class RotationMath
    {
        /// <summary>軸ごとの90度回転の種類数。X/Y/Z の正転と逆転で6通り。</summary>
        public const int StepKinds = 6;

        static readonly RotationMatrix[] s_all24 = BuildAll24();
        static readonly RotationMatrix[] s_steps = BuildSteps();

        /// <summary>全24姿勢。先頭は必ず恒等変換。</summary>
        public static IReadOnlyList<RotationMatrix> All24
        {
            get { return s_all24; }
        }

        /// <summary>軸と向きから、1手ぶんの90度回転を得る。</summary>
        public static RotationMatrix Step(RotationAxis axis, bool inverse)
        {
            return s_steps[StepIndex(axis, inverse)];
        }

        /// <summary>1手ぶんの90度回転を添字で得る（0〜5）。</summary>
        public static RotationMatrix Step(int stepIndex)
        {
            return s_steps[stepIndex];
        }

        /// <summary>軸と向きを 0〜5 の添字に変換する。</summary>
        public static int StepIndex(RotationAxis axis, bool inverse)
        {
            return (int)axis + (inverse ? 3 : 0);
        }

        static RotationMatrix[] BuildSteps()
        {
            RotationMatrix x = RotationMatrix.RotateX90;
            RotationMatrix y = RotationMatrix.RotateY90;
            RotationMatrix z = RotationMatrix.RotateZ90;

            // 逆回転は同じ軸に3回まわすのと同じ。逆行列を別に用意しなくてよい。
            return new[]
            {
                x, y, z,
                x * x * x,
                y * y * y,
                z * z * z,
            };
        }

        /// <summary>
        /// 恒等変換から X90 と Y90 を掛け続けて、それ以上増えなくなるまで集める。
        /// 結果は必ず24個になる（立方体の回転群の位数）。
        /// 24個を直接書き下すよりも、書き間違いが起きない。
        /// </summary>
        static RotationMatrix[] BuildAll24()
        {
            RotationMatrix x = RotationMatrix.RotateX90;
            RotationMatrix y = RotationMatrix.RotateY90;

            var ordered = new List<RotationMatrix>();
            var seen = new HashSet<RotationMatrix>();

            var queue = new Queue<RotationMatrix>();
            RotationMatrix identity = RotationMatrix.Identity;
            ordered.Add(identity);
            seen.Add(identity);
            queue.Enqueue(identity);

            while (queue.Count > 0)
            {
                RotationMatrix current = queue.Dequeue();

                RotationMatrix nextX = x * current;
                if (seen.Add(nextX))
                {
                    ordered.Add(nextX);
                    queue.Enqueue(nextX);
                }

                RotationMatrix nextY = y * current;
                if (seen.Add(nextY))
                {
                    ordered.Add(nextY);
                    queue.Enqueue(nextY);
                }
            }

            return ordered.ToArray();
        }
    }
}
