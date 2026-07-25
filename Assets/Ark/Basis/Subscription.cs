using System;

namespace Ark
{
    /// <summary>
    /// 購読を解除するためのハンドル
    /// </summary>
    /// <remarks>
    /// 構造体のため、購読ごとの確保は発生しません。
    /// <see cref="Dispose"/>は何度呼び出しても安全で、解除済みのハンドルが
    /// 別の購読を巻き込んで解除する事はありません。
    /// </remarks>
    public readonly struct Subscription : IDisposable, IEquatable<Subscription>
    {
        private readonly ISubscriptionSource _Source;
        private readonly int _Index;
        private readonly int _Id;

        public Subscription(ISubscriptionSource source, int index, int id)
        {
            _Source = source;
            _Index = index;
            _Id = id;
        }

        /// <summary>
        /// 解除対象を持たない空のハンドル
        /// </summary>
        public static Subscription Empty => default;

        /// <summary>
        /// 購読がまだ有効かどうか
        /// </summary>
        public bool IsActive => _Source != null && _Source.IsSubscribed(_Index, _Id);

        /// <summary>
        /// 購読を解除します
        /// </summary>
        public void Dispose()
        {
            _Source?.Unsubscribe(_Index, _Id);
        }

        public bool Equals(Subscription other)
        {
            return ReferenceEquals(_Source, other._Source) && _Index == other._Index && _Id == other._Id;
        }

        public override bool Equals(object obj)
        {
            return obj is Subscription other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_Source, _Index, _Id);
        }
    }

    /// <summary>
    /// <see cref="Subscription"/>の解除先となるイベント
    /// </summary>
    public interface ISubscriptionSource
    {
        /// <summary>
        /// 指定した購読がまだ有効かどうかを返します
        /// </summary>
        bool IsSubscribed(int index, int id);

        /// <summary>
        /// 指定した購読を解除します。既に解除済みの場合は何もしません
        /// </summary>
        void Unsubscribe(int index, int id);
    }
}
