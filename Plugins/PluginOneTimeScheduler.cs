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


public class PluginOneTimeScheduler : IPlugin {
	
    // Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
    const string PLUGIN_DESC = "Starting the One Time Scheduler";
    const string PLUGIN_NAME = "OneTimeScheduler";


    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras => MyParas;	
    string IPlugin.Desc { get { return DESC_PLUGIN; } }

  
    // Memory Definition
    [DataContract]
    public class MyMemory { 
        [DataMember] public int WorkTime;
        [DataMember] public bool BorderCut;

        public MyMemory() {
            WorkTime = 0;
            BorderCut = false;
            }
    }  

    private MyMemory mymem;

    public PluginParaReal ParaWorkTime;
    public PluginParaBool ParaBorderCut;

    // Event: On Open
    public PluginOneTimeScheduler() {
        mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem == null) {mymem = new MyMemory();};
        ParaWorkTime = new PluginParaReal(TEXT_WORKTIME, mymem.WorkTime, DESC_WORKTIME, 0);
        ParaBorderCut = new PluginParaBool(TEXT_BORDERCUT, mymem.BorderCut, DESC_BORDERCUT);
        MyParas = new List<PluginParaBase>() { ParaWorkTime, ParaBorderCut };
    }

    // Event: New MQTT data received
    void IPlugin.Todo(PluginData pd) {
    }

    // Event: Doit Button pressed
    async void IPlugin.Doit(PluginData pd) {
        string Json = "{\"sc\":{\"ots\":{\"bc\":@@BC@@,\"wtm\":@@WTM@@}}}"
            .Replace("@@WTM@@", ParaWorkTime.Real.ToString())
            .Replace("@@BC@@",  ParaBorderCut.Check ? "1" : "0");
        DeskApp.Send(Json, pd.Index);			
        MyTrace(TRACE_SENT.Replace("@@JSON@@", Json).Replace("@@MOWER@@", pd.Name));
        InfoBox(HDR_SENT, MSG_SENT, "@@JSON@@;@@MOWER@@", Json + ";" + pd.Name);
    }
  
    // Event: On Close
    void IDisposable.Dispose() { 
        mymem.WorkTime = Convert.ToInt32(ParaWorkTime.Real);
        mymem.BorderCut = ParaBorderCut.Check; 
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
	const string DESC_WORKTIME= "Define the worktime in minutes";
	const string DESC_BORDERCUT = "Mowing with or without cutting the border?";
	const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";
	
	const string MSG_SENT = "The One Time Scheduler command has been sent to @@MOWER@@.\r\n\r\n@@JSON@@";
    const string HDR_SENT = "Confirmation";
	const string TEXT_WORKTIME= "Worktime in Minutes";
	const string TEXT_BORDERCUT = "Including Border Cut";
	
	const string TRACE_SENT = "Sent: @@JSON@@ to @@MOWER@@";
#endregion Texts
}
