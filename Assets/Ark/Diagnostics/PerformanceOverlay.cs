using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ark.Diagnostics
{
    /// <summary>
    /// 画面左上にFPSとCPU / GPUのフレーム時間を表示するコンポーネント
    /// </summary>
    /// <remarks>
    /// <para>
    /// CPU / GPUの時間は<see cref="FrameTimingManager"/>から取得します。ビルドで計測するには
    /// Player Settingsの <b>Frame Timing Stats</b> を有効にしておく必要があります（エディタでは常に計測されます）。
    /// 取得できないプラットフォームやグラフィックスAPIの項目は「N/A」と表示します。
    /// </para>
    /// <para>
    /// FPSは<see cref="FpsInterval"/>秒ごとの平均を表示します。フレーム時間（CPU全体・メインスレッド・レンダースレッド・GPU）は
    /// 「直近1フレームの値(ave. <see cref="FrameTimeInterval"/>秒ごとの平均)」の形で表示します。
    /// 1フレームの値はばらつきが大きく、それだけでは傾向が読み取れないためです。
    /// </para>
    /// <para>
    /// 計測対象の負荷を増やさないよう、以下の工夫をしています。
    /// <list type="bullet">
    /// <item>専用のCanvasに置いたuGUIで表示し、文字が変わった時（<see cref="FpsInterval"/>秒ごと）しか作り直さない</item>
    /// <item>変わらないラベルと数値を別のTextに分け、作り直すのは数値だけにする</item>
    /// <item>1行の文字数を固定し、パネルの大きさを最初に決めておく事でレイアウトの再計算をなくす</item>
    /// <item>表示用の文字列は最初に確保した2本を交互に書き換えて使い回し、実行中の確保を0にする</item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class PerformanceOverlay : MonoBehaviour
    {
        /// <summary>
        /// FPSを更新する間隔（秒）
        /// </summary>
        public const float FpsInterval = 0.5f;

        /// <summary>
        /// フレーム時間の平均をとる間隔（秒）
        /// </summary>
        public const float FrameTimeInterval = 3f;

        private const string _Labels = "FPS\nCPU\n  Main\n  Render\nGPU";

        // パネルの幅を決めるための最も長くなる行。数字はどれも同じ幅なので0で代表させる
        private const string _WidestValueLine = "0000.00ms(ave. 0000.00ms)";

        private const int _LineCount = 5;
        private const int _LineLength = 25;
        private const int _ValueTextLength = _LineCount * _LineLength + (_LineCount - 1);

        // 1920x1080の時の大きさ。解像度が変わってもCanvasScalerで同じ見た目に保つ
        private const int _FontSize = 22;
        private const float _Margin = 8f;
        private const float _Padding = 6f;
        private const float _ColumnSpacing = 12f;

        private static PerformanceOverlay _Instance;

        private readonly FrameTiming[] _Timings = new FrameTiming[1];

        // 数値を組み立てる作業場所と、Textに渡す文字列2本。
        // 表示中の文字列を書き換えると、TextやTextGeneratorが「前回と同じ文字列」と判断して更新されなくなるため、
        // 必ず表示していない方に書き込んでから差し替える
        private readonly char[] _Buffer = new char[_ValueTextLength];
        private readonly string[] _ValueStrings = { new(' ', _ValueTextLength), new(' ', _ValueTextLength) };
        private int _DisplayedIndex;
        private int _Position;
        private int _LineEnd;

        private FrameTiming _LatestTiming;
        private GameObject _CanvasObject;
        private Text _ValueText;

        private float _FpsElapsedTime;
        private int _FrameCount;

        private float _FrameTimeElapsedTime;
        private bool _HasFrameTimeResult;
        private Average _CpuTime;
        private Average _CpuMainThreadTime;
        private Average _CpuRenderThreadTime;
        private Average _GpuTime;

        /// <summary>
        /// 直近<see cref="FpsInterval"/>秒の平均FPS
        /// </summary>
        public float Fps { get; private set; }

        /// <summary>
        /// 直近<see cref="FrameTimeInterval"/>秒の平均CPUフレーム時間（ミリ秒）。取得できない場合はnull
        /// </summary>
        public float? CpuFrameTime { get; private set; }

        /// <summary>
        /// 直近<see cref="FrameTimeInterval"/>秒の平均メインスレッド時間（ミリ秒）。取得できない場合はnull
        /// </summary>
        public float? CpuMainThreadFrameTime { get; private set; }

        /// <summary>
        /// 直近<see cref="FrameTimeInterval"/>秒の平均レンダースレッド時間（ミリ秒）。取得できない場合はnull
        /// </summary>
        public float? CpuRenderThreadFrameTime { get; private set; }

        /// <summary>
        /// 直近<see cref="FrameTimeInterval"/>秒の平均GPUフレーム時間（ミリ秒）。取得できない場合はnull
        /// </summary>
        public float? GpuFrameTime { get; private set; }

        /// <summary>
        /// オーバーレイを表示します。既に表示している場合はそれを返します
        /// </summary>
        /// <remarks>
        /// 生成したGameObjectはDontDestroyOnLoadされ、シーンをまたいで表示され続けます。
        /// </remarks>
        public static PerformanceOverlay Show()
        {
            if (_Instance != null)
            {
                return _Instance;
            }

            var owner = new GameObject(nameof(PerformanceOverlay));
            DontDestroyOnLoad(owner);

            return owner.AddComponent<PerformanceOverlay>();
        }

        private void Awake()
        {
            // シーンに直接アタッチされたものと Show() で作ったものが重ならないようにする
            if (_Instance != null && _Instance != this)
            {
                Destroy(this);
                return;
            }

            _Instance = this;

            CreateView();
            RebuildText();
        }

        private void OnDestroy()
        {
            // ドメインリロードを無効化している場合、破棄済みの参照が残り続けるのを防ぐ
            if (_Instance == this)
            {
                _Instance = null;
            }

            // コンポーネントだけ外された場合にも表示が残らないようにする
            if (_CanvasObject != null)
            {
                Destroy(_CanvasObject);
            }
        }

        private void Update()
        {
            var deltaTime = Time.unscaledDeltaTime;

            SampleFrameTiming();

            _FrameTimeElapsedTime += deltaTime;

            if (_FrameTimeElapsedTime >= FrameTimeInterval)
            {
                CpuFrameTime = _CpuTime.TakeResult();
                CpuMainThreadFrameTime = _CpuMainThreadTime.TakeResult();
                CpuRenderThreadFrameTime = _CpuRenderThreadTime.TakeResult();
                GpuFrameTime = _GpuTime.TakeResult();

                _FrameTimeElapsedTime = 0f;
                _HasFrameTimeResult = true;
            }

            _FpsElapsedTime += deltaTime;
            _FrameCount++;

            if (_FpsElapsedTime < FpsInterval)
            {
                return;
            }

            Fps = _FrameCount / _FpsElapsedTime;
            RebuildText();

            _FpsElapsedTime = 0f;
            _FrameCount = 0;
        }

        private void CreateView()
        {
            // 他のCanvasと分けておく事で、文字の更新が他のUIの作り直しを引き起こさない
            _CanvasObject = new GameObject("PerformanceOverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            _CanvasObject.transform.SetParent(transform, false);

            var canvas = _CanvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 他のUIより手前に出す
            canvas.sortingOrder = short.MaxValue;

            var scaler = _CanvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            var background = CreateTopLeftElement<Image>("Background", _CanvasObject.transform, new Vector2(_Margin, -_Margin));
            background.color = new Color(0f, 0f, 0f, 0.6f);

            var labelText = CreateText("Labels", background.transform, new Vector2(_Padding, -_Padding));
            labelText.text = _Labels;

            var labelSize = new Vector2(labelText.preferredWidth, labelText.preferredHeight);
            labelText.rectTransform.sizeDelta = labelSize;

            _ValueText = CreateText("Values", background.transform, new Vector2(_Padding + labelSize.x + _ColumnSpacing, -_Padding));
            _ValueText.text = _WidestValueLine;

            var valueSize = new Vector2(_ValueText.preferredWidth, labelSize.y);
            _ValueText.rectTransform.sizeDelta = valueSize;

            background.rectTransform.sizeDelta = new Vector2(
                _Padding * 2f + labelSize.x + _ColumnSpacing + valueSize.x,
                _Padding * 2f + labelSize.y);
        }

        private static Text CreateText(string name, Transform parent, Vector2 position)
        {
            var text = CreateTopLeftElement<Text>(name, parent, position);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = _FontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.supportRichText = false;

            // 想定より長い値が来ても、折り返して行がずれないようにする
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        private static T CreateTopLeftElement<T>(string name, Transform parent, Vector2 position) where T : Graphic
        {
            var element = new GameObject(name, typeof(RectTransform), typeof(T));
            element.transform.SetParent(parent, false);

            var rect = (RectTransform)element.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;

            var graphic = element.GetComponent<T>();
            graphic.raycastTarget = false;

            return graphic;
        }

        private void SampleFrameTiming()
        {
            FrameTimingManager.CaptureFrameTimings();

            if (FrameTimingManager.GetLatestTimings(1, _Timings) == 0)
            {
                return;
            }

            var timing = _Timings[0];

            _LatestTiming = timing;

            _CpuTime.Add(timing.cpuFrameTime);
            _CpuMainThreadTime.Add(timing.cpuMainThreadFrameTime);
            _CpuRenderThreadTime.Add(timing.cpuRenderThreadFrameTime);
            _GpuTime.Add(timing.gpuFrameTime);
        }

        private void RebuildText()
        {
            _Position = 0;

            BeginLine();

            if (Fps > 0f)
            {
                PutFixed(Fps, 1);
            }
            else
            {
                Put("--");
            }

            PutTimeLine(_LatestTiming.cpuFrameTime, CpuFrameTime);
            PutTimeLine(_LatestTiming.cpuMainThreadFrameTime, CpuMainThreadFrameTime);
            PutTimeLine(_LatestTiming.cpuRenderThreadFrameTime, CpuRenderThreadFrameTime);
            PutTimeLine(_LatestTiming.gpuFrameTime, GpuFrameTime);

            EndLine();

            ApplyBuffer();
        }

        /// <summary>
        /// 「1フレームの時間ms(ave. 平均ms)」の行を追記します
        /// </summary>
        private void PutTimeLine(double latest, float? average)
        {
            EndLine();
            _Buffer[_Position++] = '\n';
            BeginLine();

            // 計測できなかった項目は0になる
            PutMilliseconds(latest > 0d ? (double?)latest : null);

            Put("(ave. ");

            // 最初の平均が出るまでは計測中として扱う
            if (_HasFrameTimeResult)
            {
                PutMilliseconds(average);
            }
            else
            {
                Put("--");
            }

            Put(')');
        }

        private void PutMilliseconds(double? milliseconds)
        {
            if (milliseconds.HasValue == false)
            {
                Put("N/A");
                return;
            }

            PutFixed(milliseconds.Value, 2);
            Put("ms");
        }

        /// <summary>
        /// 0以上の値を小数点以下の桁数を固定して追記します
        /// </summary>
        /// <remarks>
        /// float.ToString() は文字列を確保するため、1文字ずつ組み立てます。
        /// </remarks>
        private void PutFixed(double value, int decimals)
        {
            var scale = 1L;

            for (var i = 0; i < decimals; i++)
            {
                scale *= 10L;
            }

            var scaled = (long)(value * scale + 0.5d);

            PutInteger(scaled / scale);
            Put('.');

            var fraction = scaled % scale;

            for (var divisor = scale / 10L; divisor > 0L; divisor /= 10L)
            {
                Put((char)('0' + fraction / divisor % 10L));
            }
        }

        private void PutInteger(long value)
        {
            if (value >= 10L)
            {
                PutInteger(value / 10L);
            }

            Put((char)('0' + value % 10L));
        }

        private void Put(string value)
        {
            foreach (var character in value)
            {
                Put(character);
            }
        }

        private void Put(char character)
        {
            // 1行の長さを超えた分は捨てる。文字列全体の長さが変わらない事が使い回しの前提
            if (_Position < _LineEnd)
            {
                _Buffer[_Position++] = character;
            }
        }

        private void BeginLine()
        {
            _LineEnd = _Position + _LineLength;
        }

        private void EndLine()
        {
            // 行の残りを空白で埋めて、全ての行を同じ長さにする
            while (_Position < _LineEnd)
            {
                _Buffer[_Position++] = ' ';
            }
        }

        /// <summary>
        /// 組み立てた文字を、表示していない方の文字列に書き込んでTextに渡します
        /// </summary>
        /// <remarks>
        /// .NETの文字列は不変という約束を破り、確保済みの文字列の中身を直接書き換えます。
        /// 書き換える文字列はこのクラスの中でしか作っておらず、Dictionaryのキーなどにも使われないため安全です。
        /// </remarks>
        private unsafe void ApplyBuffer()
        {
            // 前回と同じなら差し替えない。差し替えずに書き込み先だけ進めると、表示中の文字列を書き換えてしまう
            if (_Buffer.AsSpan().SequenceEqual(_ValueStrings[_DisplayedIndex].AsSpan()))
            {
                return;
            }

            _DisplayedIndex ^= 1;

            var target = _ValueStrings[_DisplayedIndex];

            fixed (char* source = _Buffer)
            fixed (char* destination = target)
            {
                var size = _ValueTextLength * sizeof(char);
                Buffer.MemoryCopy(source, destination, size, size);
            }

            // 中身の違う別の文字列になっているので、Textは通常どおり更新を検知する
            _ValueText.text = target;
        }

        /// <summary>
        /// 計測できたフレームだけを平均する
        /// </summary>
        private struct Average
        {
            private double _Total;
            private int _Count;

            public void Add(double value)
            {
                // 計測できなかった項目は0になる
                if (value <= 0d)
                {
                    return;
                }

                _Total += value;
                _Count++;
            }

            /// <summary>
            /// 平均を返してリセットします。1度も計測できなかった場合はnull
            /// </summary>
            public float? TakeResult()
            {
                float? result = _Count > 0 ? (float)(_Total / _Count) : null;

                _Total = 0d;
                _Count = 0;

                return result;
            }
        }
    }
}
