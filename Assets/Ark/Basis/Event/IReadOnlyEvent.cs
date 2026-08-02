using System;

namespace Ark
{
    /// <summary>
    /// 購読のみを公開するイベント
    /// </summary>
    /// <remarks>
    /// 発火する<see cref="Event.Execute"/>を持たないため、
    /// このインターフェースで公開しておけば外部から勝手に発火される事がありません。
    /// </remarks>
    public interface IReadOnlyEvent
    {
        /// <summary>
        /// 現在の購読数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// イベントを購読します
        /// </summary>
        /// <returns>解除に使用するハンドル</returns>
        Subscription Subscribe(Action action);
    }

    /// <inheritdoc cref="IReadOnlyEvent"/>
    /// <typeparam name="T">発火時に渡す値の型</typeparam>
    public interface IReadOnlyEvent<T>
    {
        /// <summary>
        /// 現在の購読数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// イベントを購読します
        /// </summary>
        /// <returns>解除に使用するハンドル</returns>
        Subscription Subscribe(Action<T> action);
    }
}
