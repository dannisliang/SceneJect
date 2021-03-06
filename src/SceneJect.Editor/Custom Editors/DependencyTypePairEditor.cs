﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SceneJect.CustomEditors
{
	[CustomPropertyDrawer(typeof(DependencyTypePair))]
	public class DependencyTypePairEditor : PropertyDrawer
	{
		private Dictionary<Type, IEnumerable<Type>> cachedTypeDictionary = new Dictionary<Type, IEnumerable<Type>>();
		private Dictionary<Type, IEnumerable<string>> cachedAssemblyQualifiedNames = new Dictionary<Type, IEnumerable<string>>();
		private Dictionary<Type, IEnumerable<string>> cachedShortTypeNames = new Dictionary<Type, IEnumerable<string>>();

		public override float GetPropertyHeight(SerializedProperty property, UnityEngine.GUIContent label)
		{
			return base.GetPropertyHeight(property, label) + 50;
		}

		//TODO: Refactor
		public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, UnityEngine.GUIContent label)
		{
			property.FindPropertyRelative("_Behaviour").objectReferenceValue = EditorGUI.ObjectField(new Rect(position.left, position.top, position.width, 15), "Dependency:", property.FindPropertyRelative("_Behaviour").objectReferenceValue, 
				typeof(MonoBehaviour), true) as MonoBehaviour;

			MonoBehaviour behaviour = property.FindPropertyRelative("_Behaviour").objectReferenceValue as MonoBehaviour;

			if (behaviour == null)
				return;

			if (GUI.Button(new Rect(position.left, position.top + 20, position.width, 25), "Lookup Types"))
			{
				if (cachedTypeDictionary.ContainsKey(behaviour.GetType()))
					RemoveFromCache(behaviour.GetType());

				cachedAssemblyQualifiedNames[behaviour.GetType()] = FindTypes(behaviour).Select(x => x.AssemblyQualifiedName);

				property.FindPropertyRelative("_ImplementedTypes")
					.WriteSerializedArray(cachedAssemblyQualifiedNames[behaviour.GetType()].ToArray());
			}

			if (!cachedTypeDictionary.ContainsKey(behaviour.GetType()))
			{
				//Let's just draw a label telling the user what type was last selected.
				string val = property.FindPropertyRelative("_SelectedType").stringValue;
				//WARNING: If you modify this rect modify the one about the popuplist.
				GUI.Label(new Rect(position.left, position.top + 50, position.width, 20), String.Concat("Current Type: ", val == null ? "[NONE]" : val.Split(',')[0]));	
				return;
			}

			if(!cachedShortTypeNames.ContainsKey(behaviour.GetType()))
				cachedShortTypeNames[behaviour.GetType()] = 
					cachedAssemblyQualifiedNames[behaviour.GetType()].Select(x => x.Split(',')[0]).ToArray();

			SerializedProperty indexProp = property.FindPropertyRelative("_SelectedPopupIndex");

			int index = indexProp.intValue;
			string selectedTypeString = property.FindPropertyRelative("_SelectedType").stringValue;

			//This can occur if the class declaration changes in such a way that the total amount of inherited or implemented types
			//are less than the last selected index of the Type selection.
			//We prevent this by trying to find the old Type in the collection or just moving down one if it was removed.
			if (index >= cachedShortTypeNames[behaviour.GetType()].Count())
			{
				index = -1;

				for(int i = 0; i < cachedAssemblyQualifiedNames[behaviour.GetType()].Count(); i++)
				{
					if(selectedTypeString == cachedAssemblyQualifiedNames[behaviour.GetType()].ElementAt(i))
					{
						index = i;
						break;
					}
				}

				if (index == -1)
					index = cachedShortTypeNames[behaviour.GetType()].Count() - 1;
			}

			//This preverses the selected Type if for some reason the collection was reordered or new Types were added to the declaration.
			string[] cachedFullTypeNamesForBehaviour = cachedAssemblyQualifiedNames[behaviour.GetType()].ToArray();

			
			//we can be sure index is in range.
			if (selectedTypeString != cachedFullTypeNamesForBehaviour[index]) 
			{
				for(int i = 0; i < cachedFullTypeNamesForBehaviour.Count(); i++)
				{
					if (cachedFullTypeNamesForBehaviour[i] == selectedTypeString)
					{
						index = i;
						break;
					}
				}
			}

			//For efficiency reasons let's make some assumptions that these IEnumerables don't get reordered.
			//Though it's possible for some reason they aren't. It's a very flimsy way to deal with this but it's better than
			//a super slow editor.
			//WARNING: If you modify this rect modify the one about the Type label.

			index = EditorGUI.Popup(new Rect(position.left, position.top + 50, position.width, 20), index, cachedShortTypeNames[behaviour.GetType()].ToArray());
			indexProp.intValue = index;

			property.FindPropertyRelative("_SelectedType").stringValue = cachedFullTypeNamesForBehaviour[index];
		}

		//This is old code that used to read the array.
		//Don't delete it so it can be referenced if we need to again in the future.
		//It's quite convoluted but some code that this was based on can be found here http://answers.unity3d.com/questions/682932/using-generic-list-with-serializedproperty-inspect.html
		/*SerializedProperty arrayProp = property.FindPropertyRelative("_ImplementedTypes");

		if (!arrayProp.isArray)
		{
			Debug.LogError("_ImplementedTypes on " + typeof(DependencyTypePair).ToString() + " is not an array contained within the SerializedProperty.");
			return;
		}

		arrayProp.Next(true);//type
		arrayProp.Next(true);//arraysize

		int arraySize = arrayProp.intValue;

		if (arraySize == 0) //The size should never be zero technically.
			return;

		cachedTypeStrings = new string[arraySize];

		arrayProp.Next(true);//moves to first val

		for (int i = 0; i < arraySize; i++)
		{
			//Debug.Log("Index: " + i + " found: " + arrayProp.stringValue);
			cachedTypeStrings[i] = arrayProp.stringValue;

			if (i < arraySize - 1)
				arrayProp.Next(false);
		}*/

		private IEnumerable<Type> FindTypes(MonoBehaviour b)
		{
			Type t = b.GetType();

			if (cachedTypeDictionary.ContainsKey(t))
				return cachedTypeDictionary[t];

			IEnumerable<Type> interfaces = t.GetInterfaces();

			List<Type> types = new List<Type>(interfaces.Count() + 1);

			types.AddRange(interfaces);

			Type baseType = t.BaseType;
			if (baseType != null)
				types.Add(t.BaseType);

			cachedTypeDictionary[t] = types;

			return FindTypes(b);
		}

		private void RemoveFromCache(Type t)
		{
			cachedTypeDictionary.Remove(t);
			cachedAssemblyQualifiedNames.Remove(t);
			cachedShortTypeNames.Remove(t);
		}
	}
}
