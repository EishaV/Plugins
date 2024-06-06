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

public class PluginSetNextZone : IPlugin {

    // Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
    const string PLUGIN_DESC ="Set the next zone";
    const string PLUGIN_NAME = "SetNextZone";	

    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras => MyParas;	
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    readonly List<string> ListZones = new() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    private ushort MyId = unchecked((ushort)-1);

    // Memory Definition
    [DataContract]
    public class MyMemory { 
    [DataMember] public int ZoneIndex;

        public MyMemory() {
            ZoneIndex = 0;
        }
    }  

    public PluginParaText ParaRuler;
    public PluginParaText ParaZoneSequence;
    public PluginParaText ParaZonePointer;
    public PluginParaCase ParaZoneIndex;

    // Event: On Open
    public PluginSetNextZone() {
        MyMemory? mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem == null) mymem = new MyMemory();
        ParaRuler = new PluginParaText(TEXT_RULER, GRD_RULER, DESC_RULER, true);
        ParaZoneSequence = new PluginParaText(TEXT_ZONE_SEQUENCE, VAL_UNKNOWN, DESC_ZONE_SEQUENCE, true);
        ParaZonePointer = new PluginParaText(TEXT_CURRENT_ZONE_POINTER, VAL_UNKNOWN, DESC_CURRENT_ZONE_POINTER, true);
        ParaZoneIndex = new PluginParaCase(TEXT_NEW_ZONE_INDEX, mymem.ZoneIndex, ListZones, DESC_NEW_ZONE_INDEX);
        MyParas = new List<PluginParaBase>() { ParaRuler, ParaZoneSequence, ParaZonePointer, ParaZoneIndex };
    }

    // Event: new MQTT data arrived
    async void IPlugin.Todo(PluginData pd) {
        if (pd.Config.Id == MyId) {
            InfoBox(MSG_HEADER, MSG_FEEDBACK, "@@ZONE@@", pd.Config.MultiZonePercs[pd.Data.LastZone].ToString());
        }
        ParaZoneSequence.Text = BeautifulTable(pd.Config.MultiZonePercs, SEP_TABLE);
        ParaZonePointer.Text = MarkPointer(pd.Data.LastZone);
    }

    // Event: Doit button pressed
    void IPlugin.Doit(PluginData pd) {
        int NoOfLeftShifts = ParaZoneIndex.Index - pd.Data.LastZone;
        NoOfLeftShifts = NoOfLeftShifts < 0 ? NoOfLeftShifts + 10 : NoOfLeftShifts;
        int[] NewZones = LeftRingShift(NoOfLeftShifts, pd.Config.MultiZonePercs);
        MyId = RandomId();        
        string Json = "{\"id\":@@ID@@, \"mzv\":[@@MZV@@]}"
            .Replace("@@ID@@", MyId.ToString())        
            .Replace("@@MZV@@", String.Join(",", NewZones));
        DeskApp.Send(Json, pd.Index);
        MyTrace(MSG_ACK_SENT + " " + Json);
    }

    // Event: On Close
    void IDisposable.Dispose() { 
        MyMemory mymem = new MyMemory();
        mymem.ZoneIndex = ParaZoneIndex.Index;
        DeskApp.PutJson<MyMemory>(MemoryFile(), mymem);
    }

    // Writing a trace text 
    private void MyTrace(string Text) {
        DeskApp.Trace(PLUGIN_NAME + ": " + Text);
    }  
    
    // Data file name 
    string DataFile(string Header, string Trailer, string Extension) {
        return Path.Combine(DeskApp.DirData, Header + PLUGIN_NAME + Trailer + Extension);  
    }

    // File name for Memory File
    string MemoryFile() {
        return DataFile("Memory_", "", ".json");  
    }
    
    
    // Random ID
    ushort RandomId() {
        Random r = new();
        return (ushort)r.Next(2, ushort.MaxValue);    
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
  
    // Beautify the table
    private string BeautifulTable(int[] Table, string Sep) {
        string[] StrTable = new string[Table.Length];
        for (int Pos = 0; Pos < Table.Length; Pos++) {
            StrTable[Pos] = Table[Pos].ToString();
        }
        return Sep.Substring(Sep.Length - 1) + String.Join(Sep, StrTable) + Sep.Substring(0, 1);
    }
		
    // Mark the pointer position
    private string MarkPointer(int Pointer) {
        int StartPos = (GRD_POINTER.Length + 2) * Pointer;
        return GRD_CURRENT_ZONE_POINTER.Remove(StartPos, GRD_POINTER.Length).Insert(StartPos, GRD_POINTER);
    }

    // Left Ring Shifts
    private int[] LeftRingShift(int NoOfShifts, int[] Table) {
        for (int Counter = 1; Counter <= NoOfShifts; Counter++) {
            int Tmp = Table[0];
            for (int Pos = 0; Pos < Table.Length - 1; Pos++) {
                Table[Pos] = Table[Pos + 1];
            }
            Table[Table.Length - 1] = Tmp;
        }
        return Table;
    }
	
#region Texts    
	// Texts
	const string DESC_CURRENT_ZONE_POINTER = "Pointer to the next zone";
	const string DESC_NEW_ZONE_INDEX = "Desired index for the next zone";
	const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";
	const string DESC_RULER = "Ruler for the zone sequence";
	const string DESC_ZONE_SEQUENCE = "Sequence of zone indices";
	const string TEXT_CURRENT_ZONE_POINTER = "Current Zone Pointer";
	const string TEXT_NEW_ZONE_INDEX = "New Zone Index";
	const string TEXT_RULER = "Ruler";
	const string TEXT_ZONE_SEQUENCE = "Zone Sequence";	
	const string GRD_CURRENT_ZONE_POINTER = "(  )  (  )  (  )  (  )  (  )  (  )  (  )  (  )  (  )  (  )";
	const string GRD_POINTER = "/||\\";
	const string GRD_RULER = "[0]  [1]  [2]  [3]  [4]  [5]  [6]  [7]  [8]  [9]";
	const string MSG_ACK_SENT = "Next zone command sent:";
    const string MSG_FEEDBACK = "The Zone Pointer is now pointing to Zone No. @@ZONE@@.";
    const string MSG_HEADER = "Feedback received:";
	const string SEP_TABLE = ")  (";
    const string VAL_UNKNOWN = "(unknown)";
#endregion Texts
}
