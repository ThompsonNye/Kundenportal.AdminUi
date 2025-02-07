# Request localization

## Request Sprache pro Aufruf setzen

Die Anwendung setzt beim Aufrufen die Sprache für den aktuellen Benutzer basierend auf dem HTTP
Header `Accept-Language`. Ein Browser setzt diesen Header bspw. automatisch basierend auf der eingestellten Sprache des
Benutzers. Dies kann bspw. so aussehen:

```
Accept-Language: de-DE,de;q=0.9,en;q=0.8,en-US;q=0.7
```

Die Standard-Sprache (bzw. Culture) für die Anwendung ist die zuerst in der Request Pipeline definierte Sprache,
d.h. `de-DE`. Diese Sprache wird als Fallback verwendet, wenn kein `Accept-Language` Header definiert ist oder dort
keine unterstützte Sprache definiert ist. Unterstützte Sprachen (bzw. Cultures) sind:

- `de-DE`
- `de`
- `en-US`
- `en`

Für mehr Informationen zum `Accept-Language` Header
siehe [hier](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language).

## Übersetzungen für Texte in der Anwendung

Damit die angefragte Sprache auch tatsächlich Wirkung entfaltet, werden Texte, für die es Übersetzungen geben soll, über
Ressourcen hinterlegt. Für jede unterstützte Sprache muss ein Text für den jeweiligen Schlüssel angegeben werden,
ansonsten wird der Fallback-Wert verwendet.

Fallback-Texte sind deutsch. Es gibt keine eigene Ressource für Deutsch.
