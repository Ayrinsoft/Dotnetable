namespace Dotnetable.Admin.Components.Pages.Media;

/// <summary>
/// Wraps a readable stream and reports cumulative bytes read (0..1) as the source is consumed.
/// Used to surface upload progress while <see cref="IBrowserFile"/> content is streamed into the server.
/// </summary>
internal sealed class ProgressStream : Stream
{
    private readonly Stream _inner;
    private readonly long _totalBytes;
    private readonly Action<double> _onProgress;
    private long _bytesRead;
    private double _lastReported = -1;

    public ProgressStream(Stream inner, long totalBytes, Action<double> onProgress)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _totalBytes = Math.Max(0, totalBytes);
        _onProgress = onProgress ?? throw new ArgumentNullException(nameof(onProgress));
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _totalBytes > 0 ? _totalBytes : _inner.Length;
    public override long Position
    {
        get => _bytesRead;
        set => throw new NotSupportedException();
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var n = await _inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        Report(n);
        return n;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var n = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        Report(n);
        return n;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var n = _inner.Read(buffer, offset, count);
        Report(n);
        return n;
    }

    private void Report(int justRead)
    {
        if (justRead <= 0) return;
        _bytesRead += justRead;
        if (_totalBytes <= 0) return;

        var fraction = Math.Clamp(_bytesRead / (double)_totalBytes, 0d, 1d);
        // Throttle UI updates (~every 0.5%).
        if (fraction < 1d && Math.Abs(fraction - _lastReported) < 0.005) return;
        _lastReported = fraction;
        _onProgress(fraction);
    }

    public override void Flush() => _inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _inner.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _inner.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
