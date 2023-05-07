using System.Text;

namespace SQLite;

public class Row
{
    const int idSize = 4;
    const int usernameSize = 32;
    const int emailSize = 255;

    const int idOffset = 0;
    const int usernameOffset = idOffset + idSize;
    const int emailOffset = usernameOffset + usernameSize;

    public const int Size = idSize + usernameSize + emailSize;

    private readonly byte[] idBytes = new byte[idSize]; // int
    private readonly byte[] usernameBytes = new byte[usernameSize]; // varchar(32)
    private readonly byte[] emailBytes = new byte[emailSize]; // varchar(255)

    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }

    public Row(int id, string username, string email)
        : this(id, username, email,
            BitConverter.GetBytes(id).AsSpan(),
            Encoding.UTF8.GetBytes(username).AsSpan(),
            Encoding.UTF8.GetBytes(email).AsSpan())
    {
    }

    private Row(int id, string username, string email, Span<byte> idSpan, Span<byte> usernameSpan, Span<byte> emailSpan)
    {
        Id = id;
        Username = username;
        Email = email;

        idSpan.CopyTo(idBytes);
        usernameSpan.CopyTo(usernameBytes);
        emailSpan.CopyTo(emailBytes);
    }

    public void Serialize(Span<byte> destination)
    {
        var id = destination[idOffset..(idOffset + idSize)];
        var username = destination[usernameOffset..(usernameOffset + usernameSize)];
        var email = destination[emailOffset..(emailOffset + emailSize)];

        idBytes.CopyTo(id);
        usernameBytes.CopyTo(username);
        emailBytes.CopyTo(email);
    }

    public static Row Deserialize(Span<byte> bytes)
    {
        var idSpan = bytes[idOffset..(idOffset + idSize)];
        var usernameSpan = bytes[usernameOffset..(usernameOffset + usernameSize)];
        var emailSpan = bytes[emailOffset..(emailOffset + emailSize)];

        var id = BitConverter.ToInt32(idSpan);
        var username = Encoding.UTF8.GetString(usernameSpan);
        var email = Encoding.UTF8.GetString(emailSpan);

        var row = new Row(id, username, email, idSpan, usernameSpan, emailSpan);

        return row;
    }

    public override string ToString() => $"({Id}, {Username}, {Email})";
}
