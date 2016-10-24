using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace Epi
{
    static class MailUtility
    {
        public static void Save(this MailMessage message, string filename, bool addUnsentHeader = true)
        {
            try
            {
                using (var filestream = File.Open(filename, FileMode.Create))
                {


                    if (addUnsentHeader)
                    {
                        var binaryWriter = new BinaryWriter(filestream);
                        binaryWriter.Write(System.Text.Encoding.UTF8.GetBytes("X-Unsent: 1" + Environment.NewLine));
                    }

                    var assembly = typeof(SmtpClient).Assembly;
                    var mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");
                    var mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);
                    var mailWriter = mailWriterContructor.Invoke(new object[] { filestream });
                    var sendMethod = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);
                    sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { mailWriter, true, true }, null);

                    if (message.Attachments.Count > 0)
                    {
                        message.Attachments[0].Dispose();
                    }
                }

                var fileContents = System.IO.File.ReadAllText(filename);
                fileContents = fileContents.Replace("ita3@cdc.gov", "");
                System.IO.File.WriteAllText(filename, fileContents);
            }
            catch { }
        }
    }
}
