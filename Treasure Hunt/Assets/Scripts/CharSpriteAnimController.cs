using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HOW TO USE:
/// 1. Create an Empty game object and add all necessary character controllers
/// 2. Add a Sprite named "Sprite" as a child of the empty game object
/// 3. Add an Animator component to the Sprite
/// 4. Place this in the parent character object
/// This class handles the animations of character sprites
/// </summary>
public class CharSpriteAnimController : MonoBehaviour
{
    private Dictionary<string, RuntimeAnimatorController> animations;
    private Animator currAnimator;
    public Animator AnimController
    {
        get { return currAnimator; }
    }
    // Start is called before the first frame update
    void Start()
    {
        animations = new Dictionary<string, RuntimeAnimatorController>();
        currAnimator = gameObject.transform.Find("Sprite").GetComponent<Animator>();
    }

    /// <summary>
    /// Add an animator controller to the character using this method
    /// </summary>
    /// <param name="ctrlName">A string to be used as a key</param>
    /// <param name="controller">A RuntimeAnimatorController used to animate the sprites</param>
    /// <returns>true if successful, false if not</returns>
    public bool AddAnimController(string ctrlName, RuntimeAnimatorController controller)
    {
        if (!animations.ContainsKey(ctrlName))
        {
            animations.Add(ctrlName, controller);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove an animation controller with this method
    /// </summary>
    /// <param name="ctrlName">The key of the animation controller</param>
    /// <returns>true if successful, false if not</returns>
    public bool RemoveAnimController(string ctrlName)
    {
        if (animations.ContainsKey(ctrlName))
        {
            animations.Remove(ctrlName);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Changes the current animation controller to another one, if it exists
    /// </summary>
    /// <param name="ctrlName">The name of the controller that's stored in this class</param>
    /// <returns>true if successful, false if not</returns>
    public bool SwitchAnimController(string ctrlName)
    {
        RuntimeAnimatorController ctrl;
        if (animations.TryGetValue(ctrlName, out ctrl))
        {
            currAnimator.runtimeAnimatorController = ctrl;
            return true;
        }
        return false;
    }
}
