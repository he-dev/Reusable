using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;
using ContentDisposition = MimeKit.ContentDisposition;

// ReSharper disable once CheckNamespace
namespace Reusable.IOnymous.Controllers
{
    public class SmtpController : MailController
    {
        public SmtpController(IImmutableContainer properties = default) : base(properties.ThisOrEmpty()) { }

        [ResourcePost]
        public async Task<IResource> SendEmailAsync(Request request)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(request.Metadata.GetItemOrDefault(MailRequestMetadata.From)));
            message.To.AddRange(request.Metadata.GetItemOrDefault(MailRequestMetadata.To).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Cc.AddRange(request.Metadata.GetItemOrDefault(MailRequestMetadata.CC, Enumerable.Empty<string>().ToList()).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Subject = request.Metadata.GetItemOrDefault(MailRequestMetadata.Subject);
            var multipart = new Multipart("mixed");
//            {
//                new TextPart(request.Properties.GetItemOrDefault(From<IMailMeta>.Select(x => x.IsHtml)) ? TextFormat.Html : TextFormat.Plain)
//                {
//                    Text = await ReadBodyAsync(body, request.Properties)
//                }
//            };

            using (var body = await request.CreateBodyStreamAsync())
            {
                multipart.Add(new TextPart(request.Metadata.GetItemOrDefault(MailRequestMetadata.IsHtml) ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = await ReadBodyAsync(body, request.Metadata)
                });
            }

            foreach (var attachment in request.Metadata.GetItemOrDefault(MailRequestMetadata.Attachments, new Dictionary<string, byte[]>()).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
            {
                var attachmentPart = new MimePart(MediaTypeNames.Application.Octet)
                {
                    Content = new MimeContent(new MemoryStream(attachment.Value).Rewind()),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachment.Key
                };
                multipart.Add(attachmentPart);
            }

            message.Body = multipart;

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync
                (
                    request.Metadata.GetItemOrDefault(SmtpRequestMetadata.Host),
                    request.Metadata.GetItemOrDefault(SmtpRequestMetadata.Port),
                    request.Metadata.GetItemOrDefault(SmtpRequestMetadata.UseSsl, false)
                );
                await smtpClient.SendAsync(message);
            }

            return Resource.DoesNotExist.FromRequest(request);
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(SmtpRequestMetadata))]
    public class SmtpRequestMetadata : SelectorBuilder<SmtpRequestMetadata>
    {
        public static Selector<string> Host = Select(() => Host);

        public static Selector<int> Port = Select(() => Port);

        public static Selector<bool> UseSsl = Select(() => UseSsl);
    }
}