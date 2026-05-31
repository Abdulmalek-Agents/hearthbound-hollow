# 🌐 Phase 56 — Arabic Localization Fix (tofu boxes → real Arabic)

> Trigger: QA video *"arabic language localization appear as square boxes not real
> arabic word need to import arabic font…"*. Switching the UI to **العربية** turned
> the Settings/Pause chrome into **□□□□ boxes**.

## Root cause
Two missing pieces (the Arabic *strings* in `LocalizationService` were already correct):
1. **No Arabic glyphs.** The body font (`LiberationSans SDF`) has no Arabic, and TMP
   had no Arabic fallback → every Arabic code point rendered as a missing-glyph box.
2. **No shaping.** TMP's classic component doesn't do Arabic contextual joining or
   RTL ordering, so even with a font you'd get disconnected, left-to-right letters
   ("not real Arabic words").

## The fix (D-073) — three parts, all pushed
| File | Role |
|---|---|
| `Scripts/Editor/Phase56_ArabicFontInstaller.cs` | Builds an Arabic-capable **dynamic** TMP font asset and registers it as a **fallback** on the default font + TMP Settings global fallback. Sources a font from `Assets/_Project/Art/Fonts/` (preferred) or an OS Arabic font (Tahoma/Arial/Segoe UI/Noto). Idempotent, auto-installs on editor load, fully defensive. |
| `Scripts/Core/ArabicShaper.cs` | Pure-C# Arabic shaper: contextual presentation forms (init/medial/final/isolated) + Lam-Alef ligatures + visual RTL ordering. Non-Arabic strings pass through untouched. |
| `Scripts/UI/LocalizedText.cs` | Runs `ArabicShaper.Shape()` on resolved Arabic before display (TMP native RTL kept off to avoid double-reversal); keeps the right-align it already did. |

The font fixes the boxes; the shaper makes letters join into real, right-to-left words.

## What you do in Unity after pulling
1. **Pull → recompile.** On load, `Phase56` auto-installs `Assets/_Project/Art/Fonts/HollowArabic SDF.asset`
   (or run **Hearthbound → ⚙️ Advanced → Phase 56 — Install Arabic Font (fix tofu)**).
2. Play → **Settings → العربية**. The chrome now shows connected Arabic, right-aligned. No boxes.
3. Confirm `🔍 Diagnose Build` is clean.

## ⚠️ Shipping note (licensing)
For immediate testing the installer may copy a **system** font (e.g. Tahoma/Arial),
which is **not redistributable**. Before shipping, drop a free **SIL-OFL** Arabic font
into `Assets/_Project/Art/Fonts/` and re-run Phase 56 — it prefers a project font over
the OS one. Recommended: **Noto Naskh Arabic**, **Amiri**, **Cairo**, or **Scheherazade**
(all OFL). The dynamic asset means no giant pre-baked atlas — glyphs rasterise on demand.

## Notes / scope
- Covers the **UI chrome** localized via `LocalizedText` (menus, settings, pause,
  character creation). Hand-written **dialogue** prose stays canonical English (D-065 —
  a dedicated writer translation pass is separate; the shaper + font are ready for it).
- The shaper handles pure-Arabic and Arabic+Latin/number runs; complex bidi paragraphs
  (heavy mixed LTR/RTL) are out of scope for the short UI labels here.
- asmdef-clean: ArabicShaper (Core), LocalizedText (UI→Core), installer (Editor). No cycles.

---
*Phase 56 — Localization lead + 2× C# Scripters + UX/UI + 2× QA (EN/AR).*
