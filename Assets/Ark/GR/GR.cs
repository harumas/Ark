using Ark.GUI;

namespace Ark
{
    /// <summary>
    /// シングルトンへの参照をまとめて公開するクラス (Global Reference)
    /// </summary>
    /// <remarks>
    /// それぞれのシングルトンが持つ<c>Instance</c>を直接呼ばず、ここを経由してアクセスします。
    /// プロジェクトにどんなシングルトンが存在するかを、このファイル1つで見渡せるようにするためのものです。
    /// </remarks>
    public static class GR
    {
        /// <summary>
        /// 画面の開閉を管理するシングルトン
        /// </summary>
        public static GUIManager GUIManager => GUIManager.Instance;
    }
}
