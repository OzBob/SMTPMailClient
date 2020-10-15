using ServiceBus.Service;
using System;
using System.Runtime.CompilerServices;

namespace Sendtestemail
{
    class Program
    {
        static void Main(string[] args)
        {
            
            if (args.Length != 6)
            {
                Console.WriteLine("Sendtestemail from to server port un pwd [msgid]");
                return;
            }
            try
            {
                var from = args[0];
                var to = args[1];
                var server = args[2];
                var port = args[3];
                var un = args[4];
                var pwd = args[5];
                string mid = args.Length>5?args[6]:null;

                Console.WriteLine("Hello World!");
                IInsolMailKit email = new InsolMailKit();

                System.Net.Mail.MailMessage mailmsg = new System.Net.Mail.MailMessage(from, to);
                bool generateMessageIdIfMissing = false;
                email.SendEmail(mailmsg, Int32.Parse(port), server, un, pwd, true, false, mid, generateMessageIdIfMissing);

                Console.WriteLine("SendTestEmail Sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendTestEmail Fail:" + ex.ToString());
            }
        }
    }
}
