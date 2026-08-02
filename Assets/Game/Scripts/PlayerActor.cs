using Ark;
using Ark.Input;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 操作キャラクター
    /// </summary>
    /// <remarks>
    /// 名前を<c>Player</c>にしないのは、生成される<c>Ark.Input.Player</c>と衝突するためです。
    /// </remarks>
    public sealed class PlayerActor : MonoBehaviour
    {
        public const float Radius = 0.34f;
        public const float AttackRange = 1.8f;
        public const int MaxHp = 3;

        private const float _MoveSpeed = 6.5f;
        private const float _DashSpeed = 19f;

        // Ark.Event は UnityEngine.Event（IMGUI）と名前が衝突するため、明示的に書く
        private readonly Ark.Event _Attacked = new();
        private readonly Ark.Event _Damaged = new();
        private readonly SubscriptionBag _Bag = new();

        private Timer _AttackCooldown = new(0.28f);
        private Timer _AttackSwing = new(0.12f);
        private Timer _DashCooldown = new(0.85f);
        private Timer _DashDuration = new(0.13f);
        private Timer _Invincible = new(1.1f);

        private InputEvent _MoveInput;
        private InputEvent _AttackInput;
        private InputEvent _DashInput;

        private SpriteRenderer _Body;
        private SpriteRenderer _Slash;
        private Vector2 _Facing = Vector2.right;
        private Vector2 _Bounds = new(8f, 4.4f);

        /// <summary>
        /// 攻撃を出した瞬間に通知します
        /// </summary>
        public IReadOnlyEvent Attacked => _Attacked;

        /// <summary>
        /// ダメージを受けた瞬間に通知します
        /// </summary>
        public IReadOnlyEvent Damaged => _Damaged;

        public int Hp { get; private set; } = MaxHp;

        public Vector2 Facing => _Facing;

        /// <summary>
        /// 攻撃判定が出ているかどうか
        /// </summary>
        public bool IsAttacking => _AttackSwing.IsOverLimit == false;

        /// <summary>
        /// 無敵時間中かどうか
        /// </summary>
        public bool IsInvincible => _Invincible.IsOverLimit == false;

        /// <summary>
        /// ダッシュが使えるようになるまでの進捗 (0 ~ 1)
        /// </summary>
        public float DashCharge => _DashCooldown.Progress;

        private void Awake()
        {
            _Body = SpriteFactory.CreateQuad("Body", GameView.PlayerColor, new Vector2(Radius * 2f, Radius * 2f), 10);
            _Body.transform.SetParent(transform, false);

            _Slash = SpriteFactory.CreateQuad("Slash", GameView.SlashColor, new Vector2(AttackRange, 0.22f), 9);
            _Slash.transform.SetParent(transform, false);
            _Slash.enabled = false;

            // 上限を超えた状態 = 待機が明けた状態、として扱う
            MakeReady(ref _AttackCooldown);
            MakeReady(ref _AttackSwing);
            MakeReady(ref _DashCooldown);
            MakeReady(ref _DashDuration);
            MakeReady(ref _Invincible);

            _MoveInput = InputProvider.CreateEvent(ActionGuid.Player.Move);
            _AttackInput = InputProvider.CreateEvent(ActionGuid.Player.Attack);
            _DashInput = InputProvider.CreateEvent(ActionGuid.Player.Sprint);

            _AttackInput.Performed.Subscribe(_ => TryAttack()).AddTo(_Bag);
            _DashInput.Performed.Subscribe(_ => TryDash()).AddTo(_Bag);
        }

        private void OnDestroy()
        {
            _Bag.Dispose();

            // InputProviderが保持している分も、ここで解放しておく
            _MoveInput?.Dispose();
            _AttackInput?.Dispose();
            _DashInput?.Dispose();

            _Attacked.Dispose();
            _Damaged.Dispose();
        }

        public void Setup(Vector2 viewHalfSize)
        {
            _Bounds = viewHalfSize - new Vector2(Radius, Radius);
        }

        /// <summary>
        /// ダメージを与えます。無敵時間中は無視されます
        /// </summary>
        /// <returns>実際にダメージが入ったかどうか</returns>
        public bool TryTakeDamage()
        {
            if (IsInvincible || Hp <= 0)
            {
                return false;
            }

            Hp--;
            _Invincible.Reset();
            _Damaged.Execute();

            return true;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            _AttackCooldown.Tick(deltaTime);
            _AttackSwing.Tick(deltaTime);
            _DashCooldown.Tick(deltaTime);
            _DashDuration.Tick(deltaTime);
            _Invincible.Tick(deltaTime);

            UpdateMove(deltaTime);
            UpdateVisual();
        }

        private void UpdateMove(float deltaTime)
        {
            var input = Vector2.ClampMagnitude(_MoveInput.ReadValue<Vector2>(), 1f);

            if (input.sqrMagnitude > 0.01f)
            {
                _Facing = input.normalized;
            }

            // ダッシュ中は入力に関係なく、踏み切った向きへ進む
            var isDashing = _DashDuration.IsOverLimit == false;
            var velocity = isDashing ? _Facing * _DashSpeed : input * _MoveSpeed;

            var position = transform.position + (Vector3)(velocity * deltaTime);

            position.x = Mathf.Clamp(position.x, -_Bounds.x, _Bounds.x);
            position.y = Mathf.Clamp(position.y, -_Bounds.y, _Bounds.y);

            transform.position = position;
        }

        private void UpdateVisual()
        {
            _Slash.enabled = IsAttacking;

            if (IsAttacking)
            {
                var angle = Mathf.Atan2(_Facing.y, _Facing.x) * Mathf.Rad2Deg;

                _Slash.transform.localPosition = _Facing * (AttackRange * 0.5f);
                _Slash.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            // 無敵時間中は点滅させる
            var color = GameView.PlayerColor;

            color.a = IsInvincible ? Mathf.PingPong(Time.time * 8f, 0.7f) + 0.3f : 1f;
            _Body.color = color;
        }

        private void TryAttack()
        {
            if (_AttackCooldown.IsOverLimit == false)
            {
                return;
            }

            _AttackCooldown.Reset();
            _AttackSwing.Reset();
            _Attacked.Execute();
        }

        private void TryDash()
        {
            if (_DashCooldown.IsOverLimit == false)
            {
                return;
            }

            _DashCooldown.Reset();
            _DashDuration.Reset();
        }

        private static void MakeReady(ref Timer timer)
        {
            timer.Tick(timer.Limit);
        }
    }
}
