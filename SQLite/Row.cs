﻿using SQLite.Exceptions;
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

    private static Encoding Encoding => Encoding.UTF8;

    public Row(int id, string username, string email)
        : this(id, username, email,
            BitConverter.GetBytes(id).AsMemory(),
            Encoding.GetBytes(username).AsMemory(),
            Encoding.GetBytes(email).AsMemory())
    {
    }

    private Row(int id, string username, string email, Memory<byte> idSpan, Memory<byte> usernameSpan, Memory<byte> emailSpan)
    {
        if (id < 0)
        {
            throw new NegativeIdException();
        }

        if (usernameSpan.Length > usernameBytes.Length
            || emailSpan.Length > emailBytes.Length)
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
        var id = destination[idOffset..(idOffset + idSize)];
        var username = destination[usernameOffset..(usernameOffset + usernameSize)];
        var email = destination[emailOffset..(emailOffset + emailSize)];

        idBytes.CopyTo(id);
        usernameBytes.CopyTo(username);
        emailBytes.CopyTo(email);
    }

    public static Row Deserialize(Memory<byte> bytes)
    {
        var idMemory = bytes[idOffset..(idOffset + idSize)];
        var usernameMemory = bytes[usernameOffset..(usernameOffset + usernameSize)];
        var emailMemory = bytes[emailOffset..(emailOffset + emailSize)];

        var id = BitConverter.ToInt32(idMemory.Span);
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
