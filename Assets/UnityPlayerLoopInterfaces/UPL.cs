using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_2020_1_OR_NEWER
using UnityEngine.LowLevel;
#else
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;
#endif

//this code was originally from https://github.com/Baste-RainGames/PlayerLoopInterface
//Comments and tags are preserved as in the original code.
//The logic is optimized and simplified where possible, while preserving functionality and readability.
/// <summary>
/// Unity exposes the PlayerLoop to allow you to insert your own "systems" to be run in similar ways to eg. Update or FixedUpate.
/// The interface for that is a bit hairy, and there are bugs that needs workarounds, so this is a nice interface for interacting with that system.
///
/// In essence, use UPL.InsertSystemBefore/After to have a callback be executed every frame, before or after some built-in system.
/// The built-in systems live in UnityEngine.Experimental.PlayerLoop, so if you want to insert a system to run just before Update, call:
///
/// UPL.InsertSystemBefore(typeof(MyType), MyMethod, typeof(UnityEngine.PlayerLoop.Update);
///
/// If you want to run in the fixed timestep (FixedUpdate), you have to insert the system as a subsystem of UnityEngine.PlayerLoop.FixedUpdate. For example, use
/// UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate:
///
/// UPL.InsertSystemBefore(typeof(MyType), MyMethod, typeof(UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate);
/// </summary>
public static class UPL
{
	private static readonly List<PlayerLoopSystem> insertedSystems = new List<PlayerLoopSystem>();

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		// Cleanup inserted systems on application quit
		Application.quitting += ClearInsertedSystems;
	}

	private static void ClearInsertedSystems()
	{
		// Remove all inserted systems
		foreach (var playerLoopSystem in insertedSystems)
			TryRemoveSystem(playerLoopSystem.type);

		// Clear the list of inserted systems
		insertedSystems.Clear();

		// Unsubscribe from application quit event
		Application.quitting -= ClearInsertedSystems;
	}

	public static void InsertSystemAfter(Type newSystemMarker, PlayerLoopSystem.UpdateFunction newSystemUpdate, Type insertAfter)
	{
		// Create a new player loop system
		var playerLoopSystem = new PlayerLoopSystem { type = newSystemMarker, updateDelegate = newSystemUpdate };
		// Insert the new system after the specified target
		InsertSystem(playerLoopSystem, insertAfter, InsertType.After);
	}

	public static void InsertSystemBefore(Type newSystemMarker, PlayerLoopSystem.UpdateFunction newSystemUpdate, Type insertBefore)
	{
		// Create a new player loop system
		var playerLoopSystem = new PlayerLoopSystem { type = newSystemMarker, updateDelegate = newSystemUpdate };
		// Insert the new system before the specified target
		InsertSystem(playerLoopSystem, insertBefore, InsertType.Before);
	}

	private static void InsertSystem(PlayerLoopSystem toInsert, Type insertTarget, InsertType insertType)
	{
		// Get the current player loop
		var rootSystem = PlayerLoop.GetCurrentPlayerLoop();
		// Attempt to insert the system
		if (InsertSystem(ref rootSystem, toInsert, insertTarget, insertType))
		{
			// Add the inserted system to the list
			insertedSystems.Add(toInsert);
			// Set the modified player loop
			PlayerLoop.SetPlayerLoop(rootSystem);
		}
	}

	private static bool InsertSystem(ref PlayerLoopSystem currentLoopRecursive, PlayerLoopSystem toInsert, Type insertTarget, InsertType insertType)
	{
		// Check if the current loop has sub-systems
		if (currentLoopRecursive.subSystemList == null)
			return false;

		// Iterate through the sub-systems
		for (int i = 0; i < currentLoopRecursive.subSystemList.Length; i++)
		{
			var subSystem = currentLoopRecursive.subSystemList[i];

			// Check if the current sub-system matches the target
			if (subSystem.type == insertTarget)
			{
				// Prepare a new list with space for the inserted system
				var newSubSystems = new PlayerLoopSystem[currentLoopRecursive.subSystemList.Length + 1];
				int insertIndex = insertType == InsertType.Before ? i : i + 1;

				// Copy existing systems before and after the insertion point
				Array.Copy(currentLoopRecursive.subSystemList, 0, newSubSystems, 0, insertIndex);
				newSubSystems[insertIndex] = toInsert;
				Array.Copy(currentLoopRecursive.subSystemList, insertIndex, newSubSystems, insertIndex + 1, currentLoopRecursive.subSystemList.Length - insertIndex);

				// Update the sub-system list
				currentLoopRecursive.subSystemList = newSubSystems;
				return true;
			}

			// Recursively insert into sub-systems
			if (InsertSystem(ref subSystem, toInsert, insertTarget, insertType))
			{
				currentLoopRecursive.subSystemList[i] = subSystem;
				return true;
			}
		}

		return false;
	}

	public static bool TryRemoveSystem(Type type)
	{
		// Get the current player loop
		var currentSystem = PlayerLoop.GetCurrentPlayerLoop();
		// Try to remove the specified system type
		bool couldRemove = TryRemoveTypeFrom(ref currentSystem, type);
		// If removed, set the modified player loop
		if (couldRemove)
			PlayerLoop.SetPlayerLoop(currentSystem);
		return couldRemove;
	}

	private static bool TryRemoveTypeFrom(ref PlayerLoopSystem currentSystem, Type type)
	{
		// Check if the current system has sub-systems
		if (currentSystem.subSystemList == null)
			return false;

		// Iterate through the sub-systems
		for (int i = 0; i < currentSystem.subSystemList.Length; i++)
		{
			// Check if the current sub-system matches the target type
			if (currentSystem.subSystemList[i].type == type)
			{
				// Remove the system from the list
				var newSubSystems = new List<PlayerLoopSystem>(currentSystem.subSystemList);
				newSubSystems.RemoveAt(i);
				currentSystem.subSystemList = newSubSystems.ToArray();
				return true;
			}

			// Recursively remove from sub-systems
			if (TryRemoveTypeFrom(ref currentSystem.subSystemList[i], type))
				return true;
		}

		return false;
	}

	private enum InsertType
	{
		Before,
		After
	}
}
