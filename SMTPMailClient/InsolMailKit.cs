using MailKit;
using MailKit.Net.Smtp;
using Org.BouncyCastle.Asn1.Esf;
using System;
using System.Security.Authentication;
using System.Threading;

namespace ServiceBus.Service
{
	public interface IInsolMailKit
	{
		void SendEmail(System.Net.Mail.MailMessage mailmsg, int smtpClientPort, string servername, string smtpUsername, string smtpPassword, bool useSSL, bool overridecertificate = false, string messageid = "", bool generateMessageIdIfMissing = false);
	}
	public class InsolMailKit : IInsolMailKit
	{
		ITransferProgress _transferProgress;
		public InsolMailKit()
		{

		}
		public InsolMailKit(ITransferProgress transferProgress)
		{
			_transferProgress = transferProgress;
		}

		public void SendEmail(System.Net.Mail.MailMessage mailmsg, int smtpClientPort, string servername, string smtpUsername, string smtpPassword, bool useSSL, bool overridecertificate = false, string messageid = "", bool generateMessageIdIfMissing = false)
		{
			if (overridecertificate)
			{
				//HACK bypass the certificate validation
				//     …a connection to the specified mail server.
				//     The useSsl argument only controls whether or not the client makes an SSL-wrapped
				//     connection. In other words, even if the useSsl parameter is false, SSL/TLS may
				//     still be used if the mail server supports the STARTTLS extension.
				//     To disable all use of SSL/TLS, use the MailKit.MailService.Connect(System.String,System.Int32,MailKit.Security.SecureSocketOptions,System.Threading.CancellationToken)
				//     overload with a value of SecureSocketOptions.None instead.
				//     To disable all use of SSL/TLS, use the 
				System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
						System.Security.Cryptography.X509Certificates.X509Certificate certificate,
						System.Security.Cryptography.X509Certificates.X509Chain chain,
						System.Net.Security.SslPolicyErrors sslPolicyErrors)
				{
					return true;
				};
			}
			var msg = MimeKit.MimeMessage.CreateFromMailMessage(mailmsg);

			if (!string.IsNullOrEmpty(messageid))
				msg.MessageId  = messageid;
			else if (generateMessageIdIfMissing)
				msg.MessageId  = SMTPMailClient.MimeUtils.GenerateMessageId(servername);
			
			//Send Message
			// Set the method that is called back when the send operation ends.
			using (var client = new SmtpClient())
			{
				System.Diagnostics.Debug.WriteLine("Connect");
				if (overridecertificate)
				{
					client.CheckCertificateRevocation = false;
				}
				if (useSSL)
				{
					client.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;// | SslProtocols.Tls13;
					//st, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default
					client.Connect(servername, smtpClientPort, MailKit.Security.SecureSocketOptions.Auto);
					//client.Connect(servername, smtpClientPort, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
					//client.Connect(servername, smtpClientPort, useSSL);
				}
				else
				{
					//if (overridecertificate)
					//{
					//    client.Connect(servername, smtpClientPort, MailKit.Security.SecureSocketOptions.None);
					//}
					//else
					client.Connect(servername, smtpClientPort, MailKit.Security.SecureSocketOptions.Auto);
				}
				/*
http://www.mimekit.net/docs/html/Working-With-SMime.htm
public void MultipartSign (MimeMessage message, X509Certificate2 certificate)
{
// digitally sign our message body using our custom S/MIME cryptography context
using (var ctx = new MySecureMimeContext ()) {
	var signer = new CmsSigner (certificate) {
		DigestAlgorithm = DigestAlgorithm.Sha1
	};

	message.Body = MultipartSigned.Create (ctx, signer, message.Body);
}
}
You can also choose to digitially sign a MimeEntity using the application/pkcs7-mime format using ApplicationPkcs7Mime.Sign
http://www.mimekit.net/docs/html/Overload_MimeKit_Cryptography_ApplicationPkcs7Mime_Sign.htm
public void Pkcs7Sign (MimeMessage message)
{
// digitally sign our message body using our custom S/MIME cryptography context
using (var ctx = new MySecureMimeContext ()) {
	// Note: this assumes that the Sender address has an S/MIME signing certificate
	// and private key with an X.509 Subject Email identifier that matches the
	// sender's email address.
	var sender = message.From.Mailboxes.FirstOrDefault ();

	message.Body = ApplicationPkcs7Mime.Sign (ctx, sender, DigestAlgorithm.Sha1, message.Body);
}
}
*/
				System.Diagnostics.Debug.WriteLine("Authenticate");

				if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
					client.Authenticate(smtpUsername, smtpPassword);
				System.Diagnostics.Debug.WriteLine("Send");
				if (_transferProgress == null)
					client.SendAsync(msg).Wait();
				else
					client.SendAsync(msg, default(CancellationToken), _transferProgress).Wait();
				System.Diagnostics.Debug.WriteLine("Discconnect");
				client.Disconnect(true);
			}
		}
	}
}
