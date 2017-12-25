using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public GameObject Joystick_BasicShoot;
	public GameObject Joystick_BasicCatch;

	public void UpdateBasicAbilityJoytick(bool hasBall)
	{
		EnableJoystickWidget(Joystick_BasicShoot, hasBall);
		EnableJoystickWidget(Joystick_BasicCatch, !hasBall);
	}

	void EnableJoystickWidget(GameObject obj, bool enabled)
	{
		obj.SetActive(enabled);
		obj.transform.Find("Joystick").GetComponentInChildren<UIJoyStick_Ability_Direction>().ResetJoystick();
	}

	public static GameObject GetJoystickObject(string widgetName)
	{
		GameObject obj = GameObject.Find(widgetName);
		UTL.TryCatchError(obj == null, "Widget '" + widgetName + "' doesn't exist.");
		return obj;
	}

	public static UIJoyStick_Ability_Direction GetJoystick(string widgetName)
	{
        UIJoyStick_Ability_Direction joystick = GetJoystickObject(widgetName).GetComponentInChildren<UIJoyStick_Ability_Direction>();
		UTL.TryCatchError(joystick == null, "Widget '" + widgetName + "' doesn't have joystick componenet.");
		return joystick;
	}
}
