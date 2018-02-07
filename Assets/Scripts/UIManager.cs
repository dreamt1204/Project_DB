using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject Widget_Connecting;

    GameObject Widget_Joystick_BasicShoot;

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    void Awake()
    {
        // Init variable instances
        Widget_Joystick_BasicShoot = GameObject.Find("Widget_Joystick_BasicShoot");
    }

    void Start()
    {
        // Enable connecting screen
        Widget_Connecting.SetActive(true);
    }

    void OnJoinedRoom()
    {
        // Disable connecting screen
        Widget_Connecting.SetActive(false);
    }

    //---------------------------
    //      Control
    //---------------------------
    public void UpdateBasicAbilityUI(PlayerState playerState)
	{
        bool hasBall = ((playerState == PlayerState.HoldingBall) || (playerState == PlayerState.AimingBall));

        // Update basic shoot joystick
        Widget_Joystick_BasicShoot.GetComponentInChildren<UIJoyStick>().ResetJoystick();
        Widget_Joystick_BasicShoot.SetActive(hasBall);
    }

    //---------------------------
    //      Get Functions
    //---------------------------
	public static UIJoyStick GetJoystick(string widgetName)
	{
        GameObject obj = GameObject.Find(widgetName);
        UTL.TryCatchError(obj == null, "Cannot find object with widgetName: '" + widgetName + "'.");

        UIJoyStick joystick = obj.GetComponentInChildren<UIJoyStick>();
		UTL.TryCatchError(joystick == null, "GameObject '" + widgetName + "' doesn't have joystick componenet attached.");

		return joystick;
	}
}
