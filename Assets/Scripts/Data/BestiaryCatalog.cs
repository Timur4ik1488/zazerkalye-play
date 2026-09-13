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

    /// <summary>Canon from https://zazerwiki.com/ and the grove session design.</summary>
    public static class BestiaryCatalog
    {
        public static readonly BestiaryEntry[] All =
        {
            new() { Id = "sator", Name = "Сатор Арепыч", Rarity = Rarity.Common, Blurb = "Бывший офицер. Имя дал Истуканус. К бобылям беспощаден: кокает без раздумий. Остальным путникам кокание — всё же выбор." },
            new() { Id = "bobyl", Name = "Бобыль", Rarity = Rarity.Common, Blurb = "Яйцеподобное существо рощи. Лесных узнают по клокочущему запаху. Кокалка — расчёска: ею успокаивают, а не казнят." },
            new() { Id = "bobyl_hard", Name = "Чёрный Эклер", Rarity = Rarity.Uncommon, Blurb = "Скорлупа потолще — кокайте дважды или подойдите сзади. Один из знаменитых бобылей рощи." },
            new() { Id = "pacanoid", Name = "Пацаноид", Rarity = Rarity.Uncommon, Blurb = "Их двое, зелёноголовые, пара. Один пропадал — равновесие рушится. Конфликт с Казимиром из‑за ложек." },
            new() { Id = "fantasmagor", Name = "Дядюшка Фантасмагор", Rarity = Rarity.Rare, Blurb = "Ключевая фигура Зазеркалья. Ему везут алмазные соления со всех уголков — опоздаешь, накличет беду." },
            new() { Id = "jvachnik", Name = "Жвачник", Rarity = Rarity.Rare, Blurb = "Хищник болот Сумеречной рощи. Ходит стаями — одного не бывает. Ночью выходит на охоту." },
            new() { Id = "mihail", Name = "Михаил", Rarity = Rarity.Secret, Blurb = "Охотник за скорлупками: с бобылей, из ямы или от пескарей. Не стой между ним и скорлупой." },
            new() { Id = "kazimir", Name = "Казимир", Rarity = Rarity.Uncommon, Blurb = "Полуволк в доспехах. Мастерски ворует ложки (уже 40+). Пацаноиды на него в ярости." },
            new() { Id = "istukanus", Name = "Истуканус", Rarity = Rarity.Rare, Blurb = "Неподвижный идол: даёт имена и задаёт вопрос. «Брунявая Чуня или Чунявая Бруня?» Правильный ответ — обои. Говорит с миром через Поленыча." },
            new() { Id = "akaky", Name = "Акакий Куролесов", Rarity = Rarity.Common, Blurb = "Безобидный житель рощи. Любит пыльцу и росу, пугается шороха. Ночь можно переждать у него в дупле." },
            new() { Id = "scripach", Name = "Болотный Скрипач", Rarity = Rarity.Secret, Blurb = "Деревянная маска, канифоль, обман. Ночью на болотах заводит путников в сторону от Спектрального колодца." },
            new() { Id = "spectral", Name = "Спектральный колодец", Rarity = Rarity.Secret, Blurb = "Легенда рощи: появляется под одним и тем же деревом. Место прячут все зазеркальцы." },
            new() { Id = "polenych", Name = "Поленыч", Rarity = Rarity.Common, Blurb = "Древоподобный проводник ПВЗ. Преклонил колено — мягкий жест дружбы. Не путать с Коленычем." },
            new() { Id = "kolenych", Name = "Коленыч", Rarity = Rarity.Rare, Blurb = "Один из опаснейших. Если преклонил полено — беги зигзагом (рывок). Состоит из кости, с виду почти как Поленыч." },
            new() { Id = "pedal", Name = "Мальчик-педаль", Rarity = Rarity.Uncommon, Blurb = "Помогает за «нажатие». Один раз за смену даёт ускорение — и скрывается в кустах." }
        };
    }
}
