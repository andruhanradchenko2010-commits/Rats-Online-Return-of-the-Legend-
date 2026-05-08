using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Steal Settings")]
    [Tooltip("Максимальный шанс воровства в процентах")]
    public float maxStealChance = 95f;

    [Tooltip("Шанс воровства на 1 уровне крысы в процентах")]
    public float minStealChance = 0f;

    [Tooltip("Уровень крысы для достижения максимального шанса")]
    public int levelForMaxChance = 55;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Результат битвы
    public class BattleResult
    {
        public bool victory;
        public int cheeseStolen;
        public int soulsGained;
        public int elixirsGained;
        public List<Rat> beatenRats;
        public List<Rat> overfedEnemies;
    }

    // Расчет шанса воровства на основе уровня крысы
    private float CalculateStealChance(Rat rat)
    {
        // Линейная интерполяция от minStealChance до maxStealChance
        float chance = minStealChance + ((rat.level - 1) / (float)(levelForMaxChance - 1)) * (maxStealChance - minStealChance);
        return Mathf.Clamp(chance, minStealChance, maxStealChance);
    }

    // PvP атака на другого игрока
    public BattleResult AttackPlayer(List<Rat> enemyRats, int enemyCheese)
    {
        BattleResult result = new BattleResult
        {
            beatenRats = new List<Rat>(),
            overfedEnemies = new List<Rat>()
        };

        // Получаем 5 случайных крыс игрока
        List<Rat> playerRats = RatManager.Instance.GetRandomBattleRats(5);

        if (playerRats.Count == 0)
        {
            result.victory = false;
            return result;
        }

        // Берем первую крысу для атаки (можно расширить логику)
        Rat attackingRat = playerRats[0];

        // Рассчитываем шанс воровства на основе уровня крысы
        float stealChance = CalculateStealChance(attackingRat);

        // Проверяем успех воровства
        float roll = Random.Range(0f, 100f);
        result.victory = roll < stealChance;

        if (result.victory)
        {
            // Успешное воровство - крадем сыр
            result.cheeseStolen = Mathf.Min(enemyCheese, Random.Range(10, 50));
            CurrencyManager.Instance.AddCheese(result.cheeseStolen);

            // Крыса становится голодной после успешной атаки
            attackingRat.SetHungry();
        }
        else
        {
            // Провал - крыса становится подбитой
            attackingRat.Beat();
            result.beatenRats.Add(attackingRat);
            result.cheeseStolen = 0;
        }

        return result;
    }

    // Защита от атаки
    public BattleResult DefendAgainstAttack(List<Rat> attackerRats)
    {
        BattleResult result = new BattleResult
        {
            beatenRats = new List<Rat>(),
            overfedEnemies = new List<Rat>()
        };

        List<Rat> defenderRats = RatManager.Instance.GetRandomBattleRats(5);

        if (defenderRats.Count == 0)
        {
            result.victory = false;
            result.cheeseStolen = Random.Range(20, 100);
            return result;
        }

        int defenderPower = defenderRats.Sum(r => r.defenseSkill + r.level);
        int attackerPower = attackerRats.Sum(r => r.theftSkill + r.attackSkill + r.level);

        float defenseChance = (float)defenderPower / (defenderPower + attackerPower);
        result.victory = Random.value < defenseChance;

        if (result.victory)
        {
            // Успешная защита
            result.cheeseStolen = 0;
        }
        else
        {
            // Неудачная защита
            result.cheeseStolen = Random.Range(10, 50);

            foreach (var rat in defenderRats)
            {
                if (Random.value < 0.4f)
                {
                    rat.Beat();
                    result.beatenRats.Add(rat);
                }
            }
        }

        return result;
    }

    // Генерация AI противника
    public List<Rat> GenerateEnemyRats(int playerLevel)
    {
        List<Rat> enemies = new List<Rat>();
        int enemyCount = Random.Range(3, 6);

        for (int i = 0; i < enemyCount; i++)
        {
            // Уровень врага зависит от уровня игрока
            int enemyLevel = Mathf.Max(1, playerLevel + Random.Range(-5, 5));

            RatType type = RatType.Gray;
            if (enemyLevel >= 41) type = RatType.BatRat;
            else if (enemyLevel >= 26) type = RatType.Joker;
            else if (enemyLevel >= 21) type = RatType.Vampire;
            else if (enemyLevel >= 16) type = RatType.Devil;
            else if (enemyLevel >= 11) type = RatType.Angel;
            else if (enemyLevel >= 6) type = RatType.Royal;

            enemyLevel = Mathf.Clamp(enemyLevel, type.GetMinLevel(), type.GetMaxLevel());

            Rat enemy = new Rat(type, enemyLevel);

            // Случайные умения
            int totalSkills = enemyLevel * 2;
            enemy.defenseSkill = Random.Range(0, totalSkills / 2);
            enemy.attackSkill = totalSkills - enemy.defenseSkill;

            enemies.Add(enemy);
        }

        return enemies;
    }

    // Симуляция турнира
    public class TournamentResult
    {
        public int place;
        public int cheeseReward;
        public int soulsReward;
        public int elixirsReward;
    }

    public TournamentResult PlayTournament()
    {
        TournamentResult result = new TournamentResult();

        List<Rat> playerRats = RatManager.Instance.GetRandomBattleRats(5);
        int playerPower = playerRats.Sum(r => r.GetTotalPower());

        // Генерируем 10 противников
        int playerLevel = CurrencyManager.Instance.GetPlayerLevel();
        int totalOpponents = 10;
        int wins = 0;

        for (int i = 0; i < totalOpponents; i++)
        {
            List<Rat> enemies = GenerateEnemyRats(playerLevel);
            int enemyPower = enemies.Sum(r => r.GetTotalPower());

            float winChance = (float)playerPower / (playerPower + enemyPower);
            if (Random.value < winChance)
            {
                wins++;
            }
        }

        // Определяем место
        result.place = totalOpponents - wins + 1;

        // Награды в зависимости от места
        if (result.place == 1)
        {
            result.cheeseReward = 500;
            result.soulsReward = 50;
            result.elixirsReward = 10;
        }
        else if (result.place <= 3)
        {
            result.cheeseReward = 300;
            result.soulsReward = 30;
            result.elixirsReward = 5;
        }
        else if (result.place <= 5)
        {
            result.cheeseReward = 150;
            result.soulsReward = 15;
            result.elixirsReward = 2;
        }
        else
        {
            result.cheeseReward = 50;
            result.soulsReward = 5;
            result.elixirsReward = 1;
        }

        // Выдаем награды
        CurrencyManager.Instance.AddCheese(result.cheeseReward);
        CurrencyManager.Instance.AddSouls(result.soulsReward);
        CurrencyManager.Instance.AddLoveElixirs(result.elixirsReward);

        // Крысы становятся голодными
        foreach (var rat in playerRats)
        {
            rat.SetHungry();
        }

        return result;
    }
}
