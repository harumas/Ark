using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ark.Scenes
{
    /// <summary>
    /// 演出付きでシーンを切り替えるクラス
    /// </summary>
    /// <remarks>
    /// 「画面を覆う → 読み込む → 覆いを外す」の順で遷移します。
    /// 演出は<see cref="Transition"/>を差し替えて変更でき、既定は<see cref="FadeTransition"/>です。
    /// 遷移中に例外が発生しても、画面を覆ったままにはしません。
    /// </remarks>
    /// <example>
    /// <code>
    /// await SceneLoader.LoadAsync("Battle");
    ///
    /// // 演出の差し替え
    /// SceneLoader.Transition = new FadeTransition(Color.white, 0.5f);
    ///
    /// // 進捗の表示（別のasyncメソッドから）
    /// while (SceneLoader.IsLoading)
    /// {
    ///     bar.value = SceneLoader.Progress;
    ///     await Awaitable.NextFrameAsync();
    /// }
    /// </code>
    /// </example>
    public static class SceneLoader
    {
        private static ISceneTransition _Transition = new FadeTransition();
        private static bool _IsLoading;
        private static float _Progress;

        /// <summary>
        /// 遷移演出。nullにすると演出なしで切り替わります
        /// </summary>
        public static ISceneTransition Transition
        {
            get => _Transition;
            set => _Transition = value;
        }

        /// <summary>
        /// 遷移中かどうか
        /// </summary>
        public static bool IsLoading => _IsLoading;

        /// <summary>
        /// 読み込みの進捗 (0 ~ 1)
        /// </summary>
        public static float Progress => _Progress;

        /// <summary>
        /// シーンを切り替えます
        /// </summary>
        /// <param name="sceneName">Build Profilesに登録済みのシーン名</param>
        /// <exception cref="ArgumentException">シーン名が空、または読み込めない場合</exception>
        /// <exception cref="InvalidOperationException">既に遷移中の場合</exception>
        public static async Awaitable LoadAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("シーン名が空です。", nameof(sceneName));
            }

            if (_IsLoading)
            {
                throw new InvalidOperationException(
                    $"既にシーン遷移中です。{nameof(IsLoading)}で確認してから呼び出してください。");
            }

            _IsLoading = true;
            _Progress = 0f;

            try
            {
                await CoverAsync();

                var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                if (operation == null)
                {
                    throw new ArgumentException(
                        $"シーン \"{sceneName}\" を読み込めません。Build Profilesのシーン一覧に登録されているか確認してください。",
                        nameof(sceneName));
                }

                await WaitForLoadAsync(operation);
                await UncoverAsync();
            }
            catch
            {
                // 失敗しても画面を覆ったままにしない
                await UncoverAsync();
                throw;
            }
            finally
            {
                _IsLoading = false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // ドメインリロードを無効化していると、前回の再生の状態が残っている
            _Transition = new FadeTransition();
            _IsLoading = false;
            _Progress = 0f;
        }

        private static async Awaitable WaitForLoadAsync(AsyncOperation operation)
        {
            while (operation.isDone == false)
            {
                _Progress = operation.progress;

                await Awaitable.NextFrameAsync();
            }

            _Progress = 1f;
        }

        private static Awaitable CoverAsync()
        {
            var transition = _Transition;

            return transition == null ? Awaitable.EndOfFrameAsync() : transition.CoverAsync();
        }

        private static Awaitable UncoverAsync()
        {
            var transition = _Transition;

            return transition == null ? Awaitable.EndOfFrameAsync() : transition.UncoverAsync();
        }
    }
}
