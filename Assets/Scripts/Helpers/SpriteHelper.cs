using UnityEngine;

public static class SpriteHelper
{
    private static Sprite redSquareSprite;

    public static Sprite GetRedSquare()
    {
        if (redSquareSprite == null)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.red;
            tex.SetPixels(pixels);
            tex.Apply();
            redSquareSprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }
        return redSquareSprite;
    }

    public static Sprite LoadRatSprite(RatType type)
    {
        string spriteName = type switch
        {
            RatType.Gray => "Серая крыса",
            RatType.Royal => "Царская крыса",
            RatType.Angel => "Ангельская крыса",
            RatType.Devil => "Дьявольская крыса",
            RatType.Vampire => "Вампир",
            RatType.Joker => "Джокер",
            RatType.BatRat => "Bat Rat",
            _ => "Серая крыса"
        };

        // Пытаемся загрузить из Resources
        Sprite sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");

        // Если не найден, возвращаем красный квадрат
        return sprite ?? GetRedSquare();
    }

    public static Sprite LoadItemSprite(ItemType type)
    {
        string spriteName = type switch
        {
            ItemType.WonderBandage => "Чудо-бинт",
            ItemType.SimpleDefib => "ДФР",
            ItemType.SuperDefib => "СуперДФР",
            ItemType.RevivalPotion => "Зелье",
            ItemType.BrownGlove => "Коричневая перчатка",
            ItemType.Hat => "Шляпка",
            ItemType.Couch => "Кушетка",
            _ => ""
        };

        Sprite sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
        return sprite ?? GetRedSquare();
    }

    public static Sprite LoadCheeseSprite()
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Сыр");
        return sprite ?? GetRedSquare();
    }

    public static Sprite LoadBarrelSprite()
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Бочка сыра");
        return sprite ?? GetRedSquare();
    }
}
