using Ark;
using Ark.Input;
using Ark.Scenes;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// リザルト画面
    /// </summary>
    public sealed class ResultBootstrap : MonoBehaviour
    {
        private readonly SubscriptionBag _Bag = new();

        private InputEvent _SubmitInput;
        private Timer _InputGuard = new(0.6f);

        private void Awake()
        {
            GameView.CreateCamera();

            _SubmitInput = InputProvider.CreateEvent(ActionGuid.Player.Attack);
            _SubmitInput.Performed.Subscribe(_ => BackToTitle()).AddTo(_Bag);
        }

        private void OnDestroy()
        {
            _Bag.Dispose();
            _SubmitInput?.Dispose();
        }

        private void Update()
        {
            _InputGuard.Tick(Time.deltaTime);
        }

        private void BackToTitle()
        {
            // 死んだ瞬間の入力でリザルトを飛ばしてしまわないよう、少し待つ
            if (_InputGuard.IsOverLimit == false || SceneLoader.IsLoading)
            {
                return;
            }

            SceneLoader.LoadAsync(SceneNames.Title).LogExceptionsAndForget();
        }

        private void OnGUI()
        {
            var center = Screen.height * 0.5f;
            var headline = GameSession.IsSurvived ? "SURVIVED" : "GAME OVER";
            var headlineColor = GameSession.IsSurvived ? GameView.PlayerColor : GameView.EnemyColor;

            GameView.Draw(GameView.Line(center - 160f, 70f), headline, 54, TextAnchor.MiddleCenter, headlineColor);

            GameView.Draw(GameView.Line(center - 70f, 60f), $"SCORE  {GameSession.Score}", 40, TextAnchor.MiddleCenter,
                Color.white);
            GameView.Draw(GameView.Line(center - 10f), $"生存時間  {GameSession.SurvivedTime:F1} 秒", 24,
                TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.6f));

            if (GameSession.Score >= GameSession.HighScore && GameSession.Score > 0)
            {
                GameView.Draw(GameView.Line(center + 40f), "NEW RECORD", 26, TextAnchor.MiddleCenter, GameView.Accent);
            }

            if (_InputGuard.IsOverLimit)
            {
                GameView.Draw(GameView.Line(center + 110f), "攻撃ボタンでタイトルへ", 24, TextAnchor.MiddleCenter,
                    new Color(1f, 1f, 1f, 0.7f));
            }
        }
    }
}
