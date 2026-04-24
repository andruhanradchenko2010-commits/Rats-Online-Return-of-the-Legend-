using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

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

        // Подсчет силы
        int playerPower = playerRats.Sum(r => r.theftSkill + r.attackSkill + r.level);
        int enemyPower = enemyRats.Sum(r => r.defenseSkill + r.level);

        // Определяем победителя
        float winChance = (float)playerPower / (playerPower + enemyPower);
        result.victory = Random.value < winChance;

        if (result.victory)
        {
            // Победа - крадем сыр
            result.cheeseStolen = Mathf.Min(enemyCheese, Random.Range(10, 50));
            CurrencyManager.Instance.AddCheese(result.cheeseStolen);

            // Шанс закормить вражеских крыс
            foreach (var enemyRat in enemyRats)
            {
                if (Random.value < 0.3f) // 30% шанс
                {
                    // Проверяем, может ли наша крыса закормить эту
                    var capableRat = playerRats.FirstOrDefault(r => r.CanOverfeed(enemyRat));
                    if (capableRat != null)
                    {
                        result.overfedEnemies.Add(enemyRat);
                        result.soulsGained += Random.Range(1, 3);
                        result.elixirsGained += Random.Range(0, 2);
                    }
                }
            }

            CurrencyManager.Instance.AddSouls(result.soulsGained);
            CurrencyManager.Instance.AddLoveElixirs(result.elixirsGained);
        }
        else
        {
            // Поражение - наши крысы прибиты
            foreach (var rat in playerRats)
            {
                if (Random.value < 0.5f) // 50% шанс быть прибитым
                {
                    rat.Beat();
                    result.beatenRats.Add(rat);
                }
            }
        }

        // Все крысы становятся голодными после битвы
        foreach (var rat in playerRats)
        {
            rat.SetHungry();
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
