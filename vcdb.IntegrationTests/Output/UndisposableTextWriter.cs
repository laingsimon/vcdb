using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Output
{
    internal class UndisposableTextWriter : TextWriter
    {
        private readonly TextWriter underlyingWriter;

        public UndisposableTextWriter(TextWriter underlyingWriter)
        {
            this.underlyingWriter = underlyingWriter;
        }

        public override void Close()
        { }

        public override ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        protected override void Dispose(bool disposing)
        { }

        #region delegating members
        public override Encoding Encoding => underlyingWriter.Encoding;

        public override IFormatProvider FormatProvider => underlyingWriter.FormatProvider;

        public override string NewLine
        {
            get => underlyingWriter.NewLine;
            set => underlyingWriter.NewLine = value;
        }

        public override void Flush()
        {
            underlyingWriter.Flush();
        }

        public override Task FlushAsync()
        {
            return underlyingWriter.FlushAsync();
        }

        public override object InitializeLifetimeService()
        {
            return underlyingWriter.InitializeLifetimeService();
        }

        public override void Write(bool value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(char value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(char[] buffer)
        {
            underlyingWriter.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            underlyingWriter.Write(buffer, index, count);
        }

        public override void Write(decimal value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(double value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(int value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(long value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(object value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            underlyingWriter.Write(buffer);
        }

        public override void Write(float value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(string value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            underlyingWriter.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            underlyingWriter.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            underlyingWriter.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] arg)
        {
            underlyingWriter.Write(format, arg);
        }

        public override void Write(StringBuilder value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(uint value)
        {
            underlyingWriter.Write(value);
        }

        public override void Write(ulong value)
        {
            underlyingWriter.Write(value);
        }

        public override Task WriteAsync(char value)
        {
            return underlyingWriter.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            return underlyingWriter.WriteAsync(buffer, index, count);
        }

        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            return underlyingWriter.WriteAsync(buffer, cancellationToken);
        }

        public override Task WriteAsync(string value)
        {
            return underlyingWriter.WriteAsync(value);
        }

        public override Task WriteAsync(StringBuilder value, CancellationToken cancellationToken = default)
        {
            return underlyingWriter.WriteAsync(value, cancellationToken);
        }

        public override void WriteLine()
        {
            underlyingWriter.WriteLine();
        }

        public override void WriteLine(bool value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(char value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(char[] buffer)
        {
            underlyingWriter.WriteLine(buffer);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            underlyingWriter.WriteLine(buffer, index, count);
        }

        public override void WriteLine(decimal value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            underlyingWriter.WriteLine(buffer);
        }

        public override void WriteLine(float value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(string value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            underlyingWriter.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            underlyingWriter.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            underlyingWriter.WriteLine(format, arg0, arg1, arg2);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            underlyingWriter.WriteLine(format, arg);
        }

        public override void WriteLine(StringBuilder value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(uint value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override void WriteLine(ulong value)
        {
            underlyingWriter.WriteLine(value);
        }

        public override Task WriteLineAsync()
        {
            return underlyingWriter.WriteLineAsync();
        }

        public override Task WriteLineAsync(char value)
        {
            return underlyingWriter.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            return underlyingWriter.WriteLineAsync(buffer, index, count);
        }

        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            return underlyingWriter.WriteLineAsync(buffer, cancellationToken);
        }

        public override Task WriteLineAsync(string value)
        {
            return underlyingWriter.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(StringBuilder value, CancellationToken cancellationToken = default)
        {
            return underlyingWriter.WriteLineAsync(value, cancellationToken);
        }
        #endregion
    }
}
