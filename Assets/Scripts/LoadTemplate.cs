using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LoadTemplate
{
    public GameObject mainCanvas;

    /*Below function will create a template using stored json data in hierarchy*/
    public void LoadTemplateInHierarchy(UIGameObjectData uiGameObjectData, TextAsset jsonFile)
    {
        if (jsonFile == null) return;

        mainCanvas = new GameObject(uiGameObjectData.name)
        {
            layer = 5
        };

        foreach (var componentType in uiGameObjectData.componentType)
        {
            AddComponentToGameObject(mainCanvas, componentType, uiGameObjectData);
        }

        mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        mainCanvas.GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mainCanvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        mainCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        mainCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
        
        AddChildGameObject(uiGameObjectData);

        SetParent(uiGameObjectData);

    }

    /*This will assign gameObject a proper parent which it belongs to according to data available*/
    private void SetParent(UIGameObjectData uiGameObjectData)
    {
        foreach (var child in uiGameObjectData.children)
        {
            GameObject.Find(child.name).transform.SetParent(child.parentName == null ? mainCanvas.transform : GameObject.Find(child.parentName).transform);
           
            SetParent(child);
            SetTemplateObject(GameObject.Find(child.name), child);
        }
    }
    
    /*This will set the stored values to the appropriate components attached to the gameObject*/
    private void SetTemplateObject(GameObject gameObject, UIGameObjectData uiGameObject)
    {
        var transform = gameObject.GetComponent<Transform>();

        transform.localRotation = uiGameObject.rotation;
        transform.localScale = uiGameObject.scale;

        transform.tag = uiGameObject.tag;

        if (transform.GetComponent<Text>() != null)
        {
            transform.GetComponent<Text>().color = uiGameObject.textColor;
            transform.GetComponent<Text>().text = uiGameObject.text;
            transform.GetComponent<Text>().resizeTextForBestFit=true;
            transform.GetComponent<Text>().alignment =uiGameObject.textAlignment;
            if (transform.parent.GetComponent<Button>() != null)
            {
                transform.GetComponent<Text>().color=Color.white;
                transform.GetComponent<Text>().resizeTextMaxSize = 50;
            }

            if (uiGameObject.tag=="BoldText")
            {
                transform.GetComponent<Text>().fontStyle = FontStyle.Bold;
            }
        }

        if (transform.GetComponent<Image>() != null)
        {
            transform.GetComponent<Image>().color = uiGameObject.color;
            if(uiGameObject.spriteLocation!=null)
                transform.GetComponent<Image>().sprite=Resources.Load<Sprite>("Sprites/" + uiGameObject.spriteLocation);
            if (uiGameObject.tag=="Fill")
            {
                transform.GetComponent<Image>().type = Image.Type.Filled;
                transform.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
                transform.GetComponent<Image>().fillAmount = (uiGameObject.rating*2)/10f;
                transform.GetComponent<Image>().preserveAspect=true;
            }

            if (transform.parent.name == "StarRating")
            {
                transform.GetComponent<Image>().preserveAspect=true;
            }
        }
        
        if (transform.GetComponent<RawImage>() != null)
        {
            transform.GetComponent<RawImage>().color = uiGameObject.color;
            if(uiGameObject.spriteLocation!=null)
                transform.GetComponent<RawImage>().texture=Resources.Load<Texture2D>("Sprites/" + uiGameObject.spriteLocation);
        }

        if (transform.GetComponent<Button>() != null)
        {
            transform.GetComponent<Image>().type = Image.Type.Sliced;
        }

       
        
        var rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = uiGameObject.uiTransformValues.minAnchor;
        rectTransform.anchorMax =  uiGameObject.uiTransformValues.maxAnchor;
        rectTransform.pivot =  uiGameObject.uiTransformValues.pivot;
        rectTransform.sizeDelta = uiGameObject.uiTransformValues.sizeDelta;
        rectTransform.offsetMax = uiGameObject.uiTransformValues.offSetMax;
        rectTransform.offsetMin = uiGameObject.uiTransformValues.offSetMin;
        
        
        transform.localPosition = uiGameObject.position;
    }

    /*This function will add gameObject and called recursively*/
    private void AddChildGameObject(UIGameObjectData uiGameObjectData)
    {
        foreach (var child in uiGameObjectData.children)
        {
            var childGameObject = new GameObject(child.name)
            {
                layer = 5
            };
            foreach (var childComponentType in child.componentType)
            {
                AddComponentToGameObject(childGameObject, childComponentType, child);
            }

            if (child.children.Count <= 0) continue;
            AddChildGameObject(child);
        }
    }

    /*This function will add components required to the current gameObject according to the data*/
    private void AddComponentToGameObject(GameObject gameObject, Type type, UIGameObjectData data)
    {
        Type componentType = GetType(type.FullName);

        try
        {
            if (componentType != null)
            {
                gameObject.AddComponent(componentType);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }

        if (componentType != null)
        {
            // Check if the GameObject already has the component
            if (gameObject.GetComponent(componentType) == null)
            {
                gameObject.AddComponent(componentType);
            }
        }
        else
        {
            Debug.LogError("Component type not found: " + type);
        }
    }

    /*This function will return the correct component type from the assembly using the string typeName*/
    public static Type GetType(string TypeName)
    {
        var type = Type.GetType(TypeName);

        if (type != null)
            return type;

        // If the TypeName is a full name, then we can try loading the defining assembly directly
        if (TypeName.Contains("."))
        {
            // Get the name of the assembly (Assumption is that we are using fully-qualified type names)
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

            // Attempt to load the indicated Assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                return null;

            // Ask that assembly to return the proper Type
            type = assembly.GetType(TypeName);
            if (type != null)
                return type;
        }

        // loaded assemblies and see if any of them define the type
        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assemblyName in referencedAssemblies)
        {
            // Load the referenced assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
            {
                // See if that assembly defines the named type
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;
            }
        }

        return null;
    }
}