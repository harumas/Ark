using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

namespace Ark.Input
{
    /// <summary>
    /// ゲームパッドのバイブレーションを管理するクラス
    /// </summary>
    public sealed class GamepadVibrator : IDisposable
    {
        private IDualMotorRumble _MotorRumble;
        private CancellationTokenSource _CTokenSource;
        private bool _Enabled;
        private bool _Disposed;

        public GamepadVibrator()
        {
            // デバイスの接続状況の変化を監視
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        /// <summary>
        /// バイブレーションが有効かどうか
        /// </summary>
        public bool IsEnabled => _Enabled;

        /// <summary>
        /// バイブレーション可能なデバイスが接続されているかどうか
        /// </summary>
        public bool IsAvailable => _MotorRumble != null;

        /// <summary>
        /// バイブレーションを有効化します
        /// </summary>
        public void Enable()
        {
            if (_Disposed || _Enabled)
            {
                return;
            }

            _Enabled = true;
            _MotorRumble ??= Gamepad.current;
        }

        /// <summary>
        /// バイブレーションを無効化します
        /// </summary>
        public void Disable()
        {
            if (!_Enabled)
            {
                return;
            }

            Reset();
            _Enabled = false;
        }

        /// <summary>
        /// 指定した秒数バイブレーションを行います
        /// </summary>
        /// <param name="right">右側の強さ (0 ~ 1)</param>
        /// <param name="left">左側の強さ (0 ~ 1)</param>
        /// <param name="duration">バイブレーションを行う秒数</param>
        public void Vibrate(float right, float left, float duration)
        {
            if (!TryGetRumble(out var rumble))
            {
                return;
            }

            // 継続中のバイブレーションを打ち切ってから上書きする
            CancelPending();

            if (duration <= 0f)
            {
                rumble.ResetHaptics();
                return;
            }

            _CTokenSource = new CancellationTokenSource();
            _ = VibrateAsync(rumble, right, left, duration, _CTokenSource.Token);
        }

        /// <summary>
        /// バイブレーションの強さを設定します
        /// </summary>
        /// <param name="right">右側の強さ (0 ~ 1)</param>
        /// <param name="left">左側の強さ (0 ~ 1)</param>
        public void SetMotorSpeed(float right, float left)
        {
            if (!TryGetRumble(out var rumble))
            {
                return;
            }

            // 継続中のバイブレーションを打ち切り、指定した強さを維持する
            CancelPending();
            rumble.SetMotorSpeeds(Mathf.Clamp01(left), Mathf.Clamp01(right));
        }

        /// <summary>
        /// バイブレーションの強さをリセットします
        /// </summary>
        public void Reset()
        {
            CancelPending();
            _MotorRumble?.ResetHaptics();
        }

        /// <summary>
        /// バイブレーションを一時停止します
        /// </summary>
        public void Pause()
        {
            if (TryGetRumble(out var rumble))
            {
                rumble.PauseHaptics();
            }
        }

        /// <summary>
        /// バイブレーションを再開します
        /// </summary>
        public void Resume()
        {
            if (TryGetRumble(out var rumble))
            {
                rumble.ResumeHaptics();
            }
        }

        /// <summary>
        /// デバイスの監視を終了し、このインスタンスを破棄します
        /// </summary>
        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            _Disposed = true;

            InputSystem.onDeviceChange -= OnDeviceChange;

            Reset();
            _MotorRumble = null;
            _Enabled = false;
        }

        private async Task VibrateAsync(IDualMotorRumble rumble, float right, float left, float duration,
            CancellationToken token)
        {
            try
            {
                rumble.SetMotorSpeeds(Mathf.Clamp01(left), Mathf.Clamp01(right));

                await Task.Delay(TimeSpan.FromSeconds(duration), token);

                // 待機中にデバイスが切り替わっている場合は触らない
                if (ReferenceEquals(rumble, _MotorRumble))
                {
                    rumble.ResetHaptics();
                }
            }
            catch (OperationCanceledException)
            {
                // 停止・上書きはキャンセルした側が行うため、ここでは何もしない
            }
            catch (Exception e)
            {
                // async void ではないため、握り潰さずにログへ出す
                Debug.LogException(e);
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not IDualMotorRumble rumble)
            {
                return;
            }

            switch (change)
            {
                // 新たなGamePadが接続された場合、それを使用する
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Enabled:
                    _MotorRumble = rumble;
                    break;

                // 使用中のGamePadが外された場合、他に接続されているGamePadを使用する
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Disabled:
                    if (!ReferenceEquals(_MotorRumble, rumble))
                    {
                        break;
                    }

                    CancelPending();
                    _MotorRumble = ReferenceEquals(Gamepad.current, rumble) ? null : Gamepad.current;
                    break;
            }
        }

        private bool TryGetRumble(out IDualMotorRumble rumble)
        {
            rumble = _MotorRumble;

            return !_Disposed && _Enabled && rumble != null;
        }

        private void CancelPending()
        {
            if (_CTokenSource == null)
            {
                return;
            }

            _CTokenSource.Cancel();
            _CTokenSource.Dispose();
            _CTokenSource = null;
        }
    }
}
