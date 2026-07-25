using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ark.Input
{
    /// <summary>
    /// 登録解除不要なInputActionを提供するクラス
    /// </summary>
    public static class InputProvider
    {
        private static readonly List<InputEvent> _InputEvents = new();
        private static bool _Releasing;

        /// <summary>
        /// 生成済みで未破棄の<see cref="InputEvent"/>の数
        /// </summary>
        public static int ActiveEventCount => _InputEvents.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // ドメインリロードを無効化している場合、前回の再生分が残っている可能性がある
            ReleaseAll();

#if !UNITY_EDITOR
            // 多重登録を避けるため、購読前に必ず解除する
            Application.quitting -= ReleaseAll;
            Application.quitting += ReleaseAll;
#endif
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeOnEditorLoad()
        {
            // ドメインリロードを無効化している場合、再生の度に多重登録されるのを避ける
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ReleaseAll();
            }
        }
#endif

        /// <summary>
        /// 指定したInputActionのイベントを生成します
        /// </summary>
        /// <param name="guid">InputActionのGuid</param>
        /// <exception cref="InvalidOperationException">Project-wide Input Actionsが未設定の場合</exception>
        /// <exception cref="ArgumentException">Guidに対応するInputActionが存在しない場合</exception>
        public static InputEvent CreateEvent(Guid guid)
        {
            var inputAction = GetActionAsset().FindAction(guid);

            if (inputAction == null)
            {
                throw new ArgumentException($"Guid: {guid} に対応するInputActionが見つかりません。", nameof(guid));
            }

            var inputEvent = new InputEvent(inputAction);
            inputEvent.Disposed += OnEventDisposed;
            _InputEvents.Add(inputEvent);

            return inputEvent;
        }

        /// <summary>
        /// 指定したInputActionMapを取得します
        /// </summary>
        /// <param name="guid">InputActionMapのGuid</param>
        /// <exception cref="InvalidOperationException">Project-wide Input Actionsが未設定の場合</exception>
        /// <exception cref="ArgumentException">Guidに対応するInputActionMapが存在しない場合</exception>
        public static InputActionMap GetActionMap(Guid guid)
        {
            var actionMap = GetActionAsset().FindActionMap(guid);

            if (actionMap == null)
            {
                throw new ArgumentException($"Guid: {guid} に対応するInputActionMapが見つかりません。", nameof(guid));
            }

            return actionMap;
        }

        /// <summary>
        /// 生成済みの<see cref="InputEvent"/>を全て破棄します
        /// </summary>
        public static void ReleaseAll()
        {
            if (_InputEvents.Count == 0)
            {
                return;
            }

            // 破棄通知によるリストの変更を避けるため、走査中はフラグを立てる
            _Releasing = true;

            try
            {
                foreach (var inputEvent in _InputEvents)
                {
                    inputEvent.Dispose();
                }
            }
            finally
            {
                _Releasing = false;
                _InputEvents.Clear();
            }
        }

        private static void OnEventDisposed(InputEvent inputEvent)
        {
            if (_Releasing)
            {
                return;
            }

            _InputEvents.Remove(inputEvent);
        }

        private static InputActionAsset GetActionAsset()
        {
            var actionAsset = InputSystem.actions;

            if (actionAsset == null)
            {
                throw new InvalidOperationException(
                    "Project-wide Input Actionsが設定されていません。Project Settings > Input System Package から設定してください。");
            }

            return actionAsset;
        }
    }
}
