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

public class PluginOffLimitsModuleSwitch : IPlugin {

    // Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
    const string PLUGIN_DESC ="Switch on/off the Off Limits Module";
    const string PLUGIN_NAME = "OffLimitsModuleSwitch";
	
	
    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras => MyParas;	
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    readonly List<string> ListSwitches = new() { "off", "on" };	
  

    // Memory Definition
    [DataContract]
    public class MyMemory { 
        [DataMember] public int NewBorder;
        [DataMember] public int NewShortCuts;

        public MyMemory() {
            NewBorder = 0;
            NewShortCuts = 0;
        }
    }  
  
    private MyMemory mymem;

    public PluginParaText ParaOldBorder;
    public PluginParaText ParaOldShortCuts;
    public PluginParaCase ParaNewBorder;
    public PluginParaCase ParaNewShortCuts;

    // Event: On Open
    public PluginOffLimitsModuleSwitch() {
        mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem == null){mymem = new MyMemory();};
        ParaOldBorder = new PluginParaText(TEXT_OLDBORDER, VAL_UNKNOWN, DESC_OLDBORDER, true);
        ParaOldShortCuts = new PluginParaText(TEXT_OLDSHORTCUTS, VAL_UNKNOWN, DESC_OLDSHORTCUTS, true);
        ParaNewBorder = new PluginParaCase(TEXT_NEWBORDER, mymem.NewBorder, ListSwitches, DESC_NEWBORDER);
        ParaNewShortCuts = new PluginParaCase(TEXT_NEWSHORTCUTS, mymem.NewShortCuts, ListSwitches, DESC_NEWSHORTCUTS);
        MyParas = new List<PluginParaBase>() { ParaOldBorder, ParaOldShortCuts, ParaNewBorder, ParaNewShortCuts };
    }

    // Event: New MQTT data received
    void IPlugin.Todo(PluginData pd) {
        int Cut = pd.Config.ModulesC.DF.Cutting == null ? 0 : pd.Config.ModulesC.DF.Cutting;
        int Fh = pd.Config.ModulesC.DF.FastHome == null ? 0 : pd.Config.ModulesC.DF.FastHome;
        ParaOldBorder.Text = ListSwitches[Cut]; 
        ParaOldShortCuts.Text = ListSwitches[Fh]; 
    }

    // Event: Doit Button pressed
    async void IPlugin.Doit(PluginData pd) {
        string Json = "{\"modules\":{\"DF\":{\"cut\":@@CUT@@,\"fh\":@@FH@@}}}"
            .Replace("@@CUT@@", ParaNewBorder.Index.ToString())
            .Replace("@@FH@@", ParaNewShortCuts.Index.ToString());
        DeskApp.Send(Json, pd.Index);			
        MyTrace(TRACE_SENT.Replace("@@JSON@@", Json).Replace("@@MOWER@@", pd.Name));
        InfoBox(HDR_SENT, MSG_SENT, "@@JSON@@;@@MOWER@@", Json + ";" + pd.Name);
    }
  
    // Event: On Close
    void IDisposable.Dispose() { 
        mymem.NewBorder = ParaNewBorder.Index; 
        mymem.NewShortCuts = ParaNewShortCuts.Index; 
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
	/////////////////////////////////////////////////////
	const string DESC_OLDBORDER = "Current Setting: Should the magnetic stripes be recognized as border when mowing?";
	const string DESC_NEWBORDER = "New Setting: Should the magnetic stripes be recognized as border when mowing?";
	const string DESC_OLDSHORTCUTS = "Current Setting: Should the magnetic stripes be recognized as shortcuts for a fast way home?";
	const string DESC_NEWSHORTCUTS = "New Setting: Should the magnetic stripes be recognized as shortcuts for a fast way home?";
	const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";
	
	const string MSG_SENT = "The new settings have been sent to @@MOWER@@.\r\n\r\n@@JSON@@";
    const string HDR_SENT = "Information";
	const string TEXT_OLDBORDER = "Current: Recognize OLM as border";
	const string TEXT_NEWBORDER = "New: Recognize OLM as border";
	const string TEXT_OLDSHORTCUTS = "Current: Recognize OLM as shortcuts";
	const string TEXT_NEWSHORTCUTS = "New: Recognize OLM as shortcuts";
	
	const string TRACE_SENT = "Sent: @@JSON@@ to @@MOWER@@";
    
    const string VAL_UNKNOWN = "(unknown)";
#endregion Texts
}

