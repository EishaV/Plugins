# Allgemeines

Plugins sind im Unterverzeichnis Plugins des AvaDeskApp Stammverzeichnisses liegende Zusatzfunktionen.
An dieser Stelle werden die allgemein verfügbaren Plugins zum Download angeboten. Plugins werden zur Laufzeit geladen und übersetzt.

##  Liste der verfügbaren Plugins
- AutoLock Switch
- Battery Level Warning 
- GoTo StartPoint
- Mowing Calendar
- Off Limits Module Switch
- One Time Scheduler Config
- Party Mode Config
- Rain Sensor Status
- Schedule Backup & Recovery
- Set Next Zone
- ZoneKeeper Switch
- Write Landroid's data to CSV file

Die Funktionen dieser Plugins werden nachfolgend kurz dokumentiert.

##  Plugin: AutoLock Switch
Plugin-Datei: PluginAutoLockSwitch.cs

Funktion: Dieses Plugin ermöglicht es, die AutoLock-Funktion eines Mähers ein- oder auszuschalten - auch bei Mähern, bei denen AutoLock nicht am Bedienfeld eingestellt werden kann.

##  Plugin: Plugin Battery Level Warning 
Plugin-Datei: PluginBatteryLevelWarning.cs

Funktion: Dieses Plugin gibt eine Meldung aus, sobald ein vorher festgelegter Batteriestand erreicht oder unterschritten wird.

##  Plugin: GoTo StartPoint
Plugin-Datei: PluginGoToStartPoint.cs

Funktion: Dieses Plugin ermöglicht es, den Mäher ab einem vorgegebenem Startpunkt für eine vorgegebene Zeit mähen zu lassen. Der Startpunkt kann dabei über eine Zonenauswahl oder über eine beliebige Eingabe einer Meterzahl bestimmt werden. 

##  Plugin: Mowing Calendar
Plugin-Datei: PluginMowingCalendar.cs

Funktion: Dieses Plugin zeigt die Mähplanung der kommenden 8 Tage an, wobei auch die jeweils anzufahrenden Zonen genannt werden. Zusätzlich kann der Mähkalender in eine Textdatei exportiert werden.

##  Plugin: Off Limits Module Switch
Plugin-Datei: PluginOffLimitsModuleSwitch.cs

Funktion: Dieses Plugin erlaubt es, das Off Limits Module ein- bzw. auszuschalten, und zwar jeweils einzeln für die Erkennung des Magnetstreifens beim Mähen und für die Erkennung der Shortcut-Markierungen für Fast Homing.

##  Plugin: One Time Scheduler Config
Plugin-Datei: PluginOneTimeScheduler.cs

Funktion: Der Mäher kann über dieses Plugin einmalig zum Mähen geschickt werden, ohne dass ein Eintrag im Kalender des Mähers erfoderlich ist. Die gewünschte Laufzeit sowie die Option "ohne/mit Kantenschnitt" können dabei gewählt werden.

##  Plugin: Party Mode Config
Plugin-Datei: PluginPartyMode.cs

Funktion: Dieses Plugin kann den Mäher für eine vorzugebende Zeit in den sogenannten Party Modus versetzen. Bei aktiviertem Party Modus ist der Mäher gegen Starten durch einen Eintrag im Mäherkalender gesperrt. Vorsicht: Diese Sperre wirkt nicht beim manuellen Starten direkt am Mäher.

##  Plugin: Rain Sensor Status
Plugin-Datei: PluginRainSensorStatus

Funktion: Das Plugin gibt Auskunft über die Nutzung und den Status des Regensensors sowie den Ablauf der Wartezeit nach der Trocknung des Regensensors.

##  Plugin: Schedule Backup & Recovery
Plugin-Datei: PluginScheduleBackupRecovery.cs

Funktion: Dieses Plugin ermöglicht es, den aktuellen Mähplan im Bereich der Desk App zwischenzuspeichern und später den aktuellen Mähplan aus einem zwischengespeicherten Mähplan zu restaurieren. Das Plugin kann bis zu drei zwischengespeicherte Mähpläne verwalten.

##  Plugin: Set Next Zone
Plugin-Datei: PluginSetNextZone.cs

Funktion: Da sich bei Mähaktionen außerhalb der geplanten Mähzeiten auch der Zeiger auf den nächsten Startpunkt ändert, kam gelegentlich der Wunsch hoch, den Zeiger wieder zurücksetzen zu können. Dies lässt der Mäher jedoch nicht zu. Es funktioniert aber der Weg, die Reihe der anzufahrenden Startpunkte so im Ring zu schieben, dass ein bestimmter Startpunkt zum Ziel des Zeigers wird. Diese Methode wird von diesem Plugin angewendet.

##  Plugin: ZoneKeeper Switch
Plugin-Datei: PluginZoneKeeperSwitch.cs

Funktion: Dieses Plugin ermöglicht es, das Zone Keeper Feature eines Mähers ein- oder auszuschalten.

##  Plugin: Write Landroid's data to CSV file
Plugin-Datei: PluginCsvLogWriter.cs

Funktion: Das Plugin schreibt fortlaufend Daten zum Mäherverhalten in eine im Data Unterverzeichnis liegende csv-Datei namens <Mähername>.csv.
