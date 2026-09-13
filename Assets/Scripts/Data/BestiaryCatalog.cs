using System;
using System.Collections.Generic;

namespace Zazerkalye.Data
{
    public enum Rarity { Common, Uncommon, Rare, Secret }

    [Serializable]
    public class BestiaryEntry
    {
        public string Id;
        public string Name;
        public Rarity Rarity;
        public string Blurb;
    }

    [Serializable]
    public class SaveData
    {
        public int Kukichi;
        public int Pakichi;
        public int BestScore;
        public int MatchesPlayed;
        public List<string> Bestiary = new() { "sator", "fantasmagor" };
    }

    /// <summary>Canon from https://zazerwiki.com/</summary>
    public static class BestiaryCatalog
    {
        public static readonly BestiaryEntry[] All =
        {
            new() { Id = "sator", Name = "Сатор Арепыч", Rarity = Rarity.Common, Blurb = "К бобылям беспощаден: кокает без раздумий." },
            new() { Id = "bobyl", Name = "Бобыль", Rarity = Rarity.Common, Blurb = "Яйцеподобное существо рощи. Узнаётся по клокочущему запаху." },
            new() { Id = "bobyl_hard", Name = "Чёрный Эклер", Rarity = Rarity.Uncommon, Blurb = "Скорлупа потолще — кокайте дважды." },
            new() { Id = "pacanoid", Name = "Пацаноид", Rarity = Rarity.Uncommon, Blurb = "Их двое. Один пропадал. Конфликт с Казимиром — из‑за ложек." },
            new() { Id = "fantasmagor", Name = "Дядюшка Фантасмагор", Rarity = Rarity.Rare, Blurb = "Ему везут алмазные соления со всех уголков Зазеркалья." },
            new() { Id = "jvachnik", Name = "Жвачник", Rarity = Rarity.Rare, Blurb = "Хищник болот. Ходит стаями — одного не бывает." },
            new() { Id = "mihail", Name = "Михаил", Rarity = Rarity.Secret, Blurb = "Охотник за скорлупками. Не стой между ним и скорлупой." },
            new() { Id = "kazimir", Name = "Казимир", Rarity = Rarity.Uncommon, Blurb = "Полуволк в доспехах. Мастерски ворует ложки." },
            new() { Id = "istukanus", Name = "Истуканус", Rarity = Rarity.Rare, Blurb = "Загадка: «Брунявая Чуня или Чунявая Бруня?» Ответ — обои." },
            new() { Id = "akaky", Name = "Акакий Куролесов", Rarity = Rarity.Common, Blurb = "Безобидный обитатель рощи. Пугается шороха листвы." },
            new() { Id = "scripach", Name = "Болотный Скрипач", Rarity = Rarity.Secret, Blurb = "Ночью уводит путников от Спектрального колодца." },
            new() { Id = "spectral", Name = "Спектральный колодец", Rarity = Rarity.Secret, Blurb = "Появляется под одним и тем же деревом." },
            new() { Id = "polenych", Name = "Поленыч", Rarity = Rarity.Common, Blurb = "Преклонил колено — значит, ты добрый путник." }
        };
    }
}
