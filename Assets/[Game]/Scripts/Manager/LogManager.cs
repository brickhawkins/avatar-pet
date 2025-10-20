using System.Text;
using TMPro;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    [SerializeField] TMP_Text logText;
    [SerializeField] int maxLogLines = 20;

    int logLines = 0;
    readonly StringBuilder logBuilder = new();

    void OnEnable()
    {
        Logger.OnLog += HandleLogEntry;
    }

    void OnDisable()
    {
        Logger.OnLog -= HandleLogEntry;
    }

    void HandleLogEntry(string log)
    {
        Append(log);
    }

    void Append(string line)
    {
        if (logText == null) return;

        logBuilder.Append(line);
        logBuilder.Append('\n');
        logLines++;

        // Trim if we exceed max lines
        if (logLines > maxLogLines)
        {
            // Remove the first line without allocating a new string
            int firstNewline = IndexOfNewline(logBuilder);
            if (firstNewline >= 0)
            {
                logBuilder.Remove(0, firstNewline + 1);
                logLines--;
            }
        }

        // Apply to TMP once per batch append
        logText.text = logBuilder.ToString();
    }

    // Finds the first '\n' in the current StringBuilder content
    static int IndexOfNewline(StringBuilder sb)
    {
        for (int i = 0; i < sb.Length; i++)
            if (sb[i] == '\n') return i;
        return -1;
    }
}