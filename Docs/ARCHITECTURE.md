# 🏗️ Hearthbound Hollow — Implementation Architecture

> **Authors:** Creative Director · Senior Unity Developer · Unity Asset Engineer · Character Animator · Critic & Review Board
> **Branch:** `feat/arabic-localization` (off `feat/mission-1-2-architecture`)
> **Status:** ⚠️ Approved with Notes — incremental phased build
> **Unity:** 6000.4.4f1 (Unity 6 LTS) · URP-Mobile target

This is the **single source of truth** for the technical implementation of Mission 1 + Mission 2. It is derived from `GAME_DESIGN.md`, the 17-codex Depth Bible, the `Mission_1_2_Focus` folder, `Asset_Analysis_Mission1-2.md`, and `ANIMATION_REQUIREMENTS.md`. Every C# script, ScriptableObject, scene, and prefab in the repo maps back to a section of this document.

---

## 0. Locked Creative Decisions

| Decision | Value | Reason |
|---|---|---|
| **Villager art** | BoZo (chibi) — A-12 | Critic Board rec; matches "cozy by candlelight" pillar; lower mobile cost |
| **Dialogue** | Yarn Spinner (free, git URL) | GDD § 9 Pillar 1: "Every line could be in a novel" — AI-gen disallowed |
| **Render Pipeline** | URP-Mobile | All Tier S/A assets URP-optimized; 60 fps on mid-range Android |
| **Player controller** | Custom `PlayerController` (Phase 26) — CharacterController-based, WASD, optional Sprint + Jump, camera-relative, Animator-aware | Character Controller Pro (A-13) is imported as reference but **not** wired — our controller is leaner + Mixamo-ready (D-036/D-037) |
| **Camera** | `SmoothFollowCamera` (Phase 26) — spring-damped follow + RMB-orbit + scroll-zoom + wall-clip; Cinemachine prefab optional via Phase 17 | Cinemachine adds a hard package dep to every gameplay scene; our camera ships cinematic-feel cozy default and both can coexist (D-039) |
| **Player Animator** | Procedurally-built `Hearthbound_Player.controller` (Phase 26) — Humanoid avatar, 1D blend tree on `Speed`, Jump/Fall/Land states | Mixamo-retargetable; BoZo's existing M_Idle/M_Walk are the fallback (D-037) |
| **Save** | JSON local + 3-rolling-slot + autosave | Mobile-safe; no cloud in M1-2 |
| **OpenAI dialogue addon** | DO NOT USE | Tagged `Reference – Do Not Use In Build` |
| **Editor entry point** (Phase 32) | **`Hearthbound → 🚀 Build Everything`** — single top-level click, chains every Phase 13 → 32 sub-builder, idempotent | **D-051** — top-level Hearthbound menu reserved for `🚀 Build Everything`, `🔍 Diagnose Build`, and the implicit `⚙️ Advanced ►` submenu (every legacy per-phase entry). |
| **Voice acting** (Phase 32 MVP) | **Open-source [Piper TTS](https://github.com/rhasspy/piper) driving `Tools/generate_voices.sh` → 77 `<char>_<id>.wav` clips (Doris 55 / Gerrold 8 / Marin 4 / Narrator 4 / Pickle 6) at 22 kHz mono PCM16. Lighter-weight `espeak-ng` fallback also chained into `🚀 Build Everything` via `Phase46_VoiceGenerator.cs`.** | **D-058** — pipeline decoupled from runtime; any 22 kHz mono PCM16 .wav drops in (ElevenLabs / XTTS / human VO) by overwriting files + rerunning `Phase32_VoiceLibraryBuilder`. **D-059** — Piper is the canonical neural-quality pipeline; espeak-ng the cross-platform in-Editor fallback. Both write to the same paths so the runtime is identical. |
| **Localization** (Phase 60) | **English source-of-truth + Arabic translation; `LocalizationService` (Core asmdef) registered before any scene Awake; JSON tables in `Assets/_Project/Localization/Resources/loc.<iso>.json` + `dialogue.<iso>.json`; runtime Arabic glyph shaping (`ArabicTextShaper`); per-locale voice clip slots (`VoiceLibrarySO.Entry.clipAr`).** | **D-060 → D-064** — translators write canonical-form text; runtime handles shaping + RTL layout; voice clips are per-locale and can be at different stages of localization (Arabic subtitles + English fallback audio is a valid intermediate state). |

---

## 1. Phased Delivery Plan

> 🚀 **User-facing entry point (Phase 32).** The user-facing entry point is **`Hearthbound → 🚀 Build Everything`**. All per-phase menu items have been moved to **`Hearthbound → ⚙️ Advanced/…`** for power users. After every `git pull`, press **🚀 Build Everything** — it is idempotent.

The build is sliced into micro-phases, each producing a buildable, mergeable, minimal-reimport delta.

| Phase | Deliverable | Status |
|---|---|---|
| **0** | Architecture + folder skeleton + asmdef graph + manifest packages | ✅ Done |
| **1** | Core systems: GameManager, EventBus, ServiceLocator, VillageState, MissionSO | ✅ |
| **2** | Memory data layer: MemoryNodeSO, MemoryConnectionSO, VillagerSO, MemoryHerb, TariffSO | ✅ |
| **3-9** | Player, mini-games, UI, Yarn Spinner, cutscenes, Save, Scenes (Bootstrap → Cottage) | ✅ |
| **10–12** | URP, shader patcher, MVP smoke-test | ✅ |
| **13–21** | Asset-driven builders (BoZo, Bamao, MeshingunStudio, AllIn1, Lumen, audio, weather, Yarn, dreams) | ✅ |
| **22-24** | Playable Mission 1 + Mission 2 capstones | ✅ |
| **25-32** | UI hotfixes, Player Controller, Animation pipeline, Onboarding, Dialogue repair, Mission 1 Polish v2 | ✅ |
| **32 (Voice Acting MVP)** | **VoiceLibrarySO + VoicePlayer + DialogueUI lineId hook + D-058** | ✅ |
| **32.6 → 32.10 (Open-source pipeline + interactive polish)** | **Piper TTS cross-platform pipeline + espeak-ng fallback + Mission 1 coverage 49 → 55 lineIds + VoiceClip events + Music/Ambient ducking + Mumble suppression + Docs/VOICE_CASTING.md + D-059** | ✅ |
| **33** | Aggregate `Diagnose Build` — chains Phase 23/26/32 sub-diagnostics | ✅ |
| **48-51** | Depth Layer — Cold Open + Echo Hologram + Preface Beat + Memory Web | ✅ |
| **60 (Arabic Localization MVP)** | **LocalizationService + ArabicTextShaper + LocalizedText + JSON tables (loc.en/ar.json + dialogue.ar.json) + VoiceLibrarySO.Entry.clipAr + Phase60_ArabicVoiceLibraryBinder + Tools/generate_voices_ar.sh + Codex 17 + D-060 → D-064** | ✅ **This PR** |

---

## 2. Project Folder Layout

```
/Assets/
├── _Project/                            <-- All studio-authored content
│   ├── Art/{Characters, Environment, Memories, UI}
│   ├── Audio/{Music, SFX, Ambience, Voice/{Doris,Gerrold,Marin,Narrator,Pickle}}   (5 char folders)
│   ├── Audio/Voice/ar/{Doris,Gerrold,Marin,Narrator,Pickle}                          (NEW Phase 60 — Arabic mirror)
│   ├── Animations/                       (Hearthbound_Player.controller + Mixamo/* subfolder)
│   ├── Fonts/Arabic/                     (NEW Phase 60 — NotoNaskhArabic-Regular.ttf installed by editor)
│   ├── Localization/Resources/           (NEW Phase 60 — loc.<iso>.json + dialogue.<iso>.json)
│   ├── Prefabs/{Player, NPCs, Memories, Props, UI, VFX}
│   ├── Resources/                        (HearthboundVoiceLibrary.asset)
│   ├── Scenes/                           (6 scenes — Bootstrap → Cottage)
│   ├── Scripts/                          (10 asmdef-isolated subsystems + Localization sub-folder)
│   ├── ScriptableObjects/{Memories, Villagers, Herbs, Missions, Tariffs, State}
│   ├── Settings/                         (HearthboundInput.inputactions)
│   ├── Yarn/
│   └── Tests/{EditMode, PlayMode}
└── ...vendor folders unchanged (BoZo, MeshingunStudio, Heat UI, Lumen, Bamao, etc.)
```

> **Why we don't relocate existing imports:** Moving them would break every `.meta` GUID reference, forcing Unity to reimport ~5 GB of textures.

---

## 3. Assembly Definitions — incremental compile graph

```
HearthboundHollow.Core
HearthboundHollow.Memory       ← Core
HearthboundHollow.Save         ← Core, Memory, Newtonsoft-Json
HearthboundHollow.Audio        ← Core
HearthboundHollow.Player       ← Core, Memory, InputSystem
HearthboundHollow.MiniGames    ← Core, Memory, Audio, InputSystem, TMP
HearthboundHollow.UI           ← Core, Memory, Audio, TMP, InputSystem
HearthboundHollow.Dialogue     ← Core, Memory, UI, TMP, [YarnSpinner if present]
HearthboundHollow.Cutscene     ← Core, Memory, UI, Timeline
HearthboundHollow.Mission      ← Core, Memory, UI, Dialogue, MiniGames, Cutscene, Save, Audio, Addressables
HearthboundHollow.Editor       ← every runtime asmdef (Editor-only, includePlatforms = ["Editor"])
HearthboundHollow.Tests.EditMode ← Core, Memory, Save, Mission, Player
```

**Phase 60 — `LocalizationService` + `ArabicTextShaper` live in Core asmdef** so every other asmdef can call them without introducing a new dependency cycle. `LocalizedText` + `RtlLayoutMirror` live in UI asmdef (TMP dependency).

---

## 4. Core Subsystems

### 4.1 Service Locator + Event Bus
Replaces traditional singletons. `ServiceLocator.Get<T>()` for service access. `EventBus.Publish<TEvent>(evt)` for decoupled communication.

**Events shipping in M1-2:**
- `MissionStartedEvent` / `MissionCompletedEvent`
- `DialogueStartedEvent` / `DialogueEndedEvent`
- `MemoryPolishedEvent(MemoryNodeSO, clarity01)`
- `MemoryCleansedEvent(MemoryNodeSO, CleanseOutcome)`
- `MoralChoiceMadeEvent(MoralChoice, MemoryNodeSO)`
- `HerbHarvestedEvent` / `TeaBrewedEvent`
- `DayEndedEvent(int dayIndex)`
- `EchoConnectionRevealedEvent(MemoryNodeSO, MemoryNodeSO)`
- **`DialogueLineStartedEvent(speaker, line, dur, HasVoiceClip)`**
- **`VoiceClipStartedEvent(lineId, clipLengthSec)` / `VoiceClipEndedEvent(lineId)`**
- **`LocaleChangedEvent(previousLocale, currentLocale)`** *(Phase 60 — every UI subscriber refreshes on the same frame)*

### 4.2 VillageState (the global game state)
A single ScriptableObject with the **full 14-dimension struct** from Codex 08, even though only 4 dimensions are written in M1-2.

### 4.3 GameManager (bootstrap + scene management)
- Loads VillageState from disk (via SaveService) or creates default
- Registers services to ServiceLocator
- Owns the `DontDestroyOnLoad` root

### 4.4 PlayerController (Phase 26 surface)

Animator parameter contract: `Speed`, `MoveX`, `MoveY`, `VelocityY`, `IsGrounded`, `IsSprinting`, `Jump`. Sprint + Jump are runtime-gated by `SettingsService.GentleMode`.

### 4.5 SmoothFollowCamera (Phase 26)

SmoothDamp follow + spherical orbit + sphere-cast wall-clip. Cinemachine-agnostic.

### 4.6 Audio subsystem (`HearthboundHollow.Audio`)

The Audio asmdef hosts every runtime audio component. None of them reference UI; communication is via `EventBus` (dialogue events) or direct `ServiceLocator.Get<>()`.

| Component | File | Role |
|---|---|---|
| `MusicLibrarySO` / `MusicPlayer` | Audio/MusicPlayer.cs | Procedural music cues. Ducks to 55% on `VoiceClipStartedEvent`. |
| `AmbientAudio` / `SfxLibrarySO` / `SfxPlayer` | Audio/AmbientAudio.cs | Per-scene ambience. Ducks to 75% on voice events. |
| `MumbleVoiceLibrarySO` / `MumbleVoicePlayer` | Audio/MumbleVoice*.cs | Syllable-pad VO synced to typewriter. Suppresses on `HasVoiceClip == true`. |
| **`VoiceLibrarySO` / `VoicePlayer`** | **Audio/Voice*.cs** | **Real per-line voice clips looked up by stable `lineId`. Phase 60 — `Entry.clipAr` for Arabic clips; `Play(lineId)` reads `LocalizationService.CurrentLocale` and picks `clipAr` on Arabic locale with English fallback (D-064).** |

### 4.7 Localization subsystem (Phase 60 — `HearthboundHollow.Core`)

The Localization subsystem is a NEW set of files under `Assets/_Project/Scripts/Core/Localization/`:

| Component | File | Role |
|---|---|---|
| `Locale` enum + `LocaleInfo` | Core/Localization/Locale.cs | English / Arabic enum + ISO code / native-name / RTL flag / voice folder helpers. |
| `LocalizationService` | Core/Localization/LocalizationService.cs | Central UI string + dialogue line lookup. JSON tables auto-loaded from Resources. PlayerPrefs persistence + system-locale auto-detect. Publishes `LocaleChangedEvent` on SetLocale (D-060, D-061). |
| `LocalizationBootstrap` | Core/Localization/LocalizationBootstrap.cs | `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` belt-and-braces installer guaranteeing the service is up before any scene Awake (D-061). |
| `ArabicTextShaper` | Core/Localization/ArabicTextShaper.cs | BiDi-lite + Arabic glyph-joining + LAM/ALEF ligatures. Walks input, substitutes presentation forms (U+FE80 block), reverses for RTL paragraph flow. Mixed strings preserved (Latin LTR + Arabic RTL). (D-063) |
| `LocaleChangedEvent` | Core/Localization/LocaleChangedEvent.cs | EventBus payload with `PreviousLocale` / `CurrentLocale` / `RtlDirectionChanged`. |
| `LocalizedText` | UI/Localization/LocalizedText.cs | TMP-aware drop-on component with inspector `key` field + live refresh on `LocaleChangedEvent` + Arabic shaping + alignment mirror (D-062). |
| `RtlLayoutMirror` | UI/Localization/RtlLayoutMirror.cs | Flips `HorizontalLayoutGroup.reverseArrangement` + mirrors anchored X for side-pinned panels on RTL. |

**Translation tables** (D-060):
- `Assets/_Project/Localization/Resources/loc.en.json` — 197 UI keys (source of truth).
- `Assets/_Project/Localization/Resources/loc.ar.json` — 197 keys in Modern Standard Arabic.
- `Assets/_Project/Localization/Resources/dialogue.ar.json` — 84 dialogue lineIds.
- `Assets/_Project/Localization/Resources/dialogue.en.json` — sentinel (English source-of-truth canon lives in Director scripts).

**Asmdef graph:** the Core asmdef gains no new dependencies. UI asmdef already references Core, so `LocalizedText` + `RtlLayoutMirror` consume the service. Audio asmdef references Core, so `VoicePlayer` can read `LocalizationService.CurrentLocale`.

---

## 5. Memory Data Model (Codex 02 § 2.1)

Every Memory is a `MemoryNodeSO`. The Echo Web `MemoryConnectionSO` schema exists from Day 1 — only one connection is revealed in M1-2.

---

## 6. Mini-Game Architecture

### 6.1 Polish (Mission 1)
- States: `Idle → Engaging → Polishing → MidwayMilestone → RevealSwell → Complete`

### 6.2 Cleanse (Mission 2)
- States: `Idle → ThreadEngaged → SealingCrack → CrackComplete → AwaitingNextCrack → Complete | CrossedCore`
- 4 outcomes: `Perfect | Acceptable | Sloppy | CrossedCore`

---

## 7. Dialogue Architecture (Yarn Spinner)

- `YarnVillageStateBridge` exposes village-state variables to Yarn.
- `DialogueUI.PresentLine(speaker, text, portrait, lineId)` accepts an optional stable `lineId`.
- **Phase 60:** Before rendering, `PresentLine` runs the speaker through `LocalizationService.GetSpeakerName` ("Doris" → "دوريس"), the line text through `GetDialogue(lineId, englishOriginal)`, then through `ArabicTextShaper.Shape` if the locale is RTL. The `VoicePlayer.Play(lineId)` call is locale-aware on the audio side (Arabic `clipAr` → English `clip` fallback).

---

## 8. UI Architecture (Heat + Bamao)

Every M1-2 UI surface routes its labels through `LocalizationService`:

| Panel | Source | Phase 60 wiring |
|---|---|---|
| Main Menu | Heat | `MainMenuController.ApplyLocalization()` on Awake + `LocaleChangedEvent` |
| Pause Menu | Heat | `PauseMenuUI.ApplyLocalization()` — title + hint + 4 buttons |
| Help Overlay (H) | Heat | `HelpOverlayUI.BuildBody()` reads `help.row.*` keys |
| Onboarding Overlay | Custom | Each `Step` carries `locKey*` triplet; `ApplyStep()` resolves via service |
| Tone Compass | Custom | `tone_compass.body` + Continue button + Gentle Mode label localized |
| Comfort Tools | Heat | **Two-button language picker** (English / العربية) + 10 localized section labels + subtitle-size tier |
| Evening Ledger | Bamao | Day title + coin balance + summary + bullet list all localized + RTL alignment |
| HUD | Custom | Day + coin labels via `hud.day_label_fmt` / `hud.coin_label_fmt` |
| Mission Title Card | Custom | Day label + title + tone localized (MissionSO `displayName` as loc key) |
| Choice Card | Bamao | Prompt + memory title + tile labels + cost preview localized + RTL alignment |
| Codex Tooltip / Memory Map | Bamao | Tooltip text + node labels localized + shaped on RTL |
| Control Hints HUD (Mission asmdef) | Custom | Chip captions (🚶 Move / ✋ Interact / ❓ Help) localized; interactable PromptText routed through HasKey + RTL shape |
| Dialogue UI | Bamao | The main runtime entry — `PresentLine` routes speaker + text + choices + advance prompt through service + shaper |
| Marin's Note (Mission asmdef) | Custom | Per-passage lineIds + localized prompts + speaker name routed through GetSpeakerName |

---

## 9. Save Architecture

- **Autosave** + **Manual** (3 rolling slots).
- Payload: `VillageStateSnapshot` (JSON).
- Path: `Application.persistentDataPath/saves/`.
- **Atomic write**: tmp file → fsync → rename.
- Phase 60 — **Locale is persisted via PlayerPrefs `hh.locale.iso`** (independent of save slot) so the same player gets the same language across all save files.

---

## 10. Mobile Performance Discipline

- URP Mobile RP: 1 directional, MSAA 2×, render scale 0.85, soft shadows OFF by default.
- Texture compression: ASTC 6×6 mobile, ETC2 fallback.
- **Voice clips (per-language):** 22 kHz mono PCM16 .wav, ~30–40 MB per locale (EN + AR = ~70 MB). Loaded with default `Decompress on Load`.
- **Localization tables (per-locale):** ~20 KB compressed JSON each, loaded on demand via `Resources.Load<TextAsset>`. Phase 60 budget: <100 KB.

---

## 11. Testing Strategy

- EditMode (NUnit): VillageState math, PlayerController surface, RippleEngine propagation, YarnBridge round-trip, **15 Phase 60 LocalizationTests (Locale ISO round-trip, JSON parser, ArabicTextShaper)**.
- PlayMode: Polish completion, Cleanse outcome state machine.

---

## 12. Risk Mitigations (Krieg's Register)

| Risk | Mitigation in this architecture |
|---|---|
| Save corruption | Atomic write + fsync + rename; 3 rolling slots |
| Scaling rework | All 14 VillageState dimensions + Echo Web schema present at default values |
| **Controller perception** | **Sprint + Jump available but off in Gentle Mode (D-036)** |
| **Mixamo unavailable** | **Phase 26 falls back to BoZo's existing 2 anims** |
| **Voice provider lock-in** (Phase 32 MVP) | **D-058 — generation pipeline decoupled from runtime.** |
| **Cross-platform TTS** (Phase 32.6) | **D-059 — Piper TTS bash + espeak-ng in-Editor fallback.** |
| **Localization debt** (Phase 60) | **D-060: English source-of-truth + missing-key fallback — game NEVER ships with an empty label.** |
| **Voice + subtitle drift** (Phase 60) | **D-064: clip and clipAr per Entry; voice falls back to English audio while subtitle stays Arabic — graceful degrade lets the team ship Arabic on day-one and iterate on voice quality over time.** |

---

## 13. Out-Of-Scope Wall (Krieg Discipline)

**Nothing below this line ships in this branch:**
- Weave / Sever / Listen / Read / Translate / Identify mini-games beyond M1-2
- Procedural villagers (only Doris + Gerrold + 1 silent lane villager)
- Predecessor (Marin) full arc — only Marin's signed notes exist
- Echo Hologram beyond M1 / Locked Room / Forgotten Year / Vance Arc / Mariska / Memory Bees
- Letter-Bird Network, Pen-Pal Villages, Dream Cinema community
- Hollow upgrades beyond Level 1 · Herbs beyond Lavender + Valerian
- Mobile IAP, gacha, energy, lootboxes (NEVER)
- **Phase 61 (UAX #9 full BiDi) — current `ArabicTextShaper` covers cozy single-line dialogue; multi-paragraph letters might need extension.**
- **Phase 62 (Persian + Urdu locales) — reuses the same shaper with extended `LetterForms` rows.**
- **Phase 63 (French / Spanish / Japanese) — pure JSON table + Piper voice model; no shaper needed.**

The codices 02–17 in `/Docs/Depth_Bible/` remain the **Scaling Reference**.

---

## 14. How This Doc Stays Honest

Every PR to this branch updates `Docs/PROGRESS.md` with phase + sub-task completed + exact files added/modified + open follow-ups + any deviation from this Architecture doc, with rationale.

**No code lands without a PROGRESS.md update.**

---

## 15. Decisions Index (cross-ref → PROGRESS.md)

D-001 → D-064 are catalogued in `Docs/PROGRESS.md`. Newest:

- **D-033** *(Phase 25 hotfix)* Procedural UI builders MUST use the two-layer pattern.
- **D-034** *(Phase 25 hotfix)* UI overlay scripts MUST self-heal in `Show()`.
- **D-035** *(Phase 26.1 hotfix)* Asmdef-locality check.
- **D-036 → D-040** *(Phase 26 Player Controller + Animation)* Sprint + Jump opt-in flags; 1D blend tree; configurable Animator parameter names; SmoothFollowCamera default; Mixamo folder.
- **D-041** *(Phase 27.1 / Phase 28)* Mesh-bottom from world-space `Renderer.bounds`.
- **D-042** *(Phase 29a)* Every TMP label MUST go through `UIAutoFitText`.
- **D-043** *(Phase 30)* Onboarding is per-save, not per-play.
- **D-044** *(Phase 30)* Context-aware HUD chips live in the Mission asmdef.
- **D-045 → D-048** *(Phase 31)* Dialogue VLG + LayoutElement + lineText-hide + number-key shortcut contracts.
- **D-049** *(Phase 31.1)* Blocking dialogue UI must expose a visible advance affordance.
- **D-050** *(Phase 31.1)* Cutscene overlays hidden-by-default.
- **D-051** *(Phase 32 UX track — Menu collapse)* Every editor action MUST register under `Hearthbound/⚙️ Advanced/…` unless explicitly promoted to top level.
- **D-052 / D-053 / D-054** *(Phase 39 — Audio + Cutscene policy)*.
- **D-055 / D-056** *(Phase 44 — Save-resume + install-pattern policy)*.
- **D-057** *(Phase 45 — Audio self-heal)*.
- **D-058** *(Phase 32 — Voice Acting MVP)* **Voice clips live under `Audio/Voice/{character}/{lineId}.wav`; pipeline decoupled from runtime; any 22 kHz mono PCM16 .wav drops in.**
- **D-059** *(Phase 32.6 → 32.10)* **Piper TTS is the canonical TTS pipeline; espeak-ng in-Editor fallback.**
- **D-060** *(Phase 60 — Arabic Localization MVP)* **English authored text is the source of truth. Every other locale is a translation against the source — held in `Assets/_Project/Localization/Resources/loc.<iso>.json` (UI strings) and `dialogue.<iso>.json` (dialogue lines). Missing translation → English fallback + one-time warning. The `LocalizationService` (Core asmdef) is the canonical lookup surface; `LocaleChangedEvent` (EventBus) is the notification.**
- **D-061** *(Phase 60)* **`LocalizationService` MUST be registered with the `ServiceLocator` before any scene's `Awake()` runs. Two installers (`SettingsServiceBootstrap` early-Awake + `LocalizationBootstrap.RuntimeInitializeOnLoad`) guarantee this for both production scenes and stand-alone EditMode tests.**
- **D-062** *(Phase 60)* **TMP labels that need locale-aware text MUST either use the `LocalizedText` component OR subscribe to `LocaleChangedEvent` and call `LocalizationService.Get` themselves. Hard-coded English in `.text = "…"` is a bug.**
- **D-063** *(Phase 60)* **Arabic glyph shaping (`ArabicTextShaper.Shape`) is the runtime's responsibility, not the translator's. The `loc.ar.json` file contains canonical-form (logical-order) Arabic. The shaper substitutes contextual presentation forms (U+FE80 block), combines lam-alef ligatures, and reverses for RTL paragraph flow before handing the string to TMP.**
- **D-064** *(Phase 60)* **Voice clips are language-tagged: English under `Audio/Voice/<Character>/`, Arabic under `Audio/Voice/ar/<Character>/`. `VoiceLibrarySO.Entry` has both `clip` (English) and `clipAr` (Arabic) fields. `VoicePlayer.Play(lineId)` selects per current locale; subtitle is ALWAYS translated by DialogueUI so voice + subtitle can be in different stages of localization (e.g. day-one Arabic ships subtitles + English audio + Arabic Piper baseline that an actor replaces later — D-058's file-swap policy applied per-locale).**

---

*Document version 1.7 — Phase 60 Arabic Localization MVP + D-060 → D-064.*
