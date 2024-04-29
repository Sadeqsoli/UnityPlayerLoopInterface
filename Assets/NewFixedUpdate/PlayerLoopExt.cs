using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public static class PlayerLoopExt
{
	delegate void NativeFunction();
	private static NativeFunction m_NativeFixedUpdateFunction;
	private static float m_CallPeriod = 1f; // Default period is 1 second
	private static float m_CurrentTime;

	// Method to set the custom fixed update period
	public static void SetUpdatePeriod(float period)
	{
		m_CallPeriod = Mathf.Max(period, 0.01f); // Ensure period is at least 0.01 seconds
	}

	private static void NewFixedUpdate()
	{
		m_CurrentTime += Time.fixedDeltaTime;
		if (m_CurrentTime > m_CallPeriod)
		{
			m_CurrentTime -= m_CallPeriod;
			Debug.Log("Custom update tick!");
		}

		m_NativeFixedUpdateFunction.Invoke();
	}

	[RuntimeInitializeOnLoadMethod]
	private static void AppStart()
	{
		var defaultSystems = PlayerLoop.GetDefaultPlayerLoop();
		var customUpdate = new PlayerLoopSystem
		{
			updateDelegate = NewFixedUpdate,
			type = typeof(PlayerLoopExt)
		};
		var nativePtr = ReplaceSystem<FixedUpdate.ScriptRunBehaviourFixedUpdate>(ref defaultSystems, customUpdate);

		PlayerLoop.SetPlayerLoop(defaultSystems);

		if (nativePtr != IntPtr.Zero)
		{
			m_NativeFixedUpdateFunction = Marshal.GetDelegateForFunctionPointer<NativeFunction>(nativePtr);
		}

		//if (nativePtr != IntPtr.Zero)
		//{
		//	unsafe
		//	{
		//		nativePtr = new IntPtr(*((Int64*)nativePtr.ToPointer()));
		//	}
		//	m_NativeFixedUpdateFunction = Marshal.GetDelegateForFunctionPointer(nativePtr, typeof(NativeFunction)) as NativeFunction;
		//}
	}

	private static IntPtr ReplaceSystem<T>(ref PlayerLoopSystem system, PlayerLoopSystem replacement)
	{
		if (system.type == typeof(T))
		{
			var nativeUpdatePtr = system.updateFunction;
			system = replacement;
			return nativeUpdatePtr;
		}

		if (system.subSystemList == null)
			return IntPtr.Zero;

		for (var i = 0; i < system.subSystemList.Length; i++)
		{
			var nativeUpdatePtr = ReplaceSystem<T>(ref system.subSystemList[i], replacement);
			if (nativeUpdatePtr == IntPtr.Zero)
				continue;

			return nativeUpdatePtr;
		}

		return IntPtr.Zero;
	}
}
