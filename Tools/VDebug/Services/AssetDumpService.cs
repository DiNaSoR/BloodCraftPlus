using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VDebug.Services;

/// <summary>
/// Dumps UI sprites/fonts/layout data to disk for external inspection.
/// </summary>
internal static class AssetDumpService
{
    const string CharacterMenuRootName = "CharacterMenu(Clone)";
    const string CharacterMenuRootAltName = "CharacterMenu";
    const string MainMenuRootName = "MainMenuCanvas(Clone)";
    const string MainMenuRootAltName = "MainMenuCanvas";
    const string MainMenuRootAltName2 = "MainMenu";
    const string MainMenuRootAltName3 = "MainMenuCanvasBase";
    const string HudMenuRootName = "HUDMenuParent";
    const string HudMenuRootAltName = "HUDMenuParent(Clone)";

    const string DumpFolderName = "VDebug/DebugDumps";

    public static void DumpMenuAssets()
    {
        DumpCharacterMenu();
        DumpMainMenu();
        DumpHudMenu();
    }

    public static void DumpCharacterMenu()
    {
        DumpMenuAssetsInternal(FindCharacterMenuRoot(), "CharacterMenu", "Character menu");
    }

    public static void DumpMainMenu()
    {
        DumpMenuAssetsInternal(FindMainMenuRoot(), "MainMenu", "Main menu");
    }

    public static void DumpHudMenu()
    {
        DumpMenuAssetsInternal(FindHudMenuRoot(), "HudMenu", "HUD menu");
    }

    static Transform FindCharacterMenuRoot()
    {
        Transform root = FindTransformByName(CharacterMenuRootName);
        return root != null ? root : FindTransformByName(CharacterMenuRootAltName);
    }

    static Transform FindMainMenuRoot()
    {
        Transform root = FindTransformByName(MainMenuRootName);
        if (root != null) return root;

        root = FindTransformByName(MainMenuRootAltName);
        if (root != null) return root;

        root = FindTransformByName(MainMenuRootAltName2);
        if (root != null) return root;

        return FindTransformByName(MainMenuRootAltName3);
    }

    static Transform FindHudMenuRoot()
    {
        Transform root = FindTransformByName(HudMenuRootName);
        return root != null ? root : FindTransformByName(HudMenuRootAltName);
    }

    static Transform FindTransformByName(string name)
    {
        Transform fallback = null;

        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform == null || !transform.name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (transform.gameObject.activeInHierarchy)
            {
                return transform;
            }

            fallback ??= transform;
        }

        return fallback;
    }

    static void DumpMenuAssetsInternal(Transform root, string folderPrefix, string displayName)
    {
        try
        {
            if (root == null)
            {
                VDebugLog.Log.LogWarning($"[Asset Dump] {displayName} root not found. Open the menu and try again.");
                return;
            }

            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string basePath = Path.Combine(Paths.BepInExRootPath, DumpFolderName, $"{folderPrefix}_{stamp}");
            string spritesPath = Path.Combine(basePath, "sprites");
            string fontsPath = Path.Combine(basePath, "fonts");
            string manifestPath = Path.Combine(basePath, "manifest.txt");

            Directory.CreateDirectory(spritesPath);
            Directory.CreateDirectory(fontsPath);

            var manifest = new StringBuilder(4096);
            manifest.AppendLine("[Root]");
            manifest.AppendLine($"label={displayName}");
            manifest.AppendLine($"path={GetPath(root)}");
            manifest.AppendLine($"active={root.gameObject.activeInHierarchy}");
            manifest.AppendLine($"dump={basePath}");
            manifest.AppendLine(string.Empty);

            DumpSprites(root, spritesPath, manifest);
            DumpFonts(root, fontsPath, manifest);
            DumpLayout(root, manifest);
            DumpLayoutElements(root, manifest);
            DumpLayoutGroups(root, manifest);
            DumpContentSizeFitters(root, manifest);

            File.WriteAllText(manifestPath, manifest.ToString());
            VDebugLog.Log.LogInfo($"[Asset Dump] {displayName} assets written to {basePath}");
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogError($"[Asset Dump] Failed to dump {displayName} assets: {ex}");
        }
    }

    static void DumpSprites(Transform root, string outputPath, StringBuilder manifest)
    {
        var savedSprites = new Dictionary<int, string>();
        var readableCache = new Dictionary<int, Texture2D>();

        Image[] images = root.GetComponentsInChildren<Image>(true);
        manifest.AppendLine("[Sprites]");

        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null || image.sprite == null)
            {
                continue;
            }

            Sprite sprite = image.sprite;
            int spriteId = sprite.GetInstanceID();

            if (!savedSprites.TryGetValue(spriteId, out string fileName))
            {
                fileName = SanitizeFileName(sprite.name);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"sprite_{spriteId}";
                }

                fileName = $"{fileName}.png";
                if (savedSprites.ContainsValue(fileName))
                {
                    fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{spriteId}.png";
                }

                savedSprites[spriteId] = fileName;

                Texture2D readable = ConvertSpriteToReadableTexture(sprite, readableCache);
                if (readable != null)
                {
                    try
                    {
                        byte[] png = readable.EncodeToPNG();
                        File.WriteAllBytes(Path.Combine(outputPath, fileName), png);
                    }
                    finally
                    {
                        UnityEngine.Object.Destroy(readable);
                    }
                }
            }

            Rect rect = sprite.rect;
            Vector4 border = sprite.border;
            manifest.AppendLine($"image={GetPath(image.transform)}|sprite={sprite.name}|file={fileName}|border=({Format(border.x)},{Format(border.y)},{Format(border.z)},{Format(border.w)})|rect=({Format(rect.x)},{Format(rect.y)},{Format(rect.width)},{Format(rect.height)})|ppu={Format(sprite.pixelsPerUnit)}");
        }

        DestroyReadableTextures(readableCache);
        manifest.AppendLine(string.Empty);
    }

    static Texture2D ConvertSpriteToReadableTexture(Sprite sprite, Dictionary<int, Texture2D> readableCache)
    {
        if (sprite == null || sprite.texture == null)
        {
            return null;
        }

        Texture2D source = sprite.texture;
        int textureId = source.GetInstanceID();
        if (!readableCache.TryGetValue(textureId, out Texture2D readableTexture))
        {
            readableTexture = CreateReadableTexture(source);
            readableCache[textureId] = readableTexture;
        }

        if (readableTexture == null)
        {
            return null;
        }

        Rect rect = sprite.textureRect;
        int x = Mathf.Clamp(Mathf.RoundToInt(rect.x), 0, readableTexture.width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(rect.y), 0, readableTexture.height - 1);
        int width = Mathf.Clamp(Mathf.RoundToInt(rect.width), 0, readableTexture.width - x);
        int height = Mathf.Clamp(Mathf.RoundToInt(rect.height), 0, readableTexture.height - y);

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        Texture2D cropped = new(width, height, TextureFormat.ARGB32, false);
        try
        {
            Color[] pixels = readableTexture.GetPixels(x, y, width, height);
            cropped.SetPixels(pixels);
            cropped.Apply();
            return cropped;
        }
        catch
        {
            UnityEngine.Object.Destroy(cropped);
            return null;
        }
    }

    static Texture2D CreateReadableTexture(Texture2D source)
    {
        if (source == null)
        {
            return null;
        }

        RenderTexture renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previousActive = RenderTexture.active;
        try
        {
            Graphics.Blit(source, renderTexture);
            RenderTexture.active = renderTexture;

            Texture2D readable = new(source.width, source.height, TextureFormat.ARGB32, false);
            readable.ReadPixels(new Rect(0f, 0f, source.width, source.height), 0, 0);
            readable.Apply();
            return readable;
        }
        catch
        {
            return null;
        }
        finally
        {
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    static void DestroyReadableTextures(Dictionary<int, Texture2D> readableCache)
    {
        foreach (Texture2D texture in readableCache.Values)
        {
            if (texture == null)
            {
                continue;
            }

            UnityEngine.Object.Destroy(texture);
        }

        readableCache.Clear();
    }

    static void DumpFonts(Transform root, string outputPath, StringBuilder manifest)
    {
        var savedFonts = new Dictionary<int, string>();

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        manifest.AppendLine("[Fonts]");

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || text.font == null)
            {
                continue;
            }

            TMP_FontAsset font = text.font;
            int fontId = font.GetInstanceID();

            if (!savedFonts.TryGetValue(fontId, out string fileName))
            {
                fileName = SanitizeFileName(font.name);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"font_{fontId}";
                }

                fileName = $"{fileName}.txt";
                savedFonts[fontId] = fileName;

                string info = font.atlas != null
                    ? $"name={font.name}\natlas={font.atlas.name}\nsize={font.atlas.width}x{font.atlas.height}"
                    : $"name={font.name}\natlas=null";

                File.WriteAllText(Path.Combine(outputPath, fileName), info);
            }

            string materialName = text.fontMaterial != null ? text.fontMaterial.name : "null";
            manifest.AppendLine($"text={GetPath(text.transform)}|font={font.name}|atlas={(font.atlas != null ? font.atlas.name : "null")}|fontSize={Format(text.fontSize)}|style={text.fontStyle}|material={materialName}");
        }

        manifest.AppendLine(string.Empty);
    }

    static void DumpLayout(Transform root, StringBuilder manifest)
    {
        RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
        manifest.AppendLine("[RectTransforms]");

        for (int i = 0; i < rects.Length; i++)
        {
            RectTransform rect = rects[i];
            if (rect == null)
            {
                continue;
            }

            manifest.AppendLine($"path={GetPath(rect)}|active={rect.gameObject.activeSelf}|anchorMin=({Format(rect.anchorMin.x)},{Format(rect.anchorMin.y)})|anchorMax=({Format(rect.anchorMax.x)},{Format(rect.anchorMax.y)})|pivot=({Format(rect.pivot.x)},{Format(rect.pivot.y)})|pos=({Format(rect.anchoredPosition.x)},{Format(rect.anchoredPosition.y)})|size=({Format(rect.sizeDelta.x)},{Format(rect.sizeDelta.y)})|rectW={Format(rect.rect.width)}|rectH={Format(rect.rect.height)}|scale=({Format(rect.localScale.x)},{Format(rect.localScale.y)},{Format(rect.localScale.z)})");
        }

        manifest.AppendLine(string.Empty);
    }

    static void DumpLayoutElements(Transform root, StringBuilder manifest)
    {
        LayoutElement[] elements = root.GetComponentsInChildren<LayoutElement>(true);
        manifest.AppendLine("[LayoutElements]");

        for (int i = 0; i < elements.Length; i++)
        {
            LayoutElement element = elements[i];
            if (element == null)
            {
                continue;
            }

            manifest.AppendLine($"path={GetPath(element.transform)}|minW={Format(element.minWidth)}|minH={Format(element.minHeight)}|prefW={Format(element.preferredWidth)}|prefH={Format(element.preferredHeight)}|flexW={Format(element.flexibleWidth)}|flexH={Format(element.flexibleHeight)}|ignoreLayout={element.ignoreLayout}");
        }

        manifest.AppendLine(string.Empty);
    }

    static void DumpLayoutGroups(Transform root, StringBuilder manifest)
    {
        LayoutGroup[] groups = root.GetComponentsInChildren<LayoutGroup>(true);
        manifest.AppendLine("[LayoutGroups]");

        for (int i = 0; i < groups.Length; i++)
        {
            LayoutGroup group = groups[i];
            if (group == null)
            {
                continue;
            }

            string padding = group.padding != null
                ? $"({group.padding.left},{group.padding.right},{group.padding.top},{group.padding.bottom})"
                : "(null)";

            HorizontalOrVerticalLayoutGroup horizontalOrVertical = group as HorizontalOrVerticalLayoutGroup;
            GridLayoutGroup grid = group as GridLayoutGroup;

            string spacing = "n/a";
            if (horizontalOrVertical != null)
            {
                spacing = Format(horizontalOrVertical.spacing);
            }
            else if (grid != null)
            {
                spacing = $"({Format(grid.spacing.x)},{Format(grid.spacing.y)})";
            }

            string controlWidth = horizontalOrVertical != null ? horizontalOrVertical.childControlWidth.ToString() : "n/a";
            string controlHeight = horizontalOrVertical != null ? horizontalOrVertical.childControlHeight.ToString() : "n/a";
            string forceExpandWidth = horizontalOrVertical != null ? horizontalOrVertical.childForceExpandWidth.ToString() : "n/a";
            string forceExpandHeight = horizontalOrVertical != null ? horizontalOrVertical.childForceExpandHeight.ToString() : "n/a";

            manifest.AppendLine($"path={GetPath(group.transform)}|type={group.GetType().Name}|padding={padding}|spacing={spacing}|align={group.childAlignment}|ctrlW={controlWidth}|ctrlH={controlHeight}|expandW={forceExpandWidth}|expandH={forceExpandHeight}");
        }

        manifest.AppendLine(string.Empty);
    }

    static void DumpContentSizeFitters(Transform root, StringBuilder manifest)
    {
        ContentSizeFitter[] fitters = root.GetComponentsInChildren<ContentSizeFitter>(true);
        manifest.AppendLine("[ContentSizeFitters]");

        for (int i = 0; i < fitters.Length; i++)
        {
            ContentSizeFitter fitter = fitters[i];
            if (fitter == null)
            {
                continue;
            }

            manifest.AppendLine($"path={GetPath(fitter.transform)}|hFit={fitter.horizontalFit}|vFit={fitter.verticalFit}");
        }

        manifest.AppendLine(string.Empty);
    }

    static string GetPath(Transform transform)
    {
        if (transform == null)
        {
            return "null";
        }

        var sb = new StringBuilder(256);
        sb.Append(transform.name);

        Transform parent = transform.parent;
        while (parent != null)
        {
            sb.Insert(0, '/');
            sb.Insert(0, parent.name);
            parent = parent.parent;
        }

        return sb.ToString();
    }

    static string SanitizeFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        char[] invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(input.Length);

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            bool isInvalid = false;

            for (int j = 0; j < invalid.Length; j++)
            {
                if (invalid[j] == c)
                {
                    isInvalid = true;
                    break;
                }
            }

            sb.Append(isInvalid ? '_' : c);
        }

        return sb.ToString();
    }

    static string Format(float value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);
}
