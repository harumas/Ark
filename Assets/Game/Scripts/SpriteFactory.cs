using UnityEngine;

namespace Game
{
    /// <summary>
    /// スプライトを持たずに済ませるため、矩形をコードから作るクラス
    /// </summary>
    internal static class SpriteFactory
    {
        private static Sprite _Square;

        /// <summary>
        /// 1辺1ユニットの白い矩形
        /// </summary>
        public static Sprite Square
        {
            get
            {
                // 再生を止めると破棄されるため、都度確認して作り直す
                if (_Square != null)
                {
                    return _Square;
                }

                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    hideFlags = HideFlags.DontSave
                };

                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                _Square = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                _Square.hideFlags = HideFlags.DontSave;

                return _Square;
            }
        }

        /// <summary>
        /// 矩形を描画するGameObjectを作ります
        /// </summary>
        public static SpriteRenderer CreateQuad(string name, Color color, Vector2 size, int sortingOrder)
        {
            var gameObject = new GameObject(name);
            var renderer = gameObject.AddComponent<SpriteRenderer>();

            renderer.sprite = Square;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            return renderer;
        }
    }
}
