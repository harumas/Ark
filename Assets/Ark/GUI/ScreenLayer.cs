namespace Ark.GUI
{
    /// <summary>
    /// 画面（Canvas）の重ね順を決める区分
    /// </summary>
    /// <remarks>
    /// 数値が大きいほど手前に描画されます。同じ層の中で複数の画面を開いた場合は、
    /// 後から開いた方が手前に来ます。
    /// </remarks>
    public enum ScreenLayer
    {
        /// <summary>常に一番奥。HUDなど常駐する表示に使います</summary>
        Background,

        /// <summary>通常のメニュー画面</summary>
        Screen,

        /// <summary>確認ダイアログなど、Screenより手前に出したいもの</summary>
        Popup,

        /// <summary>常に一番手前。ツールチップや警告表示に使います</summary>
        Tooltip
    }
}
