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
    public class Playlist : UdonSharpBehaviour
    {
        #region UExtenderLite
        int[] _ArrayUtility_Add_intarray_int(int[] _0, int _1)
        {
            var result = new int[_0.Length + 1];
            _0.CopyTo(result, 0);
            result[_0.Length] = _1;
            return result;
        }

        bool _ArrayUtility_Contains_intarray_int(int[] _0, int _1)
        {
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    return true;
            }
            return false;
        }

        int ArrayUtility_IndexOf_intarray_int(int[] _0, int _1)
        {
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    return i;
            }
            return -1;
        }

        int[] _ArrayUtility_Remove_intarray_int(int[] _0, int _1)
        {
            var cnt = 0;
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    continue;
                cnt++;
            }
            var result = new int[cnt];
            var idx = 0;
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    continue;
                result[idx] = _0[i];
                idx++;
            }
            return result;
        }
        #endregion

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [Header("Options")]
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(shuffle))]
        bool defaultShuffle = false;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(repeat))]
        bool defaultRepeat = true;
        bool playOnAwake = false;

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
        GameObject _shuffleOn;
        Button _shuffleOnButton;
        GameObject _shuffleOff;
        Button _shuffleOffButton;
        GameObject _repeatOn;
        Button _repeatOnButton;
        GameObject _repeatOff;
        Button _repeatOffButton;
        GameObject _scrollView;
        Transform _content;
        GameObject _message;
        Text _messageText;

        Slider _progressSlider;
        Text _modeText;
        VRCUrlInputField _addressInput;

        [UdonSynced, FieldChangeCallback(nameof(controlled))]
        bool _controlled = false;
        bool _wait = false;

        [UdonSynced, FieldChangeCallback(nameof(tracks))]
        int[] _tracks;
        [UdonSynced, FieldChangeCallback(nameof(track))]
        int _track;

        VRCPlayerApi _localPlayer;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Started `{nameof(Playlist)}`.");

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
            _shuffleOn = _transform.Find("Canvas/Panel/Header/Shuffle/On").gameObject;
            _shuffleOnButton = _shuffleOn.transform.Find("Button").GetComponent<Button>();
            _shuffleOff = _transform.Find("Canvas/Panel/Header/Shuffle/Off").gameObject;
            _shuffleOffButton = _shuffleOff.transform.Find("Button").GetComponent<Button>();
            _repeatOn = _transform.Find("Canvas/Panel/Header/Repeat/On").gameObject;
            _repeatOnButton = _repeatOn.transform.Find("Button").GetComponent<Button>();
            _repeatOff = _transform.Find("Canvas/Panel/Header/Repeat/Off").gameObject;
            _repeatOffButton = _repeatOff.transform.Find("Button").GetComponent<Button>();
            _scrollView = _transform.Find("Canvas/Panel/Scroll View/Scroll View").gameObject;
            _content = _scrollView.transform.Find("Viewport/Content");
            _message = _transform.Find("Canvas/Panel/Scroll View/Message").gameObject;
            _messageText = _message.transform.Find("Text").GetComponent<Text>();

            _localPlayer = Networking.LocalPlayer;

            ClearTracks();
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
            if (Networking.IsMaster/* && isOwner*/ && player.isLocal && playOnAwake && !IsTrackDoneAll())
                PlayOn();
            ValidateView();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
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
            _shuffleOn.SetActive(!shuffle);
            _shuffleOnButton.interactable = privilege;
            _shuffleOff.SetActive(shuffle);
            _shuffleOffButton.interactable = privilege;
            _repeatOn.SetActive(!repeat);
            _repeatOnButton.interactable = privilege;
            _repeatOff.SetActive(repeat);
            _repeatOffButton.interactable = privilege;
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
            ClearTracks();
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
            if (IsTrackDoneAll() && repeat)
            {
                ClearTracks();
                ReorderTracks();
                TrackDone();
                _track = GetTrackNext();
                ClearTracks();
                ReorderTracks();
            }
            PlayTracks(GetTrackNext());
            RequestSerialization();
        }

        public bool shuffle
        {
            get => defaultShuffle;
            private set
            {
                defaultShuffle = value;
                UpdateShuffle();
            }
        }

        void UpdateShuffle()
        {
            ValidateView();
        }

        public void ShuffleOn()
        {
            TakeOwnership();
            shuffle = true;
            RequestSerialization();
        }

        public void ShuffleOff()
        {
            TakeOwnership();
            shuffle = false;
            RequestSerialization();
        }

        public bool repeat
        {
            get => defaultRepeat;
            private set
            {
                defaultRepeat = value;
                UpdateRepeat();
            }
        }

        void UpdateRepeat()
        {
            ValidateView();
        }

        public void RepeatOn()
        {
            TakeOwnership();
            repeat = true;
            RequestSerialization();
        }

        public void RepeatOff()
        {
            TakeOwnership();
            repeat = false;
            RequestSerialization();
        }

        public void OnButtonClicked()
        {
            var sender = FindSender();
            if (sender == null)
                return;

            TakeOwnership();
            ClearTracks();
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

        public int[] tracks
        {
            get => _tracks;
            private set
            {
                _tracks = value;
                UpdateTracks();
            }
        }

        void UpdateTracks()
        {
            var locked = controller.locked;
            var master = Networking.IsMaster || (controller.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            for (var i = 0; i < _content.childCount; i++)
            {
                var obj = _content.GetChild(i);
                if (!obj.gameObject.activeSelf)
                    continue;
                var button = obj.Find("Button").gameObject;
                var buttonButton = button.GetComponent<Button>();
                buttonButton.interactable = privilege && !IsTrackDone(i) && !IsTrackPlaying(i);
                var progressSlider = button.transform.Find("Progress").GetComponent<Slider>();
                progressSlider.value = IsTrackDone(i) ? 1f : 0f;
            }
        }

        void ClearTracks()
        {
            _tracks = new int[0];
            for (var i = 0; i < _content.childCount; i++)
            {
                var obj = _content.GetChild(i);
                if (!obj.gameObject.activeSelf)
                    continue;
                _tracks = _ArrayUtility_Add_intarray_int(_tracks, i);
            }
        }

        void ReorderTracks()
        {
            if (!IsTrackAvailable(_track))
                return;
            var idx = ArrayUtility_IndexOf_intarray_int(_tracks, _track);
            var tmp = new int[_tracks.Length];
            Array.Copy(_tracks, idx, tmp, 0, _tracks.Length - idx);
            Array.Copy(_tracks, 0, tmp, _tracks.Length - idx, idx);
            _tracks = tmp;
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
                UpdateTrack();
            }
            ReorderTracks();
            UpdateTracks();
            PlayTrack();
        }

        void StopTracks()
        {
            var playing = IsTrackPlaying(_track);
            _controlled = false;
            _wait = false;
            StopTrack(playing);
            ClearTracks();
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

            var obj = _content.GetChild(_track);
            _progressSlider = obj.transform.Find("Button/Progress").GetComponent<Slider>();
            _modeText = obj.transform.Find("Mode").GetComponent<Text>();
            _addressInput = (VRCUrlInputField)obj.transform.Find("Address").GetComponent(typeof(VRCUrlInputField));
        }

        void PlayTrack()
        {
            if (!_controlled || !IsTrackAvailable(_track))
                return;

            core.TakeOwnership();
            core.PlayURL(uint.Parse(_modeText.text), _addressInput.GetUrl());
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
            return _tracks == null || _tracks.Length == 0;
        }

        bool IsTrackAvailable(int track)
        {
            if (IsTrackDoneAll())
                return false;
            return _ArrayUtility_Contains_intarray_int(_tracks, track);
        }

        bool IsTrackPlaying(int track)
        {
            if (IsTrackDoneAll())
                return false;
            if (!core.isPrepared && !core.isPlaying && !core.isError)
                return false;
            return _controlled && IsTrackAvailable(track) && track == _track;
        }

        bool IsTrackDone(int track)
        {
            if (IsTrackDoneAll())
                return false;
            return !_ArrayUtility_Contains_intarray_int(_tracks, track);
        }

        void TrackDone()
        {
            if (IsTrackDoneAll())
                return;
            _tracks = _ArrayUtility_Remove_intarray_int(_tracks, _track);
        }

        int GetTrackNext()
        {
            if (IsTrackDoneAll())
                return -1;
            return _tracks[shuffle ? UnityEngine.Random.Range(0, _tracks.Length) : 0];
        }
    }
}
