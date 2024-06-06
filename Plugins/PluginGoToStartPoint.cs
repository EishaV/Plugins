using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
// DeskApp Plugin interface
using Positec;
using Plugin;
// MessageBox
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

public class PluginGoToStartPoint : IPlugin {
	
    // Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
    const string PLUGIN_DESC = "Start mowing from a given startpoint";
    const string PLUGIN_NAME = "GoToStartPoint";

    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras =>  MyParas;
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    readonly List<string> ListDefinitionMethods = new() { "Definition via Zone Selection", "Definition via Meter Specification" };
    private const int INDEXZONESELECTION = 0;
    private const int INDEXMETERSPECIFICATION = 1;
    readonly List<string> ListZones = new() { CON_UNKNOWN, CON_UNKNOWN, CON_UNKNOWN, CON_UNKNOWN };
    private const int MINMINUTES = 10;
    private bool ZoneRecoveryOutstanding = false;  // flag: zone parameters have to be recovered
	private string ZoneRecoveryJSON = "";   // zone parameters backup

    // Memory Definition
    [DataContract]
    public class MyMemory { 
        [DataMember] public int DefinitionMethod;
        [DataMember] public double MeterSpecification;
        [DataMember] public int ZoneSelection;
        [DataMember] public double WorkTimeMinutes;

        public MyMemory() {
            DefinitionMethod = INDEXZONESELECTION;
            MeterSpecification = 1;
            ZoneSelection = 0;
            WorkTimeMinutes = MINMINUTES;
        }
    }

    // Form Definition
    public PluginParaText ParaMowerState;
    public PluginParaText ParaStartPoints;
    public PluginParaCase ParaDefinitionMethod;
    public PluginParaReal ParaMeterSpecification;
    public PluginParaCase ParaZoneSelection;
    public PluginParaReal ParaWorkTimeMinutes;
    
    // Event: On Open
    public PluginGoToStartPoint() {
        MyMemory? mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem == null) mymem = new MyMemory();
        ParaMowerState = new PluginParaText(TEXT_CURRENT_MOWER_STATE, CON_UNKNOWN, DESC_CURRENT_MOWER_STATE, true);
        ParaStartPoints = new PluginParaText(TEXT_CURRENT_ZONE_START_POINTS, CON_UNKNOWN, DESC_CURRENT_ZONE_START_POINTS, true);
        ParaDefinitionMethod = new PluginParaCase(TEXT_START_POINT_DEFINITION_METHOD, mymem.DefinitionMethod, ListDefinitionMethods, DESC_START_POINT_DEFINITION_METHOD);
        ParaMeterSpecification = new PluginParaReal(TEXT_METER_SPECIFICATION, mymem.MeterSpecification, DESC_METER_SPECIFICATION, 1);
        ParaZoneSelection = new PluginParaCase(TEXT_ZONE_SELECTION, mymem.ZoneSelection, ListZones, DESC_ZONE_SELECTION);
        ParaWorkTimeMinutes = new PluginParaReal(TEXT_WORK_TIME_MINUTES, mymem.WorkTimeMinutes,
            DESC_WORK_TIME_MINUTES.Replace("@@MIN@@", MINMINUTES.ToString()), MINMINUTES);
        MyParas = new List<PluginParaBase>() { ParaMowerState, ParaStartPoints, ParaDefinitionMethod, ParaMeterSpecification, ParaZoneSelection, ParaWorkTimeMinutes };
    }    

    // Event: New MQTT data arrived
    void IPlugin.Todo(PluginData pd) {
		ParaMowerState.Text = pd.Data.LastState.ToString();
		if (ZoneRecoveryOutstanding && pd.Data.LastState == StatusCode.GRASS_CUTTING) {
			ZoneRecoveryOutstanding = false;
			DeskApp.Send(ZoneRecoveryJSON, pd.Index);
			MyTrace(TRACE_SENT.Replace("@@JSON@@", ZoneRecoveryJSON));			
		}
		if (!ZoneRecoveryOutstanding) {
			ParaStartPoints.Text = string.Join(", ", pd.Config.MultiZones);
		}
        if (ListZones[0] == CON_UNKNOWN) {
            for (var Pos = 0; Pos < ListZones.Count; Pos++) {
                ListZones[Pos] = (pd.Config.MultiZones[Pos] == 0)
                    ? LST_NO_ZONE
                    : LST_ZONE
                        .Replace("@@NO@@", Pos.ToString())
                        .Replace("@@M@@", pd.Config.MultiZones[Pos].ToString());
            }
        }
    }

    // Event: Doit button pressed
    async void IPlugin.Doit(PluginData pd) {
		if (OkayVersion(pd, "3.15") && OkayHome(pd) && OkayStartPoint(ParaDefinitionMethod.Index, ParaZoneSelection.Index, pd)) {
			int Meters = (ParaDefinitionMethod.Index == INDEXZONESELECTION)
                ? pd.Config.MultiZones[(int)(ParaZoneSelection.Index)] 
                : Convert.ToInt32(ParaMeterSpecification.Real);
			ZoneRecoveryJSON = SetZoneRecoveryJSON(pd);
			MyTrace(TRACE_ZONES_BACKUP.Replace("@@JSON@@", ZoneRecoveryJSON));
			ZoneRecoveryOutstanding = true;
			string Template = "{\"mz\":[@@MZ0@@,0,0,0],\"mzv\":[0,0,0,0,0,0,0,0,0,0], \"sc\":{\"ots\":{\"bc\":0,\"wtm\":@@WTM@@}}}";
			Template = Template
				.Replace("@@MZ0@@", Meters.ToString())
				.Replace("@@WTM@@", Convert.ToInt32(ParaWorkTimeMinutes.Real).ToString());
			DeskApp.Send(Template, pd.Index);
			MyTrace(TRACE_SENT.Replace("@@JSON@@", Template));
            InfoBox(MSG_WARNING_TITLE, MSG_NO_EARLY_EXIT);
		} 
    }

    // Event: On Close
    void IDisposable.Dispose() { 
        MyMemory mymem = new MyMemory();
        mymem.DefinitionMethod = ParaDefinitionMethod.Index;
        mymem.MeterSpecification = ParaMeterSpecification.Real;
        mymem.ZoneSelection = ParaZoneSelection.Index;
        mymem.WorkTimeMinutes = ParaWorkTimeMinutes.Real;
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
    
	// Backing up the zone parameters
	private string SetZoneRecoveryJSON(PluginData pd) {
		string mz = string.Join(",", pd.Config.MultiZones);
		string mzv = string.Join(",", pd.Config.MultiZonePercs);
		string template = "{\"mz\":[@@MZ@@],\"mzv\":[@@MZV@@]}";
		return template.Replace("@@MZ@@", mz).Replace("@@MZV@@", mzv);
	}

	// Checking zone selection
	private bool OkayStartPoint(int DefMethodIndex, int ZoneIndex, PluginData pd) {
		bool Okay = false;
		switch (DefMethodIndex) {
            case INDEXZONESELECTION:
                Okay = (pd.Config.MultiZones[ZoneIndex] > 0);
                if (!Okay) {
                    InfoBox(MSG_WARNING_TITLE, MSG_NO_REAL_ZONE_INDEX, "@@ZONE@@", ZoneIndex.ToString());
                }
                break;
            case INDEXMETERSPECIFICATION:
                Okay = true;
                break;
		}
		return Okay;
	}
	
	// Checking if mower is in the HOME state
	private bool OkayHome(PluginData pd) {
		if (pd.Data.LastState == StatusCode.HOME) {
			return true;
		} else {
            InfoBox(MSG_WARNING_TITLE, MSG_HOME_ONLY);
			return false; 	
		}
	}
		
	// Checking the firmware version
	private bool OkayVersion(PluginData pd, string MinimalVersion) {
		string[] MinVer = MinimalVersion.Split('.');
		string[] CurVer = pd.Data.Firmware.ToString().Split('.', ',');
        while (CurVer[1].Length < 2) { CurVer[1] = CurVer[1] + "0"; }
        MyTrace(CurVer[1]);
		bool Okay = int.Parse(CurVer[0]) > int.Parse(MinVer[0]) || (int.Parse(CurVer[0]) == int.Parse(MinVer[0]) && int.Parse(CurVer[1]) >= int.Parse(MinVer[1]));
		if (!Okay) {
            InfoBox(MSG_WARNING_TITLE, MSG_MINIMAL_VERSION, "@@MINVER@@", MinimalVersion);
		}
		return Okay;
	}	

#region Texts
	// Texts
	const string DESC_CURRENT_MOWER_STATE = "Current Mower State\r\nMower can only be started from HOME.";
	const string DESC_CURRENT_ZONE_START_POINTS = "Defined zone startpoints";
	const string DESC_METER_SPECIFICATION = "Enter a number of meters greater than 0";
	const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";
	const string DESC_START_POINT_DEFINITION_METHOD = "Either select a zone number\r\nor enter any arbitrary meter specification";
	const string DESC_WORK_TIME_MINUTES = "Work time in minutes\r\n(minimum @@MIN@@)";
	const string DESC_ZONE_SELECTION = "Select one of the zones.";
	
	const string MSG_HOME_ONLY = "The mower can only be started from HOME.";
	const string MSG_MINIMAL_VERSION = "This plugin needs at least firmware version @@MINVER@@.";
	const string MSG_NO_EARLY_EXIT = "The mower is just starting ..." 
		+ "\r\n"
		+ "\r\nPlease do not exit the DeskApp until the mower"
        + " has started mowing at the startpoint. Otherwise"
		+ " the previous zone settings will be lost.";
	const string MSG_NO_REAL_ZONE_INDEX = "Zone @@ZONE@@ is not a real zone, as its startpoint is 0.";
	const string MSG_WARNING_TITLE = "Warning";
	
	const string TEXT_CURRENT_MOWER_STATE = "Current Mower State";
	const string TEXT_CURRENT_ZONE_START_POINTS = "Current Zone StartPoints";
	const string TEXT_METER_SPECIFICATION = "StartPoint Definition via Meter Specification";
	const string TEXT_START_POINT_DEFINITION_METHOD = "StartPoint Definition Method";
	const string TEXT_WORK_TIME_MINUTES = "Work Time in Minutes";
	const string TEXT_ZONE_SELECTION = "StartPoint Definition via Zone Selection";
	
	const string TRACE_SENT = "Sent: @@JSON@@";
	const string TRACE_ZONES_BACKUP = "Zones backed up: @@JSON@@";
    
    const string LST_ZONE = "Zone @@NO@@ = @@M@@ meter(s)";
    const string LST_NO_ZONE = "(no real zone)";
    
    const string CON_UNKNOWN = "(unknown)";
#endregion Texts
}