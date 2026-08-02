using System;
using UnityEngine;

namespace Ark
{
    /// <summary>
    /// 経過時間を計測し、上限値を超えたかどうかを保持するタイマー
    /// </summary>
    /// <remarks>
    /// 確保を伴わない構造体です。<see cref="Tick"/>を呼んだぶんだけ時間が進むため、
    /// 一時停止したい場合は<see cref="Tick"/>を呼ばないでください。
    /// <c>Time.deltaTime</c>と<c>Time.unscaledDeltaTime</c>のどちらで進めるかは渡す値で選べます。
    /// <para>
    /// <see cref="IsOverLimit"/>は超えた時点では下ろさず、次に<see cref="Tick"/>を呼ぶまで
    /// 立てたままにします。これにより、超過を必ず外部から観測できます。
    /// <see cref="AutoReset"/>が有効なら次の<see cref="Tick"/>で自動的に計測し直し、
    /// 無効なら<see cref="Reset"/>を呼ぶまで超過したまま止まります。
    /// </para>
    /// <para>
    /// 構造体のため<c>readonly</c>フィールドやプロパティ経由で保持すると、
    /// <see cref="Tick"/>などの変更を伴う呼び出しがコピーに対して行われ、
    /// 進まなくなります。必ず<c>readonly</c>を付けないフィールドで保持してください。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeField] private Timer _Spawn = new Timer(0.5f, autoReset: true);
    ///
    /// private void Update()
    /// {
    ///     _Spawn.Tick(Time.deltaTime);
    ///
    ///     // 判定は必ずTickの後に行う
    ///     if (_Spawn.IsOverLimit) Spawn();
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public struct Timer
    {
        [SerializeField] private float _Limit;
        [SerializeField] private bool _AutoReset;

        private float _Elapsed;
        private bool _IsOverLimit;
        private int _OverLimitCount;

        /// <param name="limit">上限値となる秒数。0より大きい必要があります</param>
        /// <param name="autoReset">上限を超えた後、次の<see cref="Tick"/>で計測し直すかどうか</param>
        public Timer(float limit, bool autoReset = false)
        {
            ThrowIfInvalidLimit(limit);

            _Limit = limit;
            _AutoReset = autoReset;
            _Elapsed = 0f;
            _IsOverLimit = false;
            _OverLimitCount = 0;
        }

        /// <summary>
        /// 上限値となる秒数。計測中でも変更できます
        /// </summary>
        public float Limit
        {
            readonly get => _Limit;
            set
            {
                ThrowIfInvalidLimit(value);

                _Limit = value;
            }
        }

        /// <summary>
        /// 上限を超えた後、次の<see cref="Tick"/>で計測し直すかどうか
        /// </summary>
        /// <remarks>
        /// 無効の場合、超過した状態で止まり続けます。<see cref="Reset"/>で計測し直してください。
        /// </remarks>
        public bool AutoReset
        {
            readonly get => _AutoReset;
            set => _AutoReset = value;
        }

        /// <summary>
        /// 上限を超えているかどうか
        /// </summary>
        /// <remarks>
        /// 超えた時点で立ち、次に<see cref="Tick"/>を呼ぶまで下りません。
        /// </remarks>
        public readonly bool IsOverLimit => _IsOverLimit;

        /// <summary>
        /// 計測し直してからの経過秒数
        /// </summary>
        public readonly float Elapsed => _Elapsed;

        /// <summary>
        /// 上限までの残り秒数
        /// </summary>
        public readonly float Remaining => _Limit > _Elapsed ? _Limit - _Elapsed : 0f;

        /// <summary>
        /// 上限までの進捗 (0 ~ 1)
        /// </summary>
        public readonly float Progress
        {
            get
            {
                if (_Limit > 0f == false)
                {
                    return 0f;
                }

                return _Elapsed >= _Limit ? 1f : _Elapsed / _Limit;
            }
        }

        /// <summary>
        /// 上限を超えた回数
        /// </summary>
        /// <remarks>
        /// <see cref="Reset"/>で0に戻ります。
        /// </remarks>
        public readonly int OverLimitCount => _OverLimitCount;

        /// <summary>
        /// 経過と超過フラグを消して、最初から計測し直します
        /// </summary>
        public void Reset()
        {
            _Elapsed = 0f;
            _IsOverLimit = false;
            _OverLimitCount = 0;
        }

        /// <summary>
        /// 時間を進めます
        /// </summary>
        /// <param name="deltaTime">前回からの経過秒数</param>
        /// <exception cref="ArgumentOutOfRangeException">負の値を渡した場合</exception>
        /// <exception cref="InvalidOperationException"><see cref="Limit"/>が未設定の場合</exception>
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "負の値は渡せません。");
            }

            // 上限が0だと毎回超過扱いになるため、黙って進めずに知らせる
            if (_Limit > 0f == false)
            {
                throw new InvalidOperationException(
                    $"{nameof(Limit)}が設定されていません。コンストラクタかInspectorで0より大きい秒数を指定してください。");
            }

            // 超過フラグを立てたまま1周ぶん観測させたいので、リセットは超過した次のTickで行う
            if (_IsOverLimit)
            {
                if (_AutoReset == false)
                {
                    return;
                }

                // 超過分は次の計測へ持ち越す。持ち越しが上限を超える場合は捨てる
                _Elapsed -= _Limit;

                if (_Elapsed < 0f || _Elapsed >= _Limit)
                {
                    _Elapsed = 0f;
                }

                _IsOverLimit = false;
            }

            if (deltaTime == 0f)
            {
                return;
            }

            _Elapsed += deltaTime;

            if (_Elapsed >= _Limit)
            {
                _IsOverLimit = true;
                _OverLimitCount++;
            }
        }

        private static void ThrowIfInvalidLimit(float limit)
        {
            if (limit > 0f == false)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), limit, "0より大きい値である必要があります。");
            }
        }
    }
}
