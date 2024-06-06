###########################################################################
##  Plugin: Plugin Battery Level Warning 
###########################################################################

Plugin-Datei: PluginBatteryLevelWarning.cs

Funktion: Dieses Plugin gibt eine Meldung aus, sobald ein vorher festgelegter Batteriestand erreicht oder unterschritten wird. 

Verwendung: Folgende Felder stehen zur Verfügung:
- Warning messaging active: Festlegung, ob die Ausgabe einer Warnmeldung scharf geschaltet werden soll.
- Battery level treshold value (%): Angabe des Schwellwertes für die Warnmeldung
- Current battery level (%): Anzeige des aktuellen Batteriestandes
Nach Programmbeginn wird die Überwachung des Batteriestandes erst eingeschaltet, wenn "Warning messaging active" aktiviert wird und der Doit-Button gedrückt wird. Änderungen an der Aktivierung des Warning Messagings oder des Schwellwertes werden erst nach einer erneuten Betätigung des Doit-Buttons wirksam.


Change Log:

* Version 1.0 | 28.09.2021
  . Erstausgabe

* Version 2.0 | 09.05.2023
  . Anpassung an geänderte Plugin-Schnittstelle ab DeskApp V0.32
  . läuft nur mit DeskApp ab V0.32 aufwärts
  
* Version 3.0 | 02.07.2023
  . Migration auf Avalonia UI
    
* Version 3.1 | 30.10.2023
  . Umstellung auf Avalonia 11