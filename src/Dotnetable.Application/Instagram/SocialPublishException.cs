namespace Dotnetable.Application.Instagram;

/// <summary>Telegram or X refused the call. The message is safe to show to the admin.</summary>
public sealed class SocialPublishException : Exception
{
    public SocialPublishException(string message) : base(message) { }
}
