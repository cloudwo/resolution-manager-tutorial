using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResoultionManager : MonoBehaviour {

    /namespace ProjectVelocity
{
    public class SettingsManager : MonoBehaviour
    {
        #region Attributes

        #region Player Pref Key Constants

        private const string MUSIC_PREF_KEY = "music-volume";
        private const string SFX_PREF_KEY = "sfx-volume";
        private const string RESOLUTION_PREF_KEY = "resolution";
        private const string WINDOW_MODE_PREF_KEY = "window-mode";

        #endregion

        #region Audio

        [SerializeField]
        private AudioManager audioManager;

        [SerializeField]
        private Slider sfxVolumeSlider;

        [SerializeField]
        private Slider musicVolumeSlider;

        #endregion

        #region Resolution

        [SerializeField]
        private Text resolutionText;

        Resolution[] resolutions;

        private int currentResolutionIndex = 0;

        #endregion

        #region Screen Mode

        [SerializeField]
        private Toggle fullscreenToggle;

        [SerializeField]
        private Toggle windowedToggle;
        bool currentWindowModeSetting;

        #endregion

        #region Safe Reset Video Settings

        private Coroutine resetVideoCoroutine;

        private int previousResolutionSetting;
        private bool previousWindowModeSetting;

        [SerializeField]
        private CanvasGroup resetVideoDialogue;

        [SerializeField]
        private Text resetVideoDialogueText;

        #endregion

        #endregion

        #region MonoBehaviour API

        private void Start()
        {
            resolutions = Screen.resolutions;

            currentResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_PREF_KEY, resolutions.Length - 1);
            previousResolutionSetting = currentResolutionIndex;
            SetResolution(resolutions[currentResolutionIndex]);

            currentWindowModeSetting = PlayerPrefHelper.GetBool(WINDOW_MODE_PREF_KEY, true);

            if (currentWindowModeSetting) fullscreenToggle.isOn = true;
            else windowedToggle.isOn = true;

            previousWindowModeSetting = currentWindowModeSetting;

            Screen.fullScreen = currentWindowModeSetting;
        }

        private void OnEnable()
        {
            fullscreenToggle.onValueChanged.AddListener(OnFullScreenToggleValueChanged);
        }

        private void OnDisable()
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullScreenToggleValueChanged);
        }

        #region Resolution Cycling

        public void SetCurrentResolution()
        {
            SetResolution(resolutions[currentResolutionIndex]);
        }

        public void SetResolution(Resolution resolution)
        {
            SetResolutionText(resolution);

            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt(RESOLUTION_PREF_KEY, currentResolutionIndex);
        }

        private void SetResolutionText(Resolution resolution)
        {
            resolutionText.text = resolution.width + "x" + resolution.height;
        }

        public void SetNextResolution()
        {
            currentResolutionIndex = resolutions.GetNextWrappedIndex(currentResolutionIndex);
            SetResolutionText(resolutions[currentResolutionIndex]);
        }

        public void SetPreviousResolution()
        {
            currentResolutionIndex = resolutions.GetPreviousWrappedIndex(currentResolutionIndex);
            SetResolutionText(resolutions[currentResolutionIndex]);
        }

        #endregion

        #region Window Mode

        public void SetWindowMode(bool setting)
        {
            currentWindowModeSetting = setting;
        }

        private void ApplyWindowMode(bool setting)
        {
            Screen.fullScreen = setting;
            PlayerPrefHelper.SetBool(WINDOW_MODE_PREF_KEY, setting);
        }

        #region Toggle Event Listeners

        public void OnFullScreenToggleValueChanged(bool value)
        {
            SetWindowMode(value);
        }

        #endregion

        #region Safely Reset Video Settings

        public void ResetAfterTime(int time)
        {
            ShowResetVideoSettingsDialogue();

            SetResetCountDownText(time);

            resetVideoCoroutine = StartCoroutine(IEnumeratorCountdown.CountDown(time,
                SetResetCountDownText, ResetVideoSettings));
        }

        public void ResetVideoSettings()
        {
            HideResetVideoSettingsDialogue();
            currentResolutionIndex = previousResolutionSetting;
            SetCurrentResolution();

            SetAndApplyWindowMode(previousWindowModeSetting);

            if (previousWindowModeSetting)
            {
                fullscreenToggle.isOn = true;
                windowedToggle.isOn = false;
            }
            else
            {
                fullscreenToggle.isOn = false;
                windowedToggle.isOn = true;
            }
        }

        private void SetAndApplyWindowMode(bool setting)
        {
            SetWindowMode(setting);
            ApplyWindowMode(setting);
        }

        public void ApplyCurrentWindowMode()
        {
            ApplyWindowMode(currentWindowModeSetting);
        }

        private void SetResetCountDownText(int timeLeft)
        {
            resetVideoDialogueText.text =
                "reset in " + timeLeft + " seconds.";
        }

        private void ShowResetVideoSettingsDialogue()
        {
            resetVideoDialogue.alpha = 1;
            resetVideoDialogue.blocksRaycasts = true;
            resetVideoDialogue.interactable = true;
        }

        private void HideResetVideoSettingsDialogue()
        {
            resetVideoDialogue.alpha = 0;
            resetVideoDialogue.blocksRaycasts = false;
            resetVideoDialogue.interactable = false;
        }

        public void CancelVideoResetAndConfirmChanges()
        {
            this.StopCoroutineSafe(resetVideoCoroutine);
            HideResetVideoSettingsDialogue();

            previousResolutionSetting = currentResolutionIndex;
            previousWindowModeSetting = Screen.fullScreen;
        }

        #endregion
    }
}
