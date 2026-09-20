#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PassiveBuildingPanelSetup
{
    [MenuItem("Tools/MiningGame/Create Info Panel For Selected Building")]
    public static void Create()
    {
        if (Application.isPlaying) { Debug.LogWarning("Exit Play Mode before creating panels."); return; }
        var root = Selection.activeGameObject;
        var interaction = root != null ? root.GetComponent<BuildingInteraction>() : null;
        if (interaction == null || EditorUtility.IsPersistent(root))
        {
            Debug.LogWarning("Select a building instance with BuildingInteraction in the Hierarchy.");
            return;
        }
        var serialized = new SerializedObject(interaction);
        var definition = serialized.FindProperty("definition").objectReferenceValue as BuildingDefinition;
        var panelField = serialized.FindProperty("buildingPanel");
        if (definition == null || panelField.objectReferenceValue != null)
        {
            Debug.LogWarning("Assign a Building Definition first. Building Panel must be empty; existing menus are preserved.");
            return;
        }
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Create building info panel");
        var canvasGO = new GameObject(root.name + " Info Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasGO, root.scene);
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create info canvas");
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = Box("InfoPanel", canvasGO.transform, Vector2.zero, Vector2.one, new Color(0, 0, 0, .65f));
        var card = Box("Card", panel, new Vector2(.1f, .08f), new Vector2(.9f, .92f), new Color(.10f, .13f, .17f, 1));
        var title = Text("Title", card, new Vector2(.06f, .82f), new Vector2(.94f, .96f), 32);
        title.fontStyle = FontStyles.Bold;
        var spriteRect = Box("BuildingImage", card, new Vector2(.06f, .28f), new Vector2(.33f, .78f), Color.white);
        var image = spriteRect.GetComponent<Image>();
        image.raycastTarget = false;
        var scrollGO = new GameObject("EffectScroll", typeof(RectTransform), typeof(ScrollRect));
        var scrollRect = (RectTransform)scrollGO.transform;
        scrollRect.SetParent(card, false); SetRect(scrollRect, new Vector2(.38f, .24f), new Vector2(.94f, .78f));
        var viewport = Box("Viewport", scrollRect, Vector2.zero, Vector2.one, new Color(1, 1, 1, .01f));
        viewport.gameObject.AddComponent<RectMask2D>();
        var body = Text("EffectText", viewport, Vector2.zero, Vector2.one, 24);
        var content = body.rectTransform;
        content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
        content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        body.alignment = TextAlignmentOptions.TopLeft;
        var fitter = body.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = scrollGO.GetComponent<ScrollRect>();
        scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 25;
        var buttonRect = Box("CloseButton", card, new Vector2(.35f, .06f), new Vector2(.65f, .17f), new Color(.25f, .40f, .50f, 1));
        var button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonRect.GetComponent<Image>();
        var label = Text("Label", buttonRect, Vector2.zero, Vector2.one, 24);
        label.text = "Schließen";
        var ui = panel.gameObject.AddComponent<PassiveBuildingInfoUI>();
        ui.definition = definition; ui.interaction = interaction; ui.titleText = title;
        ui.effectText = body; ui.buildingImage = image; ui.closeButton = button;
        ui.Refresh();
        panel.gameObject.SetActive(false);
        Undo.RecordObject(interaction, "Assign info panel");
        panelField.objectReferenceValue = panel.gameObject;
        serialized.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(interaction);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
        Undo.CollapseUndoOperations(group);
        Debug.Log("Info panel created and assigned. Set Effect Description on the Building Definition. Save the scene.");
    }

    private static RectTransform Box(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)go.transform; rect.SetParent(parent, false); SetRect(rect, min, max);
        go.GetComponent<Image>().color = color; return rect;
    }
    private static TextMeshProUGUI Text(string name, Transform parent, Vector2 min, Vector2 max, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rect = (RectTransform)go.transform; rect.SetParent(parent, false); SetRect(rect, min, max);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size; text.color = Color.white; text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false; return text;
    }
    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }
}
#endif
