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
    public Button generateImageButton;
    public Button generate3DModelButton;

    [Header("Output")]
    public TextMeshProUGUI suggestionText;
    public RawImage generatedImage;

    [Header("AI System")]
    public ChatGPTManager chatGPT;
    public ImageGenerator imageGenerator;
    public MeshyManager meshyManager;

    private string currentPrompt;

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
            "Jungle", "Fantasy Bedroom", "Beach", "Office", "School", "Sky", "Underwater"
        });
        themeDropdown.value = 0;

        colorPaletteDropdown.ClearOptions();
        colorPaletteDropdown.AddOptions(new List<string>
        {
            "Analogous", "Complementary", "Monochromatic", "Pastel", "Neon", "Triad", "Iridescent"
        });
        colorPaletteDropdown.value = 0;

        getSuggestionButton.onClick.AddListener(OnGetSuggestionClicked);
        generateImageButton.onClick.AddListener(OnGenerateImageClicked);
        generate3DModelButton.onClick.AddListener(OnGenerate3DModelClicked);
    }

    void OnGetSuggestionClicked()
    {
        string mood = moodDropdown.options[moodDropdown.value].text;
        string theme = themeDropdown.options[themeDropdown.value].text;
        string palette = colorPaletteDropdown.options[colorPaletteDropdown.value].text;

        currentPrompt = GeneratePrompt(mood, theme, palette);

        chatGPT.RequestSuggestion(currentPrompt, (response) =>
        {
            suggestionText.text = currentPrompt;
        });
    }

    void OnGenerateImageClicked()
    {
        if (!string.IsNullOrEmpty(currentPrompt))
        {
            imageGenerator.GenerateImageFromPrompt(currentPrompt, (Texture2D imageTex) =>
            {
                generatedImage.texture = imageTex;
                generatedImage.gameObject.SetActive(true);
            });
        }
    }

    void OnGenerate3DModelClicked()
    {
        if (!string.IsNullOrEmpty(currentPrompt))
        {
            meshyManager.Generate3DModel(currentPrompt);
        }
    }

    string GeneratePrompt(string mood, string theme, string palette)
    {
        string moodDesc = GetRandomFrom(moodDescriptions[mood.ToLower()]);
        string themeDesc = GetRandomFrom(themeDescriptions[theme.ToLower()]);
        string colorStyle = GetRandomFrom(colorDescriptions[palette.ToLower()]);

        return $"A surreal 3D environment with a '{theme}' theme and a '{mood}' mood. " +
               $"{themeDesc} {moodDesc} Use {colorStyle} across skybox, lighting, and props. " +
               "Make the environment emotionally immersive and suitable for VR.";
    }

    string GetRandomFrom(string[] options)
    {
        return options[Random.Range(0, options.Length)];
    }

    Dictionary<string, string[]> moodDescriptions = new Dictionary<string, string[]>
    {
        { "romantic", new string[] {
            "Add heart-shaped lights, roses, candles, and soft fabrics.",
            "Include floating hearts, red drapes, and gentle violin music.",
            "Use silky textures, dim lighting, and rose petals scattered around."
        }},
        { "calm", new string[] {
            "Include water reflections, slow wind movement, soft shadows.",
            "Use foggy skies, ambient music, and minimalistic props.",
            "Add light blue gradients, soft waves, and a slow breeze."
        }},
        { "chaotic", new string[] {
            "Use broken props, flickering lights, glitchy materials.",
            "Add visual distortion, random object placement, and loud sounds.",
            "Combine clashing colors, spinning elements, and smoke effects."
        }},
        { "nostalgic", new string[] {
            "Include old radios, yellowed paper, retro textures.",
            "Use sepia tones, vintage props, and crackling record music.",
            "Add 80s posters, old toys, and classic movie references."
        }},
        { "dreamy", new string[] {
            "Floating elements, sparkles, moonlight beams, cloudy fog.",
            "Add glowing butterflies, soft gradients, and slow animations.",
            "Use translucent textures, floating beds, and pastel skies."
        }},
        { "mysterious", new string[] {
            "Use foggy atmosphere, ancient symbols, and low lighting.",
            "Add locked doors, dim candles, and eerie soundscapes.",
            "Include hooded statues, shadows, and whispering winds."
        }},
        { "surreal", new string[] {
            "Add floating stairs, impossible geometry, dream-like props.",
            "Use non-Euclidean space, glowing textures, and sky portals.",
            "Combine melting clocks, giant eyes, and gravity-defying shapes."
        }}
    };

    Dictionary<string, string[]> themeDescriptions = new Dictionary<string, string[]>
    {
        { "underwater", new string[] {
            "Glowing coral reefs, bubbles, sun rays filtering through water.",
            "Include floating jellyfish, mysterious ruins, and shimmering fish.",
            "Use kelp forests, sandy ocean floors, and distant whale songs."
        }},
        { "fantasy bedroom", new string[] {
            "Floating bed, magic lamps, curtains, starry ceiling.",
            "Include flying books, glowing carpets, and whispering wardrobes.",
            "Use levitating crystals, soft pillows, and moonlit windows."
        }},
        { "jungle", new string[] {
            "Dense trees, glowing plants, tribal masks, ambient sounds.",
            "Include vines, mist, and colorful parrots.",
            "Use stone ruins, insect sounds, and wet leaves."
        }},
        { "sky", new string[] {
            "Floating islands, clouds, light rays, birds, and stars.",
            "Add rainbow bridges, airships, and floating windmills.",
            "Use sunrise gradients, gliding dragons, and drifting balloons."
        }},
        { "beach", new string[] {
            "Sand, shells, umbrellas, surfboards, and calm sea waves.",
            "Include hammocks, coconut trees, and gentle seagulls.",
            "Add driftwood, footprints, and pastel sunsets."
        }},
        { "office", new string[] {
            "Desks, monitors, coffee mugs, sticky notes, quiet lighting.",
            "Include whiteboards, paper stacks, and slow ticking clocks.",
            "Use keyboard sounds, desk plants, and tired glances."
        }},
        { "school", new string[] {
            "Chalkboards, desks, bookshelves, lockers, posters.",
            "Include scribbled notes, backpacks, and ringing bells.",
            "Use pencil drawings, lunchboxes, and a science lab."
        }}
    };

    Dictionary<string, string[]> colorDescriptions = new Dictionary<string, string[]>
    {
        { "analogous", new string[] {
            "a soft blend of similar tones like green, blue, and cyan",
            "cool adjacent hues such as teal, blue, and violet",
            "gentle color transitions like red, orange, and pink"
        }},
        { "complementary", new string[] {
            "contrasting colors like orange and teal",
            "opposing tones like blue and yellow",
            "bold contrasts such as red and green"
        }},
        { "monochromatic", new string[] {
            "shades of one dominant color",
            "gradient tones of a single hue",
            "varied brightness in one color family"
        }},
        { "pastel", new string[] {
            "muted soft tones like pink, mint, and lavender",
            "dreamy hues like baby blue and peach",
            "subtle tones such as lilac and pale yellow"
        }},
        { "neon", new string[] {
            "bold glowing colors like electric blue and hot pink",
            "fluorescent tones like lime green and magenta",
            "vibrant lights such as neon purple and cyan"
        }},
        { "triad", new string[] {
            "three evenly spaced hues like red, blue, and yellow",
            "triangular color harmony with green, orange, and purple",
            "balanced use of primary and secondary colors"
        }},
        { "iridescent", new string[] {
            "colors that shift with perspective like oil on water",
            "pearl-like hues with a glowing effect",
            "rainbow sheens and metallic reflections"
        }}
    };
}
