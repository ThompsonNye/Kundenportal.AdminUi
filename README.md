# Kundenportal.AdminUi

Diese Anwendung ermöglicht das wiederholte Anlegen von vordefinierten Datei- und Ordnerstrukturen in Nextcloud.
Zusätzlich kann sie Benutzer in Nextcloud anlegen und ihnen Zugriff auf die erstellte Struktur geben.

## Getting Started

### Die Anwendung ausführen

Benötigt Docker und Docker Compose (oder äquivalente Alternativen).

Die Abhängigkeiten starten:

```shell
docker compose -f compose.dev.yaml up -d --wait --build --pull always
```

Der Nextcloud Companion Container führt lediglich bestimmte Vorbereitungen für Nextcloud aus:

- Er legt den Ordner für Strukturen an, den die Anwendung benötigt.
- Er aktualisiert die E-Mail-Adresse des Admin-Benutzers, damit die Anwendung E-Mails an diesen Benutzer senden kann.

Anschließend stoppt der Container wieder, da seine Aufgabe erledigt ist.
Die folgende Meldung ist daher zu erwarten und in Ordnung, solange hinten der Exit Code `0` steht:

```text
container kundenportal_adminui_dev-nextcloud.companion-1 exited (0)
```

Wenn der Exit Code `0` ist, wurden die Vorbereitungen erfolgreich durchgeführt und die Anwendung kann gestartet werden.
Wenn der Exit Code etwas anderes ist, ist ein Fehler aufgetreten. Der Companion Container hat dann Nextclouds HTTP
Response ausgegeben, sodass man den Fehler so hoffentlich finden und beheben kann.

Dann die Anwendung starten.

E-Mails der Anwendung werden aktuell in die Konsole geschrieben, anstatt verschickt zu werden.

### Konfiguration

#### Vorwort

Hier werden die verfügbaren Konfigurationsoptionen dargestellt.

Hierarchieebenen in den Konfigurationsschlüsseln werden in der nachfolgenden Tabelle durch Doppelpunkte dargestellt.
Unterschiedliche Varianten, der Anwendung die Konfiguration zu übergeben, erfordern unterschiedliche Formate der
Konfiguration.

Beispiel: Die Konfiguration mit dem Schlüssel `Foo:Bar` und dem Wert `Value` kann wie folgt dargestellt werden:

In einer appsettings.*.json Datei:

```json
{
  "Foo": {
    "Bar": "Value"
  }
}
```

Als Umgebungsvariable:

```env
Foo:Bar=Value
```

Ein Doppelpunkt ist nicht auf allen Systemen möglich, daher ist es auch möglich, einen Doppelpunkt durch **zwei**
Unterstriche zu ersetzen:

```env
Foo__Bar=Value
```

#### Besonderheiten bei Umgebungsvariablen

Umgebungsvariablen stehen der Anwendung als Konfiguration zur Verfügung. Die Anwendung definiert jedoch das optionale
Präfix `Kundenportal_AdminUi_`. Umgebungsvariablen, die dieses Präfix haben, werden von der Anwendung so behandelt, als
hätten sie dieses Präfix nicht. Des Weiteren überschreiben diese Umgebungsvariablen, die dieses Präfix haben, andere
Konfigurationen, deren Schlüssel dieses Präfix nicht hat. Beispiel:

Folgende Konfiguration wird in der `appsettings.json` definiert:

```json
{
  "Foo": {
    "Bar": "Value"
  }
}
```

Alternativ kann diese Konfiguration auch bspw. als Umgebungsvariable konfiguriert sein:

```env
Foo__Bar=Value
```

Darüber hinaus wird folgende Umgebungsvariable definiert:

```env
Kundenportal_AdminUi_Foo__Bar=AnotherValue
```

Die Anwendung überschreibt die erste Konfiguration mit dem Wert der Umgebungsvariable mit Präfix. Die Anwendung sieht
lediglich die Konfiguration `Foo:Bar=AnotherValue`.

Das Präfix kann, muss aber nicht genutzt werden. Er bietet die Möglichkeit, die Umgebungsvariablen, die für die
Anwendung bestimmt sind, besser zu kennzeichnen.

#### Konfigurationsoptionen

| Schlüssel                     | Default Wert                                                            | Erforderlich                                                                                                                                                                                                       | Beschreibung                                                                         |
|-------------------------------|-------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| `ConnectionStrings:Database`  | Ein PostgreSql Connection String zu einem lokalen Dev Docker Container. | Sinnvoll                                                                                                                                                                                                           | Der Connection String zur PostgreSql Datenbank.                                      |
| `RabbitMq:Host`               | localhost                                                               | Nein                                                                                                                                                                                                               | Der Host, unter dem die RabbitMq Instanz erreichbar ist.                             |
| `RabbitMq:Port`               | 5672                                                                    | Nein                                                                                                                                                                                                               | Der Port~~~~, unter dem die RabbitMq Instanz erreichbar ist.                         |
| `RabbitMq:VirtualHost`        | / (Ein Slash, kein leerer Wert!)                                        | Nein                                                                                                                                                                                                               | Der virtuelle Host in der RabbitMq Instanz (bei Fragen siehe RabbitMq Docs)          |
| `RabbitMq:Username`           | guest                                                                   | Nein                                                                                                                                                                                                               | Der Benutzername                                                                     |
| `RabbitMq:Password`           | guest                                                                   | Nein                                                                                                                                                                                                               | Das Passwort                                                                         |
| `OTEL_SERVICE_NAME`           |                                                                         | Sinnvoll                                                                                                                                                                                                           | Service Name for the Application in OpenTelemetry traces. Nur als Umgebungsvariable. |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | http://localhost:4318                                                   | Nein                                                                                                                                                                                                               | Der Endpoint für den OpenTelemetry Exporter                                          |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | http/protobuf                                                           | Nein                                                                                                                                                                                                               | Das OLTP Protokoll. Erlaubte Werte: grpc, http/protobuf                              |
| `Nextcloud:Host`              |                                                                         | Ja                                                                                                                                                                                                                 | Der Host des Nextcloud Services                                                      |
| `Nextcloud:StructureBasePath` | /Structures                                                             |                                                                                                                                                                                                                    | Nein                                                                                 | Der Pfad, unter dem Strukturen angelegt werden. Muss mit einem / beginnen und darf nicht mit einem / enden. |
| `Nextcloud:Username`          |                                                                         | Der Benutzername des Service Benutzers, mit dem die Anwendung in Nextcloud arbeiten soll. Unter diesem Benutzer werden die Strukturen angelegt.                                                                    |
| `Nextcloud:Password`          |                                                                         | Das Password des Service Benutzers in `Nextcloud:Username`. Es wird empfohlen, für den Service Benutzer in Nextcloud ein App Passwort zu generiern und dies hier anstelle des eigentlichen Passworts zu verwenden. |
| `Nextcloud:RetryDelay`        | 100                                                                     | Die Anzahl an Millisekunden, die Polly als Basis für den exponentiellen Backup zwischen erneuten Versuchen im Falle von Fehlern verwendet.                                                                         |

## Projektaufbau

Der Aufbau des Codes orientiert sich lose an der Clean Architecture. Allerdings wird auf das Domain-Projekt verzichtet,
da kein Domain Driven Design (DDD) eingesetzt wird und das Domain Projekt daher sonst nur die Models beinhalten würde.
Die Models wurden daher einfach im Application Projekt angelegt.

Außerdem werden Services im Application Projekt, die zum gleichen Feature gehören, in wenig im Sinne von Vertical Slices
zusammen abgelegt, teilweise auch in der gleichen Datei. Hier wird aber bisher nicht strikt darauf geachtet, dabei
eine Strategie konsequent umzusetzen.

## Dokumentation

Einzelne, wichtige Konzepte werden im `docs` Ordner genauer erläutert.
