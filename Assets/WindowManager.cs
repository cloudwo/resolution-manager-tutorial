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

    #region Window Mode

    [SerializeField]
    private Toggle fullscreenToggle;

    [SerializeField]
    private Toggle windowedToggle;

    private bool currentFullScreenEnabled;

    #endregion

    #region Safe Reset Video Settings

    private Coroutine resetVideoCoroutine;

    private int previousResolutionSetting;
    private bool previousFullScreenEnabled;

    [SerializeField]
    private GameObject resetVideoDialogue;

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
        ApplyResolution(resolutions[currentResolutionIndex]);

        currentFullScreenEnabled = GetPlayerPrefBool(WINDOW_MODE_PREF_KEY, true);

        fullscreenToggle.isOn = currentFullScreenEnabled;
        windowedToggle.isOn = !currentFullScreenEnabled;

        previousFullScreenEnabled = currentFullScreenEnabled;

        Screen.fullScreen = currentFullScreenEnabled;
    }

    #endregion

    #region Edit Resolution

    #region Resolution Cycling

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

    #region Applying Resolution

    private void SetAndApplyResolution(int newResolutionIndex)
    {
        currentResolutionIndex = newResolutionIndex;
        ApplyCurrentResolution();
    }

    private void ApplyCurrentResolution()
    {
        ApplyResolution(resolutions[currentResolutionIndex]);
    }

    private void ApplyResolution(Resolution resolution)
    {
        SetResolutionText(resolution);

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(RESOLUTION_PREF_KEY, currentResolutionIndex);
    }

    #endregion

    #endregion

    #region Window Mode

    #region Edit Current Window Mode

    private void SetWindowMode(bool setting)
    {
        currentFullScreenEnabled = setting;
    }

    private void ApplyCurrentWindowMode()
    {
        ApplyWindowMode(currentFullScreenEnabled);
    }

    #endregion

    #region Apply Window Mode

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

    #region Toggle Event Listeners

    public void OnFullScreenToggleValueChanged(bool value)
    {
        SetWindowMode(value);
    }

    #endregion

    #endregion

    #region Safely Reset Video Settings

    private void ResetAfterTime(int time)
    {
        SetResetCountDownText(time);

        ShowResetVideoSettingsDialogue();

        resetVideoCoroutine = StartCoroutine(CountDown(time,
            SetResetCountDownText, ResetVideoSettings));
    }

    private void ResetVideoSettings()
    {
        HideResetVideoSettingsDialogue();
        SetAndApplyResolution(previousResolutionSetting);        

        SetAndApplyWindowMode(previousFullScreenEnabled);

        fullscreenToggle.isOn = previousFullScreenEnabled;
        windowedToggle.isOn = !previousFullScreenEnabled;
    }

    private void ConfirmChangesAndCancelVideoReset()
    {
        if (resetVideoCoroutine != null) StopCoroutine(resetVideoCoroutine);

        HideResetVideoSettingsDialogue();
        ConfirmVideoSettingChanges();
    }

    private void ConfirmVideoSettingChanges()
    {
        previousResolutionSetting = currentResolutionIndex;
        previousFullScreenEnabled = Screen.fullScreen;
    }

    #region Video Settings Reset Dialogue Visual Actions

    private void SetResetCountDownText(int timeLeft)
    {
        resetVideoDialogueText.text =
            "reset in " + timeLeft + " seconds.";
    }

    private void ShowResetVideoSettingsDialogue()
    {
        resetVideoDialogue.SetActive(true);
    }

    private void HideResetVideoSettingsDialogue()
    {
        resetVideoDialogue.SetActive(false);   
    }

    #endregion

    #endregion

    #region Misc Helpers

    #region PlayerPref Helpers

    private bool GetPlayerPrefBool(string key)
    {
        var value = PlayerPrefs.GetInt(key);
        return value > 0;
    }

    private bool GetPlayerPrefBool(string key, bool defaultValue)
    {
        if (!PlayerPrefs.HasKey(key)) return defaultValue;
        return GetPlayerPrefBool(key);
    }

    private void SetPlayerPrefBool(string key, bool value)
    {
        int integerVersion = (value == true) ? 1 : 0;
        PlayerPrefs.SetInt(key, integerVersion);
    }

    #endregion

    #region Index Wrap Helpers

    private int GetNextWrappedIndex<T>(IList<T> collection, int currentIndex)
    {
        if (collection.Count < 1) return 0;
        return (currentIndex + 1) % collection.Count;
    }

    private int GetPreviousWrappedIndex<T>(IList<T> collection, int currentIndex)
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

    public void ApplyChanges()
    {
        SetAndApplyResolution(currentResolutionIndex);
        SetAndApplyWindowMode(currentFullScreenEnabled);

        ResetAfterTime(5);
    }
}