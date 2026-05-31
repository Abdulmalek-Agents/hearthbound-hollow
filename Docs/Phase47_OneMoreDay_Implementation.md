# 🌙 Phase 47 — "One More Day" Goodnight Beat — Editor Implementation Guide

> **Feature:** A warm, curiosity-planting card shown at the end of each day, after the Evening Ledger, that whispers what tomorrow holds. The engine of cozy "one more day" retention — Stardew's sleep-transition, at our register.
>
> **Owners:** Engagement Designer (lead) · Esme Cordray (prose) · Mochi Tannenbaum (Pickle) · Sven Aleko (night beat) · Pell Doyne (cozy gate) · Senior Unity Dev + Asset Engineer.
>
> **Status:** ✅ **IMPLEMENTED (Tier 1) — 2026-05-29** on `feat/mission-1-2-architecture`. The 4 scripts, 3 director/runner edits, Yarn nodes, 2 SO assets, and the idempotent builder all ship; run `Hearthbound → 🚀 Build Everything` (Phase 47 OMD is the final step) to generate the prefab + scene wiring. See `STUDIO_LOG.md` / `Docs/PROGRESS_Phase47_OneMoreDay.md` for the build notes and the 4 intentional deviations from this guide (day-index fix, `refusedDorisOrb` branch flag, M2 `playDream:false`, decision renumbered **D-060 → D-064**).
> **Branch:** `feat/mission-1-2-architecture` · **Unity:** 6000.4.4f1 / URP-Mobile.

---

## 0. Design intent (why this exists)

The day currently ends on a *bookkeeping* beat (save slots → confirm → cut to next scene). There is no warm, forward-looking moment. That moment is the entire "one more day" hook: it makes the player press **continue** *wanting* tomorrow.

**Night order (locked):** `Evening Ledger → Dream (if any) → Goodnight Card → next scene`. The dream is tonight's memory; the card is the last warm word before morning.

**Cozy Contract compliance:** no numbers, no fail state, fully skippable, refusal path gets its own goodnight, gentle fade only (no shake/flash), Gentle-Mode safe. ✅

---

## 1. What ships in Tier 1

| Kind | Item |
|---|---|
| **New script** | `OneMoreDayCard.cs` (UI asmdef) — the parchment overlay |
| **New script** | `TomorrowTeaseSO.cs` (Mission asmdef) — per-day data |
| **New script** | `EndOfDaySequencer.cs` (Mission asmdef) — owns Ledger→Dream→Card→load |
| **New script** | `Phase47_OneMoreDayBuilder.cs` (Editor asmdef) — idempotent builder |
| **Edit** | `Mission01Director.cs` / `Mission02Director.cs` / `MissionRunner.cs` — delegate transition (opt-in) |
| **Edit** | `EveningLedger.yarn` + `Pickle.yarn` — goodnight prose |
| **Asset** | `Tomorrow_M1_Day1.asset`, `Tomorrow_M1_Day1_Refused` (one asset, branch field), `Tomorrow_M2_Day2.asset` |
| **Prefab** | `OneMoreDayCard.prefab` under `Assets/_Project/Prefabs/UI/` |
| **Docs** | `Docs/PROGRESS.md` entry + `Docs/ARCHITECTURE.md` `D-060` |

**Zero-regression guarantee:** every runtime edit is guarded by `if (endOfDaySequencer != null)`. With the sequencer unwired, the day-end runs the exact code path it runs today.

---

## 2. New script — `OneMoreDayCard.cs`

**Path:** `Assets/_Project/Scripts/UI/OneMoreDayCard.cs` (assembly: `HearthboundHollow.UI`)

The card is purely presentational — it takes already-resolved strings, so it needs **no** reference to the Mission asmdef (no asmdef cycle). Mirrors `EveningLedgerUI`'s self-heal + readability patterns.

```csharp
// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / OneMoreDayCard
//
// Phase 47 — the "One More Day" goodnight beat. Shown after the Evening
// Ledger (and after the night's Dream, if any) and before the next scene
// loads. A warm forward-look line + an optional Pickle sign-off + a single
// "Goodnight" button. No numbers, no fail state, fully skippable.
//
// Presentational only: the EndOfDaySequencer resolves the prose strings and
// passes them in, so this script takes no dependency on the Mission asmdef.

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HearthboundHollow.UI
{
    public class OneMoreDayCard : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;
        public CanvasGroup canvasGroup;     // for the soft fade-in

        [Header("Labels")]
        public TextMeshProUGUI headlineLabel;
        public TextMeshProUGUI forwardLookLabel;
        public TextMeshProUGUI pickleLabel;

        [Header("Continue")]
        public Button goodnightButton;

        [Header("Feel")]
        [Range(0.1f, 1.5f)] public float fadeInSeconds = 0.6f;
        public string headlineText = "Tomorrow";

        /// <summary>Raised when the player presses Goodnight. The sequencer
        /// awaits this before running the transition.</summary>
        public event Action OnContinue;

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);

            if (goodnightButton != null)
                goodnightButton.onClick.AddListener(() => { Hide(); OnContinue?.Invoke(); });

            // High-contrast typography to match the Evening Ledger parchment.
            UIReadabilityHelper.ApplyHeadline(headlineLabel, min: 48, max: 96);
            UIReadabilityHelper.ApplyBody(forwardLookLabel, min: 26, max: 38);
            UIReadabilityHelper.ApplyBody(pickleLabel, min: 22, max: 32);

            if (forwardLookLabel != null)
                UIReadabilityHelper.AddDarkWash(forwardLookLabel.rectTransform, padding: 18f);
            if (pickleLabel != null)
                UIReadabilityHelper.AddDarkWash(pickleLabel.rectTransform, padding: 14f);
        }

        /// <param name="forwardLook">Resolved forward-look prose (Cordray).</param>
        /// <param name="pickleSignOff">Optional Pickle goodnight; empty hides the line.</param>
        public void Show(string forwardLook, string pickleSignOff)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);   // self-heal
            if (root != null) root.SetActive(true);

            if (headlineLabel != null) headlineLabel.text = headlineText;
            if (forwardLookLabel != null) forwardLookLabel.text = forwardLook;

            if (pickleLabel != null)
            {
                bool has = !string.IsNullOrWhiteSpace(pickleSignOff);
                pickleLabel.gameObject.SetActive(has);
                if (has) pickleLabel.text = $"<i>{pickleSignOff}</i>";
            }

            if (canvasGroup != null) StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;
            canvasGroup.alpha = 0f;
            while (t < fadeInSeconds)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInSeconds);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            if (root != null && root != gameObject) root.SetActive(false);
        }
    }
}
```

---

## 3. New script — `TomorrowTeaseSO.cs`

**Path:** `Assets/_Project/Scripts/Mission/TomorrowTeaseSO.cs` (assembly: `HearthboundHollow.Mission`)

Prose lives here as `[TextArea]` text whose **canonical source is the Yarn file** (the same mirroring convention the directors already use for ledger prose — the runtime Yarn-dispatcher is a separate future pass per `PLAYTEST_AUDIT.md`). `sourceNode` records the Yarn node for traceability.

```csharp
// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / TomorrowTeaseSO
//
// Phase 47 — data for one day-end's "One More Day" goodnight card.
// Prose is mirrored from EveningLedger.yarn / Pickle.yarn (sourceNode names
// recorded for traceability). Tier 2 fields are present but unused until the
// Tier 2 visual-anticipation pass.

using UnityEngine;

namespace HearthboundHollow.Mission
{
    [CreateAssetMenu(menuName = "Hearthbound/Tomorrow Tease", fileName = "TomorrowTease")]
    public class TomorrowTeaseSO : ScriptableObject
    {
        [Tooltip("Fires after this day-end (matches VillageState.currentDayIndex).")]
        public int afterDayIndex = 1;

        [Header("Forward-look — canonical source: EveningLedger.yarn")]
        public string sourceNode = "Tomorrow_M1_Day1";
        [TextArea(2, 5)] public string forwardLookText;

        [Header("Branch variant (optional)")]
        [Tooltip("VillageState bool field name. When that field is true, the Alt text is used. Empty = always main.")]
        public string branchFlagField = "";
        public string sourceNodeAlt = "";
        [TextArea(2, 5)] public string forwardLookTextAlt;

        [Header("Pickle goodnight (optional, rendered italic)")]
        public string pickleSourceNode = "";
        [TextArea(1, 4)] public string pickleSignOffText;

        [Header("Tier 2 (unused until Tier 2)")]
        public Sprite visitorSilhouette;
        public bool showEchoThreadGlimmer;
    }
}
```

---

## 4. New script — `EndOfDaySequencer.cs`

**Path:** `Assets/_Project/Scripts/Mission/EndOfDaySequencer.cs` (assembly: `HearthboundHollow.Mission`)

Owns the night chain so the dream and card don't race the scene load (today `DreamHook` and the director both fire on the same event — this replaces that with an ordered sequence whenever the sequencer is present).

```csharp
// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / EndOfDaySequencer
//
// Phase 47 — single owner of the night chain:
//   Evening Ledger confirm  ->  Dream (if any)  ->  Goodnight Card  ->  transition
//
// Opt-in: a director only delegates here if this component is wired. When it
// is, the director does NOT also subscribe to the ledger, and DreamHook is
// left dormant (its ledger ref cleared by Phase47 builder) so dreams aren't
// double-played. With no sequencer wired, day-end behaves exactly as before.

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.UI;
using HearthboundHollow.Cutscene;

namespace HearthboundHollow.Mission
{
    public class EndOfDaySequencer : MonoBehaviour
    {
        [Header("References")]
        public OneMoreDayCard card;
        public MemoryDreamSequencer dream;

        [Header("Teases (one per day-end)")]
        public TomorrowTeaseSO[] teases;

        /// <summary>
        /// Run the night sequence, then invoke <paramref name="onComplete"/>
        /// (the director's existing EndDay()+LoadScene body).
        /// </summary>
        /// <param name="playDream">True if a dream should play tonight.</param>
        /// <param name="dreamTrigger">Director-supplied call that starts the
        /// correct dream variant (e.g. () => dream.PlayDream1()).</param>
        public void BeginNightSequence(bool playDream, Action dreamTrigger, Action onComplete)
        {
            StartCoroutine(RunSequence(playDream, dreamTrigger, onComplete));
        }

        private IEnumerator RunSequence(bool playDream, Action dreamTrigger, Action onComplete)
        {
            // 1) Dream (await its natural finish).
            if (playDream && dream != null)
            {
                bool finished = false;
                Action handler = () => finished = true;
                dream.OnDreamFinished += handler;
                Hh.Log(LogCategory.Cutscene, "EndOfDaySequencer → play dream.");
                dreamTrigger?.Invoke();
                while (!finished) yield return null;
                dream.OnDreamFinished -= handler;
            }

            // 2) Goodnight card (await Goodnight press).
            var tease = ResolveTease();
            if (card != null && tease != null)
            {
                bool advanced = false;
                Action handler = () => advanced = true;
                card.OnContinue += handler;
                card.Show(ResolveForwardLook(tease), tease.pickleSignOffText);
                Hh.Log(LogCategory.Mission, $"EndOfDaySequencer → goodnight card '{tease.sourceNode}'.");
                while (!advanced) yield return null;
                card.OnContinue -= handler;
            }

            // 3) Transition (the director's original behaviour).
            onComplete?.Invoke();
        }

        private TomorrowTeaseSO ResolveTease()
        {
            if (teases == null) return null;
            var vs = ServiceLocator.Get<VillageState>();
            int day = vs != null ? vs.currentDayIndex : 1;
            foreach (var t in teases)
                if (t != null && t.afterDayIndex == day) return t;
            return null;
        }

        private string ResolveForwardLook(TomorrowTeaseSO t)
        {
            if (!string.IsNullOrEmpty(t.branchFlagField) && BranchFlagTrue(t.branchFlagField)
                && !string.IsNullOrWhiteSpace(t.forwardLookTextAlt))
                return t.forwardLookTextAlt;
            return t.forwardLookText;
        }

        private bool BranchFlagTrue(string fieldName)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return false;
            FieldInfo f = vs.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return f != null && f.FieldType == typeof(bool) && (bool)f.GetValue(vs);
        }
    }
}
```

> **Note on `branchFlagField`:** point it at the actual `VillageState` bool that marks Doris's refusal (e.g. `dorisRefused`). If no such bool exists yet, leave `branchFlagField` empty for now and the standard goodnight always shows — the Senior Dev can add the field in a follow-up without touching this script.

---

## 5. Edits to existing runtime scripts (opt-in, ~6 lines each)

Add a serialized reference, then guard the transition. **All three are the same shape.**

### 5.1 `Mission01Director.cs`

Add fields near the other public refs (the director already exposes `eveningLedger`):

```csharp
[Header("Phase 47 — One More Day (optional)")]
public EndOfDaySequencer endOfDaySequencer;
public HearthboundHollow.Cutscene.MemoryDreamSequencer dreamSequencer;
```

Replace the body of `OnEndOfDayConfirmed()` (currently at ~line 525):

```csharp
private void OnEndOfDayConfirmed()
{
    var gm = GameManager.Instance;
    if (gm == null) return;

    System.Action transition = () =>
    {
        gm.EndDay();
        EventBus.Publish(new MissionCompletedEvent(null, "Mission01", "Day 1 — Doris's first loaves."));
        gm.LoadScene(sceneAfterEndOfDay);
    };

    if (endOfDaySequencer != null)
        endOfDaySequencer.BeginNightSequence(
            playDream: dreamSequencer != null,
            dreamTrigger: () => dreamSequencer?.PlayDream1(),
            onComplete: transition);
    else
        transition();   // unchanged legacy path
}
```

### 5.2 `Mission02Director.cs`

Same two fields. In `OnEndOfDayConfirmed()` (~line 865), wrap the existing body in a `transition` lambda exactly as above, but the dream trigger uses the M2 variant the director already computes:

```csharp
    if (endOfDaySequencer != null)
        endOfDaySequencer.BeginNightSequence(
            playDream: dreamSequencer != null,
            dreamTrigger: () => dreamSequencer?.PlayDream2(currentChoice, currentOutcome),  // use the director's chosen MoralChoice + CleanseOutcome
            onComplete: transition);
    else
        transition();
```

> Substitute `currentChoice` / `currentOutcome` with whatever fields Mission02Director already holds for the player's M2 decision (check the top of the file). If they aren't fields yet, capture them where the choice is made.

### 5.3 `MissionRunner.cs`

Add `public EndOfDaySequencer endOfDaySequencer;`. In `OnEndOfDayConfirmed()` (line 84) wrap the existing two lines:

```csharp
private void OnEndOfDayConfirmed()
{
    System.Action transition = () =>
    {
        var gm = GameManager.Instance;
        if (gm != null) gm.EndDay();
        if (!string.IsNullOrEmpty(nextMissionScene) && gm != null)
            gm.LoadScene(nextMissionScene);
    };

    if (endOfDaySequencer != null)
        endOfDaySequencer.BeginNightSequence(false, null, transition);  // MissionRunner has no dream rig
    else
        transition();
}
```

### 5.4 `DreamHook.cs` — no code change

`DreamHook` keeps working as the fallback. **When the sequencer owns the night**, the Phase 47 builder clears `DreamHook.ledger` so it stops subscribing (the sequencer now triggers the dream). Manual alternative: disable the DreamHook component in scenes 03/05.

---

## 6. Yarn prose to add *(drafts — Cordray / Tannenbaum / Ostlund finalize wording)*

### 6.1 Append to `Assets/_Project/Yarn/EveningLedger.yarn`

```yarn
title: Tomorrow_M1_Day1
---
The shelf holds one more light than it did this morning. Doris said dreams come — so sleep, and let them. Tomorrow the lane will bring someone to your door. It usually does.
===

title: Tomorrow_M1_Day1_Refused
---
You sent her home with her loaves still her own. That was allowed. The kettle's still warm, and the morning will still come. Rest.
===

title: Tomorrow_M2_Day2
---
The handkerchief is folded on the bench. Down the lane, a door will open again tomorrow — or it won't. Either way, the kettle stays warm, and you did a gentle thing today.
===
```

### 6.2 Append to `Assets/_Project/Yarn/Pickle.yarn`

```yarn
title: Pickle_Goodnight_M1
---
Pickle: Sleep. I will guard the shelf. Mostly.
===

title: Pickle_Goodnight_M2
---
Pickle: A man left lighter than he came. You did that. Don't let it go to your head.
===
```

> Mirror these strings into the matching `TomorrowTeaseSO` `forwardLookText` / `pickleSignOffText` fields (see §7). The Yarn file remains the canonical source of record.

---

## 7. ScriptableObject assets to create

Create via **Right-click in Project → Create → Hearthbound → Tomorrow Tease**, save under `Assets/_Project/ScriptableObjects/Missions/`.

### `Tomorrow_M1_Day1.asset`
| Field | Value |
|---|---|
| afterDayIndex | `1` |
| sourceNode | `Tomorrow_M1_Day1` |
| forwardLookText | *(paste the `Tomorrow_M1_Day1` line)* |
| branchFlagField | `dorisRefused` *(or leave empty until the bool exists)* |
| sourceNodeAlt | `Tomorrow_M1_Day1_Refused` |
| forwardLookTextAlt | *(paste the `_Refused` line)* |
| pickleSourceNode | `Pickle_Goodnight_M1` |
| pickleSignOffText | `Sleep. I will guard the shelf. Mostly.` |

### `Tomorrow_M2_Day2.asset`
| Field | Value |
|---|---|
| afterDayIndex | `2` |
| sourceNode | `Tomorrow_M2_Day2` |
| forwardLookText | *(paste the `Tomorrow_M2_Day2` line)* |
| pickleSourceNode | `Pickle_Goodnight_M2` |
| pickleSignOffText | `A man left lighter than he came. You did that. Don't let it go to your head.` |

---

## 8. Build the card prefab (`OneMoreDayCard.prefab`)

Either run the Phase 47 builder (§9) or build manually:

1. In the gameplay-UI canvas, create an empty `OneMoreDayCard` GameObject; add the **`OneMoreDayCard`** component + a **`CanvasGroup`**.
2. Child `Root` (full-screen) → semi-opaque dark backdrop `Image` (raycast target ON, so clicks behind are blocked).
3. Child a centered parchment panel (reuse the **Bamao parchment** sprite the Evening Ledger uses for visual consistency).
4. Add three `TextMeshProUGUI` children: **Headline** ("Tomorrow"), **ForwardLook**, **Pickle**.
5. Add a single **Goodnight** `Button` (reskin a Heat button to the warm-parchment preset).
6. Assign all of these to the component fields (`root`, `canvasGroup`, `headlineLabel`, `forwardLookLabel`, `pickleLabel`, `goodnightButton`).
7. Set the panel's initial state inactive (the script self-heals on `Show`).
8. Save as `Assets/_Project/Prefabs/UI/OneMoreDayCard.prefab`.

---

## 9. New editor builder — `Phase47_OneMoreDayBuilder.cs`

**Path:** `Assets/_Project/Scripts/Editor/Phase47_OneMoreDayBuilder.cs` (assembly: `HearthboundHollow.Editor`)

Idempotent (load-or-create + heal-then-save). Registered under `Hearthbound → ⚙️ Advanced/…` per **D-051**, and chained into `🚀 Build Everything`.

Responsibilities (mirror the existing Phase builders' structure — open it side-by-side with `Phase22`/`Phase32` builders and match their helpers):

1. **Create/heal** the two `TomorrowTeaseSO` assets in `ScriptableObjects/Missions/` with the §7 values (only fill empty fields, so designer tweaks survive re-runs).
2. **Create/heal** `OneMoreDayCard.prefab` per §8 (build the hierarchy programmatically; skip if it already exists and is valid).
3. **In scenes `03_Mission01_Hollow` and `05_Mission02_Cottage`:** instantiate the card prefab into the UI canvas, add an `EndOfDaySequencer`, wire `card` + `dream` (find the existing `MemoryDreamSequencer`) + the `teases` array, then wire the scene's director's `endOfDaySequencer` + `dreamSequencer` fields.
4. **Clear `DreamHook.ledger`** in those scenes (so the sequencer owns dreams — see §5.4).
5. `EditorSceneManager.MarkSceneDirty` + save.

Add the menu item and append the call to the `🚀 Build Everything` chain:

```csharp
[MenuItem("Hearthbound/⚙️ Advanced/Phase 47 — Build One More Day Hook")]
public static void Build() { /* steps 1–5 */ }
```

Then in the master `Build Everything` aggregator, add `Phase47_OneMoreDayBuilder.Build();` after the Phase 32 polish step.

---

## 10. Editor run order

1. Create the 4 scripts (§2–4, §9) — let Unity compile (watch the Console for green).
2. Apply the 3 director/runner edits (§5).
3. Append the Yarn nodes (§6) — re-import Yarn.
4. **`Hearthbound → 🚀 Build Everything`** (idempotent — builds the SOs, prefab, scene wiring).
5. **`Hearthbound → 🔍 Diagnose Build`** — confirm clean.
6. Press Play from `00_Bootstrap`, finish Day 1, confirm the order: Ledger → (Dream) → Goodnight card → Day 2.

---

## 11. Cozy-review checklist (must pass before merge)

- [ ] No player-visible numbers on the card.
- [ ] No "FAILED"/score language anywhere.
- [ ] Goodnight button always advances; card is skippable.
- [ ] Refusal path shows `Tomorrow_M1_Day1_Refused`, not the standard line.
- [ ] Fade-in only — no shake, no flash, no harsh sting.
- [ ] Gentle Mode: identical content, instant/!-stress fade.
- [ ] Pickle line respects Tannenbaum's budget (one short line).

## 12. QA acceptance criteria

1. **Regression gate:** with `endOfDaySequencer` unwired, day-end behaves byte-for-byte as today.
2. Day 1 confirm → (dream) → Tomorrow card → `02→03` flow, in order.
3. Missing Yarn/SO degrades gracefully (forward-look only; no NRE).
4. `🔍 Diagnose Build` clean; EditMode + PlayMode tests green.
5. Smoke: boot → Day 1 → card → Day 2 with zero NRE in the Console.

---

## 13. Docs to update on merge

**`Docs/PROGRESS.md`** — add a Phase 47 entry: files added/edited, the night-order decision, follow-ups (Tier 2 visuals; wire the `dorisRefused` bool; switch SO text → Yarn dispatcher when that pass lands).

**`Docs/ARCHITECTURE.md`** — add:

> **D-060** *(Phase 47 — One More Day)* The night transition (Ledger → Dream → Goodnight Card → scene load) is owned by a single `EndOfDaySequencer` when present, replacing the previous multi-subscriber `OnEndOfDayConfirmed` race (director + DreamHook firing simultaneously). It is opt-in: directors delegate only when the sequencer is wired, else the legacy inline path runs unchanged. Goodnight prose is mirrored from Yarn into `TomorrowTeaseSO` (canonical source remains the Yarn file) until the runtime Yarn-dispatcher pass lands.

---

*Tier 2 (deferred): visitor silhouette + half-lit Echo Web thread glimmer on the card. Tier 3 (deferred): the promise pays off next morning — a waiting cue in the world on load.*
