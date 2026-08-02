using UnityEngine;

namespace Game
{
    /// <summary>
    /// プレイヤーを追いかける敵
    /// </summary>
    /// <remarks>
    /// 数が増えるためMonoBehaviourにはせず、<see cref="GameBootstrap"/>からまとめて更新します。
    /// </remarks>
    internal sealed class EnemyActor
    {
        public const float Radius = 0.3f;

        private const float _SpawnScale = 0.2f;

        private readonly SpriteRenderer _Renderer;
        private readonly Transform _Transform;
        private readonly float _Speed;

        private float _Appearance;

        public EnemyActor(Vector2 position, float speed)
        {
            _Speed = speed;
            _Renderer = SpriteFactory.CreateQuad("Enemy", GameView.EnemyColor, Vector2.one, 5);
            _Transform = _Renderer.transform;
            _Transform.position = position;

            ApplyScale();
        }

        public Vector2 Position => _Transform.position;

        public void Update(Vector2 target, float deltaTime)
        {
            // 出現直後は小さく、少しの間で通常の大きさになる
            if (_Appearance < 1f)
            {
                _Appearance = Mathf.Min(_Appearance + deltaTime * 4f, 1f);
                ApplyScale();
            }

            var toTarget = target - Position;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                _Transform.position += (Vector3)(toTarget.normalized * (_Speed * deltaTime));
            }
        }

        public void Destroy()
        {
            if (_Renderer != null)
            {
                Object.Destroy(_Renderer.gameObject);
            }
        }

        private void ApplyScale()
        {
            var size = Mathf.Lerp(_SpawnScale, 1f, _Appearance) * (Radius * 2f);

            _Transform.localScale = new Vector3(size, size, 1f);
        }
    }
}
