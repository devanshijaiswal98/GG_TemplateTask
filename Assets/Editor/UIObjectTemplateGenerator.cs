using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Serialization;

namespace Editor
{
    public class UIObjectTemplateGenerator : EditorWindow
    {
        private TextAsset jsonFile;

        private GameObject rootGameObject;
        
        public Sprite sprite;
        Vector2 scrollPos;

        private UIGameObjectData uiGameObjectData;


        private Color textColor = Color.black, imageColor = Color.white;
        private Vector3 sampleFormatPosition, sampleFormatRotation, sampleFormatScale;
        private Color sampleFormatImageColor = Color.white;

        private Vector3 adChoiceIconPosition, adChoiceIconRotation, adChoiceIconScale;
        private Color adChoiceIconImageColor = Color.white;

        private Vector3 adTagPosition, adTagRotation, adTagScale;
        private Color adTagTextColor = Color.black;
        private string adTagText = "AD";

        private Vector3 appIconPosition, appIconRotation, appIconScale;
        private Color appIconImageColor = Color.white;
        private Sprite appIconSprite;

        private Vector3 adHeadlineTextPosition, adHeadlineTextRotation, adHeadlineTextScale;
        private Color adHeadlineTextColor = Color.black;
        private string adHeadlineText = "Test Ad : Flood - It!";

        private Vector3 starRatingPosition, starRatingRotation, starRatingScale;
        private int starRatingValue;

        private Vector3 appPriceTextPosition, appPriceTextRotation, appPriceTextScale;
        private Color appPriceTextColor = Color.black;
        private string appPriceText = "FREE";

        private Vector3 appBodyTextPosition, appBodyTextRotation, appBodyTextScale;
        private Color appBodyTextColor = Color.black;
        private string appBodyText = "Install Flood-It App for free! Free Popular \n Casual Game";

        private Vector3 appInstallBtnPosition, appInstallBtnRotation, appInstallBtnScale;
        private Color appInstallBtnColor = new Color(47, 203, 179, 255);
        private string appInstallBtnText = "INSTALL";

        private SaveTemplate saveTemplate;

        [MenuItem("Tools/UI Template Generator")]
        public static void ShowWindow()
        {
            GetWindow<UIObjectTemplateGenerator>("UI Template Generator");
        }

        private void OnGUI()
        {
            ToolKitEditorWindow();
        }

        /*Have all the Editor GUI creation and assignments*/
        private void ToolKitEditorWindow()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            GUILayout.Label("UI Template Generator", EditorStyles.boldLabel);
            jsonFile = EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false) as TextAsset;

            GUILayout.Space(10);
            rootGameObject = EditorGUILayout.ObjectField("Root GameObject Reference", rootGameObject, typeof(GameObject), true) as GameObject;

            GUILayout.Space(10);
            GUILayout.Label("Sample Format Details", EditorStyles.boldLabel);
            SetGUIComponentWise(ref sampleFormatPosition, ref sampleFormatRotation, ref sampleFormatScale, ref sampleFormatImageColor, ref textColor, false, true);

            GUILayout.Space(20);
            GUILayout.Label("RawImage_adChoicesIcon Details", EditorStyles.boldLabel);
            SetGUIComponentWise(ref adChoiceIconPosition, ref adChoiceIconRotation, ref adChoiceIconScale, ref adChoiceIconImageColor, ref textColor, false, true);

            GUILayout.Space(20);
            GUILayout.Label("adTag Details", EditorStyles.boldLabel);
            adTagText = EditorGUILayout.TextField("Text: ", adTagText);
            SetGUIComponentWise(ref adTagPosition, ref adTagRotation, ref adTagScale, ref imageColor, ref adTagTextColor, true);

            GUILayout.Space(20);
            GUILayout.Label("AppIconHolder Details", EditorStyles.boldLabel);
            SetGUIComponentWise(ref appIconPosition, ref appIconRotation, ref appIconScale, ref appIconImageColor, ref textColor, false, false);

            GUILayout.Space(20);
            GUILayout.Label("RawImage_adIcon Details", EditorStyles.boldLabel);
            appIconSprite = EditorGUILayout.ObjectField("App Icon Sprite: ", appIconSprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite;
            appIconImageColor = EditorGUILayout.ColorField("Image Color: ", appIconImageColor);

            GUILayout.Space(20);
            GUILayout.Label("App text_adHeadline Details", EditorStyles.boldLabel);
            adHeadlineText = EditorGUILayout.TextField("Text: ", adHeadlineText);
            SetGUIComponentWise(ref adHeadlineTextPosition, ref adHeadlineTextRotation, ref adHeadlineTextScale, ref imageColor, ref adHeadlineTextColor, true);

            GUILayout.Space(20);
            GUILayout.Label("Star Rating Details", EditorStyles.boldLabel);
            SetGUIComponentWise(ref starRatingPosition, ref starRatingRotation, ref starRatingScale, ref imageColor, ref textColor);
            starRatingValue = EditorGUILayout.IntSlider("Star Rating: ", starRatingValue, 1, 5);

            GUILayout.Space(20);
            GUILayout.Label("App price Details", EditorStyles.boldLabel);
            appPriceText = EditorGUILayout.TextField("Text: ", appPriceText);
            SetGUIComponentWise(ref appPriceTextPosition, ref appPriceTextRotation, ref appPriceTextScale, ref imageColor, ref appPriceTextColor, true);

            GUILayout.Space(20);
            GUILayout.Label("App Text_body Details", EditorStyles.boldLabel);
            appBodyText = EditorGUILayout.TextField("Text: ", appBodyText);
            SetGUIComponentWise(ref appBodyTextPosition, ref appBodyTextRotation, ref appBodyTextScale, ref imageColor, ref appBodyTextColor, true);

            GUILayout.Space(20);
            GUILayout.Label("App Install Button_Cta Details");
            appInstallBtnText = EditorGUILayout.TextField("Install Button Text: ", appInstallBtnText);
            SetGUIComponentWise(ref appInstallBtnPosition, ref appInstallBtnRotation, ref appInstallBtnScale, ref appInstallBtnColor, ref textColor, false, true);

            GUILayout.Space(20);
            
            if (GUILayout.Button("Create Initial JSON File", GUILayout.Height(50)))
            {
                saveTemplate.rootGameObject = rootGameObject;
                saveTemplate.InitialJsonCreation();
            }
            
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate/Load Templates", GUILayout.Height(50)))
            {
                AssetDatabase.Refresh();
                
                ReadFromFile();
                if(rootGameObject != null)
                    Undo.DestroyObjectImmediate(rootGameObject);
                
                var loadTemplate = new LoadTemplate();
                loadTemplate.LoadTemplateInHierarchy(uiGameObjectData, jsonFile);
                rootGameObject =loadTemplate.mainCanvas;
                
                Debug.Log("Generate Canvas Successfully!!!");
            }

            if (GUILayout.Button("View Templates Values", GUILayout.Height(50)))
            {
                AssetDatabase.Refresh();
                ViewTemplateValues();
            }

            if (GUILayout.Button("Update Template Values", GUILayout.Height(50)))
            {
                AssetDatabase.Refresh();
                UpdateTemplateValues();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        /*Function is used to set the GUI to the referenced variable*/
        private static void SetGUIComponentWise(ref Vector3 position, ref Vector3 rotation, ref Vector3 scale, ref Color imageColor, ref Color textColor, bool isTextColorNeeded = false, bool isImageColorNeeded = false)
        {
            position = EditorGUILayout.Vector3Field("Position: ", position);
            rotation = EditorGUILayout.Vector3Field("Rotation: ", rotation);
            scale = EditorGUILayout.Vector3Field("Scale: ", scale);

            if (isImageColorNeeded)
            {
                imageColor = EditorGUILayout.ColorField("Image Color: ", imageColor);
            }

            if (isTextColorNeeded)
            {
                textColor = EditorGUILayout.ColorField("Text Color: ", textColor);
            }
        }
        
        /*This will get all the required values from the json file and show it in the custom UIToolKit editor*/
        private void ViewTemplateValues()
        {
            ReadFromFile();

            if (jsonFile == null) return;

            if (uiGameObjectData.children.Count <= 0) return;
            foreach (var child in uiGameObjectData.children.Where(child => child.name == "SampleFormat" && child.children.Count > 0))
            {
                sampleFormatPosition = child.position;
                sampleFormatRotation = new Vector3(child.rotation.x, child.rotation.y, child.rotation.z);
                sampleFormatScale = child.scale;
                sampleFormatImageColor = child.color;

                foreach (var holderChild in child.children.Where(formatChild => formatChild.name == "Holder" && formatChild.children.Count > 0).SelectMany(formatChild => formatChild.children))
                {
                    switch (holderChild.name)
                    {
                        case "RawImage_adchoicesIcon":
                            adChoiceIconPosition = holderChild.position;
                            adChoiceIconRotation = new Vector3(holderChild.rotation.x, holderChild.rotation.y, holderChild.rotation.z);
                            adChoiceIconScale = holderChild.scale;
                            adChoiceIconImageColor = holderChild.color;
                            break;
                        case "adTag":
                            adTagPosition = holderChild.position;
                            adTagRotation = new Vector3(holderChild.rotation.x, holderChild.rotation.y, holderChild.rotation.z);
                            adTagScale = holderChild.scale;
                            adTagTextColor = holderChild.textColor;
                            break;
                    }

                    if (holderChild.name != "Padding" || holderChild.children.Count <= 0) continue;
                    foreach (var paddingChild in holderChild.children)
                    {
                        switch (paddingChild.name)
                        {
                            case "Appdetails":
                            {
                                foreach (var appIconChild in paddingChild.children)
                                {
                                    switch (appIconChild.name)
                                    {
                                        case "AppIconHolder":
                                        {
                                            appIconPosition = appIconChild.position;
                                            appIconRotation = new Vector3(appIconChild.rotation.x, appIconChild.rotation.y, appIconChild.rotation.z);
                                            appIconScale = appIconChild.scale;

                                            foreach (var appIconHolderChild in appIconChild.children.Where(appIconHolderChild => appIconHolderChild.name == "RawImage_adIcon"))
                                            {
                                                appIconSprite = Resources.Load<Sprite>("Sprites/" + appIconHolderChild.spriteLocation);
                                                appIconImageColor = appIconHolderChild.color;
                                            }

                                            break;
                                        }
                                        case "AppInfo":
                                        {
                                            foreach (var appInfoChild in appIconChild.children)
                                            {
                                                switch (appInfoChild.name)
                                                {
                                                    case "Text_adHeadline":
                                                        adHeadlineText = appInfoChild.text;
                                                        adHeadlineTextPosition = appInfoChild.position;
                                                        adHeadlineTextRotation = new Vector3(appInfoChild.rotation.x, appInfoChild.rotation.y, appInfoChild.rotation.z);
                                                        adHeadlineTextScale = appInfoChild.scale;
                                                        adHeadlineTextColor = appInfoChild.textColor;
                                                        break;
                                                    case "StarRating":
                                                    {
                                                        starRatingPosition = appInfoChild.position;
                                                        starRatingRotation = new Vector3(appInfoChild.rotation.x, appInfoChild.rotation.y, appInfoChild.rotation.z);
                                                        starRatingScale = appInfoChild.scale;
                                                        foreach (var ratingChild in appInfoChild.children.Where(ratingChild => ratingChild.tag == "Fill"))
                                                        {
                                                            starRatingValue = ratingChild.rating;
                                                        }

                                                        break;
                                                    }
                                                    case "priceTxt":
                                                        appPriceTextPosition = appInfoChild.position;
                                                        appPriceTextRotation = new Vector3(appInfoChild.rotation.x, appInfoChild.rotation.y, appInfoChild.rotation.z);
                                                        appPriceTextScale = appInfoChild.scale;
                                                        appPriceText = appInfoChild.text;
                                                        appPriceTextColor = appInfoChild.textColor;
                                                        break;
                                                }
                                            }

                                            break;
                                        }
                                        case "Text_body":
                                            appBodyText = appIconChild.text;
                                            appBodyTextPosition = appIconChild.position;
                                            appBodyTextRotation = new Vector3(appIconChild.rotation.x, appIconChild.rotation.y, appIconChild.rotation.z);
                                            appBodyTextScale = appIconChild.scale;
                                            appBodyTextColor = appIconChild.textColor;
                                            break;
                                    }
                                }

                                break;
                            }
                            case "Button_Cta":
                            {
                                appInstallBtnPosition = paddingChild.position;
                                appInstallBtnRotation = new Vector3(paddingChild.rotation.x, paddingChild.rotation.y, paddingChild.rotation.z);
                                appInstallBtnScale = paddingChild.scale;
                                appInstallBtnColor = paddingChild.color;
                                foreach (var btnChild in paddingChild.children)
                                {
                                    appInstallBtnText = btnChild.text;
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        /*Will update the values which have been changed in the editor window to json file*/
        private void UpdateTemplateValues()
        {
            ReadFromFile();

            if (jsonFile == null) return;

            if (uiGameObjectData.children.Count <= 0) return;
            foreach (var child in uiGameObjectData.children.Where(child => child.name == "SampleFormat" && child.children.Count > 0))
            {
                child.position = sampleFormatPosition;
                child.rotation = Quaternion.Euler(sampleFormatRotation.x, sampleFormatRotation.y, sampleFormatRotation.z);
                child.scale = sampleFormatScale;
                child.color = sampleFormatImageColor;

                foreach (var holderChild in child.children.Where(formatChild => formatChild.name == "Holder" && formatChild.children.Count > 0).SelectMany(formatChild => formatChild.children))
                {
                    switch (holderChild.name)
                    {
                        case "RawImage_adchoicesIcon":
                            holderChild.position = adChoiceIconPosition;
                            holderChild.rotation = Quaternion.Euler(adChoiceIconRotation.x, adChoiceIconRotation.y, adChoiceIconRotation.z);
                            holderChild.scale = adChoiceIconScale;
                            holderChild.color = adChoiceIconImageColor;
                            break;
                        case "adTag":
                            holderChild.position = adTagPosition;
                            holderChild.rotation = Quaternion.Euler(adTagRotation.x, adTagRotation.y, adTagRotation.z);
                            holderChild.scale = adTagScale;
                            holderChild.textColor = adTagTextColor;
                            break;
                    }

                    if (holderChild.name != "Padding" || holderChild.children.Count <= 0) continue;
                    foreach (var paddingChild in holderChild.children)
                    {
                        switch (paddingChild.name)
                        {
                            case "Appdetails":
                            {
                                foreach (var appIconChild in paddingChild.children)
                                {
                                    switch (appIconChild.name)
                                    {
                                        case "AppIconHolder":
                                        {
                                            appIconChild.position = appIconPosition;
                                            appIconChild.rotation = Quaternion.Euler(appIconRotation.x, appIconRotation.y, appIconRotation.z);
                                            appIconChild.scale = appIconScale;

                                            foreach (var appIconHolderChild in appIconChild.children.Where(appIconHolderChild => appIconHolderChild.name == "RawImage_adIcon"))
                                            {
                                                appIconHolderChild.spriteLocation = appIconSprite.name;
                                                appIconHolderChild.color = appIconImageColor;
                                            }

                                            break;
                                        }
                                        case "AppInfo":
                                        {
                                            foreach (var appInfoChild in appIconChild.children)
                                            {
                                                switch (appInfoChild.name)
                                                {
                                                    case "Text_adHeadline":
                                                        appInfoChild.text = adHeadlineText;
                                                        appInfoChild.position = adHeadlineTextPosition;
                                                        appInfoChild.rotation = Quaternion.Euler(adHeadlineTextRotation.x, adHeadlineTextRotation.y, adHeadlineTextRotation.z);
                                                        appInfoChild.scale = adHeadlineTextScale;
                                                        appInfoChild.textColor = adHeadlineTextColor;
                                                        break;
                                                    case "StarRating":
                                                    {
                                                        appInfoChild.position = starRatingPosition;
                                                        appInfoChild.rotation = Quaternion.Euler(starRatingRotation.x, starRatingRotation.y, starRatingRotation.z);
                                                        appInfoChild.scale = starRatingScale;
                                                        foreach (var ratingChild in appInfoChild.children.Where(ratingChild => ratingChild.tag == "Fill"))
                                                        {
                                                            ratingChild.rating = starRatingValue;
                                                        }

                                                        break;
                                                    }
                                                    case "priceTxt":
                                                        appInfoChild.position = appPriceTextPosition;
                                                        appInfoChild.rotation = Quaternion.Euler(appPriceTextRotation.x, appPriceTextRotation.y, appPriceTextRotation.z);
                                                        appInfoChild.scale = appPriceTextScale;
                                                        appInfoChild.text = appPriceText;
                                                        appInfoChild.textColor = appPriceTextColor;
                                                        break;
                                                }
                                            }

                                            break;
                                        }
                                        case "Text_body":
                                            appIconChild.text = appBodyText;
                                            appIconChild.position = appBodyTextPosition;
                                            appIconChild.rotation = Quaternion.Euler(appBodyTextRotation.x, appBodyTextRotation.y, appBodyTextRotation.z);
                                            appIconChild.scale = appBodyTextScale;
                                            appIconChild.textColor = appBodyTextColor;
                                            break;
                                    }
                                }

                                break;
                            }
                            case "Button_Cta":
                            {
                                paddingChild.position = appInstallBtnPosition;
                                paddingChild.rotation = Quaternion.Euler(appInstallBtnRotation.x, appInstallBtnRotation.y, appInstallBtnRotation.z);
                                paddingChild.scale = appInstallBtnScale;
                                paddingChild.color = appInstallBtnColor;
                                foreach (var btnChild in paddingChild.children)
                                {
                                    btnChild.text = appInstallBtnText;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            var json = JsonConvert.SerializeObject(uiGameObjectData, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = {new Vector3Converter()}
            });
            // Save JSON to a file
            var filePath = Application.dataPath + "/Resources/JSON/uiTemplateData.json";
            File.WriteAllText(filePath, json);
            Debug.Log("SuccessFully updated");
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

        /*Deserialize the json to uiGameObjectData*/
        private void ReadFromFile()
        {
            if (jsonFile != null)
            {
                var jsonText = jsonFile.text;
                uiGameObjectData = JsonConvert.DeserializeObject<UIGameObjectData>(jsonText);
            }
            else
            {
                Debug.LogError("Please select a JSON file.");
                return;
            }
        }
    }
}