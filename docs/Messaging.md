# Messaging

Die Anwendung setzt `MassTransit` als Broker in Verbindung mit `RabbitMq` als Transport ein. Dies entkoppelt die Anwendung und erhöht die Resillienz.

Die Anwendung verwendet die Möglichkeit in MassTransit, fehlgeschlagene Nachrichten erneut zuzustellen, bevor sie in eine Fehler-Queue verschoben werden. Fehlgeschlagene Nachrichten werden zunächst mehrmals nach wenigen Sekunden erneut zugestellt und anschließend mehrere Male zu späteren Zeitpunkten (jeweils wieder inkl. mehrmaligem,  Wiederholen nach wenigen Sekunden bei Fehlern). Temporäre Fehler, bspw. Überlastung der Datenbank / Nextcloud oder Konflikte in der Datenbank, die durch einen erneuten Versuch behoben werden können, können so mitigiert werden.
