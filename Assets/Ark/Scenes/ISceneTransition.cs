using UnityEngine;

namespace Ark.Scenes
{
    /// <summary>
    /// シーン遷移の演出
    /// </summary>
    /// <remarks>
    /// <see cref="SceneLoader"/>は、シーンを読み込む前に<see cref="CoverAsync"/>で画面を覆い、
    /// 読み込みが終わってから<see cref="UncoverAsync"/>で覆いを外します。
    /// フェード以外の演出（ワイプ・ロゴ付きのローディング画面など）を作る場合はこれを実装してください。
    /// </remarks>
    public interface ISceneTransition
    {
        /// <summary>
        /// 画面を覆います。フェードなら暗転にあたります
        /// </summary>
        Awaitable CoverAsync();

        /// <summary>
        /// 画面の覆いを外します。フェードなら明転にあたります
        /// </summary>
        Awaitable UncoverAsync();
    }
}
