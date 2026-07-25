using System;
using UnityEngine;

namespace Ark
{
    /// <summary>
    /// 購読の寿命を他のオブジェクトへ紐付ける拡張メソッド
    /// </summary>
    public static class SubscriptionExtensions
    {
        /// <summary>
        /// 購読を<see cref="SubscriptionBag"/>へ預け、Bagの破棄に合わせて解除されるようにします
        /// </summary>
        public static Subscription AddTo(this Subscription subscription, SubscriptionBag bag)
        {
            if (bag == null)
            {
                // 預け先がない購読は解除漏れになるため、その場で解除してから知らせる
                subscription.Dispose();
                throw new ArgumentNullException(nameof(bag));
            }

            bag.Add(subscription);

            return subscription;
        }

        /// <summary>
        /// 購読をGameObjectへ紐付け、GameObjectの破棄に合わせて解除されるようにします
        /// </summary>
        public static Subscription AddTo(this Subscription subscription, GameObject gameObject)
        {
            if (gameObject == null)
            {
                subscription.Dispose();
                throw new ArgumentNullException(nameof(gameObject));
            }

            SubscriptionDisposer.GetOrAdd(gameObject).Bag.Add(subscription);

            return subscription;
        }

        /// <summary>
        /// 購読をComponentの属するGameObjectへ紐付けます
        /// </summary>
        /// <remarks>
        /// 解除のきっかけはComponent単体ではなくGameObjectの破棄である点に注意してください。
        /// </remarks>
        public static Subscription AddTo(this Subscription subscription, Component component)
        {
            if (component == null)
            {
                subscription.Dispose();
                throw new ArgumentNullException(nameof(component));
            }

            return subscription.AddTo(component.gameObject);
        }
    }
}
