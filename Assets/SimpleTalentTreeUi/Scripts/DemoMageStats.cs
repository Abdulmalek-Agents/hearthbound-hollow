using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Example mage stats script to show how to use events from PlayerTalentLink
    /// with a Fire / Dark magic talent tree.
    /// Each public method can be bound to "On Points Changed" of a specific talent.
    /// </summary>
    public class DemoMageStats : MonoBehaviour
    {
        [Header("Base stats")]
        public int baseHealth = 100;
        public float baseFireDamage = 20f;
        public float baseDarkDamage = 20f;
        /// <summary>
        /// Base cast speed in casts per second.
        /// </summary>
        public float baseCastSpeed = 1f;
        public float baseMoveSpeed = 5f;

        [Header("Bonus from talents")]
        public int bonusHealthFromTalents;
        public float bonusFireDamageFromTalents;
        public float bonusDarkDamageFromTalents;
        public float bonusCastSpeedFromTalents;
        public float bonusMoveSpeedFromTalents;

        [Header("Final values (for debugging)")]
        public int totalHealth;
        public float totalFireDamage;
        public float totalDarkDamage;
        public float totalCastSpeed;
        public float totalMoveSpeed;

        private void Awake()
        {
            RecalculateTotals();
        }

        /// <summary>
        /// Recalculates final stats based on base values and bonuses.
        /// </summary>
        public void RecalculateTotals()
        {
            totalHealth = baseHealth + bonusHealthFromTalents;
            totalFireDamage = baseFireDamage + bonusFireDamageFromTalents;
            totalDarkDamage = baseDarkDamage + bonusDarkDamageFromTalents;
            totalCastSpeed = baseCastSpeed + bonusCastSpeedFromTalents;
            totalMoveSpeed = baseMoveSpeed + bonusMoveSpeedFromTalents;
        }

        /// <summary>
        /// Fire Mastery talent: increases fire spell damage.
        /// Bind this to OnPointsChanged of a Fire talent (e.g. "Fire Mastery" or "Fireball Focus").
        /// </summary>
        public void OnFireMasteryTalentChanged(int previousPoints, int currentPoints)
        {
            int delta = currentPoints - previousPoints;

            // Example: each point gives +6 fire damage
            float fireDamagePerPoint = 6f;
            bonusFireDamageFromTalents += delta * fireDamagePerPoint;

            RecalculateTotals();

            Debug.Log(
                $"[SimpleTalentTreeUI] Fire Mastery changed from {previousPoints} to {currentPoints}. " +
                $"Fire damage bonus: {bonusFireDamageFromTalents}. Total fire damage: {totalFireDamage}.",
                this);
        }

        /// <summary>
        /// Dark Pact talent: trades health for dark damage.
        /// Bind this to OnPointsChanged of a Dark talent (e.g. "Dark Pact" or "Corrupted Power").
        /// </summary>
        public void OnDarkPactTalentChanged(int previousPoints, int currentPoints)
        {
            int delta = currentPoints - previousPoints;

            // Example: each point reduces health by 10 but adds +8 dark damage
            int healthPenaltyPerPoint = -10;
            float darkDamagePerPoint = 8f;

            bonusHealthFromTalents += delta * healthPenaltyPerPoint;
            bonusDarkDamageFromTalents += delta * darkDamagePerPoint;

            RecalculateTotals();

            Debug.Log(
                $"[SimpleTalentTreeUI] Dark Pact changed from {previousPoints} to {currentPoints}. " +
                $"Health bonus: {bonusHealthFromTalents} (can be negative). Total health: {totalHealth}. " +
                $"Dark damage bonus: {bonusDarkDamageFromTalents}. Total dark damage: {totalDarkDamage}.",
                this);
        }

        /// <summary>
        /// Flame Dash talent: increases mobility / movement speed.
        /// Bind this to OnPointsChanged of a Fire mobility talent (e.g. "Flame Dash").
        /// </summary>
        public void OnFlameDashTalentChanged(int previousPoints, int currentPoints)
        {
            int delta = currentPoints - previousPoints;

            // Example: each point gives +0.4 move speed and +0.1 cast speed
            float moveSpeedPerPoint = 0.4f;
            float castSpeedPerPoint = 0.1f;

            bonusMoveSpeedFromTalents += delta * moveSpeedPerPoint;
            bonusCastSpeedFromTalents += delta * castSpeedPerPoint;

            RecalculateTotals();

            Debug.Log(
                $"[SimpleTalentTreeUI] Flame Dash changed from {previousPoints} to {currentPoints}. " +
                $"Move speed bonus: {bonusMoveSpeedFromTalents}. Total move speed: {totalMoveSpeed}. " +
                $"Cast speed bonus: {bonusCastSpeedFromTalents}. Total cast speed: {totalCastSpeed}.",
                this);
        }
    }
}
