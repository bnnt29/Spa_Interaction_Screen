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
        private List<Log_Element> log_Elements = new List<Log_Element>();

        public class Log_Element
        {
            public String SubType;
            public String Message;
            public byte type;
        }

        public enum MessageType
        {
            Ohne_Kategorie = 0,
            Hauptprogramm = 1,
            Benutzeroberfläche = 2,
            Zentrale = 3,
            Router = 4,
            Gastro = 5,
            Licht = 6,
            Intern = 7,
            Extern = 8,
            Konfig = 9
        }

        public void Print(String Message, MessageType? Type, String? Subtype)
        {
            String p = "";
            Log_Element log = new Log_Element();
            if(Type != null)
            {
                p += '[';
                p += getStringfromMessaeType(Type);
                p += ']';
                log.type = MTypetobyte(Type);
                if (log.type == Byte.MaxValue)
                {

                }
            }
            
        }

        public byte MTypetobyte(MessageType? type)
        {
            if(type == 0)
            {
                return Byte.MaxValue;
            }
            int t = Byte.MaxValue;
            if (type.HasValue)
            {
                t = (int)type.Value;
            }
            else
            {
                switch (type)
                {
                    case MessageType.Ohne_Kategorie:
                        t = 0;
                        break;
                    case MessageType.Hauptprogramm:
                        t = 1;
                        break;
                    case MessageType.Benutzeroberfläche:
                        t = 2;
                        break;
                    case MessageType.Zentrale:
                        t = 3;
                        break;
                    case MessageType.Router:
                        t = 4;
                        break;
                    case MessageType.Gastro:
                        t = 5;
                        break;
                    case MessageType.Licht:
                        t = 6;
                        break;
                    case MessageType.Intern:
                        t = 7;
                        break;
                    case MessageType.Extern:
                        t = 8;
                        break;
                    case MessageType.Konfig:
                        t = 9;
                        break;
                    default:
                        t = Byte.MaxValue;
                        break;
                }
            }
            return (byte)t;
        }

        public String getStringfromMessaeType(MessageType? mt)
        {
            switch (mt)
            {
                case MessageType.Ohne_Kategorie:
                    return "Ohne_Kategorie";
                case MessageType.Hauptprogramm:
                    return "Hauptprogramm";
                case MessageType.Benutzeroberfläche:
                    return "Benutzeroberfläche";
                case MessageType.Zentrale:
                    return "Zentrale";
                case MessageType.Router:
                    return "Router";
                case MessageType.Gastro:
                    return "Gastro";
                case MessageType.Licht:
                    return "Licht";
                case MessageType.Intern:
                    return "Intern";
                case MessageType.Extern:
                    return "Extern";
                case MessageType.Konfig:
                    return "Konfig";
                default:
                    return "other";
            }
        }
    }
}
