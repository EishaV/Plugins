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

public class PluginZoneKeeperSwitch : IPlugin {

    // Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
    const string PLUGIN_DESC = "Switch on/off the Zone Keeper feature";
    const string PLUGIN_NAME = "ZoneKeeperSwitch";

    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras => MyParas;	
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    readonly List<string> ListSetting = new() { "off", "on" };

    // Memory Definition
    [DataContract]
    public class MyMemory { 
        [DataMember] public int NewSetting;

        public MyMemory() {
            NewSetting = 0;
        }
    }  

    private MyMemory mymem;

    public PluginParaText ParaCurrentSetting;
    public PluginParaCase ParaNewSetting;

    // Event: On Open
    public PluginZoneKeeperSwitch() {
        mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem==null) {mymem = new MyMemory();};
        ParaCurrentSetting = new PluginParaText(TEXT_CURRENT_ZONEKEEPER_SETTING, VAL_UNKNOWN, DESC_CURRENT_ZONEKEEPER_SETTING, true);
        ParaNewSetting = new PluginParaCase(TEXT_NEW_ZONEKEEPER_SETTING, mymem.NewSetting, ListSetting, DESC_NEW_ZONEKEEPER_SETTING);
        MyParas = new List<PluginParaBase>() { ParaCurrentSetting, ParaNewSetting };
        }

    // Event: New MQTT data received
    void IPlugin.Todo(PluginData pd) {
        int Mzk = pd.Config.MultiZoneKeeper == null ? 0 : pd.Config.MultiZoneKeeper.Value;
        ParaCurrentSetting.Text = ListSetting[Mzk]; 
    }

    // Event: Doit Button pressed
    async void IPlugin.Doit(PluginData pd) {
        string Json = "{\"mzk\":@@MZK@@}"
            .Replace("@@MZK@@", ParaNewSetting.Index.ToString());
        DeskApp.Send(Json, pd.Index);			
        MyTrace(TRACE_SENT.Replace("@@JSON@@", Json).Replace("@@MOWER@@", pd.Name));
        InfoBox(HDR_SENT, ParaNewSetting.Index == 0 ? MSG_SENT_OFF : MSG_SENT_ON, "@@MOWER@@", pd.Name);
    }

    // Event: On Close
    void IDisposable.Dispose() { 
        mymem.NewSetting = ParaNewSetting.Index; 
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
    const string DESC_CURRENT_ZONEKEEPER_SETTING = "The Zone Keeper feature can be switched on or off";
    const string DESC_NEW_ZONEKEEPER_SETTING = DESC_CURRENT_ZONEKEEPER_SETTING;
    const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";

    const string MSG_SENT_ON = "The Zone Keeper feature has been switched on for @@MOWER@@.";
    const string MSG_SENT_OFF = "The Zone Keeper feature has been switched off for @@MOWER@@.";	
    const string HDR_SENT = "Confirmation";
    const string TEXT_CURRENT_ZONEKEEPER_SETTING = "Current Zone Keeper Setting";
    const string TEXT_NEW_ZONEKEEPER_SETTING = "New Zone Keeper Setting";

    const string TRACE_SENT = "Sent: @@JSON@@ to @@MOWER@@";

    const string VAL_UNKNOWN = "(unknown)";
#endregion Texts    
}
