using Ark;
using Ark.Input;
using Ark.Scenes;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// タイトル画面
    /// </summary>
    public sealed class TitleBootstrap : MonoBehaviour
    {
        private readonly SubscriptionBag _Bag = new();

        private InputEvent _SubmitInput;

        private void Awake()
        {
            GameView.CreateCamera();

            _SubmitInput = InputProvider.CreateEvent(ActionGuid.Player.Attack);
            _SubmitInput.Performed.Subscribe(_ => StartGame()).AddTo(_Bag);
        }

        private void OnDestroy()
        {
            _Bag.Dispose();
            _SubmitInput?.Dispose();
        }

        private void StartGame()
        {
            if (SceneLoader.IsLoading)
            {
                return;
            }

            SceneLoader.LoadAsync(SceneNames.Game).LogExceptionsAndForget();
        }

        private void OnGUI()
        {
            var center = Screen.height * 0.5f;

            GameView.Draw(GameView.Line(center - 140f, 80f), "ARK ARENA", 64, TextAnchor.MiddleCenter, Color.white);
            GameView.Draw(GameView.Line(center - 60f), "Ark 動作確認用ミニゲーム", 22, TextAnchor.MiddleCenter,
                new Color(1f, 1f, 1f, 0.5f));

            // 押しっぱなしでも見やすいよう点滅させる
            var alpha = Mathf.PingPong(Time.unscaledTime * 1.4f, 0.7f) + 0.3f;

            GameView.Draw(GameView.Line(center + 40f), "攻撃ボタン（マウス左 / 右トリガー）でスタート", 26,
                TextAnchor.MiddleCenter, new Color(GameView.Accent.r, GameView.Accent.g, GameView.Accent.b, alpha));

            if (GameSession.HighScore > 0)
            {
                GameView.Draw(GameView.Line(center + 110f), $"HIGH SCORE  {GameSession.HighScore}", 24,
                    TextAnchor.MiddleCenter, GameView.PlayerColor);
            }
        }
    }
}
