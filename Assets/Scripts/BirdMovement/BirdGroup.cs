using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents one group of birds with its own button priority system.
/// Each group owns its own birds and has a default behavior + up to 4 button behaviors.
///
/// Button priority: first-pressed wins. If button 0 is held and button 1 is pressed,
/// button 0 stays active. When button 0 is released, button 1 takes over.
/// All released = default behavior.
/// </summary>
[System.Serializable]
public class BirdGroup
{
    public string groupName;

    // Bird references (populated by orchestrator at spawn time)
    [System.NonSerialized] public List<Transform> birds = new List<Transform>();
    [System.NonSerialized] public List<BirdFlightPhysics> birdPhysics = new List<BirdFlightPhysics>();
    [System.NonSerialized] public List<BirdFlightAnimator> birdAnimators = new List<BirdFlightAnimator>();

    // Button priority queue — ordered by press time, first = active
    private List<int> heldButtons = new List<int>();

    /// <summary>
    /// Currently active button (0-3), or -1 if no buttons held (= default behavior).
    /// </summary>
    public int ActiveButton => heldButtons.Count > 0 ? heldButtons[0] : -1;

    /// <summary>
    /// Called when a button in this group is pressed.
    /// Returns true if the active behavior changed.
    /// </summary>
    public bool ButtonPressed(int localButton)
    {
        if (heldButtons.Contains(localButton)) return false;

        int previousActive = ActiveButton;
        heldButtons.Add(localButton);

        bool changed = ActiveButton != previousActive;
        if (changed)
        {
            Debug.Log($"{groupName}: Button {localButton} active");
        }
        return changed;
    }

    /// <summary>
    /// Called when a button in this group is released.
    /// Returns true if the active behavior changed.
    /// </summary>
    public bool ButtonReleased(int localButton)
    {
        int previousActive = ActiveButton;
        heldButtons.Remove(localButton);

        bool changed = ActiveButton != previousActive;
        if (changed)
        {
            if (ActiveButton == -1)
                Debug.Log($"{groupName}: default");
            else
                Debug.Log($"{groupName}: Button {ActiveButton} active (handoff)");
        }
        return changed;
    }

    /// <summary>
    /// Register a spawned bird with this group.
    /// </summary>
    public void RegisterBird(Transform bird)
    {
        birds.Add(bird);
        birdPhysics.Add(bird.GetComponent<BirdFlightPhysics>());
        birdAnimators.Add(bird.GetComponent<BirdFlightAnimator>());
    }
}
