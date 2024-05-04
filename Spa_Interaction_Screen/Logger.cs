using System.Diagnostics;

namespace Spa_Interaction_Screen
{
    public static class Logger
    {
        private static List<Log_Element>[] log_Elements = null;
        private static byte currentlyshowing = Byte.MaxValue;
        private static FileStream BackupLOG = null;
        private static FileStream LOG = null;
        public static RichTextBox ConsoleBox;
        public static ComboBox consoletype;
        public static ComboBox consolesubtype;
        public static ColorSlider.ColorSlider ConsoleTextscroll;
        public static bool consoleshown = false;

        public static List<Log_Element> getList(int index)
        {
            if(log_Elements != null && log_Elements.Length> index && log_Elements[index] == null || log_Elements[index].Count <= 0)
            {
                return null;
            }
            return log_Elements[index];
        }

        public static void closeStreams()
        {
            if (BackupLOG != null)
            {
                BackupLOG.Close();
                BackupLOG.Dispose();
                BackupLOG = null;
            }
            if (LOG != null)
            {
                LOG.Close();
                LOG.Dispose();
                LOG = null;
            }
        }

        public class Log_Element
        {
            public byte? SubType;
            public String Message;
            public DateTime time;
            public byte[] type;

            public override string ToString()
            {
                String p = "";
                p += TimeToString(DateTime.Now);
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
            
        }
        public static string TimeToString(DateTime Time)
        {
            string[] times;
            times = Time.GetDateTimeFormats();
            if (times.Length <= 0)
            {
                Logger.Print("No DateTimeFormats found for current Time", [MessageType.Logger], MessageSubType.Error, true);
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
                Logger.Print($"Datetime Format {Constants.DateTimeFormat} not available (try smaller number). Using different Method.", [MessageType.Logger], MessageSubType.Notice, true);
                p += Time.Year;
                p += '.';
                p += Time.Month;
                p += '.';
                p += Time.Day;
                p += ';';
                p += Time.Hour;
                p += ":";
                p += Time.Minute;
                p += ":";
                p += Time.Second;
            }
            p += ']';
            return p;
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
            Logger = 12
        }

        public enum MessageSubType : byte
        {
            Ohne_Kategorie = 0,
            Error = 1,
            Notice = 2,
            Information = 3,
        }

        public static void Print(String Message, MessageType? Type, MessageSubType? Subtype)
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


        public static void Print(String Message, MessageType?[] Type, MessageSubType? Subtype)
        {
            Print(Message, Type, Subtype, false);
        }


        public static void Print(String Message, MessageType?[] Type, MessageSubType? Subtype, bool ShowfullMessageLater)
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
                if (valid > 0)
                {
                    addElement(log, ShowfullMessageLater);
                }
                if (ShowfullMessageLater)
                {
                    Debug.Print(log.Message); 
                    if (FOpen(BackupLOG))
                    {
                        StreamWriter sw = new StreamWriter(BackupLOG);
                        sw.WriteLine(log.Message + "\n");
                        sw.Flush();
                    }
                    if (FOpen(LOG))
                    {
                        StreamWriter sw = new StreamWriter(LOG);
                        sw.WriteLine(log.Message + "\n");
                        sw.Flush();
                    }
                }
                else
                {
                    Debug.Print(log.ToString());
                    if (FOpen(BackupLOG))
                    {
                        StreamWriter sw = new StreamWriter(BackupLOG);
                        sw.WriteLine(log.ToString());
                        sw.Flush();
                    }
                    if (FOpen(LOG))
                    {
                        StreamWriter sw = new StreamWriter(LOG);
                        sw.WriteLine(log.ToString());
                        sw.Flush();
                    }
                }
            }
        }

        public static int addElement(Log_Element LE, bool ShowfullMessageLater)
        {
            if (log_Elements == null || log_Elements.Length <= 0 || BackupLOG == null)
            {
                log_Elements = new List<Log_Element>[Byte.MaxValue];
                Log_Element start = new Log_Element();
                start.type = [MTypetobyte<MessageType>(MessageType.Logger)];
                start.SubType = MTypetobyte<MessageSubType>(MessageSubType.Information);
                start.time = DateTime.Now;
                start.Message = $"Welcome to the Interaction Screen Version {Constants.CurrentVersion}";
                if (BackupLOG == null)
                {
                    File.Create(Constants.BackupLOGPath).Close();
                    BackupLOG = CreateLOGHandle(Constants.BackupLOGPath);
                    initLog(BackupLOG);
                }
                addElement(start, true);
            }
            int i;
            for (i = 0; i < LE.type.Length; i++)
            {
                if (consoletype != null)
                {
                    if(log_Elements[LE.type[i]] == null || log_Elements[LE.type[i]].Count <= 0)
                    {
                        log_Elements[LE.type[i]] = new List<Log_Element>();
                    }
                    if (log_Elements[LE.type[i]].Count <= 0)
                    {
                        consoletype.Items.Add(new Constants.ComboItem { Text = ((MessageType)LE.type[i]).ToString(), ID = LE.type[i] });
                    }
                }
                if (consoleshown && currentlyshowing == LE.type[i])
                {
                    if (ShowfullMessageLater)
                    {
                        AddConsoleLine(LE.Message);
                    }
                    else
                    {
                        AddConsoleLine(LE.ToString());
                    }
                }
                if (log_Elements[LE.type[i]] == null || log_Elements[LE.type[i]].Count <= 0)
                {
                    log_Elements[LE.type[i]] = new List<Log_Element>();
                }
                log_Elements[LE.type[i]].Add(LE);
                if (!ShowfullMessageLater  && LE.type[i] == currentlyshowing)
                {
                    AddConsoleLine(LE.ToString());
                }
            }
            return i;
        }

        public static void initLog(FileStream x)
        {
            if(x == null)
            {
                Logger.Print("FileStream for Log File init is null", MessageType.Logger, MessageSubType.Error);
            }
            StreamWriter z = new StreamWriter(x);
            z.WriteLine($"Spa_Interaction_Screen\nVersion:{Constants.CurrentVersion}\n[{Logger.TimeToString(DateTime.Now)}]"); 
            z.Flush();
        }

        public static bool FOpen(FileStream file)
        {
            return file != null && file.CanWrite;
        }

        public static byte MTypetobyte<T>(T? type) where T : struct, IConvertible
        {
            if(type == null)
            {
                return Byte.MaxValue;
            }
            Object t = Convert.ChangeType(type, ((T)type).GetTypeCode());
            return (byte)t;
        }

        public static String GetConsoleText(MessageType type)
        {
            return GetConsoleText(type, null);
        }

        public static String GetConsoleText(MessageType type, MessageSubType? subtype)
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

        public static void setCurrentlyshowing(byte show)
        {
            currentlyshowing = show;
        }

        public static void InitLogfromBackup(Config c)
        {
            if (BackupLOG == null)
            {
                File.Create(Constants.BackupLOGPath).Close();
                BackupLOG = CreateLOGHandle(Constants.BackupLOGPath);
                initLog(BackupLOG);
            }
            if(c.LogPath == null)
            {
                Debug.Print($"mainForm.config.LogoFilePath is null");
            }
            File.Copy(Constants.BackupLOGPath, c.LogPath, true);
            LOG = CreateLOGHandle(c.LogPath);
        }

        private static FileStream CreateLOGHandle(String path)
        {
            FileStream tmp = null;
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                tmp = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            else
            {
                Print("Could not create Log File", MessageType.Logger, MessageSubType.Error);
            }
            if(tmp != null)
            if (!FOpen(tmp))
            {
                tmp = null;
            }
            return tmp;
        }

        public static String AddConsoleLine(String line)
        {
            if (line != null && line.Length > 0)
            {
                if (ConsoleBox != null)
                {
                    bool scroll = true;
                    if (ConsoleBox.SelectionStart == ConsoleBox.Text.Length)
                    {
                        scroll = false;
                    }
                    ConsoleBox.Text += line;
                    ConsoleBox.Text += "\n\r";
                    if (scroll)
                    {
                        ConsoleBox.SelectionStart = ConsoleBox.Text.Length;
                        ConsoleTextscroll.Value = ConsoleBox.SelectionStart * -1 + ConsoleTextscroll.Maximum;
                        ConsoleBox.ScrollToCaret();
                    }
                    if (TextRenderer.MeasureText(ConsoleBox.Text, ConsoleBox.Font).Height > ConsoleBox.Size.Height)
                    {
                        ConsoleTextscroll.Maximum = ConsoleBox.Text.Length;
                        ConsoleTextscroll.Show();
                    }
                    else
                    {
                        ConsoleTextscroll.Hide();
                    }
                    return ConsoleBox.Text;
                }
                return line;
            }
            return "";
        }
    }
}
