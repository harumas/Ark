using System;
using UnityEngine.InputSystem;

namespace Ark.Input
{
    /// <summary>
    /// InputActionのラッパークラス
    /// </summary>
    /// <remarks>
    /// 生成元の<see cref="InputProvider"/>がインスタンスを保持しており、
    /// アプリケーション終了時に自動で破棄されるため購読解除は不要です。
    /// 途中で破棄したい場合のみ<see cref="Dispose"/>を呼び出してください。
    /// </remarks>
    public sealed class InputEvent : IDisposable
    {
        private readonly InputAction _InputAction;
        private readonly Event<InputAction.CallbackContext> _Started = new();
        private readonly Event<InputAction.CallbackContext> _Performed = new();
        private readonly Event<InputAction.CallbackContext> _Canceled = new();
        private bool _Disposed;

        internal InputEvent(InputAction inputAction)
        {
            _InputAction = inputAction ?? throw new ArgumentNullException(nameof(inputAction));

            _InputAction.started += OnStarted;
            _InputAction.performed += OnPerformed;
            _InputAction.canceled += OnCanceled;
        }

        /// <summary>
        /// 既に破棄されているかどうか
        /// </summary>
        public bool IsDisposed => _Disposed;

        public IReadOnlyEvent<InputAction.CallbackContext> Started => _Started;
        public IReadOnlyEvent<InputAction.CallbackContext> Performed => _Performed;
        public IReadOnlyEvent<InputAction.CallbackContext> Canceled => _Canceled;

        /// <summary>
        /// 破棄された事を<see cref="InputProvider"/>へ通知します
        /// </summary>
        internal event Action<InputEvent> Disposed;

        private void OnStarted(InputAction.CallbackContext ctx)
        {
            _Started.Execute(ctx);
        }

        private void OnPerformed(InputAction.CallbackContext ctx)
        {
            _Performed.Execute(ctx);
        }

        private void OnCanceled(InputAction.CallbackContext ctx)
        {
            _Canceled.Execute(ctx);
        }

        public TValue ReadValue<TValue>() where TValue : struct
        {
            return _Disposed ? default : _InputAction.ReadValue<TValue>();
        }

        public InputAction GetInputAction()
        {
            return _InputAction;
        }

        /// <summary>
        /// 全ての購読を解除し、このインスタンスを破棄します
        /// </summary>
        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            _Disposed = true;

            _InputAction.started -= OnStarted;
            _InputAction.performed -= OnPerformed;
            _InputAction.canceled -= OnCanceled;

            _Started.Dispose();
            _Performed.Dispose();
            _Canceled.Dispose();

            Disposed?.Invoke(this);
            Disposed = null;
        }
    }
}
