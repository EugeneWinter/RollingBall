#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class UnityProjectDiag : EditorWindow
{
    [MenuItem("Diagnostics/МЕГА ДИАГНОСТИКА")]
    public static void GatherInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== МЕГА ДИАГНОСТИКА ===");
        sb.AppendLine($"Дата: {System.DateTime.Now}");
        sb.AppendLine($"Сцена: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        sb.AppendLine($"Unity: {Application.unityVersion}");

        // Game View размеры
        sb.AppendLine("\n========== GAME VIEW ==========");
        sb.AppendLine($"Screen.width x height: {Screen.width} x {Screen.height}");
        sb.AppendLine($"Screen.currentResolution: {Screen.currentResolution}");
        var gameView = GetMainGameViewSize();
        sb.AppendLine($"GameView size: {gameView}");

        // Все Canvas
        sb.AppendLine("\n========== CANVAS ==========");
        var canvases = Object.FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            sb.AppendLine($"\n[Canvas: {c.gameObject.name}]");
            sb.AppendLine($"  activeInHierarchy: {c.gameObject.activeInHierarchy}");
            sb.AppendLine($"  RenderMode: {c.renderMode}");
            sb.AppendLine($"  worldCamera: {(c.worldCamera != null ? c.worldCamera.name : "null")}");
            sb.AppendLine($"  sortingOrder: {c.sortingOrder}");
            sb.AppendLine($"  scaleFactor: {c.scaleFactor}");

            var rt = c.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                sb.AppendLine($"  Canvas world corners: BL={corners[0]}, TL={corners[1]}, TR={corners[2]}, BR={corners[3]}");
                sb.AppendLine($"  Canvas rect: {rt.rect}");
                sb.AppendLine($"  Canvas localScale: {rt.localScale}");
            }

            var scaler = c.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                sb.AppendLine($"  ScaleMode: {scaler.uiScaleMode}");
                sb.AppendLine($"  ReferenceRes: {scaler.referenceResolution}");
                sb.AppendLine($"  MatchMode: {scaler.screenMatchMode}");
                sb.AppendLine($"  Match: {scaler.matchWidthOrHeight}");
                sb.AppendLine($"  scaleFactor (runtime): {scaler.scaleFactor}");
            }

            var gr = c.GetComponent<GraphicRaycaster>();
            sb.AppendLine($"  GraphicRaycaster: {(gr != null ? $"enabled={gr.enabled}" : "МИССИНГ!")}");
        }

        // EventSystem
        sb.AppendLine("\n========== EVENTSYSTEM ==========");
        var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
            sb.AppendLine("НЕТ EventSystem! Клики не будут работать!");
        else
        {
            sb.AppendLine($"EventSystem найден: {es.gameObject.name}, active={es.gameObject.activeInHierarchy}");
            var im = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            sb.AppendLine($"StandaloneInputModule: {(im != null ? "есть" : "НЕТ!")}");
        }

        // Полная иерархия с деталями
        sb.AppendLine("\n========== ПОЛНАЯ ИЕРАРХИЯ ==========");
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var obj in roots)
        {
            Traverse(obj, 0, sb);
        }

        // Детальный анализ кнопок уровней
        sb.AppendLine("\n========== АНАЛИЗ КНОПОК УРОВНЕЙ ==========");
        string[] btnNames = { "LevelBtn_1", "LevelBtn_2", "LevelBtn_3" };
        foreach (string name in btnNames)
        {
            var btn = GameObject.Find(name);
            if (btn == null)
            {
                sb.AppendLine($"\n[{name}] НЕ НАЙДЕН!");
                continue;
            }

            sb.AppendLine($"\n[{name}]");
            sb.AppendLine($"  activeInHierarchy: {btn.activeInHierarchy}");
            sb.AppendLine($"  activeSelf: {btn.activeSelf}");

            var rt = btn.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                sb.AppendLine($"  WorldCorners: BL={corners[0]}, TL={corners[1]}, TR={corners[2]}, BR={corners[3]}");
                sb.AppendLine($"  ScreenSpace BL: {WorldToScreen(corners[0])}");
                sb.AppendLine($"  ScreenSpace TR: {WorldToScreen(corners[2])}");
                sb.AppendLine($"  rect: {rt.rect}");
                sb.AppendLine($"  sizeDelta: {rt.sizeDelta}");
                sb.AppendLine($"  anchoredPosition: {rt.anchoredPosition}");
                sb.AppendLine($"  anchorMin/Max: {rt.anchorMin} / {rt.anchorMax}");
                sb.AppendLine($"  pivot: {rt.pivot}");
                sb.AppendLine($"  localScale: {rt.localScale}");
                sb.AppendLine($"  lossyScale: {rt.lossyScale}");
                sb.AppendLine($"  position: {rt.position}");
            }

            // Все компоненты
            sb.AppendLine("  Components:");
            var comps = btn.GetComponents<Component>();
            foreach (var comp in comps)
            {
                if (comp == null) { sb.AppendLine("    [MISSING SCRIPT!]"); continue; }
                sb.AppendLine($"    - {comp.GetType().Name}");
            }

            // Проверяем raycastTarget
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                sb.AppendLine($"  Image.raycastTarget: {img.raycastTarget}");
                sb.AppendLine($"  Image.color: {img.color}");
                sb.AppendLine($"  Image.enabled: {img.enabled}");
                sb.AppendLine($"  Image.canvasRenderer.cull: {img.canvasRenderer.cull}");
                sb.AppendLine($"  Image.canvasRenderer.GetAlpha: {img.canvasRenderer.GetAlpha()}");
            }

            var button = btn.GetComponent<Button>();
            if (button != null)
            {
                sb.AppendLine($"  Button.interactable: {button.interactable}");
                sb.AppendLine($"  Button.enabled: {button.enabled}");
                sb.AppendLine($"  Button.onClick persistent count: {button.onClick.GetPersistentEventCount()}");
            }

            // Дерево родителей
            sb.AppendLine("  Parent chain (снизу вверх):");
            Transform t = btn.transform.parent;
            int level = 1;
            while (t != null)
            {
                sb.Append($"    [{level}] {t.name} active={t.gameObject.activeSelf}");
                var pRt = t.GetComponent<RectTransform>();
                if (pRt != null)
                {
                    sb.Append($" scale={pRt.localScale} rect={pRt.rect}");
                }
                var cg = t.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    sb.Append($" [CanvasGroup alpha={cg.alpha} interactable={cg.interactable} blocksRaycasts={cg.blocksRaycasts}]");
                }
                var pMask = t.GetComponent<Mask>();
                if (pMask != null)
                {
                    sb.Append($" [Mask enabled={pMask.enabled}]");
                }
                sb.AppendLine();
                t = t.parent;
                level++;
            }

            // Проверяем детей
            sb.AppendLine("  Children:");
            foreach (Transform child in btn.transform)
            {
                sb.AppendLine($"    - {child.name} active={child.gameObject.activeSelf}");
            }
        }

        // CanvasGroup поиск везде
        sb.AppendLine("\n========== ПОИСК CANVAS GROUPS ==========");
        var cgs = Object.FindObjectsOfType<CanvasGroup>();
        foreach (var cg in cgs)
        {
            sb.AppendLine($"CanvasGroup на {cg.gameObject.name}: alpha={cg.alpha}, interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}");
        }

        // Скрипты на Canvas с полями
        sb.AppendLine("\n========== СКРИПТЫ И ИХ ПОЛЯ ==========");
        foreach (var c in canvases)
        {
            var behaviours = c.GetComponents<MonoBehaviour>();
            foreach (var b in behaviours)
            {
                if (b == null) continue;
                sb.AppendLine($"\n[{c.gameObject.name}] {b.GetType().Name} (enabled={b.enabled})");

                var so = new SerializedObject(b);
                var prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.name == "m_Script") continue;
                    string val = SerializedPropValue(prop);
                    sb.AppendLine($"  {prop.propertyPath} = {val}");
                }
            }
        }

        // Проверяем camera clipping
        sb.AppendLine("\n========== КАМЕРЫ ==========");
        var cams = Object.FindObjectsOfType<Camera>();
        foreach (var cam in cams)
        {
            sb.AppendLine($"[{cam.gameObject.name}] pos={cam.transform.position}, ortho={cam.orthographic}, orthoSize={cam.orthographicSize}, near={cam.nearClipPlane}, far={cam.farClipPlane}, depth={cam.depth}");
        }

        string path = Path.Combine(Application.dataPath, "mega_diagnostics.txt");
        File.WriteAllText(path, sb.ToString());
        EditorUtility.RevealInFinder(path);
        Debug.Log($"<color=green><b>МЕГА ДИАГНОСТИКА готова!</b> {path}</color>");
    }

    private static string SerializedPropValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer: return prop.intValue.ToString();
            case SerializedPropertyType.Float: return prop.floatValue.ToString("F2");
            case SerializedPropertyType.String: return $"\"{prop.stringValue}\"";
            case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
            case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "NULL";
            case SerializedPropertyType.Vector2: return prop.vector2Value.ToString();
            case SerializedPropertyType.Vector3: return prop.vector3Value.ToString();
            case SerializedPropertyType.ArraySize: return $"array_size={prop.intValue}";
            case SerializedPropertyType.Color: return prop.colorValue.ToString();
            default: return prop.propertyType.ToString();
        }
    }

    private static Vector2 WorldToScreen(Vector3 worldPos)
    {
        return new Vector2(worldPos.x, worldPos.y);
    }

    private static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }

    private static void Traverse(GameObject obj, int indent, StringBuilder sb)
    {
        string pad = new string(' ', indent * 2);
        sb.Append($"{pad}[{obj.name}] active={obj.activeSelf}");

        var rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            sb.Append($" | Pos({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0})");
            sb.Append($" Size({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0})");
            sb.Append($" LocalScale({rt.localScale.x:F1},{rt.localScale.y:F1},{rt.localScale.z:F1})");
            sb.Append($" LossyScale({rt.lossyScale.x:F2},{rt.lossyScale.y:F2},{rt.lossyScale.z:F2})");
            sb.Append($" AnchMin({rt.anchorMin.x:F1},{rt.anchorMin.y:F1})");
            sb.Append($" AnchMax({rt.anchorMax.x:F1},{rt.anchorMax.y:F1})");
            sb.Append($" Pivot({rt.pivot.x:F1},{rt.pivot.y:F1})");
        }

        var comps = obj.GetComponents<Component>();
        foreach (var comp in comps)
        {
            if (comp == null) { sb.Append(" | [MISSING!]"); continue; }
            if (comp is Transform || comp is RectTransform) continue;

            string n = comp.GetType().Name;

            if (comp is Image img)
                sb.Append($" | Image(color=#{ColorUtility.ToHtmlStringRGBA(img.color)}, raycastTarget={img.raycastTarget}, enabled={img.enabled})");
            else if (comp is Button btn)
                sb.Append($" | Button(interactable={btn.interactable}, enabled={btn.enabled}, onClickEvents={btn.onClick.GetPersistentEventCount()})");
            else if (comp is Text txt)
                sb.Append($" | Text(\"{txt.text}\", size={txt.fontSize}, color=#{ColorUtility.ToHtmlStringRGBA(txt.color)}, raycastTarget={txt.raycastTarget})");
            else if (comp is ScrollRect sr)
                sb.Append($" | ScrollRect(content={(sr.content != null ? sr.content.name : "NULL")})");
            else if (comp is Mask m)
                sb.Append($" | Mask(enabled={m.enabled}, show={m.showMaskGraphic})");
            else if (comp is HorizontalLayoutGroup hlg)
                sb.Append($" | HLayout(spacing={hlg.spacing}, ctrlW={hlg.childControlWidth}, ctrlH={hlg.childControlHeight}, forceW={hlg.childForceExpandWidth})");
            else if (comp is ContentSizeFitter csf)
                sb.Append($" | Fitter(h={csf.horizontalFit}, v={csf.verticalFit})");
            else if (comp is CanvasGroup cg)
                sb.Append($" | CanvasGroup(alpha={cg.alpha}, blocksRaycasts={cg.blocksRaycasts})");
            else if (comp is CanvasRenderer cr)
                sb.Append($" | CanvasRenderer(alpha={cr.GetAlpha()}, cull={cr.cull})");
            else
                sb.Append($" | {n}");
        }

        sb.AppendLine();

        foreach (Transform child in obj.transform)
        {
            Traverse(child.gameObject, indent + 1, sb);
        }
    }
}
#endif