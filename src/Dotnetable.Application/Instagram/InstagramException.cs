namespace Dotnetable.Application.Instagram;

/// <summary>Instagram refused the call, or the post cannot be sent. The message is safe to show to the admin.</summary>
public sealed class InstagramException : Exception
{
    public InstagramException(string message) : base(message) { }
}
