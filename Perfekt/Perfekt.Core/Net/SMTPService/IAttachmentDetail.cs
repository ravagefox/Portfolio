namespace Perfekt.Core.Net.SMTPService
{
    public interface IAttachmentDetail
    {
        string Name { get; }
        byte[] Data { get; }
        string ContentType { get; }
    }
}
