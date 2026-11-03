using Ark.Diagnostics;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 開発中はどのシーンから再生してもパフォーマンス表示が出るようにする
    /// </summary>
    internal static class PerformanceOverlayInstaller
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            PerformanceOverlay.Show();
        }
#endif
    }
}
