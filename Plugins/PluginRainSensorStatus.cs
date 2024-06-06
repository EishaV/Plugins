using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Timers;
using Positec;
using Plugin;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

public class PluginRainSensorStatus : IPlugin {

    // Version identification
    const string PLUGIN_VERSION = "3.1";
    const string PLUGIN_DATE = "30.10.2023";
    const string PLUGIN_DESC = "Rain sensor status";
    const string PLUGIN_NAME = "RainSensorStatus";

    // Global variables
    private List<PluginParaBase> MyParas;
    List<PluginParaBase> IPlugin.Paras => MyParas;	
    string IPlugin.Desc { get { return DESC_PLUGIN; } }
    DateTime MyTimeStamp;
    private Timer _timer;  

    public PluginParaText ParaRainDelay;
    public PluginParaText ParaMeaning_1;
    public PluginParaText ParaMeaning_2;
    public PluginParaText ParaEmpty1;
    public PluginParaText ParaStatus_1;
    public PluginParaText ParaStatus_2;
    public PluginParaText ParaStatus_3;
    public PluginParaText ParaEmpty2;
    public PluginParaText ParaReady;
    public PluginParaText ParaClock;

    // Event: On Open
    public PluginRainSensorStatus() {
        ParaRainDelay = new PluginParaText (DISP_RAINDELAY, VAL_UNKNOWN, DESC_UPDATING, true);
        ParaMeaning_1 = new PluginParaText (DISP_MEANING1, "", DESC_UPDATING, true);
        ParaMeaning_2 = new PluginParaText (DISP_MEANING2, "", DESC_UPDATING, true);
        ParaEmpty1 = new PluginParaText (DISP_EMPTY1, "", DESC_UPDATING, true);
        ParaStatus_1 = new PluginParaText (DISP_STATUS1, "", DESC_UPDATING, true);
        ParaStatus_2 = new PluginParaText (DISP_STATUS2, "", DESC_UPDATING, true);
        ParaStatus_3 = new PluginParaText (DISP_STATUS3, "", DESC_UPDATING, true);
        ParaEmpty2 = new PluginParaText (DISP_EMPTY2, "", DESC_UPDATING, true);
        ParaReady = new PluginParaText (DISP_READY, VAL_UNKNOWN, DESC_UPDATING, true);
        ParaClock = new PluginParaText (DISP_CLOCK, "", DESC_UPDATING, true);
        MyParas = new List<PluginParaBase>() { ParaRainDelay, ParaMeaning_1, ParaMeaning_2, ParaEmpty1,
            ParaStatus_1, ParaStatus_2,ParaStatus_3, ParaEmpty2, ParaReady, ParaClock };
        _timer = new Timer();
        _timer.Elapsed += _timer_Elapsed;	  
        _timer.Interval = 30000;
        _timer.Start();
        ShowTime();
    }

    // Event: New MQTT data arrived
    void IPlugin.Todo(PluginData pd) {
        MyTimeStamp = DateTime.Now;
        ParaRainDelay.Text = pd.Config.RainDelay.ToString();
        ParaMeaning_1.Text = pd.Config.RainDelay == 0 ? GRID_MEANINGIRRELEVANT : GRID_MEANINGRELEVANT;
        ParaMeaning_2.Text = pd.Config.RainDelay == 0 ? "" : GRID_MEANINGWAITING.Replace("@@MINUTES@@", pd.Config.RainDelay.ToString());
        ParaStatus_1.Text = pd.Config.Date.Replace("/", ".") + " " + pd.Config.Time; 
        ParaStatus_2.Text = ""; 
        ParaStatus_3.Text = ""; 
        ParaReady.Text = GRID_READYNOW;
        if (pd.Data.Rain.State == 0) {
            ParaStatus_2.Text = GRID_STATUSDRY;
            if (pd.Config.RainDelay != 0 ) {
                if (pd.Data.Rain.Counter > 0) {
                    ParaStatus_3.Text = GRID_STATUSWAITING.Replace("@@MINUTES@@", pd.Data.Rain.Counter.ToString());
                    ParaReady.Text = DateTime.Parse(ParaStatus_1.Text).AddMinutes(pd.Data.Rain.Counter).ToString("dd.MM.yyyy HH:mm");
                }   
            }
        } else {
            ParaStatus_2.Text = GRID_STATUSWET;
            ParaReady.Text =  pd.Config.RainDelay == 0 ? GRID_READYNOW : GRID_READYUNKNOWN;
        }
        ShowTime();
    }

    // Event: Doit button pressed
    async void IPlugin.Doit(PluginData pd) {
        ShowTime();
        int Seconds = (int)((DateTime.Now - MyTimeStamp).TotalMilliseconds/1000);
        if (Seconds < 59) {
            InfoBox(HEAD_UPDATING, MESS_UPDATING, "@@SECONDS@@", (60 - Seconds).ToString());
        } else {		
            DeskApp.Send("{}", pd.Index);
        }
    }

    // Event: On Close
    void IDisposable.Dispose() { 
    }

    // Show current time
    private void ShowTime() {
        ParaClock.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
    }

    // Timer Event
    private void _timer_Elapsed(object? sender, ElapsedEventArgs e) {
        ShowTime();
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
	const string DESC_UPDATING = "Updating the rain sensor status via the Doit button is only possible every 60 seconds.";
	const string DESC_PLUGIN = PLUGIN_DESC + " (v" + PLUGIN_VERSION + " | " + PLUGIN_DATE + ")";
	const string DISP_CLOCK = "Current date/time";
	const string DISP_EMPTY1 = "";
	const string DISP_EMPTY2 = "";
	const string DISP_MEANING1 = "Meaning";
	const string DISP_MEANING2 = "";
	const string DISP_RAINDELAY = "Rain delay setting";
	const string DISP_READY = "Ready to mow";
	const string DISP_STATUS1 = "Rain sensor status as of";
	const string DISP_STATUS2 = "";
	const string DISP_STATUS3 = "";
	const string GRID_MEANINGIRRELEVANT = "Rain sensor is not relevant.";
	const string GRID_MEANINGRELEVANT = "Rain sensor is relevant.";
	const string GRID_MEANINGWAITING = "@@MINUTES@@ minute(s) waiting time after rain sensor getting dry";
	const string GRID_READYNOW = "now";
	const string GRID_READYUNKNOWN = "not yet known";
	const string GRID_STATUSDRY = "Rain sensor is dry.";
	const string GRID_STATUSNOWAITING = "No waiting time";
	const string GRID_STATUSWAITING = "Still @@MINUTES@@ minute(s) waiting time";
	const string GRID_STATUSWET = "Rain sensor is wet.";
    const string HEAD_UPDATING = "Please wait ...";
	const string MESS_UPDATING = "Updating the rain sensor status is possible in @@SECONDS@@ seconds at the earliest.";
    const string VAL_UNKNOWN = "(unknown)";
#endregion Texts    
}

