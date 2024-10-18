using io.agora.rtc.demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelVideo;
using static UnityEngine.GUILayout;
using UnityEngine.Serialization;


#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

public class VideoSession : MonoBehaviour
{
    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
    #endif

    [Header("_____________Basic Configuration_____________")]
    [FormerlySerializedAs("APP_ID")]
    [SerializeField]
    private string _appID = "";

    [FormerlySerializedAs("TOKEN")]
    [SerializeField]
    private string _token = "";

    [FormerlySerializedAs("CHANNEL_NAME")]
    [SerializeField]
    private string _channelName = "";

    [SerializeField] private AppIdInput _appIdInput;

    [SerializeField] internal VideoSurface LocalView;
    [SerializeField] internal VideoSurface RemoteView;
    internal IRtcEngine RtcEngine;

    private void Start()
    {
        CheckPermissions();
        LoadAssetData();

        if (CheckAppId())
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        }
    }

    private void OnEnable()
    {
        CustomEvents.CanvasChanged += CanvasChanged;

        CustomEvents.EnableNoviceView += EnableNoviceView;

        CustomEvents.EnableNoviceView += EnableExpertView;
    }

    private void OnDisable()
    {
        CustomEvents.CanvasChanged -= CanvasChanged;

        CustomEvents.EnableNoviceView -= EnableNoviceView;

        CustomEvents.EnableNoviceView -= EnableExpertView;
    }

    private void EnableNoviceView()
    {
        RtcEngine.SwitchCamera();
    }

    private void EnableExpertView()
    {

    }

    private void CanvasChanged(Canvases canvas)
    {
        switch (canvas)
        {
            case Canvases.HomeCanvas:
                break;
            case Canvases.VideoCanvas:

                InitEngine();

                Join();

                break;
        }
    }

    private bool CheckAppId()
    {
        if(_appID.Length > 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CheckPermissions()
    {
        #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)

            foreach (string permission in permissionList)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Permission.RequestUserPermission(permission);
                }
            }

        #endif
    }


    public void Join()
    {
        // Set channel media options
        ChannelMediaOptions options = new ChannelMediaOptions();
        // Publish the audio stream collected from the microphone
        options.publishMicrophoneTrack.SetValue(true);
        // Publish the video stream collected from the camera
        options.publishCameraTrack.SetValue(true);
        // Automatically subscribe to all audio streams
        options.autoSubscribeAudio.SetValue(true);
        // Automatically subscribe to all video streams
        options.autoSubscribeVideo.SetValue(true);
        // Set the channel profile to live broadcasting
        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        // Set the user role to broadcaster
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        // Join the channel
        RtcEngine.JoinChannel(_token, _channelName, 0, options);
    }


    public void Leave()
    {
        Debug.Log("Leaving " + _channelName);
        // Leave the channel
        RtcEngine.LeaveChannel();
        // Disable the video module
        RtcEngine.DisableVideo();
        // Stop remote video rendering0
        RemoteView.SetEnable(false);
        // Stop local video rendering
        LocalView.SetEnable(false);
    }


    //Show data in AgoraBasicProfile
    [ContextMenu("ShowAgoraBasicProfileData")]
    private void LoadAssetData()
    {
        if (_appIdInput == null) return;
        _appID = _appIdInput.appID;
        _token = _appIdInput.token;
        _channelName = _appIdInput.channelName;
    }

    private void InitEngine()
    {
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext();

        context.appId = _appIdInput.appID;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
        context.areaCode = AREA_CODE.AREA_CODE_GLOB;
        var result = RtcEngine.Initialize(context);
        //this.Log.UpdateLog("Initialize result : " + result);

        RtcEngine.InitEventHandler(handler);

        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();

        // Set local video display
        LocalView.SetForUser(0, "");
        // Start rendering video
        LocalView.SetEnable(true);

        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(640, 360);
        config.frameRate = 15;
        config.bitrate = 0;
        RtcEngine.SetVideoEncoderConfiguration(config);
        RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
        if (RtcEngine == null) return;
        RtcEngine.InitEventHandler(null);
        RtcEngine.LeaveChannel();
        RtcEngine.Dispose();
    }
}

// Implement your own callback class by inheriting from the IRtcEngineEventHandler interface
internal class UserEventHandler : IRtcEngineEventHandler
{
    private readonly VideoSession _videoSample;
    internal UserEventHandler(VideoSession videoSample)
    {
        _videoSample = videoSample;
    }

    // This callback is triggered when the local user successfully joins the channel
    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
    }

    // The OnUserJoined callback is triggered when the SDK receives and successfully decodes the first frame of remote video
    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        // Set the display for the remote video
        _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        // Start video rendering
        _videoSample.RemoteView.SetEnable(true);
        Debug.Log("Remote user joined");
    }

    // This callback is triggered when the remote user leaves the current channel
    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        _videoSample.RemoteView.SetEnable(false);
    }
}

