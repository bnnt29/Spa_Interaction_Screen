using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spa_Interaction_Screen
{
    public class Logger
    {
        private List<List<Log_Element>> log_Elements = new List<List<Log_Element>>();

        public Logger() 
        {
            for(int i = 0; i < byte.MaxValue; i++)
            {
                log_Elements.Add(new List<Log_Element>());
            }
            Log_Element start = new Log_Element();
            start.type = [MTypetobyte<MessageType>(MessageType.LoggerInfo)];
            start.SubType = MTypetobyte<MessageSubType>(MessageSubType.Information);
            start.time = DateTime.Now;
            start.Message = $"Welcome to the Interaction Screen Version {Constants.CurrentVersion}";
        }

        public class Log_Element
        {
            public byte? SubType;
            public String Message;
            public DateTime time;
            public byte[] type;
        }


        public enum MessageType : byte
        {
            Ohne_Kategorie = 0,
            Hauptprogramm = 1,
            Benutzeroberfläche = 2,
            TCPReceive = 3,
            TCPSend = 4,
            Router = 5,
            Gastro = 6,
            Licht = 7,
            VideoProjektion = 8,
            Intern = 9,
            Extern = 10,
            Konfig = 11,
            LoggerInfo = 12
        }

        public enum MessageSubType : byte
        {
            Ohne_Kategorie = 0,
            Error = 1,
            Notice = 2,
            Information = 3,
        }

        public void Print(String Message, MessageType? Type, MessageSubType? Subtype)
        {
            Log_Element log = new Log_Element();
            String p = "";
            p += TimeToString(DateTime.Now); 
            p += " : ";
            if (Type != null)
            {
                p += '{';
                p += Type.ToString();
                if (Subtype != null)
                {
                    p += ':';
                    p += Subtype.ToString();
                }
                p += '}';
                log.type = [MTypetobyte(Type)];
            }
            p += ' ';
            p += Message;
            if (log.type[0] != Byte.MaxValue)
            {
                log.time = DateTime.Now;
                log.SubType = MTypetobyte(Subtype);
                log.Message = Message;
                log_Elements[log.type[0]].Add(log);
            }
            Debug.Print(p);

        }

        public void Print(String Message, MessageType?[] Type, MessageSubType? Subtype)
        {
            Log_Element log = new Log_Element();
            String p = "";
            p += TimeToString(DateTime.Now);
            p += " : ";
            int valid = 0;
            if (Type != null)
            {
                p += '{';
                p += Type.ToString();
                if (Subtype != null)
                {
                    p += ':';
                    p += Subtype.ToString();
                }
                p += '}';
                log.type = new byte[Type.Length];
                for(int i = 0; i < Type.Length; i++)
                {
                    log.type[i] = MTypetobyte(Type[i]);
                    if (log.type[i] != Byte.MaxValue) 
                    { 
                        valid++;
                    }
                    else
                    {
                        i--;
                    }
                }
            }
            p += ' ';
            p += Message;
            if (valid>0)
            {
                log.time = DateTime.Now;
                log.SubType = MTypetobyte(Subtype);
                log.Message = Message;
                for(int i = 0; i < log.type.Length; i++)
                {
                    log_Elements[log.type[i]].Add(log);
                }
            }
            Debug.Print(p);

        }

        private string TimeToString(DateTime time)
        {
            string p = "";
            p += '[';
            p += time.Year;
            p += '.';
            p += time.Month;
            p += '.';
            p += time.Day;
            p += ';';
            p += time.Hour;
            p += ":";
            p += time.Minute;
            p += ":";
            p += time.Second;
            p += ']';
            return p;
        }

        public byte MTypetobyte<T>(T? type) where T : struct, IConvertible
        {
            if(type == null)
            {
                return Byte.MaxValue;
            }
            Object t = Convert.ChangeType(type, ((T)type).GetTypeCode());
            return (byte)t;
        }
    }
}
