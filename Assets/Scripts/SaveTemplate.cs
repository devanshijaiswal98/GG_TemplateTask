using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class SaveTemplate
{
    public GameObject rootGameObject;
    int starRatingValue;
    /* Function will create a json file and save it on the given location, initial file needs to be created using the default 'Canvas' prefab in the hierarchy */
    public void InitialJsonCreation()
    {
        if (rootGameObject == null)
        {
            Debug.LogError("Root GameObject not assigned");
            return;
        }
        
        if (rootGameObject != null)
        {
            var hierarchyData = CollectHierarchyData(rootGameObject);
            
            // Convert the hierarchy data to JSON
            var json = JsonConvert.SerializeObject(hierarchyData, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = {new Vector3Converter()}
            });


            // Save JSON to a file
            var filePath = Application.dataPath + "/Resources/JSON/uiTemplateData.json";
            File.WriteAllText(filePath, json);

            Debug.Log("Hierarchy saved to: " + filePath);
        }
        else
        {
            Debug.LogError("Root GameObject not found.");
        } 
    }
    /*Collect the required data from the gameObject and add it to UIGameObjectData */
     private UIGameObjectData CollectHierarchyData(GameObject gameObject)
    {
        var compColor = new Color32();
        var textColorValue = new Color32();
    
        string textValue = null, spriteLocationValue=null;

        TextAnchor align = TextAnchor.MiddleCenter;
        
        var components = gameObject.GetComponents<Component>();
        
        var componentList = new List<Type>();
        // Loop through the components and print their types
        foreach (var component in components)
        {
            var fName = component.GetType();
            componentList.Add(fName);
            starRatingValue = 0;
            switch (fName.FullName)
            {
                case "UnityEngine.UI.RawImage":
                {
                    compColor = component.GetComponent<RawImage>().color;
                    if(component.GetComponent<RawImage>().texture != null)
                        spriteLocationValue = component.GetComponent<RawImage>().texture.name;
                    break;
                }
                case "UnityEngine.UI.Image":
                {
                    compColor = component.GetComponent<Image>().color;
                    if(component.GetComponent<Image>().sprite != null)
                        spriteLocationValue = component.GetComponent<Image>().sprite.name;
                    if(component.GetComponent<Image>().type==Image.Type.Filled)
                        starRatingValue = (int) (component.GetComponent<Image>().fillAmount * 10) / 2;
                    break;
                }
                case "UnityEngine.UI.Text":
                    textValue = component.GetComponent<Text>().text;
                    textColorValue = component.GetComponent<Text>().color;
                    align = component.GetComponent<Text>().alignment;
                    break;
                case  "UnityEngine.UI.Button":
                    compColor = component.GetComponent<Image>().color;
                    if(component.GetComponent<Image>().sprite != null)
                        spriteLocationValue = component.GetComponent<Image>().sprite.name;
                    break;
            }
        }

        UITransformValues values = new UITransformValues()
        {
            minAnchor = gameObject.GetComponent<RectTransform>().anchorMin,
            maxAnchor = gameObject.GetComponent<RectTransform>().anchorMax,
            sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta,
            offSetMax = gameObject.GetComponent<RectTransform>().offsetMax,
            offSetMin = gameObject.GetComponent<RectTransform>().offsetMin,
            pivot = gameObject.GetComponent<RectTransform>().pivot
        };
        
        var data = new UIGameObjectData()
        {
            name = gameObject.name,
            componentType = componentList,
            position = gameObject.transform.localPosition,
            rotation = gameObject.transform.localRotation,
            scale = gameObject.transform.localScale,
            color = compColor,
            textColor = textColorValue,
            tag = gameObject.tag,
            text = textValue,
            textAlignment = align,
            rating = starRatingValue,
            spriteLocation = spriteLocationValue,
            uiTransformValues = values,
            children = new List<UIGameObjectData>()
        };

        // Recursively collect data for each child
        foreach (Transform childTransform in gameObject.transform)
        {
            var childGameObject = childTransform.gameObject;
            var childData = CollectHierarchyData(childGameObject);
            childData.parentName = gameObject.name;
            data.children.Add(childData);
        }
        
        return data;
    }
    // Custom JsonConverter to handle Vector3 normalization
    private class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new {x = value.x, y = value.y, z = value.z});
        }

        public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }
    }
}