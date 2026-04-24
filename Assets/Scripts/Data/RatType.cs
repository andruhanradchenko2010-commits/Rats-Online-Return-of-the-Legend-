using UnityEngine;

public enum RatType
{
    Gray = 0,      // Серая крыса (1-5)
    Royal = 1,     // Царская крыса (6-10)
    Angel = 2,     // Ангельская крыса (11-15)
    Devil = 3,     // Дьявольская крыса (16-20)
    Vampire = 4,   // Вампир (21-25)
    Joker = 5,     // Джокер (26-40)
    BatRat = 6     // Bat Rat (41-55+)
}

public enum RatSkill
{
    Theft = 0,     // Воровство
    Mining = 1,    // Добыча
    Defense = 2,   // Защита
    Attack = 3     // Атака
}

public enum RatState
{
    Healthy = 0,   // Здоровая
    Beaten = 1,    // Прибитая
    Overfed = 2,   // Закормленная
    Dead = 3       // Мертвая
}

public static class RatTypeExtensions
{
    public static int GetMinLevel(this RatType type)
    {
        switch (type)
        {
            case RatType.Gray: return 1;
            case RatType.Royal: return 6;
            case RatType.Angel: return 11;
            case RatType.Devil: return 16;
            case RatType.Vampire: return 21;
            case RatType.Joker: return 26;
            case RatType.BatRat: return 41;
            default: return 1;
        }
    }

    public static int GetMaxLevel(this RatType type)
    {
        switch (type)
        {
            case RatType.Gray: return 5;
            case RatType.Royal: return 10;
            case RatType.Angel: return 15;
            case RatType.Devil: return 20;
            case RatType.Vampire: return 25;
            case RatType.Joker: return 40;
            case RatType.BatRat: return 55;
            default: return 5;
        }
    }

    public static int GetExpPerFeed(this RatType type)
    {
        switch (type)
        {
            case RatType.Gray: return 1;
            case RatType.Royal: return 3;
            case RatType.Angel: return 10;
            case RatType.Devil: return 20;
            case RatType.Vampire: return 30;
            case RatType.Joker: return 50;
            case RatType.BatRat: return 70;
            default: return 1;
        }
    }

    public static int GetUpgradeCost(this RatType type)
    {
        switch (type)
        {
            case RatType.Gray: return 228;
            case RatType.Royal: return 850;
            case RatType.Angel: return 1750;
            case RatType.Devil: return 6918;
            case RatType.Vampire: return 34785;
            case RatType.Joker: return 832720;
            case RatType.BatRat: return 999999;
            default: return 228;
        }
    }

    public static string GetDisplayName(this RatType type)
    {
        switch (type)
        {
            case RatType.Gray: return "Серая крыса";
            case RatType.Royal: return "Царская крыса";
            case RatType.Angel: return "Ангельская крыса";
            case RatType.Devil: return "Дьявольская крыса";
            case RatType.Vampire: return "Вампир";
            case RatType.Joker: return "Джокер";
            case RatType.BatRat: return "Bat Rat";
            default: return "Неизвестная крыса";
        }
    }
}
