using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spa_Interaction_Screen
{
    public class Logger
    {
        private List<Log_Element>[] log_Elements = new List<Log_Element>[Byte.MaxValue];
        private byte currentlyshowing = Byte.MaxValue;
        private MainForm mainForm;

        public Logger() 
        {
            for(int i = 0; i < byte.MaxValue; i++)
            {
                log_Elements[i]=new List<Log_Element>();
            }
            Log_Element start = new Log_Element();
            start.type = [MTypetobyte<MessageType>(MessageType.LoggerInfo)];
            start.SubType = MTypetobyte<MessageSubType>(MessageSubType.Information);
            start.time = DateTime.Now;
            start.Message = $"Welcome to the Interaction Screen Version {Constants.CurrentVersion}";
        }

        public List<Log_Element> getList(int index)
        {
            return log_Elements[index];
        }

        public class Log_Element
        {
            public Logger? Log;
            public byte? SubType;
            public String Message;
            public DateTime time;
            public byte[] type;

            public override string ToString()
            {
                String p = "";
                p += TimeToString(DateTime.Now, Log);
                p += " : "; 
                if (type != null && type.Length > 0)
                {
                    p += "{[";
                    for (int i = 0; i < type.Length; i++)
                    {
                        p += ((MessageType)type[i]).ToString();
                        p += ',';
                    }
                    if (p.EndsWith(','))
                    {
                        p = p.Substring(0, p.Length - 1);
                    }
                    p += ']';
                    if (SubType != null)
                    {
                        p += " : ";
                        p += ((MessageSubType)SubType).ToString();
                    }
                    p += '}';
                }
                else
                {
                    if (SubType != null)
                    {
                        p += "{";
                        p += ((MessageSubType)SubType).ToString();
                        p += '}';
                    }
                }
                p += ' ';
                p += Message;
                return p;
            }

            private string TimeToString(DateTime time, Logger Log)
            {
                string[] times;
                times = time.GetDateTimeFormats();
                if (times.Length <= 0)
                {
                    Log.Print("No DateTimeFormats found for current Time", [MessageType.LoggerInfo], MessageSubType.Error, true);
                    return "";
                }
                string p = "";
                p += '[';
                if (Constants.DateTimeFormat < times.Length)
                {
                    p += times[Constants.DateTimeFormat];
                }
                else
                {
                    Log.Print($"Datetime Format {Constants.DateTimeFormat} not available (try smaller number). Using different Method.", [MessageType.LoggerInfo], MessageSubType.Notice, true);
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
                }
                p += ']';
                return p;
            }
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
            if (Type == null)
            {
                Print(Message, (MessageType?[])null, Subtype);
            }
            else
            {
                Print(Message, [Type], Subtype);
            }
        }


        public void Print(String Message, MessageType?[] Type, MessageSubType? Subtype)
        {
            Print(Message, Type, Subtype, false);
        }


        public void Print(String Message, MessageType?[] Type, MessageSubType? Subtype, bool ShowfullMessageLater)
        {
            IEnumerable<MessageType?> CleanType = Type.Where(x => x != null);
            if (CleanType.Count() > 0)
            {
                int valid = 0;
                Log_Element log = new Log_Element();
                log.type = new byte[CleanType.Count()];
                IEnumerator<MessageType?> Enumerator = CleanType.GetEnumerator();
                while (Enumerator.MoveNext())
                {
                    log.type[valid] = MTypetobyte(Enumerator.Current);
                    if (log.type[valid] != Byte.MaxValue)
                    {
                        valid++;
                    }
                }
                log.time = DateTime.Now;
                log.SubType = MTypetobyte(Subtype);
                log.Message = Message;
                log.Log = this;
                if (valid > 0)
                {
                    for (int i = 0; i < log.type.Length; i++)
                    {
                        if (mainForm != null && mainForm.consoletype != null)
                        {
                            if (log_Elements[log.type[i]].Count <= 0)
                            {
                                mainForm.consoletype.Items.Add(new Constants.ComboItem { Text = ((MessageType)log.type[i]).ToString(), ID = log.type[i] });
                            }
                        }
                        if (mainForm != null && mainForm.consoleshown && currentlyshowing == log.type[i])
                        {
                            if (ShowfullMessageLater)
                            {
                                mainForm.AddConsoleLine(log.Message);
                            }
                            else
                            {
                                mainForm.AddConsoleLine(log.ToString());
                            }
                        }
                        log_Elements[log.type[i]].Add(log);
                        if (!ShowfullMessageLater && mainForm != null && log.type[i] == currentlyshowing)
                        {
                            mainForm.AddConsoleLine(log.ToString());
                        }
                    }
                }
                if (ShowfullMessageLater)
                {
                    Debug.Print(log.Message);
                }
                else
                {
                    Debug.Print(log.ToString());
                }
            }
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

        public String GetConsoleText(MessageType type, MessageSubType? subtype)
        {
            String c = "";
            foreach(Log_Element log in log_Elements[MTypetobyte<MessageType>(type)])
            {
                if(subtype != null)
                {
                    if(MTypetobyte<MessageSubType>(subtype) == log.SubType)
                    {
                        c += log.ToString();
                        c += "\n";
                    }
                }
                else
                {
                    c += log.ToString();
                    c += "\n";
                }
            }
            return c;
        }

        public void setCurrentlyshowing(byte show, MainForm? form)
        {
            currentlyshowing = show;
            mainForm = form;
        }
    }
}
