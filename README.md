# WhisperScapeXR
WhisperScape VR is an emotionally immersive XR experience where players shape dreamlike environments through a hybrid prompt system.

# Part 1: Scoping
## Idea
**WhisperScape VR** is an emotionally immersive XR experience where players shape dreamlike environments through a hybrid AI prompt system.

Players begin by selecting from categorized emotional tags:
- **Theme** (Dreamlike, Mysterious…)
- **Mood** (Calm, Nostalgic, Chaotic…)
- **Environment Type** (Indoor / Outdoor)
- **Color Palette** (Analogous, Monochromatic…)

Then they either:
- 💬 Speak a custom imaginative prompt
- 🤖 Or select an AI-suggested prompt generated from the chosen tags

Using **ChatGPT** for prompt logic and **Meshy.ai** for runtime 3D generation, scenes are built with interactable elements, emotional skyboxes, and ambient visuals. The user navigates in VR using hand tracking and XR locomotion.

Over time, **ML-Agents** learn emotional preferences (e.g., color, tone) and suggest fitting prompts, like a “Pinterest for emotions.”

**Sample Prompts:**

- "A nostalgic dream under a pink sky."
- "A chaotic jungle glowing in neon green."
- "A peaceful fantasy bedroom bathed in soft blue and violet."
- "A mysterious underwater at twilight with flickering candles."
- "A calm, open beach with pastel skies and drifting leaves."
- "A vibrant forest filled with golden light and curious animals."

These prompts combine emotion, palette, environment, and atmosphere to generate immersive, dreamlike scenes.

### Why It’s a Good Idea

It gives players a creative space to process and express emotions through immersive worldbuilding. The blend of AI-driven personalization with emotional design opens opportunities for both **entertainment and mental wellness**.

### Why XR Fits Perfectly

XR allows players to engage **visually, physically, and emotionally** with their environments. Full-body movement, voice interaction, and hand-tracked manipulation make the experience intimate, responsive, and unforgettable.

## Features

Core Features (Completed): 

- UI with **dropdowns** for theme, mood, environment type, and palette
- **Prompt system** that creates structured phrases based on tags
- **ChatGPT** integrated for flexible, creative prompt logic
- **Meshy AI** runtime 3D model generation using prompt data
- **GLB model loading** with fallback cube and error handling
- Tag-based **prefab creation** and dynamic scene setup in Unity
- Full VR interaction with **XR Origin Hands** and smooth locomotion

**Stretch goals:** 

These are features that are nice to have, and that you might implement if you have extra time, or in the future.

- **🧭 AI Companion**: An emotional voice agent that helps the player navigate, reflect, and stay grounded.
- **📖 Mini Emotional Interactions**: Light, story-like tasks (e.g., hug a plush, make tea, write in a floating dream journal).
- **🗂️ Emotional Memory Archive**: Save and revisit your favorite emotional dreamscapes.
- **🧠 ML-Agent Prompt Learning**: Learns user behavior and mood patterns to suggest fitting prompts like Pinterest for emotions.
- **🎧 AI Sound & Music Generation**: Dynamic emotional soundscapes and ambient music generation.
- **🗣️ Full TTS Emotional Feedback**: AI reads back scenes with emotionally aligned tones using voice feedback.
- **🖼️ Image + GLB Saving**: Save snapshots and 3D scene data of the generated environment.
- **🌀 Procedural 3D Portals**: Walk through portals to explore layered emotional worlds and alternate moods.
- **🔁 Layered Emotional Exploration**: Navigate through multiple emotional states within a single scene experience.

## Tech Stack

Here you list the different SDKs that you used to build your prototype.

- [Unity 6.0](https://unity.com/) LTS
- **XR Interaction Toolkit** + OpenXR + Meta Quest SDK
- [OpenAI API (ChatGPT/DALL·E)](https://openai.com/api/)
- [Google Gemini API](https://ai.google.dev/)
- https://www.meshy.ai/discover
- https://www.sloyd.ai/
- https://developer.nvidia.com/omniverse?sortBy=developer_learning_library%2Fsort%2Ffeatured_in.omniverse%3Adesc%2Ctitle%3Aasc&hitsPerPage=6
- https://lab.rosebud.ai/skyboxes
- [RunwayML (image and style generation)](https://runwayml.com/)
- [Rodin (3D model generation)](https://www.3daistudio.com/Dashboard)
- [Hyper3D.ai (prompt-based 3D environment)](https://hyper3d.ai/)
- [Unity ML-Agents Toolkit](https://github.com/Unity-Technologies/ml-agents)
- [Unity Muse (AI-assisted development)](https://unity.com/products/muse)
- [Adobe Color](https://color.adobe.com/create/color-wheel) (for palette logic)
- [ColorHunt](https://colorhunt.co/) (palette inspiration)
- Kaedim3D integration: https://www.kaedim3d.com/integrate,
- RunwayML API: https://docs.dev.runwayml.com/api/#tag/Start-generating/paths/~1v1~1text_to_image/post
- Google’s TTS/STT APIs or others ([examples of incorporating REST APIs in Unity](https://github.com/lauramurinova/LinguaSpace))
- Unity TTS Plugin - Meta Voice SDK - Google Cloud TTS API
- Google Speech-to-Text and Text-to-Speech: https://cloud.google.com/speech-to-text?hl=en & https://cloud.google.com/text-to-speech?hl=en
- https://assetstore.unity.com/packages/2d/textures-materials/sky/3-skyboxes-25142

## Resources/Inspiration (optional)

You can add screenshots, videos, links, etc to the resources that inspired you for this idea, or that you want to use as a reference.

- [Dream Scene Example](https://www.youtube.com/watch?v=5n_FWXuiF30)
- [Surreal Mood Prompt](https://www.youtube.com/shorts/0OClnBIVziw)
- [ChatGPT XR Integration](https://www.youtube.com/watch?v=D4WZYwj6jfE)
- [Immersive World Building](https://www.youtube.com/watch?v=T1tP39zSLf4)
- [Emotional AI Environments](https://www.youtube.com/watch?v=77kHRuJCb_c)
- [Soft Emotional Feedback](https://www.youtube.com/shorts/JophEuqiY8E)
- [Pinterest-Like Prompt Learning](https://www.youtube.com/watch?v=t9zzcRsf0IA)
- [XR Dream Worlds Playlist](https://www.youtube.com/watch?v=21OUsBrLMS8)
- [Rodin Unity Asset Generation](https://deemos.gumroad.com/l/Rodin_Demo)
- [Spatial Toolkit (UI inspiration)](https://toolkit.spatial.io/)

# Part 2: Prototype

**(This should be completed every Thursday at 8:00 am PT, so you’re ready to PITCH!)**

We provide you with a **Kanban Board** template that you can use to organize your tasks for the sprint.

[**WhisperScape XR (AI + ML-Agent)-** Kanban Board](https://www.notion.so/WhisperScape-XR-AI-ML-Agent-Kanban-Board-1f30095e34d8811c8f65ce91650075a6?pvs=21)

## Pitch Presentation

The most important part: your pitch Slide that is gonna be your support material every Thursday when you present your prototype to the rest of us.

[‘’Prototype template link”](https://docs.google.com/presentation/d/1LsEf7hn1IQdoLrYC37tWsQS3RmQxMj75_3sOZfT_LSI/edit?usp=sharing)

## Video(s)/Material(s)

**One clear video, no longer than 1:30** of your prototype.

Video files should be added to your Google Drive student folder and hyperlinked here:

‘’[Student Folder](https://drive.google.com/drive/folders/1DpqpQhZ-8qEUeOk7zoXvyyg3w5n1P2eK?usp=sharing)’’

## Key Learning Points

What did you learn this week? Did your prototype prove the idea right (or wrong)?
What would you have done differently?

- Learned how to **integrate AI prompt systems** into an XR scene and dynamically generate models.
- Understood how **emotional themes** impact perception in immersive environments.
- Built a **flexible UI + prompt system** that scales well for AI use.
- Explored **personalized scene generation** based on user-selected moods and palettes.
- Realized the importance of **reliable feedback and fallback systems** in AI pipelines.
