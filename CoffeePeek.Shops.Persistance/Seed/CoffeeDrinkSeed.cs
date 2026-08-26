using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;

namespace CoffeePeek.Shops.Persistance.Seed;

public static class CoffeeDrinkSeed
{
    public static readonly (Guid Id, string Slug, string NameRu, string NameEn, int Category, string Aliases, int Sort)[] Rows =
    [
        (CoffeeDrinkIds.Espresso, "espresso", "Эспрессо", "Espresso", 1, "эспрессо,espresso", 10),
        (CoffeeDrinkIds.Doppio, "doppio", "Доппио", "Doppio", 1, "доппио,doppio,double espresso,двойной эспрессо", 20),
        (CoffeeDrinkIds.Americano, "americano", "Американо", "Americano", 1, "американо,americano,lungo", 30),
        (CoffeeDrinkIds.Cappuccino, "cappuccino", "Капучино", "Cappuccino", 1, "капучино,капуч,cappuccino,cappucino", 40),
        (CoffeeDrinkIds.Latte, "latte", "Латте", "Latte", 1, "латте,latte,caffe latte", 50),
        (CoffeeDrinkIds.FlatWhite, "flat_white", "Флэт уайт", "Flat white", 1, "флэт уайт,флэт,flat white,flatwhite", 60),
        (CoffeeDrinkIds.Cortado, "cortado", "Кортадо", "Cortado", 1, "кортадо,cortado,piccolo,пикколо", 70),
        (CoffeeDrinkIds.Macchiato, "macchiato", "Макиато", "Macchiato", 1, "макиато,macchiato", 80),
        (CoffeeDrinkIds.Raf, "raf", "Раф", "Raf", 1, "раф,raf", 90),
        (CoffeeDrinkIds.BatchBrew, "batch_brew", "Фильтр", "Batch brew", 2, "фильтр,капелька,batch brew,drip,batch", 100),
        (CoffeeDrinkIds.V60, "v60", "V60 / воронка", "V60", 2, "v60,v-60,воронка,hario,pour over", 110),
        (CoffeeDrinkIds.Kalita, "kalita", "Калита", "Kalita", 2, "калита,kalita,wave", 120),
        (CoffeeDrinkIds.Chemex, "chemex", "Кемекс", "Chemex", 2, "кемекс,chemex", 130),
        (CoffeeDrinkIds.Aeropress, "aeropress", "Аэропресс", "AeroPress", 2, "аэропресс,aeropress,aero press", 140)
    ];
}
