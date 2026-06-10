using UnityEngine;

public static class GameConfig
{
    // Test Mode
    public static bool IsTestMode => GameManager.Instance?.TestMode ?? false;

    // Heal Settings
    public const float NORMAL_HEAL_TIME = 30f;
    public const float FAST_HEAL_TIME = 2f;

    public static float GetHealTime() => IsTestMode ? FAST_HEAL_TIME : NORMAL_HEAL_TIME;

    // Battle Settings
    public const float MIN_STEAL_CHANCE = 0f;
    public const float MAX_STEAL_CHANCE = 95f;
    public const int LEVEL_FOR_MAX_STEAL_CHANCE = 55;

    public const int TEST_MIN_STOLEN_CHEESE = 50;
    public const int TEST_MAX_STOLEN_CHEESE = 100;
    public const int NORMAL_MIN_STOLEN_CHEESE = 10;
    public const int NORMAL_MAX_STOLEN_CHEESE = 51;

    public static int GetMinStolenCheese() => IsTestMode ? TEST_MIN_STOLEN_CHEESE : NORMAL_MIN_STOLEN_CHEESE;
    public static int GetMaxStolenCheese() => IsTestMode ? TEST_MAX_STOLEN_CHEESE : NORMAL_MAX_STOLEN_CHEESE;

    // Daily Chest
    public const float DAILY_COOLDOWN_HOURS = 24f;
    public const int DAILY_CHEST_MIN_LEVEL = 6;

    // Arena
    public const float ARENA_ROUND_TIME = 180f; // 3 minutes
    public const float ARENA_FOOD_SPAWN_INTERVAL = 2f;
    public const int ARENA_MAX_FOOD_ITEMS = 20;

    // Dungeon
    public const float DUNGEON_WALL_SPAWN_CHANCE = 0.7f; // 70%
    public const int DUNGEON_BASE_BREAK_COST = 50;
    public const int DUNGEON_BREAK_COST_PER_DISTANCE = 10;

    // Overfed Death Time
    public const float OVERFED_DEATH_TIME_DAYS = 3f;
    public static float GetOverfedDeathTime() => OVERFED_DEATH_TIME_DAYS * 24f * 60f * 60f; // в секундах

    // Battle
    public const float BEATEN_RAT_CHANCE = 0.4f; // 40% шанс что крыса будет подбита при проигрыше

    // Barrel Reward
    public const float BARREL_AUTO_DISAPPEAR_TIME = 2f;
    public const float CHEESE_AUTO_COLLECT_TIME = 5f;

    // UI Update Intervals
    public const float UI_UPDATE_INTERVAL = 0.5f;
    public const float ARENA_UPDATE_INTERVAL = 0.1f;

    // Player Level Rewards
    public const int RAT_BUCKS_PER_LEVEL = 13;
}
