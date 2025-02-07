# Kundenportal.AdminUi

Diese Anwendung ermöglicht das wiederholte Anlegen von vordefinierten Datei- und Ordnerstrukturen in Nextcloud. Zusätzlich kann sie Benutzer in Nextcloud anlegen und ihnen Zugriff auf die erstellte Struktur geben.

## Getting Started

### Die Anwendung ausführen

***TODO***

### Konfiguration

#### Vorwort

Hier werden die verfügbaren Konfigurationsoptionen dargestellt.

Hierarchieebenen in den Konfigurationsschlüsseln werden in der nachfolgenden Tabelle durch Doppelpunkte dargestellt. Unterschiedliche Varianten, der Anwendung die Konfiguration zu übergeben, erfordern unterschiedliche Formate der Konfiguration.

Beispiel:  Die Konfiguration mit dem Schlüssel `Foo:Bar` und dem Wert `Value` kann wie folgt dargestellt werden:

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

Ein Doppelpunkt ist nicht auf allen Systemen möglich, daher ist es auch möglich, einen Doppelpunkt durch **zwei** Unterstrichte zu ersetzen:

```env
Foo__Bar=Value
```

#### Konfigurationsoptionen

| Schlüssel | Default Wert | Erforderlich | Beschreibung |
|------------------------------|-------------------------------------------------------------------------|--------------|-----------------------------------------------------------------------------|
| `ConnectionStrings:Database` | Ein PostgreSql Connection String zu einem lokalen Dev Docker Container. | Sinnvoll | Der Connection String zur PostgreSql Datenbank. |
| `RabbitMq:Host` | localhost | Nein | Der Host, unter dem die RabbitMq Instanz erreichbar ist. |
| `RabbitMq:Port`              | 5672                                                                    | Nein         | Der Port~~~~, unter dem die RabbitMq Instanz erreichbar ist.                    |
| `RabbitMq:VirtualHost` | / (Ein Slash, kein leerer Wert!) | Nein | Der virtuelle Host in der RabbitMq Instanz (bei Fragen siehe RabbitMq Docs) |
| `RabbitMq:Username` | guest | Nein | Der Benutzername |
| `RabbitMq:Password` | guest | Nein | Das Passwort |
