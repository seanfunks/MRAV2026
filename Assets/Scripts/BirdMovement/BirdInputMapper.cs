using UnityEngine;

/// <summary>
/// Maps a 4x4 input grid (keyboard + OSC) to 16 action IDs (0-15).
/// Keyboard layout:
///   Row 0: 1 2 3 4    → IDs 0-3
///   Row 1: Q W E R    → IDs 4-7
///   Row 2: A S D F    → IDs 8-11
///   Row 3: Z X C V    → IDs 12-15
///
/// Fires OnActionEvent(actionId, pressed) for consumers like BirdFlightController.
/// Also registers OSC handlers on /bird/R1C1../bird/R4C4 and legacy /T1../B4 addresses.
///
/// Setup: Attach to the same GameObject as BirdFlightController.
/// Assign the OSC reference in Inspector (optional — keyboard works without it).
/// </summary>
public class BirdInputMapper : MonoBehaviour
{
    public OSC osc;

    /// <summary>
    /// Event fired on action press/release. Args: (actionId 0-15, isPressed).
    /// </summary>
    public System.Action<int, bool> OnActionEvent;

    public const int ActionCount = 16;

    // 4x4 keyboard grid
    private readonly KeyCode[,] keyGrid = new KeyCode[4, 4]
    {
        { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 },
        { KeyCode.Q,      KeyCode.W,      KeyCode.E,      KeyCode.R      },
        { KeyCode.A,      KeyCode.S,      KeyCode.D,      KeyCode.F      },
        { KeyCode.Z,      KeyCode.X,      KeyCode.C,      KeyCode.V      },
    };

    private bool[] actionStates = new bool[ActionCount];

    void Start()
    {
        if (osc != null)
        {
            RegisterOSCHandlers();
        }
    }

    void Update()
    {
        // Poll keyboard
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int id = row * 4 + col;
                KeyCode key = keyGrid[row, col];

                if (Input.GetKeyDown(key))
                {
                    actionStates[id] = true;
                    OnActionEvent?.Invoke(id, true);
                }
                if (Input.GetKeyUp(key))
                {
                    actionStates[id] = false;
                    OnActionEvent?.Invoke(id, false);
                }
            }
        }
    }

    private void RegisterOSCHandlers()
    {
        // Primary: /bird/R1C1 through /bird/R4C4
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int capturedId = row * 4 + col;
                string address = $"/bird/R{row + 1}C{col + 1}";
                osc.SetAddressHandler(address, (msg) => HandleOSC(capturedId, msg));
            }
        }

        // Legacy compatibility: existing MRAVctrl-style addresses
        string[][] legacy = new string[][]
        {
            new[] { "/T1",  "/T2",  "/T3",  "/T4"  },
            new[] { "/TM1", "/TM2", "/TM3", "/TM4" },
            new[] { "/BM1", "/BM2", "/BM3", "/BM4" },
            new[] { "/B1",  "/B2",  "/B3",  "/B4"  },
        };

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int capturedId = row * 4 + col;
                osc.SetAddressHandler(legacy[row][col], (msg) => HandleOSC(capturedId, msg));
            }
        }
    }

    private void HandleOSC(int actionId, OscMessage msg)
    {
        // OSC from a pad: value > 0 = pressed, value == 0 = released
        float value = msg.values.Count > 0 ? msg.GetFloat(0) : 1f;
        bool pressed = value > 0.5f;
        actionStates[actionId] = pressed;
        OnActionEvent?.Invoke(actionId, pressed);
    }

    /// <summary>
    /// Query whether an action is currently held.
    /// </summary>
    public bool IsActionHeld(int actionId)
    {
        return actionId >= 0 && actionId < ActionCount && actionStates[actionId];
    }

    /// <summary>
    /// Gets the row/column label for an action ID (for debug display).
    /// </summary>
    public static string GetActionLabel(int actionId)
    {
        int row = actionId / 4;
        int col = actionId % 4;
        string[] rowLabels = { "1234", "QWER", "ASDF", "ZXCV" };
        return $"R{row + 1}C{col + 1} ({rowLabels[row][col]})";
    }
}
