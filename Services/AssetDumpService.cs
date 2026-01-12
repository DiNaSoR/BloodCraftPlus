using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.Services;

/// <summary>
/// Dumps UI assets and layout data for the Character menu so it can be recreated externally.
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
    const string DumpFolderName = "Eclipse/Dumps";

    /// <summary>
    /// Dumps Character menu UI assets and a manifest file to disk.
    /// </summary>
    public static void DumpCharacterMenuAssets()
    {
        DumpMenuAssetsInternal(FindCharacterMenuRoot(), "CharacterMenu", "Character menu");
    }

    /// <summary>
    /// Dumps Main Menu UI assets and a manifest file to disk.
    /// </summary>
    public static void DumpMainMenuAssets()
    {
        DumpMenuAssetsInternal(FindMainMenuRoot(), "MainMenu", "Main menu");
    }

    /// <summary>
    /// Dumps HUD menu UI assets (V Blood, Spellbook, Vampire Powers, etc.) and a manifest file to disk.
    /// </summary>
    public static void DumpHudMenuAssets()
    {
        DumpMenuAssetsInternal(FindHudMenuRoot(), "HudMenu", "HUD menu");
    }

    /// <summary>
    /// Dumps both Character menu and Main menu UI assets.
    /// </summary>
    public static void DumpMenuAssets()
    {
        DumpCharacterMenuAssets();
        DumpMainMenuAssets();
        DumpHudMenuAssets();
    }

    /// <summary>
    /// Finds the Character menu root transform, including inactive objects.
    /// </summary>
    /// <returns>The Character menu root transform, if found.</returns>
    static Transform FindCharacterMenuRoot()
    {
        Transform root = FindTransformByName(CharacterMenuRootName);
        if (root != null)
        {
            return root;
        }

        return FindTransformByName(CharacterMenuRootAltName);
    }

    /// <summary>
    /// Finds the Main menu root transform, including inactive objects.
    /// </summary>
    /// <returns>The Main menu root transform, if found.</returns>
    static Transform FindMainMenuRoot()
    {
        Transform root = FindTransformByName(MainMenuRootName);
        if (root != null)
        {
            return root;
        }

        root = FindTransformByName(MainMenuRootAltName);
        if (root != null)
        {
            return root;
        }

        root = FindTransformByName(MainMenuRootAltName2);
        if (root != null)
        {
            return root;
        }

        return FindTransformByName(MainMenuRootAltName3);
    }

    /// <summary>
    /// Finds the HUD menu root transform, including inactive objects.
    /// </summary>
    /// <returns>The HUD menu root transform, if found.</returns>
    static Transform FindHudMenuRoot()
    {
        Transform root = FindTransformByName(HudMenuRootName);
        if (root != null)
        {
            return root;
        }

        return FindTransformByName(HudMenuRootAltName);
    }

    /// <summary>
    /// Finds a transform by name (including inactive objects).
    /// </summary>
    /// <param name="name">The transform name to find.</param>
    /// <returns>The matching transform, if found.</returns>
    static Transform FindTransformByName(string name)
    {
        Transform fallback = null;

        foreach (Transform transform in UnityEngine.Resources.FindObjectsOfTypeAll<Transform>())
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

    /// <summary>
    /// Dumps UI assets for a given root.
    /// </summary>
    /// <param name="root">The root transform to dump.</param>
    /// <param name="folderPrefix">The folder prefix for the output.</param>
    /// <param name="displayName">The display name used in logs.</param>
    static void DumpMenuAssetsInternal(Transform root, string folderPrefix, string displayName)
    {
        try
        {
            if (root == null)
            {
                Core.Log.LogWarning($"[Asset Dump] {displayName} root not found. Open the menu and try again.");
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
            Core.Log.LogInfo($"[Asset Dump] {displayName} assets written to {basePath}");
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[Asset Dump] Failed to dump {displayName} assets: {ex}");
        }
    }

    /// <summary>
    /// Dumps sprites used under the Character menu to PNG files and logs metadata.
    /// </summary>
    /// <param name="root">The Character menu root.</param>
    /// <param name="outputPath">The output directory for sprites.</param>
    /// <param name="manifest">The manifest builder.</param>
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

                string filePath = Path.Combine(outputPath, $"{fileName}.png");
                if (SaveSprite(sprite, filePath, readableCache))
                {
                    savedSprites[spriteId] = $"{fileName}.png";
                }
            }

            string spriteFile = savedSprites.TryGetValue(spriteId, out string savedName) ? savedName : "unavailable";
            manifest.AppendLine($"image={GetPath(image.transform)}|sprite={sprite.name}|file={spriteFile}|border={FormatVector4(sprite.border)}|rect={FormatRect(sprite.textureRect)}|ppu={sprite.pixelsPerUnit:0.##}|type={image.type}");
        }

        manifest.AppendLine(string.Empty);

        foreach (Texture2D cached in readableCache.Values)
        {
            if (cached == null)
            {
                continue;
            }

            UnityEngine.Object.Destroy(cached);
        }
    }

    /// <summary>
    /// Dumps TextMeshPro font atlas textures and metadata used under the Character menu.
    /// </summary>
    /// <param name="root">The Character menu root.</param>
    /// <param name="outputPath">The output directory for font atlas textures.</param>
    /// <param name="manifest">The manifest builder.</param>
    static void DumpFonts(Transform root, string outputPath, StringBuilder manifest)
    {
        var savedFonts = new Dictionary<int, string>();
        var readableCache = new Dictionary<int, Texture2D>();

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
            if (!savedFonts.TryGetValue(fontId, out string atlasFile))
            {
                Texture2D atlas = font.atlasTexture;
                if (atlas != null)
                {
                    string fileName = SanitizeFileName(font.name);
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        fileName = $"font_{fontId}";
                    }

                    string atlasPath = Path.Combine(outputPath, $"{fileName}_atlas.png");
                    if (SaveTexture(atlas, atlasPath, readableCache))
                    {
                        savedFonts[fontId] = Path.GetFileName(atlasPath);
                    }
                }
            }

            string savedAtlas = savedFonts.TryGetValue(fontId, out string name) ? name : "unavailable";
            string fontName = font != null ? font.name : "null";
            string materialName = text.fontMaterial != null ? text.fontMaterial.name : "null";
            manifest.AppendLine($"text={GetPath(text.transform)}|font={fontName}|atlas={savedAtlas}|fontSize={text.fontSize:0.##}|style={text.fontStyle}|material={materialName}");
        }

        manifest.AppendLine(string.Empty);

        foreach (Texture2D cached in readableCache.Values)
        {
            if (cached == null)
            {
                continue;
            }

            UnityEngine.Object.Destroy(cached);
        }
    }

    /// <summary>
    /// Dumps layout data for all RectTransforms under the Character menu.
    /// </summary>
    /// <param name="root">The Character menu root.</param>
    /// <param name="manifest">The manifest builder.</param>
    static void DumpLayout(Transform root, StringBuilder manifest)
    {
        RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
        manifest.AppendLine("[Layout]");

        for (int i = 0; i < rects.Length; i++)
        {
            RectTransform rect = rects[i];
            if (rect == null)
            {
                continue;
            }

            Rect r = rect.rect;
            manifest.AppendLine(
                $"path={GetPath(rect)}|active={rect.gameObject.activeInHierarchy}|anchorMin={FormatVector2(rect.anchorMin)}|anchorMax={FormatVector2(rect.anchorMax)}|pivot={FormatVector2(rect.pivot)}|pos={FormatVector2(rect.anchoredPosition)}|size={FormatVector2(rect.sizeDelta)}|rectW={r.width:0.##}|rectH={r.height:0.##}|scale={FormatVector3(rect.localScale)}");
        }

        manifest.AppendLine(string.Empty);
    }

    /// <summary>
    /// Dumps LayoutElement components for precise sizing data.
    /// </summary>
    /// <param name="root">The root transform.</param>
    /// <param name="manifest">The manifest builder.</param>
    static void DumpLayoutElements(Transform root, StringBuilder manifest)
    {
        LayoutElement[] elements = root.GetComponentsInChildren<LayoutElement>(true);
        manifest.AppendLine("[LayoutElements]");

        for (int i = 0; i < elements.Length; i++)
        {
            LayoutElement elem = elements[i];
            if (elem == null)
            {
                continue;
            }

            manifest.AppendLine(
                $"path={GetPath(elem.transform)}|minW={elem.minWidth:0.##}|minH={elem.minHeight:0.##}|prefW={elem.preferredWidth:0.##}|prefH={elem.preferredHeight:0.##}|flexW={elem.flexibleWidth:0.##}|flexH={elem.flexibleHeight:0.##}|ignoreLayout={elem.ignoreLayout}");
        }

        manifest.AppendLine(string.Empty);
    }

    /// <summary>
    /// Dumps HorizontalLayoutGroup and VerticalLayoutGroup components.
    /// </summary>
    /// <param name="root">The root transform.</param>
    /// <param name="manifest">The manifest builder.</param>
    static void DumpLayoutGroups(Transform root, StringBuilder manifest)
    {
        HorizontalOrVerticalLayoutGroup[] groups = root.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true);
        manifest.AppendLine("[LayoutGroups]");

        for (int i = 0; i < groups.Length; i++)
        {
            HorizontalOrVerticalLayoutGroup group = groups[i];
            if (group == null)
            {
                continue;
            }

            string groupType = group is HorizontalLayoutGroup ? "Horizontal" : "Vertical";
            RectOffset pad = group.padding;
            manifest.AppendLine(
                $"path={GetPath(group.transform)}|type={groupType}|padding=({pad.left},{pad.right},{pad.top},{pad.bottom})|spacing={group.spacing:0.##}|align={group.childAlignment}|ctrlW={group.childControlWidth}|ctrlH={group.childControlHeight}|expandW={group.childForceExpandWidth}|expandH={group.childForceExpandHeight}");
        }

        manifest.AppendLine(string.Empty);
    }

    /// <summary>
    /// Dumps ContentSizeFitter components.
    /// </summary>
    /// <param name="root">The root transform.</param>
    /// <param name="manifest">The manifest builder.</param>
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

            manifest.AppendLine(
                $"path={GetPath(fitter.transform)}|hFit={fitter.horizontalFit}|vFit={fitter.verticalFit}");
        }

        manifest.AppendLine(string.Empty);
    }


    /// <summary>
    /// Saves a sprite as a PNG file.
    /// </summary>
    /// <param name="sprite">The sprite to save.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="readableCache">Cache of readable textures.</param>
    /// <returns>True if the sprite was saved.</returns>
    static bool SaveSprite(Sprite sprite, string filePath, Dictionary<int, Texture2D> readableCache)
    {
        if (sprite == null || sprite.texture == null)
        {
            return false;
        }

        Texture2D source = sprite.texture;
        int textureId = source.GetInstanceID();
        if (!readableCache.TryGetValue(textureId, out Texture2D readable))
        {
            readable = CreateReadableTexture(source);
            readableCache[textureId] = readable;
        }

        if (readable == null)
        {
            return false;
        }

        Rect rect = sprite.textureRect;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        Texture2D cropped = new(width, height, TextureFormat.ARGB32, false);
        Color[] pixels = readable.GetPixels(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height);
        cropped.SetPixels(pixels);
        cropped.Apply();

        File.WriteAllBytes(filePath, cropped.EncodeToPNG());
        UnityEngine.Object.Destroy(cropped);
        return true;
    }

    /// <summary>
    /// Saves a texture to PNG.
    /// </summary>
    /// <param name="texture">The texture to save.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="readableCache">Cache of readable textures.</param>
    /// <returns>True if the texture was saved.</returns>
    static bool SaveTexture(Texture2D texture, string filePath, Dictionary<int, Texture2D> readableCache)
    {
        if (texture == null)
        {
            return false;
        }

        int textureId = texture.GetInstanceID();
        if (!readableCache.TryGetValue(textureId, out Texture2D readable))
        {
            readable = CreateReadableTexture(texture);
            readableCache[textureId] = readable;
        }

        if (readable == null)
        {
            return false;
        }

        File.WriteAllBytes(filePath, readable.EncodeToPNG());
        return true;
    }

    /// <summary>
    /// Creates a readable copy of a texture.
    /// </summary>
    /// <param name="source">The source texture.</param>
    /// <returns>A readable texture.</returns>
    static Texture2D CreateReadableTexture(Texture2D source)
    {
        if (source == null)
        {
            return null;
        }

        // Always create a copy so we can safely destroy it after dumping.
        RenderTexture renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, renderTexture);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D readable = new(source.width, source.height, TextureFormat.ARGB32, false);
        readable.ReadPixels(new Rect(0f, 0f, source.width, source.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readable;
    }

    /// <summary>
    /// Sanitizes a string to be a safe file name.
    /// </summary>
    /// <param name="value">The string to sanitize.</param>
    /// <returns>A sanitized file name.</returns>
    static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value.Replace(' ', '_');
    }

    /// <summary>
    /// Formats a vector2 for logging.
    /// </summary>
    /// <param name="vector">The vector to format.</param>
    /// <returns>A formatted string.</returns>
    static string FormatVector2(Vector2 vector)
    {
        return $"({vector.x:0.##},{vector.y:0.##})";
    }

    /// <summary>
    /// Formats a vector3 for logging.
    /// </summary>
    /// <param name="vector">The vector to format.</param>
    /// <returns>A formatted string.</returns>
    static string FormatVector3(Vector3 vector)
    {
        return $"({vector.x:0.##},{vector.y:0.##},{vector.z:0.##})";
    }

    /// <summary>
    /// Formats a vector4 for logging.
    /// </summary>
    /// <param name="vector">The vector to format.</param>
    /// <returns>A formatted string.</returns>
    static string FormatVector4(Vector4 vector)
    {
        return $"({vector.x:0.##},{vector.y:0.##},{vector.z:0.##},{vector.w:0.##})";
    }

    /// <summary>
    /// Formats a rect for logging.
    /// </summary>
    /// <param name="rect">The rect to format.</param>
    /// <returns>A formatted string.</returns>
    static string FormatRect(Rect rect)
    {
        return $"({rect.x:0.##},{rect.y:0.##},{rect.width:0.##},{rect.height:0.##})";
    }

    /// <summary>
    /// Builds a full path for a transform.
    /// </summary>
    /// <param name="transform">The transform to describe.</param>
    /// <returns>The hierarchy path.</returns>
    static string GetPath(Transform transform)
    {
        var sb = new StringBuilder();
        Transform current = transform;

        while (current != null)
        {
            if (sb.Length == 0)
            {
                sb.Insert(0, current.name);
            }
            else
            {
                sb.Insert(0, $"{current.name}/");
            }

            current = current.parent;
        }

        return sb.ToString();
    }
}
