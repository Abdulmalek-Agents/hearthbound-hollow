// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / RippleEngine
//
// Applies a TariffSO to the VillageState. In Mission 1-2 only a 2-villager
// radius is used (Doris + Gerrold). The architectural seam supports the
// full Codex 08 § 4 ripple propagation in Mission 3+ without rewrite.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    public class RippleEngine
    {
        public void ApplyTariff(TariffSO tariff, VillageState state, string targetVillagerId)
        {
            if (tariff == null || state == null) return;

            state.coin = Mathf.Max(0, state.coin + tariff.coinDelta);

            if (!string.IsNullOrEmpty(targetVillagerId))
            {
                if (targetVillagerId.Equals("doris", System.StringComparison.OrdinalIgnoreCase))
                    state.trustDoris = VillageState.Adjust(state.trustDoris, tariff.trustDeltaTarget);
                else if (targetVillagerId.Equals("gerrold", System.StringComparison.OrdinalIgnoreCase))
                    state.trustGerrold = VillageState.Adjust(state.trustGerrold, tariff.trustDeltaTarget);
            }

            // Ripple to nearby villagers (M1-2: the *other* of the two main villagers)
            if (targetVillagerId == "doris")
                state.trustGerrold = VillageState.Adjust(state.trustGerrold, tariff.trustRippleNeighbors);
            else if (targetVillagerId == "gerrold")
                state.trustDoris = VillageState.Adjust(state.trustDoris, tariff.trustRippleNeighbors);

            if (targetVillagerId == "gerrold")
                state.memoryIntegrityGerrold = VillageState.Adjust(state.memoryIntegrityGerrold, tariff.memoryIntegrityDelta);
            else if (targetVillagerId == "doris")
                state.memoryIntegrityDoris = VillageState.Adjust(state.memoryIntegrityDoris, tariff.memoryIntegrityDelta);

            state.vow1Integrity = VillageState.Adjust(state.vow1Integrity, tariff.vow1Delta);
            state.vow3Integrity = VillageState.Adjust(state.vow3Integrity, tariff.vow3Delta);
            state.vow7Integrity = VillageState.Adjust(state.vow7Integrity, tariff.vow7Delta);
            state.villageGriefAverage = VillageState.Adjust(state.villageGriefAverage, tariff.villageGriefDelta);

            Hh.Log(LogCategory.Mission, $"RippleEngine applied '{tariff.displayLabel}' " +
                $"(coin {tariff.coinDelta:+#;-#;0}, trust target {tariff.trustDeltaTarget:+#;-#;0}, " +
                $"ripple {tariff.trustRippleNeighbors:+#;-#;0}, memInteg {tariff.memoryIntegrityDelta:+#;-#;0}, " +
                $"vows {tariff.vow1Delta:+#;-#;0}/{tariff.vow3Delta:+#;-#;0}/{tariff.vow7Delta:+#;-#;0}).");
        }
    }
}
