using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace HoshinoLabs.Udon.IwaSync3
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ListTab : UdonSharpBehaviour
    {
        #region UExtenderLite
        bool[] _ArrayUtility_Add_boolarray_bool(bool[] _0, bool _1)
        {
            var result = new bool[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }
        #endregion

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [Header("Options")]
        [SerializeField]
        bool allowSwitchOff = false;
        [SerializeField]
        bool multiView = false;

        [SerializeField]
        [HideInInspector]
        Transform _transform;
        [SerializeField]
        [HideInInspector]
        GameObject[] _lists;

        GameObject _lockOn;
        Button _lockOnButton;
        GameObject _lockOff;
        Button _lockOffButton;
        GameObject _scrollView;
        Transform _content;
        GameObject _message;
        Text _messageText;

        [SerializeField]
        [HideInInspector]
        [UdonSynced, FieldChangeCallback(nameof(tabs))]
        bool[] _tabs;

        bool _updatingTabs = false;

        VRCPlayerApi _localPlayer;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Started `{nameof(ListTab)}`.");

            controller.AddListener(this);

            _lockOn = _transform.Find("Canvas/Panel/Header/Lock/On").gameObject;
            _lockOnButton = _lockOn.transform.Find("Button").GetComponent<Button>();
            _lockOff = _transform.Find("Canvas/Panel/Header/Lock/Off").gameObject;
            _lockOffButton = _lockOff.transform.Find("Button").GetComponent<Button>();
            _scrollView = _transform.Find("Canvas/Panel/Scroll View/Scroll View").gameObject;
            _content = _scrollView.transform.Find("Viewport/Content");
            _message = _transform.Find("Canvas/Panel/Scroll View/Message").gameObject;
            _messageText = _message.transform.Find("Text").GetComponent<Text>();

            _localPlayer = Networking.LocalPlayer;
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
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
            ValidateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            ValidateView();
        }

        public override void OnVideoStart()
        {
            ValidateView();
        }
        #endregion

        #region VideoCoreEvent
        public void OnChangeURL()
        {
            _messageText.text = $"Loading Now";
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

            _lockOn.SetActive(!locked);
            _lockOnButton.interactable = master;
            _lockOff.SetActive(locked);
            _lockOffButton.interactable = master;
            _scrollView.SetActive(!core.isPrepared || core.isError);
            _message.SetActive(core.isPrepared && !core.isError);

            UpdateTabs();
        }

        public void LockOn()
        {
            controller.LockOn();
        }

        public void LockOff()
        {
            controller.LockOff();
        }

        public void OnButtonClicked()
        {
            if (_updatingTabs)
                return;

            var sender = FindSender();
            if (sender == null)
                return;

            TakeOwnership();
            ChoiceTab(sender.GetSiblingIndex() - 1);
            UpdateTabs();
            RequestSerialization();
        }

        Transform FindSender()
        {
            foreach (var x in _content.GetComponentsInChildren<Toggle>())
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

        public bool[] tabs
        {
            get => _tabs;
            private set
            {
                _tabs = value;
                UpdateTabs();
            }
        }

        void UpdateTabs()
        {
            _updatingTabs = true;

            var locked = controller.locked;
            var master = Networking.IsMaster || (controller.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            for (var i = 0; i < _tabs.Length; i++)
            {
                var obj = _content.GetChild(i + 1);
                if (!obj.gameObject.activeSelf)
                    continue;
                var toggle = obj.Find("Toggle").gameObject;
                var toggleToggle = toggle.GetComponent<Toggle>();
                toggleToggle.interactable = privilege;
                toggleToggle.isOn = _tabs[i];
                if (_lists[i] != null)
                    _lists[i].SetActive(_tabs[i]);
            }

            _updatingTabs = false;
        }

        void ChoiceTab(int tab)
        {
            if (!allowSwitchOff)
            {
                var cnt = 0;
                for (var i = 0; i < _tabs.Length; i++)
                    cnt += i == tab ? (_tabs[i] ? 0 : 1) : (_tabs[i] ? 1 : 0);
                if (cnt <= 0)
                    return;
            }
            if (!multiView)
            {
                for (var i = 0; i < _tabs.Length; i++)
                {
                    if (i != tab)
                        _tabs[i] = false;
                }
            }
            _tabs[tab] = !_tabs[tab];
        }
    }
}
