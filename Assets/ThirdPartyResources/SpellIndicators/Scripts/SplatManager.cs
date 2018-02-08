using UnityEngine;
using System.Linq;

namespace Werewolf.SpellIndicators {
  /// <summary>
  /// Apply this to the GameObject which holds all your Splats. Make sure the origin is correctly centered at the base of the Character.
  /// </summary>
  public class SplatManager : MonoBehaviour {

    /// <summary>
    /// Special indicator for displaying range, unselectable
    /// </summary>
    public Splat rangeIndicator;

    /// <summary>
    /// All splat types go here, any changes should be mirrored in the initializer below.
    /// </summary>
    public Splat Direction, Cone, Point;

    /// <summary>
    /// Determines whether the cursor should be hidden when a Splat is showing.
    /// </summary>
    public bool HideCursor;

    /// <summary>
    /// Returns all Splats belonging to the Manager.
    /// </summary>
    public Splat[] Splats        { get; set; }

    /// <summary>
    /// Returns the currently selected Splat.
    /// </summary>
    public Splat   CurrentSplat  { get; set; }

    void Start() {
      // Create a list of all the splats available to the manager, make sure that the splat types above are mirrored here
      Splats = new Splat[] { Direction, Cone, Point };

      // Make sure each Splat has a reference to its Manager
      Splats.ToList().ForEach(x => x.Manager = this);
    }

    // This Update method and the "HideCursor" variable can be deleted if you do not need this functionality
    void Update() {
      if (HideCursor) {
        if (CurrentSplat != null && CurrentSplat.IsVisible)
          Cursor.visible = false;
        else
          Cursor.visible = true;
      }
    }

    /// <summary>
    /// Hide the current splat
    /// </summary>
    public void Cancel() {
      if (CurrentSplat != null) {
        CurrentSplat.SetVisible(false);
        rangeIndicator.SetVisible(false);
      }
    }
  }
}