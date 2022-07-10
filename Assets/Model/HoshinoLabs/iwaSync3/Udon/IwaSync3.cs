using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace HoshinoLabs.Udon.IwaSync3
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class IwaSync3 : UdonSharpBehaviour
    {
        const string _APP_NAME = "iwaSync3";
        public
        #if !COMPILER_UDONSHARP
        static
        #endif
        string APP_NAME => _APP_NAME;
        const string _APP_VERSION = "V3.3.2";
        public
        #if !COMPILER_UDONSHARP
        static
        #endif
        string APP_VERSION => _APP_VERSION;

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [SerializeField]
        [HideInInspector]
        Transform _transform;

        GameObject _canvas1;
        GameObject _lockOn;
        Button _lockOnButton;
        GameObject _lockOff;
        Button _lockOffButton;
        Button _videoButton;
        Button _liveButton;

        GameObject _canvas2;
        GameObject _address;
        VRCUrlInputField _addressInput;
        GameObject _message;
        Text _messageText;
        GameObject _close;
        Button _closeButton;

        VRCPlayerApi _localPlayer;
        bool _local = false;
        uint _mode;
        [UdonSynced, FieldChangeCallback(nameof(on))]
        bool _on = false;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] Started `{nameof(IwaSync3)}`.");

            controller.AddListener(this);

            _canvas1 = _transform.Find("Canvas").gameObject;
            _lockOn = _transform.Find("Canvas/Panel/Lock/On").gameObject;
            _lockOnButton = _lockOn.transform.Find("Button").GetComponent<Button>();
            _lockOff = _transform.Find("Canvas/Panel/Lock/Off").gameObject;
            _lockOffButton = _lockOff.transform.Find("Button").GetComponent<Button>();
            _videoButton = _transform.Find("Canvas/Panel/Video/Button").GetComponent<Button>();
            _liveButton = _transform.Find("Canvas/Panel/Live/Button").GetComponent<Button>();

            _canvas2 = _transform.Find("Canvas (1)").gameObject;
            _address = _transform.Find("Canvas (1)/Panel/Address").gameObject;
            _addressInput = (VRCUrlInputField)_address.GetComponent(typeof(VRCUrlInputField));
            _message = _transform.Find("Canvas (1)/Panel/Message").gameObject;
            _messageText = _message.GetComponent<Text>();
            _close = _transform.Find("Canvas (1)/Panel/Close").gameObject;
            _closeButton = _close.transform.Find("Button").GetComponent<Button>();

            _localPlayer = Networking.LocalPlayer;
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ValidateView();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (isOwner && _on && !_local)
                Close();
            ValidateView();
        }
        #endregion

        #region VideoEvent
        public override void OnVideoEnd()
        {
            ValidateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            ValidateView();
        }

        public override void OnVideoLoop()
        {
            ValidateView();
        }

        public override void OnVideoReady()
        {
            ValidateView();
        }

        public override void OnVideoStart()
        {
            ValidateView();
        }
        #endregion

        #region VideoCoreEvent
        public void OnPlayerPlay()
        {
            ValidateView();
        }

        public void OnPlayerPause()
        {
            ValidateView();
        }

        public void OnPlayerStop()
        {
            ValidateView();
        }

        public void OnChangeURL()
        {
            ValidateView();
        }
        #endregion

        #region VideoControllerEvent
        public void OnChangeLock()
        {
            ValidateView();
        }
        #endregion

        public void TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public bool isOwner => Networking.IsOwner(gameObject);

        void ValidateView()
        {
            var locked = controller.locked;
            var master = Networking.IsMaster || (controller.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            _canvas1.SetActive(!core.isPrepared && !core.isPlaying && !core.isError && !_on);
            _lockOn.SetActive(!locked);
            _lockOnButton.interactable = master;
            _lockOff.SetActive(locked);
            _lockOffButton.interactable = master;
            _videoButton.interactable = privilege;
            _liveButton.interactable = privilege;

            _canvas2.SetActive(!core.isPrepared && !core.isPlaying && !core.isError && _on);
            _address.SetActive(isOwner);
            _message.SetActive(!isOwner);
            var player = Networking.GetOwner(gameObject);
            _messageText.text = $"Entering the URL... ({(player == null ? string.Empty : player.displayName)})";
            _closeButton.interactable = isOwner || privilege;
        }

        public void LockOn()
        {
            controller.LockOn();
        }

        public void LockOff()
        {
            controller.LockOff();
        }

        public bool on
        {
            get => _on;
            private set
            {
                _on = value;
                UpdateOn();
            }
        }

        void UpdateOn()
        {
            ValidateView();
        }

        public void ModeVideo()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] The mode has changed to `MODE_VIDEO`.");
            TakeOwnership();
            _local = true;
            _mode =
                #if COMPILER_UDONSHARP
                core.MODE_VIDEO
                #else
                VideoCore.MODE_VIDEO
                #endif
            ;
            _on = true;
            RequestSerialization();
            ValidateView();
        }

        public void ModeLive()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] The mode has changed to `MODE_STREAM`.");
            TakeOwnership();
            _local = true;
            _mode =
                #if COMPILER_UDONSHARP
                core.MODE_STREAM
                #else
                VideoCore.MODE_STREAM
                #endif
            ;
            _on = true;
            RequestSerialization();
            ValidateView();
        }

        public void OnURLChanged()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] The url has changed to `{_addressInput.GetUrl().Get()}`.");
            core.TakeOwnership();
            core.PlayURL(_mode, _addressInput.GetUrl());
            core.RequestSerialization();
            ValidateView();
        }

        public void ClearURL()
        {
            _addressInput.SetUrl(VRCUrl.Empty);
        }

        public void Close()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] Trigger a close event.");
            TakeOwnership();
            _local = false;
            _on = false;
            RequestSerialization();
            ValidateView();
        }
    }
}
