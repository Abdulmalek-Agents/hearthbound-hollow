# ⚖️ System 09 — The Memory Wall & Echo Web Meta-Game (Pillar P6)

> **Owner:** Choice & Consequence Architect + UI. **Implements:** the player's *long-term
> goal engine* and *visible collection* — the two things that turn "do a task" into "pursue
> a project." **Goal:** make collecting and connecting memories the addictive, self-directed
> meta-game that sits above the daily loop (the cozy equivalent of Stardew's Community Center
> bundles or the museum in Animal Crossing).

---

## 1. The problem this fixes

The Echo Web is described beautifully (`GDD §6.5`: 88 echoes, golden threads, Revelation
Chapters) but in the build it's **one invisible line between two memories**, recorded to a
save field the player never sees. There is **no collection screen, no goal, no payoff for
connecting.** The game's most distinctive meta-idea is dormant. This is the missing
**"player-authored goal"** engine — give the player a wall of memories and a web of
connections to *complete*, and they will set their own goals for hours.

---

## 2. The Memory Wall (the collection)

A physical wall in the Hollow (and a Codex screen) where **every memory you keep is displayed
as an orb.** This is the cozy collection dopamine — the museum you fill.

```
   THE MEMORY WALL                         Echoes found: 3 / 24
   ┌─────────────────────────────────────────────────────────┐
   │  ● Doris: First Loaves      ◐ Doris: Wedding Honey  ◯ ?   │
   │      │  golden thread  │                                  │
   │  ● Gerrold: The Last Week   ◯ ?                ◯ ?        │
   │                                                           │
   │  Threads:  "The Sunday Kitchen"  ✓ complete (2/2)         │
   │            "The Blue Shawl"      ◐ 1/3 — find 2 more      │
   └─────────────────────────────────────────────────────────┘
```

- **Filled orbs** = memories you've kept; **dim "?"** = memories you know exist but haven't got
  (the trust-lock / mystery-lock / unknown-lock states the M1 guide already describes for
  Doris's 4-node map).
- **Golden threads** connect memories that share an **Echo** (a person/place/object/year key).
- A small, *celebratory* counter ("Echoes found: 3 / 24") — visible progression (D-076), warm not anxious.

---

## 3. The Echo meta-game (the goal engine)

Memories carry **Echo tags** (the data field already exists in the memory schema). When the
player keeps memories that share an Echo, a **thread** forms; completing a thread to its
threshold pays off:

| Echo type | Threshold | Payoff |
|---|---|---|
| **Person-Echo** | collect 3 memories naming the same person | Reveals a relationship the village never spoke aloud → unlocks a new Request Board arc |
| **Place-Echo** | 4 sharing a place | A half-remembered village event → a Memory Dream + lore card |
| **Object-Echo** | 3 sharing an object | The object's story → a decor item for the Hollow (P3) |
| **Year-Echo** | 5 from the same year | A "Forgotten Year" fragment → predecessor-mystery progress |
| **Pattern-Echo** | 6 | A piece of the Marin mystery → the grand goal |

**This is the engine of self-set goals.** The player sees "The Blue Shawl: 1/3" on the Wall and
*decides* "I want to find the other two" — then pursues villagers/requests that might carry
them. That's Pillar P2 (board) and P6 (web) reinforcing each other into a *project the player
authors.* It's the cozy genre's bundle/collection compulsion, themed perfectly to memory.

---

## 4. The reward ladder (why connecting feels great)

Completing threads pays into *every other system*, so the meta-game is the loop's connective tissue:

- **Memory Dream** unlocks (the beautiful payoff — now *earned*, replayable from the Wall).
- **Decor / lore cards** for the Hollow (feeds P3 ownership).
- **New Request arcs** open (feeds P2 — the web *generates content*).
- **Predecessor-mystery progress** (the grand narrative goal that pulls across the whole game).
- A warm **"thread complete" beat**: the golden line blooms, Pickle remarks, a soft chime.

---

## 5. Data + implementation

Most of the data already exists:
- `VillageState.heldMemoryIds`, `revealedEchoConnectionIds` — persisted lists, already there.
- `EchoConnectionRevealedEvent` — already on the EventBus.
- `MemoryConnectionSO`, `MemoryNodeSO`, `VillagerMemoryMapSO` — already in `Scripts/Memory/`.
- `MemoryWebOverlay.cs` — **a Wall/web UI already exists** (Phase 51 depth layer) and is
  under-used. We promote it from a depth-layer curiosity to the **core meta-game screen.**

What to add:
```csharp
// EchoSO — Scripts/Memory/EchoSO.cs
[CreateAssetMenu(menuName = "Hearthbound/Memory/Echo", fileName = "Echo_")]
public class EchoSO : ScriptableObject
{
    public string echoId;                 // "ECHO_SUNDAY_KITCHEN"
    public string displayName;            // "The Sunday Kitchen"
    public EchoType type;                 // Person/Place/Object/Year/Pattern
    public int threshold = 3;
    public List<MemoryNodeSO> members;    // memories that carry this echo
    public List<string> rewardsOnComplete;// dreamId / decorId / requestArcId / mysteryStep
}
```
```csharp
// EchoWebService (Mission asmdef) — listens to memory-kept events, tallies echoes,
// fires EchoThreadCompletedEvent (add to EngagementEvents.cs), grants rewards, updates the Wall.
```

`EchoWebService.OnMemoryKept` increments each member-echo's progress; on reaching `threshold`
it grants `rewardsOnComplete` and publishes the completion event the Wall + Ledger consume.

---

## 6. Step-by-step build order (Phase 63 in the roadmap — early, it's the goal engine)

1. Add `EchoSO` + `EchoType` (Memory asmdef); author the M1-2 echoes (start with "The Sunday Kitchen" DOR↔GER, already canonical).
2. Add `EchoWebService` (Mission) + `EchoThreadCompletedEvent`.
3. Promote `MemoryWebOverlay` into the **Memory Wall** screen: filled/dim orbs, golden threads, the "X / N echoes" counter, thread progress.
4. Place a physical Wall interactable in the Hollow (reuse shelf prefabs) that opens the screen.
5. Wire rewards: completing a thread unlocks a Dream (replayable), grants decor/coin, opens a Request arc, advances the mystery.
6. Surface thread progress in the morning Agenda ("The Blue Shawl: one more to find") and Evening Ledger ("Completed an Echo!").
7. Chain builder; update `PROGRESS.md`.

---

## 7. Acceptance criteria

- [ ] Keeping a memory adds it to the Wall as a filled orb; known-but-unowned show as dim "?".
- [ ] Sharing an Echo draws a visible golden thread between memories.
- [ ] Completing a thread to threshold grants its reward (Dream/decor/arc/mystery) + a celebratory beat.
- [ ] The "Echoes found: X / N" counter is visible, accurate, and *celebratory* (never a nag).
- [ ] At least one full thread is completable within the 3-day vertical-slice gate (proves the loop closes).
- [ ] Players in playtest **name a thread they want to complete** (the self-set-goal proof, per `02 §4`).

---

## 8. Why this is the soul of long-term retention

The daily loop (P1–P5) keeps players engaged *today*. The Echo Web keeps them engaged *for the
season*: it's the slowly-filling wall, the "two more to go," the "who else was in that
kitchen?", the thread that quietly leads to *why Marin vanished.* It converts a pile of lovely
memories into a **mystery the player chooses to solve** — and the previous-keeper reveal at the
end of the web is the cozy-narrative payoff the whole game has been promising. **This is where
Hearthbound's unique hook finally becomes a unique *game*.**

---

*System 09 v1.0 — Next: `10_IMPLEMENTATION_ROADMAP.md`.*
