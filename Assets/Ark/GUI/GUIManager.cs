using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ark.GUI
{
    /// <summary>
    /// 画面（Canvas）の開閉と重ね順、カーソル表示をまとめて管理するシングルトン
    /// </summary>
    /// <remarks>
    /// <para>
    /// シーン内のGameObjectに1つアタッチしておく必要があります（<see cref="Ark.SingletonMonoBehaviour{T}"/>）。
    /// アタッチしたGameObjectは自動でDontDestroyOnLoadされ、シーンをまたいで保持されます。
    /// </para>
    /// <para>
    /// <see cref="Open"/>で開いた画面は自動的にアクティブになり、層（<see cref="ScreenLayer"/>）に応じた
    /// <c>sortingOrder</c>が割り当てられます。<see cref="Close"/>で閉じると非アクティブになります。
    /// </para>
    /// <para>
    /// 開いている画面が1つでもあればマウスカーソルを表示し、全て閉じたら非表示にします。
    /// ポーズメニューを開いた状態でその中の設定画面を開閉しても、ポーズメニュー自体は
    /// 開いたままなのでカーソルは表示され続けます（片方だけ閉じても消えません）。
    /// </para>
    /// <para>
    /// マウスの拘束（<c>Cursor.lockState</c>）は扱いません。FPS視点操作のような
    /// ロックが必要なゲームは、呼び出し側で別途制御してください。
    /// </para>
    /// </remarks>
    public sealed class GUIManager : SingletonMonoBehaviour<GUIManager>
    {
        // 層の中で重なっても足りるよう、層ごとに十分な間隔を空ける
        private const int _LayerSpacing = 1000;

        private readonly Dictionary<ScreenLayer, List<Canvas>> _OpenScreens = CreateEmptyScreenSets();
        private int _TotalOpenCount;

        /// <summary>
        /// 開いている画面が1つでもあるかどうか
        /// </summary>
        public bool IsAnyOpen => _TotalOpenCount > 0;

        /// <summary>
        /// 画面を開きます
        /// </summary>
        /// <param name="layer">重ね順を決める層</param>
        /// <param name="screen">開く画面のCanvas</param>
        /// <exception cref="ArgumentNullException">screenがnullの場合</exception>
        public void Open(ScreenLayer layer, Canvas screen)
        {
            if (screen == null)
            {
                throw new ArgumentNullException(nameof(screen));
            }

            var stack = _OpenScreens[layer];

            // 既に開いている場合は何もしない（二重に数えない）
            if (stack.Contains(screen))
            {
                return;
            }

            stack.Add(screen);
            screen.gameObject.SetActive(true);
            ReorderLayer(layer);

            _TotalOpenCount++;

            if (_TotalOpenCount == 1)
            {
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// 画面を閉じます
        /// </summary>
        /// <param name="screen">閉じる画面のCanvas。開いていない場合は何もしません</param>
        public void Close(Canvas screen)
        {
            if (screen == null)
            {
                return;
            }

            foreach (ScreenLayer layer in Enum.GetValues(typeof(ScreenLayer)))
            {
                var stack = _OpenScreens[layer];

                if (stack.Remove(screen) == false)
                {
                    continue;
                }

                screen.gameObject.SetActive(false);
                ReorderLayer(layer);

                _TotalOpenCount--;

                if (_TotalOpenCount == 0)
                {
                    Cursor.visible = false;
                }

                return;
            }
        }

        /// <summary>
        /// 指定した画面が開いているかどうか
        /// </summary>
        public bool IsOpen(Canvas screen)
        {
            if (screen == null)
            {
                return false;
            }

            foreach (ScreenLayer layer in Enum.GetValues(typeof(ScreenLayer)))
            {
                if (_OpenScreens[layer].Contains(screen))
                {
                    return true;
                }
            }

            return false;
        }

        private void ReorderLayer(ScreenLayer layer)
        {
            var stack = _OpenScreens[layer];
            var baseOrder = (int)layer * _LayerSpacing;

            // 開いている順に並べ直す事で、途中の画面を閉じても番号が飛ばず衝突しない
            for (var i = 0; i < stack.Count; i++)
            {
                stack[i].sortingOrder = baseOrder + i;
            }
        }

        private static Dictionary<ScreenLayer, List<Canvas>> CreateEmptyScreenSets()
        {
            var sets = new Dictionary<ScreenLayer, List<Canvas>>();

            foreach (ScreenLayer layer in Enum.GetValues(typeof(ScreenLayer)))
            {
                sets[layer] = new List<Canvas>();
            }

            return sets;
        }
    }
}
