# 🫖 Hearthbound Hollow — Art Production (Mission 1–2)

The complete **art production package** for the Mission 1–2 vertical slice of *Hearthbound Hollow*, a cozy memory‑broker narrative simulation. Generated from the studio canon in [`Docs/MASTER_BIBLE/`](../MASTER_BIBLE) — principally the **Engagement Bible**, the **Depth Bible**, and the **`Depth_Bible/Mission_1_2_Focus/`** specs — plus `Docs/Asset_Analysis_Mission1-2.md`.

> **Branch:** `feat/mission-1-2-architecture` · **Engine:** Unity 6000.4.4f1 (URP‑Mobile, 60 fps mid‑range) · **Scope:** Mission 1 *"Opening the Hollow"* + Mission 2 *"The Widower's Request"* · 4 scenes · **130 defined art assets** · **45‑person studio**

---

## 📦 What's in here

| File | What it is |
|---|---|
| [`00_TEAM_ROSTER.md`](00_TEAM_ROSTER.md) | The full **45‑person studio roster** — direction, production, the whole art org, audio, design, narrative, engineering, QA — with responsibilities and the convening protocol. |
| [`01_ART_PRODUCTION_PLAN.md`](01_ART_PRODUCTION_PLAN.md) | The **art production plan**: north star, palette/lighting language, pipeline & conventions, all four scenes, characters, orbs/shaders, UI, dreams, the dependency‑ordered build sequence, and acceptance criteria. |
| [`02_ASSET_DEFINITIONS.md`](02_ASSET_DEFINITIONS.md) | **Every art asset** defined — name, type, description, palette, lighting mood, location, LOD notes, file path (generated from the CSV). |
| [`03_COLOR_AND_LIGHTING_BIBLE.md`](03_COLOR_AND_LIGHTING_BIBLE.md) | The **color & lighting glossary** — master autumn palette, the 6‑emotion orb/dream palette, the 5 lighting moods, the realtime‑light budget. |
| [`ART_PRODUCTION_DASHBOARD.html`](ART_PRODUCTION_DASHBOARD.html) | An **interactive HTML dashboard** — filter/search assets, browse the roster, view palette swatches and per‑scene/category breakdowns. It loads the CSVs in `manifests/` live (best via GitHub Pages or a local server such as `python3 -m http.server`; opened straight from disk it falls back to fetching the manifests from the branch's raw URLs, so it also works in‑browser with an internet connection). |
| [`manifests/asset_manifest.csv`](manifests/asset_manifest.csv) | The machine‑readable **asset manifest** (130 rows × 14 columns) — the single source of truth. |
| [`manifests/team_roster.csv`](manifests/team_roster.csv) | The roster as CSV (45 rows). |
| [`manifests/lighting_palette.csv`](manifests/lighting_palette.csv) | The palette & lighting swatches as CSV (30 rows). |

---

## 🎯 The slice at a glance

- **4 scenes:** the Village Lane at Dusk · the Hollow Interior · the Herb Garden · the Widower's Cottage.
- **4 on‑screen characters:** Doris the Baker (BoZo Cleric reskin), Gerrold the Widower (Bard reskin), a silent lane villager (Warrior reskin), and **Pickle** the slate‑grey cat.
- **2 mini‑games:** Polish (M1, warm) and Cleanse (M2, careful) — both built on the `MemoryOrb_Master` shader.
- **2 Memory Dreams:** *The First Loaves* (warm) and *The Last Week* (4 choice variants).
- **6 realtime lights total**, build < 1.5 GB, < 200 MB textures — the URP‑Mobile budget.

## 🧭 Inviolable disciplines (carried from `CLAUDE.md`)
1. **The Cozy Contract holds, always** — nothing punishes kindness; failure is narratively absorbed; **no "FAILED" string ever ships**; Auto‑Complete on every mini‑game; cozy, opt‑in, celebratory progression feedback (D‑076).
2. **No dark monetization, ever.**
3. **All dialogue is hand‑written** — AI‑generated dialogue is forbidden (Pillar 1).
4. Respect the **Out‑of‑Scope Wall** — the rest of the 20‑acre map, the 30 villagers, and the 17 predecessor fragments are Scaling Reference.

---

## 🔁 Regenerating the docs
The CSV is the source of truth. After editing `manifests/asset_manifest.csv`, regenerate `02_ASSET_DEFINITIONS.md` and the dashboard data, then update `Docs/PROGRESS.md` (no art lands without it).

---
*Art Production package v1.0 · part of the Abdulmalek Agents game‑concept portfolio.*
