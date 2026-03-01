using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Tools > Build Item UI
/// Builds the MGS1-style item bar UI on the Canvas and creates the ItemManager GO.
/// Run once per scene setup; safe to re-run (prompts before overwriting).
/// </summary>
public static class ItemUIBuilder
{
    const string PREFAB_DIR  = "Assets/Prefabs";
    const string SLOT_PATH   = "Assets/Prefabs/ItemSlotPrefab.prefab";

    [MenuItem("Tools/Build Item UI")]
    static void Build()
    {
        // ── Find Canvas ──────────────────────────────────────────────────────
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("ItemUIBuilder",
                "No Canvas found in scene.\nAdd a Screen Space – Overlay Canvas first.", "OK");
            return;
        }

        // ── Canvas Scaler ─────────────────────────────────────────────────────
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        // ── Guard: already built? ─────────────────────────────────────────────
        Transform existing = canvas.transform.Find("ItemMenuPanel");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("ItemUIBuilder",
                    "ItemMenuPanel already exists.\nRebuild it?", "Rebuild", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // ── Ensure Prefabs folder ─────────────────────────────────────────────
        if (!AssetDatabase.IsValidFolder(PREFAB_DIR))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // ── 1. Slot prefab ────────────────────────────────────────────────────
        GameObject slotTemplate = BuildSlotTemplate();
        GameObject slotPrefab   = PrefabUtility.SaveAsPrefabAsset(slotTemplate, SLOT_PATH);
        Object.DestroyImmediate(slotTemplate);

        // ── 2. Panel hierarchy ───────────────────────────────────────────────
        GameObject panelGO    = BuildPanel(canvas.transform);
        GameObject slotsRowGO = BuildSlotsRow(panelGO.transform);
        var (nameTMP, descTMP) = BuildDescPanel(panelGO.transform);

        // ── 3. ItemUI component + wiring ─────────────────────────────────────
        ItemUI itemUI = panelGO.AddComponent<ItemUI>();
        var uiSO = new SerializedObject(itemUI);
        uiSO.FindProperty("slotsParent").objectReferenceValue  = slotsRowGO.transform;
        uiSO.FindProperty("slotPrefab").objectReferenceValue   = slotPrefab;
        uiSO.FindProperty("itemNameText").objectReferenceValue = nameTMP;
        uiSO.FindProperty("descriptionText").objectReferenceValue = descTMP;
        uiSO.ApplyModifiedProperties();

        // ── 4. ItemManager GO ─────────────────────────────────────────────────
        if (GameObject.Find("ItemManager") == null)
        {
            GameObject imGO = new GameObject("ItemManager");
            Undo.RegisterCreatedObjectUndo(imGO, "Create ItemManager");
            ItemManager im = imGO.AddComponent<ItemManager>();
            var imSO = new SerializedObject(im);
            imSO.FindProperty("itemUI").objectReferenceValue = itemUI;
            imSO.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = panelGO;

        Debug.Log("[ItemUIBuilder] Done!\n" +
                  "Next steps:\n" +
                  "  1. Right-click Assets > Create > Stealth > Item  (x2: one Noise, one Decoy)\n" +
                  "  2. Assign ItemData assets to ItemManager > Items list\n" +
                  "  3. Create a Sphere, add ThrownItem.cs, save as Prefabs/ThrownItem.prefab\n" +
                  "  4. Assign ThrownItem prefab to ItemManager > Thrown Item Prefab");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Panel: full-width dark strip anchored to bottom of screen
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject BuildPanel(Transform parent)
    {
        GameObject go = new GameObject("ItemMenuPanel");
        Undo.RegisterCreatedObjectUndo(go, "Create ItemMenuPanel");
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.04f, 0.93f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 0f);
        rt.anchorMax        = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(0f, 195f);   // 195px tall

        return go;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SlotsRow: horizontal row at bottom of panel, slots spawn here at runtime
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject BuildSlotsRow(Transform parent)
    {
        GameObject go = new GameObject("SlotsRow");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 10f);
        rt.sizeDelta        = new Vector2(640f, 100f);

        HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = 12f;
        hlg.childAlignment       = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth      = false;
        hlg.childControlHeight     = false;

        return go;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DescriptionPanel: item name + description text above the slots row
    // ─────────────────────────────────────────────────────────────────────────
    static (TextMeshProUGUI nameTMP, TextMeshProUGUI descTMP) BuildDescPanel(Transform parent)
    {
        GameObject go = new GameObject("DescriptionPanel");
        go.transform.SetParent(parent, false);

        // Anchored to top of panel, sits above the SlotsRow
        RectTransform rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(1f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, 0f);   // flush with top of panel
        rt.sizeDelta        = new Vector2(-24f, 82f); // 82px tall, inset 12px each side

        // Thin separator line at bottom of desc area
        Image sep = go.AddComponent<Image>();
        sep.color = new Color(1f, 0.88f, 0.35f, 0.25f);

        // ── Item Name ──────────────────────────────────────────────────────
        GameObject nameGO = new GameObject("ItemNameText");
        nameGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text      = "ITEM NAME";
        nameTMP.fontSize  = 15f;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color     = new Color(1f, 0.88f, 0.35f);   // MGS golden yellow
        nameTMP.alignment = TextAlignmentOptions.BottomLeft;

        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin  = new Vector2(0f, 0.52f);
        nameRT.anchorMax  = new Vector2(1f, 1f);
        nameRT.offsetMin  = new Vector2(10f, 0f);
        nameRT.offsetMax  = new Vector2(-10f, -6f);

        // ── Description ────────────────────────────────────────────────────
        GameObject descGO = new GameObject("DescriptionText");
        descGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.text      = "Description.";
        descTMP.fontSize  = 11f;
        descTMP.color     = new Color(0.75f, 0.75f, 0.75f);
        descTMP.alignment = TextAlignmentOptions.TopLeft;

        RectTransform descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin  = new Vector2(0f, 0f);
        descRT.anchorMax  = new Vector2(1f, 0.52f);
        descRT.offsetMin  = new Vector2(10f, 4f);
        descRT.offsetMax  = new Vector2(-10f, 0f);

        return (nameTMP, descTMP);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SlotPrefab template: 90×90 dark tile with Icon / Name / Count children
    // Child names must match ItemUI.RefreshSlot() look-ups exactly.
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject BuildSlotTemplate()
    {
        // Root ──────────────────────────────────────────────────────────────
        GameObject slot = new GameObject("ItemSlotPrefab");
        Image slotBg = slot.AddComponent<Image>();
        slotBg.color = new Color(0.10f, 0.10f, 0.10f, 0.90f);
        slot.GetComponent<RectTransform>().sizeDelta = new Vector2(90f, 90f);

        // Icon (upper 72% of the tile) ──────────────────────────────────────
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(slot.transform, false);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color           = Color.white;
        iconImg.preserveAspect  = true;
        RectTransform iconRT    = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.06f, 0.28f);
        iconRT.anchorMax = new Vector2(0.94f, 0.94f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        // Name (bottom-left strip) ──────────────────────────────────────────
        GameObject nameGO = new GameObject("Name");
        nameGO.transform.SetParent(slot.transform, false);
        TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text      = "ITEM";
        nameTMP.fontSize  = 8f;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color     = Color.white;
        nameTMP.alignment = TextAlignmentOptions.BottomLeft;
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0f);
        nameRT.anchorMax = new Vector2(0.65f, 0.28f);
        nameRT.offsetMin = new Vector2(3f, 2f);
        nameRT.offsetMax = Vector2.zero;

        // Count (bottom-right) ──────────────────────────────────────────────
        GameObject countGO = new GameObject("Count");
        countGO.transform.SetParent(slot.transform, false);
        TextMeshProUGUI countTMP = countGO.AddComponent<TextMeshProUGUI>();
        countTMP.text      = "3";
        countTMP.fontSize  = 13f;
        countTMP.fontStyle = FontStyles.Bold;
        countTMP.color     = new Color(1f, 0.88f, 0.35f);
        countTMP.alignment = TextAlignmentOptions.BottomRight;
        RectTransform countRT = countGO.GetComponent<RectTransform>();
        countRT.anchorMin = new Vector2(0.55f, 0f);
        countRT.anchorMax = new Vector2(1f, 0.28f);
        countRT.offsetMin = new Vector2(0f, 2f);
        countRT.offsetMax = new Vector2(-3f, 0f);

        return slot;
    }
}
