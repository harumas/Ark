using UnityEngine;

namespace Game
{
    /// <summary>
    /// シーンをまたいで結果を受け渡すクラス
    /// </summary>
    internal static class GameSession
    {
        /// <summary>
        /// 倒した数
        /// </summary>
        public static int Score;

        /// <summary>
        /// 生き延びた秒数
        /// </summary>
        public static float SurvivedTime;

        /// <summary>
        /// 制限時間を耐え切ったかどうか
        /// </summary>
        public static bool IsSurvived;

        /// <summary>
        /// これまでの最高スコア
        /// </summary>
        public static int HighScore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // ドメインリロードを無効化していると、前回の再生の値が残っている
            Score = 0;
            SurvivedTime = 0f;
            IsSurvived = false;
            HighScore = 0;
        }

        public static void Submit(int score, float survivedTime, bool isSurvived)
        {
            Score = score;
            SurvivedTime = survivedTime;
            IsSurvived = isSurvived;

            if (score > HighScore)
            {
                HighScore = score;
            }
        }
    }
}
