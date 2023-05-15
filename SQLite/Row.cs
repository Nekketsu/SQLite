using SQLite.Exceptions;
using System.Text;

namespace SQLite;

public class Row
{
    const uint columnUserNameSize = 32;
    const uint columnEmailSize = 255;

    const uint idSize = 4;
    const uint usernameSize = columnUserNameSize + 1;
    const uint emailSize = columnEmailSize + 1;

    const uint idOffset = 0;
    const uint usernameOffset = idOffset + idSize;
    const uint emailOffset = usernameOffset + usernameSize;

    public const uint Size = idSize + usernameSize + emailSize;

    private readonly byte[] idBytes = new byte[idSize]; // int
    private readonly byte[] usernameBytes = new byte[usernameSize]; // varchar(32)
    private readonly byte[] emailBytes = new byte[emailSize]; // varchar(255)

    public uint Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }

    private static Encoding Encoding => Encoding.UTF8;

    public Row(uint id, string username, string email)
        : this(id, username, email,
            BitConverter.GetBytes(id).AsMemory(),
            Encoding.GetBytes(username).AsMemory(),
            Encoding.GetBytes(email).AsMemory())
    {
    }

    private Row(uint id, string username, string email, Memory<byte> idSpan, Memory<byte> usernameSpan, Memory<byte> emailSpan)
    {
        if (usernameSpan.Length > columnUserNameSize
            || emailSpan.Length > columnEmailSize)
        {
            throw new StringTooLongException();
        }

        Id = id;
        Username = username;
        Email = email;

        idSpan.CopyTo(idBytes);
        usernameSpan.CopyTo(usernameBytes);
        emailSpan.CopyTo(emailBytes);
    }

    public void Serialize(Memory<byte> destination)
    {
        var id = destination[(int)idOffset..(int)(idOffset + idSize)];
        var username = destination[(int)usernameOffset..(int)(usernameOffset + usernameSize)];
        var email = destination[(int)emailOffset..(int)(emailOffset + emailSize)];

        idBytes.CopyTo(id);
        usernameBytes.CopyTo(username);
        emailBytes.CopyTo(email);
    }

    public static Row Deserialize(Memory<byte> bytes)
    {
        var idMemory = bytes[(int)idOffset..(int)(idOffset + idSize)];
        var usernameMemory = bytes[(int)usernameOffset..(int)(usernameOffset + usernameSize - 1)];
        var emailMemory = bytes[(int)emailOffset..(int)(emailOffset + emailSize - 1)];

        var id = BitConverter.ToUInt32(idMemory.Span);
        var username = Encoding.GetString(RemoveEndOfString(usernameMemory.Span));
        var email = Encoding.GetString(RemoveEndOfString(emailMemory.Span));

        var row = new Row(id, username, email, idMemory, usernameMemory, emailMemory);

        return row;
    }

    public override string ToString() => $"({Id}, {Username}, {Email})";

    private static Span<byte> RemoveEndOfString(Span<byte> text)
    {
        var index = text.IndexOf((byte)0);

        var span = (index >= 0)
            ? text.Slice(0, index)
            : text;

        return span;
    }
}
