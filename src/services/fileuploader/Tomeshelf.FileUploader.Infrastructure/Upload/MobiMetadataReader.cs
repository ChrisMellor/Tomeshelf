using System;
using System.IO;
using System.Text;

namespace Tomeshelf.HumbleBundle.Infrastructure.Bundles.Upload;

internal static class MobiMetadataReader
{
    public static DocumentMetadata GetMetadata(string path)
    {
        var data = File.ReadAllBytes(path);

        var pdbTitle = DecodeNullTerminated(data.AsSpan(0, 32));

        var exthPos = IndexOf(data, "EXTH"u8.ToArray());
        var meta = new DocumentMetadata();

        if ((exthPos >= 0) && ((exthPos + 12) <= data.Length))
        {
            var recordCount = ReadBE32(data, exthPos + 8);

            var p = exthPos + 12;
            for (var i = 0; (i < recordCount) && ((p + 8) <= data.Length); i++)
            {
                var type = ReadBE32(data, p);
                var len = ReadBE32(data, p + 4);
                var valueStart = p + 8;
                var valueLen = (int)len - 8;
                if (((valueStart + valueLen) > data.Length) || (valueLen < 0))
                {
                    break;
                }

                var valBytes = new ReadOnlySpan<byte>(data, valueStart, valueLen);
                var str = DecodeText(valBytes);

                meta.Title = type switch
                {
                    503 => NullIfWhiteSpace(str),
                    _ => meta.Title
                };

                p += (int)len;
            }
        }

        if (string.IsNullOrWhiteSpace(meta.Title))
        {
            meta.Title = pdbTitle;
        }

        return meta;
    }

    private static string? DecodeNullTerminated(ReadOnlySpan<byte> bytes)
    {
        var zero = bytes.IndexOf((byte)0);
        if (zero >= 0)
        {
            bytes = bytes[..zero];
        }

        var s = Encoding.Latin1
                        .GetString(bytes)
                        .Trim();

        return string.IsNullOrWhiteSpace(s)
            ? null
            : s;
    }

    private static string DecodeText(ReadOnlySpan<byte> bytes)
    {
        try
        {
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return Encoding.Latin1.GetString(bytes);
        }
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i <= (haystack.Length - needle.Length); i++)
        {
            var j = 0;
            for (; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    break;
                }
            }

            if (j == needle.Length)
            {
                return i;
            }
        }

        return -1;
    }

    private static string? NullIfWhiteSpace(string? s)
    {
        return string.IsNullOrWhiteSpace(s)
            ? null
            : s.Trim();
    }

    private static uint ReadBE32(byte[] data, int offset)
    {
        return (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
    }
}