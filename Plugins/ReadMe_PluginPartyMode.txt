###########################################################################
##  Plugin: Party Mode Config
###########################################################################

Plugin-Datei: PluginPartyMode.cs

Funktion: Dieses Plugin kann den Mäher für eine vorzugebende Zeit in den sogenannten Party Modus versetzen. Bei aktiviertem Party Modus ist der Mäher gegen Starten durch einen Eintrag im Mäherkalender gesperrt. Vorsicht: Diese Sperre wirkt nicht beim manuellen Starten direkt am Mäher.

Verwendung: Im Plugin-Reiter der Desktop App ist zunächst das Plugin zu aktivieren. Eine gewünschte Zeit in Minuten kann im Feld "New Number of Minutes" eingetragen werden. Im Feld "New PartyMode Setting" ist eine der Optionen "PartyMode off", "PartyMode unlimited on" bzw. "PartyMode limited on" auszuwählen. 

Das Zusammenspiel zwischen diesen beiden Feldern kann über eine Hilfefunktion abgefragt werden. Hierzu ist das Feld "Explanation required" anzukreizen und der Doit Button zu betätigen.


Change Log:

* Version 1.0
  . Erstausgabe
  
* Version 1.1 | 18.07.2021
  . Aktuelle Einstellungen werden angezeigt.

* Version 1.2 | 02.09.2021
  . Absicherung der korrekten Grid-Reihenfolge unter Mono
  
* Version 2.0 | 09.05.2023
  . Anpassung an geänderte Plugin-Schnittstelle ab DeskApp V0.32
  . läuft nur mit DeskApp ab V0.32 aufwärts  
  
* Version 3.0 | 08.07.2023
  . Migration auf Avalonia UI  
  
* Version 3.1 | 30.10.2023
  . Umstellung auf Avalonia 11