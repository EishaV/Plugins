###########################################################################
##  Plugin: GoTo StartPoint
###########################################################################

Plugin-Datei: PluginGoToStartPoint.cs

Funktion: Dieses Plugin ermöglicht es, den Mäher ab einem vorgegebenem Startpunkt für eine vorgegebene Zeit mähen zu lassen. Der Startpunkt kann dabei über eine Zonenauswahl oder über eine beliebige Eingabe einer Meterzahl bestimmt werden. 

Verwendung: In den Feldern "Current Mower State" bzw. "Current Zone StartPoints" werden der aktuelle Status des Mähers bzw. die aktuellen Startpunkte der Zonen angezeigt. Im Feld "StartPoint Definition Method" ist zunächst einzustellen, ob die Festlegung des Startpunktes über eine Zonenauswahl oder über die Eingabe einer beliebigen Meterzahl (als Abstand von der Ladestation) erfolgen soll. Dementsprechend ist dann eine Zonenauswahl im Feld "StartPoint Definition via Zone Selection" oder eine Metereingabe im Feld "StartPoint Definition via Meter Specification" durchzuführen. Zudem ist eine Mähzeit von mindestens 10 Minuten einzugeben. Ist mindestens die Firmware-Version 3.15 im Einsatz, steht der Mäher aufgeladen in der Station und sind die getätigten Eingaben zulässig, wird der Mäher über den One Time Scheduler gestartet.

ACHTUNG: Wird die DeskApp verlassen, ehe der Mäher am Startpunkt mit dem Mähen begonnen hat, gehen die bisherigen Zonen-Einstellungen verloren, da das Plugin zwischen Mäherausfahrt und Mähbeginn die Zoneneinstellungen temporär abändern muss.


Change Log:

* Version 1.0 | 26.06.2021
  . Erstausgabe
  
* Version 1.1 | 24.07.2021
  . Führungstexte lesbarer gestaltet.
  
* Version 2.0 | 09.05.2023
  . Anpassung an geänderte Plugin-Schnittstelle ab DeskApp V0.32
  . läuft nur mit DeskApp ab V0.32 aufwärts  
  
* Version 3.0 | 24.06.2023
  . Migration auf Avalonia UI  
  
* Version 3.1 | 30.10.2023
  . Umstellung auf Avalonia 11  