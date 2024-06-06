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

public class PluginBatteryLevelWarning : IPlugin {

	// Version identification
	/////////////////////////////////////////////////////		
	const string PLUGIN_VERSION = "3.1";
	const string PLUGIN_DATE = "30.10.2023";
	const string PLUGIN_DESCRIPTION ="Battery Level Warning Message";
	const string PLUGIN_NAME = "BatteryLevelWarning";
	/////////////////////////////////////////////////////		

    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras =>  MyParas;
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    private bool AlreadyReported = false;    

    // Memory Definition
    [DataContract]
    public class MyMemory { 
        [DataMember] public bool Active;
        [DataMember] public int Treshold;      

        public MyMemory() {
            Active = false;
            Treshold = 0;
        }
    }
    
    MyMemory mymem = new MyMemory();

    // Form Definition
    public PluginParaBool ParaActive;
    public PluginParaReal ParaTreshold;
    public PluginParaText ParaLevel;

    // Event: On Open
    public PluginBatteryLevelWarning() {
        mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem == null) mymem = new MyMemory();
        ParaActive = new PluginParaBool(DESC_ACTIVE, mymem.Active, "");
        ParaTreshold = new PluginParaReal(DESC_TRESHOLD, mymem.Treshold, "", 0, 100);
        ParaLevel = new PluginParaText(DESC_LEVEL, VAL_UNKNOWN, "", true);
        MyParas = new List<PluginParaBase>() { ParaActive, ParaTreshold, ParaLevel };
    }

    // Event: New MQTT data received
    async void IPlugin.Todo(PluginData pd) {
        ParaLevel.Text = pd.Data.Battery.Perc.ToString();
        if (mymem.Active) {
            if (Math.Round(pd.Data.Battery.Perc) <= mymem.Treshold) {
                if (!AlreadyReported) {
                    AlreadyReported = true;
                    InfoBox(DateTime.Now.ToString("dd.MM.yyyy HH:mm"), 
                        MSG_TEXT, 
                        "@@TRESHOLD@@;@@LEVEL@@", 
                        $"{mymem.Treshold.ToString()};{pd.Data.Battery.Perc.ToString()}");
                } 
            } else {
                AlreadyReported = false;
            }
        }
    }

    // Event: Doit Button pressed
    void IPlugin.Doit(PluginData pd) {
        mymem.Active = ParaActive.Check;
        mymem.Treshold = Convert.ToInt32(ParaTreshold.Real);
        AlreadyReported = false;
        DeskApp.Send("{}", pd.Index);
        MyTrace(pd.Name + " polled.");
    }

    // Event: On Close
    void IDisposable.Dispose() { 
        mymem.Active = ParaActive.Check;
        mymem.Treshold = Convert.ToInt32(ParaTreshold.Real);
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
    const string DESC_PLUGIN = PLUGIN_DESCRIPTION + " (v" + PLUGIN_VERSION + " | " + PLUGIN_DATE + ")";
    const string DESC_ACTIVE = "Warning messaging active";
    const string DESC_TRESHOLD = "Battery level treshold value (%)";
    const string DESC_LEVEL = "Current battery level (%)";
    const string MSG_TEXT = "The battery level has fallen below the treshold of @@TRESHOLD@@%."
        + "\r\n\r\nCurrent battery level: @@LEVEL@@%";
    const string VAL_UNKNOWN = "(unknown)";
#endregion Texts
}

