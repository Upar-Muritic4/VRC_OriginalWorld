using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon;

namespace HoshinoLabs.Udon.IwaSync3
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoCore : UdonSharpBehaviour
    {
        [Header("Options")]
        [SerializeField]
        uint defaultMode = _MODE_VIDEO;
        [SerializeField]
        VRCUrl defaultUrl = VRCUrl.Empty;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(loop))]
        bool defaultLoop = false;
        [SerializeField]
        float syncFrequency = 9.2f;
        [SerializeField]
        float syncThreshold = 0.92f;
        [SerializeField]
        [Range(0, 10)]
        int maxErrorRetry = 3;
        [SerializeField]
        [Range(10f, 30f)]
        float timeoutUnknownError = 10f;
        [SerializeField]
        [Range(6f, 30f)]
        float timeoutPlayerError = 6f;
        [SerializeField]
        [Range(6f, 30f)]
        float timeoutRateLimited = 6f;

        [SerializeField]
        [HideInInspector]
        Transform _transform;

        BaseVRCVideoPlayer _unityVideoPlayer;
        Renderer _unityVideoRenderer;
        BaseVRCVideoPlayer _avProVideoPlayer;
        Renderer _avProVideoRenderer;
        Animator _animator;

        MaterialPropertyBlock _properties = null;

        const uint _status_stop = 0x00010000;
        const uint _status_fetch = 0x00020000;
        const uint _status_play = 0x00040000;
        const uint _status_error = 0x00100000;
        const uint _status_live = 0x01000000;
        const uint _status_reload = 0x10000000;

        uint _status = _status_stop;
        BaseVRCVideoPlayer _player = null;
        Renderer _renderer = null;
        int _frame;
        VideoError _error;

        CustomEventInvoker _invoker;
        int _errorRetry;

        [UdonSynced, FieldChangeCallback(nameof(mode))]
        uint _mode;
        [UdonSynced, FieldChangeCallback(nameof(url))]
        VRCUrl _url = VRCUrl.Empty;
        [UdonSynced, FieldChangeCallback(nameof(clockTime))]
        int _clockTime;
        [FieldChangeCallback(nameof(offsetTime))]
        int _offsetTime;
        [UdonSynced, FieldChangeCallback(nameof(time))]
        float _time;
        [UdonSynced, FieldChangeCallback(nameof(speed))]
        float _speed = 1f;
        [UdonSynced, FieldChangeCallback(nameof(paused))]
        bool _paused = false;
        [UdonSynced, FieldChangeCallback(nameof(ownerPlaying))]
        bool _ownerPlaying = false;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Started `{nameof(VideoCore)}`.");

            _unityVideoPlayer = (BaseVRCVideoPlayer)GetComponents(typeof(BaseVRCVideoPlayer))[0];
            _unityVideoRenderer = _transform.Find("UnityVideo").GetComponent<Renderer>();
            _avProVideoPlayer = (BaseVRCVideoPlayer)GetComponents(typeof(BaseVRCVideoPlayer))[1];
            _avProVideoRenderer = _transform.Find("AVProVideo").GetComponent<Renderer>();
            _animator = GetComponent<Animator>();

            _properties = new MaterialPropertyBlock();

            _invoker = GetComponentInChildren<CustomEventInvoker>(true);
        }

        #region EventListener
        VideoCoreEventListener[] _listeners;

        public void AddListener(UdonSharpBehaviour listener)
        {
            if (_listeners == null)
                _listeners = new VideoCoreEventListener[0];
            var array = new VideoCoreEventListener[_listeners.Length + 1];
            _listeners.CopyTo(array, 0);
            array[_listeners.Length] = (VideoCoreEventListener)listener;
            _listeners = array;
        }
        #endregion

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsMaster/* && isOwner*/ && player.isLocal && IsValidURL(defaultUrl.Get()))
            {
                PlayURL(defaultMode, defaultUrl);
                RequestSerialization();
            }
        }
        #endregion

        public bool isError => (_status & _status_error) != 0;

        public VideoError error => _error;

        public int errorRetry => _errorRetry;

        #region VideoEvent
        public override void OnVideoEnd()
        {
            if (isLive)
                return;
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] The video has reached the end.");
            _unityVideoPlayer.Stop();
            _avProVideoPlayer.Stop();
            _status = (_status | _status_stop) & ~_status_fetch & ~_status_play/* & ~_status_pause*/;
            _status = _status & ~_status_error;
            _status = _status & ~_status_live;
            _status = _status & ~_status_reload;
            _url = VRCUrl.Empty;
            _paused = false;
            _ownerPlaying = false;
            // retry
            _invoker.ClearInvoke();
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of video end.");
                foreach (var x in _listeners)
                    x.OnVideoEnd();
            }
        }

        public override void OnVideoError(VideoError videoError)
        {
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] There was a `{videoError}` error in the video.");
            _unityVideoPlayer.Stop();
            _avProVideoPlayer.Stop();
            _status = (_status | _status_stop) & ~_status_fetch & ~_status_play/* & ~_status_pause*/;
            _status = _status | _status_error;
            // error
            _error = videoError;
            // retry
            _invoker.ClearInvoke();
            if (0 < _errorRetry)
                RetryVideoError(videoError);
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of video error caused by `{videoError}`.");
                foreach (var x in _listeners)
                    x.OnVideoError(videoError);
            }
        }

        public override void OnVideoLoop()
        {
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] The video looped.");
            // time
            if (isOwner)
            {
                _clockTime = Networking.GetServerTimeInMilliseconds();
                _time = 0f;
            }
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of video loop.");
                foreach (var x in _listeners)
                    x.OnVideoLoop();
            }
        }

        public override void OnVideoReady()
        {
            if (!isPrepared && !isPlaying)
            {
                _unityVideoPlayer.Stop();
                _avProVideoPlayer.Stop();
                return;
            }
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] The video is ready.");
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of video ready.");
                foreach (var x in _listeners)
                    x.OnVideoReady();
            }
        }

        public override void OnVideoStart()
        {
            if (isPlaying)
                return;
            if (!isPrepared && !isPlaying)
            {
                _unityVideoPlayer.Stop();
                _avProVideoPlayer.Stop();
                return;
            }
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] The video has started playing.");
            _animator.Rebind();
            // playing
            _status = (_status & ~_status_stop) & ~_status_fetch | _status_play;
            // livestream
            if (float.IsInfinity(_player.GetDuration()) || IsRTSP(_url.Get()))
            {
                _status = _status | _status_live;
                // event emit
                if (_listeners != null)
                {
                    Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of change live.");
                    foreach (var x in _listeners)
                        x.OnChangeLive();
                }
            }
            // loop
            UpdateLoop();
            // time
            if (isOwner && !isReload)
            {
                _clockTime = Networking.GetServerTimeInMilliseconds();
                _time = 0f;
            }
            SyncTimePeriodic();
            // speed
            UpdateSpeed();
            // pause
            if (paused)
                UpdatePaused();
            // retry
            _invoker.ClearInvoke();
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of video start.");
                foreach (var x in _listeners)
                    x.OnVideoStart();
            }
        }
        #endregion

        public void TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public bool isOwner => Networking.IsOwner(gameObject);

        public void SyncTimePeriodic()
        {
            var frame = Time.frameCount;
            if (!isPlaying || _frame == frame)
                return;
            _frame = frame;
            UpdateTime(false);
            if(isOwner)
                RequestSerialization();
            SendCustomEventDelayedSeconds(nameof(SyncTimePeriodic), syncFrequency);
        }

        const uint _MODE_VIDEO = 0x00000010;
        public
        #if !COMPILER_UDONSHARP
        static
        #endif
        uint MODE_VIDEO => _MODE_VIDEO;
        const uint _MODE_STREAM = 0x00000020;
        public
        #if !COMPILER_UDONSHARP
        static
        #endif
        uint MODE_STREAM => _MODE_STREAM;

        public bool isModeVideo => _mode == _MODE_VIDEO;
        public bool isModeStream => _mode == _MODE_STREAM;

        public uint mode
        {
            get
            {
                if (isModeStream)
                    return MODE_STREAM;
                return MODE_VIDEO;
            }
            private set
            {
                _mode = value;
                UpdateMode();
            }
        }

        void UpdateMode()
        {
            if (isModeVideo)
            {
                _player = _unityVideoPlayer;
                _renderer = _unityVideoRenderer;
                return;
            }
            if (isModeStream)
            {
                _player = _avProVideoPlayer;
                _renderer = _avProVideoRenderer;
                return;
            }
        }

        public VRCUrl url
        {
            get => _url;
            private set
            {
                _url = value;
                LoadURL();
            }
        }

        public void PlayURL(uint mode, VRCUrl url)
        {
            _mode = mode;
            UpdateMode();
            _url = url;
            _clockTime = Networking.GetServerTimeInMilliseconds();
            _time = 0f;
            _speed = 1f;
            _paused = false;
            _ownerPlaying = true;
            LoadURL();
        }

        void LoadURL()
        {
            // retry
            _errorRetry = maxErrorRetry;
            // load
            ReloadURL();
        }

        void ReloadURL()
        {
            if (!IsValidURL(_url.Get()))
                return;
            _unityVideoPlayer.Stop();
            _avProVideoPlayer.Stop();
            _status = ((_status & ~_status_stop) | _status_fetch) & ~_status_play/* & ~_status_pause*/;
            _status = _status & ~_status_error;
            _status = _status & ~_status_live;
            // timeout
            _invoker.ClearInvoke();
            _invoker.Invoke(this, nameof(TimeoutVideoLoading), timeoutUnknownError);
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of change url.");
                foreach (var x in _listeners)
                    x.OnChangeURL();
            }
            // play
            _player.PlayURL(_url);
        }

        public bool loop
        {
            get => defaultLoop;
            set
            {
                defaultLoop = value;
                UpdateLoop();
            }
        }

        void UpdateLoop()
        {
            _unityVideoPlayer.Loop = defaultLoop;
            _avProVideoPlayer.Loop = defaultLoop;
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of change loop.");
                foreach (var x in _listeners)
                    x.OnChangeLoop();
            }
        }

        public int clockTime
        {
            get => _clockTime;
            set
            {
                _clockTime = value;
            }
        }

        public int offsetTime
        {
            get => _offsetTime;
            set
            {
                _offsetTime = value;
                UpdateTime(true);
            }
        }

        public float duration
        {
            get
            {
                if (!isPlaying || isLive)
                    return 0f;
                var duration = _player.GetDuration();
                // livestream
                if (float.IsInfinity(duration))
                {
                    _status = _status | _status_live;
                    // event emit
                    if (_listeners != null)
                    {
                        Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of change live.");
                        foreach (var x in _listeners)
                            x.OnChangeLive();
                    }
                    return 0f;
                }
                return duration;
            }
        }

        public float time
        {
            get => isPlaying ? _player.GetTime() : 0f;
            set
            {
                _time = value;
                UpdateTime(true);
            }
        }

        void UpdateTime(bool force)
        {
            if (!isPlaying || isLive)
                return;
            var offset = _time;
            if (!_paused)
                offset += (Networking.GetServerTimeInMilliseconds() - _clockTime) / 1000f * _speed;
            var offsetLocal = _offsetTime / 1000f;
            if (force || syncThreshold <= Mathf.Abs((_player.GetTime() - offsetLocal) - offset))
            {
                //_player.SetTime(offset + offsetLocal - 1f);
                _player.SetTime(offset + offsetLocal);
            }
        }

        public float speed
        {
            get => _speed;
            set
            {
                _speed = value;
                UpdateSpeed();
            }
        }

        void UpdateSpeed()
        {
            _animator.SetInteger("PlayBackSpeed", (int)(_speed * 100f));
            UpdateTime(true);
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of change speed.");
                foreach (var x in _listeners)
                    x.OnChangeSpeed();
            }
        }

        public void Seek(float offset)
        {
            if (!isPlaying)
                return;
            var offsetLocal = _offsetTime / 1000f;
            _time = (_player.GetTime() - offsetLocal) + offset;
            UpdateTime(true);
        }

        public Texture texture
        {
            get
            {
                if (isModeVideo)
                {
                    _renderer.GetPropertyBlock(_properties);
                    return _properties.GetTexture("_MainTex");
                }
                return _renderer.material.GetTexture("_MainTex");
            }
        }

        public bool isPrepared => (_status & _status_fetch) != 0;
        public bool isPlaying => (_status & _status_play) != 0;

        public bool isLive => (_status & _status_live) != 0;

        public bool isReload => (_status & _status_reload) != 0;

        public bool paused
        {
            get => _paused;
            private set
            {
                _paused = value;
                UpdatePaused();
            }
        }

        void UpdatePaused()
        {
            if (isLive)
                return;
            if (_paused)
            {
                _unityVideoPlayer.Pause();
                _avProVideoPlayer.Pause();
                // event emit
                if (_listeners != null)
                {
                    Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of player pause.");
                    foreach (var x in _listeners)
                        x.OnPlayerPause();
                }
            }
            else
            {
                _unityVideoPlayer.Play();
                _avProVideoPlayer.Play();
                // event emit
                if (_listeners != null)
                {
                    Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of player play.");
                    foreach (var x in _listeners)
                        x.OnPlayerPlay();
                }
            }
        }

        public void Play()
        {
            _paused = false;
            UpdatePaused();
        }

        public void Pause()
        {
            _paused = true;
            UpdatePaused();
        }

        public bool ownerPlaying
        {
            get => _ownerPlaying;
            set
            {
                _ownerPlaying = value;
                UpdateOwnerPlaying();
            }
        }

        void UpdateOwnerPlaying()
        {
            if (_ownerPlaying)
                return;
            _unityVideoPlayer.Stop();
            _avProVideoPlayer.Stop();
            _status = (_status | _status_stop) & ~_status_fetch & ~_status_play;
            _status = _status & ~_status_error;
            _status = _status & ~_status_live;
            _status = _status & ~_status_reload;
            _url = VRCUrl.Empty;
            _paused = false;
            _ownerPlaying = false;
            // retry
            _invoker.ClearInvoke();
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of player stop.");
                foreach (var x in _listeners)
                    x.OnPlayerStop();
            }
        }

        public void Stop()
        {
            _ownerPlaying = false;
            UpdateOwnerPlaying();
        }

        public void Reload()
        {
            _status = _status | _status_reload;
            // load
            LoadURL();
        }

        public bool IsValidURL(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || IsRTSP(url);
        }

        public bool IsRTSP(string url)
        {
            return url.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("rtmp://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("rtspt://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("rtspu://", StringComparison.OrdinalIgnoreCase);
        }

        void RetryVideoError(VideoError videoError)
        {
            switch (videoError)
            {
                case VideoError.PlayerError:
                    _invoker.Invoke(this, nameof(RecoveryVideoError), timeoutPlayerError);
                    break;
                case VideoError.RateLimited:
                    _invoker.Invoke(this, nameof(RecoveryVideoError), timeoutRateLimited);
                    break;
                case VideoError.AccessDenied:
                    _invoker.Invoke(this, nameof(RecoveryVideoErrorAccessDenied), timeoutRateLimited);
                    break;
                default:
                    _errorRetry = 0;
                    break;
            }
        }

        public void TimeoutVideoLoading()
        {
            if (0 < _errorRetry)
            {
                _unityVideoPlayer.Stop();
                _avProVideoPlayer.Stop();
                _status = _status & ~_status_fetch;
                _status = _status | _status_error;
                // error
                _error = VideoError.Unknown;
                // retry
                RecoveryVideoError();
                // event emit
                if (_listeners != null)
                {
                    Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of video error caused by `{VideoError.Unknown}`.");
                    foreach (var x in _listeners)
                        x.OnVideoError(VideoError.Unknown);
                }
                return;
            }
            OnVideoError(VideoError.Unknown);
        }

        public void RecoveryVideoError()
        {
            _status = _status | _status_reload;
            // retry
            _errorRetry--;
            // load
            ReloadURL();
        }

        public void RecoveryVideoErrorAccessDenied()
        {
            //_status = _status | _status_reload;
            // error
            _error = VideoError.AccessDenied;
            // retry
            _errorRetry = 0;
            // event emit
            if (_listeners != null)
            {
                Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Emit event of video error caused by `{VideoError.AccessDenied}`.");
                foreach (var x in _listeners)
                    x.OnVideoError(VideoError.AccessDenied);
            }
        }
    }
}
