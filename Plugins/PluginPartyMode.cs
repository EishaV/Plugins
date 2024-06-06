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

public class PluginPartyMode : IPlugin {

    // Version identification
    const string VERSION_NUMBER = "3.1";
    const string VERSION_DATE = "30.10.2023";
    const string PLUGIN_DESC = "Switching party mode on/off";
    const string PLUGIN_NAME = "PartyMode";

    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras => MyParas;	
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    readonly List<string> ListOnOff = new() { "limited on", "off", "unlimited on" };

    // Memory Definition
    [DataContract]
    public class MyMemory { 
        [DataMember] public bool Explanation;
        [DataMember] public int NewSetting;
        [DataMember] public int NewMinutes;

        public MyMemory() {
            Explanation = false;
            NewSetting = 1;
            NewMinutes = 0;
        }
    }  

    private MyMemory mymem;

    public PluginParaBool ParaExplanation;
    public PluginParaText ParaCurrentSetting;
    public PluginParaText ParaCurrentMinutes;
    public PluginParaCase ParaNewSetting;
    public PluginParaReal ParaNewMinutes;

    // Event: On Open
    public PluginPartyMode() {
        mymem = DeskApp.GetJson<MyMemory>(MemoryFile());
        if (mymem == null) {mymem = new MyMemory();};
        ParaExplanation = new PluginParaBool(DISP_HELP, mymem.Explanation, DESC_HELP);
        ParaCurrentSetting = new PluginParaText(DISP_CURRENT_PARTY_MODE_SETTING, VAL_UNKNOWN, DESC_PARTY_MODE_SETTING, true);
        ParaCurrentMinutes = new PluginParaText(DISP_CURRENT_NUMBER_OF_MINUTES, VAL_UNKNOWN, DESC_NUMBER_OF_MINUTES, true);
        ParaNewSetting = new PluginParaCase(DISP_NEW_PARTY_MODE_SETTING, mymem.NewSetting, ListOnOff, DESC_PARTY_MODE_SETTING);
        ParaNewMinutes = new PluginParaReal(DISP_NEW_NUMBER_OF_MINUTES, mymem.NewMinutes, DESC_NUMBER_OF_MINUTES, 0);
        MyParas = new List<PluginParaBase>() { ParaExplanation, ParaCurrentSetting, ParaCurrentMinutes, ParaNewSetting, ParaNewMinutes };
    }

    // Event: New MQTT data received
    void IPlugin.Todo(PluginData pd) {
        ParaCurrentSetting.Text = ListOnOff[pd.Config.Schedule.Mode]; 
        ParaCurrentMinutes.Text = pd.Config.Schedule.Party.ToString();
    }

    // Event: Doit Button pressed
    async void IPlugin.Doit(PluginData pd) {
        bool Okay = false;
        if (ParaExplanation.Check) {
            InfoBox(HLP_TITLE, HLP_HDR + "\r\n\r\n" + HLP_BODY);
            ParaExplanation.Check = false;
            Okay = true;
        } else {
            if ( ParaNewSetting.Index > 0) {
                ParaNewMinutes.Real = 0;
            }
            string Json = "{\"sc\":{\"m\":@@M@@,\"distm\":@@DISTM@@}}"
                .Replace("@@M@@", ParaNewSetting.Index.ToString())
                .Replace("@@DISTM@@", Convert.ToInt32(ParaNewMinutes.Real).ToString());
            DeskApp.Send(Json, pd.Index);			
            MyTrace(TRACE_SENT.Replace("@@JSON@@", Json));
            InfoBox(HDR_SENT, MSG_SENT, "@@JSON@@", Json);
        }  
    }

    // Event: On Close
    void IDisposable.Dispose() { 
    mymem.Explanation = ParaExplanation.Check;
    mymem.NewSetting = ParaNewSetting.Index;
    mymem.NewMinutes = Convert.ToInt32(ParaNewMinutes.Real);
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
    const string DESC_PARTY_MODE_SETTING = "PartyMode setting on/off";
    const string DESC_HELP = "Activate \"Explanation required\" and press the DoIt button to get an explaining information";
    const string DESC_NUMBER_OF_MINUTES = "The duration of the lock time or zero";
    const string DESC_PLUGIN = PLUGIN_DESC + " (v" + VERSION_NUMBER + " | " + VERSION_DATE + ")";
    const string DISP_CURRENT_NUMBER_OF_MINUTES = "Current Number of Minutes";
    const string DISP_CURRENT_PARTY_MODE_SETTING = "Current PartyMode Setting";
    const string DISP_HELP = "Explanation required";
    const string DISP_NEW_NUMBER_OF_MINUTES = "New Number of Minutes";
    const string DISP_NEW_PARTY_MODE_SETTING = "New PartyMode Setting";
    const string HLP_BODY = "PartyMode limited on:"
        + "\r\n* Minutes = 0: Lock is ended (!) immediately."
        + "\r\n* Minutes > 0: Lock is started immediately for the given number of minutes."
        + "\r\n"
        + "\r\nPartyMode unlimited on:" 
        + "\r\n* An unlimited lock is started immediately, independent of the number of minutes."
        + "\r\n"
        + "\r\nPartyMode off:"
        + "\r\n* Lock is ended immediately, independent of the number of minutes.";
    const string HLP_HDR = "The following actions are performed depending on the values of the Number of Minutes"
        + " and PartyMode setting fields:";
    const string HLP_TITLE = "Explanation";
    const string HDR_SENT = "Confirmation";
    const string MSG_SENT = "Party mode setting has been sent to the mower.\r\n\r\n@@JSON@@";
    const string TRACE_HELP = "Explanation is required.";
    const string TRACE_SENT = "Sent: @@JSON@@";
    const string VAL_UNKNOWN = "(unknown)";    
#endregion Texts    
}

