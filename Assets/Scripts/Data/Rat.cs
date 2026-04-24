using System;
using UnityEngine;

[System.Serializable]
public class Rat
{
    public string id;
    public RatType type;
    public int level;
    public RatState state;

    // Характеристики (4 умения)
    public int theftSkill;    // Воровство
    public int miningSkill;   // Добыча
    public int defenseSkill;  // Защита
    public int attackSkill;   // Атака

    // Прогресс прокачки
    public int feedsRemaining; // Сколько кормежек осталось до следующего уровня
    public bool isHungry;      // Голодна ли крыса

    // Лечение
    public float healStartTime; // Время начала лечения (для прибитых)
    public int defibrillatorCount; // Сколько ДФР нужно (для закормленных)
    public float overfedTime; // Время закормки (для отслеживания смерти)

    public Sprite sprite; // Спрайт крысы

    public Rat(RatType ratType, int startLevel)
    {
        id = Guid.NewGuid().ToString();
        type = ratType;
        level = Mathf.Clamp(startLevel, ratType.GetMinLevel(), ratType.GetMaxLevel());
        state = RatState.Healthy;

        theftSkill = 0;
        miningSkill = 0;
        defenseSkill = 0;
        attackSkill = 0;

        feedsRemaining = level;
        isHungry = false;

        healStartTime = 0;
        defibrillatorCount = 0;
        overfedTime = 0;
    }

    public int GetTotalPower()
    {
        return theftSkill + miningSkill + defenseSkill + attackSkill + level;
    }

    public bool CanLevelUp()
    {
        return level < type.GetMaxLevel() && feedsRemaining <= 0;
    }

    public bool CanEvolve()
    {
        return level >= type.GetMaxLevel() && type != RatType.BatRat;
    }

    public void Feed(int cheeseAmount)
    {
        if (!isHungry) return;

        if (cheeseAmount >= level)
        {
            feedsRemaining--;
            isHungry = false;

            if (feedsRemaining <= 0 && level < type.GetMaxLevel())
            {
                level++;
                feedsRemaining = level;
            }
        }
    }

    public void SetHungry()
    {
        isHungry = true;
    }

    public void Beat()
    {
        state = RatState.Beaten;
        healStartTime = Time.time;
    }

    public void Overfeed()
    {
        state = RatState.Overfed;
        overfedTime = Time.time;
        defibrillatorCount = level;
    }

    public void Kill()
    {
        state = RatState.Dead;
    }

    public void Heal()
    {
        state = RatState.Healthy;
        healStartTime = 0;
        defibrillatorCount = 0;
        overfedTime = 0;
    }

    public bool CanFight()
    {
        return state == RatState.Healthy;
    }

    public bool CanOverfeed(Rat enemy)
    {
        // Крыса может закормить только крысу своего уровня или ниже
        return this.level >= enemy.level;
    }
}
