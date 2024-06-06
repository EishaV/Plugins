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

public class PluginAutoLockSwitch : IPlugin {

    // Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
    const string PLUGIN_DESC = "Switch on/off the AutoLock feature";
    const string PLUGIN_NAME = "AutoLockSwitch";

    // Global variables
    private List<PluginParaBase> MyParas;
    const string EMPTY = "./.";
    List<PluginParaBase> IPlugin.Paras => MyParas;	
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    readonly List<string> ListSwitches = new() { "off", "on" };
    readonly List<string> ListTimes = new() { EMPTY };

    // Memory Definition
    [DataContract]
    public class MyMemory { 
        [DataMember] public int NewSetting;
        [DataMember] public int NewSeconds;

        public MyMemory() {
            NewSetting = 0;
            NewSeconds = 0;
        }
    }  

    private MyMemory mymem;

    public PluginParaText ParaOldSetting;
    public PluginParaText ParaOldSeconds;
    public PluginParaCase ParaNewSetting;
    public PluginParaCase ParaNewSeconds;


    // Event: On Open
    public PluginAutoLockSwitch() {
        for (int i = 1; i <= 20; i++) {
            ListTimes.Add((30 * i).ToString());
        }	  
        mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem == null) {mymem = new MyMemory();};
        ParaOldSetting = new PluginParaText(TEXT_OLDSETTING, VAL_UNKNOWN, DESC_OLDSETTING, true);
        ParaOldSeconds = new PluginParaText(TEXT_OLDSECONDS, VAL_UNKNOWN, DESC_OLDSECONDS, true);
        ParaNewSetting = new PluginParaCase(TEXT_NEWSETTING, mymem.NewSetting, ListSwitches, DESC_NEWSETTING);
        ParaNewSeconds = new PluginParaCase(TEXT_NEWSECONDS, mymem.NewSeconds, ListTimes, DESC_NEWSECONDS);
        MyParas = new List<PluginParaBase>() { ParaOldSetting, ParaOldSeconds, ParaNewSetting, ParaNewSeconds };
    }


    // Event: New MQTT data received
    void IPlugin.Todo(PluginData pd) {
        int Lvl = pd.Config.AutoLock.Level == null ? 0 : pd.Config.AutoLock.Level;
        ParaOldSetting.Text = ListSwitches[Lvl]; 
        string T = pd.Config.AutoLock.Time == 0 ? EMPTY : pd.Config.AutoLock.Time.ToString();
        ParaOldSeconds.Text = T;
    }


    // Event: Doit Button pressed
    async void IPlugin.Doit(PluginData pd) {
        int OnOff = ParaNewSetting.Index;
        int Seconds = ParaNewSeconds.Index * 30;
        if (OnOff == 0) {
            Seconds = 0;
        }
        string Json = "{\"al\":{\"lvl\":@@ONOFF@@,\"t\":@@SECONDS@@}}"
            .Replace("@@ONOFF@@", OnOff.ToString())
            .Replace("@@SECONDS@@", Seconds.ToString());
        DeskApp.Send(Json, pd.Index);			
        MyTrace(TRACE_SENT.Replace("@@JSON@@", Json).Replace("@@MOWER@@", pd.Name));
        string Msg = (OnOff == 0) ? MSG_SENT_OFF : MSG_SENT_ON;
        InfoBox(HDR_SENT, Msg, "@@SECONDS@@", Seconds.ToString());
    }


    // Event: On Close
    void IDisposable.Dispose() { 
        mymem.NewSetting = ParaNewSetting.Index; 
        mymem.NewSeconds = ParaNewSeconds.Index;
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

#region Texts
    // Texts
    const string DESC_OLDSETTING = "The AutoLock feature can be switched on " 
        + "with a waiting time between 30 and 600 seconds or it can be switched off.";
    const string DESC_NEWSETTING = DESC_OLDSETTING;
    const string DESC_OLDSECONDS = "Waiting time between 30 and 600 seconds if AutLock feature is activated";
    const string DESC_NEWSECONDS = DESC_OLDSECONDS;
    const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";

    const string MSG_SENT_ON = "The AutoLock feature has been switched on with a grace period of @@SECONDS@@ seconds.";
    const string MSG_SENT_OFF = "The AutoLock feature has been switched off.";
    const string HDR_SENT = "New setting";

    const string TEXT_OLDSETTING = "Current AutoLock Setting";
    const string TEXT_NEWSETTING = "New AutoLock Setting";
    const string TEXT_OLDSECONDS = "Current Waiting Time in Seconds";
    const string TEXT_NEWSECONDS = "New Waiting Time in Seconds (30 ... 600)";

    const string TRACE_SENT = "Sent: @@JSON@@";

    const string VAL_UNKNOWN = "(unknown)";
#endregion Texts    
}

