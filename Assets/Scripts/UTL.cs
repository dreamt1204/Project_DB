using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UTL
{
	public static void TryCatchError(bool statement, string errorMessage)
	{
		if (statement)
			Debug.LogError(errorMessage);
	}
}
