﻿using System;
using System.IO;

namespace UTF8StreamReplacer.lib
{
    using Writers;

    public delegate string StringReplacer(string input);
    public delegate byte[] ByteArrayReplacer(byte[] input);

    public class UTf8StreamReplacer : Stream
    {
        private readonly Stream _stream;
        private readonly IWriter _writer;

        public UTf8StreamReplacer(Stream stream, string match, string replace)
            : this(stream, match.GetBytes(), replace.GetBytes())
        { }

        public UTf8StreamReplacer(Stream stream, byte[] match, byte[] replace)
        {
            _stream = stream;
            _writer = new SimpleReplaceWriter(stream, match, replace);
        }

        public UTf8StreamReplacer(Stream stream, StringReplacer replacer, string delimiter)
            : this(stream, replacer, delimiter, delimiter)
        { }

        public UTf8StreamReplacer(Stream stream, ByteArrayReplacer replacer, byte[] delimiter)
            : this(stream, replacer, delimiter, delimiter)
        { }

        public UTf8StreamReplacer(Stream stream, StringReplacer replacer, string startDelimiter, string endDelimiter)
            :this(stream, StringToByteArrayReplacer(replacer), startDelimiter.GetBytes(), endDelimiter.GetBytes())
        { }

        public UTf8StreamReplacer(Stream stream, ByteArrayReplacer replacer, byte[] startDelimiter, byte[] endDelimiter)
        {
            _stream = stream;
            _writer = new DelimitedReplaceWriter(stream, startDelimiter, endDelimiter, replacer);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get; set; }
        public override long Seek(long offset, SeekOrigin direction) => _stream.Seek(offset, direction);
        public override void SetLength(long length) => _stream.SetLength(length);
        public override void Close() => _stream.Close();
        public override void Flush() => _writer.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => _writer.Write(buffer, offset, count);

        private static readonly Func<StringReplacer, ByteArrayReplacer> StringToByteArrayReplacer =
            (strReplacer) => 
                (input) =>
                {
                    var replacementStr = strReplacer(input.GetString());
                    return replacementStr.GetBytes();
                };
    }
}
