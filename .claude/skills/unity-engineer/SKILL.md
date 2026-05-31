---
name: unity-engineer
description: Engineering work on Hearthbound Hollow — C# gameplay systems, the asmdef compile graph, vendored-asset integration, scenes/prefabs, the Editor build pipeline, save, and mobile performance. Use for any code change, compile error, builder script, or perf concern in this Unity 6 / URP-Mobile project.
---

# Unity Engineer — code & build discipline

Owned by **Senior Unity Developer** + **Unity Asset Engineer** + **Tech Animator** + **Audio Engineer**. Source of truth: `Docs/ARCHITECTURE.md`.

## Project facts
- Unity **6000.4.4f1 (Unity 6 LTS)**, **URP-Mobile**. Target 60 fps mid-range Android / Switch handheld.
- Studio code: `Assets/_Project/Scripts/` (161 scripts, 11 asmdefs). Vendored packs elsewhere under `Assets/` — never relocate (breaks `.meta` GUIDs → multi-GB reimport).
- Architecture: **ServiceLocator** (`ServiceLocator.Get<T>()`, NOT `.Resolve`) + **EventBus** (`EventBus.Publish<T>(evt)`) instead of singletons. `GameManager` owns the `DontDestroyOnLoad` root and bootstraps services.

## asmdef graph (do not create cycles)
```
Core → (none)
Memory → Core
Save → Core, Memory, Newtonsoft-Json
Audio → Core
Player → Core, Memory, InputSystem
MiniGames → Core, Memory, Audio, InputSystem, TMP
UI → Core, Memory, Audio, TMP, InputSystem
Dialogue → Core, Memory, UI, TMP, [YarnSpinner]
Cutscene → Core, Memory, UI, Timeline
Mission → Core, Memory, UI, Dialogue, MiniGames, Cutscene, Save, Audio, Addressables
Editor → every runtime asmdef (Editor-only)
```
**D-035:** every new script lives in an asmdef that declares the deps it uses. **Audio must never reference UI** (use EventBus + reflection — see GameManager's VoicePlayer spawn).

## Build pipeline
- User entry: **`Hearthbound → 🚀 Build Everything`** (idempotent; chains Phase 13→32 builders; load-or-create + heal-then-save) and **`🔍 Diagnose Build`** (read-only Phase 33 aggregate).
- **D-051:** new Editor menu items go under `Hearthbound → ⚙️ Advanced/…` unless Critic-promoted.
- Builders rebuild scenes 00–05 by design → **put tunable values on prefabs, not scenes.**

## Conventions
- Match existing patterns; events for cross-system comms; pool hot objects (`MemoryOrbPool`, `VfxPool`, `SfxPool`).
- Audio components that depend on a ScriptableObject library MUST `Resources.Load` self-heal in `Awake()` and log a remediation step if that fails (**D-057**).
- Player ground truth = live world-space `Renderer.bounds` via `PlayerGroundClamp`, never padded `localBounds` (**D-041**).
- Every TMP label a builder creates goes through `UIAutoFitText` (**D-042**); procedural UI uses the two-layer pattern and self-heals in `Show()` (**D-033/D-034**).

## Performance gates
- 1 directional light, MSAA 2×, render scale 0.85, soft shadows off by default. Lightmap Fusion 4 baked sets (morning/afternoon/evening/night).
- Single orb master material + GPU instancing. NPCs `CullCompletely` in the lane.
- Voice: 22 kHz mono PCM16 wav; switch to Streaming if the library exceeds 50 MB.
- Every Phase 4+ change profiles ≤16 ms on the mid-range Android proxy.

## Definition of done
1. Compiles green (no new CS errors); asmdef refs declared.
2. Idempotent if it's a builder; no destructive scene edits to tunable data.
3. EditMode/PlayMode tests still pass; add coverage for new public surface.
4. Cozy Contract intact (route player-facing changes through `cozy-review`).
5. `Docs/PROGRESS.md` updated; new architectural call gets a `D-0xx` entry in ARCHITECTURE.md.
