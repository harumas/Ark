using System;
using System.Collections.Generic;

namespace Ark
{
    /// <summary>
    /// 複数の購読をまとめて解除するためのコンテナ
    /// </summary>
    public sealed class SubscriptionBag : IDisposable
    {
        private readonly List<Subscription> _Subscriptions = new();
        private bool _Disposed;

        /// <summary>
        /// 保持している購読の数
        /// </summary>
        public int Count => _Subscriptions.Count;

        /// <summary>
        /// 既に破棄されているかどうか
        /// </summary>
        public bool IsDisposed => _Disposed;

        /// <summary>
        /// 購読を追加します
        /// </summary>
        public void Add(Subscription subscription)
        {
            // 破棄済みのBagへ渡された購読は、解除漏れにならないようその場で解除する
            if (_Disposed)
            {
                subscription.Dispose();
                return;
            }

            _Subscriptions.Add(subscription);
        }

        /// <summary>
        /// 保持している購読を全て解除します。Bagは引き続き使用できます
        /// </summary>
        public void Clear()
        {
            foreach (var subscription in _Subscriptions)
            {
                subscription.Dispose();
            }

            _Subscriptions.Clear();
        }

        /// <summary>
        /// 保持している購読を全て解除し、このBagを破棄します
        /// </summary>
        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            _Disposed = true;

            Clear();
        }
    }
}
