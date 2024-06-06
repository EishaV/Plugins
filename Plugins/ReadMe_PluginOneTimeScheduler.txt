###########################################################################
##  Plugin: Config of One Time schedule
###########################################################################

Plugin-Datei: PluginOneTimeScheduler.cs

Funktion: Der Mäher kann über dieses Plugin einmalig zum Mähen geschickt werden, ohne dass ein Eintrag im Kalender des Mähers erfoderlich ist. Die gewünschte Laufzeit sowie die Option "ohne/mit Kantenschnitt" können dabei gewählt werden.

Verwendung: Das Plugin greift nur, wenn der Mäher in der Ladestation steht und sein Akku genügend Ladung aufweist. Im Plugin-Reiter der Desktop App zunächst das Plugin aktivieren. Im Feld "Including Border Cut" kann angehakt werden, ob der Mäher mit einem Kantenschnitt beginnen soll. Das Feld "Worktime in Minutes" nimmt die gewünschte Mähzeit in Minuten auf. Der Mähvorgang beginnt mit dem Drücken des Doit-Buttons. Nach Ablauf der eingestellten Mähzeit fährt der Mäher über das Begrenzungskabel zurück zur Ladestation. Ein reiner Kantenschnitt wird über ein angehaktes "Including Border Cut" und "Worktime in Minutes" = 0 angesteuert. Werden Mähvorgänge über Eintragungen im Mäherkalender gestartet, so ist maximal ein Kantenschnitt pro Tag möglich. Über den One Time Scheduler dagegen können beliebig viele Kantenschnitte an einem Tag ausgeführt werden.