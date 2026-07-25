using UnityEngine;

namespace Ark
{
    /// <summary>
    /// GameObjectの破棄に合わせて購読を解除するコンポーネント
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class SubscriptionDisposer : MonoBehaviour
    {
        private readonly SubscriptionBag _Bag = new();

        public SubscriptionBag Bag => _Bag;

        public static SubscriptionDisposer GetOrAdd(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<SubscriptionDisposer>(out var disposer))
            {
                return disposer;
            }

            disposer = gameObject.AddComponent<SubscriptionDisposer>();
            disposer.hideFlags = HideFlags.HideInInspector;

            return disposer;
        }

        private void OnDestroy()
        {
            _Bag.Dispose();
        }
    }
}
