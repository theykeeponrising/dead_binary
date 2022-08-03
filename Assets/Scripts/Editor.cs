using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// Used for custom inspector widgets


[CustomEditor(typeof(Tile))]
public class TileCustom : Editor
{
	private Tile script;

	private void OnEnable()
	{
		// Method 1
		script = (Tile) target;
	}

    [ExecuteInEditMode]
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		// Creates a new cover object is none exists for tile already
		if (GUILayout.Button("Create Cover", GUILayout.Width(120)))
		{
			if (!script.cover)
			{
				AssetManager assetManager = GameObject.FindGameObjectWithTag("GlobalManager").GetComponent<AssetManager>();
				script.cover = Instantiate(assetManager.cover.halfWall);
				script.cover.transform.parent = script.transform;
				script.cover.transform.localPosition = new Vector3(0f, 0.425f, -1f);
				script.cover.transform.localScale = new Vector3(2f, 0.85f, 0.5f);
				EditorUtility.SetDirty(script);
			}
		}

		// Rotates existing cover object for tile
		if (GUILayout.Button("Rotate Cover", GUILayout.Width(120)))
		{
			if (script.cover)
			{
				Transform parent = script.GetComponentInParent<Tile>().transform;
				script.transform.RotateAround(parent.position, Vector3.up, 90);
				EditorUtility.SetDirty(script);
			}
		}
		
		// Deletes existing cover object for tile
		if (GUILayout.Button("Remove Cover", GUILayout.Width(120)))
		{
			if (script.cover)
			{
				DestroyImmediate(script.cover.gameObject);
				script.cover = null;
				EditorUtility.SetDirty(script);
			}
		}

		// Draw default inspector after button...
		base.OnInspectorGUI();
	}
}