/// <summary>
/// Футболист (основная таблица, сторона «много»)
/// </summary>
class Player
{
    /// <summary>Идентификатор футболиста</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор клуба (внешний ключ)</summary>
    public int ClubId { get; set; }

    /// <summary>Имя футболиста</summary>
    public string Name { get; set; }

    private int _goals;
    /// <summary>
    /// Количество забитых голов (не может быть отрицательным)
    /// </summary>
    public int Goals
    {
        get => _goals;
        set
        {
            if (value < 0)
                throw new ArgumentException(
                    "Количество голов не может быть отрицательным");
            _goals = value;
        }
    }

    /// <summary>Конструктор с параметрами</summary>
    public Player(int id, int clubId, string name, int goals)
    {
        Id = id;
        ClubId = clubId;
        Name = name;
        Goals = goals;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Player() : this(0, 0, "", 0) { }

    public override string ToString()
        => $"[{Id}] {Name}, клуб #{ClubId}, голов: {Goals}";
}