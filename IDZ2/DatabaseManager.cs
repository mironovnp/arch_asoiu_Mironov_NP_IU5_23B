using Microsoft.Data.Sqlite;

/// <summary>
/// Управление базой данных SQLite.
/// Инкапсулирует все операции с БД: создание таблиц,
/// импорт CSV, CRUD-операции, выполнение запросов для отчётов.
/// </summary>
class DatabaseManager
{
    private string _connectionString;

    /// <summary>
    /// Конструктор. Принимает путь к файлу БД.
    /// </summary>
    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    // ──────────── Инициализация ────────────

    /// <summary>
    /// Создаёт таблицы (если не существуют) и загружает CSV при первом запуске
    /// </summary>
    public void InitializeDatabase(string clubCsvPath, string playerCsvPath)
    {
        CreateTables();

        // Импорт только если таблицы пусты
        if (GetAllClubs().Count == 0 && File.Exists(clubCsvPath))
        {
            ImportClubsFromCsv(clubCsvPath);
            Console.WriteLine($"[OK] Загружены клубы из {clubCsvPath}");
        }

        if (GetAllPlayers().Count == 0 && File.Exists(playerCsvPath))
        {
            ImportPlayersFromCsv(playerCsvPath);
            Console.WriteLine($"[OK] Загружены футболисты из {playerCsvPath}");
        }
    }

    /// <summary>Создание таблиц</summary>
    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS club (
            club_id   INTEGER PRIMARY KEY AUTOINCREMENT,
            club_name TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS player (
            player_id    INTEGER PRIMARY KEY AUTOINCREMENT,
            club_id      INTEGER NOT NULL,
            player_name  TEXT    NOT NULL,
            player_goals INTEGER NOT NULL,
            FOREIGN KEY (club_id) REFERENCES club(club_id)
        );";
        cmd.ExecuteNonQuery();
    }

    /// <summary>Импорт клубов из CSV</summary>
    private void ImportClubsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;

            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO club (club_id, club_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Импорт футболистов из CSV</summary>
    private void ImportPlayersFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO player (player_id, club_id, player_name, player_goals)
            VALUES (@id, @clubId, @name, @goals)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@clubId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@goals", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }
    }

    // ──────────── Чтение данных ────────────

    /// <summary>Получить все клубы</summary>
    public List<Club> GetAllClubs()
    {
        var result = new List<Club>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT club_id, club_name FROM club ORDER BY club_id";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Club(
                reader.GetInt32(0),
                reader.GetString(1)));
        }
        return result;
    }

    /// <summary>Получить всех футболистов</summary>
    public List<Player> GetAllPlayers()
    {
        var result = new List<Player>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT player_id, club_id, player_name, player_goals FROM player ORDER BY player_id";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Player(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }
        return result;
    }

    /// <summary>Получить футболиста по Id</summary>
    public Player? GetPlayerById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT player_id, club_id, player_name, player_goals FROM player WHERE player_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new Player(
                reader.GetInt32(0), reader.GetInt32(1),
                reader.GetString(2), reader.GetInt32(3));
        }
        return null;
    }

    // ──────────── Изменение данных ────────────

    /// <summary>Добавить футболиста (Id генерируется автоматически)</summary>
    public void AddPlayer(Player player)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO player (club_id, player_name, player_goals)
        VALUES (@clubId, @name, @goals)";
        cmd.Parameters.AddWithValue("@clubId", player.ClubId);
        cmd.Parameters.AddWithValue("@name", player.Name);
        cmd.Parameters.AddWithValue("@goals", player.Goals);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Обновить данные футболиста</summary>
    public void UpdatePlayer(Player player)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        UPDATE player
        SET club_id = @clubId, player_name = @name, player_goals = @goals
        WHERE player_id = @id";
        cmd.Parameters.AddWithValue("@id", player.Id);
        cmd.Parameters.AddWithValue("@clubId", player.ClubId);
        cmd.Parameters.AddWithValue("@name", player.Name);
        cmd.Parameters.AddWithValue("@goals", player.Goals);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Удалить футболиста по Id</summary>
    public void DeletePlayer(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM player WHERE player_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ──────────── Выполнение произвольного запроса (для отчётов) ────────────

    /// <summary>
    /// Выполняет SQL-запрос и возвращает имена столбцов и строки результата.
    /// Используется классом ReportBuilder.
    /// </summary>
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        // Имена столбцов
        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        // Строки данных
        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }

        return (columns, rows);
    }

    // ──────────── Экспорт в CSV ────────────

    /// <summary>Экспорт обеих таблиц в CSV-файлы</summary>
    public void ExportToCsv(string clubPath, string playerPath)
    {
        // Экспорт клубов
        var clubLines = new List<string>();
        clubLines.Add("club_id;club_name");
        foreach (var club in GetAllClubs())
            clubLines.Add($"{club.Id};{club.Name}");
        File.WriteAllLines(clubPath, clubLines.ToArray());

        // Экспорт футболистов
        var playerLines = new List<string>();
        playerLines.Add("player_id;club_id;player_name;player_goals");
        foreach (var player in GetAllPlayers())
            playerLines.Add($"{player.Id};{player.ClubId};{player.Name};{player.Goals}");
        File.WriteAllLines(playerPath, playerLines.ToArray());
    }

    // ──────────── Фильтр по клубу ────────────

    /// <summary>Получить футболистов конкретного клуба</summary>
    public List<Player> GetPlayersByClub(int clubId)
    {
        var result = new List<Player>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT player_id, club_id, player_name, player_goals
        FROM player WHERE club_id = @clubId ORDER BY player_name";
        cmd.Parameters.AddWithValue("@clubId", clubId);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Player(
                reader.GetInt32(0), reader.GetInt32(1),
                reader.GetString(2), reader.GetInt32(3)));
        }
        return result;
    }
}