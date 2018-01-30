﻿using System.Collections; using System.Collections.Generic; using UnityEngine;  public class BasicCatch : Ability_Direction { 	//=========================== 	//      Functions 	//=========================== 	//--------------------------- 	//      Init Functions 	//--------------------------- 	public override void InitJoystick() 	{ 		type = AbilityType.Catch;  		joystick = UIManager.GetJoystick("Widget_Joystick_BasicCatch"); 		joystick.ability = this; 	}  	//--------------------------- 	//      Update 	//--------------------------- 	void Update() 	{ 		UpdateAiming(); 		CheckCatchCollision(); 	}  	void CheckCatchCollision() 	{ 		 	}  	//--------------------------- 	//      Ability action 	//--------------------------- 	public override void ShowAbility(bool show) 	{ 		if (show) 			UpdateCatchRange();  		base.ShowAbility(show); 	}  	void UpdateCatchRange() 	{ 		Projector rangeProjector = RangeIndicator.GetComponentInChildren<Projector>(); 		//rangeProjector.aspectRatio = ownerCharacter.dexterity; 	}  	public void BallCatchAction(Collision col) 	{ 		Vector3 hitPos = col.transform.position;  		Vector3 hitVec = (ownerCharacter.transform.position).normalized;  		float angle = Vector3.Angle(AimingVector, hitVec); 		Debug.Log("--" + angle); 	} }  