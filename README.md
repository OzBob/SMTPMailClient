# SMTPMailClient
MailKit wrapper for sending emails

TLS1.3 is unsupported, but if requried can be uncommented.

TLS1.2 is required for o365 integrations

Sample call can be found in Sendtestemail.Program.Main commandline test exe

A messageId is required by modern mail servers to enable DKIM:Pass  and avoid the Junk folder


