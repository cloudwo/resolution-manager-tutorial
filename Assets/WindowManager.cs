using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    #region Attributes

    #region Player Pref Key Constants

    private const string RESOLUTION_PREF_KEY = "resolution";
    private const string WINDOW_MODE_PREF_KEY = "window-mode";

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

        currentWindowModeSetting = GetPlayerPrefBool(WINDOW_MODE_PREF_KEY, true);

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

    #endregion

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
        currentResolutionIndex = GetNextWrappedIndex(resolutions, currentResolutionIndex);
        SetResolutionText(resolutions[currentResolutionIndex]);
    }

    public void SetPreviousResolution()
    {
        currentResolutionIndex = GetPreviousWrappedIndex(resolutions, currentResolutionIndex);
        SetResolutionText(resolutions[currentResolutionIndex]);
    }

    #endregion

    #region Window Mode

    #region Edit Current Window Mode

    public void SetWindowMode(bool setting)
    {
        currentWindowModeSetting = setting;
    }

    public void ApplyCurrentWindowMode()
    {
        ApplyWindowMode(currentWindowModeSetting);
    }

    #endregion

    #region Edit Window Mode

    private void SetAndApplyWindowMode(bool setting)
    {
        SetWindowMode(setting);
        ApplyWindowMode(setting);
    }

    private void ApplyWindowMode(bool setting)
    {
        Screen.fullScreen = setting;
        SetPlayerPrefBool(WINDOW_MODE_PREF_KEY, setting);
    }

    #endregion

    #endregion

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

        resetVideoCoroutine = StartCoroutine(CountDown(time,
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

    public void ConfirmChangesAndCancelVideoReset()
    {
        if (resetVideoCoroutine != null) StopCoroutine(resetVideoCoroutine);

        HideResetVideoSettingsDialogue();
        ConfirmVideoSettingChanges();
    }

    private void ConfirmVideoSettingChanges()
    {
        previousResolutionSetting = currentResolutionIndex;
        previousWindowModeSetting = Screen.fullScreen;
    }

    #region Video Settings Reset Dialogue Visual Actions

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

    #endregion

    #endregion

    #region Misc Helpers

    #region PlayerPref Helpers

    public bool GetPlayerPrefBool(string key)
    {
        var value = PlayerPrefs.GetInt(key);
        return value > 0;
    }

    public bool GetPlayerPrefBool(string key, bool defaultValue)
    {
        if (!PlayerPrefs.HasKey(key)) return defaultValue;
        return GetPlayerPrefBool(key);
    }

    public void SetPlayerPrefBool(string key, bool value)
    {
        int integerVersion = (value == true) ? 1 : 0;
        PlayerPrefs.SetInt(key, integerVersion);
    }

    #endregion

    #region Index Wrap Helpers

    public int GetNextWrappedIndex<T>(IList<T> collection, int currentIndex)
    {
        if (collection.Count < 1) return 0;
        return (currentIndex + 1) % collection.Count;
    }

    public int GetPreviousWrappedIndex<T>(IList<T> collection, int currentIndex)
    {
        if (collection.Count < 1) return 0;
        if ((currentIndex - 1) < 0) return collection.Count - 1;
        return (currentIndex - 1) % collection.Count;
    }

    #endregion

    private IEnumerator CountDown(int start, System.Action<int> iterationCallback, System.Action endCallback, int secondsPerTick = 1)
    {
        for (int i = start; i > 0; i--)
        {
            iterationCallback(i);
            yield return new WaitForSeconds(secondsPerTick);
        }

        endCallback();
    }

    #endregion
}