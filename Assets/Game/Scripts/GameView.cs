using UnityEngine;

namespace Game
{
    /// <summary>
    /// カメラとHUDの見た目をまとめたクラス
    /// </summary>
    /// <remarks>
    /// HUDはフォントアセットを用意せずに済ませるためIMGUIで描いています。
    /// テスト用のミニゲームなので、見た目より依存の少なさを優先しました。
    /// </remarks>
    internal static class GameView
    {
        public static readonly Color Background = new(0.09f, 0.10f, 0.14f);
        public static readonly Color PlayerColor = new(0.35f, 0.85f, 1f);
        public static readonly Color SlashColor = new(1f, 0.96f, 0.55f, 0.8f);
        public static readonly Color EnemyColor = new(1f, 0.42f, 0.45f);
        public static readonly Color WallColor = new(0.20f, 0.22f, 0.29f);
        public static readonly Color Accent = new(1f, 0.85f, 0.35f);

        private static GUIStyle _Style;

        /// <summary>
        /// 直交投影のカメラを作ります
        /// </summary>
        public static Camera CreateCamera()
        {
            var gameObject = new GameObject("Main Camera");
            var camera = gameObject.AddComponent<Camera>();

            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            gameObject.tag = "MainCamera";

            return camera;
        }

        /// <summary>
        /// 画面に収まる範囲の半分の大きさを返します
        /// </summary>
        public static Vector2 GetViewHalfSize(Camera camera)
        {
            var halfHeight = camera.orthographicSize;

            return new Vector2(halfHeight * camera.aspect, halfHeight);
        }

        /// <summary>
        /// IMGUIで文字を描きます
        /// </summary>
        public static void Draw(Rect rect, string text, int fontSize, TextAnchor anchor, Color color)
        {
            // GUI.skinはOnGUIの中でしか触れないため、初回描画時に作る
            _Style ??= new GUIStyle(GUI.skin.label);

            _Style.fontSize = fontSize;
            _Style.alignment = anchor;
            _Style.normal.textColor = color;
            _Style.wordWrap = false;

            GUI.Label(rect, text, _Style);
        }

        public static Rect Line(float y, float height = 44f)
        {
            return new Rect(0f, y, Screen.width, height);
        }
    }
}
