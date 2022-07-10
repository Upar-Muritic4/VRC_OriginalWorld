using System;
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
    public class Queuelist : UdonSharpBehaviour
    {
        #region UExtenderLite
        uint[] _ArrayUtility_Add_uintarray_uint(uint[] _0, uint _1)
        {
            var result = new uint[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }
        VRCUrl[] _ArrayUtility_Add_VRCUrlarray_VRCUrl(VRCUrl[] _0, VRCUrl _1)
        {
            var result = new VRCUrl[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }

        GameObject[] _ArrayUtility_Insert_GameObjectarray_int_GameObject(GameObject[] _0, int _1, GameObject _2)
        {
            var result = new GameObject[_0.Length + 1];
            Array.Copy(_0, result, _1);
            result[_1] = _2;
            Array.Copy(_0, _1, result, _1 + 1, _0.Length - _1);
            return result;
        }

        uint[] _ArrayUtility_RemoveAt_uintarray_int(uint[] _0, int _1)
        {
            var result = new uint[_0.Length - 1];
            Array.Copy(_0, result, _1);
            Array.Copy(_0, _1 + 1, result, _1, _0.Length - _1 - 1);
            return result;
        }
        GameObject[] _ArrayUtility_RemoveAt_GameObjectarray_int(GameObject[] _0, int _1)
        {
            var result = new GameObject[_0.Length - 1];
            Array.Copy(_0, result, _1);
            Array.Copy(_0, _1 + 1, result, _1, _0.Length - _1 - 1);
            return result;
        }
        VRCUrl[] _ArrayUtility_RemoveAt_VRCUrlarray_int(VRCUrl[] _0, int _1)
        {
            var result = new VRCUrl[_0.Length - 1];
            Array.Copy(_0, result, _1);
            Array.Copy(_0, _1 + 1, result, _1, _0.Length - _1 - 1);
            return result;
        }

        GameObject _Instantiate(GameObject _0, Transform _1, bool _2)
        {
            var instantiatedObject = VRCInstantiate(_0);
            var objectTransform = instantiatedObject.transform;
            objectTransform.SetParent(_1, _2);

            if (!_2)
            {
                var originalTransform = _0.transform;
                var originalPosition = originalTransform.position;
                var originalRotation = originalTransform.rotation;
                instantiatedObject.transform.SetPositionAndRotation(originalPosition, originalRotation);
                instantiatedObject.transform.localScale = originalTransform.localScale;// May not behave the same as C#
            }

            return instantiatedObject;
        }
        #endregion

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [SerializeField]
        [HideInInspector]
        Transform _transform;

        GameObject _lockOn;
        Button _lockOnButton;
        GameObject _lockOff;
        Button _lockOffButton;
        GameObject _playOn;
        Button _playOnButton;
        GameObject _playOff;
        Button _playOffButton;
        GameObject _forward;
        Button _forwardButton;
        GameObject _scrollView;
        Transform _content;
        GameObject _template;
        GameObject _message;
        Text _messageText;

        GameObject _obj = null;
        VRCUrlInputField _addressInput;

        GameObject[] _objs = new GameObject[0];
        Slider _progressSlider;

        [UdonSynced, FieldChangeCallback(nameof(controlled))]
        bool _controlled = false;
        bool _wait = false;

        [UdonSynced, FieldChangeCallback(nameof(modes))]
        uint[] _modes = new uint[0];
        [UdonSynced, FieldChangeCallback(nameof(urls))]
        VRCUrl[] _urls = new VRCUrl[0];
        [UdonSynced, FieldChangeCallback(nameof(track))]
        int _track;

        VRCPlayerApi _localPlayer;
        bool _local = false;
        uint _mode;
        [UdonSynced, FieldChangeCallback(nameof(on))]
        bool _on = false;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Started `{nameof(Queuelist)}`.");

            controller.AddListener(this);

            _lockOn = _transform.Find("Canvas/Panel/Header/Lock/On").gameObject;
            _lockOnButton = _lockOn.transform.Find("Button").GetComponent<Button>();
            _lockOff = _transform.Find("Canvas/Panel/Header/Lock/Off").gameObject;
            _lockOffButton = _lockOff.transform.Find("Button").GetComponent<Button>();
            _playOn = _transform.Find("Canvas/Panel/Header/Play/On").gameObject;
            _playOnButton = _playOn.transform.Find("Button").GetComponent<Button>();
            _playOff = _transform.Find("Canvas/Panel/Header/Play/Off").gameObject;
            _playOffButton = _playOff.transform.Find("Button").GetComponent<Button>();
            _forward = _transform.Find("Canvas/Panel/Header/Forward").gameObject;
            _forwardButton = _forward.transform.Find("Button").GetComponent<Button>();
            _scrollView = _transform.Find("Canvas/Panel/Scroll View/Scroll View").gameObject;
            _content = _scrollView.transform.Find("Viewport/Content");
            _template = _content.Find("Template").gameObject;
            _message = _transform.Find("Canvas/Panel/Scroll View/Message").gameObject;
            _messageText = _message.transform.Find("Text").GetComponent<Text>();

            _obj = GenerateChildItem();
            _addressInput = (VRCUrlInputField)_obj.transform.Find("Panel (2)/Panel/Address").GetComponent(typeof(VRCUrlInputField));

            _localPlayer = Networking.LocalPlayer;
        }

        public bool controlled
        {
            get => _controlled;
            private set
            {
                _controlled = value;
                UpdateControlled();
            }
        }

        void UpdateControlled()
        {
            ValidateView();
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ValidateView();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (isOwner && on && !_local)
                Close();
            ValidateView();
        }
        #endregion

        #region VideoEvent
        public override void OnVideoEnd()
        {
            if (isOwner && _controlled)
                Forward();
            ValidateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            if (isOwner && _controlled && core.errorRetry == 0)
                Forward();
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
            if (isOwner && _controlled)
                PlayOff();
            ValidateView();
        }

        public void OnChangeURL()
        {
            _messageText.text = $"Loading Now";
            if (isOwner && _controlled && (!core.isReload && !_wait))
                PlayOff();
            _wait = false;
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

            _lockOn.SetActive(!locked);
            _lockOnButton.interactable = master;
            _lockOff.SetActive(locked);
            _lockOffButton.interactable = master;
            _playOn.SetActive(!_controlled);
            _playOnButton.interactable = privilege && !IsTrackDoneAll();
            _playOff.SetActive(_controlled);
            _playOffButton.interactable = privilege;
            _forward.SetActive(_controlled);
            _forwardButton.interactable = privilege && IsTrackPlaying(_track);
            _scrollView.SetActive(!core.isPrepared || core.isError);
            _message.SetActive(core.isPrepared && !core.isError);

            UpdateTracks();
        }

        private void Update()
        {
            if (!_controlled)
                return;

            if (!core.isPlaying)
                return;

            if (core.isLive)
            {
                _progressSlider.value = 1f;
            }
            else
            {
                var duration = core.duration;
                if (core.duration != 0f)
                {
                    var time = core.time;
                    _progressSlider.value = time / duration;
                }
            }
        }

        public void LockOn()
        {
            controller.LockOn();
        }

        public void LockOff()
        {
            controller.LockOff();
        }

        public void PlayOn()
        {
            TakeOwnership();
            PlayTracks(GetTrackNext());
            RequestSerialization();
        }

        public void PlayOff()
        {
            TakeOwnership();
            StopTracks();
            RequestSerialization();
        }

        public void Forward()
        {
            TakeOwnership();
            TrackDone();
            PlayTracks(GetTrackNext());
            RequestSerialization();
        }

        public void OnButtonClicked()
        {
            var sender = FindSender();
            if (sender == null)
                return;

            TakeOwnership();
            PlayTracks(sender.GetSiblingIndex());
            RequestSerialization();
        }

        Transform FindSender()
        {
            foreach (var x in _content.GetComponentsInChildren<Button>())
            {
                if (x.enabled)
                    continue;
                for (var i = 0; i < _content.childCount; i++)
                {
                    var obj = _content.GetChild(i);
                    if (!x.transform.IsChildOf(obj))
                        continue;
                    return obj;
                }
            }
            return null;
        }

        public uint[] modes
        {
            get => _modes;
            private set
            {
                _modes = value;
                //UpdateTracks();
            }
        }

        public VRCUrl[] urls
        {
            get => _urls;
            private set
            {
                _urls = value;
                UpdateTracks();
                UpdateTrack();
            }
        }

        void UpdateTracks()
        {
            var locked = controller.locked;
            var master = Networking.IsMaster || (controller.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            for (var i = 0; i < _urls.Length; i++)
            {
                if (_objs.Length <= i)
                    _objs = _ArrayUtility_Insert_GameObjectarray_int_GameObject(_objs, i, GenerateChildItem());
                var obj = _objs[i];
                obj.transform.SetSiblingIndex(i);
                RefreshChildItem(obj, privilege, IsTrackPlaying(i), _urls[i]);
            }
            _obj.transform.SetSiblingIndex(_urls.Length);
            RefreshChildLastItem(_obj, privilege);
            for (var i = _objs.Length - 1; _urls.Length <= i; i--)
                DestroyChildItem(i);
        }

        GameObject GenerateChildItem()
        {
            var obj = _Instantiate(_template, _content, false);
            obj.SetActive(true);
            return obj;
        }

        void RefreshChildItem(GameObject obj, bool privilege, bool playing, VRCUrl url)
        {
            var panel1 = obj.transform.Find("Panel").gameObject;
            panel1.SetActive(true);
            var buttonButton = panel1.transform.Find("Button").GetComponent<Button>();
            buttonButton.interactable = privilege && !playing;
            var progressSlider = panel1.transform.Find("Button/Progress").GetComponent<Slider>();
            progressSlider.value = 0f;
            var trackText = panel1.transform.Find("Button/Panel/Text").GetComponent<Text>();
            trackText.text = url.Get();
            var removeButton = panel1.transform.Find("Button/Panel/Remove/Button").GetComponent<Button>();
            removeButton.interactable = privilege;

            var panel2 = obj.transform.Find("Panel (1)").gameObject;
            panel2.SetActive(false);

            var panel3 = obj.transform.Find("Panel (2)").gameObject;
            panel3.SetActive(false);
        }

        void RefreshChildLastItem(GameObject obj, bool privilege)
        {
            var panel1 = obj.transform.Find("Panel").gameObject;
            panel1.SetActive(false);

            var panel2 = obj.transform.Find("Panel (1)").gameObject;
            panel2.SetActive(!_on);
            var videoButton = panel2.transform.Find("Panel/Video/Button").GetComponent<Button>();
            videoButton.interactable = privilege;
            var liveButton = panel2.transform.Find("Panel/Live/Button").GetComponent<Button>();
            liveButton.interactable = privilege;

            var panel3 = obj.transform.Find("Panel (2)").gameObject;
            panel3.SetActive(_on);
            var address = panel3.transform.Find("Panel/Address").gameObject;
            address.SetActive(isOwner);
            var message = panel3.transform.Find("Panel/Message").gameObject;
            message.SetActive(!isOwner);
            var messageText = message.GetComponent<Text>();
            var player = Networking.GetOwner(gameObject);
            messageText.text = $"Entering the URL... ({(player == null ? string.Empty : player.displayName)})";
            var closeButton = panel3.transform.Find("Panel/Close/Button").GetComponent<Button>();
            closeButton.interactable = isOwner || privilege;
        }

        void DestroyChildItem(int index)
        {
            var obj = _objs[index];
            Destroy(obj.gameObject);
            _objs = _ArrayUtility_RemoveAt_GameObjectarray_int(_objs, index);
        }

        void ReorderTracks()
        {
            if (!IsTrackAvailable(_track))
                return;
            var tmp1 = new uint[_modes.Length];
            Array.Copy(_modes, _track, tmp1, 0, _modes.Length - _track);
            Array.Copy(_modes, 0, tmp1, _modes.Length - _track, _track);
            _modes = tmp1;
            var tmp2 = new VRCUrl[_urls.Length];
            Array.Copy(_urls, _track, tmp2, 0, _urls.Length - _track);
            Array.Copy(_urls, 0, tmp2, _urls.Length - _track, _track);
            _urls = tmp2;
        }

        void PlayTracks(int track)
        {
            _controlled = false;
            _wait = false;
            StopTrack(true);
            if (!IsTrackDoneAll() && IsTrackAvailable(track))
            {
                _controlled = true;
                _wait = true;
                _track = track;
                ReorderTracks();
                _track = GetTrackNext();
                UpdateTrack();
            }
            UpdateTracks();
            PlayTrack();
        }

        void StopTracks()
        {
            var playing = IsTrackPlaying(_track);
            _controlled = false;
            _wait = false;
            StopTrack(playing);
            UpdateTracks();
        }

        public int track
        {
            get => _track;
            private set
            {
                _track = value;
                UpdateTrack();
            }
        }

        void UpdateTrack()
        {
            if (!_controlled || !IsTrackAvailable(_track))
                return;

            var obj = _objs[_track];
            var panel1 = obj.transform.Find("Panel").gameObject;
            _progressSlider = panel1.transform.Find("Button/Progress").GetComponent<Slider>();
        }

        void PlayTrack()
        {
            if (!_controlled || !IsTrackAvailable(_track))
                return;

            core.TakeOwnership();
            core.PlayURL(_modes[_track], _urls[_track]);
            core.RequestSerialization();
        }

        void StopTrack(bool force)
        {
            if (!force && !IsTrackPlaying(_track))
                return;

            core.TakeOwnership();
            core.Stop();
            core.RequestSerialization();
        }

        bool IsTrackDoneAll()
        {
            return _urls == null || _urls.Length == 0;
        }

        bool IsTrackAvailable(int track)
        {
            if (IsTrackDoneAll())
                return false;
            return 0 <= track && track < _urls.Length;
        }

        bool IsTrackPlaying(int track)
        {
            if (IsTrackDoneAll())
                return false;
            if (!core.isPrepared && !core.isPlaying && !core.isError)
                return false;
            return _controlled && IsTrackAvailable(track) && track == _track;
        }

        void TrackDone()
        {
            if (IsTrackDoneAll())
                return;
            TrackDoneAt(_track);
        }

        void TrackDoneAt(int track)
        {
            if (IsTrackDoneAll())
                return;
            _modes = _ArrayUtility_RemoveAt_uintarray_int(_modes, track);
            _urls = _ArrayUtility_RemoveAt_VRCUrlarray_int(_urls, track);
        }

        int GetTrackNext()
        {
            if (IsTrackDoneAll())
                return -1;
            return 0;
        }

        public void OnURLRemoved()
        {
            var sender = FindSender();
            if (sender == null)
                return;

            var track = sender.GetSiblingIndex();
            if (IsTrackPlaying(track))
            {
                Forward();
                return;
            }
            TakeOwnership();
            TrackDoneAt(track);
            UpdateTracks();
            RequestSerialization();
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
            TakeOwnership();
            _local = true;
            _mode =
            #if COMPILER_UDONSHARP
            core.MODE_VIDEO
            #else
            VideoCore.MODE_VIDEO
            #endif
            ;
            on = true;
            RequestSerialization();
        }

        public void ModeLive()
        {
            TakeOwnership();
            _local = true;
            _mode =
            #if COMPILER_UDONSHARP
            core.MODE_STREAM
            #else
            VideoCore.MODE_STREAM
            #endif
            ;
            on = true;
            RequestSerialization();
        }

        public void OnURLChanged()
        {
            var url = _addressInput.GetUrl();
            if (!core.IsValidURL(url.Get()))
                return;

            TakeOwnership();
            _modes = _ArrayUtility_Add_uintarray_uint(_modes, _mode);
            _urls = _ArrayUtility_Add_VRCUrlarray_VRCUrl(_urls, url);
            _local = false;
            on = false;
            if(!core.isPrepared && !core.isPlaying && !core.isError && _urls.Length == 1)
                PlayTracks(GetTrackNext());
            RequestSerialization();
        }

        public void ClearURL()
        {
            _addressInput.SetUrl(VRCUrl.Empty);
        }

        public void Close()
        {
            TakeOwnership();
            _local = false;
            on = false;
            RequestSerialization();
        }
    }
}
