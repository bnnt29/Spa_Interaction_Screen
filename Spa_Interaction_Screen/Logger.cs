using Microsoft.VisualBasic.Logging;
using System.Diagnostics;
using System.Threading;

namespace Spa_Interaction_Screen
{
    public static class Logger
    {
        private static Semaphore __Blogpool = new Semaphore(1, 1); 
        private static Semaphore __logpool = new Semaphore(1, 1);
        private static List<Log_Element>[] log_Elements = null;
        private static byte currentlyshowing = Byte.MaxValue;
        public static RichTextBox ConsoleBox;
        public static ComboBox consoletype;
        public static ComboBox consolesubtype;
        public static ColorSlider.ColorSlider ConsoleTextscroll;
        public static bool consoleshown = false;
        public static MainForm form;
        private delegate String MyAddConsoleLine(String line);
        private delegate int MyaddElement(Log_Element LE, bool ShowfullMessageLater);

        public static List<Log_Element> getList(int index)
        {
            if (log_Elements != null && log_Elements.Length > index && log_Elements[index] == null || log_Elements[index].Count <= 0)
            {
                return null;
            }
            return log_Elements[index];
        }

        public static void Clear()
        {
            log_Elements = new List<Log_Element>[Byte.MaxValue];
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
            VideoProjection = 8,
            Intern = 9,
            Extern = 10,
            Konfig = 11,
            Logger = 12,
            SystemState = 13
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
                    Constants.InvokeDelegate<int>([log, ShowfullMessageLater], new MyaddElement(addElement), form, Logger.MessageType.Logger);
                }
                if (ShowfullMessageLater)
                {
                    WritedoubleLogMessage(log.Message);
                }
                else
                {
                    WritedoubleLogMessage(log.ToString());
                }
            }
        }

        private static void WritedoubleLogMessage(String m)
        {
            Debug.WriteLine(m);
            try
            {
                __Blogpool.WaitOne();
                WriteSingleLogMessage(Constants.BackupLOGPath, m, false);
            }
            catch (IOException ex)
            {
                initLog();
            }
            catch (Exception e)
            {
                Logger.Print(e.Message, MessageType.Logger, MessageSubType.Error);
                Logger.Print("Die Log Dateien sind seit Programm start verschwunden", MessageType.Logger, MessageSubType.Notice);
            }
            finally
            {
                __Blogpool.Release();
            }
            try
            {
                __logpool.WaitOne();
                WriteSingleLogMessage(Config.LogPath, m, false);
            }catch(IOException ex)
            {
                InitLogfromBackup();
            }
            catch(Exception e)
            {
                Logger.Print(e.Message, MessageType.Logger, MessageSubType.Error);
                Logger.Print("Die Log Dateien sind seit Programm start verschwunden", MessageType.Logger, MessageSubType.Notice);
            }
            finally
            {
                __logpool.Release();
            }
        }

        private static void WriteSingleLogMessage(String Path, String log, Boolean shouldcreate)
        {
            WriteBunchLogMessages(Path, [log], shouldcreate);
        }

        private static void WriteBunchLogMessages(String Path, String[] log, Boolean shouldcreate)
        {
            if(log == null || log.Length == 0)
            {
                return;
            }
            if (Path != null && Path.Length > 0)
            {
                if (!File.Exists(Path))
                {
                    try
                    {
                        File.Create(Path);
                    }
                    catch(Exception ex)
                    {
                        Logger.Print(ex.Message, MessageType.Logger, MessageSubType.Error);
                        Logger.Print($"Could not Create File: {Path}", MessageType.Logger, MessageSubType.Notice);
                    }
                }
                try
                {
                    using (StreamWriter sw = new StreamWriter(Path, true))
                    {
                        if (sw != null)
                        {
                            foreach(String s in log)
                            {
                                sw.WriteLine(s);
                            }
                            try
                            {
                                sw.Flush();
                            }
                            catch (Exception ex)
                            {
                                Logger.Print(ex.Message, MessageType.Logger, MessageSubType.Error);
                                Logger.Print($"Could not write to {Path}", MessageType.Logger, MessageSubType.Notice);
                            }
                            finally
                            {
                                sw.Close();
                                sw.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print(ex.Message, MessageType.Logger, MessageSubType.Error);
                    Logger.Print($"Could not create {Path} Streamwriter", MessageType.Logger, MessageSubType.Notice);
                }
            }
        }

        public static int addElement(Log_Element LE, bool ShowfullMessageLater)
        {
            if (log_Elements == null || log_Elements.Length <= 0)
            {
                log_Elements = new List<Log_Element>[Byte.MaxValue];
                Log_Element start = new Log_Element();
                start.type = [MTypetobyte<MessageType>(MessageType.Logger)];
                start.SubType = MTypetobyte<MessageSubType>(MessageSubType.Information);
                start.time = DateTime.Now;
                start.Message = $"Welcome to the Interaction Screen Version {Constants.CurrentVersion}";
                Constants.InvokeDelegate<int>([start, true], new MyaddElement(addElement), form, Logger.MessageType.Logger);
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
            }
            return i;
        }

        public static void initLog()
        {
            try
            {
                __Blogpool.WaitOne();
                WriteSingleLogMessage(Constants.BackupLOGPath, $"Spa_Interaction_Screen\nVersion:{Constants.CurrentVersion}\n{Logger.TimeToString(DateTime.Now)}", true);
            }catch(Exception ex)
            {
                Logger.Print("Konnte Backup Log nicht erstellen oder öffnen.", MessageType.Logger, MessageSubType.Error);
            }finally 
            { 
                __Blogpool.Release(); 
            }
        }

        public static bool FOpenWrite(FileStream file)
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
                        c += log.ToString() + "\r\n";
                    }
                }
                else
                {
                    c += log.ToString() + "\r\n";
                }
            }
            return c;
        }

        public static void setCurrentlyshowing(byte show)
        {
            currentlyshowing = show;
        }

        public static byte getCurrentlyshowing()
        {
            return currentlyshowing;
        }

        public static void InitLogfromBackup()
        {
            if (Constants.BackupLOGPath == null)
            {
                File.Create(Constants.BackupLOGPath).Close();
                initLog();
            }
            if(Config.LogPath == null)
            {
                Debug.Print($"LogFilePath is null");
            }
            List<Log_Element> sorted = combineandsort(log_Elements);
            String[] logs = new string[sorted.Count];
            for(int i = 0;i<sorted.Count;i++)
            {
                logs[i] = sorted[i].ToString();
            }
            try
            {
                __logpool.WaitOne();
                WriteBunchLogMessages(Config.LogPath, logs, true);
            }catch(Exception e)
            {
                Debug.Print("2");
                Logger.Print(e.Message, MessageType.Logger, MessageSubType.Error);
                Logger.Print("InitLogfromBackup", MessageType.Logger, MessageSubType.Notice);
            }
            finally
            {
                __logpool.Release();
            }
        }

        private static FileStream CreateLOGHandle(String path)
        {
            if(path == null || path.Length <= 0)
            {
                return null;
            }
            if (!File.Exists(path))
            {
                try
                {
                    File.Create(path).Close();
                }
                catch (Exception ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                    Logger.Print($"Missing Permissions to Open File:{path}", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                }
            }
            FileStream tmp = null;
            if (File.Exists(path))
            {
                tmp = CreateStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            else
            {
                Print("Could not create Log File", MessageType.Logger, MessageSubType.Error);
            }
            if(tmp != null)
            if (!FOpenWrite(tmp))
            {
                tmp = null;
            }
            return tmp;
        }

        public static String AddConsoleLine(String line)
        {
            return Constants.InvokeDelegate<String>([line], new MyAddConsoleLine(delegateAddConsoleLine), form.vlc, Logger.MessageType.Logger);
        }

        public static String delegateAddConsoleLine(String line)
        {
            if (line != null && line.Length > 0)
            {
                if (ConsoleBox != null)
                {
                    bool scroll = true;
                    try
                    {
                        if (ConsoleBox.SelectionStart == ConsoleBox.Text.Length)
                        {
                            scroll = false;
                        }
                        ConsoleBox.Text += line;
                        ConsoleBox.Text += "\r\n";
                        if (scroll)
                        {
                            ConsoleBox.SelectionStart = ConsoleBox.Text.Length;
                            int x = ((int)(ConsoleBox.SelectionStart * -1 + ConsoleTextscroll.Maximum));
                            x = Math.Max(x, ((int)ConsoleTextscroll.Minimum));
                            x = Math.Min(x, ((int)ConsoleTextscroll.Maximum));
                            ConsoleTextscroll.Value = x;
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
                    }catch(Exception ex)
                    {
                        Logger.Print(ex.Message, MessageType.Benutzeroberfläche, MessageSubType.Error);
                        Logger.Print("delegateAddConsoleLine", MessageType.Benutzeroberfläche, MessageSubType.Notice);
                        return "";
                    }
                }
                return line;
            }
            return "";
        }

        public static FileStream CreateStream(String path, FileMode fm, FileAccess fa, FileShare fs)
        {
            FileStream fstream = null;
            try
            {
                File.SetAttributes(path, FileAttributes.Normal);
                fstream = File.Open(path, fm, fa, fs);
            }
            catch (IOException ex)
            {
                MainForm.currentState = 7;
                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Logger.Print($"Could not open File: {path}", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                if (fstream != null)
                {
                    fstream.Close();
                }
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                MainForm.currentState = 7;
                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Logger.Print($"Missing Permissions to Open File:{path}", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                if (fstream != null)
                {
                    fstream.Close();
                }
                return null;
            }catch(Exception ex)
            {
                MainForm.currentState = 7;
                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Logger.Print($"Missing Permissions to Open File:{path}", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                if (fstream != null)
                {
                    fstream.Close();
                }
                return null;
            }
            return fstream;
        }

        public static List<Log_Element> combineandsort(List<Log_Element>[] ListArray)
        {
            if(ListArray == null || ListArray.Length == 0)
            {
                return new List<Log_Element>();
            }
            List<Log_Element> comb = new List<Log_Element>();
            int[] index = new int[ListArray.Length];
            int Elementsleft = 0;
            for (int i = 0; i < ListArray.Length; i++)
            {
                if (ListArray[i] == null)
                {
                    index[i] = -1;
                }
                else
                {
                    index[i] = ListArray[i].Count;
                    Elementsleft += index[i];
                }
            }
            while(Elementsleft > 0)
            {
                int nextelementlist = -1;
                DateTime oldest = DateTime.Now;
                for(int i = 0;i< ListArray.Length; i++)
                {
                    if (index[i] <= 0)
                    {
                        continue;
                    }
                    if (ListArray[i][index[i]-1].time < oldest)
                    {
                        nextelementlist = i;
                    }
                }
                if(nextelementlist > 0)
                {
                    comb.Add(ListArray[nextelementlist][index[nextelementlist]-1]);
                    index[nextelementlist]--;
                }
                else
                {
                    break;
                }
                Elementsleft--;
            }

            return comb;
        }
    }
}
