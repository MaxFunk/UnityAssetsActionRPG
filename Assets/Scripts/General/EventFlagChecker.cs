using UnityEngine;
using UnityEngine.Events;
using static EventFlags;

public class EventFlagChecker : MonoBehaviour
{
    public enum CheckMode
    {
        Read,
        Write,
        ReadWrite
    }

    [Header("Config")]
    public EventFlag flagToCheck;
    public CheckMode checkMode;
    public bool readValue = false;
    public bool writeValue = false;
    public bool checkOnAwake = true;

    [Header("Event")]
    public UnityEvent Event = new();

    void Awake()
    {
        if (checkOnAwake)
        {
            ReadFlags();
            WriteFlags();
        }
    }

    public void ReadFlags()
    {
        if (checkMode == CheckMode.Read || checkMode == CheckMode.ReadWrite)
        {
            if (GameManager.Instance.EventFlags.GetFlag(flagToCheck) == readValue && Event != null)
            {
                Event.Invoke();
            }
        }
    }

    public void WriteFlags()
    {
        if (checkMode == CheckMode.Write || checkMode == CheckMode.ReadWrite)
        {
            GameManager.Instance.EventFlags.SetFlag(flagToCheck, writeValue);
        }
    }

    public bool IsFlagValueCorrect()
    {
        if (checkMode == CheckMode.Read || checkMode == CheckMode.ReadWrite)
        {
            return GameManager.Instance.EventFlags.GetFlag(flagToCheck) == readValue;
        }

        return false;
    }
}
