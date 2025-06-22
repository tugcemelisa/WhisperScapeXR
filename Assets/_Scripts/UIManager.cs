using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Dropdowns")]
    public TMP_Dropdown moodDropdown;
    public TMP_Dropdown themeDropdown;
    public TMP_Dropdown colorPaletteDropdown;

    [Header("Buttons")]
    public Button getSuggestionButton;
    public Button regenerateButton;

    [Header("Output")]
    public TextMeshProUGUI suggestionText;

    [Header("AI System")]
    public ChatGPTManager chatGPT;

    private void Start()
    {
        moodDropdown.ClearOptions();
        moodDropdown.AddOptions(new List<string>
        {
            "Calm", "Chaotic", "Nostalgic", "Dreamy", "Mysterious", "Surreal", "Romantic"
        });
        moodDropdown.value = 0;

        themeDropdown.ClearOptions();
        themeDropdown.AddOptions(new List<string>
        {
            "Jungle", "Fantasy Bedroom", "Beach", "Office", "School", "Sky", "Garden"
        });
        themeDropdown.value = 0;

        colorPaletteDropdown.ClearOptions();
        colorPaletteDropdown.AddOptions(new List<string>
        {
            "Analogous", "Complementary", "Monochromatic", "Pastel", "Neon", "Triad", "Compound"
        });
        colorPaletteDropdown.value = 0;

        getSuggestionButton.onClick.AddListener(OnGetSuggestionClicked);
        regenerateButton.onClick.AddListener(OnGetSuggestionClicked);
    }

    void OnGetSuggestionClicked()
    {
        string mood = moodDropdown.options[moodDropdown.value].text;
        string theme = themeDropdown.options[themeDropdown.value].text;
        string palette = colorPaletteDropdown.options[colorPaletteDropdown.value].text;

        string prompt = GeneratePrompt(mood, theme, palette);

        chatGPT.RequestSuggestion(prompt, (response) =>
        {
            suggestionText.text = prompt;
        });
    }

    string GeneratePrompt(string mood, string theme, string palette)
    {
        string moodDesc = GetMoodDescription(mood.ToLower());
        string themeDesc = GetThemeDescription(theme.ToLower());
        string colorStyle = GetColorPaletteStyle(palette.ToLower());

        return $"A surreal 3D environment with a '{theme}' theme and a '{mood}' mood. " +
               $"{themeDesc} {moodDesc} Use {colorStyle} across skybox, lighting, and props. " +
               $"Make the environment emotionally immersive and suitable for VR.";
    }

    string GetMoodDescription(string mood)
    {
        switch (mood)
        {
            case "romantic": return "Add heart-shaped lights, roses, candles, and soft fabrics.";
            case "calm": return "Include water reflections, slow wind movement, soft shadows.";
            case "chaotic": return "Use broken props, flickering lights, glitchy materials.";
            case "nostalgic": return "Include old radios, yellowed paper, retro textures.";
            case "dreamy": return "Floating elements, sparkles, moonlight beams, cloudy fog.";
            case "mysterious": return "Use foggy atmosphere, ancient symbols, and low lighting.";
            case "surreal": return "Add floating stairs, impossible geometry, dream-like props.";
            default: return "";
        }
    }

    string GetThemeDescription(string theme)
    {
        switch (theme)
        {
            case "garden": return "Flowers, butterflies, grass, wooden benches, and ivy.";
            case "fantasy bedroom": return "Floating bed, magic lamps, curtains, starry ceiling.";
            case "jungle": return "Dense trees, glowing plants, tribal masks, ambient sounds.";
            case "sky": return "Floating islands, clouds, light rays, birds, and stars.";
            case "beach": return "Sand, shells, umbrellas, surfboards, and calm sea waves.";
            case "office": return "Desks, monitors, coffee mugs, sticky notes, quiet lighting.";
            case "school": return "Chalkboards, desks, bookshelves, lockers, posters.";
            default: return "";
        }
    }

    string GetColorPaletteStyle(string palette)
    {
        switch (palette)
        {
            case "analogous": return "a soft blend of similar tones like green, blue, and cyan";
            case "complementary": return "contrasting colors like orange and teal";
            case "monochromatic": return "shades of one dominant color";
            case "pastel": return "muted soft tones like pink, mint, and lavender";
            case "neon": return "bold glowing colors like electric blue and hot pink";
            case "triad": return "three evenly spaced hues like red, blue, and yellow";
            case "compound": return "blended base tones with rich contrasts";
            default: return "balanced neutral colors";
        }
    }
}
