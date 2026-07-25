using System;
using UnityEngine;

namespace Ark
{
    /// <summary>
    /// 購読解除を安全に行えるイベント
    /// </summary>
    /// <remarks>
    /// C#の<c>event</c>と異なり、購読の追加・解除で配列を作り直さないため確保が発生しません。
    /// また、1つの購読者が例外を投げても他の購読者は通知を受け取ります。
    /// 発火中に購読を解除した場合、その購読者はその発火では呼ばれません。
    /// 逆に発火中に追加した購読は、次回の発火から呼ばれます。
    /// 通知順は原則として登録順ですが、解除されたスロットを再利用するため
    /// 解除を挟むと順序が入れ替わります。順序に依存しない設計にしてください。
    /// </remarks>
    public sealed class Event : IReadOnlyEvent, ISubscriptionSource, IDisposable
    {
        private const int _InitialCapacity = 4;

        private Entry[] _Entries = Array.Empty<Entry>();
        private int _Tail;
        private int _Count;
        private int _FreeHead = -1;
        private int _LastId;
        private int _ExecuteDepth;
        private bool _HasPendingFree;
        private bool _Disposed;

        /// <inheritdoc/>
        public int Count => _Count;

        /// <summary>
        /// 既に破棄されているかどうか
        /// </summary>
        public bool IsDisposed => _Disposed;

        /// <inheritdoc/>
        public Subscription Subscribe(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (_Disposed)
            {
                throw new ObjectDisposedException(nameof(Event));
            }

            var index = RentIndex();
            var id = _LastId == int.MaxValue ? 1 : _LastId + 1;

            _LastId = id;
            _Entries[index].Action = action;
            _Entries[index].Id = id;
            _Count++;

            return new Subscription(this, index, id);
        }

        /// <summary>
        /// 購読者へ通知します
        /// </summary>
        public void Execute()
        {
            if (_Disposed)
            {
                return;
            }

            // 発火中に追加された購読を巻き込まないよう、開始時点の範囲だけを走査する
            var tail = _Tail;

            _ExecuteDepth++;

            try
            {
                for (var i = 0; i < tail; i++)
                {
                    // 発火中の解除を即座に反映するため、都度読み直す
                    var action = _Entries[i].Action;

                    if (action == null)
                    {
                        continue;
                    }

                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        // 1人の購読者の例外で他の購読者を巻き込まない
                        Debug.LogException(e);
                    }
                }
            }
            finally
            {
                _ExecuteDepth--;

                if (_ExecuteDepth == 0 && _HasPendingFree)
                {
                    RebuildFreeList();
                }
            }
        }

        /// <summary>
        /// 全ての購読を解除します
        /// </summary>
        public void Clear()
        {
            if (_Tail == 0)
            {
                return;
            }

            Array.Clear(_Entries, 0, _Tail);

            _Tail = 0;
            _Count = 0;
            _FreeHead = -1;
            _HasPendingFree = false;
        }

        /// <summary>
        /// 全ての購読を解除し、このイベントを破棄します
        /// </summary>
        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            _Disposed = true;

            Clear();
            _Entries = Array.Empty<Entry>();
        }

        bool ISubscriptionSource.IsSubscribed(int index, int id)
        {
            return IsValid(index, id);
        }

        void ISubscriptionSource.Unsubscribe(int index, int id)
        {
            if (IsValid(index, id) == false)
            {
                return;
            }

            _Entries[index].Action = null;
            _Entries[index].Id = 0;
            _Count--;

            // 発火中はスロットを再利用させず、発火の完了後にまとめて空きリストへ戻す
            if (_ExecuteDepth > 0)
            {
                _HasPendingFree = true;
                return;
            }

            _Entries[index].NextFree = _FreeHead;
            _FreeHead = index;
        }

        private bool IsValid(int index, int id)
        {
            return id != 0 && (uint)index < (uint)_Tail && _Entries[index].Id == id;
        }

        private int RentIndex()
        {
            // 発火中は空きスロットを再利用しない（走査済みの位置に割り当たると挙動が不定になるため）
            if (_ExecuteDepth == 0 && _FreeHead >= 0)
            {
                var index = _FreeHead;

                _FreeHead = _Entries[index].NextFree;

                return index;
            }

            if (_Tail == _Entries.Length)
            {
                Array.Resize(ref _Entries, _Entries.Length == 0 ? _InitialCapacity : _Entries.Length * 2);
            }

            return _Tail++;
        }

        private void RebuildFreeList()
        {
            _HasPendingFree = false;

            // 末尾側の空きスロットを切り詰める
            while (_Tail > 0 && _Entries[_Tail - 1].Action == null)
            {
                _Tail--;
            }

            _FreeHead = -1;

            // 末尾から辿る事で、若い番号から順に再利用される空きリストになる
            for (var i = _Tail - 1; i >= 0; i--)
            {
                if (_Entries[i].Action != null)
                {
                    continue;
                }

                _Entries[i].NextFree = _FreeHead;
                _FreeHead = i;
            }
        }

        private struct Entry
        {
            public Action Action;
            public int Id;
            public int NextFree;
        }
    }

    /// <inheritdoc cref="Event"/>
    /// <typeparam name="T">発火時に渡す値の型</typeparam>
    public sealed class Event<T> : IReadOnlyEvent<T>, ISubscriptionSource, IDisposable
    {
        private const int _InitialCapacity = 4;

        private Entry[] _Entries = Array.Empty<Entry>();
        private int _Tail;
        private int _Count;
        private int _FreeHead = -1;
        private int _LastId;
        private int _ExecuteDepth;
        private bool _HasPendingFree;
        private bool _Disposed;

        /// <inheritdoc/>
        public int Count => _Count;

        /// <summary>
        /// 既に破棄されているかどうか
        /// </summary>
        public bool IsDisposed => _Disposed;

        /// <inheritdoc/>
        public Subscription Subscribe(Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (_Disposed)
            {
                throw new ObjectDisposedException(nameof(Event<T>));
            }

            var index = RentIndex();
            var id = _LastId == int.MaxValue ? 1 : _LastId + 1;

            _LastId = id;
            _Entries[index].Action = action;
            _Entries[index].Id = id;
            _Count++;

            return new Subscription(this, index, id);
        }

        /// <summary>
        /// 購読者へ通知します
        /// </summary>
        public void Execute(T value)
        {
            if (_Disposed)
            {
                return;
            }

            // 発火中に追加された購読を巻き込まないよう、開始時点の範囲だけを走査する
            var tail = _Tail;

            _ExecuteDepth++;

            try
            {
                for (var i = 0; i < tail; i++)
                {
                    // 発火中の解除を即座に反映するため、都度読み直す
                    var action = _Entries[i].Action;

                    if (action == null)
                    {
                        continue;
                    }

                    try
                    {
                        action(value);
                    }
                    catch (Exception e)
                    {
                        // 1人の購読者の例外で他の購読者を巻き込まない
                        Debug.LogException(e);
                    }
                }
            }
            finally
            {
                _ExecuteDepth--;

                if (_ExecuteDepth == 0 && _HasPendingFree)
                {
                    RebuildFreeList();
                }
            }
        }

        /// <summary>
        /// 全ての購読を解除します
        /// </summary>
        public void Clear()
        {
            if (_Tail == 0)
            {
                return;
            }

            Array.Clear(_Entries, 0, _Tail);

            _Tail = 0;
            _Count = 0;
            _FreeHead = -1;
            _HasPendingFree = false;
        }

        /// <summary>
        /// 全ての購読を解除し、このイベントを破棄します
        /// </summary>
        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            _Disposed = true;

            Clear();
            _Entries = Array.Empty<Entry>();
        }

        bool ISubscriptionSource.IsSubscribed(int index, int id)
        {
            return IsValid(index, id);
        }

        void ISubscriptionSource.Unsubscribe(int index, int id)
        {
            if (IsValid(index, id) == false)
            {
                return;
            }

            _Entries[index].Action = null;
            _Entries[index].Id = 0;
            _Count--;

            // 発火中はスロットを再利用させず、発火の完了後にまとめて空きリストへ戻す
            if (_ExecuteDepth > 0)
            {
                _HasPendingFree = true;
                return;
            }

            _Entries[index].NextFree = _FreeHead;
            _FreeHead = index;
        }

        private bool IsValid(int index, int id)
        {
            return id != 0 && (uint)index < (uint)_Tail && _Entries[index].Id == id;
        }

        private int RentIndex()
        {
            // 発火中は空きスロットを再利用しない（走査済みの位置に割り当たると挙動が不定になるため）
            if (_ExecuteDepth == 0 && _FreeHead >= 0)
            {
                var index = _FreeHead;

                _FreeHead = _Entries[index].NextFree;

                return index;
            }

            if (_Tail == _Entries.Length)
            {
                Array.Resize(ref _Entries, _Entries.Length == 0 ? _InitialCapacity : _Entries.Length * 2);
            }

            return _Tail++;
        }

        private void RebuildFreeList()
        {
            _HasPendingFree = false;

            // 末尾側の空きスロットを切り詰める
            while (_Tail > 0 && _Entries[_Tail - 1].Action == null)
            {
                _Tail--;
            }

            _FreeHead = -1;

            // 末尾から辿る事で、若い番号から順に再利用される空きリストになる
            for (var i = _Tail - 1; i >= 0; i--)
            {
                if (_Entries[i].Action != null)
                {
                    continue;
                }

                _Entries[i].NextFree = _FreeHead;
                _FreeHead = i;
            }
        }

        private struct Entry
        {
            public Action<T> Action;
            public int Id;
            public int NextFree;
        }
    }
}
