using System;
using System.Collections.Generic;
using UnityEngine;

/*Class have all the properties required to be saved for the gameObject*/
[Serializable]
public class UIGameObjectData
{
   public string name;
   public List<Type> componentType;
   public Vector3 position;
   public Quaternion rotation;
   public Vector3 scale;
   public Color32 color;
   public Color32 textColor;
   public string tag;
   public string text;
   public TextAnchor textAlignment;
   public string spriteLocation;
   public int rating;
   public string parentName;
   public UITransformValues uiTransformValues;
   public List<UIGameObjectData> children;
}

/*Class have all the properties required for UI Transform*/
[Serializable]
public class UITransformValues
{
   public Vector2 minAnchor;
   public Vector2 maxAnchor;
   public Vector2 sizeDelta;
   public Vector2 offSetMax;
   public Vector2 offSetMin;
   public Vector2 pivot;
}
