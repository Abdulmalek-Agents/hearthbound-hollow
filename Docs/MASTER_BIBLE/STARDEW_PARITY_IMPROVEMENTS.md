# 🌾 Hearthbound Hollow — Getting Near *Stardew Valley*
### A Comprehensive Improvement Plan · Critic & Review Board synthesis
*Bilingual: English (below) · العربية (في الأسفل)*

> **Prepared by:** Critic & Review Board — 3× Market Critics · 2× Senior Game Directors · Lead Game & Level Designer · Systems & Progression Architect · Cozy Comfort Engineer · 4× Senior QA · Technical Lead · Community/Marketing (cozy specialist).
> **Sources:** `Docs/Engagement_Bible/01–10`, `GAME_DESIGN.md`, `Docs/Depth_Bible/`, the Mission gameplay guides, the runtime directors, and a benchmark pass against Stardew Valley, Spiritfarer, Coffee Talk, Strange Horticulture, Cozy Grove, Travellers Rest & Coral Island.
> **Branch:** `feat/mission-1-2-architecture`

---

# 🇬🇧 ENGLISH

## 1. The verdict in one sentence

> **Hearthbound Hollow has the best *hook*, writing, art and tech in cozy — but as a ~75-minute linear corridor it scored 0/7 against Stardew's retention engines. The fix is not more writing; it is building the compounding daily loop so players *want to wake up tomorrow*.**

The heart is a 9/10. The missing piece is *structure*: a reason to open the game again. The good news — confirmed by the board — is that the loop is **buildable on top of everything that already exists**, throwing nothing away. (Engagement Bible Phases 61–67 have now built the seven pillars; validation is the next gate.)

## 2. The Stardew benchmark — the 7 retention engines

Stardew retains players for *hundreds* of hours because of **seven structural engines**. We graded the original vertical slice against each.

| # | Stardew's retention engine | Hearthbound (original slice) | Target |
|---|---|---|---|
| 1 | **Compounding daily loop** — today's work pays off tomorrow | Loop runs twice, then credits | ✅ Build P1 |
| 2 | **Player-authored goals** — *you* decide today's plan | The game decides; player follows a corridor | ✅ Build P2/P6 |
| 3 | **Ownership & customization** — your farm/house changes | The Hollow is fixed set-dressing | ✅ Build P3 |
| 4 | **Many interleaving systems** — farm/fish/mine/cook… | Two mini-games only | ✅ Build P4/P5 |
| 5 | **Tangible, visible progression** — money, tools, bars | All progress hidden (old "Cordray Principle") | ✅ Build P6 + D-076 |
| 6 | **A calendar of anticipation** — festivals, birthdays | No calendar event in the build | ✅ Build P7 |
| 7 | **Surprise & variety** — random events, rare finds | Fully deterministic, identical every run | ✅ Build P2/P7 |

**Original score: 0 / 7.** The mismatch between the Steam promise ("a memory-brokerage shop simulator") and a linear narrative demo is itself a refund driver.

## 3. The five root causes (fix causes, not symptoms)

1. **"Vertical slice" became "the whole game."** 60+ phases polished a *demo* to a mirror shine without building the loop the demo was meant to preview. *Polish is not progress when you polish 2% of the game.*
2. **The "Cordray Principle" hid all feedback.** No numbers, no bars, no goals = the player can't be motivated by growth they're forbidden to see. **Cozy ≠ feedback-free.**
3. **Mechanics are "experiences," not "skills."** Polish & Cleanse are no-fail, auto-completable, one-pace. No mastery curve = no reason to do them a 20th time.
4. **Content is hand-authored and finite.** 2 villagers, 2 memories. A cozy game needs an *engine that generates gentle content*, not a corridor of two rooms.
5. **Loop-building assets sit unused.** The imported **HarvestGarden** grow/harvest pack is decorative; `DayCycleManager` only dims a cutscene light. *The raw materials are already in the project, unwired.*

## 4. The fix — the 7 Engagement Pillars (0/7 → 7/7)

The design thesis upgrades the fantasy from **"read a memory"** to **"run the Hollow"** — a daily, gentle, compounding *practice you keep*. Each pillar maps 1:1 to a missing Stardew engine.

| # | Pillar | Replaces (Stardew engine) | Player feeling | Status |
|---|---|---|---|---|
| **P1** | **The Living Day** — a real morning→night cycle you wake into & close out | Compounding loop | *"A new day. What shall I do with it?"* | ✅ Live (Ph 61) |
| **P2** | **The Request Board** — a rotating, never-empty queue of villagers + memories | Surprise/variety + infinite content | *"Who needs me today? A new face!"* | ✅ Live (Ph 62) |
| **P3** | **My Hollow** — a shop you stock, arrange, upgrade & decorate | Ownership & customization | *"This is mine, and it's getting better."* | ✅ Live (Ph 64) |
| **P4** | **The Garden & Tea** — grow herbs, brew teas, use them as tools | Interleaving system #1 | *"My lavender's ready — time to brew."* | ✅ Live (Ph 65) |
| **P5** | **The Living Workbench** — varied, juicy, skill-curved craft | Interleaving #2 + mastery | *"I'm getting good at this. One more."* | ✅ Live (Ph 66) |
| **P6** | **The Memory Wall** — collect, connect & complete the Echo Web | Player goals + visible collection | *"Two more and I complete Doris's thread!"* | ✅ Live (Ph 63) |
| **P7** | **The Almanac** — festivals, visitors & anticipation | Calendar of anticipation | *"The Honey Festival is in 3 days!"* | ✅ Live (Ph 67) |

> **Crucial discipline:** every pillar is **opt-in and unpunishing**. A player can ignore the garden, auto-complete every craft, never decorate, and still finish. The pillars add *pull*, never *push* — Stardew's depth, the Cozy Contract intact.

## 5. The new core loop (what we are actually building)

```
        ☀  A NEW MORNING  — wake; the Agenda card slides in:
            • 2–3 villagers on the Request Board
            • garden status (herb ready?)
            • 1 gentle self-set goal suggestion
                       │  player chooses the order — NO forced path
   ┌──────────┬────────┴────────┬──────────────┐
   ▼          ▼                  ▼              ▼
 OPEN SHOP  TEND GARDEN     WALK VILLAGE    DO THE CRAFT
 take a     water/harvest   deliver, chat,  Polish/Cleanse/
 memory     → brew teas     find echoes     Weave/Sort/Listen
   └──────────┴───────┬─────────┴──────────────┘
                      ▼
              💰 SPEND / GROW   ← the compounding bit
        buy a shelf, a herb bed, decor, a tool; arrange the Wall
                      ▼
        🌙 CLOSE THE LEDGER — celebratory recap of today's deeds,
        what grew, what you earned, + a tomorrow tease →
        sleep → optional Dream → A NEW (changed) MORNING ↺
```

**Why it retains:** it *compounds* (coin → shelves/beds/tools → more capacity → more memories → more coin); it's *player-directed* (the morning offers options, never a corridor); it *varies* (board reshuffles, Almanac injects festivals, garden cycles); it *pays off tomorrow*; and it *still tells the story* — Doris, Gerrold, Marin and the Echo Web are now spread across the loop as the reasons it has meaning.

## 6. Nothing is thrown away — existing content gets promoted

| Existing asset | Old role | New role in the loop |
|---|---|---|
| **Doris** | Tutorial villager, gone after Day 1 | The first **regular** on the Request Board; her Memory Map unlocks over many visits |
| **Gerrold** | One moral choice, gone after Day 2 | A **recurring relationship**; the Day-2 choice is the *first* of his arc |
| **Polish / Cleanse** | The only two mechanics | Two of a **growing verb set** with a gentle skill curve |
| **Memory Dreams** | End-of-mission cutscene | The **reward** you unlock by collecting/connecting memories (replayable) |
| **Echo Web** | A described line of light | A **real meta-game**: complete threads for Dream + lore payoffs |
| **Evening Ledger** | A save screen | The **daily recap + celebration + tomorrow-tease** |
| **HarvestGarden asset** | Decorative | The **garden loop** |
| **`DayCycleManager`** | Dims a cutscene light | The **clock** that drives the Living Day |
| **`VillageState` 14 dims** | Default-valued seams | **Now written-to and surfaced** as cozy feedback (D-076) |

## 7. Priority, status & the build order

Built **spine before limbs**: P1 → P2 → P6 → P3 → P4 → P5 → P7.

| Phase | Pillar | Deliverable | Status |
|---|---|---|---|
| 61 | P1 | Living Day: Agenda card, `DailyLoopService`, Evening recap + tomorrow-tease | ✅ Done |
| 62 | P2 | Interactive Request Board `[B]`, `RequestVisitService`, real coin economy, save schema v3 | ✅ Done |
| 63 | P6 | Memory Wall `[M]`, `EchoWebService`, Echo threads to chase/complete | ✅ Done |
| 64 | P3 | `HollowProgressionService`, shop `[U]`, coin-purse HUD | ✅ Done |
| 65 | P4 | `GardenService`, Garden `[G]`, grow→brew→use teas | ✅ Done |
| 66 | P5 | `WorkbenchService`, bench `[K]`, `CraftVerbs` + Keeper's-Hand mastery | ✅ Done |
| 67 | P7 | `AlmanacService` — Market Day / festivals / bard / birthdays | ✅ Done |
| 68 | P6+ | Procedural villagers via the Vignette Library; expand the Request pool | ✅ Done (built-in rosters; authored SOs extend with zero code) |
| 69 | all | Cozy feedback pass (D-076): coin purse, collection %, agenda — celebratory | 🟡 Core done |
| **70** | all | **G-Engage playtest** — 3 looped days; measure the "voluntary Day 4" metric | 🟡 Plan shipped |
| 71 | all | Tune from playtest; pace heavy beats across the loop | 🟡 Guidance documented |
| 72 | all | Marketing-truth pass: align README/Steam copy with the now-real loop | 🟡 Documented |

## 8. The new success metric (the only honest one)

> **Gate G-Engage:** 20 cozy-target testers play **three in-game days** (~90 min). **≥15/20 (75%)** voluntarily start a **4th day** unprompted, and **≥60%** name a self-set goal they want to pursue ("upgrade my shelf," "finish Doris's thread," "grow valerian").

Self-directed continuation is the only true measure of a cozy loop. Choose Day 4 → we have a game. Stop at Day 3 → we have a longer vignette.

## 9. The guardrails every improvement inherits

1. **The Cozy Contract holds** — no fail states, no timers unless opt-in, refusal always honored, kindness is optimal, Auto-Complete on every mini-game. **No "FAILED" string ever ships.**
2. **No dark monetization** — ever. No gacha, energy, lootboxes, P2W. Trust is the moat.
3. **Feedback is celebratory, never anxious (D-076)** — show coin, growth, collection %, the agenda — warmly. Never a punishing countdown or a red "behind schedule" number.
4. **Writing quality bar unchanged** — hand-sealed villagers stay hand-sealed; procedural variety uses the Vignette Library at the same bar. **AI-generated dialogue is forbidden.**
5. **Architecture discipline** — respect the asmdef graph; new state on `VillageState`; new events on the EventBus; data in ScriptableObjects; everything idempotent behind `🚀 Build Everything`.

## 10. The market read (will it sell?)

- **Commercial:** as a 75-min curio → ~150–250k units, ~72/100, refunded at $24.99. **With the loop shipped → a $14–18M ceiling** and a multi-year tail.
- **Cozy-community:** "beautiful but nothing to do" is the death of thin cozy games (Spirittea, Mind Scanners). A *real* loop flips the top review to *"I meant to play one day and it's 1 a.m."*
- **Retention/LiveOps:** the genre lives on **D30**. There was no D2. The loop creates D7/D30.

## 11. Recommended next steps (do these in order)

1. **Run G-Engage (Phase 70)** — the 3-day looped playtest; instrument *voluntary Day 4*. This is the single most important action; everything else is tuning.
2. **Tune pacing (Phase 71)** — space the heavy grief beats across the loop using the Comedy/Grief radius so the cozy tone never tips into relentless.
3. **Marketing-truth pass (Phase 72)** — update the README/Steam copy to match the now-real loop and stop the expectation-mismatch refund driver.
4. **Deepen content (Phase 68+)** — author 4–6 more Vignette-Library villagers and Echo threads to extend the season toward the 40-hour promise.
5. **Skill ceilings (P5)** — finish the gentle mastery curves on Polish/Cleanse/Weave so the 20th craft still satisfies.
6. **Protect the prize** — never let the loop dilute the writing. The hook is the best in the genre; we are building *the reason to keep unwrapping it.*

---
---

# 🇸🇦 العربية (Arabic — RTL)

> **مُعدّ بواسطة:** مجلس النقّاد والمراجعة — ٣ نقّاد سوق · مديرَا ألعاب أوّلان · مصمّم الألعاب والمراحل الرئيسي · مهندس الأنظمة والتقدّم · مهندس الراحة الدافئة · ٤ من كبار مختبري الجودة · القائد التقني · مسؤول المجتمع والتسويق (متخصّص الألعاب الدافئة).
> **المصادر:** `Docs/Engagement_Bible/01–10`، و`GAME_DESIGN.md`، و`Docs/Depth_Bible/`، وأدلّة اللعب، ومقارنة معيارية مع *ستارديو فالي* و*سبيريت فيرر* و*كوفي توك* و*سترينج هورتيكلتشر* و*كوزي غروف* و*ترافلرز ريست* و*كورال آيلاند*.
> **الفرع:** `feat/mission-1-2-architecture`

## ١. الحُكم في جملة واحدة

> **يمتلك «الجوف الدافئ» أفضل *فكرة جذب* وكتابة وفنّ وتقنية في فئة الألعاب الدافئة — لكنه بوصفه ممرًّا خطّيًا مدّته ~٧٥ دقيقة سجّل ٠ من ٧ مقابل محرّكات استبقاء اللاعب في ستارديو فالي. والحلّ ليس مزيدًا من الكتابة، بل بناء الحلقة اليومية المتراكمة التي تجعل اللاعب *يرغب في الاستيقاظ غدًا*.**

القلب يستحقّ ٩ من ١٠. الناقص هو *البنية*: سببٌ لإعادة فتح اللعبة. والخبر السار — كما أكّد المجلس — أنّ الحلقة **قابلة للبناء فوق كلّ ما هو موجود** دون التخلّص من أي شيء. (أنجزت مراحل «دليل التفاعل» ٦١–٦٧ الركائز السبع؛ والتحقّق هو البوابة التالية.)

## ٢. المعيار مقابل ستارديو — محرّكات الاستبقاء السبعة

يبقي ستارديو فالي اللاعبين *مئات* الساعات بفضل **سبعة محرّكات بنيوية**. قيّمنا الشريحة الأصلية مقابل كلٍّ منها.

| # | محرّك الاستبقاء في ستارديو | الجوف الدافئ (الشريحة الأصلية) | الهدف |
|---|---|---|---|
| ١ | **حلقة يومية متراكمة** — عمل اليوم يثمر غدًا | تعمل الحلقة مرّتين ثم تنتهي | ✅ بناء P1 |
| ٢ | **أهداف يضعها اللاعب** — *أنت* تقرّر خطّة اليوم | اللعبة تقرّر؛ واللاعب يتبع ممرًّا | ✅ بناء P2/P6 |
| ٣ | **التملّك والتخصيص** — مزرعتك/بيتك يتغيّر | الجوف ديكور ثابت لا يتغيّر | ✅ بناء P3 |
| ٤ | **أنظمة متشابكة كثيرة** — زراعة/صيد/تعدين/طبخ… | لعبتان مصغّرتان فقط | ✅ بناء P4/P5 |
| ٥ | **تقدّم ملموس ومرئي** — مال، أدوات، أشرطة | كلّ التقدّم مخفيّ (مبدأ كوردراي القديم) | ✅ بناء P6 + D-076 |
| ٦ | **تقويم يبعث على الترقّب** — مهرجانات، أعياد ميلاد | لا حدث تقويميّ في النسخة | ✅ بناء P7 |
| ٧ | **المفاجأة والتنوّع** — أحداث عشوائية، اكتشافات نادرة | حتميّة تمامًا ومتطابقة كلّ مرّة | ✅ بناء P2/P7 |

**النتيجة الأصلية: ٠ من ٧.** والتناقض بين وعد ستيم («محاكي متجر وساطة الذكريات») وبين عرضٍ سرديّ خطّي هو بحدّ ذاته سببٌ لطلبات الاسترداد.

## ٣. الأسباب الجذرية الخمسة (نُعالج الأسباب لا الأعراض)

1. **«الشريحة العموديّة» صارت «اللعبة كاملةً».** صقلت أكثر من ٦٠ مرحلةً *عرضًا تجريبيًّا* حتى اللمعان دون بناء الحلقة التي كان العرض يُفترض أن يُمهّد لها. *الصقل ليس تقدّمًا حين تصقل ٢٪ من اللعبة.*
2. **«مبدأ كوردراي» أخفى كلّ التغذية الراجعة.** لا أرقام ولا أشرطة ولا أهداف = لا يمكن تحفيز اللاعب بنموٍّ مُمنوعٍ من رؤيته. **الدفء ≠ غياب التغذية الراجعة.**
3. **الميكانيكا «تجارب» لا «مهارات».** التلميع والتطهير بلا فشل، وقابلان للإكمال التلقائي، وبإيقاع واحد. لا منحنى إتقان = لا سبب لتكرارهما للمرّة العشرين.
4. **المحتوى مكتوب يدويًّا ومحدود.** قريتان وذكريان فقط. اللعبة الدافئة تحتاج *محرّكًا يولّد محتوًى لطيفًا*، لا ممرًّا من غرفتين.
5. **أصولٌ تبني الحلقة تقبع بلا استخدام.** حزمة **HarvestGarden** للزراعة/الحصاد مجرّد ديكور، و`DayCycleManager` يخفت إضاءة مشهدٍ فقط. *المواد الخام موجودة في المشروع لكنها غير موصولة.*

## ٤. الحلّ — ركائز التفاعل السبع (من ٠/٧ إلى ٧/٧)

ترفع الأطروحة التصميمية الخيالَ من **«قراءة ذكرى»** إلى **«إدارة الجوف»** — *ممارسة يوميّة* دافئة متراكمة تحافظ عليها. وكلّ ركيزة تقابل محرّكًا ناقصًا من ستارديو واحدًا لواحد.

| # | الركيزة | تعوّض (محرّك ستارديو) | شعور اللاعب | الحالة |
|---|---|---|---|---|
| **P1** | **اليوم الحيّ** — دورة صباح←ليل حقيقية تستيقظ فيها وتختمها | الحلقة المتراكمة | *«يومٌ جديد. بماذا أقضيه؟»* | ✅ مُنجزة (م ٦١) |
| **P2** | **لوحة الطلبات** — طابور متجدّد لا يفرغ من القرويين والذكريات | المفاجأة/التنوّع + محتوى لا ينضب | *«من يحتاجني اليوم؟ وجهٌ جديد!»* | ✅ مُنجزة (م ٦٢) |
| **P3** | **جَوفي** — متجرٌ تُجهّزه وترتّبه وتطوّره وتزيّنه | التملّك والتخصيص | *«هذا مِلكي، وهو يتحسّن.»* | ✅ مُنجزة (م ٦٤) |
| **P4** | **الحديقة والشاي** — ازرع الأعشاب، حضّر الشاي، استخدمه كأداة | نظام متشابك ١ | *«اللافندر جاهز — وقت التحضير.»* | ✅ مُنجزة (م ٦٥) |
| **P5** | **طاولة العمل الحيّة** — حِرفة متنوّعة، ممتعة، ذات منحنى مهارة | نظام متشابك ٢ + الإتقان | *«صرتُ بارعًا. مرّة أخرى.»* | ✅ مُنجزة (م ٦٦) |
| **P6** | **جدار الذكريات** — اجمع واربط وأكمِل شبكة الأصداء | أهداف اللاعب + مجموعة مرئية | *«اثنتان وأُكمل خيط دوريس!»* | ✅ مُنجزة (م ٦٣) |
| **P7** | **التقويم** — مهرجانات وزوّار وترقّب | تقويم الترقّب | *«مهرجان العسل بعد ٣ أيام!»* | ✅ مُنجزة (م ٦٧) |

> **انضباط جوهريّ:** كلّ ركيزة **اختياريّة وغير معاقِبة**. يمكن للّاعب تجاهل الحديقة، وإكمال كلّ حِرفة تلقائيًّا، وعدم التزيين، ويُنهي اللعبة. الركائز تضيف *جذبًا* لا *دفعًا* — عمق ستارديو مع بقاء ميثاق الدفء سليمًا.

## ٥. الحلقة الأساسية الجديدة (ما نبنيه فعلًا)

```
        ☀  صباحٌ جديد  — تستيقظ؛ تنزلق بطاقة «الأجندة»:
            • ٢–٣ قرويين على لوحة الطلبات
            • حالة الحديقة (هل نضج عشب؟)
            • اقتراح هدفٍ لطيف تضعه بنفسك
                       │  اللاعب يختار الترتيب — بلا مسارٍ مفروض
   ┌──────────┬────────┴────────┬──────────────┐
   ▼          ▼                  ▼              ▼
 افتح المتجر  اعتنِ بالحديقة     تجوّل بالقرية    أنجز الحِرفة
 خذ ذكرى     اسقِ/احصد         سلّم، تحدّث،     تلميع/تطهير/
            → حضّر الشاي       اكتشف الأصداء    نسج/فرز/إصغاء
   └──────────┴───────┬─────────┴──────────────┘
                      ▼
              💰 أنفِق / انمُ   ← الجزء المتراكم
        اشترِ رفًّا، حوضَ أعشاب، زينة، أداة؛ رتّب الجدار
                      ▼
        🌙 أغلق السجلّ — مراجعة احتفائية لأعمال اليوم،
        ما الذي نما، وما الذي كسبته، + تلميح للغد →
        نَم → حلمٌ اختياريّ → صباحٌ جديد (مختلف) ↺
```

**لماذا يُبقي اللاعب؟** لأنه *يتراكم* (مال ← رفوف/أحواض/أدوات ← سعةٌ أكبر ← مالٌ أكثر)، و*يقوده اللاعب* (الصباح يعرض خيارات لا ممرًّا)، و*يتنوّع* (اللوحة تتبدّل، والتقويم يحقن المهرجانات، والحديقة تدور)، و*يثمر غدًا*، و*لا يزال يروي القصّة* — دوريس وجيرولد ومارين وشبكة الأصداء صارت موزّعة عبر الحلقة بوصفها سبب معناها.

## ٦. لا شيء يُهدر — يُرقّى المحتوى الموجود

| الأصل الموجود | الدور القديم | الدور الجديد في الحلقة |
|---|---|---|
| **دوريس** | قرويّة تعليمية تختفي بعد اليوم ١ | أوّل **زبونة دائمة** على لوحة الطلبات؛ تنكشف خريطة ذكرياتها عبر زيارات كثيرة |
| **جيرولد** | خيار أخلاقيّ واحد يختفي بعد اليوم ٢ | **علاقة متكرّرة**؛ خيار اليوم ٢ هو *أوّل* فصول قصّته |
| **التلميع/التطهير** | الميكانيكتان الوحيدتان | اثنتان من **مجموعة أفعال متنامية** بمنحنى مهارة لطيف |
| **أحلام الذكريات** | مشهد ختام المهمة | **المكافأة** التي تفتحها بجمع/ربط الذكريات (قابلة للإعادة) |
| **شبكة الأصداء** | خطّ ضوءٍ موصوف | **لعبة فوقية حقيقية**: أكمِل الخيوط لمكافآت الأحلام والحكاية |
| **سجلّ المساء** | شاشة حفظ | **المراجعة اليومية + الاحتفاء + تلميح الغد** |
| **أصل HarvestGarden** | ديكور | **حلقة الحديقة** |
| **`DayCycleManager`** | يخفت إضاءة مشهد | **الساعة** التي تُسيّر اليوم الحيّ |
| **أبعاد `VillageState` الـ١٤** | بذورٌ بقيمٍ افتراضية | **تُكتب الآن وتُعرض** كتغذية راجعة دافئة (D-076) |

## ٧. الأولوية والحالة وترتيب البناء

بُني **العمود الفقري قبل الأطراف**: P1 ← P2 ← P6 ← P3 ← P4 ← P5 ← P7.

| المرحلة | الركيزة | المُسلّم | الحالة |
|---|---|---|---|
| ٦١ | P1 | اليوم الحيّ: بطاقة الأجندة، `DailyLoopService`، مراجعة المساء + تلميح الغد | ✅ مُنجزة |
| ٦٢ | P2 | لوحة الطلبات التفاعلية `[B]`، `RequestVisitService`، اقتصاد العملة الحقيقي، مخطّط حفظ v3 | ✅ مُنجزة |
| ٦٣ | P6 | جدار الذكريات `[M]`، `EchoWebService`، خيوط أصداء للملاحقة والإكمال | ✅ مُنجزة |
| ٦٤ | P3 | `HollowProgressionService`، المتجر `[U]`، واجهة كيس العملة | ✅ مُنجزة |
| ٦٥ | P4 | `GardenService`، الحديقة `[G]`، ازرع←حضّر←استخدم الشاي | ✅ مُنجزة |
| ٦٦ | P5 | `WorkbenchService`، الطاولة `[K]`، `CraftVerbs` + إتقان «يد الحارس» | ✅ مُنجزة |
| ٦٧ | P7 | `AlmanacService` — يوم السوق / المهرجانات / الشاعر / أعياد الميلاد | ✅ مُنجزة |
| ٦٨ | P6+ | قرويّون توليديّون عبر مكتبة المقاطع؛ توسيع تجمّع الطلبات | ✅ مُنجزة (قوائم مدمجة؛ تُوسَّع بأصول بلا كود) |
| ٦٩ | الكلّ | جولة التغذية الراجعة الدافئة (D-076): كيس العملة، نسبة المجموعة، الأجندة — احتفائية | 🟡 الجوهر مُنجز |
| **٧٠** | الكلّ | **اختبار G-Engage** — ٣ أيام متكرّرة؛ قياس مقياس «اليوم الرابع الطوعي» | 🟡 الخطة سُلّمت |
| ٧١ | الكلّ | الضبط من الاختبار؛ توزيع البِيتات الثقيلة عبر الحلقة | 🟡 موثّق |
| ٧٢ | الكلّ | جولة «صدق التسويق»: مواءمة نصوص README/ستيم مع الحلقة الحقيقية | 🟡 موثّق |

## ٨. مقياس النجاح الجديد (الوحيد الصادق)

> **البوابة G-Engage:** يلعب ٢٠ مختبِرًا من الجمهور الدافئ **ثلاثة أيام داخل اللعبة** (~٩٠ دقيقة). يبدأ **١٥/٢٠ على الأقل (٧٥٪)** *يومًا رابعًا* طوعًا دون تحفيز، ويُسمّي **≥٦٠٪** هدفًا وضعوه بأنفسهم («أطوّر رفّي»، «أُكمل خيط دوريس»، «أزرع الناردين»).

الاستمرار الذاتيّ هو المقياس الحقيقي الوحيد للحلقة الدافئة. اختيار اليوم الرابع ← لدينا لعبة. التوقّف عند اليوم الثالث ← لدينا عرضٌ سرديّ أطول.

## ٩. الضوابط التي يرثها كلّ تحسين

1. **ميثاق الدفء قائم** — لا حالات فشل، ولا مؤقّتات إلّا اختياريًّا، والرفض مُكرَّم دائمًا، واللطف هو الأمثل، والإكمال التلقائيّ في كلّ لعبة مصغّرة. **لا تظهر كلمة «فشل» أبدًا.**
2. **لا تربّح مظلم** — أبدًا. لا غاتشا ولا طاقة ولا صناديق غنائم ولا دفع‑للفوز. الثقة هي الحصن.
3. **التغذية الراجعة احتفائية لا مُقلِقة (D-076)** — اعرض المال والنموّ ونسبة المجموعة والأجندة — بدفء. لا عدّ تنازليّ معاقِب ولا رقم أحمر «متأخّر عن الجدول».
4. **سقف جودة الكتابة ثابت** — القرويّون المكتوبون يدويًّا يبقون كذلك؛ والتنوّع التوليديّ يستخدم مكتبة المقاطع بالمستوى نفسه. **الحوار المولّد بالذكاء الاصطناعي ممنوع.**
5. **انضباط معماريّ** — احترِم رسم asmdef؛ الحالة الجديدة في `VillageState`؛ الأحداث الجديدة على EventBus؛ البيانات في ScriptableObjects؛ وكلّ شيء قابل لإعادة التشغيل خلف `🚀 Build Everything`.

## ١٠. قراءة السوق (هل ستُباع؟)

- **تجاريًّا:** كتحفة من ٧٥ دقيقة ← ~١٥٠–٢٥٠ ألف نسخة، ~٧٢/١٠٠، مع استرداد عند سعر ٢٤٫٩٩$. **ومع شحن الحلقة ← سقفٌ بين ١٤–١٨ مليون دولار** وذيلٌ يمتدّ سنوات.
- **مجتمع الألعاب الدافئة:** عبارة «جميلة لكن لا شيء لفعله» هي موت الألعاب الدافئة الرقيقة. الحلقة *الحقيقية* تقلب أعلى مراجعة إلى *«نويتُ لعب يومٍ واحد فإذا الساعة الواحدة فجرًا.»*
- **الاستبقاء/LiveOps:** الفئة تعيش على **اليوم ٣٠**. لم يكن هناك يوم ٢. الحلقة تخلق اليوم ٧ واليوم ٣٠.

## ١١. الخطوات التالية الموصى بها (بالترتيب)

1. **نفّذ G-Engage (المرحلة ٧٠)** — اختبار الأيام الثلاثة المتكرّرة؛ وقِس *اليوم الرابع الطوعي*. هذا أهمّ إجراء منفرد؛ وكلّ ما عداه ضبط.
2. **اضبط الإيقاع (المرحلة ٧١)** — وزّع بِيتات الحزن الثقيلة عبر الحلقة عبر «نطاق الكوميديا/الحزن» كي لا تنزلق النبرة الدافئة إلى ثِقلٍ متواصل.
3. **جولة «صدق التسويق» (المرحلة ٧٢)** — حدّث نصوص README/ستيم لتطابق الحلقة الحقيقية وتوقف سبب الاسترداد الناتج عن سوء التوقّع.
4. **عمّق المحتوى (المرحلة ٦٨+)** — اكتب ٤–٦ قرويين إضافيين وخيوط أصداء من مكتبة المقاطع لتمديد الموسم نحو وعد الـ٤٠ ساعة.
5. **أسقف المهارة (P5)** — أكمِل منحنيات الإتقان اللطيفة في التلميع/التطهير/النسج كي تبقى الحِرفة العشرون مُرضية.
6. **احمِ الجائزة** — لا تدع الحلقة تُميّع الكتابة أبدًا. الفكرة هي الأفضل في الفئة؛ ونحن نبني *سبب الاستمرار في كشفها*.

---

> **خلاصة المجلس / Board's bottom line:** *"Beautiful, but there's nothing to do" → "I meant to play one day and it's 1 a.m."* — هذا الفرق هو ما يفصل بين تحفةٍ بـ٧٢/١٠٠ ومرشّحٍ للعبة العام الدافئة. الفكرة جاهزة؛ وما نبنيه الآن هو **سبب البقاء**.

*وثيقة v1.0 — مُجمّعة من «دليل التفاعل» على فرع `feat/mission-1-2-architecture` · ٢٠٢٦ · جزء من حافظة Abdulmalek Agents.*
