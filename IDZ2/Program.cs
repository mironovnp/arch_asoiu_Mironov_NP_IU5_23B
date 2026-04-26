using System.Text;

// ══════════════════════════════════════════════════════════
//  Точка входа — консольное меню
// ══════════════════════════════════════════════════════════

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Пути к файлам
string dbPath = "football.db";
string clubCsv = Path.Combine(AppContext.BaseDirectory, "club.csv");
string playerCsv = Path.Combine(AppContext.BaseDirectory, "player.csv");

// Создаём менеджер БД и инициализируем данные
var db = new DatabaseManager(dbPath);
db.InitializeDatabase(clubCsv, playerCsv);

Console.WriteLine();

// Главный цикл меню
string choice;
do
{
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║      УПРАВЛЕНИЕ ФУТБОЛИСТАМИ        ║");
    Console.WriteLine("╠══════════════════════════════════════╣");
    Console.WriteLine("║  1 — Показать все клубы              ║");
    Console.WriteLine("║  2 — Показать всех футболистов       ║");
    Console.WriteLine("║  3 — Добавить футболиста             ║");
    Console.WriteLine("║  4 — Редактировать футболиста        ║");
    Console.WriteLine("║  5 — Удалить футболиста              ║");
    Console.WriteLine("║  6 — Отчёты                          ║");
    Console.WriteLine("║  0 — Выход                           ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");

    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowClubs(db); break;
        case "2": ShowPlayers(db); break;
        case "3": AddPlayer(db); break;
        case "4": EditPlayer(db); break;
        case "5": DeletePlayer(db); break;
        case "6": ReportsMenu(db); break;
        case "0": Console.WriteLine("ГОЛ"); break;
        default: Console.WriteLine("Неверный пункт меню."); break;
    }

    Console.WriteLine();
}
while (choice != "0");

// ══════════════════════════════════════════════════════════
//  Функции пунктов меню
// ══════════════════════════════════════════════════════════

static void ShowClubs(DatabaseManager db)
{
    Console.WriteLine("--- Все клубы ---");
    var clubs = db.GetAllClubs();
    foreach (var club in clubs)
        Console.WriteLine("  " + club);
    Console.WriteLine($"Итого: {clubs.Count}");
}

static void ShowPlayers(DatabaseManager db)
{
    Console.WriteLine("--- Все футболисты ---");
    var players = db.GetAllPlayers();
    foreach (var player in players)
        Console.WriteLine("  " + player);
    Console.WriteLine($"Итого: {players.Count}");
}

static void AddPlayer(DatabaseManager db)
{
    Console.WriteLine("--- Добавление футболиста ---");

    // Показываем клубы, чтобы пользователь выбрал
    Console.WriteLine("Доступные клубы:");
    var clubs = db.GetAllClubs();
    foreach (var club in clubs)
        Console.WriteLine("  " + club);

    // Запрос данных у пользователя
    Console.Write("ID клуба: ");
    if (!int.TryParse(Console.ReadLine(), out int clubId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    Console.Write("Имя футболиста: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0)
    {
        Console.WriteLine("Ошибка: имя не может быть пустым.");
        return;
    }

    Console.Write("Количество голов: ");
    if (!int.TryParse(Console.ReadLine(), out int goals))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    try
    {
        var player = new Player(0, clubId, name, goals);
        db.AddPlayer(player);
        Console.WriteLine("Футболист добавлен.");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditPlayer(DatabaseManager db)
{
    Console.WriteLine("--- Редактирование футболиста ---");
    Console.Write("Введите ID футболиста: ");

    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var player = db.GetPlayerById(id);
    if (player == null)
    {
        Console.WriteLine($"Футболист с ID={id} не найден.");
        return;
    }

    Console.WriteLine($"Текущие данные: {player}");
    Console.WriteLine("(нажмите Enter, чтобы оставить значение без изменений)");

    // Имя
    Console.Write($"Имя [{player.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0)
        player.Name = input;

    // Клуб
    Console.Write($"ID клуба [{player.ClubId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newClubId))
        player.ClubId = newClubId;

    // Голы
    Console.Write($"Голы [{player.Goals}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newGoals))
    {
        try
        {
            player.Goals = newGoals; // валидация в set-аксессоре
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return;
        }
    }

    db.UpdatePlayer(player);
    Console.WriteLine("Данные обновлены.");
}

static void DeletePlayer(DatabaseManager db)
{
    Console.WriteLine("--- Удаление футболиста ---");
    Console.Write("Введите ID футболиста: ");

    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var player = db.GetPlayerById(id);
    if (player == null)
    {
        Console.WriteLine($"Футболист с ID={id} не найден.");
        return;
    }

    Console.Write($"Удалить «{player.Name}»? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
    if (confirm == "да")
    {
        db.DeletePlayer(id);
        Console.WriteLine("Футболист удалён.");
    }
    else
    {
        Console.WriteLine("Удаление отменено.");
    }
}

// ══════════════════════════════════════════════════════════
//  Подменю отчётов
// ══════════════════════════════════════════════════════════

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine("  1 — Футболисты по клубам");
        Console.WriteLine("  2 — Количество футболистов в клубах");
        Console.WriteLine("  3 — Среднее голов по клубам");
        Console.WriteLine("  0 — Назад");
        Console.Write("Ваш выбор: ");

        choice = Console.ReadLine()?.Trim() ?? "";

        switch (choice)
        {
            case "1": Report1_PlayersWithClubs(db); break;
            case "2": Report2_CountByClub(db); break;
            case "3": Report3_AvgGoalsByClub(db); break;
            case "0": break;
            default: Console.WriteLine("Неверный пункт."); break;
        }

        Console.WriteLine();
    }
    while (choice != "0");
}

// ─────── Отчёт 1: Футболисты с названиями клубов (JOIN) ───────

static void Report1_PlayersWithClubs(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT p.player_name, c.club_name, p.player_goals
                 FROM player p
                 JOIN club c ON p.club_id = c.club_id
                 ORDER BY p.player_name")
        .Title("Футболисты по клубам")
        .Header("Имя", "Клуб", "Голы")
        .ColumnWidths(20, 20, 10)
        .Footer("Всего записей") // [ГРУППА В] итоговая строка
        .Print();
}

// ─────── Отчёт 2: Количество футболистов по клубам (GROUP BY + COUNT) ───

static void Report2_CountByClub(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT c.club_name, COUNT(*) AS cnt
                 FROM player p
                 JOIN club c ON p.club_id = c.club_id
                 GROUP BY c.club_name
                 ORDER BY c.club_name")
        .Title("Количество футболистов по клубам")
        .Header("Клуб", "Кол-во")
        .ColumnWidths(20, 10)
        .Footer("Всего клубов") // [ГРУППА В] итоговая строка
        .Print();
}

// ─────── Отчёт 3: Среднее голов по клубам (GROUP BY + AVG) ───────

static void Report3_AvgGoalsByClub(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT c.club_name,
                         ROUND(AVG(p.player_goals), 1) AS avg_goals
                 FROM player p
                 JOIN club c ON p.club_id = c.club_id
                 GROUP BY c.club_name
                 ORDER BY avg_goals DESC")
        .Title("Среднее количество голов по клубам")
        .Header("Клуб", "Среднее голов")
        .ColumnWidths(20, 20)
        .Footer("Всего клубов") // [ГРУППА В] итоговая строка
        .Print();
}