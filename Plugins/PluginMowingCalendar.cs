using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Positec;
using Plugin;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

public class PluginMowingCalendar : IPlugin {

	// Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
	const string PLUGIN_DESC ="Display the mowing calendar";
    const string PLUGIN_NAME = "MowingCalendar";

    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras =>  MyParas;
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    enum Action { nothing, display, export };
    private Action MyAction = Action.nothing;

    // Form Definition    
    public PluginParaText ParaZones;
    public PluginParaText ParaPointers;
    public PluginParaText ParaStatus;
    public PluginParaText ParaCorrection;
    public PluginParaText ParaTimeStamp;    
    public PluginParaBool ParaExport;
    public PluginParaText[] ParaItems = new PluginParaText[16];
    
    // Event: On Open
    public PluginMowingCalendar() {
        ParaZones = new PluginParaText(DESC_ZONES, VAL_UNKNOWN, "", true);
        ParaPointers = new PluginParaText(DESC_POINTER, VAL_UNKNOWN, "", true);
        ParaStatus = new PluginParaText(DESC_STATUS, VAL_UNKNOWN, "", true);
        ParaCorrection = new PluginParaText(DESC_CORRECTION, VAL_UNKNOWN, "", true);
        ParaTimeStamp = new PluginParaText(DESC_TIMESTAMP, VAL_UNKNOWN, "", true);
        ParaExport = new PluginParaBool(DESC_EXPORT, false, "");
        MyParas = new List<PluginParaBase>() { ParaZones,  ParaPointers, ParaStatus, ParaCorrection, ParaTimeStamp, ParaExport };
        for (int Pos = 0; Pos < 16; Pos++) {
            ParaItems[Pos] = new PluginParaText((Pos == 0 ? DESC_CALENDAR : ""), (Pos == 0 ? VAL_UNKNOWN : ""), "", true);
            MyParas.Add(ParaItems[Pos]);
        }        
     }
    
    // Event: New MQTT data received
    async void IPlugin.Todo(PluginData pd) {
        if ((MyAction == Action.display) || (MyAction == Action.export)) {
            CreateMowingCalendar(pd, MyAction == Action.export);
            MyAction = Action.nothing;
        }
    }
    
    // Event: Doit Button pressed
    void IPlugin.Doit(PluginData pd) {
        MyAction = (ParaExport.Check ? Action.export : Action.display);
        ParaExport.Check = false;
        DeskApp.Send("{}", pd.Index);
        MyTrace($"{pd.Name} polled.");
    }    

    // Event: On Close
    void IDisposable.Dispose() { 
    }

    // Writing a trace text 
    private void MyTrace(string Text) {
        DeskApp.Trace(PLUGIN_NAME + ": " + Text);
    }   
  
    // Data file name 
    string DataFile(string Header, string Trailer, string Extension) {
        return Path.Combine(DeskApp.DirData, Header + PLUGIN_NAME + Trailer + Extension);  
    }
    
    // InfoBox
    async void InfoBox(string Header, string Message, string Original = "", string Replacement = "") {
        string tmp = Message;
        if (Original != "") {
            string[] Ori = Original.Split(";");
            string[] Rep = Replacement.Split(";");
            for (int i = 0; i < Ori.Length; i++) {
                tmp = tmp.Replace(Ori[i], Rep[i]);
            }            
        }
        var msgpar = new MessageBoxStandardParams {
            ButtonDefinitions = ButtonEnum.Ok, 
            ContentHeader = PLUGIN_NAME + "\r\n" + Header,
            ContentMessage = tmp, 
            Icon = MsBox.Avalonia.Enums.Icon.Info,
            MaxWidth = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var msgbox = MessageBoxManager.GetMessageBoxStandard(msgpar);
        if(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            await msgbox.ShowWindowDialogAsync(desktop.MainWindow);
        }
    }      
    
	// Create mowing calendar for grid or for text file 
    async void CreateMowingCalendar(PluginData pd, bool ExportFlag) {
        ParaZones.Text = BeautifulTable(pd.Config.MultiZonePercs, 1, SEP_TABLE);
        ParaPointers.Text = MarkPointer(pd.Data.LastZone);
        ParaStatus.Text = pd.Data.LastState.ToString();
        ParaCorrection.Text = pd.Config.Schedule.Perc.ToString() + "%";
        ParaTimeStamp.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        string[] MySchedule = CollectDepartures(pd);
        BubbleSort(MySchedule);
        MySchedule = ZoneCalendar(pd.Data.LastZone, pd.Data.LastState, MySchedule, pd.Config.MultiZonePercs, pd.Config.Schedule.Perc);
        string[] FZCalendar = FormatZoneCalendar(MySchedule);
        if (ExportFlag == true) {
            ExportZoneCalendar(FZCalendar, pd.Name);
        }
        GridZoneCalendar(FZCalendar);
    }

	// Beautify the table
    private string BeautifulTable(int[] Table, int OffSet, string Sep) {
        string[] StrTable = new string[Table.Length];
        for (int Pos = 0; Pos < Table.Length; Pos++) {
            StrTable[Pos] = (Table[Pos] + OffSet).ToString();
        }
        return Sep.Substring(Sep.Length - 1) + String.Join(Sep, StrTable) + Sep.Substring(0, 1);
    }

    // Mark the pointer position
    private string MarkPointer(int Pointer) {
        int StartPos = 5 * Pointer;
        return GRD_CURRENTZONEPOINTER.Remove(StartPos, GRD_POINTER.Length)
            .Insert(StartPos, GRD_POINTER);
    }

    // Collect the departures
    private string[] CollectDepartures(PluginData pd) {
        string[] MyDepartures = new string[14];
        List<List<object>> Days;
        // 1st departures
        Days = pd.Config.Schedule.Days;
        for(int DayNo = 0; DayNo < 7; DayNo++) {
            MyDepartures[DayNo] = DepartureLine(DayNo, Days);
        }
        // 2nd departures
        if (pd.Config.Schedule.DDays != null) {
            Days = pd.Config.Schedule.DDays;
            for(int DayNo = 0; DayNo < 7; DayNo++) {
                MyDepartures[DayNo + 7] = DepartureLine(DayNo, Days);
            } 
        } else {
            Array.Resize(ref MyDepartures, 7);
        }
        return MyDepartures;
    }
	
    // One departure line
    private string DepartureLine(int DayNo, List<List<object>> Days) {
        // WeekDay | Start time | Duration | Border cut
        return String.Join(";", (new string[] {DayNo.ToString(), Days[DayNo][0].ToString(),
            Days[DayNo][1].ToString(), Days[DayNo][2].ToString()}));
    }	

    // Sort array in ascending order
    private void BubbleSort(string[] Arr) {
        string Tmp;
        for (int Pos = Arr.Length; Pos > 1; --Pos) {
            for (int Ind = 0; Ind < Pos - 1; ++Ind) {
                if(String.Compare(Arr[Ind], Arr[Ind + 1]) > 0) {
                    Tmp = Arr[Ind];
                    Arr[Ind] = Arr[Ind + 1];
                    Arr[Ind + 1] = Tmp;
                }
            } 
        }
    }

    // Create zone calendar
    private string[] ZoneCalendar(int LastZone, StatusCode LastState, string[] MySchedule, int[] ZoneSequence, int Perc) {
        const int NO_OF_DAYS = 8;
        int NoOfScheds = MySchedule.Length / 7;   // Single Scheduler: 1, Double Scheduler: 2
        // if mower is on the move LastZone will be incremented by 1 after returning to home
        LastZone = (LastZone + (LastState == StatusCode.HOME ? 0 : 1)) % 10;
        double CorrFactor = (100 + Perc) / 100.0;
        string[] Calendar = new string[2 * NO_OF_DAYS];
        int EntryNo = 0; 
        if (CorrFactor > 0.0) {
            DateTime StartDate = DateTime.Now; // today: starting at current time
                for (int OffSet = 0; OffSet < NO_OF_DAYS; OffSet++) {
                DateTime CurDate = StartDate.AddDays((double)OffSet);
                string CurDateText = CurDate.ToString("yyyyMMddHHmm");
                int WeekDay = (int)CurDate.DayOfWeek;
                for (int Pos = 0; Pos < NoOfScheds; Pos++) {
                    // WeekDay | Start time | Duration | Border cut
                    string[] Schedule = MySchedule[NoOfScheds * WeekDay + Pos].Split(';'); 
                    Schedule[2] = Convert.ToInt32(Int32.Parse(Schedule[2]) * CorrFactor).ToString();
                    string CurSchedText = CurDateText.Substring(0,8) + Schedule[1].Substring(0,2) + Schedule[1].Substring(3,2);
                    DateTime CurSched = new DateTime(CurDate.Year, CurDate.Month, CurDate.Day,
                        Int32.Parse(Schedule[1].Substring(0,2)), Int32.Parse(Schedule[1].Substring(3,2)), 0);
                    if (String.Compare(CurSchedText, CurDateText) >= 0) {
                        if (Schedule[2] != "0") {
                            Calendar[EntryNo] = ZoneCalendarEntry(LastZone, ZoneSequence, CurDate, CurSched, Schedule);
                            EntryNo++;
                            LastZone = (LastZone + 1) % 10;
                        }
                    }
                }
                StartDate = StartDate.Date; // next days: starting at midnight
                }		
        }
        // reset unused entries
        for (int Pos = EntryNo; Pos < 2 * NO_OF_DAYS; Pos++) {
            Calendar[Pos] = "";
        }
        return Calendar;
    }

    // Convert weekday number to a weekday
    string WeekDay(int WeekDayNo) {
        return (new string[] {"Su","Mo","Tu","We","Th","Fr","Sa"})[WeekDayNo];
    }
 
    // Zone calendar entry
    private string ZoneCalendarEntry(int LastZone, int[] ZoneSequence, DateTime CurDate, DateTime CurSched, string[] Schedule) {
        const string Sep = ";";
        return CurDate.ToString("dd/MM/yyyy")
            + Sep + WeekDay(Convert.ToInt32(CurDate.DayOfWeek))
            + Sep + CurSched.ToString("HH:mm")
            + Sep + CurSched.AddMinutes(Double.Parse(Schedule[2])).ToString("HH:mm")
            + Sep + (ZoneSequence[LastZone] + 1).ToString()
            + Sep + (Schedule[3] == "0" ? "No " : "Yes");		
    }

    // Format zone calendar
    private string[] FormatZoneCalendar(string[] MyZoneCalendar) {
        string Template =  "|  @@DATE@@  @@WEEKDAY@@  |  @@FROM@@ - @@TO@@  |  Zone @@ZONE@@  |  Border cut: @@EDGE@@  |";
        string[] TmpCalendar = new string[MyZoneCalendar.Length];
        for (int Pos = 0; Pos < MyZoneCalendar.Length; Pos++) {
            if (MyZoneCalendar[Pos] != "") {
                string[] Entries = MyZoneCalendar[Pos].Split(';'); 
                TmpCalendar[Pos] = Template
                .Replace("@@DATE@@", Entries[0])
                .Replace("@@WEEKDAY@@", Entries[1])
                .Replace("@@FROM@@", Entries[2])
                .Replace("@@TO@@", Entries[3])
                .Replace("@@ZONE@@", Entries[4])
                .Replace("@@EDGE@@", Entries[5]);
            } else {
                TmpCalendar[Pos] = "";
            }
        }
        return TmpCalendar;
    }	
		
    // Write zone calendar into grid
    private void GridZoneCalendar(string[] MyZCalendar) {
        ParaItems[0].Text = MyZCalendar[0] == "" ? GRD_NOSCHEDULEDDEPARTURES : MyZCalendar[0];
        for (int Pos = 0; Pos < 16; Pos++) {
                ParaItems[Pos].Text = MyZCalendar[Pos] == "" ? "" : MyZCalendar[Pos].Replace("|", "").Substring(2);
        }
        if (MyZCalendar[0] == "") {
            ParaItems[0].Text = GRD_NOSCHEDULEDDEPARTURES;
        }
    }

    // Write zone calendar into csv file
    async void ExportZoneCalendar(string[] MyZCalendar, string MowerName) {
        const string FRAME = "+------------------+-----------------+----------+-------------------+";
        string ExportFileName = DataFile("Export_", "_" + MowerName, ".txt");      
    
        using (StreamWriter StrWr = new StreamWriter(ExportFileName, false)) {
            StrWr.WriteLine(EXP_CREATIONDATE + ParaTimeStamp.Text);
            StrWr.WriteLine(EXP_MOWERNAME + MowerName);
            StrWr.WriteLine(EXP_CORRECTION + ParaCorrection.Text);
            StrWr.WriteLine("");
            StrWr.WriteLine(FRAME);
            for (int Pos = 0; Pos < MyZCalendar.Length; Pos++) {
                if (MyZCalendar[Pos] != "") {
                    StrWr.WriteLine(MyZCalendar[Pos]);
                }
            }
            StrWr.WriteLine(FRAME);
            StrWr.Close();
        }
        MyTrace(TRACE_EXPORT.Replace("@@FILE@@", ExportFileName));
        InfoBox(HDR_EXPORT, MSG_EXPORT, "@@FILE@@", ExportFileName);
    }

#region Texts 
    // Texts
    const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";
    const string DESC_POINTER = "Current zone pointer";
    const string DESC_CALENDAR = "Next scheduled departures";
    const string DESC_EXPORT = "Export to a text file";
    const string DESC_STATUS = "Status";
    const string DESC_CORRECTION = "Work time correction";
    const string DESC_TIMESTAMP = "Time stamp";
    const string DESC_ZONES = "Sequence of zone numbers";
    const string GRD_CURRENTZONEPOINTER = "(_)  (_)  (_)  (_)  (_)  (_)  (_)  (_)  (_)  (_)";
    const string GRD_NOSCHEDULEDDEPARTURES = "(no scheduled departures)";
    const string GRD_POINTER = "/|\\";
    const string MSG_EXPORT = "Mowing calendar exported to\r\n@@FILE@@";
    const string HDR_EXPORT = "Mowing calendar export";
    const string SEP_TABLE = ")  (";
    const string TRACE_EXPORT = "Exported to @@FILE@@";
    const string VAL_UNKNOWN = "(unknown)";
    const string EXP_MOWERNAME = "Mower Name    :  ";
    const string EXP_CREATIONDATE = "Creation Date :  ";
    const string EXP_CORRECTION = "Correction    :  ";
#endregion Texts
}
