using System.Collections.Generic;
using Ark;
using Ark.Input;
using Ark.Scenes;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// ゲーム本編。シーンにはこれ1つだけ置いてあり、残りはコードから組み立てます
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const float _TimeLimit = 60f;
        private const float _SpawnIntervalMax = 1.1f;
        private const float _SpawnIntervalMin = 0.28f;
        private const float _EnemySpeedMin = 1.8f;
        private const float _EnemySpeedMax = 3.4f;

        private readonly List<EnemyActor> _Enemies = new();
        private readonly SubscriptionBag _Bag = new();
        private readonly GamepadVibrator _Vibrator = new();

        private Timer _GameTimer = new(_TimeLimit);
        private Timer _SpawnTimer = new(_SpawnIntervalMax, autoReset: true);

        private PlayerActor _Player;
        private Vector2 _ViewHalfSize;
        private int _Score;
        private bool _IsFinished;

        private void Awake()
        {
            var camera = GameView.CreateCamera();

            _ViewHalfSize = GameView.GetViewHalfSize(camera);

            CreateWalls();
            CreatePlayer();

            _Vibrator.Enable();
        }

        private void OnDestroy()
        {
            _Bag.Dispose();
            _Vibrator.Dispose();

            foreach (var enemy in _Enemies)
            {
                enemy.Destroy();
            }

            _Enemies.Clear();
        }

        private void Update()
        {
            if (_IsFinished)
            {
                return;
            }

            var deltaTime = Time.deltaTime;

            _GameTimer.Tick(deltaTime);

            if (_GameTimer.IsOverLimit)
            {
                Finish(true);
                return;
            }

            // 時間が経つほど湧きを速くする
            _SpawnTimer.Limit = Mathf.Lerp(_SpawnIntervalMax, _SpawnIntervalMin, _GameTimer.Progress);
            _SpawnTimer.Tick(deltaTime);

            if (_SpawnTimer.IsOverLimit)
            {
                SpawnEnemy();
            }

            UpdateEnemies(deltaTime);
        }

        private void CreatePlayer()
        {
            var gameObject = new GameObject("Player");

            _Player = gameObject.AddComponent<PlayerActor>();
            _Player.Setup(_ViewHalfSize);

            _Player.Attacked.Subscribe(OnAttacked).AddTo(_Bag);
            _Player.Damaged.Subscribe(OnDamaged).AddTo(_Bag);
        }

        private void CreateWalls()
        {
            var thickness = 0.14f;
            var size = _ViewHalfSize;

            SpriteFactory.CreateQuad("WallTop", GameView.WallColor, new Vector2(size.x * 2f, thickness), 1)
                .transform.position = new Vector3(0f, size.y, 0f);
            SpriteFactory.CreateQuad("WallBottom", GameView.WallColor, new Vector2(size.x * 2f, thickness), 1)
                .transform.position = new Vector3(0f, -size.y, 0f);
            SpriteFactory.CreateQuad("WallLeft", GameView.WallColor, new Vector2(thickness, size.y * 2f), 1)
                .transform.position = new Vector3(-size.x, 0f, 0f);
            SpriteFactory.CreateQuad("WallRight", GameView.WallColor, new Vector2(thickness, size.y * 2f), 1)
                .transform.position = new Vector3(size.x, 0f, 0f);
        }

        private void SpawnEnemy()
        {
            // 画面の外周から湧かせる
            var margin = 1.2f;
            var position = Random.value < 0.5f
                ? new Vector2(Random.Range(-_ViewHalfSize.x, _ViewHalfSize.x),
                    Random.value < 0.5f ? _ViewHalfSize.y + margin : -_ViewHalfSize.y - margin)
                : new Vector2(Random.value < 0.5f ? _ViewHalfSize.x + margin : -_ViewHalfSize.x - margin,
                    Random.Range(-_ViewHalfSize.y, _ViewHalfSize.y));

            var speed = Mathf.Lerp(_EnemySpeedMin, _EnemySpeedMax, _GameTimer.Progress) * Random.Range(0.85f, 1.15f);

            _Enemies.Add(new EnemyActor(position, speed));
        }

        private void UpdateEnemies(float deltaTime)
        {
            var playerPosition = (Vector2)_Player.transform.position;

            // 破棄しながら走査するため、後ろから見ていく
            for (var i = _Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _Enemies[i];

                enemy.Update(playerPosition, deltaTime);

                var distance = Vector2.Distance(enemy.Position, playerPosition);

                if (_Player.IsAttacking && distance <= PlayerActor.AttackRange * 0.5f + EnemyActor.Radius)
                {
                    Kill(i, enemy);
                    continue;
                }

                if (distance <= PlayerActor.Radius + EnemyActor.Radius && _Player.TryTakeDamage())
                {
                    Kill(i, enemy);
                }
            }
        }

        private void Kill(int index, EnemyActor enemy)
        {
            enemy.Destroy();
            _Enemies.RemoveAt(index);

            _Score++;
        }

        private void OnAttacked()
        {
            _Vibrator.Vibrate(0.15f, 0.15f, 0.05f);
        }

        private void OnDamaged()
        {
            _Vibrator.Vibrate(0.8f, 0.8f, 0.18f);

            if (_Player.Hp <= 0)
            {
                Finish(false);
            }
        }

        private void Finish(bool isSurvived)
        {
            if (_IsFinished)
            {
                return;
            }

            _IsFinished = true;

            GameSession.Submit(_Score, _GameTimer.Elapsed, isSurvived);

            SceneLoader.LoadAsync(SceneNames.Result).LogExceptionsAndForget();
        }

        private void OnGUI()
        {
            var remaining = Mathf.CeilToInt(_GameTimer.Remaining);

            GameView.Draw(new Rect(24f, 18f, 400f, 40f), $"SCORE  {_Score}", 30, TextAnchor.UpperLeft, Color.white);
            GameView.Draw(new Rect(24f, 58f, 400f, 34f), $"HP  {new string('■', Mathf.Max(_Player.Hp, 0))}", 26,
                TextAnchor.UpperLeft, GameView.EnemyColor);

            GameView.Draw(new Rect(Screen.width - 224f, 18f, 200f, 40f), $"{remaining}", 34, TextAnchor.UpperRight,
                remaining <= 10 ? GameView.Accent : Color.white);

            var dash = _Player.DashCharge >= 1f ? "DASH READY" : "DASH ...";

            GameView.Draw(new Rect(Screen.width - 224f, 58f, 200f, 30f), dash, 20, TextAnchor.UpperRight,
                _Player.DashCharge >= 1f ? GameView.PlayerColor : Color.gray);

            GameView.Draw(new Rect(0f, Screen.height - 40f, Screen.width, 30f),
                "移動: WASD / 左スティック    攻撃: マウス左 / 右トリガー    ダッシュ: Shift / 左スティック押し込み",
                17, TextAnchor.UpperCenter, new Color(1f, 1f, 1f, 0.45f));
        }
    }
}
