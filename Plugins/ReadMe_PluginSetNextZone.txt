###########################################################################
##  Plugin: Set Next Zone
###########################################################################

Plugin-Datei: PluginSetNextZone.cs

Funktion: Da sich bei Mähaktionen außerhalb der geplanten Mähzeiten auch der Zeiger auf den nächsten Startpunkt ändert, kam gelegentlich der Wunsch hoch, den Zeiger wieder zurücksetzen zu können. Dies lässt der Mäher jedoch nicht zu. Es funktioniert aber der Weg, die Reihe der anzufahrenden Startpunkte so im Ring zu schieben, dass ein bestimmter Startpunkt zum Ziel des Zeigers wird. Diese Methode wird von diesem Plugin angewendet.

Verwendung: Im Feld Zone Sequence wird die eingestellte Reihe der Startpunktnummern angezeigt. Das Feld Ruler enthält eine Art Lineal, das einem das Abzählen der Elemente dieser Reihe erspart. Der aktuelle Zeiger auf den für die nächste Mähaktion anstehenden Startpunkt ist im Feld Current Zone Pointer visualisiert. Im Feld New Zone Index kann nun diejenige Startpunktnummer ausgewählt werden, die vom Plugin "unter den Zeiger" geschoben werden soll. Durch Betätigung des Doit-Buttons wird dieser Vorgang gestartet.


Change Log:

* Version 1.0 | 01.07.2021
  . Erstausgabe
  
* Version 2.0 | 09.05.2023
  . Anpassung an geänderte Plugin-Schnittstelle ab DeskApp V0.32
  . läuft nur mit DeskApp ab V0.32 aufwärts
  
* Version 3.0 | 23.06.2023
  . Migration auf Avalonia UI  
  
* Version 3.1 | 30.10.2023
  . Umstellung auf Avalonia 11