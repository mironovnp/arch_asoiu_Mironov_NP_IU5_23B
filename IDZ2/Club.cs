/// <summary>
/// Футбольный клуб (справочная таблица, сторона «один»)
/// </summary>
class Club
{
    /// <summary>Идентификатор клуба</summary>
    public int Id { get; set; }

    /// <summary>Название клуба</summary>
    public string Name { get; set; }

    /// <summary>Конструктор с параметрами</summary>
    public Club(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Club() : this(0, "") { }

    public override string ToString() => $"[{Id}] {Name}";
}