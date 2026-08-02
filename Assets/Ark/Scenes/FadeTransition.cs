using UnityEngine;
using UnityEngine.UI;

namespace Ark.Scenes
{
    /// <summary>
    /// 画面全体を単色で覆うフェード演出
    /// </summary>
    /// <remarks>
    /// 覆いに使うCanvasは初回に自動生成し、DontDestroyOnLoadで保持するため、
    /// シーンへの事前配置は不要です。覆っている間は入力も遮ります。
    /// 経過には<c>Time.unscaledDeltaTime</c>を使うため、<c>Time.timeScale</c>が0でも進みます。
    /// </remarks>
    public sealed class FadeTransition : ISceneTransition
    {
        private const string _ObjectName = "[Ark] FadeTransition";

        private Image _Image;
        private Color _Color;
        private float _Duration;

        public FadeTransition() : this(Color.black)
        {
        }

        /// <param name="color">覆う色</param>
        /// <param name="duration">フェードにかける秒数</param>
        public FadeTransition(Color color, float duration = 0.3f)
        {
            _Color = color;
            Duration = duration;
        }

        /// <summary>
        /// 覆う色
        /// </summary>
        public Color Color
        {
            get => _Color;
            set
            {
                // 覆っている最中でも色だけ差し替えられるよう、不透明度は維持する
                var alpha = _Image == null ? 0f : _Image.color.a;

                _Color = value;

                if (_Image != null)
                {
                    SetAlpha(alpha);
                }
            }
        }

        /// <summary>
        /// フェードにかける秒数。0にすると即座に切り替わります
        /// </summary>
        public float Duration
        {
            get => _Duration;
            set => _Duration = value > 0f ? value : 0f;
        }

        public Awaitable CoverAsync()
        {
            return FadeAsync(0f, 1f);
        }

        public Awaitable UncoverAsync()
        {
            return FadeAsync(1f, 0f);
        }

        private async Awaitable FadeAsync(float from, float to)
        {
            EnsureCanvas();

            _Image.enabled = true;
            _Image.raycastTarget = true;
            SetAlpha(from);

            if (_Duration > 0f)
            {
                var timer = new Timer(_Duration);

                while (timer.IsOverLimit == false)
                {
                    await Awaitable.NextFrameAsync();

                    timer.Tick(Time.unscaledDeltaTime);
                    SetAlpha(Mathf.Lerp(from, to, timer.Progress));
                }
            }

            SetAlpha(to);

            // 覆いを外し切ったら、描画も入力の遮断も止める
            if (to <= 0f)
            {
                _Image.enabled = false;
                _Image.raycastTarget = false;
            }
        }

        private void EnsureCanvas()
        {
            // ドメインリロードを無効化していると、破棄済みの参照が残っている事がある
            if (_Image != null)
            {
                return;
            }

            var root = new GameObject(_ObjectName,
                typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));

            Object.DontDestroyOnLoad(root);

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 他のUIより手前に出す
            canvas.sortingOrder = short.MaxValue;

            _Image = root.GetComponent<Image>();
            _Image.enabled = false;
            _Image.raycastTarget = false;

            var rect = _Image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            SetAlpha(0f);
        }

        private void SetAlpha(float alpha)
        {
            var color = _Color;

            color.a = alpha;
            _Image.color = color;
        }
    }
}
