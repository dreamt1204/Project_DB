using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Screen")]
    public GameObject Widget_Connecting;

    [Header("Control")]
    public GameObject Joystick_BasicShoot;

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
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
    // ??
    public void UpdateBasicAbilityUI(bool hasBall)
	{
        /*
        EnableJoystickWidget(Joystick_BasicShoot, hasBall);
		EnableJoystickWidget(Joystick_BasicCatch, !hasBall);
        */
	}

    // ??
	void EnableJoystickWidget(GameObject obj, bool enabled)
	{
		obj.SetActive(enabled);
		obj.transform.Find("Joystick").GetComponentInChildren<UIJoyStick>().ResetJoystick();
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
