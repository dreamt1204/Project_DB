using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCatch : Ability_Direction
{
	public override void InitJoystick()
	{
		joystick = UIManager.GetJoystick("Widget_Joystick_BasicCatch");
		joystick.ability = this;
	}
}
