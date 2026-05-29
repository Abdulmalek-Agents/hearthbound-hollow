// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / Localization / LocaleChangedEvent
//
// Published by LocalizationService whenever the player switches language.
// Subscribers (LocalizedText components, HUD, Dialogue, etc.) re-pull their
// strings + re-apply RTL layout on receipt.

namespace HearthboundHollow.Core
{
    /// <summary>
    /// Fired on the EventBus whenever the active <see cref="Locale"/>
    /// changes. Carries both the previous and new locale so subscribers can
    /// short-circuit no-op updates (e.g. font-asset reload skipped when the
    /// rendering direction didn't change).
    /// </summary>
    public readonly struct LocaleChangedEvent
    {
        public readonly Locale PreviousLocale;
        public readonly Locale CurrentLocale;

        public LocaleChangedEvent(Locale previous, Locale current)
        {
            PreviousLocale = previous;
            CurrentLocale  = current;
        }

        /// <summary>True if the RTL direction flipped — UI must mirror.</summary>
        public bool RtlDirectionChanged =>
            LocaleInfo.IsRightToLeft(PreviousLocale) !=
            LocaleInfo.IsRightToLeft(CurrentLocale);
    }
}
