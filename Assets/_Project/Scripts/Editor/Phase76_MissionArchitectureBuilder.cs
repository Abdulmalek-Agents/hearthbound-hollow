#if UNITY_EDITOR
// =============================================================================
// Phase76_MissionArchitectureBuilder.cs — Hearthbound Hollow
// Creates ALL 30 NarrativeLevelConfigSOs and ALL 20+ VillagerProfileSOs.
// Idempotent: safe to run repeatedly. Chained into Build Everything Step 25.
//
// Menu: Hearthbound → ⚙️ Advanced → 🎯 Phase 76 — Build 30-Mission Architecture
// Phase 76.
// =============================================================================
using System.IO;
using UnityEngine;
using UnityEditor;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Editor
{
    public static class Phase76_MissionArchitectureBuilder
    {
        private const string kMissionsDir  = "Assets/_Project/Resources/Missions";
        private const string kVillagersDir = "Assets/_Project/Resources/Villagers";

        [MenuItem("Hearthbound/\u2699\uFE0F Advanced/\uD83C\uDFAF Phase 76 \u2014 Build 30-Mission Architecture",
                   priority = 1076)]
        public static void BuildAll()
        {
            Debug.Log("[Phase76] Building 30-Mission Architecture...");
            EnsureDir(kMissionsDir);
            EnsureDir(kVillagersDir);

            var villagers = BuildVillagerProfiles();
            BuildMissionConfigs(villagers);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase76] ✅ 30-Mission Architecture complete.");
        }

        // =====================================================================
        //  VILLAGER PROFILES
        // =====================================================================

        private static System.Collections.Generic.Dictionary<string,VillagerProfileSO> BuildVillagerProfiles()
        {
            var map = new System.Collections.Generic.Dictionary<string,VillagerProfileSO>();

            var defs = new[]
            {
                ("doris_baker",
                 "Doris","Doris Fen", 61, "Baker",
                 "Warm, practical. Uses 'we' to mean 'I' when vulnerable. Reaches for flour when she doesn't know what else to reach for.",
                 "The grief she baked into the bread the day after her mother died. She didn't know any other way to mourn.",
                 false, -1, "en_US-kathleen-medium", "en+f3", "#D4860A", 0),

                ("gerrold_pell",
                 "Gerrold","Gerrold Pell", 67, "Retired Carpenter",
                 "Talks in paragraphs. Has been saving these words for three years. He doesn't rush. Don't rush him.",
                 "A Sunday morning he can't let himself forget even though it's the only one left and he keeps wearing it thin.",
                 false, -1, "en_US-danny-low", "en+m3", "#6B8FA8", 1),

                ("thomas_miller",
                 "Thomas","Thomas Miller", 58, "Miller",
                 "Short sentences. Long pauses. He was a man of few words before the guilt; now he's even quieter.",
                 "He watched a family go hungry rather than speak up about the bad grain. The silence was easier. He's never forgiven the silence.",
                 false, -1, "en_US-danny-low", "en+m4", "#8B7355", 2),

                ("vera_hartwell",
                 "Dr. Vera","Dr. Vera Hartwell", 44, "Apothecary",
                 "Clinical precision with warmth underneath. She diagnoses feelings the way she diagnoses ailments: accurately, gently, late.",
                 "A patient she couldn't save. She still knows the exact compound she got wrong. She wrote it in the margin of her journal in pencil so she could erase it later. She never erased it.",
                 true, 6, "en_US-amy-low", "en+f4", "#4A7A7A", 3),

                ("aldine_voss",
                 "Miss Aldine","Aldine Voss", 67, "Retired Schoolteacher",
                 "Still talks like she's at the front of a class. But the lesson she keeps reviewing is one she failed.",
                 "A student she didn't protect. The girl left the village. Miss Aldine is still teaching, but the memory sits in the back row and never raises its hand.",
                 false, -1, "en_US-kathleen-medium", "en+f2", "#7A6A5A", 4),

                ("owen_crane",
                 "Owen","Owen Crane", 52, "Blacksmith",
                 "Proud in a way that sounds like humility until you listen long enough. He knows what he did.",
                 "A trophy for craftsmanship he received the year he produced his worst work. He's kept it on the shelf because taking it down feels like admitting it.",
                 false, -1, "en_US-danny-low", "en+m2", "#5A4A3A", 5),

                ("clara_marsh",
                 "Clara","Clara Marsh", 49, "Innkeeper",
                 "Cheerful with a practiced steadiness. She refills your cup before it's empty. She's terrified of empty things.",
                 "A friendship that dissolved when she got too busy keeping an inn full of people she'd never see twice. She forgot the only person worth keeping.",
                 false, -1, "en_US-kathleen-medium", "en+f5", "#8B6A4A", 8),

                ("edmund_pace",
                 "Edmund","Edmund Pace", 61, "Village Councillor",
                 "Politically fluent. Speaks in 'we' and 'the village'. Has not used 'I' in a morally significant sentence in thirty years.",
                 "He cast the deciding vote that sealed the wrongdoing. He told himself it was for the village. He still tells himself this every morning.",
                 true, 0, "en_US-danny-low", "en+m5", "#5A6A7A", 10),

                ("ruth_calloway",
                 "Ruth","Ruth Calloway", 55, "Seamstress",
                 "Precise hands, imprecise heart. Counts stitches when she's anxious. She has been counting stitches for thirty years.",
                 "She hemmed the dress for the funeral of someone whose death she helped cause. She made perfect tiny stitches. She still makes perfect tiny stitches.",
                 true, 1, "en_US-kathleen-medium", "en+f3", "#8A5A7A", 11),

                ("finn_arley",
                 "Finn","Finn Arley", 44, "Shepherd",
                 "Sun-weathered. Does not explain himself. A man more comfortable with sheep than with the weight of what he did.",
                 "He drove a family's sheep away in the night. 'An accident.' He knows it wasn't. The sheep knew too.",
                 true, 2, "en_US-danny-low", "en+m3", "#6A7A5A", 12),

                ("nell_prior",
                 "Nell","Nell Prior", 8, "Child",
                 "Curious. Unfiltered. She describes what she saw without the vocabulary to understand it, which makes it worse.",
                 "She saw something through a window on a night the adults told her she was asleep. She described it to her mother. Her mother said she'd dreamed it. She knows she didn't.",
                 true, 3, "en_US-amy-low", "en+f5", "#D4A870", 13),

                ("august_wren",
                 "August","August Wren", 70, "Clockmaker",
                 "Meticulous. Every sentence arrived at the correct word. He is a man who believes in precision and has made one irrevocable imprecision.",
                 "Every clock in his shop shows the same wrong time. He knows the right time. He refuses to set them to it.",
                 true, 7, "en_US-danny-low", "en+m4", "#7A7A6A", 15),

                ("iris_holt",
                 "Iris","Iris Holt", 38, "Beekeeper",
                 "Soft voice. Patient. Understands grief the way she understands bees: as something living, something that can sting, something that makes honey if you're careful.",
                 "Her bees left in the same week her daughter did. She hasn't decided which departure she's still grieving.",
                 false, -1, "en_US-kathleen-medium", "en+f4", "#D4A840", 17),

                ("aldous_whitmore",
                 "Mayor Whitmore","Aldous Whitmore", 63, "Village Mayor",
                 "Carries the weight of the village as if it were an honour and not a sentence. Speaks slowly. Looks at his hands when he's lying.",
                 "He organised the sealing. He told himself it protected the village. He was partially right. He knows which part he was wrong about.",
                 true, 5, "en_US-danny-low", "en+m5", "#5A5A6A", 20),

                ("marin_vellis",
                 "Marin","Marin Vellis", 38, "Previous Hollow-Keeper",
                 "Precise. Dry wit concealing deep feeling. Every word arrived at. Her letters are the finest writing in the game.",
                 "She discovered what the village sealed. She tried to return it. The village didn't let her. She left breadcrumbs.",
                 false, -1, "en_GB-semaine-medium", "en+f2", "#9DB6CB", -1),

                ("pickle",
                 "Pickle","Pickle", -1, "Familiar",
                 "Italic. Every line. Like a footnote that escaped the text and started making observations.",
                 "Pickle knows more than he says. Pickle always knows more than he says.",
                 false, -1, "en_US-amy-low", "en+f5", "#C8A860", -1),
            };

            foreach (var d in defs)
            {
                string path = $"{kVillagersDir}/Villager_{d.Item1}.asset";
                var so = AssetDatabase.LoadAssetAtPath<VillagerProfileSO>(path)
                      ?? CreateSO<VillagerProfileSO>(path);

                so.villagerId         = d.Item1;
                so.displayName        = d.Item2;
                so.fullName           = d.Item3;
                so.age                = d.Item4;
                so.occupation         = d.Item5;
                so.voiceSignature     = d.Item6;
                so.emotionalCore      = d.Item7;
                so.isSealedFragmentHolder = d.Item8;
                so.sealedFragmentIndex    = d.Item9;
                so.piperModel             = d.Item10;
                so.espeakVariant          = d.Item11;
                so.portraitTintHex        = d.Item12;
                so.introducedInMission    = d.Item13;

                EditorUtility.SetDirty(so);
                map[d.Item1] = so;
            }

            return map;
        }

        // =====================================================================
        //  MISSION CONFIGS — all 30
        // =====================================================================

        private static void BuildMissionConfigs(
            System.Collections.Generic.Dictionary<string,VillagerProfileSO> v)
        {
            var defs = new MissionDef[]
            {
                // ACT 1 — AUTUMN OPENING (Missions 0-9)
                new(0, "M01_OpeningTheHollow",
                    "Opening the Hollow", "The first day. The first customer. The first memory.",
                    1, Season.Autumn, "doris_baker",
                    "DOR-001", MemoryOrbColor.Grief, 0.55f, 2, 0.6f, false, -1,
                    false, "", "M01_Dream_FirstLoaves", 60f,
                    "Mission01_Start", "Mission_01_Doris",
                    "Assets/_Project/Scenes/Mission01_Hollow.unity", false,
                    8, 5, 0),

                new(1, "M02_TheWidowersRequest",
                    "The Widower's Request", "An old man, a Sunday morning, and a memory that holds a name he can't let go of.",
                    1, Season.Autumn, "gerrold_pell",
                    "GER-WIFE-01", MemoryOrbColor.Grief, 0.5f, 3, 0.8f, false, -1,
                    false, "", "M02_Dream_GerroldWife", 55f,
                    "Mission02_Start", "Mission_02_Gerrold",
                    "Assets/_Project/Scenes/Mission02_Cottage.unity", true,
                    10, 8, 3),

                new(2, "M03_TheMillersGuilt",
                    "The Miller's Guilt", "Thomas Miller hasn't slept well in eleven years. He knows exactly why.",
                    1, Season.Autumn, "thomas_miller",
                    "TOM-GRAIN-01", MemoryOrbColor.Shame, 0.4f, 4, 0.7f, false, -1,
                    false, "", "M03_Dream_MillerGrain", 40f,
                    "Mission03_Start", "Mission_03_Thomas",
                    "Assets/_Project/Scenes/Mission03_Mill.unity", false,
                    8, 6, 0),

                new(3, "M04_TheApothecarySecret",
                    "The Apothecary's Secret", "Dr. Vera Hartwell would like to make her guilt clinical. She cannot.",
                    1, Season.Autumn, "vera_hartwell",
                    "VER-PATIENT-01", MemoryOrbColor.Guilt, 0.45f, 3, 0.85f, false, -1,
                    false, "", "M04_Dream_VeraPatient", 45f,
                    "Mission04_Start", "Mission_04_Vera",
                    "Assets/_Project/Scenes/Mission04_Apothecary.unity", false,
                    9, 7, 5),

                new(4, "M05_TheSchoolteachersRegret",
                    "The Schoolteacher's Regret", "Miss Aldine has been teaching wrong lessons from the right desk for thirty years.",
                    1, Season.Autumn, "aldine_voss",
                    "ALD-STUDENT-01", MemoryOrbColor.Regret, 0.5f, 2, 0.65f, false, -1,
                    false, "", "M05_Dream_AldineStudent", 40f,
                    "Mission05_Start", "Mission_05_Aldine",
                    "Assets/_Project/Scenes/Mission05_Schoolhouse.unity", false,
                    8, 6, 0),

                new(5, "M06_TheBlacksmithsPride",
                    "The Blacksmith's Pride", "Owen Crane has a trophy he never deserved, and he'd like you to take it off his hands.",
                    1, Season.Autumn, "owen_crane",
                    "OWN-TROPHY-01", MemoryOrbColor.Pride, 0.65f, 1, 0.7f, false, -1,
                    false, "", "M06_Dream_CraneTrophy", 35f,
                    "Mission06_Start", "Mission_06_Owen",
                    "Assets/_Project/Scenes/Mission06_Smithy.unity", false,
                    9, 5, 0),

                new(6, "M07_TheFirstRevelation",
                    "The First Revelation", "Three kitchens. One Sunday morning. The Memory Wall shows you something you weren't expecting.",
                    1, Season.Autumn, "doris_baker",
                    "ECHO-REVEAL-01", MemoryOrbColor.Awe, 0.9f, 0, 0.9f, false, -1,
                    false, "", "M07_Dream_FirstEcho", 50f,
                    "Mission07_Start", "Mission_07_Revelation",
                    "Assets/_Project/Scenes/Mission07_Hollow.unity", false,
                    0, 0, 10),

                new(7, "M08_TheMarketDay",
                    "The Market Day", "Three strangers arrive with memories from roads the village has never walked.",
                    1, Season.Autumn, "marin_vellis",
                    "MKT-TRAVEL-01", MemoryOrbColor.Wonder, 0.7f, 1, 0.6f, false, -1,
                    true, "Market Day in Saltmere — three traveling sellers", "M08_Dream_MarketDay", 30f,
                    "Mission08_Start", "Mission_08_Market",
                    "Assets/_Project/Scenes/Mission08_Lane.unity", false,
                    12, 0, 5),

                new(8, "M09_TheInnkeepersLoneliness",
                    "The Innkeeper's Loneliness", "Clara Marsh keeps every room full. She keeps herself empty.",
                    1, Season.Autumn, "clara_marsh",
                    "CLA-FRIEND-01", MemoryOrbColor.Longing, 0.45f, 3, 0.6f, false, -1,
                    false, "", "M09_Dream_ClaraFriend", 40f,
                    "Mission09_Start", "Mission_09_Clara",
                    "Assets/_Project/Scenes/Mission09_Inn.unity", false,
                    8, 6, 0),

                new(9, "M10_TheFirstFrost",
                    "The First Frost", "Winter settles on Saltmere. Two new arcs begin. The predecessor trail goes cold — or does it?",
                    1, Season.Winter, "pickle",
                    "FROST-TRANS-01", MemoryOrbColor.Nostalgia, 0.7f, 0, 0.5f, false, -1,
                    false, "", "M10_Dream_Frost", 35f,
                    "Mission10_Start", "Mission_10_Frost",
                    "Assets/_Project/Scenes/Mission10_Lane.unity", false,
                    0, 0, 8),

                // ACT 2 — DEEP WINTER (Missions 10-19)
                new(10, "M11_TheCouncilmansBargain",
                    "The Councillman's Bargain", "Edmund Pace would like to trade a vote he made thirty years ago for some peace tonight.",
                    2, Season.Winter, "edmund_pace",
                    "EDM-VOTE-01", MemoryOrbColor.Guilt, 0.3f, 5, 0.9f, true, 0,
                    false, "", "M11_Dream_EdmundVote", 50f,
                    "Mission11_Start", "Mission_11_Edmund",
                    "Assets/_Project/Scenes/Mission11_Council.unity", false,
                    10, 8, 8),

                new(11, "M12_TheSeamstressThread",
                    "The Seamstress's Thread", "Ruth Calloway has been making perfect stitches for thirty years. The first one is still wrong.",
                    2, Season.Winter, "ruth_calloway",
                    "RTH-DRESS-01", MemoryOrbColor.Shame, 0.35f, 4, 0.8f, true, 1,
                    false, "", "M12_Dream_RuthDress", 45f,
                    "Mission12_Start", "Mission_12_Ruth",
                    "Assets/_Project/Scenes/Mission12_Tailor.unity", false,
                    10, 7, 8),

                new(12, "M13_TheShepherdsLostFlock",
                    "The Shepherd's Lost Flock", "Finn Arley drove a family's sheep away one night. He says he doesn't remember why.",
                    2, Season.Winter, "finn_arley",
                    "FIN-SHEEP-01", MemoryOrbColor.Guilt, 0.4f, 4, 0.75f, true, 2,
                    false, "", "M13_Dream_FinnSheep", 40f,
                    "Mission13_Start", "Mission_13_Finn",
                    "Assets/_Project/Scenes/Mission13_Pasture.unity", false,
                    10, 7, 8),

                new(13, "M14_TheChildsGame",
                    "The Child's Game", "Nell Prior, age 8, saw something through a window. Her mother said she dreamed it.",
                    2, Season.Winter, "nell_prior",
                    "NEL-WINDOW-01", MemoryOrbColor.Fear, 0.7f, 1, 0.4f, true, 3,
                    false, "", "M14_Dream_NellWindow", 55f,
                    "Mission14_Start", "Mission_14_Nell",
                    "Assets/_Project/Scenes/Mission14_Lane.unity", false,
                    8, 4, 10),

                new(14, "M15_TheLockedRoom",
                    "The Locked Room", "An abandoned cottage holds a memory with no owner. Or perhaps one who never expected to be found.",
                    2, Season.Winter, "marin_vellis",
                    "LOCK-ROOM-01", MemoryOrbColor.Dread, 0.25f, 5, 0.95f, true, 4,
                    false, "", "M15_Dream_LockedRoom", 60f,
                    "Mission15_Start", "Mission_15_LockedRoom",
                    "Assets/_Project/Scenes/Mission15_Cottage.unity", false,
                    0, 0, 15),

                new(15, "M16_TheClockmakersPrecision",
                    "The Clockmaker's Precision", "August Wren keeps every clock set to the wrong time. He knows the right time. He refuses it.",
                    2, Season.Winter, "august_wren",
                    "AUG-CLOCK-01", MemoryOrbColor.Bitterness, 0.4f, 3, 0.8f, false, -1,
                    false, "", "M16_Dream_AugustClock", 45f,
                    "Mission16_Start", "Mission_16_August",
                    "Assets/_Project/Scenes/Mission16_Clockshop.unity", false,
                    10, 7, 5),

                new(16, "M17_TheFestivalOfMemory",
                    "The Festival of Memory", "The village gathers once a year to remember what it chooses to remember. Five stories. One evening.",
                    2, Season.Winter, "pickle",
                    "FEST-MEM-01", MemoryOrbColor.Nostalgia, 0.8f, 0, 0.7f, false, -1,
                    true, "Annual Festival of Memory — five optional stories", "M17_Dream_Festival", 30f,
                    "Mission17_Start", "Mission_17_Festival",
                    "Assets/_Project/Scenes/Mission17_Square.unity", false,
                    0, 0, 5),

                new(17, "M18_TheBeekeepersGrief",
                    "The Beekeeper's Grief", "Iris Holt's bees left in the same week her daughter did. She hasn't decided which departure she's still grieving.",
                    2, Season.Spring, "iris_holt",
                    "IRS-BEES-01", MemoryOrbColor.Grief, 0.5f, 2, 0.65f, false, -1,
                    false, "", "M18_Dream_IrisBees", 45f,
                    "Mission18_Start", "Mission_18_Iris",
                    "Assets/_Project/Scenes/Mission18_Meadow.unity", true,
                    9, 6, 0),

                new(18, "M19_TheOldRivalry",
                    "The Old Rivalry Resolved", "Thomas Miller and Owen Crane share a night neither can account for completely.",
                    2, Season.Spring, "thomas_miller",
                    "RIVAL-CROSS-01", MemoryOrbColor.Shame, 0.35f, 4, 0.8f, false, -1,
                    false, "", "M19_Dream_Rivalry", 40f,
                    "Mission19_Start", "Mission_19_Rivalry",
                    "Assets/_Project/Scenes/Mission19_Hollow.unity", false,
                    10, 0, 5),

                new(19, "M20_TheEchoWebExpands",
                    "The Echo Web Expands", "Five fragments. Six faces. One week in 1993 that the village calls the Forgotten Year.",
                    2, Season.Spring, "marin_vellis",
                    "ECHO-MAJOR-01", MemoryOrbColor.Awe, 0.95f, 0, 0.95f, false, -1,
                    false, "", "M20_Dream_MajorReveal", 70f,
                    "Mission20_Start", "Mission_20_EchoWeb",
                    "Assets/_Project/Scenes/Mission20_Hollow.unity", false,
                    0, 0, 20),

                // ACT 3 — SPRING RECKONING (Missions 20-29)
                new(20, "M21_TheMayorsConfession",
                    "The Mayor's Confession", "Aldous Whitmore has been carrying the Forgotten Year longer than anyone. He's tired.",
                    3, Season.Spring, "aldous_whitmore",
                    "MAY-CONF-01", MemoryOrbColor.Guilt, 0.3f, 5, 0.95f, true, 5,
                    false, "", "M21_Dream_MayorConf", 60f,
                    "Mission21_Start", "Mission_21_Mayor",
                    "Assets/_Project/Scenes/Mission21_MayorHall.unity", false,
                    12, 10, 10),

                new(21, "M22_TheDoctorsReturn",
                    "The Doctor's Return", "Dr. Vera Hartwell comes back. Her sealed memory connects to the family that was hurt.",
                    3, Season.Spring, "vera_hartwell",
                    "VER-RETURN-01", MemoryOrbColor.Guilt, 0.35f, 4, 0.9f, true, 6,
                    false, "", "M22_Dream_VeraReturn", 55f,
                    "Mission22_Start", "Mission_22_VeraReturn",
                    "Assets/_Project/Scenes/Mission22_Apothecary.unity", false,
                    12, 9, 8),

                new(22, "M23_TheLettersInTheNook",
                    "The Letters in the Nook", "Marin's final letter in the Reading Nook reveals who the family was.",
                    3, Season.Spring, "marin_vellis",
                    "MAR-FINAL-01", MemoryOrbColor.Longing, 0.9f, 0, 0.8f, false, -1,
                    false, "", "M23_Dream_MarinLetter", 50f,
                    "Mission23_Start", "Mission_23_Letters",
                    "Assets/_Project/Scenes/Mission23_Hollow.unity", false,
                    0, 0, 15),

                new(23, "M24_TheMissingFragment",
                    "The Missing Fragment", "Fragment 7 was sold to the Hollow years ago. It's been on your shelf this whole time.",
                    3, Season.Spring, "marin_vellis",
                    "MISS-FRAG-01", MemoryOrbColor.Dread, 0.4f, 4, 0.85f, true, 8,
                    false, "", "M24_Dream_MissingFrag", 45f,
                    "Mission24_Start", "Mission_24_Missing",
                    "Assets/_Project/Scenes/Mission24_Hollow.unity", false,
                    0, 0, 10),

                new(24, "M25_TheHearing",
                    "The Hearing", "The village gathers. Can the truth be presented? Should it be?",
                    3, Season.Spring, "aldous_whitmore",
                    "HEAR-TRUTH-01", MemoryOrbColor.Awe, 0.8f, 1, 0.9f, false, -1,
                    true, "The Village Hearing — truth or silence?", "M25_Dream_Hearing", 60f,
                    "Mission25_Start", "Mission_25_Hearing",
                    "Assets/_Project/Scenes/Mission25_Hall.unity", false,
                    0, 0, 5),

                new(25, "M26_TheDreamOfAllDreams",
                    "The Dream of All Dreams", "One night. Every memory on your shelf speaks at once.",
                    3, Season.Spring, "marin_vellis",
                    "DREAM-ALL-01", MemoryOrbColor.Wonder, 0.95f, 0, 0.95f, false, -1,
                    false, "", "M26_Dream_AllDreams", 120f,
                    "Mission26_Start", "Mission_26_AllDreams",
                    "Assets/_Project/Scenes/Mission26_Dreamspace.unity", false,
                    0, 0, 0),

                new(26, "M27_TheClockmakersFinalGift",
                    "The Clockmaker's Final Gift", "August Wren sets his clocks to the right time. Fragment 8, released willingly.",
                    3, Season.Spring, "august_wren",
                    "AUG-FINAL-01", MemoryOrbColor.Tenderness, 0.8f, 0, 0.85f, true, 7,
                    false, "", "M27_Dream_AugustFinal", 50f,
                    "Mission27_Start", "Mission_27_August",
                    "Assets/_Project/Scenes/Mission27_Clockshop.unity", false,
                    10, 8, 5),

                new(27, "M28_TheSealedMemoryUnlocked",
                    "The Sealed Memory Unlocked", "All nine fragments. The Memory Wall shows the complete picture for the first time.",
                    3, Season.Summer, "marin_vellis",
                    "SEAL-FULL-01", MemoryOrbColor.Awe, 1.0f, 0, 1.0f, false, -1,
                    false, "", "M28_Dream_SealedFull", 90f,
                    "Mission28_Start", "Mission_28_Sealed",
                    "Assets/_Project/Scenes/Mission28_Hollow.unity", false,
                    0, 0, 0),

                new(28, "M29_TheChoice",
                    "The Choice", "Return it. Keep it. Sell it. Or let it drift. There is no wrong answer. There are only answers.",
                    3, Season.Summer, "aldous_whitmore",
                    "CHOICE-FINAL-01", MemoryOrbColor.Grief, 0.8f, 0, 0.95f, false, -1,
                    false, "", "M29_Dream_Choice", 80f,
                    "Mission29_Start", "Mission_29_Choice",
                    "Assets/_Project/Scenes/Mission29_Hollow.unity", false,
                    0, 0, 0),

                new(29, "M30_TheNewKeeper",
                    "The New Keeper", "Marin's last message. The village knows — or doesn't. You leave something for whoever comes next.",
                    3, Season.Summer, "marin_vellis",
                    "FINAL-KEEP-01", MemoryOrbColor.Longing, 0.9f, 0, 0.8f, false, -1,
                    false, "", "M30_Dream_NewKeeper", 90f,
                    "Mission30_Start", "Mission_30_Epilogue",
                    "Assets/_Project/Scenes/Mission30_Epilogue.unity", false,
                    0, 0, 0),
            };

            foreach (var d in defs)
            {
                string path = $"{kMissionsDir}/Mission_{d.Index:D2}_Config.asset";
                var so = AssetDatabase.LoadAssetAtPath<NarrativeLevelConfigSO>(path)
                      ?? CreateSO<NarrativeLevelConfigSO>(path);

                so.missionIndex     = d.Index;
                so.missionId        = d.Id;
                so.missionTitle     = d.Title;
                so.missionSubtitle  = d.Subtitle;
                so.act              = d.Act;
                so.season           = d.Season;
                so.primaryOrbId     = d.OrbId;
                so.orbColor         = d.OrbColor;
                so.orbClarity01     = d.Clarity;
                so.orbCrackCount    = d.Cracks;
                so.orbWeight01      = d.Weight;
                so.containsSealedFragment = d.HasFragment;
                so.sealedFragmentIndex    = d.FragmentIdx;
                so.isAlmanacEvent   = d.IsAlmanac;
                so.almanacHeadline  = d.AlmanacLine;
                so.dreamYarnNode    = d.DreamNode;
                so.dreamDurationSeconds = d.DreamDur;
                so.yarnStartNode    = d.YarnStart;
                so.yarnFile         = d.YarnFile;
                so.scenePath        = d.Scene;
                so.requiresGarden   = d.NeedsGarden;
                so.coinRewardBase   = d.CoinReward;
                so.trustGrant       = d.Trust;
                so.warmthGrant      = d.Warmth;

                // Link primary villager.
                if (v.TryGetValue(d.VillagerId, out var vill))
                    so.primaryVillager = vill;

                EditorUtility.SetDirty(so);
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static T CreateSO<T>(string path) where T : ScriptableObject
        {
            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        private static void EnsureDir(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        // ─── Inline mission definition struct ────────────────────────────────

        private readonly struct MissionDef
        {
            public int Index; public string Id, Title, Subtitle;
            public int Act; public Season Season;
            public string VillagerId;
            public string OrbId; public MemoryOrbColor OrbColor;
            public float Clarity; public int Cracks; public float Weight;
            public bool HasFragment; public int FragmentIdx;
            public bool IsAlmanac; public string AlmanacLine;
            public string DreamNode; public float DreamDur;
            public string YarnStart, YarnFile, Scene;
            public bool NeedsGarden;
            public int CoinReward, Trust, Warmth;

            public MissionDef(int i,string id,string t,string sub,int act,Season s,string vid,
                string oid,MemoryOrbColor oc,float cl,int cr,float w,bool hf,int fi,
                bool ia,string al,string dn,float dd,string ys,string yf,string sc,bool ng,
                int coin,int trust,int warmth)
            {
                Index=i; Id=id; Title=t; Subtitle=sub; Act=act; Season=s; VillagerId=vid;
                OrbId=oid; OrbColor=oc; Clarity=cl; Cracks=cr; Weight=w;
                HasFragment=hf; FragmentIdx=fi; IsAlmanac=ia; AlmanacLine=al;
                DreamNode=dn; DreamDur=dd; YarnStart=ys; YarnFile=yf; Scene=sc;
                NeedsGarden=ng; CoinReward=coin; Trust=trust; Warmth=warmth;
            }
        }
    }
}
#endif
