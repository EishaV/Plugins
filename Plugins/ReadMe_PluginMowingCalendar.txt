###########################################################################
##  Plugin: Mowing Calendar
###########################################################################

Plugin-Datei: PluginMowingCalendar.cs

Funktion: Dieses Plugin zeigt die Mähplanung der kommenden 8 Tage an, wobei auch die jeweils anzufahrenden Zonen genannt werden. Zusätzlich kann der Mähkalender in eine Textdatei exportiert werden.

Verwendung: Alle im Plugin verwendeten Felder sind - mit einer Ausnahme - reine Anzeigefelder:
- Sequence of zone numbers: Eingestellte Folge der Zonennummern
- Current zone pointer: Zeiger auf die bei der nächsten Ausfahrt anzufahrende Zone
- Status: Mäherstatus
- Work time correction: Prozentangabe zur Reduktion/Erweiterung der Mähzeiten
- Time stamp: Zeitpunkt der Erstellung des Mähkalenders
- Next scheduled departures: Anzeige der anstehenden Ausfahrten mit Angabe des Datums, des Wochentages, der Startzeit, der Endezeit, der Zonennummer und eines eventuellen Kantenschnitts, wobei in die Endezeit-Berechnung der Wert für eine Reduktion/Erweiterung der Mähzeiten eingeflossen ist.
- Export to a text file: Mit der Aktivierung dieser Funktion wird über den Doit-Button ein Export des Mähkalenders in die Datei Export_MowingCalendar_<Mähername>.txt im Data Verzeichnis der Desktop App ausgeführt.

Wichtiger Hinweis: Die Vorschau der Zonenzuodnungen verliert ihre Gültigkeit, wenn ungeplante Unterbrechungen oder Abbrüche geplanter Ausfahrten auftreten oder wenn Ausfahrten außerhalb der Mähplanung (z.B. durch manuelle Starts oder durch den One Time Scheduler) vorgenommen werden. Zu den ungeplanten Unterbrechungen zählen z.B. auch Regenheimfahrten und Nachladevorgänge innerhalb der geplanten Mähzeit.

Change Log:

* Version 1.0 | 29.08.2021
  . Erstausgabe

* Version 1.1 | 30.08.2021
  . Export-Option für den Mähkalender

* Version 1.2 | 02.09.2021
  . Absicherung der korrekten Grid-Reihenfolge unter Mono
  
* Version 2.0 | 09.05.2023
  . Anpassung an geänderte Plugin-Schnittstelle ab DeskApp V0.32
  . läuft nur mit DeskApp ab V0.32 aufwärts
  
* Version 3.0 | 04.07.2023
  . Migration auf Avalonia UI
  
* Version 3.1 | 30.10.2023
  . Umstellung auf Avalonia 11